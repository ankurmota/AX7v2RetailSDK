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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Trigger that performs direct operation access checks.
        /// </summary>
        public class OperationAccessCheckTrigger : IRequestTrigger
        {
            private static readonly Dictionary<Type, RetailOperation> RequestTypeToOperationMap = new Dictionary<Type, RetailOperation>
                { 
                     // Workflow requests
                    { typeof(AddOrRemoveDiscountCodesRequest), RetailOperation.DiscountCodeBarcode },
                    { typeof(ChangedProductsSearchRequest), RetailOperation.ViewProductDetails },
                    { typeof(CreateCustomerRequest), RetailOperation.CustomerAdd },
                    { typeof(CustomersSearchRequest), RetailOperation.CustomerSearch },
                    { typeof(DeactivateDeviceRequest), RetailOperation.DeactivateDevice },
                    { typeof(DeleteStockCountRequest), RetailOperation.StockCount },
                    { typeof(GetInvoiceRealtimeResponse), RetailOperation.SalesInvoice },
                    { typeof(GetStockCountRequest), RetailOperation.StockCount },
                    { typeof(GetStoreProductAvailabilityRequest), RetailOperation.InventoryLookup },
                    { typeof(IssueLoyaltyCardRequest), RetailOperation.LoyaltyIssueCard },
                    { typeof(PickAndPackOrderRequest), RetailOperation.EditCustomerOrder },
                    { typeof(PriceCheckRequest), RetailOperation.PriceCheck },
                    { typeof(RecalculateOrderRequest), RetailOperation.RecalculateCustomerOrder },
                    { typeof(RecallCustomerOrderRequest), RetailOperation.RecallSalesOrder },
                    { typeof(RecallSalesInvoiceRequest), RetailOperation.SalesInvoice },
                    { typeof(ResumeCartRequest), RetailOperation.RecallTransaction },
                    { typeof(SaveKitTransactionRequest), RetailOperation.KitDisassembly },
                    { typeof(SaveStockCountRequest), RetailOperation.StockCount },
                    { typeof(SaveVoidTransactionRequest), RetailOperation.VoidTransaction },
                    { typeof(SuspendCartRequest), RetailOperation.SuspendTransaction },
                    { typeof(SyncStockCountRequest), RetailOperation.StockCount },
                    { typeof(UpdateCustomerRequest), RetailOperation.CustomerEdit },
                    { typeof(UnenrollUserCredentialRequest), RetailOperation.ExtendedLogOn },
                    { typeof(EnrollUserCredentialRequest), RetailOperation.ExtendedLogOn },

                    // Service requests
                    { typeof(ActivateDeviceServiceRequest), RetailOperation.ActivateDevice },
                    { typeof(UserResetPasswordServiceRequest), RetailOperation.ResetPassword },
                    { typeof(CreateHardwareStationTokenServiceRequest), RetailOperation.PairHardwareStation },
                    { typeof(CreateStockCountJournalServiceRequest), RetailOperation.StockCount },
                    { typeof(GetGiftCardServiceRequest), RetailOperation.GiftCardBalance },
                    { typeof(IssueGiftCardServiceRequest), RetailOperation.IssueGiftCertificate },
                    { typeof(AddToGiftCardServiceRequest), RetailOperation.AddToGiftCard },
                    { typeof(VoidPaymentServiceRequest), RetailOperation.VoidPayment },
                    { typeof(GetChangePaymentServiceRequest), RetailOperation.ChangeBack },
                    { typeof(VoidGiftCardServiceRequest), RetailOperation.VoidItem },
                    { typeof(GetPurchaseHistoryServiceRequest), RetailOperation.CustomerTransactions },
                    { typeof(GetOrderHistoryServiceRequest), RetailOperation.CustomerTransactions },                                        
                    
                    // Realtime requests
                    { typeof(GetPickingListRealtimeRequest), RetailOperation.PickingAndReceiving },
                    { typeof(GetPurchaseOrderRealtimeRequest), RetailOperation.PickingAndReceiving },
                    { typeof(SavePickingListRealtimeRequest), RetailOperation.PickingAndReceiving },
                    { typeof(SavePurchaseOrderRealtimeRequest), RetailOperation.PickingAndReceiving },
                    { typeof(GetTransferOrderRealtimeRequest), RetailOperation.PickingAndReceiving },
                    { typeof(SaveTransferOrderRealtimeRequest), RetailOperation.PickingAndReceiving },
                    { typeof(UserChangePasswordRealtimeRequest), RetailOperation.ChangePassword },

                    // Data requests
                    { typeof(GetOrderHistoryDataRequest), RetailOperation.CustomerTransactions },
                    { typeof(GetReportDataServiceRequest), RetailOperation.ViewReport },
                    { typeof(GetSupportedReportsDataRequest), RetailOperation.ViewReport },
                 };

            /// <summary>
            /// Gets the collection of request types supported by this trigger.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return RequestTypeToOperationMap.Keys;
                }
            }

            /// <summary>
            /// Invoked before request has been processed by <see cref="IRequestHandler"/>.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            public void OnExecuting(Request request)
            {
                ThrowIf.Null(request, "request");

                Type requestType = request.GetType();
                RetailOperation operation = RequestTypeToOperationMap[requestType];
                var checkAccessRequest = new CheckAccessServiceRequest(operation);
                request.RequestContext.Execute<NullResponse>(checkAccessRequest);
            }

            /// <summary>
            /// Invoked after request has been processed by <see cref="IRequestHandler"/>.
            /// </summary>
            /// <param name="request">The request message processed by handler.</param>
            /// <param name="response">The response message generated by handler.</param>
            public void OnExecuted(Request request, Response response)
            {
            }
        }
    }
}