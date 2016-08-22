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
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class for retrieving kit components.
        /// </summary>
        internal sealed class GetKitComponentsProcedure
        {
            private const string MasterProductRecordIdTableName = "MASTERPRODUCTRECORDIDTABLE";
            private const string StandaloneAndVariantProductRecordIdTableName = "STANDALONEANDVARIANTPRODUCTRECORDIDTABLE";
            private GetParentKitDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetKitComponentsProcedure"/> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            public GetKitComponentsProcedure(GetParentKitDataRequest request)
            {
                this.request = request;
            }
    
            public PagedResult<KitComponent> Execute()
            {
                const string GetKitComponentsQueryString = @"
                    WITH ExplodedKitComponentVariants AS
                    (
    	                -- First explode all the kit components/substitutes that are product masters
                        SELECT DISTINCT
    		                PV.RECID                     AS KITLINEPRODUCTLISTING,
    		                RKC.QUANTITY                 AS QUANTITY,
    		                UOM.SYMBOL                   AS UNIT,
    		                0                            AS CHARGE,
    		                MasterProductIds.RECID       AS KITLINEPRODUCTMASTERLISTING,
    		                1                            AS ISDEFAULTCOMPONENT,
    		                RK.PRODUCTMASTER             AS KITPRODUCTMASTERLISTING,
    		                RKC.RECID                    AS KITLINEIDENTIFIER,
    		                @bi_ChannelId                AS CHANNEL,
    		                IT.ITEMID                    AS ITEMID
    	                FROM {0} AS MasterProductIds		-- get all masters
    		                INNER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT PV ON PV.PRODUCTMASTER = MasterProductIds.RECID	-- explode variants for the masters
    		                INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.COMPONENT = PV.RECID
    		                INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    		                INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = PV.PRODUCTMASTER
    		                INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    		                INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKC.UNITOFMEASURE
                    UNION		
                        SELECT DISTINCT
    		                PV.RECID                     AS KITLINEPRODUCTLISTING,
    		                RKCS.QUANTITY                AS QUANTITY,
    		                UOM.SYMBOL                   AS UNIT,
    		                IFNULL(RKRSC.SUBSTITUTECHARGE, 0.0)  AS CHARGE,
    		                MasterProductIds.RECID       AS KITLINEPRODUCTMASTERLISTING,
    		                0                            AS ISDEFAULTCOMPONENT,
    		                RK.PRODUCTMASTER             AS KITPRODUCTMASTERLISTING,
    		                RKC.RECID                    AS KITLINEIDENTIFIER,
    		                @bi_ChannelId                AS CHANNEL,
    		                IT.ITEMID                    AS ITEMID
    	                FROM {0} AS MasterProductIds		-- get all masters
    		                INNER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT PV ON PV.PRODUCTMASTER = MasterProductIds.RECID	-- explode variants for the masters
    		                INNER JOIN [ax].RETAILKITCOMPONENTSUBSTITUTE RKCS ON RKCS.SUBSTITUTEPRODUCT = PV.RECID
    		                INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.RECID = RKCS.KITCOMPONENT
    		                INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    		                INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = PV.PRODUCTMASTER
    		                INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    		                INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKCS.UNITOFMEASURE			
    		                LEFT JOIN [ax].RETAILKITRELEASEDSUBSTITUTECHARGE RKRSC ON RKRSC.KITCOMPONENTSUBSTITUTE = RKCS.RECID AND RKRSC.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
                    ),
    
                    KitComponentVariantsInfo AS
                    (
    	                -- Collect information of all kit components/substitutes that are non product masters.
    
                        -- Standalone products
    
    	                SELECT DISTINCT
    		                RKC.COMPONENT    AS KITLINEPRODUCTLISTING,
    		                RKC.QUANTITY     AS QUANTITY,
    		                UOM.SYMBOL       AS UNIT,
    		                0                AS CHARGE,
                            StandaloneAndVariantIds.RECID AS KITLINEPRODUCTMASTERLISTING,
    		                1                AS ISDEFAULTCOMPONENT,
    		                RK.PRODUCTMASTER AS KITPRODUCTMASTERLISTING,
    		                RKC.RECID        AS KITLINEIDENTIFIER,
    		                @bi_ChannelId    AS CHANNEL,
                            IT.ITEMID        AS ITEMID
    	                FROM {1} StandaloneAndVariantIds	-- get standalone and variants
    		                INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.COMPONENT = StandaloneAndVariantIds.RECID
    		                INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    		                INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKC.UNITOFMEASURE
    		                INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    		                INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = StandaloneAndVariantIds.RECID AND IT.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    		
                        UNION
    
    	                SELECT DISTINCT
    		                RKCS.SUBSTITUTEPRODUCT        AS KITLINEPRODUCTLISTING,
    		                RKCS.QUANTITY                 AS QUANTITY,
    		                UOM.SYMBOL                    AS UNIT,
    		                IFNULL(RKRSC.SUBSTITUTECHARGE, 0.0)  AS CHARGE,
                            StandaloneAndVariantIds.RECID AS KITLINEPRODUCTMASTERLISTING,
    		                0                             AS ISDEFAULTCOMPONENT,
    		                RK.PRODUCTMASTER              AS KITPRODUCTMASTERLISTING,
    		                RKC.RECID                     AS KITLINEIDENTIFIER,
    		                @bi_ChannelId                 AS CHANNEL,
                            IT.ITEMID                     AS ITEMID
    	                FROM {1} StandaloneAndVariantIds	-- get standalone and variants
    		                INNER JOIN [ax].RETAILKITCOMPONENTSUBSTITUTE RKCS ON RKCS.SUBSTITUTEPRODUCT = StandaloneAndVariantIds.RECID
    		                INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.RECID = RKCS.KITCOMPONENT
    		                INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    		                INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKCS.UNITOFMEASURE
    		                INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    		                INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = StandaloneAndVariantIds.RECID AND IT.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    		                LEFT JOIN [ax].RETAILKITRELEASEDSUBSTITUTECHARGE RKRSC ON RKRSC.KITCOMPONENTSUBSTITUTE = RKCS.RECID AND RKRSC.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    
                        UNION
    
    	                -- Variant productS
    
    	                SELECT DISTINCT
    		                RKC.COMPONENT    AS KITLINEPRODUCTLISTING,
    		                RKC.QUANTITY     AS QUANTITY,
    		                UOM.SYMBOL       AS UNIT,
    		                0                AS CHARGE,
                            PV.PRODUCTMASTER AS KITLINEPRODUCTMASTERLISTING,
    		                1                AS ISDEFAULTCOMPONENT,
    		                RK.PRODUCTMASTER AS KITPRODUCTMASTERLISTING,
    		                RKC.RECID        AS KITLINEIDENTIFIER,
    		                @bi_ChannelId    AS CHANNEL,
                            IT.ITEMID        AS ITEMID
    	                FROM {1} StandaloneAndVariantIds	-- get standalone and variants
    		                INNER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT PV ON PV.RECID = StandaloneAndVariantIds.RECID
    		                INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.COMPONENT = StandaloneAndVariantIds.RECID
    		                INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    		                INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKC.UNITOFMEASURE
    		                INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    		                INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = PV.PRODUCTMASTER AND IT.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    		
                        UNION
    
    	                SELECT DISTINCT
    		                RKCS.SUBSTITUTEPRODUCT        AS KITLINEPRODUCTLISTING,
    		                RKCS.QUANTITY                 AS QUANTITY,
    		                UOM.SYMBOL                    AS UNIT,
    		                IFNULL(RKRSC.SUBSTITUTECHARGE, 0.0)  AS CHARGE,
                            PV.PRODUCTMASTER              AS KITLINEPRODUCTMASTERLISTING,
    		                0                             AS ISDEFAULTCOMPONENT,
    		                RK.PRODUCTMASTER              AS KITPRODUCTMASTERLISTING,
    		                RKC.RECID                     AS KITLINEIDENTIFIER,
    		                @bi_ChannelId                 AS CHANNEL,
                            IT.ITEMID                     AS ITEMID
    	                FROM {1} StandaloneAndVariantIds	-- get standalone and variants
    		                INNER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT PV ON PV.RECID = StandaloneAndVariantIds.RECID
    		                INNER JOIN [ax].RETAILKITCOMPONENTSUBSTITUTE RKCS ON RKCS.SUBSTITUTEPRODUCT = StandaloneAndVariantIds.RECID
    		                INNER JOIN [ax].RETAILKITCOMPONENT RKC ON RKC.RECID = RKCS.KITCOMPONENT
    		                INNER JOIN [ax].RETAILKIT RK ON RK.RECID = RKC.KIT
    		                INNER JOIN [ax].UNITOFMEASURE UOM ON UOM.RECID = RKCS.UNITOFMEASURE
    		                INNER JOIN [ax].RETAILCHANNELTABLE RCT ON RCT.RECID = @bi_ChannelId
    		                INNER JOIN [ax].INVENTTABLE IT ON IT.PRODUCT = StandaloneAndVariantIds.RECID AND IT.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    		                LEFT JOIN [ax].RETAILKITRELEASEDSUBSTITUTECHARGE RKRSC ON RKRSC.KITCOMPONENTSUBSTITUTE = RKCS.RECID AND RKRSC.DATAAREAID = RCT.INVENTLOCATIONDATAAREAID
    
                    ),
    
                    KitComponentInfo AS
                    (
    	                -- For the same product in a component line of a given kit, component information specified at the non-product-master level takes precedence over component information specified at the product master level.
    	                -- Hence, add an entry from the exploded table only if an entry does not already exist in the non-product master table.
    
    	                SELECT
    		                KCVI.KITLINEPRODUCTLISTING			AS KITLINEPRODUCTLISTING,
    		                KCVI.QUANTITY						AS QUANTITY,
    		                KCVI.UNIT							AS UNIT,
    		                KCVI.CHARGE							AS CHARGE,	
    		                KCVI.KITLINEPRODUCTMASTERLISTING	AS KITLINEPRODUCTMASTERLISTING,
    		                KCVI.ISDEFAULTCOMPONENT				AS ISDEFAULTCOMPONENT,
    		                KCVI.KITPRODUCTMASTERLISTING		AS KITPRODUCTMASTERLISTING,
    		                KCVI.KITLINEIDENTIFIER				AS KITLINEIDENTIFIER,
    		                KCVI.CHANNEL						AS CHANNEL,
    		                KCVI.ITEMID							AS ITEMID
    	                FROM KitComponentVariantsInfo KCVI
    
    	                UNION ALL
    
    	                SELECT
    		                EKCV.KITLINEPRODUCTLISTING			AS KITLINEPRODUCTLISTING,
    		                EKCV.QUANTITY						AS QUANTITY,
    		                EKCV.UNIT							AS UNIT,
    		                EKCV.CHARGE							AS CHARGE,
    		                EKCV.KITLINEPRODUCTMASTERLISTING	AS KITLINEPRODUCTMASTERLISTING,
    		                EKCV.ISDEFAULTCOMPONENT				AS ISDEFAULTCOMPONENT,
    		                EKCV.KITPRODUCTMASTERLISTING		AS KITPRODUCTMASTERLISTING,
    		                EKCV.KITLINEIDENTIFIER				AS KITLINEIDENTIFIER,
    		                EKCV.CHANNEL						AS CHANNEL,
                            EKCV.ITEMID							AS ITEMID
    	                FROM ExplodedKitComponentVariants EKCV
    	                WHERE NOT EXISTS
    	                (
    		                SELECT
    			                *
    		                FROM KitComponentVariantsInfo KCVI
    		                WHERE
    			                KCVI.KITLINEPRODUCTLISTING = EKCV.KITLINEPRODUCTLISTING
    			                AND KCVI.KITPRODUCTMASTERLISTING = EKCV.KITPRODUCTMASTERLISTING
    			                AND KCVI.KITLINEIDENTIFIER = EKCV.KITLINEIDENTIFIER
    	                )
                    )
    	            SELECT
    		            KCI.KITLINEPRODUCTLISTING			AS KITLINEPRODUCTLISTING,
    		            KCI.QUANTITY						AS QUANTITY,
    		            KCI.UNIT							AS UNIT,
    		            KCI.CHARGE							AS CHARGE,
    		            KCI.KITLINEPRODUCTMASTERLISTING		AS KITLINEPRODUCTMASTERLISTING,
    		            KCI.ISDEFAULTCOMPONENT				AS ISDEFAULTCOMPONENT,
    		            KCI.KITPRODUCTMASTERLISTING			AS KITPRODUCTMASTERLISTING,
    		            KCI.KITLINEIDENTIFIER				AS KITLINEIDENTIFIER,
    		            KCI.CHANNEL							AS CHANNEL,
    		            KCI.ITEMID							AS ITEMID
    	            FROM KitComponentInfo KCI";
    
                PagedResult<KitComponent> kitComponents;
    
                IEnumerable<long> standaloneAndVariantProductIds = this.request.ProductIds.Except(this.request.MasterProductIds);
                IEnumerable<long> masterProductIds = this.request.MasterProductIds;
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                using (RecordIdTableType standaloneAndVariantProductIdsTableType = new RecordIdTableType(StandaloneAndVariantProductRecordIdTableName, standaloneAndVariantProductIds))
                using (RecordIdTableType masterProductIdsTableType = new RecordIdTableType(MasterProductRecordIdTableName, masterProductIds))
                using (TempTable standaloneAndVariantProductIdsTempTable = context.CreateTemporaryTable(standaloneAndVariantProductIdsTableType.DataTable))
                using (TempTable masterProductIdsTempTable = context.CreateTemporaryTable(masterProductIdsTableType.DataTable))
                {
                    SqlQuery query = new SqlQuery(
                        GetKitComponentsQueryString,
                        masterProductIdsTempTable.TableName,
                        standaloneAndVariantProductIdsTempTable.TableName);
    
                    query.Parameters["@bi_ChannelId"] = context.ChannelId;
    
                    kitComponents = context.ReadEntity<KitComponent>(query);
                }
    
                return kitComponents;
            }
        }
    }
}
