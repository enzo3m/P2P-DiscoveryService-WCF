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
    /// Questa classe interna contiene alcuni metodi che implementano le funzionalità statistiche
    /// rese disponibili mediante la classe StatisticsToolbox.
    /// </summary>
    internal class StatisticsToolboxImpl
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

            if (functionName == "WordsCount")
            {
                string dataToProcess = root.SelectSingleNode("/task/data").InnerText;
                
                List<WordCount> result = CalculateWordsCounts(dataToProcess);
                Serialize(result, ref targetData);
            }
            else if (functionName == "MeanStdDev")
            {
                string dataToProcess = root.SelectSingleNode("/task/data").InnerText;

                MeanStdDevResult results = CalculateMeanStdDev(dataToProcess);
                Serialize(results, ref targetData);
            }
            else
            {
                throw new TaskProcessingException(string.Format("Funzione non trovata: {0}.", functionName));
            }
        }

        #region WordsCount Private Methods

        /// <summary>
        /// Calcola la frequenza di ogni parola all'interno del testo specificato
        /// e restituisce la lista delle parole trovate con la relativa frequenza.
        /// </summary>
        /// <param name="text">testo da analizzare</param>
        /// <returns>la lista delle parole trovate con la relativa frequenza</returns>
        private static List<WordCount> CalculateWordsCounts(String text)
        {
            Dictionary<string, int> table = new Dictionary<string, int>();

            string[] words = text.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                int count;
                if (table.TryGetValue(word, out count))
                {
                    count++;
                }
                else
                {
                    count = 1;
                }
                table[word] = count;
            }

            var items = from item in table
            orderby item.Value descending,
                    item.Key ascending
            select item;

            List<WordCount> result = new List<WordCount>();

            foreach (var item in items)
            {
                WordCount entry = new WordCount() { Word = item.Key, Count = item.Value };
                result.Add(entry);
            }

            return result;
        }

        /// <summary>
        /// Permette di scrivere i risultati specificati all'interno del documento xml specificato.
        /// </summary>
        /// <param name="list">la lista dei risultati da scrivere</param>
        /// <param name="document">documento xml di destinazione</param>
        private static void Serialize(List<WordCount> list, ref XmlDocument document)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<WordCount>));
            using (XmlWriter writer = document.CreateNavigator().AppendChild())
            {
                serializer.Serialize(writer, list);
            }
        }

        #endregion

        #region MeanStdDev Private Methods

        /// <summary>
        /// Calcola il valore medio e la deviazione standard relativi all'insieme di dati specificato.
        /// </summary>
        /// <param name="dataToProcess">stringa contenente i dati da analizzare</param>
        /// <returns>il valore medio e la deviazione standard</returns>
        private static MeanStdDevResult CalculateMeanStdDev(string dataToProcess)
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

            string meanValue = string.Empty;
            string stddevValue = string.Empty;

            if (valueList.Count > 0)
            {
                double mean = 0.0;
                double stddev = 0.0;

                double sum = 0.0;
                double sumOfSquares = 0.0;
                double count = valueList.Count;

                foreach (double value in valueList)
                {
                    sum += value;
                    sumOfSquares += (value * value);
                }

                mean = sum / count;
                stddev = Math.Sqrt(sumOfSquares / count - mean * mean);

                meanValue = mean.ToString(CultureInfo.InvariantCulture);
                stddevValue = stddev.ToString(CultureInfo.InvariantCulture);
            }
            
            return new MeanStdDevResult()
            {
                Mean = meanValue,
                StdDev = stddevValue,
                ExclusionList = exclusionList
            };
        }

        /// <summary>
        /// Permette di scrivere il risultato specificato all'interno del documento xml specificato.
        /// </summary>
        /// <param name="result">risultato da scrivere</param>
        /// <param name="document">documento xml di destinazione</param>
        private static void Serialize(MeanStdDevResult results, ref XmlDocument document)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MeanStdDevResult));
            using (XmlWriter writer = document.CreateNavigator().AppendChild())
            {
                serializer.Serialize(writer, results);
            }
        }

        #endregion
    }
}
