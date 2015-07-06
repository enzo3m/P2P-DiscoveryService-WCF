using System;
using System.Collections.Generic;
using System.Threading;
using System.ServiceModel;
using System.ComponentModel;

using CalcServer.Contracts;

namespace CalcServer.Client
{
    /// <summary>
    /// Questa classe permette di avviare l'elaborazione di un task richiedendo l'intervento di un nodo/servizio
    /// di elaborazione e attendendo che quest'ultimo produca gli eventuali risultati.
    /// </summary>
    public class TaskExecution
    {
        #region Events

        /// <summary>
        /// Questo evento viene generato non appena la richiesta di elaborazione inviata al servizio
        /// viene completata e sono disponibili gli eventuali risultati, oppure nel caso in cui tale
        /// richiesta è stata annullata oppure se si è verificato un errore durante la comunicazione
        /// col servizio.
        /// </summary>
        public event TaskExecutionCompletedHandler OnTaskExecutionCompleted;

        /// <summary>
        /// Questo evento viene generato quando è disponibile una nuova informazione sullo stato
        /// della richiesta di elaborazione inviata al servizio.
        /// </summary>
        public event TaskExecutionProgressHandler OnTaskExecutionProgress;

        /// <summary>
        /// Genera un nuovo evento per notificare il completamento della richiesta di elaborazione del task.
        /// </summary>
        /// <param name="cancelled">un valore che indica se il task è stato annullato</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="result">gli eventuali risultati scaricati dal server di elaborazione</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il patametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati dell'elaborazione di un task non sono disponibili
        /// a causa di un errore o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere
        /// impostato a null.
        /// </remarks>
        private void RaiseTaskExecutionCompleted(bool cancelled, Exception error, TaskResults result)
        {
            TaskExecutionCompletedHandler handler = OnTaskExecutionCompleted;
            if (handler != null)
            {
                TaskExecutionCompletedEventArgs args =
                    new TaskExecutionCompletedEventArgs(cancelled, error, result, m_TaskRequestId);
                OnTaskExecutionCompleted(this, args);
            }
        }
        
        /// <summary>
        /// Genera un nuovo evento per notificare il cambiamento di stato della richiesta di elaborazione del task.
        /// </summary>
        /// <param name="state">lo stato relativo alla richiesta di elaborazione</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il parametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati dell'elaborazione di un task non sono disponibili
        /// a causa di un errore o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere
        /// impostato a null.
        /// </remarks>
        private void RaiseTaskExecutionProgress(TaskExecutionState state, Exception error)
        {
            TaskExecutionProgressHandler handler = OnTaskExecutionProgress;
            if (handler != null)
            {
                TaskExecutionProgressEventArgs args =
                    new TaskExecutionProgressEventArgs(state, error, m_TaskRequestId);
                OnTaskExecutionProgress(this, args);
            }
        }

        #endregion

        #region Fields

        private readonly TimeSpan m_PollingInterval;
        private readonly ManualResetEvent m_PollingWaitHandle;

        private readonly BasicHttpBinding m_Binding;
        private readonly EndpointAddress m_Endpoint;

        private ProcessingServiceClient m_Proxy;
        private Dictionary<Type, TaskExecutionState> m_CommunicationErrorsMapping;

        private string m_TaskRequestId;
        private TaskData m_TaskData;
        private TaskResults m_TaskResults;

        #endregion

        #region Constructors

