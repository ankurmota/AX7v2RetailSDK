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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Helper procedure class for getting linked products.
        /// </summary>
        internal sealed class GetLinkedProductsProcedure
        {
            private readonly SqliteDatabaseContext context;
            private readonly TempTable productLookupTable;
            private readonly long channelId;
            private readonly IEnumerable<long> productIds;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetLinkedProductsProcedure"/> class.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="channelId">The channel identifier in where the search will occur.</param>
            /// <param name="productLookupTable">The product lookup temporary table.</param>
            public GetLinkedProductsProcedure(SqliteDatabaseContext context, long channelId, TempTable productLookupTable)
            {
                this.context = context;
                this.productLookupTable = productLookupTable;
                this.channelId = channelId;
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetLinkedProductsProcedure"/> class.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="channelId">The channel identifier in where the search will occur.</param>
            /// <param name="productIds">The product identifier collection.</param>
            public GetLinkedProductsProcedure(SqliteDatabaseContext context, long channelId, IEnumerable<long> productIds)
            {
                this.context = context;
                this.channelId = channelId;
                this.productIds = productIds;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            /// <returns>The collection of <see cref="LinkedProduct"/>.</returns>
            public ReadOnlyCollection<LinkedProduct> Execute()
            {
                ReadOnlyCollection<LinkedProduct> linkedProducts;
    
                if (this.productLookupTable != null)
                {
                    linkedProducts = this.Execute(this.productLookupTable);
                }
                else if (!this.productIds.IsNullOrEmpty())
                {
                    using (TempTable productInputTempTable = TempTableHelper.CreateScalarTempTable(this.context, "LOOKUPID", this.productIds))
                    {
                        linkedProducts = this.Execute(productInputTempTable);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid arguments for GetLinkedProductsProcedure.Execute(). Either productLookupTable or productIds must be provided at class constructor.");
                }
    
                return linkedProducts;
            }
    
            private ReadOnlyCollection<LinkedProduct> Execute(TempTable productInputTempTable)
            {
                const string GetLinkedProductIdsQueryCommand = @"
                    SELECT LOOKUPID
                    FROM {0} ids
    
                    UNION
    
                    SELECT lit.PRODUCT as LOOKUPID
                    FROM {0} ids
                        INNER JOIN [ax].INVENTTABLE it ON it.PRODUCT = ids.LOOKUPID AND it.DATAAREAID = @nvc_DataAreaId
                        INNER JOIN [ax].RETAILINVENTLINKEDITEM rili ON rili.ITEMID = it.ITEMID and rili.DATAAREAID = it.DATAAREAID
                        INNER JOIN [ax].INVENTTABLE lit ON lit.ITEMID = rili.LINKEDITEMID AND lit.DATAAREAID = rili.DATAAREAID";
    
                // get linked products ids
                SqlQuery getLinkedProductIdsQuery = new SqlQuery(GetLinkedProductIdsQueryCommand, productInputTempTable.TableName);
                getLinkedProductIdsQuery.Parameters["@nvc_DataAreaId"] = this.context.DataAreaId;
                IEnumerable<long> linkedProductIds = this.context.ExecuteScalarCollection<long>(getLinkedProductIdsQuery);
    
                // retrieve only assorted product ids from linked products
                GetAssortedProductsProcedure getAssortedProductsProcedure = new GetAssortedProductsProcedure(
                    this.context,
                    this.channelId,
                    linkedProductIds,
                    expandVariants: false,
                    pagingSettings: PagingInfo.AllRecords);
    
                const string GetLinkedProductDetailsQueryCommand = @"
                    SELECT DISTINCT
                        ap.PRODUCTID    AS PRODUCT,
                        lit.PRODUCT     AS LINKEDPRODUCT,
                        rili.QTY        AS QTY
                    FROM {0} ap
                    INNER JOIN [ax].RETAILINVENTLINKEDITEM rili ON rili.ITEMID = ap.ITEMID AND rili.DATAAREAID = @nvc_DataAreaId
                    INNER JOIN [ax].INVENTTABLE lit ON lit.ITEMID = rili.LINKEDITEMID AND lit.DATAAREAID = rili.DATAAREAID";
    
                ReadOnlyCollection<LinkedProduct> linkedProducts;
    
                using (TempTable assortedLinkedProducts = getAssortedProductsProcedure.GetAssortedProducts())
                {
                    SqlQuery getLinkedProductDetailsQuery = new SqlQuery(GetLinkedProductDetailsQueryCommand, assortedLinkedProducts.TableName);
                    getLinkedProductDetailsQuery.Parameters["@nvc_DataAreaId"] = this.context.DataAreaId;
                    linkedProducts = this.context.ReadEntity<LinkedProduct>(getLinkedProductDetailsQuery).Results;
                }
    
                return linkedProducts;
            }
        }
    }
}
