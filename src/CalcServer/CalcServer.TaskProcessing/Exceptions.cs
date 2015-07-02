using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalcServer.TaskProcessing
{
    #region TaskDataReadException

    /// <summary>
    /// Rappresenta un'eccezione che può verificarsi nella fase di caricamento dei dati
    /// di un task dal relativo stream sorgente, ma comunque prima che questi vengano
    /// elaborati dalla specifica classe di processing.
    /// </summary>
    public class TaskDataReadException : Exception
    {
        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskDataReadException.
        /// </summary>
        public TaskDataReadException()
        {
        }

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskDataReadException con un messaggio di errore specificato.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        public TaskDataReadException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Consente l'inizializzazione di una nuova istanza della classe TaskDataReadException con un messaggio
        /// di errore specificato e un riferimento all'eccezione interna che è la causa dell'eccezione corrente.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        /// <param name="inner">l'eccezione che rappresenta la causa dell'eccezione corrente</param>
        public TaskDataReadException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion
    }

    #endregion

    #region TaskProcessingException

    /// <summary>
    /// Rappresenta un'eccezione che può verificarsi durante l'elaborazione di un task,
    /// ad esempio quando la funzione in esso specificata non viene riconosciuta nella
    /// fase di analisi dei dati o in generale problemi riguardanti l'elaborazione dei
    /// dati relaivi al task, che potrebbe essere utile restituire all'utente.
    /// </summary>
    public class TaskProcessingException : Exception
    {
        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskProcessingException.
        /// </summary>
        public TaskProcessingException()
        {
        }

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskProcessingException con un messaggio di errore specificato.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        public TaskProcessingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Consente l'inizializzazione di una nuova istanza della classe TaskProcessingException con un messaggio
        /// di errore specificato e un riferimento all'eccezione interna che è la causa dell'eccezione corrente.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        /// <param name="inner">l'eccezione che rappresenta la causa dell'eccezione corrente</param>
        public TaskProcessingException(string message, Exception inner) : base(message, inner)
        {
        }

        #endregion
    }

    #endregion

    #region TaskResultWriteException

    /// <summary>
    /// Rappresenta un'eccezione che può verificarsi nella fase di memorizzazione dei risultati
    /// di un task sul relativo stream di destinazione, ma comunque dopo che questi sono stati
    /// elaborati dalla specifica classe di processing.
    /// </summary>
    public class TaskResultWriteException : Exception
    {
        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskResultWriteException.
        /// </summary>
        public TaskResultWriteException()
        {
        }

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskResultWriteException con un messaggio di errore specificato.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        public TaskResultWriteException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Consente l'inizializzazione di una nuova istanza della classe TaskResultWriteException con un messaggio
        /// di errore specificato e un riferimento all'eccezione interna che è la causa dell'eccezione corrente.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        /// <param name="inner">l'eccezione che rappresenta la causa dell'eccezione corrente</param>
        public TaskResultWriteException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion
    }

    #endregion

    #region TaskPerformerException

    /// <summary>
    /// Rappresenta un'eccezione che può verificarsi durante l'elaborazione di un task
    /// da parte di una classe che implementa l'interfaccia ITaskPerformer.
    /// </summary>
    public class TaskPerformerException : Exception
    {
        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskPerformerException.
        /// </summary>
        public TaskPerformerException()
        {
        }

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskPerformerException con un messaggio di errore specificato.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        public TaskPerformerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Consente l'inizializzazione di una nuova istanza della classe TaskPerformerException con un messaggio
        /// di errore specificato e un riferimento all'eccezione interna che è la causa dell'eccezione corrente.
        /// </summary>
        /// <param name="message">messaggio di errore nel quale viene indicato il motivo dell'eccezione</param>
        /// <param name="inner">l'eccezione che rappresenta la causa dell'eccezione corrente</param>
        public TaskPerformerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion
    }

    #endregion
}
