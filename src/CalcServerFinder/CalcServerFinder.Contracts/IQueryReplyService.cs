using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CalcServerFinder.Contracts
{
    /// <summary>
    /// Interfaccia per un servizio in grado di ricevere richieste e risposte.
    /// </summary>
    [ServiceContract]
    public interface IQueryReplyService
    {
        /// <summary>
        /// Permette di ricevere una richiesta da un vicino.
        /// </summary>
        /// <param name="sourceNodeId">L'identificatore associato al vicino.</param>
        /// <param name="query">La richiesta inviata dal vicino.</param>
        [OperationContract]
        void Query(string sourceNodeId, QueryData query);

        /// <summary>
        /// Permette di ricevere una risposta da un vicino.
        /// </summary>
        /// <param name="sourceNodeId">L'identificatore associato al vicino.</param>
        /// <param name="reply">La risposta inviata dal vicino.</param>
        [OperationContract]
        void Reply(string sourceNodeId, ReplyData reply);
    }
}
