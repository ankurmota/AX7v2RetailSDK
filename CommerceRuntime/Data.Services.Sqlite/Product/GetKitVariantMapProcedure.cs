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
        using System.Collections.ObjectModel;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class for retrieving kit variant map.
        /// </summary>
        internal sealed class GetKitVariantMapProcedure
        {
            private GetProductKitDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetKitVariantMapProcedure"/> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            public GetKitVariantMapProcedure(GetProductKitDataRequest request)
            {
                this.request = request;
            }
    
            public ReadOnlyCollection<KitConfigToComponentAssociation> Execute()
            {
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    long channelId = context.ChannelId;
    
                    GetAssortedProductsProcedure assortedProductsProcedure = new GetAssortedProductsProcedure(
                        context,
                        channelId,
                        this.request.KitMasterProductIds,
                        true, // skipVariantsExpansion,
                        PagingInfo.AllRecords);
    
                    using (TempTable assortedProducts = assortedProductsProcedure.GetAssortedProducts())
                    {
                        const string GetKitVariantMapQueryString = @"
                            SELECT DISTINCT
                                RKVC.COMPONENT      AS COMPONENTPRODUCTLISTING,
                                RKVC.KITVARIANT     AS KITPRODUCTVARIANTLISTING,
                                IT.PRODUCT          AS KITPRODUCTMASTERLISTING,
                                IDC.INVENTDIMID     AS INVENTDIMID,
                                ID.CONFIGID         AS CONFIGID,
                                RKVC.COMPONENTRECID AS KITLINEIDENTIFIER
                            FROM [ax].RETAILKITVARIANTCOMPONENT RKVC
                                INNER JOIN [ax].INVENTDIMCOMBINATION IDC ON IDC.DISTINCTPRODUCTVARIANT = RKVC.KITVARIANT
                                INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
                                INNER JOIN [ax].INVENTDIM ID ON ID.INVENTDIMID = IDC.INVENTDIMID AND ID.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
                                INNER JOIN [ax].INVENTTABLE IT ON IT.ITEMID = IDC.ITEMID
                                INNER JOIN {0} PID ON PID.PRODUCTID = IT.PRODUCT";
    
                        SqlQuery query = new SqlQuery(GetKitVariantMapQueryString, assortedProducts.TableName);
                        query.Parameters["@bi_ChannelId"] = channelId;
    
                        return context.ReadEntity<KitConfigToComponentAssociation>(query).Results;
                    }
                }
            }
        }
    }
}
