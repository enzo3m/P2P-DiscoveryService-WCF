using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CalcServer.Contracts
{
    /// <summary>
    /// Interfaccia del servizio per l'elaborazione di task.
    /// </summary>
    [ServiceContract]
    public interface IProcessingService
    {
        /// <summary>
        /// Verifica se la risorsa identificata dal nome e dalla versione specificati esiste ed è abilitato per
        /// il servizio, impostando l'esito della verifica in base ad una corrispondenza esatta dei parametri
        /// specificati.
        /// </summary>
        /// <param name="name">il nome completo della classe che implementa la risorsa di elaborazione</param>
        /// <param name="version">la versione della classe che implementa la risorsa di elaborazione</param>
        /// <param name="enabled">l'esito della verifica effettuata</param>
        [OperationContract]
        void QueryForResource(string name, string version, out bool enabled);

        /// <summary>
        /// Restituisce l'elenco di tutte le risorse attualmente disponibili sul servizio e abilitate, oppure
        /// un elenco vuoto se nessuna risorsa è disponibile perché disabilitate o non esistenti.
        /// </summary>
        /// <param name="resources">l'elenco delle risorse disponibili e abilitate sul servizio</param>
        [OperationContract]
        void QueryForEnabledResources(out List<String> resources);

        /// <summary>
        /// Permette il trasferimento dei dati relativi al task da elaborare ed imposta una stringa contenente
        /// l'identificativo univoco associato alla richiesta di elaborazione, necessario per ottenere in seguito
        /// i risultati dell'elaborazione.
        /// </summary>
        /// <param name="data">i dati relativi al task di cui si richiede l'elaborazione</param>
        /// <param name="id">l'identificativo associato alla richiesta di elaborazione</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubmitData(TaskData data, out string id);

        /// <summary>
        /// Permette di conoscere lo stato dell'elaborazione relativo al task specificato dall'identificativo,
        /// impostando l'informazione sullo stato al termine della chiamata a questo metodo.
        /// </summary>
        /// <param name="id">l'identificativo precedentemente associato alla richiesta di elaborazione</param>
        /// <param name="state">lo stato corrente dell'elaborazione del task</param>
        [OperationContract]
        void GetState(string id, out TaskState state);

        /// <summary>
        /// Permette il trasferimento dei risultati relativi ad un task già elaborato e specificato dal proprio
        /// identificativo, impostando l'oggetto che dovrà contenere queste informazioni.
        /// </summary>
        /// <param name="id">l'identificativo precedentemente associato alla richiesta di elaborazione</param>
        /// <param name="results">i risultati relativi al task di cui è stata completata l'elaborazione</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void GetResults(string id, out TaskResults results);
    }
}
