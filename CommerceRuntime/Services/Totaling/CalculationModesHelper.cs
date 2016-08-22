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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Helper class for calculation modes.
        /// </summary>
        internal static class CalculationModesHelper
        {
            /// <summary>
            /// Gets calculation modes for customer order.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The service response.</returns>
            public static GetCustomerOrderCalculationModesServiceResponse GetCalculationModes(GetCustomerOrderCalculationModesServiceRequest request)
            {
                CalculationModes disallowedModes;
                SalesTransaction salesTransaction = request.SalesTransaction;

                switch (salesTransaction.CustomerOrderMode)
                {
                    case CustomerOrderMode.Return:
                        // return transaction is created from invoice
                        disallowedModes = CalculationModes.Charges | CalculationModes.Discounts | CalculationModes.Prices;
                        break;

                    case CustomerOrderMode.Pickup:
                    case CustomerOrderMode.Cancellation:
                    case CustomerOrderMode.QuoteCreateOrEdit:
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                    case CustomerOrderMode.OrderRecalled:
                        disallowedModes = CalculationModes.None;
                        break;

                    default:
                        string message = string.Format(CultureInfo.InvariantCulture, "Customer order mode {0} is not supported.", salesTransaction.CustomerOrderMode);
                        throw new NotSupportedException(message);
                }

                // ~ will invert all bits - this will be the allowed modes
                CalculationModes allowedModes = ~disallowedModes;

                return new GetCustomerOrderCalculationModesServiceResponse(allowedModes);
            }
        }
    }
}
