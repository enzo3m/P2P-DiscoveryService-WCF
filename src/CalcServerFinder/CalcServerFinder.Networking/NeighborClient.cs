using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;

using CalcServerFinder.Contracts;
using CalcServerFinder.Logging;

namespace CalcServerFinder.Networking
{
    /// <summary>
    /// Questa classe implementa un proxy thread-safe dotato di una coda FIFO per la memorizzazione temporanea
    /// dei messaggi in uscita e diretti verso un servizio remoto di query/reply: l'invio dei messaggi avviene
    /// grazie ad un thread interno che, dopo aver prelevato il prossimo messaggio dalla coda, lo trasmette al
    /// servizio remoto tramite un proxy interno.
    /// La politica di gestione della coda prevede che quando non c'è più spazio disponibile per conservare un
    /// nuovo messaggio, viene scartato. Non è stato implementato nessun meccanismo di gestione della priorità
    /// tra i messaggi.
    /// </summary>
    public class NeighborClient : IDisposable
    {
        #region Fields

        // --- istanze degli oggetti singleton
        private readonly Logger m_AppLogger = Logger.Instance;

        // --- oggetto SyncLock unico per questa istanza
        private readonly object m_SyncLock = new object();

        // --- configurazione del proxy
        private readonly Binding m_BindingConfig;
        private readonly Uri m_RemoteUri;

        // --- proxy per comunicare col servizio
        private IQueryReplyService m_InternalProxy;
        private bool m_ProxyClosed;

        // --- id per questo nodo e per quello di destinazione
        private readonly string m_NodeId;
        private readonly string m_TargetNodeId;

        // --- impostazioni della coda di uscita
        private readonly int m_OutputQueueSize;
        private readonly Queue<MessageData> m_OutputQueue;

        // --- thread per l'invio dei messaggi
        private bool m_Shutdown;
        private readonly Thread m_Dispatcher;

