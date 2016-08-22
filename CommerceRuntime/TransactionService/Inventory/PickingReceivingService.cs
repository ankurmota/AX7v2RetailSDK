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
        using System.Text;
        using System.Text.RegularExpressions;
        using System.Xml.Linq;
        using Commerce.Runtime.TransactionService;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// The service to handle picking and receiving requests.
        /// </summary>
        public class PickingReceivingService : IRequestHandler
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
                        typeof(GetPurchaseOrderRealtimeRequest),
                        typeof(GetTransferOrderRealtimeRequest),
                        typeof(SavePurchaseOrderRealtimeRequest),
                        typeof(SaveTransferOrderRealtimeRequest),
                        typeof(GetPickingListRealtimeRequest),
                        typeof(SavePickingListRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the service request.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestedType = request.GetType();
    
                if (requestedType == typeof(GetPurchaseOrderRealtimeRequest))
                {
                    return GetPurchaseOrders((GetPurchaseOrderRealtimeRequest)request);
                }
    
                if (requestedType == typeof(GetTransferOrderRealtimeRequest))
                {
                    return GetTransferOrders((GetTransferOrderRealtimeRequest)request);
                }
    
                if (requestedType == typeof(SavePurchaseOrderRealtimeRequest))
                {
                    return SavePurchaseOrder((SavePurchaseOrderRealtimeRequest)request);
                }
    
                if (requestedType == typeof(SaveTransferOrderRealtimeRequest))
                {
                    return SaveTransferOrder((SaveTransferOrderRealtimeRequest)request);
                }
    
                if (requestedType == typeof(GetPickingListRealtimeRequest))
                {
                    return GetPickingLists((GetPickingListRealtimeRequest)request);
                }
    
                if (requestedType == typeof(SavePickingListRealtimeRequest))
                {
                    return SavePickingList((SavePickingListRealtimeRequest)request);
                }
    
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }
    
            /// <summary>
            /// Saves a purchase order in either local database or Ax.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            private static SavePurchaseTransferOrderRealtimeResponse SavePurchaseOrder(SavePurchaseOrderRealtimeRequest request)
            {
                if (!request.Commit)
                {
                    // Call data service
                    var inventoryDataServiceRequest = new SavePurchaseOrderLinesDataRequest(request.PurchaseOrder);
                    request.RequestContext.Runtime.Execute<NullResponse>(inventoryDataServiceRequest, request.RequestContext);
                }
                else
                {
                    TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
                    string xml = ConvertOrderToXML(request.PurchaseOrder, request.RequestContext);
    
                    ReadOnlyCollection<PickReceiveDocument> pickReceiveDocuments = transactionService.UpdatePurchaseOrder(xml);
    
                    ReadOnlyCollection<PurchaseOrder> purchaseOrders = GetPurchaseOrderFromDocument(pickReceiveDocuments[0].XMLDocument, false, request.RequestContext);
    
                    foreach (PurchaseOrder order in purchaseOrders)
                    {
                        foreach (PurchaseOrderLine line in order.OrderLines)
                        {
                            if (!line.IsCommitted)
                            {
                                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NotAllLinesSaved, "Not all lines could be saved");
                            }
                        }
    
                        // Call data service
                        var dataServiceRequest = new DeletePurchaseOrderLinesDataRequest(order.OrderId);
                        request.RequestContext.Runtime.Execute<NullResponse>(dataServiceRequest, request.RequestContext);
                    }
                }
    
                return new SavePurchaseTransferOrderRealtimeResponse();
            }
    
            /// <summary>
            /// Saves a transfer order in either local database or Ax.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            private static SavePurchaseTransferOrderRealtimeResponse SaveTransferOrder(SaveTransferOrderRealtimeRequest request)
            {
                if (!request.Commit)
                {
                    // Call data service
                    var dataServiceRequest = new SaveTransferOrderDataRequest(request.TransferOrder);
                    request.RequestContext.Runtime.Execute<NullResponse>(dataServiceRequest, request.RequestContext);
                }
                else
                {
                    TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
                    string xml = ConvertTransferOrderToXml(request.TransferOrder);
                    ReadOnlyCollection<PickReceiveDocument> pickReceiveDocuments = transactionService.UpdateTransferOrder(xml);
                    ReadOnlyCollection<TransferOrder> transferOrders = GetTransferOrderFromDocument(pickReceiveDocuments[0].XMLDocument, false, request.RequestContext);
    
                    foreach (TransferOrder order in transferOrders)
                    {
                        foreach (TransferOrderLine line in order.OrderLines)
                        {
                            if (!line.IsCommitted)
                            {
                                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NotAllLinesSaved, "Not all lines could be saved");
                            }
                        }
    
                        var dataServiceRequest = new DeleteTransferOrderLinesDataRequest(order.OrderId);
                        request.RequestContext.Runtime.Execute<NullResponse>(dataServiceRequest, request.RequestContext);
                    }
                }
    
                return new SavePurchaseTransferOrderRealtimeResponse();
            }
    
            /// <summary>
            /// Saves a picking list in either local database or Ax.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            private static SavePurchaseTransferOrderRealtimeResponse SavePickingList(SavePickingListRealtimeRequest request)
            {
                if (!request.Commit)
                {
                    // Call data service
                    var inventoryDataRequest = new SavePickingListDataRequest(request.PickingList);
                    request.RequestContext.Runtime.Execute<NullResponse>(inventoryDataRequest, request.RequestContext);
                }
                else
                {
                    TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
                    string xml = ConvertPickingListToXML(request.PickingList, request.RequestContext);
                    ReadOnlyCollection<PickReceiveDocument> pickReceiveDocuments = transactionService.UpdatePickingList(xml);
                    ReadOnlyCollection<PickingList> pickingLists = GetPickingListsFromDocument(pickReceiveDocuments[0].XMLDocument, false, request.RequestContext);
    
                    foreach (PickingList order in pickingLists)
                    {
                        foreach (PickingListLine line in order.OrderLines)
                        {
                            if (!line.IsCommitted)
                            {
                                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NotAllLinesSaved, "Not all lines could be saved");
                            }
                        }
    
                        var dataServiceRequest = new DeletePickingListLinesDataRequest(order.OrderId);
                        request.RequestContext.Runtime.Execute<NullResponse>(dataServiceRequest, request.RequestContext);
                    }
                }
    
                return new SavePurchaseTransferOrderRealtimeResponse();
            }
    
            private static string ConvertOrderToXML(PurchaseOrder purchaseOrder, RequestContext context)
            {
                StringBuilder output = new StringBuilder();
    
                output.Append("<PurchTable");
                output.Append(" PurchId=\"" + purchaseOrder.OrderId + "\"");
                output.Append(" RecId=\"" + purchaseOrder.RecordId + "\"");
                output.Append(">");
    
                foreach (PurchaseOrderLine line in purchaseOrder.OrderLines)
                {
                    output.Append(ConvertPurchaseOrderLineToXML(line, context));
                }
    
                output.Append("</PurchTable>");
    
                return output.ToString();
            }
    
            private static string ConvertPickingListToXML(PickingList pickingList, RequestContext context)
            {
                StringBuilder output = new StringBuilder();
    
                output.Append("<WMSPickingRoute");
                output.Append(" PickingRouteId=\"" + pickingList.OrderId + "\"");
                output.Append(">");
    
                foreach (PickingListLine line in pickingList.OrderLines)
                {
                    output.Append(ConvertPickingListLineToXML(line, context));
                }
    
                output.Append("</WMSPickingRoute>");
    
                return output.ToString();
            }
    
            /// <summary>
            /// Serialize to xml format of a transfer document.
            /// </summary>
            /// <param name="transferOrder">The transfer order.</param>
            /// <returns>Xml format of a transfer document.</returns>
            private static string ConvertTransferOrderToXml(TransferOrder transferOrder)
            {
                StringBuilder output = new StringBuilder();
    
                output.Append("<InventTransferTable");
                output.Append(" TransferId=\"" + transferOrder.OrderId + "\"");
                output.Append(">");
    
                foreach (TransferOrderLine line in transferOrder.OrderLines)
                {
                    output.Append(ConvertTransferOrderLineToXML(line));
                }
    
                output.Append(TransactionServiceClient.CreateExtensionPropertiesParameter(transferOrder.ExtensionProperties));
    
                output.Append("</InventTransferTable>");
    
                return output.ToString();
            }
    
            /// <summary>
            /// Convert a transfer order line to xml format that will be sent to Ax.
            /// </summary>
            /// <param name="line">The transfer order line.</param>
            /// <returns>The converted xml as string.</returns>
            private static string ConvertTransferOrderLineToXML(TransferOrderLine line)
            {
                XElement inventTransferLine =
                    new XElement(
                        "InventTransferLine",
                        new XAttribute("RecId", line.RecordId),
                        new XAttribute("ItemId", line.ItemId ?? string.Empty),
                        new XAttribute("EcoResProductName", line.ItemName ?? string.Empty),
                        new XAttribute("InventDimId", line.InventDimId ?? string.Empty),
                        new XAttribute("InventBatchId", line.InventBatchId ?? string.Empty),
                        new XAttribute("WmsLocationId", line.WMSLocationId ?? string.Empty),
                        new XAttribute("WmsPalletId", line.WMSPalletId ?? string.Empty),
                        new XAttribute("InventSiteId", line.InventSiteId ?? string.Empty),
                        new XAttribute("InventLocationId", line.InventLocationId ?? string.Empty),
                        new XAttribute("ConfigId", line.ConfigId ?? string.Empty),
                        new XAttribute("InventSizeId", line.InventSizeId ?? string.Empty),
                        new XAttribute("InventColorId", line.InventColorId ?? string.Empty),
                        new XAttribute("InventStyleId", line.InventStyleId ?? string.Empty),
                        new XAttribute("InventSerialId", line.InventSerialId ?? string.Empty),
                        new XAttribute("QtyTransfer", line.QuantityTransferred),
                        new XAttribute("QtyShipped", line.QuantityShipped),
                        new XAttribute("QtyReceived", line.QuantityReceived),
                        new XAttribute("QtyShipNow", line.QuantityShipNow),
                        new XAttribute("QtyReceiveNow", line.QuantityReceiveNow),
                        new XAttribute("QtyRemainShip", line.QuantityRemainShip),
                        new XAttribute("QtyRemainReceive", line.QuantityRemainReceive),
                        new XAttribute("UnitId", line.PurchaseUnit ?? string.Empty),
                        new XAttribute("Guid", line.Guid ?? string.Empty));
    
                return inventTransferLine.ToString();
            }
    
            /// <summary>
            /// Convert a purchase order line to xml format that will be sent to Ax.
            /// </summary>
            /// <param name="line">The transfer order line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The converted xml as string.</returns>
            private static string ConvertPurchaseOrderLineToXML(PurchaseOrderLine line, RequestContext context)
            {
                string inventLocationId = context.GetOrgUnit().InventoryLocationId;
    
                var purchLine =
                    new XElement(
                        "PurchLine",
                        new XAttribute("RecId", line.RecordId),
                        new XAttribute("ItemId", line.ItemId),
                        new XAttribute("EcoResProductName", line.ItemName ?? string.Empty),
                        new XAttribute("InventDimId", line.InventDimId ?? string.Empty),
                        new XAttribute("QtyOrdered", line.QuantityOrdered),
                        new XAttribute("PurchQty", line.PurchaseQuantity),
                        new XAttribute("PurchUnit", line.PurchaseUnit ?? string.Empty),
                        new XAttribute("PurchReceivedNow", line.PurchaseReceivedNow),
                        new XAttribute("InventBatchId", line.InventBatchId ?? string.Empty),
                        new XAttribute("WmsLocationId", line.WMSLocationId ?? string.Empty),
                        new XAttribute("WmsPalletId", line.WMSPalletId ?? string.Empty),
                        new XAttribute("InventSiteId", line.InventSiteId ?? string.Empty),
                        new XAttribute("InventLocationId", inventLocationId ?? string.Empty),
                        new XAttribute("ConfigId", line.ConfigId ?? string.Empty),
                        new XAttribute("InventSizeId", line.InventSizeId ?? string.Empty),
                        new XAttribute("InventColorId", line.InventColorId ?? string.Empty),
                        new XAttribute("InventStyleId", line.InventStyleId ?? string.Empty),
                        new XAttribute("InventSerialId", line.InventSerialId ?? string.Empty),
                        new XAttribute("Guid", line.UniqueIdentifier ?? string.Empty),
                        new XAttribute("DlvMode", line.DeliveryMethod ?? string.Empty));
    
                return purchLine.ToString();
            }
    
            /// <summary>
            /// Convert a picking list line to xml format that will be sent to Ax.
            /// </summary>
            /// <param name="line">The picking list line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The converted xml as string.</returns>
            private static string ConvertPickingListLineToXML(PickingListLine line, RequestContext context)
            {
                string inventLocationId = context.GetOrgUnit().InventoryLocationId;
    
                var pickingListLine =
                    new XElement(
                        "WMSOrderTrans",
                        new XAttribute("RecId", line.RecordId),
                        new XAttribute("ItemId", line.ItemId),
                        new XAttribute("EcoResProductName", line.ItemName ?? string.Empty),
                        new XAttribute("InventDimId", line.InventDimId ?? string.Empty),
                        new XAttribute("InventBatchId", line.InventBatchId ?? string.Empty),
                        new XAttribute("WmsLocationId", line.WMSLocationId ?? string.Empty),
                        new XAttribute("WmsPalletId", line.WMSPalletId ?? string.Empty),
                        new XAttribute("InventSiteId", line.InventSiteId ?? string.Empty),
                        new XAttribute("InventLocationId", inventLocationId ?? string.Empty),
                        new XAttribute("ConfigId", line.ConfigId ?? string.Empty),
                        new XAttribute("InventSizeId", line.InventSizeId ?? string.Empty),
                        new XAttribute("InventColorId", line.InventColorId ?? string.Empty),
                        new XAttribute("InventStyleId", line.InventStyleId ?? string.Empty),
                        new XAttribute("InventSerialId", line.InventSerialId ?? string.Empty),
                        new XAttribute("Qty", line.PurchaseReceivedNow),
                        new XAttribute("Guid", line.Guid ?? string.Empty),
                        new XAttribute("DlvMode", line.DeliveryMethod ?? string.Empty));
    
                return pickingListLine.ToString();
            }
    
            /// <summary>
            /// Get all open picking lists.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            private static Response GetPickingLists(GetPickingListRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
                string inventLocationId = request.RequestContext.GetOrgUnit().InventoryLocationId;
                ReadOnlyCollection<PickReceiveDocument> pickReceiveDocuments;
                List<PickingList> pickingLists;
    
                if (string.IsNullOrEmpty(request.OrderId))
                {
                    pickReceiveDocuments = transactionService.GetPickingLists(inventLocationId);
                    pickingLists = ConvertToPickingLists(pickReceiveDocuments, request.RequestContext);
                }
                else
                {
                    List<PickReceiveDocument> documents = new List<PickReceiveDocument> { transactionService.GetPickingList(request.OrderId, inventLocationId) };
                    pickReceiveDocuments = documents.AsReadOnly();
                    pickingLists = ConvertToPickingLists(pickReceiveDocuments, request.RequestContext);
                }
    
                return new GetPickingListRealtimeResponse(pickingLists.AsPagedResult());
            }
    
            /// <summary>
            /// Get all open purchase and/or transfer orders.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            private static Response GetPurchaseOrders(GetPurchaseOrderRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
                string inventLocationId = request.RequestContext.GetOrgUnit().InventoryLocationId;
                PagedResult<PickReceiveDocument> pickReceiveDocuments;
                PagedResult<PurchaseOrder> purchaseOrders;
    
                if (string.IsNullOrEmpty(request.OrderId))
                {
                    pickReceiveDocuments = transactionService.GetPurchaseOrders(inventLocationId);
                    purchaseOrders = ConvertToPurchaseOrders(pickReceiveDocuments, request.RequestContext);
                }
                else
                {
                    List<PickReceiveDocument> documents = new List<PickReceiveDocument> { transactionService.GetPurchaseOrder(request.OrderId, inventLocationId) };
                    pickReceiveDocuments = documents.AsPagedResult();
                    purchaseOrders = ConvertToPurchaseOrders(pickReceiveDocuments, request.RequestContext);
                }
    
                return new GetPurchaseOrderRealtimeResponse(purchaseOrders);
            }
    
            /// <summary>
            /// Get all open transfer orders.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            private static Response GetTransferOrders(GetTransferOrderRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
                string inventLocationId = request.RequestContext.GetOrgUnit().InventoryLocationId;
                ReadOnlyCollection<PickReceiveDocument> pickReceiveDocuments;
                List<TransferOrder> transferOrders;
    
                if (string.IsNullOrEmpty(request.OrderId))
                {
                    pickReceiveDocuments = transactionService.GetTransferOrders(inventLocationId);
                    transferOrders = ConvertToTransferOrders(pickReceiveDocuments, request.RequestContext);
                }
                else
                {
                    List<PickReceiveDocument> documents = new List<PickReceiveDocument> { transactionService.GetTransferOrder(request.OrderId) };
                    pickReceiveDocuments = documents.AsReadOnly();
                    transferOrders = ConvertToTransferOrders(pickReceiveDocuments, request.RequestContext);
                }
    
                return new GetTransferOrderRealtimeResponse(transferOrders.AsPagedResult());
            }
    
            /// <summary>
            /// Converts picking or receiving documents to a purchase order.
            /// </summary>
            /// <param name="searlizedDocuments">The collection of documents.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of purchase orders.</returns>
            private static PagedResult<PurchaseOrder> ConvertToPurchaseOrders(PagedResult<PickReceiveDocument> searlizedDocuments, RequestContext context)
            {
                var purchaseOrders = new List<PurchaseOrder>();
    
                foreach (PickReceiveDocument prdocument in searlizedDocuments.Results)
                {
                    string xml = RemoveXmlDeclaration(prdocument.XMLDocument);
    
                    if (!string.IsNullOrEmpty(xml))
                    {
                        purchaseOrders.AddRange(GetPurchaseOrderFromDocument(prdocument.XMLDocument, true, context));
                    }
                }
    
                return purchaseOrders.AsPagedResult();
            }
    
            /// <summary>
            /// Converts picking or receiving documents to a picking list.
            /// </summary>
            /// <param name="searlizedDocuments">The collection of documents.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of picking lists.</returns>
            private static List<PickingList> ConvertToPickingLists(ReadOnlyCollection<PickReceiveDocument> searlizedDocuments, RequestContext context)
            {
                List<PickingList> pickingLists = new List<PickingList>();
    
                foreach (PickReceiveDocument prdocument in searlizedDocuments)
                {
                    string xml = RemoveXmlDeclaration(prdocument.XMLDocument);
    
                    if (!string.IsNullOrEmpty(xml))
                    {
                        pickingLists.AddRange(GetPickingListsFromDocument(prdocument.XMLDocument, true, context));
                    }
                }
    
                return pickingLists;
            }
    
            /// <summary>
            /// Converts a xml string to a purchase or transfer orders.
            /// </summary>
            /// <param name="searlizedDocuments">The collection of documents.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of transfer orders.</returns>
            private static List<TransferOrder> ConvertToTransferOrders(ReadOnlyCollection<PickReceiveDocument> searlizedDocuments, RequestContext context)
            {
                List<TransferOrder> transferOrders = new List<TransferOrder>();
    
                foreach (PickReceiveDocument prdocument in searlizedDocuments)
                {
                    string xml = RemoveXmlDeclaration(prdocument.XMLDocument);
    
                    if (!string.IsNullOrEmpty(xml))
                    {
                        transferOrders.AddRange(GetTransferOrderFromDocument(prdocument.XMLDocument, true, context));
                    }
                }
    
                return transferOrders;
            }
    
            /// <summary>
            /// Gets picking lists from a xml.
            /// </summary>
            /// <param name="xml">The xml string.</param>
            /// <param name="getLinesFromDB">Indicates if order lines are to be retrieved from the database.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of picking lists.</returns>
            private static ReadOnlyCollection<PickingList> GetPickingListsFromDocument(string xml, bool getLinesFromDB, RequestContext context)
            {
                XDocument doc = XDocument.Parse(xml);
                XElement root = null;
    
                IEnumerable<PickingList> pickingList = new List<PickingList>();
                IEnumerable<PickingListLine> pickingListLines = new List<PickingListLine>();
                root = doc.Elements("WMSPickingRoutes").FirstOrDefault();
                bool getPickingListLines = true;
    
                IEnumerable<XElement> pickingRoutes = null;
    
                if (root == null)
                {
                    // request is get picking list details by order number
                    root = doc.Elements("WMSPickingRoute").FirstOrDefault();
    
                    if (root == null)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PurchaseTransferOrderMissing, "A picking list was not found.");
                    }
    
                    pickingRoutes = doc.Elements("WMSPickingRoute");
                }
                else
                {
                    pickingRoutes = root.Elements("WMSPickingRoute");
                    getPickingListLines = false; // the request is get all picking lists, no need to get product lines for enhancing performance.
                }
    
                pickingList = pickingRoutes.Select<XElement, PickingList>(
                      (po) =>
                      {
                          PickingList prOrder = ParsePickingList(po);
                          ReadOnlyCollection<PickingListLine> pickingListDB = null;
    
                          if (!getPickingListLines)
                          {
                              prOrder.OrderLines = new Collection<PickingListLine>();
                              prOrder.Lines = prOrder.OrderLines.Count;
                              return prOrder;
                          }
    
                          if (getLinesFromDB)
                          {
                              // Call data service
                              var dataServiceRequest = new GetPickingListDataRequest(prOrder.OrderId, QueryResultSettings.AllRecords);
                              pickingListDB = context.Runtime.Execute<EntityDataServiceResponse<PickingListLine>>(dataServiceRequest, context).PagedEntityCollection.Results;
                          }
    
                          pickingListLines = root.Elements("WMSOrderTrans").Select<XElement, PickingListLine>(
                             (poLine) =>
                             {
                                 PickingListLine prOrderLine = ParsePickingListLine(poLine);
                                 return prOrderLine;
                             });
    
                          if (pickingListDB != null && pickingListDB.Count > 0)
                          {
                              prOrder.OrderLines = pickingListDB;
                              prOrder.Lines = prOrder.OrderLines.Count;
                              prOrder.TotalReceived = (from line in prOrder.OrderLines select line.PurchaseReceivedNow).Sum();
                              prOrder.TotalItems = (from line in prOrder.OrderLines select line.QuantityOrdered).Sum();
    
                              // set quantity from transaction service values
                              foreach (var orderLine in prOrder.OrderLines)
                              {
                                  var realTimeOrderLine = pickingListLines.FirstOrDefault(p => p.InventDimId == orderLine.InventDimId &&
                                      p.ItemId == orderLine.ItemId);
                                  if (realTimeOrderLine != null)
                                  {
                                      orderLine.QuantityOrdered = realTimeOrderLine.QuantityOrdered;
                                  }
                              }
                          }
                          else
                          {
                              prOrder.OrderLines = pickingListLines.AsReadOnly();
                          }
    
                          return prOrder;
                      });
    
                return pickingList.AsReadOnly();
            }
    
            /// <summary>
            /// Gets purchase orders from a xml.
            /// </summary>
            /// <param name="xml">The xml string.</param>
            /// <param name="getLinesFromDB">Indicates if order lines are to be retrieved from the database.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of purchase orders.</returns>
            private static ReadOnlyCollection<PurchaseOrder> GetPurchaseOrderFromDocument(string xml, bool getLinesFromDB, RequestContext context)
            {
                XDocument doc = XDocument.Parse(xml);
                XElement root = null;
    
                IEnumerable<PurchaseOrder> purchaseDocs = new List<PurchaseOrder>();
                IEnumerable<PurchaseOrderLine> purchaseOrderDocLines = new List<PurchaseOrderLine>();
                root = doc.Elements("PurchTables").FirstOrDefault();
    
                if (root == null)
                {
                    root = doc.Elements("PurchTable").FirstOrDefault();
    
                    if (root == null)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PurchaseTransferOrderMissing, "A purchase order was not found.");
                    }
    
                    purchaseDocs = doc.Elements("PurchTable").Select<XElement, PurchaseOrder>(
                      (po) =>
                      {
                          PurchaseOrder prOrder = ParsePurchaseOrder(po);
                          ReadOnlyCollection<PurchaseOrderLine> purchaseOrdersDB = null;
    
                          if (getLinesFromDB)
                          {
                              // Call data service
                              var inventoryDataServiceRequest = new GetPurchaseOrderDataRequest(prOrder.OrderId, QueryResultSettings.AllRecords);
                              purchaseOrdersDB = context.Runtime.Execute<EntityDataServiceResponse<PurchaseOrderLine>>(inventoryDataServiceRequest, context).PagedEntityCollection.Results;
                          }
    
                          if (purchaseOrdersDB != null && purchaseOrdersDB.Count > 0)
                          {
                              prOrder.OrderLines = purchaseOrdersDB;
                              prOrder.Lines = prOrder.OrderLines.Count;
                              prOrder.TotalReceived = (from line in prOrder.OrderLines select line.PurchaseReceived).Sum();
                              prOrder.TotalItems = (from line in prOrder.OrderLines select line.QuantityOrdered).Sum();
                          }
                          else
                          {
                              purchaseOrderDocLines = root.Elements("PurchLine").Select<XElement, PurchaseOrderLine>(
                             (poLine) =>
                             {
                                 PurchaseOrderLine prOrderLine = ParsePurchaseOrderLine(poLine);
                                 return prOrderLine;
                             });
    
                              prOrder.OrderLines = purchaseOrderDocLines.AsReadOnly();
                          }
    
                          return prOrder;
                      });
                }
                else
                {
                    purchaseDocs = root.Elements("PurchTable").Select<XElement, PurchaseOrder>(
                            (po) =>
                            {
                                PurchaseOrder prOrder = ParsePurchaseOrder(po);
                                return prOrder;
                            });
                }
    
                return purchaseDocs.AsReadOnly();
            }
    
            /// <summary>
            /// Gets transfer orders from a xml.
            /// </summary>
            /// <param name="xml">The xml string.</param>
            /// <param name="getLinesFromDB">Indicates if order lines are to be retrieved from the database.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of transfer orders.</returns>
            private static ReadOnlyCollection<TransferOrder> GetTransferOrderFromDocument(string xml, bool getLinesFromDB, RequestContext context)
            {
                XDocument doc = XDocument.Parse(xml);
                XElement root = doc.Elements("InventTransferTables").FirstOrDefault();
    
                IEnumerable<TransferOrder> transferDocs = new List<TransferOrder>();
    
                if (root == null)
                {
                    root = doc.Elements("InventTransferTable").FirstOrDefault();
    
                    if (root == null)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PurchaseTransferOrderMissing, "A transfer order was not found.");
                    }
    
                    transferDocs = doc.Elements("InventTransferTable").Select<XElement, TransferOrder>(
                      (po) =>
                      {
                          TransferOrder prOrder = ParseTransferOrder(po, context);
                          ReadOnlyCollection<TransferOrderLine> transferOrdersDB = null;
    
                          if (getLinesFromDB)
                          {
                              // Call data service
                              var dataServiceRequest = new GetTransferOrderDataRequest(prOrder.OrderId, QueryResultSettings.AllRecords);
                              transferOrdersDB = context.Runtime.Execute<EntityDataServiceResponse<TransferOrderLine>>(dataServiceRequest, context).PagedEntityCollection.Results;
                          }
    
                          if (transferOrdersDB != null && transferOrdersDB.Count > 0)
                          {
                              prOrder.OrderLines = transferOrdersDB;
                              prOrder.Lines = prOrder.OrderLines.Count;
                              prOrder.TotalItems = (from line in prOrder.OrderLines select line.QuantityTransferred).Sum();
                          }
                          else
                          {
                              IEnumerable<TransferOrderLine> transferOrderrDocLines = root.Elements("InventTransferLine").Select<XElement, TransferOrderLine>(
                             (poLine) =>
                             {
                                 TransferOrderLine prOrderLine = ParseTransferOrderLine(poLine);
                                 return prOrderLine;
                             });
                              prOrder.OrderLines = transferOrderrDocLines.AsReadOnly();
                          }
    
                          return prOrder;
                      });
                }
                else
                {
                    transferDocs = root.Elements("InventTransferTable").Select<XElement, TransferOrder>(
                            (po) =>
                            {
                                TransferOrder prOrder = ParseTransferOrder(po, context);
                                return prOrder;
                            });
                }
    
                return transferDocs.AsReadOnly();
            }
    
            private static string RemoveXmlDeclaration(string xml)
            {
                // Remove Xml declaration
                Match xmlDeclaration = Regex.Match(xml, @"<\?xml.*\?>");
                if (xmlDeclaration.Success)
                {
                    xml = xml.Replace(xmlDeclaration.Value, string.Empty);
                }
    
                return xml;
            }
    
            private static PurchaseOrder ParsePurchaseOrder(XElement document)
            {
                PurchaseOrder purchaseOrder = new PurchaseOrder();
                decimal value;
    
                if (document != null)
                {
                    purchaseOrder.OrderId = GetAttributeValue(document, "PurchId");
                    purchaseOrder.Status = GetAttributeValue(document, "PurchStatus");
                    purchaseOrder.RecordId = GetAttributeValue(document, "RecId");
                    decimal.TryParse(GetAttributeValue(document, "LINES"), out value);
                    purchaseOrder.Lines = value;
                    decimal.TryParse(GetAttributeValue(document, "TOTALORDERED"), out value);
                    purchaseOrder.TotalItems = value;
                    decimal.TryParse(GetAttributeValue(document, "TOTALRECEIVED"), out value);
                    purchaseOrder.TotalReceived = value;
                    purchaseOrder.OrderType = PurchaseTransferOrderType.PurchaseOrder;
                }
    
                return purchaseOrder;
            }
    
            private static PickingList ParsePickingList(XElement document)
            {
                PickingList pickingList = new PickingList();
                decimal value;
    
                if (document != null)
                {
                    pickingList.OrderId = GetAttributeValue(document, "PickingRouteId");
                    if (string.IsNullOrEmpty(pickingList.OrderId))
                    {
                        pickingList.OrderId = GetAttributeValue(document, "PickingRouteID");
                    }
    
                    pickingList.RecordId = GetAttributeValue(document, "RecId");
                    decimal.TryParse(GetAttributeValue(document, "LINES"), out value);
                    pickingList.Lines = value;
                    decimal.TryParse(GetAttributeValue(document, "TOTALORDERED"), out value);
                    pickingList.TotalItems = value;
                    decimal.TryParse(GetAttributeValue(document, "TOTALRECEIVED"), out value);
                    pickingList.TotalReceived = value;
                    pickingList.OrderType = PurchaseTransferOrderType.PickingList;
                    DateTime deliveryDate;
                    DateTime.TryParse(GetAttributeValue(document, "DlvDate"), out deliveryDate);
                    pickingList.DeliveryDate = deliveryDate;
                    pickingList.DeliveryMode = GetAttributeValue(document, "DlvMode");
                    pickingList.Status = GetAttributeValue(document, "ExpeditionStatus");
                }
    
                return pickingList;
            }
    
            private static TransferOrder ParseTransferOrder(XElement document, RequestContext context)
            {
                TransferOrder transferOrder = new TransferOrder();
                decimal value;
    
                if (document != null)
                {
                    transferOrder.OrderId = GetAttributeValue(document, "TransferId");
                    transferOrder.Status = GetAttributeValue(document, "TransferStatus");
                    transferOrder.RecordId = GetAttributeValue(document, "RecId");
                    decimal.TryParse(GetAttributeValue(document, "LINES"), out value);
                    transferOrder.Lines = value;
                    decimal.TryParse(GetAttributeValue(document, "QtyTransfer"), out value);
                    transferOrder.TotalItems = value;
                    decimal.TryParse(GetAttributeValue(document, "QtyShipped"), out value);
                    transferOrder.QuantityShipped = value;
                    decimal.TryParse(GetAttributeValue(document, "QtyReceived"), out value);
                    transferOrder.QuantityReceived = value;
                    decimal.TryParse(GetAttributeValue(document, "QtyShipNow"), out value);
                    transferOrder.QuantityShipNow = value;
                    decimal.TryParse(GetAttributeValue(document, "QtyReceiveNow"), out value);
                    transferOrder.QuantityReceiveNow = value;
                    decimal.TryParse(GetAttributeValue(document, "QtyRemainShip"), out value);
                    transferOrder.QuantityShipRemaining = value;
                    decimal.TryParse(GetAttributeValue(document, "QtyRemainReceive"), out value);
                    transferOrder.QuantityReceiveRemaining = value;
                    transferOrder.InventLocationIdFrom = GetAttributeValue(document, "InventLocationIdFrom");
                    transferOrder.InventLocationIdTo = GetAttributeValue(document, "InventLocationIdTo");
    
                    string inventLocationId = context.GetOrgUnit().InventoryLocationId;
    
                    if (transferOrder.InventLocationIdFrom == inventLocationId)
                    {
                        transferOrder.OrderType = PurchaseTransferOrderType.TransferOut;
                    }
                    else if (transferOrder.InventLocationIdTo == inventLocationId)
                    {
                        transferOrder.OrderType = PurchaseTransferOrderType.TransferIn;
                    }
                    else
                    {
                        transferOrder.OrderType = PurchaseTransferOrderType.TransferOrder;
                    }
                }
    
                return transferOrder;
            }
    
            private static PurchaseOrderLine ParsePurchaseOrderLine(XElement documentLine)
            {
                PurchaseOrderLine purchaseOrderLine = new PurchaseOrderLine();
                string recIdStr;
    
                if (documentLine != null)
                {
                    recIdStr = GetAttributeValue(documentLine, "RecId");
                    purchaseOrderLine.RecordId = string.IsNullOrWhiteSpace(recIdStr) ? 0L : long.Parse(recIdStr);
                    purchaseOrderLine.ItemId = GetAttributeValue(documentLine, "ItemId");
                    purchaseOrderLine.ItemName = GetAttributeValue(documentLine, "EcoResProductName");
                    purchaseOrderLine.InventDimId = GetAttributeValue(documentLine, "InventDimId");
                    purchaseOrderLine.QuantityOrdered = Convert.ToDecimal(GetAttributeValue(documentLine, "QtyOrdered"), CultureInfo.InvariantCulture);
                    purchaseOrderLine.PurchaseQuantity = Convert.ToDecimal(GetAttributeValue(documentLine, "PurchQty"), CultureInfo.InvariantCulture);
                    purchaseOrderLine.PurchaseUnit = GetAttributeValue(documentLine, "PurchUnit");
                    purchaseOrderLine.PurchaseReceived = Convert.ToDecimal(GetAttributeValue(documentLine, "PurchReceivedNow"), CultureInfo.InvariantCulture);
                    purchaseOrderLine.PurchaseReceivedNow = 0;
                    purchaseOrderLine.InventBatchId = GetAttributeValue(documentLine, "InventBatchId");
                    purchaseOrderLine.WMSLocationId = GetAttributeValue(documentLine, "WmsLocationId");
                    purchaseOrderLine.WMSPalletId = GetAttributeValue(documentLine, "WmsPalletId");
                    purchaseOrderLine.InventSiteId = GetAttributeValue(documentLine, "InventSiteId");
                    purchaseOrderLine.InventLocationId = GetAttributeValue(documentLine, "InventLocationId");
                    purchaseOrderLine.InventSizeId = GetAttributeValue(documentLine, "InventSizeId");
                    purchaseOrderLine.InventColorId = GetAttributeValue(documentLine, "InventColorId");
                    purchaseOrderLine.InventStyleId = GetAttributeValue(documentLine, "InventStyleId");
                    purchaseOrderLine.InventSerialId = GetAttributeValue(documentLine, "InventSerialId");
                    purchaseOrderLine.ConfigId = GetAttributeValue(documentLine, "ConfigId");
                    purchaseOrderLine.IsCommitted = Convert.ToBoolean(GetAttributeValue(documentLine, "UpdatedInAx"));
                    purchaseOrderLine.Message = GetAttributeValue(documentLine, "Message");
                    purchaseOrderLine.UniqueIdentifier = GetAttributeValue(documentLine, "Guid");
                    purchaseOrderLine.DeliveryMethod = GetAttributeValue(documentLine, "DlvMode");
                }
    
                return purchaseOrderLine;
            }
    
            private static PickingListLine ParsePickingListLine(XElement documentLine)
            {
                PickingListLine pickingListLine = new PickingListLine();
                string recIdStr;
    
                if (documentLine != null)
                {
                    recIdStr = GetAttributeValue(documentLine, "RecId");
                    pickingListLine.RecordId = string.IsNullOrWhiteSpace(recIdStr) ? 0L : long.Parse(recIdStr);
                    pickingListLine.ItemId = GetAttributeValue(documentLine, "ItemId");
                    pickingListLine.ItemName = GetAttributeValue(documentLine, "EcoResProductName");
                    pickingListLine.InventDimId = GetAttributeValue(documentLine, "InventDimId");
                    pickingListLine.QuantityOrdered = Convert.ToDecimal(GetAttributeValue(documentLine, "Qty"), CultureInfo.InvariantCulture);
                    pickingListLine.InventBatchId = GetAttributeValue(documentLine, "InventBatchId");
                    pickingListLine.WMSLocationId = GetAttributeValue(documentLine, "WmsLocationId");
                    pickingListLine.WMSPalletId = GetAttributeValue(documentLine, "WmsPalletId");
                    pickingListLine.InventSiteId = GetAttributeValue(documentLine, "InventSiteId");
                    pickingListLine.InventLocationId = GetAttributeValue(documentLine, "InventLocationId");
                    pickingListLine.InventSizeId = GetAttributeValue(documentLine, "InventSizeId");
                    pickingListLine.InventColorId = GetAttributeValue(documentLine, "InventColorId");
                    pickingListLine.InventStyleId = GetAttributeValue(documentLine, "InventStyleId");
                    pickingListLine.InventSerialId = GetAttributeValue(documentLine, "InventSerialId");
                    pickingListLine.ConfigId = GetAttributeValue(documentLine, "ConfigId");
                    pickingListLine.IsCommitted = Convert.ToBoolean(GetAttributeValue(documentLine, "UpdatedInAx"));
                    pickingListLine.Message = GetAttributeValue(documentLine, "Message");
                    pickingListLine.Guid = GetAttributeValue(documentLine, "Guid");
                    pickingListLine.DeliveryMethod = GetAttributeValue(documentLine, "DlvMode");
                }
    
                return pickingListLine;
            }
    
            private static TransferOrderLine ParseTransferOrderLine(XElement documentLine)
            {
                TransferOrderLine transferOrderLine = new TransferOrderLine();
                string recIdStr;
    
                if (documentLine != null)
                {
                    recIdStr = GetAttributeValue(documentLine, "RecId");
                    transferOrderLine.RecordId = string.IsNullOrWhiteSpace(recIdStr) ? 0L : long.Parse(recIdStr);
                    transferOrderLine.ItemId = GetAttributeValue(documentLine, "ItemId");
                    transferOrderLine.ItemName = GetAttributeValue(documentLine, "EcoResProductName");
                    transferOrderLine.InventDimId = GetAttributeValue(documentLine, "InventDimId");
                    transferOrderLine.QuantityTransferred = Convert.ToDecimal(GetAttributeValue(documentLine, "QtyTransfer"), CultureInfo.InvariantCulture);
                    transferOrderLine.QuantityReceived = Convert.ToDecimal(GetAttributeValue(documentLine, "QtyReceiveNow"), CultureInfo.InvariantCulture);
                    transferOrderLine.QuantityReceiveNow = 0;
                    transferOrderLine.PurchaseUnit = GetAttributeValue(documentLine, "UnitId");
                    transferOrderLine.QuantityShipped = Convert.ToDecimal(GetAttributeValue(documentLine, "QtyShipNow"), CultureInfo.InvariantCulture);
                    transferOrderLine.QuantityShipNow = 0;
                    transferOrderLine.QuantityRemainShip = Convert.ToDecimal(GetAttributeValue(documentLine, "QtyRemainShip"), CultureInfo.InvariantCulture);
                    transferOrderLine.QuantityRemainReceive = Convert.ToDecimal(GetAttributeValue(documentLine, "QtyRemainReceive"), CultureInfo.InvariantCulture);
                    transferOrderLine.InventBatchId = GetAttributeValue(documentLine, "InventBatchId");
                    transferOrderLine.WMSLocationId = GetAttributeValue(documentLine, "WmsLocationId");
                    transferOrderLine.WMSPalletId = GetAttributeValue(documentLine, "WmsPalletId");
                    transferOrderLine.InventSiteId = GetAttributeValue(documentLine, "InventSiteId");
                    transferOrderLine.InventLocationId = GetAttributeValue(documentLine, "InventLocationId");
                    transferOrderLine.InventSizeId = GetAttributeValue(documentLine, "InventSizeId");
                    transferOrderLine.InventColorId = GetAttributeValue(documentLine, "InventColorId");
                    transferOrderLine.InventStyleId = GetAttributeValue(documentLine, "InventStyleId");
                    transferOrderLine.InventSerialId = GetAttributeValue(documentLine, "InventSerialId");
                    transferOrderLine.ConfigId = GetAttributeValue(documentLine, "ConfigId");
                    transferOrderLine.IsCommitted = Convert.ToBoolean(GetAttributeValue(documentLine, "UpdatedInAx"));
                    transferOrderLine.Message = GetAttributeValue(documentLine, "Message");
                    transferOrderLine.Guid = GetAttributeValue(documentLine, "Guid");
                    transferOrderLine.DeliveryMethod = GetAttributeValue(documentLine, "DlvMode");
                }
    
                return transferOrderLine;
            }
    
            private static string GetAttributeValue(XElement xe, string attributeName)
            {
                string result = string.Empty;
    
                XAttribute attribute = xe.Attribute(attributeName);
                if (attribute != null)
                {
                    result = attribute.Value;
                }
    
                return result;
            }
        }
    }
}
