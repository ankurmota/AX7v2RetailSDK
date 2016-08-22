/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

namespace Contoso
{
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Gift card demo mode transaction service.
        /// </summary>
        public class GiftCardTransactionServiceDemoMode : IRequestHandler
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
                        typeof(GetGiftCardRealtimeRequest),
                        typeof(IssueGiftCardRealtimeRequest),
                        typeof(AddToGiftCardRealtimeRequest),
                        typeof(PayGiftCardRealtimeRequest),
                        typeof(VoidGiftCardRealtimeRequest),
                        typeof(VoidGiftCardPaymentRealtimeRequest),
                        typeof(LockGiftCardRealtimeRequest),
                        typeof(UnlockGiftCardRealtimeRequest)
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
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetGiftCardRealtimeRequest))
                {
                    response = GetGiftCard();
                }
                else if (requestType == typeof(IssueGiftCardRealtimeRequest))
                {
                    response = IssueGiftCard();
                }
                else if (requestType == typeof(AddToGiftCardRealtimeRequest))
                {
                    response = AddToGiftCard();
                }
                else if (requestType == typeof(PayGiftCardRealtimeRequest))
                {
                    response = PayGiftCard();
                }
                else if (requestType == typeof(VoidGiftCardRealtimeRequest))
                {
                    response = VoidGiftCard();
                }
                else if (requestType == typeof(VoidGiftCardPaymentRealtimeRequest))
                {
                    response = VoidGiftCardPayment();
                }
                else if (requestType == typeof(LockGiftCardRealtimeRequest))
                {
                    response = LockGiftCard();
                }
                else if (requestType == typeof(UnlockGiftCardRealtimeRequest))
                {
                    response = UnlockGiftCard();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Pay and unlock gift card.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> PayGiftCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "PayGiftCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Reserves the gift card for a given terminal so it cannot be used on other terminals.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> LockGiftCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "LockGiftCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Removes reservation of the gift card from a given terminal so it can be used on other terminals.
            /// </summary>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse UnlockGiftCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "UnlockGiftCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Voids gift card.
            /// </summary>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse VoidGiftCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "VoidGiftCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Voids gift card payment.
            /// </summary>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse VoidGiftCardPayment()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "VoidGiftCardPayment is not supported in demo mode.");
            }
    
            /// <summary>
            /// Issue gift card.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response that contains gift card.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> IssueGiftCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "IssueGiftCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Add to gift card.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response that contains gift card.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> AddToGiftCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "AddToGiftCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Gets gift card by id.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> GetGiftCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetGiftCard is not supported in demo mode.");
            }
        }
    }
}
