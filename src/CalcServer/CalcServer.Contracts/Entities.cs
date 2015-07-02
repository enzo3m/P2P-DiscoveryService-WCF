using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CalcServer.Contracts
{
    #region TaskData

    /// <summary>
    /// Raccoglie i dati richiesti per l'elaborazione di un task e le informazioni ad essi associate.
    /// </summary>
    [DataContract]
    public class TaskData
    {
        /// <summary>
        /// Il nome che il client ha assegnato al task.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// La stringa in formato XML contenente i dati del task.
        /// </summary>
        [DataMember]
        public string Contents { get; set; }
    }

    #endregion

    #region TaskResults

    /// <summary>
    /// Raccoglie i risultati ottenuti dall'elaborazione di un task e le informazioni ad essi associate.
    /// </summary>
    [DataContract]
    public class TaskResults
    {
        /// <summary>
        /// Il tempo impiegato sul server per portare a termine l'elaborazione del task.
        /// </summary>
        [DataMember]
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// La descrizione degli eventuali errori riscontrati durante l'elaborazione del task.
        /// </summary>
        [DataMember]
        public List<TaskErrorInfo> EncounteredErrors { get; set; }

        /// <summary>
        /// La stringa in formato XML contenente i risultati del task elaborato.
        /// </summary>
        [DataMember]
        public string Contents { get; set; }
    }

    #endregion

    #region TaskErrorInfo

    /// <summary>
    /// Raccoglie le informazioni sugli eventuali errori che si possono verificare dall'inizio alla fine
    /// dell'elaborazione di un task, incluse le fasi di lettura dei dati e di scrittura dei risultati.
    /// </summary>
    [DataContract]
    public class TaskErrorInfo
    {
        #region Properties

        /// <summary>
        /// Identifica univocamente l'errore verificatosi.
        /// </summary>
        [DataMember]
        public String Id { get; set; }

        /// <summary>
        /// Il codice che descrive l'errore verificatosi.
        /// </summary>
        [DataMember]
        public TaskErrorCode Code { get; set; }

        /// <summary>
        /// I dettagli riguardanti l'errore verificatosi.
        /// </summary>
        [DataMember]
        public String Details { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskErrorInfo con i valori predefiniti: l'identificativo
        /// ed i dettagli sono impostati come stringhe vuote, mentre il codice sul valore TaskErrorCode.Unknown.
        /// </summary>
        public TaskErrorInfo()
        {
            Id = String.Empty;
            Code = TaskErrorCode.Unknown;
            Details = String.Empty;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Restituisce una copia di questo oggetto.
        /// </summary>
        /// <returns>una copia di questo oggetto</returns>
        public static TaskErrorInfo Copy(TaskErrorInfo error)
        {
            TaskErrorInfo copy = new TaskErrorInfo()
            {
                Id = string.Copy(error.Id),
                Code = error.Code,
                Details = string.Copy(error.Details)
            };

            return copy;
        }

        #endregion
    }

    #endregion

    #region ServiceFault

    /// <summary>
    /// Raccoglie le informazioni su un eventuale errore che si può verificare durante l'utilizzo del servizio
    /// di elaborazione di un task.
    /// </summary>
    [DataContract]
    public class ServiceFault
    {
        #region Properties

        /// <summary>
        /// L'identificativo univoco dell'errore verificatosi sul server.
        /// </summary>
        [DataMember]
        public String Id { get; set; }

        /// <summary>
        /// Il codice che descrive la tipologia di errore verificatosi sul server.
        /// </summary>
        [DataMember]
        public ServiceFaultCode Code { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza della classe ProcessingServiceFault con i valori predefiniti: l'identificativo
        /// viene impostato come stringa vuote, mentre il codice sul valore ProcessingServiceFaultCode.Unknown.
        /// </summary>
        public ServiceFault()
        {
            Id = String.Empty;
            Code = ServiceFaultCode.Unknown;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Restituisce una copia di questo oggetto.
        /// </summary>
        /// <returns>una copia di questo oggetto</returns>
        public static ServiceFault Copy(ServiceFault error)
        {
            ServiceFault copy = new ServiceFault()
            {
                Id = string.Copy(error.Id),
                Code = error.Code
            };

            return copy;
        }

        #endregion
    }

    #endregion
}
