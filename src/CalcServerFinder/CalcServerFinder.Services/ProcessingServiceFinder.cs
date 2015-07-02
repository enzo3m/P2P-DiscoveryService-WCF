using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

using CalcServerFinder.Logging;
using CalcServerFinder.Contracts;
using CalcServerFinder.Core;
using CalcServerFinder.Core.Extensions;

namespace CalcServerFinder.Services
{
    /// <summary>
    /// Questa classe implementa il servizio di ricerca delle risorse di elaborazione usato dai client.
    /// </summary>
    public class ProcessingServiceFinder : IProcessingServiceFinder
    {
        #region Fields

        // --- istanze degli oggetti singleton (condivisi tra tutte le istante di questo servizio)
        private static readonly Logger m_AppLogger = Logger.Instance;
        private static readonly ResourceCache m_ResourceCache = ResourceCache.Instance;
        private static readonly SearchManager m_SearchManager = SearchManager.Instance;
        private static readonly CommunicationHandler m_CommunicationHandler = CommunicationHandler.Instance;

        #endregion

        #region Public Methods

        /// <summary>
        /// Questo metodo permette di interrogare un nodo di ricerca e di avviare eventualmente una nuova ricerca.
        /// Prima verifica se il nodo di ricerca interrogato possiede una o più risorse compatibili con le opzioni
        /// specificate ed eventualmente le aggiunge ai risultati da inviare al client. Successivamente, controlla
        /// se la cache del gestore delle ricerche contiene uno o più risultati relativi ad una ricerca uguale, ma
        /// non ancora scaduta, ed eventualmente aggiunge anche questi ai risultati da inviare al client. Nel caso
        /// in cui la cache del gestore delle ricerche non dovesse contenere nessun risultato che corrisponde alle
        /// opzioni specificate, inizia una nuova ricerca attraverso la rete dei nodi di ricerca, ma nel frattempo
        /// restituisce al client i risultati disponibili oppure una lista vuota.
        /// </summary>
        /// <param name="options">Le opzioni di ricerca specificate dal client.</param>
        /// <param name="addresses">Gli eventuali indirizzi che corrispondono alle opzioni di ricerca.</param>
        public void Search(SearchOptions options, out List<Uri> addresses)
        {
            WriteToLog("Search options: <{0}-{1}>", options.Name, options.Version);

            // Trova eventuali risorse disponibili in questo nodo di ricerca.
            List<Uri> found = m_ResourceCache.Search(
                delegate(Uri uri, IEnumerable<TaskPerformerInfo> resources)
                {
                    foreach (var resource in resources)
                    {
                        if (resource.Name == options.Name && resource.Version == options.Version)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            ).ToList<Uri>();

            // Trova eventuali risorse disponibili in altri nodi di ricerca.
            SearchData data = new SearchData(options);
            SearchResult result;
            if (!m_SearchManager.TryEnqueueNewSearch(data, out result))
            {
                found.AddRange(result.FoundServices);   // ricerca già presente --> recupera risultati correnti
            }
            else 
            {
                WriteToLog("Starting new search...");

                if (m_CommunicationHandler.CreateNewSearch(data))
                {
                    WriteToLog("New search started.");
                }
                else
                {
                    WriteToLog("Error starting new search.");

                    m_SearchManager.Remove(data);   // rollback
                }
            }

            addresses = found.ToHashSet<Uri>().ToList<Uri>();
        }

        #endregion

        #region Logging Private Methods

        /// <summary>
        /// Permette l'accesso alle funzionalità di logging da parte dei metodi di questa classe,
        /// aggiungendo informazioni accessorie in aggiunta alla descrizione specificata.
        /// </summary>
        /// <param name="format">Una stringa in formato composito.</param>
        /// <param name="args">Un array di oggetti contenente zero o più oggetti da formattare.</param>
        private void WriteToLog(string format, params object[] args)
        {
            DateTime logTime = DateTime.Now;
            string moduleInfo = string.Format("{0}. Thread: {1}.",
                GetType().FullName.ToString(),   // oppure: GetType().AssemblyQualifiedName.ToString(),
                Thread.CurrentThread.ManagedThreadId);
            string textToLog = string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args);

            m_AppLogger.Write(logTime, moduleInfo, textToLog);
        }

        #endregion
    }
}
