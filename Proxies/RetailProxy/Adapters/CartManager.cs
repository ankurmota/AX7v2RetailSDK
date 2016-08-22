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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        internal class CartManager : ICartManager
        {
            public Task<Cart> Create(Cart entity)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).CreateOrTransferCart(entity));
            }
    
            public Task<Cart> Read(string id)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetCart(id));
            }
    
            public Task<PagedResult<Cart>> ReadAll(QueryResultSettings queryResultSettings)
            {
                throw new NotSupportedException();
            }
    
            public Task<Cart> Update(Cart entity)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).CreateOrUpdateCart(entity));
            }
    
            public Task Delete(Cart entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<SalesOrder> Checkout(string id, string receiptEmail, TokenizedPaymentCard tokenizedPaymentCard, string receiptNumberSequence, IEnumerable<CartTenderLine> cartTenderLines)
            {
                if (cartTenderLines != null)
                {
                    return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).Checkout(id, receiptEmail, cartTenderLines));
                }
                else if (tokenizedPaymentCard != null)
                {
                    return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).Checkout(id, receiptEmail, tokenizedPaymentCard, receiptNumberSequence));
                }
                else
                {
                    return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).Checkout(id, receiptEmail, receiptNumberSequence));
                }
            }

            public Task<PagedResult<SalesLineDeliveryOption>> GetLineDeliveryOptions(string id, System.Collections.Generic.IEnumerable<LineShippingAddress> lineShippingAddresses, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetLineDeliveryOptions(id, lineShippingAddresses, queryResultSettings));
            }
    
            public Task<PagedResult<TenderLine>> GetPaymentsHistory(string id, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetPaymentsHistory(id, queryResultSettings));
            }
    
            public Task<SalesOrder> Void(string id, System.Collections.Generic.IEnumerable<ReasonCodeLine> reasonCodeLines)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).VoidCart(id, reasonCodeLines));
            }
    
            public Task<Cart> AddCartLines(string id, System.Collections.Generic.IEnumerable<CartLine> cartLines)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).AddCartLines(id, cartLines));
            }
    
            public Task<Cart> UpdateCartLines(string id, System.Collections.Generic.IEnumerable<CartLine> cartLines)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).UpdateCartLines(id, cartLines));
            }
    
            public Task<Cart> VoidCartLines(string id, System.Collections.Generic.IEnumerable<CartLine> cartLines)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).VoidCartLines(id, cartLines));
            }
    
            public Task ValidateTenderLineForAdd(string id, TenderLine tenderLine)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).ValidateTenderLineForAdd(id, tenderLine));
            }
    
            public Task<Cart> AddTenderLine(string id, CartTenderLine cartTenderLine)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).AddTenderLine(id, cartTenderLine));
            }
    
            public Task<Cart> AddPreprocessedTenderLine(string id, TenderLine preprocessedTenderLine)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).AddTenderLine(id, preprocessedTenderLine));
            }
    
            public Task<Cart> UpdateTenderLineSignature(string id, string tenderLineId, string signatureData)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).UpdateSignatureOnTenderLine(id, tenderLineId, signatureData));
            }
    
            public Task<Cart> VoidTenderLine(string id, string tenderLineId, System.Collections.Generic.IEnumerable<ReasonCodeLine> reasonCodeLines, bool? isPreprocessed = false)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).VoidTenderLine(id, tenderLineId, reasonCodeLines, (bool)isPreprocessed));
            }
    
            public Task<Cart> Copy(string id, int targetCartType)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).CopyCart(id, (CartType)targetCartType));
            }
    
            public Task<Cart> IssueGiftCard(string id, string giftCardId, decimal amount, string currencyCode, string lineDescription)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).IssueGiftCard(id, giftCardId, amount, currencyCode, lineDescription));
            }
    
            public Task<Cart> RefillGiftCard(string id, string giftCardId, decimal amount, string currencyCode, string lineDescription)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).AddToGiftCard(id, giftCardId, amount, currencyCode, lineDescription));
            }
    
            public Task<Cart> Suspend(string id)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).SuspendCart(id));
            }
    
            public Task<Cart> Resume(string id)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).ResumeCart(id));
            }
    
            public Task<Cart> RemoveDiscountCodes(string id, IEnumerable<string> discountCodes)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).RemoveDiscountCodesFromCart(id, discountCodes));
            }
    
            public Task<PagedResult<Cart>> GetSuspended(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetSuspendedCarts(queryResultSettings));
            }
    
            public Task<Cart> OverrideCartLinePrice(string id, string cartLineId, decimal price)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).OverridePrice(id, cartLineId, price, CalculationModes.Discounts | CalculationModes.Taxes | CalculationModes.AmountDue | CalculationModes.Deposit | CalculationModes.Totals));
            }
    
            public Task<Cart> RecallOrder(string transactionId, string salesId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).RecallCustomerOrder(transactionId, salesId));
            }
    
            public Task<Cart> RecallQuote(string transactionId, string quoteId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).RecallCustomerQuote(transactionId, quoteId));
            }
    
            public Task<Cart> RecalculateOrder(string id)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).RecalculateCustomerOrder(id));
            }
    
            public Task<CartPromotions> GetPromotions(string id)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetPromotions(id));
            }
    
            public Task<Cart> RecallSalesInvoice(string transactionId, string invoiceId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).RecallSalesInvoice(transactionId, invoiceId));
            }
    
            public Task<Cart> AddDiscountCode(string id, string discountCode)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).AddDiscountCodesToCart(id, new[] { discountCode }.ToObservableCollection()));
            }
    
            public Task<Cart> RemoveCartLines(string id, System.Collections.Generic.IEnumerable<string> cartLineIds)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).DeleteCartLines(id, cartLineIds));
            }
    
            public Task<CartDeliveryPreferences> GetDeliveryPreferences(string id)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetDeliveryPreferences(id));
            }
    
            public Task<PagedResult<DeliveryOption>> GetDeliveryOptions(string id, Address shippingAddress, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetDeliveryOptions(id, shippingAddress, queryResultSettings));
            }
    
            public Task<Cart> UpdateLineDeliverySpecifications(string id, System.Collections.Generic.IEnumerable<LineDeliverySpecification> lineDeliverySpecifications)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).UpdateLineDeliverySpecifications(id, lineDeliverySpecifications));
            }
    
            public Task<Cart> UpdateDeliverySpecification(string id, DeliverySpecification deliverySpecification)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).UpdateDeliverySpecification(id, deliverySpecification));
            }
    
            public Task<PagedResult<Cart>> Search(CartSearchCriteria cartSearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).SearchCarts(cartSearchCriteria, queryResultSettings));
            }
    
            public Task<CardPaymentAcceptPoint> GetCardPaymentAcceptPoint(string id, CardPaymentAcceptSettings cardPaymentAcceptSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetCardPaymentAcceptPoint(id, cardPaymentAcceptSettings));
            }
    
            public Task<CardPaymentAcceptResult> RetrieveCardPaymentAcceptResult(string resultAccessCode)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).RetrieveCardPaymentAcceptResult(resultAccessCode));
            }
        }
    }
}
