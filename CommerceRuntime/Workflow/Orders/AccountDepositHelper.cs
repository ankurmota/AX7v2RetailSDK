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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Customer Account Deposit Helper class.
        /// </summary>
        internal static class AccountDepositHelper
        {
            /// <summary>
            /// Validate customer account deposit transactions.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            public static void ValidateCustomerAccountDepositTransaction(RequestContext context, SalesTransaction salesTransaction)
            {
                if (salesTransaction.TransactionType != SalesTransactionType.CustomerAccountDeposit && salesTransaction.CustomerAccountDepositLines.Any())
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountDepositLinesNotAllowed, "Customer account deposit lines should be present on the transaction.");
                }

                // Make sure it is a customer account deposit transaction before applying additional validation
                if (salesTransaction.TransactionType != SalesTransactionType.CustomerAccountDeposit)
                {
                    return;
                }

                if (salesTransaction.CustomerAccountDepositLines.Any())
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.CustomerAccountDeposit));
                }

                // Validate that the transaction status is not set on customer account deposit lines.
                if (salesTransaction.CustomerAccountDepositLines.Any(customerAccountDepositLine => customerAccountDepositLine.TransactionStatus != TransactionStatus.Normal))
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountDepositLineDoesNotAllowSettingTransactionStatus, "The income/ expense line cannot have the transaction status set.");
                }

                // Check that the cart only has a single customer account deposit line. No other lines are allowed.
                if (salesTransaction.SalesLines.Any() || salesTransaction.CustomerAccountDepositLines.HasMultiple()  || salesTransaction.IncomeExpenseLines.Any())
                {
                   throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountDepositMultipleCartLinesNotAllowed, "Sales lines are not allowed on a customer account deposit transaction.");
                }

                // Transaction has to have customer id
                if (string.IsNullOrWhiteSpace(salesTransaction.CustomerId))
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountNumberIsNotSet, "The customer id must be set for account deposit transaction.");
                }

                // Adding cashier (or) manual discounts are not allowed.
                if (salesTransaction.TotalManualDiscountAmount != 0 || salesTransaction.TotalManualDiscountPercentage != 0)
                {
                   throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DiscountAmountInvalidated, "The discount is not allowed in account deposit transaction.");
                }

                // Analyze deposit cart line
                if (salesTransaction.CustomerAccountDepositLines.Any())
                {
                    CustomerAccountDepositLine depositLine = salesTransaction.CustomerAccountDepositLines.SingleOrDefault();

                    // Only positive deposit is allowed.
                    if (depositLine.Amount <= 0)
                    {
                        throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountDepositCannotBeNegative, "The customer account deposit amount must be positive.");
                    }

                    if (string.IsNullOrWhiteSpace(depositLine.CustomerAccount))
                    {
                        throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountNumberIsNotSet, "The customer id must be set for account deposit transaction.");
                    }

                    Customer customerOnTransaction = GetCustomer(context, salesTransaction.CustomerId);

                    bool useInvoiceAccount = !string.IsNullOrWhiteSpace(customerOnTransaction.InvoiceAccount);
                    string customerAccountForDeposit = useInvoiceAccount ? customerOnTransaction.InvoiceAccount : customerOnTransaction.AccountNumber;
                    if (!string.IsNullOrWhiteSpace(depositLine.CustomerAccount) && !depositLine.CustomerAccount.Equals(customerAccountForDeposit, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountMismatchBetweenTransactionAndDepositLine, "The customer account on the deposit line is different from the one on the cart.");
                    }
                }
            }

            /// <summary>
            /// Customer account deposit transactions cannot be tendered with the 'On Account' payment method.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="tenderLineBase">The tender line.</param>
            public static void ValidateCustomerAccountDepositPaymentRestrictions(RequestContext context, SalesTransaction transaction, TenderLineBase tenderLineBase)
            {
                if (transaction.CartType != CartType.AccountDeposit)
                {
                    return;
                }

                var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var tenderTypes = context.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, context).PagedEntityCollection.Results;
                TenderType tenderType = tenderTypes.Single(t => t.TenderTypeId.Equals(tenderLineBase.TenderTypeId));

                // It should not be allowed to pay with and deposit to the same customer account.
                if (tenderType.OperationType == RetailOperation.PayCustomerAccount &&
                    tenderLineBase.CustomerId.Equals(transaction.CustomerId))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotPayForCustomerAccountDepositWithCustomerAccountPaymentMethod, "Customer account deposit transactions cannot be tendered with the 'On Account' payment method.");
                }
            }

            /// <summary>
            /// Loads customer.
            /// </summary>
            /// <param name="context">The service request context.</param>
            /// <param name="accountNumber">The account number.</param>
            /// <returns>The customer, or null if not found.</returns>
            private static Customer GetCustomer(RequestContext context, string accountNumber)
            {
                var getCustomerDataRequest = new GetCustomerDataRequest(accountNumber);
                SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                Customer customer = getCustomerDataResponse.Entity;
                return customer;
            }
        }
    }
}
