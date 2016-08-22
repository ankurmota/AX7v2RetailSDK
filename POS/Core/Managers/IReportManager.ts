/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IReportManagerName: string = "IReportManager";
    

    export interface IReportManager {
        /**
         * Get all reports.
         * @param {string} locale The locale.
         * @return {IAsyncResult<Entities.ReportDataSet>} The async result.
         */
        getListOfReportsAsync(locale: string): IAsyncResult<Entities.ReportDataSet>;

        /**
         * Get report output.
         * @param {string} reportId The report identifier.
         * @param {Entities.CommerceProperty[]} parameters The parameters array.
         * @param {string} locale The locale.
         * @return {IAsyncResult<Entities.ReportDataSet>} The async result.
         */
        getReportsDataAsync(reportId: string, parameters: Entities.CommerceProperty[], locale: string): IAsyncResult<Entities.ReportDataSet>;
    }
}
