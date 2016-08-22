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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Credit Memo real time service.
        /// </summary>
        public class CreditMemoRealtimeService : IRequestHandler
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
                    response = GetCreditMemo((GetCreditMemoRealtimeRequest)request);
                }
                else if (requestType == typeof(IssueCreditMemoRealtimeRequest))
                {
                    response = IssueCreditMemo((IssueCreditMemoRealtimeRequest)request);
                }
                else if (requestType == typeof(PayCreditMemoRealtimeRequest))
                {
                    response = PayCreditMemo((PayCreditMemoRealtimeRequest)request);
                }
                else if (requestType == typeof(LockCreditMemoRealtimeRequest))
                {
                    response = LockCreditMemo((LockCreditMemoRealtimeRequest)request);
                }
                else if (requestType == typeof(UnlockCreditMemoRealtimeRequest))
                {
                    response = UnlockCreditMemo((UnlockCreditMemoRealtimeRequest)request);
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
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse PayCreditMemo(PayCreditMemoRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                transactionClient.PayCreditMemo(request.CreditMemoId, request.StoreId, request.TerminalId, request.StaffId, request.TransactionId, request.ReceiptId, request.CurrencyCode, request.Amount);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Reserves the credit memo for a given terminal so it cannot be used on other terminals.
            /// </summary>
            /// <param name="request">The <see cref="LockCreditMemoRealtimeRequest"/> request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{CreditMemo}"/> response.</returns>
            private static SingleEntityDataServiceResponse<CreditMemo> LockCreditMemo(LockCreditMemoRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                string cardCurrencyCode;
                decimal amount;
    
                transactionClient.LockCreditMemo(request.CreditMemoId, request.StoreId, request.TerminalId, out cardCurrencyCode, out amount);
                var creditMemo = new CreditMemo
                {
                    Id = request.CreditMemoId,
                    CurrencyCode = cardCurrencyCode,
                    Balance = amount
                };
    
                return new SingleEntityDataServiceResponse<CreditMemo>(creditMemo);
            }
    
            /// <summary>
            /// Removes reservation of the credit memo from a given terminal so it can be used on other terminals.
            /// </summary>
            /// <param name="request">The <see cref="UnlockCreditMemoRealtimeRequest"/> request.</param>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse UnlockCreditMemo(UnlockCreditMemoRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                transactionClient.UnlockCreditMemo(request.CreditMemoId, request.StoreId, request.TerminalId);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Issue credit memo.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{T}"/> response that contains memo id.</returns>
            private static SingleEntityDataServiceResponse<string> IssueCreditMemo(IssueCreditMemoRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                string memoId = transactionClient.IssueCreditMemo(request.StoreId, request.TerminalId, request.StaffId, request.TransactionId, request.ReceiptId, request.CurrencyCode, request.Amount);
    
                return new SingleEntityDataServiceResponse<string>(memoId);
            }
    
            /// <summary>
            /// Gets credit memo by id.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{CreditMemo}"/> response.</returns>
            private static SingleEntityDataServiceResponse<CreditMemo> GetCreditMemo(GetCreditMemoRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                string currencyCode;
                decimal balance;
    
                transactionClient.GetCreditMemo(request.CreditMemoId, out currencyCode, out balance);
    
                var creditMemo = new CreditMemo
                {
                    Id = request.CreditMemoId,
                    CurrencyCode = currencyCode,
                    Balance = balance
                };
    
                return new SingleEntityDataServiceResponse<CreditMemo>(creditMemo);
            }
        }
    }
}
