/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='InventoryOperationHandlers.ts' />

// aliasing of modules and enum
import Operations = Commerce.Operations;
import Validators = Operations.Validators;
import RetailOperation = Operations.RetailOperation;

// after the DOM content is loaded we register all the operation handlers with the operations manager.
document.addEventListener('DOMContentLoaded', function () {

    var operationsManager = Operations.OperationsManager.instance;

    /**
     * This is the global map for all operations.
     * Any operation needs to be registered here for use
     * NOTE: ONE ENTRY PER Commerce.Operations.RetailOperation ENTRY REQUIRED
     */
    operationsManager.registerOperationHandler({
        id: RetailOperation.ItemSale,
        handler: new Operations.ItemSaleOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ChangeUnitOfMeasure,
        handler: new Operations.ChangeUnitOfMeasureOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] },
            <Operations.IValidator<Commerce.Model.Entities.CartLine[]>>{
                dataAccessor: (options: Operations.IChangeUnitOfMeasureOperationOptions) => {
                    return options.cartLineUnitOfMeasures.map(clp => clp.cartLine);
                },
                validatorFunctions: [
                    Validators.notHaveOverridenPrice,
                    Validators.notAllowQuantityUpdate
                ]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.SalesPerson,
        handler: new Operations.SalesPersonOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.OverrideTaxTransaction,
        handler: new Operations.OverrideTransactionTaxOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.OverrideTaxTransactionList,
        handler: new Operations.OverrideTransactionTaxFromListOperationHandler(),
        validators: [
            { validatorFunctions: [Operations.Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.OverrideTaxLine,
        handler: new Operations.OverrideLineProductTaxOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.OverrideTaxLineList,
        handler: new Operations.OverrideLineProductTaxFromListOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.InventoryLookup,
        handler: new Operations.InventoryLookupOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.StockCount,
        handler: new Operations.StockCountOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PriceCheck,
        handler: new Operations.PriceCheckOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.DepositOverride,
        handler: new Operations.DepositOverrideOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ReturnItem,
        handler: new Operations.ReturnProductOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnNonReturnCustomerOrderOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] },
            <Operations.IValidator<Operations.IReturnProductOperationOptions>>{
                dataAccessor: (options: Operations.IReturnProductOperationOptions) => { return options; },
                validatorFunctions: [Validators.returnLimitsValidator]
            },
            <Operations.IValidator<Commerce.Model.Entities.CartLine>>{
                dataAccessor: (options: Operations.IReturnProductOperationOptions) => {
                    return options.productReturnDetails.filter((value: Commerce.Model.Entities.ProductReturnDetails): boolean => {
                        return !Commerce.ObjectExtensions.isNullOrUndefined(value.cartLine);
                    }).map((val: Commerce.Model.Entities.ProductReturnDetails): Commerce.Model.Entities.CartLine => {
                            return val.cartLine;
                    });
                },
                validatorFunctions: [Validators.returnCartLinesOperationValidator]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ReturnTransaction,
        handler: new Operations.ReturnTransactionOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnNonReturnCustomerOrderOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.VoidItem,
        handler: new Operations.VoidProductsOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.notIncomeExpenseTransaction,
                    Validators.notAllowedOnCustomerAccountDeposit
                ]
            },
            <Operations.IValidator<Commerce.Model.Entities.CartLine[]>>{
                dataAccessor: (options: Operations.IVoidProductsOperationOptions) => { return options.cartLines; },
                validatorFunctions: [Validators.singleCartLineOperationValidator]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ItemComment,
        handler: new Operations.ProductCommentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PriceOverride,
        handler: new Operations.PriceOverrideOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.notIncomeExpenseTransaction,
                    Validators.isCustomerOrderOrQuoteInCreateOrEditState,
                    Validators.notAllowedOnCustomerAccountDeposit
                ]
            },
            <Operations.IValidator<Commerce.Model.Entities.CartLine[]>>{
                dataAccessor: (options: Operations.IPriceOverrideOperationOptions) => {
                    return options.cartLinePrices.map(clp => clp.cartLine);
                },
                validatorFunctions: [
                    Validators.singleCartLineOperationValidator,
                    Validators.nonVoidedOperationValidator,
                    Validators.notFromAReceiptOperationValidator,
                    Validators.notFromAGiftCertificateOperationValidator,
                    Validators.itemAllowsPriceOverrideOperationValidator
                ]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.SetQuantity,
        handler: new Operations.SetQuantityOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ClearQuantity,
        handler: new Operations.ClearQuantityOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.existingCart,
                    Validators.notAllowedOnCustomerAccountDeposit
                ]
            },
            <Operations.IValidator<Commerce.Model.Entities.CartLine[]>> {
                dataAccessor: (options: Operations.IClearQuantityOperationOptions) => {
                    return options.cartLines;
                },
                validatorFunctions: [
                    Validators.singleCartLineOperationValidator,
                    Validators.nonVoidedOperationValidator,
                    Validators.notFromAGiftCertificateOperationValidator,
                    Validators.notAllowedOnSerializedProductCartLinesOperationValidator,
                    Validators.notAllowedToKeyInQuantityOnProductCartLinesOperationValidator
                ]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PickingAndReceiving,
        handler: new Operations.PickingAndReceivingOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });

    // ===== TENDER TYPES ========
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayCash,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayCard,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayCustomerAccount,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayCurrency,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayCheck,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayCreditMemo,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayGiftCertificate,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayLoyalty,
        handler: new Operations.PaymentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PayCashQuick,
        handler: new Operations.PayCashQuickOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler(<Operations.IOperation>{
        id: RetailOperation.VoidPayment,
        handler: new Operations.VoidPaymentOperationHandler(),
        validators: [
            <Operations.IValidator<Commerce.Model.Entities.TenderLine[]>>{
                dataAccessor: (options: Operations.IVoidPaymentOperationOptions) => { return options.tenderLines; },
                validatorFunctions: [Operations.Validators.singlePaymentLineOperationValidator]
            },
            {
                validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator]
            }
        ]
    });
    //===== TRANSACTION =========
    operationsManager.registerOperationHandler({
        id: RetailOperation.LineDiscountAmount,
        handler: new Operations.LineDiscountAmountOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.existingCart,
                    Validators.notAllowedOnCustomerAccountDeposit
                    ]
            },
            <Operations.IValidator<Commerce.Model.Entities.CartLine[]>>{
                dataAccessor: (options: Operations.ILineDiscountOperationOptions) => {
                    return options.cartLineDiscounts.map(cld => cld.cartLine);
                },
                validatorFunctions: [Validators.nonVoidedOperationValidator]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.LineDiscountPercent,
        handler: new Operations.LineDiscountPercentOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.existingCart,
                    Validators.notAllowedOnCustomerAccountDeposit
                ]
            },
            <Operations.IValidator<Commerce.Model.Entities.CartLine[]>>{
                dataAccessor: (options: Operations.ILineDiscountOperationOptions) => {
                    return options.cartLineDiscounts.map(cld => cld.cartLine);
                },
                validatorFunctions: [Validators.nonVoidedOperationValidator]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.TotalDiscountAmount,
        handler: new Operations.TotalDiscountAmountOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.existingCart,
                    Validators.notAllowedOnCustomerAccountDeposit
                ]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.TotalDiscountPercent,
        handler: new Operations.TotalDiscountPercentOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.existingCart,
                    Validators.notAllowedOnCustomerAccountDeposit
                ]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.DiscountCodeBarcode,
        handler: new Operations.AddDiscountCodeBarcodeOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CalculateFullDiscounts,
        handler: new Operations.CalculateTotalOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.existingCart,
                    Validators.notAllowedOnCustomerAccountDeposit
                ]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.VoidTransaction,
        handler: new Operations.VoidTransactionOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.TransactionComment,
        handler: new Operations.TransactionCommentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.InvoiceComment,
        handler: new Operations.InvoiceCommentOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.SuspendTransaction,
        handler: new Operations.SuspendTransactionOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.RecallTransaction,
        handler: new Operations.RecallTransactionOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.RecallSalesOrder,
        handler: new Operations.RecallCustomerOrderOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CustomerAccountDeposit,
        handler: new Operations.CustomerAccountDepositOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCartWithoutCustomerAccountOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCartWithNonCustomerAccountDepositLines] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });

    //===== OTHER =========
    operationsManager.registerOperationHandler({
        id: RetailOperation.AddAffiliation,
        handler: new Operations.AddAffiliationOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.AddAffiliationFromList,
        handler: new Operations.AddAffiliationFromListOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.LoyaltyRequest,
        handler: new Operations.AddLoyaltyCardOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.IssueCreditMemo,
        handler: new Operations.IssueCreditMemoOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.LoyaltyIssueCard,
        handler: new Operations.IssueLoyaltyCardOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.IssueGiftCertificate,
        handler: new Operations.IssueGiftCardOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.DisplayTotal,
        handler: new Operations.DisplayTotalOperationHandler(),
        validators: [
            {
                validatorFunctions: [
                    Validators.notAllowedInNonDrawerModeOperationValidator,
                    Validators.existingCart
                ]
            }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.AddToGiftCard,
        handler: new Operations.AddGiftCardOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.GiftCardBalance,
        handler: new Operations.GiftCardBalanceOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.IncomeAccounts,
        handler: new Operations.IncomeAccountsOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ExpenseAccounts,
        handler: new Operations.ExpenseAccountsOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });

    //==== CUSTOMER ======
    operationsManager.registerOperationHandler({
        id: RetailOperation.SetCustomer,
        handler: new Operations.AddCustomerToSalesOrderOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnRecalledOrder] },
            <Operations.IValidator<string>> {
                dataAccessor: (options: Operations.IAddCustomerToSalesOrderOperationOptions) => {
                    return options.customerId;
                },
                validatorFunctions: [
                    Validators.notAllowedOnCustomerAccountDepositWithNewCustomer
                ]
            }
        ],
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CustomerSearch,
        handler: new Operations.CustomerSearchOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedOnRecalledOrder] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ],
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CustomerAdd,
        handler: new Operations.CustomerAddOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CustomerEdit,
        handler: new Operations.CustomerEditOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CustomerClear,
        handler: new Operations.CustomerClearOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedOnRecalledOrder] },
            { validatorFunctions: [Validators.notAllowedOnCartWithCustomerAccountTenderLineOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.EditCustomerOrder,
        handler: new Operations.UpdateCustomerOrderOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CreateCustomerOrder,
        handler: new Operations.CreateCustomerOrderOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CreateQuotation,
        handler: new Operations.CreateCustomerQuoteOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.RecalculateCustomerOrder,
        handler: new Operations.RecalculateCustomerOrderOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PickupAllProducts,
        handler: new Operations.PickupAllOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PickupSelectedProducts,
        handler: new Operations.PickupSelectedOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ShipAllProducts,
        handler: new Operations.ShipAllOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ShipSelectedProducts,
        handler: new Operations.ShipSelectedOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.SetQuotationExpirationDate,
        handler: new Operations.SetQuotationExpirationDateOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            { validatorFunctions: [Validators.notAllowedOnCustomerAccountDeposit] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PaymentsHistory,
        handler: new Operations.GetPaymentsHistoryOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] },
            <Operations.IValidator<Commerce.Proxy.Entities.Cart>>{
                dataAccessor: (options: Operations.IGetPaymentsHistoryOperationOptions) => {
                    return options.cart
                },
                validatorFunctions: [Validators.paymentsHistoryOperationValidator]
            }
        ]
    });

    //==== TERMINAL ======
    operationsManager.registerOperationHandler({
        id: RetailOperation.LogOff,
        handler: new Operations.LogoffOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.DeactivateDevice,
        handler: new Operations.DeactivateDeviceOperationHandler()
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ChangeHardwareStation,
        handler: new Operations.SelectHardwareStationOperationHandler()
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PairHardwareStation,
        handler: new Operations.PairHardwareStationOperationHandler()
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.OpenDrawer,
        handler: new Operations.OpenCashDrawerOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.DatabaseConnectionStatus,
        handler: new Operations.DatabaseConnectionStatusOperationHandler()
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ChangePassword,
        handler: new Operations.ChangePasswordOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ResetPassword,
        handler: new Operations.ResetPasswordOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] }
        ]
    });

    //==== DAILY OPERATIONS ======
    operationsManager.registerOperationHandler({
        id: RetailOperation.TimeRegistration,
        handler: new Operations.TimeClockOperationHandler()
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ViewTimeClockEntries,
        handler: new Operations.ViewTimeClockEntriesOperationHandler()
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ShowJournal,
        handler: new Operations.ShowJournalOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.TenderDeclaration,
        handler: new Operations.TenderDeclarationOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            <Operations.IValidator<Commerce.Proxy.Entities.Shift>>{
                dataAccessor: (options: Operations.ITenderDeclarationOperationOptions) => {
                    return !Commerce.ObjectExtensions.isNullOrUndefined(options) ? options.shift : undefined;
                },
                validatorFunctions: [Validators.notAllowedInNonDrawerModeUnlessShiftIsProvidedOperationValidator]
            },
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.BlindCloseShift,
        handler: new Operations.BlindCloseShiftOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ShowBlindClosedShifts,
        handler: new Operations.ShowBlindClosedShiftsOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.SuspendShift,
        handler: new Operations.SuspendShiftOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.CloseShift,
        handler: new Operations.CloseShiftOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] },
            <Operations.IValidator<Commerce.Proxy.Entities.Shift>>{
                dataAccessor: (options: Operations.ICloseShiftOperationOptions) => {
                    return !Commerce.ObjectExtensions.isNullOrUndefined(options) ? options.shift : undefined;
                },
                validatorFunctions: [Validators.notAllowedInNonDrawerModeUnlessShiftIsProvidedOperationValidator]
            },
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PrintX,
        handler: new Operations.PrintXOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] },
            <Operations.IValidator<Commerce.Proxy.Entities.Shift>>{
                dataAccessor: (options: Operations.IPrintXOperationOptions) => {
                    return !Commerce.ObjectExtensions.isNullOrUndefined(options) ? options.shift : undefined;
                },
                validatorFunctions: [Validators.notAllowedInNonDrawerModeUnlessShiftIsProvidedOperationValidator]
            },
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.PrintZ,
        handler: new Operations.PrintZOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.notAllowedInOffline] },
            { validatorFunctions: [Validators.noExistingCart] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.DeclareStartAmount,
        handler: new Operations.DeclareStartAmountOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.FloatEntry,
        handler: new Operations.FloatEntryOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.TenderRemoval,
        handler: new Operations.TenderRemovalOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.SafeDrop,
        handler: new Operations.SafeDropOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.BankDrop,
        handler: new Operations.BankDropOperationHandler(),
        validators: [
            { validatorFunctions: [Validators.noExistingCart] },
            { validatorFunctions: [Validators.notAllowedInNonDrawerModeOperationValidator] }
        ]
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.BlankOperation,
        handler: new Operations.BlankOperationHandler()
    });
    operationsManager.registerOperationHandler({
        id: RetailOperation.ExtendedLogOn,
        handler: new Operations.ExtendedLogOnOperationHandler()
    });
});
