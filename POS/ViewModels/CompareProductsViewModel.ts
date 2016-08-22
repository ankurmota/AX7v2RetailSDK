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
     * View Model used to compare products
     */
    export class CompareProductsViewModel extends ViewModelBase {

        public products: ObservableArray<Proxy.Entities.Product>;
        public allProductProperties: ObservableArray<any>;

        constructor() {
            super();

            this.products = ko.observableArray<Proxy.Entities.Product>([]);
            this.allProductProperties = ko.observableArray<Proxy.Entities.Product>([]);
        }

        /**
         * Returns a data structure which maps all existing properties to each product,
         * with its given property value from the database, otherwise assigns the property as "N/A" (localized)
         */
        public getPropertySetForDisplay() {
            var uniqueProductPropertyKeys = Object.create(null);
            var defaultEmptyValue = Commerce.ViewModelAdapter.getResourceString("string_705");

            var productPropertyCollection = this.parseProductProperties();

            // Iterates over the properties (by KeyName) of each selected product
            // and adds the KeyName to the 'keys' object. Results in a unique list of all KeyNames.
            for (var i = 0; i < productPropertyCollection.length; i++) {
                for (var key in productPropertyCollection[i].propertySet) {
                    uniqueProductPropertyKeys[key] = "";
                }
            }

            // We need to map product property to a collection which can be used for display
            var productPropertySetForDisplay = [];
            for (var i = 0; i < productPropertyCollection.length; i++) {
                var fullPropertySet = [];
                var recordId = productPropertyCollection[i].recordId;

                for (var key in uniqueProductPropertyKeys) {
                    var keyVal = {
                        KeyName: key,
                        ValueString: (
                        productPropertyCollection[i].propertySet[key] ?
                        productPropertyCollection[i].propertySet[key] : defaultEmptyValue
                        )
                    };

                    fullPropertySet.push(keyVal);
                }

                productPropertySetForDisplay.push({
                    productRecordId: recordId,
                    productProperties: fullPropertySet
                });
            }

            this.allProductProperties(productPropertySetForDisplay);
        }

        /**
         * Converts product properties of all selected products to
         * {"keyname" : "value string"} for easier manipulation when creating the prepared product data object.
         */
        private parseProductProperties(): Array<any> {
            var formattedProductProperties = [];
            var products: Proxy.Entities.Product[] = this.products();

            products.forEach((p: Proxy.Entities.Product) => {
                var ppt: Proxy.Entities.ProductPropertyTranslation = ArrayExtensions.firstOrUndefined(p.ProductProperties);
                if (ppt === undefined || !ArrayExtensions.hasElements(ppt.TranslatedProperties)) {
                    return;
                }

                var tempProductPropertyCollection = Object.create(null);
                ppt.TranslatedProperties.forEach((pp: Proxy.Entities.ProductProperty) => {
                    if (pp.PropertyTypeValue >= Proxy.Entities.ProductPropertyType.Video) {
                        return;
                    }

                    tempProductPropertyCollection[pp.FriendlyName] = pp.ValueString;
                });

                formattedProductProperties.push({
                    recordId: p.RecordId,
                    propertySet: tempProductPropertyCollection
                });
            });

            return formattedProductProperties;
        }

        /**
         * Takes a given record id and removes the matching element(s) and its
         * corresponding product properties. Updates the UI.
         * @param {number} productRecordId - Record Id of the product to be removed
         */
        public removeProductsFromCompareView(productRecordId: number) {
            // remove the selected product data (entirely) from the products field
            this.products(this.products().filter(function (item) {
                return item.RecordId != productRecordId;
            }));

            // remove the product properties list from the allProductProperties field
            this.allProductProperties(this.allProductProperties().filter(function (item) {
                return item.productRecordId != productRecordId;
            }));

            // rebuild the allProductProperties array to reflect only the properties of the remaining products
            this.getPropertySetForDisplay();
        }
    }
}