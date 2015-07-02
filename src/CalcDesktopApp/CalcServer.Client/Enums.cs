using System;
using System.Collections.Generic;

using CalcServer.Contracts;

namespace CalcServer.Client
{
    #region TaskExecutionState

    /// <summary>
    /// Rappresenta lo stato della richiesta di elaborazione di un task.
    /// </summary>
    public enum TaskExecutionState
    {
        /// <summary>
        /// Indica che il proxy è in fase di inizializzazione.
        /// </summary>
        InitializingProxy,

        /// <summary>
        /// Indica che il proxy è stato inizializzato.
        /// </summary>
        ProxyInitialized,

        /// <summary>
        /// Indica che la richiesta contenente i dati è in fase di invio al servizio di elaborazione.
        /// </summary>
        SendingRequest,

        /// <summary>
        /// Indica che la richiesta contenente i dati è stata ricevuta correttamente dal servizio di elaborazione.
        /// </summary>
        RequestSent,

        /// <summary>
        /// Indica che la richiesta di elaborazione è stata annullata.
        /// </summary>
        RequestCancelled,

        /// <summary>
        /// Indica che si è verificato un errore di timeout durante la comunicazione col servizio.
        /// </summary>
        TimeoutError,

        /// <summary>
        /// Indica che si il servizio di elaborazione non è stato trovato.
        /// </summary>
        ServiceNotFoundError,

        /// <summary>
        /// Indica che si è verificato un errore di comunicazione col servizio.
        /// </summary>
        CommunicationError,

        /// <summary>
        /// Indica che si è verificato un errore in un'operazione del servizio.
        /// </summary>
        OperationError,

        /// <summary>
        /// Indica che si è verificato un errore sconosciuto durante la comunicazione col servizio.
        /// </summary>
        UnknownError,

        /// <summary>
        /// Indica che i risultati dell'elaborazione sono in fase di download.
        /// </summary>
        DownloadingResults,

        /// <summary>
        /// Indica che i risultati dell'elaborazione sono stati scaricati correttamente.
        /// </summary>
        ResultsDownloaded,

        /// <summary>
        /// Indica il proxy è in fase di chiusura.
        /// </summary>
        DisposingProxy,

        /// <summary>
        /// Indica che il proxy è stato chiuso.
        /// </summary>
        ProxyDisposed,
    }

    #endregion

    #region TaskExecutionStateDescriptions

    /// <summary>
    /// Questa classe contiene un mapping tra lo stato di esecuzione di un task e la corrispondente descrizione.
    /// </summary>
    public class TaskExecutionStateDescriptions
    {
        private readonly static Dictionary<TaskExecutionState, String> m_InternalTable_1 =
            new Dictionary<TaskExecutionState, String>()
            {
                { TaskExecutionState.InitializingProxy, "Inizializzazione proxy in corso..." },
                { TaskExecutionState.ProxyInitialized, "Proxy inizializzato." },
                { TaskExecutionState.SendingRequest, "Invio richiesta in corso..." },
                { TaskExecutionState.RequestSent, "Richiesta inviata." },
                { TaskExecutionState.RequestCancelled, "Richiesta annullata." },
                { TaskExecutionState.TimeoutError, "Timeout." },
                { TaskExecutionState.ServiceNotFoundError, "Servizio non trovato." },
                { TaskExecutionState.CommunicationError, "Errore di comunicazione." },
                { TaskExecutionState.OperationError, "L'operazione del servizio ha riscontrato un errore." },
                { TaskExecutionState.UnknownError, "Errore sconosciuto." },
                { TaskExecutionState.DownloadingResults, "Download dei risultati in corso..." },
                { TaskExecutionState.ResultsDownloaded, "Download dei risultati completato." },
                { TaskExecutionState.DisposingProxy, "Chiusura proxy in corso..." },
                { TaskExecutionState.ProxyDisposed, "Proxy chiuso." }
            };

        private readonly static Dictionary<ServiceFaultCode, String> m_InternalTable_2 =
            new Dictionary<ServiceFaultCode, String>()
            {
                { ServiceFaultCode.ComponentUnavailable, "Risorsa di elaborazione non disponibile sul server." },
                { ServiceFaultCode.InternalError, "Errore interno del server." },
                { ServiceFaultCode.ReceiveTaskDataFailed, "Errore durante l'invio dei dati." },
                { ServiceFaultCode.SendTaskResultsFailed, "Errore durante la ricezione dei risultati." },
                { ServiceFaultCode.TaskDataFormatError, "Formato dei dati non valido." },
                { ServiceFaultCode.TaskGenerateRequestIdFailed, "Impossibile generare un ID per la richiesta." },
                { ServiceFaultCode.TaskResultsNotFound, "I risultati del task non sono stati trovati." },
                { ServiceFaultCode.Unknown, "Errore sconosciuto sul server." }
            };

        /// <summary>
        /// Restituisce la descrizione in formato stringa dello stato specificato.
        /// </summary>
        /// <param name="state">lo stato di cui si richiede la descrizione</param>
        /// <returns>la descrizione in formato stringa dello stato specificato</returns>
        public static string GetStateDescription(TaskExecutionState state)
        {
            string description;
            if (m_InternalTable_1.TryGetValue(state, out description))
                return description;
            return string.Empty;

        }

        /// <summary>
        /// Restituisce la descrizione in formato stringa dello stato specificato.
        /// </summary>
        /// <param name="state">lo stato di cui si richiede la descrizione</param>
        /// <returns>la descrizione in formato stringa dello stato specificato</returns>
        public static string GetStateDescription(ServiceFaultCode state)
        {
            string description;
            if (m_InternalTable_2.TryGetValue(state, out description))
                return description;
            return string.Empty;

        }
    }

    #endregion
}
