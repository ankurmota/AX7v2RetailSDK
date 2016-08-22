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
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        internal static class StoreOperationServiceHelper
        {
            /// <summary>
            /// Validates the counting difference for tender declaration operation.
            /// </summary>
            /// <param name="request">Drop and declare service request.</param>
            internal static void ValidateTenderDeclarationCountingDifference(SaveDropAndDeclareServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.ShiftId, "request.ShiftId");
    
                if (request.TransactionType == TransactionType.TenderDeclaration)
                {
                    // Retrieves the tender types info
                    Dictionary<string, TenderType> tenderTypeDict = StoreOperationServiceHelper.GetChannelTenderTypes(request.RequestContext);
    
                    // Check if tender declaration recount is needed
                    bool isRecountNeeded = request.TenderDetails.All(t => tenderTypeDict.ContainsKey(t.TenderTypeId)) && request.TenderDetails.Any(t => t.TenderRecount < tenderTypeDict[t.TenderTypeId].MaxRecount);
    
                    if (isRecountNeeded)
                    {
                        // Retrieves the expected shift tender amounts per tender type
                        Dictionary<string, ShiftTenderLine> expectedShiftTenderAmounts = StoreOperationServiceHelper.GetShiftRequiredAmountsPerTender(request.RequestContext, request.ShiftTerminalId, request.ShiftId);
    
                        // Validates the counting amount in tender declaration lines
                        foreach (TenderDetail declartionLine in request.TenderDetails)
                        {
                            string tenderTypeId = declartionLine.TenderTypeId;
                            decimal countingDifferenceAllowed = tenderTypeDict[tenderTypeId].MaxCountingDifference;
                            decimal expectedShiftTenderAmount = expectedShiftTenderAmounts.ContainsKey(tenderTypeId) ? expectedShiftTenderAmounts[tenderTypeId].TenderedAmountOfStoreCurrency : 0M;
                            int maxRecountAllowed = tenderTypeDict[tenderTypeId].MaxRecount;
                            string tenderName = tenderTypeDict[tenderTypeId].Name;
    
                            // Checks if current number of recount exceeds the permissible recount
                            if (declartionLine.TenderRecount < maxRecountAllowed)
                            {
                                // Checks if the difference between current tender delcartion amount with expected tender amounts
                                // exceeds the permissible counting difference for that tender type
                                if (Math.Abs(declartionLine.Amount - expectedShiftTenderAmount) > countingDifferenceAllowed)
                                {
                                    throw new TenderValidationException(
                                        tenderTypeId,
                                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MaxCountingDifferenceExceeded,
                                        string.Format("The counted amount differs from the total sales amount for the {0} tender type.", tenderName))
                                    {
                                        LocalizedMessageParameters = new object[] { tenderName }
                                    };
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the object of tender drop and declare operation.
            /// </summary>
            /// <param name="request">The SaveDropAndDeclareServiceRequest object.</param>
            /// <returns>The tender drop and declare operation object.</returns>
            internal static DropAndDeclareTransaction ConvertTenderDropAndDeclareTransaction(SaveDropAndDeclareServiceRequest request)
            {
                RequestContext context = request.RequestContext;
                var transaction = new DropAndDeclareTransaction
                {
                    Id = request.TransactionId,
                    ShiftId = request.ShiftId,
                    ShiftTerminalId = string.IsNullOrWhiteSpace(request.ShiftTerminalId) ? context.GetPrincipal().ShiftTerminalId : request.ShiftTerminalId,
                    TransactionType = request.TransactionType,
                    ChannelCurrencyExchangeRate = StoreOperationServiceHelper.GetExchangeRate(context),
                    StoreId = context.GetOrgUnit().OrgUnitNumber,
                    StaffId = context.GetPrincipal().UserId,
                    TerminalId = context.GetTerminal().TerminalId,
                    ChannelCurrency = context.GetChannelConfiguration().Currency,   // channel currency code
                    Description = request.Description
                };

                transaction.TenderDetails = new Collection<TenderDetail>();
                foreach (var tenderDetail in request.TenderDetails)
                {
                    TenderDetail tender = ConvertToTenderDetail(context, tenderDetail);
                    transaction.TenderDetails.Add(tender);
                }

                if (request.ReasonCodeLines != null && request.ReasonCodeLines.Any())
                {
                    // Read reason code details from request for [Tender Declaration] store operation
                    transaction.ReasonCodeLines = new Collection<ReasonCodeLine>();
                    foreach (var reasonCodeLine in request.ReasonCodeLines)
                    {
                        transaction.ReasonCodeLines.Add(reasonCodeLine);
                    }
                }

                return transaction;
            }

            /// <summary>
            /// Gets the tender detail object from the client.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="tenderDetail">The tender detail object from client.</param>
            /// <returns>The tender detail object to be saved on channel database.</returns>
            internal static TenderDetail ConvertToTenderDetail(RequestContext context, TenderDetail tenderDetail)
            {
                var tender = new TenderDetail
                {
                    BankBagNumber = tenderDetail.BankBagNumber,  // bankbag number
                    TenderTypeId = tenderDetail.TenderTypeId,    // tender type identifier
                    Amount = tenderDetail.Amount                // amount in channel currency
                };

                string channelCurrency = context.GetChannelConfiguration().Currency;
                string foreignCurrency = string.IsNullOrWhiteSpace(tenderDetail.ForeignCurrency) ? channelCurrency : tenderDetail.ForeignCurrency;

                // Check if the foreign currency of the transaction is in channel currency
                if (foreignCurrency.Equals(channelCurrency, StringComparison.OrdinalIgnoreCase))
                {
                    tender.ForeignCurrency = channelCurrency;               // foreign currency code is same as store currency code
                    tender.AmountInForeignCurrency = tenderDetail.Amount;   // foreign amount is same as store amount
                    tender.ForeignCurrencyExchangeRate = 1m;                // foreign to channel currency exchange rate is 1
                }
                else
                {
                    tender.ForeignCurrency = foreignCurrency;                                       // foreign currency code
                    tender.AmountInForeignCurrency = tenderDetail.AmountInForeignCurrency;          // foreign amount
                    tender.ForeignCurrencyExchangeRate = tenderDetail.ForeignCurrencyExchangeRate;  // foreign to channel currency exchange rate
                }
    
                // Retrieve the amount in company currency with the exchange rate between foreign and company currency
                Tuple<decimal, decimal> companyCurrencyValues = StoreOperationServiceHelper.GetCompanyCurrencyValues(context, tender.AmountInForeignCurrency, tender.ForeignCurrency);
                tender.AmountInCompanyCurrency = companyCurrencyValues.Item1;       // amount MST
                tender.CompanyCurrencyExchangeRate = companyCurrencyValues.Item2;   // exchange rate MST
    
                return tender;
            }

            /// <summary>
            /// Gets the object of non sales tender operation.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="shiftId">The shift identifier.</param>
            /// <param name="shiftTerminalId">The shift terminal identifier.</param>
            /// <param name="tenderType">The non sale tender type.</param>
            /// <returns>The non sales tender operation object.</returns>
            internal static NonSalesTransaction ConvertToNonSalesTenderTransaction(RequestContext context, string shiftId, string shiftTerminalId, TransactionType tenderType)
            {
                var transaction = new NonSalesTransaction
                {
                    ShiftId = shiftId,
                    TransactionType = tenderType,
                    TenderTypeId = StoreOperationServiceHelper.GetCashTenderTypeIdentifier(context),  // Default it to cash.
                    StoreId = context.GetOrgUnit().OrgUnitNumber,
                    StaffId = context.GetPrincipal().UserId,
                    TerminalId = context.GetTerminal().TerminalId,
                    ShiftTerminalId = string.IsNullOrWhiteSpace(shiftTerminalId) ? context.GetPrincipal().ShiftTerminalId : shiftTerminalId
                };

                return transaction;
            }
    
            /// <summary>
            /// Gets the object of non sales tender operation.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="nonSaleTenderServiceRequest">The non-sale shift tender operation request.</param>
            /// <returns>The non sales tender operation object.</returns>
            internal static NonSalesTransaction ConvertToNonSalesTenderTransaction(RequestContext context, SaveNonSaleTenderServiceRequest nonSaleTenderServiceRequest)
            {
                string channelCurrency = context.GetChannelConfiguration().Currency;
                var transaction = new NonSalesTransaction
                {
                    Id = nonSaleTenderServiceRequest.TransactionId,
                    ShiftId = nonSaleTenderServiceRequest.ShiftId,
                    ShiftTerminalId = string.IsNullOrWhiteSpace(nonSaleTenderServiceRequest.ShiftTerminalId) ? context.GetPrincipal().ShiftTerminalId : nonSaleTenderServiceRequest.ShiftTerminalId,
                    Description = nonSaleTenderServiceRequest.Description,
                    TransactionType = nonSaleTenderServiceRequest.TransactionType,
                    TenderTypeId = StoreOperationServiceHelper.GetCashTenderTypeIdentifier(context),  // Default it to cash.
                    StoreId = context.GetOrgUnit().OrgUnitNumber,
                    StaffId = context.GetPrincipal().UserId,
                    TerminalId = context.GetTerminal().TerminalId,
                    ChannelCurrencyExchangeRate = StoreOperationServiceHelper.GetExchangeRate(context),
                    Amount = nonSaleTenderServiceRequest.Amount,                    // amount in store currency
                    ForeignCurrency = string.IsNullOrWhiteSpace(nonSaleTenderServiceRequest.Currency) ? channelCurrency : nonSaleTenderServiceRequest.Currency,
                    ChannelCurrency = context.GetChannelConfiguration().Currency   // channel currency code
                };
    
                // Retrieve the amount in foreign currency with the exchange rate between foreign and channel currency
                Tuple<decimal, decimal> foreignCurrencyValues = StoreOperationServiceHelper.GetForeignCurrencyValues(context, transaction.Amount, transaction.ForeignCurrency);
                transaction.AmountInForeignCurrency = foreignCurrencyValues.Item1;          // foreign currency amount
                transaction.ForeignCurrencyExchangeRate = foreignCurrencyValues.Item2;      // foreign currency exchange rate
    
                // Retrieve the amount in company currency with the exchange rate between foreign and company currency
                Tuple<decimal, decimal> companyCurrencyValues = StoreOperationServiceHelper.GetCompanyCurrencyValues(context, transaction.AmountInForeignCurrency, transaction.ForeignCurrency);
                transaction.AmountInCompanyCurrency = companyCurrencyValues.Item1;      // amount MST
                transaction.CompanyCurrencyExchangeRate = companyCurrencyValues.Item2;  // exchange rate MST

                if (nonSaleTenderServiceRequest.ReasonCodeLines != null && nonSaleTenderServiceRequest.ReasonCodeLines.Any())
                {
                    // Read reason code details from service request for open drawer operation
                    transaction.ReasonCodeLines = new Collection<ReasonCodeLine>();
                    foreach (var reasonCodeLine in nonSaleTenderServiceRequest.ReasonCodeLines)
                    {
                        transaction.ReasonCodeLines.Add(reasonCodeLine);
                    }
                }

                return transaction;
            }
    
            /// <summary>
            /// Gets the exchange rate between company currency and channel currency.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The exchange rate between company and channel currency.</returns>
            private static decimal GetExchangeRate(RequestContext context)
            {
                // Default exchange rate value if the company currency and the channel currency are the same.
                decimal exchangeRate = 1.00M;
    
                if (!context.GetChannelConfiguration().Currency.Equals(context.GetChannelConfiguration().CompanyCurrency, StringComparison.OrdinalIgnoreCase))
                {
                    var currencyRequest = new GetExchangeRateServiceRequest(context.GetChannelConfiguration().CompanyCurrency, context.GetChannelConfiguration().Currency);
                    var currencyResponse = context.Execute<GetExchangeRateServiceResponse>(currencyRequest);
                    exchangeRate = currencyResponse.ExchangeRate;
                }
    
                return exchangeRate;
            }
    
            /// <summary>
            /// Gets the amount in company currency and exchange rate between the foreign currency and company currency.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="foreignCurrencyAmount">The foreign currency amount to be converted into company currency.</param>
            /// <param name="foreignCurrency">The foreign currency code.</param>
            /// <returns>The Item1 is the company amount and the Item2 is the exchange rate from foreign currency and company currency.</returns>
            private static Tuple<decimal, decimal> GetCompanyCurrencyValues(RequestContext context, decimal foreignCurrencyAmount, string foreignCurrency)
            {
                decimal companyAmount = foreignCurrencyAmount;
                decimal companyExchangeRate = 1.00M;
    
                if (!foreignCurrency.Equals(context.GetChannelConfiguration().CompanyCurrency, StringComparison.OrdinalIgnoreCase))
                {
                    var currencyRequest = new GetCurrencyValueServiceRequest(foreignCurrency, context.GetChannelConfiguration().CompanyCurrency, foreignCurrencyAmount);
                    var currencyResponse = context.Execute<GetCurrencyValueServiceResponse>(currencyRequest);
                    companyAmount = currencyResponse.RoundedConvertedAmount;
                    companyExchangeRate = currencyResponse.ExchangeRate;   // exchange rate from foreign to company currency
                }
    
                return new Tuple<decimal, decimal>(companyAmount, companyExchangeRate);
            }
    
            /// <summary>
            /// Gets the amount in foreign currency and exchange rate between the foreign currency and channel currency.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="chanelCurrencyAmount">The channel currency amount to be converted into foreign currency.</param>
            /// <param name="foreignCurrency">The foreign currency code.</param>
            /// <returns>The Item1 is the foreign currency amount and the Item2 is the exchange rate between the foreign currency and channel currency.</returns>
            private static Tuple<decimal, decimal> GetForeignCurrencyValues(RequestContext context, decimal chanelCurrencyAmount, string foreignCurrency)
            {
                decimal foreignAmount = chanelCurrencyAmount;
                decimal foreignExchangeRate = 1.00M;
    
                if (!foreignCurrency.Equals(context.GetChannelConfiguration().Currency, StringComparison.OrdinalIgnoreCase))
                {
                    var currencyRequest = new GetCurrencyValueServiceRequest(context.GetChannelConfiguration().Currency, foreignCurrency, chanelCurrencyAmount);
                    var currencyResponse = context.Execute<GetCurrencyValueServiceResponse>(currencyRequest);
                    decimal storeExchangeRate = currencyResponse.ExchangeRate;   // exchange rate from store to foreign currency
                    foreignAmount = currencyResponse.RoundedConvertedAmount;
                    foreignExchangeRate = (storeExchangeRate == 0m) ? 1m : 1m / storeExchangeRate;   // exchange rate from foreign to channel currency
                }
    
                return new Tuple<decimal, decimal>(foreignAmount, foreignExchangeRate);
            }
    
            /// <summary>
            /// Retrieves the cash tender type identifier.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The tender type identifier for cash tender type.</returns>
            private static string GetCashTenderTypeIdentifier(RequestContext context)
            {
                string cashTenderTypeId = null;
                int payCashOperationId = (int)RetailOperation.PayCash;
    
                var dataServiceRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                EntityDataServiceResponse<TenderType> dataServiceResponse = context.Execute<EntityDataServiceResponse<TenderType>>(dataServiceRequest);
                ReadOnlyCollection<TenderType> tenderTypes = dataServiceResponse.PagedEntityCollection.Results;
    
                if (tenderTypes != null)
                {
                    cashTenderTypeId = tenderTypes.Where(t => t.OperationId == payCashOperationId).Select(t => t.TenderTypeId).FirstOrDefault();
                }
    
                return cashTenderTypeId != null ? cashTenderTypeId : "-1"; // Defaulting to -1, same as EPOS
            }
    
            /// <summary>
            /// Gets the tender types for the channel.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The dictionary of tender types where key is tender type identifier, and value is tender type object.</returns>
            private static Dictionary<string, TenderType> GetChannelTenderTypes(RequestContext context)
            {
                long channelId = context.GetPrincipal().ChannelId;
                var dataServiceRequest = new GetChannelTenderTypesDataRequest(channelId, QueryResultSettings.AllRecords);
                EntityDataServiceResponse<TenderType> dataServiceResponse = context.Runtime.Execute<EntityDataServiceResponse<TenderType>>(dataServiceRequest, context);
                ReadOnlyCollection<TenderType> tenderTypes = dataServiceResponse.PagedEntityCollection.Results;
    
                if (tenderTypes == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, 
                        string.Format("The tender types of channel {0} was not found.", channelId));
                }
    
                var tenderTypeDict = tenderTypes.ToDictionary(t => t.TenderTypeId, t => t);
    
                return tenderTypeDict;
            }
    
            /// <summary>
            /// Gets required tender declaration amounts of a shift per tender type.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="shiftTerminalId">The shift terminal identifier.</param>
            /// <param name="shiftId">The shift identifier.</param>
            /// <returns>The dictionary of shift tender lines where key is tender type identifier, and value is shift tender line object.</returns>
            private static Dictionary<string, ShiftTenderLine> GetShiftRequiredAmountsPerTender(RequestContext context, string shiftTerminalId, string shiftId)
            {
                var dataServiceRequest = new GetShiftRequiredAmountsPerTenderDataRequest(shiftTerminalId, shiftId);
                EntityDataServiceResponse<ShiftTenderLine> dataServiceResponse = context.Runtime.Execute<EntityDataServiceResponse<ShiftTenderLine>>(dataServiceRequest, context);
                ReadOnlyCollection<ShiftTenderLine> shiftTenderLines = dataServiceResponse.PagedEntityCollection.Results;
    
                if (shiftTenderLines == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                        string.Format("No tender line was found on the terminal {0} for shift {1}.", shiftTerminalId, shiftId));
                }
    
                var shiftTenderLineDict = shiftTenderLines.ToDictionary(s => s.TenderTypeId, s => s);
    
                return shiftTenderLineDict;
            }
        }
    }
}