        #endregion

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza della classe NeighborClient usando i parametri specificati per la configurazione
        /// del proxy interno e per l'allocazione della coda dedicata ai messaggi in uscita.
        /// </summary>
        /// <param name="bindingConfig">Le impostazioni di configurazione del proxy interno.</param>
        /// <param name="remoteUri">L'uri del servizio remoto con cui il proxy deve comunicare.</param>
        /// <param name="nodeId">L'identificatore univoco del nodo rappresentato da questa istanza dell'applicazione.</param>
        /// <param name="nodeId">L'identificatore univoco del nodo di destinazione dei messaggi inviati da questa applicazione.</param>
        /// <param name="outputQueueSize">Il massimo numero di messaggi che possono essere inseriti nella coda.</param>
        /// <exception cref="ArgumentNullException">bindingConfig e/o remoteUri e/o nodeId e/o targetNodeId sono sull.</exception>
        /// <exception cref="ArgumentOutOfRangeException">outputQueueSize è minore o uguale a zero.</exception>
        /// <remarks>
        /// La configurazione del proxy interno è posticipata e viene pertanto eseguita non appena verrà richiesta
        /// la prima comunicazione col servizio remoto.
        /// </remarks>
        public NeighborClient(Binding bindingConfig, Uri remoteUri, string nodeId, string targetNodeId, int outputQueueSize)
        {
            if (bindingConfig == null)
                throw new ArgumentNullException("bindingConfig");

            if (remoteUri == null)
                throw new ArgumentNullException("remoteUri");

            if (nodeId == null)
                throw new ArgumentNullException("nodeId");

            if (targetNodeId == null)
                throw new ArgumentNullException("targetNodeId");

            if (targetNodeId == nodeId)
                throw new ArgumentException("Target node identifier must be different from current node identifier.", "targetNodeId");

            if (outputQueueSize < 1)
                throw new ArgumentOutOfRangeException("outputQueueSize", "The size must be positive.");

            m_BindingConfig = bindingConfig;
            m_RemoteUri = remoteUri;
            
            m_NodeId = nodeId;
            m_TargetNodeId = targetNodeId;

            m_InternalProxy = null;
            m_ProxyClosed = false;
            
            m_OutputQueueSize = outputQueueSize;
            m_OutputQueue = new Queue<MessageData>(outputQueueSize);

            m_Shutdown = false;
            m_Dispatcher = new Thread(Send) { IsBackground = true };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Ottiene l'uri remoto del servizio di elaborazione associato a questo NeighborClient.
        /// </summary>
        public Uri RemoteUri
        {
            get { return m_RemoteUri; }
        }

        /// <summary>
        /// Ottiene l'identificatore del nodo di destinazione dei messaggi inviati da questo NeighborClient.
        /// </summary>
        public string TargetNodeId
        {
            get { return m_TargetNodeId; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Richiede l'avvio del thread responsabile dell'invio dei messaggi via via messi in coda e restituisce true
        /// se la richiesta di avvio è stata completata. Se questo metodo viene invocato dopo aver chiamato il metodo
        /// Dispose, il thread non viene in ogni caso riavviato e questo metodo restituisce true per segnalare che il
        /// thread è in fase di shutdown o che ha già terminato la propria esecuzione.
        /// </summary>
        /// <returns>true se il thread non è in fase di shutdown; in caso contrario, false.</returns>
        public bool Run()
        {
            lock (m_SyncLock)
            {
                if (m_Shutdown) return false;
            }

            m_Dispatcher.Start();
            return true;
        }

        /// <summary>
        /// Inserisce il messaggio specificato nella coda di uscita di questo NeighborClient e restituisce true
        /// se il messaggio è stato effettivamente inserito in coda, altrimenti false se non è stato possibile
        /// aggiungere il messaggio perché la coda era piena.
        /// </summary>
        /// <param name="message">Il messaggio da inserire nella coda di uscita.</param>
        /// <returns>true se il messaggio è stato inserito in coda; in caso contrario, false.</returns>
        public bool Enqueue(MessageData message)
        {
            lock (m_SyncLock)
            {
                if (m_OutputQueue.Count == m_OutputQueueSize) return false;

                m_OutputQueue.Enqueue(message);
                Monitor.Pulse(m_SyncLock);   // notifica l'unico consumatore
            }

            return true;
        }

        /// <summary>
        /// Rilascia le risorse utilizzate per la comunicazione col servizio remoto di query/reply e richiede
        /// lo shutdown del thread dedicato all'invio dei messaggi. Questo metodo restituisce il controllo al
        /// chiamante non appena tale thread termina effettivamente la propria esecuzione.
        /// </summary>
        public void Dispose()
        {
            Dispose(Timeout.Infinite);
        }

        /// <summary>
        /// Rilascia le risorse utilizzate per la comunicazione col servizio remoto di query/reply e richiede
        /// lo shutdown del thread dedicato all'invio dei messaggi. Questo metodo restituisce il controllo al
        /// chiamante dopo un tempo massimo pari al timeout specificato.
        /// </summary>
        /// <param name="millisecondsTimeout">Il tempo massimo di attesa per il termine di esecuzione del thread.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Il valore di millisecondsTimeout è negativo e non è uguale a Timeout.Infinite in millisecondi.
        /// </exception>
        public void Dispose(int millisecondsTimeout)
        {
            lock (m_SyncLock)
            {
                if (m_InternalProxy != null)
                {
                    ICommunicationObject proxy = m_InternalProxy as ICommunicationObject;

                    try
                    {
                        proxy.Close();
                    }
                    catch
                    {
                        if (proxy.State == CommunicationState.Faulted)
                        {
                            proxy.Abort();
                        }
                    }
                    finally
                    {
                        m_InternalProxy = null;
                        m_ProxyClosed = true;
                    }
                }

                if (!m_Shutdown)
                {
                    m_Shutdown = true;
                    Monitor.Pulse(m_SyncLock);
                }
            }

            m_Dispatcher.Join(millisecondsTimeout);
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Il metodo eseguito nel thread dedicato all'invio dei messaggi verso il servizio remoto di query/reply.
        /// </summary>
        private void Send()
        {
            WriteToLog("The dispatcher for node {0} is running...", m_TargetNodeId);

            while (true)
            {
                MessageData message;

                lock (m_SyncLock)
                {
                    while (!m_Shutdown && m_OutputQueue.Count == 0) Monitor.Wait(m_SyncLock);
                    if (m_Shutdown) break;

                    message = m_OutputQueue.Dequeue();
                }

                DateTime detection;
                if (TrySendMessage(message, out detection))
                {
                    WriteToLog("Message {0} sent to node {1} at {2}.", message.MsgId, m_TargetNodeId, detection);
                }
                else
                {
                    WriteToLog("Unable to send message {0} to node {1}.", message.MsgId, m_TargetNodeId);
                }
            }

            WriteToLog("The dispatcher for node {0} has been stopped.", m_TargetNodeId);
        }

        /// <summary>
        /// Prova ad inviare il messaggio specificato al servizio configurato con la specifica istanza di questa classe.
        /// </summary>
        /// <param name="message">La query o la reply da inviare al servizio di un nodo vicino.</param>
        /// <param name="detection">L'eventuale istante di rilevamento del vicino.</param>
        /// <returns>true se non si sono vericati errori durante l'invio della query; in caso contrario, false.</returns>
        /// <remarks>
        /// Se si verificano errori durante la trasmissione del messaggio al servizio, questo metodo restituisce false
        /// e imposta il valore predefinito di un oggetto DateTime come istante di rilevazione, pertanto il valore non
        /// deve essere considerato. Invece, se il metodo restituisce true, vuol dire che non è avvenuto nessun errore
        /// durante la trasmissione del messaggio e quindi l'istante di rilevazione del servizio è corretto.
        /// </remarks>
        private bool TrySendMessage(MessageData message, out DateTime detection)
        {
            lock (m_SyncLock)
            {
                if (m_ProxyClosed || m_BindingConfig == null || m_RemoteUri == null)
                {
                    detection = default(DateTime);
                    return false;
                }

                try
                {
                    if (m_InternalProxy == null)
                    {
                        ChannelFactory<IQueryReplyService> factory =
                            new ChannelFactory<IQueryReplyService>(m_BindingConfig, new EndpointAddress(m_RemoteUri));

                        m_InternalProxy = factory.CreateChannel();
                    }

                    if (message is QueryData)
                    {
                        WriteToLog("Dispatching query {0} to node {1}...", message.MsgId, m_TargetNodeId);

                        m_InternalProxy.Query(m_NodeId, message as QueryData);
                    }
                    else if (message is ReplyData)
                    {
                        WriteToLog("Dispatching reply {0} to node {1}...", message.MsgId, m_TargetNodeId);

                        m_InternalProxy.Reply(m_NodeId, message as ReplyData);
                    }
                    else
                    {
                        detection = default(DateTime);
                        return false;
                    }

                    detection = DateTime.Now;
                    return true;
                }
                catch
                {
                    if (m_InternalProxy != null)
                    {
                        ICommunicationObject proxy = m_InternalProxy as ICommunicationObject;

                        if (proxy.State == CommunicationState.Faulted)
                        {
                            proxy.Abort();
                            m_InternalProxy = null;
                        }
                    }

                    detection = default(DateTime);
                    return false;
                }
            }
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
