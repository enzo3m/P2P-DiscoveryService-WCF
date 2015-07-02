using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalcServer.Logging
{
    /// <summary>
    /// Rappresenta l'interfaccia che deve essere implementata per assicurare la memorizzazione
    /// dei messaggi di log su un supporto di memorizzazione persistente.
    /// </summary>
    public interface ILogHandler : IDisposable
    {
        /// <summary>
        /// Questo metodo deve consentire il trasferimento del messaggio specificato all'interno
        /// di questo ILogHandler, tuttavia non è richiesto che venga necessariamente scritto su
        /// un supporto di memorizzazione persistente: infatti, la sua scrittura potrebbe essere
        /// posticipata per ridurre il numero di accessi al predetto supporto.
        /// </summary>
        /// <param name="message">il messaggio di log da trasferire a questo ILogHandler</param>
        /// <remarks>
        /// L'implementazione sottostante potrebbe lanciare delle eccezioni dipendenti dal tipo
        /// di oggetti utilizzati e dai relativi metodi invocati.
        /// </remarks>
        void Write(LogMessage message);

        /// <summary>
        /// Questo metodo deve consentire di forzare la scrittura di tutti quei messaggi di log
        /// che potrebbero non essere stati scritti sul supporto di memorizzazione persistente.
        /// </summary>
        /// <remarks>
        /// L'implementazione sottostante potrebbe lanciare delle eccezioni dipendenti dal tipo
        /// di oggetti utilizzati e dai relativi metodi invocati.
        /// </remarks>
        void Flush();
    }
}
