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
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using DM = Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// CreateCustomerRequest class.
        /// </summary>
        public sealed class CreateCustomerRequestHandler : SingleRequestHandler<CreateCustomerRequest, CreateCustomerResponse>
        {
            private const string NewCustomerEmailId = "NewCust";
            private const string MappingFieldEmail = "Email";
            private const string MappingFieldName = "Name";
            private const string MappingFieldPhone = "Phone";
            private const string MappingFieldUrl = "Url";
            private const string MappingFieldAccountNumber = "AccountNumber";
    
            /// <summary>
            /// Executes the workflow to create the customer.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override CreateCustomerResponse Process(CreateCustomerRequest request)
            {
                ThrowIf.Null(request, "request");
    
                CustomerHelper.ValidateAddresses(this.Context, request.NewCustomer.Addresses);
    
                // Checking whether this is context of RetailStore or not because DeviceConfiguration exists only for RetailStores and not for OnlineStores
                if (this.Context.GetPrincipal().IsEmployee)
                {
                    DeviceConfiguration deviceConfiguration = request.RequestContext.GetDeviceConfiguration();
                    System.Diagnostics.Debug.WriteLine(deviceConfiguration.CreateAsyncCustomers);
    
                    if (deviceConfiguration.CreateAsyncCustomers)
                    {
                        request.NewCustomer.IsAsyncCustomer = true;
                    }
                }
    
                // save new customer
                var saveCustomerServiceRequest = new SaveCustomerServiceRequest(request.NewCustomer);
                var saveCustomerServiceResponse = this.Context.Execute<SaveCustomerServiceResponse>(saveCustomerServiceRequest);

                Customer addedCustomer = saveCustomerServiceResponse.UpdatedCustomer;
    
                if (addedCustomer != null && !string.IsNullOrWhiteSpace(addedCustomer.Email))
                {
                    ICollection<NameValuePair> mappings = GetCreateCustomerEmailMappings(addedCustomer);
    
                    // Send new customer email to customer
                    var sendCustomerEmailServiceRequest = new SendEmailRealtimeRequest(addedCustomer.Email, mappings, addedCustomer.Language ?? CultureInfo.CurrentUICulture.ToString(), null, NewCustomerEmailId);
    
                    // don't fail the customer creation if there is is an error sending the email, log the error.
                    try
                    {
                        this.Context.Execute<NullResponse>(sendCustomerEmailServiceRequest);
                    }
                    catch (Exception ex)
                    {
                        RetailLogger.Log.CrtWorkflowCreateCustomerEmailFailure(ex);
                    }
                }
    
                return new CreateCustomerResponse(addedCustomer);
            }
    
            private static ICollection<NameValuePair> GetCreateCustomerEmailMappings(DM.Customer addedCustomer)
            {
                ICollection<NameValuePair> mappings = new List<NameValuePair>();
    
                mappings.Add(new NameValuePair { Name = MappingFieldName, Value = addedCustomer.Name });
                mappings.Add(new NameValuePair { Name = MappingFieldEmail, Value = addedCustomer.Email });
                mappings.Add(new NameValuePair { Name = MappingFieldPhone, Value = addedCustomer.Phone });
                mappings.Add(new NameValuePair { Name = MappingFieldUrl, Value = addedCustomer.Url });
                mappings.Add(new NameValuePair { Name = MappingFieldAccountNumber, Value = addedCustomer.AccountNumber });
    
                return mappings;
            }
        }
    }
}
