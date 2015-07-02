using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CalcServerFinder.Core.Extensions;

namespace CalcServerFinder.Core
{
    /// <summary>
    /// Questa classe rappresenta la tabella di inoltro dei messaggi che questa istanza dell'applicazione ha inviato
    /// ai propri nodi vicini della rete: si tratta delle richieste giunte dai vicini e selezionate per l'inoltro ad
    /// altri vicini e delle richieste create dal nodo stesso e pronte per essere inviate ai vicini.
    /// Grazie alla tabella di inoltro, quando il nodo riceve una risposta, è in grado di conoscere il vicino da cui
    /// è giunta la relativa richiesta, in modo da potergli inviare la risposta ricevuta, che potrà così arrivare al
    /// nodo che ha originariamente creato e inviato la richiesta.
    /// </summary>
    internal sealed class ForwardingTable
    {
        #region Entry

        /// <summary>
        /// Rappresenta un'entry value della tabella di inoltro e contiene una stringa che identifica univocamente
        /// il vicino di provenienza del messaggio, l'istante in cui tale messaggio è stato inserito in tabella ed
        /// un eventuale riferimento ai dati di ricerca.
        /// </summary>
        internal class Entry
        {
            /// <summary>
            /// Ottiene o imposta una stringa che identifica univocamente la connessione relativa al vicino
            /// di provenienza del messaggio: deve essere impostato a null se il messaggio è stato creato e
            /// inviato dal nodo stesso.
            /// </summary>
            public string SourceConnection { get; set; }

            /// <summary>
            /// Ottiene o imposta un riferimento che identifica univocamente la ricerca associata a questa entry:
            /// deve essere impostato a null se il messaggio è stato ricevuto da un vicino.
            /// </summary>
            public SearchData SearchReference { get; set; }

            /// <summary>
            /// Ottiene o imposta l'istante in cui un'istanza di questa classe è stata inserita in tabella.
            /// </summary>
            public DateTime InsertionTime { get; set; }
        }

        #endregion

        #region Fields

        private readonly TimeSpan m_ExpiryInterval;
        private readonly Dictionary<Guid, Entry> m_InternalTable;

