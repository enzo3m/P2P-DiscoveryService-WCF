using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using CalcServerFinder.Configuration;
using CalcServerFinder.Logging;
using CalcServerFinder.Contracts;
using CalcServerFinder.Core;

namespace CalcServerFinder.Services
{
    /// <summary>
    /// Questa classe implementa un servizio per la ricezione delle richieste e delle risposte inviate da un vicino.
    /// </summary>
    public class QueryReplyService : IQueryReplyService
    {
        #region Fields

        // --- istanze degli oggetti singleton (condivisi tra tutte le istanze di questo servizio)
        private static readonly Settings m_AppConfig = Settings.Instance;
        private static readonly Logger m_AppLogger = Logger.Instance;
        private static readonly CommunicationHandler m_MessagesHandler = CommunicationHandler.Instance;

        // --- id relativi ai vicini
        private static readonly HashSet<string> m_KnownNeighbors = new HashSet<string>(m_AppConfig.NeighborsIdentifiers);

        #endregion

        #region Public Methods

        /// <summary>
        /// Permette di ricevere una richiesta da un vicino.
        /// </summary>
        /// <param name="sourceNodeId">L'identificatore associato al vicino.</param>
        /// <param name="query">La richiesta inviata dal vicino.</param>
        public void Query(string sourceNodeId, QueryData query)
        {
            if (m_KnownNeighbors.Contains(sourceNodeId))
            {
                WriteToLog("The received query from node {0} has been accepted: {1}.", sourceNodeId, query.MsgId);

                m_MessagesHandler.HandleReceivedQuery(sourceNodeId, query);
            }
            else
            {
                WriteToLog("The received query from node {0} has been dropped: {1}.", sourceNodeId, query.MsgId);
            }
        }

        /// <summary>
        /// Permette di ricevere una risposta da un vicino.
        /// </summary>
        /// <param name="sourceNodeId">L'identificatore associato al vicino.</param>
        /// <param name="reply">La risposta inviata dal vicino.</param>
        public void Reply(string sourceNodeId, ReplyData reply)
        {
            if (m_KnownNeighbors.Contains(sourceNodeId))
            {
                WriteToLog("The received reply from node {0} has been accepted: {1}.", sourceNodeId, reply.MsgId);

                m_MessagesHandler.HandleReceivedReply(reply);
            }
            else
            {
                WriteToLog("The received reply from node {0} has been dropped: {1}.", sourceNodeId, reply.MsgId);
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
