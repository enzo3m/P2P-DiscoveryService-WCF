using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalcServer.TaskProcessing
{
    /// <summary>
    /// Un'istanza di questa classe ha il compito di gestire l'apertura e la chiusura dello stream sorgente
    /// e dello stream di destinazione associati ai relativi file, al fine di permetterne l'elaborazione da
    /// parte di un oggetto che implementa l'interfaccia ITaskPerformer, sollevando così quest'ultimo dalla
    /// necessità di dover gestire le suddette operazioni.
    /// </summary>
    public class TaskPerformerContext
    {
        #region Fields

        private ITaskPerformer m_TaskPerformer;

        #endregion

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di TaskPerformerContext, associandovi l'istanza specificata di ITaskPerformer.
        /// </summary>
        /// <param name="performer">l'istanza di ITaskPerformer da associare a questo nuovo oggetto</param>
        /// <exception cref="ArgumentNullException">performer è null</exception>
        public TaskPerformerContext(ITaskPerformer performer)
        {
            if (performer == null) { throw new ArgumentNullException("performer"); }
            m_TaskPerformer = performer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Il nome completo della classe che implementa il task performer.
        /// </summary>
        /// <remarks>
        /// Il nome completo della classe che implementa il task performer viene ricavata dall'istanza
        /// di ITaskPerformer associata a questo oggetto in fase di inizializzazione.
        /// </remarks>
        string Name { get { return m_TaskPerformer.Name; } }

        /// <summary>
        /// La versione della classe che implementa il task performer.
        /// </summary>
        /// <remarks>
        /// La versione della classe che implementa il task performer viene ricavata dall'istanza
        /// di ITaskPerformer associata a questo oggetto in fase di inizializzazione.
        /// </remarks>
        string Version { get { return m_TaskPerformer.Version; } }

        /// <summary>
        /// L'autore della classe che implementa il task performer.
        /// </summary>
        /// <remarks>
        /// L'autore della classe che implementa il task performer viene ricavata dall'istanza
        /// di ITaskPerformer associata a questo oggetto in fase di inizializzazione.
        /// </remarks>
        string Author { get { return m_TaskPerformer.Author; } }

        /// <summary>
        /// La descrizione della classe che implementa il task performer.
        /// </summary>
        /// <remarks>
        /// La descrizione della classe che implementa il task performer viene ricavata dall'istanza
        /// di ITaskPerformer associata a questo oggetto in fase di inizializzazione.
        /// </remarks>
        string Description { get { return m_TaskPerformer.Description; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Esegue il task i cui dati sono accessibili dal file di origine specificato
        /// e salva i risultati ottenuti dall'esecuzione del task sul file specificato
        /// come destinazione.
        /// </summary>
        /// <param name="sourceFileName">nome del file da cui leggere i dati del task da eseguire</param>
        /// <param name="targetFileName">nome del file su cui scrivere i risultati del task eseguito</param>
        /// <exception cref="ArgumentNullException">
        /// I parametri sourceFileName e/o targetFileName sono null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// I parametri sourceFileName e/o targetFileName sono stringhe di lunghezza zero, contengono
        /// solo spazi vuoti oppure uno o più caratteri non validi.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Il file sorgente specificato in sourceFileName non è stato trovato.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// I percorsi specificati nei parametri sourceFileName e/o targetFileName non sono validi (ad esempio,
        /// si trovano su un'unità non mappata).
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Nei parametri sourceFileName e/o targetFileName, il percorso, il nome del file o entrambi superano
        /// la lunghezza massima definita dal sistema. Su piattaforme Windows, ad esempio, i percorsi devono
        /// essere composti da un numero di caratteri inferiore a 248, mentre i nomi di file da un numero di
        /// caratteri inferiore a 260.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// I parametri specificati in sourceFileName e/o targetFileName sono directory, oppure non si dispone
        /// delle autorizzazioni necessarie per aprire in sola lettura il file specificato in sourceFileName
        /// o per aprire in scrittura il file specificato in targetFileName.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// I parametri specificati in sourceFileName e/o targetFileName sono in un formato non valido.
        /// </exception>
        /// <exception cref="IOException">
        /// Si è verificato un errore di I/O, ad esempio il file specificato in targetFileName è già presente,
        /// oppure il relativo flusso è stato chiuso.
        /// </exception>
        /// <exception cref="SecurityException">
        /// Il chiamante non dispone dell'autorizzazione richiesta per scrivere sul file specificato tramite
        /// il parametro targetFileName.
        /// </exception>
        /// <exception cref="TaskPerformerException">
        /// Si è verificata un'eccezione durante l'elaborazione del task, che non ne ha consentito
        /// il completamento, provocata da un errore manifestatosi nella classe usata per eseguire
        /// l'elaborazione del task. Questa eccezione contiene, come eccezione interna, quella che
        /// è stata lanciata dal metodo Execute della classe che implementa ITaskPerformer.
        /// </exception>
        public void Execute(string sourceFileName, string targetFileName)
        {
            FileStream sourceStream = null;
            FileStream targetStream = null;

            try
            {
                sourceStream = File.OpenRead(sourceFileName);
                targetStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write);

                try
                {
                    // Esegue il task su un componente specifico.
                    m_TaskPerformer.Execute(sourceStream, targetStream);
                }
                catch (Exception e)
                {
                    // Wrapping per distinguere la provenienza.
                    throw new TaskPerformerException(e.Message, e);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (sourceStream != null)
                {
                    sourceStream.Close();
                    sourceStream.Dispose();
                    sourceStream = null;
                }

                if (targetStream != null)
                {
                    targetStream.Close();
                    targetStream.Dispose();
                    targetStream = null;
                }
            }
        }

        #endregion
    }
}