        #endregion

        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza vuota della classe ForwardingTable impostando un intervallo massimo
        /// di permanenza per ogni elemento pari al valore specificato.
        /// </summary>
        /// <param name="expiryInterval">L'intervallo di permanenza massima nella tabella di inoltro.</param>
        public ForwardingTable(TimeSpan expiryInterval)
        {
            m_ExpiryInterval = expiryInterval;
            m_InternalTable = new Dictionary<Guid, Entry>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Aggiunge alla tabella le informazioni relative al messaggio ricevuto da un vicino (e possibile candidato
        /// per essere inoltrato, se non duplicato) oppure generato dallo stesso nodo e pronto per l'invio ai vicini,
        /// restituendo true se sono state aggiunte in modo corretto, altrimenti false se l'id del messaggio era già
        /// presente in tabella (duplicato).
        /// </summary>
        /// <param name="msgId">L'id del messaggio pronto per l'eventuale invio ai vicini.</param>
        /// <param name="connectionId">L'eventuale identificativo della connessione di provenienza del messaggio.</param>
        /// <param name="searchRef">L'eventuale riferimento ai dati di ricerca.</param>
        /// <returns>true se l'id del messaggio non è un duplicato; in caso contrario false.</returns>
        /// <remarks>
        /// Il riferimento ai dati di ricerca viene utilizzato soltanto dal nodo in cui inizia la ricerca, poiché
        /// consente di raccogliere i risultati via via in arrivo dai vicini, pertanto è necessario impostarlo se
        /// la ricerca inizia nel nodo corrente, altrimenti deve essere impostato a null.
        /// </remarks>
        public bool Add(Guid msgId, string connectionId, SearchData searchRef)
        {
            if (!ContainsEntry(msgId))
            {
                m_InternalTable.Add(msgId, new Entry
                {
                    SourceConnection = connectionId,
                    SearchReference = searchRef,
                    InsertionTime = DateTime.Now
                });

                return true;
            }

            return false; // duplicato
        }

        /// <summary>
        /// Verifica se l'id del messaggio specificato è associato ad una entry non scaduta in tabella e restituisce
        /// true in caso affermativo, altrimenti false.
        /// </summary>
        /// <param name="msgId">L'id del messaggio da verificare.</param>
        /// <returns>true se l'id del messaggio è ancora presente in tabella; in caso contrario, false.</returns>
        /// <remarks>
        /// Se l'id del messaggio è associato ad una entry scaduta, questa viene rimossa dalla tabella insieme all'id,
        /// pertanto questo metodo restituirà false per indicare che all'id specificato non è associata nessuna entry
        /// non scaduta.
        /// </remarks>
        public bool ContainsEntry(Guid msgId)
        {
            Entry temp;
            return TryGetEntry(msgId, out temp);
        }

        /// <summary>
        /// Restituisce l'elemento della tabella associato all'id del messaggio specificato, oppure null se non viene
        /// trovato, perché per esempio è scaduto o è stato precedentemente rimosso perché il vicino si è disconnesso.
        /// </summary>
        /// <param name="msgId">L'id del messaggio.</param>
        /// <returns>L'elemento della tabella associato all'identificatore specificato (null se non viene trovato).</returns>
        public Entry GetEntry(Guid msgId)
        {
            Entry entry;
            if (TryGetEntry(msgId, out entry))
            {
                return entry;
            }
            return null;
        }

        /// <summary>
        /// Verifica se l'id del messaggio specificato è presente nella tabella di inoltro ed eventualmente imposta
        /// in uscita l'identificativo della connessione di provenienza (oppure null se il messaggio è stato creato
        /// da questa istanza dell'applicazione), restituendo true se l'id del messaggio è tuttora associato ad una
        /// connessione con un vicino o al nodo stesso, altrimenti false se l'id del messaggio non è presente.
        /// </summary>
        /// <param name="msgId">L'id del messaggio da verificare.</param>
        /// <param name="connectionId">L'eventuale identificativo impostato con la connessione di provenienza.</param>
        /// <returns>true se l'id del messaggio è ancora presente in tabella; in caso contrario, false.</returns>
        /// <remarks>
        /// Se l'id del messaggio è associato ad una entry scaduta, questa viene rimossa dalla tabella insieme all'id,
        /// pertanto questo metodo restituirà false per indicare che all'id specificato non è associata nessuna entry
        /// non scaduta.
        /// </remarks>
        public bool TryGetSourceConnection(Guid msgId, out string connectionId)
        {
            Entry entry;
            if (!TryGetEntry(msgId, out entry))
            {
                connectionId = null;
                return false;
            }

            connectionId = entry.SourceConnection;
            return true;
        }

        /// <summary>
        /// Rimuove tutti gli elementi della tabella di inoltro che sono stati memorizzati in essa per un intervallo
        /// di tempo superiore a quello specificato nel costruttore e restituisce il numero di elementi effettivamente
        /// rimossi.
        /// </summary>
        /// <returns>Il numero di elementi scaduti ed effettivamente rimossi.</returns>
        public int Clean()
        {
            DateTime detectionTimeLimit;
            if (!DateTime.Now.TrySubtract(m_ExpiryInterval, out detectionTimeLimit)) return 0;

            List<Guid> itemsToRemove = new List<Guid>();

            foreach (var item in m_InternalTable)
            {
                Entry entry = item.Value;

                if (detectionTimeLimit > entry.InsertionTime)
                {
                    itemsToRemove.Add(item.Key);
                }
            }

            foreach (var msgId in itemsToRemove)
            {
                m_InternalTable.Remove(msgId);
            }

            return itemsToRemove.Count;
        }

        /// <summary>
        /// Rimuove tutti gli elementi della tabella di inoltro che fanno riferimento al vicino specificato
        /// e restituisce il numero di entry effettivamente rimosse al termine della procedura di rimozione.
        /// </summary>
        /// <param name="connectionId">L'identificativo del vicino presente negli elementi da rimuovere.</param>
        /// <returns>Il numero di elementi rimossi dalla tabella di inoltro.</returns>
        public int Clean(string connectionId)
        {
            List<Guid> itemsToRemove = new List<Guid>();

            foreach (var item in m_InternalTable)
            {
                Entry entry = item.Value;

                if (connectionId != null && connectionId == entry.SourceConnection)
                {
                    itemsToRemove.Add(item.Key);
                }
            }

            foreach (var msgId in itemsToRemove)
            {
                m_InternalTable.Remove(msgId);
            }

            return itemsToRemove.Count;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Restituisce il numero di elementi presenti in questa tabella, conteggiando anche quelli scaduti
        /// che non sono ancora stati rimossi.
        /// </summary>
        public int Count
        {
            get { return m_InternalTable.Count; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Questo metodo privato accede alla tabella interna col fine di verificare che l'id del messaggio specificato
        /// non sia associato ad una entry scaduta ed eventualmente imposta in uscita in valore prelevato dalla tabella
        /// e restituisce true: in sostanza è un metodo di utilità invocato dai metodi di questa classe, che richiedono
        /// l'accesso ad entry non scadute.
        /// Qualora la entry richiesta dovesse risultare scaduta, la elimina e restituisce false; analogamente, se l'id
        /// specificato non esiste all'interno della tabella, questo metodo restituisce false.
        /// </summary>
        /// <param name="msgId">L'id del messaggio associato alla entry da verificare ed eventualmente recuperare.</param>
        /// <param name="entry">L'eventuale entry non scaduta, impostata quando termina questo metodo.</param>
        /// <returns>true se l'id specificato è associato ad una entry non scaduta; altrimenti false.</returns>
        private bool TryGetEntry(Guid msgId, out Entry entry)
        {
            Entry temp;
            if (m_InternalTable.TryGetValue(msgId, out temp)) // msgId non può essere null perché è una struct
            {
                if (temp == null || temp.InsertionTime + m_ExpiryInterval < DateTime.Now)
                {
                    // elemento non valido o scaduto
                    m_InternalTable.Remove(msgId);
                }
                else
                {
                    entry = temp;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        #endregion
    }
}
