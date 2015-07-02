using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CalcServerFinder.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface IProcessingServiceFinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options">i parametri utili a configurare le opzioni di ricerca dei servizi di calcolo</param>
        /// <param name="addresses">gli indirizzi dei servizi di calcolo compatibili con le opzioni di ricerca</param>
        [OperationContract]
        void Search(SearchOptions options, out List<Uri> addresses);
    }
}
