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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Gets the commerce lists specified by the request.
        /// </summary>
        public sealed class GetCommerceListRequestHandler : SingleRequestHandler<GetCommerceListRequest, GetCommerceListResponse>
        {
            /// <summary>
            /// Gets the commerce lists specified by the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="GetCommerceListResponse"/>Object containing the commerce lists.</returns>
            protected override GetCommerceListResponse Process(GetCommerceListRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                
                // Async customers do not have commerce lists.
                if (this.IsAsyncCustomer(request.RequestContext, request.CustomerAccountNumber))
                {
                    return new GetCommerceListResponse(PagedResult<CommerceList>.Empty());
                }
    
                // Employee can read any commerce lists.
                // Customer can only read lists owned by him or shared to him.
                // Anonymous is not allowed to read commerce lists.
                ICommercePrincipal principal = request.RequestContext.GetPrincipal();
                if (principal.IsAnonymous)
                {
                    throw new NotSupportedException("Anonymous is not allowed to read commerce lists.");
                }
                else if (principal.IsCustomer)
                {
                    request.CustomerAccountNumber = principal.UserId;
                }
    
                // Form a request and get the response
                GetCommerceListRealtimeRequest serviceRequest = new GetCommerceListRealtimeRequest
                {
                    Id = request.Id,
                    CustomerAccountNumber = request.CustomerAccountNumber,
                    FavoriteFilter = request.FavoriteFilter,
                    PublicFilter = request.PublicFilter,
                    QueryResultSettings = request.QueryResultSettings
                };
    
                GetCommerceListRealtimeResponse serviceResponse = this.Context.Execute<GetCommerceListRealtimeResponse>(serviceRequest);
                return new GetCommerceListResponse(serviceResponse.Clists);            
            }

            /// <summary>
            /// Check if an account number maps to an async customer.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="customerAccountNumber">The customer account number.</param>
            /// <returns>A boolean indicating whether or not the customer is an async customer.</returns>
            private bool IsAsyncCustomer(RequestContext context, string customerAccountNumber)
            {
                if (!string.IsNullOrWhiteSpace(customerAccountNumber))
                {
                    var getCustomersServiceRequest = new GetCustomersServiceRequest(QueryResultSettings.SingleRecord, customerAccountNumber);
                    var getCustomersServiceResponse = context.Execute<GetCustomersServiceResponse>(getCustomersServiceRequest);

                    if (getCustomersServiceResponse.Customers.FirstOrDefault().IsAsyncCustomer)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
