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
        internal sealed class SearchProductsByCategoryIdProcedure
        {
            // Variable names.
            private const string CatalogIdVariableName = "@bi_CatalogId";
            private const string CategoryIdVariableName = "@bi_CategoryId";
            private const string LocaleVariableName = "@nvc_Locale";
            private const string SkipVariableName = "@bi_Skip";
            private const string TopVariableName = "@bi_Top";
    
            // Request.
            private GetProductSearchResultsDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SearchProductsByCategoryIdProcedure"/> class.
            /// </summary>
            /// <param name="request">The data request.</param>
            public SearchProductsByCategoryIdProcedure(GetProductSearchResultsDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the stored procedure.
            /// </summary>
            /// <returns>The data response.</returns>
            public PagedResult<ProductSearchResult> Execute()
            {
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    string query = @"
                            DECLARE @tvp_SubCategoryIds [crt].[RECORDIDTABLETYPE]
    
                            ;WITH CategoryHierarchyTree (RECID)
                            AS
                            (
                                SELECT [cchv].RECID
                                FROM [crt].CHANNELCATEGORYHIERARCHYVIEW cchv
                                WHERE [cchv].RECID = @bi_CategoryId AND [cchv].CHANNELID = @bi_ChannelId
    
                                UNION ALL
    
                                SELECT [cchv].RECID
                                FROM [crt].CHANNELCATEGORYHIERARCHYVIEW cchv
                                INNER JOIN CategoryHierarchyTree ct ON [cchv].PARENTCATEGORY = [ct].RECID AND [cchv].CHANNELID = @bi_ChannelId
                            )
    
                            INSERT INTO @tvp_SubCategoryIds(RECID)
                            SELECT [ct].RECID FROM CategoryHierarchyTree ct
                            INNER JOIN [ax].ECORESCATEGORY erc ON [erc].RECID = [ct].RECID
    
                            SELECT
                                [par].PRODUCTID AS RECID,
                                [it].ITEMID AS ITEMID,
                                [erpt].NAME AS NAME,
                                CASE [par].PRODUCTID
                                    WHEN [ictap].PRODUCT THEN ([ictap].AMOUNT_SUM/[ictap].AMOUNT_COUNT)
                                    ELSE ([icbp].AMOUNT_SUM/[icbp].AMOUNT_COUNT)
                                END AS PRICE,
                                COALESCE([tvt].TRANSLATION, [ertv].TEXTVALUE) AS PRIMARYIMAGE
                            FROM [crt].PRODUCTASSORTMENTRULES par
                            INNER JOIN [crt].PRODUCTCATEGORYRULES pcr ON [par].CHANNELID = [pcr].CHANNELID AND [pcr].CATALOGID = @bi_CatalogId AND [par].PRODUCTID = [pcr].PRODUCTID AND [par].VARIANTID = 0 AND @dt_ChannelDate BETWEEN COALESCE([pcr].VALIDFROM, DATEADD(d, -10, GETDATE())) AND COALESCE([pcr].VALIDTO, DATEADD(d, 10, GETDATE()))
                            INNER JOIN @tvp_SubCategoryIds cids ON [cids].RECID = [pcr].CATEGORYID
                            INNER JOIN [ax].INVENTTABLE it ON [it].PRODUCT = [par].PRODUCTID
                            INNER JOIN [ax].ECORESPRODUCTTRANSLATION erpt ON [erpt].PRODUCT = [par].PRODUCTID AND [erpt].LANGUAGEID = @nvc_Locale
                            LEFT OUTER JOIN [crt].DEFAULTPRODUCTATTRIBUTEGROUPDEFAULTVALUEVIEW gpavv ON [gpavv].CHANNEL = @bi_ChannelId
                            LEFT OUTER JOIN [ax].ECORESTEXTVALUE ertv ON [ertv].RECID = [gpavv].VALUE
                            LEFT OUTER JOIN [crt].GETTEXTVALUETRANSLATION(@bi_ChannelId) tvt ON [tvt].VALUE = [ertv].RECID AND [tvt].LANGUAGE = @nvc_Locale
                            LEFT OUTER JOIN [crt].ITEMCHANNELTRADEAGREEMENTPRICEVIEW ictap WITH (NOEXPAND) ON [ictap].CHANNEL = [par].CHANNELID AND [ictap].PRODUCT = [par].PRODUCTID AND @dt_ChannelDate BETWEEN [ictap].FROMDATE AND [ictap].TODATE
                            LEFT OUTER JOIN [crt].ITEMCHANNELBASEPRICEVIEW icbp WITH (NOEXPAND) ON [icbp].CHANNEL = [par].CHANNELID AND [icbp].PRODUCT = [par].PRODUCTID
                            WHERE [par].CHANNELID = @bi_ChannelId AND @dt_ChannelDate BETWEEN [par].VALIDFROM AND [par].VALIDTO
                                AND (
                                    @bi_CatalogId = 0 OR
                                    EXISTS
                                    (
                                        SELECT 1 FROM [crt].PRODUCTCATALOGRULES pcr
                                        WHERE [pcr].CHANNELID = [par].CHANNELID AND [pcr].CATALOGID = @bi_CatalogId AND [pcr].PRODUCTID = [par].PRODUCTID AND @dt_ChannelDate BETWEEN [pcr].VALIDFROM AND [pcr].VALIDTO
                                    )
                                )
                            ORDER BY [erpt].NAME
                            OFFSET (SELECT @bi_Skip FROM @tvp_QueryResultSettings) ROWS
                            FETCH NEXT (SELECT @bi_Top FROM @tvp_QueryResultSettings) ROWS ONLY";
    
                    long currentChannelId = this.request.RequestContext.GetPrincipal().ChannelId;
                    SqlQuery sqlQuery = new SqlQuery(query);
                    sqlQuery.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    sqlQuery.Parameters[CatalogIdVariableName] = this.request.CatalogId;
                    sqlQuery.Parameters[LocaleVariableName] = this.request.RequestContext.LanguageId;
                    sqlQuery.Parameters[DatabaseAccessor.ChannelDateVariableName] = this.request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    sqlQuery.Parameters[CategoryIdVariableName] = this.request.CategoryId;
                    sqlQuery.Parameters[SkipVariableName] = this.request.QueryResultSettings.Paging.Skip;
                    sqlQuery.Parameters[TopVariableName] = this.request.QueryResultSettings.Paging.NumberOfRecordsToFetch;
    
                    return new PagedResult<ProductSearchResult>(databaseContext.ReadEntity<ProductSearchResult>(sqlQuery).Results, this.request.QueryResultSettings.Paging);
                }
            }
        }
    }
}
