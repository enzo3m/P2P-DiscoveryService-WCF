using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CalcServerFinder.Core.Extensions;
using CalcServerFinder.Contracts;
using CalcServerFinder.Logging;
using CalcServerFinder.Configuration;

namespace CalcServerFinder.Core
{
    #region SearchData

    /// <summary>
    /// Definisce i dati richiesti per avviare la ricerca di una risorsa di calcolo.
    /// </summary>
    public class SearchData : IEquatable<SearchData>
    {
        #region Fields

        private readonly string m_ResourceName;
        private readonly string m_ResourceVersion;
        private readonly int m_HashCode;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public SearchData(SearchOptions options)
        {
            m_ResourceName = (options != null ? options.Name : string.Empty);
            m_ResourceVersion = (options != null ? options.Version : string.Empty);
            m_HashCode = (m_ResourceName + m_ResourceVersion).GetHashCode();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Il nome completo della classe che implementa la risorsa di calcolo cercata.
        /// </summary>
        public string Name { get { return m_ResourceName; } }

        /// <summary>
        /// La versione della classe che implementa la risorsa di calcolo cercata.
        /// </summary>
        public string Version { get { return m_ResourceVersion; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Crea una nuova istanza di SearchData utilizzando l'istanza di SearchOptions specificata.
        /// </summary>
        /// <param name="options">Le opzioni di ricerca ricevute da un client.</param>
        /// <param name="data">I dati di ricerca ricavati dalle opzioni di ricerca.</param>
        /// <returns>true se i dati di ricerca vengono ricavati correttamente; in caso contrario, false.</returns>
        public static bool TryCreate(SearchOptions options, out SearchData data)
        {
            if (options != null)
            {
                data = new SearchData(options);
                return true;
            }

            data = default(SearchData);
            return false;
        }

        /// <summary>
        /// Restituisce le opzioni di ricerca utilizzate per costruire questo oggetto.
        /// </summary>
        /// <returns>le opzioni di ricerca utilizzate per costruire questo oggetto.</returns>
        public SearchOptions GetSearchOptions()
        {
            return new SearchOptions()
            {
                Name = this.Name,
                Version = this.Version
            };
        }

        #endregion

        #region IEquatable Implementation

        /// <summary>
        /// Determina se questa istanza e un altro oggetto SearchParams specificato hanno lo stesso valore.
        /// </summary>
        /// <param name="other">l'oggetto con cui confrontare questa istanza</param>
        /// <returns>true se l'oggetto corrente è uguale al parametro other; in caso contrario, false.</returns>
        public bool Equals(SearchData other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return (Name.Equals(other.Name) && Version.Equals(other.Version));
        }

        #endregion

        #region Override

        /// <summary>
        /// Restituisce la rappresentazione in formato stringa di questo oggetto, ottenuta concatenando
        /// le proprietà Name e Version e separandole con un singolo carattere trattino-meno.
        /// </summary>
        /// <returns>la rappresentazione in formato stringa di questo oggetto</returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", Name, Version);
        }

        /// <summary>
        /// Restituisce il codice hash di questo oggetto.
        /// </summary>
        /// <returns>il codice hash di questo oggetto</returns>
        public override int GetHashCode()
        {
            return m_HashCode;
        }

        /// <summary>
        /// Determina se questa istanza e un oggetto specificato, che deve essere anche un oggetto SearchParams,
        /// hanno lo stesso valore.
        /// </summary>
        /// <param name="obj">oggetto SearchParams da confrontare con questa istanza</param>
        /// <returns>
        /// true se il parametro obj è un oggetto SearchParams e il relativo valore corrisponde a quello
        /// di questa istanza; in caso contrario false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SearchData)) return false;
            return Equals((SearchData)obj);
        }

        #endregion

        #region Overloading

        public static bool operator ==(SearchData left, SearchData right)
        {
            if (object.ReferenceEquals(left, right)) return true;
            if (object.ReferenceEquals(left, null)) return false;
            if (object.ReferenceEquals(right, null)) return false;

            return left.Equals(right);
        }

        public static bool operator !=(SearchData left, SearchData right)
        {
            if (object.ReferenceEquals(left, right)) return false;
            if (object.ReferenceEquals(left, null)) return true;
            if (object.ReferenceEquals(right, null)) return true;

            return !left.Equals(right);
        }

        #endregion
    }

