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
    namespace Commerce.Runtime.DataServices.Sqlite.Product
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class for product search.
        /// </summary>
        internal sealed class SearchProductsProcedure
        {
            private ProductSearchDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SearchProductsProcedure"/> class.
            /// </summary>
            /// <param name="request">The product search request.</param>
            public SearchProductsProcedure(ProductSearchDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the database operation for product search.
            /// </summary>
            /// <returns>Returns the product search result.</returns>
            public ProductSearchDataResponse Execute()
            {
                ProductSearchCriteria queryCriteria = this.request.QueryCriteria;
                ICollection<long> productIds;
    
                if (queryCriteria.Context.CatalogId != 0)
                {
                    new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_RequestParametersNotSupported, "Search products using catalog is not supported for SQLite.");
                }
    
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    long channelId = queryCriteria.Context.ChannelId.GetValueOrDefault(databaseContext.ChannelId);
    
                    if (!string.IsNullOrWhiteSpace(queryCriteria.SearchCondition))
                    {
                        productIds = this.SearchByKeyword(databaseContext, queryCriteria.SearchCondition, channelId);
                    }
                    else if (queryCriteria.ItemIds.Any())
                    {
                        productIds = this.SearchByItemId(databaseContext, queryCriteria.ItemIds);
                    }
                    else if (queryCriteria.CategoryIds.Any())
                    {
                        productIds = this.SearchByCategoryId(databaseContext, queryCriteria.CategoryIds, channelId);
                    }
                    else
                    {
                        throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_RequestParametersNotSupported, "Supported product search conditions for SQLite are: SearchCondition, ItemIds, CategoryIds.");
                    }
                }
    
                return new ProductSearchDataResponse(productIds);
            }
    
            private ICollection<long> SearchByCategoryId(SqliteDatabaseContext databaseContext, IList<long> categoryIds, long channelId)
            {
                GetProductsByCategoryProcedure getProductsByCategoryProcedure = new GetProductsByCategoryProcedure(databaseContext, categoryIds, channelId);
                return getProductsByCategoryProcedure.Execute();
            }
    
            private ICollection<long> SearchByKeyword(SqliteDatabaseContext context, string keyword, long channelId)
            {
                const string Query = @"
                    SELECT p.RECID
                    FROM [ax].ECORESPRODUCT p
                    INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
                    INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = p.RECID AND it.ITEMID LIKE @nvc_SearchCondition AND IT.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    
                    UNION
    
                    SELECT EP.RECID
                    FROM [ax].ECORESPRODUCT EP
                    JOIN [ax].RETAILSTANDARDATTRIBUTE RSA ON RSA.STANDARDATTRIBUTEID = 1  -- ProductNumber
                    JOIN [crt].PUBPRODUCTATTRIBUTECHANNELMETADATAVIEW m ON m.ACTUALATTRIBUTE = rsa.ATTRIBUTE AND m.CHANNEL = @bi_ChannelId
                    WHERE ep.DISPLAYPRODUCTNUMBER LIKE @nvc_SearchCondition
    
                    UNION
    
                    SELECT EP.RECID
                    FROM [ax].ECORESPRODUCT EP
                    JOIN [ax].RETAILSTANDARDATTRIBUTE RSA ON RSA.STANDARDATTRIBUTEID IN (2, 4)  -- ProductName, ProductDescription
                    JOIN [crt].PUBPRODUCTATTRIBUTECHANNELMETADATAVIEW m ON m.ACTUALATTRIBUTE = rsa.ATTRIBUTE AND m.CHANNEL = @bi_ChannelId
                    JOIN [ax].ECORESPRODUCTTRANSLATION erpt ON erpt.PRODUCT = ep.RECID
                    WHERE erpt.LANGUAGEID = @nvc_LanguageId AND (erpt.NAME LIKE @nvc_SearchCondition OR erpt.DESCRIPTION LIKE @nvc_SearchCondition)
    
                    UNION
    
                    SELECT EP.RECID
                    FROM [ax].ECORESPRODUCT EP
                    JOIN [ax].RETAILSTANDARDATTRIBUTE RSA ON RSA.STANDARDATTRIBUTEID = 3  -- SearchName
                    JOIN [crt].PUBPRODUCTATTRIBUTECHANNELMETADATAVIEW m ON m.ACTUALATTRIBUTE = rsa.ATTRIBUTE AND m.CHANNEL = @bi_ChannelId
                    WHERE EP.SEARCHNAME LIKE @nvc_SearchCondition
    
                    UNION
    
                    -- color
                    SELECT EPVDV.DISTINCTPRODUCTVARIANT AS RECID
                    FROM [ax].ECORESPRODUCTVARIANTCOLOR EPVC
                    JOIN [ax].ECORESPRODUCTVARIANTDIMENSIONVALUE EPVDV ON EPVDV.RECID = EPVC.RECID
                    JOIN [ax].ECORESCOLOR EC ON EC.RECID = EPVC.COLOR AND EC.NAME LIKE @nvc_SearchCondition
                    JOIN [ax].RETAILSTANDARDATTRIBUTE RSA ON RSA.STANDARDATTRIBUTEID = 5  -- Color
                    JOIN [crt].PUBPRODUCTATTRIBUTECHANNELMETADATAVIEW m ON m.ACTUALATTRIBUTE = rsa.ATTRIBUTE AND m.CHANNEL = @bi_ChannelId
    
                    UNION
    
                    -- style
                    SELECT EPVDV.DISTINCTPRODUCTVARIANT AS RECID
                    FROM [ax].ECORESPRODUCTVARIANTSTYLE EPVS
                    JOIN [ax].ECORESPRODUCTVARIANTDIMENSIONVALUE EPVDV ON EPVDV.RECID = EPVS.RECID
                    JOIN [ax].ECORESSTYLE EC ON EC.RECID = EPVS.STYLE AND EC.NAME LIKE @nvc_SearchCondition
                    JOIN [ax].RETAILSTANDARDATTRIBUTE RSA ON RSA.STANDARDATTRIBUTEID = 6  -- Style
                    JOIN [crt].PUBPRODUCTATTRIBUTECHANNELMETADATAVIEW m ON m.ACTUALATTRIBUTE = rsa.ATTRIBUTE AND m.CHANNEL = @bi_ChannelId
    
                    UNION
    
                    -- size
                    SELECT EPVDV.DISTINCTPRODUCTVARIANT AS RECID
                    FROM [ax].ECORESPRODUCTVARIANTSIZE EPVS
                    JOIN [ax].ECORESPRODUCTVARIANTDIMENSIONVALUE EPVDV ON EPVDV.RECID = EPVS.RECID
                    JOIN [ax].ECORESSIZE EC ON EC.RECID = EPVS.SIZE AND EC.NAME LIKE @nvc_SearchCondition
                    JOIN [ax].RETAILSTANDARDATTRIBUTE RSA ON RSA.STANDARDATTRIBUTEID = 7  -- Size
                    JOIN [crt].PUBPRODUCTATTRIBUTECHANNELMETADATAVIEW m ON m.ACTUALATTRIBUTE = rsa.ATTRIBUTE AND m.CHANNEL = @bi_ChannelId
    
                    UNION
    
                    -- configuration
                    SELECT EPVDV.DISTINCTPRODUCTVARIANT AS RECID
                    FROM [ax].ECORESPRODUCTVARIANTCONFIGURATION EPVC
                    JOIN [ax].ECORESPRODUCTVARIANTDIMENSIONVALUE EPVDV ON EPVDV.RECID = EPVC.RECID
                    JOIN [ax].ECORESCONFIGURATION EC ON EC.RECID = EPVC.CONFIGURATION AND EC.NAME LIKE @nvc_SearchCondition
                    JOIN [ax].RETAILSTANDARDATTRIBUTE RSA ON RSA.STANDARDATTRIBUTEID = 8  -- Configuration
                    JOIN [crt].PUBPRODUCTATTRIBUTECHANNELMETADATAVIEW m ON m.ACTUALATTRIBUTE = rsa.ATTRIBUTE AND m.CHANNEL = @bi_ChannelId
    ";
    
                SqlQuery sqlQuery = new SqlQuery(Query);
                sqlQuery.Parameters["@bi_ChannelId"] = channelId;
                sqlQuery.Parameters["@nvc_SearchCondition"] = "%" + keyword + "%";
                sqlQuery.Parameters["@nvc_LanguageId"] = this.request.RequestContext.GetChannelConfiguration().DefaultLanguageId;
    
                return context.ExecuteScalarCollection<long>(sqlQuery);
            }
    
            private ICollection<long> SearchByItemId(SqliteDatabaseContext context, IEnumerable<ProductLookupClause> itemIdLookupCollection)
            {
                const string Query = @"
                    -- Retrieve product identifiers for item identifiers.
                    SELECT it.PRODUCT AS RECID
                    FROM {0} ids
                    INNER JOIN [ax].INVENTTABLE it ON it.ITEMID = ids.ITEMID AND it.DATAAREAID = @nvc_DataAreaId
                    WHERE ids.INVENTDIMID = ''
    
                    UNION ALL
    
                    -- Retrieve variant identifiers for inventory dimensions.
                    SELECT idc.DISTINCTPRODUCTVARIANT AS RECID
                    FROM {0} ids
                    INNER JOIN [ax].INVENTDIMCOMBINATION idc ON idc.ITEMID = ids.ITEMID AND idc.INVENTDIMID = ids.INVENTDIMID AND idc.DATAAREAID = @nvc_DataAreaId
                    WHERE ids.INVENTDIMID != ''
                ";
    
                using (var itemIdTable = new ItemIdSearchTableType(itemIdLookupCollection))
                using (var itemIdTempTable = context.CreateTemporaryTable(itemIdTable.DataTable))
                {
                    var sqlQuery = new SqlQuery(Query, itemIdTempTable.TableName);
                    sqlQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
                    return context.ExecuteScalarCollection<long>(sqlQuery);
                }
            }
        }
    }
}
