using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using CalcServer.Logging;
using CalcServer.Contracts;

namespace CalcServer.TaskProcessing
{
    /// <summary>
    /// Assicura un accesso centralizzato alle funzionalità di accodamento e di elaborazione dei task,
    /// fornendo un'istanza thread-safe di TaskProcessingManager unica nell'applicazione, ottenuta con
    /// l'impiego del pattern singleton in versione thread-safe.
    /// </summary>
    public class TaskProcessingManager
    {
        #region Singleton

        private static readonly TaskProcessingManager m_Instance = new TaskProcessingManager();

        /// <summary>
        /// Ottiene l'istanza thread-safe di TaskProcessingManager unica nell'applicazione.
        /// </summary>
        public static TaskProcessingManager Instance { get { return m_Instance; } }

        /// <summary>
        /// Il costruttore privato evita che possano essere create ulteriori istanze.
        /// </summary>
        private TaskProcessingManager() { ; }

        #endregion

        #region Fields

        private readonly object m_ContextProviderLocker = new object();
        private ITaskPerformerContextProvider m_ContextProvider = new TaskPerformerContextManager();

        private readonly object m_PendingTasksLocker = new object();
        private readonly TaskProcessingTable m_PendingTasks = new TaskProcessingTable();

        // Ipotesi: i metodi della classe Logger sono thread-safe.
        private readonly Logger m_AppLogger = Logger.Instance;

        #endregion

        #region Public Methods

        /// <summary>
        /// Imposta il provider interno di accesso ai contesti di elaborazione dei task, utilizzando quello
        /// specificato, restituendo true se quest'ultimo è diverso da null; in caso contrario, viene usato
        /// il provider predefinito e restituisce false per segnalarlo.
        /// </summary>
        /// <param name="provider">il provider da usare per l'elaborazione dei task</param>
        /// <returns>true se il nuovo provider viene impostato correttamente</returns>
        public bool SetContextProvider(ITaskPerformerContextProvider provider)
        {
            if (provider == null)
            {
                lock (m_ContextProviderLocker)
                {
                    m_ContextProvider = new TaskPerformerContextManager();
                }
                return false;
            }
            else
            {
                lock (m_ContextProviderLocker)
                {
                    m_ContextProvider = provider;
                }
                return true;
            }
        }

