using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalcServerFinder.Configuration
{
    /// <summary>
    /// Conserva tutte le impostazioni dell'applicazione, semplificandone l'accesso da parte dei vari moduli.
    /// Questa classe è stata sviluppata secondo le regole di implementazione del pattern singleton thread-safe.
    /// </summary>
    public sealed class Settings
    {
        #region Singleton

        private static readonly Settings m_Instance = new Settings();

        /// <summary>
        /// Ottiene l'istanza thread-safe di Settings unica nell'applicazione.
        /// </summary>
        public static Settings Instance { get { return m_Instance; } }

        /// <summary>
        /// Il costruttore privato evita che possano essere create ulteriori istanze.
        /// </summary>
        private Settings()
        {
            LoadServers("Servers.txt");
            LoadNodeId("NodeId.txt");
            LoadNeighborsEndpoints("Neighbors.txt");
        }

        #endregion

        #region Fields

        private readonly TimeSpan m_ResourceMonitorOpenTimeout = new TimeSpan(0, 1, 0);
        private readonly TimeSpan m_ResourceMonitorCloseTimeout = new TimeSpan(0, 1, 0);

        private readonly TimeSpan m_ResourceCacheRefreshPeriod = new TimeSpan(0, 10, 0);
        private readonly uint m_ResourceCacheRefreshFailuresLimit = 4;

        private readonly HashSet<Uri> m_ResourceMonitorEndpoints = new HashSet<Uri>();

        private readonly TimeSpan m_SearchExpiryInterval = new TimeSpan(0, 1, 0);
        private readonly TimeSpan m_SearchEngineCleaningPeriod = new TimeSpan(0, 10, 0);

        private readonly byte m_MessageInitialTtl = 5;
        private readonly TimeSpan m_ForwardingEntryExpiryInterval = new TimeSpan(0, 2, 0);
        private readonly TimeSpan m_ForwardingTableCleaningPeriod = new TimeSpan(0, 10, 0);

        private readonly Dictionary<string, Uri> m_NeighborsInfo = new Dictionary<string, Uri>();
        private readonly int m_OutputBufferSize = 20;
        private string m_NodeId = null;

        private readonly string m_LogsFolder = "Logs";

        // --- descrizione degli eventuali errori
        private readonly List<string> m_InitializationErrors = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// Permette di verificare la presenza di eventuali errori nella configurazione.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return m_InitializationErrors.Count > 0;
            }
        }

        /// <summary>
        /// Restituisce la descrizione completa di ogni eventuale errore di configurazione.
        /// </summary>
        public string[] Errors
        {
            get
            {
                return m_InitializationErrors.ToArray();
            }
        }

        /// <summary>
        /// L'intervallo massimo di attesa per l'apertura della connessione verso un server di calcolo.
        /// </summary>
        public TimeSpan ResourceMonitorOpenTimeout
        {
            get { return m_ResourceMonitorOpenTimeout; }
        }

        /// <summary>
        /// L'intervallo massimo di attesa per la chiusura della connessione verso un server di calcolo.
        /// </summary>
        public TimeSpan ResourceMonitorCloseTimeout
        {
            get { return m_ResourceMonitorCloseTimeout; }
        }

        /// <summary>
        /// La periodicità di aggiornamento della cache contenente le informazioni sulle risorse di calcolo.
        /// </summary>
        /// <remarks>
        /// La procedura di refresh prevede che vengano interrogati nuovamente i server di elaborazione per verificare
        /// se le risorse di calcolo da essi fornite siano cambiate e per controllare se sono ancora in linea. Inoltre,
        /// durante alcune fasi di refresh, vengono rimossi anche gli elementi che eventualmente sono scaduti, tenendo
        /// conto della scadenza specificata.
        /// </remarks>
        public TimeSpan ResourceCacheRefreshPeriod
        {
            get { return m_ResourceCacheRefreshPeriod; }
        }

        /// <summary>
        /// Il numero massimo di tentativi di refresh di un elemento della cache, prima che lo stesso venga considerato
        /// non più valido e quindi essere rimosso dalla stessa.
        /// </summary>
        public uint ResourceCacheRefreshFailuresLimit
        {
            get { return m_ResourceCacheRefreshFailuresLimit; }
        }

        /// <summary>
        /// L'insieme degli indirizzi dei server di calcolo da monitorare.
        /// </summary>
        public IEnumerable<Uri> ResourceMonitorEndpoints
        {
            get
            {
                return m_ResourceMonitorEndpoints.ToArray();
            }
        }

        /// <summary>
        /// L'intervallo di tempo che deve trascorrere dall'inizio di una ricerca prima che quest'ultima debba essere
        /// considerata non valida nella cache delle ricerche effettuate e quindi candidata per essere ignorata dalle
        /// ricerchie e poi rimossa.
        /// </summary>
        public TimeSpan SearchExpiryInterval
        {
            get { return m_SearchExpiryInterval; }
        }

        /// <summary>
        /// L'intervallo di tempo che scandisce la periodica verifica della cache di ricerca per l'eventuale
        /// rimozione degli elementi considerati non più validi in base all'intervallo di validità impostato.
        /// </summary>
        public TimeSpan SearchEngineCleaningPeriod
        {
            get { return m_SearchEngineCleaningPeriod; }
        }

        /// <summary>
        /// Il valore di time-to-live di un messaggio inviato ad un vicino della rete.
        /// </summary>
        public byte MessageInitialTtl
        {
            get { return m_MessageInitialTtl; }
        }

        /// <summary>
        /// L'intervallo di tempo che deve trascorrere dal momento in cui un elemento viene aggiunto nella tabella
        /// di inoltro, per poter essere considerato obsoleto e quindi candidato per essere ignorato e poi rimosso.
        /// </summary>
        public TimeSpan ForwardingEntryExpiryInterval
        {
            get { return m_ForwardingEntryExpiryInterval; }
        }

        /// <summary>
        /// L'intervallo di tempo che scandisce la periodica verifica della tabella di inoltro per l'eventuale
        /// rimozione degli elementi considerati non più validi in base all'intervallo di validità impostato.
        /// </summary>
        public TimeSpan ForwardingTableCleaningPeriod
        {
            get { return m_ForwardingTableCleaningPeriod; }
        }

        /// <summary>
        /// Ottiene l'identificatore di ogni vicino e l'indirizzo in cui è attivo il servizio di ricezione dei messaggi.
        /// </summary>
        public IEnumerable<KeyValuePair<string, Uri>> NeighborsInfo
        {
            get
            {
                return m_NeighborsInfo.ToArray<KeyValuePair<string, Uri>>();
            }
        }

        /// <summary>
        /// Gli identificatori relativi ai vicini da cui accettare i messaggi in ingresso.
        /// </summary>
        public IEnumerable<string> NeighborsIdentifiers
        {
            get
            {
                return m_NeighborsInfo.Keys.ToArray();
            }
        }

        /// <summary>
        /// Gli indirizzi dei servizi attivi sui vicini.
        /// </summary>
        public IEnumerable<Uri> NeighborsEndpoints
        {
            get
            {
                return m_NeighborsInfo.Values.ToArray();
            }
        }

        /// <summary>
        /// La capacità di ogni buffer di uscita.
        /// </summary>
        public int OutputBufferSize
        {
            get
            {
                return m_OutputBufferSize;
            }
        }

        /// <summary>
        /// L'identificatore associato a questa istanza dell'applicazione.
        /// </summary>
        public string NodeId
        {
            get
            {
                return m_NodeId;
            }
        }

        /// <summary>
        /// Il percorso della cartella dedicata alla memorizzazione dei file di log.
        /// </summary>
        public string LogsFolder
        {
            get { return m_LogsFolder; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Prova a caricare l'elenco degli indirizzi dei server di elaborazione da monitorare durante l'esecuzione
        /// di questa istanza dell'applicazione. Se si dovessero verificare errori durante il caricamento dei dati,
        /// ne viene conservata la descrizione completa, così che il chiamante sia in grado di accedervi.
        /// </summary>
        private void LoadServers(string filename)
        {
            try
            {
                string[] lines = File.ReadAllLines(filename);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;   // ignora righe vuote

                    string uriString = lines[i].Trim();
                    Uri uri = new Uri(uriString);
                    if (!InsertResourceMonitorEndpoint(uri))
                    {
                        string message = string.Format("Error loading configuration file {0}: line {1} contains an invalid uri.", filename, i);
                        m_InitializationErrors.Add(message);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Error loading configuration file {0}: {1}", filename, ex.ToString());
                m_InitializationErrors.Add(message);
            }
        }

        /// <summary>
        /// Inserisce l'indirizzo remoto specificato alla lista degli indirizzi dei server di elaborazione che occorre
        /// monitorare per mantenere aggiornato l'elenco delle risorse di calcolo disponibili, e restituisce true se
        /// l'uri è stato aggiunto, oppure false se non è stato possibile aggiungerlo perchè già presente o perché
        /// è stato specificato un valore null.
        /// </summary>
        /// <param name="uri">l'indirizzo remoto del server di elaborazione da monitorare</param>
        /// <returns>true se l'uri specificato è stata aggiunto; in caso contratio false.</returns>
        private bool InsertResourceMonitorEndpoint(Uri uri)
        {
            if (uri == null) return false;
            return m_ResourceMonitorEndpoints.Add(uri);
        }

        /// <summary>
        /// Prova a caricare dal file specificato l'identificatore associato a questa istanza dell'applicazione.
        /// Qualora si dovessero verificare errori nella fase di caricamento dei dati, ne viene conservata la
        /// descrizione completa, così che il chiamante sia in grado di accedervi.
        /// </summary>
        private void LoadNodeId(string filename)
        {
            try
            {
                string text = File.ReadAllText(filename);
                m_NodeId = (text != null ? text.Trim() : string.Empty);

                if (string.IsNullOrEmpty(m_NodeId))
                {
                    string message = string.Format("Error loading configuration file {0}: the node identifier is empty.", filename);
                    m_InitializationErrors.Add(message);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Error loading configuration file {0}: {1}", filename, ex.ToString());
                m_InitializationErrors.Add(message);
            }
        }

        /// <summary>
        /// Prova a caricare dal file specificato l'elenco degli indirizzi dei vicini configurati per questa istanza
        /// dell'applicazione e gli identificatori ad essi associati. Qualora si dovessero verificare errori in fase
        /// di caricamento dei dati, ne viene conservata la descrizione completa, così che il chiamante sia in grado
        /// di accedervi.
        /// </summary>
        private void LoadNeighborsEndpoints(string filename)
        {
            try
            {
                string[] lines = File.ReadAllLines(filename);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;   // ignora righe vuote

                    string[] splitted = lines[i].Split(',');
                    if (splitted == null || splitted.Length != 2)
                    {
                        string message = string.Format("Error loading configuration file {0}: line {1} is invalid.", filename, i);
                        m_InitializationErrors.Add(message);

                        continue;
                    }

                    string id = splitted[0].Trim();
                    Uri uri = new Uri(splitted[1].Trim());

                    if (!InsertNeighborEndpoint(id, uri))
                    {
                        string message = string.Format("Error loading configuration file {0}: line {1} contains invalid data.", filename, i);
                        m_InitializationErrors.Add(message);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Error loading configuration file {0}: {1}", filename, ex.ToString());
                m_InitializationErrors.Add(message);
            }
        }

        /// <summary>
        /// Inserisce l'indirizzo remoto specificato nella lista degli indirizzi dei vicini con cui collegarsi
        /// per effettuare la ricerca delle risorse di calcolo disponibili nella rete, e restituisce true se
        /// l'uri è stato aggiunto, oppure false se non è stato possibile aggiungerlo perchè già presente o
        /// perché è stato specificato un valore null.
        /// </summary>
        /// <param name="id">L'identificatore associato al vicino con cui collegarsi.</param>
        /// <param name="uri">L'indirizzo remoto del vicino con cui collegarsi.</param>
        /// <returns>true se l'uri specificato è stata aggiunto; in caso contrario false.</returns>
        private bool InsertNeighborEndpoint(string id, Uri uri)
        {
            if (string.IsNullOrWhiteSpace(id) || uri == null) return false;

            if (!m_NeighborsInfo.ContainsKey(id) && !m_NeighborsInfo.ContainsValue(uri))
            {
                m_NeighborsInfo.Add(id, uri);
                return true;
            }

            return false;
        }

        #endregion
    }
}
