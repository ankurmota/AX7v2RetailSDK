/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";
    export class ImageDisplayHelper{

         /**
         * Returns string of image url or base64 based on connection status.
         *
         * @param {Model.Entities.Customer} customer the customer.
         * @return {string} image url or base64 string.
         */
        static GetCustomerImageContent(customer: Commerce.Model.Entities.Customer): string {
            // Here no further validation is required. This will send the correct image content to the converter.
            // In converter, this input will be validated.
            if (Commerce.Session.instance.connectionStatus == Commerce.ConnectionStatusType.Online) {
                if (!Commerce.ObjectExtensions.isNullOrUndefined(customer)
                    && ArrayExtensions.hasElements(customer.Images) 
                    && !Commerce.StringExtensions.isNullOrWhitespace(customer.Images[0].Uri)) {
                    return customer.Images[0].Uri;
                }
            }
            else {
                if (!Commerce.ObjectExtensions.isNullOrUndefined(customer)
                    && !Commerce.ObjectExtensions.isNullOrUndefined(customer.OfflineImage)) {
                    return customer.OfflineImage;
                }
            }
        }
    }

}