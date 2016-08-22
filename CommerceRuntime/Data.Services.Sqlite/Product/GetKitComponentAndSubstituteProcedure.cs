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
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class for retrieving kit definition.
        /// </summary>
        internal sealed class GetKitComponentAndSubstituteProcedure
        {
            private GetProductKitDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetKitComponentAndSubstituteProcedure"/> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            public GetKitComponentAndSubstituteProcedure(GetProductKitDataRequest request)
            {
                this.request = request;
            }
    
            public ReadOnlyCollection<KitComponent> Execute()
            {
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    long channelId = context.ChannelId;
    
                    GetAssortedProductsProcedure assortedProductsProcedure = new GetAssortedProductsProcedure(
                        context,
                        channelId,
                        this.request.KitMasterProductIds,
                        true, // skipVariantsExpansion
                        PagingInfo.AllRecords);
    
                    using (TempTable assortedProducts = assortedProductsProcedure.GetAssortedProducts())
                    {
                        const string GetKitComponentQueryString = @"
                            SELECT DISTINCT
    	                        RKC.COMPONENT    AS KITLINEPRODUCTLISTING,
    	                        0                AS KITLINEPRODUCTMASTERLISTING,
    	                        RKC.QUANTITY     AS QUANTITY,
    	                        UOM.SYMBOL       AS UNIT,
    	                        0                AS CHARGE,
    	                        1                AS ISDEFAULTCOMPONENT,
    	                        RK.PRODUCTMASTER AS KITPRODUCTMASTERLISTING,
    	                        RKC.RECID        AS KITLINEIDENTIFIER,
    	                        IT.ITEMID        AS ITEMID
                            FROM [ax].RETAILKITCOMPONENT RKC
    	                        INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    	                        INNER JOIN {0} IDS ON IDS.PRODUCTID = RK.PRODUCTMASTER
    	                        INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = RKC.COMPONENT
    	                        INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKC.UNITOFMEASURE
    
                            UNION
    
                            SELECT DISTINCT
    	                        RKC.COMPONENT    AS KITLINEPRODUCTLISTING,
    	                        PV.PRODUCTMASTER AS KITLINEPRODUCTMASTERLISTING,
    	                        RKC.QUANTITY     AS QUANTITY,
    	                        UOM.SYMBOL       AS UNIT,
    	                        0                AS CHARGE,
    	                        1                AS ISDEFAULTCOMPONENT,
    	                        RK.PRODUCTMASTER AS KITPRODUCTMASTERLISTING,
    	                        RKC.RECID        AS KITLINEIDENTIFIER,
    	                        IT.ITEMID        AS ITEMID
                            FROM [ax].RETAILKITCOMPONENT RKC
    	                        INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    	                        INNER JOIN {0} IDS ON IDS.PRODUCTID = RK.PRODUCTMASTER	
    	                        INNER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT PV ON PV.RECID = RKC.COMPONENT
    	                        INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = PV.PRODUCTMASTER
    	                        INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKC.UNITOFMEASURE
    
                            UNION
    
                            SELECT DISTINCT
    	                        RKCS.SUBSTITUTEPRODUCT              AS KITLINEPRODUCTLISTING,
    	                        0                                   AS KITLINEPRODUCTMASTERLISTING,
    	                        RKCS.QUANTITY                       AS QUANTITY,
    	                        UOM.SYMBOL                          AS UNIT,
    	                        IFNULL(RKRSC.SUBSTITUTECHARGE, 0.0) AS CHARGE,
    	                        0                                   AS ISDEFAULTCOMPONENT,
    	                        RK.PRODUCTMASTER                    AS KITPRODUCTMASTERLISTING,
    	                        RKC.RECID                           AS KITLINEIDENTIFIER,
    	                        IT.ITEMID                           AS ITEMID
                            FROM [ax].RETAILKITCOMPONENTSUBSTITUTE RKCS
    	                        INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.RECID = RKCS.KITCOMPONENT
    	                        INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    	                        INNER JOIN {0} IDS ON IDS.PRODUCTID = RK.PRODUCTMASTER
    	                        INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = RKCS.SUBSTITUTEPRODUCT
    	                        INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    	                        INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKCS.UNITOFMEASURE
    	                        LEFT JOIN [ax].RETAILKITRELEASEDSUBSTITUTECHARGE RKRSC ON RKRSC.KITCOMPONENTSUBSTITUTE = RKCS.RECID AND RKRSC.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    
                            UNION		
    
                            SELECT DISTINCT
    	                        RKCS.SUBSTITUTEPRODUCT       AS KITLINEPRODUCTLISTING,
    	                        PV.PRODUCTMASTER            AS KITLINEPRODUCTMASTERLISTING,
    	                        RKCS.QUANTITY                AS QUANTITY,
    	                        UOM.SYMBOL                   AS UNIT,
    	                        IFNULL(RKRSC.SUBSTITUTECHARGE, 0.0)  AS CHARGE,
    	                        0                            AS ISDEFAULTCOMPONENT,
    	                        RK.PRODUCTMASTER             AS KITPRODUCTMASTERLISTING,
    	                        RKC.RECID                    AS KITLINEIDENTIFIER,
    	                        IT.ITEMID                    AS ITEMID
                            FROM [ax].RETAILKITCOMPONENTSUBSTITUTE RKCS
    	                        INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.RECID = RKCS.KITCOMPONENT
    	                        INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    	                        INNER JOIN {0} IDS ON IDS.PRODUCTID = RK.PRODUCTMASTER
    	                        INNER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT PV ON PV.RECID = RKCS.SUBSTITUTEPRODUCT
    	                        INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = PV.PRODUCTMASTER
    	                        INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKCS.UNITOFMEASURE			
    	                        INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    	                        LEFT JOIN [ax].RETAILKITRELEASEDSUBSTITUTECHARGE RKRSC ON RKRSC.KITCOMPONENTSUBSTITUTE = RKCS.RECID AND RKRSC.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID";
    
                        SqlQuery query = new SqlQuery(GetKitComponentQueryString, assortedProducts.TableName);
                        query.Parameters["@bi_ChannelId"] = channelId;
    
                        return context.ReadEntity<KitComponent>(query).Results;
                    }
                }
            }
        }
    }
}
