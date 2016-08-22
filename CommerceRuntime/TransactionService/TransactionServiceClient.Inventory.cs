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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using CRT = Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            /// <summary>
            /// The default number of picking and receiving documents.
            /// </summary>
            private const int PickReceiveDocumentLength = 1;
    
            // Transaction service client method names.
            private const string InventoryLookupMethodName = "InventoryLookup";
            private const string GetPurchaseOrdersMethodName = "GetOpenPurchaseOrders";
            private const string GetTransferOrdersMethodName = "GetOpenTransferOrders";
            private const string GetPurchaseOrderMethodName = "GetPurchaseOrder";
            private const string GetTransferOrderMethodName = "GetTransferOrder";
            private const string UpdatePurchaseOrderMethodName = "UpdatePurchaseOrder";
            private const string UpdateTransferOrderMethodName = "UpdateTransferOrder";
            private const string UpdatePickingListMethodName = "UpdatePickingList";
            private const string GetPickingListsMethodName = "GetPickingLists";
            private const string GetPickingListMethodName = "GetPickingList";
    
            /// <summary>
            /// Gets the inventory lookup.
            /// </summary>
            /// <param name="itemId">The item identifier.</param>
            /// <param name="variantId">The variant identifier.</param>
            /// <returns>A collection of inventory information.</returns>
            public ReadOnlyCollection<InventoryInfo> InventoryLookup(string itemId, string variantId)
            {
                ThrowIf.Null<string>(itemId, "itemId");
                ThrowIf.Null<string>(variantId, "variantId");
    
                var data = this.InvokeMethodAllowNullResponse(InventoryLookupMethodName, itemId, variantId);
    
                // Parse response data
                List<InventoryInfo> results = new List<InventoryInfo>();
    
                const int InventoryInfoLength = 4;
    
                for (int currentStartIndex = 0; currentStartIndex + InventoryInfoLength <= data.Count; currentStartIndex += InventoryInfoLength)
                {
                    InventoryInfo inventory = new InventoryInfo();
                    inventory.ItemId = (string)data[currentStartIndex];
                    inventory.InventoryLocationId = (string)data[currentStartIndex + 1];
                    inventory.StoreName = (string)data[currentStartIndex + 2];
                    inventory.InventoryAvailable = Convert.ToString(data[currentStartIndex + 3]);
    
                    results.Add(inventory);
                }
    
                return new ReadOnlyCollection<InventoryInfo>(results);
            }
    
            /// <summary>
            /// Updates the returned quantity of the retail transaction sales for each returned item.
            /// </summary>
            /// <param name="returnedItems">The returned items.</param>
            public void MarkItemsReturned(ItemReturn[] returnedItems)
            {
                ThrowIf.NullOrEmpty(returnedItems, "returnedItems");
    
                // Prepare parameter list
                string returnedItemXml = SerializationHelper.SerializeObjectToXml(returnedItems, typeof(ItemReturn[]));
    
                // Invoke
                this.InvokeMethodNoDataReturn(MarkItemsReturnedMethodName, new object[] { returnedItemXml });
            }
    
            /// <summary>
            /// Get open purchase orders for a store.
            /// </summary>
            /// <param name="storeNumber">The store number.</param>
            /// <returns>A collection of receive documents.</returns>
            public PagedResult<PickReceiveDocument> GetPurchaseOrders(string storeNumber)
            {
                ThrowIf.Null<string>(storeNumber, "storeNumber");
    
                var data = this.InvokeMethod(GetPurchaseOrdersMethodName, storeNumber);
    
                // Parse response data
                List<PickReceiveDocument> results = new List<PickReceiveDocument>();
    
                for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                {
                    PickReceiveDocument pickReceiveDocument = new PickReceiveDocument();
                    pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
    
                    results.Add(pickReceiveDocument);
                }
    
                return results.AsPagedResult();
            }
    
            /// <summary>
            /// Gets open transfer orders from Ax.
            /// </summary>
            /// <param name="storeNumber">The store number.</param>
            /// <returns>A collection of picking documents.</returns>
            public ReadOnlyCollection<PickReceiveDocument> GetTransferOrders(string storeNumber)
            {
                ThrowIf.Null<string>(storeNumber, "terminalId");
    
                var data = this.InvokeMethod(GetTransferOrdersMethodName, storeNumber);
    
                // Parse response data
                List<PickReceiveDocument> results = new List<PickReceiveDocument>();
    
                for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                {
                    PickReceiveDocument pickReceiveDocument = new PickReceiveDocument();
                    pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
    
                    results.Add(pickReceiveDocument);
                }
    
                return new ReadOnlyCollection<PickReceiveDocument>(results);
            }
    
            /// <summary>
            /// Gets picking lists from Ax.
            /// </summary>
            /// <param name="storeNumber">The store number.</param>
            /// <returns>A collection of picking documents.</returns>
            public ReadOnlyCollection<PickReceiveDocument> GetPickingLists(string storeNumber)
            {
                ThrowIf.Null<string>(storeNumber, "terminalId");
    
                var data = this.InvokeMethod(GetPickingListsMethodName, storeNumber);
    
                // Parse response data
                try
                {
                    List<PickReceiveDocument> results = new List<PickReceiveDocument>();
    
                    for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                    {
                        PickReceiveDocument pickReceiveDocument = new PickReceiveDocument();
                        pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
    
                        results.Add(pickReceiveDocument);
                    }
    
                    return new ReadOnlyCollection<PickReceiveDocument>(results);
                }
                catch (Exception ex)
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        ex,
                        string.Format("Unable to parse service response data: {0}", data));
                }
            }
    
            /// <summary>
            /// Gets a purchase order with lines from Ax.
            /// </summary>
            /// <param name="orderId">The order identifier.</param>
            /// <param name="storeNumber">The store number.</param>
            /// <returns>A receiving document.</returns>
            public PickReceiveDocument GetPurchaseOrder(string orderId, string storeNumber)
            {
                ThrowIf.Null<string>(storeNumber, "storeNumber");
                ThrowIf.Null<string>(orderId, "orderId");
    
                var data = this.InvokeMethod(GetPurchaseOrderMethodName, orderId, storeNumber);
    
                // Parse response data
                PickReceiveDocument pickReceiveDocument = null;
    
                for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                {
                    pickReceiveDocument = new PickReceiveDocument();
                    pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
                }
    
                return pickReceiveDocument;
            }
    
            /// <summary>
            /// Gets a picking list with lines from Ax.
            /// </summary>
            /// <param name="orderId">The order identifier.</param>
            /// <param name="storeNumber">The store number.</param>
            /// <returns>A receiving document.</returns>
            public PickReceiveDocument GetPickingList(string orderId, string storeNumber)
            {
                ThrowIf.Null<string>(storeNumber, "storeNumber");
                ThrowIf.Null<string>(orderId, "orderId");
    
                var data = this.InvokeMethod(GetPickingListMethodName, orderId, storeNumber);
    
                // Parse response data
                try
                {
                    PickReceiveDocument pickReceiveDocument = null;
    
                    for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                    {
                        pickReceiveDocument = new PickReceiveDocument();
                        pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
                    }
    
                    return pickReceiveDocument;
                }
                catch (Exception ex)
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        ex,
                        string.Format("Unable to parse service response data: {0}", data));
                }
            }
    
            /// <summary>
            /// Gets a transfer order with lines from Ax.
            /// </summary>
            /// <param name="orderId">The order identifier.</param>
            /// <returns>A picking document.</returns>
            public PickReceiveDocument GetTransferOrder(string orderId)
            {
                ThrowIf.Null<string>(orderId, "orderId");
    
                var data = this.InvokeMethod(GetTransferOrderMethodName, orderId);
    
                // Parse response data
                PickReceiveDocument pickReceiveDocument = null;
    
                for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                {
                    pickReceiveDocument = new PickReceiveDocument();
                    pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
                }
    
                return pickReceiveDocument;
            }
    
            /// <summary>
            /// Updates a purchase order in AX.
            /// </summary>
            /// <param name="orderXML">The order as an xml document.</param>
            /// <returns>The saved order as a document.</returns>
            public ReadOnlyCollection<PickReceiveDocument> UpdatePurchaseOrder(string orderXML)
            {
                ThrowIf.Null<string>(orderXML, "orderXML");
    
                ReadOnlyCollection<object> data = this.InvokeMethod(UpdatePurchaseOrderMethodName, orderXML);
    
                // Parse response data
                List<PickReceiveDocument> results = new List<PickReceiveDocument>();
    
                for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                {
                    PickReceiveDocument pickReceiveDocument = new PickReceiveDocument();
                    pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
    
                    results.Add(pickReceiveDocument);
                }
    
                return new ReadOnlyCollection<PickReceiveDocument>(results);
            }
    
            /// <summary>
            /// Updates a picking list in Ax.
            /// </summary>
            /// <param name="orderXML">The order as an xml document.</param>
            /// <returns>The saved order as a document.</returns>
            public ReadOnlyCollection<PickReceiveDocument> UpdatePickingList(string orderXML)
            {
                ThrowIf.Null<string>(orderXML, "orderXML");
    
                var data = this.InvokeMethod(UpdatePickingListMethodName, orderXML);
    
                // Parse response data
                try
                {
                    List<PickReceiveDocument> results = new List<PickReceiveDocument>();
    
                    for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                    {
                        PickReceiveDocument pickReceiveDocument = new PickReceiveDocument();
                        pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
    
                        results.Add(pickReceiveDocument);
                    }
    
                    return new ReadOnlyCollection<PickReceiveDocument>(results);
                }
                catch (Exception ex)
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        ex,
                        string.Format("Unable to parse service response data: {0}", data));
                }
            }
    
            /// <summary>
            /// Updates a transfer order in Ax.
            /// </summary>
            /// <param name="orderXML">The order as an xml document.</param>
            /// <returns>The saved order as a document.</returns>
            public ReadOnlyCollection<PickReceiveDocument> UpdateTransferOrder(string orderXML)
            {
                ThrowIf.Null<string>(orderXML, "orderXML");
    
                var data = this.InvokeMethod(UpdateTransferOrderMethodName, orderXML);
    
                // Parse response data
                List<PickReceiveDocument> results = new List<PickReceiveDocument>();
    
                for (int currentStartIndex = 0; currentStartIndex + PickReceiveDocumentLength <= data.Count; currentStartIndex += PickReceiveDocumentLength)
                {
                    PickReceiveDocument pickReceiveDocument = new PickReceiveDocument();
                    pickReceiveDocument.XMLDocument = (string)data[currentStartIndex];
    
                    results.Add(pickReceiveDocument);
                }
    
                return new ReadOnlyCollection<PickReceiveDocument>(results);
            }
        }
    }
}
