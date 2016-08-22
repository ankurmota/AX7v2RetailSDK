/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/CommerceTypes.g.ts'/>
///<reference path='../../Entities/Customer.ts'/>
///<reference path='../../Entities/CustomerType.ts'/>
///<reference path='../../Entities/Error.ts'/>
///<reference path='../../Entities/ReceiptOption.ts'/>
///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../Utilities/CustomerHelper.ts'/>
///<reference path='../../RegularExpressionValidations.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../ICustomerManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class CustomerManager implements Commerce.Model.Managers.ICustomerManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Validates an entire array of customer addresses.
         * @param {Entities.Address[]} customerAddresses The array of customer addresses to validate.
         * @return {Entities.Error[]} An array of errors describing which fields are invalid, if any.
         */
        public validateCustomerAddresses(customerAddresses: Entities.Address[]): Entities.Error[] {
            var errors: Entities.Error[] = [];
            var countries: Model.Entities.CountryRegionInfo[] = Commerce.ApplicationContext.Instance.Countries;

            if (!ObjectExtensions.isNullOrUndefined(customerAddresses)) {
                for (var i: number = 0; i < customerAddresses.length; i++) {
                    var customerAddress: Entities.Address = customerAddresses[i];
                    errors = errors.concat(this.validateCustomerAddress(customerAddress, countries));
                }
            }

            return errors;
        }

        /**
         * Validates a single customer address.
         * @param {Entities.Address[]} customerAddresses The array of customer addresses to validate.
         * @param {Entities.CountryRegionInfo[]} [countriesInfo] The array of country region information
         * to determine which fields to validate based on localization.
         * @return {Entities.Error[]} An array of errors describing which fields are invalid, if any.
         */
        public validateCustomerAddress(customerAddress: Entities.Address, countriesInfo?: Model.Entities.CountryRegionInfo[]): Entities.Error[] {
            var errors: Entities.Error[] = [];
            var countries: Entities.CountryRegionInfo[] = countriesInfo || Commerce.ApplicationContext.Instance.Countries;

            // Commerce.Model.Entities.CountryRegionInfo
            if (!ObjectExtensions.isNullOrUndefined(customerAddress) && StringExtensions.isEmptyOrWhitespace(customerAddress.ThreeLetterISORegionName)) {
                errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_COUNTRY));
            } else {
                // ThreeLetterISORegionName must be passed in order for us to proceed with validation:
                // get the format lines for the given country
                var matchingCountry: Entities.CountryRegionInfo[] = countries.filter((c: Entities.CountryRegionInfo) =>
                    c.CountryRegionId === customerAddress.ThreeLetterISORegionName);
                var addressFormatLines: Entities.AddressFormattingInfo[] = matchingCountry.length > 0 ? matchingCountry[0].AddressFormatLines : [];

                if (!ArrayExtensions.hasElements(addressFormatLines)) {
                    // address format lines cannot be null; throws an exception if the address format lines are not returned.
                    errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_STREET));
                }

                for (var i: number = 0; i < addressFormatLines.length; i++) {
                    var addressComponentName: number = addressFormatLines[i].AddressComponentNameValue;

                    if (addressComponentName === Entities.AddressFormatLineType.StreetName
                        && ObjectExtensions.isNullOrUndefined(customerAddress.Street)
                        || StringExtensions.isEmptyOrWhitespace(customerAddress.Street)) {
                        errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_STREET));
                    }

                    if (addressComponentName === Entities.AddressFormatLineType.ZipCode
                        && ObjectExtensions.isNullOrUndefined(customerAddress.ZipCode)
                        || StringExtensions.isEmptyOrWhitespace(customerAddress.ZipCode)) {
                        errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_ZIPCODE));
                    }

                    if (addressComponentName === Entities.AddressFormatLineType.State
                        && ObjectExtensions.isNullOrUndefined(customerAddress.State)
                        || StringExtensions.isEmptyOrWhitespace(customerAddress.State)) {
                        errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_STATE));
                    }

                    if (addressComponentName === Entities.AddressFormatLineType.City
                        && ObjectExtensions.isNullOrUndefined(customerAddress.City)
                        || StringExtensions.isEmptyOrWhitespace(customerAddress.City)) {
                        errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_CITY));
                    }
                }
            }
            return errors;
        }

        /**
         * Adds a new customer.
         * @param {Commerce.Model.Entities.Customer} customer The customer to be added.
         * @returns {IAsyncResult<Entities.Customer>} The async result.
         */
        public addNewCustomerAsync(customer: Entities.Customer): IAsyncResult<Entities.Customer> {
            // validate new customer
            var errors: Entities.Error[] = this.validateCreateOrUpdateCustomer(customer);

            if (ArrayExtensions.hasElements(errors)) {
                return AsyncResult.createRejected<Entities.Customer>(errors);
            }

            this.updateFullAddresses(customer.Addresses);

            var request: Common.IDataServiceRequest = this._commerceContext.customers().create(customer);
            return request.execute<Entities.Customer>();
        }

        /**
         * Get customer details.
         * @param {string} accountNumber Customer account number.
         * @returns {IAsyncResult<Entities.ICustomerDetails>} The async result.
         */
        public getCustomerDetailsAsync(accountNumber: string): IAsyncResult<Entities.ICustomerDetails> {
            var request: Common.IDataServiceRequest = this._commerceContext.customers(accountNumber).read();
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var customer: Entities.Customer;

            asyncQueue.enqueue((): IVoidAsyncResult => {
                return request.execute<Entities.Customer>()
                    .done((result: Entities.Customer): void => {
                        customer = result;
                    });
            }).enqueue((): IVoidAsyncResult => {
                if (ObjectExtensions.isNullOrUndefined(customer)) {
                    var errors: Model.Entities.Error[] =
                        [new Model.Entities.Error(ErrorTypeEnum.CUSTOMER_NOT_FOUND, false, null, null, [accountNumber])];

                    return VoidAsyncResult.createRejected(errors);
                }

                return VoidAsyncResult.createResolved();
            });

            return asyncQueue.run().map((value: ICancelableResult): Entities.ICustomerDetails => {
                var primaryAddress: Entities.Address = null;
                if (Commerce.ArrayExtensions.hasElements(customer.Addresses)) {
                    for (var index: number = 0; index < customer.Addresses.length; index++) {
                        if (customer.Addresses[index].IsPrimary) {
                            primaryAddress = customer.Addresses[index];
                            break;
                        }
                    }
                }

                return <Entities.ICustomerDetails>{ customer: customer, primaryAddress: primaryAddress };
            });
        }

        /**
         * Get a page of records.
         * @param {Entities.CustomerSearchCriteria} searchCriteria The criteria to use for the search.
         * @param {number} [pageSize] Number of records per page to return.
         * @param {number} [skip] Number of records to skip.
         * @return {IAsyncResult<Entities.Customer[]>} The async result.
         */
        public getRecordsPage(searchCriteria: Entities.CustomerSearchCriteria, pageSize: number, skip: number): IAsyncResult<Entities.Customer[]> {
            if (searchCriteria) {
                var query: Proxy.CustomersDataServiceQuery = this._commerceContext.customers();
                query.top(pageSize).skip(skip);

                return query.search(searchCriteria).execute<Entities.Customer[]>();
            }

            return AsyncResult.createResolved<Entities.Customer[]>([]);
        }

        /**
         * Get the list of customer types.
         * @returns {IAsyncResult<Entities.ICustomerType[]>} The async result.
         */
        public getCustomerTypesAsync(): IAsyncResult<Entities.ICustomerType[]> {
            var customerTypes: Entities.ICustomerType[] = [
                { Code: Entities.CustomerType.Person, Name: Commerce.ViewModelAdapter.getResourceString("string_303") },
                { Code: Entities.CustomerType.Organization, Name: Commerce.ViewModelAdapter.getResourceString("string_304") }
            ];

            return AsyncResult.createResolved<Entities.ICustomerType[]>(customerTypes);
        }

        /**
         * Get the list of customer groups.
         * @returns {IAsyncResult<Entities.CustomerGroup[]>} The async result.
         */
        public getCustomerGroupsAsync(): IAsyncResult<Entities.CustomerGroup[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCustomerGroups();
            return request.execute<Entities.CustomerGroup[]>()
                .map((customerGroups: Entities.CustomerGroup[]): Entities.CustomerGroup[] => {
                    customerGroups.forEach((cg: Entities.CustomerGroup) => {
                        // if the values for description are empty just use the customer group identifier.
                        if (StringExtensions.isEmptyOrWhitespace(cg.CustomerGroupName)) {
                            cg.CustomerGroupName = cg.CustomerGroupNumber;
                        }
                    });

                    return customerGroups;
                });
        }

        /**
         * Get the list of receipt options.
         * @returns {IAsyncResult<Entities.ReceiptOption[]>} The async result.
         */
        public getAllReceiptOptionsAsync(): IAsyncResult<Entities.ReceiptOption[]> {
            var receiptOptions: Entities.ReceiptOption[] = [
                { Code: Entities.ReceiptOptionCodeEnum.POS, ResourceString: "string_1363" },
                { Code: Entities.ReceiptOptionCodeEnum.Email, ResourceString: "string_1364" },
                { Code: Entities.ReceiptOptionCodeEnum.Both, ResourceString: "string_1365" }
            ];

            return AsyncResult.createResolved<Entities.ReceiptOption[]>(receiptOptions);
        }

        /**
         * Updates a customer.
         * @param {Entities.Customer} customer The customer to be updated.
         * @returns {IVoidAsyncResult} The async result.
         */
        public updateCustomerAsync(customer: Entities.Customer): IVoidAsyncResult {
            // validate updated customer
            var errors: Entities.Error[] = this.validateCreateOrUpdateCustomer(customer);
           if (ArrayExtensions.hasElements(errors)) {
                RetailLogger.modelManagersCustomerManagerCustomerValidationFailed();
                return VoidAsyncResult.createRejected(errors);
            }

            // update addresses
            this.updateFullAddresses(customer.Addresses);

            // update customer
            var request: Common.IDataServiceRequest = this._commerceContext.customers(customer.AccountNumber).update(customer);
            return request.execute().done((customer: Entities.Customer) => {
                if (!ObjectExtensions.isNullOrUndefined(Session.instance.Customer) &&
                    Session.instance.Customer.AccountNumber === customer.AccountNumber) {
                    Session.instance.Customer = customer;
                }
            });
        }

        /**
         * Search customer from a keyword.
         * @param {string} keyword A keyword to be searched.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of customers to skip.
         * @returns {IAsyncResult<Entities.GlobalCustomer[]>} The async result.
         */
        public searchCustomersAsync(
            keyword: string,
            pageSize: number = Commerce.Config.defaultPageSize,
            skip: number = 0): IAsyncResult<Entities.GlobalCustomer[]> {

            if (StringExtensions.isNullOrWhitespace(keyword)) {
                return AsyncResult.createResolved<Entities.GlobalCustomer[]>([]);
            }

            var query: Proxy.CustomersDataServiceQuery = this._commerceContext.customers();
            var searchCriteria: Entities.CustomerSearchCriteria = {
                Keyword: keyword,
                DataLevelValue: 4
            };

            query.top(pageSize).skip(skip).inlineCount();
            return query.search(searchCriteria).execute<Entities.GlobalCustomer[]>();
        }

        /**
         * Loads a customer's wish lists.
         * @param {number} customerId The account number of the customer.
         * @returns {IAsyncResult<Entities.CommerceList[]>} The async result contains of array of customer's wish lists.
         */
        public getWishListsByCustomerIdAsync(customerId: string): IAsyncResult<Entities.CommerceList[]> {
            // customer null validation
            if (ObjectExtensions.isNullOrUndefined(customerId) || StringExtensions.isEmptyOrWhitespace(customerId)) {
                RetailLogger.modelManagersCustomerManagerCustomerIsNull();
            }

            var query: Proxy.CommerceListsDataServiceQuery = this._commerceContext.commerceLists();
            return query.getByCustomer(customerId).execute<Entities.CommerceListLine[]>();
        }

        /**
         * Issues a loyalty card.
         * @param {Entities.LoyaltyCard} loyaltyCard The loyalty card to be updated.
         * @returns {IAsyncResult<Entities.LoyaltyCard>} The async result.
         */
        public issueLoyaltyCardAsync(loyaltyCard: Entities.LoyaltyCard): IAsyncResult<Entities.LoyaltyCard> {
            // issue loyalty card
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().issueLoyaltyCard(loyaltyCard);
            return request.execute<Entities.LoyaltyCard>();
        }

        /**
         * Gets a loyalty card by the card number.
         * @param {string} cardNumber The card number.
         * @returns {IAsyncResult<Entities.LoyaltyCard>} The async result.
         */
        public getLoyaltyCardAsync(cardNumber: string): IAsyncResult<Entities.LoyaltyCard> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getLoyaltyCard(cardNumber);
            return request.execute<Entities.LoyaltyCard>();
        }

        /**
         * Gets all the loyalty cards for the given customer
         * @param {string} accountNumber The customer account number.
         * @returns {IAsyncResult<Entities.LoyaltyCard[]>} The async result.
         */
        public getCustomerLoyaltyCardsAsync(accountNumber: string): IAsyncResult<Entities.LoyaltyCard[]> {
            // get customer loyalty cards
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCustomerLoyaltyCards(accountNumber);
            return request.execute<Entities.LoyaltyCard[]>();
        }

        /**
         * Gets the customer balances.
         * @param {string} accountNumber The account number.
         * @param {string} invoiceAccountNumber The invoice account number.
         * @returns {IAsyncResult<Entities.CustomerBalances>} The async result.
         */
        public getCustomerBalanceAsync(accountNumber: string, invoiceAccountNumber: string): IAsyncResult<Entities.CustomerBalances> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCustomerBalance(accountNumber, invoiceAccountNumber);
            return request.execute<Entities.CustomerBalances>();
        }

        /**
         * Gets the purchase history for a customer.
         * @param {string} accountNumber The account number.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of customers to skip.
         * @returns {IAsyncResult<Entities.PurchaseHistory[]>} The async result.
         */
         public getPurchaseHistoryAsync(accountNumber: string, pageSize?: number, skip?: number): IAsyncResult<Entities.PurchaseHistory[]> {
             var query: Proxy.CustomersDataServiceQuery = this._commerceContext.customers(accountNumber);
             query.top(pageSize).skip(skip);
             return query.getPurchaseHistory().execute<Entities.PurchaseHistory[]>();
         }

        /**
         * Validate a new customer object before it is created on retail server.
         * These validations should validate simple fields shown in UI like name and address to reduce trip to retail server.
         * @param {Commerce.Model.Entities.Customer} newCustomer A new customer to be validated.
         * @returns {Entities.Error[]} List of errors found after validation is done.
         */
        private validateCreateOrUpdateCustomer(newCustomer: Entities.Customer): Entities.Error[] {
            var errors: Entities.Error[] = [];

            // customer null validation
            if (ObjectExtensions.isNullOrUndefined(newCustomer)) {
                RetailLogger.genericError("Customer cannot be null or undefined.");
                errors.push(new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR));
                return errors;
            }

            // in this case we are trying to create a customer from a customer search results. 
            // The customer is created on the server so no validation is needed.
            if (!StringExtensions.isEmptyOrWhitespace(newCustomer.NewCustomerPartyNumber)) {
                return errors;
            }

            if (newCustomer.CustomerTypeValue === Model.Entities.CustomerType.Organization) {
                // organization name not empty string validation
                if (StringExtensions.isEmptyOrWhitespace(newCustomer.Name)) {
                    errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_NAME));
                }

                // organization name regex
                if (!Core.RegularExpressionValidations.validateNameFieldForCustomer(newCustomer.Name)) {
                    errors.push(new Entities.Error(ErrorTypeEnum.INVALID_NAME_FORMAT));
                }
            } else {
                // customer first & last name not empty string validation
                if (StringExtensions.isEmptyOrWhitespace(newCustomer.FirstName) || StringExtensions.isEmptyOrWhitespace(newCustomer.LastName)) {
                    errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_NAME));
                }

                // customer valid first & last name
                if (!Core.RegularExpressionValidations.validateNameFieldForCustomer(newCustomer.FirstName)
                    || !Core.RegularExpressionValidations.validateNameFieldForCustomer(newCustomer.LastName)) {
                    RetailLogger.genericError("Invalid values were given for customer first or last name. Please use text only.");
                    errors.push(new Entities.Error(ErrorTypeEnum.INVALID_NAME));
                }
            }

            // URL validation
            if (!ObjectExtensions.isNullOrUndefined(newCustomer.Url) && !StringExtensions.isEmptyOrWhitespace(newCustomer.Url)
                && !Core.RegularExpressionValidations.validateUrl(newCustomer.Url)) {
                errors.push(new Entities.Error(ErrorTypeEnum.INVALID_URL));
            }

            // email validation
            if (!ObjectExtensions.isNullOrUndefined(newCustomer.Email) && !StringExtensions.isEmptyOrWhitespace(newCustomer.Email)
                && !Core.RegularExpressionValidations.validateEmail(newCustomer.Email)) {
                errors.push(new Entities.Error(ErrorTypeEnum.INVALID_EMAIL));
            }

            // phone validation
            if (!ObjectExtensions.isNullOrUndefined(newCustomer.Phone) && !StringExtensions.isEmptyOrWhitespace(newCustomer.Phone)
                && !Core.RegularExpressionValidations.validatePhone(newCustomer.Phone)) {
                errors.push(new Entities.Error(ErrorTypeEnum.INVALID_PHONE));
            }

            // address validation
            // it is not required to have an address
            errors = errors.concat(this.validateCustomerAddresses(newCustomer.Addresses));

            // name validation as per spec on create customer
            if (newCustomer.CustomerTypeValue === Entities.CustomerType.Person
                && StringExtensions.isEmptyOrWhitespace(newCustomer.FirstName)
                && StringExtensions.isEmptyOrWhitespace(newCustomer.MiddleName)
                && StringExtensions.isEmptyOrWhitespace(newCustomer.LastName)
                && StringExtensions.isEmptyOrWhitespace(newCustomer.NewCustomerPartyNumber)) {
                RetailLogger.genericError("At least one field between first name, middle name, and last name required if customer type is Person.");
                errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_NAME));
            } else if (newCustomer.CustomerTypeValue === Entities.CustomerType.Organization
                && StringExtensions.isEmptyOrWhitespace(newCustomer.Name)
                && StringExtensions.isEmptyOrWhitespace(newCustomer.NewCustomerPartyNumber)) {

                errors.push(new Entities.Error(ErrorTypeEnum.EMPTY_NAME));
            }

            return errors;
        }

        /**
         * Update full addresses.
         */
        private updateFullAddresses(customerAddresses: Entities.Address[]): void {
            if (ArrayExtensions.hasElements(customerAddresses)) {
                for (var i: number = 0; i < customerAddresses.length; i++) {
                    this.updateFullAddress(customerAddresses[i]);
                }
            }
        }

        /**
         * Update customer full address.
         */
        private updateFullAddress(customerAddress: Entities.Address): void {
            customerAddress.FullAddress = customerAddress.Street + "\n"
            + customerAddress.City + ", "
            + customerAddress.State + " "
            + customerAddress.ZipCode + "\n"
            + customerAddress.ThreeLetterISORegionName;
        }
    }
}
