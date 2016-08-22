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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// FedEx Adapter Implementation.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Refers to company name.")]
    
        public sealed class FedExService : INamedRequestHandler
        {
            /// <summary>
            /// Gets the unique name for this request handler.
            /// </summary>
            public string HandlerName
            {
                get { return "FedEx"; }
            }
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetShippingRateFromCarrierServiceRequest),
                        typeof(GetTrackingInformationFromCarrierServiceRequest),
                        typeof(ValidateShippingAddressCarrierServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
                Type requestType = request.GetType();
    
                if (requestType == typeof(GetShippingRateFromCarrierServiceRequest))
                {
                    response = GetShippingRate((GetShippingRateFromCarrierServiceRequest)request);
                }
                else if (requestType == typeof(GetTrackingInformationFromCarrierServiceRequest))
                {
                    response = GetTrackingDetails((GetTrackingInformationFromCarrierServiceRequest)request);
                }
                else if (requestType == typeof(ValidateShippingAddressCarrierServiceRequest))
                {
                    response = ValidateShippingAddress((ValidateShippingAddressCarrierServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request));
                }
    
                return response;
            }
    
            /// <summary>
            /// Validates the shipping address.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The address validation response.
            /// </returns>
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "Sample code.")]
            private static ValidateShippingAddressCarrierServiceResponse ValidateShippingAddress(ValidateShippingAddressCarrierServiceRequest request)
            {
                // Implement the webservice to call to validate the address.
                bool isValid = true;
    
                return new ValidateShippingAddressCarrierServiceResponse(isValid, new Collection<Address>());
            }
    
            /// <summary>
            /// Gets the shipping rate.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The shipping Rate response from carrier.
            /// </returns>
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "Sample code.")]
            private static GetShippingRateFromCarrierServiceResponse GetShippingRate(GetShippingRateFromCarrierServiceRequest request)
            {
                // Implement the carrier service call to compute the shipping.
                decimal rates = 0;
                return new GetShippingRateFromCarrierServiceResponse(rates);
            }
    
            /// <summary>
            /// Gets the tracking details.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The tracking details response from carrier.
            /// </returns>
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "Sample code.")]
            private static GetTrackingInformationFromCarrierServiceResponse GetTrackingDetails(GetTrackingInformationFromCarrierServiceRequest request)
            {
                return new GetTrackingInformationFromCarrierServiceResponse(new Collection<TrackingInfo>().AsPagedResult());
            }
        }
    }
}
