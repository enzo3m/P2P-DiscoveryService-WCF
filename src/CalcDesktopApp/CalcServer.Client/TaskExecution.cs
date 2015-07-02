using System;
using System.Collections.Generic;
using System.Threading;
using System.ServiceModel;
using System.ComponentModel;

using CalcServer.Contracts;

namespace CalcServer.Client
{
    /// <summary>
    /// Questa classe permette di avviare l'elaborazione di un task richiedendo l'intervento di un servizio
    /// di elaborazione e attendendo che quest'ultimo produca gli eventuali risultati.
    /// </summary>
    public class TaskExecution
    {
        #region Events

        /// <summary>
        /// Questo evento viene generato non appena la richiesta di elaborazione inviata al servizio
        /// viene completata e sono disponibili gli eventuali risultati, oppure nel caso in cui tale
        /// richiesta è stata annullata oppure se si è verificato un errore durante la comunicazione
        /// col servizio.
        /// </summary>
        public event TaskExecutionCompletedHandler OnTaskExecutionCompleted;

        /// <summary>
        /// Questo evento viene generato quando è disponibile una nuova informazione sullo stato
        /// della richiesta di elaborazione inviata al servizio.
        /// </summary>
        public event TaskExecutionProgressHandler OnTaskExecutionProgress;

        /// <summary>
        /// Genera un nuovo evento per notificare il completamento della richiesta di elaborazione del task.
        /// </summary>
        /// <param name="cancelled">un valore che indica se il task è stato annullato</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <param name="result">gli eventuali risultati scaricati dal server di elaborazione</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il patametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati dell'elaborazione di un task non sono disponibili
        /// a causa di un errore o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere
        /// impostato a null.
        /// </remarks>
        private void RaiseTaskExecutionCompleted(bool cancelled, Exception error, TaskResults result)
        {
            TaskExecutionCompletedHandler handler = OnTaskExecutionCompleted;
            if (handler != null)
            {
                TaskExecutionCompletedEventArgs args =
                    new TaskExecutionCompletedEventArgs(cancelled, error, result, m_TaskRequestId);
                OnTaskExecutionCompleted(this, args);
            }
        }
        
        /// <summary>
        /// Genera un nuovo evento per notificare il cambiamento di stato della richiesta di elaborazione del task.
        /// </summary>
        /// <param name="state">lo stato relativo alla richiesta di elaborazione</param>
        /// <param name="error">l'eventuale errore verificatosi durante la comunicazione col servizio</param>
        /// <remarks>
        /// Se non si è verificato alcun errore durante la comunicazione col servizio, il parametro corrispondente
        /// va impostato a null. In modo analogo, se i risultati dell'elaborazione di un task non sono disponibili
        /// a causa di un errore o perché l'operazione è stata annullata, il parametro corrispondente dovrà essere
        /// impostato a null.
        /// </remarks>
        private void RaiseTaskExecutionProgress(TaskExecutionState state, Exception error)
        {
            TaskExecutionProgressHandler handler = OnTaskExecutionProgress;
            if (handler != null)
            {
                TaskExecutionProgressEventArgs args =
                    new TaskExecutionProgressEventArgs(state, error, m_TaskRequestId);
                OnTaskExecutionProgress(this, args);
            }
        }

        #endregion

        #region Fields

        private readonly TimeSpan m_PollingInterval;
        private readonly ManualResetEvent m_PollingWaitHandle;

        private readonly BasicHttpBinding m_Binding;
        private readonly EndpointAddress m_Endpoint;

        private ProcessingServiceClient m_Proxy;
        private Dictionary<Type, TaskExecutionState> m_CommunicationErrorsMapping;

        private string m_TaskRequestId;
        private TaskData m_TaskData;
        private TaskResults m_TaskResults;

        #endregion

        #region Constructors

