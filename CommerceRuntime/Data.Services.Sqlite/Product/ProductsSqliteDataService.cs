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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Product;

        /// <summary>
        /// Product SQLite data service class.
        /// </summary>
        public sealed class ProductsSqliteDataService : IRequestHandler
        {
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(GetProductsDataRequest),
                typeof(GetProductBehaviorDataRequest)
            };
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return SupportedRequestTypesArray; }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
    
                if (request is GetProductsDataRequest)
                {
                    response = ProcessGetProductsDataRequest((GetProductsDataRequest)request);
                }
                else if (request is GetProductBehaviorDataRequest)
                {
                    response = ProcessGetProductBehaviorDataRequest((GetProductBehaviorDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static EntityDataServiceResponse<Microsoft.Dynamics.Commerce.Runtime.DataModel.Product> ProcessGetProductsDataRequest(GetProductsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.QueryResultSettings.ColumnSet == null || request.QueryResultSettings.ColumnSet.Count <= 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.QueryResultSettings.ColumnSet, "Columnset cannot be null or empty for this data request.");
                }
    
                if (request.ProductIds.IsNullOrEmpty() && request.ItemAndInventDimIdCombinations.IsNullOrEmpty())
                {
                    throw new ArgumentOutOfRangeException("request", "The GetProductsDataRequest cannot be processed when both product ids and item-inventdim ids are specified. Please specify only one.");
                }
    
                if (request.ProductIds.IsNullOrEmpty() && request.ItemAndInventDimIdCombinations.IsNullOrEmpty())
                {
                    return new EntityDataServiceResponse<Microsoft.Dynamics.Commerce.Runtime.DataModel.Product>(new PagedResult<Microsoft.Dynamics.Commerce.Runtime.DataModel.Product>(new List<Microsoft.Dynamics.Commerce.Runtime.DataModel.Product>().AsReadOnly()));
                }
    
                PagedResult<Microsoft.Dynamics.Commerce.Runtime.DataModel.Product> results = null;
    
                if (!request.ProductIds.IsNullOrEmpty())
                {
                    var getProductsByIdsProcedure = new GetProductsByIdsProcedure(request);
                    results = getProductsByIdsProcedure.Execute();
                }
                else if (!request.ItemAndInventDimIdCombinations.IsNullOrEmpty())
                {
                    throw new NotImplementedException("Coming soon!");
                }
    
                return new EntityDataServiceResponse<Microsoft.Dynamics.Commerce.Runtime.DataModel.Product>(results);
            }
    
            private static EntityDataServiceResponse<ProductBehavior> ProcessGetProductBehaviorDataRequest(GetProductBehaviorDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.QueryResultSettings.ColumnSet == null || request.QueryResultSettings.ColumnSet.Count <= 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.QueryResultSettings.ColumnSet, "Columnset cannot be null or empty for this data request.");
                }
    
                var getProductBehaviorByProductIdsProcedure = new GetProductBehaviorByProductIdsProcedure(request);
                var results = getProductBehaviorByProductIdsProcedure.Execute();
                return new EntityDataServiceResponse<ProductBehavior>(results);
            }
        }
    }
}
