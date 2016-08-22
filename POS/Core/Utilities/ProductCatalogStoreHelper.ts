/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Core.d.ts'/>
///<reference path='../Session.ts'/>

module Commerce {
    "use strict";

    export class ProductCatalogStoreHelper {

        public static getStoreHeaderDetails(): string {
            var storeInformation: Model.Entities.OrgUnit = Commerce.Session.instance.productCatalogStore.Store;
            var storeId: string = storeInformation.OrgUnitNumber;
            var storeName: string = storeInformation.OrgUnitName;
            var catalogName: string = Commerce.Session.instance.catalogName;

            switch (Commerce.Session.instance.productCatalogStore.StoreType) {

                case (Model.Entities.StoreButtonControlType.Warehouse):
                    return Commerce.ViewModelAdapter.getResourceString("string_5504");
                default:
                    return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_611"),
                        storeId,
                        storeName,
                        catalogName);
            }

        }
    }
}