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
    /// Esempio di task performer che permette di eseguire alcuni calcoli statistici.
    /// </summary>
    public class StatisticsToolbox : ITaskPerformer
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string Name { get { return "CalcServer.Toolboxes.StatisticsToolbox"; } }

        /// <summary>
        /// 
        /// </summary>
        public string Version { get { return "1.0.0.0"; } }

        /// <summary>
        /// 
        /// </summary>
        public string Author { get { return "Vincenzo"; } }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get { return "Toolbox per i calcoli statistici."; } }

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

            StatisticsToolboxImpl.Execute(sourceData, ref targetData);

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