    #endregion

    #region SearchResult

    /// <summary>
    /// Contiene i risultati di una ricerca sui servizi di elaborazione compatibili con i dati di ricerca, quindi
    /// l'elenco degli eventuali uri con cui è possibile contattare tali servizi e l'istante in cui è iniziata la
    /// ricerca sul servent.
    /// </summary>
    public class SearchResult
    {
        #region Fields

        private DateTime m_StartingTime;

        private HashSet<Uri> m_FoundServices;

        #endregion

        #region Properties

        /// <summary>
        /// L'istante di inizio della ricerca sul servent.
        /// </summary>
        public DateTime StartingTime
        {
            get { return m_StartingTime; }
            private set { m_StartingTime = value; }
        }

        /// <summary>
        /// L'elenco degli uri relativi ai servizi di elaborazione trovati.
        /// </summary>
        public IEnumerable<Uri> FoundServices
        {
            get { return m_FoundServices; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Crea una nuova istanza di questa classe con i parametri specificati.
        /// </summary>
        /// <param name="services">L'elenco degli uri relativi ai servizi di calcolo trovati.</param>
        /// <param name="starting">L'istante di inizio della ricerca da parte del client.</param>
        public SearchResult(IEnumerable<Uri> services, DateTime starting)
        {
            m_FoundServices = (services != null ? new HashSet<Uri>(services) : new HashSet<Uri>());
            StartingTime = starting;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Aggiunge l'uri specificato a questo oggetto contenente i risultati di una ricerca e restituisce true
        /// se l'uri specificato è stato aggiunto, altrimenti false se l'uri specificato era già presente oppure
        /// se è stato specificato null.
        /// </summary>
        /// <param name="service">L'uri relativo ad un servizio di elaborazione.</param>
        /// <returns>true se l'uri specificato è stato aggiunto; in caso contrario, false.</returns>
        public bool Add(Uri service)
        {
            return (service != null ? m_FoundServices.Add(service) : false);
        }

        /// <summary>
        /// Aggiunge l'insieme specificato di uri a questo oggetto contenente i risultati di una ricerca e restituisce
        /// true se è stato aggiunto almeno un uri, altrimenti false se tutti gli uri specificati erano già presenti o
        /// se è stato specificato null.
        /// </summary>
        /// <param name="services">L'insieme di uri relativi ad più servizi di elaborazione.</param>
        /// <returns>true se almeno un uri è stato aggiunto; in caso contrario, false.</returns>
        public bool Add(IEnumerable<Uri> services)
        {
            if (services == null) return false;

            int itemsCountBefore = m_FoundServices.Count;

            foreach (var uri in services)
            {
                m_FoundServices.Add(uri);
            }

            return (itemsCountBefore < m_FoundServices.Count);
        }

        /// <summary>
        /// Rimuove l'uri specificato da questo oggetto contenente i risultati di una ricerca e restituisce true
        /// se l'uri specificato è stato rimosso, altrimenti false se l'uri specificato non era presente oppure
        /// se è stato specificato null.
        /// </summary>
        /// <param name="service">L'uri relativo ad un servizio di elaborazione.</param>
        /// <returns>true se l'uri specificato è stato rimosso; in caso contrario, false.</returns>
        public bool Remove(Uri service)
        {
            return (service != null ? m_FoundServices.Remove(service) : false);
        }

        /// <summary>
        /// Restituisce una copia profonda di questo oggetto.
        /// </summary>
        /// <returns>una copia profonda di questo oggetto.</returns>
        public SearchResult Copy()
        {
            return new SearchResult(m_FoundServices.ToArray(), m_StartingTime);
        }

        #endregion
    }

    #endregion

    #region SearchManager

    /// <summary>
    /// Questa classe permette di gestire i risultati di ricerca ricevuti dai vicini, memorizzandoli all'interno di
    /// una cache.
    /// Inoltre, si occupa di effettuare anche la pulizia periodica della cache, rimuovendo le ricerche troppo vecchie.
    /// </summary>
    public sealed class SearchManager
    {
        #region Fields

        // --- istanze degli oggetti globali
        private readonly Settings m_AppConfig = Settings.Instance;
        private readonly Logger m_AppLogger = Logger.Instance;
        
        // --- impostazioni
        private readonly TimeSpan m_ExpiryInterval;

        // --- cache di ricerca
        private readonly object m_CacheLocker = new object();
        private readonly Dictionary<SearchData, SearchResult> m_Cache = new Dictionary<SearchData, SearchResult>();

        // --- timer per rimuovere periodicamente gli elementi scaduti
        private readonly TimeSpan m_CacheCleaningPeriod;
        private readonly Timer m_CleaningTimer = null;
        private readonly object m_CleaningCallbackLocker = new object();
        
        #endregion

        #region Singleton

        private static readonly SearchManager m_Instance = new SearchManager();

        /// <summary>
        /// Costruttore privato non accessibile.
        /// </summary>
        private SearchManager()
        {
            m_ExpiryInterval = m_AppConfig.SearchExpiryInterval;
            m_CacheCleaningPeriod = m_AppConfig.SearchEngineCleaningPeriod;
            m_CleaningTimer = new Timer(CleaningTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Ottiene il riferimento all'unica istanza di questo SearchManager.
        /// </summary>
        public static SearchManager Instance { get { return m_Instance; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Avvia la verifica periodica delle ricerche più vecchie per poterle rimuovere.
        /// </summary>
        public void Run()
        {
            m_CleaningTimer.Change(m_CacheCleaningPeriod, m_CacheCleaningPeriod);
        }

        /// <summary>
        /// Ferma la verifica periodica degli ricerche più vecchie da eliminare.
        /// </summary>
        /// <returns>Viene restituito true se la funzione viene eseguita correttamente; in caso contrario false.</returns>
        public bool Dispose()
        {
            return Dispose(TimeSpan.Zero);
        }

        /// <summary>
        /// Ferma la verifica periodica degli ricerche più vecchie da eliminare.
        /// </summary>
        /// <param name="waitingTimeLimit">L'intervallo massimo di attesa.</param>
        /// <returns>Viene restituito true se la funzione viene eseguita correttamente; in caso contrario false.</returns>
        public bool Dispose(TimeSpan waitingTimeLimit)
        {
            ManualResetEvent notifyHandle = new ManualResetEvent(false);
            m_CleaningTimer.Dispose(notifyHandle);
            return notifyHandle.WaitOne(waitingTimeLimit);
        }
        
        /// <summary>
        /// Verifica che i dati di ricerca specificati non corrispondano ad una ricerca esistente non ancora scaduta
        /// ed eventualmente li mette in coda e restituisce true. Qualora il parametro specificato in ingresso è una
        /// ricerca esistente e non scaduta, restituisce false per indicare che non sono stati inseriti nuovi dati e
        /// imposta in uscita una copia dei risultati finora trovati. Infine, se si specifica null come parametro di
        /// ingresso relativo ai dati di ricerca, questo metodo restituisce false e imposta a null i risultati.
        /// </summary>
        /// <param name="data">I dati che rappresentano la ricerca da aggiungere al gestore delle ricerche.</param>
        /// <param name="result">Gli eventuali risultati di ricerca non scaduti, copiati all'uscita di questo metodo.</param>
        /// <returns>true se è stata aggiunta una nuova ricerca; in caso contrario, false.</returns>
        public bool TryEnqueueNewSearch(SearchData data, out SearchResult result)
        {
            if (data == null)
            {
                result = null;
                return false;
            }

            lock (m_CacheLocker)
            {
                SearchResult temp;
                if (TryGetResult(data, out temp))
                {
                    result = temp.Copy();
                    return false;   // ricerca già esistente e non scaduta
                }

                m_Cache[data] = new SearchResult(Enumerable.Empty<Uri>(), DateTime.Now);
            }

            result = null;
            return true;
        }

        /// <summary>
        /// Aggiorna i risultati associati ai dati di ricerca specificati, aggiungendo gli uri specificati all'insieme
        /// di quelli finora trovati e restituendo true se i dati di ricerca specificati esistono nella cache e quindi
        /// è stato possibile aggiornare i risultati associati; altrimenti, cioè se i dati di ricerca non esistono, ad
        /// esempio perché sono scaduti e/o sono stati rimossi, restituisce false.
        /// </summary>
        /// <param name="data">I dati che rappresentano univocamente la ricerca da aggiornare con i nuovi risultati.</param>
        /// <param name="services">L'elenco degli uri da aggiungere ai risultati della ricerca specificata.</param>
        /// <returns>true se la ricerca specificata esiste ed è stata aggiornata; in caso contrario, false.</returns>
        public bool UpdateResult(SearchData data, IEnumerable<Uri> services)
        {
            if (data == null) return false;

            lock (m_CacheLocker)
            {
                SearchResult temp;
                if (TryGetResult(data, out temp))
                {
                    if (services != null)
                    {
                        temp.Add(services);
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se i dati di ricerca specificati sono associati ad eventuali risultati non scaduti ed in tal caso
        /// ne imposta una copia in uscita e restituisce true. Qualora i dati di ricerca specificati dovessero risultare
        /// scaduti, li elimina e restituisce false; analogamente, se i dati specificati non esistono nella cache oppure
        /// se è stato specificato un valore null, questo metodo restituisce false.
        /// </summary>
        /// <param name="data">I dati che rappresentano univocamente la ricerca associata ai risultati richiesti.</param>
        /// <param name="result">Gli eventuali risultati di ricerca non scaduti, copiati all'uscita di questo metodo.</param>
        /// <returns>true se i dati di ricerca specificati sono associati a risultati non scaduti; altrimenti false.</returns>
        public bool TryGetResultCopy(SearchData data, out SearchResult result)
        {
            if (data == null)
            {
                result = null;
                return false;
            }

            lock (m_CacheLocker)
            {
                SearchResult temp;
                if (TryGetResult(data, out temp))
                {
                    result = temp.Copy();
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Rimuove la ricerca indicizzata dai dati specificati e restituisce true se è stata rimossa con successo,
        /// altrimenti false se i dati specificati non erano presenti.
        /// </summary>
        /// <param name="data">I dati che rappresentano univocamente la ricerca da rimuovere.</param>
        /// <returns>true se la ricerca specificata esiste ed è stata rimossa; in caso contrario, false.</returns>
        public bool Remove(SearchData data)
        {
            if (data == null) return false;

            lock (m_CacheLocker)
            {
                return m_Cache.Remove(data);
            }
        }

        #endregion

        #region Utility Private Methods

        /// <summary>
        /// Questo metodo privato accede alla cache delle ricerche senza effettuare alcun lock perché ha l'unico scopo
        /// di verificare se i dati di ricerca specificati sono associati ad eventuali risultati non scaduti ed in tal
        /// caso li imposta in uscita e restituisce true: in sostanza è un metodo di utilità invocato da alcuni metodi
        /// di questa classe che richiedono l'accesso a risultati di ricerca non scaduti.
        /// Qualora i dati di ricerca specificati dovessero risultare scaduti, li elimina e restituisce false; in modo
        /// analogo, se i dati specificati non esistono all'interno della cache, questo metodo restituisce false.
        /// </summary>
        /// <param name="data">I dati che rappresentano univocamente la ricerca associata ai risultati richiesti.</param>
        /// <param name="result">Gli eventuali risultati di ricerca non scaduti, impostati quando termina questo metodo.</param>
        /// <returns>true se i dati di ricerca specificati sono associati a risultati non scaduti; altrimenti false.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se si specifica null nel parametro di ingresso relativo ai dati di ricerca.
        /// </exception>
        /// <remarks>
        /// NOTA IMPORTANTE: questo metodo non effettua un lock durante l'accesso alla cache, poiché è stato pensato
        /// per essere invocato da tutti gli altri metodi di questa classe che hanno bisogno di accedere ai risultati
        /// di una ricerca, così che non usino mai ricerche scadute, pertanto occorre che effettuino un lock prima di
        /// invocarlo.
        /// </remarks>
        private bool TryGetResult(SearchData data, out SearchResult result)
        {
            SearchResult temp;
            if (m_Cache.TryGetValue(data, out temp))
            {
                if (temp == null || temp.StartingTime + m_ExpiryInterval < DateTime.Now)
                {
                    // elemento non valido o scaduto
                    m_Cache.Remove(data);
                }
                else
                {
                    result = temp;
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Rimuove le ricerche scadute, ovvero quelle il cui istante di inizio, rispetto all'istante corrente,
        /// ha una distanza temporale maggiore dell'intervallo di tempo stabilito come scadenza per un elemento
        /// della cache di ricerca.
        /// Nessun elemento verrà eliminato da questa cache qualora l'intervallo di tempo stabilito è maggiore
        /// della differenza tra l'istante attuale e il valore minimo rappresentabile con un oggetto DateTime.
        /// </summary>
        /// <remarks>
        /// NOTA IMPORTANTE: questo metodo effettua un lock durante la pulizia della cache, cioè dalla verifica
        /// degli elementi scaduti e fino alla rimozione degli stessi, pertanto tutti i metodi di questa classe
        /// non devono effettuare il lock sulla cache, poiché si verificherebbe un deadlock.
        /// </remarks>
        private void Clean()
        {
            DateTime startingTimeLimit;
            if (!DateTime.Now.TrySubtract(m_ExpiryInterval, out startingTimeLimit)) return;

            int removedSearchTasksCount;
            int pendingSearchTasksCount;

            lock (m_CacheLocker)
            {
                List<SearchData> keysToRemove = new List<SearchData>();

                foreach (var item in m_Cache)
                {
                    SearchResult searchResult = item.Value;
                    if (startingTimeLimit > searchResult.StartingTime)
                    {
                        keysToRemove.Add(item.Key);   // elemento scaduto
                    }
                }

                foreach (var key in keysToRemove)
                {
                    m_Cache.Remove(key);
                }

                removedSearchTasksCount = keysToRemove.Count;
                pendingSearchTasksCount = m_Cache.Count;
            }

            WriteToLog("Removal of older search tasks: removed {0} items. Pending search tasks: {1}.",
                removedSearchTasksCount, pendingSearchTasksCount);
        }

        #endregion

        #region Timer Callback

        /// <summary>
        /// Metodo di callback invocato periodicamente per pulire la cache di ricerca tramite rimozione di tutti
        /// gli elementi scaduti in essa presenti.
        /// </summary>
        /// <param name="state">L'eventuale oggetto specificato nel costruttore dell'oggetto Timer.</param>
        /// <remarks>
        /// Questa callback crea un'istanza di System.Threading.Tasks.Task che permette di eseguire il metodo di
        /// pulizia in un thread di background.
        /// </remarks>
        private void CleaningTimerCallback(object state)
        {
            if (Monitor.TryEnter(m_CleaningCallbackLocker))   // no callback sovrapposte
            {
                try
                {
                    // Elimina i risultati di ricerca troppo vecchi.
                    Task.Factory.StartNew(() => Clean(),
                            CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                finally
                {
                    Monitor.Exit(m_CleaningCallbackLocker);
                }
            }
        }

        #endregion

        #region Logging Private Methods

        /// <summary>
        /// Permette l'accesso alle funzionalità di logging da parte dei metodi di questa classe,
        /// aggiungendo informazioni accessorie in aggiunta alla descrizione specificata.
        /// </summary>
        /// <param name="format">Una stringa in formato composito.</param>
        /// <param name="args">Un array di oggetti contenente zero o più oggetti da formattare.</param>
        private void WriteToLog(string format, params object[] args)
        {
            DateTime logTime = DateTime.Now;
            string moduleInfo = string.Format("{0}. Thread: {1}.",
                GetType().FullName.ToString(),   // oppure: GetType().AssemblyQualifiedName.ToString(),
                Thread.CurrentThread.ManagedThreadId);
            string textToLog = string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args);

            m_AppLogger.Write(logTime, moduleInfo, textToLog);
        }

        #endregion
    }

    #endregion
}
