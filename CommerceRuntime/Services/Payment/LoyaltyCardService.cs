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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates service that implements gift card operations.
        /// </summary>
        public class LoyaltyCardService : IOperationRequestHandler
        {
            /// <summary>
            /// Gets a collection of operation identifiers supported by this request handler.
            /// </summary>
            public IEnumerable<int> SupportedOperationIds
            {
                get
                {
                    return new[]
                    {
                        (int)RetailOperation.PayLoyalty
                    };
                }
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
                        typeof(CalculatePaymentAmountServiceRequest),
                        typeof(AuthorizePaymentServiceRequest),
                        typeof(CapturePaymentServiceRequest),
                        typeof(VoidPaymentServiceRequest),
                        typeof(GetChangePaymentServiceRequest)
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
    
                if (requestType == typeof(CalculatePaymentAmountServiceRequest))
                {
                    response = CalculatePaymentAmount((CalculatePaymentAmountServiceRequest)request);
                }
                else if (requestType == typeof(AuthorizePaymentServiceRequest))
                {
                    response = AuthorizePayment((AuthorizePaymentServiceRequest)request);
                }
                else if (requestType == typeof(VoidPaymentServiceRequest))
                {
                    response = CancelPayment((VoidPaymentServiceRequest)request);
                }
                else if (requestType == typeof(GetChangePaymentServiceRequest))
                {
                    response = GetChange((GetChangePaymentServiceRequest)request);
                }
                else if (requestType == typeof(CapturePaymentServiceRequest))
                {
                    response = CapturePayment((CapturePaymentServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Calculate amount to do be paid.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the updated tender line.</returns>
            private static CalculatePaymentAmountServiceResponse CalculatePaymentAmount(CalculatePaymentAmountServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                // no calculation required.
                return new CalculatePaymentAmountServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Authorizes the payment.
            /// This step checks whether the loyalty card has enough reward points to redeem. If yes, it decides the points
            /// to redeem based on redeem ranking.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the authorized tender line.</returns>
            private static AuthorizePaymentServiceResponse AuthorizePayment(AuthorizePaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.TenderLine == null)
                {
                    throw new ArgumentException("request.TenderLine cannot be null.");
                }
    
                if (request.RequestContext == null)
                {
                    throw new ArgumentException("request.RequestContext cannot be null.");
                }
    
                if (request.Transaction == null)
                {
                    throw new ArgumentException("request.Transaction cannot be null.");
                }
    
                // Check tender amount.
                if (request.TenderLine.Amount == 0m)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "The tender amount must be greater than zero.");
                }
    
                // Check tender currency.
                if (string.IsNullOrWhiteSpace(request.TenderLine.Currency))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "The tender currency is missing.");
                }
    
                // Check if the transaction already has loyalty payments
                var activeTenderLines = request.Transaction.ActiveTenderLines;
                if (activeTenderLines != null && activeTenderLines.Any(line => !string.IsNullOrWhiteSpace(line.LoyaltyCardId)))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_NoMoreThanOneLoyaltyTender, "The transaction cannot contain more than one loyalty payment line.");
                }
    
                SalesOrder salesOrder = request.Transaction as SalesOrder;
                if (salesOrder != null && salesOrder.HasLoyaltyPayment
                    && (salesOrder.CustomerOrderMode != CustomerOrderMode.Cancellation || salesOrder.AmountDue > decimal.Zero))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_NoMoreThanOneLoyaltyTender, "The transaction cannot contain more than one loyalty payment line.");
                }
    
                // Check whether the loyalty card is valid.
                var getLoyaltyCardDataRequest = new GetLoyaltyCardDataRequest(request.TenderLine.LoyaltyCardId);
                LoyaltyCard loyaltyCard = request.RequestContext.Execute<SingleEntityDataServiceResponse<LoyaltyCard>>(getLoyaltyCardDataRequest).Entity;
                if (loyaltyCard == null || string.IsNullOrWhiteSpace(loyaltyCard.CardNumber))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidLoyaltyCardNumber, "The loyalty card number does not exists.");
                }
    
                // Check whether the loyalty card is blocked.
                if (loyaltyCard.CardTenderType == LoyaltyCardTenderType.Blocked)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_BlockedLoyaltyCard, "The loyalty card is blocked.");
                }
    
                if (loyaltyCard.CardTenderType == LoyaltyCardTenderType.NoTender)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_NoTenderLoyaltyCard, "The loyalty card is not allowed for payment.");
                }
    
                // Calculate redeem trans and fill in the sales transaction.
                if (request.TenderLine.Amount >= 0)
                {
                    LoyaltyServiceHelper.FillInLoyaltyRewardPointLinesForPayment(
                            request.RequestContext,
                            request.Transaction,
                            loyaltyCard,
                            request.TenderLine.Amount,
                            request.TenderLine.Currency);
                }
                else
                {
                    LoyaltyServiceHelper.FillInLoyaltyRewardPointLinesForRefund(
                            request.RequestContext,
                            request.Transaction,
                            loyaltyCard,
                            request.TenderLine.Amount,
                            request.TenderLine.Currency);
                }
    
                // Set Card Type Id for Loyalty Card if not set by the client.
                if (request.TenderLine.CardTypeId == null)
                {
                    string tenderTypeId = request.TenderLine.TenderTypeId;
                    var cardTypeDataRequest = new GetCardTypeDataRequest(QueryResultSettings.AllRecords);
                    var cardTypeInfoResponse = request.RequestContext.Execute<EntityDataServiceResponse<CardTypeInfo>>(cardTypeDataRequest);
                    IEnumerable<CardTypeInfo> cardTypes = cardTypeInfoResponse.PagedEntityCollection.Results;
                    CardTypeInfo loyaltyCardTypeInfo = cardTypes.FirstOrDefault(cardType => cardType.PaymentMethodId == tenderTypeId);
                    if (loyaltyCardTypeInfo == null)
                    {
                        throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_ConfigurationSettingNotFound, "The loyalty card payment as a tender type card is not configured for the channel.");
                    }
    
                    request.TenderLine.CardTypeId = loyaltyCardTypeInfo.TypeId;
                }
    
                // Authorize.
                request.TenderLine.Status = TenderLineStatus.PendingCommit;
                request.TenderLine.IsVoidable = true;
    
                return new AuthorizePaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Captures the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the captured tender line.</returns>
            private static CapturePaymentServiceResponse CapturePayment(CapturePaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.RequestContext == null)
                {
                    throw new ArgumentException("request.RequestContext cannot be null.");
                }
    
                // Post redeem type reward point trans to HQ.
                var redeemLoyaltyRewardPointServiceRequest = new PostLoyaltyCardRewardPointRealtimeRequest(LoyaltyRewardPointEntryType.Redeem, request.Transaction);
                request.RequestContext.Execute<NullResponse>(redeemLoyaltyRewardPointServiceRequest);
    
                var refundLoyaltyRewardPointServiceRequest = new PostLoyaltyCardRewardPointRealtimeRequest(LoyaltyRewardPointEntryType.Refund, request.Transaction);
                request.RequestContext.Execute<NullResponse>(refundLoyaltyRewardPointServiceRequest);
    
                request.TenderLine.Status = TenderLineStatus.Committed;
                request.TenderLine.IsVoidable = false;
    
                return new CapturePaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Cancels the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the canceled tender line.</returns>
            private static VoidPaymentServiceResponse CancelPayment(VoidPaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.TenderLine == null)
                {
                    throw new ArgumentException("request.TenderLine cannot be null.");
                }
    
                if (request.RequestContext == null)
                {
                    throw new ArgumentException("request.RequestContext cannot be null.");
                }
    
                if (request.Transaction == null)
                {
                    throw new ArgumentException("request.Transaction cannot be null.");
                }
    
                // Remove redeem reward point lines
                if (request.Transaction.LoyaltyRewardPointLines != null)
                {
                    IEnumerable<LoyaltyRewardPointLine> otherLines = request.Transaction.LoyaltyRewardPointLines.Where(
                        l => (l.EntryType != LoyaltyRewardPointEntryType.Redeem && l.EntryType != LoyaltyRewardPointEntryType.Refund));
                    request.Transaction.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>(otherLines.ToList());
                }
    
                request.TenderLine.Status = TenderLineStatus.Voided;
                request.TenderLine.IsVoidable = false;
    
                return new VoidPaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Gets the change.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// A response containing the change tender line.
            /// </returns>
            private static GetChangePaymentServiceResponse GetChange(GetChangePaymentServiceRequest request)
            {
                // Change cannot be given in loyalty cards because loyalty card balance is meansured using loyalty points.
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_ChangeTenderTypeNotSupported, string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
            }
        }
    }
}
