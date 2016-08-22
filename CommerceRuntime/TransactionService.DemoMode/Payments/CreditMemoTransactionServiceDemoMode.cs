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
        /// Credit Memo demo mode transaction service.
        /// </summary>
        public class CreditMemoTransactionServiceDemoMode : IRequestHandler
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
                        typeof(GetCreditMemoRealtimeRequest),
                        typeof(IssueCreditMemoRealtimeRequest),
                        typeof(PayCreditMemoRealtimeRequest),
                        typeof(LockCreditMemoRealtimeRequest),
                        typeof(UnlockCreditMemoRealtimeRequest)
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
                if (requestType == typeof(GetCreditMemoRealtimeRequest))
                {
                    response = GetCreditMemo();
                }
                else if (requestType == typeof(IssueCreditMemoRealtimeRequest))
                {
                    response = IssueCreditMemo();
                }
                else if (requestType == typeof(PayCreditMemoRealtimeRequest))
                {
                    response = PayCreditMemo();
                }
                else if (requestType == typeof(LockCreditMemoRealtimeRequest))
                {
                    response = LockCreditMemo();
                }
                else if (requestType == typeof(UnlockCreditMemoRealtimeRequest))
                {
                    response = UnlockCreditMemo();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Pay by credit memo.
            /// </summary>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse PayCreditMemo()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "PayCreditMemo is not supported in demo mode.");
            }
    
            /// <summary>
            /// Reserves the credit memo for a given terminal so it cannot be used on other terminals.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{CreditMemo}"/> response.</returns>
            private static SingleEntityDataServiceResponse<CreditMemo> LockCreditMemo()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "LockCreditMemo is not supported in demo mode.");
            }
    
            /// <summary>
            /// Removes reservation of the credit memo from a given terminal so it can be used on other terminals.
            /// </summary>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse UnlockCreditMemo()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "UnlockCreditMemo is not supported in demo mode.");
            }
    
            /// <summary>
            /// Issue credit memo.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{T}"/> response that contains memo id.</returns>
            private static SingleEntityDataServiceResponse<string> IssueCreditMemo()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "IssueCreditMemo is not supported in demo mode.");
            }
    
            /// <summary>
            /// Gets credit memo by id.
            /// </summary>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{CreditMemo}"/> response.</returns>
            private static SingleEntityDataServiceResponse<CreditMemo> GetCreditMemo()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetCreditMemo is not supported in demo mode.");
            }
        }
    }
}
