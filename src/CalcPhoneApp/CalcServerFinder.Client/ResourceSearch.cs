using System;
using System.Collections.Generic;
using System.Threading;
using System.ServiceModel;
using System.ComponentModel;

using CalcServerFinder.Contracts;

namespace CalcServerFinder.Client
{
    /// <summary>
    /// Questa classe permette di ricercare un nodo/servizio di elaborazione tramite un nodo/servizio di ricerca.
    /// </summary>
    public class ResourceSearch
    {
        #region Events

        /// <summary>
        /// Questo evento viene generato quando è disponibile una nuova informazione sullo stato della ricerca
        /// attualmente in esecuzione sul nodo di ricerca.
        /// </summary>
        public event ResourceSearchProgressHandler OnResourceSearchProgress;

        /// <summary>
        /// Genera un nuovo evento per notificare il cambiamento di stato della ricerca attualmente in esecuzione
        /// sul nodo di ricerca.
        /// </summary>
        /// <param name="state">lo stato attuale riguardante la ricerca della risorsa</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="result">gli eventuali risultati finora trovati dal nodo di ricerca</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il parametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati di ricerca non sono disponibili a causa di un errore
        /// o perché l'operazione è stata annullata o non sono stati trovati risultati, il parametro corrispondente
        /// dovrà essere impostato a null o con una lista vuota.
        /// </remarks>
        private void RaiseResourceSearchProgress(ResourceSearchState state, Exception error, List<Uri> result)
        {
            ResourceSearchProgressHandler handler = OnResourceSearchProgress;
            if (handler != null)
            {
                ResourceSearchProgressEventArgs args =
                    new ResourceSearchProgressEventArgs(state, error, result);
                OnResourceSearchProgress(this, args);
            }
        }

        /// <summary>
        /// Questo evento viene generato non appena il client ha concluso la comunicazione col nodo di ricerca,
        /// in seguito allo scadere del numero massimo di tentativi impostato per l'esecuzione della ricerca.
        /// </summary>
        public event ResourceSearchCompletedHandler OnResourceSearchCompleted;

        /// <summary>
        /// Genera un nuovo evento per notificare il completamento della ricerca di risorse da parte del client.
        /// </summary>
        /// <param name="cancelled">un valore che indica se la ricerca è stata annullata</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="result">gli eventuali risultati trovati dal nodo di ricerca</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il parametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati di ricerca non sono disponibili a causa di un errore
        /// o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere impostato a null o con
        /// una lista vuota.
        /// </remarks>
        private void RaiseResourceSearchCompleted(bool cancelled, Exception error, List<Uri> result)
        {
            ResourceSearchCompletedHandler handler = OnResourceSearchCompleted;
            if (handler != null)
            {
                ResourceSearchCompletedEventArgs args =
                    new ResourceSearchCompletedEventArgs(cancelled, error, result);
                OnResourceSearchCompleted(this, args);
            }
        }

        #endregion

        #region Fields

        private readonly TimeSpan m_PollingInterval;
        private readonly ManualResetEvent m_PollingWaitHandle;

        private readonly uint m_SearchRetryLimit;
        private int m_SearchRetryCounter;

        private readonly BasicHttpBinding m_Binding;
        private readonly EndpointAddress m_Endpoint;

        private ProcessingServiceFinderClient m_Proxy;
        private Dictionary<Type, ResourceSearchState> m_CommunicationErrorsMapping;

        private SearchOptions m_SearchOptions;
        private List<Uri> m_SearchResult;

        #endregion

        #region Constructors

