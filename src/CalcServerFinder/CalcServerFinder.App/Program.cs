using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;

using CalcServerFinder.Configuration;
using CalcServerFinder.Logging;
using CalcServerFinder.Contracts;
using CalcServerFinder.Services;
using CalcServerFinder.Core;
using CalcServerFinder.Networking;

namespace CalcServerFinder.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger appLogger = Logger.Instance;

            if (VerifyConfigurationStatus() && ConfigureLogger())
            {
                ResourceCacheUpdater rcu = new ResourceCacheUpdater();
                rcu.Run();

                SearchManager sm = SearchManager.Instance;
                sm.Run();

                CommunicationHandler ch = CommunicationHandler.Instance;
                ch.Run();

                NeighborhoodManager nm = NeighborhoodManager.Instance;
                nm.Run();
                                
                ServiceHost sh1 = null;
                ServiceHost sh2 = null;

                try
                {
                    sh1 = new ServiceHost(typeof(ProcessingServiceFinder));
                    sh1.Open();

                    Console.WriteLine("The service ProcessingServiceFinder is ready.");
                    Console.WriteLine("Press <Enter> to stop the service...");

                    sh2 = new ServiceHost(typeof(QueryReplyService));
                    sh2.Open();

                    Console.WriteLine("The service QueryReplyService is ready.");
                    Console.WriteLine("Press <Enter> to stop the service...");

                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while starting the service: {0}", e.ToString());
                    Console.WriteLine();
                }
                finally
                {
                    if (sh1 != null)
                    {
                        try
                        {
                            sh1.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error while stopping the service: {0}", e.ToString());
                            Console.WriteLine();
                            sh1.Abort();
                        }
                    }

                    if (sh2 != null)
                    {
                        try
                        {
                            sh2.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error while stopping the service: {0}", e.ToString());
                            Console.WriteLine();
                            sh2.Abort();
                        }
                    }
                }

                rcu.Dispose(TimeSpan.FromSeconds(3));
                sm.Dispose(TimeSpan.FromSeconds(3));
                ch.Dispose(TimeSpan.FromSeconds(3));
                nm.Dispose(TimeSpan.FromSeconds(3));
            }

            //appLogger.Flush();
            //appLogger.Cleanup();
            
            Console.WriteLine("Press <Enter> to close the application...");
            Console.ReadLine();
        }

        /// <summary>
        /// Verifica lo stato della configurazione e restituisce true se non sono avvenuti errori durante la fase
        /// di caricamento della configurazione, altrimenti, mostra i dettagli relativi agli errori e restituisce
        /// false.
        /// </summary>
        /// <returns>true se la configurazione è stata caricata correttamente; in caso contrario, false.</returns>
        static bool VerifyConfigurationStatus()
        {
            Settings settings = Settings.Instance;

            if (settings.HasErrors)
            {
                foreach (var message in settings.Errors)
                {
                    Console.WriteLine("----------");
                    Console.WriteLine(message);
                    Console.WriteLine();
                }

                return false;
            }
            else
            {
                foreach (var uri in settings.ResourceMonitorEndpoints)
                {
                    Console.WriteLine("processing server = {0}", uri);
                }
                Console.WriteLine("----------");

                Console.WriteLine("NodeId = {0} --> current node", settings.NodeId);
                foreach (var neighbor in settings.NeighborsInfo)
                {
                    Console.WriteLine("NodeId = {0}, URI = {1}", neighbor.Key, neighbor.Value);
                }
                Console.WriteLine("----------");

                return true;
            }
        }

        /// <summary>
        /// Prova a configurare il gestore delle operazioni di logging, creando la cartella in cui verrà memorizzato
        /// il nuovo file di log e un nuovo file di testo in cui verrà conservato il log delle operazioni di logging
        /// e restituendo true se non si verificano errori durante la procedura. In caso contrario viene mostrato un
        /// errore esplicativo e restituisce false.
        /// </summary>
        /// <returns>true se non si verificano errori durante la configurazione del logger, altrimenti false</returns>
        static bool ConfigureLogger()
        {
            /*Settings appConfig = Settings.Instance;
            Logger appLogger = Logger.Instance;

            string logFileName = string.Format("{0:yyyyMMdd_HHmmss}.log", DateTime.Now);

            try
            {
                FileLogHandler handler = new FileLogHandler(Path.Combine(appConfig.LogsFolder, logFileName));
                handler.LogBufferMaxSize = 10;
                appLogger.Add(handler);
            }
            catch (Exception e)
            {
                Console.WriteLine("Errore durante la configurazione del logger: {0}", e.ToString());
                Console.WriteLine();
                return false;
            }*/

            return true;
        }

    }
}
