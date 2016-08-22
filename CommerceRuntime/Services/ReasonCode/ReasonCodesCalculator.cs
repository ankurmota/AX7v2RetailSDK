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
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the business logic for calculating reason codes.
        /// </summary>
        internal static class ReasonCodesCalculator
        {
            /// <summary>
            /// Random seed used for info codes. Only one to initialize one time.
            /// </summary>
            private static readonly Random RandomSeed = new Random((int)DateTime.Now.Ticks);
    
            /// <summary>
            /// Calculates the required reason codes.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The calculate required reason codes response.</returns>
            public static CalculateRequiredReasonCodesServiceResponse CalculateRequiredReasonCodes(CalculateRequiredReasonCodesServiceRequest request)
            {
                // Keeps track of required reason code lines added.
                Dictionary<string, ReasonCode> requiredReasonCodes = new Dictionary<string, ReasonCode>();
                HashSet<string> transactionRequiredReasonCodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<ReasonCodeRequirement> reasonCodeRequirements = new HashSet<ReasonCodeRequirement>();
    
                // calculate reason codes for source type
                CalculateRequiredReasonCodesBySourceType(request, requiredReasonCodes, transactionRequiredReasonCodeIds, reasonCodeRequirements);
    
                // calculate reason codes for sales lines
                foreach (var salesLine in request.SalesLines)
                {
                    // calculate specific reason codes
                    CalculateReasonCodesSpecificToEntity(
                        request,
                        salesLine.ProductId.ToString(CultureInfo.InvariantCulture),
                        ReasonCodeTableRefType.Item,
                        salesLine.ItemId,
                        string.Empty,
                        string.Empty,
                        requiredReasonCodes,
                        reasonCodeRequirements,
                        salesLine.ReasonCodeLines,
                        salesLine);
                }
    
                // calculate reason codes for tender lines
                foreach (var tenderLine in request.TenderLines)
                {
                    // calculate specific reason codes
                    CalculateRequiredSpecificReasonCodesOnTenderLine(
                        request, requiredReasonCodes, reasonCodeRequirements, tenderLine);
                }
    
                // calculate reason codes for affiliation lines
                foreach (var salesAffiliationLoyaltyTier in request.SalesAffiliationLoyaltyTiers)
                {
                    // Calculate specific reason codes
                    CalculateRequiredSpecificReasonCodesOnAffiliationLine(
                        request, requiredReasonCodes, reasonCodeRequirements, salesAffiliationLoyaltyTier);
                }
    
                // calculate reason codes for income expense lines
                foreach (var incomeExpenseLine in request.SalesTransaction.IncomeExpenseLines)
                {
                    CalculateReasonCodesSpecificToEntity(
                        request,
                        System.Enum.GetName(typeof(IncomeExpenseAccountType), incomeExpenseLine.AccountType),
                        ReasonCodeTableRefType.IncomeExpense,
                        incomeExpenseLine.StoreNumber,
                        incomeExpenseLine.IncomeExpenseAccount,
                        string.Empty,
                        requiredReasonCodes,
                        reasonCodeRequirements,
                        request.SalesTransaction.ReasonCodeLines,
                        null);
                }
    
                // calculate reason codes for customer
                CalculateReasonCodesSpecificToEntity(
                    request,
                    request.SalesTransaction.CustomerId,
                    ReasonCodeTableRefType.Customer,
                    request.SalesTransaction.CustomerId,
                    string.Empty,
                    string.Empty,
                    requiredReasonCodes,
                    reasonCodeRequirements,
                    request.SalesTransaction.ReasonCodeLines,
                    null);
    
                // move the customer and income expense reason codes to transaction level.
                var ids = reasonCodeRequirements.Where(r => (r.TableRefType == ReasonCodeTableRefType.IncomeExpense || r.TableRefType == ReasonCodeTableRefType.Customer))
                    .Select(r => r.ReasonCodeId);
    
                transactionRequiredReasonCodeIds.AddRange(ids);
    
                return new CalculateRequiredReasonCodesServiceResponse(
                    requiredReasonCodes.Values,
                    transactionRequiredReasonCodeIds,
                    reasonCodeRequirements);
            }
    
            /// <summary>
            /// Calculates the required reason codes given the source type.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="requiredReasonCodes">The collection to which required reason codes are added.</param>
            /// <param name="transactionRequiredReasonCodeIds">The identifiers of reason codes required at transaction level.</param>
            /// <param name="reasonCodeRequirements">The collection of reason code requirements.</param>
            private static void CalculateRequiredReasonCodesBySourceType(
                CalculateRequiredReasonCodesServiceRequest request,
                Dictionary<string, ReasonCode> requiredReasonCodes,
                HashSet<string> transactionRequiredReasonCodeIds,
                HashSet<ReasonCodeRequirement> reasonCodeRequirements)
            {
                // Look up operation level reason codes if available.
                if (request.SourceType != ReasonCodeSourceType.None)
                {
                    ReasonCodeSettings settingsDictionary = GetReasonCodeSettings(request.RequestContext);
                    string reasonCodeId = settingsDictionary[request.SourceType];
    
                    if (!string.IsNullOrWhiteSpace(reasonCodeId))
                    {
                        GetReasonCodesDataRequest getReasonCodeRequest = new GetReasonCodesDataRequest(QueryResultSettings.AllRecords, new string[] { reasonCodeId });
                        IEnumerable<ReasonCode> reasonCodes = request.RequestContext.Execute<EntityDataServiceResponse<ReasonCode>>(getReasonCodeRequest).PagedEntityCollection.Results;
    
                        foreach (var reasonCode in reasonCodes)
                        {
                            if (IsTransactionSourceType(request.SourceType)
                                && ShouldReasonCodeBeApplied(reasonCode, request.SalesTransaction))
                            {
                                if (!ContainsReasonCode(request.SalesTransaction.ReasonCodeLines, reasonCode.ReasonCodeId))
                                {
                                    requiredReasonCodes[reasonCode.ReasonCodeId] = reasonCode;
                                    transactionRequiredReasonCodeIds.Add(reasonCode.ReasonCodeId);
                                }
    
                                var triggeredReasonCodes = CalculateTriggeredReasonCodes(
                                    new ReasonCode[] { reasonCode }, request.SalesTransaction.ReasonCodeLines, request.RequestContext);
    
                                if (triggeredReasonCodes.Any())
                                {
                                    requiredReasonCodes.AddRange(triggeredReasonCodes.ToDictionary(rc => rc.ReasonCodeId, rc => rc));
                                    transactionRequiredReasonCodeIds.AddRange(triggeredReasonCodes.Select(rc => rc.ReasonCodeId));
                                }
                            }
                            else
                            {
                                foreach (var salesLine in request.SalesLines)
                                {
                                    if (ShouldReasonCodeBeApplied(reasonCode, request.SalesTransaction))
                                    {
                                        if (!ContainsReasonCode(salesLine.ReasonCodeLines, reasonCode.ReasonCodeId))
                                        {
                                            var reasonCodeRequirement = new ReasonCodeRequirement()
                                            {
                                                ReasonCodeId = reasonCode.ReasonCodeId,
                                                SourceId = salesLine.ProductId.ToString(CultureInfo.InvariantCulture),
                                                TableRefTypeValue = (int)ReasonCodeTableRefType.Item,
                                            };
    
                                            reasonCodeRequirements.Add(reasonCodeRequirement);
                                            requiredReasonCodes[reasonCode.ReasonCodeId] = reasonCode;
                                        }
    
                                        var triggeredReasonCodes = CalculateTriggeredReasonCodes(
                                            new ReasonCode[] { reasonCode }, salesLine.ReasonCodeLines, request.RequestContext);
    
                                        if (triggeredReasonCodes.Any())
                                        {
                                            requiredReasonCodes.AddRange(triggeredReasonCodes.ToDictionary(rc => rc.ReasonCodeId, rc => rc));
                                            reasonCodeRequirements.AddRange(triggeredReasonCodes.Select(rc => new ReasonCodeRequirement()
                                            {
                                                ReasonCodeId = rc.ReasonCodeId,
                                                SourceId = salesLine.ProductId.ToString(CultureInfo.InvariantCulture),
                                                TableRefTypeValue = (int)ReasonCodeTableRefType.Item,
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// Calculate specific reason codes on a tender line and add them to the incoming collection.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="requiredReasonCodes">The collection to which required reason codes are added.</param>
            /// <param name="reasonCodeRequirements">The required specific reason codes map.</param>
            /// <param name="tenderLine">The tenderLine on which to calculate required reason codes.</param>
            private static void CalculateRequiredSpecificReasonCodesOnTenderLine(
                CalculateRequiredReasonCodesServiceRequest request,
                IDictionary<string, ReasonCode> requiredReasonCodes,
                HashSet<ReasonCodeRequirement> reasonCodeRequirements,
                TenderLine tenderLine)
            {
                if (tenderLine.Status == TenderLineStatus.Historical)
                {
                    return;
                }
    
                // if tenderline is a card, refrelation3 becomes tenderline.cardtypeid and tablerefid is creditcard
                var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(request.RequestContext.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var tenderTypes = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, request.RequestContext).PagedEntityCollection.Results;
    
                ReasonCodeTableRefType tableRef = ReasonCodeTableRefType.Tender;
                string refRelation3 = string.Empty;
    
                TenderType tenderType = tenderTypes.Where(type => type.TenderTypeId == tenderLine.TenderTypeId).SingleOrDefault();
    
                if (tenderType.OperationId == (int)RetailOperation.PayCard)
                {
                    refRelation3 = tenderLine.CardTypeId;
                    tableRef = ReasonCodeTableRefType.CreditCard;
                }
    
                CalculateReasonCodesSpecificToEntity(
                    request,
                    tenderLine.TenderTypeId,
                    tableRef,
                    request.SalesTransaction.StoreId,
                    tenderLine.TenderTypeId,
                    refRelation3,
                    requiredReasonCodes,
                    reasonCodeRequirements,
                    tenderLine.ReasonCodeLines,
                    null);
            }
    
            /// <summary>
            /// Calculate specific reason codes on an affiliation line and add them to the incoming collection.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="requiredReasonCodes">The collection to which required reason codes are added.</param>
            /// <param name="reasonCodeRequirements">The required specific reason codes map.</param>
            /// <param name="salesAffiliationLoyaltyTier">The sales affiliation loyalty tier on which to calculate required reason codes.</param>
            private static void CalculateRequiredSpecificReasonCodesOnAffiliationLine(
                CalculateRequiredReasonCodesServiceRequest request,
                IDictionary<string, ReasonCode> requiredReasonCodes,
                HashSet<ReasonCodeRequirement> reasonCodeRequirements,
                SalesAffiliationLoyaltyTier salesAffiliationLoyaltyTier)
            {
                // Gets the affiliation according to the foreign key AffiliationId of the salesAffiliationLoyaltyTier.
                GetAffiliationByAffiliationIdDataRequest dataRequest = new GetAffiliationByAffiliationIdDataRequest(salesAffiliationLoyaltyTier.AffiliationId);
                Affiliation affiliation = request.RequestContext.Execute<SingleEntityDataServiceResponse<Affiliation>>(dataRequest).Entity;
    
                if (affiliation != null)
                {
                    CalculateReasonCodesSpecificToEntity(
                        request,
                        salesAffiliationLoyaltyTier.AffiliationId.ToString(CultureInfo.InvariantCulture),
                        ReasonCodeTableRefType.Affiliation,
                        affiliation.Name,
                        string.Empty,
                        string.Empty,
                        requiredReasonCodes,
                        reasonCodeRequirements,
                        salesAffiliationLoyaltyTier.ReasonCodeLines,
                        null);
                }
            }
    
            /// <summary>
            /// Verifies whether the reason code line is present on the collection.
            /// </summary>
            /// <param name="reasonCodeLines">The collection to check against.</param>
            /// <param name="reasonCodeId">The reason code identifier.</param>
            /// <returns><c>True</c> if the reason code is present on the collection, or <c>false</c> otherwise.</returns>
            private static bool ContainsReasonCode(ICollection<ReasonCodeLine> reasonCodeLines, string reasonCodeId)
            {
                return reasonCodeLines.Any(r => string.Equals(r.ReasonCodeId, reasonCodeId, StringComparison.OrdinalIgnoreCase));
            }
    
            /// <summary>
            /// Gets the reason code settings dictionary.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>The reason code settings dictionary.</returns>
            private static ReasonCodeSettings GetReasonCodeSettings(RequestContext context)
            {
                ReasonCodeSettings settings = null;
    
                GetReasonCodeSettingsDataRequest getReasonCodeSettingsDataRequest = new GetReasonCodeSettingsDataRequest(QueryResultSettings.SingleRecord);
                settings = context.Runtime.Execute<SingleEntityDataServiceResponse<ReasonCodeSettings>>(getReasonCodeSettingsDataRequest, context).Entity;
    
                // Reason code settings should be available for retail store.
                if (settings == null)
                {
                    var getChannelByIdDataRequest = new GetChannelByIdDataRequest(context.GetPrincipal().ChannelId);
                    Channel currentChannel = context.Runtime.Execute<SingleEntityDataServiceResponse<Channel>>(getChannelByIdDataRequest, context).Entity;
    
                    if (currentChannel.OrgUnitType == RetailChannelType.RetailStore)
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                            "The required reason code settings not found.");
                    }
    
                    // Use default settings for non-retail-store channels.
                    settings = ReasonCodeSettings.DefaultSettings;
                }
    
                return settings;
            }
    
            /// <summary>
            /// Calculates the specific reason code on an entity like sales line, tender line etc.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="sourceId">The source identifier.</param>
            /// <param name="tableRefType">The table identifier for the specific reason code.</param>
            /// <param name="refRelation">The first reference relation key corresponding to the entity and table.</param>
            /// <param name="refRelation2">The second reference relation key corresponding to the entity and table.</param>
            /// <param name="refRelation3">The third reference relation key corresponding to the entity and table.</param>
            /// <param name="requiredReasonCodes">The collection to which required reason codes are added.</param>
            /// <param name="reasonCodeRequirements">The required specific reason codes map.</param>
            /// <param name="presentReasonCodeLines">The collection with the already present reason code lines.</param>
            /// <param name="salesLine">The sales line when applicable.</param>
            private static void CalculateReasonCodesSpecificToEntity(
                CalculateRequiredReasonCodesServiceRequest request,
                string sourceId,
                ReasonCodeTableRefType tableRefType,
                string refRelation,
                string refRelation2,
                string refRelation3,
                IDictionary<string, ReasonCode> requiredReasonCodes,
                HashSet<ReasonCodeRequirement> reasonCodeRequirements,
                IEnumerable<ReasonCodeLine> presentReasonCodeLines,
                SalesLine salesLine)
            {
                GetReasonCodesByTableRefTypeDataRequest getReasonCodesByTableRefTypeDataRequest = new GetReasonCodesByTableRefTypeDataRequest(tableRefType, refRelation, refRelation2, refRelation3, QueryResultSettings.AllRecords);
                IEnumerable<ReasonCode> reasonCodes = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ReasonCode>>(getReasonCodesByTableRefTypeDataRequest, request.RequestContext).PagedEntityCollection.Results;
    
                reasonCodes = AddLinkedReasonCodes(reasonCodes, request.RequestContext);
    
                var triggeredReasonCodes = CalculateTriggeredReasonCodes(reasonCodes, presentReasonCodeLines, request.RequestContext);
                reasonCodes = reasonCodes.Union(triggeredReasonCodes).Distinct();
    
                reasonCodes = AddLinkedReasonCodes(reasonCodes, request.RequestContext);
    
                foreach (var reasonCode in reasonCodes)
                {
                    if (presentReasonCodeLines.Any(rc => string.Equals(rc.ReasonCodeId, reasonCode.ReasonCodeId, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
    
                    bool entitySpecificApplicability = true;
    
                    switch (tableRefType)
                    {
                        case ReasonCodeTableRefType.Item:
                            entitySpecificApplicability = HasSalesLineSpecificApplicability(reasonCode, salesLine);
                            break;
                    }
    
                    if (entitySpecificApplicability
                        && ShouldReasonCodeBeApplied(reasonCode, request.SalesTransaction))
                    {
                        var reasonCodeRequirement = new ReasonCodeRequirement()
                        {
                            ReasonCodeId = reasonCode.ReasonCodeId,
                            SourceId = sourceId,
                            TableRefTypeValue = (int)tableRefType,
                        };
    
                        reasonCodeRequirements.Add(reasonCodeRequirement);
                        requiredReasonCodes[reasonCode.ReasonCodeId] = reasonCode;
                    }
                }
            }
    
            /// <summary>
            /// Adds reason codes that are linked by other reason codes.
            /// </summary>
            /// <param name="reasonCodes">The reason codes collection.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Collection of reason codes with reason codes which are linked by present reason codes.</returns>
            private static IEnumerable<ReasonCode> AddLinkedReasonCodes(IEnumerable<ReasonCode> reasonCodes, RequestContext context)
            {
                var reasonCodesById = reasonCodes.ToDictionary(r => r.ReasonCodeId);
                var reasonCodesList = reasonCodes.ToList();
    
                for (int i = 0; i < reasonCodesList.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(reasonCodesList[i].LinkedReasonCodeId) && !reasonCodesById.ContainsKey(reasonCodesList[i].LinkedReasonCodeId))
                    {
                        var missingReasonCodes = CalculateLinkedReasonCodes(reasonCodesList, reasonCodesList[i], context);
                        reasonCodesList.InsertRange(i + 1, missingReasonCodes);
                    }
                }
    
                return reasonCodesList.Distinct();
            }
    
            /// <summary>
            /// Performs entity specific applicability checks for sales line.
            /// </summary>
            /// <param name="reasonCode">The reason code.</param>
            /// <param name="salesLine">The sales line.</param>
            /// <returns>
            /// True if reason code should be applied; otherwise, false.
            /// </returns>
            private static bool HasSalesLineSpecificApplicability(ReasonCode reasonCode, SalesLine salesLine)
            {
                bool result = false;
    
                switch (reasonCode.InputRequiredType)
                {
                    case ReasonCodeInputRequiredType.None:
                    case ReasonCodeInputRequiredType.Always:
                        result = true;
                        break;
                    case ReasonCodeInputRequiredType.Positive:
                        if (salesLine.Quantity > 0)
                        {
                            result = true;
                        }
    
                        break;
                    case ReasonCodeInputRequiredType.Negative:
                        if (salesLine.Quantity < 0)
                        {
                            result = true;
                        }
    
                        break;
                }
    
                return result;
            }
    
            /// <summary>
            /// Checks if reason code should be applied. The random factor and once per transaction flags are checked.
            /// </summary>
            /// <param name="reasonCode">The reason code.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <returns>
            /// True if reason code should be applied; otherwise, false.
            /// </returns>
            private static bool ShouldReasonCodeBeApplied(ReasonCode reasonCode, SalesTransaction transaction)
            {
                bool result;
    
                int randomValue = RandomSeed.Next(100);
                decimal reasonCodeRandomFactor = reasonCode.RandomFactor;
    
                // If unspecified, assume the random factor is 100%.
                if (reasonCodeRandomFactor == 0)
                {
                    reasonCodeRandomFactor = 100;
                }
    
                result = randomValue < reasonCodeRandomFactor;
    
                // While not technically required - just a fallback to ensure to always return true if 100% (or greater).
                result = result || (reasonCodeRandomFactor >= 100m);
    
                // Check if reason code is once per transaction and has been processed already.
                if (result && reasonCode.OncePerTransaction)
                {
                    result = result && !ReasonCodeInTransaction(reasonCode, transaction);
                }
    
                return result;
            }
    
            /// <summary>
            /// Checks for the presence of a reason code in the entire transaction. Includes sales lines, tender lines, and affiliations.
            /// </summary>
            /// <param name="reasonCode">The reason code to check for.</param>
            /// <param name="transaction">The transaction object.</param>
            /// <returns>True if reason code is found; false otherwise.</returns>
            private static bool ReasonCodeInTransaction(ReasonCode reasonCode, SalesTransaction transaction)
            {
                // Check at transaction level.
                if (ContainsReasonCode(transaction.ReasonCodeLines, reasonCode.ReasonCodeId))
                {
                    return true;
                }
    
                // Check the sales lines.
                foreach (SalesLine salesLine in transaction.SalesLines)
                {
                    if (ContainsReasonCode(salesLine.ReasonCodeLines, reasonCode.ReasonCodeId))
                    {
                        return true;
                    }
                }
    
                // Check the tender lines.
                foreach (TenderLine tenderLine in transaction.TenderLines)
                {
                    if (ContainsReasonCode(tenderLine.ReasonCodeLines, reasonCode.ReasonCodeId))
                    {
                        return true;
                    }
                }
    
                // Check the affiliations.
                foreach (SalesAffiliationLoyaltyTier affiliationLoyaltyTier in transaction.AffiliationLoyaltyTierLines)
                {
                    if (ContainsReasonCode(affiliationLoyaltyTier.ReasonCodeLines, reasonCode.ReasonCodeId))
                    {
                        return true;
                    }
                }
    
                return false;
            }
    
            /// <summary>
            /// Verifies whether the source type is applicable to the whole transaction or to the lines.
            /// </summary>
            /// <param name="reasonCodeSourceType">The reason code source type.</param>
            /// <returns><c>True</c> if the source type is applicable to the whole transaction or <c>false</c> if it is applicable to the lines.</returns>
            private static bool IsTransactionSourceType(ReasonCodeSourceType reasonCodeSourceType)
            {
                switch (reasonCodeSourceType)
                {
                    case ReasonCodeSourceType.AddSalesperson:
                    case ReasonCodeSourceType.EndOfTransaction:
                    case ReasonCodeSourceType.NegativeAdjustment:
                    case ReasonCodeSourceType.StartOfTransaction:
                    case ReasonCodeSourceType.TenderDeclaration:
                    case ReasonCodeSourceType.TotalDiscount:
                    case ReasonCodeSourceType.TransactionTaxChange:
                    case ReasonCodeSourceType.VoidPayment:
                    case ReasonCodeSourceType.VoidTransaction:
                        return true;
                    case ReasonCodeSourceType.ItemDiscount:
                    case ReasonCodeSourceType.ItemNotOnFile:
                    case ReasonCodeSourceType.LineItemTaxChange:
                    case ReasonCodeSourceType.Markup:
                    case ReasonCodeSourceType.OverridePrice:
                    case ReasonCodeSourceType.ReturnTransaction:
                    case ReasonCodeSourceType.ReturnItem:
                    case ReasonCodeSourceType.SerialNumber:
                    case ReasonCodeSourceType.VoidItem:
                        return false;
                    case ReasonCodeSourceType.None:
                    default:
                        throw new NotSupportedException("The type is not supported.");
                }
            }
    
            /// <summary>
            /// Calculates reason codes which can be triggered by another reason codes.
            /// </summary>
            /// <param name="reasonCodes">The reason codes collection.</param>
            /// <param name="reasonCodeLines">The reason code lines collection.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Collection of reason codes which can be triggered by another reason codes.</returns>
            private static IEnumerable<ReasonCode> CalculateTriggeredReasonCodes(
                IEnumerable<ReasonCode> reasonCodes, IEnumerable<ReasonCodeLine> reasonCodeLines, RequestContext context)
            {
                HashSet<string> missingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                IDictionary<string, ReasonCode> triggeredReasonCodes = new Dictionary<string, ReasonCode>();
                var subReasonCodes = GetReasonSubCodesById(reasonCodes);
                var reasonCodeLinesById = reasonCodeLines.ToDictionary(r => r.ReasonCodeId);
    
                foreach (var reasonCodeLine in reasonCodeLinesById.Values)
                {
                    ReasonSubCode subCode;
                    if (subReasonCodes.TryGetValue(reasonCodeLine.ReasonCodeId + reasonCodeLine.SubReasonCodeId, out subCode))
                    {
                        // if this sub code triggers another reason code.
                        if (subCode.TriggerFunctionType == TriggerFunctionType.InfoCode &&
                            !string.IsNullOrWhiteSpace(subCode.TriggerCode))
                        {
                            // check whether we have the reason code already.
                            var reasonCode = reasonCodes.FirstOrDefault(r => string.Equals(r.ReasonCodeId, subCode.TriggerCode, StringComparison.OrdinalIgnoreCase));
                            if (reasonCode != null)
                            {
                                if (!triggeredReasonCodes.ContainsKey(reasonCode.ReasonCodeId)
                                    && !reasonCodeLinesById.ContainsKey(reasonCode.ReasonCodeId))
                                {
                                    triggeredReasonCodes[reasonCode.ReasonCodeId] = reasonCode;
                                }
                            }
                            else
                            {
                                missingIds.Add(subCode.TriggerCode);
                            }
                        }
                    }
                }
    
                if (missingIds.Any())
                {
                    GetReasonCodesDataRequest getReasonCodeRequest = new GetReasonCodesDataRequest(QueryResultSettings.AllRecords, missingIds);
                    var missingReasonCodes = context.Execute<EntityDataServiceResponse<ReasonCode>>(getReasonCodeRequest).PagedEntityCollection.Results;
    
                    foreach (var missingReasonCode in missingReasonCodes)
                    {
                        if (!triggeredReasonCodes.ContainsKey(missingReasonCode.ReasonCodeId)
                            && !reasonCodeLinesById.ContainsKey(missingReasonCode.ReasonCodeId))
                        {
                            triggeredReasonCodes[missingReasonCode.ReasonCodeId] = missingReasonCode;
                        }
                    }
                }
    
                return triggeredReasonCodes.Values;
            }
    
            /// <summary>
            /// Calculates reason codes that are linked by other reason codes.
            /// </summary>
            /// <param name="reasonCodes">The reason codes collection.</param>
            /// <param name="reasonCode">The reason code.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Collection of reason codes which are linked by present reason codes and not on the original collection.</returns>
            private static IEnumerable<ReasonCode> CalculateLinkedReasonCodes(
                IEnumerable<ReasonCode> reasonCodes,
                ReasonCode reasonCode,
                RequestContext context)
            {
                HashSet<string> missingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                IDictionary<string, ReasonCode> linkedReasonCodes = new Dictionary<string, ReasonCode>();
                var reasonCodesById = reasonCodes.ToDictionary(r => r.ReasonCodeId);
    
                missingIds.Add(reasonCode.LinkedReasonCodeId);
    
                // get all the missing reason codes. also makes sure all the linked reason codes by the missing reason codes are also included and so on.
                while (missingIds.Any())
                {
                    HashSet<string> missingLinkedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    GetReasonCodesDataRequest getReasonCodeRequest = new GetReasonCodesDataRequest(QueryResultSettings.AllRecords, missingIds);
                    var missingReasonCodes = context.Execute<EntityDataServiceResponse<ReasonCode>>(getReasonCodeRequest).PagedEntityCollection.Results;
    
                    foreach (var missingReasonCode in missingReasonCodes)
                    {
                        if (!linkedReasonCodes.ContainsKey(missingReasonCode.ReasonCodeId) && !reasonCodesById.ContainsKey(missingReasonCode.ReasonCodeId))
                        {
                            linkedReasonCodes[missingReasonCode.ReasonCodeId] = missingReasonCode;
    
                            // if the new reason code links to a different one, and the new one is not present, adds it for later.
                            if (!string.IsNullOrWhiteSpace(missingReasonCode.LinkedReasonCodeId)
                                && !reasonCodesById.ContainsKey(missingReasonCode.LinkedReasonCodeId))
                            {
                                missingLinkedIds.Add(missingReasonCode.LinkedReasonCodeId);
                            }
                        }
                    }
    
                    // verify the new linked reason codes
                    missingIds = missingLinkedIds;
                }
    
                return linkedReasonCodes.Values;
            }
    
            /// <summary>
            /// Gets dictionary of reason sub codes from reason codes collection.
            /// </summary>
            /// <param name="reasonCodes">The collection of reason codes.</param>
            /// <returns>The dictionary of reason sub codes.</returns>
            private static IDictionary<string, ReasonSubCode> GetReasonSubCodesById(IEnumerable<ReasonCode> reasonCodes)
            {
                Collection<ReasonSubCode> reasonSubCodes = new Collection<ReasonSubCode>();
    
                var reasonCodeWithSubCodes = reasonCodes.Where(r => r.ReasonSubCodes.Any());
                foreach (var reasonWithSubReason in reasonCodeWithSubCodes)
                {
                    reasonSubCodes.AddRange(reasonWithSubReason.ReasonSubCodes);
                }
    
                return reasonSubCodes.Distinct().ToDictionary(r => r.ReasonCodeId + r.SubCodeId, r => r);
            }
        }
    }
}
