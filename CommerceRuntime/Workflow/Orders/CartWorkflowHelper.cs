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
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Helper class for shopping cart related workflows.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "In progress of refactoring.")]
        public static class CartWorkflowHelper
        {
            /// <summary>
            /// The payment manager service name.
            /// </summary>
            internal const string PaymentManagerServiceName = "PaymentManager";

            /// <summary>
            /// Loads a returned sales transaction. 
            /// </summary>
            /// <param name="context">The request context. It must contains sales transaction which has at least one sales lines for return.</param>
            /// <param name="cart">The cart object.</param>
            /// <param name="originalTransaction">Original transaction.</param>
            /// <param name="transactionOperationType">Operation type of the transaction.</param>
            /// <returns>The loaded returned sales transaction.</returns>
            public static SalesTransaction LoadSalesTransactionForReturn(RequestContext context, Cart cart, SalesTransaction originalTransaction, TransactionOperationType transactionOperationType)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(cart, "cart");

                string transactionIdToLoad = ValidateAndGetReturnTransactionId(cart, originalTransaction);

                if (transactionIdToLoad == null)
                {
                    return null;
                }

                if (transactionOperationType != TransactionOperationType.Void)
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ReturnTransaction));
                }

                SalesOrderSearchCriteria criteria = new SalesOrderSearchCriteria()
                {
                    TransactionIds = new[] { transactionIdToLoad },
                    IncludeDetails = true,
                    SearchLocationType = SearchLocation.All,
                    SalesTransactionTypes = new[] { SalesTransactionType.Sales },
                    SearchType = OrderSearchType.SalesTransaction
                };
                GetOrdersServiceRequest request = new GetOrdersServiceRequest(criteria, QueryResultSettings.AllRecords);

                GetOrdersServiceResponse response = context.Execute<GetOrdersServiceResponse>(request);
                if (!response.Orders.Results.Any())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, 
                        string.Format("Return transaction with id '{0}' not found.", transactionIdToLoad));
                }

                return response.Orders.Results[0];
            }

            /// <summary>
            /// Writes an entry into the audit table.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="source">The log source.</param>
            /// <param name="logEntry">The log entry.</param>
            public static void LogAuditEntry(RequestContext context, string source, string logEntry)
            {
                ThrowIf.Null(context, "context");

                var auditLogServiceRequest = new InsertAuditLogServiceRequest(source, logEntry, AuditLogTraceLevel.Trace, unchecked((int)context.RequestTimer.ElapsedMilliseconds));
                context.Execute<NullResponse>(auditLogServiceRequest);
            }

            /// <summary>
            /// Loads the sales transactions given the cart identifier.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cartId">Cart identifier.</param>
            /// <returns>The loaded sales transaction.</returns>
            public static SalesTransaction LoadSalesTransaction(RequestContext context, string cartId)
            {
                return LoadSalesTransaction(context, cartId, ignoreProductDiscontinuedNotification: false);
            }

            /// <summary>
            /// Loads the sales transactions given the cart identifier.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cartId">Cart identifier.</param>
            /// <param name="ignoreProductDiscontinuedNotification">If set to 'true' and some products on the cart are not available notification will not be fired.</param>
            /// <returns>The loaded sales transaction.</returns>
            public static SalesTransaction LoadSalesTransaction(RequestContext context, string cartId, bool ignoreProductDiscontinuedNotification)
            {
                ThrowIf.Null(context, "context");

                // Try to load the transaction
                var getCartRequest = new GetCartRequest(
                    new CartSearchCriteria { CartId = cartId }, 
                    QueryResultSettings.SingleRecord,
                    includeHistoricalTenderLines: false,
                    ignoreProductDiscontinuedNotification: ignoreProductDiscontinuedNotification);
                var getCartResponse = context.Execute<GetCartResponse>(getCartRequest);

                return getCartResponse.Transactions.SingleOrDefault();
            }

            /// <summary>
            /// Creates a sales transaction with given transaction id and customer id.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transactionId">The transaction id.</param>
            /// <param name="customerId">The customer id.</param>
            /// <returns>The newly created sales transaction.</returns>
            public static SalesTransaction CreateSalesTransaction(RequestContext context, string transactionId, string customerId)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.NullOrWhiteSpace(transactionId, "transactionId");

                NetTracer.Information("CartWorkflowHelper.CreateSalesTransaction(): TransactionId = {0}, CustomerId = {1}", transactionId, customerId);

                SalesTransaction transaction = LoadSalesTransaction(context, transactionId);
                if (transaction != null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DuplicateObject, transactionId);
                }

                // Gets the channel configuration
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();

                // Creates the transaction
                transaction = new SalesTransaction
                {
                    Id = transactionId,
                    ShiftId = context.GetPrincipal().ShiftId,
                    ShiftTerminalId = context.GetPrincipal().ShiftTerminalId,
                    CustomerId = customerId
                };

                GetCurrentTerminalIdDataRequest dataRequest = new GetCurrentTerminalIdDataRequest();
                transaction.TerminalId = context.Execute<SingleEntityDataServiceResponse<string>>(dataRequest).Entity;

                transaction.IsTaxIncludedInPrice = channelConfiguration.PriceIncludesSalesTax;
                transaction.InventoryLocationId = channelConfiguration.InventLocation;

                transaction.BeginDateTime = context.GetNowInChannelTimeZone();

                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore)
                {
                    if (context.GetOrgUnit() != null)
                    {
                        transaction.StoreId = context.GetOrgUnit().OrgUnitNumber;
                    }
                }

                return transaction;
            }

            /// <summary>
            /// Saves the context's sales transaction to the DB.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Transaction to save.</param>
            public static void SaveSalesTransaction(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.SaveSalesTransaction(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                try
                {
                    context.Execute<NullResponse>(new SaveCartDataRequest(new[] { transaction }));
                }
                catch (StorageException e)
                {
                    // if there is a concurrency exception, surface this to the the caller as a validation issue that can be retried
                    if (e.ErrorResourceId == StorageErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectVersionMismatchError.ToString())
                    {
                        throw new CartValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartVersion,
                            transaction.Id, 
                            e,
                            "Cart version mismatch");
                    }

                    throw;
                }
            }

            /// <summary>
            /// Transfer the context's sales transaction to the DB.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Transaction to save.</param>
            public static void TransferSalesTransaction(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.TransferSalesTransaction(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                // Transfer sales transaction has to be seamless.
                // Row version check need to be ignored since data gets tranfered from different storage.
                context.Execute<NullResponse>(new SaveCartDataRequest(new[] { transaction }, ignoreRowVersionCheck: true));
            }

            /// <summary>
            /// Validates the update cart request.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="returnedSalesTransaction">The returned sales transaction.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            /// <param name="isGiftCardOperation">True if request is a result of gift card operation.</param>
            /// <param name="productByRecordId">The mapping of products by record identifier.</param>
            /// <exception cref="CartValidationException">Invalid cart.</exception>
            public static void ValidateUpdateCartRequest(
                RequestContext context, 
                SalesTransaction salesTransaction, 
                SalesTransaction returnedSalesTransaction, 
                Cart newCart, 
                bool isGiftCardOperation,
                IDictionary<long, SimpleProduct> productByRecordId)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(newCart, "newCart");

                NetTracer.Information("CartWorkflowHelper.ValidateUpdateCartRequest(): TransactionId = {0}, CustomerId = {1}", newCart.Id, newCart.CustomerId);

                // If cart type is None it means type has not been changed so we check cart type on existing cart.
                var cartType = newCart.CartType == CartType.None ? salesTransaction.CartType : newCart.CartType;

                CartLineValidationResults validationResults = new CartLineValidationResults();
                HashSet<string> lineIdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Validate readonly properties
                foreach (DataValidationFailure failure in ReadOnlyAttribute.CheckReadOnlyProperties(newCart, salesTransaction))
                {
                    validationResults.AddLineResult(0, failure);
                }

                // Validates conflicts in cart lines
                CartWorkflowHelper.ValidateConflicts(newCart.CartLines, lineIdSet, validationResults);

                // Set to true if it is a return transaction by receipt.
                bool isCashAndCarryReturnByReceipt = cartType != CartType.CustomerOrder &&
                    (((returnedSalesTransaction != null) && returnedSalesTransaction.IsReturnByReceipt) || newCart.CartLines.Any(l => !string.IsNullOrWhiteSpace(l.ReturnTransactionId)));

                if (isCashAndCarryReturnByReceipt)
                {
                    // Validate conflicts in cart lines for return
                    CartWorkflowHelper.ValidateReturnConflicts(returnedSalesTransaction, newCart.CartLines, validationResults);
                }

                // Validate all the lines.
                Dictionary<string, SalesLine> salesLineByLineId = salesTransaction.SalesLines
                    .ToDictionary(sl => sl.LineId, sl => sl);

                Dictionary<decimal, SalesLine> returnSalesLineByLineNumber = null;
                if (returnedSalesTransaction != null)
                {
                    // Validate all the lines.
                    returnSalesLineByLineNumber = returnedSalesTransaction.SalesLines
                        .ToDictionary(sl => sl.LineNumber, sl => sl);
                }

                CartWorkflowHelper.ValidateSalesLineOperations(context, newCart, salesTransaction, salesLineByLineId, returnSalesLineByLineNumber, isGiftCardOperation, validationResults, productByRecordId);
                CartWorkflowHelper.ValidateCartLineUnitOfMeasureAndQuantity(context, newCart, salesTransaction, validationResults);

                CartWorkflowHelper.ValidateCustomerAccount(context, newCart, salesTransaction);

                if (cartType == CartType.IncomeExpense)
                {
                    ValidateIncomeExpenseTransaction(context, newCart, validationResults);
                }

                // Loyalty validations
                CartWorkflowHelper.ValidateLoyaltyCard(context, newCart, salesTransaction, validationResults);

                // Customer order validations
                CustomerOrderWorkflowHelper.CustomerOrderCartValidations(context, newCart, salesTransaction, returnedSalesTransaction, validationResults);

                // Invoice lines validations
                CartWorkflowHelper.ValidateTransactionWithInvoiceLines(newCart, salesTransaction, validationResults);

                if (validationResults.HasErrors)
                {
                    foreach (var validationResult in validationResults.ValidationResults)
                    {
                        foreach (var validationFailure in validationResult.Value)
                        {
                            NetTracer.Information("Cart line validation failure. Line id: {0}. Details: {1}", validationResult.Key, validationFailure.ToString());
                        }
                    }

                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, newCart.Id, validationResults, "Validation failures occurred when verifying cart for update.");
                }

                // Gift card validations.
                // 1. If Cart has only gift card/s then total manual discount cannot be applied.
                // 2. If cart has gift card and products then discount is applied only to products.
                // 3. Cart line discount cannot be applied to gift card and is checked in a different method, ValidateSalesLineOperations above.
                if (salesTransaction.SalesLines.All(cartLine => cartLine.IsGiftCardLine) && (newCart.TotalManualDiscountAmount != 0 || newCart.TotalManualDiscountPercentage != 0))
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_GiftCardDiscountNotAllowed, newCart.Id);
                }

                // Validate the discount privilege for employee.
                CartWorkflowHelper.ValidateEmployeeDiscountPermission(context, salesTransaction, newCart);
            }

            /// <summary>
            /// Validates the save cart lines request.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="request">The request.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="returnTransaction">Transaction that is being returned.</param>
            /// <param name="productByRecordId">The mapping of products by record identifier.</param>
            public static void ValidateSaveCartLinesRequest(RequestContext context, SaveCartLinesRequest request, SalesTransaction transaction, SalesTransaction returnTransaction, IDictionary<long, SimpleProduct> productByRecordId)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(request, "request");

                ValidateUpdateCartRequest(context, transaction, returnTransaction, request.Cart, isGiftCardOperation: false, productByRecordId: productByRecordId);
            }

            /// <summary>
            /// Performs the save cart request on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="request">The save cart request.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="returnTransaction">Transaction that is being returned.</param>
            /// <param name="newSalesLineIdSet">New sales line id set.</param>
            /// <param name="productByRecordId">The mapping of products by record identifier.</param>
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Nothing obvious to refactor.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Nothing obvious to refactor.")]
            public static void PerformSaveCartOperations(
                RequestContext context, 
                SaveCartRequest request, 
                SalesTransaction transaction, 
                SalesTransaction returnTransaction, 
                HashSet<string> newSalesLineIdSet,
                IDictionary<long, SimpleProduct> productByRecordId)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Cart, "request.Cart");
                ThrowIf.Null(newSalesLineIdSet, "newSalesLineIdSet");

                NetTracer.Information("CartWorkflowHelper.PerformSaveCartOperations(): TransactionId = {0}, CustomerId = {1}", request.Cart.Id, request.Cart.CustomerId);

                // Look at all the lines
                Dictionary<string, SalesLine> salesLineByLineId = transaction.SalesLines
                    .ToDictionary(sl => sl.LineId, sl => sl, StringComparer.OrdinalIgnoreCase);

                // Add or Update transaction level reason code lines.
                ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnTransaction(transaction, request.Cart);

                // Add or update affiliations when add or change customer from the Cart if the customer contains affiliations.
                CartWorkflowHelper.AddOrUpdateAffiliationLinesFromCustomer(context, transaction, request.Cart);

                // Check permission for price overridden if any cartline has overridden price.
                if (request.Cart.CartLines.Any(c => c.IsPriceOverridden))
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.PriceOverride));
                }

                // Set OriginalPrice value
                foreach (CartLine cartLine in request.Cart.CartLines)
                {
                    SalesLine salesLine;
                    if (salesLineByLineId.TryGetValue(cartLine.LineId, out salesLine)
                        && cartLine.IsPriceOverridden
                        && !salesLine.IsPriceOverridden
                        && cartLine.LineData.OriginalPrice == null)
                    {
                        cartLine.OriginalPrice = salesLine.Price;
                    }
                }

                CartWorkflowHelper.CheckChangeUnitOfMeasureOperation(context, request.Cart, salesLineByLineId.Values);

                // Update address information on the request
                CustomerOrderWorkflowHelper.FillAddressInformation(context, transaction, request.Cart, salesLineByLineId);

                // copy properties from cart to sales transaction but ignore nulls (this enables delta cart updates)
                transaction.CopyPropertiesFrom(request.Cart);

                OrderWorkflowHelper.FillTransactionWithContextData(context, transaction);

                transaction.AttributeValues.Clear();
                foreach (AttributeValueBase attributeValueBase in request.Cart.AttributeValues)
                {
                    if (attributeValueBase != null)
                    {
                        transaction.AttributeValues.Add(attributeValueBase);
                    }
                }

                // check for reason codes on total discount
                if (transaction.TotalManualDiscountAmount != 0 || transaction.TotalManualDiscountPercentage != 0)
                {
                    ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTransaction(context, transaction, ReasonCodeSourceType.TotalDiscount);
                }

                HashSet<string> originalSalesLineIdSet = new HashSet<string>(salesLineByLineId.Keys, StringComparer.OrdinalIgnoreCase);
                
                // Perform save operations for lines
                CartWorkflowHelper.PerformSaveCartLineOperations(context, request, transaction, returnTransaction, salesLineByLineId, productByRecordId);

                foreach (KeyValuePair<string, SalesLine> pair in salesLineByLineId)
                {
                    string salesLineId = pair.Key;
                    if (!originalSalesLineIdSet.Contains(salesLineId))
                    {
                        newSalesLineIdSet.Add(salesLineId);
                    }
                }

                // Save the tax override codes on the transaction and lines.
                CartWorkflowHelper.UpdateTaxOverrideCodes(context, transaction, request.Cart);

                // Perform save income (or) expense lines
                transaction.IncomeExpenseLines.Clear();
                transaction.IncomeExpenseLines.AddRange(request.Cart.IncomeExpenseLines);

                // Perform save customer account deposit lines.
                transaction.CustomerAccountDepositLines.Clear();
                transaction.CustomerAccountDepositLines.AddRange(request.Cart.CustomerAccountDepositLines);

                UpdateIncomeExpenseTransactionFields(context, transaction, request.Cart);

                // Set default transaction data for account deposit cart
                UpdateAccountDepositTransactionFields(context, transaction, request.Cart);

                // Set default data on all sales lines
                SetDefaultDataOnSalesLines(context, transaction, salesLineByLineId.Values);

                // Refresh the sales lines
                transaction.SalesLines.Clear();
                transaction.SalesLines.AddRange(salesLineByLineId.Values);

                // Gets the inventory site identifier for the sales transaction inventory location identifier.
                CartWorkflowHelper.GetSalesTransactionWarehouseInformation(context, transaction);

                // Calculate required reason codes on transaction level.
                ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTransaction(context, transaction, ReasonCodeSourceType.None);

                // Update customer order related fields
                CustomerOrderWorkflowHelper.UpdateCustomerOrderFieldsOnSave(context, transaction, request.Cart);

                // Refresh transaction level affiliation lines.
                CartWorkflowHelper.AddOrUpdateAffiliationLines(context, request.Cart.AffiliationLines, transaction, returnTransaction);

                // Refresh loyalty groups and tiers on the transaction.
                CartWorkflowHelper.RefreshSalesLoyaltyTierLines(context, transaction);
            }

            /// <summary>
            /// Sets the default data on sales lines.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="salesLines">The sales lines.</param>
            public static void SetDefaultDataOnSalesLines(RequestContext context, SalesTransaction transaction, IEnumerable<SalesLine> salesLines)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(salesLines, "salesLines");

                IEnumerable<Item> items = GetItemsForSalesLines(context, salesLines);
                Dictionary<string, Item> itemsByItemId = items.ToDictionary(i => i.ItemId, i => i, StringComparer.OrdinalIgnoreCase);

                Dictionary<ItemUnitConversion, UnitOfMeasureConversion> unitOfMeasureConversionMap = GetUnitOfMeasureConversionMap(context, salesLines);

                foreach (SalesLine salesLine in salesLines)
                {
                    // Keep the current field values for non-product cart lines, such as gift card, invoice line, account deposit, etc.
                    if (salesLine.IsProductLine)
                    {
                        Item cartItem;
                        if (itemsByItemId.TryGetValue(salesLine.ItemId, out cartItem))
                        {
                            SetSalesLineDefaultsFromItemData(salesLine, cartItem);
                        }
                        else
                        {
                            NetTracer.Warning("Item information for item id {0} was not found.", salesLine.ItemId);
                        }

                        // Set Unit of Measure conversion if the convert to unit of measure specified.
                        UnitOfMeasureConversion conversion = GetUnitOfMeasureConversion(salesLine, unitOfMeasureConversionMap);
                        if (conversion != null)
                        {
                            ChangeUnitOfMeasure(context, salesLine, conversion);
                        }
                        else
                        {
                            salesLine.UnitOfMeasureConversion = UnitOfMeasureConversion.CreateDefaultUnitOfMeasureConversion();
                        }

                        switch (transaction.CartType)
                        {
                            case CartType.Checkout:
                            case CartType.Shopping:
                                SetSalesLineDefaultsFromOrderHeader(salesLine, transaction);
                                break;

                            case CartType.CustomerOrder:
                                break;

                            case CartType.IncomeExpense:
                            case CartType.AccountDeposit:
                                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SalesLineNotAllowed, "Sales line is not allowed on the transaction.");

                            case CartType.None:
                            default:
                                throw new NotSupportedException(string.Format("Cart type {0} is not supported.", transaction.CartType));
                        }
                    }
                }
            }

            /// <summary>
            /// Updates income expense transaction fields.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="salesTransaction">Set sales transaction.</param>
            /// <param name="cart">Set cart received on the request.</param>
            public static void UpdateIncomeExpenseTransactionFields(RequestContext context, SalesTransaction salesTransaction, Cart cart)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                // only make changes if it's a income expense cart.
                if (salesTransaction.CartType != CartType.IncomeExpense)
                {
                    return;
                }

                salesTransaction.TransactionType = SalesTransactionType.IncomeExpense;
            }

            /// <summary>
            /// Updates Account Deposit Transaction fields.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="salesTransaction">Set sales transaction.</param>
            /// <param name="cart">Set cart received on the request.</param>
            public static void UpdateAccountDepositTransactionFields(RequestContext context, SalesTransaction salesTransaction, Cart cart)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                // only make changes if it's an account deposit cart.
                if (salesTransaction.CartType != CartType.AccountDeposit)
                {
                    return;
                }

                salesTransaction.TransactionType = SalesTransactionType.CustomerAccountDeposit;
            }

            /// <summary>
            /// Removes any tender lines that have the IsHistorical value set to true.
            /// </summary>
            /// <param name="cart">Cart to filter the tender lines on.</param>
            public static void RemoveHistoricalTenderLines(Cart cart)
            {
                ThrowIf.Null(cart, "cart");

                cart.TenderLines = cart.TenderLines.Where(c => c.IsHistorical == false).ToArray();
            }

            /// <summary>
            /// Converts from a sales transaction to a cart.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="salesTransaction">The sales transaction to convert from.</param>
            /// <returns>The populated cart.</returns>
            public static Cart ConvertToCart(RequestContext context, SalesTransaction salesTransaction)
            {
                if (salesTransaction == null)
                {
                    return null;
                }

                Cart cart = new Cart();
                cart.CopyPropertiesFrom(salesTransaction);
                cart.CartStatus = GetCartStatus(salesTransaction);

                cart.AttributeValues.AddRange(salesTransaction.AttributeValues);
                cart.DiscountCodes.AddRange(salesTransaction.DiscountCodes);
                cart.ChargeLines.AddRange(salesTransaction.ChargeLines);
                cart.TaxOverrideCode = salesTransaction.TaxOverrideCode;

                if (context.GetChannelConfiguration().DisplayTaxPerTaxComponent)
                {
                    InsertTaxDetailIntoCart(context, cart, salesTransaction);
                }

                foreach (SalesLine salesLine in salesTransaction.SalesLines)
                {
                    CartLine cartLine = new CartLine()
                    {
                        LineId = salesLine.LineId,
                        ProductId = salesLine.ProductId,
                        ItemId = salesLine.ItemId,
                        InventoryDimensionId = salesLine.InventoryDimensionId,
                        TaxOverrideCode = salesLine.TaxOverrideCode
                    };

                    cartLine.LineData.CopyPropertiesFrom(salesLine);

                    cartLine.LineData.DiscountLines.Clear();
                    cartLine.LineData.DiscountLines.AddRange(salesLine.DiscountLines);

                    cartLine.LineData.ReasonCodeLines.Clear();
                    cartLine.LineData.ReasonCodeLines.AddRange(salesLine.ReasonCodeLines);

                    cartLine.LineData.ChargeLines.Clear();
                    cartLine.LineData.ChargeLines.AddRange(salesLine.ChargeLines);

                    cartLine.LineData.LineIdsLinkedProductMap.Clear();
                    cartLine.LineData.LineIdsLinkedProductMap.AddRange(salesLine.LineIdsLinkedProductMap);

                    cart.CartLines.Add(cartLine);
                }

                cart.IncomeExpenseLines.AddRange(salesTransaction.IncomeExpenseLines);
                cart.TenderLines.AddRange(salesTransaction.TenderLines);
                cart.ReasonCodeLines.AddRange(salesTransaction.ReasonCodeLines);
                cart.CustomerAccountDepositLines.AddRange(salesTransaction.CustomerAccountDepositLines);

                CopyAffiliationLoyaltyTierToCart(salesTransaction, cart);

                return cart;
            }

            /// <summary>
            /// Converts from a cart to a sales transaction.
            /// </summary>
            /// <param name="cart">The cart to convert from.</param>
            /// <returns>The populated sales transaction.</returns>
            public static SalesTransaction ConvertToSalesTransaction(Cart cart)
            {
                if (cart == null)
                {
                    return null;
                }

                SalesTransaction salesTransaction = new SalesTransaction();
                salesTransaction.CopyPropertiesFrom(cart);

                salesTransaction.AttributeValues.AddRange(cart.AttributeValues);
                salesTransaction.DiscountCodes.AddRange(cart.DiscountCodes);
                salesTransaction.ChargeLines.AddRange(cart.ChargeLines);
                salesTransaction.TaxOverrideCode = cart.TaxOverrideCode;

                foreach (CartLine cartLine in cart.CartLines)
                {
                    SalesLine salesLine = new SalesLine()
                    {
                        LineId = cartLine.LineId,
                        ProductId = cartLine.ProductId,
                        ItemId = cartLine.ItemId,
                        InventoryDimensionId = cartLine.InventoryDimensionId,
                        TaxOverrideCode = cartLine.TaxOverrideCode
                    };

                    salesLine.CopyPropertiesFrom(cartLine.LineData);

                    salesLine.DiscountLines.Clear();
                    salesLine.DiscountLines.AddRange(cartLine.LineData.DiscountLines);

                    salesLine.ReasonCodeLines.Clear();
                    salesLine.ReasonCodeLines.AddRange(cartLine.LineData.ReasonCodeLines);

                    salesLine.ChargeLines.Clear();
                    salesLine.ChargeLines.AddRange(cartLine.LineData.ChargeLines);

                    salesLine.LineIdsLinkedProductMap.Clear();
                    salesLine.LineIdsLinkedProductMap.AddRange(cartLine.LineData.LineIdsLinkedProductMap);

                    salesTransaction.SalesLines.Add(salesLine);
                }

                salesTransaction.IncomeExpenseLines.AddRange(cart.IncomeExpenseLines);
                salesTransaction.TenderLines.AddRange(cart.TenderLines);
                salesTransaction.ReasonCodeLines.AddRange(cart.ReasonCodeLines);

                CopyAffiliationLoyaltyTierToSalesTransaction(cart, salesTransaction);

                return salesTransaction;
            }

            /// <summary>
            /// Converts CartTenderLine to TenderLine.
            /// </summary>
            /// <param name="cartTenderLine">The cart tender line.</param>
            /// <returns>The tender line.</returns>
            public static TenderLine ConvertToTenderLine(CartTenderLine cartTenderLine)
            {
                ThrowIf.Null(cartTenderLine, "cartTenderLine");

                TenderLine tenderLine = new TenderLine
                {
                    TenderLineId = cartTenderLine.TenderLineId
                };

                tenderLine.CopyPropertiesFrom(cartTenderLine);

                // CopyPropertiesFrom() copied all properties including those not available on destination class (TenderLine does not have PaymentCard property).
                // In order to avoid persisting payment card information as part of serialized TenderLine setting it to null.
                if (tenderLine.GetProperties().ContainsKey(CartTenderLine.PaymentCardColumn))
                {
                    tenderLine.GetProperties().Remove(CartTenderLine.PaymentCardColumn);
                }

                if (cartTenderLine.ReasonCodeLines != null &&
                    cartTenderLine.ReasonCodeLines.Any())
                {
                    tenderLine.ReasonCodeLines.Clear();

                    // Retrieves existing reason code lines here for only update so that
                    // the new reason code lines can be added through AddOrUpdateReasonCodeLinesOnTenderLine()
                    IEnumerable<ReasonCodeLine> existingReasonCodeLines = cartTenderLine.ReasonCodeLines.Where(l => !string.IsNullOrWhiteSpace(l.LineId));
                    tenderLine.ReasonCodeLines.AddRange(existingReasonCodeLines);
                }

                return tenderLine;
            }

            /// <summary>
            /// Calculates the various totals depending on the specified calculation mode.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="requestedMode">The calculation mode.</param>
            public static void Calculate(RequestContext context, SalesTransaction transaction, CalculationModes? requestedMode)
            {
                CartWorkflowHelper.Calculate(context, transaction, requestedMode, null, false, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }

            /// <summary>
            /// Calculates the various totals depending on the specified calculation mode.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="requestedMode">The calculation mode.</param>
            /// <param name="isItemSaleForSure">A flag indicating whether it is an item sale.</param>
            /// <param name="newSalesLineIdSet">New sales line id set.</param>
            public static void Calculate(RequestContext context, SalesTransaction transaction, CalculationModes? requestedMode, bool isItemSaleForSure, HashSet<string> newSalesLineIdSet)
            {
                CartWorkflowHelper.Calculate(context, transaction, requestedMode, null, isItemSaleForSure, newSalesLineIdSet);
            }

            /// <summary>
            /// Calculates the various totals depending on the specified calculation mode.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="requestedMode">The calculation mode.</param>
            /// <param name="discountCalculationMode">The discount calculation mode.</param>
            public static void Calculate(RequestContext context, SalesTransaction transaction, CalculationModes? requestedMode, DiscountCalculationMode? discountCalculationMode)
            {
                CartWorkflowHelper.Calculate(context, transaction, requestedMode, discountCalculationMode, false, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }

            /// <summary>
            /// Calculates the various totals depending on the specified calculation mode.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="requestedMode">The calculation mode.</param>
            /// <param name="discountCalculationMode">The discount calculation mode.</param>
            /// <param name="isItemSaleForSure">A flag indicating whether it is an item sale.</param>
            /// <param name="newSalesLineIdSet">New sales line id set.</param>
            public static void Calculate(
                RequestContext context,
                SalesTransaction transaction,
                CalculationModes? requestedMode,
                DiscountCalculationMode? discountCalculationMode,
                bool isItemSaleForSure,
                HashSet<string> newSalesLineIdSet)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(newSalesLineIdSet, "newSalesLineIdSet");

                CalculationModes mode = requestedMode ?? GetCalculationModes(context, transaction);

                if (mode == CalculationModes.None)
                {
                    // No calculations are required.
                    return;
                }

                if (mode.HasFlag(CalculationModes.Prices) || mode.HasFlag(CalculationModes.Discounts))
                {
                    CartWorkflowHelper.AssociateCatalogsToSalesLines(context, transaction);
                }

                if (discountCalculationMode == null && mode.HasFlag(CalculationModes.Prices) && mode.HasFlag(CalculationModes.Discounts) && DelayMulitpleItemDiscountCalculation(context, transaction))
                {
                    transaction = CalculateIndependentPricesAndDiscounts(context, transaction, isItemSaleForSure, newSalesLineIdSet);
                }
                else
                {
                    if (mode.HasFlag(CalculationModes.Prices))
                    {
                        CalculatePrices(context, transaction);
                    }

                    if (mode.HasFlag(CalculationModes.Discounts))
                    {
                        transaction = CalculateDiscounts(context, transaction, discountCalculationMode);
                    }
                }

                if (mode.HasFlag(CalculationModes.Charges))
                {
                    transaction = CalculateCharges(context, transaction);
                }

                if (mode.HasFlag(CalculationModes.Taxes))
                {
                    transaction = CalculateTaxes(context, transaction);
                }

                if (mode.HasFlag(CalculationModes.Totals))
                {
                    transaction = CalculateTotals(context, transaction);
                    RoundTotals(context, transaction);
                }

                if (mode.HasFlag(CalculationModes.Deposit))
                {
                    CustomerOrderWorkflowHelper.CalculateDeposit(context, transaction);
                }

                if (mode.HasFlag(CalculationModes.AmountDue))
                {
                    transaction = CalculateAmountsPaidAndDue(context, transaction);
                }
            }

            /// <summary>
            /// Gets the products in cart lines.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="cartLines">The cart lines.</param>
            /// <returns>A dictionary mapping product identifiers to products.</returns>
            public static Dictionary<long, SimpleProduct> GetProductsInCartLines(RequestContext context, IEnumerable<CartLine> cartLines)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(cartLines, "cartLines");

                // only calculate price on product retrieval if price key in is done
                bool mustCalculateProductPrice = cartLines.Any(cartLine => cartLine.IsPriceKeyedIn);

                long channelId = context.GetPrincipal().ChannelId;
                var productIdsInTheCart = cartLines.Where(cl => cl.LineData.ProductId != 0).Select(cl => cl.LineData.ProductId).Distinct();
                var productsRequest = new GetProductsServiceRequest(channelId, productIdsInTheCart, calculatePrice: mustCalculateProductPrice, settings: QueryResultSettings.AllRecords)
                {
                    SearchLocation = SearchLocation.All
                };

                var products = context.Execute<GetProductsServiceResponse>(productsRequest).Products.Results.OrderBy(p => p.IsRemote);

                var productMap = new Dictionary<long, SimpleProduct>();
                foreach (var product in products)
                {
                    if (!productMap.ContainsKey(product.RecordId))
                    {
                        productMap[product.RecordId] = product;
                    }
                }

                var cartLinesWithoutProductIds = cartLines.Where(l => l.LineData.ProductId == 0);
                if (cartLinesWithoutProductIds.Any())
                {
                    var productLookupClauses = new List<ProductLookupClause>();
                    var cartLinesWithItemIds = cartLines.Where(l => !string.IsNullOrEmpty(l.LineData.ItemId) && l.LineData.ProductId == 0);

                    foreach (var cartLine in cartLinesWithItemIds)
                    {
                        if (!productLookupClauses.Any(l => l.ItemId.Equals(cartLine.LineData.ItemId) && ((string.IsNullOrWhiteSpace(cartLine.InventoryDimensionId) && string.IsNullOrWhiteSpace(l.InventDimensionId)) || l.InventDimensionId.Equals(cartLine.LineData.InventoryDimensionId))))
                        {
                            productLookupClauses.Add(new ProductLookupClause(cartLine.LineData.ItemId, cartLine.LineData.InventoryDimensionId));
                        }
                    }

                    var getProductByItemIdRequest = new GetProductsServiceRequest(channelId, productLookupClauses, QueryResultSettings.AllRecords)
                    {
                        SearchLocation = SearchLocation.All
                    };
                    var productsByItemIds = context.Execute<GetProductsServiceResponse>(getProductByItemIdRequest).Products.Results;

                    foreach (var product in productsByItemIds)
                    {
                        productMap[product.RecordId] = product;
                        var cartLinesMatchingItemId = cartLines.Where(l => l.LineData.ItemId.Equals(product.ItemId) && (string.IsNullOrWhiteSpace(l.LineData.InventoryDimensionId) || l.LineData.InventoryDimensionId.Equals(product.InventoryDimensionId)));

                        if (cartLinesMatchingItemId.IsNullOrEmpty())
                        {
                            throw new InvalidOperationException(string.Format("Product not found in cartLines was retrieved. ItemId: {0}; InventDimId: {1}.", product.ItemId, product.InventoryDimensionId));
                        }

                        foreach (var cartLine in cartLinesMatchingItemId)
                        {
                            cartLine.LineData.ProductId = product.RecordId;
                        }
                    }
                }

                return productMap;
            }

            /// <summary>
            /// The a sales line from the sales transaction by the line number.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The sales line.</returns>
            public static SalesLine GetSalesLineByNumber(SalesTransaction salesTransaction, decimal lineNumber)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");

                return salesTransaction.SalesLines
                    .Where(sl => sl.LineNumber == lineNumber)
                    .Select(sl => sl).FirstOrDefault();
            }

            /// <summary>
            /// Sets the sales line based on returned sales line.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="returnedSalesLine">The returned sales line.</param>
            /// <param name="returnTransaction">The return transaction.</param>
            /// <param name="quantity">The quantity.</param>
            public static void SetSalesLineBasedOnReturnedSalesLine(SalesLine salesLine, SalesLine returnedSalesLine, SalesTransaction returnTransaction, decimal quantity)
            {
                ThrowIf.Null(salesLine, "salesLine");
                ThrowIf.Null(returnedSalesLine, "returnedSalesLine");
                ThrowIf.Null(returnTransaction, "returnTransaction");

                decimal originalLineNumber = salesLine.LineNumber;
                string originalInventoryLocationId = salesLine.InventoryLocationId;
                salesLine.CopyPropertiesFrom(returnedSalesLine);

                // add a line identifier if none
                if (string.IsNullOrWhiteSpace(salesLine.LineId))
                {
                    AssignUniqueLineId(salesLine);
                }

                // Restore original sales line values
                salesLine.LineNumber = originalLineNumber;
                salesLine.InventoryLocationId = originalInventoryLocationId;

                // Mark the item that it's an item that was returned with a receipt.  That means that the item line is not eligible
                // for price changes & overrides, discounts, qty changes above the allowed return qty, etc....
                salesLine.IsReturnByReceipt = true;

                // Set return fields
                salesLine.ReturnTransactionId = returnTransaction.Id;
                salesLine.ReturnLineNumber = returnedSalesLine.LineNumber;
                salesLine.ReturnChannelId = returnedSalesLine.ReturnChannelId;
                salesLine.ReturnStore = returnTransaction.StoreId;
                salesLine.ReturnTerminalId = returnTransaction.TerminalId;

                // Copy quantity from the new cart line
                salesLine.Quantity = quantity;

                // Reconstruct discount lines
                // Discount - Handling, copy from returned sales line
                if (returnedSalesLine.DiscountLines.Count > 0)
                {
                    // Copy the same discount lines
                    foreach (DiscountLine returnDisLine in returnedSalesLine.DiscountLines)
                    {
                        DiscountLine salesLineDiscount = returnDisLine.Clone<DiscountLine>();
                        salesLine.DiscountLines.Add(salesLineDiscount);
                    }
                }
            }

            /// <summary>
            /// Sets the default properties on the sales line from the order header.
            /// </summary>
            /// <param name="salesLine">The sales line from the sales transaction.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            public static void SetSalesLineDefaultsFromOrderHeader(SalesLine salesLine, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(salesLine, "salesLine");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                if (string.IsNullOrWhiteSpace(salesLine.DeliveryMode))
                {
                    salesLine.DeliveryMode = salesTransaction.DeliveryMode;
                }

                if (salesLine.ShippingAddress == null && salesTransaction.ShippingAddress != null)
                {
                    salesLine.ShippingAddress = new Address();
                    salesLine.ShippingAddress.CopyPropertiesFrom(salesTransaction.ShippingAddress);
                }

                if (string.IsNullOrWhiteSpace(salesLine.InventoryLocationId))
                {
                    salesLine.InventoryLocationId = salesTransaction.InventoryLocationId;
                }
            }

            /// <summary>
            /// Gets all the periodic discounts on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            public static void LoadAllPeriodicDiscounts(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.GetAllPeriodicDiscounts(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                GetAllPeriodicDiscountsServiceRequest request = new GetAllPeriodicDiscountsServiceRequest(transaction);
                context.Execute<GetPriceServiceResponse>(request);
            }

            /// <summary>
            /// Add or updates tender lines on the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">Current transaction.</param>
            /// <param name="updatedTenderLine">The tender line to be updated or added.</param>
            public static void AddOrUpdateTenderLine(RequestContext context, SalesTransaction salesTransaction, TenderLineBase updatedTenderLine)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(updatedTenderLine, "updatedTenderLine");

                TenderLine tenderLineOnCart;

                // Keeps track of the tender line being created or updated.
                Dictionary<string, TenderLine> tenderLineByLineId = salesTransaction.TenderLines.ToDictionary(sl => sl.TenderLineId, sl => sl);

                if (updatedTenderLine.TenderLineId == null || !tenderLineByLineId.ContainsKey(updatedTenderLine.TenderLineId))
                {
                    AccountDepositHelper.ValidateCustomerAccountDepositPaymentRestrictions(context, salesTransaction, updatedTenderLine);

                    if (updatedTenderLine is TenderLine && ((TenderLine)updatedTenderLine).IsPreProcessed)
                    {
                        tenderLineOnCart = (TenderLine)updatedTenderLine;
                    }
                    else
                    {
                        tenderLineOnCart = CartWorkflowHelper.ConvertToTenderLine((CartTenderLine)updatedTenderLine);
                    }

                    if (tenderLineOnCart.Status != TenderLineStatus.Historical)
                    {
                        if (!salesTransaction.IsDiscountFullyCalculated)
                        {
                            switch (salesTransaction.CartType)
                            {
                                case CartType.AccountDeposit:
                                case CartType.IncomeExpense:
                                    {
                                        break;
                                    }

                                default:
                                    {
                                        if (salesTransaction.CustomerOrderMode != CustomerOrderMode.Return && salesTransaction.CustomerOrderMode != CustomerOrderMode.Pickup)
                                        {
                                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DiscountMustBeCalculatedBeforeCheckout, "Discount must be calculated before making a payment.");
                                        }

                                        break;
                                    }
                            }
                        }

                        // Authorize the tender line and add it to cart.
                        ServiceRequest authorizeRequest;
                        if (tenderLineOnCart.IsPreProcessed)
                        {
                            authorizeRequest = new AuthorizePaymentServiceRequest(salesTransaction, tenderLineOnCart, skipLimitValidation: false);
                        }
                        else if (((CartTenderLine)updatedTenderLine).TokenizedPaymentCard != null)
                        {
                            authorizeRequest = new AuthorizeTokenizedCardPaymentServiceRequest(tenderLineOnCart, ((CartTenderLine)updatedTenderLine).TokenizedPaymentCard);
                        }
                        else
                        {
                            authorizeRequest = new AuthorizePaymentServiceRequest(salesTransaction, tenderLineOnCart);
                        }

                        IRequestHandler paymentManagerHandler = context.Runtime.GetRequestHandler(authorizeRequest.GetType(), PaymentManagerServiceName);
                        AuthorizePaymentServiceResponse authorizeResponse = context.Execute<AuthorizePaymentServiceResponse>(authorizeRequest, paymentManagerHandler);

                        // Add the tender line to cart
                        if (authorizeResponse.TenderLine == null)
                        {
                            throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToAuthorizePayment, "Payment service did not return tender line.");
                        }

                        tenderLineOnCart = authorizeResponse.TenderLine;
                    }

                    tenderLineOnCart.TenderLineId = Guid.NewGuid().ToString("N");
                    tenderLineByLineId.Add(tenderLineOnCart.TenderLineId, tenderLineOnCart);
                }
                else
                {
                    // existing tender line
                    tenderLineOnCart = tenderLineByLineId[updatedTenderLine.TenderLineId];

                    // If the pre-processed tender line already exists, but the status has changed to Committed, remove the existing tender line from the cart, and add the new one.
                    if (updatedTenderLine.Status == TenderLineStatus.Committed && tenderLineOnCart.Status == TenderLineStatus.PendingCommit)
                    {
                        if (tenderLineOnCart.IsPreProcessed)
                        {
                            var captureRequest = new CapturePaymentServiceRequest((TenderLine)updatedTenderLine, salesTransaction);
                            IRequestHandler paymentManagerHandler = context.Runtime.GetRequestHandler(captureRequest.GetType(), PaymentManagerServiceName);
                            CapturePaymentServiceResponse response = context.Execute<CapturePaymentServiceResponse>(captureRequest, paymentManagerHandler);
                            tenderLineOnCart.Status = response.TenderLine.Status;
                            tenderLineOnCart.Authorization = response.TenderLine.Authorization;
                            tenderLineOnCart.CardToken = response.TenderLine.CardToken;
                            tenderLineOnCart.IsVoidable = response.TenderLine.IsVoidable;
                        }
                        else
                        {
                            throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Tender line can be captured explicitly only if it is pre-processed.");
                        }
                    }

                    CartLineValidationResults validationResults = new CartLineValidationResults();

                    // Validate readonly properties
                    foreach (DataValidationFailure failure in ReadOnlyAttribute.CheckReadOnlyProperties(updatedTenderLine, tenderLineOnCart))
                    {
                        validationResults.AddLineResult(0, failure);
                    }

                    if (validationResults.HasErrors)
                    {
                        foreach (var validationResult in validationResults.ValidationResults)
                        {
                            foreach (var validationFailure in validationResult.Value)
                            {
                                NetTracer.Information("Tender line validation failure. Line id: {0}. Details: {1}", validationResult.Key, validationFailure.ToString());
                            }
                        }

                        throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, salesTransaction.Id, validationResults, "Validation failures occurred when verifying tender line for update.");
                    }
                }

                // Update the tender line with signature.
                tenderLineOnCart.SignatureData = updatedTenderLine.SignatureData;

                // Add or update reason code lines on tender line.
                ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnTenderLine(tenderLineOnCart, updatedTenderLine, salesTransaction.Id);

                // Calculate the required reason codes on the tender line.
                ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTenderLine(context, salesTransaction, tenderLineOnCart, ReasonCodeSourceType.None);

                // Update the tender lines.
                salesTransaction.TenderLines.Clear();
                salesTransaction.TenderLines.AddRange(tenderLineByLineId.Values);

                // Update amount due
                CartWorkflowHelper.Calculate(context, salesTransaction, CalculationModes.AmountDue);
            }

            /// <summary>
            /// Voids tender lines on the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="tenderLineBase">The tender line to be updated or added.</param>
            /// <param name="salesTransaction">Current transaction.</param>
            public static void VoidTenderLine(RequestContext context, TenderLineBase tenderLineBase, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(tenderLineBase, "tenderLineBase");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                // Keeps track of the tender line being created or updated.
                Dictionary<string, TenderLine> tenderLineByLineId = salesTransaction.TenderLines.ToDictionary(sl => sl.TenderLineId, sl => sl);

                // existing tender line
                TenderLine tenderLineOnCart = tenderLineByLineId[tenderLineBase.TenderLineId];
                if (tenderLineOnCart.Status == TenderLineStatus.Voided)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentAlreadyVoided, "The payment has been voided already.");
                }

                if (tenderLineBase is TenderLine)
                {
                    tenderLineOnCart.IsPreProcessed = ((TenderLine)tenderLineBase).IsPreProcessed;
                }

                var voidPaymentServiceRequest = new VoidPaymentServiceRequest(salesTransaction, tenderLineOnCart);
                IRequestHandler paymentManagerService = context.Runtime.GetRequestHandler(voidPaymentServiceRequest.GetType(), PaymentManagerServiceName);

                var response = context.Execute<VoidPaymentServiceResponse>(voidPaymentServiceRequest, paymentManagerService);

                tenderLineByLineId[response.TenderLine.TenderLineId] = response.TenderLine;
                tenderLineOnCart = response.TenderLine;

                // Calculate the required reason codes on the tender line for voiding payments.
                ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTenderLine(context, salesTransaction, tenderLineOnCart, ReasonCodeSourceType.VoidPayment);

                // Update the tender lines.
                salesTransaction.TenderLines.Clear();
                salesTransaction.TenderLines.AddRange(tenderLineByLineId.Values);

                // Update amount due
                CartWorkflowHelper.Calculate(context, salesTransaction, CalculationModes.AmountDue);
            }

            /// <summary>
            /// Sets the loyalty card in the cart from the customer.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cart">The cart with updates or new cart from the client.</param>
            public static void SetLoyaltyCardFromCustomer(RequestContext context, Cart cart)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(cart, "cart");

                if (!string.IsNullOrWhiteSpace(cart.CustomerId))
                {
                    // Find the loyalty cards by customer number
                    var request = new GetCustomerLoyaltyCardsDataRequest(cart.CustomerId)
                    {
                        QueryResultSettings = QueryResultSettings.AllRecords
                    };
                    var response = context.Runtime.Execute<EntityDataServiceResponse<LoyaltyCard>>(request, context);

                    if (response != null && response.PagedEntityCollection.Results != null && response.PagedEntityCollection.Results.Count > 0)
                    {
                        List<LoyaltyCard> activeLoyaltyCards = (from c in response.PagedEntityCollection.Results
                                                                where c.CardTenderType != LoyaltyCardTenderType.Blocked
                                                                select c).ToList();

                        // Only when the customer has a single active loyalty card, we add the card to the transaction.
                        if (activeLoyaltyCards.Count == 1)
                        {
                            cart.LoyaltyCardId = activeLoyaltyCards.Single().CardNumber;
                        }
                    }
                    else
                    {
                        // No loyalty card for customer hence remove LoyaltyCard from the cart
                        cart.LoyaltyCardId = string.Empty;
                    }
                }
            }

            /// <summary>
            /// Validate user permissions on cart.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="newCart">The new cart being updated.</param>
            /// <param name="context">The request context.</param>
            public static void ValidateCartPermissions(SalesTransaction transaction, Cart newCart, RequestContext context)
            {
                ValidateCommonPermissionsForCart(transaction, newCart, context);
            }

            /// <summary>
            /// Generates a new random transaction id.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>
            /// The new transaction id.
            /// </returns>
            public static string GenerateRandomTransactionId(RequestContext context)
            {
                GenerateUrlSafeSecureIdentifierServiceRequest identifierRequest = new GenerateUrlSafeSecureIdentifierServiceRequest(32);
                return context.Execute<GenerateUrlSafeSecureIdentifierServiceResponse>(identifierRequest).GeneratedIdentifier;
            }

            /// <summary>
            /// Calculates the paid and due amounts for the transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The transaction to calculate the amount due for.</param>
            /// <returns>Updated transaction.</returns>
            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Dynamics.Retail.Diagnostics.NetTracer.Warning(System.String)", Justification = "Logging, not user message.")]
            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CalculateAmountsPaidAndDue", Justification = "Method name")]
            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CartWorkflowHelper", Justification = "Class name")]
            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CartType", Justification = "Property name")]
            public static SalesTransaction CalculateAmountsPaidAndDue(RequestContext context, SalesTransaction salesTransaction)
            {
                CalculateAmountPaidAndDueServiceRequest calculateRequest = new CalculateAmountPaidAndDueServiceRequest(salesTransaction);
                return context.Execute<CalculateAmountPaidAndDueServiceResponse>(calculateRequest).Transaction;
            }

            /// <summary>
            /// Validate whether the employee/cashier is able to perform return item or return transaction within the permissible limit.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction object.</param>
            /// <param name="cartType">The cart type.</param>
            public static void ValidateReturnPermission(RequestContext context, SalesTransaction salesTransaction, CartType cartType)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                // Set to true if the transaction contains any return item.
                bool isReturnTransaction = salesTransaction.ActiveSalesLines.Any(l => l.Quantity < 0);

                cartType = cartType == CartType.None ? salesTransaction.CartType : cartType;

                if (!isReturnTransaction || (cartType == CartType.CustomerOrder))
                {
                    return;
                }

                // The permission validation logic is copied from EPOS (ItemSale.cs) for return item and transaction.
                EmployeePermissions employeePermisssion = EmployeePermissionHelper.GetEmployeePermissions(context, context.GetPrincipal().UserId);
                decimal maxLineReturnAmount = employeePermisssion.MaximumLineReturnAmount;
                decimal maxTotalReturnAmount = employeePermisssion.MaxTotalReturnAmount;
                var activeSalesLines = salesTransaction.ActiveSalesLines;

                // Check if the return item permission is satisified
                if (maxLineReturnAmount > 0)
                {
                    foreach (SalesLine salesLine in activeSalesLines)
                    {
                        if ((salesLine.Quantity < 0) &&
                            (decimal.Negate(salesLine.TotalAmount) > maxLineReturnAmount))
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReturnItemPriceExceeded,
                                string.Format(CultureInfo.CurrentUICulture, "The return item price exceeds the limit set for user: {0}", context.GetPrincipal().UserId));
                        }
                    }
                }

                // Check if the return transaction permission is satisified
                decimal totalReturnAmount = activeSalesLines.Where(l => l.Quantity < 0).Sum(l => l.TotalAmount);

                if ((maxTotalReturnAmount > 0) &&
                    (decimal.Negate(totalReturnAmount) > maxTotalReturnAmount))
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReturnTransactionTotalExceeded,
                        string.Format(CultureInfo.CurrentUICulture, "The return transaction total amount exceeds the limit set for user: {0}", context.GetPrincipal().UserId));
                }
            }

            /// <summary>
            /// Tries to delete a cart represented by the <paramref name="salesTransaction"/> and traces warning in case of failure.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The transaction representing the cart.</param>
            public static void TryDeleteCart(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                try
                {
                    var deleteCartRequest = new DeleteCartDataRequest(new[] { salesTransaction.Id });
                    context.Runtime.Execute<NullResponse>(deleteCartRequest, context);
                }
                catch (Exception ex)
                {
                    NetTracer.Warning(ex, ex.Message);
                }
            }

            /// <summary>
            /// Validates price on sales line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="productsByRecordId">Products by record identifier.</param>
            /// <remarks>This method cannot to be executed before price calculation.</remarks>
            public static void ValidateSalesLinePrice(RequestContext context, SalesTransaction salesTransaction, IDictionary<long, SimpleProduct> productsByRecordId)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(productsByRecordId, "productsByRecordId");

                CartLineValidationResults validationResults = new CartLineValidationResults();                
                int lineIndex = 0;

                foreach (SalesLine salesLine in salesTransaction.SalesLines)
                {
                    if (salesLine.IsProductLine && !salesLine.IsReturnByReceipt && !salesLine.IsVoided)
                    {
                        // we only need to validate zero price on lines being changed / added
                        // curently we only load products for lines being changed / added
                        SimpleProduct product;

                        if (productsByRecordId.TryGetValue(salesLine.ProductId, out product))
                        {
                            ProductBehavior rules = product.Behavior;
                            bool isPriceSetByClient = salesLine.IsPriceKeyedIn || salesLine.IsPriceOverridden;

                            if (!rules.IsZeroSalePriceAllowed && salesLine.Price == decimal.Zero)
                            {
                                validationResults.AddLineResult(
                                    lineIndex,
                                    new DataValidationFailure(
                                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ZeroPriceIsNotAllowed,
                                        "Selected product cannot have zero price. ProductId: {0}.",
                                        product.RecordId));
                            }

                            if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore
                                && !isPriceSetByClient 
                                && salesLine.Price == 0 
                                && context.GetDeviceConfiguration().MustKeyInPriceIfZero)
                            {
                                validationResults.AddLineResult(
                                    lineIndex,
                                    new DataValidationFailure(
                                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MustKeyInNewPrice,
                                        "No price was entered for this product. ProductId: {0}.",
                                        product.RecordId));
                            }
                        }
                    }

                    lineIndex++;
                }

                if (validationResults.HasErrors)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, salesTransaction.Id, validationResults, "Validation failures occurred when verifying cart prices.");
                }
            }

            /// <summary>
            /// Add or update affiliations automatically during customer added, updated or removed.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="existingTransaction">The sales transaction.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            internal static void AddOrUpdateAffiliationLinesFromCustomer(RequestContext context, SalesTransaction existingTransaction, Cart newCart)
            {
                // Check whether the customer account number is updated.
                // 1. Add a customer: The old cart does not have a customer or there is no cart; the new cart has a customer.
                // 2. Clear the customer: The old cart already has a customer; the new cart has the customer number set to empty.
                // 3. Change a customer: Both the old cart and the new cart have a customer number set, but they are different.
                bool addCustomer = false;
                bool clearCustomer = false;
                bool updateCustomer = false;

                if (newCart.AffiliationLines == null)
                {
                    newCart.AffiliationLines = new List<AffiliationLoyaltyTier>();
                    if (existingTransaction != null)
                    {
                        // If we are not modifying the affiliations on the cart copy existing ones from the sales transaction.
                        CopyAffiliationLoyaltyTierToCart(existingTransaction, newCart);
                    }
                }

                if (existingTransaction != null)
                {
                    addCustomer = string.IsNullOrWhiteSpace(existingTransaction.CustomerId) && !string.IsNullOrWhiteSpace(newCart.CustomerId);
                    clearCustomer = !addCustomer
                                           && !string.IsNullOrWhiteSpace(existingTransaction.CustomerId)
                                           && newCart.CustomerId != null
                                           && newCart.CustomerId.Length == 0;
                    updateCustomer = !addCustomer
                                           && !clearCustomer
                                           && !string.IsNullOrWhiteSpace(existingTransaction.CustomerId)
                                           && !string.IsNullOrWhiteSpace(newCart.CustomerId)
                                           && !newCart.CustomerId.Equals(existingTransaction.CustomerId, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    addCustomer = !string.IsNullOrWhiteSpace(newCart.CustomerId);
                }

                bool customerChanged = addCustomer || clearCustomer || updateCustomer;

                if (context.GetChannelConfiguration() == null)
                {
                    throw new ArgumentException("ChannelConfiguration is not set in context", "context");
                }

                // If the request comes from RetailStore, the customer affiliations will be added to the Cart only when the customer is changed.
                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore)
                {
                    if (customerChanged)
                    {
                        UpdateCartWithFilteredAffiliations(existingTransaction, newCart, true);

                        if (addCustomer || updateCustomer)
                        {
                            AddCustomerAffiliationsToCart(context, newCart);
                        }
                    }
                }
                else if (context.GetChannelConfiguration().ChannelType == RetailChannelType.OnlineStore ||
                    context.GetChannelConfiguration().ChannelType == RetailChannelType.SharePointOnlineStore)
                {
                    // If the request comes from OnlineStore, then re-add the customer and Loyalty affiliations to the Cart forcibly,
                    // because during the shopping process, the customer's affiliations may be changed.
                    UpdateCartWithFilteredAffiliations(existingTransaction, newCart, false);
                    AddCustomerAffiliationsToCart(context, newCart);
                }
            }

            /// <summary>
            /// Validates whether the customer account can be updated.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">The cart with updates or new cart from the client.</param>
            /// <param name="existingTransaction">The existing sales transaction from the DB.</param>
            internal static void ValidateCustomerAccount(RequestContext context, Cart newCart, SalesTransaction existingTransaction)
            {
                // Trims the customerId of new cart if it contains only blank
                if (!string.IsNullOrEmpty(newCart.CustomerId)
                    && string.IsNullOrWhiteSpace(newCart.CustomerId))
                {
                    newCart.CustomerId = string.Empty;
                }

                // Check if CustomerId of new cart is nonempty and does not equal to the existing CustomerId if existing transaction exists
                if (!string.IsNullOrEmpty(newCart.CustomerId)
                    && !(existingTransaction != null && newCart.CustomerId.Equals(existingTransaction.CustomerId)))
                {
                    var getCustomerDataRequest = new GetCustomerDataRequest(newCart.CustomerId);
                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest, context);
                    Customer customer = getCustomerDataResponse.Entity;

                    // Check if the customer exists and has the same CustomerId
                    if (customer == null || !customer.AccountNumber.Equals(newCart.CustomerId))
                    {
                        throw new CartValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerNotFound, 
                            newCart.Id,
                            string.Format("Customer \"{0}\" not found", newCart.CustomerId));
                    }

                    // Avoid adding Blocked customer to transaction.
                    if (customer.Blocked)
                    {
                        string message = string.Format("Failed to add customer '{0}' to cart. Blocked customers are not allowed for transactions.", customer.AccountNumber);
                        throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountIsBlocked, message);
                    }

                    // Avoid adding Blocked Invoice accounts into transaction.
                    if (!string.IsNullOrWhiteSpace(customer.InvoiceAccount))
                    {
                        getCustomerDataRequest = new GetCustomerDataRequest(customer.InvoiceAccount);
                        getCustomerDataResponse = context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest, context);
                        Customer invoiceAccountCustomer = getCustomerDataResponse.Entity;
                        if (invoiceAccountCustomer != null && invoiceAccountCustomer.Blocked)
                        {
                            string message = string.Format("Failed to add customer due to blocked invoice account '{0}'. Blocked accounts are not allowed for transactions.", customer.InvoiceAccount);
                            throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerInvoiceAccountIsBlocked, message);
                        }
                    }
                }
            }

            /// <summary>
            /// Validates whether the quantity change is allowed.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="existingLine">Existing line.</param>
            /// <param name="updatedLine">Updated line.</param>
            /// <param name="validationFailures">Collection of validation failures.</param>
            internal static void ValidateQuantityChangeAllowed(RequestContext context, SalesLine existingLine, CartLine updatedLine, Collection<DataValidationFailure> validationFailures)
            {
                if (Math.Abs(existingLine.Quantity) != Math.Abs(updatedLine.Quantity))
                {
                    // Validates set quantity permission
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SetQuantity));

                    // Changing quantity for linked lines is not allowed.
                    if (!string.IsNullOrWhiteSpace(existingLine.LinkedParentLineId))
                    {
                        validationFailures.Add(new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_OperationNotAllowedOnLinkedProduct,
                                "Changing quantity on linked cart line is not allowed. Line number: {0}",
                                existingLine.LineNumber));
                    }
                }
            }

            /// <summary>
            /// Gets identifier of transaction that is being returned.
            /// </summary>
            /// <param name="updatedCart">Updated cart (changed lines only).</param>
            /// <param name="originalCart">Original cart.</param>
            /// <returns>Transaction identifier or 'Null' if no transaction being returned.</returns>
            private static string ValidateAndGetReturnTransactionId(Cart updatedCart, SalesTransaction originalCart)
            {
                // First get original return transaction ids.
                Dictionary<string, string> returnTransactionIdsPerLine = originalCart.ActiveSalesLines
                    .Where(l => l.IsReturnByReceipt && !string.IsNullOrWhiteSpace(l.ReturnTransactionId))
                    .ToDictionary(k => k.LineId, v => v.ReturnTransactionId);

                // Remove transaction ids that were voided.
                IEnumerable<string> voidedCartLineIds = updatedCart.CartLines.Where(l => l.IsVoided).Select(l => l.LineId);
                foreach (string lineId in voidedCartLineIds)
                {
                    returnTransactionIdsPerLine.Remove(lineId);
                }

                // Do not expect violations here because existing cart was already validated.
                string originalReturnTransactionId = returnTransactionIdsPerLine.Values.Distinct().SingleOrDefault();

                // Get newly added return transaction ids.
                IEnumerable<string> newReturnTransactionIds = updatedCart.CartLines
                    .Where(l => string.IsNullOrEmpty(l.LineId) && l.LineData != null && l.LineData.IsReturnByReceipt && !string.IsNullOrWhiteSpace(l.LineData.ReturnTransactionId))
                    .Select(l => l.ReturnTransactionId).Distinct();

                string newReturnTransactionId = null;

                if (!newReturnTransactionIds.IsNullOrEmpty())
                {
                    // Check if several cart lines were added with different return transaction ids.
                    if (newReturnTransactionIds.HasMultiple())
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotReturnMultipleTransactions,
                            string.Format("Only one transaction can be returned per cart. Return transaction ids: {0}", string.Join(", ", newReturnTransactionIds)));
                    }

                    // Check if existing cart (non voided lines) and newly added lines return different transactions.
                    newReturnTransactionId = newReturnTransactionIds.Single();
                    if (!string.IsNullOrWhiteSpace(originalReturnTransactionId) && (originalReturnTransactionId != newReturnTransactionId))
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotReturnMultipleTransactions,
                            string.Format("Only one transaction can be returned per cart. Existing return transaction id: {0}, new return transaction id {1}", originalReturnTransactionId, newReturnTransactionId));
                    }
                }

                return newReturnTransactionId ?? originalReturnTransactionId;
            }

            /// <summary>
            /// Adds affiliations with the type General to the cart.
            /// </summary>
            /// <param name="existingTransaction">The sales transaction.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            /// <param name="isRetailStore">True is the request is done against Retail Store cart.</param>
            /// <remarks>If the request was done not against Retail Store cart then affiliations like Loyalty or Unknown or those who have customer assigned are not added while copying from the SalesTransaction.</remarks>
            private static void UpdateCartWithFilteredAffiliations(SalesTransaction existingTransaction, Cart newCart, bool isRetailStore)
            {
                if (existingTransaction != null)
                {
                    // Only keep the affiliations added manually and loyalty card affiliations, remove the old customer affiliations.
                    IEnumerable<AffiliationLoyaltyTier> affiliations = from a in existingTransaction.AffiliationLoyaltyTierLines
                                                                       where string.IsNullOrWhiteSpace(a.CustomerId)
                                                                        && !IsExistSameAffiliation(a.AffiliationId, newCart.AffiliationLines)
                                                                        && ((!isRetailStore && (a.AffiliationType == RetailAffiliationType.Loyalty || a.AffiliationType == RetailAffiliationType.Unknown)) || isRetailStore)
                                                                       select new AffiliationLoyaltyTier
                                                                       {
                                                                           AffiliationId = a.AffiliationId,
                                                                           AffiliationType = a.AffiliationType,
                                                                           LoyaltyTierId = 0,
                                                                           ReasonCodeLines = a.ReasonCodeLines
                                                                       };

                    newCart.AffiliationLines.AddRange(affiliations);
                }
            }

            /// <summary>
            /// Adds customer's affiliations to Cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            private static void AddCustomerAffiliationsToCart(RequestContext context, Cart newCart)
            {
                // If customer ID is specified, means the cart is not for anonymous customer, then try to retrieve affiliations.
                // this check is needed to avoid performance issue - the method CustomerDataManager.GetCustomers retrieves all information about customers
                // according to input creteria, but if the input criteria is not specified then the method will retrive all customers from the DB which is not needed in this case.
                if (!string.IsNullOrWhiteSpace(newCart.CustomerId))
                {
                    // Get the customer affiliations.
                    var getCustomersServiceRequest = new GetCustomersServiceRequest(QueryResultSettings.SingleRecord, newCart.CustomerId);
                    var getCustomerServiceResponse = context.Execute<GetCustomersServiceResponse>(getCustomersServiceRequest);
                    var customer = getCustomerServiceResponse.Customers.SingleOrDefault();
                    IList<CustomerAffiliation> customerAffiliations = null;
                    if (customer != null)
                    {
                        customerAffiliations = customer.CustomerAffiliations;
                    }

                    if (customerAffiliations != null && customerAffiliations.Count > 0)
                    {
                        // Select affiliations that are not already in Cart from customer affiliations.
                        IEnumerable<AffiliationLoyaltyTier> cartAffiliationLoyaltyTiers = customerAffiliations.Where(cA => { return !IsExistSameAffiliation(cA.RetailAffiliationId, newCart.AffiliationLines); }).Select(
                           cA => new AffiliationLoyaltyTier()
                           {
                               AffiliationId = cA.RetailAffiliationId,
                               AffiliationType = RetailAffiliationType.General,
                               CustomerId = newCart.CustomerId,
                               LoyaltyTierId = 0,
                           });

                        newCart.AffiliationLines.AddRange(cartAffiliationLoyaltyTiers);
                    }
                }
            }

            /// <summary>
            ///  Copies the sales transaction affiliations to the cart.
            /// </summary>
            /// <param name="salesTransaction">Sales transaction that contains affiliations.</param>
            /// <param name="cart">Cart to copy affiliations to.</param>
            private static void CopyAffiliationLoyaltyTierToCart(SalesTransaction salesTransaction, Cart cart)
            {
                if (cart.AffiliationLines == null)
                {
                    cart.AffiliationLines = new List<AffiliationLoyaltyTier>();
                }

                foreach (SalesAffiliationLoyaltyTier tier in salesTransaction.AffiliationLoyaltyTierLines)
                {
                    AffiliationLoyaltyTier affiliationLoyaltyTier = new AffiliationLoyaltyTier { AffiliationId = tier.AffiliationId, LoyaltyTierId = tier.LoyaltyTierId, AffiliationType = tier.AffiliationType, CustomerId = tier.CustomerId };
                    affiliationLoyaltyTier.CopyPropertiesFrom(tier);

                    affiliationLoyaltyTier.ReasonCodeLines.Clear();
                    affiliationLoyaltyTier.ReasonCodeLines.AddRange(tier.ReasonCodeLines);
                    cart.AffiliationLines.Add(affiliationLoyaltyTier);
                }
            }

            /// <summary>
            ///  Copies the cart affiliations to the sales transaction.
            /// </summary>
            /// <param name="cart">Cart that contains affiliations.</param>
            /// <param name="salesTransaction">SalesTransaction to copy affiliations to.</param>
            private static void CopyAffiliationLoyaltyTierToSalesTransaction(Cart cart, SalesTransaction salesTransaction)
            {
                if (salesTransaction.AffiliationLoyaltyTierLines == null)
                {
                    salesTransaction.AffiliationLoyaltyTierLines = new Collection<SalesAffiliationLoyaltyTier>();
                }

                foreach (AffiliationLoyaltyTier tier in cart.AffiliationLines)
                {
                    SalesAffiliationLoyaltyTier affiliationLoyaltyTier = new SalesAffiliationLoyaltyTier { AffiliationId = tier.AffiliationId, LoyaltyTierId = tier.LoyaltyTierId, AffiliationType = tier.AffiliationType, CustomerId = tier.CustomerId };
                    affiliationLoyaltyTier.CopyPropertiesFrom(tier);

                    affiliationLoyaltyTier.ReasonCodeLines.Clear();
                    affiliationLoyaltyTier.ReasonCodeLines.AddRange(tier.ReasonCodeLines);
                    salesTransaction.AffiliationLoyaltyTierLines.Add(affiliationLoyaltyTier);
                }
            }

            /// <summary>
            /// Perform save operations for cart lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="request">The request object.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="returnTransaction">Transaction that is being returned.</param>
            /// <param name="salesLineByLineId">The mapping sales line id to sales line object.</param>
            /// <param name="productByRecordId">The mapping of products by record identifier.</param>
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            private static void PerformSaveCartLineOperations(
                RequestContext context,
                SaveCartRequest request,
                SalesTransaction transaction,
                SalesTransaction returnTransaction,
                Dictionary<string, SalesLine> salesLineByLineId,
                IDictionary<long, SimpleProduct> productByRecordId)
            {
                List<SalesLine> addedLines = new List<SalesLine>();
                CartType cartType = transaction.CartType;

                bool cartChangesContainLinkedProducts = productByRecordId.Any(p => !p.Value.LinkedProducts.IsNullOrEmpty());
                foreach (CartLine cartLine in request.Cart.CartLines)
                {
                    SalesLine salesLine;
                    ReasonCodeSourceType reasonCodeSourceType = ReasonCodeSourceType.None;

                    SimpleProduct product;
                    long productId = cartLine.LineData != null ? cartLine.LineData.ProductId : salesLineByLineId[cartLine.LineId].ProductId;
                    bool isProductFound = productByRecordId.TryGetValue(productId, out product);

                    foreach (ReasonCodeLine reasonCodeLine in cartLine.ReasonCodeLines)
                    {
                        // Reason code type is not provide by client so we need to set it for line aggregation.
                        reasonCodeLine.LineType = ReasonCodeLineType.Sales;
                    }

                    if (!cartChangesContainLinkedProducts && AggregateSalesLines(context, transaction.ActiveSalesLines, cartLine, product))
                    {
                        // If item is aggregated simply increase quantity and skip additional lookups.
                        continue;
                    }

                    if (!transaction.SalesLines.Any(i => i.LineId == cartLine.LineId))
                    {
                        salesLine = new SalesLine();
                        if (!cartLine.LineData.IsReturnByReceipt)
                        {
                            // Creates a sales line base on the cart line
                            salesLine.CopyPropertiesFrom(cartLine.LineData);

                            // Set ItemId and VariantInventDimId of the sales line, if the cart line is constructed from listing.
                            if (cartLine.LineData.IsProductLine)
                            {
                                if (!isProductFound)
                                {
                                    string message = string.Format("The specified product identifier ({0}) could not be found.", cartLine.LineData.ProductId);
                                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, message);
                                }

                                if (product.IsRemote && cartType != CartType.CustomerOrder)
                                {
                                    string message = string.Format("Failed to add product '{0}' to cart. Remote products are only supported in customer order mode.", cartLine.LineData.ProductId);
                                    throw new DataValidationException(
                                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RemoteProductsNotSupportedWithCurrentTransactionType, message);
                                }

                                salesLine.ProductSource = product.IsRemote ? ProductSource.Remote : ProductSource.Local;
                                salesLine.ItemId = product.ItemId;
                                salesLine.InventoryDimensionId = product.InventoryDimensionId;
                                salesLine.ProductId = cartLine.ProductId;
                                salesLine.Variant = ProductVariant.ConvertFrom(product);
                            }
                        }
                        else
                        {
                            // Creates a sales line base on the retuned sales line - return is by receipt
                            var returnedSalesLine = GetSalesLineByNumber(
                                returnTransaction, cartLine.LineData.ReturnLineNumber);
                            SetSalesLineBasedOnReturnedSalesLine(
                                salesLine, returnedSalesLine, returnTransaction, cartLine.LineData.Quantity);

                            // Set reason code source type for return transaction - return is using receipt.
                            reasonCodeSourceType = ReasonCodeSourceType.ReturnTransaction;
                        }

                        salesLine.Found = true;

                        if (string.IsNullOrEmpty(salesLine.LineId))
                        {
                            AssignUniqueLineId(salesLine);
                        }

                        if (!cartLine.LineData.IsReturnByReceipt && !cartLine.IsCustomerAccountDeposit)
                        {
                            addedLines.Add(salesLine);
                        }

                        salesLineByLineId.Add(salesLine.LineId, salesLine);
                    }
                    else
                    {
                        // If no line data is set, it means remove
                        if (cartLine.LineData == null)
                        {
                            salesLine = salesLineByLineId[cartLine.LineId];

                            // Removing the linked products' sales lines if any.
                            if (salesLine.LineIdsLinkedProductMap.Any())
                            {
                                foreach (string lineId in salesLine.LineIdsLinkedProductMap.Keys)
                                {
                                    salesLineByLineId.Remove(lineId);
                                }
                            }

                            // Removing the reference to the linked product from the parent product sales line if the linked product is removed from cart.
                            if (!string.IsNullOrWhiteSpace(salesLine.LinkedParentLineId))
                            {
                                salesLineByLineId[salesLine.LinkedParentLineId].LineIdsLinkedProductMap.Remove(salesLine.LineId);
                            }

                            salesLineByLineId.Remove(cartLine.LineId);
                        }
                        else
                        {
                            salesLine = salesLineByLineId[cartLine.LineId];

                            if (!salesLine.IsReturnByReceipt)
                            {
                                // If voiding salesline, void the sales lines of the linked products if any.
                                if (cartLine.IsVoided && !salesLine.IsVoided && salesLine.LineIdsLinkedProductMap.Any())
                                {
                                    foreach (string lineId in salesLine.LineIdsLinkedProductMap.Keys)
                                    {
                                        if (salesLineByLineId[lineId] != null)
                                        {
                                            salesLineByLineId[lineId].IsVoided = true;
                                        }
                                        else
                                        {
                                            throw new DataValidationException(
                                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, 
                                                string.Format("Sales line of the linked product with id : {0} was not found.", lineId));
                                        }
                                    }
                                }

                                // If unvoiding salesline, unvoid the sales lines of the linked products if any.
                                if (!cartLine.IsVoided && salesLine.IsVoided && salesLine.LineIdsLinkedProductMap.Any())
                                {
                                    foreach (string lineId in salesLine.LineIdsLinkedProductMap.Keys)
                                    {
                                        if (salesLineByLineId[lineId] != null)
                                        {
                                            salesLineByLineId[lineId].IsVoided = false;
                                        }
                                        else
                                        {
                                            throw new DataValidationException(
                                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, 
                                                string.Format("Sales line of the linked product with id : {0} was not found.", lineId));
                                        }
                                    }
                                }

                                // Copy the properties from the cart line
                                salesLine.CopyPropertiesFrom(cartLine.LineData);

                                // we have to preserve the LineId, regardless what is set on line data
                                salesLine.LineId = cartLine.LineId;
                                salesLine.IsPriceOverridden = cartLine.IsPriceOverridden;

                                // Set reason code source type for prive override
                                if (cartLine.IsPriceOverridden)
                                {
                                    reasonCodeSourceType = ReasonCodeSourceType.OverridePrice;
                                }
                            }
                            else
                            {
                                // For return
                                // Keep the properties on the sales line and only copy the quantity from the cart line
                                salesLine.Quantity = cartLine.LineData.Quantity;

                                // Set reason code source type for return item.
                                reasonCodeSourceType = ReasonCodeSourceType.ReturnItem;
                            }

                            // If user provides an empty address, we need to empty current address, since property copy will not change the address
                            if (cartLine.ShippingAddress != null && cartLine.ShippingAddress.IsEmpty())
                            {
                                salesLine.ShippingAddress = new Address();
                            }

                            // if the sales line has linked sales lines, update the quantity of linked sales line also.
                            if (salesLine.LineIdsLinkedProductMap != null)
                            {
                                foreach (string lineId in salesLine.LineIdsLinkedProductMap.Keys)
                                {
                                    decimal linkedProductQty = salesLine.LineIdsLinkedProductMap[lineId].Quantity;
                                    salesLineByLineId[lineId].Quantity = salesLine.Quantity * (linkedProductQty <= 0 ? 1 : linkedProductQty);
                                }
                            }
                        }
                    }

                    // Process reason code lines on the cart line.
                    ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnSalesLine(
                        salesLine, cartLine, transaction.Id);

                    // Calculate the required reason code on the sales line.
                    if (transaction.CartType != CartType.CustomerOrder)
                    {
                        // calculate required reason codes for discounts
                        if ((salesLine.LineManualDiscountAmount != 0 || salesLine.LineManualDiscountPercentage != 0)
                            && !salesLine.IsReturnByReceipt)
                        {
                            ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnSalesLine(context, transaction, salesLine, ReasonCodeSourceType.ItemDiscount);
                        }

                        ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnSalesLine(context, transaction, salesLine, reasonCodeSourceType);
                    }
                }

                decimal lineNumber = 1;
                foreach (var salesLine in salesLineByLineId.Values)
                {
                    salesLine.LineNumber = lineNumber++;
                }

                SetLinkedProductLineReferences(addedLines, productByRecordId);

                // Validate quantity limit for the added or updated cart lines
                CartWorkflowHelper.ValidateSalesLineQuantityLimit(context, request.Cart.CartLines);
            }

            /// <summary>
            /// Validate permission for unit of measure operation.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cart">The Cart request.</param>
            /// <param name="salesLines">The sales line value.</param>
            private static void CheckChangeUnitOfMeasureOperation(RequestContext context, Cart cart, IEnumerable<SalesLine> salesLines)
            {
                foreach (CartLine cartLine in cart.CartLines)
                {
                    if (!string.IsNullOrWhiteSpace(cartLine.UnitOfMeasureSymbol))
                    {
                        SalesLine salesLine = salesLines.Where(sl => sl.LineId == cartLine.LineId)
                                                                    .SingleOrDefault();

                        if (salesLine != null && (cartLine.UnitOfMeasureSymbol != salesLine.UnitOfMeasureSymbol))
                        {
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ChangeUnitOfMeasure));
                        }
                    }
                }
            }

            /// <summary>
            /// Set the unit of measure conversion in sales line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="conversions">The list of item unit conversions on sales lines.</param>
            /// <returns>The collection of unit of measure conversions.</returns>
            private static ReadOnlyCollection<UnitOfMeasureConversion> GetUnitOfMeasureConversions(RequestContext context, IEnumerable<ItemUnitConversion> conversions)
            {
                var getUomConvertionDataRequest = new GetUnitOfMeasureConversionDataRequest(conversions, QueryResultSettings.AllRecords);
                return context.Runtime.Execute<GetUnitOfMeasureConversionDataResponse>(getUomConvertionDataRequest, context).UnitConversions.Results;
            }

            /// <summary>
            /// Gets the calculation modes based on the operation being performed.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>The filtered calculation modes.</returns>
            private static CalculationModes GetCalculationModes(RequestContext context, SalesTransaction transaction)
            {
                switch (transaction.CartType)
                {
                    case CartType.Checkout:
                    case CartType.Shopping:
                        return CalculationModes.All;

                    case CartType.IncomeExpense:
                    case CartType.AccountDeposit:
                        return CalculationModes.Totals | CalculationModes.AmountDue;

                    case CartType.CustomerOrder:
                        return CustomerOrderWorkflowHelper.GetCalculationModes(context, transaction);

                    default:
                        string message = string.Format(CultureInfo.InvariantCulture, "Cart type {0} is not supported.", transaction.CartType);
                        throw new NotSupportedException(message);
                }
            }

            /// <summary>
            /// Sets the references between parent and linked lines (if any).
            /// </summary>
            /// <param name="addedLines">The collection of added lines.</param>
            /// <param name="productsById">The map between the product ids and the products.</param>
            private static void SetLinkedProductLineReferences(List<SalesLine> addedLines, IDictionary<long, SimpleProduct> productsById)
            {
                foreach (SalesLine line in addedLines)
                {
                    if (!string.IsNullOrWhiteSpace(line.LinkedParentLineId))
                    {
                        // nested linked products are not supported so if this line was already claimed as "linked" skip it.
                        continue;
                    }

                    if (!line.IsProductLine)
                    {
                        // if not a product line, it cannot be a linked product
                        continue;
                    }
                    
                    SimpleProduct addedProduct;
                    if (!productsById.TryGetValue(line.ProductId, out addedProduct))
                    {
                        string errorMessage = string.Format("Product id {0}, from cart line {1}, was expected on the 'productsById' dictionary, but it was not found.", line.ProductId, line.LineId);
                        throw new InvalidOperationException(errorMessage);
                    }

                    ICollection<SimpleLinkedProduct> linkedProducts = addedProduct.LinkedProducts;
                    if (linkedProducts.IsNullOrEmpty())
                    {
                        continue;
                    }

                    foreach (SimpleLinkedProduct linkedProduct in linkedProducts)
                    {
                        // linked products are configured between master products this is why looking by item id.
                        SalesLine linkedLine = addedLines.FirstOrDefault(l => l.ItemId.Equals(linkedProduct.ItemId, StringComparison.OrdinalIgnoreCase));

                        if (linkedLine == null)
                        {
                            // currently there is not requirement to fail if linked product(s) were not added to the cart.
                            continue;
                        }

                        // set references between parent and linked lines.
                        linkedLine.LinkedParentLineId = line.LineId;
                        line.LineIdsLinkedProductMap.Add(
                            linkedLine.LineId,
                            new LinkedProduct()
                            {
                                ProductRecordId = addedProduct.RecordId,
                                LinkedProductRecordId = linkedProduct.RecordId,
                                Quantity = linkedProduct.Quantity
                            });
                    }
                }
            }

            /// <summary>
            /// Performs invoice line specific validations for the transaction.
            /// </summary>
            /// <param name="newCart">The new cart.</param>
            /// <param name="salesTransaction">The old transaction.</param>
            /// <param name="validationResults">The validation results.</param>
            private static void ValidateTransactionWithInvoiceLines(Cart newCart, SalesTransaction salesTransaction, CartLineValidationResults validationResults)
            {
                bool hasInvoiceLines = newCart.CartLines.Any(c => c.IsInvoiceLine);

                if (hasInvoiceLines)
                {
                    DataValidationFailure failure;

                    // Customer is read-only if there are invoice lines.
                    ReadOnlyAttribute.AssertPropertyNotChanged(RetailTransactionTableSchema.CustomerIdColumn, newCart, salesTransaction, out failure);
                    if (failure != null)
                    {
                        validationResults.AddLineResult(0, failure);
                    }
                }
            }

            /// <summary>
            /// Calculates the totals on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Updated transaction.</returns>
            private static SalesTransaction CalculateTotals(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.CalculateTotals(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                CalculateTotalsServiceRequest request = new CalculateTotalsServiceRequest(transaction);
                return context.Execute<CalculateTotalsServiceResponse>(request).Transaction;
            }

            /// <summary>
            /// Gets the sales lines prices for each item on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Updated transaction.</returns>
            private static SalesTransaction CalculatePrices(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.CalculatePrices(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                UpdatePriceServiceRequest updatePriceRequest = new UpdatePriceServiceRequest(transaction);
                return context.Execute<GetPriceServiceResponse>(updatePriceRequest).Transaction;
            }

            /// <summary>
            /// Calculate the charges on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Updated transaction.</returns>
            private static SalesTransaction CalculateCharges(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.CalculateCharges(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                GetChargesServiceRequest request = new GetChargesServiceRequest(transaction);
                return context.Execute<GetChargesServiceResponse>(request).Transaction;
            }

            /// <summary>
            /// Calculate the discounts on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="discountCalculationMode">The discount calculation mode.</param>
            /// <returns>Updated transaction.</returns>
            private static SalesTransaction CalculateDiscounts(RequestContext context, SalesTransaction transaction, DiscountCalculationMode? discountCalculationMode)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.CalculateDiscounts(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                SalesTransaction cart = transaction;

                if (discountCalculationMode == null)
                {
                    // If delayed discount calculation is enabled and calculation was not triggered previously only calculate simple discount.
                    if (DelayMulitpleItemDiscountCalculation(context, cart))
                    {
                        discountCalculationMode = DiscountCalculationMode.CalculateOffer;
                    }
                    else
                    {
                        discountCalculationMode = DiscountCalculationMode.CalculateAll;
                    }
                }

                CalculateDiscountsServiceRequest request = new CalculateDiscountsServiceRequest(transaction, discountCalculationMode.Value);
                return context.Execute<GetPriceServiceResponse>(request).Transaction;
            }

            /// <summary>
            /// Gets the sales lines independent prices and discounts for each item on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="isItemSaleForSure">A flag indicating whether it is an item sale.</param>
            /// <param name="newSalesLineIdSet">New sales line id set.</param>
            /// <returns>Updated transaction.</returns>
            private static SalesTransaction CalculateIndependentPricesAndDiscounts(RequestContext context, SalesTransaction transaction, bool isItemSaleForSure, HashSet<string> newSalesLineIdSet)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.CalculateIndependentPricesAndDiscounts(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                GetIndependentPriceDiscountServiceRequest priceAndDiscountRequest = new GetIndependentPriceDiscountServiceRequest(transaction, isItemSaleForSure, newSalesLineIdSet);
                return context.Execute<GetPriceServiceResponse>(priceAndDiscountRequest).Transaction;
            }

            private static bool DelayMulitpleItemDiscountCalculation(RequestContext context, SalesTransaction transaction)
            {
                bool delayMulitpleItemDiscountCalculation = !transaction.IsDiscountFullyCalculated;

                if (delayMulitpleItemDiscountCalculation)
                {
                    bool manuallyCalculateDiscounts = false;
                    if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore)
                    {
                        DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();
                        manuallyCalculateDiscounts = deviceConfiguration.ManuallyCalculateComplexDiscounts;
                    }

                    delayMulitpleItemDiscountCalculation = manuallyCalculateDiscounts;
                }

                return delayMulitpleItemDiscountCalculation;
            }

            /// <summary>
            /// Update tax override codes on the transaction and lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="cart">The cart.</param>
            private static void UpdateTaxOverrideCodes(RequestContext context, SalesTransaction transaction, Cart cart)
            {
                // copy cart level tax override
                if (!string.IsNullOrWhiteSpace(cart.TaxOverrideCode))
                {
                    transaction.TaxOverrideCode = cart.TaxOverrideCode;

                    ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTransaction(context, transaction, ReasonCodeSourceType.TransactionTaxChange);
                }

                // copy line level tax override
                // inspect line level overrides
                // note: it is not a meaningful business case to have both cart and line level overrides, but technically it is still possible
                // on the off chance, this happens, line's will override cart-inherited overrides
                IList<CartLine> cartLines = cart.CartLines.Where(p => !p.IsVoided).AsReadOnly();

                foreach (CartLine cartLine in cartLines)
                {
                    if (!string.IsNullOrWhiteSpace(cartLine.TaxOverrideCode))
                    {
                        // update line
                        var salesLine = transaction.SalesLines.Single(p => string.Equals(p.LineId, cartLine.LineId, StringComparison.OrdinalIgnoreCase));
                        salesLine.TaxOverrideCode = cartLine.TaxOverrideCode;

                        ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnSalesLine(context, transaction, salesLine, ReasonCodeSourceType.LineItemTaxChange);
                    }
                }
            }

            /// <summary>
            /// Aggregate sales line with existing lines.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="existingLines">Collection of existing lines.</param>
            /// <param name="updatedOrAddedLine">Updated or newly added line.</param>
            /// <param name="product">The product.</param>
            /// <returns>
            /// True if line is aggregated; False otherwise.
            /// </returns>
            private static bool AggregateSalesLines(RequestContext context, IList<SalesLine> existingLines, CartLine updatedOrAddedLine, SimpleProduct product)
            {
                if (!context.GetPrincipal().IsInRole(CommerceRoles.Employee))
                {
                    return false;
                }

                if (existingLines.Any(l => l.LineId == updatedOrAddedLine.LineId))
                {
                    // Line already exists. Skip aggregation.
                    return false;
                }

                var deviceConfiguration = context.GetDeviceConfiguration();

                if (deviceConfiguration.AllowItemsAggregation)
                {
                    foreach (SalesLine existingLine in existingLines)
                    {
                        if (AllowAggregation(updatedOrAddedLine, existingLine, product))
                        {
                            // Updating line to represent a quantity update.
                            existingLine.Quantity += updatedOrAddedLine.Quantity;
                            updatedOrAddedLine.LineData.CopyPropertiesFrom(existingLine);
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Checks if lines can be aggregated.
            /// </summary>
            /// <param name="newLine">Newly added line.</param>
            /// <param name="existingLine">Existing line.</param>
            /// <param name="product">The product.</param>
            /// <returns>True if lines can be aggregated; False otherwise.</returns>
            private static bool AllowAggregation(CartLine newLine, SalesLine existingLine, SimpleProduct product)
            {
                if (existingLine.IsGiftCardLine || newLine.IsGiftCardLine)
                {
                    return false;
                }

                if (existingLine.IsVoided || newLine.IsVoided)
                {
                    return false;
                }

                if (existingLine.ProductId != newLine.ProductId)
                {
                    // If non-variant product is added by barcode only ItemId is set on the line.
                    if (newLine.ProductId != 0 || (existingLine.ItemId != newLine.ItemId))
                    {
                        return false;
                    }
                }

                if (existingLine.SerialNumber != newLine.SerialNumber)
                {
                    return false;
                }

                if (product != null && product.Behavior.MustPromptForSerialNumberOnlyAtSale)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(newLine.UnitOfMeasureSymbol) && !existingLine.SalesOrderUnitOfMeasure.Equals(newLine.UnitOfMeasureSymbol, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if ((newLine.IsPriceOverridden || existingLine.IsPriceOverridden) && (newLine.Price != existingLine.Price))
                {
                    return false;
                }

                if ((newLine.IsPriceKeyedIn || existingLine.IsPriceKeyedIn) && (newLine.Price != existingLine.Price))
                {
                    return false;
                }

                if (existingLine.LinkedParentLineId != null || existingLine.LineIdsLinkedProductMap.Any())
                {
                    return false;
                }

                if (Math.Sign(newLine.Quantity) != Math.Sign(existingLine.Quantity))
                {
                    return false;
                }

                if (existingLine.IsReturnByReceipt)
                {
                    return false;
                }

                string existingComment = existingLine.Comment ?? string.Empty;
                string newComment = newLine.Comment ?? string.Empty;
                if (!existingComment.Equals(newComment, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                IEnumerable<ReasonCodeLine> existingReasonCodes = existingLine.ReasonCodeLines ?? Enumerable.Empty<ReasonCodeLine>();
                IEnumerable<ReasonCodeLine> newReasonCodes = newLine.ReasonCodeLines ?? Enumerable.Empty<ReasonCodeLine>();
                if (!existingReasonCodes.SequenceEqual(newReasonCodes))
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Check permissions for operations that are common for both customer orders and sales orders cart types.
            /// Such as comment, adding a customer etc.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="newCart">The new cart being updated.</param>
            /// <param name="context">The request context.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength", Justification = "String will be null in case of patch when no change is to be made")]
            private static void ValidateCommonPermissionsForCart(SalesTransaction transaction, Cart newCart, RequestContext context)
            {
                // Check if the comment has been added.
                if (!string.IsNullOrWhiteSpace(newCart.Comment))
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.TransactionComment));
                }

                if (transaction == null)
                {
                    // Check if a customer is being added.
                    if (!string.IsNullOrWhiteSpace(newCart.CustomerId))
                    {
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SetCustomer));
                    }

                    // Check if a loyalty card is being added
                    if (!string.IsNullOrWhiteSpace(newCart.LoyaltyCardId))
                    {
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.LoyaltyRequest));
                    }
                }
                else
                {
                    bool isCustomerIdSetOnNewCart = newCart.IsPropertyDefined(c => c.CustomerId);

                    if (isCustomerIdSetOnNewCart)
                    {
                        if (string.IsNullOrWhiteSpace(transaction.CustomerId) && !string.IsNullOrWhiteSpace(newCart.CustomerId))
                        {
                            // customer being added
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SetCustomer));
                        }
                        else if (!string.IsNullOrWhiteSpace(transaction.CustomerId) && string.IsNullOrWhiteSpace(newCart.CustomerId))
                        {
                            // customer being removed
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.CustomerClear));
                        }
                        else if (!string.IsNullOrWhiteSpace(transaction.CustomerId)
                            && !string.IsNullOrWhiteSpace(newCart.CustomerId)
                            && !transaction.CustomerId.Equals(newCart.CustomerId))
                        {
                            // customer being changed
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SetCustomer));
                        }
                    }

                    // Check if a loyalty card is being added or updated
                    bool addLoyaltyCard = string.IsNullOrWhiteSpace(transaction.LoyaltyCardId) && !string.IsNullOrWhiteSpace(newCart.LoyaltyCardId);
                    bool updateLoyaltyCard = !addLoyaltyCard
                                            && !string.IsNullOrWhiteSpace(transaction.LoyaltyCardId)
                                            && !string.IsNullOrWhiteSpace(newCart.LoyaltyCardId)
                                            && !newCart.LoyaltyCardId.Equals(transaction.LoyaltyCardId, StringComparison.OrdinalIgnoreCase);

                    if (addLoyaltyCard || updateLoyaltyCard)
                    {
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.LoyaltyRequest));
                    }
                }
            }

            /// <summary>
            /// Validate whether the employee/cashier is able to set discounts within the permissible limit.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="newCart">New cart.</param>
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Adding discount validation")]
            private static void ValidateEmployeeDiscountPermission(RequestContext context, SalesTransaction transaction, Cart newCart)
            {
                if (newCart == null)
                {
                    // no changes to validate
                    return;
                }

                EmployeePermissions employeePermissions = null;
                string currentEmployeeId = context.GetPrincipal().UserId;

                // Avoid discount type mix by amount and percentage.
                if (newCart.TotalManualDiscountAmount != 0 &&
                    newCart.TotalManualDiscountPercentage != 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MultipleEmployeeTotalDiscountsNotAllowed, "Total Discount Amount and Percentage cannot be applied at the same time.");
                }

                // Detect discount amount change, and validate access and limits for this operation.
                if (newCart.TotalManualDiscountAmount != 0 && newCart.TotalManualDiscountAmount != transaction.TotalManualDiscountAmount)
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.TotalDiscountAmount));
                    employeePermissions = EmployeePermissionHelper.GetEmployeePermissions(context, currentEmployeeId);

                    if (employeePermissions.MaximumTotalDiscountAmount != 0
                            && newCart.TotalManualDiscountAmount != 0
                            && newCart.TotalManualDiscountAmount > employeePermissions.MaximumTotalDiscountAmount)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeDiscountExceeded, string.Format(CultureInfo.CurrentUICulture, "Total Discount Amount exceeds the limit set for employee: {0}", currentEmployeeId));
                    }
                }

                // Detect discount percentage change, and validate access and limits for this operation.
                if (newCart.TotalManualDiscountPercentage != 0 && newCart.TotalManualDiscountPercentage != transaction.TotalManualDiscountPercentage)
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.TotalDiscountPercent));

                    if (employeePermissions == null)
                    {
                        employeePermissions = EmployeePermissionHelper.GetEmployeePermissions(context, currentEmployeeId);
                    }

                    if (employeePermissions.MaximumTotalDiscountPercentage != 0
                            && newCart.TotalManualDiscountPercentage != 0
                            && newCart.TotalManualDiscountPercentage > employeePermissions.MaximumTotalDiscountPercentage)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeDiscountExceeded, string.Format(CultureInfo.CurrentUICulture, "Total Discount Percentage exceeds the limit set for employee: {0}", currentEmployeeId));
                    }
                }

                // Validate new cart lines for discount application.
                foreach (var cartLine in newCart.CartLines)
                {
                    bool lineDiscountAmountAccessGranted = false;
                    bool lineDiscountPercentAccessGranted = false;
                    if (cartLine.LineManualDiscountAmount != 0 && cartLine.LineManualDiscountPercentage != 0)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MultipleEmployeeLineDiscountsNotAllowed, "Line Discount Amount and Percentage cannot be applied at the same time.");
                    }

                    if (cartLine.LineManualDiscountAmount != 0 || cartLine.LineManualDiscountPercentage != 0)
                    {
                        // find corresponding sales line in current transaction.
                        SalesLine salesLine = transaction.SalesLines.SingleOrDefault(sl => sl.LineId == cartLine.LineId);
                        if (!lineDiscountAmountAccessGranted 
                            && (salesLine == null || salesLine.LineManualDiscountAmount != cartLine.LineManualDiscountAmount))
                        {
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.LineDiscountAmount));
                            lineDiscountAmountAccessGranted = true;
                        }

                        if (!lineDiscountPercentAccessGranted
                            && (salesLine == null || salesLine.LineManualDiscountPercentage != cartLine.LineManualDiscountPercentage))
                        {
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.LineDiscountPercent));
                            lineDiscountPercentAccessGranted = true;
                        }

                        if (employeePermissions == null)
                        {
                            employeePermissions = EmployeePermissionHelper.GetEmployeePermissions(context, currentEmployeeId);
                        }

                        if (employeePermissions.MaximumLineDiscountAmount != 0
                            && cartLine.LineManualDiscountAmount != 0 
                            && cartLine.LineManualDiscountAmount > employeePermissions.MaximumLineDiscountAmount)
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeDiscountExceeded, string.Format(CultureInfo.CurrentUICulture, "Line Discount Amount exceeds the limit set for employee: {0}", currentEmployeeId));
                        }

                        if (employeePermissions.MaximumDiscountPercentage != 0
                            && cartLine.LineManualDiscountPercentage != 0 
                            && cartLine.LineManualDiscountPercentage > employeePermissions.MaximumDiscountPercentage)
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeDiscountExceeded, string.Format(CultureInfo.CurrentUICulture, "Line Discount Percentage exceeds the limit set for employee: {0}", currentEmployeeId));
                        }
                    }
                }
            }

            /// <summary>
            /// Calculates taxes on the sales line for each item on the context's sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Updated transaction.</returns>
            private static SalesTransaction CalculateTaxes(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                NetTracer.Information("CartWorkflowHelper.CalculateTaxes(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);

                // Compute tax on items
                CalculateTaxServiceRequest calculateTaxRequest = new CalculateTaxServiceRequest(transaction);
                return context.Execute<CalculateTaxServiceResponse>(calculateTaxRequest).Transaction;
            }

            /// <summary>
            /// Rounds the grand total on the sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            private static void RoundTotals(RequestContext context, SalesTransaction transaction)
            {
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();

                GetRoundedValueServiceRequest request = new GetRoundedValueServiceRequest(
                    transaction.TotalAmount,
                    channelConfiguration.Currency,
                    numberOfDecimals: 0,
                    useSalesRounding: false);
                GetRoundedValueServiceResponse response = context.Execute<GetRoundedValueServiceResponse>(request);

                transaction.TotalAmount = response.RoundedValue;
            }

            /// <summary>
            /// Gets the sales transaction inventory location and site identifiers.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            private static void GetSalesTransactionWarehouseInformation(RequestContext context, SalesTransaction transaction)
            {
                if (string.IsNullOrWhiteSpace(transaction.InventoryLocationId))
                {
                    ChannelConfiguration configuration = context.GetChannelConfiguration();

                    transaction.InventoryLocationId = configuration.InventLocation;
                }
            }

            #region Cart Validations
            /// <summary>
            /// Validates whether two operations are supposed to be performed on the same sales line.
            /// </summary>
            /// <param name="cartLines">The cart line collection.</param>
            /// <param name="lineIdSet">The line id set.</param>
            /// <param name="cartLineValidationResults">The cart line validation results.</param>
            private static void ValidateConflicts(IEnumerable<CartLine> cartLines, HashSet<string> lineIdSet, CartLineValidationResults cartLineValidationResults)
            {
                int index = 0;
                foreach (CartLine salesLine in cartLines)
                {
                    if (salesLine == null)
                    {
                        // If we have no sales line, there's no conflict.
                        continue;
                    }

                    DataValidationFailure result = null;
                    if (lineIdSet.Contains(salesLine.LineId))
                    {
                        result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ConflictingCartLineOperation);
                    }

                    if (!string.IsNullOrWhiteSpace(salesLine.LineId))
                    {
                        lineIdSet.Add(salesLine.LineId);
                    }

                    if (result != null)
                    {
                        cartLineValidationResults.AddLineResult(index, result);
                    }

                    index++;
                }
            }

            /// <summary>
            /// Validates whether sales lines have conflicts on return transaction identifier or return line numbers.
            /// </summary>
            /// <param name="returnedSalesTransaction">The returned sales transaction.</param>
            /// <param name="cartLines">The cart line collection.</param>
            /// <param name="cartLineValidationResults">The cart line validation results.</param>
            private static void ValidateReturnConflicts(SalesTransaction returnedSalesTransaction, IList<CartLine> cartLines, CartLineValidationResults cartLineValidationResults)
            {
                string returnTransactionId = null;
                if (returnedSalesTransaction != null)
                {
                    returnTransactionId = returnedSalesTransaction.Id;
                }

                List<decimal> returnLineNumberList = new List<decimal>();
                foreach (CartLine cl in cartLines)
                {
                    if (cl != null && cl.LineData != null && cl.LineData.IsReturnByReceipt)
                    {
                        DataValidationFailure result = null;

                        if (string.IsNullOrWhiteSpace(returnTransactionId) || !returnTransactionId.Equals(cl.LineData.ReturnTransactionId, StringComparison.OrdinalIgnoreCase))
                        {
                            // Check whehter the return transaction is found and whether the cart line matches the return transaction Id
                            result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ConflictingCartLineOperation);
                        }
                        else if (returnLineNumberList.Contains(cl.LineData.ReturnLineNumber))
                        {
                            // Check whether the return line number already exists in the cart
                            result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ConflictingCartLineOperation);
                        }
                        else
                        {
                            returnLineNumberList.Add(cl.LineData.ReturnLineNumber);
                        }

                        if (result != null)
                        {
                            cartLineValidationResults.AddLineResult(cartLines.IndexOf(cl), result);
                        }
                    }
                }
            }

            /// <summary>
            /// Validates whether the update operations can be performed.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            /// <param name="existingTransaction">The existing sales transaction from the DB.</param>
            /// <param name="salesLineByLineId">The dictionary of sales lines by line id.</param>
            /// <param name="returnSalesLineByLineNumber">The dictionary of return sales lines by line number.</param>
            /// <param name="isGiftCardOperation">True if request is a result of gift card operation.</param>
            /// <param name="cartLineValidationResults">The cart line validation results.</param>
            /// <param name="productByRecordId">The mapping of products by record identifier.</param>
            private static void ValidateSalesLineOperations(
                RequestContext context,
                Cart newCart,
                SalesTransaction existingTransaction,
                Dictionary<string, SalesLine> salesLineByLineId,
                Dictionary<decimal, SalesLine> returnSalesLineByLineNumber,
                bool isGiftCardOperation,
                CartLineValidationResults cartLineValidationResults,
                IDictionary<long, SimpleProduct> productByRecordId)
            {
                int index = 0;
                bool checkReturnItemOperation = true;

                foreach (CartLine cartLine in newCart.CartLines)
                {
                    if (cartLine == null || cartLine.LineData == null)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                                    "Cart line or cart line data cannot be null.");

                        cartLineValidationResults.AddLineResult(index, validationFailure);
                    }
                    else
                    {
                        // check for return item permission
                        // if ReturnTransactionId is present, this is return by receipt, covered by ReturnTransaction operation and should not be checked here
                        if (checkReturnItemOperation && string.IsNullOrWhiteSpace(cartLine.LineData.ReturnTransactionId) && cartLine.Quantity < 0m && !cartLine.IsVoided)
                        {
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ReturnItem));
                            checkReturnItemOperation = false;
                        }

                        Collection<DataValidationFailure> results;
                        if (existingTransaction.SalesLines.All(i => i.LineId != cartLine.LineId))
                        {
                            results = CartWorkflowHelper.ValidateCartLineForAdd(context, newCart, existingTransaction, salesLineByLineId, productByRecordId, returnSalesLineByLineNumber, cartLine, isGiftCardOperation);
                        }
                        else
                        {
                            results = CartWorkflowHelper.ValidateCartLineForUpdate(context, newCart, existingTransaction, salesLineByLineId, returnSalesLineByLineNumber, cartLine);
                        }

                        foreach (DataValidationFailure dataValidationFailure in results)
                        {
                            cartLineValidationResults.AddLineResult(index, dataValidationFailure);
                        }
                    }

                    index++;
                }
            }

            /// <summary>
            /// Validates whether a line can be added.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            /// <param name="existingTransaction">The existing sales transaction from the DB.</param>
            /// <param name="salesLineByLineId">The dictionary of sales lines by line id.</param>
            /// <param name="productsById">A dictionary mapping product identifiers to products for the items in the cart.</param>
            /// <param name="returnSalesLineByLineNumber">The dictionary of return sales lines by line number.</param>
            /// <param name="cartLine">The cart line.</param>
            /// <param name="isGiftCardOperation">True if request is a result of gift card operation.</param>
            /// <returns>Return the collection of validation failures for the line.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Cart line data validation needs new objects.")]
            private static Collection<DataValidationFailure> ValidateCartLineForAdd(
                RequestContext context,
                Cart newCart,
                SalesTransaction existingTransaction,
                Dictionary<string, SalesLine> salesLineByLineId,
                IDictionary<long, SimpleProduct> productsById,
                IDictionary<decimal, SalesLine> returnSalesLineByLineNumber,
                CartLine cartLine,
                bool isGiftCardOperation)
            {
                // If cart type is None it means type has not been changed so we check cart type on existing cart.
                CartType cartType = newCart.CartType == CartType.None ? existingTransaction.CartType : newCart.CartType;

                Collection<DataValidationFailure> validationFailures = new Collection<DataValidationFailure>();

                validationFailures.AddRange(ReadOnlyAttribute.CheckReadOnlyProperties(cartLine.LineData));

                // Verify that IsGiftCardLine property is set only when executing gift card operation and vice versa.
                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore && isGiftCardOperation != cartLine.LineData.IsGiftCardLine)
                {
                    DataValidationFailure validationFailure = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PropertyUpdateNotAllowed);
                    validationFailures.Add(validationFailure);
                }

                if (!cartLine.LineData.IsReturnByReceipt)
                {
                    // If the quantity is greater than zero, the sales line is a normal sale.
                    // If the quantity is less than zero, it is a negative sale (manual return).
                    if (cartLine.LineData.Quantity == 0)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SalesMustHaveQuantityGreaterThanZero,
                            "Quantity must be greater than 0 for sales.");

                        validationFailures.Add(validationFailure);
                    }

                    if (cartLine.LineData.IsProductLine)
                    {
                        SimpleProduct product;

                        if (!productsById.TryGetValue(cartLine.LineData.ProductId, out product))
                        {
                            string message = string.Format("The specified product identifier ({0}) could not be found.", cartLine.LineData.ProductId);
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, message);
                        }

                        if (!product.IsDistinct)
                        {
                            NotifyProductMaster(context, cartLine.ProductId);
                        }

                        ValidateProductForSale(context, product, cartType, cartLine, validationFailures);
                    }
                }
                else
                {
                    // Cart line for return
                    if (string.IsNullOrWhiteSpace(cartLine.LineData.ReturnTransactionId)
                        || cartLine.LineData.ReturnLineNumber <= 0
                        || returnSalesLineByLineNumber == null)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineAdd,
                            "ReturnTransactionId must be set, along with return line number.");

                        validationFailures.Add(validationFailure);

                        // cannot proceed with validations if we failed this one
                        return validationFailures;
                    }

                    // If the quantity is less than zero, it is a negative sale (manual return).
                    if (cartLine.LineData.Quantity >= 0)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReturnsMustHaveQuantityLesserThanZero,
                            "Quantity must be lesser than 0 for returns.");

                        validationFailures.Add(validationFailure);
                    }

                    // Check whether the return transaction ID matches the existing cart lines for return
                    // and whether the ReturnLineNumber is already in the cart
                    foreach (var sl in salesLineByLineId.Values)
                    {
                        if (!sl.IsVoided && sl.IsReturnByReceipt
                            && (!cartLine.LineData.ReturnTransactionId.Equals(sl.ReturnTransactionId, StringComparison.OrdinalIgnoreCase)
                                || sl.ReturnLineNumber == cartLine.LineData.ReturnLineNumber))
                        {
                            DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineAdd,
                                "ReturnTransactionId or ReturnLineNumber do not match expected value.");

                            validationFailures.Add(validationFailure);
                            return validationFailures;
                        }
                    }

                    // Check whether the cart line is allowed for return
                    // The returnSaleLine cannot be voided
                    // The returnSaleLine must have the item Id
                    // The return quantity cannot exceed the quantity
                    var returnSalesLine = returnSalesLineByLineNumber[cartLine.LineData.ReturnLineNumber];
                    if (returnSalesLine == null
                        || string.IsNullOrWhiteSpace(returnSalesLine.ItemId)
                        || returnSalesLine.IsVoided)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineAdd,
                                "Original sales line was not found or line is not valid.");

                        validationFailures.Add(validationFailure);
                        return validationFailures;
                    }

                    if (returnSalesLine.Quantity < returnSalesLine.ReturnQuantity + Math.Abs(cartLine.LineData.Quantity))
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotReturnMoreThanPurchased,
                                "It is not allowed to return more quantities than the amount purchased.");

                        validationFailures.Add(validationFailure);
                    }
                }

                if (cartType == CartType.CustomerOrder)
                {
                    CustomerOrderWorkflowHelper.ValidateCartLineForAdd(cartLine, newCart, existingTransaction, validationFailures);
                }

                return validationFailures;
            }

            private static void ValidateProductForSale(RequestContext context, SimpleProduct product, CartType cartType, CartLine cartLine, ICollection<DataValidationFailure> validationFailures)
            {
                ProductBehavior rules = product.Behavior;

                if (rules == null)
                {
                    return;
                }

                DateTime currentDate = context.GetNowInChannelTimeZone().Date;

                DataValidationFailure validationFailure;
                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore
                    && (currentDate < rules.ValidFromDateForSaleAtPhysicalStores.Date
                    || currentDate >= rules.ValidToDateForSaleAtPhysicalStores.Date))
                {
                    validationFailure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ProductIsNotActive,
                        "Selected product has not been activated for sale. ProductId: {0}",
                        product.RecordId);

                    validationFailures.Add(validationFailure);
                }

                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore && !rules.IsSaleAtPhysicalStoresAllowed)
                {
                    validationFailure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ProductIsBlocked,
                        "Selected product is blocked and cannot be sold. ProductId: {0}",
                        product.RecordId);

                    validationFailures.Add(validationFailure);
                }

                if (rules.HasSerialNumber
                        && (cartType != CartType.CustomerOrder
                        || (cartType == CartType.CustomerOrder && !rules.MustPromptForSerialNumberOnlyAtSale)))
                {
                    if (string.IsNullOrWhiteSpace(cartLine.SerialNumber))
                    {
                        // Throw data serial number validation error if:
                        // * Cart type is non customer order, product has serial number properties, but serial number is null.
                        // * Cart type is customer order, product has serial number properties, serial number is null,
                        //   and product does not have active in sales process.
                        // If cart type is customer order and product has active in sales process, do not capture serial number.
                        // Serial number will be captured when pick up the products or doing packing / invoice (for shipped products).
                        validationFailure = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SerialNumberMissing, "Serial number for item {0} is missing.", product.ItemId);

                        validationFailures.Add(validationFailure);
                    }

                    if (cartLine.Quantity > 1)
                    {
                        validationFailure = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SerializedQuantityGreaterThanOne, "Products with serial number cannot be added with quantity greater than one.");

                        validationFailures.Add(validationFailure);
                    }
                }

                ValidateProductPriceForSale(context, product, cartLine, validationFailures);
            }

            /// <summary>
            /// Validates product prices on the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="product">The product on the cart line.</param>
            /// <param name="cartLine">The cart line.</param>
            /// <param name="validationFailures">The collection of validation results.</param>
            /// <remarks>This method is executed before full cart price calculation and relies on product price.
            /// Product price is only calculated if cartLine.IsPriceKeyedIn is true, thus all validations in this method can only depend
            /// on product.Price if cartLine.IsPriceKeyedIn is true. For any other price validations that need price,
            /// please add them on <see cref="ValidateSalesLinePrice"/> that happens after full cart price calculations.</remarks>
            private static void ValidateProductPriceForSale(RequestContext context, SimpleProduct product, CartLine cartLine, ICollection<DataValidationFailure> validationFailures)
            {
                ProductBehavior rules = product.Behavior;
                DataValidationFailure validationFailure;

                bool isPriceSetByClient = cartLine.IsPriceKeyedIn || cartLine.IsPriceOverridden;
                decimal productPrice = product.AdjustedPrice;
                decimal linePrice = isPriceSetByClient ? cartLine.Price : productPrice;

                if ((context.GetChannelConfiguration().ChannelType == RetailChannelType.OnlineStore ||
                    context.GetChannelConfiguration().ChannelType == RetailChannelType.SharePointOnlineStore) &&
                    isPriceSetByClient && linePrice < decimal.Zero)
                {
                    validationFailure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPriceEncountered,
                        "Selected product cannot have negative price. ProductId: {0}",
                        product.RecordId);

                    validationFailures.Add(validationFailure);
                }

                switch (rules.KeyInPrice)
                {
                    case KeyInPriceRestriction.NotAllowed:
                        if (cartLine.IsPriceKeyedIn)
                        {
                            validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EnteringPriceNotAllowed,
                                "Entering a price is not allowed for this product. ProductId: {0}. Product price: {1}. Cart line price: {2}.",
                                product.RecordId,
                                productPrice,
                                cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }

                        break;
                    case KeyInPriceRestriction.HigherOrEqualPrice:
                        if (!cartLine.IsPriceKeyedIn)
                        {
                            validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MustKeyInNewPrice,
                                "No price was entered for this product. ProductId: {0}. Product price: {1}. Cart line price: {2}.",
                                product.RecordId,
                                productPrice,
                                cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }
                        else if (cartLine.Price < productPrice)
                        {
                            validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MustKeyInEqualHigherPrice,
                                "Price entered for this product must be equal or higher than the product's original price. ProductId: {0}. Product price: {1}. Cart line price: {2}.",
                                product.RecordId,
                                productPrice,
                                cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }

                        // If the request is coming from online store then setting the price should not be allowed even if the value
                        // for KeyInPrices is MustKeyInEqualHigherPrice. We only allow C2 customer to enter a price when the value of KeyInPrices is MustKeyInNewPrice.
                        if (cartLine.IsPriceKeyedIn && (context.GetChannelConfiguration().ChannelType == RetailChannelType.OnlineStore ||
                            context.GetChannelConfiguration().ChannelType == RetailChannelType.SharePointOnlineStore))
                        {
                            validationFailure = new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EnteringPriceNotAllowed,
                            "Entering a price is not allowed for this product and a channel with channel type {0}. ProductId: {1}. Product price: {2}. Cart line price: {3}.",
                            context.GetChannelConfiguration().ChannelType,
                            product.RecordId,
                            productPrice,
                            cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }

                        break;
                    case KeyInPriceRestriction.LowerOrEqualPrice:
                        if (!cartLine.IsPriceKeyedIn)
                        {
                            validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MustKeyInNewPrice,
                                "No price was entered for this product. ProductId: {0}. Product price: {1}. Cart line price: {2}.",
                                product.RecordId,
                                productPrice,
                                cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }
                        else if (cartLine.Price > productPrice)
                        {
                            validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MustKeyInEqualLowerPrice,
                                "Price entered for this product must be equal or lower than the product's original price. ProductId: {0}. Product price: {1}. Cart line price: {2}.",
                                product.RecordId,
                                productPrice,
                                cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }

                        // If the request is coming from online store then setting the price should not be allowed even if the value
                        // for KeyInPrices is MustKeyInEqualLowerPrice. We only allow C2 customer to enter a price when the value of KeyInPrices is MustKeyInNewPrice.
                        if (cartLine.IsPriceKeyedIn && (context.GetChannelConfiguration().ChannelType == RetailChannelType.OnlineStore ||
                            context.GetChannelConfiguration().ChannelType == RetailChannelType.SharePointOnlineStore))
                        {
                            validationFailure = new DataValidationFailure(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EnteringPriceNotAllowed,
                            "Entering a price is not allowed for this product and a channel with channel type {0}. ProductId: {1}. Product price: {2}. Cart line price: {3}.",
                            context.GetChannelConfiguration().ChannelType,
                            product.RecordId,
                            productPrice,
                            cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }

                        break;
                    case KeyInPriceRestriction.NewPrice:
                        if (!cartLine.IsPriceKeyedIn)
                        {
                            validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MustKeyInNewPrice,
                                "No price was entered for this product. ProductId: {0}. Product price: {1}. Cart line price: {2}.",
                                product.RecordId,
                                productPrice,
                                cartLine.Price);

                            validationFailures.Add(validationFailure);
                        }

                        break;
                    case KeyInPriceRestriction.None:
                        if (context.GetChannelConfiguration().ChannelType == RetailChannelType.OnlineStore ||
                              context.GetChannelConfiguration().ChannelType == RetailChannelType.SharePointOnlineStore)
                        {
                            // If the request is coming from online store then setting the price should not be allowed even if the value
                            // for KeyInPrices is NotMandatory. We only allow C2 customer to enter a price when the value of KeyInPrices is MustKeyInNewPrice.
                            if (cartLine.IsPriceKeyedIn)
                            {
                                validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EnteringPriceNotAllowed,
                                "Entering a price is not allowed for this product and a channel with channel type {0}. ProductId: {1}. Product price: {2}. Cart line price: {3}.",
                                context.GetChannelConfiguration().ChannelType,
                                product.RecordId,
                                productPrice,
                                cartLine.Price);

                                validationFailures.Add(validationFailure);
                            }
                        }

                        break;
                    default:
                        throw new NotSupportedException(string.Format("UnsupportedType value for KeyInPrice: {0}", rules.KeyInPriceValue));
                }
            }

            private static void NotifyProductMaster(RequestContext context, long productId)
            {
                // Load the product details for the given Item Id and return the product master response.
                // See how we can return the product master along with the error, for now including the Product Id in the error message.
                // The client should display the product master page with all variants to choose.
                ProductMasterPageNotification notification = new ProductMasterPageNotification(productId);
                context.Notify(notification);
            }

            /// <summary>
            /// Validates whether a line can be updated.
            /// </summary>
            /// <param name="context">The Request context.</param>
            /// <param name="newCart">The cart with updates/ new cart from the client.</param>
            /// <param name="existingTransaction">The existing sales transaction from the DB.</param>
            /// <param name="salesLineByLineId">The dictionary of sales lines by line id.</param>
            /// <param name="returnSalesLineByLineNumber">The dictionary of return sales lines by line number.</param>
            /// <param name="cartLine">The cart line.</param>
            /// <returns>Return the collection of validation failures for the line.</returns>
            private static Collection<DataValidationFailure> ValidateCartLineForUpdate(
                RequestContext context,
                Cart newCart,
                SalesTransaction existingTransaction,
                Dictionary<string, SalesLine> salesLineByLineId,
                Dictionary<decimal, SalesLine> returnSalesLineByLineNumber,
                CartLine cartLine)
            {
                Collection<DataValidationFailure> validationFailures = new Collection<DataValidationFailure>();

                if (!cartLine.IsGiftCardLine
                    && !cartLine.IsInvoiceLine
                    && string.IsNullOrWhiteSpace(cartLine.LineId))
                {
                    // Not valid. Missing required LineId field.
                    DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineUpdate,
                                "LineId is missing.");

                    validationFailures.Add(validationFailure);
                    return validationFailures;
                }

                string cartLineDataId = cartLine.LineData.LineId;
                if (cartLineDataId != null &&
                    !string.Equals(cartLineDataId, cartLine.LineId, StringComparison.Ordinal))
                {
                    // Cart.LineId did not match Cart.LineData.LineId.
                    DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineUpdate,
                                "CartLine.LineId does not match CartLineData.LineId.");

                    validationFailures.Add(validationFailure);
                    return validationFailures;
                }

                if (!salesLineByLineId.ContainsKey(cartLine.LineId))
                {
                    // Cart line to be updated was not found.
                    DataValidationFailure validationFailure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineUpdate,
                        "Cart line to be updated could not be found.");

                    validationFailures.Add(validationFailure);
                    return validationFailures;
                }

                var salesLine = salesLineByLineId[cartLine.LineId];
                validationFailures.AddRange(ReadOnlyAttribute.CheckReadOnlyProperties(cartLine.LineData, salesLine));

                // For POS stores the Gift card line should not be updated. This is not the case for the online Store front scenario.
                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore)
                {
                    if (salesLine.IsGiftCardLine != cartLine.IsGiftCardLine)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PropertyUpdateNotAllowed);
                        validationFailures.Add(validationFailure);
                    }

                    // Fail if trying to add discount on gift card.
                    if (cartLine.IsGiftCardLine && (cartLine.LineManualDiscountAmount != 0 || cartLine.LineManualDiscountPercentage != 0))
                    {
                        throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_GiftCardDiscountNotAllowed, newCart.Id);
                    }

                    // Fail if trying to update gift card line except cases when gift card line is voided.
                    if (salesLine.IsGiftCardLine && (!cartLine.IsVoided || salesLine.IsVoided))
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PropertyUpdateNotAllowed);
                        validationFailures.Add(validationFailure);
                    }
                }

                // Depending on cart type we need different validations for operations performed on cart
                CartType cartType = newCart.CartType != CartType.None ? newCart.CartType : existingTransaction.CartType;

                switch (cartType)
                {
                    case CartType.Shopping:
                    case CartType.Checkout:
                        CartWorkflowHelper.ValidateCashAndCarryCartLineForUpdate(context, salesLineByLineId, returnSalesLineByLineNumber, cartLine, validationFailures);
                        break;

                    case CartType.CustomerOrder:
                        CustomerOrderWorkflowHelper.ValidateCartLineForUpdate(context, newCart, existingTransaction, salesLineByLineId, cartLine, validationFailures);
                        break;

                    case CartType.IncomeExpense:
                    case CartType.AccountDeposit:
                        CartWorkflowHelper.ValidateNoSalesLineOnTransaction(existingTransaction, validationFailures);
                        break;

                    default:
                        string message = string.Format(CultureInfo.InvariantCulture, "Cart type {0} is not supported.", cartType);
                        throw new NotSupportedException(message);
                }

                return validationFailures;
            }

            /// <summary>
            /// Validates that the transaction does not contain any sales lines.
            /// </summary>
            /// <param name="salesTransaction">Sales transaction.</param>
            /// <param name="validationFailures">Validation failures collection.</param>
            private static void ValidateNoSalesLineOnTransaction(SalesTransaction salesTransaction, Collection<DataValidationFailure> validationFailures)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");

                // Sales line is not allowed in income/ expense and customer account deposit transactions.
                if (salesTransaction.SalesLines.Any())
                {
                    var validationFailure = new DataValidationFailure(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SalesLineNotAllowed,
                        "Sales lines are not allowed on the transaction.");

                    validationFailures.Add(validationFailure);
                }
            }

            /// <summary>
            /// Validates whether a line can be updated for a cash and carry transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesLineByLineId">The dictionary of sales lines by line id.</param>
            /// <param name="returnSalesLineByLineNumber">The dictionary of return sales lines by line number.</param>
            /// <param name="cartLine">The cart line.</param>
            /// <param name="validationFailures">The validation result collection.</param>
            private static void ValidateCashAndCarryCartLineForUpdate(RequestContext context, Dictionary<string, SalesLine> salesLineByLineId, Dictionary<decimal, SalesLine> returnSalesLineByLineNumber, CartLine cartLine, Collection<DataValidationFailure> validationFailures)
            {
                SalesLine cartlineForUpdate = salesLineByLineId[cartLine.LineId];
                ValidateQuantityChangeAllowed(context, cartlineForUpdate, cartLine, validationFailures);

                if (!cartlineForUpdate.IsReturnByReceipt)
                {
                    // For regular cart line (not return)
                    // If the quantity is greater than zero, the sales line is a normal sale.
                    // If the quantity is less than zero, it is a negative sale (manual return).
                    if (cartLine.LineData.Quantity == 0)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SalesMustHaveQuantityGreaterThanZero,
                                "Quantity must be positive.");

                        validationFailures.Add(validationFailure);
                    }

                    // No price override for gift card lines.
                    if (cartlineForUpdate.IsGiftCardLine && cartLine.IsPriceOverridden)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NoPriceOverrideForGiftCards,
                                "Cannot override price for gift card items.");

                        validationFailures.Add(validationFailure);
                    }

                    // No price override for invoice lines.
                    if (cartlineForUpdate.IsInvoiceLine && cartLine.IsPriceOverridden)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NoPriceOverrideForInvoiceLines,
                                "Cannot override price for invoice lines.");

                        validationFailures.Add(validationFailure);
                    }
                }
                else
                {
                    // For cart line for return
                    // Quantity must be less than zero
                    if (cartLine.LineData.Quantity >= 0)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReturnsMustHaveQuantityLesserThanZero,
                                "Quantity must be less than zero.");

                        validationFailures.Add(validationFailure);
                    }

                    // Try to find the return sales line
                    if (returnSalesLineByLineNumber == null || !returnSalesLineByLineNumber.ContainsKey(cartlineForUpdate.ReturnLineNumber))
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCartSalesLineUpdate,
                                "Could not find return sales line by ReturnLineNumber.");

                        validationFailures.Add(validationFailure);

                        // if we couldn't find return line, we cannot proceed with validation
                        return;
                    }

                    // The return quantity cannot exceed the quantity
                    var returnSalesLine = returnSalesLineByLineNumber[cartlineForUpdate.ReturnLineNumber];
                    if (returnSalesLine.Quantity < returnSalesLine.ReturnQuantity + Math.Abs(cartLine.LineData.Quantity))
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotReturnMoreThanPurchased,
                                "The returned quantity exceeds purchased quantity.");

                        validationFailures.Add(validationFailure);
                    }

                    // No price override for return lines
                    if (cartLine.Price != cartlineForUpdate.Price)
                    {
                        DataValidationFailure validationFailure = new DataValidationFailure(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NoPriceOverrideForReturns,
                                "Cannot override price for return items.");

                        validationFailures.Add(validationFailure);
                    }
                }
            }

            /// <summary>
            /// Validates whether the unit of measure of cart lines are set correctly.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">The cart whose cart line's unit of measure to be validated.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="cartLineValidationResults">The cart line validation result collection.</param>
            private static void ValidateCartLineUnitOfMeasureAndQuantity(RequestContext context, Cart newCart, SalesTransaction salesTransaction, CartLineValidationResults cartLineValidationResults)
            {
                // Retrieve all UoMs info in the cart
                IEnumerable<string> unitIds = GetUnitOfMeasureSymbols(context, newCart.CartLines);

                if (unitIds.Any())
                {
                    var getUnitsOfMeasureByUnitIds = new GetUnitsOfMeasureDataRequest(unitIds, QueryResultSettings.AllRecords);
                    var result = context.Runtime.Execute<EntityDataServiceResponse<UnitOfMeasure>>(getUnitsOfMeasureByUnitIds, context).PagedEntityCollection.Results;

                    if (result == null || !result.Any())
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidUnitOfMeasure, "Cart contains invalid unit of measure.");
                    }

                    // Verify if cart line quantity respects UoM's decimal point
                    int index = 0;
                    foreach (CartLine cartLine in newCart.CartLines)
                    {
                        string symbol = cartLine.UnitOfMeasureSymbol;
                        decimal quantity = cartLine.Quantity;

                        // Check if the quantity and UoM symbol are set
                        if (quantity != 0 && !string.IsNullOrWhiteSpace(symbol))
                        {
                            SalesLine salesLine = salesTransaction.SalesLines.FirstOrDefault(l => l.LineId == cartLine.LineId);

                            if (salesLine == null || quantity != salesLine.Quantity)
                            {
                                UnitOfMeasure uom = result.FirstOrDefault(unit => unit.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
                                if (uom != null)
                                {
                                    // Check if decimal precision of the UoM is respected
                                    if (!VerifyQuantityDecimalPrecision(quantity, uom.DecimalPrecision))
                                    {
                                        DataValidationFailure validationFailure = new DataValidationFailure(
                                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidQuantity,
                                            "The quantity of item exceeds the allowed decimal precision for the unit of measure.");

                                        cartLineValidationResults.AddLineResult(index, validationFailure);
                                    }
                                }
                            }

                            // Changes the unit of measure on a line with an overridden price is not allowed.
                            if (cartLine.IsPriceOverridden
                                && !symbol.Equals(salesLine.UnitOfMeasureSymbol, StringComparison.OrdinalIgnoreCase))
                            {
                                DataValidationFailure validationFailure = new DataValidationFailure(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotUpdateUnitOfMeasureOnPriceOverriddenLine,
                                    "Unit of measure cannot be updated on a line with overridden price.");

                                cartLineValidationResults.AddLineResult(index, validationFailure);
                            }
                        }

                        index++;
                    }
                }
            }

            /// <summary>
            /// Retrieves the unit of measure symbols for the items in the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cartLines">The cart lines.</param>
            /// <returns>The unit of measure symbols.</returns>
            private static IEnumerable<string> GetUnitOfMeasureSymbols(RequestContext context, IEnumerable<CartLine> cartLines)
            {
                // Retreive the cart lines whose units of measure are not set
                IEnumerable<CartLine> updatingLines = cartLines.Where(cartLine => string.IsNullOrWhiteSpace(cartLine.UnitOfMeasureSymbol));

                if (updatingLines.Any())
                {
                    IEnumerable<long> productIds = updatingLines.Select(l => l.ProductId).Distinct();

                    var getItemsRequest = new GetItemsDataRequest(productIds);
                    IEnumerable<Item> items = context.Runtime.Execute<GetItemsDataResponse>(getItemsRequest, context).Items;
                    Dictionary<long, string> unitOfMeasureDict = items.ToDictionary(i => i.Product, i => i.SalesUnitOfMeasure);

                    // Update the cart line unit of measure with the default value if it is not set
                    foreach (CartLine cartLine in updatingLines)
                    {
                        if (unitOfMeasureDict.ContainsKey(cartLine.ProductId))
                        {
                            cartLine.UnitOfMeasureSymbol = unitOfMeasureDict[cartLine.ProductId];
                        }
                    }
                }

                // Retrieve the non-empty units of measure on the cart
                IEnumerable<string> units = cartLines.Where(cartLine => !string.IsNullOrWhiteSpace(cartLine.UnitOfMeasureSymbol)).Select(l => l.UnitOfMeasureSymbol).Distinct();
                return units;
            }

            /// <summary>
            /// Verifies the decimal precision of the item's unit of measure is respected.
            /// </summary>
            /// <param name="quantity">Actual quantity value of the item from cart line.</param>
            /// <param name="decimalPrecision">Allowed decimal precision for the unit of measure.</param>
            /// <returns>Return true if the quantity's decimal precision respects the unit of measure.</returns>
            private static bool VerifyQuantityDecimalPrecision(decimal quantity, int decimalPrecision)
            {
                decimal order = (decimal)Math.Pow(10, decimalPrecision);
                quantity = Math.Abs(quantity);

                if ((quantity == 0) ||
                    (decimalPrecision < 0) ||
                    (decimal.MaxValue / order < quantity))
                {
                    return false;
                }

                decimal shift = quantity * order;           // Right shift the decimal point
                decimal integer = decimal.Truncate(shift);  // Get the integer part
                decimal fractional = shift - integer;       // Get the fractional part

                // If fractional part is 0, the quantity is in the decimal precision
                return fractional.Equals(0M);
            }

            /// <summary>
            /// Validate income or expense transactions.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="cart">Request cart.</param>
            /// <param name="cartLineValidationResults">Cart line validation results.</param>
            private static void ValidateIncomeExpenseTransaction(RequestContext context, Cart cart, CartLineValidationResults cartLineValidationResults)
            {
                if (cart.IncomeExpenseLines != null)
                {
                    // Adding both income and expense account types are not valid.
                    if (cart.IncomeExpenseLines.Any(incomeLine => incomeLine.AccountType == IncomeExpenseAccountType.Income)
                    && cart.IncomeExpenseLines.Any(expenseLine => expenseLine.AccountType == IncomeExpenseAccountType.Expense))
                    {
                        var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IncomeExpenseAccountsInSameCart, "The income/ expense line cannot contain both income and expense account types in transaction");
                        cartLineValidationResults.AddLineResult(0, result);
                    }

                    // Validate that the transaction status is not set on income expense lines.
                    if (cart.IncomeExpenseLines.Any(incomeExpenseLine => incomeExpenseLine.TransactionStatusValue != (int)TransactionStatus.Normal))
                    {
                        var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IncomeExpenseLineDoesNotAllowSettingTransactionStatus, "The income/ expense line cannot have the transaction status set.");
                        cartLineValidationResults.AddLineResult(0, result);
                    }
                }

                // Validate income (or) expense permissions.
                if (cart.IncomeExpenseLines != null && cart.IncomeExpenseLines.Any())
                {
                    if (cart.IncomeExpenseLines[0].AccountType == IncomeExpenseAccountType.Income)
                    {
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.IncomeAccounts));
                    }
                    else if (cart.IncomeExpenseLines[0].AccountType == IncomeExpenseAccountType.Expense)
                    {
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ExpenseAccounts));
                    }
                }

                // Adding customer account is not allowed.
                if (!string.IsNullOrWhiteSpace(cart.CustomerId))
                {
                    var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IncomeExpenseCartDoesNotAllowCustomer, "The customer id is not allowed in income or expense transaction");
                    cartLineValidationResults.AddLineResult(0, result);
                }

                // Adding cashier (or) manual discounts are not allowed.
                if (cart.TotalManualDiscountAmount != 0 || cart.TotalManualDiscountPercentage != 0)
                {
                    var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IncomeExpenseCartDoesNotAllowDiscounts, "The total discount is not allowed in income or expense transaction");
                    cartLineValidationResults.AddLineResult(0, result);
                }

                // Adding sales line is not allowed.
                if (cart.CartLines.Any())
                {
                    var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IncomeExpenseCartDoesNotAllowSalesLine, "The sales line not allowed in income or expense transaction");
                    cartLineValidationResults.AddLineResult(0, result);
                }
            }

            /// <summary>
            /// Validates whether the loyalty card number can be updated.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">The cart with updates or new cart from the client.</param>
            /// <param name="existingTransaction">The existing sales transaction from the DB.</param>
            /// <param name="cartLineValidationResults">The cart line validation results.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "The method code is pretty short and clear.")]
            private static void ValidateLoyaltyCard(RequestContext context, Cart newCart, SalesTransaction existingTransaction, CartLineValidationResults cartLineValidationResults)
            {
                // Check whether the loyalty card number is updated.
                // 1. Add a loyalty card: The old cart does not have a loyalty card; the new cart has a loyalty card.
                // 2. Clear the loyalty card: The old cart already has a loyalty card; the new cart has the card number set to empty.
                // 3. Change a loyalty card: Both the old cart and the new cart have a loyalty card number set, but they are different.
                bool addLoyaltyCard = string.IsNullOrWhiteSpace(existingTransaction.LoyaltyCardId) && !string.IsNullOrWhiteSpace(newCart.LoyaltyCardId);
                bool clearLoyaltyCard = !addLoyaltyCard
                                        && !string.IsNullOrWhiteSpace(existingTransaction.LoyaltyCardId)
                                        && newCart.LoyaltyCardId != null
                                        && newCart.LoyaltyCardId.Length == 0;
                bool updateLoyaltyCard = !addLoyaltyCard
                                        && !clearLoyaltyCard
                                        && !string.IsNullOrWhiteSpace(existingTransaction.LoyaltyCardId)
                                        && !string.IsNullOrWhiteSpace(newCart.LoyaltyCardId)
                                        && !newCart.LoyaltyCardId.Equals(existingTransaction.LoyaltyCardId, StringComparison.OrdinalIgnoreCase);
                bool loyaltyCardChanged = addLoyaltyCard || clearLoyaltyCard || updateLoyaltyCard;

                // Check whether the customer account number is updated.
                // 1. Add a customer: The old cart does not have a customer; the new cart has a customer.
                // 2. Clear the customer: The old cart already has a customer; the new cart has the customer number set to empty.
                // 3. Change a customer: Both the old cart and the new cart have a customer number set, but they are different.
                bool addCustomer = string.IsNullOrWhiteSpace(existingTransaction.CustomerId) && !string.IsNullOrWhiteSpace(newCart.CustomerId);
                bool clearCustomer = !addCustomer
                                        && !string.IsNullOrWhiteSpace(existingTransaction.CustomerId)
                                        && newCart.CustomerId != null
                                        && newCart.CustomerId.Length == 0;
                bool updateCustomer = !addCustomer
                                        && !clearCustomer
                                        && !string.IsNullOrWhiteSpace(existingTransaction.CustomerId)
                                        && !string.IsNullOrWhiteSpace(newCart.CustomerId)
                                        && !newCart.CustomerId.Equals(existingTransaction.CustomerId, StringComparison.OrdinalIgnoreCase);
                bool customerChanged = addCustomer || clearCustomer || updateCustomer;

                if (loyaltyCardChanged && customerChanged)
                {
                    // Change the loyalty card and the customer at the same time.
                    var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotUpdateCustomerAndLoyaltyCardAtTheSameTime);
                    cartLineValidationResults.AddLineResult(0, result);
                }
                else if (loyaltyCardChanged || customerChanged)
                {
                    if (addLoyaltyCard || updateLoyaltyCard)
                    {
                        // Add or update loyalty card.
                        SetCustomerFromLoyaltyCard(context, newCart, existingTransaction, cartLineValidationResults);
                    }
                    else if (addCustomer || updateCustomer)
                    {
                        // Add or update customer
                        SetLoyaltyCardFromCustomer(context, newCart);
                    }
                    else if (clearLoyaltyCard)
                    {
                        // Clear loyalty card should also clear the customer
                        newCart.CustomerId = string.Empty;
                    }
                    else
                    {
                        // Clear customer should also clear the loyalty card
                        newCart.LoyaltyCardId = string.Empty;
                    }
                }
            }

            /// <summary>
            /// Sets the customer in the cart from the loyalty card.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="newCart">The cart with updates or new cart from the client.</param>
            /// <param name="existingTransaction">The existing sales transaction from the DB.</param>
            /// <param name="cartLineValidationResults">The cart line validation results.</param>
            private static void SetCustomerFromLoyaltyCard(RequestContext context, Cart newCart, SalesTransaction existingTransaction, CartLineValidationResults cartLineValidationResults)
            {
                // Check if the loyalty card exists and not blocked
                var request = new GetLoyaltyCardDataRequest(newCart.LoyaltyCardId);
                var response = context.Runtime.Execute<SingleEntityDataServiceResponse<LoyaltyCard>>(request, context);
                var loyaltyCard = response.Entity;
                if (loyaltyCard == null || string.IsNullOrWhiteSpace(loyaltyCard.CardNumber))
                {
                    var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidLoyaltyCardNumber);
                    cartLineValidationResults.AddLineResult(0, result);
                }
                else if (loyaltyCard.CardTenderType == LoyaltyCardTenderType.Blocked)
                {
                    var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_BlockedLoyaltyCard);
                    cartLineValidationResults.AddLineResult(0, result);
                }
                else
                {
                    // For valid loyalty card...
                    if (!string.IsNullOrWhiteSpace(loyaltyCard.PartyNumber))
                    {
                        // The loyalty card has an owner, however the customer does not exist in the currenct channel.
                        // Try to create the customer in the current channel.
                        if (string.IsNullOrWhiteSpace(loyaltyCard.CustomerAccount))
                        {
                            Customer newCustomer = new Customer { NewCustomerPartyNumber = loyaltyCard.PartyNumber };
                            var saveCustomerServiceRequest = new SaveCustomerServiceRequest(newCustomer);
                            var saveCustomerServiceResponse = context.Execute<SaveCustomerServiceResponse>(saveCustomerServiceRequest);

                            loyaltyCard.CustomerAccount = saveCustomerServiceResponse.UpdatedCustomer.AccountNumber;
                        }

                        if (!string.IsNullOrWhiteSpace(loyaltyCard.CustomerAccount))
                        {
                            if (string.IsNullOrWhiteSpace(existingTransaction.CustomerId) && context.GetPrincipal().IsEmployee)
                            {
                                // Add the owner customer to the transaction.
                                newCart.CustomerId = loyaltyCard.CustomerAccount;
                            }
                            else if (!loyaltyCard.CustomerAccount.Equals(existingTransaction.CustomerId, StringComparison.OrdinalIgnoreCase))
                            {
                                // Throw error if it's different from the current customer.
                                var result = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ConflictLoyaltyCardCustomerAndTransactionCustomer);
                                cartLineValidationResults.AddLineResult(0, result);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Validates whether the quantity of each line in the aggregated cart lines is within the permissible quantity limit.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cartLines">The cart lines whose quantity is to be checked.</param>
            private static void ValidateSalesLineQuantityLimit(RequestContext context, IEnumerable<CartLine> cartLines)
            {
                if (!context.GetPrincipal().IsInRole(CommerceRoles.Employee))
                {
                    return;
                }

                ThrowIf.Null(cartLines, "cartLines");

                // Retrieve the quantity limit
                DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();
                decimal maximumQuantity = deviceConfiguration.MaximumQuantity;

                // Verify if cart line quantity respects the allowable quantity limit
                bool quantityLimitExceeded = (maximumQuantity > 0) && cartLines.Any(cl => Math.Abs(cl.LineData.Quantity) > maximumQuantity);
                if (quantityLimitExceeded)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ItemQuantityExceeded, "The quantity of item exceeds the allowed maximum quantity.");
                }
            }

            #endregion

            /// <summary>
            /// Sets the default properties on the sales line from the item information.
            /// </summary>
            /// <param name="salesLine">The sales line from the sales transaction.</param>
            /// <param name="item">The item corresponding to the sales line.</param>
            private static void SetSalesLineDefaultsFromItemData(SalesLine salesLine, Item item)
            {
                salesLine.OriginalSalesOrderUnitOfMeasure = item.SalesUnitOfMeasure;

                if (string.IsNullOrWhiteSpace(salesLine.SalesOrderUnitOfMeasure))
                {
                    salesLine.SalesOrderUnitOfMeasure = item.SalesUnitOfMeasure;
                }

                // apply default only if there is no Unit Of Measure on the line
                if (string.IsNullOrWhiteSpace(salesLine.UnitOfMeasureSymbol))
                {
                    salesLine.UnitOfMeasureSymbol = salesLine.SalesOrderUnitOfMeasure;
                }

                salesLine.InventOrderUnitOfMeasure = item.InventoryUnitOfMeasure;
            }

            /// <summary>
            /// Retrieves the item data for the items in the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesLines">The sales lines for which to retrieve item information.</param>
            /// <returns>The cart items.</returns>
            private static IEnumerable<Item> GetItemsForSalesLines(RequestContext context, IEnumerable<SalesLine> salesLines)
            {
                IEnumerable<string> itemIds = salesLines.Where(l => l.IsProductLine).Select(l => l.ItemId);

                IEnumerable<Item> items;
                if (itemIds.Any())
                {
                    var getItemsRequest = new GetItemsDataRequest(itemIds)
                    {
                        QueryResultSettings = new QueryResultSettings(new ColumnSet("ITEMID", "UNITID", "ITEMTAXGROUPID", "INVENTUNITID", "NAME", "PRODUCT"), PagingInfo.AllRecords)
                    };
                    var getItemsResponse = context.Runtime.Execute<GetItemsDataResponse>(getItemsRequest, context);
                    items = getItemsResponse.Items;
                }
                else
                {
                    items = new Collection<Item>();
                }

                return items;
            }

            /// <summary>
            /// Assigns id for the new line.
            /// </summary>
            /// <param name="salesLine">Line to update.</param>
            private static void AssignUniqueLineId(SalesLine salesLine)
            {
                // Using format 'N' to remove dashes from the GUID.
                salesLine.LineId = Guid.NewGuid().ToString("N");
                foreach (ReasonCodeLine reasonCodeLine in salesLine.ReasonCodeLines)
                {
                    reasonCodeLine.ParentLineId = salesLine.LineId;
                }
            }

            /// <summary>
            /// Adds or updates affiliation lines on the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="affiliationLoyaltyTiers">The affiliation lines of the cart to be updated or added.</param>
            /// <param name="salesTransaction">Current transaction.</param>
            /// <param name="returnTransaction">Transaction being returned.</param>
            private static void AddOrUpdateAffiliationLines(RequestContext context, IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers, SalesTransaction salesTransaction, SalesTransaction returnTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(affiliationLoyaltyTiers, "affiliationLoyaltyTiers");

                // Divide affiliation and loyalty tier lines
                List<SalesAffiliationLoyaltyTier> affiliationLines = (from line in salesTransaction.AffiliationLoyaltyTierLines
                                                                      where line.AffiliationType == RetailAffiliationType.General
                                                                        && affiliationLoyaltyTiers.Any(tiers => tiers.AffiliationId == line.AffiliationId)
                                                                      select line).ToList();

                List<SalesAffiliationLoyaltyTier> loyaltyTierLines = (from line in salesTransaction.AffiliationLoyaltyTierLines
                                                                      where line.AffiliationType != RetailAffiliationType.General
                                                                      select line).ToList();

                // Keeps track of the affiliation line being created or updated.
                Dictionary<long, SalesAffiliationLoyaltyTier> salesAffiliationLoyaltyTierByLineId = affiliationLines.ToDictionary(a => a.AffiliationId, a => a);

                foreach (AffiliationLoyaltyTier affiliationLoyaltyTier in affiliationLoyaltyTiers)
                {
                    SalesAffiliationLoyaltyTier salesAffiliationLoyaltyTier;
                    if (!salesAffiliationLoyaltyTierByLineId.ContainsKey(affiliationLoyaltyTier.AffiliationId))
                    {
                        salesAffiliationLoyaltyTier = new SalesAffiliationLoyaltyTier
                        {
                            AffiliationType = affiliationLoyaltyTier.AffiliationType,
                            AffiliationId = affiliationLoyaltyTier.AffiliationId,
                            ChannelId = context.GetPrincipal().ChannelId,
                            LoyaltyTierId = affiliationLoyaltyTier.LoyaltyTierId,
                            ReceiptId = salesTransaction.ReceiptId,
                            StaffId = salesTransaction.StaffId,
                            TerminalId = salesTransaction.TerminalId,
                            TransactionId = salesTransaction.Id,
                            CustomerId = affiliationLoyaltyTier.CustomerId
                        };

                        salesAffiliationLoyaltyTierByLineId.Add(salesAffiliationLoyaltyTier.AffiliationId, salesAffiliationLoyaltyTier);
                    }
                    else
                    {
                        salesAffiliationLoyaltyTier = salesAffiliationLoyaltyTierByLineId[affiliationLoyaltyTier.AffiliationId];
                    }

                    // Add or update reason code lines on the affiliation line.
                    ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnAffiliationLine(salesAffiliationLoyaltyTier, affiliationLoyaltyTier, salesTransaction.Id);
                }

                // Check whether current request is related terminal or not,
                // this is in order to distinguish request from RS service or eCommerce.
                ICommercePrincipal principal = context.GetPrincipal();

                if (principal != null && !principal.IsTerminalAgnostic)
                {
                    // Calculate the required reason codes on the affiliation lines.
                    // All the affiliations' reason codes should be calculated at the same time,
                    // because selecting multi affiliations at the same time is supported at client side.
                    if (salesTransaction.CartType != CartType.CustomerOrder || string.IsNullOrEmpty(salesTransaction.SalesId))
                    {
                        ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnAffiliationLines(context, salesTransaction, salesAffiliationLoyaltyTierByLineId.Values, ReasonCodeSourceType.None);
                    }
                }

                // Update the affiliation lines.
                salesTransaction.AffiliationLoyaltyTierLines.Clear();
                salesTransaction.AffiliationLoyaltyTierLines.AddRange(salesAffiliationLoyaltyTierByLineId.Values);
                salesTransaction.AffiliationLoyaltyTierLines.AddRange(loyaltyTierLines);

                // Copy the affiliations of the returned transaction to current transaction.
                if (returnTransaction != null
                    && returnTransaction.AffiliationLoyaltyTierLines.Count > 0)
                {
                    var returnedSalesAffiliationLoyaltyTier = returnTransaction.AffiliationLoyaltyTierLines.Where(a => !IsLoyaltyTierAffiliation(context, a.AffiliationId) && !IsExistSameAffiliation(a.AffiliationId, salesTransaction.AffiliationLoyaltyTierLines));
                    salesTransaction.AffiliationLoyaltyTierLines.AddRange(returnedSalesAffiliationLoyaltyTier);
                }
            }

            /// <summary>
            /// Refresh the loyalty group and tier lines on the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">Current transaction.</param>
            private static void RefreshSalesLoyaltyTierLines(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                // Clear the loyalty group and tier lines, but keep the affiliation lines.
                List<SalesAffiliationLoyaltyTier> affiliationLines = (from line in salesTransaction.AffiliationLoyaltyTierLines
                                                                      where line.AffiliationType == RetailAffiliationType.General
                                                                      select line).ToList();
                salesTransaction.AffiliationLoyaltyTierLines.Clear();
                salesTransaction.AffiliationLoyaltyTierLines.AddRange(affiliationLines);

                // Add lines from loyalty groups and tiers of the loyalty card
                if (!string.IsNullOrWhiteSpace(salesTransaction.LoyaltyCardId))
                {
                    var getLoyaltyCardAffiliationsDataRequest = new GetLoyaltyCardAffiliationsDataRequest(salesTransaction.LoyaltyCardId, salesTransaction)
                        {
                            QueryResultSettings = QueryResultSettings.AllRecords
                        };

                    ReadOnlyCollection<SalesAffiliationLoyaltyTier> salesAffiliationLoyaltyTiers = context.Runtime
                        .Execute<EntityDataServiceResponse<SalesAffiliationLoyaltyTier>>(getLoyaltyCardAffiliationsDataRequest, context).PagedEntityCollection.Results;

                    salesTransaction.AffiliationLoyaltyTierLines.AddRange(salesAffiliationLoyaltyTiers);
                }
            }

            /// <summary>
            /// Check whether exist same affiliation or not.
            /// </summary>
            /// <param name="affiliationId">The affiliation Id.</param>
            /// <param name="salesAffiliationLoyaltyTiers">The affiliationLoyaltyTier collection.</param>
            /// <returns>Return true if exist same affiliation, otherwise return false.</returns>
            private static bool IsExistSameAffiliation(long affiliationId, IList<SalesAffiliationLoyaltyTier> salesAffiliationLoyaltyTiers)
            {
                if (salesAffiliationLoyaltyTiers != null)
                {
                    return salesAffiliationLoyaltyTiers.Any(s => s.AffiliationId == affiliationId);
                }

                return false;
            }

            /// <summary>
            /// Check whether the affiliation is loyalty affiliation or not.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="affiliationId">The affiliation Id.</param>
            /// <returns>Return true if it is loyalty affiliation, otherwise return false.</returns>
            private static bool IsLoyaltyTierAffiliation(RequestContext context, long affiliationId)
            {
                GetAffiliationsDataRequest getAffiliationsDataRequest = new GetAffiliationsDataRequest(
                    RetailAffiliationType.Loyalty,
                    QueryResultSettings.AllRecords);

                ReadOnlyCollection<Affiliation> affiliations = context.Execute<EntityDataServiceResponse<Affiliation>>(getAffiliationsDataRequest).PagedEntityCollection.Results;

                return affiliations.Any(a => a.RecordId == affiliationId);
            }

            /// <summary>
            /// Check whether exist same affiliation or not.
            /// </summary>
            /// <param name="affiliationId">The affiliation Id.</param>
            /// <param name="affiliationLoyaltyTiers">The affiliationLoyaltyTier collection.</param>
            /// <returns>Return true if exist same affiliation, otherwise return false.</returns>
            private static bool IsExistSameAffiliation(long affiliationId, IList<AffiliationLoyaltyTier> affiliationLoyaltyTiers)
            {
                if (affiliationLoyaltyTiers != null)
                {
                    return affiliationLoyaltyTiers.Any(s => s.AffiliationId == affiliationId);
                }

                return false;
            }

            private static CartStatus GetCartStatus(SalesTransaction transaction)
            {
                ThrowIf.Null(transaction, "transaction");

                if (transaction.IsSuspended)
                {
                    return CartStatus.Suspended;
                }

                if (transaction.EntryStatus == TransactionStatus.Normal || transaction.EntryStatus == TransactionStatus.Posted)
                {
                    return CartStatus.Created;
                }

                throw new DataValidationException(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidStatus,
                    string.Format("Cannot get the supported cart status from existing SalesTransaction record, TransactionID = {0}.", transaction.Id));
            }

            /// <summary>
            /// Insert tax detail on sales line and charge line into tax details of cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cart">The cart convert to.</param>
            /// <param name="salesTransaction">The salesTransaction convert from.</param>
            private static void InsertTaxDetailIntoCart(RequestContext context, Cart cart, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(cart, "cart");
                ThrowIf.Null(salesTransaction, "theTransaction");

                IList<TaxLineIndia> indiaTaxItems = new List<TaxLineIndia>();

                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    // Add tax line on sales line
                    foreach (TaxLine taxLine in salesLine.TaxLines)
                    {
                        TaxLineIndia taxLineIndia = taxLine as TaxLineIndia;
                        if (taxLineIndia != null)
                        {
                            indiaTaxItems.Add(taxLineIndia);
                        }
                    }

                    // Add tax line on charge line of sales line
                    foreach (ChargeLine chargeLine in salesLine.ChargeLines)
                    {
                        foreach (TaxLine taxLine in chargeLine.TaxLines)
                        {
                            TaxLineIndia taxLineIndia = taxLine as TaxLineIndia;
                            if (taxLineIndia != null)
                            {
                                indiaTaxItems.Add(taxLineIndia);
                            }
                        }
                    }
                }

                // Add tax line on charge of sales head
                foreach (ChargeLine chargeLine in salesTransaction.ChargeLines)
                {
                    foreach (TaxLine taxLineInCharge in chargeLine.TaxLines)
                    {
                        TaxLineIndia taxLineIndia = taxLineInCharge as TaxLineIndia;
                        if (taxLineIndia != null)
                        {
                            indiaTaxItems.Add(taxLineIndia);
                        }
                    }
                }

                if (indiaTaxItems.Count > 0)
                {
                    cart.TaxViewLines = BuildTaxSummaryPerComponentNotShowTaxonTax(context, indiaTaxItems);
                }
            }

            /// <summary>
            /// Build tax summary line for India, with tax amounts be aggregated by "main" tax codes (which are not India tax on tax codes).
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="indiaTaxItems">All tax items of the India retail transaction.</param>
            /// <returns>The tax summary lines of the India.</returns>
            /// <remarks>
            /// For example, the retail transaction has four sale line items, as follows,
            /// <c>
            /// Item ID | Price | Tax code | Formula       | Tax basis | Tax rate | Tax amount
            /// 0001    | 100   | SERV5    | Line amount   | 100.00    |  5%      |  5.00
            ///         |       | E-CSS5   | Excl.+[SERV5] |   5.00    |  5%      |  0.25
            /// 0002    | 100   | VAT10    | Line amount   | 100.00    | 10%      | 10.00
            ///         |       | Surchg2  | Excl.+[VAT10] |  10.00    |  2%      |  0.20
            /// 0003    | 100   | SERV4    | Line amount   | 100.00    |  4%      |  4.00
            ///         |       | E-CSS5   | Excl.+[SERV4] |   4.00    |  5%      |  0.20
            /// 0004    | 100   | VAT12    | Line amount   | 100.00    | 12%      | 12.00
            ///         |       | Surchg2  | Excl.+[VAT12] |  12.00    |  2%      |  0.24
            /// And the tax summary lines will be as follows,
            ///  SERV5 @ 5.25    $5.25
            ///  SERV4 @ 4.20    $4.20
            /// VAT10 @ 10.20   $10.20
            /// VAT12 @ 12.24   $12.24.
            /// </c>
            /// </remarks>
            private static IList<TaxViewLine> BuildTaxSummaryPerComponentNotShowTaxonTax(RequestContext context, IList<TaxLineIndia> indiaTaxItems)
            {
                ThrowIf.Null(indiaTaxItems, "indiaTaxItems");

                if (indiaTaxItems.Count == 0)
                {
                    throw new ArgumentException("The specified collection cannot be empty.", "indiaTaxItems");
                }

                // Group tax lines as per tax code and depended tax code for tax on tax line.
                // Tax on tax lines will be grouped into the first depended line.
                List<TaxLineIndia> lines = new List<TaxLineIndia>();
                var groups = indiaTaxItems.GroupBy(x =>
                {
                    string taxCode = x.IsTaxOnTax ?
                        x.TaxCodesInFormula.First() :
                        x.TaxCode;
                    return new { x.TaxGroup, taxCode };
                });

                // Calculate and re-create tax lines by populating tax compoenent and tax rate as per above groups.
                foreach (var gp in groups)
                {
                    TaxLineIndia t = new TaxLineIndia
                    {
                        TaxGroup = gp.Key.TaxGroup,
                        TaxCode = gp.Key.taxCode,
                        TaxBasis = gp.Where(g => !g.IsTaxOnTax).Select(tb => tb.TaxBasis).Sum(),
                        Amount = gp.Sum(x => x.Amount),

                        TaxComponent = gp.First(x => !x.IsTaxOnTax).TaxComponent
                    };
                    t.Percentage = t.TaxBasis != decimal.Zero ? 100 * t.Amount / t.TaxBasis : decimal.Zero;

                    lines.Add(t);
                }

                // Build tax summary view lines based on above calculated tax lines.
                IList<TaxViewLine> taxSummary = new Collection<TaxViewLine>();
                var taxlines = from taxline in lines
                               orderby taxline.TaxComponent
                               group taxline by new { taxline.TaxComponent, taxline.Percentage };
                foreach (var group in taxlines)
                {
                    TaxViewLine t = new TaxViewLine();
                    ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                    GetRoundedValueServiceRequest request = new GetRoundedValueServiceRequest(
                        group.Key.Percentage,
                        channelConfiguration.Currency,
                        numberOfDecimals: 0,
                        useSalesRounding: true);
                    GetRoundedValueServiceResponse response = context.Execute<GetRoundedValueServiceResponse>(request);
                    decimal taxRate = response.RoundedValue;

                    t.TaxId = group.Key.TaxComponent + " @ " + taxRate.ToString() + "%";
                    t.TaxAmount = group.Sum(x => x.Amount);
                    taxSummary.Add(t);
                }

                return taxSummary;
            }

            private static void AssociateCatalogsToSalesLines(RequestContext context, SalesTransaction transaction)
            {
                if (transaction != null && transaction.ActiveSalesLines.Any())
                {
                    CartWorkflowHelper.PopulateMasterProductIds(context, transaction.ActiveSalesLines);

                    List<long> productIds = new List<long>();
                    foreach (SalesLine salesLine in transaction.PriceCalculableSalesLines)
                    {
                        productIds.Add(salesLine.ProductId);
                        if (salesLine.MasterProductId != 0 && salesLine.MasterProductId != salesLine.ProductId)
                        {
                            productIds.Add(salesLine.MasterProductId);
                        }
                    }

                    GetProductCatalogAssociationsDataRequest getProductCatalogAssociationsRequest = new GetProductCatalogAssociationsDataRequest(productIds)
                        {
                            QueryResultSettings = QueryResultSettings.AllRecords
                        };
                    ReadOnlyCollection<ProductCatalogAssociation> productCatalogs = context.Runtime
                        .Execute<GetProductCatalogAssociationsDataResponse>(getProductCatalogAssociationsRequest, context).CatalogAssociations;

                    foreach (SalesLine salesLine in transaction.PriceCalculableSalesLines)
                    {
                        foreach (ProductCatalogAssociation productCatalogAssociation in productCatalogs)
                        {
                            if (productCatalogAssociation.ProductRecordId == salesLine.ProductId || (salesLine.MasterProductId != 0 && productCatalogAssociation.ProductRecordId == salesLine.MasterProductId))
                            {
                                salesLine.CatalogIds.Add(productCatalogAssociation.CatalogRecordId);
                            }
                        }
                    }
                }
            }

            private static void PopulateMasterProductIds(RequestContext context, IEnumerable<SalesLine> salesLines)
            {
                IEnumerable<string> masterItemIds = salesLines.Where(l => !string.IsNullOrWhiteSpace(l.ItemId) && !string.IsNullOrWhiteSpace(l.InventoryDimensionId)).Select(l => l.ItemId).Distinct(StringComparer.OrdinalIgnoreCase);
                if (masterItemIds.Any())
                {
                    GetItemsDataRequest getItemsRequest = new GetItemsDataRequest(masterItemIds)
                    {
                        QueryResultSettings = new QueryResultSettings(new ColumnSet("ItemId", "PRODUCT"), PagingInfo.AllRecords)
                    };
                    GetItemsDataResponse getItemsResponse = context.Execute<GetItemsDataResponse>(getItemsRequest);
                    IDictionary<string, Item> itemIdDictionary = getItemsResponse.Items.ToDictionary(p => p.ItemId, p => p, StringComparer.OrdinalIgnoreCase);

                    foreach (SalesLine salesLine in salesLines)
                    {
                        if (salesLine.MasterProductId == 0)
                        {
                            Item item;
                            if (!string.IsNullOrWhiteSpace(salesLine.ItemId) && itemIdDictionary.TryGetValue(salesLine.ItemId, out item))
                            {
                                if (item.Product != salesLine.ProductId)
                                {
                                    salesLine.MasterProductId = item.Product;
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the unit of measure conversion map.
            /// </summary>
            /// <param name="context">The data request context.</param>
            /// <param name="salesLines">The sales lines.</param>
            /// <returns>A dictionary whose key is <see cref="ItemUnitConversion"/> and value is <see cref="UnitOfMeasureConversion"/>.</returns>
            private static Dictionary<ItemUnitConversion, UnitOfMeasureConversion> GetUnitOfMeasureConversionMap(RequestContext context, IEnumerable<SalesLine> salesLines)
            {
                var itemUnitConversions = new HashSet<ItemUnitConversion>();
                foreach (SalesLine salesLine in salesLines)
                {
                    var conversion = new ItemUnitConversion
                    {
                        FromUnitOfMeasure = salesLine.SalesOrderUnitOfMeasure,
                        ToUnitOfMeasure = salesLine.OriginalSalesOrderUnitOfMeasure,
                        ItemId = salesLine.ItemId
                    };
                    itemUnitConversions.Add(conversion);
                }

                Dictionary<ItemUnitConversion, UnitOfMeasureConversion> unitOfMeasureConversionMap = null;

                if (itemUnitConversions.Any())
                {
                    unitOfMeasureConversionMap = CartWorkflowHelper.GetUnitOfMeasureConversions(context, itemUnitConversions).ToDictionary(key =>
                        new ItemUnitConversion
                        {
                            FromUnitOfMeasure = key.FromUnitOfMeasureId,
                            ToUnitOfMeasure = key.ToUnitOfMeasureId,
                            ItemId = key.ItemId
                        });
                }

                return unitOfMeasureConversionMap;
            }

            /// <summary>
            /// Gets the unit of measure conversion for this sales line from the conversion map.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="unitOfMeasureConversionMap">The unit of measure conversion map.</param>
            /// <returns>The unit of measure conversion.</returns>
            private static UnitOfMeasureConversion GetUnitOfMeasureConversion(SalesLine salesLine, Dictionary<ItemUnitConversion, UnitOfMeasureConversion> unitOfMeasureConversionMap)
            {
                var conversion = new ItemUnitConversion
                {
                    FromUnitOfMeasure = salesLine.SalesOrderUnitOfMeasure,
                    ToUnitOfMeasure = salesLine.OriginalSalesOrderUnitOfMeasure,
                    ItemId = salesLine.ItemId
                };

                if (unitOfMeasureConversionMap == null
                    || !unitOfMeasureConversionMap.ContainsKey(conversion))
                {
                    return null;
                }
                else
                {
                    return unitOfMeasureConversionMap[conversion];
                }
            }

            /// <summary>
            /// Changes the unit of measure for this sales line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="unitOfMeasureConversion">The unit of measure conversion.</param>
            private static void ChangeUnitOfMeasure(RequestContext context, SalesLine salesLine, UnitOfMeasureConversion unitOfMeasureConversion)
            {
                // Set unit of measure conversion value.
                salesLine.UnitOfMeasureConversion = unitOfMeasureConversion;

                // Round the quantity if necessary.
                var roundRequest = new GetRoundQuantityServiceRequest(salesLine.Quantity, salesLine.UnitOfMeasureSymbol);
                salesLine.Quantity = context.Execute<GetRoundQuantityServiceResponse>(roundRequest).RoundedValue;
            }
        }
    }
}