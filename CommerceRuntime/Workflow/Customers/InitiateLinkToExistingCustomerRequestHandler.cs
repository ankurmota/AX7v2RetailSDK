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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Encapsulates the workflow to link the current user to an existing customer. 
        /// </summary>
        public sealed class InitiateLinkToExistingCustomerRequestHandler : SingleRequestHandler<InitiateLinkToExistingCustomerRequest, LinkToExistingCustomerResponse>
        {
            private const string ActivationTokenPropertyName = "ActivationToken";

            /// <summary>
            /// Executes the workflow to send an e-mail to the specified customer.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override LinkToExistingCustomerResponse Process(InitiateLinkToExistingCustomerRequest request)
            {
                ThrowIf.Null(request, "request");

                if (string.IsNullOrWhiteSpace(request.EmailAddressOfExistingCustomer))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "An email address is required.");
                }

                ICommercePrincipal principal = request.RequestContext.GetPrincipal();
                string externalIdentityId = principal.ExternalIdentityId;
                string externalIdentityIssuer = principal.ExternalIdentityIssuer;

                if (principal.IsEmployee)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "Employee user is not authorized to perform this operation.");    
                }

                if (string.IsNullOrEmpty(externalIdentityId))
                {
                    throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "The external identity is not set on the pricinpal");
                }

                if (string.IsNullOrEmpty(externalIdentityIssuer))
                {
                    throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "The external identity issuer is not set on the pricinpal");
                }

                // Validate the customer
                GlobalCustomer customer = this.GetCustomer(request.EmailAddressOfExistingCustomer);
                if (customer == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerNotFound,
                        string.Format("No customer found using the specified email address. The email address is '{0}'.", request.EmailAddressOfExistingCustomer));
                }

                ICollection<NameValuePair> updatedEmailTemplateProperties = InitiateLinkToExistingCustomerRequestHandler.GetUpdatedEmailTemplateProperties(request.EmailTemplateProperties, request.ActivationToken);

                // Send email to customer
                this.SendCustomerEmail(customer.Email, request.EmailTemplateId, updatedEmailTemplateProperties);

                // Save the request to channel DB
                var serviceRequest = new InitiateLinkToExistingCustomerServiceRequest(
                    request.EmailAddressOfExistingCustomer,
                    request.ActivationToken,
                    externalIdentityId,
                    externalIdentityIssuer,
                    customer.AccountNumber);

                var serviceResponse = this.Context.Execute<LinkToExistingCustomerServiceResponse>(serviceRequest);

                return new LinkToExistingCustomerResponse(serviceResponse.LinkToExistingCustomerResult);
            }

            private static ICollection<NameValuePair> GetUpdatedEmailTemplateProperties(IEnumerable<NameValuePair> emailTemplateProperties, string activationToken)
            {
                List<NameValuePair> updatedEmailTemplateProperties = emailTemplateProperties != null ? emailTemplateProperties.ToList() : new List<NameValuePair>();

                NameValuePair activationTokenProperty = updatedEmailTemplateProperties.SingleOrDefault(x => string.Equals(x.Name, InitiateLinkToExistingCustomerRequestHandler.ActivationTokenPropertyName, System.StringComparison.OrdinalIgnoreCase));

                if (activationTokenProperty == null)
                {
                    activationTokenProperty = new NameValuePair()
                    {
                        Name = InitiateLinkToExistingCustomerRequestHandler.ActivationTokenPropertyName,
                        Value = activationToken,
                    };

                    updatedEmailTemplateProperties.Add(activationTokenProperty);
                }
                else
                {
                    activationTokenProperty.Value = activationToken;
                }

                return updatedEmailTemplateProperties;
            }

            private GlobalCustomer GetCustomer(string emailAddress)
            {
                CustomerSearchCriteria criteria = new CustomerSearchCriteria { Keyword = emailAddress };
                var searchCustomerRequest = new CustomersSearchServiceRequest(criteria, QueryResultSettings.AllRecords);
                var searchCustomerResponse = this.Context.Execute<CustomersSearchServiceResponse>(searchCustomerRequest);
                var matchedSearchResults = searchCustomerResponse.Customers.Results.Where(c => emailAddress.Equals(c.Email, System.StringComparison.OrdinalIgnoreCase));

                GlobalCustomer foundCustomer = matchedSearchResults.FirstOrDefault();

                return foundCustomer;
            }

            private void SendCustomerEmail(string email, string templateId, ICollection<NameValuePair> properties)
            {
                var serviceRequest = new SendEmailRealtimeRequest(
                    email,
                    properties,
                    this.Context.GetChannelConfiguration().DefaultLanguageId,
                    xmlData: null,
                    emailId: templateId);

                this.Context.Execute<NullResponse>(serviceRequest);
            }
        }
    }
}
