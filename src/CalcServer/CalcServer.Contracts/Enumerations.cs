using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CalcServer.Contracts
{
    #region TaskState

    /// <summary>
    /// Rappresenta i possibili stati in cui si può trovare un task.
    /// </summary>
    [DataContract]
    public enum TaskState
    {
        /// <summary>
        /// Indica che il task non è presente.
        /// </summary>
        [EnumMember]
        None,

        /// <summary>
        /// Il task è pronto per essere elaborato.
        /// </summary>
        [EnumMember]
        Ready,

        /// <summary>
        /// Il processo di elaborazione del task è iniziato.
        /// </summary>
        [EnumMember]
        Started,

        /// <summary>
        /// I risultati dell'elaborazione del task sono disponibili.
        /// </summary>
        [EnumMember]
        Completed
    }

    #endregion

    #region TaskErrorCode

    /// <summary>
    /// Elenca le tipologie di errore che si possono verificare dall'inizio alla fine dell'elaborazione
    /// di un task, incluse le fasi di lettura dei dati e di scrittura dei risultati.
    /// </summary>
    [DataContract]
    public enum TaskErrorCode
    {
        /// <summary>
        /// Indica che la tipologia di errore non è stata specificata.
        /// </summary>
        [EnumMember]
        Unknown,

        /// <summary>
        /// Indica che il componente richiesto per l'elaborazione di un task non è stato trovato, poiché
        /// potrebbe essere stato rimosso o disattivato dall'amministratore del sistema, oppure potrebbe
        /// non essere stato installato.
        /// </summary>
        [EnumMember]
        ComponentNotFound,

        /// <summary>
        /// Indica che il componente per l'elaborazione di un task ha riscontrato un errore durante la lettura
        /// dei dati dal file associato a tale task, pertanto l'elaborazione non può proseguire. Le cause alla
        /// base di questo errore sono da ricercare nel formato del file, non supportato dall'attuale versione
        /// della classe che implementa la funzionalità richiesta per l'elaborazione.
        /// </summary>
        [EnumMember]
        ComponentReadDataFailed,

        /// <summary>
        /// Indica che il componente per l'elaborazione di un task ha riscontrato un errore durante le operazioni
        /// inerenti l'elaborazione di tale task, che quindi non può essere completata in modo corretto. Le cause
        /// che potrebbero aver provocato questo errore dipendono dalla classe che implementa le funzionalità del
        /// componente.
        /// </summary>
        [EnumMember]
        ComponentProcessingFailed,

        /// <summary>
        /// Indica che il componente per l'elaborazione di un task ha riscontrato un errore durante la scrittura
        /// dei risultati sul file di destinazione, quindi i dati derivanti dall'elaborazione non possono essere 
        /// salvati. Le cause di questo errore sono da ricercare nella classe che implementa le funzionalità del
        /// componente.
        /// </summary>
        [EnumMember]
        ComponentWriteResultFailed,

        /// <summary>
        /// Indica che il componente per l'elaborazione di un task ha riscontrato un errore la cui tipologia è
        /// sconosciuta poiché la relativa eccezione non è stata rilanciata attraverso i tre tipi di eccezione
        /// previsti per i componenti di elaborazione.
        /// </summary>
        [EnumMember]
        ComponentUnknownError,

        /// <summary>
        /// Indica che si è verificato un errore interno prima che i dati del task contenuti nel file di origine
        /// venissero passati al componente. Questo errore potrebbe dipendere da problemi di accesso a tale file
        /// o di creazione del file di destinazione.
        /// </summary>
        [EnumMember]
        InternalError
    }

    #endregion

    #region ServiceFaultCode

    /// <summary>
    /// Elenca le tipologie di errore che si possono verificare durante l'utilizzo del servizio di elaborazione
    /// dei task inviati dagli utenti.
    /// </summary>
    [DataContract]
    public enum ServiceFaultCode
    {
        /// <summary>
        /// Indica che la tipologia di errore non è stata specificata.
        /// </summary>
        [EnumMember]
        Unknown,

        /// <summary>
        /// Indica che non è stato possibile generare l'identificativo univoco da associare alla richiesta
        /// di elaborazione di un task, quindi non è stato possibile accettare la richiesta.
        /// </summary>
        [EnumMember]
        TaskGenerateRequestIdFailed,

        /// <summary>
        /// Indica che si è verificato un errore durante la ricezione del messaggio contenente i dati del task
        /// di cui è stata richiesta l'elaborazione.
        /// </summary>
        [EnumMember]
        ReceiveTaskDataFailed,

        /// <summary>
        /// Indica che il file contenente i dati del task da eseguire non è stato scritto nel formato corretto,
        /// quindi non è stato possibile proseguire con la richiesta.
        /// </summary>
        [EnumMember]
        TaskDataFormatError,

        /// <summary>
        /// Indica che il componente richiesto per l'elaborazione di un task non è disponibile sul server,
        /// quindi non è stato possibile proseguire con la richiesta.
        /// </summary>
        [EnumMember]
        ComponentUnavailable,

        /// <summary>
        /// Indica che i risultati dell'elaborazione di un task non sono stati trovati.
        /// </summary>
        [EnumMember]
        TaskResultsNotFound,

        /// <summary>
        /// Indica che si è verificato un errore durante l'invio del messaggio contenente i risultati del task
        /// di cui è stata richiesta l'elaborazione.
        /// </summary>
        [EnumMember]
        SendTaskResultsFailed,

        /// <summary>
        /// Indica che si è verificato un errore interno.
        /// </summary>
        [EnumMember]
        InternalError
    }

    #endregion
}
