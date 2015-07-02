using System;

using CalcServer.Contracts;

namespace CalcServer.Client
{
    #region TaskExecutionCompleted

    /// <summary>
    /// Notifica il completamento di una richiesta inviata ad un servizio di elaborazione.
    /// </summary>
    public class TaskExecutionCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Ottiene un valore che indica se l'elaborazione del task è stata annullata.
        /// </summary>
        public bool Cancelled { get; private set; }

        /// <summary>
        /// Ottiene l'eccezione contenente l'eventuale errore verificatosi durante la comunicazione
        /// col servizio di elaborazione, altrimenti null se non si è verificato nessun errore.
        /// </summary>
        /// <remarks>
        /// Questa eccezione non include gli eventuali errori verificatisi sul server durante l'elaborazione
        /// del task, poiché questi verrebbero inclusi all'interno dei risultati di elaborazione.
        /// </remarks>
        public Exception Error { get; private set; }

        /// <summary>
        /// Ottiene un oggetto contenente i risultati eventualmente scaricati dal servizio di elaborazione,
        /// altrimenti null se l'operazione è stata interrotta oppure se si è verificato un errore durante
        /// la comunicazione col servizio.
        /// </summary>
        public TaskResults Result { get; private set; }

        /// <summary>
        /// Ottiene l'identificativo del task assegnato dal servizio di elaborazione.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Istanzia un nuovo oggetto della classe TaskExecutionCompletedEventArgs con i parametri specificati.
        /// </summary>
        /// <param name="cancelled">un valore che indica se il task è stato annullato</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="result">gli eventuali risultati scaricati dal server di elaborazione</param>
        /// <param name="id">l'identificativo del task assegnato dal servizio di elaborazione</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il patametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati dell'elaborazione di un task non sono disponibili
        /// a causa di un errore o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere
        /// impostato a null.
        /// </remarks>
        public TaskExecutionCompletedEventArgs(bool cancelled, Exception error, TaskResults result, string id)
        {
            Cancelled = cancelled;
            Error = error;
            Result = result;
            Id = id;
        }
    }

    /// <summary>
    /// Delegato che agisce come firma per il metodo che viene invocato quando si verifica l'evento.
    /// </summary>
    /// <param name="sender">un riferimento al mittente dell'evento</param>
    /// <param name="args">un oggetto contenente le informazioni sull'evento</param>
    public delegate void TaskExecutionCompletedHandler(object sender, TaskExecutionCompletedEventArgs args);

    #endregion

    #region TaskExecutionProgress
    
    /// <summary>
    /// Notifica il progredire dello stato di una richiesta inviata ad un servizio di elaborazione.
    /// </summary>
    public class TaskExecutionProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Ottiene un valore che indica lo stato della richiesta di elaborazione del task.
        /// </summary>
        public TaskExecutionState State { get; private set; }

        /// <summary>
        /// Ottiene l'eccezione contenente l'eventuale errore verificatosi durante la comunicazione
        /// col servizio di elaborazione, altrimenti null se non si è verificato nessun errore.
        /// </summary>
        /// <remarks>
        /// Questa eccezione non include gli eventuali errori verificatisi sul server durante l'elaborazione
        /// del task, poiché questi verrebbero inclusi all'interno dei risultati di elaborazione.
        /// </remarks>
        public Exception Error { get; private set; }

        /// <summary>
        /// Ottiene l'identificativo del task assegnato dal servizio di elaborazione.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Istanzia un nuovo oggetto della classe TaskExecutionProgressEventArgs con i parametri specificati.
        /// </summary>
        /// <param name="state">lo stato relativo alla richiesta di elaborazione</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="id">l'identificativo del task assegnato dal servizio di elaborazione</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il parametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati dell'elaborazione di un task non sono disponibili
        /// a causa di un errore o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere
        /// impostato a null.
        /// </remarks>
        public TaskExecutionProgressEventArgs(TaskExecutionState state, Exception error, string id)
        {
            State = state;
            Error = error;
            Id = id;
        }
    }

    /// <summary>
    /// Delegato che agisce come firma per il metodo che viene invocato quando si verifica l'evento.
    /// </summary>
    /// <param name="sender">un riferimento al mittente dell'evento</param>
    /// <param name="args">un oggetto contenente le informazioni sull'evento</param>
    public delegate void TaskExecutionProgressHandler(object sender, TaskExecutionProgressEventArgs args);

    #endregion
}
