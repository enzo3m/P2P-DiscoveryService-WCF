using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

using CalcServer.Logging;
using CalcServer.Configuration;
using CalcServer.Contracts;
using CalcServer.TaskProcessing;

namespace CalcServer.Services
{
    /// <summary>
    /// Implementa il backend del servizio di elaborazione con i metodi necessari al funzionamento dello stesso.
    /// </summary>
    internal class ProcessingServiceBackend
    {
        #region Fields

        private readonly Settings m_AppConfig = Settings.Instance;
        private readonly Logger m_AppLogger = Logger.Instance;
        private readonly TaskProcessingManager m_TaskScheduler = TaskProcessingManager.Instance;

        // La tabella sottostante associa l'id della richiesta con l'id di elaborazione
        // e deve essere condivisa tra tutte le istanze attive del servizio.
        private readonly object m_IdTableLocker = new object();
        private readonly Dictionary<string, string> m_IdTable = new Dictionary<string, string>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Verifica se la risorsa di elaborazione specificata è abilitata su questo server ed in caso positivo
        /// restituisce true, altrimenti restituisce false.
        /// </summary>
        /// <param name="name">il nome completo della classe che implementa la risorsa di elaborazione</param>
        /// <param name="version">la versione della classe che implementa la risorsa di elaborazione</param>
        /// <returns>true se la risorsa di elaborazione specificata è abilitata, altrimenti false</returns>
        public bool IsResourceEnabled(string name, string version)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            string resource = string.Format("{0}-{1}", name, version);
            return m_AppConfig.IsResourceEnabled(resource);
        }

        /// <summary>
        /// Restituisce l'elenco di tutte le risorse attualmente disponibili sul servizio e abilitate, oppure
        /// un elenco vuoto se nessuna risorsa è disponibile perché disabilitate o non esistenti.
        /// </summary>
        /// <returns>l'elenco delle risorse disponibili e abilitate sul servizio</returns>
        public List<String> GetEnabledResources()
        {
            return new List<string>(m_AppConfig.Resources);
        }

