/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";


    /**
     * Represents the customer search view model.
     */
    export class CustomerSearchViewModel extends ViewModelBase {
        private _customerSearchCriteria: Proxy.Entities.CustomerSearchCriteria;

        constructor() {
            super();
        }

        /**
         * Set customer search criteria for paging
         * @param {Entities.CustomerSearchCriteria} [customerSearchCriteria] Customer Search Criteria
         */
        public set CustomerSearchCriteria(customerSearchCriteria: Proxy.Entities.CustomerSearchCriteria) {
            this._customerSearchCriteria = customerSearchCriteria;

            if (this._customerSearchCriteria) {
                this._customerSearchCriteria.DataLevelValue = 1;

                // Search only local customers for demo mode
                if (Commerce.Config.isDemoMode) {
                    this._customerSearchCriteria.SearchOnlyCurrentCompany = true;
                }
            }
        }

        /**
         * Searches for customers.
         * @param {number} pageSize The number of records per page.
         * @param {number} skip The number of customers to skip.
         * @return {IAsyncResult<Proxy.Entities.Customer[]>} The async result containing the customers that match the search.
         */
        public searchCustomers(pageSize: number, skip: number): IAsyncResult<Proxy.Entities.Customer[]> {
            // If no customer search criteria has been set do not execute the workflow.
            if (ObjectExtensions.isNullOrUndefined(this._customerSearchCriteria)
                || StringExtensions.isNullOrWhitespace(this._customerSearchCriteria.Keyword)) {
                return AsyncResult.createResolved<Proxy.Entities.Customer[]>([]);
            }

            var searchResult: IAsyncResult<Proxy.Entities.Customer[]>;

            // Only execute the pre/post triggers if we are retrieving the first page of results.
            if (ObjectExtensions.isNullOrUndefined(skip) || NumberExtensions.areEquivalent(skip, 0)) {
                searchResult = Triggers.TriggerHelper.executeTriggerWorkflowAsync(
                    (): IAsyncResult<ICancelableResult> => {
                        var preTriggerOptions: Triggers.IPreCustomerSearchTriggerOptions = { cart: Session.instance.cart, searchText: this._customerSearchCriteria.Keyword };
                        return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreCustomerSearch, preTriggerOptions);
                    },
                    (): IAsyncResult<Proxy.Entities.Customer[]> => {
                        return this.customerManager.getRecordsPage(this._customerSearchCriteria, pageSize, skip);
                    },
                    (customers: Proxy.Entities.Customer[]): IVoidAsyncResult => {
                        var postTriggerOptions: Triggers.IPostCustomerSearchTriggerOptions = { cart: Session.instance.cart, customers: customers };
                        return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostCustomerSearch, postTriggerOptions);
                    }).map((result: ICancelableDataResult<Proxy.Entities.Customer[]>): Proxy.Entities.Customer[]=> {
                        return result && !result.canceled ? result.data : [];
                    });
            } else {
                searchResult = this.customerManager.getRecordsPage(this._customerSearchCriteria, pageSize, skip);
            }

            return searchResult.fail((errors: Proxy.Entities.Error[]): void => {
                NotificationHandler.displayClientErrors(errors);
            });
        }

        public addCustomerToSale(
            globalCustomer: Model.Entities.GlobalCustomer,
            cartAffiliations: Model.Entities.AffiliationLoyaltyTier[]): IAsyncResult<Model.Entities.Customer> {
            var customer: Model.Entities.Customer;
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.crossCompanyCustomerTransferAsync(globalCustomer)
                        .done((result) => { customer = result; });
                }).enqueue(() => {
                    var options: Operations.IAddCustomerToSalesOrderOperationOptions = {
                        customerId: customer.AccountNumber,
                        cartAffiliations: cartAffiliations
                    };

                    return this.operationsManager.runOperation(
                        Operations.RetailOperation.SetCustomer, options);
                });

            return asyncQueue.run().map(() => { return customer; });
        }

        /**
         * Transfers an existing customer to the current company from another company. Returns the same customer back if
         * already part of the current company.
         * 
         * @param {Commerce.Model.Entities.Customer} customer Customer to be transferred.
         * @returns {IAsyncResult<Entities.Customer>} The async result.
         */
        public crossCompanyCustomerTransferAsync(globalCustomer: Model.Entities.GlobalCustomer): IAsyncResult<Model.Entities.Customer> {
            // If not a cross company customer, return the customer
            if (!Commerce.CustomerHelper.isCrossCompanyCustomer(globalCustomer)) {
                return AsyncResult.createResolved(new Model.Entities.CustomerClass({
                    AccountNumber: globalCustomer.AccountNumber,
                    Name: globalCustomer.FullName
                }));
            }

            // Add the customer to the company. No validation is required as all fields should come from the
            // customer on the server.
            var newCustomer = new Model.Entities.CustomerClass();
            newCustomer.NewCustomerPartyNumber = globalCustomer.PartyNumber;

            return this.customerManager.addNewCustomerAsync(newCustomer);
        }
    }
}