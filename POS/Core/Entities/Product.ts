/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='CommerceTypes.g.ts'/>

module Commerce.Proxy.Entities {

    /**
     * Represents the details of a product sale or return.
     */
    export interface ProductSaleReturnDetails {
        /**
         * The product to be sold or returned.
         */
        product?: Model.Entities.SimpleProduct;

        /**
         * The RecordId of the product to be sold or returned.
         */
        productId?: number;

        /**
         * The quantity to be sold or returned.
         */
        quantity: number;

        /**
        * The unit of the product to be sold or returned.
        */
        unitOfMeasureSymbol?: string;

        /**
         * The optional barcode of the product to be sold or returned.
         */
        barcode?: Proxy.Entities.Barcode;
    }

    /**
     * Represents the information for a sales line return.
     */
    export interface SalesLineReturn {

        /**
         * Return transaction identifier.
         */
        returnTransactionId: string;

        /**
         * The sales line of the product being returned.
         */
        salesLine: Model.Entities.SalesLine;

        /**
         * The quantity to be returned.
         */
        quantity: number;
    }

    /**
     * Represents the details of a product return.
     */
    export interface ProductReturnDetails {
        /**
         * The product manual return.
         */
        manualReturn: ProductSaleReturnDetails;

        /**
         * The cart line to return.
         */
        cartLine: Model.Entities.CartLine;

        /**
         * The sales line to return.
         */
        salesLineReturn: SalesLineReturn;
    }
}