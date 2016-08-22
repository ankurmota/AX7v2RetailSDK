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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Procedure class to get products by their respective product identifiers.
        /// </summary>
        internal sealed class GetProductsByIdsProcedure
        {
            // Variable names.
            private const string IsRemoteVariableName = "@i_IsRemote";
            private const string LocaleVariableName = "@nvc_Locale";
            private const string ProductIdsVariableName = "@tvp_ProductIds";
    
            // Request.
            private GetProductsDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProductsByIdsProcedure"/> class.
            /// </summary>
            /// <param name="request">The data request.</param>
            public GetProductsByIdsProcedure(GetProductsDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the stored procedure.
            /// </summary>
            /// <returns>The data response.</returns>
            public PagedResult<Product> Execute()
            {
                if (!this.request.ProductIds.Any())
                {
                    return new PagedResult<Product>(new List<Product>().AsReadOnly());
                }
    
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                using (RecordIdTableType type = new RecordIdTableType(this.request.ProductIds, ProductIdsVariableName))
                {
                    string query = @"
                        -- Retrieve variant id = 0 products.
                        SELECT DISTINCT
                            [itm].UNITID AS DEFAULTUNITOFMEASURE,
                            [erpt].[DESCRIPTION] AS [DESCRIPTION],
                            [it].ITEMID AS ITEMID,
                            [erpt].NAME AS NAME,
                            CASE
                                WHEN [pv].RECID IS NOT NULL THEN [par].VARIANTID
                                ELSE [par].PRODUCTID
                            END AS RECID,
                            CASE
                                WHEN [par].VARIANTID = 0 AND [rk].RECID IS NOT NULL THEN 1  -- Kit Master
                                WHEN [rk].RECID IS NOT NULL AND [pv].RECID IS NOT NULL THEN 2  -- Kit Variant
                                WHEN [pv].RECID IS NULL AND [pv2].RECID IS NOT NULL THEN 3  -- Master
                                WHEN [pv].RECID IS NOT NULL THEN 5  -- Variant
                                ELSE 4  -- Standalone
                            END AS PRODUCTTYPE
                        FROM @tvp_ProductIds ids
                        INNER JOIN [crt].PRODUCTASSORTMENTRULES par ON [par].CHANNELID = @bi_ChannelId AND [par].PRODUCTID = [ids].RECID AND [par].VARIANTID = 0 AND [par].ISREMOTE = @i_IsRemote AND @dt_ChannelDate BETWEEN [par].VALIDFROM AND [par].VALIDTO
                        INNER JOIN [ax].INVENTTABLE it ON [it].PRODUCT = [par].PRODUCTID
                        INNER JOIN [ax].INVENTTABLEMODULE itm ON [itm].ITEMID = [it].ITEMID AND [itm].DATAAREAID = [it].DATAAREAID AND [itm].MODULETYPE = 2  -- Sales
                        INNER JOIN [ax].ECORESPRODUCTTRANSLATION erpt ON [erpt].PRODUCT = [par].PRODUCTID AND [erpt].LANGUAGEID = @nvc_Locale
                        LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT pv ON [pv].RECID = [par].VARIANTID
                        LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT pv2 ON [pv2].PRODUCTMASTER = [par].PRODUCTID
                        LEFT OUTER JOIN [ax].RETAILKIT rk ON [rk].PRODUCTMASTER = [par].PRODUCTID
    
                        UNION ALL
    
                        -- Retrieve variant id != 0 products.
                        SELECT DISTINCT
                            [itm].UNITID AS DEFAULTUNITOFMEASURE,
                            [erpt].[DESCRIPTION] AS [DESCRIPTION],
                            [it].ITEMID AS ITEMID,
                            [erpt].NAME AS NAME,
                            CASE
                                WHEN [pv].RECID IS NOT NULL THEN [par].VARIANTID
                                ELSE [par].PRODUCTID
                            END AS RECID,
                            CASE
                                WHEN [par].VARIANTID = 0 AND [rk].RECID IS NOT NULL THEN 1  -- Kit Master
                                WHEN [rk].RECID IS NOT NULL AND [pv].RECID IS NOT NULL THEN 2  -- Kit Variant
                                WHEN [pv].RECID IS NULL AND [pv2].RECID IS NOT NULL THEN 3  -- Master
                                WHEN [pv].RECID IS NOT NULL THEN 5  -- Variant
                                ELSE 4  -- Standalone
                            END AS PRODUCTTYPE
                        FROM @tvp_ProductIds ids
                        INNER JOIN [crt].PRODUCTASSORTMENTRULES par ON [par].CHANNELID = @bi_ChannelId AND [par].VARIANTID = [ids].RECID AND [par].ISREMOTE = @i_IsRemote AND @dt_ChannelDate BETWEEN [par].VALIDFROM AND [par].VALIDTO
                        INNER JOIN [ax].INVENTTABLE it ON [it].PRODUCT = [par].PRODUCTID
                        INNER JOIN [ax].INVENTTABLEMODULE itm ON [itm].ITEMID = [it].ITEMID AND [itm].DATAAREAID = [it].DATAAREAID AND [itm].MODULETYPE = 2  -- Sales
                        INNER JOIN [ax].ECORESPRODUCTTRANSLATION erpt ON [erpt].PRODUCT = [par].PRODUCTID AND [erpt].LANGUAGEID = @nvc_Locale
                        LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT pv ON [pv].RECID = [par].VARIANTID
                        LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT pv2 ON [pv2].PRODUCTMASTER = [par].PRODUCTID
                        LEFT OUTER JOIN [ax].RETAILKIT rk ON [rk].PRODUCTMASTER = [par].PRODUCTID";
    
                    long currentChannelId = this.request.RequestContext.GetPrincipal().ChannelId;
                    SqlQuery sqlQuery = new SqlQuery(query, type.DataTable);
                    sqlQuery.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    sqlQuery.Parameters[DatabaseAccessor.ChannelDateVariableName] = this.request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    sqlQuery.Parameters[LocaleVariableName] = this.request.RequestContext.LanguageId;
                    sqlQuery.Parameters[ProductIdsVariableName] = type.DataTable;

                    if (this.request.DownloadedProductsFilter.HasValue)
                    {
                        if (this.request.DownloadedProductsFilter.Value)
                        {
                            sqlQuery.Parameters[IsRemoteVariableName] = 1;
                        }
                        else
                        {
                            sqlQuery.Parameters[IsRemoteVariableName] = 0;
                        }
                    }
    
                    return databaseContext.ReadEntity<Product>(sqlQuery);
                }
            }
        }
    }
}
