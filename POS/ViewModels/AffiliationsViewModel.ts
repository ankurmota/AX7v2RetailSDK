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

    export class AffiliationsViewModel extends ViewModelBase {
        public affiliations: ObservableArray<Proxy.Entities.Affiliation>;
        public loyaltyTierId: number;

        /**
         * Initializes a new instance of the AffiliationsViewModel class.
         */
        constructor() {
            super();

            this.affiliations = ko.observableArray<Proxy.Entities.Affiliation>([]);
        }

        /**
         * Loads the view model by getting the affiliations.
         * @return {IVoidAsyncResult} The async result.
         */
        public load(): IVoidAsyncResult {
            return this.cartManager.getAffiliationsAsync()
                .done((affiliations: Proxy.Entities.Affiliation[]) => {
                    this.affiliations(affiliations || []);
                });
        }

        /**
         * Get the affiliations in current cart.
         * @return {Dictionary<Proxy.Entities.Affiliation>} The cart affiliation dictionary.
         */
        public getAffiliationsInCart(): Dictionary<Proxy.Entities.Affiliation> {
            var cartAffiliationDictionary: Dictionary<Proxy.Entities.Affiliation> = new Dictionary<Proxy.Entities.Affiliation>();
            var cartAffiliations = Session.instance.cart.AffiliationLines;

            if (!ObjectExtensions.isNullOrUndefined(cartAffiliations)) {
                cartAffiliations.forEach((cartAffiliation) => {
                    // Here only the AffiliationId is useful, other information of the cart affiliation is useless, so keeping the AffiliationId is enough.
                    cartAffiliationDictionary.setItem(cartAffiliation.AffiliationId, null);
                });
            }

            return cartAffiliationDictionary;
        }
    }
}