        /// <summary>
        /// Istanzia un nuovo oggetto della classe TaskExecution con i parametri specificati.
        /// </summary>
        /// <param name="serviceUri">indirizzo del servizio di elaborazione del task</param>
        /// <param name="pollingInterval">tempo di attesa nella verifica di completamento del task</param>
        /// <exception cref="UriFormatException">
        /// Se si specifica un URI non valido come indirizzo del servizio di elaborazione.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Se si specifica il valore null come indirizzo del servizio di elaborazione.
        /// </exception>
        public TaskExecution(string serviceUri, TimeSpan pollingInterval)
        {
            m_PollingInterval = pollingInterval;
            m_PollingWaitHandle = new ManualResetEvent(false);

            m_Binding = new BasicHttpBinding();
            m_Endpoint = new EndpointAddress(serviceUri);

            m_Proxy = null;
            m_CommunicationErrorsMapping = new Dictionary<Type, TaskExecutionState>()
            {
                { typeof(TimeoutException), TaskExecutionState.TimeoutError },
                { typeof(EndpointNotFoundException), TaskExecutionState.ServiceNotFoundError },
                { typeof(CommunicationException), TaskExecutionState.CommunicationError }
            };

            m_TaskRequestId = string.Empty;
            m_TaskData = new TaskData() { Name = string.Empty, Contents = string.Empty };
            m_TaskResults = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Imposta i dati relativi al task da elaborare.
        /// </summary>
        /// <param name="name">il nome da associare al task</param>
        /// <param name="contents">la stringa in formato XML contenente i dati del task</param>
        public void SetTaskData(string name, string contents)
        {
            m_TaskData.Name = name;
            m_TaskData.Contents = contents;
        }

        /// <summary>
        /// Determina l'avvio della procedura che permette di inviare il task specificato al servizio di elaborazione,
        /// di verificarne periodicamente il completamento e di eseguire alla fine il download dei relativi risultati
        /// eventualmente prodotti.
        /// </summary>
        /// <remarks>
        /// Prima di invocare questo metodo, è necessario utilizzare il metodo SetTaskData per poter impostare i dati
        /// relativi al task da elaborare.
        /// Inoltre, questo metodo non dovrebbe essere invocato sul thread della UI, in quanto la verifica periodica
        /// del completamento dell'elaborazione blocca tale thread per l'intervallo di polling specificato.
        /// </remarks>
        public void Start()
        {
            if (m_Proxy == null)
            {
                try
                {
                    RaiseTaskExecutionProgress(TaskExecutionState.InitializingProxy, null);
                    m_Proxy = new ProcessingServiceClient(m_Binding, m_Endpoint);
                    m_Proxy.Open();
                    RaiseTaskExecutionProgress(TaskExecutionState.ProxyInitialized, null);

                    RaiseTaskExecutionProgress(TaskExecutionState.SendingRequest, null);
                    m_TaskRequestId = m_Proxy.SubmitData(m_TaskData);
                    RaiseTaskExecutionProgress(TaskExecutionState.RequestSent, null);

                    bool completed = false;
                    while (!completed)
                    {
                        m_PollingWaitHandle.WaitOne(m_PollingInterval);
                        TaskState ts = m_Proxy.GetState(m_TaskRequestId);

                        switch (ts)
                        {
                            case TaskState.Completed:
                                RaiseTaskExecutionProgress(TaskExecutionState.DownloadingResults, null);
                                m_TaskResults = m_Proxy.GetResults(m_TaskRequestId);
                                RaiseTaskExecutionProgress(TaskExecutionState.ResultsDownloaded, null);
                                completed = true;
                                break;

                            case TaskState.None:
                                completed = true;
                                break;

                            default:
                                completed = false;
                                break;
                        }
                    }

                    RaiseTaskExecutionProgress(TaskExecutionState.DisposingProxy, null);
                    m_Proxy.Close();
                    RaiseTaskExecutionProgress(TaskExecutionState.ProxyDisposed, null);

                    RaiseTaskExecutionCompleted(false, null, m_TaskResults);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }
        }

        #endregion
                
        #region Error Handling

        /// <summary>
        /// Permette di gestire un errore verificatosi durante la procedura e restituisce true se è stata gestita,
        /// altrimenti false se non è stato necessario gestirlo poiché di fatto non è avvenuta alcun errore.
        /// </summary>
        /// <param name="error">l'eccezione che ha provocato l'errore</param>
        private void HandleError(Exception error)
        {
            if (error != null)
            {
                TaskExecutionState state;
                if (m_CommunicationErrorsMapping.TryGetValue(error.GetType(), out state))
                {
                    RaiseTaskExecutionProgress(state, error);

                    m_Proxy.Abort();
                    RaiseTaskExecutionProgress(TaskExecutionState.ProxyDisposed, error);

                    m_Proxy = null;
                }
                else
                {
                    if (error is FaultException<ServiceFault>)
                    {
                        RaiseTaskExecutionProgress(TaskExecutionState.OperationError, error);

                        RaiseTaskExecutionProgress(TaskExecutionState.DisposingProxy, error);
                        m_Proxy.Close();
                    }
                    else
                    {
                        RaiseTaskExecutionProgress(TaskExecutionState.UnknownError, error);

                        m_Proxy.Abort();
                        RaiseTaskExecutionProgress(TaskExecutionState.ProxyDisposed, error);

                        m_Proxy = null;
                    }
                }

                RaiseTaskExecutionCompleted(false, error, null);
            }
        }

        #endregion
    }
}
