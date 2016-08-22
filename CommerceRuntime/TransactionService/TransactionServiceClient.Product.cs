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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.IO;
        using System.Linq;
        using System.Xml;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using CP = Retail.TransactionServices.ClientProxy;

        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Attribute names.
            private const string ItemIdAttributeName = "ItemId";
            private const string NameAttributeName = "Name";
            private const string PriceAttributeName = "Price";
            private const string RecordIdAttributeName = "RecordId";

            // API names.
            private const string GetProductDataMethodName = "getProductData";
            private const string GetProductsByCategoryMethodName = "getProductsByCategory";
            private const string GetProductsByKeywordMethodName = "getProductsByKeyword";

            /// <summary>
            /// Gets the products using the specified category identifier.
            /// </summary>
            /// <param name="currentChannelId">The channel identifier of the current context.</param>
            /// <param name="targetChannelId">The channel identifier of the target channel.</param>
            /// <param name="targetCatalogId">The catalog identifier in the target channel.</param>
            /// <param name="targetCategoryId">The category identifier in the target channel.</param>
            /// <param name="skip">The number of records to skip.</param>
            /// <param name="top">The maximum number of records to return.</param>
            /// <param name="attributeIds">The comma-separated list of attribute record identifiers to retrieve. Specify '*' to retrieve all attributes.</param>
            /// <param name="includeProductsFromDescendantCategories">Whether category based product search should return products from all descendant categories.</param>
            /// <returns>A collection of products under the specified category.</returns>
            public ReadOnlyCollection<Product> GetProductsByCategory(
                long currentChannelId, long targetChannelId, long targetCatalogId, long targetCategoryId, long skip, long top, string attributeIds, bool includeProductsFromDescendantCategories)
            {
                if (currentChannelId <= 0)
                {
                    throw new ArgumentOutOfRangeException("currentChannelId", "The current channel identifier is required.");
                }

                if (skip < 0 || skip == int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("skip");
                }

                if (top < 0)
                {
                    throw new ArgumentOutOfRangeException("top", "The value must be a positive integer.");
                }

                ThrowIf.NullOrWhiteSpace(attributeIds, "attributeIds");

                CP.RetailTransactionServiceResponse serviceResponse = this.GetResponseFromMethod(
                    GetProductsByCategoryMethodName,
                    currentChannelId,
                    targetCategoryId,
                    skip + 1, // 1-based in AX
                    top,
                    "ItemId", // order by
                    1, // sort order
                    false, // return total count
                    string.Empty, // language
                    targetChannelId,
                    targetCatalogId,
                    attributeIds,
                    true,           // _includePrice
                    includeProductsFromDescendantCategories);

                // Check result
                if (!serviceResponse.Success)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure,
                        string.Format("Invoke method {0} failed: {1}", GetProductsByCategoryMethodName, serviceResponse.Message));
                }

                // Throw if service response does not contain any data.
                if (serviceResponse.Data == null || serviceResponse.Data.Length == 0)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        "Service response does not contain any data.");
                }

                string productsXml = (string)serviceResponse.Data[0];
                var products = this.ConvertProductsXmlToProducts(productsXml);
                return products;
            }

            /// <summary>
            /// Gets the products matching the specified search keywords.
            /// </summary>
            /// <param name="currentChannelId">The channel identifier of the current context.</param>
            /// <param name="targetChannelId">The channel identifier of the target channel.</param>
            /// <param name="targetCatalogId">The catalog identifier in the target channel.</param>
            /// <param name="keyword">The search keyword.</param>
            /// <param name="skip">The number of records to skip.</param>
            /// <param name="top">The maximum number of records to return.</param>
            /// <param name="attributeIds">The comma-separated list of attribute record identifiers to retrieve. Specify '*' to retrieve all attributes.</param>
            /// <returns>A collection of products matching the specified search keywords.</returns>
            public ReadOnlyCollection<Product> GetProductsByKeyword(
                long currentChannelId, long targetChannelId, long targetCatalogId, string keyword, long skip, long top, string attributeIds)
            {
                if (currentChannelId <= 0)
                {
                    throw new ArgumentOutOfRangeException("currentChannelId", "The current channel identifier is required.");
                }

                ThrowIf.NullOrWhiteSpace(keyword, "keyword");

                if (skip < 0 || skip == int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("skip");
                }

                if (top < 0)
                {
                    throw new ArgumentOutOfRangeException("top", "The value must be a positive integer.");
                }

                ThrowIf.NullOrWhiteSpace(attributeIds, "attributeIds");

                CP.RetailTransactionServiceResponse serviceResponse = this.GetResponseFromMethod(
                    GetProductsByKeywordMethodName,
                    currentChannelId,
                    keyword,
                    skip + 1, // 1-based in AX
                    top,
                    "ItemId", // order by
                    1, // sort order
                    false, // return total count
                    string.Empty, // language
                    targetChannelId,
                    targetCatalogId,
                    attributeIds);

                // Check result
                if (!serviceResponse.Success)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure,
                        string.Format("Invoke method {0} failed: {1}", GetProductsByKeywordMethodName, serviceResponse.Message));
                }

                // Throw if service response does not contain any data.
                if (serviceResponse.Data == null || serviceResponse.Data.Length == 0)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        "Service response does not contain any data.");
                }

                string productsXml = (string)serviceResponse.Data[0];
                var products = this.ConvertProductsXmlToProducts(productsXml);
                return products;
            }

            /// <summary>
            /// Gets the product data (as an XML document) for the specified product identifiers.
            /// </summary>
            /// <param name="productIds">A collection of product identifiers.</param>
            /// <returns>An XML document containing all of the relevant product data.</returns>
            public XDocument GetProductData(IEnumerable<long> productIds)
            {
                ThrowIf.Null(productIds, "productIds");

                return this.GetProductData<long>(productIds, "Product");
            }

            /// <summary>
            /// Gets the product data (as an XML document) for the specified item identifiers.
            /// </summary>
            /// <param name="itemIds">A collection of item identifiers.</param>
            /// <returns>An XML document containing all of the relevant product data.</returns>
            public XDocument GetProductData(IEnumerable<string> itemIds)
            {
                ThrowIf.Null(itemIds, "itemIds");

                return this.GetProductData<string>(itemIds, "ItemId");
            }

            /// <summary>
            /// Gets the product search results using the specified category identifier.
            /// </summary>
            /// <param name="currentChannelId">The identifier of the channel that the request is originating from.</param>
            /// <param name="categoryId">The identifier of the category to which the results must belong.</param>
            /// <param name="locale">The culture specific language identifier to which the results must be translated.</param>
            /// <param name="targetChannelId">The identifier of the channel to which the resultant product representatives must belong to.</param>
            /// <param name="targetCatalogId">The identifier of the catalog to which the resultant product representatives must belong to.</param>
            /// <param name="attributeIdCollectionString">The attribute values to retrieve along with the result set.</param>
            /// <param name="settings">The settings to use while processing this request.</param>
            /// <returns>A collection of product search results representing products in the requested category or its sub-categories.</returns>
            internal ReadOnlyCollection<ProductSearchResult> SearchProductsByCategoryId(long currentChannelId, long categoryId, string locale, long targetChannelId, long targetCatalogId, string attributeIdCollectionString, QueryResultSettings settings)
            {
                CP.RetailTransactionServiceResponse transactionServiceResponse = this.GetResponseFromMethod(
                    GetProductsByCategoryMethodName,
                    currentChannelId,
                    categoryId,
                    settings.Paging.Skip + 1,
                    settings.Paging.NumberOfRecordsToFetch,
                    settings.Sorting == null ? RecordIdAttributeName : settings.Sorting.ToString(),  // order by
                    settings.Sorting == null || !settings.Sorting.IsSpecified ? 0 : (settings.Sorting.Columns.First().IsDescending ? 1 : 0),  // sort order: Ascending by default
                    settings.Paging.CalculateRecordCount,  // return total count
                    locale,
                    targetChannelId,
                    targetCatalogId,
                    attributeIdCollectionString,
                    true,  // _includePrice
                    true);  // includeProductsFromDescendantCategories

                // Check result
                if (!transactionServiceResponse.Success)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure,
                        string.Format("Invoke method {0} failed: {1}", GetProductsByCategoryMethodName, transactionServiceResponse.Message));
                }

                // Throw if service response does not contain any data.
                if (transactionServiceResponse.Data == null || transactionServiceResponse.Data.Length == 0)
                {
                    throw new CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError, "Service response does not contain any data.");
                }

                string searchResultsXml = (string)transactionServiceResponse.Data[0];
                var results = this.GetResultsFromXml(searchResultsXml);

                return results;
            }

            /// <summary>
            /// Gets the product search results using the specified category identifier.
            /// </summary>
            /// <param name="currentChannelId">The identifier of the channel that the request is originating from.</param>
            /// <param name="searchText">The search text that the result should be relevant to.</param>
            /// <param name="targetChannelId">The identifier of the channel to which the resultant product representatives must belong to.</param>
            /// <param name="targetCatalogId">The identifier of the catalog to which the resultant product representatives must belong to.</param>
            /// <param name="attributeIdCollectionString">The attribute values to retrieve along with the result set.</param>
            /// <param name="settings">The settings to use while processing this request.</param>
            /// <returns>A collection of product search results representing products in the requested category or its sub-categories.</returns>
            internal ReadOnlyCollection<ProductSearchResult> SearchProductsByText(long currentChannelId, string searchText, long targetChannelId, long targetCatalogId, string attributeIdCollectionString, QueryResultSettings settings)
            {
                CP.RetailTransactionServiceResponse transactionServiceResponse = this.GetResponseFromMethod(
                    GetProductsByKeywordMethodName,
                    currentChannelId,
                    searchText,
                    settings.Paging.Skip + 1,  // 1-based in AX
                    settings.Paging.NumberOfRecordsToFetch,
                    settings.Sorting == null ? RecordIdAttributeName : settings.Sorting.ToString(),  // order by
                    settings.Sorting == null || !settings.Sorting.IsSpecified ? (settings.Sorting.Columns.First().IsDescending ? 1 : 0) : 0,  // sort order: Ascending by default
                    false,  // return total count
                    string.Empty,  // language
                    targetChannelId,
                    targetCatalogId,
                    attributeIdCollectionString);

                // Check result
                if (!transactionServiceResponse.Success)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure,
                        string.Format("Invoke method {0} failed: {1}", GetProductsByCategoryMethodName, transactionServiceResponse.Message));
                }

                // Throw if service response does not contain any data.
                if (transactionServiceResponse.Data == null || transactionServiceResponse.Data.Length == 0)
                {
                    throw new CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError, "Service response does not contain any data.");
                }

                string searchResultsXml = (string)transactionServiceResponse.Data[0];
                var results = this.GetResultsFromXml(searchResultsXml);

                return results;
            }

            private static Dictionary<string, string> BuildDataSourceMap(object[] list)
            {
                if (list == null || list.Length == 0)
                {
                    throw new ArgumentNullException("list", "The data source map must be specified.");
                }

                int startIndex = 4;
                int expectedLength = Convert.ToInt32(list[3]);
                int actualLength = (list.Length - startIndex) / 2;

                if (expectedLength != actualLength)
                {
                    string message = string.Format("The number of entries in the data source map was unexpected. Actual = {0}, Expected = {1}.", actualLength, expectedLength);
                    throw new InvalidOperationException(message);
                }

                var map = new Dictionary<string, string>(expectedLength);
                for (int i = startIndex; i < list.Length; i += 2)
                {
                    string key = (string)list[i];
                    string tableName = (string)list[i + 1];

                    map.Add(key, tableName);
                }

                return map;
            }

            private static XDocument UpdateTableNamesWithDataSourceMap(string productsXml, IDictionary<string, string> map)
            {
                // Remove the namespace from the XML document to allow us to parse it in SQL.
                System.Xml.XmlDocument dom = new System.Xml.XmlDocument();
                dom.XmlResolver = null;
                TextReader textReader = new StringReader(productsXml);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.XmlResolver = null;
                settings.DtdProcessing = DtdProcessing.Prohibit;
                XmlReader reader = XmlReader.Create(textReader, settings);
                dom.Load(reader);
                textReader = new StringReader(dom.OuterXml.Replace(dom.DocumentElement.NamespaceURI, string.Empty));
                reader = XmlReader.Create(textReader, settings);
                dom.Load(reader);
                dom.DocumentElement.RemoveAllAttributes();

                XDocument xmlDocument = XDocument.Parse(dom.OuterXml);
                if (xmlDocument != null)
                {
                    var elements = xmlDocument.Root.Descendants();
                    foreach (XElement element in elements)
                    {
                        string elementName = element.Name.LocalName;
                        if (map.ContainsKey(elementName))
                        {
                            string tableName = map[elementName];
                            element.Name = XName.Get(tableName, element.Name.NamespaceName);
                        }
                    }
                }

                return xmlDocument;
            }

            private XDocument GetProductData<T>(IEnumerable<T> ids, string rangeFieldName)
            {
                CP.RetailTransactionServiceResponse serviceResponse = this.GetResponseFromMethod(
                    GetProductDataMethodName, string.Join(",", ids), 1, rangeFieldName);

                // Check result
                if (!serviceResponse.Success)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterTransactionServiceMethodCallFailure,
                        string.Format("Invoke method {0} failed: {1}", GetProductDataMethodName, serviceResponse.Message));
                }

                // Throw if service response does not contain any data.
                if (serviceResponse.Data == null || serviceResponse.Data.Length == 0)
                {
                    throw new CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        "Service response does not contain any data.");
                }

                string productsXml = (string)serviceResponse.Data[0];
                object[] dataSource = (object[])serviceResponse.Data[1];
                var dictionary = BuildDataSourceMap(dataSource);

                return UpdateTableNamesWithDataSourceMap(productsXml, dictionary);
            }

            private ReadOnlyCollection<Product> ConvertProductsXmlToProducts(string productsXml)
            {
                List<Product> products = new List<Product>();

                XDocument xmlDocument = XDocument.Parse(productsXml);
                if (xmlDocument != null)
                {
                    string languageId = this.context.LanguageId;

                    var elements = xmlDocument.Root.Elements();
                    foreach (var element in elements)
                    {
                        Product product = new Product();
                        product.RecordId = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(element, "RecordId"));
                        product.ItemId = TransactionServiceClient.GetAttributeValue(element, "ItemId");
                        product.ProductNumber = product.ItemId;

                        // Set the IsRemote flag to true for each of the retrieved products.
                        product.IsRemote = true;

                        string isKit = TransactionServiceClient.GetAttributeValue(element, "IsKit");
                        if (!string.IsNullOrWhiteSpace(isKit))
                        {
                            product.IsKit = Convert.ToBoolean(isKit);
                        }

                        string isMasterProduct = TransactionServiceClient.GetAttributeValue(element, "IsMasterProduct");
                        if (!string.IsNullOrWhiteSpace(isMasterProduct))
                        {
                            product.IsMasterProduct = Convert.ToBoolean(isMasterProduct);
                        }

                        string price = TransactionServiceClient.GetAttributeValue(element, "Price");
                        if (!string.IsNullOrWhiteSpace(price))
                        {
                            product.Price = Convert.ToDecimal(price);
                        }

                        int i = 0;
                        var attributes = element.Elements("AttributeValues").Elements();
                        foreach (var attribute in attributes)
                        {
                            long attributeId = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(attribute, "Attribute"));
                            string textValue = TransactionServiceClient.GetAttributeValue(attribute, "TextValue");
                            ////string unitValue = TransactionServiceClient.GetAttributeValue(attribute, "Unit");

                            var getChannelProductAttributeByIdDataRequest = new GetChannelProductAttributeByIdDataRequest(attributeId, new ColumnSet());
                            var productAttribute = this.context.Runtime.Execute<SingleEntityDataServiceResponse<AttributeProduct>>(getChannelProductAttributeByIdDataRequest, this.context).Entity;
                            if (productAttribute == null)
                            {
                                continue;
                            }

                            ProductProperty property = new ProductProperty();
                            property.AttributeValueId = productAttribute.AttributeValueRecordId;
                            property.RecordId = productAttribute.RecordId;
                            property.PropertyType = (ProductPropertyType)productAttribute.DataType;
                            property.KeyName = productAttribute.KeyName;
                            property.Value = textValue;
                            ////property.UnitText = unitValue;

                            // Create the default product schema.
                            product.IndexedProductSchema.Add(productAttribute.KeyName, i++);
                            product.SetProperty(productAttribute.KeyName, property);

                            if (!product.IndexedProductProperties.ContainsKey(languageId))
                            {
                                product.IndexedProductProperties[languageId] = new ProductPropertyDictionary();
                            }

                            product.IndexedProductProperties[languageId].Add(productAttribute.KeyName, property);
                        }

                        ProductBuilder.CompleteProductInstantiation(product);
                        products.Add(product);
                    }
                }

                return products.AsReadOnly();
            }

            private ReadOnlyCollection<ProductSearchResult> GetResultsFromXml(string searchResultsXml)
            {
                List<ProductSearchResult> productSearchResults = new List<ProductSearchResult>();
                XDocument xmlDocument = XDocument.Parse(searchResultsXml);

                if (xmlDocument != null)
                {
                    var elements = xmlDocument.Root.Elements();

                    foreach (var element in elements)
                    {
                        ProductSearchResult searchResult = new ProductSearchResult();
                        searchResult.ItemId = TransactionServiceClient.GetAttributeValue(element, ItemIdAttributeName);
                        searchResult.Name = TransactionServiceClient.GetAttributeValue(element, NameAttributeName);
                        searchResult.RecordId = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(element, RecordIdAttributeName));
                        var attribute = element.Elements("AttributeValues").Elements().FirstOrDefault();
                        
                        if (attribute != null)
                        {
                            XDocument imageXml = XDocument.Parse(TransactionServiceClient.GetAttributeValue(attribute, "TextValue"));
                            XElement mediaLocation = imageXml.Root.Elements("RichMediaLocation").FirstOrDefault();
                            searchResult.PrimaryImageUrl = mediaLocation.Elements("Url").FirstOrDefault().Value;
                            searchResult.PrimaryImageUrl = searchResult.PrimaryImageUrl.Replace("{ProductNumber}", searchResult.ItemId);
                        }

                        if (!string.IsNullOrWhiteSpace(TransactionServiceClient.GetAttributeValue(element, PriceAttributeName)))
                        {
                            searchResult.Price = Convert.ToDecimal(TransactionServiceClient.GetAttributeValue(element, PriceAttributeName));
                        }

                        productSearchResults.Add(searchResult);
                    }
                }

                return productSearchResults.AsReadOnly();
            }
        }
    }
}