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
        using System.Globalization;
        using System.Windows;
    
        /// <summary>
        /// Class to handle errors.
        /// </summary>
        public static class ErrorHandler
        {
            /// <summary>
            /// Displays error message and exits the installer application.
            /// </summary>
            /// <param name="userFriendlyMessage">User friendly message.</param>
            /// <param name="detailedMessage">Detailed error message.</param>
            /// <param name="logFilePath">Log file path.</param>
            /// <param name="uiMode">Indicates whether installer is running in UI mode or not.</param>
            public static void ShowErrorAndExit(string userFriendlyMessage, string detailedMessage, string logFilePath, bool uiMode)
            {
                InstallerDiagnostics.WriteToEventLog(string.Format(CultureInfo.CurrentCulture, "Please look up execution logs at: {0}", logFilePath));
                InstallerDiagnostics.WriteToEventLog(detailedMessage);
                InstallerDiagnostics.WriteMessageToFile(logFilePath, userFriendlyMessage);
                InstallerDiagnostics.WriteMessageToFile(logFilePath, detailedMessage);
    
                if (!string.IsNullOrEmpty(userFriendlyMessage) && uiMode)
                {
                    MessageBox.Show(userFriendlyMessage);
                }
    
                Environment.Exit(1);
            }
    
            /// <summary>
            /// Displays the exception and exits the installer application.
            /// </summary>
            /// <param name="ex">Exception to be displayed.</param>
            /// <param name="logFilePath">Path to log file.</param>
            /// <param name="uiMode">Whether app is running in UI mode or not.</param>
            public static void ShowExceptionAndExit(Exception ex, string logFilePath, bool uiMode)
            {
                string userFriendlyMessage = Messages.ErrorMessageErrorOccuredContactSupport;
                string detailedMessage = userFriendlyMessage;
    
                if (ex != null)
                {
                    // In case if unpacking fails, should show message box etc.
                    if (!string.IsNullOrEmpty(ex.Message))
                    {
                        userFriendlyMessage += Environment.NewLine + string.Format(CultureInfo.CurrentCulture, Messages.ErrorMessageGeneralError, ex.Message);
                    }
    
                    detailedMessage = ex.ToString();
                }
    
                ErrorHandler.ShowErrorAndExit(userFriendlyMessage, detailedMessage, logFilePath, uiMode);
            }
    
            /// <summary>
            /// Event handler for unhandled exceptions for the application domain.
            /// </summary>
            /// <param name="logFilePath">Path to log file.</param>
            /// <param name="e">Exception event arguments.</param>
            /// <param name="uiMode">Whether app is running in UI mode or not.</param>
            public static void CurrentDomain_UnhandledException(string logFilePath, UnhandledExceptionEventArgs e, bool uiMode)
            {
                if (e != null && e.ExceptionObject != null)
                {
                    Exception ex = e.ExceptionObject as Exception;
                    ErrorHandler.ShowExceptionAndExit(ex, logFilePath, uiMode);
                }
            }
        }
    }
}
