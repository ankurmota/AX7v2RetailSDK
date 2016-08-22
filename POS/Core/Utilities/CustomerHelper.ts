/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/CustomerType.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='../Core.d.ts'/>

module Commerce {
    "use strict";

    export class CustomerHelper {
        /**
         * Computes the customer type.
         * @param {Proxy.Entities.Customer} customer The customer.
         * @return {string} Customer type or an empty string if the customer type does not exist.
         */
        public static computeCustomerType(customer: Proxy.Entities.Customer): string {
            var result: string = "";
            if (!ObjectExtensions.isNullOrUndefined(customer)) {
                var customerType: Proxy.Entities.CustomerType = CustomerHelper.getCustomerType(customer.CustomerTypeValue);
                switch (customerType) {
                    case Proxy.Entities.CustomerType.Organization:
                        result = Commerce.ViewModelAdapter.getResourceString("string_304");
                        break;
                    case Proxy.Entities.CustomerType.Person:
                        result = Commerce.ViewModelAdapter.getResourceString("string_303");
                        break;
                }
            }

            return result;
        }

        /**
         * Checks whether the customer is a cross-company customer.
         * @param {Proxy.Entities.Customer} customer The customer.
         * @return {boolean} True if cross company customer, false if empty or not a cross company customers.
         */
        public static isCrossCompanyCustomer(customer: Proxy.Entities.GlobalCustomer): boolean {
            var result: boolean = false;
            if (!ObjectExtensions.isNullOrUndefined(customer)) {
                result = StringExtensions.isNullOrWhitespace(customer.AccountNumber);
            }

            return result;
        }

        /**
         * Get primary customer address from the customer object.
         * @param {Proxy.Entities.Customer} customer The customer object.
         * @return {Proxy.Entities.Address} Customer primary address, null if there is not a primary address.
         */
        public static getPrimaryCustomerAddressFromCustomerObject(customer: Proxy.Entities.Customer): Proxy.Entities.Address {
            if (!ObjectExtensions.isNullOrUndefined(customer) && !ObjectExtensions.isNullOrUndefined(customer.Addresses)) {
                for (var i: number = 0; i < customer.Addresses.length; i++) {
                    if (customer.Addresses[i].IsPrimary) {
                        return customer.Addresses[i];
                    }
                }
            }

            return null;
        }

        /**
         * Get customer type enum given the customer type.
         * @param {number} customerTypeEnum The customer type value.
         * @returns {Proxy.Entities.CustomerType} The customer type enumeration.
         */
        public static getCustomerType(customerTypeEnum: number): Proxy.Entities.CustomerType {
            var customerType: Proxy.Entities.CustomerType = Proxy.Entities.CustomerType.None;

            switch (customerTypeEnum) {
                case Proxy.Entities.CustomerType.Person:
                case 11779:
                case 12120:
                case 12023:
                    customerType = Proxy.Entities.CustomerType.Person;
                    break;
                case Proxy.Entities.CustomerType.Organization:
                case 1899:
                case 1929:
                case 1930:
                case 6213:
                case 7492:
                case 7618:
                case 8060:
                    customerType = Proxy.Entities.CustomerType.Organization;
                    break;
            }

            if (customerType !== Proxy.Entities.CustomerType.None) {
                return customerType;
            } else if (customerTypeEnum >= 11780) {
                return Proxy.Entities.CustomerType.Person;
            }

            return Proxy.Entities.CustomerType.Organization;
        }

        /**
         * Gets the customer balance on a customer given a Customer object and CustomerBalances object.
         * @param {Proxy.Entities.Customer} customer The customer object.
         * @param {Proxy.Entities.CustomerBalances} customerBalances The customer balances object.
         * @return number The customer balance. It will be 0 NaN if a required customer value was not supplied.
         */
        public static getCustomerBalance(customer: Proxy.Entities.Customer, customerBalances: Proxy.Entities.CustomerBalances): number {
            if (!ObjectExtensions.isNullOrUndefined(customer) && !ObjectExtensions.isNullOrUndefined(customerBalances)) {
                var customerBalance: number = !StringExtensions.isNullOrWhitespace(customer.InvoiceAccount)
                    ? (customerBalances.InvoiceAccountBalance + customerBalances.InvoiceAccountPendingBalance)
                    : (customerBalances.Balance + customerBalances.PendingBalance);

                return NumberExtensions.roundToNDigits(customerBalance, NumberExtensions.getDecimalPrecision());
            }

            return Number.NaN;
        }

        /**
         * Gets the credit limit for a customer given a Customer object and CustomerBalances object.
         * @param {Proxy.Entities.Customer} customer The customer object.
         * @param {Proxy.Entities.CustomerBalances} customerBalances The customer balances object.
         * @return number The customer balance. It will be 0 NaN if a required customer value was not supplied.
         */
        public static getCustomerCreditLimit(customer: Proxy.Entities.Customer, customerBalances: Proxy.Entities.CustomerBalances): number {
            var customerCreditLimit: number = Number.NaN;
            if (!ObjectExtensions.isNullOrUndefined(customer) && !ObjectExtensions.isNullOrUndefined(customerBalances)) {
                if (!StringExtensions.isNullOrWhitespace(customer.InvoiceAccount)) {
                    customerCreditLimit = customerBalances.InvoiceAccountCreditLimit;
                } else {
                    customerCreditLimit = customerBalances.CreditLimit;
                }
            }

            return customerCreditLimit;
        }
    }
}