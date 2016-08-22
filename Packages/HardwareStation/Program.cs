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
    namespace Retail.Deployment.PackageSetup
    {
        using System;
        using System.Diagnostics;
        using System.IO;
        using System.Reflection;
        using System.Windows;
        using Retail.Deployment.SelfService.BootstrapperLibrary;

        /// <summary>
        /// Entry point of an application.
        /// </summary>
        public static class Program
        {
            private const string AssemblyRootNamespace = "Microsoft.Dynamics.Retail.Deployment.PackageSetup";
            private const string NoUIArg = "-q";
            private const string CustomizedFilesArgument = "-CustomizedFilesPath";
            private const string BootStrapperVersionArgument = "-BootStrapperVersion";
            private const string SettingsArgument = "-settings";
            private static bool uiMode = true;
            private static Action<string> progressMessageLogger;

            /// <summary>
            /// Entry point method.
            /// </summary>
            /// <param name="args">Process arguments.</param>
            [STAThread]
            public static void Main(string[] args)
            {
                // Note: Main should not contain any method calls from referenced assemblies.
                Assembly assembly = Assembly.GetExecutingAssembly();

                // Resolve the embedded assembly.
                AppDomain.CurrentDomain.AssemblyResolve +=
                    (object sender, ResolveEventArgs eventArgs) => ResolveEmbeddedAssembly(eventArgs, assembly);

                // Execute the installer. 
                // We need a separate function here because we need to resolve the embedded assembly first and then use the methods it provides.
                Execute(args, assembly);
            }

            private static void Execute(string[] args, Assembly assembly)
            {
                string extractionDirectoryPath = Utilities.GetResourceExtractionDirectoryPath(assembly);
                string logFilePath = InstallerDiagnostics.GetExeLogFilePath(extractionDirectoryPath, assembly);
                progressMessageLogger = (string message) => InstallerDiagnostics.LogProgressMessage(message, logFilePath);

                uiMode = GetUImode(args);
                AppDomain.CurrentDomain.UnhandledException +=
                    (object sender, UnhandledExceptionEventArgs e) => ErrorHandler.CurrentDomain_UnhandledException(logFilePath, e, uiMode);

                InstallerDiagnostics.CreateSelfServiceEventSource();
                Utilities.ValidateNoProcessWithSameNameIsRunning(Process.GetCurrentProcess(), logFilePath, uiMode);

                ContentExtractor contentExtractor = new ContentExtractor(assembly, AssemblyRootNamespace, extractionDirectoryPath);

                UIHelper.ShowMainView(uiMode);

                InstallerDiagnostics.LogProgressMessage(Messages.ProgressMessageUnpackingResources, logFilePath);
                contentExtractor.UnpackResources();

                InstallerDiagnostics.LogProgressMessage(Messages.ProgressMessageValidatingNet45Installed, logFilePath);
                Utilities.ValidateNet45prereq(logFilePath, uiMode);

                InstallerDiagnostics.LogProgressMessage(Messages.ProgressMessageUnpackingContent, logFilePath);
                contentExtractor.UnzipContent();

                UIHelper.HideMainView();

                args = GetUpdatedArgsForSetup(args, contentExtractor, assembly);

                InstallerDiagnostics.LogProgressMessage(Messages.ProgressMessageRunningMainSetupApp, logFilePath);
                Utilities.RunProcessAsyncAndExitWhenCompleted(contentExtractor.GetSetupExePath(), args);

                // We need this to begin running a standard application message loop on the main thread.
                // So that main thread does not exit and waits until setup run finishes.
                Application application = new Application();
                application.Run();
            }

            private static string[] GetUpdatedArgsForSetup(string[] args, ContentExtractor contentExtractor, Assembly assembly)
            {
                // Customized file path.
                string customizedFilesPath = Utilities.GetCustomizedFilesPath(contentExtractor.GetContentExtractionPath());
                args = Utilities.GetArgsWithNonExistingParameterAppended(args, CustomizedFilesArgument, customizedFilesPath);

                // Setup bootstrapper version.
                string bootStrapperVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
                args = Utilities.GetArgsWithNonExistingParameterAppended(args, BootStrapperVersionArgument, bootStrapperVersion);

                // Try to lookup settings file that was downloaded from AX and put side by side with bootstrapper.
                string configurationSettingsFilePath = Utilities.GetSettingsFilePathIfExists(assembly.Location, progressMessageLogger);
                if (!string.IsNullOrWhiteSpace(configurationSettingsFilePath))
                {
                    args = Utilities.GetArgsWithNonExistingParameterAppended(args, SettingsArgument, configurationSettingsFilePath);
                }

                return args;
            }

            private static Assembly ResolveEmbeddedAssembly(ResolveEventArgs args, Assembly executingAssembly)
            {
                if (args == null)
                {
                    throw new ArgumentNullException("args");
                }

                if (executingAssembly == null)
                {
                    throw new ArgumentNullException("executingAssembly");
                }

                // InstallerDiagnostics.WriteMessageToFile(logFilePath, Constants.LogMessageResolvingModule, args.Name);

                // Get .dll name that application tries to lookup
                string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name;
                dllName = dllName.Replace(".dll", string.Empty) + ".dll";

                // Try fetch embedded resource name of the .dll which is typically <Root namespace>.<Dll name>
                string foundEmbeddedResourceName = null;
                foreach (string embeddedResourceName in executingAssembly.GetManifestResourceNames())
                {
                    if (embeddedResourceName.EndsWith(dllName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundEmbeddedResourceName = embeddedResourceName;
                        break;
                    }
                }

                Assembly result = null;
                if (!string.IsNullOrEmpty(foundEmbeddedResourceName))
                {
                    using (Stream embeddedResource = executingAssembly.GetManifestResourceStream(foundEmbeddedResourceName))
                    {
                        if (embeddedResource != null)
                        {
                            byte[] assemblyBytes = new byte[embeddedResource.Length];
                            embeddedResource.Read(assemblyBytes, 0, assemblyBytes.Length);
                            result = Assembly.Load(assemblyBytes);
                        }
                    }
                }

                return result;
            }

            private static bool GetUImode(string[] args)
            {
                return !Utilities.CheckIfParameterExists(args, NoUIArg);
            }
        }
    }
}