        /// <summary>
        /// Istanzia un nuovo oggetto della classe ResourceSearch con i parametri specificati.
        /// </summary>
        /// <param name="serviceUri">indirizzo del servizio di ricerca</param>
        /// <param name="pollingInterval">tempo di attesa per avviare una nuova ricerca</param>
        /// <param name="retryLimit">numero massimo di tentativi di ricerca</param>
        /// <exception cref="UriFormatException">
        /// Se si specifica un URI non valido come indirizzo del servizio di ricerca.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Se si specifica il valore null come indirizzo del servizio di ricerca.
        /// </exception>
        public ResourceSearch(string serviceUri, TimeSpan pollingInterval, uint retryLimit)
        {
            m_PollingInterval = pollingInterval;
            m_PollingWaitHandle = new ManualResetEvent(false);

            m_SearchRetryLimit = retryLimit;
            m_SearchRetryCounter = 0;

            m_Binding = new BasicHttpBinding();
            m_Endpoint = new EndpointAddress(serviceUri);

            m_Proxy = null;
            m_CommunicationErrorsMapping = new Dictionary<Type, ResourceSearchState>()
            {
                { typeof(TimeoutException), ResourceSearchState.TimeoutError },
                { typeof(EndpointNotFoundException), ResourceSearchState.ServiceNotFoundError },
                { typeof(CommunicationException), ResourceSearchState.CommunicationError }
            };

            m_SearchOptions = new SearchOptions() { Name = string.Empty, Version = string.Empty };
            m_SearchResult = new List<Uri>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Imposta le opzioni di ricerca.
        /// </summary>
        /// <param name="name">nome completo del componente che implementa la risorsa di elaborazione</param>
        /// <param name="version">versione del componente che implementa la risorsa di elaborazione</param>
        public void SetSearchOptions(string name, string version)
        {
            m_SearchOptions.Name = name;
            m_SearchOptions.Version = version;
        }

        /// <summary>
        /// Determina l'avvio della procedura che permette di cercare la risorsa di elaborazione in base alle opzioni
        /// di ricerca impostate e di verificare periodicamente (per un massimo di tentativi stabilito) gli eventuali
        /// risultati via via trovati dal nodo di ricerca.
        /// </summary>
        /// <remarks>
        /// Prima di invocare questo metodo, occorre usare il metodo SetSearchOptions per poter impostare le opzioni
        /// di ricerca.
        /// Inoltre, questo metodo non dovrebbe essere invocato sul thread della UI, in quanto la verifica periodica
        /// della disponibilità di nuovi risultati bloccherebbe tale thread per l'intervallo di polling specificato.
        /// </remarks>
        public void Start()
        {
            if (m_Proxy == null)
            {
                BeginOpenProxy();
            }
        }

        #endregion

        #region Proxy Handling Methods

        /// <summary>
        /// Inizia la configurazione del proxy per la comunicazione col servizio di ricerca.
        /// </summary>
        private void BeginOpenProxy()
        {
            RaiseResourceSearchProgress(ResourceSearchState.InitializingProxy, null, null);

            m_Proxy = new ProcessingServiceFinderClient(m_Binding, m_Endpoint);

            m_Proxy.OpenCompleted += new EventHandler<AsyncCompletedEventArgs>(Proxy_OpenCompleted);
            m_Proxy.CloseCompleted += new EventHandler<AsyncCompletedEventArgs>(Proxy_CloseCompleted);
            m_Proxy.SearchCompleted += new EventHandler<SearchCompletedEventArgs>(Proxy_SearchCompleted);
            
            m_Proxy.OpenAsync();
        }

        /// <summary>
        /// Inizia la chiusura del canale di comunicazione tra il proxy e il servizio.
        /// </summary>
        private void BeginCloseProxy()
        {
            RaiseResourceSearchProgress(ResourceSearchState.DisposingProxy, null, null);

            m_Proxy.CloseAsync();
        }

        /// <summary>
        /// Invia i dati del task al servizio di ricerca.
        /// </summary>
        private void BeginSearch()
        {
            RaiseResourceSearchProgress(ResourceSearchState.StartingSearch, null, null);

            m_Proxy.SearchAsync(m_SearchOptions);
        }

        /// <summary>
        /// Ripete la ricerca oppure inizia a chiudere il proxy.
        /// </summary>
        private void RepeatSearchOrClose()
        {
            if (m_SearchRetryCounter < m_SearchRetryLimit)
            {
                m_PollingWaitHandle.WaitOne(m_PollingInterval);
                BeginSearch();
            }
            else
            {
                RaiseResourceSearchCompleted(false, null, m_SearchResult);
                BeginCloseProxy();
            }
        }

        /// <summary>
        /// Permette di annullare la registrazione agli eventi generati dal proxy.
        /// </summary>
        private void UnregisterProxyEvents()
        {
            if (m_Proxy != null)
            {
                m_Proxy.OpenCompleted -= new EventHandler<AsyncCompletedEventArgs>(Proxy_OpenCompleted);
                m_Proxy.CloseCompleted -= new EventHandler<AsyncCompletedEventArgs>(Proxy_CloseCompleted);
                m_Proxy.SearchCompleted -= new EventHandler<SearchCompletedEventArgs>(Proxy_SearchCompleted);
            }
        }

        #endregion

        #region Async Result Methods

        /// <summary>
        /// Questo metodo viene automaticamente invocato quando il proxy completa la sua configurazione, momento
        /// in cui può essere usato per iniziare la ricerca tramite il servizio di ricerca.
        /// </summary>
        /// <param name="sender">l'oggetto che ha generato l'evento</param>
        /// <param name="args">informazioni aggiuntive sull'evento</param>
        private void Proxy_OpenCompleted(object sender, AsyncCompletedEventArgs args)
        {
            if (HandleCancellationIfRequired(args.Cancelled)) return;
            if (HandleErrorIfRequired(args.Error)) return;

            RaiseResourceSearchProgress(ResourceSearchState.ProxyInitialized, null, null);

            BeginSearch();
        }

        /// <summary>
        /// Questo metodo viene automaticamente invocato nel momento in cui il proxy completa la chiusura del canale
        /// di comunicazione e permette di rimuovere la registrazione degli eventi associati al proxy e di riportare
        /// quest'ultimo al valore null.
        /// </summary>
        /// <param name="sender">l'oggetto che ha generato l'evento</param>
        /// <param name="args">informazioni aggiuntive sull'evento</param>
        private void Proxy_CloseCompleted(object sender, AsyncCompletedEventArgs args)
        {
            if (HandleCancellationIfRequired(args.Cancelled)) return;
            if (HandleErrorIfRequired(args.Error)) return;

            RaiseResourceSearchProgress(ResourceSearchState.ProxyDisposed, null, null);

            UnregisterProxyEvents();
            m_Proxy = null;
        }

        /// <summary>
        /// Questo metodo viene invocato non appena i dati del task sono stati ricevuti dal servizio, permettendo
        /// in tal modo di entrare nella successiva fase, cioè la verifica periodica dello stato di completamento
        /// dell'elaborazione.
        /// </summary>
        /// <param name="sender">l'oggetto che ha generato l'evento</param>
        /// <param name="args">informazioni aggiuntive sull'evento</param>
        private void Proxy_SearchCompleted(object sender, SearchCompletedEventArgs args)
        {
            if (HandleCancellationIfRequired(args.Cancelled)) return;
            if (HandleErrorIfRequired(args.Error)) return;

            List<Uri> result = new List<Uri>();

            if (args.Result != null)
            {
                foreach (var uri in args.Result)
                {
                    if (!m_SearchResult.Contains(uri))
                    {
                        // aggiorna i risultati totali
                        m_SearchResult.Add(uri);

                        // aggiorna i nuovi risultati
                        result.Add(uri);
                    }
                }
            }

            m_SearchRetryCounter++;

            RaiseResourceSearchProgress(ResourceSearchState.FoundResults, null, result);

            RepeatSearchOrClose();
        }

        #endregion

        #region Error/Cancellation Handling

        /// <summary>
        /// Permette di gestire la cancellazione di una ricerca e restituisce true se è stata gestita chiudendo
        /// il proxy, oppure false se non è stato necessario gestirla perché non è avvenuta alcuna cancellazione.
        /// </summary>
        /// <param name="cancelled">un valore che indica se la ricerca è stata annullata o meno</param>
        /// <returns>true se l'eventuale cancellazione della ricerca è stata gestita, altrimenti false</returns>
        private bool HandleCancellationIfRequired(bool cancelled)
        {
            if (cancelled)
            {
                RaiseResourceSearchProgress(ResourceSearchState.SearchCancelled, null, null);
                RaiseResourceSearchCompleted(true, null, m_SearchResult);
                BeginCloseProxy();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Permette di gestire un errore verificatosi durante la procedura e restituisce true se è stata gestita,
        /// altrimenti false se non è stato necessario gestirlo poiché di fatto non è avvenuta alcun errore.
        /// </summary>
        /// <param name="error">l'eccezione che ha provocato l'errore</param>
        /// <returns>true se l'eventuale errore è stata gestito, altrimenti false</returns>
        private bool HandleErrorIfRequired(Exception error)
        {
            if (error != null)
            {
                ResourceSearchState state;
                if (m_CommunicationErrorsMapping.TryGetValue(error.GetType(), out state))
                {
                    RaiseResourceSearchProgress(state, error, null);

                    m_Proxy.Abort();
                    RaiseResourceSearchProgress(ResourceSearchState.ProxyDisposed, error, null);

                    UnregisterProxyEvents();
                    m_Proxy = null;
                }
                else
                {
                    RaiseResourceSearchProgress(ResourceSearchState.UnknownError, error, null);

                    m_Proxy.Abort();
                    RaiseResourceSearchProgress(ResourceSearchState.ProxyDisposed, error, null);

                    UnregisterProxyEvents();
                    m_Proxy = null;
                }

                RaiseResourceSearchCompleted(false, error, m_SearchResult);

                return true;
            }

            return false;
        }

        #endregion
    }
}
