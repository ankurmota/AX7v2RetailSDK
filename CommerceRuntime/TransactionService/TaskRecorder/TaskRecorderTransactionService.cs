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
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// Task recorder transaction service class.
        /// </summary>
        public class TaskRecorderTransactionService
            : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(GetStorageAccessTokenForUploadRealtimeRequest),
                    typeof(UploadRecordingRealtimeRequest),
                    typeof(GenerateRecordingFileRealtimeRequest),
                    typeof(GenerateTrainingDocumentRealtimeRequest),
                    typeof(GenerateBusinessProcessModelPackageRealtimeRequest),
                    typeof(GenerateRecordingBundleRealtimeRequest),
                    typeof(DownloadRecordingRealtimeRequest),
                    typeof(GetBusinessProcessModelLibrariesRealtimeRequest),
                    typeof(GetBusinessProcessModelLibraryRealtimeRequest),
                    typeof(SearchTaskGuidesByTitleRealtimeRequest),
                    typeof(LoadRecordingFromFileRealtimeRequest)
                };
                }
            }

            /// <summary>
            /// Executes the service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetStorageAccessTokenForUploadRealtimeRequest))
                {
                    response = GetStorageAccessTokenForUpload((GetStorageAccessTokenForUploadRealtimeRequest)request);
                }
                else if (requestType == typeof(UploadRecordingRealtimeRequest))
                {
                    response = UploadRecording((UploadRecordingRealtimeRequest)request);
                }
                else if (requestType == typeof(GenerateRecordingFileRealtimeRequest))
                {
                    response = GenerateRecordingFile((GenerateRecordingFileRealtimeRequest)request);
                }
                else if (requestType == typeof(GenerateTrainingDocumentRealtimeRequest))
                {
                    response = GenerateTrainingDocument((GenerateTrainingDocumentRealtimeRequest)request);
                }
                else if (requestType == typeof(GenerateBusinessProcessModelPackageRealtimeRequest))
                {
                    response = GenerateBusinessProcessModelPackage((GenerateBusinessProcessModelPackageRealtimeRequest)request);
                }
                else if (requestType == typeof(GenerateRecordingBundleRealtimeRequest))
                {
                    response = GenerateRecordingBundle((GenerateRecordingBundleRealtimeRequest)request);
                }
                else if (requestType == typeof(DownloadRecordingRealtimeRequest))
                {
                    response = DownloadRecording((DownloadRecordingRealtimeRequest)request);
                }
                else if (requestType == typeof(GetBusinessProcessModelLibrariesRealtimeRequest))
                {
                    response = GetBusinessProcessModelLibraries((GetBusinessProcessModelLibrariesRealtimeRequest)request);
                }
                else if (requestType == typeof(GetBusinessProcessModelLibraryRealtimeRequest))
                {
                    response = GetBusinessProcessModelLibrary((GetBusinessProcessModelLibraryRealtimeRequest)request);
                }
                else if (requestType == typeof(SearchTaskGuidesByTitleRealtimeRequest))
                {
                    response = SearchTaskGuidesByTitle((SearchTaskGuidesByTitleRealtimeRequest)request);
                }
                else if (requestType == typeof(LoadRecordingFromFileRealtimeRequest))
                {
                    response = LoadRecordingFromFile((LoadRecordingFromFileRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Gets a storage access token for upload.
            /// </summary>
            /// <param name="request">Request for getting the storage access token.</param>
            /// <returns>The storage access token.</returns>
            private static SingleEntityDataServiceResponse<StorageAccessToken> GetStorageAccessTokenForUpload(GetStorageAccessTokenForUploadRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                StorageAccessToken storageAccessToken = transactionService.GetStorageAccessTokenForUpload();

                return new SingleEntityDataServiceResponse<StorageAccessToken>(storageAccessToken);
            }

            /// <summary>
            /// Upload the recording to Lifecycle Services.
            /// </summary>
            /// <param name="request">Request for uploading the recording.</param>
            /// <returns>The null response.</returns>
            private static Response UploadRecording(UploadRecordingRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                transactionService.UploadRecording(request.Recording, request.BusinessProcessModelLineId);
                
                return new NullResponse();
            }

            /// <summary>
            /// Serialize the recording and upload to Azure blob storage.
            /// </summary>
            /// <param name="request">Request for serializing the recording.</param>
            /// <returns>The URL to the serialized recording.</returns>
            private static SingleEntityDataServiceResponse<string> GenerateRecordingFile(GenerateRecordingFileRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                string serializedRecordingUrl = transactionService.GenerateRecordingFile(request.Recording);

                return new SingleEntityDataServiceResponse<string>(serializedRecordingUrl);
            }

            /// <summary>
            /// Generates a training document.
            /// </summary>
            /// <param name="request">Request for generating the training document.</param>
            /// <returns>The URL to the training document.</returns>
            private static SingleEntityDataServiceResponse<string> GenerateTrainingDocument(GenerateTrainingDocumentRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                string trainingDocumentUrl = transactionService.GenerateTrainingDocument(request.Recording);

                return new SingleEntityDataServiceResponse<string>(trainingDocumentUrl);
            }

            /// <summary>
            /// Generates a business process model package.
            /// </summary>
            /// <param name="request">Request for generating the business process model package.</param>
            /// <returns>
            /// The URL to download the business process model package.
            /// </returns>
            private static SingleEntityDataServiceResponse<string> GenerateBusinessProcessModelPackage(GenerateBusinessProcessModelPackageRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                string businessProcessModelPackageUrl = transactionService.GenerateBusinessProcessModelPackage(request.Recording);

                return new SingleEntityDataServiceResponse<string>(businessProcessModelPackageUrl);
            }

            /// <summary>
            /// Generates a recording bundle.
            /// </summary>
            /// <param name="request">Request for generating the recording bundle.</param>
            /// <returns>
            /// The URL to download the recording bundle.
            /// </returns>
            private static SingleEntityDataServiceResponse<string> GenerateRecordingBundle(GenerateRecordingBundleRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                string recordingBundleUrl = transactionService.GenerateRecordingBundle(request.Recording);

                return new SingleEntityDataServiceResponse<string>(recordingBundleUrl);
            }

            /// <summary>
            /// Downloads the recording from LCS.
            /// </summary>
            /// <param name="request">Request for downloading the recording.</param>
            /// <returns>
            /// The downloaded recording from LCS.
            /// </returns>
            private static SingleEntityDataServiceResponse<Recording> DownloadRecording(DownloadRecordingRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                Recording recording = transactionService.DownloadRecording(request.BusinessProcessModelLineId);

                return new SingleEntityDataServiceResponse<Recording>(recording);
            }

            /// <summary>
            /// Gets the business process model libraries.
            /// </summary>
            /// <param name="request">Request for getting the business process model libraries.</param>
            /// <returns>
            /// The response containing the business process model libraries.
            /// </returns>
            private static GetBusinessProcessModelLibrariesRealtimeResponse GetBusinessProcessModelLibraries(GetBusinessProcessModelLibrariesRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                var response = transactionService.GetBusinessProcessModelLibraries(request.QueryResultSettings);

                return new GetBusinessProcessModelLibrariesRealtimeResponse(response);
            }

            /// <summary>
            /// Gets a single business process model library framework.
            /// </summary>
            /// <param name="request">Request for getting the business process model library framework.</param>
            /// <returns>
            /// The response containing the business process model library framework.
            /// </returns>
            private static SingleEntityDataServiceResponse<Framework> GetBusinessProcessModelLibrary(GetBusinessProcessModelLibraryRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                var response = transactionService.GetBusinessProcessModelLibrary(request.BusinessProcessModelFrameworkId, request.HierarchyDepth);

                return new SingleEntityDataServiceResponse<Framework>(response);
            }

            /// <summary>
            /// Searches for task guides by title.
            /// </summary>
            /// <param name="request">Request for searching task guides by title.</param>
            /// <returns>
            /// The response containing task guide search results.
            /// </returns>
            private static SingleEntityDataServiceResponse<TaskGuidesSearchResult> SearchTaskGuidesByTitle(SearchTaskGuidesByTitleRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                var response = transactionService.SearchTaskGuidesByTitle(request.BusinessProcessModelFrameworkId, request.TaskGuideSearchText, request.QueryTypeValue);

                return new SingleEntityDataServiceResponse<TaskGuidesSearchResult>(response);
            }

            /// <summary>
            /// Loads and returns a recording from an XML file.
            /// </summary>
            /// <param name="request">Request for loading recording from file.</param>
            /// <returns>
            /// The recording from the XML file.
            /// </returns>
            private static SingleEntityDataServiceResponse<Recording> LoadRecordingFromFile(LoadRecordingFromFileRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                var response = transactionService.LoadRecordingFromFile(request.RecordingUrl);

                return new SingleEntityDataServiceResponse<Recording>(response);
            }
        }
    }
}
