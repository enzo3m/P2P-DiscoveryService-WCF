using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using CalcServer.TaskProcessing;

namespace CalcServer.Toolboxes
{
    /// <summary>
    /// Esempio di task performer che permette di eseguire alcuni calcoli matematici.
    /// </summary>
    public class MathToolbox : ITaskPerformer
    {
        #region Properties

        /// <summary>
        /// Restituisce il nome completo della classe che implementa questo task performer.
        /// </summary>
        public string Name { get { return "CalcServer.Toolboxes.MathToolbox"; } }

        /// <summary>
        /// Restituisce la versione della classe che implementa questo task performer.
        /// </summary>
        public string Version { get { return "1.0.0.0"; } }

        /// <summary>
        /// Restituisce l'autore della classe che implementa questo task performer.
        /// </summary>
        public string Author { get { return "Vincenzo"; } }

        /// <summary>
        /// Restituisce la descrizione della classe che implementa questo task performer.
        /// </summary>
        public string Description { get { return "Toolbox per i calcoli matematici."; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Esegue il task i cui dati sono accessibili dallo stream di origine specificato
        /// e salva i risultati ottenuti dall'esecuzione del task sullo stream specificato
        /// come destinazione.
        /// </summary>
        /// <param name="sourceStream">stream da cui leggere i dati del task da eseguire</param>
        /// <param name="targetStream">stream su cui scrivere i risultati del task eseguito</param>
        public void Execute(Stream sourceStream, Stream targetStream)
        {
            XmlDocument sourceData = new XmlDocument();
            XmlDocument targetData = new XmlDocument();

            try
            {
                sourceData.Load(sourceStream);
            }
            catch (XmlException e)
            {
                throw new TaskDataReadException("Il caricamento o il parsing dei dati ha restituito un errore.", e);
            }

            MathToolboxImpl.Execute(sourceData, ref targetData);

            try
            {
                targetData.Save(targetStream);
            }
            catch (XmlException e)
            {
                throw new TaskResultWriteException("Il risultato dell'elaborazione ha un formato non corretto.", e);
            }
        }

        #endregion
    }
}
