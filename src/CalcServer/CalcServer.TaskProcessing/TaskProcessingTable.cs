using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CalcServer.TaskProcessing
{
    #region FilterTaskMetadata

    /// <summary>
    /// Delegato che consente di stabilire se le condizioni sul task specificato sono verificate o meno.
    /// </summary>
    /// <param name="tm">i metadati del task da analizzare per valutare le condizioni</param>
    /// <returns>true se il task soddisfa le condizioni, altrimenti false</returns>
    /// <remarks>
    /// Poiché le istanze di TaskMetadata possono essere anche impostate a null, prima di accedere alle
    /// relative property, è bene verificare che l'istanza specificata sia diversa da null.
    /// </remarks>
    public delegate bool FilterTaskMetadata(TaskMetadata tm);

    #endregion

    #region TaskProcessingTable

    /// <summary>
    /// Rappresenta una collezione a cui è possibile aggiungere i metadati relativi a task multipli, con
    /// un'assegnazione automatica di un identificativo univoco dipendente dall'ordine di inserimento.
    /// </summary>
    /// <remarks>
    /// Questa classe non è intrinsecamente thread-safe, quindi se occorre condividere la stessa istanza
    /// tra differenti thread, è necessario utilizzare gli appositi costrutti offerti dal linguaggio per
    /// garantire la mutua esclusione: ciò garantisce anche che l'identificativo generato sia differente
    /// per ognuno dei task inseriti.
    /// Inoltre, le istanze di TaskMetadata, inserite in questa collezione tramite i metodi InsertTask o
    /// UpdateTask, possono essere anche null, quindi occorre sempre verificare che l'istanza restituita
    /// da uno dei metodi di questa collezione sia diversa da null.
    /// </remarks>
    internal sealed class TaskProcessingTable : IEnumerable<KeyValuePair<String, TaskMetadata>>
    {
        #region Fields

        private readonly Dictionary<String, TaskMetadata> m_Data = new Dictionary<String, TaskMetadata>();
        private UInt64 m_Counter = 0;
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Aggiunge un oggetto contenente i metadati di un task e lo associa ad un identificativo
        /// univoco, generato automaticamente e restituito in output al termine dell'aggiunta.
        /// </summary>
        /// <param name="tm">l'oggetto contenente i metadati del task</param>
        /// <param name="id">l'identificativo univoco associato all'oggetto inserito</param>
        public void InsertTask(TaskMetadata tm, out string id)
        {
            id = (m_Counter++).ToString();
            m_Data.Add(id, tm);
        }

        /// <summary>
        /// Rimuove i metadati del task specificato e restituisce true se quest'ultimo viene trovato
        /// e rimosso, altrimenti restituisce false se non è presente alcun oggetto associato all'id
        /// specificato oppure se viene specificato un id con valore null.
        /// </summary>
        /// <param name="id">l'identificativo univoco associato ai metadati del task da rimuovere</param>
        /// <returns>true se il task specificato esiste e viene rimosso, altrimenti false</returns>
        public bool RemoveTask(string id)
        {
            if (id != null)
            {
                return m_Data.Remove(id);
            }

            return false;
        }

        /// <summary>
        /// Rimuove tutti i task che corrispondono alle condizioni definite dal predicato specificato.
        /// </summary>
        /// <param name="match">delegato che definisce le condizioni dei task da rimuovere</param>
        /// <remarks>
        /// Si tenga presente che l'oggetto contenente i metadati di un task può essere null.
        /// </remarks>
        public void RemoveTasks(FilterTaskMetadata match)
        {
            if (match != null)
            {
                List<String> tasksToRemove = new List<String>();

                foreach (KeyValuePair<String, TaskMetadata> item in m_Data)
                {
                    String id = item.Key;
                    TaskMetadata tm = item.Value;

                    if (match(tm))
                    {
                        tasksToRemove.Add(id);
                    }
                }

                foreach (var id in tasksToRemove)
                {
                    m_Data.Remove(id);
                }
            }
        }

        /// <summary>
        /// Aggiorna i metadati del task specificato e restituisce true se quest'ultimo viene trovato
        /// e aggiornato, oppure restituisce false se, invece, non è presente alcun oggetto associato
        /// all'id specificato oppure se viene specificato un id con valore null.
        /// </summary>
        /// <param name="id">l'identificativo univoco associato ai metadati del task da aggiornare</param>
        /// <param name="tm">i metadati aggiornati</param>
        /// <returns>true se il task specificato esiste e viene aggiornato, altrimenti false</returns>
        public bool UpdateTask(string id, TaskMetadata tm)
        {
            if (id != null && m_Data.ContainsKey(id))
            {
                m_Data[id] = tm;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Prova ad ottenere l'oggetto contenente i metadati associati all'identificativo specificato.
        /// Restituisce true se tale identificativo specificato esiste ed in tal caso assegna in output
        /// l'oggetto con i metadati richiesti. Altrimenti, se l'identificativo specificato non esiste,
        /// restituisce false ed imposta l'output a null.
        /// </summary>
        /// <param name="id">l'identificativo univoco associato ai metadati del task richiesto</param>
        /// <param name="tm">l'oggetto contenente i metadati richiesti</param>
        /// <returns>true, se esiste l'oggetto con i metadati cercati, altrimenti false</returns>
        public bool TryGetTask(string id, out TaskMetadata tm)
        {
            if (id != null)
            {
                return m_Data.TryGetValue(id, out tm);
            }

            tm = null;
            return false;
        }

        #endregion

        #region IEnumerable Methods

        /// <summary>
        /// Restituisce un enumeratore che scorre l'insieme di task contenuti in questo oggetto.
        /// </summary>
        /// <returns>un enumeratore che scorre l'insieme di task contenuti in questo oggetto</returns>
        public IEnumerator<KeyValuePair<String, TaskMetadata>> GetEnumerator()
        {
            return m_Data.GetEnumerator();
        }

        /// <summary>
        /// Restituisce un enumeratore che scorre l'insieme di task contenuti in questo oggetto.
        /// </summary>
        /// <returns>un enumeratore che scorre l'insieme di task contenuti in questo oggetto</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Data.GetEnumerator();
        }

        #endregion
    }

    #endregion
}
