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
        using System.Globalization;
        using Commerce.Runtime.TransactionService;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// Email Service class.
        /// </summary>
        public class EmailService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(SendEmailRealtimeRequest)
                };
                }
            }

            /// <summary>
            /// Executes the service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                var sendEmailRequest = request as SendEmailRealtimeRequest;
                if (sendEmailRequest != null)
                {
                    return SendEmailToCustomer(sendEmailRequest);
                }

                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
            }

            /// <summary>
            /// Sends an email to the requested customer using the email template defined in AX.
            /// </summary>
            /// <param name="request">Request containing the customer and email template identifier.</param>
            /// <returns>SendCustomerEmailServiceResponse object.</returns>
            private static NullResponse SendEmailToCustomer(SendEmailRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);

                transactionService.SendEmail(request.EmailAddress, request.Language, request.EmailId, request.Mappings, request.XmlData);

                return new NullResponse();
            }
        }
    }
}
