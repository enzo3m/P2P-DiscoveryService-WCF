using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;

using CalcServerFinder.Configuration;
using CalcServerFinder.Logging;

namespace CalcServerFinder.Core
{
    #region ResourceCacheSelection

    /// <summary>
    /// Delegato per effettuare la selezione degli elementi della ResourceCache in base all'uri del server di calcolo
    /// e/o alle informazioni sulle risorse di calcolo da esso fornite.
    /// </summary>
    /// <param name="uri">L'uri da verificare.</param>
    /// <param name="resources">L'elenco delle informazioni da verificare.</param>
    /// <returns>true se si vuole selezionare l'elemento, false altrimenti.</returns>
    public delegate bool ResourceCacheSelection(Uri uri, IEnumerable<TaskPerformerInfo> resources);

    #endregion

    #region ResourceCache

    /// <summary>
    /// Rappresenta una cache a cui è possibile aggiungere le informazioni relative alle risorse di calcolo
    /// fornite dai server di calcolo connessi a questa applicazione: tali informazioni vengono indicizzate
    /// tramite gli uri dei servizi attivi sui predetti server.
    /// Inoltre, sfruttando il pattern singleton in versione thread-safe, assicura un accesso centralizzato
    /// alle funzionalità di caching, poiché fornisce un'istanza thread-safe e unica nell'applicazione.
    /// </summary>
    /// <remarks>
    /// L'aggiornamento di questa cache avviene grazie a un thread separato, implementato all'interno della
    /// classe ResourceCacheUpdater, che interroga periodicamente i vari server di elaborazione associati a
    /// questa istanza di applicazione e che li rimuove quando non forniscono nessuna risposta per un certo
    /// numero di volte.
    /// </remarks>
    public sealed class ResourceCache
    {
        #region ValueHolder

        /// <summary>
        /// Permette di associare all'uri del server di calcolo non solo l'elenco delle risorse di elaborazione fornite,
        /// ma anche l'istante più recente di rilevazione del server, cioè l'istante dell'ultima risposta ricevuta.
        /// </summary>
        private class ValueHolder
        {
            #region Fields

            private DateTime m_LastDetection;

            private HashSet<TaskPerformerInfo> m_ResourcesList;

            private uint m_RefreshFailuresCount;

            #endregion

            #region Properties

            /// <summary>
            /// L'istante di più recente rilevazione del server di calcolo.
            /// </summary>
            public DateTime LastDetection
            {
                get { return m_LastDetection; }
                private set { m_LastDetection = value; }
            }

            /// <summary>
            /// L'elenco delle informazioni sulle risorse di calcolo fornite dall'uri.
            /// </summary>
            public IEnumerable<TaskPerformerInfo> ResourcesList
            {
                get { return m_ResourcesList; }
                private set { m_ResourcesList = new HashSet<TaskPerformerInfo>(value); }
            }

