using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;

using CalcServerFinder.Configuration;
using CalcServerFinder.Logging;
using CalcServerFinder.Contracts;

namespace CalcServerFinder.Networking
{
    /// <summary>
    /// Questa classe gestisce le connessioni tra il nodo rappresentato dall'istanza corrente dell'applicazione
    /// e i nodi ad esso vicini, pertanto si occupa dello scambio dei messaggi tra questo nodo ed i nodi vicini,
    /// monitorando anche gli istanti di rilevamento degli stessi, in modo da valutare l'eventuale uscita degli
    /// stessi dalla rete.
    /// Questa classe è stata sviluppata sfruttando il pattern singleton nella sua versione thread-safe.
    /// </summary>
    public sealed class NeighborhoodManager : IDisposable
    {
        #region Fields

        // --- istanze degli oggetti singleton
        private readonly Logger m_AppLogger = Logger.Instance;

        // --- tabella dei vicini configurati
        private readonly object m_NeighborsTableLocker = new object();
        private readonly Dictionary<string, NeighborClient> m_NeighborsTable;

        // --- impostazioni client di query/reply
        private readonly string m_NodeId;
        private readonly int m_OutputBufferSize;

        #endregion

        #region Singleton

        private static readonly NeighborhoodManager m_Instance = new NeighborhoodManager();

        /// <summary>
        /// Ottiene l'istanza thread-safe di NeighborhoodManager unica nell'applicazione.
        /// </summary>
        public static NeighborhoodManager Instance { get { return m_Instance; } }

        /// <summary>
        /// Il costruttore privato evita che possano essere create ulteriori istanze.
        /// </summary>
        private NeighborhoodManager()
        {
            Settings settings = Settings.Instance;

            m_NodeId = settings.NodeId;

            Binding binding = new BasicHttpBinding();

            m_OutputBufferSize = settings.OutputBufferSize;

            m_NeighborsTable = new Dictionary<string, NeighborClient>();
            foreach (var neighbor in settings.NeighborsInfo)
                m_NeighborsTable.Add(neighbor.Key, new NeighborClient(binding, neighbor.Value, m_NodeId, neighbor.Key, m_OutputBufferSize));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            List<NeighborClient> queuesToRun = new List<NeighborClient>();

            lock (m_NeighborsTableLocker)
            {
                foreach (var client in m_NeighborsTable.Values)
                {
                    queuesToRun.Add(client);
                }
            }

            foreach (var client in queuesToRun)
            {
                client.Run();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(Timeout.Infinite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        public void Dispose(TimeSpan timeout)
        {
            int millisecondsTimeout = (int)(Math.Ceiling(timeout.TotalMilliseconds));
            Dispose(millisecondsTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        public void Dispose(int millisecondsTimeout)
        {
            List<NeighborClient> queuesToDispose = new List<NeighborClient>();

            lock (m_NeighborsTableLocker)
            {
                foreach (var client in m_NeighborsTable.Values)
                {
                    queuesToDispose.Add(client);
                }
            }

            foreach (var client in queuesToDispose)
            {
                client.Dispose(millisecondsTimeout);
            }
        }

        /// <summary>
        /// Invia la richiesta specificata a tutte le connessioni attive relative ai vicini, escludendo l'eventuale
        /// connessione specificata come destinazione della richiesta.
        /// </summary>
        /// <param name="query">I dati della richiesta da inviare.</param>
        /// <param name="excludedConnectionId">L'identificativo della connessione da escludere (id del vicino).</param>
        /// <remarks>
        /// Per inviare la richiesta specificata a tutti i vicini, è sufficiente specificare null come identificativo
        /// della connessione da escludere: in tal modo non verrà esclusa nessuna connessione tra tutte quelle attive.
        /// </remarks>
        public void SendQuery(QueryData query, string excludedConnectionId)
        {
            if (query == null) return;

            List<NeighborClient> destinationNeighbors = new List<NeighborClient>();

            lock (m_NeighborsTableLocker)
            {
                foreach (var item in m_NeighborsTable)   // seleziona i vicini di destinazione
                {
                    string neighborId = item.Key;   // l'identificatore del vicino
                    if (string.IsNullOrWhiteSpace(excludedConnectionId) || neighborId != excludedConnectionId)
                    {
                        NeighborClient clientObj = item.Value;   // l'oggetto da usare per inviare il messaggio
                        if (clientObj != null)
                        {
                            destinationNeighbors.Add(clientObj);
                        }
                    }
                }
            }

            foreach (var clientObj in destinationNeighbors)
            {
                if (clientObj.Enqueue(query))   // suppone implementazione thread-safe
                {
                    WriteToLog("Query {0} sent to connection {1}.", query.MsgId, clientObj.TargetNodeId);
                }
                else
                {
                    WriteToLog("Unable to send query {0} to connection {1}: output queue is full.", query.MsgId, clientObj.TargetNodeId);
                }
            }
        }

        /// <summary>
        /// Verifica se l'identificativo della connessione specificata esiste e se è associato ad una connessione
        /// attiva con un vicino ed in tal caso invia la risposta specificata con i risultati di una ricerca.
        /// </summary>
        /// <param name="reply">I dati della risposta da inviare.</param>
        /// <param name="connectionId">L'identificativo della connessione su cui inviare la risposta (id del vicino).</param>
        public void SendReply(ReplyData reply, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId) || reply == null) return;

            NeighborClient neighbor = null;

            lock (m_NeighborsTableLocker)
            {
                if (!m_NeighborsTable.TryGetValue(connectionId, out neighbor) || neighbor == null)
                {
                    return;
                }
            }

            if (neighbor.Enqueue(reply))   // suppone implementazione thread-safe
            {
                WriteToLog("Reply {0} sent back to connection {1}. Found services: [{2}].",
                    reply.MsgId, connectionId, string.Join(", ", reply.FoundServices));
            }
            else
            {
                WriteToLog("Unable to send reply {0} back to connection {1}: output queue is full.", reply.MsgId, connectionId);
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
}