        /// <summary>
        /// Istanzia un nuovo oggetto della classe TaskExecution con i parametri specificati.
        /// </summary>
        /// <param name="serviceUri">indirizzo del servizio di elaborazione del task</param>
        /// <param name="pollingInterval">tempo di attesa nella verifica di completamento del task</param>
        /// <exception cref="UriFormatException">
        /// Se si specifica un URI non valido come indirizzo del servizio di elaborazione.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Se si specifica il valore null come indirizzo del servizio di elaborazione.
        /// </exception>
        public TaskExecution(string serviceUri, TimeSpan pollingInterval)
        {
            m_PollingInterval = pollingInterval;
            m_PollingWaitHandle = new ManualResetEvent(false);

            m_Binding = new BasicHttpBinding();
            m_Endpoint = new EndpointAddress(serviceUri);

            m_Proxy = null;
            m_CommunicationErrorsMapping = new Dictionary<Type, TaskExecutionState>()
            {
                { typeof(TimeoutException), TaskExecutionState.TimeoutError },
                { typeof(EndpointNotFoundException), TaskExecutionState.ServiceNotFoundError },
                { typeof(CommunicationException), TaskExecutionState.CommunicationError }
            };

            m_TaskRequestId = string.Empty;
            m_TaskData = new TaskData() { Name = string.Empty, Contents = string.Empty };
            m_TaskResults = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Imposta i dati relativi al task da elaborare.
        /// </summary>
        /// <param name="name">il nome da associare al task</param>
        /// <param name="contents">la stringa in formato XML contenente i dati del task</param>
        public void SetTaskData(string name, string contents)
        {
            m_TaskData.Name = name;
            m_TaskData.Contents = contents;
        }

        /// <summary>
        /// Determina l'avvio della procedura che permette di inviare il task specificato al servizio di elaborazione,
        /// di verificarne periodicamente il completamento e di eseguire alla fine il download dei relativi risultati
        /// eventualmente prodotti.
        /// </summary>
        /// <remarks>
        /// Prima di invocare questo metodo, è necessario utilizzare il metodo SetTaskData per poter impostare i dati
        /// relativi al task da elaborare.
        /// Inoltre, questo metodo non dovrebbe essere invocato sul thread della UI, in quanto la verifica periodica
        /// del completamento dell'elaborazione bloccherebbe tale thread per l'intervallo di polling specificato.
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
        /// Inizia la configurazione del proxy per la comunicazione col servizio di elaborazione.
        /// </summary>
        private void BeginOpenProxy()
        {
            RaiseTaskExecutionProgress(TaskExecutionState.InitializingProxy, null);

            m_Proxy = new ProcessingServiceClient(m_Binding, m_Endpoint);

            m_Proxy.OpenCompleted += new EventHandler<AsyncCompletedEventArgs>(Proxy_OpenCompleted);
            m_Proxy.CloseCompleted += new EventHandler<AsyncCompletedEventArgs>(Proxy_CloseCompleted);
            m_Proxy.SubmitDataCompleted += new EventHandler<SubmitDataCompletedEventArgs>(Proxy_SubmitDataCompleted);
            m_Proxy.GetStateCompleted += new EventHandler<GetStateCompletedEventArgs>(Proxy_GetStateCompleted);
            m_Proxy.GetResultsCompleted += new EventHandler<GetResultsCompletedEventArgs>(Proxy_GetResultsCompleted);
            
            m_Proxy.OpenAsync();
        }
        
        /// <summary>
        /// Inizia la chiusura del canale di comunicazione tra il proxy e il servizio.
        /// </summary>
        private void BeginCloseProxy()
        {
            RaiseTaskExecutionProgress(TaskExecutionState.DisposingProxy, null);

            m_Proxy.CloseAsync();
        }

        /// <summary>
        /// Invia i dati del task al servizio di elaborazione.
        /// </summary>
        private void BeginSubmitData()
        {
            RaiseTaskExecutionProgress(TaskExecutionState.SendingRequest, null);

            m_Proxy.SubmitDataAsync(m_TaskData);
        }

        /// <summary>
        /// Attende l'intervallo di tempo configurato per effettuare polling sullo stato del task, per interrogare
        /// successivamente il servizio di elaborazione e richiedere lo stato del task precedentemente inviato.
        /// </summary>
        private void BeginGetState()
        {
            if (!string.IsNullOrWhiteSpace(m_TaskRequestId))
            {
                m_PollingWaitHandle.WaitOne(m_PollingInterval);
                m_Proxy.GetStateAsync(m_TaskRequestId);
            }
        }

        /// <summary>
        /// Inizia il download dei risultati del task dal servizio di elaborazione.
        /// </summary>
        private void BeginGetResults()
        {
            RaiseTaskExecutionProgress(TaskExecutionState.DownloadingResults, null);

            if (!string.IsNullOrWhiteSpace(m_TaskRequestId))
            {
                m_Proxy.GetResultsAsync(m_TaskRequestId);
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
                m_Proxy.SubmitDataCompleted -= new EventHandler<SubmitDataCompletedEventArgs>(Proxy_SubmitDataCompleted);
                m_Proxy.GetStateCompleted -= new EventHandler<GetStateCompletedEventArgs>(Proxy_GetStateCompleted);
                m_Proxy.GetResultsCompleted -= new EventHandler<GetResultsCompletedEventArgs>(Proxy_GetResultsCompleted);
            }
        }

        #endregion

        #region Async Result Methods

        /// <summary>
        /// Questo metodo viene automaticamente invocato quando il proxy completa la sua configurazione, momento
        /// in cui può essere usato per iniziare la fase di invio dei dati del task al servizio di elaborazione.
        /// </summary>
        /// <param name="sender">l'oggetto che ha generato l'evento</param>
        /// <param name="args">informazioni aggiuntive sull'evento</param>
        private void Proxy_OpenCompleted(object sender, AsyncCompletedEventArgs args)
        {
            if (HandleCancellationIfRequired(args.Cancelled)) return;
            if (HandleErrorIfRequired(args.Error)) return;

            RaiseTaskExecutionProgress(TaskExecutionState.ProxyInitialized, null);

            BeginSubmitData();
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

            RaiseTaskExecutionProgress(TaskExecutionState.ProxyDisposed, null);

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
        private void Proxy_SubmitDataCompleted(object sender, SubmitDataCompletedEventArgs args)
        {
            if (HandleCancellationIfRequired(args.Cancelled)) return;
            if (HandleErrorIfRequired(args.Error)) return;

            m_TaskRequestId = args.Result;

            RaiseTaskExecutionProgress(TaskExecutionState.RequestSent, null);

            BeginGetState();
        }

        /// <summary>
        /// Questo metodo viene invocato nel momento in cui è disponibile un risultato sulla verifica dello stato:
        /// se il task risulta completato, viene avviata la fase di recupero dei risultati, altrimenti si attende
        /// un'altra verifica.
        /// </summary>
        /// <param name="sender">l'oggetto che ha generato l'evento</param>
        /// <param name="args">informazioni aggiuntive sull'evento</param>
        private void Proxy_GetStateCompleted(object sender, GetStateCompletedEventArgs args)
        {
            if (HandleCancellationIfRequired(args.Cancelled)) return;
            if (HandleErrorIfRequired(args.Error)) return;

            TaskState ts = args.Result;
            switch (ts)
            {
                case TaskState.Completed:
                    BeginGetResults();
                    break;

                case TaskState.None:
                    BeginCloseProxy();
                    break;

                default:
                    BeginGetState();
                    break;
            }
        }

        /// <summary>
        /// Questo metodo viene invocato nel momento in cui gli eventuali risultati dell'elaborazione sono stati
        /// ricevuti: in tal caso viene segnalato il completamento dell'elaborazione e richiesta la chiusura del
        /// canale di comunicazione.
        /// </summary>
        /// <param name="sender">l'oggetto che ha generato l'evento</param>
        /// <param name="args">informazioni aggiuntive sull'evento</param>
        private void Proxy_GetResultsCompleted(object sender, GetResultsCompletedEventArgs args)
        {
            if (HandleCancellationIfRequired(args.Cancelled)) return;
            if (HandleErrorIfRequired(args.Error)) return;

            RaiseTaskExecutionProgress(TaskExecutionState.ResultsDownloaded, null);

            m_TaskResults = args.Result;

            RaiseTaskExecutionCompleted(false, null, m_TaskResults);
            BeginCloseProxy();
        }

        #endregion
        
        #region Error/Cancellation Handling

        /// <summary>
        /// Permette di gestire la cancellazione di una richiesta e restituisce true se è stata gestita chiudendo
        /// il proxy, oppure false se non è stato necessario gestirla perché non è avvenuta alcuna cancellazione.
        /// </summary>
        /// <param name="cancelled">un valore che indica se la richiesta di elaborazione è stata annullata o meno</param>
        /// <returns>true se l'eventuale cancellazione della richiesta è stata gestita, altrimenti false</returns>
        private bool HandleCancellationIfRequired(bool cancelled)
        {
            if (cancelled)
            {
                RaiseTaskExecutionProgress(TaskExecutionState.RequestCancelled, null);
                RaiseTaskExecutionCompleted(true, null, null);
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
                TaskExecutionState state;
                if (m_CommunicationErrorsMapping.TryGetValue(error.GetType(), out state))
                {
                    RaiseTaskExecutionProgress(state, error);

                    m_Proxy.Abort();
                    RaiseTaskExecutionProgress(TaskExecutionState.ProxyDisposed, error);

                    UnregisterProxyEvents();
                    m_Proxy = null;
                }
                else
                {
                    if (error is FaultException<ServiceFault>)
                    {
                        RaiseTaskExecutionProgress(TaskExecutionState.OperationError, error);
                        BeginCloseProxy();
                    }
                    else
                    {
                        RaiseTaskExecutionProgress(TaskExecutionState.UnknownError, error);

                        m_Proxy.Abort();
                        RaiseTaskExecutionProgress(TaskExecutionState.ProxyDisposed, error);

                        UnregisterProxyEvents();
                        m_Proxy = null;
                    }
                }

                RaiseTaskExecutionCompleted(false, error, null);

                return true;
            }

            return false;
        }

        #endregion
    }
}
