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
    export class SalesInvoicesViewModel extends ViewModelBase {

        /**
         * Gets all sales invoices related to a sales order.
         * @param {string} salesId the order's id.
         * @returns {IAsyncResult<Model.Entities.SalesInvoice[]>} async result with sales invoices.
         */
        public getSalesInvoicesBySalesId(salesId: string): IAsyncResult<Model.Entities.SalesInvoice[]> {
            return this.salesOrderManager.getSalesInvoicesBySalesIdAsync(salesId);
        }
    }
}