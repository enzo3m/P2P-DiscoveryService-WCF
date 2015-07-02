using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CalcServer.Toolboxes
{
    #region IndexValuePair Class

    /// <summary>
    /// Una coppia di valori, di cui uno rappresenta il dato e l'altro il relativo indice.
    /// </summary>
    [XmlRoot]
    public class IndexValuePair
    {
        /// <summary>
        /// L'indice in cui si trova il dato.
        /// </summary>
        [XmlAttribute]
        public int Index { get; set; }

        /// <summary>
        /// La stringa contenente il dato.
        /// </summary>
        [XmlAttribute]
        public string Data { get; set; }
    }

    #endregion

    #region WordCount Class

    /// <summary>
    /// La frequenza di una parola trovata in un testo.
    /// </summary>
    [XmlRoot]
    public class WordCount
    {
        /// <summary>
        /// La parola trovata.
        /// </summary>
        [XmlAttribute("Word")]
        public string Word { get; set; }

        /// <summary>
        /// Il numero di occorrenze della parola.
        /// </summary>
        [XmlAttribute("Count")]
        public int Count { get; set; }

        /// <summary>
        /// La rapprestazione come stringa di questo oggetto.
        /// </summary>
        /// <returns>la rapprestazione come stringa di questo oggetto</returns>
        public override string ToString()
        {
            return string.Format("[{0},{1}]", Word, Count);
        }
    }

    #endregion

    #region MeanStdDevResult Class

    /// <summary>
    /// Il risultato di un'operazione di calcolo di media e deviazione standard.
    /// </summary>
    [XmlRoot]
    public class MeanStdDevResult
    {
        /// <summary>
        /// La stringa contenente il valore medio calcolato.
        /// </summary>
        [XmlElement]
        public string Mean { get; set; }

        /// <summary>
        /// La stringa contenente la deviazione standard calcolata.
        /// </summary>
        [XmlElement]
        public string StdDev { get; set; }

        /// <summary>
        /// La lista dei dati esclusi dal calcolo perché non validi.
        /// </summary>
        [XmlElement(ElementName = "InputDataError", Type = typeof(List<IndexValuePair>))]
        public List<IndexValuePair> ExclusionList { get; set; }
    }

    #endregion

    #region MinMaxResult Class

    /// <summary>
    /// Il risultato di un'operazione di calcolo del massimo e del minimo di un insieme di valori.
    /// </summary>
    [XmlRoot]
    public class MinMaxResult
    {
        /// <summary>
        /// Stringa contenente il valore minimo calcolato.
        /// </summary>
        [XmlElement]
        public string MinValue { get; set; }

        /// <summary>
        /// Stringa contenente il valore massimo calcolato.
        /// </summary>
        [XmlElement]
        public string MaxValue { get; set; }

        /// <summary>
        /// La lista dei dati esclusi dal calcolo perché non validi.
        /// </summary>
        [XmlElement(ElementName = "InputDataError", Type = typeof(List<IndexValuePair>))]
        public List<IndexValuePair> ExclusionList { get; set; }
    }

    #endregion

    #region MinMaxIdxResult Class

    /// <summary>
    /// Il risultato di un'operazione di calcolo del massimo e del minimo di un insieme di valori
    /// e dei relativi indici all'interno dell'insieme di dati analizzato.
    /// </summary>
    [XmlRoot]
    public class MinMaxIdxResult
    {
        /// <summary>
        /// Stringa contenente il valore minimo calcolato.
        /// </summary>
        [XmlElement]
        public string MinValue { get; set; }

        /// <summary>
        /// Stringa contenente l'indice del valore minimo calcolato.
        /// </summary>
        [XmlElement]
        public string MinValueIndex { get; set; }

        /// <summary>
        /// Stringa contenente il valore massimo calcolato.
        /// </summary>
        [XmlElement]
        public string MaxValue { get; set; }

        /// <summary>
        /// Stringa contenente l'indice del valore massimo calcolato.
        /// </summary>
        [XmlElement]
        public string MaxValueIndex { get; set; }

        /// <summary>
        /// La lista dei dati esclusi dal calcolo perché non validi.
        /// </summary>
        [XmlElement(ElementName = "InputDataError", Type = typeof(List<IndexValuePair>))]
        public List<IndexValuePair> ExclusionList { get; set; }
    }

    #endregion
}
