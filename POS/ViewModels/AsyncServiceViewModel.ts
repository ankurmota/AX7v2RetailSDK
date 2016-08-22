/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import ErrorParser = Proxy.Context.ErrorParser;

    /**
     * Represents the async service view model.
     */
    export class AsyncServiceViewModel extends ViewModelBase {

        // Public properties
        public static downloadIntervalInMillisecond: number = 60000;
        public static uploadIntervalInMillisecond: number = 60000;
        public static initialIntervalInMillisecond: number = 60000;
        public downloadLink: string;
        public downloadSession: Model.Entities.DownloadSession;
        public downloadSessions: Model.Entities.DownloadSession[];
        public terminalId: string;
        public offlineDatabaseConnectionString: string;
        public sqlCommandTimeout: number = 3600;
        public intervalMultiplier: number = 60000;
        public hasUploadData: boolean = false;
        public uploadIntervalWithPendingData: number = 1000;
        private static syncingDownloadData: boolean = false;
        private static syncingUploadData: boolean = false;

        constructor() {
            super();
            AsyncServiceViewModel.downloadIntervalInMillisecond = Commerce.Config.defaultOfflineDownloadIntervalInMilliseconds;
            AsyncServiceViewModel.uploadIntervalInMillisecond = Commerce.Config.defaultOfflineUploadIntervalInMilliseconds;
            this.offlineDatabaseConnectionString = Commerce.Config.offlineDatabase;
            this.sqlCommandTimeout = Commerce.Config.sqlCommandTimeout;
        }

        /**
          * Start the data sync for offline.
          */
        public static execute(): void {
            function CDXDownload() {
                var asyncServiceViewModel = new Commerce.ViewModels.AsyncServiceViewModel();
                asyncServiceViewModel.executeDownload();
            }

            Session.instance.offlineParameters.syncDownloadOfflineData = setTimeout(CDXDownload, AsyncServiceViewModel.initialIntervalInMillisecond);

            function CDXUpload() {
                var asyncServiceViewModel = new Commerce.ViewModels.AsyncServiceViewModel();
                asyncServiceViewModel.executeUpload();
            }

            Session.instance.offlineParameters.syncUploadOfflineData = setTimeout(CDXUpload, AsyncServiceViewModel.initialIntervalInMillisecond);
        }

        /**
         * Send download request to check if there exists new data to download from AX. If so, download the data and apply to database.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public executeDownload() {
            var downloadSessions: Model.Entities.DownloadSession[];
            var dataStoreName: string;
            var terminalId = ApplicationStorage.getItem(ApplicationStorageIDs.REGISTER_ID_KEY);
            var requireInitialSync: boolean;
            AsyncServiceViewModel.syncingDownloadData = true;
            Session.instance.offlineSyncing(true);

            if (terminalId == null) {
                RetailLogger.viewModelGetTerminalIdFailed();
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.asyncServiceManager.getTerminalDataStoreNameAsync(terminalId)
                        .done((result: any) => {
                            if (ObjectExtensions.isNullOrUndefined(result)) {
                                dataStoreName = result;
                            } else if (!ObjectExtensions.isNullOrUndefined(result.value)) {
                                dataStoreName = result.value;
                            } else {
                                dataStoreName = result;
                            }
                        })
                        .fail(() => {
                            if (Commerce.Session.instance.connectionStatus === ConnectionStatusType.Online) {
                                RetailLogger.viewModelGetTerminalDataStoreNameFailed(terminalId);
                            }
                        });
                });

            asyncQueue
                .enqueue(() => {
                    if (!dataStoreName || dataStoreName.length == 0) {
                        Session.instance.offlineParameters.offlineModeDisabled = true;
                        return VoidAsyncResult.createRejected(<Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.ASYNC_CLIENT_OFFLINE_NOT_ENABLED_ON_TERMINAL)]);
                    }
                    else {
                        if (Session.instance.offlineParameters.offlineModeDisabled) {
                            Session.instance.offlineParameters.offlineModeDisabled = false;
                        }
                        return VoidAsyncResult.createResolved();
                    }
                });

            asyncQueue
                .enqueue(() => {
                    return this.asyncServiceManager.getDownloadIntervalAsync(dataStoreName)
                        .done((result) => {
                            if (parseInt(result) != 0) {
                                AsyncServiceViewModel.downloadIntervalInMillisecond = parseInt(result) * this.intervalMultiplier;
                            }
                        })
                        .fail(() => {
                            RetailLogger.viewModelGetDownloadIntervalFailed(dataStoreName);
                        });
                });

            asyncQueue
                .enqueue(() => {
                    return this.asyncServiceManager.getDownloadSessionsAsync(dataStoreName)
                        .done((result) => {
                            downloadSessions = result;
                        })
                        .fail(() => {
                            RetailLogger.viewModelGetDownloadSessionsFailed(dataStoreName);
                        });
                });

            asyncQueue
                .enqueue(() => {
                    if (ArrayExtensions.hasElements(downloadSessions)) {
                        Session.instance.pendingDownloadSessionCount(downloadSessions.length);
                        return VoidAsyncResult.createResolved();
                    } else {
                        // If no download sessions available, update pending download session count to 0 if not.
                        if (Session.instance.pendingDownloadSessionCount() !== 0) {
                            Session.instance.pendingDownloadSessionCount(0);
                        }
                    }

                    return VoidAsyncResult.createRejected(<Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.ASYNC_CLIENT_ZERO_DOWNLOAD_SESSION)]);
                });

            asyncQueue
                .enqueue(() => {
                    var asyncResult = new AsyncResult();
                    // Only execute this request when there are available download sessions to download to reduce the frequency of interacting with database.
                    Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.deleteExpiredSessionsAsync(this.offlineDatabaseConnectionString)
                        .done((result: Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientResponseMessage) => {
                            if (!result.requestSuccess) {
                                RetailLogger.viewModelDeleteExpiredSessionFailed(result.statusText);
                            }

                            asyncResult.resolve(result.statusText);
                        });

                    return asyncResult;
                });

            asyncQueue.enqueue(() => {
                var asyncResult = new VoidAsyncResult();
                var current = this;

                ObjectExtensions.forEachAsync(downloadSessions, function (downloadSession, next) {
                    var status: number;
                    var workingFolder: string;
                    var fileName: string;
                    var uri: string;

                    downloadSession.DataStoreName = dataStoreName;
                    downloadSession.StatusValue = Model.Entities.DownloadSessionStatus.Requested;
                    var requestDate = new Date();
                    var requestTime = requestDate.getUTCFullYear() + '-' + (requestDate.getUTCMonth() + 1) + '-' + requestDate.getUTCDate();
                    requestTime += ' ' + requestDate.getUTCHours() + ':' + requestDate.getUTCMinutes() + ':' + requestDate.getUTCSeconds();
                    downloadSession.DateRequested = requestTime;

                    var innerQueue = new AsyncQueue();

                    innerQueue.enqueue(() => {
                        return current.asyncServiceManager.getDownloadLinkAsync(dataStoreName, downloadSession.Id)
                            .done((result) => {
                                uri = result;
                            })
                            .fail(() => {
                                RetailLogger.viewModelGetDownloadLinkFailed(dataStoreName, downloadSession.Id);
                            });

                        return asyncResult;
                    });

                    innerQueue
                        .enqueue(() => {
                            if (uri === "") {
                                downloadSession.StatusValue = Model.Entities.DownloadSessionStatus.DownloadFailed;
                                downloadSession.Message = Commerce.ViewModelAdapter.getResourceString("string_29051");
                            }
                            return VoidAsyncResult.createResolved();
                        });

                    innerQueue.enqueue(() => {
                        if (downloadSession.StatusValue !== Model.Entities.DownloadSessionStatus.DownloadFailed) {
                            var asyncResult = new AsyncResult();
                            try {
                                Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.downloadFileAsync(uri, downloadSession.Checksum)
                                    .done((result: Microsoft.Dynamics.Commerce.ClientBroker.DownloadFileResponseMessage) => {
                                        if (result.requestSuccess == false) {
                                            RetailLogger.viewModelDownloadFileFailed(result.statusText);
                                            downloadSession.StatusValue = Model.Entities.DownloadSessionStatus.DownloadFailed;
                                            downloadSession.Message = result.statusText;
                                            asyncResult.resolve(result.statusText);
                                        }
                                        else {
                                            workingFolder = result.workingFolder;
                                            fileName = result.fileName;
                                            downloadSession.StatusValue = Model.Entities.DownloadSessionStatus.Downloaded;
                                            var downloadDate = new Date();
                                            // AX side API can not parse one digit second while it is able to parse hour and minute correctly. Add special handling to retrieve seconds.
                                            var utcSeconds = (downloadDate.getUTCSeconds() < 10) ? ('0' + downloadDate.getUTCSeconds()) : downloadDate.getUTCSeconds().toString();
                                            var downloadTime = downloadDate.getUTCFullYear() + '-' + (downloadDate.getUTCMonth() + 1) + '-' + downloadDate.getUTCDate();
                                            downloadTime += ' ' + downloadDate.getUTCHours() + ':' + downloadDate.getUTCMinutes() + ':' + utcSeconds;
                                            downloadSession.DateDownloaded = downloadTime;
                                            asyncResult.resolve(result.statusText);
                                        }
                                    });
                            }
                            catch (error) {
                                RetailLogger.viewModelDownloadFileBrokerRequestFailed(error.message)
                                asyncResult.reject(ErrorParser.parseErrorMessage(error));
                            }
                            return asyncResult;
                        } else {
                            return VoidAsyncResult.createResolved();
                        }
                    });

                    innerQueue.enqueue(() => {
                        if (downloadSession.StatusValue !== Model.Entities.DownloadSessionStatus.DownloadFailed) {
                            var asyncResult = new AsyncResult();
                            try {
                                Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.applyFileToOfflineDbAsync(current.offlineDatabaseConnectionString, workingFolder, fileName, terminalId, current.sqlCommandTimeout)
                                    .done((result: Microsoft.Dynamics.Commerce.ClientBroker.ApplySessionFileResponseMessage) => {
                                        if (!result.requestSuccess) {
                                            RetailLogger.viewModelApplyToOfflineDbFailed(result.statusText);
                                            downloadSession.StatusValue = Model.Entities.DownloadSessionStatus.ApplyFailed;
                                            downloadSession.Message = result.statusText;
                                            asyncResult.resolve(result.statusText);
                                        }
                                        else {
                                            downloadSession.StatusValue = Model.Entities.DownloadSessionStatus.Applied;
                                            downloadSession.RowsAffected = result.rowsAffected;
                                            asyncResult.resolve(result.statusText);
                                        }
                                    });
                            }
                            catch (error) {
                                RetailLogger.viewModelApplyToOfflineDbBrokerRequestFailed(error.message);
                                asyncResult.reject(ErrorParser.parseErrorMessage(error));
                            }
                            return asyncResult;
                        } else {
                            return VoidAsyncResult.createResolved();
                        }
                    });

                    innerQueue.enqueue(() => {
                        var asyncResult = new AsyncResult();
                        try {
                            return current.UpdateDownloadSessionStatus(downloadSession)
                                .done((result: Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientResponseMessage) => {
                                    if (!result.requestSuccess) {
                                        RetailLogger.viewModelUpdateDownloadSessionStatusBrokerRequestFailed(result.statusText);
                                        asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_FAIL_UPDATE_DOWNLOAD_SESSION_STATUS)]);
                                    }
                                    else {
                                        asyncResult.resolve(result.statusText);
                                    }
                                });
                        }
                        catch (error) {
                            asyncResult.reject(ErrorParser.parseErrorMessage(error));
                        }

                        return asyncResult;
                    });

                    innerQueue.enqueue(() => {
                        // Download Session status has updated when download uri cannot be retrieved. 
                        if (uri !== "") {
                            return current.asyncServiceManager.updateDownloadSessionAsync(downloadSession)
                                .done(() => {
                                })
                                .fail((error: Model.Entities.Error[]) => {
                                    RetailLogger.viewModelUpdateDownloadSessionStatusFailed();
                                });
                        } else {
                            return VoidAsyncResult.createResolved();
                        }
                    });

                    innerQueue.run()
                        .always(() => {
                            var PendingDownloadSessions = (downloadSession) => {
                                return downloadSession.StatusValue !== 4;
                            };

                            Session.instance.pendingDownloadSessionCount(downloadSessions.filter(PendingDownloadSessions).length);
                            next();
                        });
                }, () => asyncResult.resolve());

                return asyncResult;
            });

            return asyncQueue.run()
                .always(() => {
                    AsyncServiceViewModel.syncingDownloadData = false;
                    if (AsyncServiceViewModel.syncingUploadData != true) {
                        Session.instance.offlineSyncing(false);
                    }

                    if (!Session.instance.offlineParameters.offlineModeDisabled) {
                        clearTimeout(Session.instance.offlineParameters.syncDownloadOfflineData);
                        Session.instance.offlineParameters.syncDownloadOfflineData = setTimeout(CDXDownload, AsyncServiceViewModel.downloadIntervalInMillisecond);
                    } else {
                        clearTimeout(Session.instance.offlineParameters.syncDownloadOfflineData);
                    }
                });

            function CDXDownload() {
                var asyncServiceViewModel = new Commerce.ViewModels.AsyncServiceViewModel();
                asyncServiceViewModel.executeDownload();
            }
        }

        /**
         * Send upload request to check if there exists new transaction data to upload from offline database. If so, upload the transaction data to Retail Server database.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public executeUpload() {
            var fileName: string;
            var executeSuccess: boolean;
            var dataStoreName: string;
            var asyncResult = new AsyncResult<any>(null);
            var loadUploadTransactionResponseMessage;
            var uploadJobDefinitions: string[];
            var uploadTransactionData: string[];
            var uploadSuccess: boolean;
            var current = this;
            AsyncServiceViewModel.syncingUploadData = true;
            Session.instance.offlineSyncing(true);

            var terminalId = ApplicationStorage.getItem(ApplicationStorageIDs.REGISTER_ID_KEY);
            if (terminalId == null) {
                RetailLogger.viewModelGetTerminalIdFailed();
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.asyncServiceManager.getTerminalDataStoreNameAsync(terminalId)
                        .done((result: any) => {
                            if (ObjectExtensions.isNullOrUndefined(result)) {
                                dataStoreName = result;
                            } else if (!ObjectExtensions.isNullOrUndefined(result.value)) {
                                dataStoreName = result.value;
                            } else {
                                dataStoreName = result;
                            }
                        })
                        .fail(() => {
                            if (Commerce.Session.instance.connectionStatus === ConnectionStatusType.Online) {
                                RetailLogger.viewModelGetTerminalDataStoreNameFailed(terminalId);
                            }
                        });
                });

            asyncQueue
                .enqueue(() => {
                    if (!dataStoreName || dataStoreName.length == 0) {
                        Session.instance.offlineParameters.offlineModeDisabled = true;
                        return VoidAsyncResult.createRejected(<Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.ASYNC_CLIENT_OFFLINE_NOT_ENABLED_ON_TERMINAL)]);
                    }
                    else {
                        return VoidAsyncResult.createResolved();
                    }
                });

            asyncQueue
                .enqueue(() => {
                    return this.asyncServiceManager.getUploadIntervalAsync(dataStoreName)
                        .done((result) => {
                            if (parseInt(result) != 0) {
                                AsyncServiceViewModel.uploadIntervalInMillisecond = parseInt(result) * this.intervalMultiplier;
                            }
                        })
                        .fail((error: Model.Entities.Error[]) => {
                            RetailLogger.viewModelGetUploadIntervalFailed(dataStoreName);
                            asyncResult.reject(error);
                        });
                });

            asyncQueue
                .enqueue(() => {
                    return this.asyncServiceManager.getUploadJobDefinitionsAsync(dataStoreName)
                        .done((result) => {
                            uploadJobDefinitions = result;
                        })
                        .fail(() => {
                            RetailLogger.viewModelGetUploadJobDefinitionsFailed(dataStoreName);
                        });
                });

            asyncQueue
                .enqueue(() => {
                    if (ArrayExtensions.hasElements(uploadJobDefinitions)) {
                        return VoidAsyncResult.createResolved();
                    }

                    return VoidAsyncResult.createRejected(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_EMPTY_UPLOAD_JOB_DEFINITION)]);
                });

            asyncQueue
                .enqueue(() => {
                    var asyncResult = new AsyncResult();
                    Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.loadTransactionDataAsync(this.offlineDatabaseConnectionString, uploadJobDefinitions, terminalId)
                        .done((result: Microsoft.Dynamics.Commerce.ClientBroker.LoadUploadTransactionResponseMessage) => {
                            uploadTransactionData = result.offlineTransactionDataList;
                            if (uploadTransactionData.length == 0) {
                                current.hasUploadData = false;
                                asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_NO_TRANSACTION_DATA)]);
                            }
                            else if (!result.requestSuccess) {
                                current.hasUploadData = false;
                                RetailLogger.viewModelLoadUploadTransactionsFailed(result.statusText);
                                asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_CANNOT_LOAD_OFFLINE_TRANSACTION_DATA)]);
                            }
                            else {
                                current.hasUploadData = true;
                                asyncResult.resolve(result.statusText);
                            }
                        });

                    return asyncResult;
                });

            asyncQueue
                .enqueue(() => {
                    var asyncResult = new VoidAsyncResult();
                    this.asyncServiceManager.syncOfflineTransactionAsync(uploadTransactionData)
                        .done((result: boolean) => {
                            uploadSuccess = result;
                            asyncResult.resolve();
                        })
                        .fail(() => {
                            RetailLogger.viewModelSyncOfflineTransactionsFailed();
                            uploadSuccess = false;
                            asyncResult.resolve();
                        });

                    return asyncResult;
                });

            asyncQueue
                .enqueue(() => {
                    var asyncResult = new AsyncResult();
                    if (uploadSuccess == true) {
                        Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.purgeOfflineTransactionsAsync(this.offlineDatabaseConnectionString, uploadJobDefinitions)
                            .done((result: Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientResponseMessage) => {
                                if (!result.requestSuccess) {
                                    RetailLogger.viewModelPurgeOfflineTransactionsFailed(result.statusText);
                                    asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_FAIL_PURGE_OFFLINE_TRANSACTION_DATA)]);
                                }
                                else {
                                    asyncResult.resolve(result.statusText);
                                }
                            });
                    } else {
                        Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.updateUploadFailedStatusAsync(this.offlineDatabaseConnectionString)
                            .done((result: Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientResponseMessage) => {
                                if (!result.requestSuccess) {
                                    asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_FAIL_UPDATE_UPLOAD_FAILED_STATUS)]);
                                }
                                else {
                                    asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_FAIL_UPLOAD_DATA)]);
                                }
                            });
                    }

                    return asyncResult;
                });

            asyncQueue.run().done(() => {
                AsyncServiceViewModel.syncingUploadData = false;
                if (AsyncServiceViewModel.syncingDownloadData != true) {
                    Session.instance.offlineSyncing(false);
                }

                if (!Session.instance.offlineParameters.offlineModeDisabled) {
                    clearTimeout(Session.instance.offlineParameters.syncUploadOfflineData);
                    if (current.hasUploadData) {
                        Session.instance.offlineParameters.syncUploadOfflineData = setTimeout(CDXUpload, this.uploadIntervalWithPendingData);
                    }
                    else {
                        Session.instance.offlineParameters.syncUploadOfflineData = setTimeout(CDXUpload, AsyncServiceViewModel.uploadIntervalInMillisecond);
                    }
                } else {
                    clearTimeout(Session.instance.offlineParameters.syncUploadOfflineData);
                }
            })
                .fail(() => {
                    AsyncServiceViewModel.syncingUploadData = false;
                    if (AsyncServiceViewModel.syncingDownloadData != true) {
                        Session.instance.offlineSyncing(false);
                    }

                    if (!Session.instance.offlineParameters.offlineModeDisabled) {
                        clearTimeout(Session.instance.offlineParameters.syncUploadOfflineData);
                        Session.instance.offlineParameters.syncUploadOfflineData = setTimeout(CDXUpload, AsyncServiceViewModel.uploadIntervalInMillisecond);
                    } else {
                        clearTimeout(Session.instance.offlineParameters.syncUploadOfflineData);
                    }
                });

            function CDXUpload() {
                var asyncServiceViewModel = new Commerce.ViewModels.AsyncServiceViewModel();
                asyncServiceViewModel.executeUpload();
            }
        }

        /**
         * Update download session status in offline database
         *
         * @param {downloadSession} The download session.
         */
        private UpdateDownloadSessionStatus(downloadSession: Commerce.Model.Entities.DownloadSession): any {
            var asyncClientDownloadSession = new Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientDownloadSession();
            asyncClientDownloadSession.id = downloadSession.Id;
            asyncClientDownloadSession.checksum = downloadSession.Checksum;
            asyncClientDownloadSession.fileSize = downloadSession.FileSize;
            asyncClientDownloadSession.status = downloadSession.StatusValue;
            asyncClientDownloadSession.jobId = downloadSession.JobId;
            asyncClientDownloadSession.jobDescription = downloadSession.JobDescription;
            asyncClientDownloadSession.dateRequested = downloadSession.DateRequested;
            asyncClientDownloadSession.dateDownloaded = downloadSession.DateDownloaded;
            asyncClientDownloadSession.message = downloadSession.Message;

            return Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.updateDownloadSessionStatusAsync(this.offlineDatabaseConnectionString, asyncClientDownloadSession);
        }
    }
}