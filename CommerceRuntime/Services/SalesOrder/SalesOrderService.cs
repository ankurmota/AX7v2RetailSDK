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
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using System.Xml;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Framework;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using DM = Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Sales Order Service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Pending code refactoring per bug 683291.")]
        public class SalesOrderService : IRequestHandler
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
                        typeof(GetOrdersServiceRequest),
                        typeof(CreateSalesOrderServiceRequest),
                        typeof(GetNextReceiptIdServiceRequest),
                        typeof(GenerateOrderNumberServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Execute requests.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response result;
                CreateSalesOrderServiceRequest salesOrderServiceRequest;
                GetOrdersServiceRequest getOrdersRequest;
                GetNextReceiptIdServiceRequest getNextReceiptIdRequest;
                GenerateOrderNumberServiceRequest generateOrderNumberServiceRequest;
    
                if ((getOrdersRequest = request as GetOrdersServiceRequest) != null)
                {
                    NetTracer.Information("SalesOrder.Execute(): Request=GetOrdersServiceRequest");
                    result = GetOrders(getOrdersRequest);
                }
                else if ((salesOrderServiceRequest = request as CreateSalesOrderServiceRequest) != null)
                {
                    NetTracer.Information("SalesOrder.Execute(): Request=CreateSalesOrderRequest");
                    result = CreateSalesOrder(salesOrderServiceRequest);
                }
                else if ((getNextReceiptIdRequest = request as GetNextReceiptIdServiceRequest) != null)
                {
                    NetTracer.Information("SalesOrder.Execute(): Request=GetNextReceiptIdServiceRequest");
                    result = GetNextReceiptId(getNextReceiptIdRequest);
                }
                else if ((generateOrderNumberServiceRequest = request as GenerateOrderNumberServiceRequest) != null)
                {
                    NetTracer.Information("SalesOrder.Execute(): Request=GenerateOrderNumberServiceRequest");
                    result = GenerateOrderNumber(generateOrderNumberServiceRequest);
                }
                else
                {
                    NetTracer.Information("SalesOrder.Execute(): Unknown Request");
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request));
                }
    
                return result;
            }
    
            /// <summary>
            /// Get the aggregate Sales Status from the combination of Order Status and Document Status.
            /// </summary>
            /// <param name="salesOrderStatus">The sales order status.</param>
            /// <param name="salesOrderDocStatus">The sales order document status.</param>
            /// <returns>The sales status.</returns>
            internal static SalesStatus GetSalesStatus(SalesOrderStatus salesOrderStatus, DocumentStatus salesOrderDocStatus)
            {
                SalesStatus salesStatus = GetSalesStatus(salesOrderStatus);
    
                switch (salesStatus)
                {
                    case SalesStatus.Unknown:
                    case SalesStatus.Created:
                        SalesStatus documentStatus = GetSalesStatus(salesOrderDocStatus);
                        if (documentStatus == SalesStatus.Unknown)
                        {
                            return salesStatus;
                        }
    
                        return documentStatus;
                    default:
                        return salesStatus;
                }
            }
    
            /// <summary>
            /// Convert SalesOrderStatus to SalesStatus.
            /// </summary>
            /// <param name="salesOrderStatus">The sales order status.</param>
            /// <returns>The sales status.</returns>
            internal static SalesStatus GetSalesStatus(SalesOrderStatus salesOrderStatus)
            {
                switch (salesOrderStatus)
                {
                    case SalesOrderStatus.Backorder: return SalesStatus.Created;
                    case SalesOrderStatus.Delivered: return SalesStatus.Delivered;
                    case SalesOrderStatus.Invoiced: return SalesStatus.Invoiced;
                    case SalesOrderStatus.Canceled: return SalesStatus.Canceled;
                    default: return SalesStatus.Unknown;
                }
            }
    
            /// <summary>
            /// Convert DocumentStatus to SalesStatus.
            /// </summary>
            /// <param name="docStatus">The document status.</param>
            /// <returns>The sales status.</returns>
            internal static SalesStatus GetSalesStatus(DocumentStatus docStatus)
            {
                switch (docStatus)
                {
                    case DocumentStatus.None: return SalesStatus.Created;
                    case DocumentStatus.PickingList: return SalesStatus.Processing;
                    case DocumentStatus.PackingSlip: return SalesStatus.Delivered;
                    case DocumentStatus.Invoice: return SalesStatus.Invoiced;
                    case DocumentStatus.Canceled: return SalesStatus.Canceled;
                    case DocumentStatus.Lost: return SalesStatus.Lost;
                    default: return SalesStatus.Unknown;
                }
            }
    
            /// <summary>
            /// Parse a Sales Order from XML.
            /// </summary>
            /// <param name="xml">The XML document to parse.</param>
            /// <returns>Sales Order object if successful, null otherwise.</returns>
            protected static DM.SalesOrder ParseSalesOrderFromXml(string xml)
            {
                NetTracer.Information("SalesOrder.ParseSalesOrderFromXml(): xml='{0}'", xml);
    
                /*
                <?xml version="1.0" encoding="utf-8"?>
                <SalesTable RecId="22565421758" SalesId="SO-101248"
                            SalesName="Black Curve Airport (US)" CustAccount="2014"
                            ChannelReferenceId="" ChannelId="1233464" SalesStatus="1" DocumentStatus="0"
                            DlvMode="30" AddressRecId="5637145507" DeliveryName="Black Curve Airport (US) (Delivery)"
                            InventSiteId="" InventLocationId=""
                            CreatedDateTime="08/05/2012 16:58:55"
                            ReceiptDateRequested="18-05-2012"
                            TotalAmount="45.060" TotalTaxAmount="7.070" TotalDiscount="0.000">
                    <LogisticsPostalAddress RecId="5637145507" Location="5637149072"
                                            Address="123 Blue Road&#xA;Pueblo, CO 81001&#xA;US"
                                            BuildingCompliment="" PostBox=""
                                            County="PUEBLO" City="Pueblo"
                                            DistrictName="" Street="123 Blue Road"
                                            StreetNumber="" State="CO"
                                            ZipCode="81001" CountryRegionId="USA" />
                    <SalesLine RecId="22565422151" ItemId="0003"
                                InventDimId="00008902_069" SalesQty="1.00"
                                SalesDeliverNow="0.00" SalesPrice="34.990"
                                TaxGroup="WAKING" TaxItemGroup="ALL"
                                LineAmount="34.990" LineAmountExclTax="34.990" LineAmountDisc="0.000"
                                DlvMode="30" AddressRecId="5637145507" DeliveryName="Black Curve Airport (US) (Delivery)"
                                InventBatchId="" WmsLocationId="" WmsPalletId=""
                                InventSiteId="S1" InventLocationId="S0002" ConfigId="" InventSizeId="" InventColorId="" InventStyleId="" InventSerialId="">
                    <LogisticsPostalAddress RecId="5637145507" Location="5637149072"
                                            Address="123 Blue Road&#xA;Pueblo, CO 81001&#xA;US"
                                            BuildingCompliment="" PostBox=""
                                            County="PUEBLO" City="Pueblo"
                                            DistrictName="" Street="123 Blue Road"
                                            StreetNumber="" State="CO" ZipCode="81001"
                                            CountryRegionId="USA" />
                    <MarkupTrans MarkupCode="02" Value="2.000" CurrencyCode="USD" TaxGroup="WAKING" TaxItemGroup="" TaxAmount="0.000" ModuleType="1" CalculatedAmount="3.000" MarkupCategory="0" />
                    </SalesLine>
                    <MarkupTrans MarkupCode="01" Value="1.000" CurrencyCode="USD" TaxGroup="WAKING" TaxItemGroup="" TaxAmount="0.000" ModuleType="1" CalculatedAmount="3.000" MarkupCategory="0" />
                    <EcoResTextValue Name="TextAttribute" TextValue="Some attribute text!!" />
                </SalesTable>
                */
    
                DM.SalesOrder order = null;
                XDocument doc = null;
                XElement header = null;
    
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    try
                    {
                        doc = XDocument.Parse(xml);
                    }
                    catch (XmlException ex)
                    {
                        throw new ArgumentOutOfRangeException("Invalid xml", ex);
                    }
    
                    if (doc != null && doc.Root != null)
                    {
                        header = doc.Elements("SalesTable").FirstOrDefault();
                    }
    
                    if (header != null)
                    {
                        order = new DM.SalesOrder();
    
                        // Get header attributes
                        order.RecordId = long.Parse(header.Attribute("RecId").Value);
                        order.SalesId = header.Attribute("SalesId").Value;
                        order.ChannelReferenceId = header.Attribute("ChannelReferenceId").Value;
                        long channelId;
                        long.TryParse(header.Attribute("ChannelId").Value, out channelId);
                        order.ChannelId = channelId;
                        order.CustomerId = header.Attribute("CustAccount").Value;
                        order.Name = header.Attribute("SalesName").Value;
                        order.Id = header.Attribute("TransactionId").Value;
                        order.CreatedDateTime = AxContainerHelper.ParseDateString(header.Attribute("CreatedDateTime").Value, DateTime.UtcNow, DateTimeStyles.AssumeUniversal);
                        order.TotalAmount = decimal.Parse(header.Attribute("TotalAmount").Value);
                        order.TaxAmount = decimal.Parse(header.Attribute("TotalTaxAmount").Value);
                        order.TotalDiscount = decimal.Parse(header.Attribute("TotalDiscount").Value);
                        order.ChargeAmount = decimal.Parse(header.Attribute("TotalCharges").Value);
                        order.InventoryLocationId = header.Attribute("InventLocationId").Value;
                        order.DeliveryMode = header.Attribute("DlvMode").Value;
                        order.RequestedDeliveryDate = AxContainerHelper.ParseDateString(header.Attribute("ReceiptDateRequested").Value, DateTime.UtcNow, DateTimeStyles.AssumeUniversal);
    
                        SalesOrderStatus orderStatus = (SalesOrderStatus)int.Parse(header.Attribute("SalesStatus").Value);
                        DocumentStatus docStatus = (DocumentStatus)int.Parse(header.Attribute("DocumentStatus").Value);
                        order.Status = GetSalesStatus(orderStatus, docStatus);
    
                        XElement addressNode = header.Elements("LogisticsPostalAddress").FirstOrDefault();
                        if (addressNode != null)
                        {
                            Address address = Address.ParseAddressFromXml(addressNode);
                            address.Name = header.Attribute("DeliveryName").Value;
                            order.ShippingAddress = address;
                        }
    
                        // Get lines
                        ParseLinesFromXml(order, header);
    
                        // Get header charges
                        ParseChargesFromXml(header, order.ChargeLines);
    
                        // Get any attributes
                        ParseAttributesFromXml(order, header);
                    }
                }
    
                return order;
            }
    
            /// <summary>
            /// Get sales order header information from AXContainer.
            /// </summary>
            /// <param name="salesRecord">The sales record.</param>
            /// <returns>The sales order.</returns>
            protected static DM.SalesOrder GetSalesOrderSummaryFromContainer(IList salesRecord)
            {
                if (salesRecord == null)
                {
                    throw new ArgumentNullException("salesRecord");
                }
    
                NetTracer.Information("SalesOrder.GetSalesOrderSummaryFromContainer(): ContainerLength='{0}'", salesRecord.Count);
    
                ////[true,                                                                         //0-result
                ////"",                                                                           //1-comment
                ////salesTable        ? salesTable.SalesId                                : "",   //2-SalesId
                ////0,                                                                            //3-not used
                ////salesTable        ? totalDepositsMade                                 : 0,    //4-Total deposits made to this sales order
                ////salesTable        ? totalAmount                                       : 0,    //5-Total sales order amount
                ////salesTable        ? salesTable.CustAccount                            : "",   //6-Customer Id on sales order
                ////custTable         ? custTable.name()                                  : "",   //7-Customer name on sales order
                ////salesTable        ? DateTimeUtil::toStr(salesTable.createdDateTime)   : "",   //8-Date of sales order creation
                ////enum2str(salesTable.SalesType),                                               //9-sales type label
                ////enum2str(salesTable.SalesStatus),                                             //10-sales status label
                ////salesTable        ? salesTable.DocumentStatus                         : 0,    //11-doc status
                ////true,                                                                         //12-true = sales order (false = quotation)
                ////salesTable        ? salesTable.DlvMode                                : "",   //13-Delivery mode
                ////salesTable        ? salesTable.SalesStatus                            : 0,    //14-sales status
                ////salesTable        ? totalTaxAmount                                    : 0,    //15-TaxAmount
                ////salesTable        ? totalDiscountAmount                               : 0,    //16-DiscountAmount
                ////salesTable        ? totalChargeAmount                                 : 0,    //17-ChargeAmount
                ////salesTable        ? salesTable.InventLocationId                       : "",   //18-LocationId
                ////salesTable        ? salesTable.inventsiteId                           : "",   //19-SiteId
                ////rsoTable          ? rsoTable.ChannelReferenceId                       : "",   //20-Channel Ref Id.
                ////rtosTable         ? rtosTable.transactionId                           : ""    //21-TransactionId
                ////salesTable        ? salesTable.Email                                  : "",   //22-Email
                ////rsoTable          ? int642str(rsoTable.RetailChannel)                 : ""    //23-Channel Identifier.
                ////salesTable        ? int642str(salesTable.RecId)                       : "0"   //24-SalesTable Rec Identifier.
                ////];
    
                DM.SalesOrder row = null;
                bool success = AxContainerHelper.BooleanAtIndex(salesRecord, 0);
    
                // The particular sales order at this position in the container is not blank, extract it
                if (success)
                {
                    row = new DM.SalesOrder();
    
                    // some of these fields may not be properly initialized even if recordRetVal is true
                    row.SalesId = AxContainerHelper.StringAtIndex(salesRecord, 2);
                    row.TotalAmount = AxContainerHelper.DecimalAtIndex(salesRecord, 5);
                    row.CustomerId = AxContainerHelper.StringAtIndex(salesRecord, 6);
                    row.Name = AxContainerHelper.StringAtIndex(salesRecord, 7);
                    row.CreatedDateTime = AxContainerHelper.DateTimeAtIndex(salesRecord, 8, DateTimeKind.Utc);
    
                    row.Status = AxContainerHelper.BooleanAtIndex(salesRecord, 12)
                        ? GetSalesStatus((SalesOrderStatus)salesRecord[14], (DocumentStatus)salesRecord[11])
                        : SalesStatus.Unknown;
    
                    row.DeliveryMode = AxContainerHelper.StringAtIndex(salesRecord, 13);
                    row.TaxAmount = AxContainerHelper.DecimalAtIndex(salesRecord, 15);
                    row.TotalDiscount = AxContainerHelper.DecimalAtIndex(salesRecord, 16);
                    row.ChargeAmount = AxContainerHelper.DecimalAtIndex(salesRecord, 17);
                    row.InventoryLocationId = AxContainerHelper.StringAtIndex(salesRecord, 18);
    
                    // Channel Ref and TransactionId
                    row.ChannelReferenceId = AxContainerHelper.StringAtIndex(salesRecord, 20);
                    row.Id = AxContainerHelper.StringAtIndex(salesRecord, 21);
    
                    row.ChannelId = AxContainerHelper.LongAtIndex(salesRecord, 23);
                    row.RecordId = AxContainerHelper.LongAtIndex(salesRecord, 24);
                }
    
                // Either NULL, or a valid entity
                return row;
            }
    
            /// <summary>
            /// Get sales orders using the request criteria.
            /// </summary>
            /// <param name="request">Request containing the criteria used to retrieve sales orders.</param>
            /// <returns>GetOrdersServiceResponse object.</returns>
            private static GetOrdersServiceResponse GetOrders(GetOrdersServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                NetTracer.Information("SalesOrder.GetOrders()");
    
                IEnumerable<SalesOrder> localSalesTransactions = Enumerable.Empty<SalesOrder>();
                IEnumerable<SalesOrder> remoteRecords = Enumerable.Empty<SalesOrder>();
    
                if (request.Criteria == null || request.Criteria.IsEmpty())
                {
                    throw new ArgumentException("Must pass at least one search criteria.");
                }
    
                if (request.Criteria.SearchType == OrderSearchType.None)
                {
                    if (request.Criteria.TransactionIds.Any() &&
                        !string.IsNullOrWhiteSpace(request.Criteria.TransactionIds.FirstOrDefault()))
                    {
                        request.Criteria.SearchType = OrderSearchType.SalesTransaction;
                    }
                    else if (!string.IsNullOrWhiteSpace(request.Criteria.SalesId))
                    {
                        request.Criteria.SearchType = OrderSearchType.SalesOrder;
                    }
                    else
                    {
                        request.Criteria.SearchType = OrderSearchType.SalesTransaction; // Default value.
                    }
                }
    
                if (request.Criteria.SearchLocationType.HasFlag(SearchLocation.Local))
                {
                    var searchOrdersDataRequest = new SearchSalesTransactionDataRequest(
                        request.Criteria,
                        request.QueryResultSettings);
                    EntityDataServiceResponse<SalesOrder> searchOrdersDataResponse = request.RequestContext.Runtime
                        .Execute<EntityDataServiceResponse<SalesOrder>>(searchOrdersDataRequest, request.RequestContext);
    
                    localSalesTransactions = searchOrdersDataResponse.PagedEntityCollection.Results;
                }
    
                if (request.Criteria.SearchLocationType.HasFlag(SearchLocation.Remote))
                {
                    try
                    {
                        GetOrdersRealtimeRequest realtimeRequest = new GetOrdersRealtimeRequest(request.Criteria, request.QueryResultSettings);
                        GetOrdersRealtimeResponse getOrdersServiceResponse = request.RequestContext.Execute<GetOrdersRealtimeResponse>(realtimeRequest);
                        remoteRecords = getOrdersServiceResponse.Orders.Results;
                    }
                    catch (Exception e)
                    {
                        // Throws the exception if search remote is intentional.
                        if (request.Criteria.SearchLocationType == SearchLocation.Remote)
                        {
                            throw;
                        }
    
                        // Eats the exception since search remote is optional and log the error details as a warning.
                        NetTracer.Warning(e, "Search remote orders failed.");
                    }
                }
    
                IEnumerable<SalesOrder> mergedOrders;
    
                switch (request.Criteria.SearchType)
                {
                    case OrderSearchType.ConsolidateOrder:
                        mergedOrders = MergeOrdersBySalesId(remoteRecords, localSalesTransactions);
                        break;
    
                    case OrderSearchType.SalesTransaction:

                        mergedOrders = MultiDataSourcesPagingHelper.MergeResults<SalesOrder, SalesOrder>(
                            remoteRecords, 
                            localSalesTransactions,
                            (salesOrder) => { return salesOrder; },
                            new OrderComparer());
                        break;
    
                    case OrderSearchType.SalesOrder:
                        mergedOrders = GetSalesOrdersFromResult(remoteRecords);
                        break;
    
                    default:
                        throw new NotSupportedException(string.Format("SearchType not supported: {0}", request.Criteria.SearchType));
                }
    
                foreach (SalesOrder order in mergedOrders)
                {
                    SetNonPersistentProperties(order);

                    // Populate transaction level tax lines according to the tax lines of all its sales lines.
                    foreach (SalesLine salesLine in order.SalesLines)
                    {
                        if (salesLine.SalesStatus == SalesStatus.Canceled ||
                            salesLine.Status == TransactionStatus.Canceled ||
                            salesLine.Status == TransactionStatus.Voided)
                        {
                            continue;
                        }
                        
                        SalesTransactionTotaler.AddToTaxItems(order, salesLine);
                    }
                }
    
                // Sorts merged orders.
                if (request.QueryResultSettings.Sorting != null && request.QueryResultSettings.Sorting.IsSpecified)
                {
                    mergedOrders = mergedOrders.Sort(request.QueryResultSettings.Sorting.GetEnumerator());
                }
                else
                {
                    mergedOrders = mergedOrders.OrderByDescending(order => order.CreatedDateTime);
                }
    
                // Paginates merged orders.
                var response = new GetOrdersServiceResponse(new PagedResult<SalesOrder>(mergedOrders.AsReadOnly(), request.QueryResultSettings.Paging));
                return response;
            }

            /// <summary>
            /// Retrieve sales orders only from the result collections provided.
            /// </summary>
            /// <param name="remoteRecords">The remote collection.</param>
            /// <returns>The sales orders enumerable.</returns>
            private static IEnumerable<SalesOrder> GetSalesOrdersFromResult(IEnumerable<SalesOrder> remoteRecords)
            {
                Dictionary<string, SalesOrder> recordBySalesId = new Dictionary<string, SalesOrder>();
    
                // remote records could be sales transactions or orders
                foreach (SalesOrder record in remoteRecords)
                {
                    // this means that the record information comes directly from the SalesTable or SalesQuotation, not from retail transaction table
                    bool isOrderOrQuote = record.RecordId != 0;
                    string salesId = record.SalesId;
    
                    // sales order MUST have a sales id and be an order or quote
                    if (!string.IsNullOrWhiteSpace(salesId) && isOrderOrQuote)
                    {
                        recordBySalesId[salesId] = record;
                    }
                }
    
                return recordBySalesId.Values;
            }
  
            /// <summary>
            /// Merge local and remote sales orders by sales identifier.
            /// Key identifier for merging is sales order identifier, so
            /// there will be no duplicated order with the same sales order identifier.
            /// </summary>
            /// <param name="remoteOrders">The list of remote orders.</param>
            /// <param name="localOrders">The list of local orders.</param>
            /// <returns>Merged orders from remote and local orders.</returns>
            private static ICollection<SalesOrder> MergeOrdersBySalesId(IEnumerable<SalesOrder> remoteOrders, IEnumerable<SalesOrder> localOrders)
            {
                IEnumerable<SalesOrder> bulkOrders = remoteOrders.Concat(localOrders);
                Dictionary<string, SalesOrder> distinctSalesOrder = new Dictionary<string, SalesOrder>();
                HashSet<string> transactionIdentifiersCollection = new HashSet<string>();
                IList<SalesOrder> mergedOrders = new List<SalesOrder>();
                string salesId, transactionIdentifier;
    
                foreach (SalesOrder salesOrder in bulkOrders)
                {
                    salesId = salesOrder.SalesId;
                    transactionIdentifier = salesOrder.Id;
    
                    if (!string.IsNullOrWhiteSpace(salesId))
                    {
                        if (!transactionIdentifiersCollection.Contains(transactionIdentifier))
                        {
                            transactionIdentifiersCollection.Add(transactionIdentifier);
                        }
    
                        if (distinctSalesOrder.ContainsKey(salesId))
                        {
                            // Order comes from RetailTransactionTable has RecordId 0.
                            if (salesOrder.RecordId > 0)
                            {
                                distinctSalesOrder[salesId] = salesOrder;
                            }
                        }
                        else
                        {
                            distinctSalesOrder.Add(salesId, salesOrder);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(transactionIdentifier))
                    {
                        if (!transactionIdentifiersCollection.Contains(transactionIdentifier))
                        {
                            transactionIdentifiersCollection.Add(transactionIdentifier);
                            mergedOrders.Add(salesOrder);
                        }
                    }
                    else
                    {
                        mergedOrders.Add(salesOrder);
                    }
                }
    
                foreach (KeyValuePair<string, SalesOrder> orderPair in distinctSalesOrder)
                {
                    mergedOrders.Add(orderPair.Value);
                }
    
                return mergedOrders;
            }
    
            /// <summary>
            /// Gets the next receipt identifier.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetNextReceiptIdServiceResponse GetNextReceiptId(GetNextReceiptIdServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                ThrowIf.Null(request.RequestContext.GetPrincipal(), "request.RequestContext.GetPrincipal()");
                ThrowIf.NullOrWhiteSpace(request.ReceiptNumberSequence, "request.ReceiptNumberSequence");
    
                NetTracer.Information("SalesOrder.GetNextReceiptId(): Begin");
    
                // Get the functionality profile Id of the store
                string orgUnitNumber = request.RequestContext.GetOrgUnit().OrgUnitNumber;
                string functionalityProfileId = request.RequestContext.GetOrgUnit().FunctionalityProfileId;
    
                if (string.IsNullOrWhiteSpace(functionalityProfileId))
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration,
                        ExceptionSeverity.Warning,
                        string.Format("The store '{0}' does not have a functionality profile configured.", orgUnitNumber));
                }
    
                var receiptTransactionType = NumberSequenceSeedTypeHelper.GetReceiptTransactionType(request.TransactionType, request.NetAmountWithNoTax, request.CustomerOrderMode);
                var dataRequest = new GetReceiptMaskDataRequest(functionalityProfileId, receiptTransactionType);
                ReceiptMask mask = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptMask>>(dataRequest, request.RequestContext).Entity;
    
                string terminalId = request.RequestContext.GetTerminal().TerminalId;
    
                var channelDateTime = request.RequestContext.GetNowInChannelTimeZone();
    
                string maskFormat = mask != null ? mask.Mask : string.Empty;
    
                // Generate the next receipt Id
                var nextReceiptId = ReceiptMaskFiller.FillMask(maskFormat, request.ReceiptNumberSequence, orgUnitNumber, terminalId, request.RequestContext.GetPrincipal().UserId, channelDateTime.DateTime);
    
                NetTracer.Information("SalesOrder.GetNextReceiptId(): End");
    
                return new GetNextReceiptIdServiceResponse(nextReceiptId);
            }
    
            /// <summary>
            /// Generates the order number.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Response containing the order number.</returns>
            private static GenerateOrderNumberServiceResponse GenerateOrderNumber(GenerateOrderNumberServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                string defaultOrderNumberMask = "#######@@@@@";
    
                var identifierRequest = new GenerateAlphanumericSecureIdentifierServiceRequest((uint)defaultOrderNumberMask.Length);
                string generatedIdentifier = request.RequestContext.Runtime.Execute<GenerateAlphanumericSecureIdentifierServiceResponse>(identifierRequest, request.RequestContext).GeneratedIdentifier;
                string orderNumber = OrderNumberMaskFiller.ApplyMask(generatedIdentifier, defaultOrderNumberMask, request.CharacterPool);
    
                return new GenerateOrderNumberServiceResponse(orderNumber);
            }
    
            /// <summary>
            /// Create a sales order from the given context.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The create sales order service response.</returns>
            private static CreateSalesOrderServiceResponse CreateSalesOrder(CreateSalesOrderServiceRequest request)
            {
                NetTracer.Information("SalesOrder.CreateSalesOrderRequest(): Begin");
    
                var salesOrder = CreateSalesOrder(request.RequestContext, request.Transaction);
    
                NetTracer.Information("SalesOrder.CreateSalesOrderRequest(): End");
    
                return new CreateSalesOrderServiceResponse(salesOrder);
            }
    
            /// <summary>
            /// Parse attributes from sales order xml node.
            /// </summary>
            /// <param name="order">The order.</param>
            /// <param name="header">The header.</param>
            private static void ParseAttributesFromXml(DM.SalesOrder order, XElement header)
            {
                foreach (XElement attribute in header.Elements("EcoResTextValue"))
                {
                    AttributeTextValue atv = new AttributeTextValue
                    {
                        Name = attribute.Attribute("Name").Value,
                        TextValue = attribute.Attribute("TextValue").Value
                    };
    
                    order.AttributeValues.Add(atv);
                }
            }
    
            /// <summary>
            /// Parse sale lines from sales order XML node.
            /// </summary>
            /// <param name="order">The sales order.</param>
            /// <param name="header">The header.</param>
            private static void ParseLinesFromXml(DM.SalesOrder order, XElement header)
            {
                foreach (XElement line in header.Elements("SalesLine"))
                {
                    decimal lineAmount = decimal.Parse(line.Attribute("LineAmount").Value);
                    decimal lineAmountExclTax = decimal.Parse(line.Attribute("LineAmountExclTax").Value);
    
                    SalesLine saleLine = new SalesLine
                    {
                        DeliveryMode = line.Attribute("DlvMode").Value,
                        InventoryLocationId = line.Attribute("InventLocationId").Value,
                        ProductId = long.Parse(line.Attribute("ListingId").Value),
                        ItemId = line.Attribute("ItemId").Value,
                        InventoryDimensionId = line.Attribute("InventDimId").Value,
                        ItemTaxGroupId = line.Attribute("TaxItemGroup").Value,
                        Price = decimal.Parse(line.Attribute("SalesPrice").Value),
                        Quantity = decimal.Parse(line.Attribute("SalesQty").Value),
                        SalesTaxGroupId = line.Attribute("TaxGroup").Value,
                        NetAmount = lineAmount,
                        SalesDate = order.CreatedDateTime
                    };
    
                    saleLine.TaxLines.Add(new TaxLine
                    {
                        Amount = lineAmount - lineAmountExclTax,
                        IsIncludedInPrice = false,
                    });
    
                    XElement addressNode = line.Elements("LogisticsPostalAddress").FirstOrDefault();
                    if (addressNode != null)
                    {
                        Address address = Address.ParseAddressFromXml(addressNode);
                        address.Name = line.Attribute("DeliveryName").Value;
                        saleLine.ShippingAddress = address;
                    }
    
                    order.SalesLines.Add(saleLine);
                }
            }
    
            /// <summary>
            /// Parse charge lines from a SalesOrder or SaleLine xml node.
            /// </summary>
            /// <param name="node">The XML node.</param>
            /// <param name="charges">The collection of charge lines.</param>
            private static void ParseChargesFromXml(XElement node, IList<ChargeLine> charges)
            {
                foreach (XElement charge in node.Elements("MarkupTrans"))
                {
                    ChargeLine c = new ChargeLine
                    {
                        CalculatedAmount = decimal.Parse(charge.Attribute("CalculatedAmount").Value),
                        ChargeCode = charge.Attribute("MarkupCode").Value,
                        ChargeType = ChargeType.ManualCharge,
                        ItemTaxGroupId = charge.Attribute("TaxItemGroup").Value,
                        ModuleType = (ChargeModule)int.Parse(charge.Attribute("ModuleType").Value),
                        SalesTaxGroupId = charge.Attribute("TaxGroup").Value,
                        Value = decimal.Parse(charge.Attribute("Value").Value),
                        ChargeMethod = (ChargeMethod)int.Parse(charge.Attribute("MarkupCategory").Value),
                    };
    
                    decimal taxAmount = decimal.Parse(charge.Attribute("TaxAmount").Value);
                    c.TaxLines.Add(new TaxLine
                    {
                        Amount = taxAmount,
                        IsIncludedInPrice = false,
                    });
    
                    charges.Add(c);
                }
            }
    
            /// <summary>
            /// Create the sales order in the local channel database.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <returns>The sales order created.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Saving order is an exceptional complicated process.")]
            private static SalesOrder CreateSalesOrder(RequestContext context, SalesTransaction transaction)
            {
                NetTracer.Information("SalesOrder.CreateSalesOrder()");
    
                // Validate transaction
                transaction.ValidateSalesOrder(context);
    
                // Save sales order.
                SalesTransaction orderToBeCreated = PrepareSalesOrderToBeCreated(context, transaction);
    
                var saveTransactionRequest = new SaveSalesTransactionDataRequest(orderToBeCreated);
                context.Runtime.Execute<NullResponse>(saveTransactionRequest, context);
    
                // Get the sales order created.
                var criteria = new SalesOrderSearchCriteria()
                {
                    TransactionIds = new[] { orderToBeCreated.Id },
                    StoreId = orderToBeCreated.StoreId,
                    TerminalId = orderToBeCreated.TerminalId,
                    CustomerAccountNumber = orderToBeCreated.CustomerId,
                    ChannelReferenceId = orderToBeCreated.ChannelReferenceId,
                    SearchLocationType = SearchLocation.Local,
                    IncludeDetails = true
                };
    
                // searching in local db only
                var getOrderRequest = new GetOrdersServiceRequest(criteria, QueryResultSettings.AllRecords);
                GetOrdersServiceResponse response = context.Execute<GetOrdersServiceResponse>(getOrderRequest);
    
                if (response.Orders == null || !response.Orders.Results.Any())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                        string.Format("Unable to get the sales order created. Transaction ID: {0}, Store ID: {1}, Terminal ID: {2}", orderToBeCreated.Id, orderToBeCreated.StoreId, orderToBeCreated.TerminalId));
                }
                else if (response.Orders.Results.HasMultiple())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DuplicateObject,
                        string.Format("{0} orders found with Transaction ID: {1}, Store ID: {2}, Terminal ID: {3}", response.Orders.Results.Count(), orderToBeCreated.Id, orderToBeCreated.StoreId, orderToBeCreated.TerminalId));
                }
    
                SalesOrder order = response.Orders.Results.Single();
                RefillSalesOrder(context, transaction, order);
    
                return order;
            }
    
            /// <summary>
            /// Refills the sales order.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="order">The order.</param>
            private static void RefillSalesOrder(RequestContext context, SalesTransaction transaction, SalesOrder order)
            {
                // Add tender lines
                if (order.TenderLines == null || !order.TenderLines.Any())
                {
                    order.TenderLines = new Collection<TenderLine>(transaction.TenderLines);
                }
    
                // Add reward point lines
                if ((order.LoyaltyRewardPointLines == null || !order.LoyaltyRewardPointLines.Any())
                    && (transaction.LoyaltyRewardPointLines != null && transaction.LoyaltyRewardPointLines.Any()))
                {
                    order.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>(transaction.LoyaltyRewardPointLines);
                }
    
                if (transaction.EntryStatus != TransactionStatus.Voided && transaction.LoyaltyRewardPointLines.Any())
                {
                    // Post reward points via CDX Real-time Service
                    // Only post points when such configuration is enabled.
                    var channelConfiguration = context.GetChannelConfiguration();
                    if (!channelConfiguration.EarnLoyaltyOffline)
                    {
                        try
                        {
                            context.Execute<NullResponse>(new PostLoyaltyCardRewardPointRealtimeRequest(LoyaltyRewardPointEntryType.ReturnEarned, transaction));
                            context.Execute<NullResponse>(new PostLoyaltyCardRewardPointRealtimeRequest(LoyaltyRewardPointEntryType.Earn, transaction));
                        }
                        catch (FeatureNotSupportedException)
                        {
                            // If we are in offline mode, a feature not supported exception will be thrown.
                            // If this is the case, an Information event will be already logged.
                        }
                        catch (Exception ex)
                        {
                            // There is an error posting reward points.
                            // We cannot let that stop the transaction because the transaction has already been submitted.
                            // Therefore, we just log error. The missing points will be posted in AX by batch jobs.
                            RetailLogger.Log.CrtServicesSalesOrderRealTimeServicePostLoyaltyCardRewardPointsFailure(ex);
                        }
                    }
                }
            }
    
            /// <summary>
            /// Prepares the sales order to be created.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="transaction">The transaction.</param>
            /// <returns>The sales transaction.</returns>
            /// <exception cref="ConfigurationException">Required Service missing: {0}.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            private static SalesTransaction PrepareSalesOrderToBeCreated(RequestContext context, SalesTransaction transaction)
            {
                if (transaction.EntryStatus != TransactionStatus.Voided)
                {
                    try
                    {
                        if (transaction.IsReturnByReceipt)
                        {
                            // Call the transaction service to update return quantities
                            context.Execute<NullResponse>(new MarkReturnedItemsRealtimeRequest(transaction));
                        }
                    }
                    catch (FeatureNotSupportedException)
                    {
                        // Realtime service is not supported in current configuration.
                        // It should not block checkout workflow hence suppress exception (it is already logged as warning by CommerceRuntime).
                    }
                    catch (HeadquarterTransactionServiceException ex)
                    {
                        // Realtime service call failed. It should not block checkout workflow hence log and suppress exception.
                        RetailLogger.Log.CrtServicesSalesOrderTransactionServiceMarkReturnedItemsFailure(ex);
                    }
    
                    // Mark return quantities if needed
                    if (transaction.ActiveSalesLines.Any(activeSalesLine => activeSalesLine.IsReturnByReceipt))
                    {
                        // Call the data service to update return quantity locally
                        var dataRequest = new UpdateReturnQuantitiesDataRequest(transaction.ActiveSalesLines);
                        context.Runtime.Execute<NullResponse>(dataRequest, context);
                    }
    
                    // Earn loyalty reward points
                    var calculateLoyaltyRewardPointsServiceRequest = new CalculateLoyaltyRewardPointsServiceRequest(transaction);
                    var calculateLoyaltyRewardPointsServiceResponse = context.Execute<CalculateLoyaltyRewardPointsServiceResponse>(calculateLoyaltyRewardPointsServiceRequest);
                    transaction = calculateLoyaltyRewardPointsServiceResponse.SalesTransaction;
    
                    // Set statement properties (not applicable for orders created from storefront).
                    if (transaction.TransactionType != SalesTransactionType.PendingSalesOrder)
                    {
                        DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();
    
                        if (deviceConfiguration == null)
                        {
                            string exceptionMessage = string.Format("Device configuration not found for channel '{0}' terminal '{1}'.", context.GetPrincipal().ChannelId, context.GetTerminal().TerminalId);
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, exceptionMessage);
                        }
    
                        string statementCode;
                        switch (deviceConfiguration.StatementMethod)
                        {
                            case StatementMethod.PosTerminal:
                                statementCode = context.GetTerminal().TerminalId;
                                break;
                            case StatementMethod.Staff:
                                statementCode = context.GetPrincipal().UserId;
                                break;
                            default:
                                statementCode = string.Empty;
                                break;
                        }
    
                        transaction.StatementCode = statementCode;
                    }
    
                    transaction.ChannelCurrencyExchangeRate = GetExchangeRate(context);
                }
    
                return transaction;
            }
    
            /// <summary>
            /// Sets the properties on sales order that are not persisted.
            /// </summary>
            /// <param name="order">Instance of <see cref="SalesOrder"/>.</param>
            private static void SetNonPersistentProperties(SalesOrder order)
            {
                foreach (SalesLine line in order.SalesLines)
                {
                    // Total and gross amounts are not persisted in the database and has to be calculated each time order is loaded.
                    line.TotalAmount = line.NetAmount + line.TaxAmount;
                    line.GrossAmount = line.NetAmountWithAllInclusiveTax + line.DiscountAmount;
    
                    // we don't persist the channel identifier for the line
                    line.ReturnChannelId = order.ChannelId;
                }
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
    
                if (!context.GetChannelConfiguration().CompanyCurrency.Equals(context.GetChannelConfiguration().Currency, StringComparison.OrdinalIgnoreCase))
                {
                    var getEchangeRateRequest = new GetExchangeRateServiceRequest(context.GetChannelConfiguration().CompanyCurrency, context.GetChannelConfiguration().Currency);
                    var response = context.Execute<GetExchangeRateServiceResponse>(getEchangeRateRequest);
                    exchangeRate = response.ExchangeRate;
                }
    
                return exchangeRate;
            }
        }
    }
}
