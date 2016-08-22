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
        using System.Text;
        using System.Threading.Tasks;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
    
        /// <summary>
        /// Helper class for getting products categories.
        /// </summary>
        internal class GetProductsByCategoryProcedure
        {
            private const string QueryCommand = @"
    		    WITH GETCATALOGPRODUCTCATEGORY AS
    		    (
    			    SELECT
    				    rpcp.CATALOG,
    				    rpcp.PRODUCT,
    				    rpcpc.CATEGORY,
    				    rpcpc.CATEGORYHIERARCHY,
    				    rpcpc.INCLUDEEXCLUDETYPE
    			    FROM [ax].RETAILPUBCATALOGPRODUCT rpcp
    			    JOIN [ax].RETAILPUBCATALOGPRODUCTCATEGORY rpcpc ON rpcp.ORIGIN = rpcpc.CATALOGPRODUCT
    		    ),
    
    		    GETPUBCATALOGATTRIBUTEINHERITED AS
    		    (
    			    SELECT
    				    rpc.ORIGIN AS [CATALOG]
    			    FROM [ax].RETAILPUBCATALOG rpc
    			    INNER JOIN [crt].PUBCATALOGCHANNELATTRIBUTEINHERITEDVIEW pccaiv ON pccaiv.[CATALOG] = rpc.ORIGIN AND pccaiv.CHANNEL = @bi_ChannelId
    			    WHERE @dt_ChannelDate BETWEEN rpc.PUBLISHEDVALIDFROM AND rpc.PUBLISHEDVALIDTO
    		    ),
    
    		    GETCATALOGCHANNELPRODUCTCATEGORY AS
    		    (
    			    SELECT
    				    cpc.CATALOG,
    				    cpc.PRODUCT,
    				    cpc.CATEGORY,
    				    cpc.INCLUDEEXCLUDETYPE
    			    FROM GETCATALOGPRODUCTCATEGORY cpc
    				    JOIN GETPUBCATALOGATTRIBUTEINHERITED rpc ON rpc.[CATALOG] = cpc.[CATALOG]
    				    JOIN [crt].PUBCATALOGCHANNELVIEW pccv ON pccv.[CATALOG] = rpc.[CATALOG]
    				    JOIN [ax].RETAILPUBRETAILCHANNELTABLE rprct ON rprct.ORIGINID = pccv.[CHANNEL]
    			    WHERE rprct.ORIGINID = @bi_ChannelId AND rprct.CATEGORYHIERARCHY = cpc.CATEGORYHIERARCHY
    		    ),
    
    		    GETNONCATALOGPRODUCTCATEGORY AS
    		    (
    			    SELECT
    				    erpc.PRODUCT,
    				    erpc.CATEGORY,
    				    erpc.CATEGORYHIERARCHY
    			    FROM [ax].ECORESPRODUCTCATEGORY AS erpc
    			    JOIN [ax].RETAILPUBECORESCATEGORY AS rperc ON rperc.CHANNEL = @bi_ChannelId AND rperc.ORIGINID = erpc.CATEGORY
    		
    			    UNION ALL
    
    			    SELECT
    				    erdpv.RECID AS PRODUCT,
    				    erpc.CATEGORY,
    				    erpc.CATEGORYHIERARCHY
    			    FROM [ax].ECORESDISTINCTPRODUCTVARIANT AS erdpv
    			    JOIN [ax].ECORESPRODUCTCATEGORY AS erpc ON erpc.PRODUCT = erdpv.PRODUCTMASTER
    			    JOIN [ax].RETAILPUBECORESCATEGORY AS rperc ON rperc.CHANNEL = @bi_ChannelId AND rperc.ORIGINID = ERPC.CATEGORY
    		    ),
    
    		    GETPRODUCTCATEGORIES AS
    		    (
    			    -- CatalogId = 0: Shared categories
    			    SELECT
    				    ncpc.PRODUCT,
    				    ncpc.CATEGORY,
    				    0 AS [CATALOG]
    			    FROM GETNONCATALOGPRODUCTCATEGORY ncpc		
    			    JOIN [ax].RETAILPUBRETAILCHANNELTABLE rprct ON rprct.ORIGINID = @bi_ChannelId
    			    WHERE
                        rprct.CATEGORYHIERARCHY = ncpc.CATEGORYHIERARCHY			
                        AND EXISTS
                        (
                            SELECT * FROM {0} categoryIds WHERE categoryIds.RECID = ncpc.CATEGORY
                        )
    
    			    UNION ALL
    
    			    -- CatalogId = 0: Catalog specific categories
    			    SELECT
    				    ccpc.PRODUCT,
    				    ccpc.CATEGORY,
    				    ccpc.[CATALOG]
    			    FROM GETCATALOGCHANNELPRODUCTCATEGORY ccpc		
    			    WHERE
                        ccpc.INCLUDEEXCLUDETYPE = 1    -- Include
                        AND EXISTS
                        (
                            SELECT * FROM {0} categoryIds WHERE categoryIds.RECID = ccpc.CATEGORY
                        )
    		    )
    
    		    SELECT DISTINCT
    
    			    pc.PRODUCT AS RECID
    
    		    FROM GETPRODUCTCATEGORIES pc";
    
            private SqliteDatabaseContext context;
            private IEnumerable<long> categoryIds;
            private long channelId;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProductsByCategoryProcedure"/> class.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="categoryIds">The category identifier collection.</param>
            /// <param name="channelId">The channel identifier.</param>
            public GetProductsByCategoryProcedure(SqliteDatabaseContext context, IEnumerable<long> categoryIds, long channelId)
            {
                this.context = context;
                this.categoryIds = categoryIds;
                this.channelId = channelId;
            }
    
            /// <summary>
            /// Executes the get products by category procedure.
            /// </summary>
            /// <returns>The collection of product identifiers.</returns>
            public ReadOnlyCollection<long> Execute()
            {
                var categoryIdsTableType = new RecordIdTableType(this.categoryIds);
    
                using (TempTable tmpCategoryIds = this.context.CreateTemporaryTable(categoryIdsTableType.DataTable))
                {
                    var query = new SqlQuery(QueryCommand, categoryIdsTableType.DataTable.TableName);
                    query.Parameters["@bi_ChannelId"] = this.channelId;
                    query.Parameters["@dt_ChannelDate"] = this.context.ChannelDateTimeNow;
    
                    return this.context.ExecuteScalarCollection<long>(query);
                }
            }
        }
    }
}
