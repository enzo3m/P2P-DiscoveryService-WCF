using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace CalcServer.TaskProcessing
{
    #region ITaskPerformerContextProvider

    /// <summary>
    /// Interfaccia di sola lettura per accedere ad un insieme di contesti di elaborazione, ognuno dei quali è associato
    /// al nome completo ed alla versione della classe che lo implementa.
    /// </summary>
    public interface ITaskPerformerContextProvider
    {
        /// <summary>
        /// L'implementazione di questo metodo deve provare ad ottenere il contesto di elaborazione associato alla classe
        /// specificata, restituendo true se tale classe esiste e pertanto è possibile eseguire il task, altrimenti false
        /// se non esiste e quindi non è possibile eseguire task.
        /// </summary>
        /// <param name="className">nome completo della classe di cui si vuole ottenere il contesto di elaborazione</param>
        /// <param name="classVersion">versione della classe di cui si vuole ottenere il contesto di elaborazione</param>
        /// <param name="context">
        /// Quando termina, questo metodo deve restituire il contesto di elaborazione associato alla classe specificata
        /// nel caso in cui la classe venga trovata; in caso contrario deve restituire il contesto predefinito, che non
        /// esegue alcuna elaborazione.
        /// </param>
        /// <returns>
        /// true, se l'oggetto che implementa l'interfaccia ITaskPerformerContextProvider contiene il contesto cercato per
        /// l'elaborazione e specificato dal nome completo e dalla versione della classe, altrimenti false
        /// </returns>
        bool TryGetContext(string className, string classVersion, out TaskPerformerContext context);
    }

    #endregion

    #region TaskPerformerContextManager

    /// <summary>
    /// Fornisce un accesso centralizzato per la gestione di un insieme di task performer, con lo scopo
    /// di poterli utilizzare in modo semplice specificando il nome completo e la versione della classe
    /// che implementa le funzionalità richieste.
    /// </summary>
    /// <remarks>
    /// Questa classe non è intrinsecamente thread-safe, quindi se occorre condividere la stessa istanza
    /// tra differenti thread, è necessario utilizzare gli appositi costrutti offerti dal linguaggio per
    /// garantire la mutua esclusione.
    /// </remarks>
    public class TaskPerformerContextManager : ITaskPerformerContextProvider
    {
        #region Fields

        private readonly Dictionary<String, TaskPerformerContext> m_TaskPerformers;
        private readonly TaskPerformerContext m_NoneTaskPerformerContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Inizializza un nuovo oggetto della classe TaskPerformerContextManager vuoto.
        /// </summary>
        public TaskPerformerContextManager()
        {
            m_TaskPerformers = new Dictionary<String, TaskPerformerContext>();
            m_NoneTaskPerformerContext = new TaskPerformerContext(new NoneTaskPerformer());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Aggiunge l'istanza specificata di ITaskPerformer a questo provider, restituendo true
        /// se l'operazione viene completata con successo, oppure false se in questo provider ne
        /// è già presente una avente stesso nome e stessa versione, o se quella specificata non
        /// è valida. Per poter essere considerata valida, l'istanza specificata non deve essere
        /// null e deve contenere un nome e una versione che non siano vuoti.
        /// </summary>
        /// <param name="classInstance">istanza di ITaskPerformer da aggiungere</param>
        /// <returns>true, se l'istanza è stata aggiunta al provider, false altrimenti</returns>
        public bool Add(ITaskPerformer classInstance)
        {
            bool isValidClass = true;
            isValidClass = isValidClass && classInstance != null;
            isValidClass = isValidClass && !string.IsNullOrEmpty(classInstance.Name);
            isValidClass = isValidClass && !string.IsNullOrEmpty(classInstance.Version);

            if (isValidClass)
            {
                String classId = GetClassIdentifier(classInstance.Name, classInstance.Version);
                if (!m_TaskPerformers.ContainsKey(classId))
                {
                    TaskPerformerContext context = new TaskPerformerContext(classInstance);
                    m_TaskPerformers.Add(classId, context);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Rimuove da questo provider l'istanza di ITaskPerformer caratterizzata da nome e versione
        /// specificati, restituendo true se l'operazione viene completata correttamente, o false se
        /// il nome e la versione specificati non identificano un'istanza di questo provider, oppure
        /// se sono vuoti o null.
        /// </summary>
        /// <param name="className">nome completo della classe da rimuovere</param>
        /// <param name="classVersion">versione della classe da rimuovere</param>
        /// <returns>true, se l'istanza è stata rimossa dal provider</returns>
        public bool Remove(string className, string classVersion)
        {
            bool isValidClass = true;
            isValidClass = isValidClass && !string.IsNullOrEmpty(className);
            isValidClass = isValidClass && !string.IsNullOrEmpty(classVersion);

            if (isValidClass)
            {
                String classId = GetClassIdentifier(className, classVersion);
                return m_TaskPerformers.Remove(classId);
            }

            return false;
        }

        /// <summary>
        /// Prova ad ottenere il contesto di elaborazione relativo alla classe specificata. Restituisce true
        /// se tale classe esiste e quindi è possibile eseguire il task, oppure false se non esiste e quindi
        /// non è possibile eseguire task.
        /// </summary>
        /// <param name="className">nome completo della classe da utilizzare per l'elaborazione del task</param>
        /// <param name="classVersion">versione della classe da utilizzare per l'elaborazione del task</param>
        /// <param name="context">
        /// Quando termina, questo metodo restituisce il contesto di elaborazione associato alla classe specificata
        /// nel caso in cui la classe venga trovata; in caso contrario restituisce il contesto predefinito, che non
        /// esegue alcuna elaborazione.
        /// </param>
        /// <returns>true, se esiste il contesto cercato per l'elaborazione, altrimenti false</returns>
        public bool TryGetContext(string className, string classVersion, out TaskPerformerContext context)
        {
            if (className != null && classVersion != null)
            {
                string classId = GetClassIdentifier(className, classVersion);

                if (classId.Length > 0 && m_TaskPerformers.TryGetValue(classId, out context))
                {
                    return true;
                }
            }

            context = m_NoneTaskPerformerContext;
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Restituisce l'identificatore univoco della classe specificata, in base al suo nome completo
        /// ed alla relativa versione.
        /// </summary>
        /// <param name="name">nome completo della classe</param>
        /// <param name="version">versione della classe</param>
        /// <returns>l'identificatore univoco della classe specificata</returns>
        private string GetClassIdentifier(string name, string version)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }
            else if (string.IsNullOrEmpty(version) || string.IsNullOrWhiteSpace(version))
            {
                return name;
            }
            else
            {
                return string.Format("{0}-{1}", name, version);
            }
        }

        #endregion
    }

    #endregion

    #region NoneTaskPerformer

    /// <summary>
    /// Implementa il comportamento nullo nell'esecuzione di un task: apre il file specificato
    /// come sorgente e lo chiude senza leggerlo, ne crea uno vuoto come destinazione e quindi
    /// non esegue alcuna operazione.
    /// </summary>
    internal class NoneTaskPerformer : ITaskPerformer
    {
        #region Properties

        /// <summary>
        /// Restituisce una stringa vuota, poiché non implementa nessuna funzionalità.
        /// </summary>
        public string Name { get { return string.Empty; } }

        /// <summary>
        /// Restituisce una stringa vuota, poiché non implementa nessuna funzionalità.
        /// </summary>
        public string Version { get { return string.Empty; } }

        /// <summary>
        /// Restituisce una stringa vuota, poiché non implementa nessuna funzionalità.
        /// </summary>
        public string Author { get { return string.Empty; } }

        /// <summary>
        /// Restituisce una stringa vuota, poiché non implementa nessuna funzionalità.
        /// </summary>
        public string Description { get { return string.Empty; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Questo metodo non esegue alcuna elaborazione sullo stream sorgente e non scrive nessun dato
        /// sullo stream di destinazione, quindi il file associato a quest'ultimo risulterà vuoto.
        /// </summary>
        /// <param name="sourceStream">stream da cui leggere i dati del task da eseguire</param>
        /// <param name="targetStream">stream su cui scrivere i risultati del task eseguito</param>
        public void Execute(Stream sourceStream, Stream targetStream)
        {
            ;   // implementazione assente perché non esegue alcuna elaborazione
        }

        #endregion
    }

    #endregion
}
