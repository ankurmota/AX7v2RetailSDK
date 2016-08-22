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

    export class ReportsViewController extends ViewControllerBase {
        private ViewModel: Commerce.ViewModels.ReportsViewModel;
        public commonHeaderData;

        constructor(options?: any) {
            super(true);

            this.ViewModel = new Commerce.ViewModels.ReportsViewModel(options);
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
           
             // Load Common Header
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_2400")); // Reports
            // Set report title based on connection status online/offline
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2409")); // Reports
            } else {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2410")); // Reports
            }
        }

        /**
         * Report invoked click handler.
         *
         * @param {Commerce.TileList.IItemInvokedArgs} eventArgs The event args containing the selected data
         */
        private ReportInvokedHandler(eventArgs: Commerce.TileList.IItemInvokedArgs) {
            Commerce.ViewModelAdapter.navigate("ReportDetailsView", eventArgs.data);
        }
    }
}