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
    namespace Commerce.Runtime.DataServices.SqlServer.DataServices
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// The data request handler for sales transaction.
        /// </summary>
        public sealed class SalesTransactionDataService : IRequestHandler
        {
            private const string TransactionIdColumn = "TRANSACTIONID";
            private const string EmailColumn = "EMAIL";
            private const string ItemIdColumn = "ITEMID";
            private const string BarcodeColumn = "BARCODE";
            private const string InventSerialIdColumn = "INVENTSERIALID";
            private const string StaffColumn = "STAFF";
            private const string ReceiptIdColumn = "RECEIPTID";
            private const int TransactionIdLength = 44;
            private const string DataAreaIdColumn = "DATAAREAID";
            private const string StoreIdColumn = "STOREID";
            private const string TaxCodeColumn = "TAXCODE";
            private const string TerminalIdColumn = "TERMINALID";
            private const string TaxItemGroupColumn = "TAXITEMGROUP";
            private const string OriginalTaxItemGroupColumn = "ORIGINALTAXITEMGROUP";
            private const string RetailTransactionPaymentTransView = "RETAILTRANSACTIONPAYMENTTRANSVIEW";
            private const string RetailTransactionInfoCodeTransView = "RETAILTRANSACTIONINFOCODETRANSVIEW";
            private const string RetailTransactionAttributeTransView = "RETAILTRANSACTIONATTRIBUTETRANSVIEW";
            private const string RetailTransactionAddressTransView = "RETAILTRANSACTIONADDRESSTRANSVIEW";
            private const string RetailTransactionAffiliationTransView = "RETAILTRANSACTIONAFFILIATIONTRANSVIEW";
            private const string RetailTransactionMarkupTransView = "RETAILTRANSACTIONMARKUPTRANSVIEW";
            private const string RetailTransactionOrderInvoiceTransView = "RETAILTRANSACTIONORDERINVOICETRANSVIEW";
            private const string RetailTransactionIncomeExpenseTransView = "RETAILTRANSACTIONINCOMEEXPENSETRANSVIEW";
            private const string RetailTransactionCustomerAccountDepositTransView = "RETAILTRANSACTIONCUSTOMERACCOUNTDEPOSITTRANSVIEW";
            private const string ReceiptMaskView = "RECEIPTMASKVIEW";
            private const string TransactionPropertiesView = "TRANSACTIONPROPERTIESVIEW";
            private const string ReceiptProfilesView = "RECEIPTPROFILESVIEW";
            private const string ReceiptPrintersView = "RECEIPTPRINTERSVIEW";
            private const string ReceiptInfoView = "RECEIPTINFOVIEW";
            private const string SalesTransactionViewName = "CARTSVIEW";

            private const string StoreColumn = "STORE";
            private const string TerminalColumn = "TERMINAL";
            private const string DeliverNameColumn = "DELIVERYNAME";
            private const string ZipCodeColumn = "ZIPCODE";
            private const string CountyRegionIdColumn = "COUNTRYREGIONID";
            private const string StateColumn = "STATE";
            private const string CityColumn = "CITY";
            private const string CountyColumn = "COUNTY";
            private const string StreetColumn = "STREET";
            private const string EmailContentColumn = "EMAILCONTENT";
            private const string PhoneColumn = "PHONE";
            private const string StreetNumberColumn = "STREETNUMBER";
            private const string DistrictNameColumn = "DISTRICTNAME";
            private const string SalesNameColumn = "SALESNAME";
            private const string TextValueColumn = "TEXTVALUE";
            private const string NameColumn = "NAME";
            private const string IsIncludedInPriceColumn = "ISINCLUDEDINPRICE";
            private const string AmountColumn = "AMOUNT";
            private const string CorrencyCodeColumn = "CORRENCYCODE";
            private const string MarkUpCodeColumn = "MARKUPCODE";
            private const string MarkUpLineNumColumn = "MARKUPLINENUM";
            private const string TaxGroupColumn = "TAXGROUP";
            private const string OriginalSalesTaxGroupColumn = "ORIGINALTAXGROUP";
            private const string ValueColumn = "VALUE";
            private const string CreatedDateTimeColumn = "CREATEDDATETIME";
            private const string VariantIdColumn = "VARIANTID";
            private const string UnitColumn = "UNIT";
            private const string TaxAmountColumn = "TAXAMOUNT";
            private const string ShippingDateRequestedColumn = "SHIPPINGDATEREQUESTED";
            private const string ReceiptDateRequestedColumn = "RECEIPTDATEREQUESTED";
            private const string QtyColumn = "QTY";
            private const string PriceColumn = "PRICE";
            private const string PriceChangeColumn = "PRICECHANGE";
            private const string OriginalPriceColumn = "ORIGINALPRICE";
            private const string NetAmountColumn = "NETAMOUNT";
            private const string NetAmountWithAllInclusiveTaxColumn = "NETAMOUNTINCLTAX";
            private const string LogisticsPostalAddressColumn = "LOGISTICSPOSTALADDRESS";
            private const string ListingIdColumn = "LISTINGID";
            private const string LineNumColumn = "LINENUM";
            private const string InventSiteIdColumn = "INVENTSITEID";
            private const string InventLocationIdColumn = "INVENTLOCATIONID";
            private const string InventDimIdColumn = "INVENTDIMID";
            private const string DlvModeColumn = "DLVMODE";
            private const string DiscAmountColumn = "DISCAMOUNT";
            private const string CatalogIdColumn = "CATALOG";
            private const string TotalDiscAmountColumn = "TOTALDISCAMOUNT";
            private const string TotalDiscPctColumn = "TOTALDISCPCT";
            private const string LineDscAmountColumn = "LINEDSCAMOUNT";
            private const string LineManualDiscountAmountColumn = "LINEMANUALDISCOUNTAMOUNT";
            private const string LineManualDiscountPercentageColumn = "LINEMANUALDISCOUNTPERCENTAGE";
            private const string PeriodicDiscAmountColumn = "PERIODICDISCAMOUNT";
            private const string PeriodicDiscPctColumn = "PERIODICPERCENTAGEDISCOUNT";
            private const string ChannelColumn = "CHANNEL";
            private const string TypeColumn = "TYPE";
            private const string PeriodicDiscountOfferIdColumn = "PERIODICDISCOUNTOFFERID";
            private const string DiscountCodeColumn = "DISCOUNTCODE";
            private const string ChargeMethodColumn = "METHOD";
            private const string CalculatedAmountColumn = "CALCULATEDAMOUNT";
            private const string StaffIdColumn = "STAFFID";
            private const string TransTimeColumn = "TRANSTIME";
            private const string TransDateColumn = "TRANSDATE";
            private const string TransactionStatusColumn = "TRANSACTIONSTATUS";
            private const string ReturnNoSaleColumn = "RETURNNOSALE";
            private const string ReturnTransactionIdColumn = "RETURNTRANSACTIONID";
            private const string ReturnLineNumberColumn = "RETURNLINENUM";
            private const string ReturnStoreColumn = "RETURNSTORE";
            private const string ReturnTerminalIdColumn = "RETURNTERMINALID";
            private const string ReasonCodeIdColumn = "REASONCODEID";
            private const string InformationColumn = "INFORMATION";
            private const string InformationAmountColumn = "INFOAMOUNT";
            private const string ItemTenderColumn = "ITEMTENDER";
            private const string InputTypeColumn = "INPUTTYPE";
            private const string SubReasonCodeIdColumn = "SUBREASONCODEID";
            private const string StatementCodeColumn = "STATEMENTCODE";
            private const string SourceCodeColumn = "SOURCECODE";
            private const string SourceCode2Column = "SOURCECODE2";
            private const string SourceCode3Column = "SOURCECODE3";
            private const string ParentLineNumColumn = "PARENTLINENUM";
            private const string CommentColumn = "COMMENT";
            private const string GiftCardColumn = "GIFTCARD";
            private const string DiscountLineNumColumn = "LINENUM";
            private const string DiscountOriginTypeColumn = "DISCOUNTORIGINTYPE";
            private const string ManualDiscountTypeColumn = "MANUALDISCOUNTTYPE";
            private const string CustomerDiscountTypeColumn = "CUSTOMERDISCOUNTTYPE";
            private const string DealPriceColumn = "DEALPRICE";
            private const string DiscountAmountColumn = "DISCOUNTAMOUNT";
            private const string DiscountPercentageColumn = "PERCENTAGE";
            private const string AffiliationColumn = "AFFILIATION";
            private const string CardNumberColumn = "CARDNUMBER";
            private const string CustAccountColumn = "CUSTACCOUNT";
            private const string EntryDateColumn = "ENTRYDATE";
            private const string EntryTimeColumn = "ENTRYTIME";
            private const string EntryTypeColumn = "ENTRYTYPE";
            private const string ExpirationDateColumn = "EXPIRATIONDATE";
            private const string LoyaltyTierColumn = "LOYALTYTIER";
            private const string RewardPointColumn = "REWARDPOINT";
            private const string RewardPointAmountQtyColumn = "REWARDPOINTAMOUNTQTY";
            private const string IncomeExpenseAccountColumn = "INCOMEEXPENSEACCOUNT";
            private const string AccountTypeColumn = "ACCOUNTTYPE";
            private const string ElectronicDeliveryEmailColumn = "ELECTRONICDELIVERYEMAIL";
            private const string ElectronicDeliveryEmailContentColumn = "ELECTRONICDELIVERYEMAILCONTENT";
            private const string InvoiceIdColumn = "INVOICEID";
            private const string InvoiceAmountColumn = "AMOUNTCUR";
            private const string TaxAmountInclusiveColumn = "TAXAMOUNTINCLUSIVE";
            private const string TaxAmountExclusiveColumn = "TAXAMOUNTEXCLUSIVE";
            private const string FulfillmentStoreIdColumn = "FULFILLMENTSTOREID";
            private const string Separator = "$";
            private const int NumberOfDecimals = 2;

            private const string DataAreaIdVariableName = "@DataAreaId";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "False positive.")]
            private const decimal OrderLevelChargeLineNumber = 0;

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "False positive.")]
            private const decimal OrderLevelReasonCodeLineNumber = -1;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Num", Justification = "Reviewed")]
            private const string SaleLineNumColumn = "SALELINENUM";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(SaveSalesTransactionDataRequest),
                    typeof(SearchSalesTransactionDataRequest),
                    typeof(GetReceiptMaskDataRequest),
                    typeof(GetTenderLinesForSalesOrderDataRequest),
                    typeof(GetReceiptLayoutIdDataRequest),
                    typeof(GetReceiptInfoDataRequest),
                    typeof(GetPrintersDataRequest),
                    typeof(GetCartsDataRequest),
                };
                }
            }

            /// <summary>
            /// Gets the sales transaction to be saved.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Response response;

                if (request is SaveSalesTransactionDataRequest)
                {
                    response = SaveSalesTransaction((SaveSalesTransactionDataRequest)request);
                }
                else if (request is SearchSalesTransactionDataRequest)
                {
                    response = SearchSalesTransaction((SearchSalesTransactionDataRequest)request);
                }
                else if (request is GetReceiptMaskDataRequest)
                {
                    response = GetReceiptMask((GetReceiptMaskDataRequest)request);
                }
                else if (request is GetTenderLinesForSalesOrderDataRequest)
                {
                    response = GetTenderLinesForSalesOrder((GetTenderLinesForSalesOrderDataRequest)request);
                }
                else if (request is GetReceiptLayoutIdDataRequest)
                {
                    response = GetReceiptLayoutId((GetReceiptLayoutIdDataRequest)request);
                }
                else if (request is GetReceiptInfoDataRequest)
                {
                    response = GetReceiptInfo((GetReceiptInfoDataRequest)request);
                }
                else if (request is GetPrintersDataRequest)
                {
                    response = GetPrinters((GetPrintersDataRequest)request);
                }
                else if (request is GetCartsDataRequest)
                {
                    response = GetCart((GetCartsDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }

                return response;
            }

            internal static void FillSalesOrderMembers(IEnumerable<SalesOrder> salesOrders, bool includeDetails, RequestContext context)
            {
                ThrowIf.Null(salesOrders, "salesOrders");

                if (includeDetails)
                {
                    var noPaging = QueryResultSettings.AllRecords;
                    FillAddresses(salesOrders, noPaging, context);
                    FillSalesLines(salesOrders, noPaging, context);
                    PopulateTaxLines(salesOrders, context);
                    FillInvoiceLines(salesOrders, noPaging, context);
                    FillIncomeExpenseLines(salesOrders, noPaging, context);
                    FillTenderLines(salesOrders, noPaging, context);
                    FillOrderAttributes(salesOrders, noPaging, context);
                    FillTransactionProperties(salesOrders, noPaging, context);
                    FillOrderAffiliations(salesOrders, noPaging, context);
                    FillOrderChargeLines(salesOrders, noPaging, context);
                    FillLoyaltyRewardPointLines(salesOrders, noPaging, context);
                    FillReasonCodeLines(salesOrders, noPaging, context);
                    FillCustomerAccountDepositLines(salesOrders, noPaging, context);
                }

                foreach (SalesOrder order in salesOrders)
                {
                    string email = (string)order.GetProperty(EmailColumn) ?? string.Empty;
                    order.ContactInformationCollection.Add(new ContactInformation { ContactInformationType = ContactInformationType.Email, Value = email });

                    // Remove property to prevent it from appearing in ExtensionProperties.
                    order.GetProperties().Remove(EmailColumn);

                    string phone = (string)order.GetProperty(PhoneColumn) ?? string.Empty;
                    order.ContactInformationCollection.Add(new ContactInformation { ContactInformationType = ContactInformationType.Phone, Value = phone });

                    // Remove property to prevent it from appearing in ExtensionProperties.
                    order.GetProperties().Remove(PhoneColumn);

                    // The order level discounts and subtotal are no persisted so we calculate it here.
                    // The logic here consistent with the logic in SalesTransactionTotaler.
                    order.LineDiscount = order.ActiveSalesLines.Sum(s => s.LineDiscount);
                    order.PeriodicDiscountAmount = order.ActiveSalesLines.Sum(s => s.PeriodicDiscount);
                    order.TotalDiscount = order.ActiveSalesLines.Sum(s => s.TotalDiscount);

                    if (order.TransactionType == SalesTransactionType.IncomeExpense)
                    {
                        order.NetAmountWithNoTax = order.IncomeExpenseLines.Sum(s => s.Amount);
                    }
                    else if (order.TransactionType == SalesTransactionType.CustomerAccountDeposit)
                    {
                        order.NetAmountWithNoTax = order.CustomerAccountDepositLines.Sum(s => s.Amount);

                        // Amount paid is not persisted, so we set it here.
                        order.AmountPaid = order.CustomerAccountDepositLines.Sum(s => s.Amount);
                    }
                    else
                    {
                        order.NetAmountWithNoTax = order.ActiveSalesLines.Sum(s => s.NetAmountWithNoTax());
                        order.TaxAmountInclusive = order.ActiveSalesLines.Sum(s => s.TaxAmountInclusive);
                    }

                    order.SubtotalAmount = order.NetAmountWithNoTax + order.TaxAmountInclusive;
                }
            }

            private static EntityDataServiceResponse<SalesTransaction> GetCart(GetCartsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                // ORDER BY clause is required because there is no RECID column on SALESTRANSACTION table.
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = SalesTransactionViewName,
                    Where = "CHANNELID = @channelId",
                    OrderBy = "CREATEDDATETIME DESC"
                };

                query.Parameters["@channelId"] = request.RequestContext.GetPrincipal().ChannelId;

                if (!string.IsNullOrWhiteSpace(request.SearchCriteria.CustomerAccountNumber))
                {
                    query.Where += "  AND CUSTOMERID IN (@customerId";
                    query.Parameters["@customerId"] = request.SearchCriteria.CustomerAccountNumber;

                    if (request.SearchCriteria.IncludeAnonymous)
                    {
                        query.Where += ", ''";
                    }

                    query.Where += ")";
                }

                if (!string.IsNullOrWhiteSpace(request.SearchCriteria.CartId))
                {
                    query.Where += "  AND TRANSACTIONID = @transactionId";
                    query.Parameters["@transactionId"] = request.SearchCriteria.CartId;
                }

                if (request.SearchCriteria.SuspendedOnly)
                {
                    query.Where += " AND ISSUSPENDED = 1";
                }

                if (request.SearchCriteria.CartType != null)
                {
                    query.Where += " AND TYPE = @i_Type";
                    query.Parameters["@i_Type"] = (int)request.SearchCriteria.CartType;
                }

                PagedResult<SalesTransactionData> transactionsData;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    transactionsData = databaseContext.ReadEntity<SalesTransactionData>(query);
                }

                PagedResult<SalesTransaction> transactions = transactionsData.ConvertTo<SalesTransaction>(transaction => SalesTransactionConverter.ConvertFromData(transaction));
                
                return new EntityDataServiceResponse<SalesTransaction>(transactions);
            }

            private static EntityDataServiceResponse<Printer> GetPrinters(GetPrintersDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.TerminalId, "request.TerminalId");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                // Default query
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = ReceiptPrintersView,
                    OrderBy = "RECID"
                };

                var printerWhereClause = new List<string>();

                // Update query when receipt type is given.
                if (request.ReceiptType != null)
                {
                    printerWhereClause.Add("RECEIPTTYPE = @ReceiptType");
                    query.Parameters["@ReceiptType"] = (int)request.ReceiptType;
                }

                // Update query when layout id is given.
                if (request.LayoutId != null)
                {
                    printerWhereClause.Add("FORMLAYOUTID = @LayoutId");
                    query.Parameters["@LayoutId"] = request.LayoutId;
                }

                // Update query when hardware profile id is set.
                if (!string.IsNullOrWhiteSpace(request.HardwareProfileId))
                {
                    printerWhereClause.Add("HARDWAREPROFILEID = @HardwareProfileId");
                    query.Parameters["@HardwareProfileId"] = request.HardwareProfileId;
                }

                // Compose the where clause
                if (printerWhereClause.Count != 0)
                {
                    query.Where = string.Join(" AND ", printerWhereClause);
                }

                PagedResult<Printer> printers;

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    printers = databaseContext.ReadEntity<Printer>(query);
                }

                return new EntityDataServiceResponse<Printer>(printers);
            }

            private static SingleEntityDataServiceResponse<ReceiptInfo> GetReceiptInfo(GetReceiptInfoDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.LayoutId, "request.LayoutId");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = ReceiptInfoView,
                    Where = "FORMLAYOUTID = @LayoutId",
                };

                query.Parameters["@LayoutId"] = request.LayoutId;

                ReceiptInfo receiptInfo;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    receiptInfo = databaseContext.ReadEntity<ReceiptInfo>(query).SingleOrDefault();
                }

                if (receiptInfo != null)
                {
                    receiptInfo.IsCopy = request.CopyReceipt;
                }

                return new SingleEntityDataServiceResponse<ReceiptInfo>(receiptInfo);
            }

            private static SingleEntityDataServiceResponse<string> GetReceiptLayoutId(GetReceiptLayoutIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ReceiptProfileId, "request.ReceiptProfileId");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = ReceiptProfilesView,
                    Where = "RECEIPTTYPE = @ReceiptType AND PROFILEID = @ProfileId",
                };

                query.Parameters["@ProfileId"] = request.ReceiptProfileId;
                query.Parameters["@ReceiptType"] = (int)request.ReceiptType;

                ReceiptProfile receiptProfile;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    receiptProfile = databaseContext.ReadEntity<ReceiptProfile>(query).SingleOrDefault();
                }

                return new SingleEntityDataServiceResponse<string>(receiptProfile == null ? string.Empty : receiptProfile.ReceiptLayoutId);
            }

            private static EntityDataServiceResponse<TenderLine> GetTenderLinesForSalesOrder(GetTenderLinesForSalesOrderDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.TransactionId, "request.TransactionId");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = RetailTransactionPaymentTransView,
                    Where = "TRANSACTIONID = @TransactionId",
                    OrderBy = CreatedDateTimeColumn
                };

                query.Parameters["@TransactionId"] = request.TransactionId;

                PagedResult<TenderLine> tenderLines;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    tenderLines = databaseContext.ReadEntity<TenderLine>(query);
                }

                var tendeLinesById = tenderLines.Results.ToDictionary(t => t.LineNumber.ToString(), t => t);

                // reads reason code lines
                query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = RetailTransactionInfoCodeTransView,
                    Where = "TRANSACTIONID = @TransactionId AND TYPE = @Type"
                };

                query.Parameters["@TransactionId"] = request.TransactionId;
                query.Parameters["@Type"] = ReasonCodeLineType.Payment;

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var reasonCodeLines = databaseContext.ReadEntity<ReasonCodeLine>(query).Results;
                    foreach (ReasonCodeLine reasonCodeLine in reasonCodeLines)
                    {
                        tendeLinesById[reasonCodeLine.ParentLineId].ReasonCodeLines.Add(reasonCodeLine);
                    }
                }

                return new EntityDataServiceResponse<TenderLine>(tenderLines);
            }

            private static EntityDataServiceResponse<SalesOrder> SearchSalesTransaction(SearchSalesTransactionDataRequest request)
            {
                // retrieve transactions
                var getSalesTransactionsRequest = new GetSalesTransactionDataRequest(request.SearchCriteria, request.QueryResultSettings);
                EntityDataServiceResponse<SalesOrder> response = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<SalesOrder>>(getSalesTransactionsRequest, request.RequestContext);

                IReadOnlyCollection<SalesOrder> salesOrders = response.PagedEntityCollection.Results;
                SalesTransactionDataService.FillSalesOrderMembers(salesOrders, true, request.RequestContext);

                return new EntityDataServiceResponse<SalesOrder>(response.PagedEntityCollection);
            }
         
            /// <summary>
            ///  Updates the orders with its tender lines.
            /// </summary>
            /// <param name="orders">A collection of orders to get tender lines for.</param>
            /// <param name="settings">Query settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillTenderLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(order => order.Id, order => order);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionPaymentTransView,
                    OrderBy = LineNumColumn,
                };

                ReadOnlyCollection<TenderLine> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<TenderLine>(query).Results;
                    }
                }

                foreach (TenderLine line in results)
                {
                    var transactionId = Convert.ToString(line.GetProperty(TransactionIdColumn));
                    orderDictionary[transactionId].TenderLines.Add(line);
                }
            }

            /// <summary>
            ///  Updates the orders with income and expense lines.
            /// </summary>
            /// <param name="orders">A collection of orders to get income and expense lines for.</param>
            /// <param name="settings">Query settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillIncomeExpenseLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                Dictionary<string, SalesOrder> orderDictionary = orders.ToDictionary(order => order.Id, order => order);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionIncomeExpenseTransView,
                    OrderBy = LineNumColumn,
                };

                ReadOnlyCollection<IncomeExpenseLine> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<IncomeExpenseLine>(query).Results;
                    }
                }

                foreach (IncomeExpenseLine line in results)
                {
                    line.Amount = decimal.Negate(line.Amount);
                    var transactionId = Convert.ToString(line.GetProperty(TransactionIdColumn));
                    orderDictionary[transactionId].IncomeExpenseLines.Add(line);
                }
            }

            /// <summary>
            ///  Updates the orders with customer account deposit lines.
            /// </summary>
            /// <param name="orders">A collection of orders to get customer account deposit lines for.</param>
            /// <param name="settings">Query settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillCustomerAccountDepositLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                Dictionary<string, SalesOrder> orderDictionary = orders.ToDictionary(order => order.Id, order => order);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionCustomerAccountDepositTransView,
                    OrderBy = LineNumColumn,
                };

                ReadOnlyCollection<CustomerAccountDepositLine> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<CustomerAccountDepositLine>(query).Results;
                    }
                }

                foreach (CustomerAccountDepositLine line in results)
                {
                    var transactionId = Convert.ToString(line.GetProperty(TransactionIdColumn));
                    orderDictionary[transactionId].CustomerAccountDepositLines.Add(line);
                }
            }

            /// <summary>
            /// Updates the order with its invoice sales lines.
            /// </summary>
            /// <param name="orders">A collection of orders to get sales lines for.</param>
            /// <param name="settings">Query settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillInvoiceLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(order => order.Id, order => order);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionOrderInvoiceTransView,
                    OrderBy = LineNumColumn,
                };

                ReadOnlyCollection<SalesLine> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<SalesLine>(query).Results;
                    }
                }

                foreach (SalesLine line in results)
                {
                    var transactionId = (string)line.GetProperty(TransactionIdColumn);
                    line.ItemId = string.Empty;
                    line.NetAmount = line.Price = line.TotalAmount = line.InvoiceAmount;
                    line.IsInvoiceLine = true;
                    line.Quantity = decimal.One;
                    orderDictionary[transactionId].SalesLines.Add(line);
                }
            }

            /// <summary>
            ///  Updates the orders with its sales lines.
            /// </summary>
            /// <param name="orders">A collection of orders to get sales lines for.</param>
            /// <param name="settings">Query settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillSalesLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(x => x.Id, x => x);

                SalesLinesQueryCriteria criteria = new SalesLinesQueryCriteria();
                criteria.TransactionIds = orderDictionary.Keys;
                QueryResultSettings queryResultSettings = new QueryResultSettings(settings.ColumnSet, settings.Paging, new SortingInfo(LineNumColumn, isDescending: false));
                GetSalesLinesDataRequest dataServiceRequest = new GetSalesLinesDataRequest(criteria, queryResultSettings);

                ReadOnlyCollection<SalesLine> results = context.Runtime.Execute<EntityDataServiceResponse<SalesLine>>(dataServiceRequest, context).PagedEntityCollection.Results;

                FillAddresses(results, settings, context);

                // Collect the list of variant identifiers from the transaction.
                var itemVariantIds = new Collection<ItemVariantInventoryDimension>();
                foreach (SalesLine line in results)
                {
                    if (!string.IsNullOrEmpty(line.ItemId) && !string.IsNullOrEmpty(line.InventoryDimensionId))
                    {
                        itemVariantIds.Add(new ItemVariantInventoryDimension(line.ItemId, line.InventoryDimensionId));
                    }
                }

                // Retrieve all of the variants in a single database roundtrip and create a map for lookups.
                var variantsMap = new Dictionary<ItemVariantInventoryDimension, ProductVariant>();
                if (itemVariantIds.Any())
                {
                    var getVariantsRequest = new GetProductVariantsDataRequest(itemVariantIds);
                    ReadOnlyCollection<ProductVariant> variants = context.Runtime.Execute<EntityDataServiceResponse<ProductVariant>>(getVariantsRequest, context).PagedEntityCollection.Results;
                    variantsMap = variants.ToDictionary(key => new ItemVariantInventoryDimension(key.ItemId, key.InventoryDimensionId));
                }

                // For all sales lines that had variants, we update the variant information.
                foreach (SalesLine line in results)
                {
                    var transactionId = (string)line.GetProperty(TransactionIdColumn);
                    orderDictionary[transactionId].SalesLines.Add(line);
                    FillDiscountLines(line, settings, context);
                    if (!string.IsNullOrEmpty(line.ItemId) && !string.IsNullOrEmpty(line.InventoryDimensionId))
                    {
                        ProductVariant variant;
                        var itemVariantId = new ItemVariantInventoryDimension(line.ItemId, line.InventoryDimensionId);
                        if (variantsMap.TryGetValue(itemVariantId, out variant))
                        {
                            line.Variant = variant;
                        }
                    }

                    if (line.LineDiscount != decimal.Zero && line.Quantity != decimal.Zero && line.Price != decimal.Zero)
                    {
                        line.LinePercentageDiscount = Math.Round((line.LineDiscount / (line.Price * line.Quantity)) * 100m, 2);
                    }
                }
            }

            /// <summary>
            /// Populates Tax Lines for Sales Orders.
            /// </summary>
            /// <param name="orders">The sales orders.</param>
            /// <param name="context">The executing context.</param>
            private static void PopulateTaxLines(IEnumerable<SalesOrder> orders, RequestContext context)
            {
                if (orders == null || !orders.Any())
                {
                    return;
                }

                GetTaxLinesDataRequest request = new GetTaxLinesDataRequest(orders.Select(order => order.Id));
                ReadOnlyCollection<TaxLine> taxLines = context.Runtime.Execute<EntityDataServiceResponse<TaxLine>>(request, context).PagedEntityCollection.Results;
                var taxLinesByTransactionIdAndLineNumber = taxLines.ToLookup(x => x.TransactionId + Separator + x.SaleLineNumber.ToString(), x => x);

                foreach (SalesOrder order in orders)
                {
                    foreach (SalesLine salesLine in order.SalesLines)
                    {
                        IEnumerable<TaxLine> taxLinesBySalesLine = taxLinesByTransactionIdAndLineNumber[order.Id + Separator + salesLine.LineNumber.ToString()];
                        
                        foreach (TaxLine taxLine in taxLinesBySalesLine)
                        {
                            taxLine.TaxBasis = salesLine.NetAmount;
                        }

                        salesLine.TaxLines.AddRange(taxLinesBySalesLine);

                        CalculateTaxLinesPercentage(salesLine);
                        CalculateSalesLineTaxPercent(salesLine);
                    }
                }
            }

            /// <summary>
            /// Back calculates the percentage of each tax line in a sales line.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            private static void CalculateTaxLinesPercentage(SalesLine salesLine)
            {
                foreach (TaxLine taxLine in salesLine.TaxLines)
                {
                    if (salesLine.NetAmountWithNoTax() != 0)
                    {
                        taxLine.Percentage = Math.Round(Math.Abs(taxLine.Amount / salesLine.NetAmountWithNoTax()) * 100, NumberOfDecimals);
                    }
                }
            }

            /// <summary>
            /// Back calculates the tax percentage of this sales line.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            private static void CalculateSalesLineTaxPercent(SalesLine salesLine)
            {
                if (salesLine.NetAmountWithNoTax() != 0)
                {
                    salesLine.TaxRatePercent = Math.Round(Math.Abs(salesLine.TaxAmount / salesLine.NetAmountWithNoTax()) * 100, NumberOfDecimals);
                }
            }

            /// <summary>
            ///  Populates the ShippingAddress for a Collection of SalesOrder or SalesLine.
            /// </summary>
            /// <param name="entities">A Collection of SalesOrders or SalesLines.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillAddresses(IEnumerable<CommerceEntity> entities, QueryResultSettings settings, RequestContext context)
            {
                Dictionary<long, bool> customerAddresses = new Dictionary<long, bool>();
                List<string> orderAddresses = new List<string>();

                foreach (var entity in entities)
                {
                    long customerAddressId = (long)(entity.GetProperty(LogisticsPostalAddressColumn) ?? 0);

                    if (customerAddressId != 0)
                    {
                        if (!customerAddresses.ContainsKey(customerAddressId))
                        {
                            customerAddresses.Add(customerAddressId, true);
                        }
                    }
                    else
                    {
                        string transactionId = (string)entity.GetProperty(TransactionIdColumn);
                        orderAddresses.Add(transactionId);
                    }
                }

                IEnumerable<Address> customerAddressResult = new CustomerDataManager(context).GetAddresses(customerAddresses.Keys).Results;
                Dictionary<long, Address> customerAddressDictionary = customerAddressResult.ToDictionary(address => address.RecordId, address => address);

                IEnumerable<Address> orderAddressResult = GetOneTimeAddress(orderAddresses, settings, context);
                Dictionary<string, Address> orderAddressDictionary = orderAddressResult.ToDictionary(address => string.Format("{0}_{1:00.000}", address.GetProperty(TransactionIdColumn), address.GetProperty(SaleLineNumColumn)), address => address);

                foreach (CommerceEntity entity in entities)
                {
                    long addressId = (long)(entity.GetProperty(LogisticsPostalAddressColumn) ?? 0);

                    // Remove property to prevent it from appearing in ExtensionProperties.
                    entity.GetProperties().Remove(LogisticsPostalAddressColumn);

                    Address shippingAddress;
                    if (addressId != 0)
                    {
                        customerAddressDictionary.TryGetValue(addressId, out shippingAddress);
                    }
                    else
                    {
                        string key = string.Format("{0}_{1:00.000}", entity.GetProperty(TransactionIdColumn), (decimal)(entity.GetProperty(LineNumColumn) ?? 0.0m));
                        orderAddressDictionary.TryGetValue(key, out shippingAddress);
                    }

                    entity.SetProperty("SHIPPINGADDRESS", shippingAddress);
                }
            }

            /// <summary>
            /// Gets a collection of addresses associated with the Order header or lines.
            /// </summary>
            /// <param name="transactionIds">The collection of TransactionIds.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>A collection of one time customer addresses or empty list.</returns>
            /// <param name="context">The request context.</param>
            private static IEnumerable<Address> GetOneTimeAddress(IEnumerable<string> transactionIds, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(transactionIds, "transactionIds");

                if (!transactionIds.Any())
                {
                    return Enumerable.Empty<Address>();
                }

                var addressesQuery = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionAddressTransView
                };

                using (var transactionIdTableType = new StringIdTableType(transactionIds, TransactionIdColumn))
                {
                    addressesQuery.Parameters["@tvp_transactionIds"] = transactionIdTableType;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        return databaseContext.ReadEntity<Address>(addressesQuery).Results;
                    }
                }
            }

            /// <summary>
            /// Updates the order with its reason code lines.
            /// </summary>
            /// <param name="orders">The sales order.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillReasonCodeLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(x => x.Id, x => x);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionInfoCodeTransView,
                    OrderBy = TransactionIdColumn + ", " + ParentLineNumColumn + ", " + LineNumColumn,
                };

                ReadOnlyCollection<ReasonCodeLine> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;

                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<ReasonCodeLine>(query).Results;
                    }
                }

                foreach (ReasonCodeLine code in results)
                {
                    var transactionId = code.TransactionId;
                    SalesOrder order = orderDictionary[transactionId];

                    decimal parentLineNumber = decimal.Parse(code.ParentLineId);

                    if (parentLineNumber == OrderLevelReasonCodeLineNumber)
                    {
                        order.ReasonCodeLines.Add(code);
                    }
                    else
                    {
                        switch (code.LineType)
                        {
                            case ReasonCodeLineType.Sales:
                                SalesLine line = order.SalesLines.Where(o => o.LineNumber == parentLineNumber).SingleOrDefault();
                                if (line == null)
                                {
                                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IdMismatch, string.Format("Failed to map reason code to sales line. Line number {0} not found.", code.ParentLineId));
                                }

                                line.ReasonCodeLines.Add(code);
                                break;

                            case ReasonCodeLineType.Affiliation:
                                AffiliationLoyaltyTier affiliationLoyaltyTierLine = order.AffiliationLoyaltyTierLines.Where(a => a.AffiliationId == parentLineNumber).SingleOrDefault();
                                if (affiliationLoyaltyTierLine == null)
                                {
                                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IdMismatch, string.Format("Failed to map reason code to affiliationLoyaltyTier line. AffiliationLoyaltyTier Idd {0} not found.", code.ParentLineId));
                                }

                                affiliationLoyaltyTierLine.ReasonCodeLines.Add(code);
                                break;

                            case ReasonCodeLineType.Payment:
                                TenderLine tenderLine = order.TenderLines.Where(o => o.LineNumber == parentLineNumber).SingleOrDefault();
                                if (tenderLine == null)
                                {
                                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IdMismatch, string.Format("Failed to map reason code to tender line. Line number {0} not found.", code.ParentLineId));
                                }

                                tenderLine.ReasonCodeLines.Add(code);
                                break;
                        }
                    }
                }
            }

            /// <summary>
            /// Updates the sales lines with their discount lines.
            /// </summary>
            /// <param name="line">The sales line.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillDiscountLines(SalesLine line, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(line, "salesLine");
                ThrowIf.Null(settings, "settings");

                DiscountLinesQueryCriteria criteria = new DiscountLinesQueryCriteria();
                criteria.TransactionId = Convert.ToString(line.GetProperty(TransactionIdColumn));
                criteria.LineNumber = line.LineNumber;
                QueryResultSettings queryResultSettings = new QueryResultSettings(settings.ColumnSet, settings.Paging, new SortingInfo(LineNumColumn, false));
                GetDiscountLinesDataRequest dataServiceRequest = new GetDiscountLinesDataRequest(criteria, queryResultSettings);

                var response = context.Runtime.Execute<EntityDataServiceResponse<DiscountLine>>(dataServiceRequest, context);

                foreach (DiscountLine code in response.PagedEntityCollection.Results)
                {
                    line.DiscountLines.Add(code);
                }
            }

            /// <summary>
            ///  Updates the orders with its attributes.
            /// </summary>
            /// <param name="orders">A collection of orders..</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillOrderAttributes(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(x => x.Id, x => x);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionAttributeTransView,
                    OrderBy = NameColumn,
                };

                ReadOnlyCollection<AttributeTextValue> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;

                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<AttributeTextValue>(query).Results;
                    }
                }

                foreach (var attribute in results)
                {
                    var transactionId = (string)attribute.GetProperty(TransactionIdColumn);
                    orderDictionary[transactionId].AttributeValues.Add(attribute);
                }
            }

            /// <summary>
            ///  Updates the orders with its transaction properties.
            /// </summary>
            /// <param name="orders">A collection of orders..</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillTransactionProperties(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(x => x.Id, x => x);

                var query = new SqlPagedQuery(settings)
                {
                    From = TransactionPropertiesView,
                    OrderBy = SaleLineNumColumn,
                };

                ReadOnlyCollection<TransactionProperty> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;

                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<TransactionProperty>(query).Results;
                    }
                }

                foreach (var property in results)
                {
                    var order = orderDictionary[property.TransactionId];

                    if (property.IsHeaderProperty)
                    {
                        order.PersistentProperties[property.Name] = property.Value;
                    }
                    else
                    {
                        SalesLine line = order.SalesLines[(int)property.SalesLineNumber - 1];
                        line.PersistentProperties[property.Name] = property.Value;
                    }
                }
            }

            /// <summary>
            /// Fill affiliation value to the affiliation table.
            /// </summary>
            /// <param name="salesAffiliationLoyaltyTier">The sales affiliation.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="affiliationsTable">The affiliations data table.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="context">The request context.</param>
            private static void FillAffiliation(SalesAffiliationLoyaltyTier salesAffiliationLoyaltyTier, SalesTransaction transaction, DataTable affiliationsTable, DataTable reasonCodeTable, RequestContext context)
            {
                DataRow row = affiliationsTable.NewRow();

                SetField(row, AffiliationColumn, salesAffiliationLoyaltyTier.AffiliationId);
                SetField(row, LoyaltyTierColumn, salesAffiliationLoyaltyTier.LoyaltyTierId);
                SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                SetField(row, ReceiptIdColumn, transaction.ReceiptId ?? string.Empty);
                SetField(row, StaffColumn, StringDataHelper.TruncateString(transaction.StaffId, 25));
                SetField(row, StoreIdColumn, GetStoreId(transaction));
                SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);

                affiliationsTable.Rows.Add(row);

                FillReasonCodesForAffiliationLine(salesAffiliationLoyaltyTier, transaction, reasonCodeTable, context);
            }

            /// <summary>
            /// Saves the reason codes for affiliation line.
            /// </summary>
            /// <param name="salesAffiliationLoyaltyTier">The sales affiliation line.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="context">The request context.</param>
            private static void FillReasonCodesForAffiliationLine(
                SalesAffiliationLoyaltyTier salesAffiliationLoyaltyTier,
                SalesTransaction transaction,
                DataTable reasonCodeTable,
                RequestContext context)
            {
                ThrowIf.Null(salesAffiliationLoyaltyTier, "salesAffiliationLoyaltyTier");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(reasonCodeTable, "reasonCodeTable");

                if (salesAffiliationLoyaltyTier.ReasonCodeLines.Any())
                {
                    FillReasonCodeLines(salesAffiliationLoyaltyTier.ReasonCodeLines, transaction, reasonCodeTable, (decimal)salesAffiliationLoyaltyTier.AffiliationId, context);
                }
            }

            /// <summary>
            /// Saves the reason code line.
            /// </summary>
            /// <param name="reasonCodeLines">The reason code lines.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="parentLineNumber">The parent line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillReasonCodeLines(IEnumerable<ReasonCodeLine> reasonCodeLines, SalesTransaction transaction, DataTable reasonCodeTable, decimal parentLineNumber, RequestContext context)
            {
                ThrowIf.Null(reasonCodeLines, "reasonCodeLines");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(reasonCodeTable, "reasonCodeTable");

                foreach (var reasonCodeLine in reasonCodeLines)
                {
                    DataRow row = reasonCodeTable.NewRow();
                    SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                    SetField(row, TransDateColumn, transaction.BeginDateTime.Date);

                    // trans time is stored as seconds (integer) in the database
                    SetField(row, TransTimeColumn, (int)transaction.BeginDateTime.TimeOfDay.TotalSeconds);
                    SetField(row, LineNumColumn, GetNextLineNumber(reasonCodeTable));
                    SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                    SetField(row, TypeColumn, (int)reasonCodeLine.LineType);
                    SetField(row, ReasonCodeIdColumn, StringDataHelper.TruncateString(reasonCodeLine.ReasonCodeId, 10));
                    SetField(row, InformationColumn, StringDataHelper.TruncateString(reasonCodeLine.Information, 100));
                    SetField(row, InformationAmountColumn, reasonCodeLine.InformationAmount);
                    SetField(row, StoreColumn, GetStoreId(transaction));
                    SetField(row, TerminalColumn, transaction.TerminalId ?? string.Empty);
                    SetField(row, StaffColumn, StringDataHelper.TruncateString(transaction.StaffId, 25));
                    SetField(row, ItemTenderColumn, StringDataHelper.TruncateString(reasonCodeLine.ItemTender, 10));
                    SetField(row, AmountColumn, reasonCodeLine.Amount);
                    SetField(row, InputTypeColumn, (int)reasonCodeLine.InputType);
                    SetField(row, SubReasonCodeIdColumn, StringDataHelper.TruncateString(reasonCodeLine.SubReasonCodeId, 10));
                    SetField(row, StatementCodeColumn, StringDataHelper.TruncateString(reasonCodeLine.StatementCode, 25));
                    SetField(row, SourceCodeColumn, StringDataHelper.TruncateString(reasonCodeLine.SourceCode, 20));
                    SetField(row, SourceCode2Column, StringDataHelper.TruncateString(reasonCodeLine.SourceCode2, 20));
                    SetField(row, SourceCode3Column, StringDataHelper.TruncateString(reasonCodeLine.SourceCode3, 20));
                    SetField(row, ParentLineNumColumn, parentLineNumber);

                    reasonCodeTable.Rows.Add(row);
                }
            }

            /// <summary>
            /// Gets the next line number.
            /// </summary>
            /// <param name="dataTable">The data table.</param>
            /// <returns>The next line number.</returns>
            private static decimal GetNextLineNumber(DataTable dataTable)
            {
                decimal lineNumber = 1m;
                if (dataTable.Rows.Count != 0)
                {
                    lineNumber = (decimal)dataTable.Rows[dataTable.Rows.Count - 1][LineNumColumn] + 1;
                }

                return lineNumber;
            }

            /// <summary>
            /// Gets the store associated with the sales transaction.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <returns>The store identifier.</returns>
            private static string GetStoreId(SalesTransaction transaction)
            {
                ThrowIf.Null(transaction, "transaction");

                return transaction.StoreId ?? string.Empty;
            }

            /// <summary>
            /// Add a field to a data table.
            /// </summary>
            /// <param name="row">The data row.</param>
            /// <param name="field">The field name.</param>
            /// <param name="value">The field value.</param>
            private static void SetField(DataRow row, string field, object value)
            {
                ThrowIf.Null(row, "row");
                ThrowIf.Null(field, "field");

                row[field] = value;
            }

            /// <summary>
            /// Updates the orders with its affiliations.
            /// </summary>
            /// <param name="orders">A collection of orders.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillOrderAffiliations(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(x => x.Id, x => x);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionAffiliationTransView,
                    Where = string.Format("{0} = {1}", DataAreaIdColumn, DataAreaIdVariableName)
                };

                query.Parameters[DataAreaIdVariableName] = context.GetChannelConfiguration().InventLocationDataAreaId;

                ReadOnlyCollection<SalesAffiliationLoyaltyTier> results;

                using (StringIdTableType transactionIdTableType = new StringIdTableType(orderDictionary.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<SalesAffiliationLoyaltyTier>(query).Results;
                    }
                }

                foreach (var affiliation in results)
                {
                    var transactionId = (string)affiliation.GetProperty(TransactionIdColumn);
                    orderDictionary[transactionId].AffiliationLoyaltyTierLines.Add(affiliation);
                }
            }

            /// <summary>
            /// Updates the order with its charge lines.
            /// </summary>
            /// <param name="orders">The sales order.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillOrderChargeLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var ordersById = orders.ToDictionary(x => x.Id, x => x);

                var query = new SqlPagedQuery(settings)
                {
                    From = RetailTransactionMarkupTransView,
                    OrderBy = TransactionIdColumn + ", " + SaleLineNumColumn + ", " + MarkUpLineNumColumn,
                };

                ReadOnlyCollection<ChargeLine> results;
                using (StringIdTableType transactionIdTableType = new StringIdTableType(ordersById.Keys, TransactionIdColumn))
                {
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<ChargeLine>(query).Results;
                    }
                }

                foreach (ChargeLine chargeLine in results)
                {
                    var transactionId = chargeLine.TransactionId;
                    SalesOrder order = ordersById[transactionId];

                    decimal saleLineNumber = chargeLine.SaleLineNumber;

                    // If the sale line number not equal OrderLevelChargeLineNumber means it is a sale line's charge, otherwise it is a transaction header's line charge.
                    if (saleLineNumber != OrderLevelChargeLineNumber)
                    {
                        SalesLine line = order.SalesLines.FirstOrDefault(o => o.LineNumber == saleLineNumber);
                        if (line == null)
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IdMismatch, string.Format("Failed to map charge to sales line. Line number {0} not found.", chargeLine.SaleLineNumber));
                        }

                        line.ChargeLines.Add(chargeLine);
                    }
                }
            }

            /// <summary>
            /// Updates the orders with its reward point lines.
            /// </summary>
            /// <param name="orders">A collection of orders.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="context">The request context.</param>
            private static void FillLoyaltyRewardPointLines(IEnumerable<SalesOrder> orders, QueryResultSettings settings, RequestContext context)
            {
                ThrowIf.Null(orders, "orders");
                ThrowIf.Null(settings, "settings");

                if (!orders.Any())
                {
                    return;
                }

                var orderDictionary = orders.ToDictionary(x => x.Id, x => x);

                LoyaltyRewardPointLinesQueryCriteria criteria = new LoyaltyRewardPointLinesQueryCriteria();
                criteria.TransactionIds = orderDictionary.Keys;
                QueryResultSettings queryResultSettings = new QueryResultSettings(settings.ColumnSet, settings.Paging);
                GetLoyaltyRewardPointLinesDataRequest dataServiceRequest = new GetLoyaltyRewardPointLinesDataRequest(criteria, queryResultSettings);

                var response = context.Runtime.Execute<EntityDataServiceResponse<LoyaltyRewardPointLine>>(dataServiceRequest, context);

                foreach (var line in response.PagedEntityCollection.Results)
                {
                    var transactionId = (string)line.GetProperty(TransactionIdColumn);
                    orderDictionary[transactionId].LoyaltyRewardPointLines.Add(line);
                }
            }

            private static NullResponse SaveSalesTransaction(SaveSalesTransactionDataRequest request)
            {
                SetIsCreatedOfflineOnTransaction(request.SalesTransaction, request.RequestContext);

                using (DataTable transactionTable = new DataTable("RETAILTRANSACTIONTABLETYPE"))
                using (DataTable paymentTable = new DataTable("RETAILTRANSACTIONPAYMENTTRANSTABLETYPE"))
                using (DataTable linesTable = new DataTable("RETAILTRANSACTIONSALESTRANSTABLETYPE"))
                using (DataTable incomeExpenseTable = new DataTable("RETAILINCOMEEXPENSETABLETYPE"))
                using (DataTable markupTable = new DataTable("RETAILTRANSACTIONMARKUPTRANSTABLETYPE"))
                using (DataTable taxTable = new DataTable("RETAILTRANSACTIONTAXTRANSTABLETYPE"))
                using (DataTable attributeTable = new DataTable("RETAILTRANSACTIONATTRIBUTETRANSTABLETYPE"))
                using (DataTable addressTable = new DataTable("RETAILTRANSACTIONADDRESSTRANSTABLETYPE"))
                using (DataTable discountTable = new DataTable("RETAILTRANSACTIONDISCOUNTTRANSTABLETYPE"))
                using (DataTable reasonCodeTable = new DataTable("RETAILTRANSACTIONINFOCODETRANSTABLETYPE"))
                using (DataTable propertiesTable = new DataTable("RETAILTRANSACTIONPROPERTIESTABLETYPE"))
                using (DataTable affiliationsTable = new DataTable("RETAILTRANSACTIONAFFILIATIONTRANSTABLETYPE"))
                using (DataTable rewardPointTable = new DataTable("RETAILTRANSACTIONLOYALTYREWARDPOINTTRANSTABLETYPE"))
                using (DataTable customerOrderTable = new DataTable("CUSTOMERORDERTRANSACTIONTABLETYPE"))
                using (DataTable invoiceTable = new DataTable("RETAILTRANSACTIONORDERINVOICETRANSTABLETYPE"))
                using (DataTable customerAccountDepositTable = new DataTable("RETAILTRANSACTIONCUSTOMERACCOUNTDEPOSITTRANSTABLETYPE"))
                {
                    RetailTransactionTableSchema.PopulateSchema(transactionTable);
                    RetailTransactionPaymentSchema.PopulatePaymentSchema(paymentTable);
                    PopulateSaleLineSchema(linesTable);
                    PopulateIncomeExpenseLineSchema(incomeExpenseTable);
                    PopulateMarkupSchema(markupTable);
                    PopulateTaxSchema(taxTable);
                    PopulateAttributeSchema(attributeTable);
                    PopulateAddressSchema(addressTable);
                    PopulateDiscountSchema(discountTable);
                    PopulateInvoiceSchema(invoiceTable);
                    PopulateCustomerAccountDepositLineSchema(customerAccountDepositTable);

                    PopulateReasonCodeSchema(reasonCodeTable);
                    PopulatePropertiesSchema(propertiesTable);
                    PopulateRewardPointSchema(rewardPointTable);
                    PopulateAffiliationsSchema(affiliationsTable);

                    CustomerOrderTransactionTableSchema.PopulateSchema(customerOrderTable);

                    FillOrderHeader(
                        request.SalesTransaction,
                        transactionTable,
                        markupTable,
                        attributeTable,
                        addressTable,
                        reasonCodeTable,
                        propertiesTable,
                        rewardPointTable,
                        request.RequestContext);

                    FillOrderLines(
                        request.SalesTransaction,
                        linesTable,
                        incomeExpenseTable,
                        markupTable,
                        taxTable,
                        addressTable,
                        discountTable,
                        reasonCodeTable,
                        propertiesTable,
                        affiliationsTable,
                        invoiceTable,
                        customerAccountDepositTable,
                        request.RequestContext);

                    // Online order not originated from store front may not have card payment information
                    if (request.SalesTransaction.TenderLines != null)
                    {
                        FillOrderPayments(request.SalesTransaction, request.SalesTransaction.TenderLines, paymentTable, reasonCodeTable, request.RequestContext);
                    }

                    if (request.SalesTransaction.CartType == CartType.CustomerOrder)
                    {
                        FillCustomerOrder(request.SalesTransaction, customerOrderTable, request.RequestContext);
                    }

                    var insertTablesRequest = new InsertSalesTransactionTablesDataRequest(
                        transactionTable,
                        markupTable,
                        taxTable,
                        paymentTable,
                        linesTable,
                        incomeExpenseTable,
                        attributeTable,
                        addressTable,
                        discountTable,
                        reasonCodeTable,
                        propertiesTable,
                        rewardPointTable,
                        affiliationsTable,
                        customerOrderTable,
                        invoiceTable,
                        customerAccountDepositTable);

                    request.RequestContext.Runtime.Execute<NullResponse>(insertTablesRequest, request.RequestContext);
                }

                return new NullResponse();
            }

            /// <summary>
            /// Sets the <see cref="SalesTransaction.IsCreatedOffline"/> flag according to the database that the transaction will be saved to.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="requestContext">The request context.</param>
            private static void SetIsCreatedOfflineOnTransaction(SalesTransaction salesTransaction, RequestContext requestContext)
            {
                // if connection string associated to context maps to a database that does not hold channel master data
                // then the transaction has been created "offline"
                salesTransaction.IsCreatedOffline = !requestContext.Runtime.Configuration.IsMasterDatabaseConnectionString;
            }

            /// <summary>
            /// Save the customer order specific fields.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="customerOrderTable">The customer order table.</param>
            /// <param name="context">The request context.</param>
            private static void FillCustomerOrder(SalesTransaction transaction, DataTable customerOrderTable, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(customerOrderTable, "customerOrderTable");

                DataRow row = customerOrderTable.NewRow();

                SetField(row, CustomerOrderTransactionTableSchema.TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                SetField(row, CustomerOrderTransactionTableSchema.DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                SetField(row, CustomerOrderTransactionTableSchema.StoreColumn, GetStoreId(transaction));
                SetField(row, CustomerOrderTransactionTableSchema.TerminalColumn, transaction.TerminalId ?? string.Empty);

                SetField(row, CustomerOrderTransactionTableSchema.CancellationChargeColumn, transaction.CancellationCharge);
                SetField(row, CustomerOrderTransactionTableSchema.DepositOverrideColumn, transaction.IsDepositOverridden ? (decimal?)transaction.OverriddenDepositAmount : null);
                SetField(row, CustomerOrderTransactionTableSchema.PrepaymentInvoicedColumn, transaction.PrepaymentAmountInvoiced);
                SetField(row, CustomerOrderTransactionTableSchema.CalculatedDepositAmountColumn, transaction.CalculatedDepositAmount);
                SetField(row, CustomerOrderTransactionTableSchema.PrepaymentPaidColumn, transaction.PrepaymentAmountPaid);
                SetField(row, CustomerOrderTransactionTableSchema.RequiredDepositColumn, transaction.RequiredDepositAmount);
                SetField(row, CustomerOrderTransactionTableSchema.QuoteExpirationDateColumn, DateTimeOffsetDataHelper.GetDbDateTimeOrDefault(transaction.QuotationExpiryDate));
                SetField(row, CustomerOrderTransactionTableSchema.CustomerOrderTypeColumn, (int)transaction.CustomerOrderType);
                SetField(row, CustomerOrderTransactionTableSchema.CustomerOrderModeColumn, (int)transaction.CustomerOrderMode);

                customerOrderTable.Rows.Add(row);
            }

            /// <summary>
            /// Save payments.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="tenderLines">The tender lines containing payment information.</param>
            /// <param name="paymentTable">The payment table.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="context">The request context.</param>
            private static void FillOrderPayments(
                SalesTransaction transaction,
                IEnumerable<TenderLine> tenderLines,
                DataTable paymentTable,
                DataTable reasonCodeTable,
                RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(tenderLines, "tenderLines");
                ThrowIf.Null(paymentTable, "paymentTable");
                int lineNum = 1;

                DateTimeOffset channelNow = context.GetNowInChannelTimeZone();

                foreach (TenderLine tenderLine in tenderLines)
                {
                    // Ignore historical tender lines
                    if (tenderLine.Status != TenderLineStatus.Historical)
                    {
                        tenderLine.LineNumber = lineNum++;
                        DataRow row = paymentTable.NewRow();

                        SetField(row, RetailTransactionPaymentSchema.ForeignCurrencyAmountColumn, tenderLine.AmountInTenderedCurrency);
                        SetField(row, RetailTransactionPaymentSchema.ForeignCurrencyTableColumn, tenderLine.Currency ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.ForeignCurrencyExchangeRateTableColumn, tenderLine.ExchangeRate);
                        SetField(row, RetailTransactionPaymentSchema.AmountTenderedColumn, tenderLine.Amount);
                        SetField(row, RetailTransactionPaymentSchema.CompanyCurrencyExchangeRateColumn, tenderLine.CompanyCurrencyExchangeRate);
                        SetField(row, RetailTransactionPaymentSchema.CompanyCurrencyAmountColumn, tenderLine.AmountInCompanyCurrency);
                        SetField(row, RetailTransactionPaymentSchema.DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                        SetField(row, RetailTransactionPaymentSchema.LineNumColumn, tenderLine.LineNumber);
                        SetField(row, RetailTransactionPaymentSchema.PaymentCardTokenColumn, tenderLine.CardToken ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.PaymentAuthorizationColumn, tenderLine.Authorization ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.IsPaymentCapturedColumn, tenderLine.IsPaymentCaptured);
                        SetField(row, RetailTransactionPaymentSchema.StoreColumn, GetStoreId(transaction));
                        SetField(row, RetailTransactionPaymentSchema.TenderTypeTableColumn, tenderLine.TenderTypeId ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.IsChangeLineColumn, tenderLine.IsChangeLine);
                        SetField(row, RetailTransactionPaymentSchema.TerminalColumn, transaction.TerminalId ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.StaffIdColumn, StringDataHelper.TruncateString(transaction.StaffId, 25));
                        SetField(row, RetailTransactionPaymentSchema.TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                        SetField(row, RetailTransactionPaymentSchema.SigCapDataColumn, tenderLine.SignatureData);
                        SetField(row, RetailTransactionPaymentSchema.TransactionStatusColumn, tenderLine.TransactionStatus);
                        SetField(row, RetailTransactionPaymentSchema.ReceiptIdColumn, StringDataHelper.TruncateString(transaction.ReceiptId, 18));
                        SetField(row, RetailTransactionPaymentSchema.GiftCardIdColumn, tenderLine.GiftCardId ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.IsPrepaymentColumn, tenderLine.IsDeposit);
                        SetField(row, RetailTransactionPaymentSchema.LoyaltyCardIdColumn, tenderLine.LoyaltyCardId ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.CardOrAccountColumn, tenderLine.CardOrAccount ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.CardTypeIdColumn, tenderLine.CardTypeId ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.CreditMemoIdColumn, tenderLine.CreditMemoId ?? string.Empty);
                        SetField(row, RetailTransactionPaymentSchema.TransDateColumn, tenderLine.TenderDate.GetValueOrDefault(channelNow).Date);
                        SetField(row, RetailTransactionPaymentSchema.TransTimeColumn, (int)tenderLine.TenderDate.GetValueOrDefault(channelNow).TimeOfDay.TotalSeconds);

                        paymentTable.Rows.Add(row);

                        // Save the reason code lines for the tender line
                        SaveReasonCodesForTenderLine(tenderLine, transaction, reasonCodeTable, context);
                    }
                }
            }

            /// <summary>
            /// Saves the reason codes for tender lines.
            /// </summary>
            /// <param name="tenderLine">The tender line.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="context">The request context.</param>
            private static void SaveReasonCodesForTenderLine(
                TenderLine tenderLine,
                SalesTransaction transaction,
                DataTable reasonCodeTable,
                RequestContext context)
            {
                ThrowIf.Null(tenderLine, "tenderLine");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(reasonCodeTable, "reasonCodeTable");

                if (tenderLine.ReasonCodeLines != null &&
                    tenderLine.ReasonCodeLines.Any())
                {
                    FillReasonCodeLines(tenderLine.ReasonCodeLines, transaction, reasonCodeTable, tenderLine.LineNumber, context);
                }
            }

            /// <summary>
            /// Save item lines.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="lineTable">The line data table.</param>
            /// <param name="incomeExpenseTable">The income/ expense line data table.</param>
            /// <param name="markupTable">The markup data table.</param>
            /// <param name="taxTable">The tax data table.</param>
            /// <param name="addressTable">The address data table.</param>
            /// <param name="discountTable">The discount data table.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="propertiesTable">The properties table.</param>
            /// <param name="affiliationsTable">The affiliations table.</param>
            /// <param name="invoiceLinesTable">The invoice table.</param>
            /// <param name="customerAccountDepositTable">The customer account deposit line data table.</param>
            /// <param name="context">The request context.</param>
            private static void FillOrderLines(
                SalesTransaction salesTransaction,
                DataTable lineTable,
                DataTable incomeExpenseTable,
                DataTable markupTable,
                DataTable taxTable,
                DataTable addressTable,
                DataTable discountTable,
                DataTable reasonCodeTable,
                DataTable propertiesTable,
                DataTable affiliationsTable,
                DataTable invoiceLinesTable,
                DataTable customerAccountDepositTable,
                RequestContext context)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(lineTable, "lineTable");

                // Go through all the saleItems in the transaction and save them in the database.
                decimal lineNum = 1;
                foreach (SalesLine line in salesTransaction.SalesLines)
                {
                    line.LineNumber = lineNum++;
                    FillItemLine(line, salesTransaction, lineTable, markupTable, taxTable, addressTable, discountTable, reasonCodeTable, propertiesTable, invoiceLinesTable, context);
                }

                foreach (IncomeExpenseLine incomeExpenseLine in salesTransaction.IncomeExpenseLines)
                {
                    FillIncomeExpenseLine(salesTransaction, incomeExpenseLine, incomeExpenseTable, context);
                }

                foreach (SalesAffiliationLoyaltyTier salesAffiliationLoyaltyTier in salesTransaction.AffiliationLoyaltyTierLines)
                {
                    FillAffiliation(salesAffiliationLoyaltyTier, salesTransaction, affiliationsTable, reasonCodeTable, context);
                }

                foreach (CustomerAccountDepositLine customerAccountDepositLine in salesTransaction.CustomerAccountDepositLines)
                {
                    FillCustomerAccountDepositLine(salesTransaction, customerAccountDepositLine, customerAccountDepositTable, context);
                }
            }

            /// <summary>
            /// Save individual income / expense lines.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="incomeExpenseLine">The income / expense lines.</param>
            /// <param name="incomeExpenseTable">The income / expense table.</param>
            /// <param name="context">The request context.</param>
            private static void FillIncomeExpenseLine(
                SalesTransaction salesTransaction,
                IncomeExpenseLine incomeExpenseLine,
                DataTable incomeExpenseTable,
                RequestContext context)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(incomeExpenseLine, "incomeExpenseLine");
                ThrowIf.Null(incomeExpenseTable, "incomeExpenseTable");

                DataRow row = incomeExpenseTable.NewRow();

                SetField(row, TransactionIdColumn, salesTransaction.Id);
                SetField(row, ReceiptIdColumn, StringDataHelper.TruncateString(salesTransaction.ReceiptId, 18));
                SetField(row, IncomeExpenseAccountColumn, incomeExpenseLine.IncomeExpenseAccount);
                SetField(row, StoreColumn, GetStoreId(salesTransaction));
                SetField(row, TerminalColumn, salesTransaction.TerminalId ?? string.Empty);
                SetField(row, StaffColumn, StringDataHelper.TruncateString(salesTransaction.StaffId, 25));
                SetField(row, TransactionStatusColumn, incomeExpenseLine.TransactionStatus);
                SetField(row, AmountColumn, decimal.Negate(incomeExpenseLine.Amount));
                SetField(row, AccountTypeColumn, incomeExpenseLine.AccountType);

                SetField(row, TransDateColumn, salesTransaction.BeginDateTime.Date);

                // trans time is stored as seconds (integer) in the database
                SetField(row, TransTimeColumn, (int)salesTransaction.BeginDateTime.TimeOfDay.TotalSeconds);
                SetField(row, ChannelColumn, context.GetPrincipal().ChannelId);
                SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);

                incomeExpenseTable.Rows.Add(row);
            }

            /// <summary>
            /// Saves the customer account deposit line.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="customerAccountDepositLine">The customer account deposit line.</param>
            /// <param name="customerAccountDepositTable">The customer account deposit data table.</param>
            /// <param name="context">The request context.</param>
            private static void FillCustomerAccountDepositLine(
                SalesTransaction salesTransaction,
                CustomerAccountDepositLine customerAccountDepositLine,
                DataTable customerAccountDepositTable,
                RequestContext context)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(customerAccountDepositLine, "customerAccountDepositLine");
                ThrowIf.Null(customerAccountDepositTable, "customerAccountDepositTable");

                DataRow row = customerAccountDepositTable.NewRow();

                SetField(row, TransactionIdColumn, salesTransaction.Id);
                SetField(row, ReceiptIdColumn, StringDataHelper.TruncateString(salesTransaction.ReceiptId, 18));
                SetField(row, CustAccountColumn, customerAccountDepositLine.CustomerAccount);
                SetField(row, StoreColumn, GetStoreId(salesTransaction));
                SetField(row, TerminalColumn, salesTransaction.TerminalId ?? string.Empty);
                SetField(row, StaffColumn, StringDataHelper.TruncateString(salesTransaction.StaffId, 25));
                SetField(row, TransactionStatusColumn, customerAccountDepositLine.TransactionStatus);
                SetField(row, AmountColumn, customerAccountDepositLine.Amount);
                SetField(row, CommentColumn, StringDataHelper.TruncateString(customerAccountDepositLine.Comment, 60));

                SetField(row, TransDateColumn, salesTransaction.BeginDateTime.Date);

                // trans time is stored as seconds (integer) in the database
                SetField(row, TransTimeColumn, (int)salesTransaction.BeginDateTime.TimeOfDay.TotalSeconds);
                SetField(row, ChannelColumn, context.GetPrincipal().ChannelId);
                SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);

                customerAccountDepositTable.Rows.Add(row);
            }

            /// <summary>
            /// Save individual item line.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="lineTable">The line table.</param>
            /// <param name="markupTable">The markup table.</param>
            /// <param name="taxTable">The tax table.</param>
            /// <param name="addressTable">The address table.</param>
            /// <param name="discountTable">The discount table.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="propertiesTable">The properties table.</param>
            /// <param name="invoiceTable">The invoice table.</param>
            /// <param name="context">The request context.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "To be refactored.")]
            private static void FillItemLine(
                SalesLine salesLine,
                SalesTransaction transaction,
                DataTable lineTable,
                DataTable markupTable,
                DataTable taxTable,
                DataTable addressTable,
                DataTable discountTable,
                DataTable reasonCodeTable,
                DataTable propertiesTable,
                DataTable invoiceTable,
                RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(salesLine, "saleItem");
                ThrowIf.Null(lineTable, "lineTable");

                if (salesLine.IsInvoiceLine)
                {
                    // If the line is an Invoice line, then we save a reduced set of details into the InvoiceTable instead of the regular SalesLine table(s).
                    DataRow row = invoiceTable.NewRow();
                    SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                    SetField(row, StoreIdColumn, (salesLine.FulfillmentStoreId ?? transaction.StoreId) ?? string.Empty);
                    SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                    SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                    SetField(row, LineNumColumn, salesLine.LineNumber);
                    SetField(row, TransactionStatusColumn, (int)salesLine.Status);
                    SetField(row, InvoiceIdColumn, salesLine.InvoiceId);
                    SetField(row, InvoiceAmountColumn, salesLine.InvoiceAmount);
                    invoiceTable.Rows.Add(row);
                }
                else
                {
                    // Save the line into the normal set of Sales Line tables.
                    DataRow row = lineTable.NewRow();

                    SetField(row, CustAccountColumn, transaction.CustomerId ?? string.Empty);
                    SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                    SetField(row, LineManualDiscountAmountColumn, salesLine.LineManualDiscountAmount);
                    SetField(row, LineManualDiscountPercentageColumn, salesLine.LineManualDiscountPercentage);
                    SetField(row, DiscAmountColumn, salesLine.LineDiscount + salesLine.TotalDiscount + salesLine.PeriodicDiscount);
                    SetField(row, TotalDiscAmountColumn, salesLine.TotalDiscount);
                    SetField(row, TotalDiscPctColumn, salesLine.TotalPercentageDiscount);
                    SetField(row, LineDscAmountColumn, salesLine.LineDiscount);
                    SetField(row, PeriodicDiscAmountColumn, salesLine.PeriodicDiscount);
                    SetField(row, PeriodicDiscPctColumn, salesLine.PeriodicPercentageDiscount);
                    SetField(row, DlvModeColumn, salesLine.DeliveryMode ?? transaction.DeliveryMode ?? string.Empty);
                    SetField(row, InventLocationIdColumn, StringDataHelper.TruncateString(string.IsNullOrEmpty(salesLine.InventoryLocationId) ? (transaction.InventoryLocationId ?? string.Empty) : salesLine.InventoryLocationId, 10));
                    SetField(row, InventSerialIdColumn, salesLine.SerialNumber ?? string.Empty);
                    SetField(row, InventSiteIdColumn, string.Empty);
                    SetField(row, ItemIdColumn, salesLine.ItemId ?? string.Empty);
                    SetField(row, BarcodeColumn, salesLine.Barcode ?? string.Empty);
                    SetField(row, LineNumColumn, salesLine.LineNumber);
                    SetField(row, ListingIdColumn, salesLine.ProductId.ToString());

                    var salesLineTransactionDate = salesLine.SalesDate ?? transaction.BeginDateTime;

                    SetField(row, TransDateColumn, salesLineTransactionDate.Date);
                    SetField(row, TransTimeColumn, (int)salesLineTransactionDate.TimeOfDay.TotalSeconds);

                    // If the address is not set at the line, defaults it to the order.
                    Address shippingAddress = salesLine.ShippingAddress ?? transaction.ShippingAddress;
                    long addressId = GetAddressRecordId(shippingAddress);
                    SetField(row, LogisticsPostalAddressColumn, addressId);
                    SetField(row, NetAmountColumn, salesLine.NetAmountWithoutTax * -1);
                    SetField(row, NetAmountWithAllInclusiveTaxColumn, salesLine.NetAmountWithAllInclusiveTax * -1);
                    SetField(row, TaxAmountColumn, salesLine.TaxAmount * -1);
                    SetField(row, PriceColumn, salesLine.Price); // Price with all inclusive (even non-exempt) tax
                    SetField(row, PriceChangeColumn, salesLine.IsPriceOverridden);
                    SetField(row, OriginalPriceColumn, salesLine.OriginalPrice.HasValue ? salesLine.OriginalPrice.Value : salesLine.Price);
                    SetField(row, QtyColumn, salesLine.Quantity * -1);

                    // If the delivery mode is set at the line, use the dates set at the line
                    bool useDatesFromLine = !string.IsNullOrWhiteSpace(salesLine.DeliveryMode);
                    DateTime? deliveryDate = DateTimeOffsetDataHelper.GetDbDateTimeOrDefault(useDatesFromLine ? salesLine.RequestedDeliveryDate : transaction.RequestedDeliveryDate);

                    SetField(row, ReceiptDateRequestedColumn, deliveryDate);
                    SetField(row, ShippingDateRequestedColumn, deliveryDate);
                    SetField(row, StoreColumn, GetStoreId(transaction));
                    SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                    SetField(row, UnitColumn, salesLine.SalesOrderUnitOfMeasure ?? string.Empty);
                    SetField(row, TaxGroupColumn, salesLine.SalesTaxGroupId ?? string.Empty);
                    SetField(row, OriginalSalesTaxGroupColumn, salesLine.OriginalSalesTaxGroupId ?? string.Empty);
                    SetField(row, TaxItemGroupColumn, salesLine.ItemTaxGroupId ?? string.Empty);
                    SetField(row, OriginalTaxItemGroupColumn, salesLine.OriginalItemTaxGroupId ?? string.Empty);
                    SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                    SetField(row, StaffIdColumn, StringDataHelper.TruncateString(transaction.StaffId, 25));
                    SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));

                    string variantId = null;
                    if (salesLine.Variant != null)
                    {
                        variantId = salesLine.Variant.VariantId;
                    }

                    SetField(row, VariantIdColumn, variantId ?? string.Empty);
                    SetField(row, ReturnNoSaleColumn, salesLine.IsReturnByReceipt);
                    SetField(row, ReturnTransactionIdColumn, salesLine.ReturnTransactionId ?? string.Empty);
                    SetField(row, ReturnLineNumberColumn, salesLine.ReturnLineNumber);
                    SetField(row, ReturnStoreColumn, salesLine.ReturnStore ?? string.Empty);
                    SetField(row, ReturnTerminalIdColumn, salesLine.ReturnTerminalId ?? string.Empty);
                    SetField(row, ReceiptIdColumn, transaction.ReceiptId ?? string.Empty);
                    SetField(row, TransactionStatusColumn, (int)salesLine.Status);
                    SetField(row, CommentColumn, StringDataHelper.TruncateString(salesLine.Comment, 60));
                    SetField(row, GiftCardColumn, salesLine.IsGiftCardLine ? 1 : 0);
                    SetField(row, CatalogIdColumn, salesLine.CatalogId);
                    SetField(row, ElectronicDeliveryEmailColumn, salesLine.ElectronicDeliveryEmailAddress);
                    SetField(row, ElectronicDeliveryEmailContentColumn, salesLine.ElectronicDeliveryEmailContent);
                    SetField(row, FulfillmentStoreIdColumn, salesLine.FulfillmentStoreId ?? string.Empty);

                    lineTable.Rows.Add(row);

                    // Save charges, addresses, and discounts for the sales line.
                    FillChargesForItemLine(salesLine.ChargeLines, transaction, salesLine, markupTable, salesLine.LineNumber, context);

                    if (addressId == 0)
                    {
                        // Save the one-time address
                        FillAddress(shippingAddress, transaction, addressTable, salesLine.LineNumber, salesLine.FulfillmentStoreId, context);
                    }

                    FillItemDiscountLines(transaction, salesLine, discountTable, salesLine.LineNumber, context);

                    // Save reason codes for the sales line.
                    FillReasonCodesForSalesLine(salesLine, transaction, reasonCodeTable, context);
                }

                // Save taxes and properties for both sales & invoice lines
                FillItemTaxLines(transaction, salesLine, taxTable, salesLine.LineNumber, context);
                SavePropertySet(transaction, salesLine.PersistentProperties, propertiesTable, context, salesLine.LineNumber);
            }

            /// <summary>
            /// Save all tax lines for an individual sale line item.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="saleItem">The sale item.</param>
            /// <param name="taxTable">The tax table.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillItemTaxLines(SalesTransaction transaction, SalesLine saleItem, DataTable taxTable, decimal lineNumber, RequestContext context)
            {
                ThrowIf.Null<SalesLine>(saleItem, "saleItem");
                ThrowIf.Null(taxTable, "taxTable");

                foreach (TaxLine taxItem in saleItem.TaxLines)
                {
                    FillTaxLine(transaction, saleItem, taxItem, taxTable, lineNumber, context);
                }
            }

            /// <summary>
            /// Save an individual item tax line.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="saleItem">The sale item.</param>
            /// <param name="taxItem">The tax item.</param>
            /// <param name="taxTable">The tax table.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillTaxLine(SalesTransaction transaction, SalesLine saleItem, TaxLine taxItem, DataTable taxTable, decimal lineNumber, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null<SalesLine>(saleItem, "saleItem");
                ThrowIf.Null<TaxLine>(taxItem, "taxItem");
                ThrowIf.Null(taxTable, "taxTable");

                DataRow row = taxTable.NewRow();
                SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                SetField(row, StoreIdColumn, GetStoreId(transaction));
                SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                SetField(row, SaleLineNumColumn, lineNumber);
                SetField(row, TaxCodeColumn, taxItem.TaxCode ?? string.Empty);
                SetField(row, AmountColumn, taxItem.Amount);
                SetField(row, IsIncludedInPriceColumn, transaction.IsTaxIncludedInPrice);
                taxTable.Rows.Add(row);
            }

            /// <summary>
            /// Saves the reason codes for sales line.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="context">The request context.</param>
            private static void FillReasonCodesForSalesLine(
                SalesLine salesLine,
                SalesTransaction transaction,
                DataTable reasonCodeTable,
                RequestContext context)
            {
                ThrowIf.Null(salesLine, "salesLine");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(reasonCodeTable, "reasonCodeTable");

                if (salesLine.ReasonCodeLines != null &&
                    salesLine.ReasonCodeLines.Any())
                {
                    FillReasonCodeLines(salesLine.ReasonCodeLines, transaction, reasonCodeTable, salesLine.LineNumber, context);
                }
            }

            /// <summary>
            /// Save discount lines for an individual sale line item.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="saleItem">The sale line item.</param>
            /// <param name="discountTable">The discount table.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillItemDiscountLines(SalesTransaction transaction, SalesLine saleItem, DataTable discountTable, decimal lineNumber, RequestContext context)
            {
                ThrowIf.Null<SalesLine>(saleItem, "saleItem");
                ThrowIf.Null(discountTable, "discountTable");

                decimal discountLineNumber = 1;
                foreach (DiscountLine line in saleItem.DiscountLines)
                {
                    FillDiscountLine(transaction, saleItem, line, discountTable, lineNumber, discountLineNumber, context);
                    discountLineNumber++;
                }
            }

            /// <summary>
            /// Save an individual discount line.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="saleItem">The sale line item.</param>
            /// <param name="discount">The discount line.</param>
            /// <param name="discountTable">The discount table.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="discountLineNumber">The discount line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillDiscountLine(SalesTransaction transaction, SalesLine saleItem, DiscountLine discount, DataTable discountTable, decimal lineNumber, decimal discountLineNumber, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(saleItem, "saleItem");
                ThrowIf.Null(discount, "discount");
                ThrowIf.Null(discountTable, "discountTable");

                DataRow row = discountTable.NewRow();
                SetField(row, AmountColumn, discount.EffectiveAmount);
                SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                SetField(row, DealPriceColumn, discount.DealPrice);
                SetField(row, DiscountAmountColumn, discount.Amount);
                SetField(row, DiscountCodeColumn, discount.DiscountCode ?? string.Empty);
                SetField(row, SaleLineNumColumn, lineNumber);
                SetField(row, DiscountPercentageColumn, discount.Percentage);
                SetField(row, PeriodicDiscountOfferIdColumn, discount.OfferId ?? string.Empty);
                SetField(row, StoreIdColumn, GetStoreId(transaction));
                SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                SetField(row, DiscountLineNumColumn, discountLineNumber);
                SetField(row, DiscountOriginTypeColumn, discount.DiscountLineTypeValue);
                SetField(row, CustomerDiscountTypeColumn, discount.CustomerDiscountTypeValue);
                SetField(row, ManualDiscountTypeColumn, discount.ManualDiscountTypeValue);

                discountTable.Rows.Add(row);
            }

            /// <summary>
            /// Save Charges for a line.
            /// </summary>
            /// <param name="charges">The charges.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="lineItem">The line item.</param>
            /// <param name="markupTable">The markup table.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillChargesForItemLine(IEnumerable<ChargeLine> charges, SalesTransaction transaction, SalesLine lineItem, DataTable markupTable, decimal lineNumber, RequestContext context)
            {
                ThrowIf.Null<IEnumerable<ChargeLine>>(charges, "charges");
                ThrowIf.Null<SalesLine>(lineItem, "lineItem");

                int markupLineNumber = 0;

                // AX doesn't consider price charges as markup lines, so exclude them from this table.
                foreach (ChargeLine charge in charges.Where(c => c.ChargeType != ChargeType.PriceCharge))
                {
                    FillChargeLine(charge, transaction, markupLineNumber++, markupTable, lineNumber, context);
                }
            }

            /// <summary>
            /// Save header information.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="transactionTable">The transaction data table.</param>
            /// <param name="markupTable">The markup data table.</param>
            /// <param name="attributeTable">The attribute data table.</param>
            /// <param name="addressTable">The address data table.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="propertiesTable">The properties table.</param>
            /// <param name="rewardPointTable">The reward point table.</param>
            /// <param name="context">The request context.</param>
            private static void FillOrderHeader(
                SalesTransaction transaction,
                DataTable transactionTable,
                DataTable markupTable,
                DataTable attributeTable,
                DataTable addressTable,
                DataTable reasonCodeTable,
                DataTable propertiesTable,
                DataTable rewardPointTable,
                RequestContext context)
            {
                ThrowIf.Null<SalesTransaction>(transaction, "transaction");
                ThrowIf.Null<DataTable>(transactionTable, "transactionTable");
                ThrowIf.Null<DataTable>(markupTable, "markupTable");
                ThrowIf.Null<DataTable>(attributeTable, "attributeTable");

                DataRow row = transactionTable.NewRow();

                SetField(row, RetailTransactionTableSchema.DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                SetField(row, RetailTransactionTableSchema.StoreColumn, GetStoreId(transaction));
                SetField(row, RetailTransactionTableSchema.TerminalColumn, transaction.TerminalId ?? string.Empty);
                SetField(row, RetailTransactionTableSchema.TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                SetField(row, RetailTransactionTableSchema.SalesOrderIdColumn, StringDataHelper.TruncateString(transaction.SalesId, 20));
                SetField(row, RetailTransactionTableSchema.ChannelReferenceIdColumn, StringDataHelper.TruncateString(transaction.ChannelReferenceId, 50));
                SetField(row, RetailTransactionTableSchema.TypeColumn, (int)transaction.TransactionType);
                SetField(row, RetailTransactionTableSchema.ChannelCurrencyColumn, context.GetChannelConfiguration().Currency ?? string.Empty);
                SetField(row, RetailTransactionTableSchema.ChannelCurrencyExchangeRateColumn, transaction.ChannelCurrencyExchangeRate);
                SetField(row, RetailTransactionTableSchema.EntryStatusColumn, (int)transaction.EntryStatus);
                SetField(row, RetailTransactionTableSchema.InventoryLocationIdColumn, StringDataHelper.TruncateString(transaction.InventoryLocationId, 10));
                SetField(row, RetailTransactionTableSchema.InventSiteIdColumn, string.Empty);
                SetField(row, RetailTransactionTableSchema.IncomeExpenseAmountColumn, transaction.IncomeExpenseTotalAmount * -1);
                SetField(row, RetailTransactionTableSchema.CustomerIdTableColumn, StringDataHelper.TruncateString(transaction.CustomerId, 38));
                SetField(row, RetailTransactionTableSchema.DeliveryModeTableColumn, StringDataHelper.TruncateString(transaction.DeliveryMode, 10));
                SetField(row, RetailTransactionTableSchema.ReceiptDateRequestedColumn, DateTimeOffsetDataHelper.GetDbDateTimeOrDefault(transaction.RequestedDeliveryDate));
                SetField(row, RetailTransactionTableSchema.ShippingDateRequestedColumn, DateTimeOffsetDataHelper.GetDbDateTimeOrDefault(transaction.RequestedDeliveryDate));
                SetField(row, RetailTransactionTableSchema.LoyaltyCardIdColumn, StringDataHelper.TruncateString(transaction.LoyaltyCardId, 30));
                SetField(row, RetailTransactionTableSchema.ReceiptEmailColumn, StringDataHelper.TruncateString(transaction.ReceiptEmail, 80));
                SetField(row, RetailTransactionTableSchema.ReceiptIdColumn, StringDataHelper.TruncateString(transaction.ReceiptId, 18));
                SetField(row, RetailTransactionTableSchema.StaffIdColumn, StringDataHelper.TruncateString(transaction.StaffId, 25));
                SetField(row, RetailTransactionTableSchema.NumberOfPaymentLinesColumn, transaction.TenderLines == null ? 0 : transaction.TenderLines.Count);
                SetField(row, RetailTransactionTableSchema.TimeWhenTransactionClosedColumn, context.GetNowInChannelTimeZone().TimeOfDay.TotalSeconds);
                SetField(row, RetailTransactionTableSchema.SaleIsReturnSaleColumn, transaction.IsReturnByReceipt ? 1 : 0);
                SetField(row, RetailTransactionTableSchema.DiscAmountColumn, transaction.DiscountAmount);
                SetField(row, RetailTransactionTableSchema.TotalDiscAmountColumn, transaction.TotalDiscount);
                SetField(row, RetailTransactionTableSchema.CustomerDiscountAmountColumn, transaction.LineDiscount);
                SetField(row, RetailTransactionTableSchema.TotalManualDiscountAmountColumn, transaction.TotalManualDiscountAmount);
                SetField(row, RetailTransactionTableSchema.TotalManualDiscountPercentageColumn, transaction.TotalManualDiscountPercentage);
                SetField(row, RetailTransactionTableSchema.ShiftIdColumn, transaction.ShiftId);
                SetField(row, RetailTransactionTableSchema.IsCreatedOfflineColumn, transaction.IsCreatedOffline);

                // For account deposit scenario we have to match ePOS behavior
                if (transaction.CartType == CartType.AccountDeposit)
                {
                    SetField(row, RetailTransactionTableSchema.NumberOfItemsColumn, 0m);
                    SetField(row, RetailTransactionTableSchema.NumberOfItemLinesColumn, 0m);

                    // In case of deposit we have to credit any payments being made (sum of tender lines), and debit nothing (zero)
                    SetField(row, RetailTransactionTableSchema.NetAmountColumn, 0m);
                    SetField(row, RetailTransactionTableSchema.AmountPaidTableColumn, 0m);
                    SetField(row, RetailTransactionTableSchema.SalesPaymentDifferenceColumn, decimal.Zero);
                    SetField(row, RetailTransactionTableSchema.GrossAmountColumn, 0m);
                }
                else
                {
                    SetField(row, RetailTransactionTableSchema.GrossAmountColumn, decimal.Negate(transaction.TotalAmount));
                    SetField(row, RetailTransactionTableSchema.NetAmountColumn, decimal.Negate(transaction.NetAmountWithNoTax));
                    SetField(row, RetailTransactionTableSchema.AmountPaidTableColumn, transaction.AmountPaid);
                    SetField(row, RetailTransactionTableSchema.SalesPaymentDifferenceColumn, transaction.SalesPaymentDifference);
                    SetField(row, RetailTransactionTableSchema.NumberOfItemsColumn, transaction.NumberOfItems);

                    // Number of all lines, both valid and voided.
                    SetField(row, RetailTransactionTableSchema.NumberOfItemLinesColumn, transaction.SalesLines == null ? 0m : (decimal)transaction.SalesLines.Count);
                }

                string terminalId = string.Empty;
                if (!string.IsNullOrEmpty(transaction.ShiftTerminalId))
                {
                    terminalId = transaction.ShiftTerminalId;
                }
                else if (context != null && context.GetTerminal() != null)
                {
                    terminalId = context.GetTerminal().TerminalId;
                }

                SetField(row, RetailTransactionTableSchema.ShiftTerminalIdColumn, terminalId);
                SetField(row, RetailTransactionTableSchema.CommentColumn, StringDataHelper.TruncateString(transaction.Comment, 60));
                SetField(row, RetailTransactionTableSchema.InvoiceCommentColumn, StringDataHelper.TruncateString(transaction.InvoiceComment, 60));
                SetField(row, RetailTransactionTableSchema.DescriptionColumn, string.Empty);

                SetField(row, RetailTransactionTableSchema.TransDateColumn, transaction.BeginDateTime.Date);

                // trans time is stored as seconds (integer) in the database
                SetField(row, RetailTransactionTableSchema.TransTimeColumn, (int)transaction.BeginDateTime.TimeOfDay.TotalSeconds);
                SetField(row, RetailTransactionTableSchema.BusinessDateColumn, TransactionLogDataManager.CalculateBusinessDate(context, transaction.BeginDateTime));
                SetField(row, RetailTransactionTableSchema.StatementCodeColumn, transaction.StatementCode ?? string.Empty);

                long addressId = GetAddressRecordId(transaction.ShippingAddress);
                SetField(row, LogisticsPostalAddressColumn, addressId);

                transactionTable.Rows.Add(row);

                // Add markup charges for the header.
                FillChargesForHeader(transaction.ChargeLines, transaction, markupTable, OrderLevelChargeLineNumber, context);

                FillAttributesForHeader(transaction, attributeTable, context);

                // Save transaction level reason codes.
                SaveReasonCodesForHeader(transaction, reasonCodeTable, context);

                SavePropertySet(transaction, transaction.PersistentProperties, propertiesTable, context);

                if (addressId == 0)
                {
                    // Save the one-time address on order.
                    FillAddress(transaction.ShippingAddress, transaction, addressTable, 0, context);
                }

                // Save reward point lines.
                FillRewardPointLines(transaction, rewardPointTable, context);
            }

            /// <summary>
            /// Fills in the reward point lines for save.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="rewardPointTable">The reward point data table.</param>
            /// <param name="context">The request context.</param>
            private static void FillRewardPointLines(SalesTransaction transaction, DataTable rewardPointTable, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(rewardPointTable, "rewardPointTable");

                foreach (var rewardPointLine in transaction.LoyaltyRewardPointLines)
                {
                    DataRow row = rewardPointTable.NewRow();

                    SetField(row, AffiliationColumn, rewardPointLine.LoyaltyGroupRecordId);
                    SetField(row, CardNumberColumn, rewardPointLine.LoyaltyCardNumber);
                    SetField(row, CustAccountColumn, rewardPointLine.CustomerAccount ?? string.Empty);
                    SetField(row, EntryDateColumn, rewardPointLine.EntryDate.Date);
                    SetField(row, EntryTimeColumn, rewardPointLine.EntryTime);
                    SetField(row, EntryTypeColumn, (int)rewardPointLine.EntryType);
                    SetField(row, ExpirationDateColumn, DateTimeOffsetDataHelper.GetDateOrDefaultSqlDate(rewardPointLine.ExpirationDate));
                    SetField(row, LineNumColumn, rewardPointLine.LineNumber);
                    SetField(row, LoyaltyTierColumn, rewardPointLine.LoyaltyTierRecordId);
                    SetField(row, ReceiptIdColumn, transaction.ReceiptId ?? string.Empty);
                    SetField(row, RewardPointColumn, rewardPointLine.RewardPointRecordId);
                    SetField(row, RewardPointAmountQtyColumn, rewardPointLine.RewardPointAmountQuantity);
                    SetField(row, StaffColumn, StringDataHelper.TruncateString(transaction.StaffId, 25));
                    SetField(row, StoreIdColumn, GetStoreId(transaction));
                    SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                    SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                    SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);

                    rewardPointTable.Rows.Add(row);
                }
            }

            /// <summary>
            /// Saves the one-time address if the address fields are present.
            /// </summary>
            /// <param name="address">Address to be saved.</param>
            /// <param name="transaction">Sales transaction.</param>
            /// <param name="addressTable">Address data table.</param>
            /// <param name="lineNumber">Line number - zero indicates header level.</param>
            /// <param name="context">The request context.</param>
            private static void FillAddress(Address address, SalesTransaction transaction, DataTable addressTable, decimal lineNumber, RequestContext context)
            {
                FillAddress(address, transaction, addressTable, lineNumber, GetStoreId(transaction), context);
            }

            /// <summary>
            /// Saves the one-time address if the address fields are present.
            /// </summary>
            /// <param name="address">Address to be saved.</param>
            /// <param name="transaction">Sales transaction.</param>
            /// <param name="addressTable">Address data table.</param>
            /// <param name="lineNumber">Line number - zero indicates header level.</param>
            /// <param name="storeNumber">The store number on sales transaction on fill order header address/ the store number on  sales line on fill item line address. The latter is required mainly for online channel multi mode shipping where the transaction will not not have any store number on it.</param>
            /// <param name="context">The request context.</param>
            private static void FillAddress(Address address, SalesTransaction transaction, DataTable addressTable, decimal lineNumber, string storeNumber, RequestContext context)
            {
                if (address == null)
                {
                    return;
                }

                DataRow row = addressTable.NewRow();
                SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                SetField(row, StoreColumn, storeNumber ?? string.Empty);
                SetField(row, TerminalColumn, transaction.TerminalId ?? string.Empty);
                SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                SetField(row, SaleLineNumColumn, lineNumber);
                SetField(row, DeliverNameColumn, address.Name ?? string.Empty);
                SetField(row, SalesNameColumn, transaction.Name ?? string.Empty);
                SetField(row, ZipCodeColumn, address.ZipCode ?? string.Empty);
                SetField(row, CountyRegionIdColumn, address.ThreeLetterISORegionName ?? string.Empty);
                SetField(row, StateColumn, address.State ?? string.Empty);
                SetField(row, CityColumn, address.City ?? string.Empty);
                SetField(row, CountyColumn, address.County ?? string.Empty);
                SetField(row, StreetColumn, address.Street ?? string.Empty);
                SetField(row, StreetNumberColumn, address.StreetNumber ?? string.Empty);
                SetField(row, DistrictNameColumn, address.DistrictName ?? string.Empty);
                SetField(row, EmailColumn, address.Email ?? string.Empty);
                SetField(row, EmailContentColumn, address.EmailContent ?? string.Empty);

                ContactInformation contactInformation = transaction.ContactInformationCollection.FirstOrDefault(
                    c => c.ContactInformationType == ContactInformationType.Email);

                contactInformation = transaction.ContactInformationCollection.FirstOrDefault(
                    c => c.ContactInformationType == ContactInformationType.Phone);
                string phone = contactInformation != null ? contactInformation.Value : string.Empty;

                SetField(row, PhoneColumn, phone ?? string.Empty);
                addressTable.Rows.Add(row);
            }

            /// <summary>
            /// Save all elements in a property set.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="propertySet">The property set.</param>
            /// <param name="propertiesTable">The property table.</param>
            /// <param name="context">The request context.</param>
            /// <param name="lineNumber">The sales line number or 0 if sales header.</param>
            private static void SavePropertySet(SalesTransaction transaction, ParameterSet propertySet, DataTable propertiesTable, RequestContext context, decimal lineNumber = 0m)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(propertySet, "propertySet");
                ThrowIf.Null(propertiesTable, "propertiesTable");

                foreach (var keyValuePair in propertySet)
                {
                    DataRow row = propertiesTable.NewRow();

                    SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                    SetField(row, StoreColumn, GetStoreId(transaction));
                    SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                    SetField(row, TransactionIdColumn, transaction.Id);
                    SetField(row, SaleLineNumColumn, lineNumber);
                    SetField(row, NameColumn, keyValuePair.Key);
                    SetField(row, ValueColumn, keyValuePair.Value.ToString());
                    propertiesTable.Rows.Add(row);
                }
            }

            /// <summary>
            /// Saves the reason codes for transaction level.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="context">The request context.</param>
            private static void SaveReasonCodesForHeader(
                SalesTransaction transaction,
                DataTable reasonCodeTable,
                RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(reasonCodeTable, "reasonCodeTable");

                if (transaction.ReasonCodeLines != null &&
                    transaction.ReasonCodeLines.Any())
                {
                    // Line number -1.00 indicate no parent line
                    FillReasonCodeLines(transaction.ReasonCodeLines, transaction, reasonCodeTable, -1m, context);
                }
            }

            /// <summary>
            /// Save attribute collection for the order.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="attributeTable">The attribute data table.</param>
            /// <param name="context">The request context.</param>
            private static void FillAttributesForHeader(SalesTransaction transaction, DataTable attributeTable, RequestContext context)
            {
                foreach (AttributeValueBase value in transaction.AttributeValues)
                {
                    // Only TextValues are supported
                    AttributeTextValue textValue = value as AttributeTextValue;
                    if (textValue != null)
                    {
                        DataRow row = attributeTable.NewRow();
                        SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                        SetField(row, StoreColumn, GetStoreId(transaction));
                        SetField(row, TerminalColumn, transaction.TerminalId ?? string.Empty);
                        SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                        SetField(row, NameColumn, textValue.Name ?? string.Empty);
                        SetField(row, TextValueColumn, textValue.TextValue ?? string.Empty);
                        attributeTable.Rows.Add(row);
                    }
                }
            }

            /// <summary>
            /// Gets the record identifier of the given address.
            /// </summary>
            /// <param name="shippingAddress">The shipping address.</param>
            /// <returns>The record identifier if the shipping address is a customer address or <c>0</c> if it is not.</returns>
            private static long GetAddressRecordId(Address shippingAddress)
            {
                Address address = shippingAddress as Address;
                return (address != null) ? address.RecordId : 0;
            }

            /// <summary>
            /// Save charges for the order header.
            /// </summary>
            /// <param name="charges">The charges.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="markupTable">The markup table.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillChargesForHeader(IEnumerable<ChargeLine> charges, SalesTransaction transaction, DataTable markupTable, decimal lineNumber, RequestContext context)
            {
                ThrowIf.Null<IEnumerable<ChargeLine>>(charges, "charges");

                int markupLineNumber = 0;
                foreach (ChargeLine charge in charges)
                {
                    FillChargeLine(charge, transaction, markupLineNumber++, markupTable, lineNumber, context);
                }
            }

            /// <summary>
            /// Save charge from a line.
            /// </summary>
            /// <param name="chargeLine">The charge line.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="markupLineNumber">The markup line number.</param>
            /// <param name="markupTable">The markup table.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="context">The request context.</param>
            private static void FillChargeLine(
                ChargeLine chargeLine,
                SalesTransaction transaction,
                int markupLineNumber,
                DataTable markupTable,
                decimal lineNumber,
                RequestContext context)
            {
                ThrowIf.Null<ChargeLine>(chargeLine, "ChargeLine");
                ThrowIf.Null<SalesTransaction>(transaction, "transaction");
                ThrowIf.Null<DataTable>(markupTable, "markupTable");

                DataRow row = markupTable.NewRow();

                SetField(row, CorrencyCodeColumn, context.GetChannelConfiguration().Currency ?? string.Empty);
                SetField(row, MarkUpCodeColumn, chargeLine.ChargeCode ?? string.Empty);
                SetField(row, MarkUpLineNumColumn, markupLineNumber);
                SetField(row, SaleLineNumColumn, lineNumber);
                SetField(row, StoreColumn, GetStoreId(transaction));
                SetField(row, TaxGroupColumn, chargeLine.SalesTaxGroupId ?? string.Empty);
                SetField(row, TaxItemGroupColumn, chargeLine.ItemTaxGroupId ?? string.Empty);
                SetField(row, TerminalIdColumn, transaction.TerminalId ?? string.Empty);
                SetField(row, TransactionIdColumn, StringDataHelper.TruncateString(transaction.Id, TransactionIdLength));
                SetField(row, ValueColumn, chargeLine.Value);
                SetField(row, ChargeMethodColumn, chargeLine.ChargeMethod);
                SetField(row, CalculatedAmountColumn, chargeLine.CalculatedAmount);
                SetField(row, TaxAmountColumn, chargeLine.TaxAmount);
                SetField(row, TaxAmountInclusiveColumn, chargeLine.TaxAmountInclusive);
                SetField(row, TaxAmountExclusiveColumn, chargeLine.TaxAmountExclusive);
                SetField(row, DataAreaIdColumn, context.GetChannelConfiguration().InventLocationDataAreaId);
                markupTable.Rows.Add(row);
            }

            /// <summary>
            /// Populates the table with the AddressTrans schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateAddressSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(SaleLineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(DeliverNameColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(SalesNameColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ZipCodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(CountyRegionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StateColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(CityColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(CountyColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StreetColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(EmailColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(EmailContentColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(PhoneColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StreetNumberColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(DistrictNameColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with the AttributeTrans schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateAttributeSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TextValueColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(NameColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with TaxTrans schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateTaxSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");
                tableSchema.Columns.Add(AmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(IsIncludedInPriceColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(SaleLineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(StoreIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TaxCodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with MarkupTrans schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateMarkupSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");
                tableSchema.Columns.Add(CorrencyCodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(MarkUpCodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(MarkUpLineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(SaleLineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TaxGroupColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TaxItemGroupColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ValueColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(CalculatedAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(ChargeMethodColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(TaxAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TaxAmountInclusiveColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TaxAmountExclusiveColumn, typeof(decimal)).DefaultValue = 0m;
            }

            /// <summary>
            /// Populates the table with income and expense schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateIncomeExpenseLineSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ReceiptIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(IncomeExpenseAccountColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StaffColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionStatusColumn, typeof(int)).DefaultValue = (int)TransactionStatus.Normal;
                tableSchema.Columns.Add(AmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(AccountTypeColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(TransDateColumn, typeof(DateTime)).DefaultValue = null;
                tableSchema.Columns.Add(TransTimeColumn, typeof(int)).DefaultValue = null;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ChannelColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with customer account deposit schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateCustomerAccountDepositLineSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ReceiptIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(CustAccountColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StaffColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionStatusColumn, typeof(int)).DefaultValue = (int)TransactionStatus.Normal;
                tableSchema.Columns.Add(AmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TransDateColumn, typeof(DateTime)).DefaultValue = null;
                tableSchema.Columns.Add(TransTimeColumn, typeof(int)).DefaultValue = null;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ChannelColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(CommentColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with SalesTrans schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateSaleLineSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(CustAccountColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(CreatedDateTimeColumn, typeof(DateTime)).DefaultValue = DateTime.UtcNow;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(LineManualDiscountAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(LineManualDiscountPercentageColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(DiscAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TotalDiscAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TotalDiscPctColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(LineDscAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(PeriodicDiscAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(PeriodicDiscPctColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(DlvModeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(InventDimIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(InventLocationIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(InventSerialIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(InventSiteIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ItemIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(BarcodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(LineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(ListingIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(LogisticsPostalAddressColumn, typeof(long)).DefaultValue = 0;
                tableSchema.Columns.Add(NetAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(NetAmountWithAllInclusiveTaxColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(PriceColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(PriceChangeColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(OriginalPriceColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(QtyColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(ReceiptDateRequestedColumn, typeof(DateTime)).DefaultValue = null;
                tableSchema.Columns.Add(ShippingDateRequestedColumn, typeof(DateTime)).DefaultValue = null;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TaxAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TaxGroupColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(OriginalSalesTaxGroupColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TaxItemGroupColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(OriginalTaxItemGroupColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StaffIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(UnitColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(VariantIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ReturnNoSaleColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(ReturnTransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ReturnLineNumberColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(ReturnStoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ReturnTerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ReceiptIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransDateColumn, typeof(DateTime)).DefaultValue = null;
                tableSchema.Columns.Add(TransTimeColumn, typeof(int)).DefaultValue = null;
                tableSchema.Columns.Add(TransactionStatusColumn, typeof(int)).DefaultValue = (int)TransactionStatus.Normal;
                tableSchema.Columns.Add(CommentColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(GiftCardColumn, typeof(bool)).DefaultValue = false;
                tableSchema.Columns.Add(CatalogIdColumn, typeof(long)).DefaultValue = 0;
                tableSchema.Columns.Add(ElectronicDeliveryEmailColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ElectronicDeliveryEmailContentColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(FulfillmentStoreIdColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with Discount schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateDiscountSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");
                tableSchema.Columns.Add(AmountColumn, typeof(decimal)).DefaultValue = decimal.Zero;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(DealPriceColumn, typeof(decimal)).DefaultValue = decimal.Zero;
                tableSchema.Columns.Add(DiscountAmountColumn, typeof(decimal)).DefaultValue = decimal.Zero;
                tableSchema.Columns.Add(DiscountCodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(LineNumColumn, typeof(decimal)).DefaultValue = decimal.Zero;
                tableSchema.Columns.Add(DiscountPercentageColumn, typeof(decimal)).DefaultValue = decimal.Zero;
                tableSchema.Columns.Add(PeriodicDiscountOfferIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(SaleLineNumColumn, typeof(decimal)).DefaultValue = decimal.Zero;
                tableSchema.Columns.Add(StoreIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(DiscountOriginTypeColumn, typeof(int)).DefaultValue = (int)DiscountLineType.None;
                tableSchema.Columns.Add(CustomerDiscountTypeColumn, typeof(int)).DefaultValue = (int)CustomerDiscountType.None;
                tableSchema.Columns.Add(ManualDiscountTypeColumn, typeof(int)).DefaultValue = (int)ManualDiscountType.None;
            }

            /// <summary>
            /// Populates the table with the reason code schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateReasonCodeSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(LineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TypeColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(ReasonCodeIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(InformationColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(InformationAmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TransDateColumn, typeof(DateTime)).DefaultValue = null;
                tableSchema.Columns.Add(TransTimeColumn, typeof(int)).DefaultValue = null;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StaffColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ItemTenderColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(AmountColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(InputTypeColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(SubReasonCodeIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StatementCodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(SourceCodeColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(SourceCode2Column, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(SourceCode3Column, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ParentLineNumColumn, typeof(decimal)).DefaultValue = 0m;
            }

            /// <summary>
            /// Populate the table with Properties schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulatePropertiesSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StoreColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(SaleLineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(NameColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ValueColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with the reward point schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateRewardPointSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(AffiliationColumn, typeof(long)).DefaultValue = 0;
                tableSchema.Columns.Add(CardNumberColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(CustAccountColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(EntryDateColumn, typeof(DateTime)).DefaultValue = DateTime.UtcNow;
                tableSchema.Columns.Add(EntryTimeColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(EntryTypeColumn, typeof(int)).DefaultValue = 0;
                tableSchema.Columns.Add(ExpirationDateColumn, typeof(DateTime)).DefaultValue = DateTime.UtcNow;
                tableSchema.Columns.Add(LineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(LoyaltyTierColumn, typeof(long)).DefaultValue = 0;
                tableSchema.Columns.Add(ReceiptIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(RewardPointColumn, typeof(long)).DefaultValue = 0;
                tableSchema.Columns.Add(RewardPointAmountQtyColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(StaffColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StoreIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with the affiliations schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateAffiliationsSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(AffiliationColumn, typeof(long)).DefaultValue = 0;
                tableSchema.Columns.Add(LoyaltyTierColumn, typeof(long)).DefaultValue = 0;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(ReceiptIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StaffColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StoreIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
            }

            /// <summary>
            /// Populates the table with the invoice schema.
            /// </summary>
            /// <param name="tableSchema">The table schema.</param>
            private static void PopulateInvoiceSchema(DataTable tableSchema)
            {
                ThrowIf.Null(tableSchema, "tableSchema");

                tableSchema.Columns.Add(DataAreaIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(StoreIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TerminalIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(TransactionIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(LineNumColumn, typeof(decimal)).DefaultValue = 0m;
                tableSchema.Columns.Add(TransactionStatusColumn, typeof(int)).DefaultValue = (int)TransactionStatus.Normal;
                tableSchema.Columns.Add(InvoiceIdColumn, typeof(string)).DefaultValue = string.Empty;
                tableSchema.Columns.Add(InvoiceAmountColumn, typeof(decimal)).DefaultValue = 0m;
            }

            private static SingleEntityDataServiceResponse<ReceiptMask> GetReceiptMask(GetReceiptMaskDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.FunctionalityProfileId, "request.FunctionalityProfileId");

                // Query mask by functionality profile Id and receipt transaction type
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    From = ReceiptMaskView,
                    Where = "FUNCPROFILEID = @FuncProfileId AND RECEIPTTRANSTYPE = @ReceiptTransType",
                };

                query.Parameters["@FuncProfileId"] = request.FunctionalityProfileId;
                query.Parameters["@ReceiptTransType"] = request.ReceiptTransactionType;

                ReadOnlyCollection<ReceiptMask> masks;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    masks = databaseContext.ReadEntity<ReceiptMask>(query).Results;
                }

                return new SingleEntityDataServiceResponse<ReceiptMask>(masks.Any() ? masks.SingleOrDefault() : null);
            }
        }
    }
}
