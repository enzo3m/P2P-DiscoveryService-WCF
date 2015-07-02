using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CalcServerFinder.Configuration;
using CalcServerFinder.Contracts;
using CalcServerFinder.Logging;
using CalcServerFinder.Networking;

namespace CalcServerFinder.Core
{
    /// <summary>
    /// Questa classe gestisce la comunicazione tra il nodo rappresentato dall'istanza corrente dell'applicazione
    /// e i nodi ad esso vicini, analizzando le richieste e le risposte in arrivo, per decidere se debbano essere
    /// inoltrate o meno e a quali vicini.
    /// Questa classe è stata sviluppata sfruttando il pattern singleton nella sua versione thread-safe.
    /// </summary>
    public sealed class CommunicationHandler
    {
        #region Fields

        // --- istanze degli oggetti singleton
        private readonly Settings m_AppConfig = Settings.Instance;
        private readonly Logger m_AppLogger = Logger.Instance;
        private readonly ResourceCache m_ResourceCache = ResourceCache.Instance;
        private readonly SearchManager m_SearchManager = SearchManager.Instance;
        private readonly NeighborhoodManager m_NeighborhoodManager = NeighborhoodManager.Instance;

        // --- tabella dei messaggi inoltrati e inviati
        private readonly object m_ForwardingTableLocker = new object();
        private readonly ForwardingTable m_ForwardingTable = null;

        // --- valore iniziale del TTL per i messaggi inviati
        private readonly byte m_MessageInitialTtl;

        // --- timer per rimuovere periodicamente gli elementi scaduti dalla tabella di inoltro
        private readonly TimeSpan m_ForwardingTableCleaningPeriod;
        private readonly Timer m_CleaningTimer = null;
        private readonly object m_CleaningCallbackLocker = new object();

        #endregion

        #region Singleton

        private static readonly CommunicationHandler m_Instance = new CommunicationHandler();

