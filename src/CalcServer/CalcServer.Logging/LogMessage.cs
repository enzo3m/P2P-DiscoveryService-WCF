using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalcServer.Logging
{
    /// <summary>
    /// Definisce un messaggio di log, ovvero la tipologia di informazioni che possono essere trasferite
    /// ad un ILogHandler, affinché possano essere scritte su un supporto di memorizzazione persistente.
    /// </summary>
    public class LogMessage
    {
        /// <summary>
        /// Ottiene o imposta l'istante di tempo associato al messaggio di log.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Ottiene o imposta il nome del modulo a cui il messaggio di log si riferisce.
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// Ottiene o imposta il testo del messaggio di log.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Restituisce una rappresentazione in formato stringa di questo oggetto.
        /// </summary>
        /// <returns>una rappresentazione in formato stringa di questo oggetto</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(string.Format("{0:r}", Time)); // RFC1123
            str.Append(Environment.NewLine);
            str.Append("Module: " + Module);
            str.Append(Environment.NewLine);
            str.Append(Text);
            return str.ToString();
        }
    }
}
