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
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Helper class for customer order logic.
        /// </summary>
        public static class CustomerOrderWorkflowHelper
        {
            private const string PaymentCardTokenParameter = "CustomerOrderPaymentCardTokenParameter";
            private const string PreviousCustomerOrderModeParamter = "CustomerOrderPreviousCustomerOrderMode";
    
            /// <summary>
            /// Validates a customer order cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cart">The cart received on the request.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            /// <param name="returnedSalesTransaction">The returned sales transaction to be validated.</param>
            /// <param name="validationResults">The validation results to add validation errors to.</param>
            public static void CustomerOrderCartValidations(RequestContext context, Cart cart, SalesTransaction salesTransaction, SalesTransaction returnedSalesTransaction, CartLineValidationResults validationResults)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(validationResults, "validationResults");
    
                if (cart.CartType != CartType.CustomerOrder &&
                    salesTransaction.CartType != CartType.CustomerOrder)
                {
                    // Do not validate if not customer order
                    return;
                }
    
                // Validate mode changes
                CustomerOrderWorkflowHelper.ValidateCustomerOrderModeOnCart(cart, salesTransaction);
    
                // Keep previous customer order mode
                context.SetProperty(PreviousCustomerOrderModeParamter, salesTransaction.CustomerOrderMode);
    
                // Mode specific validations
                CustomerOrderMode customerOrderMode = cart.CustomerOrderMode != CustomerOrderMode.None
                                                          ? cart.CustomerOrderMode
                                                          : salesTransaction.CustomerOrderMode;
    
                // Validate permissions for the required operation
                CustomerOrderWorkflowHelper.ValidateCartPermissions(context, cart, salesTransaction, customerOrderMode);
    
                // Common header validations
                CustomerOrderWorkflowHelper.ValidateCartHeaderUpdate(cart, salesTransaction, validationResults);
    
                switch (customerOrderMode)
                {
                    case CustomerOrderMode.QuoteCreateOrEdit:
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                        CustomerOrderWorkflowHelper.ValidateCartForCreationOrEdition(
                            context,
                            cart,
                            salesTransaction,
                            returnedSalesTransaction,
                            validationResults);
                        break;
    
                    case CustomerOrderMode.OrderRecalled:
                        // This happens when recalling the order and saving the cart locally
                        break;
    
                    case CustomerOrderMode.Cancellation:
                        CustomerOrderWorkflowHelper.ValidateCartForCancellation(cart, salesTransaction, validationResults);
                        break;
    
                    case CustomerOrderMode.Pickup:
                        CustomerOrderWorkflowHelper.ValidateCartForPickup(cart, salesTransaction, context.GetChannelConfiguration(), validationResults);
                        break;
    
                    case CustomerOrderMode.Return:
                        break;
                }
            }
    
            /// <summary>
            /// Validates whether a line can be added.
            /// </summary>
            /// <param name="cartLine">The cart line.</param>
            /// <param name="cart">The cart.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            public static void ValidateCartLineForAdd(CartLine cartLine, Cart cart, SalesTransaction salesTransaction, Collection<DataValidationFailure> validationFailures)
            {
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(cartLine, "cartLine");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(validationFailures, "validationFailures");
    
                CustomerOrderWorkflowHelper.ValidateCartLineIsProduct(cartLine, validationFailures);
    
                CustomerOrderMode orderMode = cart.CustomerOrderMode != CustomerOrderMode.None ? cart.CustomerOrderMode : salesTransaction.CustomerOrderMode;
    
                CustomerOrderWorkflowHelper.ValidateCustomerOrderModeForCartLineAdd(orderMode, validationFailures);
            }
    
            /// <summary>
            /// Validates whether a line can be updated.
            /// </summary>
            /// <param name="context">The Request context.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            /// <param name="existingTransaction">The existing sales transaction from the DB.</param>
            /// <param name="salesLineByLineId">The dictionary of sales lines by line id.</param>
            /// <param name="cartLine">The cart line.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ByLine", Justification = "Not Hungarian notation.")]
            public static void ValidateCartLineForUpdate(
                RequestContext context,
                Cart newCart,
                SalesTransaction existingTransaction,
                Dictionary<string, SalesLine> salesLineByLineId,
                CartLine cartLine,
                Collection<DataValidationFailure> validationFailures)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(newCart, "newCart");
                ThrowIf.Null(existingTransaction, "existingTransaction");
                ThrowIf.Null(salesLineByLineId, "salesLineByLineId");
                ThrowIf.Null(cartLine, "cartLine");
                ThrowIf.Null(validationFailures, "validationFailures");
    
                CustomerOrderMode customerOrderMode = newCart.CustomerOrderMode != CustomerOrderMode.None
                                                          ? newCart.CustomerOrderMode
                                                          : existingTransaction.CustomerOrderMode;

                SalesLine salesLineForUpdate = salesLineByLineId[cartLine.LineId];
                CartWorkflowHelper.ValidateQuantityChangeAllowed(context, salesLineForUpdate, cartLine, validationFailures);

                // Check that cart is not changing customer order properties restricted to create/edit modes
                if (customerOrderMode != CustomerOrderMode.QuoteCreateOrEdit &&
                    customerOrderMode != CustomerOrderMode.CustomerOrderCreateOrEdit)
                {
                    CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                        salesLineForUpdate,
                        cartLine.LineData,
                        validationFailures,
                        CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.FulfillmentStoreId),
                        CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.DeliveryMode),
                        CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.RequestedDeliveryDate),
                        CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.DeliveryModeChargeAmount),
                        CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.ShippingAddress));
    
                    if (cartLine.IsVoided)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCustomerOrderModeForVoidProducts,
                                "Cart line can be voided must only at the time of CustomerOrderCreateOrEdit or QuoteCreateOrEdit.");
    
                        validationFailures.Add(validationFailure);
                        return;
                    }
                }

                decimal newQuantity = cartLine.LineData.Quantity;
                switch (customerOrderMode)
                {
                    case CustomerOrderMode.Return:
                        if (newQuantity > 0)
                        {
                            DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReturnsMustHaveQuantityLesserThanZero,
                                "Cart line quantity must be non-negative for returns.");
    
                            validationFailures.Add(validationFailure);
                        }
                        else if (Math.Abs(newQuantity) > salesLineForUpdate.QuantityInvoiced)
                        {
                            // When returning a product, we cannot let the customer return more than what was invoiced
                            DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotReturnMoreThanPurchased,
                                "When returning a product, it is not allowed to return more than what was invoiced.");
    
                            validationFailures.Add(validationFailure);
                        }
    
                        // this values are populated when recalling a cart for return and should not be changed by the client
                        CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                            salesLineForUpdate,
                            cartLine.LineData,
                            validationFailures,
                            CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.ReturnInventoryTransactionId),
                            CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.ReturnLineNumber),
                            CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.ReturnTransactionId));
                        break;
    
                    case CustomerOrderMode.Pickup:
                        // when picking up a product, we cannot let the customer pickup products that were not selected for pickup
                        string pickupDeliveryMode = context.GetChannelConfiguration().PickupDeliveryModeCode ?? string.Empty;
                        bool isPickup = pickupDeliveryMode.Equals(salesLineForUpdate.DeliveryMode, StringComparison.OrdinalIgnoreCase);
    
                        if (isPickup)
                        {
                            // when picking up a product, we cannot let the customer take more than is left to be picked up
                            CustomerOrderWorkflowHelper.ValidateCartLineQuantityForPickup(salesLineForUpdate, newQuantity, validationFailures);
                        }
                        else
                        {
                            // Do not let updates happen on non-pickup line
                            CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                                salesLineForUpdate,
                                cartLine.LineData,
                                validationFailures,
                                CommerceEntityExtensions.GetColumnName<CartLineData>(c => c.Quantity));
                        }
    
                        break;
    
                    case CustomerOrderMode.Cancellation:
                        // on cancellation, we cannot change line quantities
                        if (newQuantity != salesLineForUpdate.Quantity)
                        {
                            DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineUpdate,
                                "Cannot change line quantities when cancelling the order.");
    
                            validationFailures.Add(validationFailure);
                        }
    
                        break;
    
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                    case CustomerOrderMode.OrderRecalled:
                    case CustomerOrderMode.QuoteCreateOrEdit:
    
                        CustomerOrderWorkflowHelper.ValidateDeliveryLinePermissions(context, existingTransaction, cartLine, salesLineByLineId[cartLine.LineId]);
    
                        // we can only have line quantity 0 for pickups
                        if (newQuantity <= 0M)
                        {
                            DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineUpdate,
                                "Line quantity must be greater than 0.");
    
                            validationFailures.Add(validationFailure);
                        }
    
                        CustomerOrderWorkflowHelper.ValidateCartLineDeliveryDate(context, cartLine, validationFailures);
    
                        break;
    
                    default:
                        throw new NotSupportedException(customerOrderMode + " customer order mode is not supported.");
                }
            }
    
            /// <summary>
            /// Updates customer order related fields at the time of save cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to be updated.</param>
            /// <param name="cart">The cart received on the request.</param>
            public static void UpdateCustomerOrderFieldsOnSave(RequestContext context, SalesTransaction salesTransaction, Cart cart)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // only make changes if it's a customer order
                if (salesTransaction.CartType != CartType.CustomerOrder)
                {
                    return;
                }
    
                CustomerOrderMode previousCustomerOrderMode = (CustomerOrderMode)context.GetProperty(PreviousCustomerOrderModeParamter);
                CustomerOrderMode customerOrderMode = cart.CustomerOrderMode;
    
                // Update transaction type
                salesTransaction.TransactionType = SalesTransactionType.CustomerOrder;
    
                // if cart has new mode let's update sales transaction
                if (customerOrderMode != CustomerOrderMode.None)
                {
                    salesTransaction.CustomerOrderMode = customerOrderMode;
                }
                else
                {
                    // cart didn't changed the mode, take mode from the sales transaction
                    customerOrderMode = salesTransaction.CustomerOrderMode;
                }
    
                // Deposit override
                CustomerOrderWorkflowHelper.UpdateDepositOverride(cart, salesTransaction, previousCustomerOrderMode);
    
                switch (customerOrderMode)
                {
                    // only update shipping charges if editing/creating customer order or quote
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                    case CustomerOrderMode.QuoteCreateOrEdit:
                        // Update shipping charges
                        CustomerOrderWorkflowHelper.UpdateShippingChargeCodes(context, salesTransaction);
                        break;
    
                    case CustomerOrderMode.Cancellation:
                        // Update cancellation charge
                        CustomerOrderWorkflowHelper.UpdateCancellationCharge(context, salesTransaction);
                        break;
    
                    case CustomerOrderMode.Pickup:
                        // Set non pickup line's quantity to zero
                        CustomerOrderWorkflowHelper.UpdateLinesOnPickup(context.GetChannelConfiguration(), salesTransaction);
                        break;
                }
    
                CustomerOrderWorkflowHelper.UnlockPricesOnExpiredQuote(context, salesTransaction);
            }
    
            /// <summary>
            /// Updates the customer order fields on checkout.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            public static void UpdateCustomerOrderFieldsOnCheckout(SalesTransaction salesTransaction)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                CustomerOrderWorkflowHelper.UpdateDeliveryDates(salesTransaction);
            }
    
            /// <summary>
            /// Calculates the deposit and updates the sales transaction deposit amount.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>        
            public static void CalculateDeposit(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                CalculateDepositServiceRequest depositRequest = new CalculateDepositServiceRequest(salesTransaction);
                context.Execute<CalculateDepositServiceResponse>(depositRequest);
    
                CustomerOrderWorkflowHelper.CalculateEstimatedShippingAuthorizationAmount(context, salesTransaction);
            }
    
            /// <summary>
            /// Validates a sales transaction for customer order creation and update.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>        
            public static void ValidateOrderAndQuoteCreationAndUpdate(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                List<DataValidationFailure> validationFailures = new List<DataValidationFailure>();
    
                if (salesTransaction.CustomerOrderMode != CustomerOrderMode.CustomerOrderCreateOrEdit && // mode must be CreateOrEdit for customer order or quote
                    salesTransaction.CustomerOrderMode != CustomerOrderMode.QuoteCreateOrEdit)
                {
                    validationFailures.Add(new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                        "Invalid customer order mode '{0}' for this operation. Only creation/update of customer order and quote are valid.",
                        salesTransaction.CustomerOrderMode));
                }
    
                try
                {
                    // General order validations
                    OrderWorkflowHelper.ValidateContextForCreateOrder(context, salesTransaction);
                }
                catch (DataValidationException ex)
                {
                    if (ex.ValidationResults.Any())
                    {
                        validationFailures.AddRange(ex.ValidationResults);
                    }
                    else
                    {
                        DataValidationErrors error;
                        validationFailures.Add(Enum.TryParse(ex.ErrorResourceId, out error)
                            ? new DataValidationFailure(error, ex.Message)
                            : new DataValidationFailure(DataValidationErrors.None, ex.Message));
                    }
                }
    
                // Checkout and payment validations
                CustomerOrderWorkflowHelper.ValidateOrderForCheckout(context, salesTransaction, validationFailures, true);
    
                // Quote validations
                if (salesTransaction.CustomerOrderMode == CustomerOrderMode.QuoteCreateOrEdit)
                {
                    CustomerOrderWorkflowHelper.ValidateQuoteCreationOrEdition(salesTransaction, validationFailures);
                }
    
                // Delivery mode, date and address validations
                CustomerOrderWorkflowHelper.ValidateOrderDelivery(context, salesTransaction, validationFailures);
    
                // if we have validation errors, throw an aggregate exception containing them
                if (validationFailures.Count > 0)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError,
                        validationFailures,
                        "Failures when validation customer order (or quote) for creation or edition.");
                }
            }
    
            /// <summary>
            /// Validates the order for pick up.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            public static void ValidateOrderForPickup(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                List<DataValidationFailure> validationFailures = new List<DataValidationFailure>();
    
                if (salesTransaction.CustomerOrderMode != CustomerOrderMode.Pickup)
                {
                    validationFailures.Add(new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                        "Invalid customer order mode '{0}' for this operation. Only customer order pick up is valid.",
                        salesTransaction.CustomerOrderMode));
                }
    
                CustomerOrderWorkflowHelper.ValidateOrderForCheckout(context, salesTransaction, validationFailures);
    
                // Get lines for pick up
                IEnumerable<SalesLine> pickupSalesLines = salesTransaction.ActiveSalesLines.Where(salesLine => salesLine.Quantity > 0);
    
                if (!pickupSalesLines.Any())
                {
                    validationFailures.Add(new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SalesLineMustHavePickupDeliveryMode,
                        "At least one pick up line must be present in the transaction with quantity greater than 0."));
                }
    
                // Get current store and pickup delivery mode
                string pickupDeliveryMode = context.GetChannelConfiguration().PickupDeliveryModeCode ?? string.Empty;
    
                IList<CartLine> cartLines = pickupSalesLines.Select(line => new CartLine()
                {
                    LineData =
                    {
                        ProductId = line.ProductId
                    }
                }).ToList();
                Dictionary<long, SimpleProduct> productsDictionary = CartWorkflowHelper.GetProductsInCartLines(context, cartLines);
    
                // Validate that lines are set for pick up at store
                foreach (SalesLine salesLine in pickupSalesLines)
                {
                    if (productsDictionary[salesLine.ProductId].Behavior.HasSerialNumber && string.IsNullOrWhiteSpace(salesLine.SerialNumber))
                    {
                        validationFailures.Add(new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SerialNumberMissing,
                            "Serial number for item {0} is missing",
                            salesLine.ItemId));
                    }
    
                    CustomerOrderWorkflowHelper.ValidatePickupCartLinesHavePickupDeliveryMode(salesLine, salesLine.Quantity, pickupDeliveryMode, validationFailures);
                    CustomerOrderWorkflowHelper.ValidateCartLineFulfillmentStoreNumberForPickup(context.GetOrgUnit(), salesLine, validationFailures);
                }
    
                if (validationFailures.Any())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError,
                        validationFailures,
                        "Failures when validation customer order for pick up.");
                }
            }
    
            /// <summary>
            /// Validates the order for cancellation.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            public static void ValidateOrderForCancellation(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                List<DataValidationFailure> validationFailures = new List<DataValidationFailure>();
    
                if (salesTransaction.CustomerOrderMode != CustomerOrderMode.Cancellation)
                {
                    validationFailures.Add(new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                        "Invalid customer order mode '{0}' for this operation. Only customer order cancellation is valid.",
                        salesTransaction.CustomerOrderMode));
                }
    
                CustomerOrderWorkflowHelper.ValidateOrderForCheckout(context, salesTransaction, validationFailures);
    
                // if we have validation errors, throw an aggregate exception containing them
                if (validationFailures.Any())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError,
                        validationFailures,
                        "Failures when validation customer order cancellation.");
                }
            }
    
            /// <summary>
            /// Validates the order for return.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            public static void ValidateOrderForReturn(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                List<DataValidationFailure> validationFailures = new List<DataValidationFailure>();
    
                CustomerOrderWorkflowHelper.ValidateOrderForCheckout(context, salesTransaction, validationFailures);
    
                if (salesTransaction.CustomerOrderMode != CustomerOrderMode.Return)
                {
                    validationFailures.Add(new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCustomerOrderModeForReturnTransaction,
                        "Invalid customer order mode '{0}' for this operation. Only customer order return is valid.",
                        salesTransaction.CustomerOrderMode));
                }
    
                GetReturnOrderReasonCodesDataRequest getReturnOrderReasonCodesDataRequest = new GetReturnOrderReasonCodesDataRequest(QueryResultSettings.AllRecords);
                ReadOnlyCollection<ReasonCode> returnOrderReasonCodes = context.Runtime.Execute<EntityDataServiceResponse<ReasonCode>>(getReturnOrderReasonCodesDataRequest, context).PagedEntityCollection.Results;
    
                // check if there are sales lines that don't have a return reason code set
                // only consider lines with quantity < 0, because lines with 0 are not being returned
                if (returnOrderReasonCodes.Count > 0)
                {
                    if (salesTransaction.ActiveSalesLines.Any(salesLine => salesLine.Quantity < 0
                        && !returnOrderReasonCodes.Any(returnOrderReasonCode => salesLine.ReasonCodeLines.Any(reasonCodeLine => reasonCodeLine.ReasonCodeId == returnOrderReasonCode.ReasonCodeId))))
                    {
                        validationFailures.Add(new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredReasonCodesMissing,
                            "All sales line must have a reason code for the return transaction."));
                    }
                }
    
                // if we have validation errors, throw an aggregate exception containing them
                if (validationFailures.Any())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError,
                        validationFailures,
                        "Failures when validation customer order return.");
                }
            }
    
            /// <summary>
            /// Fills missing requirements for the order.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            public static void FillMissingRequirementsForOrder(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // Add common missing dependencies to the sales transaction
                OrderWorkflowHelper.FillMissingRequirementsForOrder(context, salesTransaction);
    
                var itemVariantIds = new Collection<ItemVariantInventoryDimension>();
                var salesLinesWithVariants = new Collection<SalesLine>();
                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    // Populate variant information for sales lines
                    if (!string.IsNullOrWhiteSpace(salesLine.InventoryDimensionId) &&
                        (salesLine.Variant == null
                        || string.IsNullOrWhiteSpace(salesLine.Variant.InventoryDimensionId)
                        || string.IsNullOrEmpty(salesLine.Variant.VariantId)))
                    {
                        var itemVariantId = new ItemVariantInventoryDimension(salesLine.ItemId, salesLine.InventoryDimensionId);
                        itemVariantIds.Add(itemVariantId);
    
                        salesLinesWithVariants.Add(salesLine);
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
                foreach (SalesLine salesLine in salesLinesWithVariants)
                {
                    ProductVariant variant;
                    var itemVariantId = new ItemVariantInventoryDimension(salesLine.ItemId, salesLine.InventoryDimensionId);
                    if (variantsMap.TryGetValue(itemVariantId, out variant))
                    {
                        salesLine.Variant = variant;
                    }
                }
            }
    
            /// <summary>
            /// Handles payments for customer orders, it includes:
            /// * Capturing deposit.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <returns>Updated sales transaction.</returns>
            public static SalesTransaction HandlePayments(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                
                // Process payments
                return OrderWorkflowHelper.ProcessCheckoutPayments(context, salesTransaction);
            }
    
            /// <summary>
            /// Handles payments for customer orders, it includes:
            /// * Capturing deposit using the token passed for shipped items.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="tokenizedPaymentCard">The tokenized card used in the headquarters to fulfill the shipped portion of this order.</param>
            /// <returns>Updated sales transaction.</returns>
            public static SalesTransaction HandlePayments(RequestContext context, SalesTransaction salesTransaction, TokenizedPaymentCard tokenizedPaymentCard)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(tokenizedPaymentCard, "tokenizedPaymentCard");
    
                TenderLine tenderLine = new TenderLine
                {
                    Amount = 0,
                    Currency = context.GetChannelConfiguration().Currency,
                    TenderTypeId = tokenizedPaymentCard.TenderType,
                    CardTypeId = tokenizedPaymentCard.CardTypeId,
                    MaskedCardNumber = tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber,
                    CardToken = tokenizedPaymentCard.CardTokenInfo.CardToken,
                };
    
                // Save token
                context.SetProperty(PaymentCardTokenParameter, tenderLine);
    
                // Process payments
                return OrderWorkflowHelper.ProcessCheckoutPayments(context, salesTransaction);
            }
    
            /// <summary>
            /// Saves a customer order using transaction service.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <returns>The sales order representing the saved customer order.</returns>
            public static SalesOrder SaveCustomerOrder(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                if (salesTransaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit)
                {
                    // Create a new customer order, all payments are deposit.
                    foreach (TenderLine tenderLine in salesTransaction.TenderLines)
                    {
                        tenderLine.IsDeposit = true;
                    }
                }

                TenderLine tokenTenderLine = context.GetProperty(PaymentCardTokenParameter) as TenderLine;
                if (tokenTenderLine != null)
                {
                    salesTransaction.TenderLines.Add(tokenTenderLine);
                }
    
                bool isOffline = false;
    
                // Find out if we need to support offline customer orders.
                if (string.IsNullOrWhiteSpace(salesTransaction.SalesId) &&
                    (salesTransaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit ||
                     salesTransaction.CustomerOrderMode == CustomerOrderMode.QuoteCreateOrEdit))
                {
                    DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();
    
                    isOffline = deviceConfiguration.CreateOfflineCustomerOrders;
                }
    
                if (isOffline)
                {
                    // Yes it is an offline customer order/quote
                    salesTransaction.TransactionType = salesTransaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit
                                                        ? SalesTransactionType.AsyncCustomerOrder
                                                        : SalesTransactionType.AsyncCustomerQuote;
                }
                else
                {
                    // A customer order is posted through TS in online mode.
                    salesTransaction.EntryStatus = TransactionStatus.Posted;
                    foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                    {
                        // change line status to Posted if they do not have one yet
                        if (salesLine.Status == TransactionStatus.Normal)
                        {
                            salesLine.Status = TransactionStatus.Posted;
                        }
                    }
    
                    SaveCustomerOrderRealtimeRequest saveCustomerOrderRequest = new SaveCustomerOrderRealtimeRequest(
                        salesTransaction,
                        context.GetChannelConfiguration(),
                        cardTokenInfo: null,
                        cardAuthorizationTokenResponseXml: null);
    
                    context.Execute<Response>(saveCustomerOrderRequest);
    
                    // We want to save cart in two situations:
                    // 1) Order was saved successfully - we save the cart so we can provide the workflow caller with updated cart information
                    // 2) If transaction service fails due to timeout, AX could have saved the order but we won't know if it succeeded on AX or not
                    // we need to save the order locally so in case of retry we use the same receipt id
                    CartWorkflowHelper.SaveSalesTransaction(context, salesTransaction);
                    salesTransaction = CartWorkflowHelper.LoadSalesTransaction(context, salesTransaction.Id);
                }
    
                // Save order locally for End of day flow support
                SalesOrder salesOrder = OrderWorkflowHelper.CreateSalesOrder(context, salesTransaction);
                salesOrder.SalesId = salesTransaction.SalesId;
                salesOrder.CustomerOrderType = salesTransaction.CustomerOrderType;
    
                return salesOrder;
            }
    
            /// <summary>
            /// Gets the calculation modes based on the operation being performed.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <returns>The filtered calculation modes.</returns>
            public static CalculationModes GetCalculationModes(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // Get calculation modes from service
                return context.Execute<GetCustomerOrderCalculationModesServiceResponse>(new GetCustomerOrderCalculationModesServiceRequest(salesTransaction)).CalculationModes;
            }
    
            /// <summary>
            /// Populates the sales transaction.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="cartId">The cart id.</param>
            /// <param name="email">The email.</param>
            /// <returns>Loaded transaction.</returns>
            public static SalesTransaction GetSalesTransaction(RequestContext context, string cartId, string email)
            {
                ThrowIf.Null(context, "context");
    
                SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(context, cartId);
    
                if (salesTransaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, cartId);
                }
    
                if (!string.IsNullOrEmpty(email))
                {
                    salesTransaction.ReceiptEmail = email;
                }
    
                OrderWorkflowHelper.FillTransactionWithContextData(context, salesTransaction);
    
                return salesTransaction;
            }
    
            /// <summary>
            /// Saves the order as a transaction and converts it to a cart.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="order">The sales order.</param>
            /// <returns>The converted cart.</returns>
            public static Cart SaveTransactionAndConvertToCart(RequestContext context, SalesOrder order)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(order, "order");
    
                SalesTransaction transaction = order;
    
                // Set transaction default values
                if (string.IsNullOrWhiteSpace(transaction.Id))
                {
                    transaction.Id = CartWorkflowHelper.GenerateRandomTransactionId(context);
                }
    
                // Channel and terminal don't come from ax
                transaction.ChannelId = context.GetPrincipal().ChannelId;
    
                // Lock prices on all lines
                foreach (SalesLine salesLine in transaction.SalesLines)
                {
                    salesLine.IsPriceLocked = true; //DEMO4 //TODO: Uncomment this for real implementation
                }
    
                transaction.TerminalId = context.GetTerminal().TerminalId;
    
                // Perform order calculations (deposit, amount due, etc)
                CartWorkflowHelper.Calculate(context, transaction, requestedMode: null);
    
                // Save order as a local transaction
                context.Runtime.Execute<NullResponse>(new SaveCartDataRequest(new[] { transaction }), context);
    
                // Convert the SalesOrder into a cart object for the client
                Cart cart = CartWorkflowHelper.ConvertToCart(context, order);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
    
                return cart;
            }
    
            /// <summary>
            /// Fills cart with address information that is not present on the request (cart).
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction being operated on.</param>
            /// <param name="cart">The cart on the request.</param>
            /// <param name="salesLineByLineId">A dictionary whose key is the sales line id and the value is the sales line.</param>
            /// <remarks>When we receive a cart request the Shipping address might be incomplete, so it needs to be fetched from the DB based on its record id.
            /// This method will compare the existing address on the transaction header / lines with the correspondent address on the cart header / lines. Only when
            /// the address is changed, added or remove that it will fetch the addresses from the DB.</remarks>
            public static void FillAddressInformation(RequestContext context, SalesTransaction salesTransaction, Cart cart, Dictionary<string, SalesLine> salesLineByLineId)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(salesLineByLineId, "salesLineByLineId");
    
                Collection<long> addressRecordIds = new Collection<long>();
    
                // only run code on customer order context
                if (salesTransaction.CartType != CartType.CustomerOrder && cart.CartType != CartType.CustomerOrder)
                {
                    return;
                }
    
                foreach (CartLine cartLine in cart.CartLines)
                {
                    SalesLine salesLine;
                    Address existingAddress = salesLineByLineId.TryGetValue(cartLine.LineId, out salesLine)
                        ? salesLine.ShippingAddress
                        : null;
    
                    if (HasShippingAddressChanged(cartLine.ShippingAddress, existingAddress))
                    {
                        // keep the address record id so we can fetch it from DB
                        addressRecordIds.Add(cartLine.ShippingAddress.RecordId);
                    }
                }
    
                bool mustUpdateHeaderAddress = HasShippingAddressChanged(cart.ShippingAddress, salesTransaction.ShippingAddress);
    
                if (mustUpdateHeaderAddress)
                {
                    addressRecordIds.Add(cart.ShippingAddress.RecordId);
                }
    
                Address address;
    
                var dataServiceRequest = new GetAddressesDataRequest(addressRecordIds);
                EntityDataServiceResponse<Address> dataServiceResponse = context.Runtime.Execute<EntityDataServiceResponse<Address>>(dataServiceRequest, context);
                IReadOnlyCollection<Address> addresses = dataServiceResponse.PagedEntityCollection.Results;
                Dictionary<long, Address> addressByRecordId = addresses.ToDictionary(a => a.RecordId);
    
                // update header address
                if (mustUpdateHeaderAddress)
                {
                    if (addressByRecordId.TryGetValue(cart.ShippingAddress.RecordId, out address))
                    {
                        cart.ShippingAddress.CopyFrom(address);
                    }
                }
                else if (cart.ShippingAddress != null)
                {
                    // we don't want to update the address
                    cart.ShippingAddress.CopyFrom(salesTransaction.ShippingAddress ?? new Address());
                }
    
                // update line addresses
                foreach (CartLine cartLine in cart.CartLines)
                {
                    SalesLine salesLine;
                    Address existingAddress = salesLineByLineId.TryGetValue(cartLine.LineId, out salesLine)
                        ? salesLine.ShippingAddress
                        : null;
    
                    if (HasShippingAddressChanged(cartLine.ShippingAddress, existingAddress))
                    {
                        if (addressByRecordId.TryGetValue(cartLine.ShippingAddress.RecordId, out address))
                        {
                            cartLine.LineData.ShippingAddress.CopyFrom(address);
                        }
                    }
                    else if (existingAddress != null && existingAddress.RecordId != 0 &&
                        cartLine.ShippingAddress != null && cartLine.ShippingAddress.RecordId == 0)
                    {
                        // explicitly remove address from cart line.
                        cartLine.LineData.ShippingAddress = null;
                    }
    
                    // Ignore other shipping address update requests. List of known ignored updates:
                    // 1. Existing address and updated address has the same RecordId, but other properties (e.g. Street number) are different.
                    // 2. Updated ShippingAddress property is null from client call. This condition will be treated with no updates. Client should
                    //    instantiate empty ShippingAddress if they want to clear the shipping address.
                    // 3. Updating null or empty existing address with null or empty updated address.
                }
            }

            /// <summary>
            /// Populate receipt identifier from transaction if the property is null or empty.
            /// Receipt id will be mandatory when doing payment by credit memo.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="receiptNumberSequence">The receipt number sequence.</param>
            /// <returns>SalesTransaction entity that has receipt identifier.</returns>
            public static SalesTransaction FillInReceiptId(RequestContext context, SalesTransaction salesTransaction, string receiptNumberSequence)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");

                bool transactionHasReceiptId = !string.IsNullOrWhiteSpace(salesTransaction.ReceiptId);

                // Fill in receipt id if transaction does not have it.
                if (!transactionHasReceiptId)
                {
                    // Get the receipt Id based on the mask defined.
                    OrderWorkflowHelper.FillInReceiptId(context, salesTransaction, receiptNumberSequence);
                }

                if (string.IsNullOrWhiteSpace(salesTransaction.ChannelReferenceId))
                {
                    salesTransaction.ChannelReferenceId = salesTransaction.ReceiptId;
                }

                return salesTransaction;
            }
    
            /// <summary>
            /// Rounding helper method for rounding currency amounts.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="amount">The amount to be rounded.</param>
            /// <returns>The rounded amount.</returns>
            private static decimal RoundCurrencyAmount(RequestContext context, decimal amount)
            {
                GetRoundedValueServiceRequest request = new GetRoundedValueServiceRequest(
                    amount,
                    context.GetChannelConfiguration().Currency,
                    numberOfDecimals: 0,
                    useSalesRounding: false);
    
                GetRoundedValueServiceResponse response = context.Execute<GetRoundedValueServiceResponse>(request);
    
                return response.RoundedValue;
            }
    
            /// <summary>
            /// Updates the delivery dates.
            /// </summary>
            /// <remarks>We set header's requested delivery date to all lines, that have empty delivery dates, then we set minimum delivery date to the header.
            /// Client set's delivery dates only on line-level, and here we need to take care of header's delivery date.</remarks>
            /// <param name="salesTransaction">The sales transaction.</param>
            private static void UpdateDeliveryDates(SalesTransaction salesTransaction)
            {
                if (!salesTransaction.RequestedDeliveryDate.HasValue)
                {
                    return;
                }
    
                DateTimeOffset? minDeliveryDate = null;
    
                foreach (var salesLine in salesTransaction.ActiveSalesLines)
                {
                    // Copy delivery date from header to line, if one has not been set on the client.
                    if (!salesLine.RequestedDeliveryDate.HasValue)
                    {
                        salesLine.RequestedDeliveryDate = salesTransaction.RequestedDeliveryDate;
                    }
    
                    // Find the minimum delivery date
                    if (!minDeliveryDate.HasValue || salesLine.RequestedDeliveryDate.Value < minDeliveryDate)
                    {
                        minDeliveryDate = salesLine.RequestedDeliveryDate.Value;
                    }
                }
    
                // Update header's delivery date, as it may not updated by clients.
                salesTransaction.RequestedDeliveryDate = minDeliveryDate;
            }
    
            /// <summary>
            /// Updates the deposit override value in the transaction.
            /// </summary>
            /// <param name="cart">The cart from the request.</param>
            /// <param name="salesTransaction">The transaction being updated.</param>
            /// <param name="previousCustomerOrderMode">The previous customer order mode.</param>
            private static void UpdateDepositOverride(Cart cart, SalesTransaction salesTransaction, CustomerOrderMode previousCustomerOrderMode)
            {
                /*
                 * This methods does:
                 * 1. Apply deposit override amount from cart to transaction
                 * 2. For pick up operation:
                 *  2.1. Applies the full available deposit if operation is picking up all remaining items (for pick up)
                 *  2.2. If not case 2.1. and we are converting a recalled order to a pickup, we need to zero the amount of overridden deposit
                 *       as the cashier must explicitly set how much deposit override should be applied for the operation
                 */
    
                if (salesTransaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit ||
                    salesTransaction.CustomerOrderMode == CustomerOrderMode.Pickup)
                {
                    if (cart.OverriddenDepositAmount.HasValue)
                    {
                        // only update deposit override if value is set on cart
                        salesTransaction.OverriddenDepositAmount = cart.OverriddenDepositAmount.Value;
                    }
    
                    if (salesTransaction.CustomerOrderMode == CustomerOrderMode.Pickup
                        && salesTransaction.IsDepositOverridden)
                    {
                        if (previousCustomerOrderMode == CustomerOrderMode.OrderRecalled)
                        {
                            // if we are converting a recalled order to a pick up operation
                            // and it was created using deposit override, then the cashier must manually provide how much deposit
                            // should be used towards this pick up operation
                            // thus we need to zero the deposit override amount
                            salesTransaction.OverriddenDepositAmount = decimal.Zero;
                        }
                        else
                        {
                            // if it's a full pick up and we don't have anything left to be shipped
                            // basically, after this operation, no items will be left for invoicing
                            if (CustomerOrderWorkflowHelper.AreAllItemsInvoiced(salesTransaction))
                            {
                                // we need to apply full available deposit
                                salesTransaction.OverriddenDepositAmount = salesTransaction.AvailableDepositAmount;
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// Determine if all items are invoiced on a transaction.
            /// </summary>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>True if all items are invoiced on this transaction otherwise false.</returns>
            private static bool AreAllItemsInvoiced(SalesTransaction transaction)
            {
                return transaction.ActiveSalesLines
                            .All(salesLine => salesLine.Quantity + salesLine.QuantityInvoiced == salesLine.QuantityOrdered);
            }
    
            /// <summary>
            /// Unlock prices for a customer quote that is expired.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            private static void UnlockPricesOnExpiredQuote(RequestContext context, SalesTransaction salesTransaction)
            {
                // Unlock prices on all lines if it's an expired quote
                if (salesTransaction.CustomerOrderType == CustomerOrderType.Quote
                    && salesTransaction.QuotationExpiryDate < context.GetNowInChannelTimeZone())
                {
                    foreach (SalesLine salesLine in salesTransaction.SalesLines)
                    {
                        salesLine.IsPriceLocked = false;
                    }
                }
            }
    
            /// <summary>
            /// Updates shipping charges.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            private static void UpdateShippingChargeCodes(RequestContext context, SalesTransaction salesTransaction)
            {
                bool isShippingChargeCodeSetOnChannel = !string.IsNullOrWhiteSpace(context.GetChannelConfiguration().ShippingChargeCode);
    
                bool isHeaderForPickup = !string.IsNullOrEmpty(salesTransaction.DeliveryMode) && salesTransaction.DeliveryMode.Equals(context.GetChannelConfiguration().PickupDeliveryModeCode, StringComparison.OrdinalIgnoreCase);
    
                if (isShippingChargeCodeSetOnChannel)
                {
                    // retrive charge details
                    GetChargeLinesDataRequest getChargeDetailsDataRequest = new GetChargeLinesDataRequest(context.GetChannelConfiguration().ShippingChargeCode, ChargeModule.Sales, QueryResultSettings.AllRecords);
                    ChargeLine shippingChargeDetails = context.Execute<SingleEntityDataServiceResponse<ChargeLine>>(getChargeDetailsDataRequest).Entity;
    
                    // Populate header level charge information
                    decimal? deliveryCharge = salesTransaction.DeliveryModeChargeAmount;
    
                    CustomerOrderWorkflowHelper.UpdateShippingChargeCodes(
                            context.GetChannelConfiguration(),
                            shippingChargeDetails,
                            salesTransaction.ChargeLines,
                            ref deliveryCharge,
                            salesTransaction.DeliveryMode,
                            isHeaderForPickup,
                            string.Empty);
    
                    salesTransaction.DeliveryModeChargeAmount = deliveryCharge;
    
                    // Popuplate line level charge information
                    foreach (SalesLine salesLine in salesTransaction.SalesLines)
                    {
                        deliveryCharge = salesLine.DeliveryModeChargeAmount;
    
                        CustomerOrderWorkflowHelper.UpdateShippingChargeCodes(
                            context.GetChannelConfiguration(),
                            shippingChargeDetails,
                            salesLine.ChargeLines,
                            ref deliveryCharge,
                            salesLine.DeliveryMode,
                            isHeaderForPickup,
                            salesLine.SalesOrderUnitOfMeasure);
    
                        salesLine.DeliveryModeChargeAmount = deliveryCharge;
                    }
                }
                else
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ChargeNotConfiguredOnHeadquarters,
                        "Shipping charge code was not configured on the headquarters. It's not possible to add shipping charges to the order.");
                }
            }
    
            /// <summary>
            /// Updates shipping charges.
            /// </summary>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <param name="shippingChargeDetails">The shipping charge details line.</param>
            /// <param name="chargeLines">The charge line collection.</param>        
            /// <param name="deliveryChargeAmount">The delivery charge used.</param>        
            /// <param name="deliveryMode">The delivery mode used.</param>        
            /// <param name="isHeaderForPickup">A value indicating whether the sales transaction header delivery mode is set to pick up.</param>        
            /// <param name="salesUnitOfMeasure">The sales line unit of measure.</param>
            private static void UpdateShippingChargeCodes(
                ChannelConfiguration channelConfiguration,
                ChargeLine shippingChargeDetails,
                IList<ChargeLine> chargeLines,
                ref decimal? deliveryChargeAmount,
                string deliveryMode,
                bool isHeaderForPickup,
                string salesUnitOfMeasure)
            {
                // use header definition if line does not provide one
                bool isForPickup = string.IsNullOrEmpty(deliveryMode)
                    ? isHeaderForPickup
                    : deliveryMode.Equals(channelConfiguration.PickupDeliveryModeCode, StringComparison.OrdinalIgnoreCase);
    
                // if this is a pickup line, we must not charge for delivery
                if (isForPickup && deliveryChargeAmount.HasValue)
                {
                    deliveryChargeAmount = decimal.Zero;
                }
    
                // Update charge lines based on charge amount
                CustomerOrderWorkflowHelper.UpdateChargeLines(
                    deliveryChargeAmount,
                    channelConfiguration.Currency,
                    salesUnitOfMeasure,
                    shippingChargeDetails,
                    chargeLines);
            }
    
            /// <summary>
            /// Updates cancellation charges.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            private static void UpdateCancellationCharge(RequestContext context, SalesTransaction salesTransaction)
            {
                bool isCancellationChargeCodeSetOnChannel = !string.IsNullOrWhiteSpace(context.GetChannelConfiguration().CancellationChargeCode);
    
                if (isCancellationChargeCodeSetOnChannel)
                {
                    CustomerOrderMode previousCustomerOrderMode = (CustomerOrderMode)context.GetProperty(PreviousCustomerOrderModeParamter);
    
                    // If we just converted the cart to cancellation
                    if (previousCustomerOrderMode == CustomerOrderMode.OrderRecalled)
                    {
                        // We need to add default cancellation charges
                        salesTransaction.CancellationCharge = CustomerOrderWorkflowHelper.RoundCurrencyAmount(
                            context,
                            context.GetChannelConfiguration().CancellationChargePercentage / 100M * salesTransaction.TotalAmount);
                    }
    
                    // Retrive charge details
                    GetChargeLinesDataRequest getChargeDetailsDataRequest = new GetChargeLinesDataRequest(context.GetChannelConfiguration().CancellationChargeCode, ChargeModule.Sales, QueryResultSettings.AllRecords);
                    ChargeLine cancellationChargeDetails = context.Execute<SingleEntityDataServiceResponse<ChargeLine>>(getChargeDetailsDataRequest).Entity;
    
                    // Update charge lines based on charge amount
                    CustomerOrderWorkflowHelper.UpdateChargeLines(
                        salesTransaction.CancellationCharge,
                        context.GetChannelConfiguration().Currency,
                        string.Empty,
                        cancellationChargeDetails,
                        salesTransaction.ChargeLines);
                }
                else
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ChargeNotConfiguredOnHeadquarters,
                        "Cancellation charge code was not configured on the headquarters. It's not possible to add cancellation charges to the order.");
                }
            }
    
            /// <summary>
            /// Updates charge lines with a specific charge.
            /// Creates a new charge line if charge does not exist on charge line collection, updates charge line if charge already exists on collection
            /// or remove charge from collection if amount is set to zero.
            /// </summary>
            /// <param name="chargeAmount">The charge amount.</param>
            /// <param name="currency">The currency used for the charge.</param>        
            /// <param name="salesUnitOfMeasure">The sales line unit of measure.</param>
            /// <param name="chargeDetails">The shipping charge details line.</param>
            /// <param name="chargeLines">The charge line collection.</param>        
            private static void UpdateChargeLines(
                decimal? chargeAmount,
                string currency,
                string salesUnitOfMeasure,
                ChargeLine chargeDetails,
                IList<ChargeLine> chargeLines)
            {
                if (chargeAmount.HasValue)
                {
                    ChargeLine shippingCharge = chargeLines.SingleOrDefault(
                        charge => charge.ChargeCode.Equals(chargeDetails.ChargeCode, StringComparison.OrdinalIgnoreCase));
    
                    // Charge already exists on collection
                    if (shippingCharge != null)
                    {
                        // if charge amount was set to zero
                        if (chargeAmount.Value == decimal.Zero)
                        {
                            // we need to remove charge
                            chargeLines.Remove(shippingCharge);
                        }
                        else
                        {
                            // otherwise, just update charge amount
                            shippingCharge.NetAmount = chargeAmount.Value;
                            shippingCharge.CalculatedAmount = chargeAmount.Value;
                            shippingCharge.NetAmountPerUnit = shippingCharge.NetAmount / shippingCharge.Quantity;
    
                            // setting fields below required for "tax inclusive" tax amount calculation
                            shippingCharge.GrossAmount = chargeAmount.Value;
                            shippingCharge.NetAmountWithAllInclusiveTax = shippingCharge.GrossAmount;
                        }
                    }
                    else if (chargeAmount.Value != decimal.Zero)
                    {
                        // charge is not there - let's create it
                        chargeLines.Add(
                            new ChargeLine()
                            {
                                ChargeCode = chargeDetails.ChargeCode,
                                CalculatedAmount = chargeAmount.Value,
                                NetAmount = chargeAmount.Value,
                                NetAmountPerUnit = chargeAmount.Value,
                                ModuleType = chargeDetails.ModuleType,
                                CurrencyCode = currency,
                                ChargeMethod = ChargeMethod.Fixed,
                                ChargeType = ChargeType.ManualCharge,
                                ItemTaxGroupId = chargeDetails.ItemTaxGroupId,
                                SalesOrderUnitOfMeasure = salesUnitOfMeasure,
                                Quantity = 1,
                                GrossAmount = chargeAmount.Value,
                                NetAmountWithAllInclusiveTax = chargeAmount.Value
                            });
                    }
                }
            }
    
            /// <summary>
            /// Update non-pickup sales lines to have quantity 0.
            /// </summary>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            private static void UpdateLinesOnPickup(ChannelConfiguration channelConfiguration, SalesTransaction salesTransaction)
            {
                string pickupDeliveryMode = channelConfiguration.PickupDeliveryModeCode ?? string.Empty;
    
                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    // if not pick up line
                    if (!pickupDeliveryMode.Equals(salesLine.DeliveryMode, StringComparison.OrdinalIgnoreCase))
                    {
                        salesLine.Quantity = decimal.Zero;
                    }
                }
            }
    
            #region Cart Validations
    
            /// <summary>
            /// Validates sales person changes.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="updatedCart">Proposed cart to be updated.</param>
            /// <param name="originalTransaction">The original transaction.</param>
            private static void ValidateSalesPerson(RequestContext context, Cart updatedCart, SalesTransaction originalTransaction)
            {
                // if staff id is not set on the cart, the client does not want to change it
                if (!updatedCart.IsPropertyDefined(cart => cart.StaffId))
                {
                    return;
                }
    
                if (!string.IsNullOrWhiteSpace(originalTransaction.StaffId) && string.IsNullOrWhiteSpace(updatedCart.StaffId))
                {
                    // user tries to remove sales person.
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SalespersonClear));
                }
                else if (!string.Equals(originalTransaction.StaffId, updatedCart.StaffId, StringComparison.OrdinalIgnoreCase))
                {
                    // user tries to modify sales person.
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SalesPerson));
                }
            }
    
            /// <summary>
            /// Validates a customer order transaction for creation or edition.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cart">The cart received on the request.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            /// <param name="returnedSalesTransaction">The returned sales transaction to be validated.</param>
            /// <param name="validationResults">The validation results to add validation errors to.</param>
            private static void ValidateCartForCreationOrEdition(RequestContext context, Cart cart, SalesTransaction salesTransaction, SalesTransaction returnedSalesTransaction, CartLineValidationResults validationResults)
            {
                // we are in creation mode if we don't have a sales id
                bool isCreation = string.IsNullOrWhiteSpace(salesTransaction.SalesId);
    
                CustomerOrderMode customerOrderMode = isCreation ? cart.CustomerOrderMode : (CustomerOrderMode)context.GetProperty(PreviousCustomerOrderModeParamter);
    
                if (returnedSalesTransaction != null)
                {
                    validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCustomerOrderModeForReturnTransaction));
                }
    
                if (!isCreation)
                {
                    // when editing a customer order, we cannot change the customer id
                    if (cart.CustomerId != null && cart.CustomerId != salesTransaction.CustomerId)
                    {
                        validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotChangeCustomerIdWhenEditingCustomerOrder));
                    }
                }
    
                // Specific validations for customer order and quote
                switch (customerOrderMode)
                {
                    case CustomerOrderMode.QuoteCreateOrEdit:
                        CustomerOrderWorkflowHelper.ValidateQuoteForCartCreationOrEdition(cart, validationResults);
                        break;
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                        CustomerOrderWorkflowHelper.ValidateCustomerOrderForCartCreationOrEdition(cart, salesTransaction, validationResults);
                        break;
                }
            }
    
            /// <summary>
            /// Validates customer order for creation or edition.
            /// </summary>
            /// <param name="cart">The cart received on the request.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            /// <param name="validationResults">The validation results to add validation errors to.</param>
            private static void ValidateCustomerOrderForCartCreationOrEdition(Cart cart, SalesTransaction salesTransaction, CartLineValidationResults validationResults)
            {
                if (cart.OverriddenDepositAmount.HasValue)
                {
                    decimal depositOverrideAmount = cart.OverriddenDepositAmount.Value;
    
                    if (depositOverrideAmount < 0M)
                    {
                        validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DepositMustBeGreaterThanZero, "Deposit override amount must not be less than 0 or greater than order total."));
                    }
                    else if (depositOverrideAmount > salesTransaction.TotalAmount)
                    {
                        validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DepositOverrideMustNotBeGreaterThanTotalAmount, "Deposit override amount must not be less than 0 or greater than order total."));
                    }
                }
            }
    
            /// <summary>
            /// Validates quote for creation or edition.
            /// </summary>
            /// <param name="cart">The cart received on the request.</param>
            /// <param name="validationResults">The validation results to add validation errors to.</param>
            private static void ValidateQuoteForCartCreationOrEdition(Cart cart, CartLineValidationResults validationResults)
            {
                if (cart.OverriddenDepositAmount.HasValue)
                {
                    // quotes cannot have deposit override
                    validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_QuoteMustNotHaveDepositOverride, "Deposit override is not allowed for quotes."));
                }

                // Validation for Quote Expiration Date
                DateTimeOffset minQuoteDate = DateTimeOffset.UtcNow.AddHours(-12);
                if (cart.QuotationExpiryDate.HasValue && cart.QuotationExpiryDate.Value < minQuoteDate)
                {
                    validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_QuoteMustHaveValidQuotationExpiryDate, "Quote must have a valid expiry date."));
                }
            }
    
            /// <summary>
            /// Validates customer order mode on cart.
            /// </summary>
            /// <param name="cart">The cart received on the request.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            private static void ValidateCustomerOrderModeOnCart(Cart cart, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // we should only update customer order mode if our cart type is CustomerOrder
                if (salesTransaction.CartType == CartType.CustomerOrder)
                {
                    CustomerOrderMode newCustomerOrderMode = cart.CustomerOrderMode;
                    bool isModeChangeValid;
    
                    if (newCustomerOrderMode != CustomerOrderMode.None && newCustomerOrderMode != salesTransaction.CustomerOrderMode)
                    {
                        // if client set some mode different than None and current sales transaction mode, we need to update our transaction
    
                        // depending on current mode, we need to deny the change
                        switch (salesTransaction.CustomerOrderMode)
                        {
                            // very first time transaction is being created
                            case CustomerOrderMode.None:
                                isModeChangeValid = newCustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit
                                    || newCustomerOrderMode == CustomerOrderMode.QuoteCreateOrEdit
                                    || newCustomerOrderMode == CustomerOrderMode.OrderRecalled;
                                break;
    
                            case CustomerOrderMode.OrderRecalled:
                                // whether the order has been (partially) invoiced or not
                                bool isOrderInvoiced = salesTransaction.ActiveSalesLines.Any(salesLine => salesLine.QuantityInvoiced > 0M);
    
                                if (isOrderInvoiced)
                                {
                                    // if order is (partially) invoiced, we can only return or pick up
                                    isModeChangeValid = newCustomerOrderMode == CustomerOrderMode.Return
                                        || newCustomerOrderMode == CustomerOrderMode.Pickup;
                                }
                                else
                                {
                                    // when recalling annot invoiced order we may go to any other state (but none)
                                    isModeChangeValid = newCustomerOrderMode != CustomerOrderMode.None;
                                }
    
                                break;
    
                            // when creating/editing quote, we can convert it to customer order as well
                            case CustomerOrderMode.QuoteCreateOrEdit:
                                isModeChangeValid = newCustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit;
                                break;
    
                            // for all other modes, they are final, we cannot change it once we get to them
                            case CustomerOrderMode.CustomerOrderCreateOrEdit:
                            case CustomerOrderMode.Cancellation:
                            case CustomerOrderMode.Pickup:
                            case CustomerOrderMode.Return:
                                isModeChangeValid = false;
                                break;
    
                            default:
                                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Customer order mode '{0}' is not supported.", newCustomerOrderMode));
                        }
                    }
                    else
                    {
                        // if we got here, it means the cart is not changing the customer order mode
                        // if sales transaction mode is recall and client is not chaning it
                        // them we cannot allow the request to proceed
                        isModeChangeValid = salesTransaction.CustomerOrderMode != CustomerOrderMode.OrderRecalled;
                    }
    
                    if (!isModeChangeValid)
                    {
                        string errorMessage = string.Format(
                            CultureInfo.InvariantCulture,
                            "It is not allowed to change a customer order mode from '{0}' to '{1}'.",
                            salesTransaction.CustomerOrderMode,
                            newCustomerOrderMode);
    
                        // if mode change is invalid, we shouldn't check any mode specific validations, so we need to throw here
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCustomerOrderModeChange, errorMessage);
                    }
                }
            }
    
            /// <summary>
            /// Validate customer order permissions when updating the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">Cart to be updated.</param>
            /// <param name="salesTransaction">Existing sales transaction.</param>
            /// <param name="customerOrderMode">The current customer order mode.</param>
            private static void ValidateCartPermissions(RequestContext context, Cart newCart, SalesTransaction salesTransaction, CustomerOrderMode customerOrderMode)
            {
                // if the mode has changed, then we need to check for permissions
                if (salesTransaction.CustomerOrderMode != customerOrderMode)
                {
                    RetailOperation operationBeingPerformed;
                    bool isEdition = !string.IsNullOrEmpty(salesTransaction.SalesId);
    
                    switch (customerOrderMode)
                    {
                        case CustomerOrderMode.CustomerOrderCreateOrEdit:
                            operationBeingPerformed = isEdition ? RetailOperation.EditCustomerOrder : RetailOperation.CreateCustomerOrder;
                            break;
    
                        case CustomerOrderMode.QuoteCreateOrEdit:
                            operationBeingPerformed = isEdition ? RetailOperation.EditQuotation : RetailOperation.CreateQuotation;
                            break;

                        // these modes don't have a specific operation defined, so they are consider as a update on the order
                        // a user can only pickup an order if he/she can recall it
                        case CustomerOrderMode.Pickup:
                        case CustomerOrderMode.Return:
                        case CustomerOrderMode.Cancellation:
                            operationBeingPerformed = RetailOperation.EditCustomerOrder;
                            break;
    
                        case CustomerOrderMode.OrderRecalled:
                            operationBeingPerformed = RetailOperation.RecallSalesOrder;
                            break;
    
                        default:
                            throw new NotSupportedException(string.Format("Permissions not supported for customer order mode '{0}'.", customerOrderMode));
                    }
    
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(operationBeingPerformed));
                }
    
                bool depositOverrideBeingChanged = newCart.IsPropertyDefined(x => x.OverriddenDepositAmount)
                    && newCart.OverriddenDepositAmount != salesTransaction.OverriddenDepositAmount;
    
                // if deposit override has changed, validate deposit override operation
                if (depositOverrideBeingChanged)
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.DepositOverride));
                }
    
                // validate sales person on this customer order.
                CustomerOrderWorkflowHelper.ValidateSalesPerson(context, newCart, salesTransaction);
    
                // validate header delivery constraints
                CustomerOrderWorkflowHelper.ValidateDeliveryHeaderPermissions(context, newCart, salesTransaction);
    
                // validate expiration date permissions for quote
                CustomerOrderWorkflowHelper.ValidateQuoteExpirationDatePermissions(context, newCart, salesTransaction);
            }
    
            /// <summary>
            /// Validate ship selected or pick up selected permissions on customer order.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">Existing sales transaction.</param>
            /// <param name="updatedCartLine">Cart line to be updated.</param>
            /// <param name="salesLine">Existing sales line.</param>
            private static void ValidateDeliveryLinePermissions(RequestContext context, SalesTransaction salesTransaction, CartLine updatedCartLine, SalesLine salesLine)
            {
                if (!string.IsNullOrWhiteSpace(salesTransaction.DeliveryMode))
                {
                    // This is pick up all or ship all request, skip line permission validations.
                    return;
                }
    
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                string pickupDeliveryModeCode = channelConfiguration.PickupDeliveryModeCode;
    
                // if delivery mode on cart line is changed and new delivery mode is not empty, then check retail operation
                // pick up selected or ship selected.
                if (!string.Equals(salesLine.DeliveryMode, updatedCartLine.DeliveryMode, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(updatedCartLine.DeliveryMode))
                {
                    if (string.Equals(pickupDeliveryModeCode, updatedCartLine.DeliveryMode, StringComparison.OrdinalIgnoreCase))
                    {
                        // user tries to update delivery mode on cart line to pick up
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.PickupSelectedProducts));
                    }
                    else
                    {
                        // user tries to update delivery mode on cart line to non pick up
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ShipSelectedProducts));
                    }
                }
            }
    
            /// <summary>
            /// Validate ship all or pick up all permissions on customer order.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">Cart to be updated.</param>
            /// <param name="salesTransaction">Existing sales transaction.</param>
            private static void ValidateDeliveryHeaderPermissions(RequestContext context, Cart newCart, SalesTransaction salesTransaction)
            {
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                string pickupDeliveryModeCode = channelConfiguration.PickupDeliveryModeCode;
    
                // if delivery mode on cart header is changed and new delivery mode is not empty, then check retail operation
                // pick up all or ship all.
                if (!string.Equals(salesTransaction.DeliveryMode, newCart.DeliveryMode, StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(newCart.DeliveryMode))
                {
                    if (string.Equals(pickupDeliveryModeCode, newCart.DeliveryMode, StringComparison.OrdinalIgnoreCase))
                    {
                        // user tries to update delivery mode on cart line to pick up all
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.PickupAllProducts));
                    }
                    else
                    {
                        // user tries to update delivery mode on cart line to ship all
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ShipAllProducts));
                    }
                }
            }
    
            /// <summary>
            /// Validates permission on quote expiration date changes.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">Cart to be updated.</param>
            /// <param name="salesTransaction">Existing sales transaction.</param>
            private static void ValidateQuoteExpirationDatePermissions(RequestContext context, Cart newCart, SalesTransaction salesTransaction)
            {
                if (newCart.QuotationExpiryDate.HasValue && (!salesTransaction.QuotationExpiryDate.HasValue 
                    || salesTransaction.QuotationExpiryDate.Value.CompareTo(newCart.QuotationExpiryDate.Value) != 0))
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SetQuotationExpirationDate));
                }
            }
    
            /// <summary>
            /// Validates updates on cart header.
            /// </summary>
            /// <param name="cart">The cart from the request.</param>
            /// <param name="salesTransaction">The sales transaction related to the request.</param>
            /// <param name="validationResults">The validation results.</param>
            private static void ValidateCartHeaderUpdate(Cart cart, SalesTransaction salesTransaction, CartLineValidationResults validationResults)
            {
                CustomerOrderMode customerOrderMode = cart.CustomerOrderMode != CustomerOrderMode.None
                                                          ? cart.CustomerOrderMode
                                                          : salesTransaction.CustomerOrderMode;
    
                switch (customerOrderMode)
                {
                    // On recall, we take whatever values AX provides us
                    case CustomerOrderMode.OrderRecalled:
                        return;
    
                    case CustomerOrderMode.Return:
                    case CustomerOrderMode.Cancellation:
                        CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                            salesTransaction,
                            cart,
                            validationResults,
                            RetailTransactionTableSchema.CustomerIdColumn,
                            RetailTransactionTableSchema.DeliveryModeColumn,
                            SalesTransaction.DeliveryModeChargeColumn,
                            SalesTransaction.ShippingAddressColumn,
                            SalesTransaction.QuoteExpiryDateColumn,
                            SalesTransaction.SalesIdColumn,
                            SalesTransaction.OverriddenDepositAmountColumn);
                        break;
    
                    case CustomerOrderMode.Pickup:
                        CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                            salesTransaction,
                            cart,
                            validationResults,
                            RetailTransactionTableSchema.CustomerIdColumn,
                            RetailTransactionTableSchema.DeliveryModeColumn,
                            SalesTransaction.DeliveryModeChargeColumn,
                            SalesTransaction.ShippingAddressColumn,
                            SalesTransaction.QuoteExpiryDateColumn,
                            SalesTransaction.SalesIdColumn);
                        break;
    
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                        // Properties that cannot be changed on customer order create or edit mode
                        CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                            salesTransaction,
                            cart,
                            validationResults,
                            SalesTransaction.QuoteExpiryDateColumn,
                            SalesTransaction.SalesIdColumn);
                        break;
    
                    case CustomerOrderMode.QuoteCreateOrEdit:
                        // no restrictions on quote edit or create mode
                        CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                            salesTransaction,
                            cart,
                            validationResults,
                            SalesTransaction.SalesIdColumn);
                        break;
    
                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Customer order mode {0} is not supported.", customerOrderMode));
                }
            }
    
            /// <summary>
            /// Validates a customer order transaction for pick up.
            /// </summary>
            /// <param name="cart">The cart received on the request.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="validationResults">The validation results to add validation errors to.</param>
            private static void ValidateCartForCancellation(Cart cart, SalesTransaction salesTransaction, CartLineValidationResults validationResults)
            {
                // Checks if cancellation charge is not negative
                if (cart.CancellationChargeAmount.HasValue && cart.CancellationChargeAmount < decimal.Zero)
                {
                    validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCancellationCharge, "Cancellation charge cannot be negative."));
                }
    
                if (salesTransaction.SalesLines.Any(salesLine => salesLine.QuantityInvoiced > 0))
                {
                    validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "It is not possible to cancel an order already invoiced."));
                }
            }
    
            /// <summary>
            /// Validates a customer order transaction for pick up.
            /// </summary>
            /// <param name="cart">The cart received on the request.</param>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <param name="validationResults">The validation results to add validation errors to.</param>
            private static void ValidateCartForPickup(
                Cart cart,
                SalesTransaction salesTransaction,
                ChannelConfiguration channelConfiguration,
                CartLineValidationResults validationResults)
            {
                CustomerOrderMode previousCustomerOrderMode = salesTransaction.CustomerOrderMode;
    
                // if we are setting some deposit override
                if (cart.OverriddenDepositAmount.HasValue)
                {
                    // Deposit override is not valid during a pickup of an order that was NOT previously a Deposit Override.
                    if (!salesTransaction.IsDepositOverridden)
                    {
                        string message = "Deposit override cannot be applied to a pick up operation for an order that had no original deposit override.";
                        validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_OrderWasNotCreatedWithDepositOverride, message));
                    }
    
                    // If we are converting the order from recall to pick up we don't need to validate overridden deposit amount
                    // as we will overwrite it to zero always
                    // If that's not the case, validate amount of overridden deposit used for pickup
                    if (previousCustomerOrderMode != CustomerOrderMode.OrderRecalled)
                    {
                        // we don't allow to change deposit if all items are going to be picked up
                        if ((cart.OverriddenDepositAmount != salesTransaction.OverriddenDepositAmount) && CustomerOrderWorkflowHelper.AreAllItemsInvoiced(salesTransaction))
                        {
                            string message = "Deposit override may not be changed on a pick up operation for an order that has all items invoiced.";
                            validationResults.AddLineResult(0, new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DepositOverrideMayNotBeChanged, message));
                        }
    
                        // when picking up, the deposit collected during creation is used as credit when calculating amount due for the item
                        // if we override the amount of deposit (credit) used during pick up, we cannot allow more deposit (credit) than what 
                        // we can only override more than available
                        if (cart.OverriddenDepositAmount > salesTransaction.AvailableDepositAmount)
                        {
                            DataValidationFailure failure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPickupDepositOverrideAmount,
                                "Deposit override for pickup cannot be greater than amount of deposit available.");
    
                            validationResults.AddLineResult(0, failure);
                        }
    
                        // gets by how much the deposit overriden amount is being changed
                        decimal depositIncreaseAmount = cart.OverriddenDepositAmount.Value - salesTransaction.OverriddenDepositAmount.GetValueOrDefault();
    
                        // checks that we don't let the cashier increase deposit applied beyond the value of the order
                        if (depositIncreaseAmount > salesTransaction.AmountDue)
                        {
                            DataValidationFailure failure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPickupDepositOverrideAmount,
                                "Deposit override for pickup cannot be greater than what the transaction is worth.");
    
                            validationResults.AddLineResult(0, failure);
                        }
                    }
                }
                else
                {
                    // a client explicitly sends null for the amount (wants to clear the override)
                    // we don't allow to clear deposit if it was applied originally
                    if (cart.IsPropertyDefined(x => x.OverriddenDepositAmount) && salesTransaction.IsDepositOverridden)
                    {
                        DataValidationFailure failure = new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DepositOverrideMayNotBeCleared,
                            "Deposit override may not be cleared on pick up operation for an order that has an original deposit override.");
    
                        validationResults.AddLineResult(0, failure);
                    }
                }
    
                string pickupDeliveryMode = channelConfiguration.PickupDeliveryModeCode ?? string.Empty;
    
                bool existsPickupLineOnTransaction = salesTransaction.ActiveSalesLines
                    .Any(salesLine => salesLine.Quantity > 0 &&
                        pickupDeliveryMode.Equals(salesLine.DeliveryMode, StringComparison.OrdinalIgnoreCase));
    
                if (cart.CartLines.All(cartLine => cartLine.Quantity == 0)
                    && !existsPickupLineOnTransaction)
                {
                    DataValidationFailure failure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SalesLineMustHavePickupDeliveryMode,
                        "At least one line must have quantities set.");
    
                    validationResults.AddLineResult(0, failure);
                }
            }
    
            /// <summary>
            /// Validates quote creation or edition.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction to be validated.</param>
            /// <param name="validationExceptions">The validation exception collection.</param>
            private static void ValidateQuoteCreationOrEdition(SalesTransaction salesTransaction, ICollection<DataValidationFailure> validationExceptions)
            {
                // must not have tender lines (only voided ones)
                if (salesTransaction.TenderLines.Any(tenderLine => tenderLine.Status != TenderLineStatus.Voided))
                {
                    validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_QuoteMustNotHaveAnyPayment, "Quote creation or edition must have no payments."));
                }
    
                // must not have deposit override set
                if (salesTransaction.IsDepositOverridden)
                {
                    validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_QuoteMustNotHaveDepositOverride, "Quote creation or edition must have no deposit override amount set."));
                }

                DateTimeOffset minQuoteDate = DateTimeOffset.UtcNow.AddHours(-12);
                if (!salesTransaction.QuotationExpiryDate.HasValue || salesTransaction.QuotationExpiryDate.Value < minQuoteDate)
                {
                    validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_QuoteMustHaveValidQuotationExpiryDate, "Quote must have a valid expiry date."));
                }
            }
    
            /// <summary>
            /// Validates order delivery information.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="validationExceptions">A collection of validation exceptions.</param>
            /// <remarks>InventLocationID is not validated for the header as it will be set as part of order creation workflow.</remarks>
            private static void ValidateOrderDelivery(RequestContext context, SalesTransaction salesTransaction, List<DataValidationFailure> validationExceptions)
            {
                string pickupDeliveryMode = context.GetChannelConfiguration().PickupDeliveryModeCode;

                DateTimeOffset minDeliveryDate = DateTimeOffset.UtcNow.AddHours(-12);
    
                // Check shipping addresses, dates and mode
                bool headerHasShippingAddress = salesTransaction.ShippingAddress != null &&
                                                salesTransaction.ShippingAddress.RecordId != 0L;
    
                bool headerHasDeliveryDate = salesTransaction.RequestedDeliveryDate.HasValue;
    
                bool headerHasDeliveryMode = !string.IsNullOrWhiteSpace(salesTransaction.DeliveryMode);
                
                bool isHeaderDeliveryChargeSet = salesTransaction.DeliveryModeChargeAmount.GetValueOrDefault(decimal.Zero) != decimal.Zero;
    
                bool isDeliveryChargeSetOnAnyLine = false;
    
                if (headerHasDeliveryDate)
                {
                    if (salesTransaction.RequestedDeliveryDate.Value < minDeliveryDate)
                    {
                        validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidShippingDate, "Header's delivery date in the past."));
                    }
                }
    
                if (isHeaderDeliveryChargeSet)
                {
                    if (salesTransaction.DeliveryModeChargeAmount < decimal.Zero)
                    {
                        validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidDeliveryCharge, "Header's delivery charge amount must be non-negative."));
                    }
                }
    
                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    isDeliveryChargeSetOnAnyLine = isDeliveryChargeSetOnAnyLine || salesLine.DeliveryModeChargeAmount.GetValueOrDefault(decimal.Zero) != decimal.Zero;
    
                    // Shipping address
                    if (!headerHasShippingAddress &&
                        (salesLine.ShippingAddress == null ||
                        salesLine.ShippingAddress.RecordId == 0L))
                    {
                        validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Sales transaction has no shipping address, thus line must have it defined."));
                    }
    
                    // Delivery date
                    if (!salesLine.RequestedDeliveryDate.HasValue)
                    {
                        if (!headerHasDeliveryDate)
                        {
                            // line has no delivery date, neither does the header
                            validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Sales transaction has no delivery date, thus line must have it defined."));
                        }
                    }
                    else if (salesLine.RequestedDeliveryDate.Value < minDeliveryDate)
                    {
                        // date in the past
                        validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidShippingDate, "Line's delivery date in the past."));
                    }
    
                    bool lineHasDeliveryMode = !string.IsNullOrWhiteSpace(salesLine.DeliveryMode);
    
                    // the line delivery mode, or the header's, in case the line does not provide a delivery mode
                    string lineInheritedDeliveryMode = lineHasDeliveryMode
                        ? salesLine.DeliveryMode
                        : salesTransaction.DeliveryMode;
    
                    bool isLineToBePickedUpAtStore = pickupDeliveryMode.Equals(lineInheritedDeliveryMode, StringComparison.OrdinalIgnoreCase);
    
                    // line is to be picked up at store
                    // check if fulfillment store was set for line
                    if (isLineToBePickedUpAtStore && string.IsNullOrWhiteSpace(salesLine.FulfillmentStoreId))
                    {
                        validationExceptions.Add(
                                new DataValidationFailure(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound,
                                    "Store number must be provided when item is set to be picked up at store."));
                    }
    
                    // Delivery mode
                    if (!headerHasDeliveryMode && !lineHasDeliveryMode)
                    {
                        // if line has not delivery mode neither the header does, report error
                        validationExceptions.Add(
                            new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound,
                                "Sales transaction has no delivery mode, thus line must have it defined."));
                    }
    
                    // Checks if charge is non negative
                    if (salesLine.DeliveryModeChargeAmount.HasValue && salesLine.DeliveryModeChargeAmount < decimal.Zero)
                    {
                        validationExceptions.Add(
                            new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidDeliveryCharge,
                                "Line {0} delivery charge amount must be non-negative.",
                                salesLine.LineId));
                    }
                }
    
                // Checks if we have charges set only on header or lines
                if (isHeaderDeliveryChargeSet && isDeliveryChargeSetOnAnyLine)
                {
                    validationExceptions.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidDeliveryCharge, "Delivery charges cannot be set on both header and lines."));
                }
            }
    
            /// <summary>
            /// Applies default validations required on checkout (order completion).
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction being checkout.</param>
            /// <param name="validationExceptions">The validation exception list.</param>
            /// <param name="skipAmountValidation">True if skip amount validation for checkout, false otherwise.</param>
            private static void ValidateOrderForCheckout(RequestContext context, SalesTransaction salesTransaction, IList<DataValidationFailure> validationExceptions, bool skipAmountValidation = false)
            {
                // Check if types and modes are correctly set for creation/edition
                if (salesTransaction.TransactionType != SalesTransactionType.CustomerOrder ||
                    salesTransaction.CartType != CartType.CustomerOrder)
                {
                    validationExceptions.Add(new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound,
                        "Customer order/quote must have transaction type, cart type correctly defined."));
                }
    
                // Customer must be set
                if (string.IsNullOrWhiteSpace(salesTransaction.CustomerId))
                {
                    validationExceptions.Add(new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound,
                        "Customer order/quote must have customer id set."));
                }
    
                if (!skipAmountValidation)
                {
                try
                {
                    // Validate payments
                    OrderWorkflowHelper.CalculateAndValidateAmountPaidForCheckout(context, salesTransaction);
                }
                catch (DataValidationException dataValidationException)
                {
                    validationExceptions.AddRange(dataValidationException.ValidationResults);
                }
            }
            }
    
            #region CartLine validators
    
            /// <summary>
            /// Validates delivery date for cart line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cartLine">The cart line to validate.</param>
            /// <param name="validationFailures">The validation exception list.</param>
            private static void ValidateCartLineDeliveryDate(RequestContext context, CartLine cartLine, ICollection<DataValidationFailure> validationFailures)
            {
                DateTime channelDate = context.GetNowInChannelTimeZone().Date;
    
                DateTime? requestedDeliveryDate = cartLine.RequestedDeliveryDate.HasValue
                    ? (DateTime?)cartLine.RequestedDeliveryDate.Value.Date
                    : null;
    
                if (requestedDeliveryDate.HasValue && requestedDeliveryDate < channelDate)
                {
                    validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidShippingDate, "Invalid delivery date {0} (UTC) for cartline with id {1}. Delivery date should be greater than today's date.", requestedDeliveryDate, cartLine.LineId));
                }
            }
    
            /// <summary>
            /// Validate that we don't have a non-pickup line with quantity greater than 0 on the pick up transaction.
            /// </summary>
            /// <param name="salesLine">The cart line.</param>
            /// <param name="quantity">Quantity to pickup.</param>
            /// <param name="pickupDeliveryMode">The code for 'pickup' delivery mode.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            /// <returns>True if no validation errors found, otherwise false.</returns>
            private static bool ValidatePickupCartLinesHavePickupDeliveryMode(
                SalesLine salesLine,
                decimal quantity,
                string pickupDeliveryMode,
                ICollection<DataValidationFailure> validationFailures)
            {
                bool isValid = true;
    
                if (!pickupDeliveryMode.Equals(salesLine.DeliveryMode, StringComparison.OrdinalIgnoreCase) && quantity > 0)
                {
                    validationFailures.Add(
                        new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SalesLineMustHavePickupDeliveryMode,
                            "All lines with quantity greater than 0 must have delivery mode set to pickup at store."));
    
                    isValid = false;
                }
    
                return isValid;
            }
    
            /// <summary>
            /// Validate that line is set for pick up at store.
            /// </summary>
            /// <param name="currentStore">The current store.</param>
            /// <param name="salesLine">The cart line.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            private static void ValidateCartLineFulfillmentStoreNumberForPickup(
                OrgUnit currentStore,
                SalesLine salesLine,
                ICollection<DataValidationFailure> validationFailures)
            {
                if (!currentStore.OrgUnitNumber.Equals(salesLine.FulfillmentStoreId, StringComparison.OrdinalIgnoreCase) ||
                    !currentStore.InventoryLocationId.Equals(salesLine.InventoryLocationId, StringComparison.OrdinalIgnoreCase))
                {
                    validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidStoreNumber, "It's only possible to pick up items for current store."));
                }
            }
    
            /// <summary>
            /// Validates whether product quantity is less or equal than is left to be picked up.
            /// </summary>
            /// <param name="salesLineForUpdate">Cart line.</param>
            /// <param name="newQuantity">New quantity.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            /// <returns>True if no validation errors found, otherwise false.</returns>
            private static bool ValidateCartLineQuantityForPickup(
                SalesLine salesLineForUpdate,
                decimal newQuantity,
                Collection<DataValidationFailure> validationFailures)
            {
                bool isValid = true;
                if (newQuantity < 0M ||
                    (newQuantity + salesLineForUpdate.QuantityInvoiced > salesLineForUpdate.QuantityOrdered))
                {
                    DataValidationFailure validationFailure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToPickupMoreThanQtyRemaining,
                        "When picking up a product, it is not allowed to take more than is left to be picked up.");
    
                    validationFailures.Add(validationFailure);
                    isValid = false;
                }
    
                return isValid;
            }
    
            /// <summary>
            /// Validates whether cart line is product.
            /// </summary>
            /// <param name="cartLine">Cart line.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            private static void ValidateCartLineIsProduct(CartLine cartLine, Collection<DataValidationFailure> validationFailures)
            {
                if (!cartLine.LineData.IsProductLine)
                {
                    DataValidationFailure validationFailure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotAddNonProductItemToCustomerOrder,
                        "You cannot add a non-product line to customer order");
    
                    validationFailures.Add(validationFailure);
                }
            }
    
            /// <summary>
            /// Validates whether customer order mode valid for adding a new cart line.
            /// </summary>
            /// <param name="cartCustomerOrderMode">Customer order mode of the cart.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            private static void ValidateCustomerOrderModeForCartLineAdd(CustomerOrderMode cartCustomerOrderMode, Collection<DataValidationFailure> validationFailures)
            {
                if (cartCustomerOrderMode != CustomerOrderMode.CustomerOrderCreateOrEdit &&
                    cartCustomerOrderMode != CustomerOrderMode.QuoteCreateOrEdit)
                {
                    DataValidationFailure validationFailure = new DataValidationFailure(
                       DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCustomerOrderModeForAddCartLine,
                       "Invalid customer order mode for add a cart line. You can add a cart line to customer order only during creation or edition.");
    
                    validationFailures.Add(validationFailure);
                }
            }
            #endregion
    
            /// <summary>
            /// Validates if a property has not been changed.
            /// </summary>
            /// <param name="originalEntity">The entity before the possible change.</param>
            /// <param name="newEntity">The entity after the possible change.</param>
            /// <param name="columnName">The property's name.</param>
            /// <returns>The validation failure or null if no difference is found on the property.</returns>
            private static DataValidationFailure ValidateUnchangedProperty(
                CommerceEntity originalEntity,
                CommerceEntity newEntity,
                string columnName)
            {
                DataValidationFailure failure;
    
                ReadOnlyAttribute.AssertPropertyNotChanged(columnName, newEntity, originalEntity, out failure);
    
                return failure;
            }
    
            /// <summary>
            /// Validates if a property has not been changed.
            /// </summary>
            /// <param name="originalEntity">The entity before the possible change.</param>
            /// <param name="newEntity">The entity after the possible change.</param>        
            /// <param name="validationResults">The collection of validation results.</param>
            /// <param name="columnNames">The properties' name.</param>
            private static void ValidateUnchangedProperty(
                CommerceEntity originalEntity,
                CommerceEntity newEntity,
                CartLineValidationResults validationResults,
                params string[] columnNames)
            {
                foreach (string columnName in columnNames)
                {
                    var failure = CustomerOrderWorkflowHelper.ValidateUnchangedProperty(
                        originalEntity, newEntity, columnName);
    
                    if (failure != null)
                    {
                        validationResults.AddLineResult(0, failure);
                    }
                }
            }
    
            /// <summary>
            /// Validates if a property has not been changed.
            /// </summary>
            /// <param name="originalEntity">The entity before the possible change.</param>
            /// <param name="newEntity">The entity after the possible change.</param>        
            /// <param name="validationFailures">The collection of validation failures.</param>
            /// <param name="columnNames">The properties' name.</param>
            private static void ValidateUnchangedProperty(
                CommerceEntity originalEntity,
                CommerceEntity newEntity,
                Collection<DataValidationFailure> validationFailures,
                params string[] columnNames)
            {
                foreach (string columnName in columnNames)
                {
                    var failure = CustomerOrderWorkflowHelper.ValidateUnchangedProperty(originalEntity, newEntity, columnName);
    
                    if (failure != null)
                    {
                        validationFailures.Add(failure);
                    }
                }
            }
    
            #endregion
    
            /// <summary>
            /// Decides whether a shipping address has changed.
            /// </summary>
            /// <param name="newShippingAddress">The shipping address provided on the request (cart).</param>
            /// <param name="existingShippingAddress">The existing shipping address on the transaction.</param>
            /// <returns>Returns whether the transaction shipping address should be updated with the new shipping address.</returns>
            private static bool HasShippingAddressChanged(Address newShippingAddress, Address existingShippingAddress)
            {
                bool isNewShippingAddressProvided = newShippingAddress != null && newShippingAddress.RecordId != 0;
    
                // if the existing address was never set
                if (existingShippingAddress == null || existingShippingAddress.RecordId == 0)
                {
                    // and the new address is provided, then we need to update the transaction / line shipping address
                    return isNewShippingAddressProvided;
                }
    
                // otherwise, we only need to update the existing address if it is different from the new shipping address
                return isNewShippingAddressProvided && newShippingAddress.RecordId != existingShippingAddress.RecordId;
            }
    
            /// <summary>
            /// Calculates the estimated shipping authorization amount for shipped product lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The current sales transaction.</param>
            private static void CalculateEstimatedShippingAuthorizationAmount(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                decimal? estimatedShippingAmount = null;
    
                // shipping amount estimation only available for customer order creation or update
                // and when there is something to pay for (total amount > 0)
                if (salesTransaction.TotalAmount > 0
                    && (salesTransaction.CartType == CartType.CustomerOrder || salesTransaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit))
                {
                    // since this customer order creation / edition, all charges are shipping charges
                    // get total tax on charge
                    decimal totalTaxOnShippingCharge = salesTransaction.ChargeCalculableSalesLines.SelectMany(salesLine => salesLine.ChargeLines)
                        .Concat(salesTransaction.ChargeLines)
                        .Sum(chargeLine => chargeLine.TaxAmountExclusive);
    
                    // charge total = total charge amount + total charge tax
                    decimal shippingChargeTotalWithTax = salesTransaction.ChargesTotal() + totalTaxOnShippingCharge;
    
                    string pickupDeliveryMode = context.GetChannelConfiguration().PickupDeliveryModeCode;
    
                    // get the total sales line amount with tax for shipping lines
                    decimal totalShippingLineAmountWithTax = salesTransaction.SalesLines
                        .Where(salesLine => salesLine.IsVoided == false)
                        .Where(salesLine => !string.Equals(salesLine.DeliveryMode, pickupDeliveryMode, StringComparison.OrdinalIgnoreCase))
                        .Sum(salesLine => salesLine.NetAmountWithTax());
    
                    // total amount (with tax and charges) of the order that is due for shipping
                    decimal shippedOrderAmount = totalShippingLineAmountWithTax + shippingChargeTotalWithTax;
    
                    // percentage of the order total amount that comes from shipping sales lines
                    decimal shippingAmountPercentage = shippedOrderAmount / salesTransaction.TotalAmount;
    
                    // how much deposit it's estimated to be used for shipping (this is an estimation, since we do not control deposit override)
                    decimal estimatedDepositAppliedOnShipping = salesTransaction.RequiredDepositAmount * shippingAmountPercentage;
    
                    // take away the estimated deposit applied on the shipping amount from the order shipping amount to discover the amount of money
                    // the customer will have to pay for the shipping lines
                    decimal shippingAmountNotCoveredByDeposit = RoundCurrencyAmount(context, shippedOrderAmount - estimatedDepositAppliedOnShipping);
    
                    // clamp to zero to avoid negative values due to rounding issues
                    estimatedShippingAmount = Math.Max(0, shippingAmountNotCoveredByDeposit);
                }
    
                salesTransaction.EstimatedShippingAmount = estimatedShippingAmount;
            }
        }
    }
}