        /// <summary>
        /// Costruttore privato non accessibile.
        /// </summary>
        private CommunicationHandler()
        {
            m_MessageInitialTtl = m_AppConfig.MessageInitialTtl;

            m_ForwardingTable = new ForwardingTable(m_AppConfig.ForwardingEntryExpiryInterval);

            m_ForwardingTableCleaningPeriod = m_AppConfig.ForwardingTableCleaningPeriod;
            m_CleaningTimer = new Timer(CleaningTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Ottiene il riferimento all'unica istanza di questo CommunicationManager.
        /// </summary>
        public static CommunicationHandler Instance { get { return m_Instance; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Avvia la verifica periodica degli elementi obsoleti presenti nella tabella di inoltro per poterli rimuovere.
        /// </summary>
        public void Run()
        {
            m_CleaningTimer.Change(m_ForwardingTableCleaningPeriod, m_ForwardingTableCleaningPeriod);
        }

        /// <summary>
        /// Ferma la verifica periodica degli elementi obsoleti presenti nella tabella di inoltro.
        /// </summary>
        /// <returns>Viene restituito true se la funzione viene eseguita correttamente; in caso contrario false.</returns>
        public bool Dispose()
        {
            return Dispose(TimeSpan.Zero);
        }

        /// <summary>
        /// Ferma la verifica periodica degli elementi obsoleti presenti nella tabella di inoltro.
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
        /// Avvia una nuova ricerca in base ai dati di ricerca specificati, creando un nuovo messaggio di richiesta
        /// con un valore di TimeToLive impostato secondo la configurazione e inviandolo a tutti gli attuali vicini
        /// di questo nodo della rete. Nell'eventualità remota che l'identificativo generato per il nuovo messaggio
        /// corrisponda ad uno degli identificativi attualmente presenti nella tabella di inoltro, il messaggio non
        /// viene inviato e questo metodo restituisce false; in caso contrario, restituisce true.
        /// </summary>
        /// <param name="data">I dati che rappresentano la ricerca da effettuare.</param>
        /// <returns>true se richiesta relativa alla ricerca viene inviata ai vicini; in caso contrario, false.</returns>
        public bool CreateNewSearch(SearchData data)
        {
            Guid msgId = Guid.NewGuid();
            
            lock (m_ForwardingTableLocker)
            {
                if (!m_ForwardingTable.Add(msgId, null, data))
                {
                    return false;   // GUID duplicato
                }
            }

            WriteToLog("Sending query {0}: search data = {1}...", msgId, data);

            QueryData query = new QueryData(msgId, m_MessageInitialTtl, 0) { Options = data.GetSearchOptions() };
            SendQuery(query, null);

            return true;
        }

        /// <summary>
        /// Gestisce una richiesta ricevuta da un vicino, verificando se deve essere inoltrata ai restanti vicini ed
        /// elaborandola per fornire un'eventuale risposta destinata alla connessione di provenienza della richiesta.
        /// La procedura prevede l'aggiornamento preliminare di due proprietà relative alla richiesta: il TimeToLive
        /// viene decrementato di 1, mentre HopsCount viene incrementato di 1. Se il TimeToLive è diventato zero, la
        /// richiesta non viene inoltrata ai restanti vicini, altrimenti verifica che l'identificativo del messaggio
        /// non sia già presente nella tabella di inoltro, prima di inserirlo in essa ed inoltrare la richiesta agli
        /// altri vicini, per poi iniziare l'elaborazione in background della richiesta. Invece, se l'identificativo
        /// è già presente nella tabella di inoltro, la richiesta non viene né inoltrata né elaborata.
        /// </summary>
        /// <param name="sourceConnectionId">L'identificativo della connessione di provenienza della richiesta.</param>
        /// <param name="query">I dati della richiesta ricevuta da un vicino.</param>
        /// <remarks>
        /// Se si specifica null per almeno uno dei due parametri, questo metodo non esegue alcuna azione, ignorando
        /// i parametri specificati.
        /// </remarks>
        public void HandleReceivedQuery(string sourceConnectionId, QueryData query)
        {
            if (query.TimeToLive > 0)
            {
                Task.Factory.StartNew(() => ForwardReceivedQuery(sourceConnectionId, query),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                // Inizia l'elaborazione in background della richiesta.
                Task.Factory.StartNew(() => ReplyToQuery(query, sourceConnectionId),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Gestisce una risposta ricevuta da un vicino, verificando se occorre inviarla ad uno dei vicini successivi
        /// oppure se è giunta a destinazione, ovvero presso il nodo da cui è partita la richiesta a cui tale risposta
        /// si riferisce.
        /// La procedura prevede l'aggiornamento preliminare di due proprietà relative alla risposta: il TimeToLive
        /// viene decrementato di 1, mentre HopsCount viene incrementato di 1. Se il TimeToLive si mantiene maggiore
        /// di zero, la risposta viene inviata al vicino successivo, altrimenti vuol dire che la risposta è arrivata
        /// a destinazione: in quest'ultimo caso, recupera il riferimento alla ricerca inizialmente inviata e lo usa
        /// per aggiornare i risultati della ricerca con quelli contenuti nella risposta.
        /// </summary>
        /// <param name="reply">I dati della risposta ricevuta da un vicino.</param>
        /// <remarks>
        /// Se si specifica null per l'unico parametro previsto, questo metodo non esegue alcuna azione, ignorando
        /// il parametro specificato.
        /// </remarks>
        public void HandleReceivedReply(ReplyData reply)
        {
            if (reply.TimeToLive > 0)
            {
                Task.Factory.StartNew(() => ForwardReceivedReply(reply),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Inoltra la richiesta ricevuta da un vicino, verificando se deve essere inoltrata ai restanti vicini
        /// di questo nodo, oppure se deve essere scartata perché scaduta. La procedura prevede l'aggiornamento
        /// preliminare di due proprietà relative alla richiesta: il TimeToLive viene decrementato di 1, mentre
        /// HopsCount viene incrementato di 1. Se il TimeToLive si è azzerato, la richiesta non viene inoltrata
        /// ai restanti vicini, altrimenti verifica che l'identificativo del messaggio non sia già nella tabella
        /// di inoltro, prima di inserirlo in essa ed inoltrare la richiesta agli altri vicini. Invece, qualora
        /// l'identificativo sia già presente nella tabella di inoltro, la richiesta non viene inoltrata.
        /// </summary>
        /// <param name="sourceConnectionId">L'identificativo della connessione di provenienza della richiesta.</param>
        /// <param name="query">I dati della richiesta ricevuta da un vicino.</param>
        /// <remarks>
        /// Se si specifica null per almeno uno dei due parametri, questo metodo non esegue alcuna azione, ignorando
        /// i parametri specificati.
        /// </remarks>
        private void ForwardReceivedQuery(string sourceConnectionId, QueryData query)
        {
            Thread.Sleep(100);   // simula ritardo di rete

            if (string.IsNullOrWhiteSpace(sourceConnectionId) || query == null || query.TimeToLive == 0) return;

            query.TimeToLive--;
            query.HopsCount++;

            if (query.TimeToLive > 0)   // verifica se la query deve essere inoltrata
            {
                WriteToLog("Forwarding query {0}...", query.MsgId);

                lock (m_ForwardingTableLocker)
                {
                    if (!m_ForwardingTable.Add(query.MsgId, sourceConnectionId, null))
                    {
                        return;   // ignora messaggio duplicato
                    }
                }

                SendQuery(query, sourceConnectionId);   // inoltra messaggio ai restanti vicini
            }
            else
            {
                lock (m_ForwardingTableLocker)
                {
                    if (m_ForwardingTable.ContainsEntry(query.MsgId))
                    {
                        return;   // ignora messaggio duplicato
                    }
                }

                WriteToLog("Query {0} from connection {1} has expired.", query.MsgId, sourceConnectionId);
            }
        }

        /// <summary>
        /// Inoltra una risposta ricevuta da un vicino, verificando se occorre inviarla ad uno dei vicini successivi
        /// oppure se è giunta a destinazione, ovvero presso il nodo da cui è partita la richiesta a cui tale risposta
        /// si riferisce.
        /// La procedura prevede l'aggiornamento preliminare di due proprietà relative alla risposta: il TimeToLive
        /// viene decrementato di 1, mentre HopsCount viene incrementato di 1. Se il TimeToLive si mantiene maggiore
        /// di zero, la risposta viene inviata al vicino successivo, altrimenti vuol dire che la risposta è arrivata
        /// a destinazione: in quest'ultimo caso, recupera il riferimento alla ricerca inizialmente inviata e lo usa
        /// per aggiornare i risultati della ricerca con quelli contenuti nella risposta.
        /// </summary>
        /// <param name="reply">I dati della risposta ricevuta da un vicino.</param>
        /// <remarks>
        /// Se si specifica null per l'unico parametro previsto, questo metodo non esegue alcuna azione, ignorando
        /// il parametro specificato.
        /// </remarks>
        private void ForwardReceivedReply(ReplyData reply)
        {
            Thread.Sleep(100);   // simula ritardo di rete

            if (reply == null || reply.TimeToLive == 0) return;

            reply.TimeToLive--;
            reply.HopsCount++;

            if (reply.TimeToLive > 0)   // verifica che la risposta non sia giunta a destinazione
            {
                string targetConnectionId;

                lock (m_ForwardingTableLocker)
                {
                    if (!m_ForwardingTable.TryGetSourceConnection(reply.MsgId, out targetConnectionId) || targetConnectionId == null)
                    {
                        return;   // ignora messaggio con identificativo non trovato
                    }
                }

                WriteToLog("Sending received reply {0} back to connection {1}. Found services: [{2}].",
                    reply.MsgId, targetConnectionId, string.Join(", ", reply.FoundServices));

                SendReply(reply, targetConnectionId);   // inoltra messaggio al vicino di provenienza della richiesta
            }
            else   // TTL si è azzerato
            {
                WriteToLog("Reply {0} arrived to destination. Found services: [{1}].",
                    reply.MsgId, string.Join(", ", reply.FoundServices));

                SearchData searchReference = null;   // riferimento alla ricerca inizialmente inviata

                lock (m_ForwardingTableLocker)
                {
                    ForwardingTable.Entry entry = m_ForwardingTable.GetEntry(reply.MsgId);
                    if (entry != null && entry.SourceConnection == null)
                    {
                        searchReference = entry.SearchReference;
                    }
                }

                if (searchReference != null)
                {
                    bool updated = m_SearchManager.UpdateResult(searchReference, reply.FoundServices);
                    if (updated)
                    {
                        // aggiornamento completato correttamente
                        WriteToLog("Updated search results from the reply {0}.", reply.MsgId);
                    }
                    else
                    {
                        // ricerca scaduta
                        WriteToLog("Expired target search for the reply {0}.", reply.MsgId);
                    }
                }
            }
        }

        /// <summary>
        /// Invia la richiesta specificata a tutte le connessioni attive relative ai vicini, escludendo l'eventuale
        /// connessione specificata come destinazione della richiesta.
        /// </summary>
        /// <param name="query">I dati della richiesta da inviare.</param>
        /// <param name="excludedConnectionId">L'identificativo della connessione da escludere.</param>
        /// <exception cref="ArgumentNullException">query è null.</exception>
        /// <remarks>
        /// Per inviare la richiesta specificata a tutti i vicini, è sufficiente specificare null come identificativo
        /// della connessione da escludere: in tal modo non verrà esclusa nessuna connessione tra tutte quelle attive.
        /// </remarks>
        private void SendQuery(QueryData query, string excludedConnectionId)
        {
            m_NeighborhoodManager.SendQuery(query, excludedConnectionId);
            
            WriteToLog("Query {0} sent to all connections. Excluded connection: {1}.",
                query.MsgId, (excludedConnectionId != null ? excludedConnectionId : "none"));
        }

        /// <summary>
        /// Verifica se l'identificativo della connessione specificata esiste e se è associato ad una connessione
        /// attiva con un vicino ed in tal caso invia la risposta specificata con i risultati di una ricerca.
        /// </summary>
        /// <param name="reply">I dati della risposta da inviare.</param>
        /// <param name="connectionId">L'identificativo della connessione su cui inviare la risposta.</param>
        private void SendReply(ReplyData reply, string connectionId)
        {
            m_NeighborhoodManager.SendReply(reply, connectionId);

            WriteToLog("Reply {0} sent back to connection {1}. Found services: [{2}].",
                reply.MsgId, connectionId, string.Join(", ", reply.FoundServices));
        }
        
        /// <summary>
        /// Verifica quali, tra le risorse conosciute da questo nodo, sono compatibili con le opzioni di ricerca
        /// specificate nella query ricevuta, quindi invia l'eventuale risposta sulla connessione di provenienza
        /// della query.
        /// </summary>
        /// <param name="query">I dati della richiesta ricevuta.</param>
        /// <param name="sourceConnectionId">L'identificativo della connessione di provenienza della richiesta.</param>
        /// <remarks>
        /// Se si specifica null per almeno uno dei due parametri oppure se l'identificativo della connessione
        /// di provenienza della richiesta è una stringa vuota oppure formata da soli spazi, questo metodo non
        /// esegue alcuna azione, ignorando i parametri specificati.
        /// </remarks>
        private void ReplyToQuery(QueryData query, string sourceConnectionId)
        {
            Thread.Sleep(100);   // simula ritardo di rete

            if (string.IsNullOrWhiteSpace(sourceConnectionId) || query == null) return;

            WriteToLog("Processing query {0} ...", query.MsgId);

            List<Uri> found = m_ResourceCache.Search(
                delegate(Uri uri, IEnumerable<TaskPerformerInfo> resources)
                {
                    foreach (var resource in resources)
                    {
                        if (resource.Name == query.Options.Name && resource.Version == query.Options.Version)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            ).ToList<Uri>();

            WriteToLog("Processed query {0}: {1} resources found.", query.MsgId, found.Count);

            if (found.Count > 0)
            {
                ReplyData reply = new ReplyData(query.MsgId, query.HopsCount, 0) { FoundServices = found };
                SendReply(reply, sourceConnectionId);
            }
        }

        #endregion

        #region Timer Callback

        /// <summary>
        /// Metodo di callback invocato periodicamente per pulire la tabella di inoltro tramite rimozione di tutti
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
                    // Elimina gli elementi troppo vecchi dalla tabella di inoltro.
                    Task.Factory.StartNew(() => CleanForwardingTable(),
                            CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                finally
                {
                    Monitor.Exit(m_CleaningCallbackLocker);
                }
            }
        }

        #endregion

        #region Utility Private Methods

        /// <summary>
        /// Rimuove gli elementi scaduti dalla tabella di inoltro, ovvero quelli il cui istante di inserimento,
        /// rispetto all'istante corrente, ha una distanza temporale maggiore dell'intervallo di tempo stabilito
        /// come scadenza degli elementi di tale tabella.
        /// </summary>
        private void CleanForwardingTable()
        {
            int removedSearchTasksCount;
            int pendingSearchTasksCount;

            lock (m_ForwardingTableLocker)
            {
                removedSearchTasksCount = m_ForwardingTable.Clean();
                pendingSearchTasksCount = m_ForwardingTable.Count;
            }

            WriteToLog("Removal of older forwarding entries: removed {0} items. Remaining entries: {1}.",
                removedSearchTasksCount, pendingSearchTasksCount);
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
}
