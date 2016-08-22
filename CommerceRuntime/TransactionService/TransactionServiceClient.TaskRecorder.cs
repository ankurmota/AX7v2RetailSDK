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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.IO;
        using System.Linq;
        using System.Runtime.Serialization;
        using System.Text;
        using System.Xml;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using CRT = Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Transaction Service Commerce Runtime Client APIs for task recorder.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string GetStorageAccessTokenForUploadMethodName = "GetStorageAccessTokenForUpload";
            private const string UploadRecordingMethodName = "UploadRecording";
            private const string GenerateRecordingFileMethodName = "GenerateRecordingFile";
            private const string GenerateTrainingDocumentMethodName = "GenerateTrainingDocument";
            private const string GenerateBusinessProcessModelPackageMethodName = "GenerateBusinessProcessModelPackage";
            private const string GenerateRecordingBundleMethodName = "GenerateRecordingBundle";
            private const string DownloadRecordingMethodName = "DownloadRecording";
            private const string GetBusinessProcessModelLibrariesMethodName = "GetBusinessProcessModelLibraries";
            private const string GetBusinessProcessModelLibraryMethodName = "GetBusinessProcessModelLibrary";
            private const string SearchTaskGuidesByTitleMethodName = "SearchTaskGuidesByTitle";
            private const string LoadRecordingFromFileMethodName = "LoadRecordingFromFile";

            // Storage access token constants.
            private const int StorageAccessTokenSize = 2;
            private const int UrlIndex = 0;
            private const int SasKeyIndex = 1;

            /// <summary>
            /// Gets the storage access token for upload.
            /// </summary>
            /// <returns>The storage access token.</returns>
            public StorageAccessToken GetStorageAccessTokenForUpload()
            {
                ReadOnlyCollection<object> storageAccessData = this.InvokeMethod(GetStorageAccessTokenForUploadMethodName);
                if (storageAccessData == null || storageAccessData.Count < StorageAccessTokenSize)
                {
                    throw new CRT.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                return GetStorageAccessToken(storageAccessData);
            }

            /// <summary>
            /// Uploads the recording to LCS.
            /// </summary>
            /// <param name="recording">The recording.</param>
            /// <param name="bpmLineId">The business process model line identifier.</param>
            public void UploadRecording(Recording recording, int bpmLineId)
            {
                ThrowIf.Null(recording, "recording");
                ThrowIf.Null(recording.RootScope, "recording.RootScope");

                string serializedRecording = SerializeRecordingForUpload(recording);
                object[] parameters =
                    {
                    serializedRecording,
                    bpmLineId,
                };
                this.InvokeMethodNoDataReturn(UploadRecordingMethodName, parameters);
            }

            /// <summary>
            /// Serializes the recording object and uploads it to Azure blob storage.
            /// </summary>
            /// <param name="recording">The recording.</param>
            /// <returns>
            /// The URL to download the serialized recording.
            /// </returns>
            public string GenerateRecordingFile(Recording recording)
            {
                ThrowIf.Null(recording, "recording");
                ThrowIf.Null(recording.RootScope, "recording.RootScope");

                string serializedRecording = SerializeRecordingForUpload(recording);
                object[] parameters =
                    {
                    serializedRecording,
                    recording.Name
                };
                var result = this.InvokeMethod(GenerateRecordingFileMethodName, parameters);

                return result.FirstOrDefault().ToString();
            }

            /// <summary>
            /// Generates a training document.
            /// </summary>
            /// <param name="recording">The recording.</param>
            /// <returns>
            /// The URL to download the training document.
            /// </returns>
            public string GenerateTrainingDocument(Recording recording)
            {
                ThrowIf.Null(recording, "recording");
                ThrowIf.Null(recording.RootScope, "recording.RootScope");

                string serializedRecording = SerializeRecordingForUpload(recording);
                object[] parameters =
                    {
                    serializedRecording,
                    recording.Name
                };
                var result = this.InvokeMethod(GenerateTrainingDocumentMethodName, parameters);

                return result.FirstOrDefault().ToString();
            }
            
            /// <summary>
            /// Generates a business process model package.
            /// </summary>
            /// <param name="recording">The recording.</param>
            /// <returns>
            /// The URL to download the business process model package.
            /// </returns>
            public string GenerateBusinessProcessModelPackage(Recording recording)
            {
                ThrowIf.Null(recording, "recording");
                ThrowIf.Null(recording.RootScope, "recording.RootScope");

                string serializedRecording = SerializeRecordingForUpload(recording);
                object[] parameters =
                    {
                    serializedRecording,
                    recording.Name
                };
                var result = this.InvokeMethod(GenerateBusinessProcessModelPackageMethodName, parameters);

                return result.FirstOrDefault().ToString();
            }

            /// <summary>
            /// Generates a recording bundle.
            /// </summary>
            /// <param name="recording">The recording.</param>
            /// <returns>
            /// The URL to download the recording bundle.
            /// </returns>
            public string GenerateRecordingBundle(Recording recording)
            {
                ThrowIf.Null(recording, "recording");
                ThrowIf.Null(recording.RootScope, "recording.RootScope");

                string serializedRecording = SerializeRecordingForUpload(recording);
                object[] parameters =
                    {
                    serializedRecording,
                    recording.Name
                };
                var result = this.InvokeMethod(GenerateRecordingBundleMethodName, parameters);

                return result.FirstOrDefault().ToString();
            }

            /// <summary>
            /// Downloads the recording from LCS.
            /// </summary>
            /// <param name="businessProcessModelLineId">The business process model line identifier.</param>
            /// <returns>
            /// The downloaded recording from LCS.
            /// </returns>
            public Recording DownloadRecording(int businessProcessModelLineId)
            {
                object[] parameters =
                {
                    businessProcessModelLineId
                };
                var result = this.InvokeMethod(DownloadRecordingMethodName, parameters);
                Recording recording = DeserializeRecording(result.FirstOrDefault().ToString());

                return recording;
            }

            /// <summary>
            /// Gets the business process model libraries.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>
            /// The collection of business process model libraries.
            /// </returns>
            public PagedResult<Framework> GetBusinessProcessModelLibraries(QueryResultSettings queryResultSettings)
            {
                ThrowIf.Null(queryResultSettings, "queryResultSettings");

                var result = this.InvokeMethod(GetBusinessProcessModelLibrariesMethodName);
                IEnumerable<Framework> frameworks = Deserialize<IEnumerable<Framework>>(result.FirstOrDefault().ToString());
                if (frameworks == null || !frameworks.Any())
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_LCSLibrariesNotConfigured,
                        "Please setup up the default LCS project and BPM libraries in Dynamics AX > System administration > Help.");
                }

                return new PagedResult<Framework>(new ReadOnlyCollection<Framework>(new List<Framework>(frameworks)), queryResultSettings.Paging);
            }

            /// <summary>
            /// Gets a single business process model library.
            /// </summary>
            /// <param name="businessProcessModelFrameworkId">The business process model framework identifier.</param>
            /// <param name="hierarchyDepth">The hierarchy depth of the framework.</param>
            /// <returns>
            /// A single business process model library framework.
            /// </returns>
            public Framework GetBusinessProcessModelLibrary(int businessProcessModelFrameworkId, int hierarchyDepth)
            {
                object[] parameters =
                {
                    businessProcessModelFrameworkId,
                    hierarchyDepth
                };
                var result = this.InvokeMethod(GetBusinessProcessModelLibraryMethodName, parameters);
                Framework framework = Deserialize<Framework>(result.FirstOrDefault().ToString());

                return framework;
            }

            /// <summary>
            /// Searches for task guides by title.
            /// </summary>
            /// <param name="businessProcessModelFrameworkId">The business process model framework identifier.</param>
            /// <param name="taskGuideSearchText">The task guide search text.</param>
            /// <param name="queryTypeValue">The value of the query type.</param>
            /// <returns>
            /// A task guides search result.
            /// </returns>
            public TaskGuidesSearchResult SearchTaskGuidesByTitle(int businessProcessModelFrameworkId, string taskGuideSearchText, int queryTypeValue)
            {
                ThrowIf.NullOrWhiteSpace(taskGuideSearchText, "taskGuideSearchText");
                if (!Enum.IsDefined(typeof(QueryType), queryTypeValue) || queryTypeValue == (int)QueryType.None)
                {
                    throw new ArgumentException("The query type value is invalid.");
                }

                object[] parameters =
                {
                    businessProcessModelFrameworkId,
                    taskGuideSearchText,
                    queryTypeValue
                };

                var result = this.InvokeMethod(SearchTaskGuidesByTitleMethodName, parameters);
                TaskGuidesSearchResult taskGuidesSearchResult = Deserialize<TaskGuidesSearchResult>(result.FirstOrDefault().ToString());

                return taskGuidesSearchResult;
            }

            /// <summary>
            /// Loads and returns a recording from an XML file.
            /// </summary>
            /// <param name="recordingUrl">The recording URL.</param>
            /// <returns>
            /// A recording.
            /// </returns>
            public Recording LoadRecordingFromFile(string recordingUrl)
            {
                ThrowIf.NullOrWhiteSpace(recordingUrl, "recordingUrl");

                object[] parameters =
                {
                    recordingUrl
                };
                var result = this.InvokeMethod(LoadRecordingFromFileMethodName, parameters);
                Recording recording = DeserializeRecording(result.FirstOrDefault().ToString());

                return recording;
            }

            /// <summary>
            /// Gets StorageAccessToken from data returned by transaction service.
            /// </summary>
            /// <param name="storageAccessData">Storage access data from service.</param>
            /// <returns>The storage access token.</returns>
            private static StorageAccessToken GetStorageAccessToken(ReadOnlyCollection<object> storageAccessData)
            {
                StorageAccessToken token = new StorageAccessToken();
                token.Url = storageAccessData[UrlIndex].ToString();
                token.SasKey = storageAccessData[SasKeyIndex].ToString();

                return token;
            }

            /// <summary>
            /// Serializes the recording.
            /// </summary>
            /// <param name="recording">The recording.</param>
            /// <returns>The serialized recording.</returns>
            private static string SerializeRecordingForUpload(Recording recording)
            {
                if (recording.RootScope != null)
                {
                    SetParentScopeAndUserActions(recording.RootScope, ref recording);
                }

                ConvertFormContextEntriesToFormContexts(ref recording);
                string text;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    SerializeRecording(memoryStream, recording);
                    byte[] data = new byte[memoryStream.Length];
                    Array.Copy(memoryStream.GetBuffer(), data, data.Length);

                    text = Encoding.UTF8.GetString(data);
                }

                return text;
            }

            /// <summary>
            /// Serializes recording to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="recording">The recording.</param>
            private static void SerializeRecording(Stream stream, Recording recording)
            {
                using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true }))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Recording));
                    serializer.WriteObject(writer, recording);
                }
            }

            /// <summary>
            /// Recursively sets the parent scope for all of the scopes in the recording and populates the flattened array of user actions.
            /// </summary>
            /// <param name="scope">The scope.</param>
            /// <param name="recording">The recording.</param>
            private static void SetParentScopeAndUserActions(Scope scope, ref Recording recording)
            {
                if (scope.Children.IsNullOrEmpty())
                {
                    return;
                }

                foreach (var child in scope.Children)
                {
                    child.Parent = scope;

                    if (child.GetType() == typeof(Scope))
                    {
                        SetParentScopeAndUserActions((Scope)child, ref recording);
                    }
                    else if (child.GetType().IsSubclassOf(typeof(UserAction)))
                    {
                        UserAction userAction = (UserAction)child;
                        userAction.Recording = recording;
                        recording.AddUserAction(userAction);
                    }
                }
            }

            /// <summary>
            /// Convert FormContextEntries to FormContexts in preparation for serialization.
            /// </summary>
            /// <param name="recording">The recording.</param>
            private static void ConvertFormContextEntriesToFormContexts(ref Recording recording)
            {
                if (recording == null || recording.FormContextEntries == null)
                {
                    return;
                }

                recording.FormContexts = new Dictionary<string, FormContextForAX>();
                foreach (var entry in recording.FormContextEntries)
                {
                    recording.FormContexts.Add(entry.FormId, new FormContextForAX(entry.FormContext));
                }

                recording.FormContextEntries = null;
            }

            /// <summary>
            /// Deserializes a recording from the specified input stream.
            /// </summary>
            /// <param name="input">The input string.</param>
            /// <returns>
            /// Deserialized recording.
            /// </returns>
            private static Recording DeserializeRecording(string input)
            {
                Recording recording = Deserialize<Recording>(input);
                recording = ConvertFormContextsToFormContextEntries(recording);
                if (recording.RootScope != null)
                {
                    SetParentScopeIds(recording.RootScope, ref recording);
                }

                return recording;
            }

            /// <summary>
            /// Deserializes a collection of frameworks.
            /// </summary>
            /// <typeparam name="T">Type of target object.</typeparam>
            /// <param name="serializedFrameworksXml">The collection of frameworks in XML format.</param>
            /// <returns>
            /// Deserialized collection of frameworks.
            /// </returns>
            private static T Deserialize<T>(string serializedFrameworksXml) where T : class
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.XmlResolver = null;
                using (XmlReader reader = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(serializedFrameworksXml)), xmlReaderSettings))
                {
                    return serializer.ReadObject(reader) as T;
                }
            }

            /// <summary>
            /// Convert FormContexts to FormContextEntries after deserialization.
            /// </summary>
            /// <param name="recording">The recording.</param>
            /// <returns>The recording with form context entries.</returns>
            private static Recording ConvertFormContextsToFormContextEntries(Recording recording)
            {
                if (recording == null || recording.FormContexts == null)
                {
                    return recording;
                }

                recording.FormContextEntries = new List<FormContextDictionaryEntry>();
                foreach (var formContext in recording.FormContexts)
                {
                    recording.FormContextEntries.Add(new FormContextDictionaryEntry(formContext.Key, new FormContext(formContext.Value)));
                }

                recording.FormContexts = null;

                return recording;
            }

            /// <summary>
            /// Recursively sets the parent scope identifiers and populates the list of scopes.
            /// </summary>
            /// <param name="scope">The scope.</param>
            /// <param name="recording">The recording.</param>
            private static void SetParentScopeIds(Scope scope, ref Recording recording)
            {
                if (recording.Scopes == null)
                {
                    recording.Scopes = new List<Scope>();
                }

                recording.Scopes.Add(scope);
                if (scope.Children.IsNullOrEmpty())
                {
                    return;
                }

                foreach (var child in scope.Children)
                {
                    child.ParentScopeId = scope.Id;
                    if (child.GetType() == typeof(Scope))
                    {
                        SetParentScopeIds((Scope)child, ref recording);
                    }
                }
            }
        }
    }
}