        /// <summary>
        /// Schedula il task specificato per l'esecuzione e imposta in output l'identificativo assegnato,
        /// restituendo true se l'oggetto contenente i metadati specificati è diverso da null. Altrimenti,
        /// cioè se l'oggetto specificato è null, l'identificativo viene impostato a null e questo metodo
        /// restituisce false, non potendo schedulare un task null.
        /// </summary>
        /// <param name="tm">i metadati del task da schedulare per l'elaborazione</param>
        /// <param name="id">l'identificativo assegnato al task</param>
        /// <returns>true se il task specificato è stato schedulato, altrimenti false</returns>
        public bool InsertUserTask(TaskMetadata tm, out string id)
        {
            if (tm != null)
            {
                string newId;
                lock (m_PendingTasksLocker)
                {
                    m_PendingTasks.InsertTask(tm, out newId);
                }
                id = newId;

                Task.Factory.StartNew(() => ExecuteTask(newId),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                return true;
            }

            id = null;
            return false;
        }

        /// <summary>
        /// Rimuove il task associato all'identificativo specificato e restituisce true se il task è stato rimosso
        /// con successo, altrimenti false se non è stato possibile rimuovere il task perché non presente. Qualora
        /// si rimuova un task non ancora iniziato, esso non verrà elaborato. Se si rimuove un task durante la sua
        /// elaborazione, i risultati prodotti verranno rimossi.
        /// </summary>
        /// <param name="id">l'identificativo associato al task da rimuovere</param>
        /// <returns>true se il task è stato rimosso, altrimenti false se non è presente</returns>
        public bool RemoveUserTask(string id)
        {
            lock (m_PendingTasksLocker)
            {
                return m_PendingTasks.RemoveTask(id);
            }
        }

        /// <summary>
        /// Rimuove tutti i task che corrispondono alle condizioni definite dal predicato specificato.
        /// </summary>
        /// <param name="match">delegato che definisce le condizioni dei task da rimuovere</param>
        public void RemoveUserTasks(FilterTaskMetadata match)
        {
            lock (m_PendingTasksLocker)
            {
                m_PendingTasks.RemoveTasks(match);
            }
        }

        /// <summary>
        /// Prova ad ottenere lo stato del task associato all'identificativo specificato. Restituisce true
        /// se tale identificativo esiste ed in tal caso assegna in output lo stato del task richiesto. In
        /// caso contrario, ovvero se l'identificativo specificato non esiste, restituisce false e imposta
        /// l'output al valore TaskState.None.
        /// </summary>
        /// <param name="id">l'identificativo associato al task richiesto</param>
        /// <param name="ts">lo stato relativo al task richiesto</param>
        /// <returns>true, se esiste il task richiesto, altrimenti false</returns>
        public bool TryGetUserTaskState(string id, out TaskState ts)
        {
            lock (m_PendingTasksLocker)
            {
                TaskMetadata temp;
                if (m_PendingTasks.TryGetTask(id, out temp) && temp != null)
                {
                    ts = temp.State;
                    return true;
                }
            }

            ts = TaskState.None;
            return false;
        }

        /// <summary>
        /// Prova ad ottenere l'oggetto contenente i metadati del task associati all'identificativo specificato.
        /// Restituisce true se tale identificativo specificato esiste ed in tal caso assegna in output l'oggetto
        /// con i metadati richiesti. Altrimenti, se l'identificativo specificato non esiste, restituisce false e
        /// imposta l'output a null.
        /// </summary>
        /// <param name="id">l'identificativo associato al task richiesto</param>
        /// <param name="tm">l'oggetto contenente i metadati richiesti</param>
        /// <returns>true, se esiste l'oggetto con i metadati cercati, altrimenti false</returns>
        /// <remarks>
        /// I metadati del task specificato vengono copiati nell'oggetto restituito.
        /// </remarks>
        public bool TryGetUserTask(string id, out TaskMetadata tm)
        {
            lock (m_PendingTasksLocker)
            {
                TaskMetadata temp;
                if (m_PendingTasks.TryGetTask(id, out temp) && temp != null)
                {
                    tm = TaskMetadata.Copy(temp);
                    return true;
                }
            }

            tm = null;
            return false;
        }

        #endregion

        #region Processing Private Methods

        /// <summary>
        /// Esegue il task identificato dai dati specificati se esso è ancora presente in elenco.
        /// </summary>
        /// <param name="id">l'identificativo di elaborazione del task da eseguire</param>
        private void ExecuteTask(object id)
        {
            string taskProcId = id as string;
            
            TaskMetadata tm;
            lock (m_PendingTasksLocker)
            {
                if (!m_PendingTasks.TryGetTask(taskProcId, out tm))
                {
                    return;   // task non trovato
                }

                tm.UpdateOnStarting(DateTime.Now);

                if (!m_PendingTasks.UpdateTask(taskProcId, tm))
                {
                    return;   // task non trovato
                }
            }

            WriteToLog("Starting Task (processing id = {0})...\n{1}", taskProcId, BuildTaskDescriptionString(tm));

            TaskErrorInfo error = null;
            try
            {
                TaskPerformerContext currentTaskProcessingContext;
                bool found;

                lock (m_ContextProviderLocker)
                {
                    found = m_ContextProvider.TryGetContext(tm.TaskPerformerClassName,
                        tm.TaskPerformerClassVersion, out currentTaskProcessingContext);
                }

                if (found)
                {
                    currentTaskProcessingContext.Execute(
                        tm.PathToSourceFile, tm.PathToTargetFile);
                }
                else
                {
                    HandleTaskPerformerClassError(taskProcId, tm.TaskPerformerClassName,
                        tm.TaskPerformerClassVersion, out error);
                }
            }
            catch (TaskPerformerException e)
            {
                HandleTaskPerformerError(taskProcId, e, out error);
            }
            catch (Exception e)
            {
                HandleInternalError(taskProcId, e, out error);
            }
            finally
            {
                tm.UpdateOnCompletion(DateTime.Now);
                if (error != null)
                {
                    tm.Errors.Add(error);
                }
            }

            WriteToLog("Completing Task (processing id = {0})...\n{1}", taskProcId, BuildTaskDescriptionString(tm));

            if (tm != null)
            {
                lock (m_PendingTasksLocker)
                {
                    if (!m_PendingTasks.UpdateTask(taskProcId, tm))
                    {
                        ;   // task non trovato
                    }
                }
            }
        }

        /// <summary>
        /// Costruisce una rappresentazione in formato stringa delle informazioni principali sul task specificato.
        /// </summary>
        /// <param name="tm">le informazioni relative al task</param>
        /// <returns>una rappresentazione in formato stringa delle informazioni principali sul task</returns>
        private string BuildTaskDescriptionString(TaskMetadata tm)
        {
            StringBuilder str = new StringBuilder();

            if (tm != null)
            {
                str.Append("Source: " + tm.PathToSourceFile);
                str.Append(Environment.NewLine);
                str.Append("Target: " + tm.PathToTargetFile);
                str.Append(Environment.NewLine);
                str.Append("Component: class = " + tm.TaskPerformerClassName + ", version = " + tm.TaskPerformerClassVersion);
                str.Append(Environment.NewLine);
                str.Append("Ready: " + tm.ReadyTime.ToString());
                str.Append(Environment.NewLine);
                str.Append("Starting: " + tm.StartingTime.ToString());
                str.Append(Environment.NewLine);
                str.Append("Completion: " + tm.CompletionTime.ToString());
                str.Append(Environment.NewLine);
                str.Append("State: " + tm.State.ToString());
                str.Append(Environment.NewLine);
                str.Append("Errors: " + tm.Errors.Count);
            }

            return str.ToString();
        }
        
        #endregion

        #region Error Handling Private Methods

        /// <summary>
        /// Gestisce l'errore dovuto al componente non trovato per l'elaborazione del task.
        /// </summary>
        /// <param name="taskProcId">identificativo di elaborazione del task che ha provocato l'errore</param>
        /// <param name="className">nome completo della classe mancante</param>
        /// <param name="classVersion">versione della classe mancante</param>
        /// <param name="error">l'errore da restituire all'utente</param>
        private void HandleTaskPerformerClassError(string taskProcId, string className, string classVersion, out TaskErrorInfo error)
        {
            string errorId;
            string errorDetails = string.Format("Component Not Available: class = {0}, version = {1}", className, classVersion);
            HandleError(taskProcId, errorDetails, out errorId);

            // Errore da restituire all'utente.
            error = new TaskErrorInfo() { Id = errorId, Details = errorDetails, Code = TaskErrorCode.ComponentNotFound };
        }

        /// <summary>
        /// Gestisce gli errori che si possono verificare durante l'elaborazione del task.
        /// </summary>
        /// <param name="taskProcId">identificativo di elaborazione del task che ha provocato l'errore</param>
        /// <param name="exception">l'eccezione contenente gli eventuali dettagli dell'errore</param>
        /// <param name="error">l'errore da restituire all'utente</param>
        private void HandleTaskPerformerError(string taskProcId, TaskPerformerException exception, out TaskErrorInfo error)
        {
            string errorId;
            string errorDetails = exception.ToString();
            HandleError(taskProcId, errorDetails, out errorId);

            var inner = exception.InnerException;

            TaskErrorCode code;
            string details;

            if (inner is TaskDataReadException)
            {
                code = TaskErrorCode.ComponentReadDataFailed;
                details = errorDetails;
            }
            else if (inner is TaskProcessingException)
            {
                code = TaskErrorCode.ComponentProcessingFailed;
                details = errorDetails;
            }
            else if (inner is TaskResultWriteException)
            {
                code = TaskErrorCode.ComponentWriteResultFailed;
                details = errorDetails;
            }
            else
            {
                code = TaskErrorCode.ComponentUnknownError;
                details = string.Empty;   // exception shielding
            }

            // Errore da restituire all'utente.
            error = new TaskErrorInfo() { Id = errorId, Details = details, Code = code };
        }

        /// <summary>
        /// Gestisce gli errori che si possono verificare durante l'apertura del file di origine contenente
        /// i dati associati al task oppure durante la creazione del file di destinazione per i risultati.
        /// </summary>
        /// <param name="taskProcId">identificativo di elaborazione del task che ha provocato l'errore</param>
        /// <param name="exception">l'eccezione contenente gli eventuali dettagli dell'errore</param>
        /// <param name="error">l'errore da restituire all'utente</param>
        private void HandleInternalError(string taskProcId, Exception exception, out TaskErrorInfo error)
        {
            string errorId;
            string errorDetails = exception.ToString();
            HandleError(taskProcId, errorDetails, out errorId);

            // Errore da restituire all'utente.
            error = new TaskErrorInfo()
            {
                Id = errorId,
                Details = string.Empty,   // exception shielding
                Code = TaskErrorCode.InternalError
            };
        }

        /// <summary>
        /// Genera un identificativo univoco per l'errore e lo associa ai dati specificati per effettuarne
        /// il log mediante l'event logger di questa applicazione.
        /// </summary>
        /// <param name="taskProcId">identificativo di elaborazione del task che ha provocato l'errore</param>
        /// <param name="errorDetails">informazioni che descrivono l'errore verificatosi</param>
        /// <param name="errorId">identificativo univoco dell'errore verificatosi</param>
        private void HandleError(string taskProcId, string errorDetails, out string errorId)
        {
            errorId = Guid.NewGuid().ToString();
            WriteToLog("{0}{1}.{2}{3}{4}.{5}{6}{7}",
                "Error Id: ", errorId,
                Environment.NewLine, "Task Processing Id: ", taskProcId,
                Environment.NewLine, "Error Description: ", errorDetails);
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
                GetType().AssemblyQualifiedName.ToString(),   // oppure: GetType().FullName.ToString(),
                Thread.CurrentThread.ManagedThreadId);
            string textToLog = string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args);

            m_AppLogger.Write(logTime, moduleInfo, textToLog);
        }

        #endregion
    }
}
