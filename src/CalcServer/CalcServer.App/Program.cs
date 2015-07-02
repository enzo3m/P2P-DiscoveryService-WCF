using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.ServiceModel;

using CalcServer.Configuration;
using CalcServer.Logging;
using CalcServer.Services;
using CalcServer.TaskProcessing;
using CalcServer.Toolboxes;

namespace CalcServer.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger appLogger = Logger.Instance;

            if (ReadResourcesList() && ConfigureLogger() && CreateWorkingFolders())
            {
                StatisticsToolbox statisticsToolbox = new StatisticsToolbox();
                MathToolbox mathToolbox = new MathToolbox();

                TaskPerformerContextManager provider = new TaskPerformerContextManager();
                provider.Add(statisticsToolbox);
                provider.Add(mathToolbox);

                TaskProcessingManager manager = TaskProcessingManager.Instance;
                manager.SetContextProvider(provider);

                ServiceHost host = null;
                try
                {
                    host = new ServiceHost(typeof(ProcessingService));
                    host.Open();

                    Console.WriteLine("The service ProcessingService is ready.");
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
                    if (host != null)
                    {
                        try
                        {
                            host.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error while stopping the service: {0}", e.ToString());
                            Console.WriteLine();
                            host.Abort();
                        }
                    }
                }
            }

            appLogger.Flush();
            appLogger.Cleanup();

            Console.WriteLine("Press <Enter> to close the application...");
            Console.ReadLine();
        }

        /// <summary>
        /// Prova a caricare l'elenco delle risorse (nome e versione) da attivare in questo server e restituisce
        /// true se non si verificano errori durante la lettura di tale elenco. In caso contrario viene mostrato
        /// un errore esplicativo e restituisce false.
        /// </summary>
        /// <returns>true se l'elenco delle risorse viene caricato correttamente, altrimenti false</returns>
        static bool ReadResourcesList()
        {
            Settings appConfig = Settings.Instance;

            try
            {
                string[] lines = File.ReadAllLines("Resources.txt");
                foreach (var line in lines)
                {
                    string resource = line.Trim();
                    if (!string.IsNullOrWhiteSpace(resource))
                    {
                        if (appConfig.InsertResource(resource))
                        {
                            Console.WriteLine(resource);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while loading the configuration: {0}", e.ToString());
                Console.WriteLine();
                return false;
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
            Settings appConfig = Settings.Instance;
            Logger appLogger = Logger.Instance;
            
            string logFileName = string.Format("{0:yyyyMMdd_HHmmss}.log", DateTime.Now);

            try
            {
                FileLogHandler handler = new FileLogHandler(Path.Combine(appConfig.LogsFolder, logFileName));
                handler.LogBufferMaxSize = 10;
                appLogger.Add(handler);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during configuration of the logger: {0}", e.ToString());
                Console.WriteLine();
                return false;
            }
        }

        /// <summary>
        /// Prova a creare le cartelle di lavoro necessarie per la memorizzazione dei dati in arrivo e dei risultati
        /// di elaborazione, restituendo true se sono state create correttamente. In caso contrario, se la procedura
        /// di creazione delle cartelle dovesse riscontrare qualche problema, viene mostrato un errore esplicativo e
        /// il metodo restituisce false.
        /// </summary>
        /// <returns>true se le cartelle di lavoro vengono create correttamente, altrimenti false</returns>
        static bool CreateWorkingFolders()
        {
            Settings appConfig = Settings.Instance;
            return CreateFolder(appConfig.ReadyTasksFolder) && CreateFolder(appConfig.CompletedTasksFolder);
        }

        /// <summary>
        /// Prova a creare la cartella sul percorso specificato, restituendo true se è stata creata correttamente
        /// o se esiste già e quindi non occorre crearla. In caso contrario, viene mostrato un errore esplicativo
        /// e il metodo restituisce false.
        /// </summary>
        /// <param name="path">il percorso completo o relativo della cartella da creare</param>
        /// <returns>true se la cartella è stata creata o se esiste già, altrimenti false</returns>
        static bool CreateFolder(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while creating the folder: {0}", e.ToString());
                    Console.WriteLine();
                    return false;
                }
            }
        }
    }
}
