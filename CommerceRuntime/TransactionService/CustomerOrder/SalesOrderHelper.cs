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
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Helper class to convert the searchCustomerOrderList transaction server APIs.
        /// </summary>
        internal static class SalesOrderHelper
        {
            /// <summary>
            /// Converts the customer order info object into a sales order.
            /// </summary>
            /// <param name="orderInfo">Customer order info object to be converted.</param>
            /// <param name="channelConfiguration">The CRT channel configuration.</param>
            /// <param name="context">The CRT context.</param>
            /// <returns>The converted sales order.</returns>
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            public static SalesOrder GetSalesOrderFromInfo(CustomerOrderInfo orderInfo, ChannelConfiguration channelConfiguration, RequestContext context)
            {
                // Stores the local copy. There is high probability of having the same shipping/delivery address on all the lines.
                Dictionary<long, Address> shippingAddressDictionary = new Dictionary<long, Address>();

                decimal shippingChargeAmount;
    
                ColumnSet columnSet = new ColumnSet();
                var salesOrder = new SalesOrder
                    {
                        SalesId = orderInfo.Id,
                        TransactionType = SalesTransactionType.CustomerOrder,
                        CustomerOrderMode = CustomerOrderMode.OrderRecalled,
                        CartType = CartType.CustomerOrder,
                        CustomerOrderType = orderInfo.OrderType,
                        StoreId = orderInfo.StoreId,
                        IsTaxIncludedInPrice = Convert.ToBoolean(orderInfo.IsTaxIncludedInPrice)
                };
    
                switch (orderInfo.OrderType)
                {
                    case CustomerOrderType.Quote:
                        salesOrder.Status = Utilities.GetSalesStatus((SalesQuotationStatus)orderInfo.Status);
                        break;
                    case CustomerOrderType.SalesOrder:
                        salesOrder.Status = Utilities.GetSalesStatus((SalesOrderStatus)orderInfo.Status, (DocumentStatus)orderInfo.DocumentStatus);
                        break;
                    default:
                        salesOrder.Status = SalesStatus.Unknown;
                        break;
                }
    
                DateTimeOffset currentChannelDate = context.GetNowInChannelTimeZone();
    
                salesOrder.RequestedDeliveryDate = Utilities.ParseDateStringAsDateTimeOffset(orderInfo.RequestedDeliveryDateString, currentChannelDate.Date, currentChannelDate.Offset);
                salesOrder.QuotationExpiryDate = Utilities.ParseDateStringAsDateTimeOffset(orderInfo.ExpiryDateString, currentChannelDate.Date, currentChannelDate.Offset);
    
                // CreationDate is stored in UTC. It needs to be converted to local time zone where order is accessed.
                salesOrder.BeginDateTime = Utilities.ParseDateString(orderInfo.CreationDateString, currentChannelDate.ToUniversalTime().DateTime, DateTimeStyles.AssumeUniversal);
    
                salesOrder.Comment = orderInfo.Comment;
    
                // Header delivery
                salesOrder.InventoryLocationId = orderInfo.WarehouseId;
                salesOrder.DeliveryMode = orderInfo.DeliveryMode;

                foreach (var discountCode in orderInfo.DiscountCodes)
                {
                    salesOrder.DiscountCodes.Add(discountCode);
                }
    
                // Customer info
                salesOrder.CustomerId = orderInfo.CustomerAccount;
                long addressRecordIdLong = 0;
                if (long.TryParse(orderInfo.AddressRecordId, out addressRecordIdLong))
                {
                    var dataServiceRequest = new GetAddressDataRequest(addressRecordIdLong, columnSet);
                    SingleEntityDataServiceResponse<Address> dataServiceResponse = context.Execute<SingleEntityDataServiceResponse<Address>>(dataServiceRequest);

                    if (dataServiceResponse.Entity == null)
                    {
                        Utilities.DownloadCustomerData(context, salesOrder.CustomerId);
                        dataServiceResponse = context.Execute<SingleEntityDataServiceResponse<Address>>(dataServiceRequest);
                    }

                    if (dataServiceResponse.Entity != null)
                    {
                        salesOrder.ShippingAddress = dataServiceResponse.Entity;
                        shippingAddressDictionary.Add(salesOrder.ShippingAddress.RecordId, salesOrder.ShippingAddress);
                    }
                }

                if (!string.IsNullOrEmpty(orderInfo.SalespersonStaffId))
                {
                    // Sets the sales person id and name according to AX values
                    // This is done because we do not know whether the sales person information is available on this store
                    salesOrder.StaffId = orderInfo.SalespersonStaffId;
                }
    
                salesOrder.ChannelReferenceId = orderInfo.ChannelReferenceId;
                salesOrder.LoyaltyCardId = orderInfo.LoyaltyCardId;
    
                salesOrder.ReceiptEmail = orderInfo.Email;
    
                string shippingChargeCode = channelConfiguration.ShippingChargeCode;              

                // Items
                int lineId = 0;
    
                foreach (ItemInfo item in orderInfo.Items)
                {
                    lineId++;
                    var lineItem = new SalesLine
                        {
                            LineId = lineId.ToString(CultureInfo.InvariantCulture),
                            Found = true,
                            RecordId = item.RecId,
                            ItemId = item.ItemId,
                            Comment = item.Comment,
                            Quantity = item.Quantity,
                            ReturnQuantity = item.Quantity,
                            SalesOrderUnitOfMeasure = item.Unit,
                            UnitOfMeasureSymbol = item.Unit,
                            Price = item.Price,
                            NetAmount = item.NetAmount,
                            QuantityOrdered = item.Quantity,
                            QuantityInvoiced = item.QuantityPicked,
                            DeliveryMode = item.DeliveryMode,
                            RequestedDeliveryDate = Utilities.ParseDateStringAsDateTimeOffset(item.RequestedDeliveryDateString, currentChannelDate.Date, currentChannelDate.Offset),
                            FulfillmentStoreId = item.FulfillmentStoreId,
                            InventoryLocationId = item.WarehouseId,
                            SerialNumber = item.SerialId,
                            BatchId = item.BatchId,
                            Status = TransactionStatus.Normal,
                            SalesStatus = Utilities.GetSalesStatus((SalesOrderStatus)item.Status)
                        };
    
                    // Copy charges to line and calculates total shipping charge amount
                    lineItem.ChargeLines.AddRange(SalesOrderHelper.CreateChargeLines(item.Charges, shippingChargeCode, salesOrder.BeginDateTime, out shippingChargeAmount));
                    lineItem.DeliveryModeChargeAmount = shippingChargeAmount;
    
                    // Line level discount amounts
                    lineItem.LineDiscount = item.LineDscAmount;
                    lineItem.PeriodicDiscount = item.PeriodicDiscount;
                    lineItem.PeriodicPercentageDiscount = item.PeriodicPercentageDiscount;
                    lineItem.LineManualDiscountAmount = item.LineManualDiscountAmount;
                    lineItem.LineManualDiscountPercentage = item.LineManualDiscountPercentage;
                    lineItem.TotalDiscount = item.TotalDiscount;
                    lineItem.TotalPercentageDiscount = item.TotalPctDiscount;
    
                    // Copy discounts to line
                    lineItem.DiscountLines.AddRange(SalesOrderHelper.CreateDiscountLines(item.Discounts));
    
                    long itemAddressRecordIdLong;
                    if (long.TryParse(item.AddressRecordId, out itemAddressRecordIdLong))
                    {
                        Address lineLevelshippingAddress = new Address();

                        if (!shippingAddressDictionary.TryGetValue(itemAddressRecordIdLong, out lineLevelshippingAddress))
                        {
                            var dataServiceRequest = new GetAddressDataRequest(itemAddressRecordIdLong, columnSet);
                            SingleEntityDataServiceResponse<Address> dataServiceResponse = context.Execute<SingleEntityDataServiceResponse<Address>>(dataServiceRequest);
                            
                            // If address not found download and get.
                            if (dataServiceResponse.Entity == null)
                            {
                                Utilities.DownloadCustomerData(context, salesOrder.CustomerId);
                                dataServiceResponse = context.Execute<SingleEntityDataServiceResponse<Address>>(dataServiceRequest);
                            }

                            if (dataServiceResponse.Entity != null)
                            {
                                lineItem.ShippingAddress = dataServiceResponse.Entity;
                                shippingAddressDictionary.Add(lineItem.ShippingAddress.RecordId, lineItem.ShippingAddress);
                            }
                        }
                        else
                        {
                            lineItem.ShippingAddress = lineLevelshippingAddress;
                        }
                    }
    
                    Utilities.SetUpVariantAndProduct(context, item.InventDimensionId, lineItem.ItemId, lineItem);
    
                    lineItem.DiscountAmount = item.Discount;
    
                    // Set tax info after defaults, as it may have been overridden.
                    lineItem.SalesTaxGroupId = item.SalesTaxGroup ?? string.Empty;
                    lineItem.ItemTaxGroupId = item.ItemTaxGroup ?? string.Empty;
    
                    // Add it to the transaction
                    salesOrder.SalesLines.Add(lineItem);
                }
    
                // Charges for the header
                salesOrder.ChargeLines.AddRange(SalesOrderHelper.CreateChargeLines(orderInfo.Charges, shippingChargeCode, salesOrder.BeginDateTime, out shippingChargeAmount));
                salesOrder.DeliveryModeChargeAmount = shippingChargeAmount;
    
                // Payments
                // - total up amounts
                // - add history entries
                decimal nonPrepayments = decimal.Zero;
                decimal prepaymentAmountPaid = decimal.Zero;
    
                int tenderLineId = 0;                        

                foreach (PaymentInfo payment in orderInfo.Payments)
                {
                    if (salesOrder.TenderLines == null)
                    {
                        salesOrder.TenderLines = new Collection<TenderLine>();
                    }
    
                    decimal amount = 0M;
                    if (string.IsNullOrWhiteSpace(payment.Currency) || payment.Currency.Equals(channelConfiguration.Currency, StringComparison.OrdinalIgnoreCase))
                    {
                        amount = payment.Amount;                      
                    }
                    else
                    {                        
                        GetCurrencyValueServiceRequest currencyValueRequest = new GetCurrencyValueServiceRequest(payment.Currency, channelConfiguration.Currency, payment.Amount);
                        GetCurrencyValueServiceResponse currencyValueResponse = context.Execute<GetCurrencyValueServiceResponse>(currencyValueRequest);                        
                        amount = currencyValueResponse.RoundedConvertedAmount;                     
                    }
    
                    if (payment.Prepayment)
                    {
                        // Sum prepayments to track total deposits paid
                        prepaymentAmountPaid += amount;
                    }
                    else
                    {
                        // Sum non-prepayments as base for calculating deposits applied to pickups
                        nonPrepayments += amount;
                    }
    
                    tenderLineId++;
                    var tenderLine = new TenderLine
                            {
                                TenderLineId = tenderLineId.ToString(CultureInfo.InvariantCulture),
                                Amount = payment.Amount,
                                Currency = payment.Currency,
                                CardTypeId = string.Empty,
                                Status = TenderLineStatus.Historical,
                                IsVoidable = false,
                                TenderDate =
                                    Utilities.ParseDateString(
                                        payment.DateString,
                                        currentChannelDate.Date,
                                        DateTimeStyles.None) // On channel timezone
                            };
    
                    salesOrder.TenderLines.Add(tenderLine);
                }
    
                if (orderInfo.Affiliations != null && orderInfo.Affiliations.Any())
                {
                    salesOrder.AffiliationLoyaltyTierLines.Clear();
                    salesOrder.AffiliationLoyaltyTierLines.AddRange(orderInfo.Affiliations.Select(line =>
                        new SalesAffiliationLoyaltyTier
                        {
                            AffiliationId = line.AffiliationRecordId,
                            LoyaltyTierId = line.LoyaltyTierRecordId,
                            AffiliationType = line.AffiliationType
                        }));
                }
    
                // Prepayment/Deposit override info
                if (orderInfo.PrepaymentAmountOverridden)
                {
                    salesOrder.OverriddenDepositAmount = prepaymentAmountPaid;
                }
    
                salesOrder.PrepaymentAmountPaid = prepaymentAmountPaid;
    
                // Portion of the prepayment that has been applied to invoices
                // (total amount invoiced less payments, difference is the deposit applied)
                salesOrder.PrepaymentAmountInvoiced = orderInfo.PreviouslyInvoicedAmount - nonPrepayments;

                // if the prepayment invoiced is greater than the total paid as deposit, there is no credit left
                salesOrder.AvailableDepositAmount = Math.Max(decimal.Zero, salesOrder.PrepaymentAmountPaid - salesOrder.PrepaymentAmountInvoiced);
                
                salesOrder.HasLoyaltyPayment = orderInfo.HasLoyaltyPayment;
                salesOrder.CurrencyCode = orderInfo.CurrencyCode;
                //DEMO4 NEW //Add extension property HasReturns
                salesOrder.SetProperty("HasReturns", orderInfo.HasReturns.ToLower() == "true");


                return salesOrder;
            }

            /// <summary>
            /// Creates the charge lines from a charge info collection and calculates the shipping charge amount from charge lines.
            /// </summary>
            /// <param name="chargeInfoCollection">The charge info collection.</param>
            /// <param name="shippingChargeCode">The shipping charge code.</param>
            /// <param name="beginDateTime">The begin date time of order.</param>            
            /// <param name="shippingChargeAmount">The amount of shipping charges.</param>
            /// <returns>The shipping charge amount.</returns>
            private static List<ChargeLine> CreateChargeLines(IEnumerable<ChargeInfo> chargeInfoCollection, string shippingChargeCode, DateTimeOffset beginDateTime, out decimal shippingChargeAmount)
            {
                List<ChargeLine> chargeLines = new List<ChargeLine>();
                shippingChargeAmount = decimal.Zero;
    
                foreach (ChargeInfo charge in chargeInfoCollection)
                {
                    ChargeLine chargeLine = SalesOrderHelper.CreateChargeLine(charge);

                    if (!chargeLine.BeginDateTime.IsValidAxDateTime())
                    {
                        chargeLine.BeginDateTime = beginDateTime.UtcDateTime;
                    }                                       

                    if (!chargeLine.EndDateTime.IsValidAxDateTime())
                    {
                        chargeLine.EndDateTime = DateTimeOffsetExtensions.AxMaxDateValue.UtcDateTime;
                    }                   

                    chargeLines.Add(chargeLine);
    
                    if (chargeLine.ChargeCode.Equals(shippingChargeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        shippingChargeAmount += chargeLine.CalculatedAmount;
                    }                    
                }
    
                return chargeLines;
            }
    
            /// <summary>
            /// Creates the discount lines from a discount info collection.
            /// </summary>
            /// <param name="discountInfoCollection">Discount info collection.</param>
            /// <returns>List of discount lines.</returns>
            private static IEnumerable<DiscountLine> CreateDiscountLines(IEnumerable<DiscountInfo> discountInfoCollection)
            {
                return discountInfoCollection.Select(SalesOrderHelper.CreateDiscountLine);
            }
    
            /// <summary>
            /// Create a charge line from the charge info object.
            /// </summary>
            /// <param name="charge">The charge info object.</param>
            /// <returns>The created charge line.</returns>
            private static ChargeLine CreateChargeLine(ChargeInfo charge)
            {
                var lineCharge = new ChargeLine
                {
                        NetAmount = charge.Amount,
                        CalculatedAmount = charge.Amount,
                        NetAmountPerUnit = charge.Amount,
                        Quantity = 1,
                        ChargeCode = charge.Code ?? string.Empty,
                        SalesTaxGroupId = charge.SalesTaxGroup ?? string.Empty,
                        ItemTaxGroupId = charge.TaxGroup ?? string.Empty,
                        ModuleType = ChargeModule.Sales,
                        ChargeType = ChargeType.ManualCharge,
                        GrossAmount = charge.Amount,
                        NetAmountWithAllInclusiveTax = charge.Amount                    
                };
    
                return lineCharge;
            }
    
            /// <summary>
            /// Create a discount line from the discount info object.
            /// </summary>
            /// <param name="discount">The discount info object.</param>
            /// <returns>The created discount line.</returns>
            private static DiscountLine CreateDiscountLine(DiscountInfo discount)
            {
                return new DiscountLine
                    {
                        EffectiveAmount = discount.Amount,
                        Amount = discount.DiscountAmount,
                        DiscountLineType = (DiscountLineType)discount.DiscountOriginType,
                        DiscountCode = discount.DiscountCode,
                        CustomerDiscountType = (CustomerDiscountType)discount.CustomerDiscountType,
                        ManualDiscountType = (ManualDiscountType)discount.ManualDiscountType,
                        OfferId = discount.PeriodicDiscountOfferId,
                        Percentage = discount.Percentage,
                        DealPrice = discount.DealPrice,
                        OfferName = discount.OfferName
                    };
            }
        }
    }
}
