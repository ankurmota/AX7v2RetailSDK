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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Procedure class to get items by item ids.
        /// </summary>
        internal sealed class GetItemsProcedure
        {
            private GetItemsDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetItemsProcedure"/> class.
            /// </summary>
            /// <param name="request">The data request.</param>
            public GetItemsProcedure(GetItemsDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the stored procedure.
            /// </summary>
            /// <returns>The data response.</returns>
            public GetItemsDataResponse Execute()
            {
                ReadOnlyCollection<Item> items;
    
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    IEnumerable<long> productIds;
    
                    if (!this.request.ItemIds.IsNullOrEmpty())
                    {
                        productIds = this.GetProductIdsFromItemIds(databaseContext, this.request.ItemIds);
                    }
                    else if (!this.request.ProductIds.IsNullOrEmpty())
                    {
                        productIds = this.request.ProductIds;
                    }
                    else
                    {
                        productIds = new long[0];
                    }
    
                    if (productIds.Any())
                    {
                        using (TempTable assortedProducts = this.GetAssortedProductsTable(databaseContext, productIds))
                        {
                            items = this.GetProductIdentities(databaseContext, assortedProducts);
                        }
                    }
                    else
                    {
                        items = new ReadOnlyCollection<Item>(new List<Item>());
                    }
                }
    
                return new GetItemsDataResponse(items);
            }
    
            private IEnumerable<long> GetProductIdsFromItemIds(SqliteDatabaseContext databaseContext, IEnumerable<string> itemIds)
            {
                using (TempTable recidTable = TempTableHelper.CreateScalarTempTable(databaseContext, "RECID", itemIds))
                {
                    string query = @"SELECT it.PRODUCT FROM {0} ids INNER JOIN [ax].INVENTTABLE it ON it.ITEMID = ids.RECID AND it.DATAAREAID = @DataAreaId";
                    var sqlQuery = new SqlQuery(query, recidTable.TableName);
                    sqlQuery.Parameters["@DataAreaId"] = databaseContext.DataAreaId;
                    return databaseContext.ExecuteScalarCollection<long>(sqlQuery);
                }
            }
    
            private TempTable GetAssortedProductsTable(SqliteDatabaseContext databaseContext, IEnumerable<long> productIds)
            {
                GetAssortedProductsProcedure assortedProductsProcedure = new GetAssortedProductsProcedure(
                    databaseContext,
                    databaseContext.ChannelId,
                    productIds,
                    expandVariants: true,
                    pagingSettings: this.request.QueryResultSettings.Paging);
    
                return assortedProductsProcedure.GetAssortedProducts();
            }
    
            private ReadOnlyCollection<Item> GetProductIdentities(SqliteDatabaseContext databaseContext, TempTable assortedProducts)
            {
                string query = @"
            SELECT DISTINCT
            it.[RECID],
            it.[ITEMID],
            erpt.[NAME],
            erpt.[DESCRIPTION],
            itm.[PRICE],
            itm.[UNITID],
            it.[PRODUCT],
            itm.[MARKUPGROUPID],
            itm.[MARKUP],
            itm.[ALLOCATEMARKUP],
            itm.[PRICEQTY],
            itm.[LINEDISC],
            itm.[MULTILINEDISC],
            itm.[ENDDISC],
            rit.[NODISCOUNTALLOWED],
            itm.[TAXITEMGROUPID] AS 'ITEMTAXGROUPID',
            iitm.[UNITID] AS 'INVENTUNITID'
        FROM {0} ap
        INNER JOIN [crt].CHANNELLANGUAGESVIEW clv ON clv.CHANNEL = @ChannelId AND clv.ISDEFAULT = 1 -- Use default channel language
        INNER JOIN [ax].INVENTTABLE it ON it.PRODUCT = ap.PRODUCTID AND it.DATAAREAID = @DataAreaId
        INNER JOIN [ax].INVENTTABLEMODULE itm ON itm.MODULETYPE = 2 AND itm.ITEMID = it.ITEMID AND itm.DATAAREAID = it.DATAAREAID -- Module type for sales order
        INNER JOIN [ax].INVENTTABLEMODULE iitm ON iitm.MODULETYPE = 0 AND iitm.ITEMID = it.ITEMID AND iitm.DATAAREAID = it.DATAAREAID -- Module type for inventory
        INNER JOIN [ax].ECORESPRODUCTTRANSLATION erpt ON erpt.PRODUCT = it.PRODUCT AND erpt.LANGUAGEID = clv.LANGUAGEID
        INNER JOIN [ax].RETAILINVENTTABLE rit ON it.ITEMID = rit.ITEMID AND rit.DATAAREAID = it.DATAAREAID";
    
                SqlQuery sqlQuery = new SqlQuery(query, assortedProducts.TableName);
                sqlQuery.Parameters["@ChannelId"] = databaseContext.ChannelId;
                sqlQuery.Parameters["@DataAreaId"] = databaseContext.DataAreaId;
    
                return databaseContext.ReadEntity<Item>(sqlQuery).Results;
            }
        }
    }
}
