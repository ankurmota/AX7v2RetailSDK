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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Helper class to get product variants.
        /// </summary>
        internal sealed class GetProductVariantsProcedure
        {
            private const string QueryBody = @"
                (
                    WITH PRODUCTVARIANTS as
                    (
    	                SELECT
    			                ProductIds.ITEMID as 'ITEMID',
    			                ProductIds.RETAILVARIANTID AS 'VARIANTID',
    			                id.INVENTDIMID AS 'INVENTDIMID',
    			                ProductIds.DISTINCTPRODUCTVARIANT,
    			                id.INVENTSIZEID AS 'SIZEID',
    			                id.INVENTCOLORID AS 'COLORID',
    			                id.INVENTSTYLEID AS 'STYLEID',
    			                id.CONFIGID AS 'CONFIGID',
    			                COALESCE(dvc.NAME, '') AS 'COLOR',
    			                COALESCE(dvsz.NAME, '') AS 'SIZE',
    			                COALESCE(dvst.NAME, '') AS 'STYLE',
    			                COALESCE(dvcfg.NAME, '') AS 'CONFIG'
    		
                        FROM
                        (
                            {0}
                        ) ProductIds
    		
    		                INNER JOIN [ax].INVENTDIM id ON id.INVENTDIMID = ProductIds.INVENTDIMID AND id.DATAAREAID = @DATAAREAID
    
                            -- Color
    		                LEFT OUTER JOIN [ax].ECORESCOLOR erc ON erc.NAME = id.INVENTCOLORID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERCOLOR erpmc ON erpmc.COLOR = erc.RECID AND erpmc.COLORPRODUCTMASTER = ProductIds.LOOKUPID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERDIMVALUETRANSLATION dvc ON DVC.PRODUCTMASTERDIMENSIONVALUE = erpmc.RECID AND dvc.LANGUAGEID = @LANGUAGEID
    
    		                -- Size
    		                LEFT OUTER JOIN [ax].ECORESSIZE ers ON ers.NAME = id.INVENTSIZEID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERSIZE erpms ON erpms.SIZE = ers.RECID AND erpms.SIZEPRODUCTMASTER = ProductIds.LOOKUPID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERDIMVALUETRANSLATION dvsz ON dvsz.PRODUCTMASTERDIMENSIONVALUE = erpms.RECID AND dvsz.LANGUAGEID = @LANGUAGEID
    
                            -- Style
    		                LEFT OUTER JOIN [ax].ECORESSTYLE erst ON erst .NAME = id.INVENTSTYLEID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERSTYLE erpmst ON erpmst.STYLE = erst .RECID AND erpmst.STYLEPRODUCTMASTER = ProductIds.LOOKUPID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERDIMVALUETRANSLATION dvst ON dvst.PRODUCTMASTERDIMENSIONVALUE = erpmst.RECID AND dvst.LANGUAGEID = @LANGUAGEID
    
    		                -- Configuration
    		                LEFT OUTER JOIN [ax].ECORESCONFIGURATION ercfg ON ercfg.NAME = id.CONFIGID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERCONFIGURATION erpmcfg ON erpmcfg.CONFIGURATION = ercfg.RECID AND erpmcfg.CONFIGPRODUCTMASTER = ProductIds.LOOKUPID
    		                LEFT OUTER JOIN [ax].ECORESPRODUCTMASTERDIMVALUETRANSLATION dvcfg ON dvcfg.PRODUCTMASTERDIMENSIONVALUE = erpmcfg.RECID AND dvcfg.LANGUAGEID = @LANGUAGEID
                    )
    
                    SELECT
    		                [vv].ITEMID                         as ITEMID,
                            [vv].VARIANTID                      as VARIANTID,
                            [vv].INVENTDIMID                    as INVENTDIMID,
                            [vv].DISTINCTPRODUCTVARIANT         as DISTINCTPRODUCTVARIANT,
                            [vv].SIZEID                         as SIZEID,
                            [vv].COLORID                        as COLORID,
                            [vv].STYLEID                        as STYLEID,
                            [vv].CONFIGID                       as CONFIGID,
                            [vv].COLOR                          as COLOR,
                            [vv].SIZE                           as SIZE,
                            [vv].STYLE                          as STYLE,
                            [vv].CONFIG                         as CONFIG
                    FROM PRODUCTVARIANTS vv
                )";
    
            private const string FromRecordId = @"
                            SELECT
                                input.ITEMID,
                                input.VARIANTID AS DISTINCTPRODUCTVARIANT,
                                input.LOOKUPID,
                                idc.RETAILVARIANTID,
                                idc.INVENTDIMID
                            FROM {0} input
    		                CROSS JOIN [ax].INVENTDIMCOMBINATION idc ON idc.DISTINCTPRODUCTVARIANT = input.VARIANTID AND idc.DATAAREAID = @DATAAREAID";
    
            private const string FromItemAndInventoryDimensionId = @"
                            SELECT
                                input.ITEMID,
                                idc.DISTINCTPRODUCTVARIANT,
                                pdv.PRODUCTMASTER as LOOKUPID,
                                idc.RETAILVARIANTID,
                                input.VARIANTINVENTDIMID AS INVENTDIMID
                            FROM {0} input
                            CROSS JOIN [ax].INVENTDIMCOMBINATION idc ON idc.ITEMID = input.ITEMID AND idc.INVENTDIMID = input.VARIANTINVENTDIMID AND idc.DATAAREAID = @DATAAREAID
                            INNER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT pdv ON pdv.RECID = idc.DISTINCTPRODUCTVARIANT
    ";
    
            private string languageId;
            private SqliteDatabaseContext context;
            private QueryResultSettings settings;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProductVariantsProcedure"/> class.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="languageId">The language identifier for retrieving localized values.</param>
            /// <param name="settings">The query settings object.</param>
            public GetProductVariantsProcedure(SqliteDatabaseContext context, string languageId, QueryResultSettings settings)
            {
                this.languageId = languageId;
                this.context = context;
                this.settings = settings;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            /// <param name="productLookupTable">The products lookup temporary table.</param>
            /// <returns>The collection of product variants.</returns>
            public PagedResult<ProductVariant> Execute(TempTable productLookupTable)
            {
                return this.Execute(productLookupTable.TableName, FromRecordId);
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            /// <param name="itemVariantDimensions">The collection of product item variant dimensions.</param>
            /// <returns>The collection of product variants.</returns>
            public PagedResult<ProductVariant> Execute(IEnumerable<ItemVariantInventoryDimension> itemVariantDimensions)
            {
                using (ItemVariantInventoryDimensionTableType itemVariantTableTableType = new ItemVariantInventoryDimensionTableType(itemVariantDimensions))
                using (TempTable itemVariantTempTable = this.context.CreateTemporaryTable(itemVariantTableTableType.DataTable))
                {
                    return this.Execute(itemVariantTempTable.TableName, FromItemAndInventoryDimensionId);
                }
            }
    
            private PagedResult<ProductVariant> Execute(string inputTableName, string fromClause)
            {
                string fromQuery = string.Format(fromClause, inputTableName);
                string query = string.Format(QueryBody, fromQuery);
    
                var sqlQuery = new SqlPagedQuery(this.settings)
                {
                    From = query,
                    Aliased = true,
                    DatabaseSchema = string.Empty
                };
    
                sqlQuery.Parameters["@LANGUAGEID"] = this.languageId;
                sqlQuery.Parameters["@DATAAREAID"] = this.context.DataAreaId;
    
                return this.context.ReadEntity<ProductVariant>(sqlQuery);
            }
        }
    }
}
