/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IReportManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class ReportManager implements Commerce.Model.Managers.IReportManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get all reports.
         * @return {IAsyncResult<Entities.ReportDataSet>} The async result.
         */
        public getListOfReportsAsync(): IAsyncResult<Entities.ReportDataSet> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().searchReportDataSet("ALL", null);

            return request.execute<Entities.ReportDataSet>();
        }

        /**
         * Get report output.
         * @param {string} reportId The report identifier.
         * @param {Entities.CommerceProperty[]} parameters The parameters array.
         * @return {IAsyncResult<Entities.ReportDataSet>} The async result.
         */
        public getReportsDataAsync(reportId: string, parameters: Entities.CommerceProperty[]): IAsyncResult<Entities.ReportDataSet> {
            if (!ArrayExtensions.hasElements(parameters)) {
                reportId = "ALL";
            }

            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().searchReportDataSet(reportId, parameters);

            return request.execute<Entities.ReportDataSet>();
        }
    }
}
