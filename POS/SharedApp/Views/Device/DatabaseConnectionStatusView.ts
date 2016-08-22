/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class DatabaseConnectionStatusViewController extends ViewControllerBase {
        public viewModel: Commerce.ViewModels.DatabaseConnectionStatusViewModel;
        public commonHeaderData: Commerce.Controls.CommonHeaderData;
        public indeterminateWaitVisible: Observable<boolean>;
        public isCommandEnable: Observable<boolean>;
        public connectionButtonText: Observable<string>;
        public viewDownload: Observable<boolean>;

        constructor() {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.indeterminateWaitVisible = ko.observable(false);
            this.isCommandEnable = ko.observable(false);
            this.connectionButtonText = ko.observable(StringExtensions.EMPTY);
            this.viewModel = new Commerce.ViewModels.DatabaseConnectionStatusViewModel();
            this.viewDownload = ko.observable(true);

            //Load Common Header
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_6600"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_6601"));

            Commerce.Session.instance.offlineSyncing.subscribe(() => {
                this.viewModel.showOfflineSyncStatus();
                this.viewModel.loadOfflineSyncStats();
                // Only load pending transaction count when offline is available.
                if (Session.instance.isOfflineAvailable) {
                    this.viewModel.loadPendingTransactionCount();
                }
            });

            Commerce.Session.instance.pendingDownloadSessionCount.subscribe(() => {
                this.viewModel.pendingDownloadSessionCount(Commerce.Session.instance.pendingDownloadSessionCount().toString());
            });

            this.updateUI();
        }

        public load(): void {
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.viewModel.loadOfflineSyncStats();
                });

            asyncQueue.run()
                .always(() => {
                    this.viewModel.showOfflineSyncStatus();
                });

            // Only load pending transaction count when offline is available.
            if (Session.instance.isOfflineAvailable) {
                this.viewModel.loadPendingTransactionCount();
            }

            this.viewModel.pendingDownloadSessionCount(Commerce.Session.instance.pendingDownloadSessionCount().toString());
        }

        public toggleConnection(): void {
            var errors = [];
            this.indeterminateWaitVisible(true);

            this.viewModel.toggleConnection()
                .done(() => {
                    this.updateUI();
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public syncOfflineData(): void {
            var errors = [];
            this.indeterminateWaitVisible(true);

            this.viewModel.syncOfflineData()
                .done(() => {
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public showDownloadViewHandler() {
            this.viewDownload(true);
        }

        public showUploadViewHandler() {
            this.viewDownload(false);
        }

        private updateUI() {
            if (Session.instance.connectionStatus == ConnectionStatusType.Online) {
                this.connectionButtonText(Commerce.ViewModelAdapter.getResourceString("string_6606"));
            }
            else {
                this.connectionButtonText(Commerce.ViewModelAdapter.getResourceString("string_6605"));
            }
        }
    }
}
