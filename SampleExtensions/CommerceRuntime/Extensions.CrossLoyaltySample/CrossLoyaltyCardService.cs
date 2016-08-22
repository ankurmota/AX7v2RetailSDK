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
    namespace Commerce.Runtime.CrossLoyaltySample
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Messages;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Service class responsible executing the service requests.
        /// </summary>
        public class CrossLoyaltyCardService : SingleRequestHandler<GetCrossLoyaltyCardRequest, GetCrossLoyaltyCardResponse>
        {
            /// <summary>
            /// Process method. 
            /// </summary>
            /// <param name="request">The request with the loyalty number.</param>
            /// <returns>The discount value.</returns>
            protected override GetCrossLoyaltyCardResponse Process(GetCrossLoyaltyCardRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                var serviceResponse = new GetCrossLoyaltyCardResponse(0);

                // do the actual work here
                if (string.Equals(request.LoyaltyCardNumber, "425-999-2222"))
                {
                    serviceResponse = new GetCrossLoyaltyCardResponse(2);
                }
                else if (string.Equals(request.LoyaltyCardNumber, "425-999-3333"))
                {
                    serviceResponse = new GetCrossLoyaltyCardResponse(3);
                }
                else if (string.Equals(request.LoyaltyCardNumber, "425-999-4444"))
                {
                    serviceResponse = new GetCrossLoyaltyCardResponse(4);
                }
                else if (string.Equals(request.LoyaltyCardNumber, "425-999-5555"))
                {
                    serviceResponse = new GetCrossLoyaltyCardResponse(5);
                }

                string logMessage = "GetCrossLoyaltyCardRequest successfully handled with card number '{0}'. Returning discount '{1}'.";
                RetailLogger.Log.ExtendedInformationalEvent(logMessage, request.LoyaltyCardNumber, serviceResponse.Discount.ToString());

                return serviceResponse;
            }
        }
    }
}
