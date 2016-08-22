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
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.Diagnostics;
        using System.Globalization;
        using System.IO;
        using System.Linq;
        using System.Reflection;
        using System.Runtime.ExceptionServices;
        using Microsoft.Win32;
    
        /// <summary>
        /// Utility class for common helper functions.
        /// </summary>
        public static class Utilities
        {
            /// <summary>
            /// Runs process for given path. Waits until process exits.
            /// </summary>
            /// <param name="exePath">Path to an .exe to run.</param>
            /// <param name="arguments">Arguments to pass to process.</param>
            /// <param name="createNoWindow">Flag indicating whether process should not be run in new window.</param>
            /// <returns>Exit code that process returned.</returns>
            public static int RunProcess(string exePath, string arguments, bool createNoWindow)
            {
                if (string.IsNullOrEmpty(exePath))
                {
                    throw new ArgumentNullException("exePath");
                }
    
                if (!File.Exists(exePath))
                {
                    throw new FileNotFoundException(exePath);
                }
    
                string processDirectory = Directory.GetParent(exePath).FullName;
    
                ProcessStartInfo appProcInfo = new ProcessStartInfo();
                appProcInfo.FileName = exePath;
                appProcInfo.Arguments = arguments;
                appProcInfo.UseShellExecute = false;
                appProcInfo.CreateNoWindow = createNoWindow;
    
                if (!string.IsNullOrEmpty(processDirectory))
                {
                    appProcInfo.WorkingDirectory = processDirectory;
                }
    
                Process appProc = Process.Start(appProcInfo);
                appProc.WaitForExit();
                int exitCode = appProc.ExitCode;
                return exitCode;
            }
    
            /// <summary>
            /// Runs the installer application asynchronously and exits when completed.
            /// </summary>
            /// <param name="exePath">Path to setup file.</param>
            /// <param name="args">Arguments to setup.</param>
            public static void RunProcessAsyncAndExitWhenCompleted(string exePath, string[] args)
            {
                if (args == null)
                {
                    throw new ArgumentNullException("args");
                }
    
                args = args.Select(arg => arg.Trim().Contains("\"") ? arg : string.Format(CultureInfo.InvariantCulture, "\"{0}\"", arg)).ToArray();
    
                string arguments = string.Join(" ", args);
    
                BackgroundWorker worker = new BackgroundWorker();
                int exitCode = 0;
                worker.DoWork += delegate
                {
                    // This function will be executed in the context of worker thread
                    exitCode = Utilities.RunProcess(exePath, arguments, false);
                };
                worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    Exception ex = e.Error;
                    (sender as BackgroundWorker).Dispose();
    
                    if (ex == null)
                    {
                        Environment.Exit(exitCode);
                    }
                    else
                    {
                        // This exception will be handled by global Unhandled Dispatcher Exceptions Handler
                        // Note: we want to rethrow exception without losing it's stack trace.
                        // Therefore we need this logic.
                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                };

                worker.RunWorkerAsync();
            }
    
            /// <summary>
            /// Validates whether .NET 4.5 is installed.
            /// </summary>
            /// <param name="logFilePath">Path to log file.</param>
            /// <param name="uiMode">Whether app is running in UI mode or not.</param>
            public static void ValidateNet45prereq(string logFilePath, bool uiMode)
            {
                // Check function is created based on following MSDN article:
                // https://msdn.microsoft.com/en-us/library/hh925568%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
                const int Net451ReleaseVersion = 378675;
    
                int actualNetReleaseVersion = GetNet45ReleaseVersion();
                if (actualNetReleaseVersion < Net451ReleaseVersion)
                {
                    ErrorHandler.ShowErrorAndExit(Messages.NetPrerequisiteMissingMessage, string.Empty, logFilePath, uiMode);
                }
            }
    
            /// <summary>
            /// Checks if another setup instance is running.
            /// </summary>
            /// <param name="currentProcess">Current installer process.</param>
            /// <param name="logFilePath">Path to log file.</param>
            /// <param name="uiMode">Whether app is running in UI mode or not.</param>
            public static void ValidateNoProcessWithSameNameIsRunning(Process currentProcess, string logFilePath, bool uiMode)
            {
                // Check if another instance of this setup is already running.
                Process anotherSetupProcess = Process.GetProcesses()
                    .Where(
                        (process) => process.Id != currentProcess.Id &&
                        !Path.GetExtension(process.ProcessName).Equals(".vshost", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(process.ProcessName).Equals(Path.GetFileNameWithoutExtension(currentProcess.ProcessName), StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
    
                if (anotherSetupProcess != null)
                {
                    ErrorHandler.ShowErrorAndExit(Messages.ErrorMessageAnotherInstanceAlreadyRunning, string.Empty, logFilePath, uiMode);
                }
            }
    
            /// <summary>
            /// Gets resource extraction path.
            /// </summary>
            /// <param name="assembly">Executing assembly.</param>
            /// <returns>Path to resource file.</returns>
            public static string GetResourceExtractionDirectoryPath(Assembly assembly)
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException("assembly");
                }
    
                string contentDirectoryName = string.Format(CultureInfo.InvariantCulture, "{0}.content", assembly.GetName().Name);
                string result = Path.Combine(Path.GetTempPath(), contentDirectoryName);
                return result;
            }
    
            /// <summary>
            /// Checks if the given parameter exists in the command line arguments.
            /// </summary>
            /// <param name="args">Command line arguments.</param>
            /// <param name="paramName">Parameter name to check.</param>
            /// <returns>True if the parameter exists, false otherwise.</returns>
            public static bool CheckIfParameterExists(string[] args, string paramName)
            {
                if (args == null)
                {
                    throw new ArgumentNullException("args");
                }
    
                return args.Where(arg => arg.Trim().Equals(paramName, StringComparison.OrdinalIgnoreCase)).Any();
            }
    
            /// <summary>
            /// Append parameter if it does not exist.
            /// </summary>
            /// <param name="args">Command line parameters.</param>
            /// <param name="paramName">Parameter name to set.</param>
            /// <param name="paramValue">Parameter value to set.</param>
            /// <returns>Command line arguments.</returns>
            public static string[] GetArgsWithNonExistingParameterAppended(string[] args, string paramName, string paramValue)
            {
                if (args == null)
                {
                    throw new ArgumentNullException("args");
                }
    
                if (!Utilities.CheckIfParameterExists(args, paramName))
                {
                    if (!string.IsNullOrWhiteSpace(paramValue))
                    {
                        List<string> argsList = new List<string>(args);
                        argsList.Add(paramName);
                        argsList.Add(paramValue);
    
                        args = argsList.ToArray();
                    }
                }
    
                return args;
            }
    
            /// <summary>
            /// Gets the path to customized files directory.
            /// </summary>
            /// <param name="contentDirectoryPath">Path to the content directory.</param>
            /// <returns>Path to the customized files directory.</returns>
            public static string GetCustomizedFilesPath(string contentDirectoryPath)
            {
                string expectedPath = Path.Combine(contentDirectoryPath, "CustomizedFiles");
    
                return Directory.Exists(expectedPath) ? expectedPath : null;
            }

            /// <summary>
            /// Gets path to a settings file that is expected to be side by side with installer.
            /// </summary>
            /// <param name="bootStrapperFilePath">Full path to entry setup .exe file.</param>
            /// <param name="progressMessageLogger">Delegate to log progress messages.</param>
            /// <returns>Path to settings file if it exists, or null if it does not.</returns>
            public static string GetSettingsFilePathIfExists(string bootStrapperFilePath, Action<string> progressMessageLogger)
            {
                if (string.IsNullOrWhiteSpace(bootStrapperFilePath))
                {
                    throw new ArgumentNullException("bootStrapperFilePath");
                }

                if (progressMessageLogger == null)
                {
                    throw new ArgumentNullException("progressMessageLogger");
                }

                if (!File.Exists(bootStrapperFilePath))
                {
                    string errorMessage = string.Format(CultureInfo.InvariantCulture, Messages.ErrorMessageFileDoesNotExist, bootStrapperFilePath);
                    throw new ArgumentException(errorMessage, "bootStrapperFilePath");
                }

                // Get all files like bootsraperfilename*.xml from bootstrapper directory.
                string directoryPath = Path.GetDirectoryName(bootStrapperFilePath);
                string bootStrapperFileNameNoExtension = Path.GetFileNameWithoutExtension(bootStrapperFilePath);
                string targetConfigFileNameMask = string.Format(CultureInfo.InvariantCulture, "{0}*.xml", bootStrapperFileNameNoExtension);

                progressMessageLogger.Invoke(string.Format(
                    CultureInfo.CurrentCulture, Messages.ProgressMessageSearchingForSettingsFilesInDirectory, targetConfigFileNameMask, directoryPath));

                string[] configFilePaths = Directory.GetFiles(directoryPath, targetConfigFileNameMask, SearchOption.TopDirectoryOnly);

                // Get first file with latest Creation time UTC if any.
                string result = null;
                if (configFilePaths != null && configFilePaths.Length > 0)
                {
                    string filesFoundListMessage = Environment.NewLine + string.Join(Environment.NewLine, configFilePaths);
                    progressMessageLogger.Invoke(string.Format(
                        CultureInfo.CurrentCulture, Messages.ProgressMessageFoundXSettingsFiles, configFilePaths.Length, filesFoundListMessage));

                    progressMessageLogger.Invoke(Messages.ProgressMessagePickingFileWithLatestCreationTime);
                    result = configFilePaths.OrderByDescending((string filePath) => new FileInfo(filePath).CreationTimeUtc.Ticks).FirstOrDefault();
                    progressMessageLogger.Invoke(string.Format(CultureInfo.CurrentCulture, Messages.ProgressMessageSettingsFilePickedIs, result));
                }
                else
                {
                    progressMessageLogger.Invoke(Messages.ProgressMessageNoSettingsFilesFound);
                }

                return result;
            }

            private static int GetNet45ReleaseVersion()
            {
                const string Net45VersionKeyPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
                const string ReleasePropertyName = "Release";
    
                int result = 0;
                using (RegistryKey netVersionKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(Net45VersionKeyPath))
                {
                    if (netVersionKey != null)
                    {
                        object releaseValue = netVersionKey.GetValue(ReleasePropertyName);
                        if (releaseValue != null)
                        {
                            int.TryParse(releaseValue.ToString(), out result);
                        }
                    }
                }
    
                return result;
            }
        }
    }
}
