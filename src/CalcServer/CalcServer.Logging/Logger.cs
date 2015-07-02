using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalcServer.Logging
{
    /// <summary>
    /// Assicura un accesso centralizzato alle funzionalità di logging, fornendo un'istanza thread-safe
    /// della classe Logger e unica nell'applicazione, grazie all'uso del pattern singleton in versione
    /// thread-safe.
    /// </summary>
    public sealed class Logger
    {
        #region Singleton

        private static readonly Logger m_Instance = new Logger();

        /// <summary>
        /// Costruttore privato non accessibile.
        /// </summary>
        private Logger() { ; }

        /// <summary>
        /// Ottiene il riferimento all'unica istanza di questo EventLogger.
        /// </summary>
        public static Logger Instance { get { return m_Instance; } }

        #endregion

        #region Fields

        private readonly object m_SyncLock = new object();
        private readonly List<ILogHandler> m_LogHandlers = new List<ILogHandler>();
        private readonly ISet<Exception> m_LogErrors = new HashSet<Exception>();

        #endregion

        #region Properties

        /// <summary>
        /// Ottiene l'insieme degli errori verificatisi durante il logging dei messaggi.
        /// </summary>
        public IList<Exception> Errors
        {
            get
            {
                List<Exception> list = new List<Exception>(m_LogErrors);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Ottiene il numero di errori verificatisi durante il logging dei messaggi.
        /// </summary>
        public int ErrorsCount { get { return m_LogErrors.Count; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Aggiunge un gestore dei messaggi di log e restituisce true se l'oggetto specificato è stato aggiunto,
        /// altrimenti false se è già presente oppure è null e quindi non è stato aggiunto.
        /// </summary>
        /// <param name="handler">il gestore da aggiungere</param>
        /// <returns>true se l'oggetto specificato è stato aggiunto, altrimenti false se è null o già presente</returns>
        public bool Add(ILogHandler handler)
        {
            lock (m_SyncLock)
            {
                if (handler != null && !m_LogHandlers.Contains(handler))
                {
                    m_LogHandlers.Add(handler);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Rimuove un gestore dei messaggi di log e restituisce true se l'oggetto specificato è stato rimosso,
        /// altrimenti false se non è presente oppure è null e quindi non è stato possibile rimuoverlo.
        /// </summary>
        /// <param name="handler">il gestore da rimuovere</param>
        /// <returns>true se l'oggetto specificato è stato rimosso, altrimenti false se non è presente</returns>
        public bool Remove(ILogHandler handler)
        {
            lock (m_SyncLock)
            {
                return (handler != null ? m_LogHandlers.Remove(handler) : false);
            }
        }

        /// <summary>
        /// Trasferisce i dati specificati a tutti gli ILogHangler registrati con questo Logger.
        /// </summary>
        /// <param name="time">l'istante da associare al messaggio di log</param>
        /// <param name="module">il nome del modulo che ha invocato questo metodo</param>
        /// <param name="text">il testo del messaggio di log</param>
        /// <remarks>
        /// I dati specificati non vengono scritti immediatamente sui supporti di memorizzazione persistente
        /// degli ILogHandler registrati con questo Logger: la scrittura potrebbe essere posticipata al fine
        /// di ridurre l'impatto prestazionale dovuto ad eventuali supporti più lenti.
        /// </remarks>
        public void Write(DateTime time, string module, string text)
        {
            LogMessage message = new LogMessage()
            {
                Time = time,
                Module = module,
                Text = text
            };

            lock (m_SyncLock)
            {
                foreach (var handler in m_LogHandlers)
                {
                    if (handler != null)
                    {
                        try { handler.Write(message); }
                        catch (Exception e) { m_LogErrors.Add(e); }
                    }
                }
            }
        }

        /// <summary>
        /// Forza tutti gli ILogHandler registrati con questo Logger a procedere con la scrittura immediata
        /// dei messaggi di log che non sono tuttora stati scritti sui relativi supporti di memorizzazione
        /// persistente.
        /// </summary>
        public void Flush()
        {
            lock (m_SyncLock)
            {
                foreach (var handler in m_LogHandlers)
                {
                    if (handler != null)
                    {
                        try { handler.Flush(); }
                        catch (Exception e) { m_LogErrors.Add(e); }
                    }
                }
            }
        }

        /// <summary>
        /// Invoca il metodo Dispose di tutti gli ILogHandler registrati con questo Logger affinché possano
        /// rilasciare le risorse associate alla loro attività.
        /// </summary>
        public void Cleanup()
        {
            lock (m_SyncLock)
            {
                foreach (var handler in m_LogHandlers)
                {
                    if (handler != null)
                    {
                        handler.Dispose();
                    }
                }
            }
        }

        #endregion
    }
}
