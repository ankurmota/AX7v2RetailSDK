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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Encapsulates the implementation of the loyalty service.
        /// </summary>
        public class LoyaltyService : IRequestHandler
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
                        typeof(GetLoyaltyCardStatusServiceRequest),
                        typeof(CalculateLoyaltyRewardPointsServiceRequest),
                        typeof(IssueLoyaltyCardServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Execute the service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The response.
            /// </returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetLoyaltyCardStatusServiceRequest))
                {
                    response = GetLoyaltyCardStatus((GetLoyaltyCardStatusServiceRequest)request);
                }
                else if (requestType == typeof(CalculateLoyaltyRewardPointsServiceRequest))
                {
                    response = CalculateLoyaltyRewardPoints((CalculateLoyaltyRewardPointsServiceRequest)request);
                }
                else if (requestType == typeof(IssueLoyaltyCardServiceRequest))
                {
                    response = IssueLoyaltyCard((IssueLoyaltyCardServiceRequest)request);
                }
                else
                {
                    RetailLogger.Log.CrtServicesUnsupportedRequestType(request.GetType(), "LoyaltyService");
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                NetTracer.Information("Completed Loyalty.Execute");
                return response;
            }
    
            /// <summary>
            /// Gets the transactions for a loyalty card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response containing the loyalty card transactions.</returns>
            private static IssueLoyaltyCardServiceResponse IssueLoyaltyCard(IssueLoyaltyCardServiceRequest request)
            {
                var serviceRequest = new IssueLoyaltyCardRealtimeRequest(
                    request.LoyaltyCardNumber,
                    request.LoyaltyCardTenderType,
                    request.PartyRecordId,
                    request.ChannelId);
    
                IssueLoyaltyCardRealtimeResponse serviceResponse = request.RequestContext.Execute<IssueLoyaltyCardRealtimeResponse>(serviceRequest);
                LoyaltyCard loyaltyCard = serviceResponse.LoyaltyCard;
    
                if (loyaltyCard != null && loyaltyCard.PartyRecordId != 0)
                {
                    loyaltyCard.CustomerAccount = request.CustomerAccountNumber;
                }
    
                // Insert the issue loyalty card into the channel database
                var insertLoyaltyCardDataRequest = new InsertLoyaltyCardDataRequest(loyaltyCard);
                request.RequestContext.Execute<NullResponse>(insertLoyaltyCardDataRequest);
    
                return new IssueLoyaltyCardServiceResponse(loyaltyCard);
            }
    
            /// <summary>
            /// Gets the loyalty card status including the loyalty groups and the reward points status.
            /// </summary>
            /// <param name="request">The request containing the card number.</param>
            /// <returns>The response containing the loyalty card status.</returns>
            private static GetLoyaltyCardStatusServiceResponse GetLoyaltyCardStatus(GetLoyaltyCardStatusServiceRequest request)
            {
                // Get loyalty card basic information
                var getLoyaltyCardDataRequest = new GetLoyaltyCardDataRequest(request.LoyaltyCardNumber);
                LoyaltyCard loyaltyCard = request.RequestContext.Execute<SingleEntityDataServiceResponse<LoyaltyCard>>(getLoyaltyCardDataRequest).Entity;

                if (loyaltyCard == null)
                {
                    return new GetLoyaltyCardStatusServiceResponse();
                }
    
                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(loyaltyCard.CustomerAccount, throwOnValidationFailure: true);
                request.RequestContext.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
    
                // Get loyalty groups and loyalty tiers
                DateTimeOffset channelDateTime = request.RequestContext.GetNowInChannelTimeZone();
                var getLoyaltyGroupsAndTiersDataRequest = new GetLoyaltyGroupsAndTiersDataRequest(request.LoyaltyCardNumber, request.RetrieveRewardPointStatus);
                getLoyaltyGroupsAndTiersDataRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                loyaltyCard.LoyaltyGroups = request.RequestContext.Execute<EntityDataServiceResponse<LoyaltyGroup>>(getLoyaltyGroupsAndTiersDataRequest).PagedEntityCollection.Results;
    
                // Get reward points status
                if (request.RetrieveRewardPointStatus)
                {
                    var serviceRequest = new GetLoyaltyCardRewardPointsStatusRealtimeRequest(
                        channelDateTime,
                        request.LoyaltyCardNumber,
                        excludeBlocked: false,
                        excludeNoTender: false,
                        includeRelatedCardsForContactTender: false,
                        includeNonRedeemablePoints: false,
                        includeActivePointsOnly: false);
    
                    EntityDataServiceResponse<LoyaltyCard> serviceResponse = request.RequestContext.Execute<EntityDataServiceResponse<LoyaltyCard>>(serviceRequest);
                    LoyaltyCard loyaltyCardWithPoints = serviceResponse.PagedEntityCollection.FirstOrDefault();
    
                    if (loyaltyCardWithPoints != null)
                    {
                        loyaltyCard.RewardPoints = loyaltyCardWithPoints.RewardPoints;
                    }
                }
    
                var response = new GetLoyaltyCardStatusServiceResponse(loyaltyCard);
                return response;
            }
    
            /// <summary>
            /// Calculates the loyalty reward points for a given transaction.
            /// </summary>
            /// <param name="request">The request containing the transaction.</param>
            /// <returns>The response containing the transaction with calculated reward points.</returns>
            private static CalculateLoyaltyRewardPointsServiceResponse CalculateLoyaltyRewardPoints(CalculateLoyaltyRewardPointsServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.SalesTransaction == null)
                {
                    throw new ArgumentNullException("request", "request.SalesTransaction");
                }
    
                // Calculate loyalty only when the loyalty card number exists.
                // Calculate loyalty only when the transaction type is Sales (i.e. Cash and carry).
                // Other transactions such as PendingOrder and CustomerOrder will calculate in HQ after invoicing.
                var loyaltyCardNumber = request.SalesTransaction.LoyaltyCardId;
                if (!string.IsNullOrWhiteSpace(loyaltyCardNumber)
                    && request.SalesTransaction.TransactionType == Microsoft.Dynamics.Commerce.Runtime.DataModel.SalesTransactionType.Sales)
                {
                    // Clear existing reward point lines.
                    if (request.SalesTransaction.LoyaltyRewardPointLines != null && request.SalesTransaction.LoyaltyRewardPointLines.Count > 0)
                    {
                        IEnumerable<LoyaltyRewardPointLine> otherLines = request.SalesTransaction.LoyaltyRewardPointLines.Where<LoyaltyRewardPointLine>(
                                l => (l.EntryType != LoyaltyRewardPointEntryType.Earn && l.EntryType != LoyaltyRewardPointEntryType.ReturnEarned));
                        request.SalesTransaction.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>(otherLines.ToList());
                    }
    
                    // Find the applied earn scheme lines of the current channel
                    var getLoyaltySchemeLineEarnDataRequest = new GetLoyaltySchemeLineEarnDataRequest(request.RequestContext.GetPrincipal().ChannelId, loyaltyCardNumber);
                    getLoyaltySchemeLineEarnDataRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                    var earnSchemeLines = request.RequestContext.Execute<EntityDataServiceResponse<LoyaltySchemeLineEarn>>(getLoyaltySchemeLineEarnDataRequest).PagedEntityCollection.Results;
    
                    // Calculate returned reward points, deduct points first before earn new points
                    LoyaltyServiceHelper.FillInLoyaltyRewardPointLinesForReturn(request.RequestContext, request.SalesTransaction, earnSchemeLines, loyaltyCardNumber);
    
                    // Calculate earned reward points
                    LoyaltyServiceHelper.FillInLoyaltyRewardPointLinesForSales(request.RequestContext, request.SalesTransaction, earnSchemeLines, loyaltyCardNumber);
                }
    
                var response = new CalculateLoyaltyRewardPointsServiceResponse(request.SalesTransaction);
                return response;
            }
        }
    }
}
