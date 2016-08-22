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
        using System.Linq;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Helper procedure class to get assorted products.
        /// </summary>
        internal sealed class GetAssortedProductsProcedure
        {
            private const string TempAssortedProductsTableNamePrefix = "tmpAssortedProducts_{0}";
            private const string UnpagedTempAssortedProductsTableNamePrefix = "unpaged";
            private const string ProductLookupTableNamePrefix = "tmpProductIds_{0}";
    
            private SqliteDatabaseContext context;
            private IEnumerable<long> productIds;
            private long channelId;
            private bool expandVariants;
            private PagingInfo pageSettings;
            private string productLookupTableName;
            private string tempAssortedProductsTableName;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetAssortedProductsProcedure"/> class.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="channelId">The channel id in which to execute the request.</param>
            /// <param name="productIds">The collection of product identifiers to check for assortment.</param>
            /// <param name="expandVariants">Whether variants should not be expanded in case a product master / variant is included in the query.</param>
            /// <param name="pagingSettings">The page settings configuration object.</param>
            public GetAssortedProductsProcedure(SqliteDatabaseContext context, long channelId, IEnumerable<long> productIds, bool expandVariants, PagingInfo pagingSettings)
            {
                this.context = context;
                this.productIds = productIds;
                this.channelId = channelId;
                this.expandVariants = expandVariants;
                this.pageSettings = pagingSettings;
            }
    
            private string ProductLookupTableName
            {
                get
                {
                    if (this.productLookupTableName == null)
                    {
                        this.productLookupTableName = string.Format(ProductLookupTableNamePrefix, this.context.GetNextContextIdentifier());
                    }
    
                    return this.productLookupTableName;
                }
            }
    
            private string TempAssortedProductsTableName
            {
                get
                {
                    if (this.tempAssortedProductsTableName == null)
                    {
                        this.tempAssortedProductsTableName = string.Format(TempAssortedProductsTableNamePrefix, this.context.GetNextContextIdentifier());
                    }
    
                    return this.tempAssortedProductsTableName;
                }
            }
    
            private string UnpagedTempAssortedProductsTableName
            {
                get
                {
                    return this.tempAssortedProductsTableName + UnpagedTempAssortedProductsTableNamePrefix;
                }
            }
    
            /// <summary>
            /// Gets a temporary table containing the assorted products identifiers based on the input collection of <see name="productIds"/>.
            /// </summary>
            /// <returns>A temporary table containing the assorted products identifiers based on the input collection of <see name="productIds"/>.</returns>
            public TempTable GetAssortedProducts()
            {
                string assortmentQueryText = @"
                    WITH AssortedProductsToChannel as
                    (
    	                SELECT ral.PRODUCTID, ral.VARIANTID, ral.LINETYPE, ral.VALIDFROM, ral.VALIDTO, rct.INVENTLOCATION
    		                FROM [ax].RETAILASSORTMENTLOOKUP ral
    		                INNER JOIN [ax].RETAILASSORTMENTLOOKUPCHANNELGROUP ralcg ON ralcg.ASSORTMENTID = ral.ASSORTMENTID
    		                INNER JOIN [ax].RETAILCHANNELTABLE rct ON rct.OMOPERATINGUNITID = ralcg.OMOPERATINGUNITID
    	                WHERE
    		                rct.RECID = @CHANNEL
    		                AND @DATE BETWEEN ral.VALIDFROM AND ral.VALIDTO
                    )
    
                    INSERT INTO {0} (PRODUCTID, VARIANTID, LOOKUPID, ISMASTER, ITEMID)
                    SELECT
    	                PRODUCTID,
    	                VARIANTID,
    	                LOOKUPID,
    	                0 AS ISMASTER,
                        it.ITEMID
                    FROM {1} IDS
                        INNER JOIN [ax].INVENTTABLE it ON it.PRODUCT = IDS.LOOKUPID AND it.DATAAREAID = @DATAAREAID -- Include only Released items
                    WHERE
    	                EXISTS
    	                (
    		                SELECT * FROM AssortedProductsToChannel AP
    		                WHERE
    			                AP.LINETYPE = 1
    			                AND AP.PRODUCTID = IDS.LOOKUPID
    			                AND (AP.VARIANTID = 0 OR AP.VARIANTID = IDS.VARIANTID)
    	                )
    	                AND NOT EXISTS
    	                (
    		                SELECT * FROM AssortedProductsToChannel AP
    		                WHERE
    			                AP.LINETYPE = 0
    			                AND AP.PRODUCTID = IDS.LOOKUPID
    			                AND (AP.VARIANTID = 0 OR AP.VARIANTID = IDS.VARIANTID)
    	                );";
    
                const string IncludeMasterQueryText = @"
                    -- includes a record for each master with VARIANTID = 0
                    INSERT INTO {0} (PRODUCTID, VARIANTID, LOOKUPID, ISMASTER, ITEMID)
                    SELECT
    	                R.LOOKUPID,
    	                0 AS VARIANTID,
    	                R.LOOKUPID,
    	                1 as ISMASTER,
                        R.ITEMID
                    FROM {0} R WHERE R.VARIANTID <> 0 GROUP BY LOOKUPID;";
    
                // change temp table name depending on the paging settigns to make sure
                // this code always returns the temp table with same name (TempAssortedProductsTableName)
                string assortedProductsTableName = this.pageSettings.NoPageSizeLimit
                    ? this.TempAssortedProductsTableName
                    : this.UnpagedTempAssortedProductsTableName;
    
                // Create temp table to hold assorted products
                ProductLookupTableType assortedProductsTableDefinition = new ProductLookupTableType(assortedProductsTableName);
                TempTable assortedProducts = this.context.CreateTemporaryTable(assortedProductsTableDefinition.DataTable);
    
                using (TempTable productRecidTable = this.PrepareInput())
                {
                    SqlQuery assortmentQuery = new SqlQuery(assortmentQueryText, assortedProducts.TableName, productRecidTable.TableName);
                    assortmentQuery.Parameters["@CHANNEL"] = this.channelId;
                    assortmentQuery.Parameters["@DATE"] = this.context.ChannelDateTimeNow;
                    assortmentQuery.Parameters["@DATAAREAID"] = this.context.DataAreaId;
    
                    this.context.ExecuteNonQuery(assortmentQuery);
    
                    if (!this.pageSettings.NoPageSizeLimit)
                    {
                        using (TempTable unpagedResultSet = assortedProducts)
                        {
                            // page the result set
                            assortedProducts = this.PageResults(this.context, unpagedResultSet, this.pageSettings);
                        }
                    }
    
                    SqlQuery includeMaterProductQuery = new SqlQuery(IncludeMasterQueryText, assortedProducts.TableName);
                    this.context.ExecuteNonQuery(includeMaterProductQuery);
                }
    
                return assortedProducts;
            }
    
            /// <summary>
            /// Page the results based on the paging configuration.
            /// </summary>
            /// <param name="dbContext">The database context.</param>
            /// <param name="unpagedResultSet">The result set to be paginated.</param>
            /// <param name="pagingInfo">The paging info configuration object.</param>
            /// <returns>The paginated result set.</returns>
            private TempTable PageResults(SqliteDatabaseContext dbContext, TempTable unpagedResultSet, PagingInfo pagingInfo)
            {
                // create table definition
                ProductLookupTableType pagedResultTableDefinition = new ProductLookupTableType(this.TempAssortedProductsTableName);
    
                // and temp table to hold paged results
                TempTable pagedResultTempTable = dbContext.CreateTemporaryTable(pagedResultTableDefinition.DataTable);
    
                string[] columnNames = pagedResultTableDefinition.DataTable.Columns.Select(column => column.ColumnName).ToArray();
                string selectColumns = string.Join(",", columnNames);
    
                // insert into paged result temp table, all records from the unpaged temp table based on unique lookup id
                const string PaginateResultsQueryCommand = @"
                    INSERT INTO {1}
                    (
                        {2}
                    )
                    SELECT
                        {2}
                    FROM {0} UNPAGEDRESULT
                    WHERE
                        LOOKUPID IN
                        (
                            SELECT DISTINCT LOOKUPID
                            FROM {0}
                            ORDER BY LOOKUPID ASC
                            LIMIT @limitValue OFFSET @offsetValue
                        );";
    
                SqlQuery query = new SqlQuery(PaginateResultsQueryCommand, unpagedResultSet.TableName, pagedResultTempTable.TableName, selectColumns);
                query.Parameters["@limitValue"] = pagingInfo.Top;
                query.Parameters["@offsetValue"] = pagingInfo.Skip;
    
                // executes query
                dbContext.ExecuteNonQuery(query);
    
                return pagedResultTempTable;
            }
    
            /// <summary>
            /// Gets a product lookup table that matches the product id and variant id.
            /// </summary>
            /// <returns>A product lookup table that matches the product id and variant id.</returns>
            private TempTable PrepareInput()
            {
                TempTable productLookupTempTable;
    
                // creates table to keep the input productids
                using (TempTable productIdTable = TempTableHelper.CreateScalarTempTable(this.context, "RECID", this.productIds))
                {
                    DataTable productRecIdTableDefinition = new DataTable(this.ProductLookupTableName);
                    productRecIdTableDefinition.Columns.Add(ProductLookupTableType.ProductIdColumnName, typeof(long));
                    productRecIdTableDefinition.Columns.Add(ProductLookupTableType.VariantIdColumnName, typeof(long));
                    productRecIdTableDefinition.Columns.Add(ProductLookupTableType.LookupIdColumnName, typeof(long));
    
                    // Creates the temp table
                    productLookupTempTable = this.context.CreateTemporaryTable(productRecIdTableDefinition);
    
                    string query;
    
                    if (this.expandVariants)
                    {
                        // Populates the temp table with a select, expanding variants
                        query = @"
                                INSERT INTO {0} (PRODUCTID, VARIANTID, LOOKUPID)
                                SELECT DISTINCT
            	                    COALESCE(PVM.RECID, PVV.RECID, I.RECID) as PRODUCTID, -- actual productid, either the variant, or the master/standalone id
            	                    COALESCE(PVM.RECID, PVV.RECID, 0) as VARIANTID,
            	                    COALESCE(PVV.PRODUCTMASTER, I.RECID) as LOOKUPID -- master/standalone
                                FROM {1} I
            	                    LEFT JOIN ax.ECORESDISTINCTPRODUCTVARIANT PVV ON PVV.RECID = I.RECID -- variant
            	                    -- if I.RECID is master product or variant, this join will add all master's variants to the result set
                                    LEFT JOIN ax.ECORESDISTINCTPRODUCTVARIANT PVM ON PVM.PRODUCTMASTER = COALESCE(PVV.PRODUCTMASTER, I.RECID) -- master";
                    }
                    else
                    {
                        // Populates the temp table with a select, excluding variants, but retrieving variant's masters
                        query = @"
                                INSERT INTO {0} (PRODUCTID, VARIANTID, LOOKUPID)
                                SELECT
                                    PRODUCTS.PRODUCTID as PRODUCTID,
                                    0         as VARIANTID,
                                    PRODUCTS.PRODUCTID as LOOKUPID
                                FROM
                                (
                                    -- select all product ids that are NOT variants
                                    SELECT DISTINCT
            	                        I.RECID as PRODUCTID
                                    FROM {1} I
                                    WHERE NOT EXISTS
                                    (
                                        -- select all products ids that are variants
                                        SELECT * FROM ax.ECORESDISTINCTPRODUCTVARIANT PVV
                                        WHERE
                                            PVV.RECID = I.RECID
                                    )
    
                                    UNION
    
                                    -- find all product ids that are variants and return their master id instead
                                    SELECT DISTINCT
            	                        PVV.PRODUCTMASTER as PRODUCTID
                                    FROM {1} I
                                    INNER JOIN ax.ECORESDISTINCTPRODUCTVARIANT PVV ON PVV.RECID = I.RECID
    
                                ) AS PRODUCTS";
                    }
    
                    SqlQuery sqlQuery = new SqlQuery(query, productLookupTempTable.TableName, productIdTable.TableName);
                    this.context.ExecuteNonQuery(sqlQuery);
                }
    
                return productLookupTempTable;
            }
        }
    }
}
