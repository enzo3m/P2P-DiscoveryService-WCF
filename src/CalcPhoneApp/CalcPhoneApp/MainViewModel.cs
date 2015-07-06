using System;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml.Linq;

using CalcServer.Contracts;
using CalcServer.Client;
using CalcServerFinder.Client;

namespace CalcPhoneApp
{
    /// <summary>
    /// Rappresenta il ViewModel principale di questa applicazione.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private readonly UiThreadUpdater m_Updater;

        private string m_TaskData;
        private string m_SearchNodeUri;

        private ObservableCollection<String> m_ProcessingServicesList;
        private int m_SelectedProcessingServiceIndex;

        private string m_TaskResult;
        private string m_ReportText;

        private bool m_IsBusy;

        private readonly CommandHandler<Object> m_SearchCommand;
        private readonly CommandHandler<Object> m_ProcessCommand;
        private readonly CommandHandler<Object> m_ClearCommand;

        private uint m_TasksCounter;

        #endregion

        #region Constructors

        /// <summary>
        /// Istanzia un nuovo oggetto.
        /// </summary>
        public MainViewModel()
        {
            m_Updater = new UiThreadUpdater();

            m_TaskData = string.Empty;
            m_SearchNodeUri = string.Empty;

            m_ProcessingServicesList = new ObservableCollection<String>();
            m_SelectedProcessingServiceIndex = -1;

            m_TaskResult = string.Empty;
            m_ReportText = string.Empty;

            m_IsBusy = false;

            m_SearchCommand = new CommandHandler<Object>(
                (s) => { ExecuteSearch(); },   // ICommand.Execute
                (s) => { return CanExecuteSearch(); }   // ICommand.CanExecute
            );

            m_ProcessCommand = new CommandHandler<Object>(
                (s) => { ExecuteProcess(); },   // ICommand.Execute
                (s) => { return CanExecuteProcess(); }   // ICommand.CanExecute
            );

            m_ClearCommand = new CommandHandler<Object>(
                (s) => { ExecuteClear(); },   // ICommand.Execute
                (s) => { return CanExecuteClear(); }   // ICommand.CanExecute
            );

            m_TasksCounter = 0;
        }

        #endregion

        #region Public ICommand Properties

        /// <summary>
        /// Il comando che permette di avviare la ricerca.
        /// </summary>
        public ICommand SearchCommand { get { return m_SearchCommand; } }

        /// <summary>
        /// Il comando che permette di avviare l'elaborazione.
        /// </summary>
        public ICommand ProcessCommand { get { return m_ProcessCommand; } }

        /// <summary>
        /// Il comando che permette di pulire l'interfaccia grafica.
        /// </summary>
        public ICommand ClearCommand { get { return m_ClearCommand; } }

        #endregion

        #region Properties

