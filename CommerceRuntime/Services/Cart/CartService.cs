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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Cart Service class.
        /// </summary>
        public class CartService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetSalesTransactionsServiceRequest)
                    };
                }
            }

            /// <summary>
            /// Executes the service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The response.
            /// </returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestedType = request.GetType();

                if (requestedType == typeof(GetSalesTransactionsServiceRequest))
                {
                    return GetSalesTransactions((GetSalesTransactionsServiceRequest)request);
                }

                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }

            /// <summary>
            /// Load sales transactions using the search criteria.
            /// </summary>
            /// <param name="request">Request containing the criteria used to retrieve sales transactions.</param>
            /// <returns>Instance of <see cref="GetSalesTransactionsServiceResponse"/>.</returns>
            private static GetSalesTransactionsServiceResponse GetSalesTransactions(GetSalesTransactionsServiceRequest request)
            {
                ThrowIf.Null(request, "request");

                IDictionary<string, IList<SalesLine>> linesWithUnavailableProducts;

                // Try to load the transaction
                GetCartsDataRequest getSalesTransactionDatasetDataRequest = new GetCartsDataRequest(request.SearchCriteria, request.QueryResultSettings);
                PagedResult<SalesTransaction> salesTransactions = request.RequestContext.Execute<EntityDataServiceResponse<SalesTransaction>>(getSalesTransactionDatasetDataRequest).PagedEntityCollection;

                if (salesTransactions != null)
                {
                    // Only search remote in case of customer order.
                    SearchLocation productSearchLocation = salesTransactions.Results.Any(t => t.CartType == CartType.CustomerOrder) ? SearchLocation.All : SearchLocation.Local;

                    linesWithUnavailableProducts = PopulateProductOnSalesLines(request.RequestContext, salesTransactions.Results, request.MustRemoveUnavailableProductLines, productSearchLocation);
                }
                else
                {
                    salesTransactions = PagedResult<SalesTransaction>.Empty();
                    linesWithUnavailableProducts = new Dictionary<string, IList<SalesLine>>(); 
                }

                GetSalesTransactionsServiceResponse response = new GetSalesTransactionsServiceResponse(salesTransactions, linesWithUnavailableProducts);
                return response;
            }

            /// <summary>
            /// Populates the product information on the sales lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransactions">The list of sales transactions.</param>
            /// <param name="mustRemoveUnavailableProductLines">A flag indicating whether to remove cart lines with unavailable products.</param>
            /// <param name="productsSearchLocation">Products search location.</param>
            /// <returns>Collection of unavailable product identifiers.</returns>
            private static IDictionary<string, IList<SalesLine>> PopulateProductOnSalesLines(RequestContext context, IEnumerable<SalesTransaction> salesTransactions, bool mustRemoveUnavailableProductLines, SearchLocation productsSearchLocation)
            {
                var productIdToLineAndTransactionMapping = new Dictionary<long, List<Tuple<SalesLine, SalesTransaction>>>();
                var linesWithUnavilableProducts = new Dictionary<string, IList<SalesLine>>();

                foreach (SalesTransaction transaction in salesTransactions)
                {
                    IEnumerable<SalesLine> productSalesLines = transaction.SalesLines.Where(line => line.ProductId != 0);
                    foreach (SalesLine line in productSalesLines)
                    {
                        long productId = line.ProductId;
                        if (!productIdToLineAndTransactionMapping.ContainsKey(productId))
                        {
                            productIdToLineAndTransactionMapping.Add(productId, new List<Tuple<SalesLine, SalesTransaction>>());
                        }

                        productIdToLineAndTransactionMapping[productId].Add(new Tuple<SalesLine, SalesTransaction>(line, transaction));
                    }
                }

                if (productIdToLineAndTransactionMapping.IsNullOrEmpty())
                {
                    return linesWithUnavilableProducts;
                }

                // Get referenced productIds for CartLines to be created
                var productIds = productIdToLineAndTransactionMapping.Keys.ToList();
                var request = new GetProductsServiceRequest(context.GetPrincipal().ChannelId, productIds, calculatePrice: false, settings: QueryResultSettings.AllRecords)
                {
                    SearchLocation = productsSearchLocation
                };

                var results = context.Runtime.Execute<GetProductsServiceResponse>(request, context).Products.Results.OrderBy(p => p.IsRemote);

                var productByRecordId = new Dictionary<long, SimpleProduct>();
                foreach (var product in results)
                {
                    if (!productByRecordId.ContainsKey(product.RecordId))
                    {
                        productByRecordId[product.RecordId] = product;
                    }
                }

                foreach (KeyValuePair<long, List<Tuple<SalesLine, SalesTransaction>>> mapping in productIdToLineAndTransactionMapping)
                {
                    SimpleProduct product;
                    bool productFound = productByRecordId.TryGetValue(mapping.Key, out product);

                    foreach (Tuple<SalesLine, SalesTransaction> lineTransactionPair in mapping.Value)
                    {
                        SalesLine salesLine = lineTransactionPair.Item1;
                        SalesTransaction transaction = lineTransactionPair.Item2;

                        if (!productFound)
                        {
                            if (mustRemoveUnavailableProductLines)
                            {
                                transaction.SalesLines.Remove(salesLine);
                            }
                            else
                            {
                                salesLine.Variant = null;
                            }

                            if (!linesWithUnavilableProducts.ContainsKey(transaction.Id))
                            {
                                linesWithUnavilableProducts.Add(transaction.Id, new List<SalesLine>());
                            }

                            linesWithUnavilableProducts[transaction.Id].Add(salesLine);
                            continue;
                        }

                        if (product.IsDistinct)
                        {
                            salesLine.Variant = ProductVariant.ConvertFrom(product);
                        }
                    }
                }

                return linesWithUnavilableProducts;
            }
        }
    }
}