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
    namespace Commerce.Runtime.Services.CustomerOrder
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class to serialize a sales transaction to be send to transaction server APIs.
        /// </summary>
        internal static class SalesTransactionSerializationHelper
        {
            /// <summary>
            /// Converts a sales transaction into the XML that transaction service can consume.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction to be converted.</param>
            /// <param name="cardToken">The card token object.</param>
            /// <param name="cardTokenResponseXml">The card token response XML.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The customer order info object.</returns>
            public static string ConvertSalesTransactionToXml(
                SalesTransaction salesTransaction,
                CardTokenInfo cardToken,
                string cardTokenResponseXml,
                RequestContext context)
            {
                string storeCurrency = context.GetChannelConfiguration().Currency;
    
                CustomerOrderInfo parameters = new CustomerOrderInfo();
    
                parameters.OrderType = salesTransaction.CustomerOrderMode == CustomerOrderMode.QuoteCreateOrEdit
                    ? CustomerOrderType.Quote
                    : CustomerOrderType.SalesOrder;
                parameters.AutoPickOrder = false;
    
                parameters.Id = salesTransaction.SalesId;
                parameters.QuotationId = salesTransaction.SalesId;
    
                parameters.CustomerAccount = salesTransaction.CustomerId;
                parameters.ChannelRecordId = salesTransaction.ChannelId.ToString(CultureInfo.InvariantCulture);
                parameters.SalespersonStaffId = salesTransaction.StaffId ?? string.Empty;
                parameters.IsTaxIncludedInPrice = salesTransaction.IsTaxIncludedInPrice.ToString();
    
                parameters.WarehouseId = salesTransaction.InventoryLocationId;
                parameters.CurrencyCode = storeCurrency;
                parameters.StoreId = salesTransaction.StoreId;
                parameters.TerminalId = salesTransaction.TerminalId;
                parameters.TransactionId = salesTransaction.Id;
                parameters.LocalHourOfDay = DateTime.Now.Hour + 1; // headquarters will match this value with an id
    
                parameters.AddressRecordId = SalesTransactionSerializationHelper.ConvertAddressRecIdToString(salesTransaction.ShippingAddress);
    
                parameters.ExpiryDateString = Utilities.ConvertDateToAxString(salesTransaction.QuotationExpiryDate);
    
                parameters.RequestedDeliveryDateString = Utilities.ConvertDateToAxString(salesTransaction.RequestedDeliveryDate);
                parameters.DeliveryMode = string.IsNullOrWhiteSpace(salesTransaction.DeliveryMode) ? string.Empty : salesTransaction.DeliveryMode;
                parameters.PrepaymentAmountOverridden = salesTransaction.IsDepositOverridden;
                parameters.PrepaymentAmountApplied = salesTransaction.PrepaymentAmountAppliedOnPickup;
                parameters.OriginalTransactionTime = DateTime.UtcNow;
    
                parameters.TotalManualDiscountAmount = salesTransaction.TotalManualDiscountAmount;
                parameters.TotalManualDiscountPercentage = salesTransaction.TotalManualDiscountPercentage;
    
                parameters.Email = salesTransaction.ReceiptEmail;
                parameters.Comment = salesTransaction.Comment ?? string.Empty;
                parameters.LoyaltyCardId = string.IsNullOrWhiteSpace(salesTransaction.LoyaltyCardId) ? string.Empty : salesTransaction.LoyaltyCardId;
    
                parameters.ChannelReferenceId = salesTransaction.ChannelReferenceId;
    
                parameters.ReturnReasonCodeId = SalesTransactionSerializationHelper.GetReturnReasonCodeId(salesTransaction, context);
    
                if (cardToken != null)
                {
                    parameters.CreditCardToken = cardTokenResponseXml;
                }
    
                // Sales lines
                SalesTransactionSerializationHelper.FillSaleLines(parameters, salesTransaction);
    
                // Payments
                SalesTransactionSerializationHelper.FillPaymentInformation(parameters, salesTransaction, storeCurrency);
    
                // Charges
                SalesTransactionSerializationHelper.ConvertChargeLinesToChargeInfos(salesTransaction.ChargeLines, parameters.Charges);
    
                // Affiliations and loyalty tiers
                SalesTransactionSerializationHelper.FillAffiliationsAndLoyaltyTiers(parameters, salesTransaction);
    
                // Discount codes
                parameters.DiscountCodes.AddRange(salesTransaction.DiscountCodes.ToArray());
    
                // Extension properties
                parameters.ExtensionProperties.AddRange(salesTransaction.ExtensionProperties);
    
                return parameters.ToXml();
            }
    
            /// <summary>
            /// Fills the customer order info object with affiliations and loyalty tiers from the sales transaction.
            /// </summary>
            /// <param name="parameters">CustomerOrderInfo object to be filled.</param>
            /// <param name="salesTransaction">The sales transaction to be used as a base.</param>
            private static void FillAffiliationsAndLoyaltyTiers(CustomerOrderInfo parameters, SalesTransaction salesTransaction)
            {
                var affiliations = salesTransaction.AffiliationLoyaltyTierLines.Select(line =>
                    new AffiliationInfo
                    {
                        AffiliationRecordId = line.AffiliationId,
                        LoyaltyTierRecordId = line.LoyaltyTierId,
                        AffiliationType = line.AffiliationType
                    });
    
                parameters.Affiliations.AddRange(affiliations.ToList());
            }
    
            /// <summary>
            /// Fills a customer order info object with payment information from a sales transaction.
            /// </summary>
            /// <param name="parameters">CustomerOrderInfo object to be filled.</param>
            /// <param name="salesTransaction">The sales transaction to be used as base.</param>
            /// <param name="storeCurrency">The store currency.</param>
            private static void FillPaymentInformation(CustomerOrderInfo parameters, SalesTransaction salesTransaction, string storeCurrency)
            {
                foreach (TenderLine tenderLine in salesTransaction.TenderLines)
                {
                    if (tenderLine.Status != TenderLineStatus.Voided
                        && tenderLine.Status != TenderLineStatus.Historical
                        && (tenderLine.Amount != decimal.Zero || !tenderLine.IsPreProcessed))
                    {
                        PaymentInfo paymentInfo = new PaymentInfo()
                        {
                            PaymentType = tenderLine.TenderTypeId,
                            CardType = tenderLine.CardTypeId ?? string.Empty,
                            Amount = tenderLine.Currency == null ? tenderLine.AmountInCompanyCurrency : tenderLine.AmountInTenderedCurrency,
                            Currency = tenderLine.Currency ?? storeCurrency,
                            Prepayment = tenderLine.IsDeposit,
                            CreditCardAuthorization = tenderLine.Authorization ?? string.Empty,
                            PaymentCaptured = tenderLine.IsPaymentCaptured,
                            CreditCardToken = tenderLine.CardToken ?? string.Empty
                        };
    
                        parameters.Payments.Add(paymentInfo);
                    }
                }
            }
    
            /// <summary>
            /// Fills a customer order info object with a sales transaction sales line information.
            /// </summary>
            /// <param name="parameters">The customer order info object to be filled.</param>
            /// <param name="salesTransaction">The sales transaction object used as source.</param>
            private static void FillSaleLines(CustomerOrderInfo parameters, SalesTransaction salesTransaction)
            {
                ProductVariant emptyVariant = new ProductVariant();
    
                // Line Items
                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    if (!salesLine.IsVoided && !(salesTransaction.CustomerOrderMode == CustomerOrderMode.Return && salesLine.Quantity == 0))
                    {
                        //DEMO4 //TODO: AM //For a pick up order, Filter lines with quantity equal to zero
                        if (salesTransaction.CustomerOrderMode == CustomerOrderMode.Pickup && salesLine.TotalAmount == 0)
                            continue;

                        // use property from header, override with line property if available
                        string deliveryMode = string.IsNullOrWhiteSpace(salesLine.DeliveryMode) ? parameters.DeliveryMode : salesLine.DeliveryMode;
    
                        // use property from header, override with line property if available
                        string deliveryDateString =
                            Utilities.IsDateNullOrDefaultValue(salesLine.RequestedDeliveryDate)
                                ? parameters.RequestedDeliveryDateString
                                : Utilities.ConvertDateToAxString(salesLine.RequestedDeliveryDate);
    
                        // If no line-level warehouse is specified, fall back to the header warehouse
                        string inventLocationId = string.IsNullOrWhiteSpace(salesLine.InventoryLocationId) ? parameters.WarehouseId : salesLine.InventoryLocationId;
    
                        // AX SO line stores discount amount per item, CRT stores for whole line, calculate per item discount amount
                        decimal lineDiscount = salesLine.Quantity == 0M ? 0M : (salesLine.TotalDiscount + salesLine.LineDiscount + salesLine.PeriodicDiscount) / salesLine.Quantity;
    
                        string lineAddress = SalesTransactionSerializationHelper.ConvertAddressRecIdToString(salesLine.ShippingAddress);
    
                        // use header address is line does not have it set
                        if (string.IsNullOrWhiteSpace(lineAddress))
                        {
                            lineAddress = parameters.AddressRecordId;
                        }
    
                        ProductVariant variant = salesLine.Variant ?? emptyVariant;
    
                        ItemInfo itemInfo = new ItemInfo()
                        {
                            RecId = salesLine.RecordId,
    
                            // quantity
                            ItemId = salesLine.ItemId,
                            Quantity = salesLine.Quantity,
                            Comment = salesLine.Comment,
                            Unit = salesLine.SalesOrderUnitOfMeasure,
    
                            // pricing
                            Price = salesLine.Price,
                            Discount = lineDiscount,
                            NetAmount = salesLine.NetAmount,
                            ItemTaxGroup = salesLine.ItemTaxGroupId,
                            SalesTaxGroup = salesLine.SalesTaxGroupId,
                            SalesMarkup = 0M,
    
                            PeriodicDiscount = salesLine.PeriodicDiscount,
                            PeriodicPercentageDiscount = salesLine.PeriodicPercentageDiscount,
                            LineDscAmount = salesLine.LineDiscount,
                            LineManualDiscountAmount = salesLine.LineManualDiscountAmount,
                            LineManualDiscountPercentage = salesLine.LineManualDiscountPercentage,
                            TotalDiscount = salesLine.TotalDiscount,
                            TotalPctDiscount = salesLine.TotalPercentageDiscount,
    
                            // delivery
                            WarehouseId = inventLocationId,
                            AddressRecordId = lineAddress,
                            DeliveryMode = deliveryMode,
                            RequestedDeliveryDateString = deliveryDateString,
                            FulfillmentStoreId = salesLine.FulfillmentStoreId,
    
                            // inventDim
                            BatchId = salesLine.BatchId,
                            SerialId = salesLine.SerialNumber,
                            VariantId = variant.VariantId ?? string.Empty,
                            ColorId = variant.ColorId ?? string.Empty,
                            SizeId = variant.SizeId ?? string.Empty,
                            StyleId = variant.StyleId ?? string.Empty,
                            ConfigId = variant.ConfigId ?? string.Empty,
    
                            // Return
                            InvoiceId = salesLine.ReturnTransactionId,
                            ReturnInventTransId = salesLine.ReturnInventTransId,
    
                            Catalog = salesLine.CatalogId,
    
                            Giftcard = false,
                        };
    
                        // line-level misc. charges
                        SalesTransactionSerializationHelper.ConvertChargeLinesToChargeInfos(salesLine.ChargeLines, itemInfo.Charges);
    
                        // line -level discount
                        SalesTransactionSerializationHelper.ConvertDiscountLineToDiscountInfos(salesLine.DiscountLines, itemInfo.Discounts);
    
                        parameters.Items.Add(itemInfo);
                    }
                }
            }
    
            /// <summary>
            /// Converts a collection of charge lines into a collection of charge information.
            /// </summary>
            /// <param name="chargeLines">The collection of charge lines to be converted.</param>
            /// <param name="chargeInfoCollection">The collection of charge info to have the new charges copied to.</param>
            private static void ConvertChargeLinesToChargeInfos(IEnumerable<ChargeLine> chargeLines, Collection<ChargeInfo> chargeInfoCollection)
            {
                foreach (ChargeLine chargeLine in chargeLines)
                {
                    ChargeInfo chargeInfo = new ChargeInfo()
                    {
                        Amount = chargeLine.CalculatedAmount,
                        Code = chargeLine.ChargeCode,
                        SalesTaxGroup = chargeLine.SalesTaxGroupId,
                        TaxGroup = chargeLine.ItemTaxGroupId,
                        Method = chargeLine.ChargeMethod
                    };
    
                    chargeInfoCollection.Add(chargeInfo);
                }
            }
    
            /// <summary>
            /// Converts a collection of discount lines into a collection of discount information.
            /// </summary>
            /// <param name="discountLines">The collection of discount lines to be converted.</param>
            /// <param name="discountInfoCollection">The collection of discount info to have the new charges copied to.</param>
            private static void ConvertDiscountLineToDiscountInfos(IEnumerable<DiscountLine> discountLines, Collection<DiscountInfo> discountInfoCollection)
            {
                foreach (DiscountLine discountLine in discountLines)
                {
                    DiscountInfo discountInfo = new DiscountInfo()
                    {
                        Amount = discountLine.EffectiveAmount,
                        CustomerDiscountType = (int)discountLine.CustomerDiscountType,
                        DiscountCode = discountLine.DiscountCode ?? string.Empty,
                        DiscountOriginType = (int)discountLine.DiscountLineType,
                        ManualDiscountType = (int)discountLine.ManualDiscountType,
                        PeriodicDiscountOfferId = discountLine.OfferId ?? string.Empty,
                        Percentage = discountLine.Percentage,
                        DiscountAmount = discountLine.Amount,
                        DealPrice = discountLine.DealPrice,
                    };
                    discountInfoCollection.Add(discountInfo);
                }
            }
    
            /// <summary>
            /// Gets the return reason code for the transaction or empty, if none.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction to get the reason code from.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The reason code for the transaction or empty if none.</returns>
            private static string GetReturnReasonCodeId(SalesTransaction salesTransaction, RequestContext context)
            {
                // transaction server expects only one reason code, so we just take it from the first line that has reason codes
                SalesLine firstLineWithReasonCode = salesTransaction.SalesLines.FirstOrDefault(sl => sl.ReasonCodeLines.Any());
    
                GetReturnOrderReasonCodesDataRequest getReturnOrderReasonCodesDataRequest = new GetReturnOrderReasonCodesDataRequest(QueryResultSettings.AllRecords);
                ReadOnlyCollection<ReasonCode> returnOrderReasonCodes = context.Runtime.Execute<EntityDataServiceResponse<ReasonCode>>(getReturnOrderReasonCodesDataRequest, context).PagedEntityCollection.Results;
    
                ReasonCodeLine returnReasonCodeLine = firstLineWithReasonCode != null
                    ? firstLineWithReasonCode.ReasonCodeLines.FirstOrDefault(reasonCodeLine => returnOrderReasonCodes.Any(returnOrderReasonCode => returnOrderReasonCode.ReasonCodeId == reasonCodeLine.ReasonCodeId))
                    : null;
    
                return (returnReasonCodeLine != null && !string.IsNullOrWhiteSpace(returnReasonCodeLine.ReasonCodeId)) ? returnReasonCodeLine.ReasonCodeId : null;
            }
    
            /// <summary>
            /// Gets the record identifier from an address.
            /// </summary>
            /// <param name="address">The address to get the record identifier from.</param>
            /// <returns>The record id for the address record or NULL if address is null or has invalid record identifier.</returns>
            private static string ConvertAddressRecIdToString(Address address)
            {
                if (address == null || address.RecordId == 0)
                {
                    return string.Empty;
                }
    
                return address.RecordId.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
