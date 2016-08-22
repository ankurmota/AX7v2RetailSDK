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
        /// Procedure class to get behavior of products by their respective product identifiers.
        /// </summary>
        internal sealed class GetProductBehaviorByProductIdsProcedure
        {
            // Variable names.
            private const string IsRemoteVariableName = "@i_IsRemote";
            private const string ProductIdsVariableName = "@tvp_ProductIds";
    
            // Request.
            private GetProductBehaviorDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProductBehaviorByProductIdsProcedure"/> class.
            /// </summary>
            /// <param name="request">The data request.</param>
            public GetProductBehaviorByProductIdsProcedure(GetProductBehaviorDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the stored procedure.
            /// </summary>
            /// <returns>The data response.</returns>
            public PagedResult<ProductBehavior> Execute()
            {
                if (!this.request.ProductIds.Any())
                {
                    return new PagedResult<ProductBehavior>(new List<ProductBehavior>().AsReadOnly());
                }
    
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                using (RecordIdTableType type = new RecordIdTableType(this.request.ProductIds, ProductIdsVariableName))
                {
                    string query = @"
                        -- Retrieve behvaviors for variant id = 0 products.
                        SELECT DISTINCT
                            CASE
                                WHEN [siv].PRODUCTID IS NULL THEN 0
                                ELSE 1
                            END AS HASSERIALNUMBER,
                            CASE [rit].PROHIBITRETURN_RU
                                WHEN 0 THEN 1
                                WHEN 1 THEN 0
                            END AS ISRETURNALLOWED,
                            [rit].BLOCKEDONPOS AS ISSALEATPHYSICALSTORESALLOWED,
                            [rit].QTYBECOMESNEGATIVE AS ISNEGATIVEQUANTITYALLOWED,
                            CASE [rit].NODISCOUNTALLOWED
                                WHEN 0 THEN 1
                                WHEN 1 THEN 0
                            END AS ISDISCOUNTALLOWED,
                            [rk].DISASSEMBLYATREGISTERALLOWED AS ISKITDISASSEMBLYALLOWED,
                            [rit].ZEROPRICEVALID AS ISZEROSALEPRICEALLOWED,
                            [rit].KEYINGINPRICE AS KEYINPRICE,
                            [rit].KEYINGINQTY AS KEYINQUANTITY,
                            [rit].MUSTKEYINCOMMENT AS MUSTKEYINCOMMENT,
                            [rit].PRINTVARIANTSSHELFLABELS AS MUSTPRINTINDIVIDUALSHELFLABELSFORVARIANTS,
                            [siv].ISSALESPROCESSACTIVATED AS MUSTPROMPTFORSERIALNUMBERONLYATSALE,
                            [rit].SCALEITEM AS MUSTWEIGHPRODUCTATSALE,
                            CASE
                                WHEN [par].VARIANTID != 0 AND [par].VARIANTID = [erdpv].RECID THEN [par].VARIANTID
                                WHEN [par].VARIANTID = 0 AND [par].PRODUCTID = [erdpv].PRODUCTMASTER THEN [par].PRODUCTID
                                ELSE [par].PRODUCTID
                            END AS PRODUCTID,
                            [rit].DATETOACTIVATEITEM AS VALIDFROMDATEFORSALEATPHYSICALSTORES,
                            [rit].DATETOBEBLOCKED AS VALIDTODATEFORSALEATPHYSICALSTORES
                        FROM @tvp_ProductIds ids
                        INNER JOIN [crt].PRODUCTASSORTMENTRULES par ON [par].CHANNELID = @bi_ChannelId AND [par].PRODUCTID = [ids].RECID AND [par].VARIANTID = 0 AND [par].ISREMOTE = @i_IsRemote AND @dt_ChannelDate BETWEEN [par].VALIDFROM AND [par].VALIDTO
                        INNER JOIN [ax].RETAILCHANNELTABLE rct ON [rct].RECID = [par].CHANNELID
                        INNER JOIN [ax].INVENTTABLE it ON [it].PRODUCT = [par].PRODUCTID AND [rct].INVENTLOCATIONDATAAREAID = [it].DATAAREAID
                        INNER JOIN [ax].RETAILINVENTTABLE rit ON [rit].ITEMID = [it].ITEMID AND [rit].DATAAREAID = [it].DATAAREAID
                        LEFT OUTER JOIN [crt].SERIALIZEDITEMSVIEW siv ON [siv].PRODUCTID = [par].PRODUCTID AND [siv].ITEMDATAAREAID = [it].DATAAREAID
                        LEFT OUTER JOIN [ax].RETAILKIT rk ON [rk].PRODUCTMASTER = [par].PRODUCTID AND [par].VARIANTID = 0
                        LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT erdpv ON [erdpv].RECID = [par].VARIANTID
    
                        UNION ALL
    
                        -- Retrieve behvaviors for variant id != 0 products.
                        SELECT DISTINCT
                            CASE
                                WHEN [siv].PRODUCTID IS NULL THEN 0
                                ELSE 1
                            END AS HASSERIALNUMBER,
                            CASE [rit].PROHIBITRETURN_RU
                                WHEN 0 THEN 1
                                WHEN 1 THEN 0
                            END AS ISRETURNALLOWED,
                            [rit].BLOCKEDONPOS AS ISSALEATPHYSICALSTORESALLOWED,
                            [rit].QTYBECOMESNEGATIVE AS ISNEGATIVEQUANTITYALLOWED,
                            CASE [rit].NODISCOUNTALLOWED
                                WHEN 0 THEN 1
                                WHEN 1 THEN 0
                            END AS ISDISCOUNTALLOWED,
                            [rk].DISASSEMBLYATREGISTERALLOWED AS ISKITDISASSEMBLYALLOWED,
                            [rit].ZEROPRICEVALID AS ISZEROSALEPRICEALLOWED,
                            [rit].KEYINGINPRICE AS KEYINPRICE,
                            [rit].KEYINGINQTY AS KEYINQUANTITY,
                            [rit].MUSTKEYINCOMMENT AS MUSTKEYINCOMMENT,
                            [rit].PRINTVARIANTSSHELFLABELS AS MUSTPRINTINDIVIDUALSHELFLABELSFORVARIANTS,
                            [siv].ISSALESPROCESSACTIVATED AS MUSTPROMPTFORSERIALNUMBERONLYATSALE,
                            [rit].SCALEITEM AS MUSTWEIGHPRODUCTATSALE,
                            CASE
                                WHEN [par].VARIANTID != 0 AND [par].VARIANTID = [erdpv].RECID THEN [par].VARIANTID
                                WHEN [par].VARIANTID = 0 AND [par].PRODUCTID = [erdpv].PRODUCTMASTER THEN [par].PRODUCTID
                                ELSE [par].PRODUCTID
                            END AS PRODUCTID,
                            [rit].DATETOACTIVATEITEM AS VALIDFROMDATEFORSALEATPHYSICALSTORES,
                            [rit].DATETOBEBLOCKED AS VALIDTODATEFORSALEATPHYSICALSTORES
                        FROM @tvp_ProductIds ids
                        INNER JOIN [crt].PRODUCTASSORTMENTRULES par ON [par].CHANNELID = @bi_ChannelId AND [par].VARIANTID = [ids].RECID AND [par].ISREMOTE = @i_IsRemote AND @dt_ChannelDate BETWEEN [par].VALIDFROM AND [par].VALIDTO
                        INNER JOIN [ax].RETAILCHANNELTABLE rct ON [rct].RECID = [par].CHANNELID
                        INNER JOIN [ax].INVENTTABLE it ON [it].PRODUCT = [par].PRODUCTID AND [rct].INVENTLOCATIONDATAAREAID = [it].DATAAREAID
                        INNER JOIN [ax].RETAILINVENTTABLE rit ON [rit].ITEMID = [it].ITEMID AND [rit].DATAAREAID = [it].DATAAREAID
                        LEFT OUTER JOIN [crt].SERIALIZEDITEMSVIEW siv ON [siv].PRODUCTID = [par].PRODUCTID AND [siv].ITEMDATAAREAID = [it].DATAAREAID
                        LEFT OUTER JOIN [ax].RETAILKIT rk ON [rk].PRODUCTMASTER = [par].PRODUCTID AND [par].VARIANTID = 0
                        LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT erdpv ON [erdpv].RECID = [par].VARIANTID";
    
                    long currentChannelId = this.request.RequestContext.GetPrincipal().ChannelId;
                    SqlQuery sqlQuery = new SqlQuery(query, type.DataTable);
                    sqlQuery.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    sqlQuery.Parameters[DatabaseAccessor.ChannelDateVariableName] = this.request.RequestContext.GetNowInChannelTimeZone().DateTime;
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
    
                    return databaseContext.ReadEntity<ProductBehavior>(sqlQuery);
                }
            }
        }
    }
}
