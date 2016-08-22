/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// CustomerHelper class for the Customers workflow.
        /// </summary>
        internal static class CustomerHelper
        {
            /// <summary>
            /// Validate customer addresses.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="customerAddresses">The customer addresses.</param>
            internal static void ValidateAddresses(RequestContext context, IEnumerable<Address> customerAddresses)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(customerAddresses, "customerAddresses");
    
                Collection<DataValidationFailure> validationFailures = new Collection<DataValidationFailure>();
    
                foreach (Address customerAddress in customerAddresses)
                {
                    // don't validate those addresses that are selected for deactivation/delete
                    if (customerAddress.Deactivate)
                    {
                        continue;
                    }
    
                    // don't validate if this address is empty and the recid=0
                    if (customerAddress.IsEmpty() && customerAddress.RecordId == 0)
                    {
                        continue;
                    }
    
                    // Validates the address
                    ValidateAddressDataRequest validateAddressRequest = new ValidateAddressDataRequest(customerAddress);
                    ValidateAddressDataResponse response = context.Execute<ValidateAddressDataResponse>(validateAddressRequest);
    
                    if (!response.IsAddressValid)
                    {
                        validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAddress, response.InvalidAddressComponentName));
                    }
                }
    
                if (validationFailures.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, validationFailures, "Customer address validation failures.");
                }
            }
        }
    }
}
