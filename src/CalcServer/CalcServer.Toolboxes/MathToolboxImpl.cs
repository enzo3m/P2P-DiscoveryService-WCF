using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;

using CalcServer.TaskProcessing;

namespace CalcServer.Toolboxes
{
    /// <summary>
    /// Questa classe interna contiene alcuni metodi che implementano le funzionalità matematiche
    /// rese disponibili mediante la classe MathToolbox.
    /// </summary>
    internal class MathToolboxImpl
    {
        /// <summary>
        /// Esegue il task i cui dati sono accessibili dal documento xml di origine specificato
        /// e salva i risultati ottenuti dall'esecuzione del task sul documento xml specificato
        /// come destinazione.
        /// </summary>
        /// <param name="sourceData">documento xml da cui leggere i dati del task da eseguire</param>
        /// <param name="targetData">documento xml su cui scrivere i risultati del task eseguito</param>
        internal static void Execute(XmlDocument sourceData, ref XmlDocument targetData)
        {
            XmlElement root = sourceData.DocumentElement;

            XmlNodeList nodeList = root.SelectNodes("/task/function[@functionName]/@functionName");
            string functionName = (nodeList.Count == 1 ? nodeList.Item(0).Value : string.Empty);

            if (functionName == "MinMax")
            {
                string dataToProcess = root.SelectSingleNode("/task/data").InnerText;

                MinMaxResult result = CalculateMinMax(dataToProcess);
                Serialize(result, ref targetData);
            }
            else if (functionName == "MinMaxIdx")
            {
                string dataToProcess = root.SelectSingleNode("/task/data").InnerText;

                MinMaxIdxResult result = CalculateMinMaxIdx(dataToProcess);
                Serialize(result, ref targetData);
            }
            else
            {
                throw new TaskProcessingException(string.Format("Funzione non trovata: {0}.", functionName));
            }
        }

        #region MinMax Private Methods

        /// <summary>
        /// Calcola il valore minimo e il valore massimo relativi all'insieme di dati specificato.
        /// </summary>
        /// <param name="dataToProcess">stringa contenente i dati da analizzare</param>
        /// <returns>il valore minimo e il valore massimo</returns>
        private static MinMaxResult CalculateMinMax(string dataToProcess)
        {
            string[] data = dataToProcess.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            List<Double> valueList = new List<Double>();
            List<IndexValuePair> exclusionList = new List<IndexValuePair>();

            for (int i = 0; i < data.Length; i++)
            {
                string valueStr = data[i];
                double value;
                if (double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    valueList.Add(value);
                }
                else
                {
                    exclusionList.Add(new IndexValuePair() { Index = i, Data = valueStr });
                }
            }

            string minValue = string.Empty;
            string maxValue = string.Empty;

            if (valueList.Count > 0)
            {
                minValue = valueList.Min().ToString(CultureInfo.InvariantCulture);
                maxValue = valueList.Max().ToString(CultureInfo.InvariantCulture);
            }

            return new MinMaxResult()
            {
                MinValue = minValue,
                MaxValue = maxValue,
                ExclusionList = exclusionList
            };
        }

        /// <summary>
        /// Permette di scrivere il risultato specificato all'interno del documento xml specificato.
        /// </summary>
        /// <param name="result">risultato da scrivere</param>
        /// <param name="document">documento xml di destinazione</param>
        private static void Serialize(MinMaxResult result, ref XmlDocument document)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MinMaxResult));
            using (XmlWriter writer = document.CreateNavigator().AppendChild())
            {
                serializer.Serialize(writer, result);
            }
        }

        #endregion

        #region MinMaxIdx Private Methods

        /// <summary>
        /// Calcola il valore minimo e il valore massimo relativi all'insieme di dati specificato
        /// e restituisce anche gli indici di dove si trovano.
        /// </summary>
        /// <param name="dataToProcess">stringa contenente i dati da analizzare</param>
        /// <returns>il valore minimo, il valore massimo e i relativi indici</returns>
        private static MinMaxIdxResult CalculateMinMaxIdx(string dataToProcess)
        {
            string[] data = dataToProcess.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            List<Double> valueList = new List<Double>();
            List<IndexValuePair> exclusionList = new List<IndexValuePair>();

            for (int i = 0; i < data.Length; i++)
            {
                string valueStr = data[i];
                double value;
                if (double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    valueList.Add(value);
                }
                else
                {
                    exclusionList.Add(new IndexValuePair() { Index = i, Data = valueStr });
                }
            }

            string minValue = string.Empty;
            string maxValue = string.Empty;
            string minValueIndex = string.Empty;
            string maxValueIndex = string.Empty;

            if (valueList.Count > 0)
            {
                minValue = valueList.Min().ToString(CultureInfo.InvariantCulture);
                maxValue = valueList.Max().ToString(CultureInfo.InvariantCulture);
                minValueIndex = valueList.IndexOf(valueList.Min()).ToString(CultureInfo.InvariantCulture);
                maxValueIndex = valueList.IndexOf(valueList.Max()).ToString(CultureInfo.InvariantCulture);
            }

            return new MinMaxIdxResult()
            {
                MinValue = minValue,
                MinValueIndex = minValueIndex,
                MaxValue = maxValue,
                MaxValueIndex = maxValueIndex,
                ExclusionList = exclusionList
            };
        }

        /// <summary>
        /// Permette di scrivere il risultato specificato all'interno del documento xml specificato.
        /// </summary>
        /// <param name="result">risultato da scrivere</param>
        /// <param name="document">documento xml di destinazione</param>
        private static void Serialize(MinMaxIdxResult result, ref XmlDocument document)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MinMaxIdxResult));
            using (XmlWriter writer = document.CreateNavigator().AppendChild())
            {
                serializer.Serialize(writer, result);
            }
        }

        #endregion

    }
}
