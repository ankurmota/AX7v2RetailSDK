/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>

module Commerce {
    "use strict";

    import Entities = Proxy.Entities;

    export class SimpleProductHelper {
        /**
         * Formats the variant description based on the dimension values for the specified product.
         * @param {Entities.SimpleProduct | Entities.ProductComponent} product The product for which to create the description.
         * @return {string} The description of the variant.
         */
        public static getVariantDescription(product: Entities.SimpleProduct | Entities.ProductComponent): string {
            if (product.ProductTypeValue !== Entities.ProductType.Variant) {
                return StringExtensions.EMPTY;
            }

            return SimpleProductHelper.getProductDimensionsDescription(product.Dimensions);
        }

        /**
         * Gets the formatted description string for the provided product dimensions.
         * @param {Entities.ProductDimension[]} dimensions The product dimensions for which to get the description.
         * @return {string} The description of the product dimensions.
         */
        public static getProductDimensionsDescription(dimensions: Entities.ProductDimension[]): string {
            if (!ArrayExtensions.hasElements(dimensions)) {
                return StringExtensions.EMPTY;
            } else if (dimensions.length === 1) {
                return dimensions[0].DimensionValue.Value;
            }

            var formatString: string = ViewModelAdapter.getResourceString("string_4385"); // "{0}, {1}"
            var dimensionValues: string[] = dimensions.map((dimension: Entities.ProductDimension): string => {
                return ObjectExtensions.isNullOrUndefined(dimension.DimensionValue) ? StringExtensions.EMPTY : dimension.DimensionValue.Value;
            });

            // We build the description string this way in order to support LTR and RTL with the same logic.
            var formattedDescription: string = StringExtensions.format(formatString, dimensionValues[0], dimensionValues[1]);
            for (var i: number = 2; i < dimensionValues.length; ++i) {
                formattedDescription = StringExtensions.format(formatString, formattedDescription, dimensionValues[i]);
            }

            return formattedDescription;
        }

        /**
         * Gets the dimension value label.
         * @param {Entities.SimpleProduct} product The product from which to get the dimension value.
         * @param {Entities.ProductDimensionType} dimensionType The dimension type for which to get the value label.
         * @return {string} The dimension value label.
         */
        public static getDimensionValue(product: Entities.SimpleProduct, dimensionType: Entities.ProductDimensionType): string {
            var dimension: Entities.ProductDimension = ArrayExtensions.firstOrUndefined(product.Dimensions, (d: Entities.ProductDimension): boolean => {
                return d.DimensionTypeValue === dimensionType;
            });

            return ObjectExtensions.isNullOrUndefined(dimension) ? StringExtensions.EMPTY : dimension.DimensionValue.Value;
        }
    }
}