            /// <summary>
            /// Il numero di tentativi consecutivi di aggiornamento falliti.
            /// </summary>
            public uint RefreshFailuresCount
            {
                get { return m_RefreshFailuresCount; }
                set { m_RefreshFailuresCount = value; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Crea una nuova istanza di questa classe con i parametri specificati.
            /// </summary>
            /// <param name="resources">L'elenco delle informazioni sulle risorse di calcolo fornite dall'uri.</param>
            /// <param name="detection">L'istante più recente in cui il server di calcolo è stato rilevato.</param>
            public ValueHolder(IEnumerable<TaskPerformerInfo> resources, DateTime detection)
            {
                ResourcesList = resources;
                LastDetection = detection;
                RefreshFailuresCount = 0;
            }

            #endregion
        }

        #endregion

        #region Singleton

        private static readonly ResourceCache m_Instance = new ResourceCache();

        /// <summary>
        /// Costruttore privato non accessibile.
        /// </summary>
        private ResourceCache() { ; }

        /// <summary>
        /// Ottiene il riferimento all'unica istanza di questa ResourceCache.
        /// </summary>
        public static ResourceCache Instance { get { return m_Instance; } }

        #endregion

        #region Fields

        private readonly object m_CacheLock = new object();
        private readonly Dictionary<Uri, ValueHolder> m_Cache = new Dictionary<Uri, ValueHolder>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Aggiorna le informazioni sulle risorse di calcolo associate all'uri specificato, sostituendo con quelle
        /// specificate quelle attualmente memorizzate in questa cache, modificando l'istante di ultima rilevazione
        /// con il valore specificato e azzerando infine il numero degli eventuali precedenti tentativi consecutivi
        /// falliti di aggiornamento.
        /// </summary>
        /// <param name="uri">L'uri da conservare nella cache o da aggiornare con le nuove informazioni.</param>
        /// <param name="detection">L'istante di più recente rilevazione del server di calcolo.</param>
        /// <param name="resources">Le informazioni sulle risorse di calcolo associate all'uri.</param>
        /// <remarks>
        /// Qualora si dovesse specificare null per almeno uno dei primi due parametri, la cache non subirà nessuna
        /// modifica.
        /// </remarks>
        public void UpdateOnDetection(Uri uri, IEnumerable<TaskPerformerInfo> resources, DateTime detection)
        {
            if (uri == null || resources == null) return;

            lock (m_CacheLock)
            {
                m_Cache[uri] = new ValueHolder(resources, detection);
            }
        }

        /// <summary>
        /// Aggiorna le informazioni sulle risorse di calcolo associate all'uri specificato, in particolare incrementa
        /// il numero di tentativi consecutivi falliti di aggiornamento, verifica se il valore incrementato è maggiore
        /// del limite specificato ed in caso affermativo rimuove l'elemento dalla cache e restituisce true.
        /// </summary>
        /// <param name="uri">L'uri candidato per l'eventuale rimozione o per l'incremento degli errori di aggiornamento.</param>
        /// <param name="refreshFailuresLimit">Il numero massimo di tentativi consecutivi falliti di aggiornamento.</param>
        /// <returns>true se l'uri specificato è stato rimosso; in caso contrario false.</returns>
        /// <remarks>
        /// Qualora si dovesse specificare null per il primo parametro, la cache non subirà nessuna modifica.
        /// </remarks>
        public bool UpdateOnFailure(Uri uri, uint refreshFailuresLimit)
        {
            if (uri == null) return false;

            lock (m_CacheLock)
            {
                ValueHolder temp;
                if (m_Cache.TryGetValue(uri, out temp))
                {
                    if (temp.RefreshFailuresCount < refreshFailuresLimit)
                    {
                        temp.RefreshFailuresCount++;
                    }

                    if (temp.RefreshFailuresCount == refreshFailuresLimit)
                    {
                        return m_Cache.Remove(uri);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Cerca gli elementi che soddisfano i criteri di ricerca specificati e, dai risultati ottenuti, restituisce
        /// una copia degli uri.
        /// </summary>
        /// <param name="select">I criteri di ricerca basati sull'uri e sulle informazioni ad esso associate.</param>
        /// <returns>una copia degli uri relativi agli elementi che soddisfano i criteri di ricerca specificati</returns>
        public IEnumerable<Uri> Search(ResourceCacheSelection select)
        {
            List<Uri> result = new List<Uri>();

            if (select != null)
            {
                lock (m_CacheLock)
                {
                    foreach (var item in m_Cache)
                    {
                        if (select(item.Key, item.Value.ResourcesList))
                        {
                            result.Add(item.Key);
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }

    #endregion

    #region ResourceCacheUpdater

    /// <summary>
    /// Questa classe permette di interrogare periodicamente i server di calcolo configurati con quest'applicazione,
    /// ovvero i server di calcolo noti a questa istanza dell'applicazione, con lo scopo di mantenere aggiornata la
    /// cache contenente le informazioni sulle risorse di calcolo disponibili su tali server. Inoltre, si occupa di
    /// effettuare anche la pulizia della cache, rimuovendo gli elementi scaduti.
    /// </summary>
    /// <remarks>
    /// I tempi di aggiornamento della cache vengono recuperati dalle impostazioni di questa istanza di applicazione:
    /// il periodo di refresh stabilisce quanto spesso debbano essere interrogati i server di elaborazione, affinché
    /// le informazioni contenute in cache siano mantenute aggiornate; l'intervallo di scadenza degli elementi nella
    /// cache è l'intervallo di tempo per il quale si può supporre che l'informazione rimanga valida: dal momento in
    /// cui questo intervallo viene superato, il ResourceCacheUpdater rimuoverà l'informazione durante il successivo
    /// aggiornamento, a meno che gli elementi scaduti non vengano aggiornati nuovamente.
    /// </remarks>
    public sealed class ResourceCacheUpdater
    {
        #region Fields

        // --- istanze degli oggetti singleton
        private readonly Settings m_AppConfig = Settings.Instance;
        private readonly Logger m_AppLogger = Logger.Instance;
        private readonly ResourceCache m_ResourceCache = ResourceCache.Instance;

        // --- impostazioni di aggiornamento e contatori
        private readonly TimeSpan m_RefreshPeriod;
        private readonly uint m_RefreshFailuresLimit;

        // --- oggetti per interrogare i server
        private readonly List<ProcessingServiceMonitor> m_ResourceMonitors;

        // --- timer per interrogare periodicamente i server
        private readonly Timer m_UpdateTimer;
        private readonly object m_UpdateCallbackLocker = new object();
        
        #endregion

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza della classe ResourceCacheUpdater che permette di interrogare periodicamente i server
        /// di calcolo (noti a questa istanza dell'applicazione), in modo da mantenere aggiornata la cache contenente le
        /// informazioni sulle risorse di calcolo disponibili su tali server.
        /// </summary>
        /// <remarks>
        /// Le impostazioni dei proxy usati per comunicare con i server di calcolo noti, inclusi gli indirizzi di questi
        /// ultimi, vengono recuperate tramite la classe Settings.
        /// </remarks>
        public ResourceCacheUpdater()
        {
            m_RefreshPeriod = m_AppConfig.ResourceCacheRefreshPeriod;
            m_RefreshFailuresLimit = m_AppConfig.ResourceCacheRefreshFailuresLimit;
            
            Binding binding = new BasicHttpBinding()
            {
                OpenTimeout = m_AppConfig.ResourceMonitorOpenTimeout,
                CloseTimeout = m_AppConfig.ResourceMonitorCloseTimeout
            };

            m_ResourceMonitors = new List<ProcessingServiceMonitor>();
            foreach (var uri in m_AppConfig.ResourceMonitorEndpoints)
                m_ResourceMonitors.Add(new ProcessingServiceMonitor(binding, uri));

            m_UpdateTimer = new Timer(UpdateTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Avvia il monitoraggio periodico dei server di calcolo noti.
        /// </summary>
        public void Run()
        {
            m_UpdateTimer.Change(TimeSpan.Zero, m_RefreshPeriod);
        }

        /// <summary>
        /// Ferma il monitoraggio periodico dei server di calcolo noti.
        /// </summary>
        /// <returns>Viene restituito true se la funzione viene eseguita correttamente; in caso contrario false.</returns>
        public bool Dispose()
        {
            return Dispose(TimeSpan.Zero);
        }

        /// <summary>
        /// Ferma il monitoraggio periodico dei server di calcolo.
        /// </summary>
        /// <param name="waitingTimeLimit">L'intervallo massimo di attesa.</param>
        /// <returns>Viene restituito true se la funzione viene eseguita correttamente; in caso contrario false.</returns>
        public bool Dispose(TimeSpan waitingTimeLimit)
        {
            ManualResetEvent notifyHandle = new ManualResetEvent(false);
            m_UpdateTimer.Dispose(notifyHandle);   // disabilita il timer

            bool result = notifyHandle.WaitOne(waitingTimeLimit);
            
            foreach (var monitor in m_ResourceMonitors)
                monitor.Close();   // rilascia le risorse dei proxy

            return result;
        }

        #endregion

        #region Timer Callback

        /// <summary>
        /// Metodo di callback invocato periodicamente per interrogare i server di calcolo noti a questa istanza
        /// dell'applicazione, con l'obiettivo di mantenere aggiornata la cache delle risorse di calcolo.
        /// </summary>
        /// <param name="state">L'eventuale oggetto specificato nel costruttore dell'oggetto Timer.</param>
        /// <remarks>
        /// Questa callback crea un'istanza di System.Threading.Tasks.Task per ognuno degli indirizzo dei server
        /// di calcolo noti e le avvia, in modo da eseguire la procedura di aggiornamento su uno o più thread di
        /// background.
        /// </remarks>
        private void UpdateTimerCallback(object state)
        {
            if (Monitor.TryEnter(m_UpdateCallbackLocker))   // no callback sovrapposte
            {
                try
                {
                    // Aggiorna la cache con le informazioni ricevute dai server di elaborazione.
                    foreach (var monitor in m_ResourceMonitors)
                    {
                        ProcessingServiceMonitor monitorTemp = monitor;

                        Task.Factory.StartNew(() => Refresh(monitorTemp),
                            CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    }
                }
                finally
                {
                    Monitor.Exit(m_UpdateCallbackLocker);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Aggiorna la cache interrogando il server di calcolo specificato, con lo scopo di ricevere le informazioni
        /// sulle risorse di calcolo disponibili, cioè l'elenco dei task performer attivi sul server.
        /// </summary>
        /// <param name="monitor">
        /// Il riferimento all'oggetto che, tramite un proxy, interroga il server in modo da ottenere le informazioni
        /// richieste e conseguentemente aggiornare la cache delle risorse di calcolo.
        /// </param>
        private void Refresh(ProcessingServiceMonitor monitor)
        {
            IEnumerable<TaskPerformerInfo> resources = null;
            DateTime detection;

            if (monitor.TryGetResources(out resources, out detection))
            {
                m_ResourceCache.UpdateOnDetection(monitor.RemoteUri, resources, detection);   // aggiorna la cache

                WriteToLog("Update: uri = {0}, detection = {1:o}, resources = [{2}].", monitor.RemoteUri,
                    detection.ToUniversalTime(), string.Join<TaskPerformerInfo>(", ", resources));
            }
            else
            {
                if (m_ResourceCache.UpdateOnFailure(monitor.RemoteUri, m_RefreshFailuresLimit))   // aggiorna la cache
                {
                    WriteToLog("No update received from {0} ...removed from cache.", monitor.RemoteUri);
                }
                else
                {
                    WriteToLog("No update received from {0}.", monitor.RemoteUri);
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
