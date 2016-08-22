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
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Helper class for reason codes work flows.
        /// </summary>
        internal static class ReasonCodesWorkflowHelper
        {
            #region AddOrUpdateReasonCodes
    
            /// <summary>
            /// Adds or updates reason code lines for cart level reason code lines.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="cart">The cart to update or add.</param>
            public static void AddOrUpdateReasonCodeLinesOnTransaction(SalesTransaction salesTransaction, Cart cart)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(cart, "cart");
    
                if (cart.ReasonCodeLines != null &&
                    cart.ReasonCodeLines.Any())
                {
                    AddOrUpdateReasonCodeLinesHelper(salesTransaction.ReasonCodeLines, cart.ReasonCodeLines, salesTransaction.Id, string.Empty, ReasonCodeLineType.Header);
                }
            }
    
            /// <summary>
            /// Adds the or update reason code lines for cart line.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="cartLine">The cart line.</param>
            /// <param name="transactionId">The transaction id.</param>
            public static void AddOrUpdateReasonCodeLinesOnSalesLine(SalesLine salesLine, CartLine cartLine, string transactionId)
            {
                ThrowIf.Null(salesLine, "salesLine");
                ThrowIf.Null(cartLine, "cartLine");
    
                if (cartLine.LineData != null &&
                    cartLine.LineData.ReasonCodeLines != null &&
                    cartLine.LineData.ReasonCodeLines.Any())
                {
                    AddOrUpdateReasonCodeLinesHelper(
                        reasonCodeLinesToUpdate: salesLine.ReasonCodeLines,
                        reasonCodeLines: cartLine.LineData.ReasonCodeLines,
                        transactionId: transactionId,
                        parentLineId: salesLine.LineId,
                        reasonCodeLineType: ReasonCodeLineType.Sales);
                }
            }
    
            /// <summary>
            /// Adds the or update reason code lines.
            /// </summary>
            /// <param name="tenderLine">The tender line.</param>
            /// <param name="cartTenderLine">The cart tender line.</param>
            /// <param name="transactionId">The transaction id.</param>
            public static void AddOrUpdateReasonCodeLinesOnTenderLine(TenderLine tenderLine, TenderLineBase cartTenderLine, string transactionId)
            {
                ThrowIf.Null(tenderLine, "tenderLine");
                ThrowIf.Null(cartTenderLine, "cartTenderLine");
    
                if (cartTenderLine.ReasonCodeLines != null &&
                    cartTenderLine.ReasonCodeLines.Any())
                {
                    AddOrUpdateReasonCodeLinesHelper(
                        reasonCodeLinesToUpdate: tenderLine.ReasonCodeLines,
                        reasonCodeLines: cartTenderLine.ReasonCodeLines,
                        transactionId: transactionId,
                        parentLineId: tenderLine.TenderLineId,
                        reasonCodeLineType: ReasonCodeLineType.Payment);
                }
            }
    
            /// <summary>
            /// Adds or updates the reason code lines for affiliation line.
            /// </summary>
            /// <param name="salesAffiliationLoyaltyTier">The sales affiliation line.</param>
            /// <param name="affiliationLoyaltyTier">The cart affiliation line.</param>
            /// <param name="transactionId">The transaction id.</param>
            public static void AddOrUpdateReasonCodeLinesOnAffiliationLine(SalesAffiliationLoyaltyTier salesAffiliationLoyaltyTier, AffiliationLoyaltyTier affiliationLoyaltyTier, string transactionId)
            {
                ThrowIf.Null(salesAffiliationLoyaltyTier, "salesAffiliationLoyaltyTier");
                ThrowIf.Null(affiliationLoyaltyTier, "affiliationLoyaltyTier");
                ThrowIf.NullOrWhiteSpace(transactionId, "transactionId");
    
                if (affiliationLoyaltyTier.ReasonCodeLines.Any())
                {
                    AddOrUpdateReasonCodeLinesHelper(
                        reasonCodeLinesToUpdate: salesAffiliationLoyaltyTier.ReasonCodeLines,
                        reasonCodeLines: affiliationLoyaltyTier.ReasonCodeLines,
                        transactionId: transactionId,
                        parentLineId: salesAffiliationLoyaltyTier.AffiliationId.ToString(CultureInfo.InvariantCulture),
                        reasonCodeLineType: ReasonCodeLineType.Affiliation);
                }
            }
    
            #endregion
    
            #region CalculateRequiredReasonCodes
    
            /// <summary>
            /// Calculates the required reason codes for the entire transaction.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="sourceType">Type of the source.</param>
            public static void CalculateRequiredReasonCodes(RequestContext requestContext, SalesTransaction transaction, ReasonCodeSourceType sourceType)
            {
                ThrowIf.Null(requestContext, "requestContext");
                ThrowIf.Null(transaction, "transaction");
    
                var serviceRequest = new CalculateRequiredReasonCodesServiceRequest(transaction, sourceType);
    
                CalculateRequiredReasonCodesHelper(requestContext, serviceRequest);
            }
    
            /// <summary>
            /// Calculates the required reason codes on the current sales transaction level.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="sourceType">Type of the source.</param>
            public static void CalculateRequiredReasonCodesOnTransaction(RequestContext requestContext, SalesTransaction salesTransaction, ReasonCodeSourceType sourceType)
            {
                ThrowIf.Null(requestContext, "requestContext");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                var serviceRequest = new CalculateRequiredReasonCodesServiceRequest(salesTransaction, sourceType);
    
                CalculateRequiredReasonCodesHelper(requestContext, serviceRequest);
            }
    
            /// <summary>
            /// Calculates the required reason codes on sales line.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="sourceType">Type of the source.</param>
            public static void CalculateRequiredReasonCodesOnSalesLine(RequestContext requestContext, SalesTransaction transaction, SalesLine salesLine, ReasonCodeSourceType sourceType)
            {
                ThrowIf.Null(requestContext, "requestContext");
                ThrowIf.Null(salesLine, "salesLine");
    
                var serviceRequest = new CalculateRequiredReasonCodesServiceRequest(transaction, sourceType, new[] { salesLine });
    
                CalculateRequiredReasonCodesHelper(requestContext, serviceRequest);
            }
    
            /// <summary>
            /// Calculates the required reason codes on tender line.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="tenderLine">The tender line.</param>
            /// <param name="sourceType">Type of the source.</param>
            public static void CalculateRequiredReasonCodesOnTenderLine(RequestContext requestContext, SalesTransaction transaction, TenderLine tenderLine, ReasonCodeSourceType sourceType)
            {
                ThrowIf.Null(requestContext, "requestContext");
                ThrowIf.Null(tenderLine, "tenderLine");
    
                var serviceRequest = new CalculateRequiredReasonCodesServiceRequest(transaction, sourceType, new[] { tenderLine });
    
                CalculateRequiredReasonCodesHelper(requestContext, serviceRequest);
            }
    
            /// <summary>
            /// Calculates the required reason codes on affiliation line.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="salesAffiliationLoyaltyTiers">The sales affiliation lines.</param>
            /// <param name="sourceType">Type of the source.</param>
            public static void CalculateRequiredReasonCodesOnAffiliationLines(RequestContext requestContext, SalesTransaction transaction, IEnumerable<SalesAffiliationLoyaltyTier> salesAffiliationLoyaltyTiers, ReasonCodeSourceType sourceType)
            {
                ThrowIf.Null(requestContext, "requestContext");
                ThrowIf.Null(salesAffiliationLoyaltyTiers, "salesAffiliationLoyaltyTiers");
    
                var serviceRequest = new CalculateRequiredReasonCodesServiceRequest(transaction, sourceType, salesAffiliationLoyaltyTiers);
    
                CalculateRequiredReasonCodesHelper(requestContext, serviceRequest);
            }
    
            /// <summary>
            /// Calculates the required reason codes on an income expense line.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="incomeExpenseLine">The income expense line.</param>
            /// <param name="sourceType">Type of the source.</param>
            public static void CalculateRequiredReasonCodesOnIncomeExpenseLine(RequestContext requestContext, SalesTransaction transaction, IncomeExpenseLine incomeExpenseLine, ReasonCodeSourceType sourceType)
            {
                ThrowIf.Null(requestContext, "requestContext");
                ThrowIf.Null(incomeExpenseLine, "incomeExpenseLine");
    
                var serviceRequest = new CalculateRequiredReasonCodesServiceRequest(transaction, sourceType, new[] { incomeExpenseLine });
    
                CalculateRequiredReasonCodesHelper(requestContext, serviceRequest);
            }
    
            #endregion
    
            /// <summary>
            /// Validates the required reason code lines filled.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <exception cref="ConfigurationException">Required Service missing.</exception>
            /// <exception cref="DataValidationException">One or more reason codes required for the transaction are missing.</exception>
            public static void ValidateRequiredReasonCodeLinesFilled(RequestContext requestContext, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(requestContext, "requestContext");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                var serviceRequest = new CalculateRequiredReasonCodesServiceRequest(salesTransaction, ReasonCodeSourceType.None);
                var serviceResponse = requestContext.Execute<CalculateRequiredReasonCodesServiceResponse>(serviceRequest);
    
                ReasonCodesWorkflowHelper.ThrowIfRequiredReasonCodesMissing(serviceResponse);
            }
    
            /// <summary>
            /// Adds or updates reason code lines.
            /// </summary>
            /// <param name="reasonCodeLinesToUpdate">The reason code lines to update.</param>
            /// <param name="reasonCodeLines">The reason code lines.</param>
            /// <param name="transactionId">The transaction id.</param>
            /// <param name="parentLineId">The parent line id.</param>
            /// <param name="reasonCodeLineType">The reason code line type.</param>
            internal static void AddOrUpdateReasonCodeLinesHelper(ICollection<ReasonCodeLine> reasonCodeLinesToUpdate, IEnumerable<ReasonCodeLine> reasonCodeLines, string transactionId, string parentLineId, ReasonCodeLineType reasonCodeLineType)
            {
                var reasonCodeLinesToUpdateById = reasonCodeLinesToUpdate.ToDictionary(r => r.ReasonCodeId);
                foreach (ReasonCodeLine reasonCodeLine in reasonCodeLines)
                {
                    // we cannot add two reason code lines with same reason code identifier
                    // in this case, it should be an update
                    if (string.IsNullOrWhiteSpace(reasonCodeLine.LineId)
                        && !reasonCodeLinesToUpdateById.ContainsKey(reasonCodeLine.ReasonCodeId))
                    {
                        reasonCodeLine.LineId = Guid.NewGuid().ToString("N");
    
                        ReasonCodeLine newReasonCodeLine = new ReasonCodeLine();
                        newReasonCodeLine.CopyFrom(reasonCodeLine);
                        newReasonCodeLine.ParentLineId = parentLineId;
                        newReasonCodeLine.TransactionId = transactionId;
                        newReasonCodeLine.LineType = reasonCodeLineType;
    
                        // Add new reason code line to sales tranaction.
                        reasonCodeLinesToUpdate.Add(newReasonCodeLine);
                    }
                    else
                    {
                        // Update the corresponding reason code line if changed.
                        ReasonCodeLine reasonCodeLineToUpdate;
                        if (reasonCodeLinesToUpdateById.TryGetValue(reasonCodeLine.ReasonCodeId, out reasonCodeLineToUpdate))
                        {
                            reasonCodeLineToUpdate.CopyPropertiesFrom(reasonCodeLine);
                            reasonCodeLineToUpdate.IsChanged = true;
                        }
                    }
                }
            }
    
            /// <summary>
            /// Helper methods to calculates the required reason codes.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="serviceRequest">The service request.</param>
            /// <exception cref="ConfigurationException">Required Service missing: {0}.</exception>
            private static void CalculateRequiredReasonCodesHelper(RequestContext requestContext, CalculateRequiredReasonCodesServiceRequest serviceRequest)
            {
                ThrowIf.Null(serviceRequest.SalesTransaction, "serviceRequest.SalesTransaction");
                
                // Reason codes are only calculated for retail stores and carts that are not customer orders.
                if ((requestContext.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore) &&
                    (serviceRequest.SalesTransaction.CartType != CartType.CustomerOrder))
                {
                    var serviceResponse = requestContext.Execute<CalculateRequiredReasonCodesServiceResponse>(serviceRequest);
                    ReasonCodesWorkflowHelper.ThrowIfRequiredReasonCodesMissing(serviceResponse);
                }
            }
    
            /// <summary>
            /// Throws exception if any requires reason code are missing.
            /// </summary>
            /// <param name="serviceResponse">The service response.</param>
            private static void ThrowIfRequiredReasonCodesMissing(CalculateRequiredReasonCodesServiceResponse serviceResponse)
            {
                if (serviceResponse.RequiredReasonCodes.Any())
                {
                    throw new MissingRequiredReasonCodeException(
                        serviceResponse.RequiredReasonCodes,
                        serviceResponse.TransactionRequiredReasonCodeIds,
                        serviceResponse.ReasonCodeRequirements,
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredReasonCodesMissing,
                        "One or more reason codes required for the transaction are missing.");
                }
            }
        }
    }
}
