/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/Customer.ts'/>
///<reference path='../Entities/CustomerType.ts'/>
///<reference path='../Entities/Error.ts'/>
///<reference path='../Entities/ReceiptOption.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var ICustomerManagerName: string = "ICustomerManager";
    

    export interface ICustomerManager {
        /**
         * Get a page of records.
         * @param {Entities.CustomerSearchCriteria} searchCriteria The criteria to use for the search.
         * @param {number} [pageSize] Number of records per page to return.
         * @param {number} [skip] Number of records to skip.
         * @return {IAsyncResult<Entities.Customer[]>} The async result.
         */
        getRecordsPage(searchCriteria: Entities.CustomerSearchCriteria, pageSize?: number, skip?: number): IAsyncResult<Entities.Customer[]>;

        /**
         * Get customer details.
         * @param {string} accountNumber Customer account number.
         * @returns {IAsyncResult<Entities.ICustomerDetails>} The async result.
         */
        getCustomerDetailsAsync(accountNumber: string): IAsyncResult<Entities.ICustomerDetails>;

        /**
         * Get the list of customer types.
         * @returns {IAsyncResult<Entities.ICustomerType[]>} The async result.
         */
        getCustomerTypesAsync(): IAsyncResult<Entities.ICustomerType[]>;

        /**
         * Get the list of customer groups.
         * @returns {IAsyncResult<Entities.CustomerGroup[]>} The async result.
         */
        getCustomerGroupsAsync(): IAsyncResult<Entities.CustomerGroup[]>;

        /**
         * Adds a new customer.
         * @param {Commerce.Model.Entities.Customer} customer The customer to be added.
         * @returns {IAsyncResult<Entities.Customer>} The async result.
         */
        addNewCustomerAsync(customer: Entities.Customer): IAsyncResult<Entities.Customer>;

        /**
         * Get the list of receipt options.
         * @returns {IAsyncResult<Entities.ReceiptOption[]>} The async result.
         */
        getAllReceiptOptionsAsync(): IAsyncResult<Entities.ReceiptOption[]>;

        /**
         * Updates a customer.
         * @param {Entities.Customer} customer The customer to be updated.
         * @returns {IVoidAsyncResult} The async result.
         */
        updateCustomerAsync(customer: Entities.Customer): IVoidAsyncResult;

        /**
         * Search customer from a keyword.
         * @param {string} keyword A keyword to be searched.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of customers to skip.
         * @returns {IAsyncResult<Entities.Customer[]>} The async result.
         */
        searchCustomersAsync(keyword: string, pageSize?: number, skip?: number): IAsyncResult<Entities.GlobalCustomer[]>;

        /**
         * Loads a customer's wish lists.
         * @param {number} customerId The account number of the customer.
         * @returns {IAsyncResult<Entities.CommerceList[]>} The async result contains of array of customer's wish lists.
         */
        getWishListsByCustomerIdAsync(customerId: string): IAsyncResult<Entities.CommerceList[]>;

        /**
         * Validates an entire array of customer addresses.
         * @param {Entities.Address[]} customerAddresses The array of customer addresses to validate.
         * @return {Entities.Error[]} An array of errors describing which fields are invalid, if any.
         */
        validateCustomerAddresses(customerAddresses: Entities.Address[]): Entities.Error[];

        /**
         * Validates a single customer address.
         * @param {Entities.Address[]} customerAddresses The array of customer addresses to validate.
         * @param {Entities.CountryRegionInfo[]} [countriesInfo] The array of country region information
         * to determine which fields to validate based on localization.
         * @return {Entities.Error[]} An array of errors describing which fields are invalid, if any.
         */
        validateCustomerAddress(customerAddress: Entities.Address, countriesInfo?: Model.Entities.CountryRegionInfo[]): Entities.Error[];

        /**
         * Issues a loyalty card.
         * @param {Entities.LoyaltyCard} loyaltyCard The loyalty card to be updated.
         * @returns {IAsyncResult<Entities.LoyaltyCard>} The async result.
         */
        issueLoyaltyCardAsync(loyaltyCard: Entities.LoyaltyCard): IAsyncResult<Entities.LoyaltyCard>;

        /**
         * Gets a loyalty card by the card number.
         * @param {string} cardNumber The card number.
         * @returns {IAsyncResult<Entities.LoyaltyCard>} The async result.
         */
        getLoyaltyCardAsync(cardNumber: string): IAsyncResult<Entities.LoyaltyCard>;

        /**
         * Gets all the loyalty cards for the given customer
         * @param {string} accountNumber The customer account number.
         * @returns {IAsyncResult<Entities.LoyaltyCard[]>} The async result.
         */
        getCustomerLoyaltyCardsAsync(accountNumber: string): IAsyncResult<Entities.LoyaltyCard[]>;

        /**
         * Gets the customer balances.
         * @param {string} accountNumber The account number.
         * @param {string} invoiceAccountNumber The invoice account number.
         * @returns {IAsyncResult<Entities.CustomerBalances>} The async result.
         */
        getCustomerBalanceAsync(accountNumber: string, invoiceAccountNumber: string): IAsyncResult<Entities.CustomerBalances>;

        /**
         * Gets the purchase history for a customer.
         * @param {string} accountNumber The account number.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of customers to skip.
         * @returns {IAsyncResult<Entities.PurchaseHistory[]>} The async result.
         */
        getPurchaseHistoryAsync(accountNumber: string, pageSize?: number, skip?: number): IAsyncResult<Entities.PurchaseHistory[]>;
    }
}
