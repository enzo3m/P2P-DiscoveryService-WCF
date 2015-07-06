using System;
using System.Collections.Generic;

using CalcServerFinder.Contracts;

namespace CalcServerFinder.Client
{
    #region ResourceSearchState

    /// <summary>
    /// Rappresenta lo stato della procedura di ricerca delle risorse.
    /// </summary>
    public enum ResourceSearchState
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
        /// Indica che la richiesta contenente le opzioni di ricerca è in fase di invio al servizio di ricerca.
        /// </summary>
        StartingSearch,

        /// <summary>
        /// Indica che la ricerca è stata annullata.
        /// </summary>
        SearchCancelled,

        /// <summary>
        /// Indica che si è verificato un errore di timeout durante la comunicazione col servizio di ricerca.
        /// </summary>
        TimeoutError,

        /// <summary>
        /// Indica che si il servizio di ricerca non è stato trovato.
        /// </summary>
        ServiceNotFoundError,

        /// <summary>
        /// Indica che si è verificato un errore di comunicazione col servizio di ricerca.
        /// </summary>
        CommunicationError,

        /// <summary>
        /// Indica che si è verificato un errore sconosciuto durante la comunicazione col servizio.
        /// </summary>
        UnknownError,

        /// <summary>
        /// Indica che sono disponibili i risultati trovati dal servizio di ricerca.
        /// </summary>
        FoundResults,

        /// <summary>
        /// Indica il proxy è in fase di chiusura.
        /// </summary>
        DisposingProxy,

        /// <summary>
        /// Indica che il proxy è stato chiuso.
        /// </summary>
        ProxyDisposed
    }

    #endregion

    #region ResourceSearchStateDescriptions

    /// <summary>
    /// Questa classe contiene un mapping tra lo stato di ricerca di una risorsa e la corrispondente descrizione.
    /// </summary>
    public class ResourceSearchStateDescriptions
    {
        private readonly static Dictionary<ResourceSearchState, String> m_InternalTable_1 =
            new Dictionary<ResourceSearchState, String>()
            {
                { ResourceSearchState.InitializingProxy, "Inizializzazione proxy in corso..." },
                { ResourceSearchState.ProxyInitialized, "Proxy inizializzato." },
                { ResourceSearchState.StartingSearch, "Ricerca in corso..." },
                { ResourceSearchState.SearchCancelled, "Ricerca annullata." },
                { ResourceSearchState.TimeoutError, "Timeout." },
                { ResourceSearchState.ServiceNotFoundError, "Servizio non trovato." },
                { ResourceSearchState.CommunicationError, "Errore di comunicazione." },
                { ResourceSearchState.UnknownError, "Errore sconosciuto." },
                { ResourceSearchState.FoundResults, "Ricezione dati..." },
                { ResourceSearchState.DisposingProxy, "Chiusura proxy in corso..." },
                { ResourceSearchState.ProxyDisposed, "Proxy chiuso." }
            };

        /// <summary>
        /// Restituisce la descrizione in formato stringa dello stato specificato.
        /// </summary>
        /// <param name="state">lo stato di cui si richiede la descrizione</param>
        /// <returns>la descrizione in formato stringa dello stato specificato</returns>
        public static string GetStateDescription(ResourceSearchState state)
        {
            string description;
            if (m_InternalTable_1.TryGetValue(state, out description))
                return description;
            return string.Empty;
        }
    }

    #endregion
}
