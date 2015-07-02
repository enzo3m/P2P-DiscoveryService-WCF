using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalcServer.Logging
{
    /// <summary>
    /// Gestisce la memorizzazione dei messaggi di log all'interno di un file di testo specificato durante
    /// la creazione di una nuova istanza di questa classe.
    /// </summary>
    public class FileLogHandler : ILogHandler
    {
        #region Fields

        private StreamWriter m_LogWriter;
        private readonly List<LogMessage> m_LogBuffer;
        private int m_LogBufferMaxSize;

        #endregion

        #region Constants

        /// <summary>
        /// Limite predefinito di messaggi di log da mantenere in memoria.
        /// </summary>
        public const int DefaultLogBufferMaxSize = 100;

        #endregion

        #region Constructors

        /// <summary>
        /// Crea un nuovo file o apre quello esistente per scrivervi i messaggi di log.
        /// </summary>
        /// <param name="path">File da aprire per la scrittura.</param>
        /// <exception cref="IOException">
        /// Si è verificato un errore di I/O durante la creazione dell'eventuale directory.
        /// </exception>
        /// <exception cref="SecurityException">
        /// Il chiamante non dispone dell'autorizzazione richiesta.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Accesso negato alla cartella di destinazione.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Il percorso è una stringa vuota, contiene solo spazi vuoti oppure uno o più caratteri non validi.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Il parametro path è null.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Il percorso, il nome file o entrambi superano la lunghezza massima definita dal sistema.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Il percorso specificato non è valido (ad esempio, si trova su un'unità non mappata).
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Il parametro path è in un formato non valido.
        /// </exception>
        /// <remarks>
        /// Se il file al percorso specificato esiste già, allora viene sovrascritto.
        /// </remarks>
        public FileLogHandler(string path)
        {
            FileInfo fi = new FileInfo(path);
            fi.Directory.Create();

            m_LogWriter = File.CreateText(path);
            m_LogBuffer = new List<LogMessage>();
            m_LogBufferMaxSize = DefaultLogBufferMaxSize;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Ottiene o imposta il numero massimo di messaggi di log da mantenere in memoria prima che vengano
        /// salvati su un supporto di memorizzazione persistente rappresentato dal file di log.
        /// </summary>
        /// <remarks>
        /// Qualora si dovesse specificare un valore non positivo, il numero massimo di messaggi di log viene
        /// impostato al valore di default, definito nella costante DefaultLogBufferMaxSize.
        /// </remarks>
        public int LogBufferMaxSize
        {
            get { return m_LogBufferMaxSize; }
            set { m_LogBufferMaxSize = (value > 0 ? value : DefaultLogBufferMaxSize); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Richiede la scrittura del messaggio specificato all'interno del file di log.
        /// </summary>
        /// <param name="message">Messaggio di log da aggiungere al file di log.</param>
        /// <exception cref="IOException">
        /// Si è verificato un errore di I/O.
        /// </exception>
        public void Write(LogMessage message)
        {
            if (message != null)
            {
                m_LogBuffer.Add(message);

                if (m_LogBuffer.Count >= m_LogBufferMaxSize)
                {
                    Flush();
                }
            }
        }

        /// <summary>
        /// Svuota tutti i buffer di questo FileLogHandler e fa si che ogni messaggio di log sia scritto su file.
        /// </summary>
        /// <exception cref="IOException">
        /// Si è verificato un errore di I/O.
        /// </exception>
        public void Flush()
        {
            try
            {
                if (m_LogWriter != null)
                {
                    foreach (var message in m_LogBuffer)
                    {
                        m_LogWriter.WriteLine();
                        m_LogWriter.WriteLine(new string('-', 100));
                        m_LogWriter.WriteLine(message);
                    }
                    m_LogWriter.Flush();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                m_LogBuffer.Clear();
            }
        }

        /// <summary>
        /// Chiude questo FileLogHandler e rilascia tutte le risorse ad esso associate.
        /// </summary>
        public void Dispose()
        {
            if (m_LogWriter != null)
            {
                m_LogWriter.Close(); // --> Dispose(true)
                m_LogWriter = null;
            }
        }

        #endregion
    }
}
