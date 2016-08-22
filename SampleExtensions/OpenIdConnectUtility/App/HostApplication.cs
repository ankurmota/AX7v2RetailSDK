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
    namespace Retail.Tools.OpendIdConnectUtility
    {
        using System;
        using System.Diagnostics;
        using System.Windows;

        /// <summary>
        /// Interaction logic for application.
        /// </summary>
        public partial class HostApplication : Application
        {
            /// <summary>
            /// Number of application parameters when it should use default credentials stored in the app.config.
            /// </summary>
            public const int ParamNumDefaultAccount = 2;

            /// <summary>
            /// Number of application parameters when it should use credentials passed as arguments.
            /// </summary>
            public const int ParamNumCustomAccount = 4;

            /// <summary>
            /// Error code indicating that invalid number of parameters was passed.
            /// </summary>
            public const int ErrorInvalidParameters = -3;

            /// <summary>
            /// Initializes a new instance of the <see cref="HostApplication" /> class.
            /// </summary>
            public HostApplication()
            {
                Dispatcher.UnhandledException += this.Dispatcher_UnhandledException;
                int argsCount = Environment.GetCommandLineArgs().Length;

                // The tool can be called in 2 scenarios:
                // a) by passing just single parameter - path to the file where the token should be saved
                // b) by passing 3 parameters where first one is the path to the file where the token should be saved and last 2 are login and password
                // Note that the name of the exe file is implicitly exists in command arguments as well, therefore number of actual parameters is (a) or (b) above + 1
                if (argsCount != ParamNumDefaultAccount && argsCount != ParamNumCustomAccount)
                {
                    Environment.Exit(ErrorInvalidParameters);
                }
            }

            /// <summary>
            /// Entry point for the application.
            /// </summary>
            [STAThread]
            private static void Main()
            {
                HostApplication app = new HostApplication();
                app.Run(new MainWindow());
            }

            private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
            {
                Trace.TraceError(e.Exception.Message);
            }
        }
    }
}