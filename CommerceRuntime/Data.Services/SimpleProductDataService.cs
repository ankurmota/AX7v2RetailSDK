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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Product data service class.
        /// </summary>
        public sealed class SimpleProductDataService : IRequestHandler
        {
            private const string ProductIdsVariableName = "@tvp_ProductIds";
            private const string ProductColumnName = "PRODUCT";
            private const string ProductIdVariableName = "PRODUCTID";
            private const string ProductDimensionsViewName = "PRODUCTDIMENSIONSVIEW";
            private const string MediaAttributesViewName = "MEDIAATTRIBUTESVIEW";
            private const string ProductIdsToUnitsOfMeasureView = "PRODUCTIDSTOUNITSOFMEASUREVIEW";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetProductMediaAttributeSchemaEntriesDataRequest),
                        typeof(GetProductDimensionsDataRequest),
                        typeof(GetUnitsOfMeasureOfProductsDataRequest)
                    };
                }
            }

            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                Response response;

                if (request is GetProductMediaAttributeSchemaEntriesDataRequest)
                {
                    response = GetProductMediaAttributeSchemaEntries((GetProductMediaAttributeSchemaEntriesDataRequest)request);
                }
                else if (request is GetProductDimensionsDataRequest)
                {
                    response = GetProductDimensions((GetProductDimensionsDataRequest)request);
                }
                else if (request is GetUnitsOfMeasureOfProductsDataRequest)
                {
                    response = GetUnitsOfMeasureOfProducts((GetUnitsOfMeasureOfProductsDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }

                return response;
            }

            private static EntityDataServiceResponse<ProductDimension> GetProductDimensions(GetProductDimensionsDataRequest request)
            {
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Distinct = true,  // Need to perform distinct to avoid retrieving a row for every variant type product combination.
                    Aliased = true,
                    From = ProductDimensionsViewName
                };

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                using (RecordIdTableType productIds = new RecordIdTableType(request.ProductIds, ProductIdVariableName))
                {
                    query.Parameters[ProductIdsVariableName] = productIds;

                    return new EntityDataServiceResponse<ProductDimension>(databaseContext.ReadEntity<ProductDimension>(query));
                }
            }

            private static EntityDataServiceResponse<ProductAttributeSchemaEntry> GetProductMediaAttributeSchemaEntries(GetProductMediaAttributeSchemaEntriesDataRequest request)
            {
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = MediaAttributesViewName
                };

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    return new EntityDataServiceResponse<ProductAttributeSchemaEntry>(databaseContext.ReadEntity<ProductAttributeSchemaEntry>(query));
                }
            }

            private static EntityDataServiceResponse<UnitOfMeasure> GetUnitsOfMeasureOfProducts(GetUnitsOfMeasureOfProductsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Select = new ColumnSet(new string[] { "RECID", "SYMBOL", "DECIMALPRECISION", "DESCRIPTION" }),
                    Aliased = true,
                    From = ProductIdsToUnitsOfMeasureView
                };

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                using (RecordIdTableType productIds = new RecordIdTableType(request.ProductIds, ProductColumnName))
                {
                    query.Parameters[ProductIdsVariableName] = productIds;
                    query.Where = string.Format("DATAAREAID = '{0}' AND (LANGUAGEID IS NULL OR LANGUAGEID = '{1}')", request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId, request.RequestContext.LanguageId);

                    return new EntityDataServiceResponse<UnitOfMeasure>(databaseContext.ReadEntity<UnitOfMeasure>(query));
                }
            }
        }
    }
}
