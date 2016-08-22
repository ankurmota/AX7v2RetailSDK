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

    export class ReportDetailsViewController extends ViewControllerBase {
        private ViewModel: Commerce.ViewModels.ReportDetailsViewModel;
        public commonHeaderData;

        constructor(report) {
            super(true);

            this.ViewModel = new Commerce.ViewModels.ReportDetailsViewModel(report);
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            // Load Common Header
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(this.ViewModel.ReportTitle());

            // Set report title based on connection status online/offline
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2409")); // Reports
            } else {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2410")); // Reports
            }
        }

        public RunReportHandler() {
            this.ViewModel.GetReportDataFromParameters().done(() => {
                this.GetReportDataSuccessCallBack();
            }).fail((errors: Model.Entities.Error[]) => {
                NotificationHandler.displayClientErrors(errors);
            });
        }

        private GetReportDataSuccessCallBack() {
            var options = {
                "ReportID": this.ViewModel.Report.REPORTID,
                "ReportTitle": this.ViewModel.ReportTitle,
                "Output": this.ViewModel.Output,
                "ResultsCaption": this.ViewModel.ResultsCaption,
                "Charts": this.ViewModel.Charts,
                "HasTotalRow": this.ViewModel.HasTotalRow,
                "HasDisclaimer": this.ViewModel.HasDisclaimer
            };

            if (this.ViewModel.Output.length > 0) {
                // only navigates to results view when report is not empty
                Commerce.ViewModelAdapter.navigate("ReportResultsView", options);
            } else {
                NotificationHandler.displayErrorMessage("string_2403");
            }

        }
    }
}
