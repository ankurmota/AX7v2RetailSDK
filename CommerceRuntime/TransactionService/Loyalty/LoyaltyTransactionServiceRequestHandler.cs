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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Loyalty transaction service request handler class.
        /// </summary>
        public class LoyaltyTransactionServiceRequestHandler : IRequestHandler
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
                        typeof(IssueLoyaltyCardRealtimeRequest),
                        typeof(GetLoyaltyCardTransactionsRealtimeRequest),
                        typeof(GetLoyaltyCardRewardPointsStatusRealtimeRequest),
                        typeof(PostLoyaltyCardRewardPointRealtimeRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
    
                Response response;
                if (requestType == typeof(IssueLoyaltyCardRealtimeRequest))
                {
                    response = IssueLoyaltyCard((IssueLoyaltyCardRealtimeRequest)request);
                }
                else if (requestType == typeof(GetLoyaltyCardTransactionsRealtimeRequest))
                {
                    response = GetLoyaltyCard((GetLoyaltyCardTransactionsRealtimeRequest)request);
                }
                else if (requestType == typeof(GetLoyaltyCardRewardPointsStatusRealtimeRequest))
                {
                    response = GetLoyaltyCardRewardPointsStatus((GetLoyaltyCardRewardPointsStatusRealtimeRequest)request);
                }
                else if (requestType == typeof(PostLoyaltyCardRewardPointRealtimeRequest))
                {
                    response = PostLoyaltyCardRewardPointTransaction((PostLoyaltyCardRewardPointRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Issues loyalty card from AX.
            /// </summary>
            /// <param name="request">The issue loyalty card request.</param>
            /// <returns>The issue loyalty card response.</returns>
            private static IssueLoyaltyCardRealtimeResponse IssueLoyaltyCard(IssueLoyaltyCardRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                LoyaltyCard loyaltyCard = transactionService.IssueLoyaltyCard(
                    request.LoyaltyCardNumber,
                    request.LoyaltyCardTenderType,
                    request.PartyRecordId,
                    request.ChannelId);
    
                return new IssueLoyaltyCardRealtimeResponse(loyaltyCard);
            }
    
            /// <summary>
            /// Gets loyalty card transactions from AX.
            /// </summary>
            /// <param name="request">The get loyalty card transactions request.</param>
            /// <returns>The get loyalty card transactions response.</returns>
            private static GetLoyaltyCardTransactionsRealtimeResponse GetLoyaltyCard(GetLoyaltyCardTransactionsRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                PagedResult<LoyaltyCardTransaction> transactions = transactionService.GetLoyaltyCardTransactions(
                    request.LoyaltyCardNumber,
                    request.RewardPointId,
                    request.QueryResultSettings.Paging.Top,
                    request.QueryResultSettings.Paging.Skip,
                    request.QueryResultSettings.Paging.CalculateRecordCount);
    
                return new GetLoyaltyCardTransactionsRealtimeResponse(transactions);
            }
    
            /// <summary>
            /// Gets loyalty card reward points status from AX.
            /// </summary>
            /// <param name="request">The get loyalty card reward points status request.</param>
            /// <returns>The get loyalty card reward points status response.</returns>
            private static EntityDataServiceResponse<LoyaltyCard> GetLoyaltyCardRewardPointsStatus(GetLoyaltyCardRewardPointsStatusRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                Collection<LoyaltyCard> cardsStatus = transactionService.GetLoyaltyCardRewardPointsStatus(
                    request.ChannelLocalDate,
                    request.LoyaltyCardNumber,
                    request.ExcludeBlocked,
                    request.ExcludeNoTender,
                    request.IncludeRelatedCardsForContactTender,
                    request.IncludeNonRedeemablePoints,
                    request.IncludeActivePointsOnly,
                    request.RequestContext.LanguageId);
    
                return new EntityDataServiceResponse<LoyaltyCard>(cardsStatus.AsPagedResult());
            }
    
            /// <summary>
            /// Posts loyalty card reward point transaction in AX.
            /// </summary>
            /// <param name="request">The post loyalty card reward point transaction request.</param>
            /// <returns>A loyalty card reward point transaction response.</returns>
            private static NullResponse PostLoyaltyCardRewardPointTransaction(PostLoyaltyCardRewardPointRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                transactionService.PostLoyaltyCardRewardPointTrans(
                    request.Transaction,
                    request.EntryType,
                    request.RequestContext.GetChannelConfiguration());
    
                return new NullResponse();
            }
        }
    }
}
