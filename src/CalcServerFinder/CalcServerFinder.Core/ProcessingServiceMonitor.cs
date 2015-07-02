using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CalcServerFinder.Core
{
    /// <summary>
    /// Questa classe permette di interrogare un servizio di elaborazione remota per conoscere le risorse di calcolo
    /// che sono eventualmente disponibili per l'utilizzo da parte di un'applicazione client.
    /// </summary>
    public class ProcessingServiceMonitor
    {
        #region Fields

        // --- configurazione del proxy
        private readonly Binding m_BindingConfig;
        private readonly Uri m_RemoteUri;

        // --- proxy per comunicare col server di calcolo
        private ProcessingServiceClient m_InternalProxy;
        private bool m_MonitorClosed;

        private readonly object m_SyncLock = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza della classe ProcessingServiceMonitor usando i parametri specificati
        /// per la configurazione di un proxy interno.
        /// </summary>
        /// <param name="bindingConfig">La configurazione del proxy interno.</param>
        /// <param name="remoteUri">L'uri del servizio di elaborazione.</param>
        /// <remarks>
        /// La configurazione del proxy interno è posticipata e viene pertanto eseguita non appena sarà
        /// richiesta la prima comunicazione col servizio di elaborazione.
        /// </remarks>
        public ProcessingServiceMonitor(Binding bindingConfig, Uri remoteUri)
        {
            m_BindingConfig = bindingConfig;
            m_RemoteUri = remoteUri;

            m_InternalProxy = null;
            m_MonitorClosed = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Ottiene l'uri remoto del servizio di elaborazione associato a questo oggetto.
        /// </summary>
        public Uri RemoteUri
        {
            get { return m_RemoteUri; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Interroga il servizio di elaborazione remota specificato al momento della creazione di questo oggetto
        /// al fine di ricevere le eventuali risorse di calcolo messe a disposizione da tale servizio e l'istante
        /// di ricezione della risposta da parte del servizio stesso. Se non si verifica nessun errore durante la
        /// comunicazione col servizio, questo metodo restituisce true; in caso contratio restituisce false.
        /// </summary>
        /// <param name="resources">L'elenco risorse eventualmente disponibili sul server di elaborazione.</param>
        /// <param name="detection">L'istante di più recente rilevazione del server di elaborazione.</param>
        /// <returns>true se non si verifica nessun errore durante la comunicazione; in caso contrario false.</returns>
        public bool TryGetResources(out IEnumerable<TaskPerformerInfo> resources, out DateTime detection)
        {
            lock (m_SyncLock)
            {
                if (m_MonitorClosed || m_BindingConfig == null || m_RemoteUri == null)
                {
                    resources = default(IEnumerable<TaskPerformerInfo>);
                    detection = default(DateTime);
                    return false;
                }

                try
                {
                    if (m_InternalProxy == null)
                    {
                        m_InternalProxy = new ProcessingServiceClient(m_BindingConfig, new EndpointAddress(m_RemoteUri));
                        m_InternalProxy.Open();
                    }

                    string[] resourcesList = m_InternalProxy.QueryForEnabledResources();
                    detection = DateTime.Now;

                    HashSet<TaskPerformerInfo> resourceSet = new HashSet<TaskPerformerInfo>();
                    foreach (var resourceItem in resourcesList)
                        resourceSet.Add(new TaskPerformerInfo(resourceItem));

                    resources = resourceSet;
                    return true;
                }
                catch
                {
                    if (m_InternalProxy != null)
                    {
                        if (m_InternalProxy.State == CommunicationState.Faulted)
                        {
                            m_InternalProxy.Abort();
                            m_InternalProxy = null;
                        }
                    }

                    resources = default(IEnumerable<TaskPerformerInfo>);
                    detection = default(DateTime);
                    return false;
                }
            }
        }

        /// <summary>
        /// Rilascia le risorse utilizzate per la comunicazione col server di elaborazione.
        /// </summary>
        public void Close()
        {
            lock (m_SyncLock)
            {
                if (m_InternalProxy != null)
                {
                    try
                    {
                        m_InternalProxy.Close();
                    }
                    catch
                    {
                        if (m_InternalProxy.State == CommunicationState.Faulted)
                        {
                            m_InternalProxy.Abort();
                        }
                    }
                    finally
                    {
                        m_InternalProxy = null;
                        m_MonitorClosed = true;
                    }
                }
            }
        }

        #endregion
    }
}
