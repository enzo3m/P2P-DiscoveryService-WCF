using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalcServer.TaskProcessing
{
    /// <summary>
    /// Rappresenta l'interfaccia che occorre implementare per la creazione di una nuova classe
    /// capace di eseguire un task personalizzato sull'insieme di dati specificato, ammesso che
    /// quest'ultimo sia compatibile con la classe sviluppata.
    /// </summary>
    public interface ITaskPerformer
    {
        #region Properties

        /// <summary>
        /// Il nome completo della classe che implementa il task performer.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// La versione della classe che implementa il task performer.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// L'autore della classe che implementa il task performer.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// La descrizione della classe che implementa il task performer.
        /// </summary>
        string Description { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Esegue il task i cui dati sono accessibili dallo stream di origine specificato
        /// e salva i risultati ottenuti dall'esecuzione del task sullo stream specificato
        /// come destinazione.
        /// </summary>
        /// <param name="sourceStream">stream da cui leggere i dati del task da eseguire</param>
        /// <param name="targetStream">stream su cui scrivere i risultati del task eseguito</param>
        void Execute(Stream sourceStream, Stream targetStream);

        #endregion
    }
}
