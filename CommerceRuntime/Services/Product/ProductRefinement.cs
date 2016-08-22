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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Implementation for product refinement service.
        /// </summary>
        internal static class ProductRefinement
        {
            /// <summary>
            /// Retrieves refiners for a given product search criteria.
            /// </summary>
            /// <param name="request">The product refiners request.</param>
            /// <returns>The product refiners response.</returns>
            public static GetProductRefinersServiceResponse GetProductRefiners(GetProductRefinersRequest request)
            {
                var dataRequest = new GetProductRefinersDataRequest(request.SearchCriteria);
    
                var dataResponse = request.RequestContext.Execute<GetProductRefinersDataResponse>(dataRequest);
    
                var refiners = AssembleRefiners(dataResponse.Refiners, dataResponse.RefinerValues);
    
                return new GetProductRefinersServiceResponse(refiners);
            }
    
            /// <summary>
            /// Refines the products.
            /// </summary>
            /// <param name="products">The products.</param>
            /// <param name="refinementCriteria">The refinement criteria.</param>
            /// <returns>A collection of products filtered by refinementCriteria.</returns>
            internal static ReadOnlyCollection<Product> RefineProducts(IEnumerable<Product> products, IEnumerable<ProductRefinerValue> refinementCriteria)
            {
                var productIdsNotSatisfyingRefinementCriteria = new List<long>();
                var refinedProducts = new List<Product>();
                var attributeRefinementCriteria = refinementCriteria.Where(r => r.RefinerSource == ProductRefinerSource.Attribute);
    
                foreach (var product in products)
                {
                    var refinementDictionary = attributeRefinementCriteria.GroupBy(r => r.RefinerRecordId, r => r, (key, values) => new { RecordId = key, Values = values });
    
                    foreach (var criterionSet in refinementDictionary)
                    {
                        bool isAnyCriteriaFromSetSatisfied = false;
    
                        foreach (var criterion in criterionSet.Values)
                        {
                            if (IsAttributeRefinementCriteriaSatisfied(criterion, product))
                            {
                                isAnyCriteriaFromSetSatisfied = true;
                                break;
                            }
                        }
    
                        if (!isAnyCriteriaFromSetSatisfied)
                        {
                            productIdsNotSatisfyingRefinementCriteria.Add(product.RecordId);
                        }
                    }
                }
    
                foreach (var product in products)
                {
                    if (!productIdsNotSatisfyingRefinementCriteria.Contains(product.RecordId))
                    {
                        refinedProducts.Add(product);
                    }
                }
    
                return refinedProducts.AsReadOnly();
            }
    
            /// <summary>
            /// Checks if the given criterion is satisfied by the product.
            /// </summary>
            /// <param name="criterion">The refinement criteria to match against the product.</param>
            /// <param name="product">The product that needs to be verified.</param>
            /// <returns>True if the attribute based criteria is satisfied by the given product.</returns>
            private static bool IsAttributeRefinementCriteriaSatisfied(ProductRefinerValue criterion, Product product)
            {
                if (!product.IsMasterProduct)
                {
                    if (IsCriteriaSatisfied(criterion, product.IndexedProductProperties))
                    {
                        return true;
                    }
                }
                else
                {
                    var variants = product.GetVariants();
    
                    // Check if crieria matches a property at master product level
                    if (IsCriteriaSatisfied(criterion, product.IndexedProductProperties))
                    {
                        return true;
                    }
    
                    // If criteria didn't match at master product level, look for it at the variant level
                    foreach (var variant in variants)
                    {
                        if (IsCriteriaSatisfied(criterion, variant.IndexedProperties))
                        {
                            return true;
                        }
                    }
                }
    
                return false;
            }
    
            /// <summary>
            /// Checks if the refiner value is satisfied by one of the properties in the given dictionary.
            /// </summary>
            /// <param name="criterion">The product refiner value to be used as the criterion to be verified.</param>
            /// <param name="indexedProperties">The product property translation dictionary for a product.</param>
            /// <returns>True if the criteria matches to at least one of the properties in the dictionary.</returns>
            private static bool IsCriteriaSatisfied(ProductRefinerValue criterion, ProductPropertyTranslationDictionary indexedProperties)
            {
                foreach (var locale in indexedProperties.Keys)
                {
                    foreach (var prop in indexedProperties[locale])
                    {
                        if (prop.Value.RecordId != criterion.RefinerRecordId)
                        {
                            continue;
                        }
    
                        if (criterion.LeftValueBoundString.Equals(criterion.RightValueBoundString) &&
                            prop.Value.Value.Equals(criterion.LeftValueBoundString))
                        {
                            return true;
                        }
    
                        switch (criterion.DataType)
                        {
                            case AttributeDataType.Decimal:
                            case AttributeDataType.Currency:
                                decimal leftDecimalBound = decimal.Parse(criterion.LeftValueBoundString);
                                decimal rightDecimalBound = decimal.Parse(criterion.RightValueBoundString);
                                if (decimal.Parse(prop.Value.ValueString) >= leftDecimalBound &&
                                    decimal.Parse(prop.Value.ValueString) <= rightDecimalBound)
                                {
                                    return true;
                                }
    
                                break;
    
                            case AttributeDataType.Integer:
                                int leftIntBound = int.Parse(criterion.LeftValueBoundString);
                                int rightIntBound = int.Parse(criterion.RightValueBoundString);
                                if (int.Parse(prop.Value.ValueString) >= leftIntBound &&
                                    int.Parse(prop.Value.ValueString) <= rightIntBound)
                                {
                                    return true;
                                }
    
                                break;
    
                            default:
                                return false;
                        }
                    }
                }
    
                return false;
            }
    
            private static ReadOnlyCollection<ProductRefiner> AssembleRefiners(IEnumerable<ProductRefiner> refiners, IEnumerable<ProductRefinerValue> refinerValues)
            {
                var result = new List<ProductRefiner>();
                foreach (var refiner in refiners)
                {
                    var values = refinerValues.Where(value => value.RefinerSource == refiner.Source && value.RefinerRecordId == refiner.RecordId);
    
                    // Preventing refiners without values from being returned
                    if (values.IsNullOrEmpty() || !AddValuesToRefiners(refiner, values))
                    {
                        continue;
                    }
    
                    result.Add(refiner);
                }
    
                return result.AsReadOnly();
            }
    
            /// <summary>
            /// Instantiates the values for this refiner.
            /// </summary>
            /// <param name="refiner">The product refiner to which values are to be associated.</param>
            /// <param name="refinerValues">The refiner values to be associated with the refiner.</param>
            /// <returns>A value indicating whether the refiner values were successfully initialized/associated or not.</returns>
            private static bool AddValuesToRefiners(ProductRefiner refiner, IEnumerable<ProductRefinerValue> refinerValues)
            {
                if (refinerValues == null)
                {
                    throw new ArgumentNullException("refinerValues");
                }
    
                var values = refinerValues.Where(v => v.RefinerRecordId == refiner.RecordId && v.RefinerSource == refiner.Source);
                var sortedRefinerValues = new List<ProductRefinerValue>();
    
                if (refiner.DataType == AttributeDataType.Currency || refiner.DataType == AttributeDataType.Decimal)
                {
                    sortedRefinerValues = values.OrderBy(v => float.Parse(v.Value)).ToList();
                }
                else if (refiner.DataType == AttributeDataType.Integer)
                {
                    sortedRefinerValues = values.OrderBy(v => int.Parse(v.Value)).ToList();
                }
                else if (refiner.DataType == AttributeDataType.Text)
                {
                    sortedRefinerValues = values.OrderBy(v => v.Value).ToList();
                }
    
                // Processing refiner values based on refiner display template.
                if (refiner.DisplayTemplate == DisplayTemplate.Slider)
                {
                    // There should be at least two values to display.
                    // min and max values for a slider refiner.
                    if (refinerValues.Count() < 2)
                    {
                        return false;
                    }
    
                    // Sort the refiner values to calculate the minimum and maximum values.
                    var min = sortedRefinerValues.First();
                    var max = sortedRefinerValues.Last();
    
                    min.LeftValueBoundString = min.Value;
                    min.RightValueBoundString = max.Value;
    
                    refiner.Values = new List<ProductRefinerValue> { min };
                    return true;
                }
                else if (refiner.DisplayTemplate == DisplayTemplate.Range)
                {
                    /*
                     * Creating range style refiner values using threshold values string.
                     * For example, a string of: "10;100;500;1000"
                     * would be translated to:
                     * Less than 10 (null, 10]
                     * 10 - 100 [10, 100]
                     * 100 - 500 [100, 500]
                     * 500 - 1000 [500, 1000]
                     * 1000 or more [1000, null)
                     */
                    var thresholdValues = new List<ProductRefinerValue>();
                    string[] thresholdValueStrings = refiner.ThresholdValues.Split(';');
    
                    for (int i = 0; i < thresholdValueStrings.Length; i++)
                    {
                        ProductRefinerValue threshold = new ProductRefinerValue();
    
                        if (i == 0)
                        {
                            continue;
                        }
    
                        threshold.RefinerRecordId = refiner.RecordId;
                        threshold.RefinerSource = refiner.Source;
                        threshold.DataType = refiner.DataType;
                        threshold.LeftValueBoundString = thresholdValueStrings[i - 1];
                        threshold.RightValueBoundString = thresholdValueStrings[i];
                        thresholdValues.Add(threshold);
                    }
    
                    refiner.Values = thresholdValues;
    
                    return true;
                }
    
                // No need for any special processing for list style refiners.
                // Refiner values for singular values would be of format [a, a].
                foreach (var value in sortedRefinerValues)
                {
                    value.LeftValueBoundString = value.Value;
                    value.RightValueBoundString = value.Value;
                }
    
                refiner.Values = sortedRefinerValues;
    
                return true;
            }
        }
    }
}
