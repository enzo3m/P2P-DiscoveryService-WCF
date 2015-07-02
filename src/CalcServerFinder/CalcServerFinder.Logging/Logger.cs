using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CalcServerFinder.Logging
{
    /// <summary>
    /// Rappresenta un semplice modulo di logging che visualizza i messaggi di log sulla console.
    /// La scrittura di ogni singolo messaggio di log è thread-safe e la classe in questioneè stata sviluppata
    /// seguendo il pattern singleton.
    /// </summary>
    public class Logger
    {
        #region Singleton

        private static readonly Logger m_Instance = new Logger();

        /// <summary>
        /// Ottiene l'istanza thread-safe di Logger unica nell'applicazione.
        /// </summary>
        public static Logger Instance { get { return m_Instance; } }

        /// <summary>
        /// Il costruttore privato evita che possano essere create ulteriori istanze.
        /// </summary>
        private Logger() { ; }

        #endregion

        #region Members

        private static readonly object m_SyncLock = new object();

        #endregion

        #region Public Methods

        /// <summary>
        /// Formatta e visualizza i dati specificati nella console di esecuzione dell'applicazione.
        /// </summary>
        /// <param name="time">l'istante da associare al messaggio di log</param>
        /// <param name="module">il nome del modulo che ha invocato questo metodo</param>
        /// <param name="text">il testo del messaggio di log</param>
        public void Write(DateTime time, string module, string text)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine();
            message.AppendLine("===");
            message.AppendFormat("{0:r}", time);
            message.AppendLine();
            message.AppendFormat("Module: {0}", module);
            message.AppendLine();
            message.AppendLine("===");
            message.AppendLine(text);
            message.AppendLine("---");

            Task.Factory.StartNew(() => Console.WriteLine(message),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #endregion
    }
}
