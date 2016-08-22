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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the workflow to send an e-mail to a specified user.
        /// </summary>
        public sealed class SendCustomerEmailRequestHandler : SingleRequestHandler<SendCustomerEmailRequest, NullResponse>
        {
            /// <summary>
            /// Executes the workflow to send an e-mail to the specified customer.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override NullResponse Process(SendCustomerEmailRequest request)
            {
                ThrowIf.Null(request, "request");
    
                Customer customer = this.GetCustomer(request.CustomerAccountNumber);
                if (customer == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerNotFound, "No customer found using the specified customer account number.");
                }
    
                if (string.IsNullOrWhiteSpace(customer.Email))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "No e-mail address exists for the specified customer.");
                }
    
                this.SendCustomerEmail(customer, request.EmailTemplateId, request.EmailTemplateProperties);
    
                return new NullResponse();
            }
    
            private Customer GetCustomer(string customerAccountNumber)
            {
                // No need to worry about the QueryResultSettings since we expect at most a single result.
                var serviceRequest = new GetCustomersServiceRequest(QueryResultSettings.SingleRecord, customerAccountNumber);
                var serviceResponse = this.Context.Execute<GetCustomersServiceResponse>(serviceRequest);
    
                return serviceResponse.Customers.SingleOrDefault();
            }
    
            private void SendCustomerEmail(Customer customer, string templateId, ICollection<NameValuePair> properties)
            {
                var serviceRequest = new SendEmailRealtimeRequest(
                    customer.Email,
                    properties,
                    customer.Language ?? this.Context.LanguageId,
                    null /*xmlData*/,
                    templateId);
    
                this.Context.Execute<NullResponse>(serviceRequest);
            }
        }
    }
}
