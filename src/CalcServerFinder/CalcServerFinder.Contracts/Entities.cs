using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CalcServerFinder.Contracts
{
    #region SearchOptions

    /// <summary>
    /// Conserva le opzioni di ricerca inviate dai client per definire i parametri di ricerca di una risorsa di calcolo.
    /// </summary>
    [DataContract]
    public class SearchOptions
    {
        /// <summary>
        /// Il nome completo della classe che implementa la risorsa di calcolo cercata.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// La versione della classe che implementa la risorsa di calcolo cercata.
        /// </summary>
        [DataMember]
        public string Version { get; set; }
    }

    #endregion

    #region MessageData

    /// <summary>
    /// Questa classe astratta contiene i campi che devono essere presenti in tutti le tipologie di messaggi scambiati
    /// tra i vari nodi della rete e necessari affinché possano essere instradati correttamente.
    /// </summary>
    [DataContract]
    public abstract class MessageData
    {
        /// <summary>
        /// L'identificatore univoco del messaggio.
        /// </summary>
        [DataMember]
        public Guid MsgId { get; private set; }

        /// <summary>
        /// Il TTL relativo al messaggio.
        /// </summary>
        [DataMember]
        public byte TimeToLive { get; set; }

        /// <summary>
        /// Il numero di hops relativo al messaggio.
        /// </summary>
        [DataMember]
        public byte HopsCount { get; set; }

        /// <summary>
        /// Il costruttore invocato dalle sottoclassi per istanziare un'istanza di una specifica classe.
        /// </summary>
        /// <param name="msgid">L'identificatore univoco per il messaggio.</param>
        /// <param name="ttl">Il time-to-live relativo al messaggio.</param>
        /// <param name="hops">Il numero di hops del messaggio.</param>
        public MessageData(Guid msgid, Byte ttl, Byte hops)
        {
            MsgId = msgid;
            TimeToLive = ttl;
            HopsCount = hops;
        }
    }

    #endregion

    #region QueryData

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class QueryData : MessageData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgid"></param>
        /// <param name="ttl"></param>
        /// <param name="hops"></param>
        public QueryData(Guid msgid, Byte ttl, Byte hops) : base(msgid, ttl, hops) { ; }

        /// <summary>
        /// I parametri di ricerca necessari per interrogare un peer della rete.
        /// </summary>
        [DataMember]
        public SearchOptions Options { get; set; }
    }

    #endregion

    #region ReplyData

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class ReplyData : MessageData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgid"></param>
        /// <param name="ttl"></param>
        /// <param name="hops"></param>
        public ReplyData(Guid msgid, Byte ttl, Byte hops) : base(msgid, ttl, hops) { ; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<Uri> FoundServices { get; set; }
    }

    #endregion
}
