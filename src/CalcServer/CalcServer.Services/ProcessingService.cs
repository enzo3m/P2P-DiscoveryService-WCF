using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;

using CalcServer.Contracts;
using CalcServer.TaskProcessing;

namespace CalcServer.Services
{
    /// <summary>
    /// Implementazione del servizio per l'elaborazione di task.
    /// </summary>
    public class ProcessingService : IProcessingService
    {
        private readonly static ProcessingServiceBackend Backend = new ProcessingServiceBackend();

        /// <summary>
        /// Verifica se la risorsa identificata dal nome e dalla versione specificati esiste ed è abilitato per
        /// il servizio, impostando l'esito della verifica in base ad una corrispondenza esatta dei parametri
        /// specificati.
        /// </summary>
        /// <param name="name">il nome completo della classe che implementa la risorsa di elaborazione</param>
        /// <param name="version">la versione della classe che implementa la risorsa di elaborazione</param>
        /// <param name="enabled">l'esito della verifica effettuata</param>
        public void QueryForResource(string name, string version, out bool enabled)
        {
            enabled = Backend.IsResourceEnabled(name, version);

            Backend.WriteToLog("QueryForResource: name = {0}, version = {1}, enabled = {2}.", name, version, enabled);
        }

        /// <summary>
        /// Restituisce l'elenco di tutte le risorse attualmente disponibili sul servizio e abilitate, oppure
        /// un elenco vuoto se nessuna risorsa è disponibile perché disabilitate o non esistenti.
        /// </summary>
        /// <param name="resources">l'elenco delle risorse disponibili e abilitate sul servizio</param>
        public void QueryForEnabledResources(out List<String> resources)
        {
            resources = Backend.GetEnabledResources();

            Backend.WriteToLog("QueryForEnabledResources: {0}.", string.Join(", ", resources));
        }

        /// <summary>
        /// Permette il trasferimento dei dati relativi al task da elaborare ed imposta una stringa contenente
        /// l'identificativo univoco associato alla richiesta di elaborazione, necessario per ottenere in seguito
        /// i risultati dell'elaborazione.
        /// </summary>
        /// <param name="data">i dati relativi al task di cui si richiede l'elaborazione</param>
        /// <param name="id">l'identificativo associato alla richiesta di elaborazione</param>
        public void SubmitData(TaskData data, out string id)
        {
            Backend.WriteToLog("SubmitData: receiving data, name = {0}.", data.Name);
            
            ServiceFault fault = null;

            // Genera un ID univoco da associare alla richiesta.
            string taskRequestId;
            if (!Backend.TryGetRandomId(out taskRequestId, out fault))
            {
                throw new FaultException<ServiceFault>(fault);
            }

            Backend.WriteToLog("SubmitData: task request id = {0}.", taskRequestId);

            string tdFilePath = Backend.GetTaskDataFilePath();   // task data file path

            // Salva i dati ricevuti sul task da elaborare.
            if (!Backend.TrySaveDataToFile(data.Contents, tdFilePath, out fault))
            {
                throw new FaultException<ServiceFault>(fault);
            }

            Backend.WriteToLog("SubmitData: task request id = {0}, file saved to {1}.", taskRequestId, tdFilePath);

            // Verifica che la risorsa sia disponibile.
            string className, classVersion;
            if (!Backend.TrySearchResource(tdFilePath, out className, out classVersion, out fault))
            {
                throw new FaultException<ServiceFault>(fault);
            }

            string trFilePath = Backend.GetTaskResultsFilePath(); // task results file path

            // Prepara il task da elaborare.
            TaskMetadata tm = new TaskMetadata(className, classVersion, tdFilePath, trFilePath);
            tm.UpdateOnReady(DateTime.Now);

            // Inserisce il task in coda allo scheduler.
            if (!Backend.TryQueueTask(tm, taskRequestId, out fault))
            {
                throw new FaultException<ServiceFault>(fault);
            }

            Backend.WriteToLog("SubmitData: task scheduled with request id = {0}, target file = {1}.",
                taskRequestId, trFilePath);

            id = taskRequestId;
        }

        /// <summary>
        /// Permette di conoscere lo stato dell'elaborazione relativo al task specificato dall'identificativo,
        /// impostando l'informazione sullo stato al termine della chiamata a questo metodo.
        /// </summary>
        /// <param name="id">l'identificativo precedentemente associato alla richiesta di elaborazione</param>
        /// <param name="state">lo stato corrente dell'elaborazione del task</param>
        public void GetState(string id, out TaskState state)
        {
            state = Backend.GetTaskState(id);
            
            Backend.WriteToLog("GetState: task request id = {0}, state = {1}.", id, state);
        }

        /// <summary>
        /// Permette il trasferimento dei risultati relativi ad un task già elaborato e specificato dal proprio
        /// identificativo, impostando l'oggetto che dovrà contenere queste informazioni.
        /// </summary>
        /// <param name="id">l'identificativo precedentemente associato alla richiesta di elaborazione</param>
        /// <param name="results">i risultati relativi al task di cui è stata completata l'elaborazione</param>
        public void GetResults(string id, out TaskResults results)
        {
            Backend.WriteToLog("GetResults: task request id = {0}.", id);

            ServiceFault fault = null;

            // Ottiene la copia dei metadati relativi al task.
            TaskMetadata tm;
            if (!Backend.TryGetUserTask(id, out tm, out fault))
            {
                throw new FaultException<ServiceFault>(fault);
            }

            Backend.WriteToLog("GetResults: sending results from file {0}.", tm.PathToTargetFile);

            // Prepara e invia il risultato.
            try
            {
                results = new TaskResults()
                {
                    ElapsedTime = Backend.GetProcessingTime(tm),
                    EncounteredErrors = tm.Errors,
                    Contents = File.ReadAllText(tm.PathToTargetFile)
                };
            }
            catch (Exception e)
            {
                Backend.HandleError(e.ToString(), ServiceFaultCode.SendTaskResultsFailed, out fault);
                throw new FaultException<ServiceFault>(fault);
            }
        }
    }
}
