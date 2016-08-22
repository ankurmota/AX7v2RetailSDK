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
        /// <summary>
        /// Class for all constants.
        /// </summary>
        public static class Messages
        {
            /// <summary>
            /// Error message indicating another instance of installer is already running.
            /// </summary>
            public const string ErrorMessageAnotherInstanceAlreadyRunning = "Another instance of this setup is already running. Please close the other instance and then try again.";
    
            /// <summary>
            /// Error message indicating .NET pre-requisite is missing.
            /// </summary>
            public const string NetPrerequisiteMissingMessage = "Cannot find a supported version of the .NET. This product requires that the .NET Framework be installed on the local computer. The supported version of the .NET Framework is available here: http://go.microsoft.com/fwlink/?LinkId=609053";
    
            /// <summary>
            /// Error message indicating generic message.
            /// </summary>
            public const string ErrorMessageGeneralError = "Error: {0}";
    
            /// <summary>
            /// Error message indicating content unpack failure.
            /// </summary>
            public const string ErrorMessageFailedToUnpackContent = "Failed to unpack content. Please see log {0} for more details.";
    
            /// <summary>
            /// Error message indicating generic error.
            /// </summary>
            public const string ErrorMessageErrorOccuredContactSupport = "Installer application encountered an error and needs to exit. Please try again or contact technical support.";
    
            /// <summary>
            /// Message indicating to look up execution logs for details.
            /// </summary>
            public const string MessageInspectExecutionLogs = "Please look up execution logs at: {0}";
    
            /// <summary>
            /// Message indication module resolution.
            /// </summary>
            public const string LogMessageResolvingModule = "Resolving module {0}";
    
            /// <summary>
            /// Message indicating main setup is running.
            /// </summary>
            public const string ProgressMessageRunningMainSetupApp = "Running main setup application...";
    
            /// <summary>
            /// Message indicating that content is being unpacked.
            /// </summary>
            public const string ProgressMessageUnpackingContent = "Unpacking content...";
    
            /// <summary>
            /// Message indicating that resources are being unpacked.
            /// </summary>
            public const string ProgressMessageUnpackingResources = "Unpacking resources...";
    
            /// <summary>
            /// Message indicating that .NET 4.5 installation is being validated.
            /// </summary>
            public const string ProgressMessageValidatingNet45Installed = "Validating that .NET 4.5 is installed...";

            /// <summary>
            /// Message for searching settings files in directory.
            /// </summary>
            public const string ProgressMessageSearchingForSettingsFilesInDirectory = "Searching for settings files {0} in directory '{1}'";

            /// <summary>
            /// Message when no settings files were found.
            /// </summary>
            public const string ProgressMessageNoSettingsFilesFound = "No settings files found.";

            /// <summary>
            /// Message when settings files were found.
            /// </summary>
            public const string ProgressMessageFoundXSettingsFiles = "Found {0} settings files: {1}";

            /// <summary>
            /// Message when picking file with latest creation time.
            /// </summary>
            public const string ProgressMessagePickingFileWithLatestCreationTime = "Picking file with latest creation time.";

            /// <summary>
            /// Message when picked settings file.
            /// </summary>
            public const string ProgressMessageSettingsFilePickedIs = "Settings file picked is '{0}'";

            /// <summary>
            /// Message when file does not exist.
            /// </summary>
            public const string ErrorMessageFileDoesNotExist = "File '{0}' does not exist.";
        }
    }
}