        /// <summary>
        /// La stringa contenente i dati da elaborare.
        /// </summary>
        public string TaskData
        {
            get { return m_TaskData; }
            set
            {
                if (value == m_TaskData) return;
                m_TaskData = value;
                OnPropertyChanged("TaskData");
                m_SearchCommand.RaiseCanExecuteChanged();
                m_ProcessCommand.RaiseCanExecuteChanged();
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// La stringa contenente l'indirizzo del nodo di ricerca.
        /// </summary>
        public string SearchNodeUri
        {
            get { return m_SearchNodeUri; }
            set
            {
                if (value == m_SearchNodeUri) return;
                m_SearchNodeUri = value;
                OnPropertyChanged("SearchNodeUri");
                m_SearchCommand.RaiseCanExecuteChanged();
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// La lista contenente gli indirizzi dei servizi di elaborazione disponibili.
        /// </summary>
        public ObservableCollection<String> ProcessingServicesList
        {
            get { return m_ProcessingServicesList; }
            set
            {
                m_ProcessingServicesList = value;
                OnPropertyChanged("ProcessingServicesList");
                m_ProcessCommand.RaiseCanExecuteChanged();
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// L'indice dell'elemento selezionato nella lista degli indirizzi dei servizi di elaborazione disponibili.
        /// </summary>
        public int SelectedProcessingServiceIndex
        {
            get { return m_SelectedProcessingServiceIndex; }
            set
            {
                if (value == m_SelectedProcessingServiceIndex) return;
                m_SelectedProcessingServiceIndex = value;
                OnPropertyChanged("SelectedProcessingServiceIndex");
                m_ProcessCommand.RaiseCanExecuteChanged();
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// La stringa contenente i risultati dell'elaborazione.
        /// </summary>
        public string TaskResult
        {
            get { return m_TaskResult; }
            set
            {
                if (value == m_TaskResult) return;
                m_TaskResult = value;
                OnPropertyChanged("TaskResult");
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// La stringa contenente il testo del resoconto.
        /// </summary>
        public string ReportText
        {
            get { return m_ReportText; }
            set
            {
                if (value == m_ReportText) return;
                m_ReportText = value;
                OnPropertyChanged("ReportText");
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Lo stato corrente dell'applicazione.
        /// </summary>
        /// <remarks>
        /// Questa proprietà viene impostata a true non appena viene avviato un comando di ricerca, di elaborazione
        /// o di pulizia, per poi essere impostato a false non appena l'esecuzione del comando viene completata.
        /// </remarks>
        public bool IsBusy
        {
            get { return m_IsBusy; }
            private set
            {
                if (value == m_IsBusy) return;
                m_IsBusy = value;
                m_SearchCommand.RaiseCanExecuteChanged();
                m_ProcessCommand.RaiseCanExecuteChanged();
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region ICommand Actions

        /// <summary>
        /// Invia una richiesta di ricerca al servizio remoto e attende che venga completata.
        /// </summary>
        /// <remarks>
        /// Il lavoro viene eseguito su un nuovo thread.
        /// </remarks>
        private void ExecuteSearch()
        {
            ProcessingServicesList.Clear();
            SelectedProcessingServiceIndex = -1;
            TaskResult = string.Empty;
            ReportText = string.Empty;

            string resourceClassName, resourceClassVersion;
            if (!TryExtractResourceInfo(TaskData, out resourceClassName, out resourceClassVersion))
            {
                UpdateReportText("Errore: impossibile trovare nome e versione del componente di elaborazione.");
                return;
            }

            if (!IsValidUri(SearchNodeUri))
            {
                UpdateReportText("Errore: indirizzo non valido per il servizio di ricerca ({0}).", SearchNodeUri);
                return;
            }

            IsBusy = true;

            new Thread(() =>
            {
                ResourceSearch rs = new ResourceSearch(SearchNodeUri, TimeSpan.FromSeconds(5), 6);
                rs.OnResourceSearchProgress += new ResourceSearchProgressHandler(ResourceSearch_Progress);
                rs.OnResourceSearchCompleted += new ResourceSearchCompletedHandler(ResourceSearch_Completed);

                rs.SetSearchOptions(resourceClassName, resourceClassVersion);
                rs.Start();
            }
            ) { Name = "SearchThread", IsBackground = true }.Start();
        }

        /// <summary>
        /// Invia una richiesta di elaborazione al servizio remoto e attende che venga completata.
        /// </summary>
        /// <remarks>
        /// Il lavoro viene eseguito su un nuovo thread.
        /// </remarks>
        private void ExecuteProcess()
        {
            TaskResult = string.Empty;
            ReportText = string.Empty;

            string uriProcessingService = ProcessingServicesList[SelectedProcessingServiceIndex];
            string currentTaskName = "Task-" + (m_TasksCounter++).ToString();

            if (!IsValidUri(uriProcessingService))
            {
                UpdateReportText("Errore: indirizzo non valido per il servizio di elaborazione ({0}).", uriProcessingService);
                return;
            }

            IsBusy = true;

            new Thread(() =>
            {
                TaskExecution te = new TaskExecution(uriProcessingService, TimeSpan.FromSeconds(2));
                te.OnTaskExecutionProgress += new TaskExecutionProgressHandler(TaskExecution_Progress);
                te.OnTaskExecutionCompleted += new TaskExecutionCompletedHandler(TaskExecution_Completed);

                te.SetTaskData(currentTaskName, TaskData);
                te.Start();
            }
            ) { Name = "ProcessingThread", IsBackground = true }.Start();
        }

        /// <summary>
        /// Pulisce tutti i campi della UI reinizializzandola allo stato iniziale.
        /// </summary>
        private void ExecuteClear()
        {
            IsBusy = true;

            TaskData = string.Empty;
            SearchNodeUri = string.Empty;

            ProcessingServicesList.Clear();
            SelectedProcessingServiceIndex = -1;

            TaskResult = string.Empty;
            ReportText = string.Empty;

            IsBusy = false;
        }

        #endregion

        #region ICommand Conditions

        /// <summary>
        /// Verifica se sussistono tutte le condizioni per abilitare il comando di ricerca.
        /// </summary>
        /// <returns>true se è possibile abilitare il comando di ricerca, altrimenti false</returns>
        private bool CanExecuteSearch()
        {
            return !IsBusy && (
                !string.IsNullOrWhiteSpace(TaskData) && !string.IsNullOrWhiteSpace(SearchNodeUri)
                );
        }

        /// <summary>
        /// Verifica se sussistono tutte le condizioni per abilitare il comando di elaborazione.
        /// </summary>
        /// <returns>true se è possibile abilitare il comando di elaborazione, altrimenti false</returns>
        private bool CanExecuteProcess()
        {
            return !IsBusy && (
                !string.IsNullOrEmpty(TaskData) &&
                ProcessingServicesList.Count > 0 && SelectedProcessingServiceIndex >= 0 && SelectedProcessingServiceIndex < ProcessingServicesList.Count
                );
        }

        /// <summary>
        /// Verifica se sussiste almeno una condizione per abilitare il comando di pulizia.
        /// </summary>
        /// <returns>true se è possibile abilitare il comando di pulizia, altrimenti false</returns>
        private bool CanExecuteClear()
        {
            return !IsBusy && (
                !string.IsNullOrEmpty(TaskData) || !string.IsNullOrEmpty(SearchNodeUri) ||
                ProcessingServicesList.Count > 0 ||
                !string.IsNullOrEmpty(TaskResult) || !string.IsNullOrEmpty(ReportText)
                );
        }

        #endregion

        #region Task Execution Events Handling

        /// <summary>
        /// Questo metodo viene invocato per ogni passo della richiesta di elaborazione.
        /// </summary>
        /// <param name="sender">l'oggetto che ha invocato questo metodo</param>
        /// <param name="args">le informazioni aggiuntive sull'evento</param>
        private void TaskExecution_Progress(object sender, TaskExecutionProgressEventArgs args)
        {
            string report = TaskExecutionStateDescriptions.GetStateDescription(args.State) +
                (!string.IsNullOrWhiteSpace(args.Id) ? string.Format(" Task Id = {0}.", args.Id) : string.Empty);

            m_Updater.Update(() =>
            {
                UpdateReportText(report);
            }
            );
        }

        /// <summary>
        /// Questo metodo viene invocato nel momento in cui viene completata la richiesta di elaborazione.
        /// </summary>
        /// <param name="sender">l'oggetto che ha invocato questo metodo</param>
        /// <param name="args">le informazioni aggiuntive sull'evento</param>
        private void TaskExecution_Completed(object sender, TaskExecutionCompletedEventArgs args)
        {
            string result = string.Empty;
            StringBuilder report = new StringBuilder();

            if (args.Cancelled)
            {
                report.Append("Richiesta di elaborazione annullata.");
                if (!string.IsNullOrWhiteSpace(args.Id))
                    report.AppendFormat(" Task Id = {0}.", args.Id);
            }
            else if (args.Error != null)
            {
                report.Append("Errore durante la richiesta.");
                if (!string.IsNullOrWhiteSpace(args.Id))
                    report.AppendFormat(" Task Id = {0}.", args.Id);

                if (args.Error is FaultException<ServiceFault>)
                {
                    FaultException<ServiceFault> fault = args.Error as FaultException<ServiceFault>;
                    report.AppendFormat(" Identificativo errore: {0}. ", fault.Detail.Id);
                    report.Append(TaskExecutionStateDescriptions.GetStateDescription(fault.Detail.Code));
                }
                else
                {
                    report.Append(' ');
                    report.Append(args.Error.Message);
                }
            }
            else
            {
                TaskResults tr = args.Result;
                result = tr.Contents;
                report.AppendLine("Richiesta di elaborazione completata con successo.");
                report.AppendLine("Tempo di elaborazione sul server: " + tr.ElapsedTime + ".");

                if (tr.EncounteredErrors != null && tr.EncounteredErrors.Length > 0)
                {
                    report.AppendLine("Errori di elaborazione: " + tr.EncounteredErrors.Length + ".");
                    foreach (var pe in tr.EncounteredErrors)
                    {
                        report.AppendLine(string.Format("[Id = {0}, Codice = {1}, {2}]", pe.Id, pe.Code, pe.Details));
                    }
                }
            }

            m_Updater.Update(() =>
            {
                TaskResult = result;
                UpdateReportText(report.ToString());

                IsBusy = false;
            }
            );
        }

        #endregion

        #region Resource Search Events Handling

        /// <summary>
        /// Questo metodo viene invocato ogni volta che si ricevono nuovi risultati di ricerca.
        /// </summary>
        /// <param name="sender">l'oggetto che ha invocato questo metodo</param>
        /// <param name="args">le informazioni aggiuntive sull'evento</param>
        private void ResourceSearch_Progress(object sender, ResourceSearchProgressEventArgs args)
        {
            List<String> result = new List<String>();
            string report = ResourceSearchStateDescriptions.GetStateDescription(args.State);

            if (args.Result != null)
            {
                foreach (var uri in args.Result)
                {
                    result.Add(uri.ToString());
                }
            }

            m_Updater.Update(() =>
            {
                UpdateProcessingServicesList(result);
                UpdateReportText(report);
            }
            );
        }

        /// <summary>
        /// Questo metodo viene invocato nel momento in cui viene completata la ricerca.
        /// </summary>
        /// <param name="sender">l'oggetto che ha invocato questo metodo</param>
        /// <param name="args">le informazioni aggiuntive sull'evento</param>
        private void ResourceSearch_Completed(object sender, ResourceSearchCompletedEventArgs args)
        {
            List<String> result = new List<String>();
            StringBuilder report = new StringBuilder();

            if (args.Result != null)
            {
                foreach (var uri in args.Result)
                {
                    result.Add(uri.ToString());
                }
            }

            if (args.Cancelled)
            {
                report.Append("Ricerca annullata.");
            }
            else if (args.Error != null)
            {
                report.Append("Errore durante la ricerca.");
                report.Append(' ');
                report.Append(args.Error.Message);
            }
            else
            {
                report.Append("Ricerca completata con successo.");
                report.Append(' ');
                report.AppendFormat("Trovati {0} risultati.", result.Count);
            }

            m_Updater.Update(() =>
            {
                UpdateProcessingServicesList(result);
                UpdateReportText(report.ToString());

                IsBusy = false;
            }
            );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Aggiorna il testo del resoconto, accodandovi una stringa di formato composito, che contiene
        /// zero o più elementi di formato, secondo le stesse regole previste per l'utilizzo del metodo
        /// <see cref="System.String.Format(System.String, System.Object[])"/>.
        /// </summary>
        /// <param name="format">stringa di formato composito</param>
        /// <param name="args">matrice di oggetti da formattare</param>
        private void UpdateReportText(string format, params object[] args)
        {
            m_ReportText += string.Format(format, args);
            m_ReportText += Environment.NewLine;

            OnPropertyChanged("ReportText");
            m_ClearCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Aggiorna l'elenco degli indirizzi relativi ai servizi di elaborazione, aggiungendovi quelli specificati,
        /// quindi senza rimuovere quelli eventualmente già presenti ed escludendo eventuali valori duplicati.
        /// </summary>
        /// <param name="addresses">gli indirizzi da aggiungere</param>
        private void UpdateProcessingServicesList(List<String> addresses)
        {
            if (addresses != null)
            {
                foreach (var addr in addresses)
                {
                    UpdateProcessingServicesList(addr);
                }
            }
        }

        /// <summary>
        /// Aggiorna l'elenco degli indirizzi relativi ai servizi di elaborazione, aggiungendovi quello specificato,
        /// quindi senza rimuovere quelli eventualmente già presenti, ma escludendolo se è già presente nell'elenco.
        /// </summary>
        /// <param name="address">l'indirizzo da aggiungere</param>
        private void UpdateProcessingServicesList(string address)
        {
            if (!m_ProcessingServicesList.Contains(address))
            {
                m_ProcessingServicesList.Add(address);

                OnPropertyChanged("ProcessingServicesList");
                m_ProcessCommand.RaiseCanExecuteChanged();
                m_ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Verifica se la stringa specificata rappresenta un URI valido.
        /// </summary>
        /// <param name="uriString">la stringa da verificare</param>
        /// <returns>true se la stringa specificata rappresenta un URI valido</returns>
        private bool IsValidUri(string uriString)
        {
            Uri result;
            return Uri.TryCreate(uriString, UriKind.Absolute, out result);
        }

        /// <summary>
        /// Prova ad estrarre le informazioni sulla risorsa di elaborazione richiesta per elaborare il task
        /// descritto dalla stringa XML specificata. Se il nome e la versione del componente che implementa
        /// la risorsa di elaborazione sono presenti nel testo XML specificato, vengono impostati in output
        /// e questo metodo restituisce true, altrimenti restituisce false.
        /// </summary>
        /// <param name="text">la stringa XML contenente la descrizione del task da elaborare</param>
        /// <param name="name">l'eventuale nome della risorsa di elaborazione specificata nella stringa XML</param>
        /// <param name="version">l'eventuale versione della risorsa di elaborazione specificata nella stringa XML</param>
        /// <returns>true se il nome e la versione della risorsa di elaborazione vengono trovati nella stringa XML</returns>
        public bool TryExtractResourceInfo(string text, out string name, out string version)
        {
            bool result;

            try
            {
                XDocument document = XDocument.Parse(text);
                XElement root = (document.Root.Name == "task" ? document.Root : null);
                
                XElement component = (root != null ? root.Element("component") : null);
                
                name = (component != null ? component.Attribute("className").Value : string.Empty);
                version = (component != null ? component.Attribute("classVersion").Value : string.Empty);

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version))
                {
                    name = version = null;
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            catch
            {
                name = version = null;
                result = false;
            }

            return result;
        }

        #endregion
    }
}
