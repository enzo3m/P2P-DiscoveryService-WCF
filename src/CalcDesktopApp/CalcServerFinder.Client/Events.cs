using System;
using System.Collections.Generic;

using CalcServerFinder.Contracts;

namespace CalcServerFinder.Client
{
    #region ResourceSearchProgress

    /// <summary>
    /// Notifica lo stato attuale di una ricerca in fase di esecuzione su un nodo di ricerca.
    /// </summary>
    public class ResourceSearchProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Ottiene un valore che indica lo stato attuale della ricerca di una risorsa.
        /// </summary>
        public ResourceSearchState State { get; private set; }

        /// <summary>
        /// Ottiene l'eccezione contenente l'eventuale errore verificatosi durante la comunicazione
        /// col servizio attivo sul nodo di ricerca, altrimenti null se non si è verificato nessun errore.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Ottiene una lista contenente i risultati finora trovati dal servizio attivo sul nodo di ricerca,
        /// altrimenti null se l'operazione è stata interrotta oppure se si è verificato un errore durante
        /// la comunicazione col servizio.
        /// </summary>
        public List<Uri> Result { get; private set; }

        /// <summary>
        /// Istanzia un nuovo oggetto della classe ResourceSearchProgressEventArgs con i parametri specificati.
        /// </summary>
        /// <param name="state">lo stato attuale riguardante la ricerca della risorsa</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="result">gli eventuali risultati finora trovati dal servizio attivo sul nodo di ricerca</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il parametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati di ricerca non sono disponibili a causa di un errore
        /// o perché l'operazione è stata annullata o non sono stati trovati risultati, il parametro corrispondente
        /// dovrà essere impostato a null o con una lista vuota.
        /// </remarks>
        public ResourceSearchProgressEventArgs(ResourceSearchState state, Exception error, List<Uri> result)
        {
            State = state;
            Error = error;
            Result = result;
        }
    }

    /// <summary>
    /// Delegato che agisce come firma per il metodo che viene invocato quando si verifica l'evento.
    /// </summary>
    /// <param name="sender">un riferimento al mittente dell'evento</param>
    /// <param name="args">un oggetto contenente le informazioni sull'evento</param>
    public delegate void ResourceSearchProgressHandler(object sender, ResourceSearchProgressEventArgs args);

    #endregion

    #region ResourceSearchCompleted

    /// <summary>
    /// Notifica il completamento di una ricerca richiesta ad un servizio di ricerca.
    /// </summary>
    public class ResourceSearchCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Ottiene un valore che indica se la ricerca è stata annullata.
        /// </summary>
        public bool Cancelled { get; private set; }

        /// <summary>
        /// Ottiene l'eccezione contenente l'eventuale errore verificatosi durante la comunicazione
        /// col servizio di ricerca, altrimenti null se non si è verificato nessun errore.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Ottiene una lista contenente tutti i risultati trovati dal servizio di ricerca, altrimenti null
        /// se l'operazione è stata interrotta oppure se si è verificato un errore durante la comunicazione
        /// col servizio.
        /// </summary>
        public List<Uri> Result { get; private set; }

        /// <summary>
        /// Istanzia un nuovo oggetto della classe ResourceSearchCompletedEventArgs con i parametri specificati.
        /// </summary>
        /// <param name="cancelled">un valore che indica se la ricerca è stata annullata</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="result">gli eventuali risultati trovati dal servizio di ricerca</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il parametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati di ricerca non sono disponibili a causa di un errore
        /// o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere impostato a null o con
        /// una lista vuota.
        /// </remarks>
        public ResourceSearchCompletedEventArgs(bool cancelled, Exception error, List<Uri> result)
        {
            Cancelled = cancelled;
            Error = error;
            Result = result;
        }
    }

    /// <summary>
    /// Delegato che agisce come firma per il metodo che viene invocato quando si verifica l'evento.
    /// </summary>
    /// <param name="sender">un riferimento al mittente dell'evento</param>
    /// <param name="args">un oggetto contenente le informazioni sull'evento</param>
    public delegate void ResourceSearchCompletedHandler(object sender, ResourceSearchCompletedEventArgs args);

    #endregion
}