        /// <summary>
        /// Restituisce il tempo impiegato per elaborare il task descritto dai metadati specificati.
        /// </summary>
        /// <param name="tm">i metadati relativi al task</param>
        /// <returns>il tempo impiegato per elaborare il task</returns>
        /// <remarks>
        /// Se l'oggetto contenente i metadati è null oppure se in essi non sono stati impostati l'istante
        /// di completamento dell'elaborazione e/o l'istante di inizio dell'elaborazione, viene restituito
        /// il valore TimeSpan.Zero.
        /// </remarks>
        public TimeSpan GetProcessingTime(TaskMetadata tm)
        {
            if (tm != null && tm.CompletionTime.HasValue && tm.StartingTime.HasValue)
            {
                DateTime t1 = tm.StartingTime.Value;
                DateTime t2 = tm.CompletionTime.Value;

                if (t2 > t1)
                {
                    return t2.Subtract(t1);
                }
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Prova a generare un identificativo di 16 byte sicuro dal punto di vista crittografico, restituendo true
        /// se la procedura viene completata correttamente. In caso contrario, viene impostato l'errore esplicativo
        /// e il metodo restituisce false.
        /// </summary>
        /// <param name="id">l'identificativo generato in modo sicuro, oppure null in caso di errore</param>
        /// <param name="error">l'eventuale errore impostato qualora si dovessero verificare dei problemi</param>
        /// <returns>true se l'identificativo viene generato correttamente, altrimenti false</returns>
        /// <remarks>
        /// Se l'identificativo viene generato correttamente, l'oggetto dedicato all'eventuale errore viene impostato
        /// al valore null, altrimenti al suo interno vengono impostati il codice d'errore e un identificativo che lo
        /// identifica univocamente sul server, oltre ad effettuare il logging dell'errore.
        /// </remarks>
        public bool TryGetRandomId(out string id, out ServiceFault error)
        {
            bool result;
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            try
            {
                byte[] randomBytes = new byte[16];
                rng.GetBytes(randomBytes);

                Guid guid = new Guid(randomBytes);

                id = guid.ToString();
                error = null;
                result = true;
            }
            catch (CryptographicException e)
            {
                HandleError(e.ToString(), ServiceFaultCode.TaskGenerateRequestIdFailed, out error);
                id = null;
                result = false;
            }
            finally
            {
                rng.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Restituisce un nuovo percorso per il prossimo file in cui salvare i dati associati ad un task,
        /// generando casualmente il nome del file e impostandovi l'estensione prevista.
        /// </summary>
        /// <returns>un nuovo percorso per il prossimo file in cui salvare i dati di un task da elaborare</returns>
        public string GetTaskDataFilePath()
        {
            return GenerateRandomFileName(m_AppConfig.ReadyTasksFolder, "xml");
        }

        /// <summary>
        /// Restituisce un nuovo percorso per il prossimo file in cui salvare i risultati dell'elaborazione,
        /// di un task, generando casualmente il nome del file e impostandovi l'estensione prevista.
        /// </summary>
        /// <returns>un nuovo percorso per il prossimo file in cui salvare i risultati di un task elaborato</returns>
        public string GetTaskResultsFilePath()
        {
            return GenerateRandomFileName(m_AppConfig.CompletedTasksFolder, "xml");
        }

        /// <summary>
        /// Prova a salvare in locale su file la stringa in formato XML contenente i dati del task e restituisce
        /// true se la procedura viene completata correttamente. In caso contrario, restituisce false ed imposta
        /// l'errore esplicativo.
        /// </summary>
        /// <param name="contents">stringa in formato XML contenente i dati del task</param>
        /// <param name="targetFileName">il percorso completo sul server in cui salvare i dati ricevuti</param>
        /// <param name="error">l'eventuale errore impostato qualora si dovessero verificare dei problemi</param>
        /// <returns>true se non si verificano errori durante la scrittura del file, altrimenti false</returns>
        /// <remarks>
        /// Se la stringa ricevuta viene scritta con successo, l'oggetto dedicato all'eventuale errore è impostato
        /// al valore null, altrimenti al suo interno vengono impostati il codice d'errore e un identificativo
        /// univoco sul server, oltre ad effettuare il logging dell'errore.
        /// </remarks>
        public bool TrySaveDataToFile(string contents, string targetFileName, out ServiceFault error)
        {
            bool result;
            
            try
            {
                File.WriteAllText(targetFileName, contents);

                error = null;
                result = true;
            }
            catch (Exception e)
            {
                HandleError(e.ToString(), ServiceFaultCode.ReceiveTaskDataFailed, out error);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Carica il file specificato contenente i dati da elaborare e prova ad estrarre le informazioni relative
        /// alla risorsa richiesta per la loro elaborazione. In caso di successo, verifica se tale risorsa è stata
        /// attivata su questo server e in caso affermativo restituisce true e imposta il nome e la versione della
        /// risorsa (il nome completo e la versione della classe che implementa la risorsa di elaborazione ).
        /// In caso contrario, se si verificano errori durante la lettura dei dati, oppure se la risorsa richiesta
        /// non è stata abilitata su questo server, il metodo restituisce false ed imposta un errore esplicativo.
        /// </summary>
        /// <param name="tdFilePath">il percorso completo del file da cui estrarre le informazioni</param>
        /// <param name="name">nome completo della classe che implementa la risorsa di elaborazione</param>
        /// <param name="version">versione della classe che implementa la risorsa di elaborazione</param>
        /// <param name="error">l'eventuale errore impostato qualora si dovessero verificare dei problemi</param>
        /// <returns>true se non si verificano errori durante la lettura del file, altrimenti false</returns>
        /// <remarks>
        /// Se le informazioni vengono lette correttamente, l'oggetto dedicato all'eventuale errore è impostato
        /// al valore null, altrimenti al suo interno vengono impostati il codice d'errore e un identificativo
        /// univoco sul server, oltre ad effettuare il logging dell'errore.
        /// </remarks>
        public bool TrySearchResource(string tdFilePath, out string name, out string version, out ServiceFault error)
        {
            bool result;
            
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(tdFilePath);

                XmlElement root = document.DocumentElement;
                XmlNodeList nodeList = root.SelectNodes("/task/component[@className and @classVersion]");
                XmlNode node = (nodeList.Count == 1 ? nodeList[0] : null);

                name = (node != null ? node.Attributes["className"].Value : string.Empty);
                version = (node != null ? node.Attributes["classVersion"].Value : string.Empty);

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version))
                {
                    HandleError("TrySearchResource: unable to find the information about the requested processing resource.",
                        ServiceFaultCode.TaskDataFormatError, out error);
                    name = version = null;
                    result = false;
                }
                else if (IsResourceEnabled(name, version))
                {
                    error = null;
                    result = true;
                }
                else
                {
                    HandleError("TrySearchResource: the requested processing resource has not been enabled.",
                        ServiceFaultCode.ComponentUnavailable, out error);
                    name = version = null;
                    result = false;
                }
            }
            catch (Exception e)
            {
                HandleError(e.ToString(), ServiceFaultCode.InternalError, out error);
                name = version = null;
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Prova a mettere il task specificato in coda per l'elaborazione associandolo all'identificativo specificato
        /// e restituisce true se l'inserimento viene completate con successo. In caso contrario, restituisce false ed
        /// imposta l'errore esplicativo.
        /// </summary>
        /// <param name="tm">i metadati relativi al task di cui è stata richiesta l'elaborazione</param>
        /// <param name="taskRequestId">l'identificativo univoco della richiesta di elaborazione</param>
        /// <param name="error">l'eventuale errore impostato qualora si dovessero verificare dei problemi</param>
        /// <returns>true se non si verificano errori durante l'accodamento del task, altrimenti false</returns>
        /// <remarks>
        /// Se il task viene accodato con successo, l'oggetto dedicato all'eventuale errore è impostato al valore null,
        /// altrimenti al suo interno vengono impostati il codice d'errore e un identificativo univoco sul server,
        /// oltre ad effettuare il logging dell'errore.
        /// </remarks>
        public bool TryQueueTask(TaskMetadata tm, string taskRequestId, out ServiceFault error)
        {
            bool result;

            string taskProcId;
            if (m_TaskScheduler.InsertUserTask(tm, out taskProcId))
            {
                if (TryInsertIdPair(taskRequestId, taskProcId))
                {
                    WriteToLog("TryQueueTask: task scheduled with request id = {0}, processing id = {1}.",
                        taskRequestId, taskProcId);
                    error = null;
                    result = true;
                }
                else
                {
                    string errorDetails = "TryQueueTask: unable to insert task identifiers within table: " +
                        string.Format("request id = {0}, processing id = {1}.", taskRequestId, taskProcId);
                    HandleError(errorDetails, ServiceFaultCode.InternalError, out error);
                    result = false;
                }
            }
            else
            {
                HandleError("TryQueueTask: unable to queue a new task.", ServiceFaultCode.InternalError, out error);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Verifica che i metadati del task associato all'identificativo specificato siano esistenti ed eventualmente
        /// ne restituisce lo stato corrente. In caso contrario, restituisce il valore TaskState.None per indicare che
        /// il task non è presente.
        /// </summary>
        /// <param name="taskRequestId">l'identificativo associato al task di cui si richiede lo stato attuale</param>
        /// <returns>lo stato attuale del task identificato, altrimenti TaskState.None se il task non esiste</returns>
        public TaskState GetTaskState(string taskRequestId)
        {
            string taskProcId;

            lock (m_IdTableLocker)
            {
                string temp;
                if (!m_IdTable.TryGetValue(taskRequestId, out temp))
                {
                    return TaskState.None;
                }
                taskProcId = string.Copy(temp);
            }

            TaskState taskCurrentState;
            if (!m_TaskScheduler.TryGetUserTaskState(taskProcId, out taskCurrentState))
            {
                return TaskState.None;
            }

            return taskCurrentState;
        }

        /// <summary>
        /// Verifica che i metadati del task associato all'identificativo specificato siano esistenti ed eventualmente
        /// li copia in output e restituisce true. In caso contrario restituisce false ed imposta i metadati in output
        /// al valore null e un errore esplicativo.
        /// </summary>
        /// <param name="taskRequestId">l'identificativo univoco della richiesta di elaborazione</param>
        /// <param name="tm">la copia dei metadati relativi al task di cui è stata richiesta l'elaborazione</param>
        /// <param name="error">l'eventuale errore impostato qualora si dovessero verificare dei problemi</param>
        /// <returns>true se non si verificano errori durante il recupero dei metadati, altrimenti false</returns>
        /// <remarks>
        /// Se i metadati richiesti vengono trovati, l'oggetto dedicato all'eventuale errore è impostato al valore null,
        /// altrimenti al suo interno vengono impostati il codice d'errore e un identificativo univoco sul server, oltre
        /// ad effettuare il logging dell'errore.
        /// </remarks>
        public bool TryGetUserTask(string taskRequestId, out TaskMetadata tm, out ServiceFault error)
        {
            string taskProcId;
            if (!TryGetProcessingId(taskRequestId, out taskProcId))
            {
                string errorDetails = string.Format("TryGetUserTask: unable to find processing id for request id = {0}.", taskRequestId);
                HandleError(errorDetails, ServiceFaultCode.TaskResultsNotFound, out error);
                
                tm = null;
                return false;
            }

            if (!m_TaskScheduler.TryGetUserTask(taskProcId, out tm))
            {
                string errorDetails = "TryGetUserTask: unable to find task for " + 
                    string.Format("request id = {0}, processing id = {1}.", taskRequestId, taskProcId);
                HandleError(errorDetails, ServiceFaultCode.TaskResultsNotFound, out error);

                tm = null;
                return false;
            }

            error = null;
            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Genera un nome di file casuale con l'estensione specificata e lo concatena al percorso specificato.
        /// </summary>
        /// <param name="path">il percorso a cui concatenare il nome generato per il file</param>
        /// <param name="extension">estensione, con o senza un punto iniziale</param>
        /// <returns>il percorso del file il cui nome è stato generato casualmente</returns>
        private string GenerateRandomFileName(string path, string extension)
        {
            string filename = Path.ChangeExtension(Guid.NewGuid().ToString(), extension);
            string filepath;

            try
            {
                filepath = Path.Combine(path, filename);
            }
            catch (ArgumentException e)
            {
                HandleError(e.ToString());
                filepath = filename;
            }

            return filepath;
        }

        /// <summary>
        /// Prova ad associare tra loro l'identificativo associato alla richiesta e quello associato all'elaborazione
        /// del task e restituisce true se l'associazione ha successo, altrimenti restituisce false.
        /// </summary>
        /// <param name="taskRequestId">l'identificativo associato alla richiesta</param>
        /// <param name="taskProcId">l'identificativo associato all'elaborazione</param>
        /// <returns>true se l'associazione tra i due identificativi ha successo, altrimenti false</returns>
        private bool TryInsertIdPair(string taskRequestId, string taskProcId)
        {
            lock (m_IdTableLocker)
            {
                if (taskRequestId != null && taskProcId != null && !m_IdTable.ContainsKey(taskRequestId))
                {
                    m_IdTable.Add(taskRequestId, taskProcId);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRequestId"></param>
        /// <param name="taskProcId"></param>
        /// <returns></returns>
        private bool TryGetProcessingId(string taskRequestId, out string taskProcId)
        {
            lock (m_IdTableLocker)
            {
                string temp;
                if (!m_IdTable.TryGetValue(taskRequestId, out temp))
                {
                    taskProcId = null;
                    return false;
                }
                taskProcId = string.Copy(temp);
            }
            return true;
        }

        #endregion

        #region Error Handling Public Methods

        /// <summary>
        /// Genera un identificativo univoco per l'errore e lo associa ai dati specificati per poterne effettuare
        /// il logging tramite il logger di questa applicazione ed imposta in uscita l'errore esplicativo.
        /// </summary>
        /// <param name="errorDetails">le informazioni che descrivono l'errore verificatosi</param>
        /// <param name="errorCode">il codice che descrive l'errore verificatosi</param>
        /// <param name="error">l'errore esplicativo contenente identificativo e codice descrittivo</param>
        public void HandleError(string errorDetails, ServiceFaultCode errorCode, out ServiceFault error)
        {
            string errorId;
            HandleError(errorDetails, out errorId);
            error = new ServiceFault() { Id = errorId, Code = errorCode };
        }

        #endregion

        #region Error Handling Private Methods

        /// <summary>
        /// Genera un identificativo univoco per l'errore e lo associa ai dati specificati per poterne effettuare
        /// il logging tramite il logger di questa applicazione.
        /// </summary>
        /// <param name="errorDetails">le informazioni che descrivono l'errore verificatosi</param>
        private void HandleError(string errorDetails)
        {
            string errorId;
            HandleError(errorDetails, out errorId);
        }

        /// <summary>
        /// Genera un identificativo univoco per l'errore e lo associa ai dati specificati per poterne effettuare
        /// il logging tramite il logger di questa applicazione ed imposta in uscita l'identificativo dell'errore.
        /// </summary>
        /// <param name="errorDetails">le informazioni che descrivono l'errore verificatosi</param>
        /// <param name="errorId">l'identificativo univoco dell'errore verificatosi</param>
        private void HandleError(string errorDetails, out string errorId)
        {
            errorId = Guid.NewGuid().ToString();
            WriteToLog("{0}{1}.{2}{3}{4}",
                "Error Id: ", errorId,
                Environment.NewLine, "Error Description: ", errorDetails);
        }

        #endregion

        #region Logging Public Methods

        /// <summary>
        /// Garantisce l'accesso alle funzionalità di logging da parte dei metodi di questa classe e del servizio,
        /// aggiungendo informazioni accessorie oltre alla descrizione specificata.
        /// </summary>
        /// <param name="format">una stringa in formato composito</param>
        /// <param name="args">n array di oggetti contenente zero o più oggetti da formattare</param>
        public void WriteToLog(string format, params object[] args)
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
