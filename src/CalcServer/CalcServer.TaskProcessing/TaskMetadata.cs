using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using CalcServer.Contracts;

namespace CalcServer.TaskProcessing
{
    /// <summary>
    /// Contiene i metadati associati ad un task che permettono di descriverlo in maniera completa,
    /// ovvero la posizione di dati e risultati, le informazioni sullo stato dell'elaborazione, le
    /// modalità di elaborazione e gli eventuali errori verificatisi durante l'elaborazione.
    /// </summary>
    [DataContract]
    public class TaskMetadata
    {
        #region Properties

        /// <summary>
        /// Il percorso completo del file sorgente contenente i dati del task.
        /// </summary>
        [DataMember]
        public string PathToSourceFile { get; set; }

        /// <summary>
        /// Il percorso completo del file di destinazione contenente i risultati del task.
        /// </summary>
        [DataMember]
        public string PathToTargetFile { get; set; }

        /// <summary>
        /// Il nome completo della classe che implementa il task performer richiesto.
        /// </summary>
        [DataMember]
        public string TaskPerformerClassName { get; set; }

        /// <summary>
        /// La versione della classe che implementa il task performer richiesto.
        /// </summary>
        [DataMember]
        public string TaskPerformerClassVersion { get; set; }

        /// <summary>
        /// Eventuale istante in cui il task diventa disponibile per essere elaborato.
        /// </summary>
        [DataMember]
        public Nullable<DateTime> ReadyTime { get; set; }

        /// <summary>
        /// Eventuale istante di inizio elaborazione del task.
        /// </summary>
        [DataMember]
        public Nullable<DateTime> StartingTime { get; set; }

        /// <summary>
        /// Eventuale istante di fine elaborazione del task.
        /// </summary>
        [DataMember]
        public Nullable<DateTime> CompletionTime { get; set; }

        /// <summary>
        /// Lo stato in cui si trova il task.
        /// </summary>
        [DataMember]
        public TaskState State { get; set; }

        /// <summary>
        /// Gli eventuali errori avvenuti dal momento in cui il task è disponibile.
        /// </summary>
        [DataMember]
        public List<TaskErrorInfo> Errors { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Inizializza una nuova istanza della classe TaskMetadata in base ai valori specificati
        /// per il nome completo e la versione della classe da impiegare per l'elaborazione ed in
        /// base ai nomi dei file recanti il percorso dei dati di origine e di destinazione.
        /// </summary>
        /// <param name="className">nome completo della classe che implementa il task performer</param>
        /// <param name="classVersion">versione della classe che implementa il task performer</param>
        /// <param name="sourceFileName">percorso in cui si trovano i dati da elaborare</param>
        /// <param name="targetFileName">percorso in cui salvare i risultati dell'elaborazione</param>
        /// <remarks>
        /// L'istante in cui il task diventa disponibile per essere elaborato viene inizializzato a null,
        /// quindi occorre impostarlo in modo esplicito insieme allo stato, utilizzando l'apposito metodo
        /// UpdateOnReady.
        /// Gli istanti di inizio e di completamento dell'elaborazione sono inizializzati col valore null
        /// e possono essere impostati usanso i metodi UpdateOnStarting e UpdateOnCompletion: tali metodi
        /// permettono anche di impostare opportunamente lo stato.
        /// Infine, la lista contenente gli eventuali è inizialmente vuota, pertanto gli eventuali errori
        /// vanno aggiunti ad essa utilizzando la relativa property.
        /// </remarks>
        public TaskMetadata(string className, string classVersion, string sourceFileName, string targetFileName)
        {
            TaskPerformerClassName = className;
            TaskPerformerClassVersion = classVersion;
            PathToSourceFile = sourceFileName;
            PathToTargetFile = targetFileName;

            State = TaskState.None;

            ReadyTime = null;
            StartingTime = null;
            CompletionTime = null;

            Errors = new List<TaskErrorInfo>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Permette di aggiornare le informazioni sul task relative all'istante in cui il task
        /// diventa disponibile per l'elaborazione, modificando opportunamente anche lo stato.
        /// </summary>
        /// <param name="ready">istante in cui il task diventa disponibile per essere elaborato</param>
        public void UpdateOnReady(DateTime ready)
        {
            ReadyTime = ready;
            State = TaskState.Ready;
        }

        /// <summary>
        /// Permette di aggiornare le informazioni sul task relative all'istante di inizio elaborazione,
        /// modificando opportunamente anche lo stato.
        /// </summary>
        /// <param name="started">istante di inizio dell'elaborazione</param>
        public void UpdateOnStarting(DateTime started)
        {
            StartingTime = started;
            State = TaskState.Started;
        }

        /// <summary>
        /// Permette di aggiornare le informazioni sul task relative all'istante di fine elaborazione,
        /// modificando opportunamente anche lo stato.
        /// </summary>
        /// <param name="finished">istante di completamento dell'elaborazione</param>
        public void UpdateOnCompletion(DateTime finished)
        {
            CompletionTime = finished;
            State = TaskState.Completed;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Restituisce una copia di questo oggetto.
        /// </summary>
        /// <returns>una copia di questo oggetto</returns>
        public static TaskMetadata Copy(TaskMetadata tm)
        {
            TaskMetadata copy = new TaskMetadata(null, null, null, null)
            {
                TaskPerformerClassName = string.Copy(tm.TaskPerformerClassName),
                TaskPerformerClassVersion = string.Copy(tm.TaskPerformerClassVersion),
                PathToSourceFile = string.Copy(tm.PathToSourceFile),
                PathToTargetFile = string.Copy(tm.PathToTargetFile),

                State = tm.State,

                ReadyTime = tm.ReadyTime,
                StartingTime = tm.StartingTime,
                CompletionTime = tm.CompletionTime,

                Errors = new List<TaskErrorInfo>(tm.Errors.Count)
            };

            foreach (TaskErrorInfo error in tm.Errors) { copy.Errors.Add(TaskErrorInfo.Copy(error)); }

            return copy;
        }

        #endregion
    }
}
