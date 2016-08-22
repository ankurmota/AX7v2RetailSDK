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
        using System.ServiceModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Retail.TransactionServices.ClientProxy;
        using CP = Retail.TransactionServices.ClientProxy;
        using DM = Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Async Service Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            /// <summary>
            /// Gets data store name for terminal.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="terminalId">Terminal id.</param>
            /// <returns>Data store name.</returns>
            public string GetTerminalDataStoreName(Guid activityId, string terminalId)
            {
                string dataStoreName = string.Empty;
    
                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.GetTerminalDataStoreName request = new CP.GetTerminalDataStoreName() { terminalId = terminalId };
                    dataStoreName = channel.GetTerminalDataStoreName(request).result;
                    channel.Close();
                }
    
                return dataStoreName;
            }

            /// <summary>
            /// Gets download interval for data store.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="dataStoreName">Data store name.</param>
            /// <returns>Download interval.</returns>
            public int GetDownloadInterval(Guid activityId, string dataStoreName)
            {
                int downloadInterval = 0;
    
                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.GetDownloadInterval request = new CP.GetDownloadInterval() { dataStoreName = dataStoreName };
                    downloadInterval = channel.GetDownloadInterval(request).result;
                    channel.Close();
                }
    
                return downloadInterval;
            }

            /// <summary>
            /// Gets upload interval for data store.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="dataStoreName">Data store name.</param>
            /// <returns>Upload interval.</returns>
            public int GetUploadInterval(Guid activityId, string dataStoreName)
            {
                int uploadInterval = 0;
    
                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.GetUploadInterval request = new CP.GetUploadInterval() { dataStoreName = dataStoreName };
                    uploadInterval = channel.GetUploadInterval(request).result;
                    channel.Close();
                }
    
                return uploadInterval;
            }

            /// <summary>
            /// Validates data store and user name.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="dataStoreName">Data store name.</param>
            /// <param name="userName">User name.</param>
            /// <returns>Is data store valid.</returns>
            public bool ValidateDataStore(Guid activityId, string dataStoreName, string userName)
            {
                bool isDataStoreValid = false;
    
                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.ValidateDataStore request = new CP.ValidateDataStore() { dataStoreName = dataStoreName, userName = userName };
                    isDataStoreValid = channel.ValidateDataStore(request).result;
                    channel.Close();
                }
    
                return isDataStoreValid;
            }

            /// <summary>
            /// Gets download link for session.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="dataStoreName">Data store name.</param>
            /// <param name="downloadSessionId">Download session id.</param>
            /// <returns>Download link.</returns>
            public string GetDownloadLink(Guid activityId, string dataStoreName, long downloadSessionId)
            {
                string downloadUri = string.Empty;

                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.GetDownloadUri request = new CP.GetDownloadUri()
                    {
                        dataStoreName = dataStoreName,
                        downloadSessionId = downloadSessionId
                    };
                    downloadUri = channel.GetDownloadUri(request).result;
                    channel.Close();
                }

                return downloadUri;
            }

            /// <summary>
            /// Updates download session status in database.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="downloadSession">The download session.</param>
            public void UpdateDownloadSessionStatus(Guid activityId, DM.DownloadSession downloadSession)
            {
                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.DownloadSessionUpdateStatus session = ConvertDownloadSessionsUpdateStatus(downloadSession);
                    CP.UpdateDownloadSessionStatus request = new CP.UpdateDownloadSessionStatus() { downloadSession = session };
                    channel.UpdateDownloadSessionStatus(request);
                    channel.Close();
                }
            }

            /// <summary>
            /// Gets download sessions for data store.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="dataStoreName">Data store name.</param>
            /// <param name="settings">The query result setting.</param>
            /// <returns>Download session collection.</returns>
            public PagedResult<DM.DownloadSession> GetDownloadSessions(Guid activityId, string dataStoreName, QueryResultSettings settings)
            {
                System.Collections.ObjectModel.ReadOnlyCollection<DM.DownloadSession> sessions;

                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.GetDownloadSessions request = new CP.GetDownloadSessions()
                    {
                        dataStoreName = dataStoreName
                    };
                    sessions = ConvertDownloadSessions(channel.GetDownloadSessions(request).result);
                    channel.Close();
                }

                // paginate results here
                return sessions.AsPagedResult(settings);
            }

            /// <summary>
            /// Gets upload job Definitions for terminal.
            /// </summary>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <param name="dataStoreName">The data store name.</param>
            /// <returns>Upload job Definitions collection.</returns>
            public IReadOnlyCollection<string> GetUploadJobDefinitions(Guid activityId, string dataStoreName)
            {
                IReadOnlyCollection<string> jobDefinitions;
    
                using (RetailRealTimeServiceContractChannel channel = this.clientFactory.CreateTransactionServiceClient())
                using (var contextScope = new OperationContextScope(channel))
                {
                    this.SetActivityIdInHttpHeader(activityId);
                    CP.GetUploadJobDefinitions request = new CP.GetUploadJobDefinitions() { dataStoreName = dataStoreName };
                    jobDefinitions = channel.GetUploadJobDefinitions(request).result;
                    channel.Close();
                }
    
                return jobDefinitions;
            }

            /// <summary>
            /// Converts proxy download sessions to data contract download session.
            /// </summary>
            /// <param name="sessions">Proxy download sessions.</param>
            /// <returns>Data contract download sessions.</returns>
            private static ReadOnlyCollection<DM.DownloadSession> ConvertDownloadSessions(CP.DownloadSession[] sessions)
            {
                List<DM.DownloadSession> returnSessions = new List<DM.DownloadSession>();
                foreach (CP.DownloadSession session in sessions)
                {
                    returnSessions.Add(new DM.DownloadSession
                    {
                        Id = session.Id,
                        JobId = session.JobId,
                        JobDescription = session.JobDescription,
                        FileSize = session.FileSize,
                        Checksum = session.Checksum,
                        Message = session.Message
                    });
                }
    
                return returnSessions.AsReadOnly<DM.DownloadSession>();
            }

            /// <summary>
            /// Converts data contract download session to proxy DownloadSessionUpdateStatus .
            /// </summary>
            /// <param name="session">Data contract download session.</param>
            /// <returns>Proxy DownloadSessionUpdateStatus.</returns>
            private static CP.DownloadSessionUpdateStatus ConvertDownloadSessionsUpdateStatus(DM.DownloadSession session)
            {
                CP.DownloadSessionUpdateStatus returnSession = new CP.DownloadSessionUpdateStatus
                {
                    DataStoreName = session.DataStoreName,
                    DateDownloaded = session.DateDownloaded,
                    Id = session.Id,
                    RowsAffected = session.RowsAffected,
                    Status = (CP.RetailCDXDownloadSessionStatus)Enum.Parse(typeof(CP.RetailCDXDownloadSessionStatus), session.Status.ToString(), true),
                    Message = session.Message
                };

                return returnSession;
            }
        }
    }
}
