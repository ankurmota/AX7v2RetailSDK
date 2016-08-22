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

    /**
     * Represents the database connection status view model.
     */
    export class DatabaseConnectionStatusViewModel extends ViewModelBase {
        private retentionDay: number = 7;
        public status: Observable<string>;
        public pendingTransactionCount: Observable<string>;
        public pendingDownloadSessionCount: Observable<string>;
        public offlineSyncStatus: Observable<string>;
        public lastSyncDateTime: Date = DateExtensions.getMinDate();
        public downloadSessionStatusTable: ObservableArray<Commerce.Model.Entities.OfflineSyncStatsLine>;
        public uploadSessionStatusTable: ObservableArray<Commerce.Model.Entities.OfflineSyncStatsLine>;
        public offlineDatabaseConnectionString: string;

        /**
         * Constructs a database status view model.
         *
         * @param {any} callerContext The caller context.
         */
        constructor() {
            super();

            this.status = ko.observable("");
            this.pendingTransactionCount = ko.observable("0");
            this.offlineSyncStatus = ko.observable(Commerce.ViewModelAdapter.getResourceString("string_6636"));
            this.downloadSessionStatusTable = ko.observableArray(<Commerce.Model.Entities.OfflineSyncStatsLine[]>[]);
            this.uploadSessionStatusTable = ko.observableArray(<Commerce.Model.Entities.OfflineSyncStatsLine[]>[]);
            this.pendingDownloadSessionCount = ko.observable("0");
            this.offlineDatabaseConnectionString = Commerce.Config.offlineDatabase;
            this.updateState();
        }

        /**
         * Loads the offline sync statistics.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public loadOfflineSyncStats(): IVoidAsyncResult {
            if ((Commerce.Config.offlineDatabase != null) && (Commerce.Config.offlineDatabase.length != 0)) {
                var asyncResult: VoidAsyncResult = new VoidAsyncResult();
                var current = this;
                Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientRequest.getOfflineSyncStatsAsync(this.offlineDatabaseConnectionString)
                    .done((result: Microsoft.Dynamics.Commerce.ClientBroker.GetOfflineSyncStatsResponseMessage) => {
                        if (result.requestSuccess) {
                            var offlineSyncStats: Microsoft.Dynamics.Commerce.ClientBroker.AsyncClientOfflineSyncStatsLine[] = result.offlineSyncStats;
                            var downloadSessions: Commerce.Model.Entities.OfflineSyncStatsLine[] = [];
                            var uploadSessions: Commerce.Model.Entities.OfflineSyncStatsLine[] = [];
                            var lastSyncDateTime = this.lastSyncDateTime;

                            if (!ObjectExtensions.isNullOrUndefined(offlineSyncStats)) {
                                ObjectExtensions.forEachAsync(offlineSyncStats, function (offlineSyncStatsLine, next) {
                                    var asyncClientOfflineSyncStatsLine: Commerce.Model.Entities.OfflineSyncStatsLineClass = new Commerce.Model.Entities.OfflineSyncStatsLineClass();
                                    asyncClientOfflineSyncStatsLine.JobDescription = offlineSyncStatsLine.jobDescription;
                                    asyncClientOfflineSyncStatsLine.Status = offlineSyncStatsLine.status;
                                    var utcDate: Date = offlineSyncStatsLine.lastSyncDateTime;
                                    asyncClientOfflineSyncStatsLine.LastSyncDateTime = new Date(utcDate.getTime() - utcDate.getTimezoneOffset() * 60 * 1000);
                                    asyncClientOfflineSyncStatsLine.FileSize = offlineSyncStatsLine.fileSize;
                                    asyncClientOfflineSyncStatsLine.IsUploadJob = Boolean(offlineSyncStatsLine.isUploadJob);

                                    if (!asyncClientOfflineSyncStatsLine.IsUploadJob) {
                                        downloadSessions.push(asyncClientOfflineSyncStatsLine);
                                    } else {
                                        uploadSessions.push(asyncClientOfflineSyncStatsLine);
                                    }

                                    lastSyncDateTime = asyncClientOfflineSyncStatsLine.LastSyncDateTime > lastSyncDateTime ? asyncClientOfflineSyncStatsLine.LastSyncDateTime : lastSyncDateTime;
                                    next();
                                });

                                this.lastSyncDateTime = lastSyncDateTime;
                                this.downloadSessionStatusTable(downloadSessions);
                                this.uploadSessionStatusTable(uploadSessions);
                            }

                            asyncResult.resolve();
                        } else {
                            RetailLogger.viewModelGetOfflineSyncStatsFailed(result.statusText);
                            asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.ASYNC_CLIENT_FAILED_TO_GET_OFFLINE_SYNC_STATS)]);
                        }

                        return asyncResult;
                    });
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * loads the number of pending transactions from offline database.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public loadPendingTransactionCount(): IVoidAsyncResult {
            if ((Commerce.Config.offlineDatabase != null) && (Commerce.Config.offlineDatabase.length != 0)) {
                return this.storeOperationsManager.getOfflinePendingTransactionCount()
                    .done((numberOfTransactions) => {
                        if (!isNaN(numberOfTransactions)) {
                            this.pendingTransactionCount(numberOfTransactions.toString());
                        }
                        else {
                            this.pendingTransactionCount(Commerce.ViewModelAdapter.getResourceString("string_6621"));
                        }
                    });
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Toggles the data source connection.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public toggleConnection(): IVoidAsyncResult {
            var managerFactory: Model.Managers.RetailServerManagerFactory = <Model.Managers.RetailServerManagerFactory>Model.Managers.Factory;

            return managerFactory.toggleConnection()
                .done(() => {
                    this.updateState();
                });
        }

        /**
         * Syncs the offline data.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public syncOfflineData(): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult(this);
            if ((Commerce.Config.offlineDatabase != null) && (Commerce.Config.offlineDatabase.length != 0)) {
                if (Session.instance.offlineSyncing() === false) {
                    // Stops the existing timers and kicks off a new one immediately.
                    var asyncServiceViewModel = new Commerce.ViewModels.AsyncServiceViewModel();
                    clearTimeout(Session.instance.offlineParameters.syncDownloadOfflineData);
                    Session.instance.offlineParameters.syncDownloadOfflineData = 0;
                    clearTimeout(Session.instance.offlineParameters.syncUploadOfflineData);
                    Session.instance.offlineParameters.syncUploadOfflineData = 0;
                    asyncServiceViewModel.executeDownload();
                    asyncServiceViewModel.executeUpload();

                    asyncResult.resolve();
                } else {
                    asyncResult.resolve();
                }
            } else {
                asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.OFFLINE_MODE_NOT_SUPPORTED)]);
            }

            return asyncResult;
        }

        /**
         * Shows offline sync status.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public showOfflineSyncStatus(): IVoidAsyncResult {
            if (!Commerce.Utilities.OfflineHelper.isOfflineEnabled() || Session.instance.offlineParameters.offlineModeDisabled) {
                this.offlineSyncStatus(Commerce.ViewModelAdapter.getResourceString("string_6628"));
            } else if (Session.instance.offlineSyncing() === true) {
                this.offlineSyncStatus(Commerce.ViewModelAdapter.getResourceString("string_6626"));
            } else {
                this.offlineSyncStatus(this.getOfflineSyncStatusMessage());
            }

            return VoidAsyncResult.createResolved();
        }

        private getOfflineSyncStatusMessage(): string {
            var lastSyncDateTimeNumber: number = this.lastSyncDateTime.getTime();
            if (lastSyncDateTimeNumber < 0) {
                return Commerce.ViewModelAdapter.getResourceString("string_6627");
            }

            var timeDiffInSeconds: number = DateExtensions.now.getTime() / 1000 - lastSyncDateTimeNumber / 1000;
            if (timeDiffInSeconds < 0) {
                return Commerce.ViewModelAdapter.getResourceString("string_6635");
            } else if (timeDiffInSeconds / (3600 * 24) > this.retentionDay) {
                return Commerce.ViewModelAdapter.getResourceString("string_6630");
            } else if (timeDiffInSeconds / (3600 * 24) > 1) {
                return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_6631"), Math.floor(timeDiffInSeconds / (3600 * 24)));
            } else {
                if (timeDiffInSeconds / 3600 > 1) {
                    return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_6632"), Math.floor(timeDiffInSeconds / 3600));
                } else {
                    if (timeDiffInSeconds / 60 > 1) {
                        return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_6633"), Math.floor(timeDiffInSeconds / 60));
                    } else {
                        return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_6634"), Math.floor(timeDiffInSeconds));
                    }
                }
            }
        }

        private updateState(): void {
            if (Session.instance.connectionStatus == ConnectionStatusType.Online) {
                this.status(Commerce.ViewModelAdapter.getResourceString("string_77"));
            } else {
                this.status(Commerce.ViewModelAdapter.getResourceString("string_78"));
            }
        }
    }
}
