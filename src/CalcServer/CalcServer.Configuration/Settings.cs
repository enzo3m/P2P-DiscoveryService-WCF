using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalcServer.Configuration
{
    /// <summary>
    /// Permette di caricare le impostazioni da file di configurazione, semplificandone l'accesso da parte
    /// dei moduli del server.
    /// Qualora durante il caricamento delle impostazioni si verificano degli errori, questi vengono conservati
    /// su file di log e vengono configurati i valori predefiniti delle impostazioni, garantendo quindi sempre
    /// la presenza di valori validi.
    /// La classe in questione è stata sviluppata seguendo il pattern singleton.
    /// </summary>
    public class Settings
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
        private Settings() { ; }

        #endregion

        #region Fields

        private readonly string m_ReadyTasksFolderPath = "Repository" + Path.DirectorySeparatorChar + "ReadyTasks";
        private readonly string m_CompletedTasksFolderPath = "Repository" + Path.DirectorySeparatorChar + "CompletedTasks";
        private readonly string m_LogsFolder = "Logs";

        private readonly object m_ResourcesLocker = new object();
        private readonly HashSet<string> m_Resources = new HashSet<string>();

        #endregion

        #region Properties

        /// <summary>
        /// Il percorso della cartella dedicata alla memorizzazione dei file ricevuti.
        /// </summary>
        public string ReadyTasksFolder
        {
            get { return m_ReadyTasksFolderPath; }
        }

        /// <summary>
        /// Il percorso della cartella dedicata alla memorizzazione dei file da inviare.
        /// </summary>
        public string CompletedTasksFolder
        {
            get { return m_CompletedTasksFolderPath; }
        }

        /// <summary>
        /// Il percorso della cartella dedicata alla memorizzazione dei file di log.
        /// </summary>
        public string LogsFolder
        {
            get { return m_LogsFolder; }
        }

        /// <summary>
        /// L'elenco delle risorse di elaborazione disponibili e abilitate.
        /// </summary>
        public IEnumerable<string> Resources
        {
            get
            {
                lock (m_ResourcesLocker)
                {
                    return m_Resources.ToList().ToArray();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Controlla la risorsa di elaborazione specificata, restituendo true se risulta abilitata, oppure false se
        /// invece non esiste e quindi vuol dire che è disabilitata. La risorsa deve essere specificata come stringa
        /// contenente nome completo e versione della classe che implementa tale risorsa: queste due informazioni
        /// devono essere separate opportunamente con un singolo carattere trattino-meno.
        /// </summary>
        /// <param name="resource">la stringa contenente nome e versione della risorsa di calcolo da verificare</param>
        /// <returns>true se la risorsa specificata esiste e quindi è abilitata, altrimenti false</returns>
        public bool IsResourceEnabled(string resource)
        {
            lock (m_ResourcesLocker)
            {
                return m_Resources.Contains(resource);
            }
        }

        /// <summary>
        /// Inserisce la risorsa specificata nella lista delle risorse disponibili elencate nella configurazione
        /// e restituisce true se la risorsa è stata aggiunta, oppure false se non è stato possibile aggiungerla
        /// perchè già presente. La risorsa deve essere specificata in una stringa contenente il nome completo e
        /// la versione della classe che implementa tale risorsa: queste due informazioni devono essere separate
        /// opportunamente con un singolo carattere trattino-meno.
        /// </summary>
        /// <param name="resource">la stringa contenente nome e versione della risorsa di calcolo da aggiungere</param>
        /// <returns>true se la risorsa specificata è stata aggiunta, altrimenti false</returns>
        /// <remarks>
        /// Ogni risorsa aggiunta a questa configurazione verrà abilitata e quindi risulterà disponibile durante
        /// il funzionamento del servizio.
        /// </remarks>
        public bool InsertResource(string resource)
        {
            lock (m_ResourcesLocker)
            {
                return m_Resources.Add(resource);
            }
        }

        /// <summary>
        /// Rimuove la risorsa specificata dalla lista delle risorse disponibili elencate nella configurazione
        /// e restituisce true se la risorsa è stata rimossa, oppure false se non è stato possibile rimuoverla
        /// perchè non presente nella lista. La risorsa va specificata come stringa contenente nome completo e
        /// versione della classe che implementa tale risorsa: queste due informazioni devono essere separate
        /// opportunamente con un singolo carattere trattino-meno.
        /// </summary>
        /// <param name="resource">la stringa contenente nome e versione della risorsa di calcolo da rimuovere</param>
        /// <returns>true se la risorsa specificata è stata rimossa, altrimenti false</returns>
        /// <remarks>
        /// Ogni risorsa rimossa da questa configurazione verrà disabilitata e pertanto non sarà disponibile
        /// durante il funzionamento del servizio.
        /// </remarks>
        public bool RemoveResource(string resource)
        {
            lock (m_ResourcesLocker)
            {
                return m_Resources.Remove(resource);
            }
        }

        #endregion
    }
}
