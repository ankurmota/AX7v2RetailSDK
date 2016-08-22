/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Retail.Deployment.SelfService.BootstrapperLibrary
    {
        using System;
        using System.Diagnostics;
        using System.Globalization;
        using System.IO;
        using System.Reflection;

        /// <summary>
        /// Diagnostics class for self service packages \ zipper.
        /// </summary>
        public static class InstallerDiagnostics
        {
            private const string EventLogName = "Application";
            private const string EventSourceName = "Microsoft Dynamics AX Retail: Self Service Deployment";
            private const string RetailLogsFolderName = "RetailLogs";

            /// <summary>
            /// Logs the message to file.
            /// </summary>
            /// <param name="message">Message to log.</param>
            /// <param name="logFilePath">Path to log file.</param>
            public static void LogProgressMessage(string message, string logFilePath)
            {
                InstallerDiagnostics.WriteMessageToFile(logFilePath, message);
            }
    
            /// <summary>
            /// Writes message to self-service event log.
            /// </summary>
            /// <param name="message">Message to write.</param>
            public static void WriteToEventLog(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        CreateSelfServiceEventSource();
                        EventLog.WriteEntry(EventSourceName, message, EventLogEntryType.Error);
                    }
                    catch
                    {
                    }
                }
            }
    
            /// <summary>
            /// Writes message to given file.
            /// </summary>
            /// <param name="logFilePath">File path.</param>
            /// <param name="message">Message format to write.</param>
            /// <param name="args">Message format arguments.</param>
            public static void WriteMessageToFile(string logFilePath, string message, params object[] args)
            {
                if (message != null && args != null && args.Length != 0)
                {
                    message = string.Format(CultureInfo.CurrentCulture, message, args);
                }
    
                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        message = DateTime.UtcNow.ToString() + ": " + message;
                        File.AppendAllText(logFilePath, message);
                        File.AppendAllText(logFilePath, Environment.NewLine);
                    }
                    catch
                    {
                    }
                }
            }
    
            /// <summary>
            /// Creates self-service event source.
            /// </summary>
            public static void CreateSelfServiceEventSource()
            {
                try
                {
                    if (!EventLog.SourceExists(EventSourceName))
                    {
                        EventLog.CreateEventSource(EventSourceName, EventLogName);
                    }
                }
                catch
                {
                }
            }
    
            /// <summary>
            /// Gets the installer executable log file path.
            /// </summary>
            /// <param name="logDirectoryPath">Path to the log directory.</param>
            /// <param name="assembly">Executing assembly.</param>
            /// <returns>Path to the installer executable log file.</returns>
            public static string GetExeLogFilePath(string logDirectoryPath, Assembly assembly)
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException("assembly");
                }
    
                if (string.IsNullOrEmpty(logDirectoryPath))
                {
                    throw new ArgumentNullException("logDirectoryPath");
                }

                string assemblyFileName = Path.GetFileName(assembly.Location);
                string assemblyLogFileName = Path.ChangeExtension(assemblyFileName, ".log");
                string result = Path.Combine(Path.GetTempPath(), RetailLogsFolderName, assemblyLogFileName);
                return result;
            }
        }
    }
}
