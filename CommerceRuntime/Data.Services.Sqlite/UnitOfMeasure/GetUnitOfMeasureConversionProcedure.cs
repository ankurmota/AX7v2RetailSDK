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
    namespace Commerce.Runtime.DataServices.Sqlite.DataServices.UnitOfMeasure
    {
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class to get unit of measure conversions.
        /// </summary>
        internal sealed class GetUnitOfMeasureConversionProcedure
        {
            private const string ItemIdColumnName = "ITEMID";
            private const string FromUnitIdColumnName = "FROMUNITID";
            private const string ToUnitIdColumnName = "TOUNITID";
    
            private GetUnitOfMeasureConversionDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetUnitOfMeasureConversionProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            public GetUnitOfMeasureConversionProcedure(GetUnitOfMeasureConversionDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the database code to get unit of measure conversions.
            /// </summary>
            /// <returns>The response message.</returns>
            public GetUnitOfMeasureConversionDataResponse Execute()
            {
                PagedResult<UnitOfMeasureConversion> result = null;
    
                const string QueryCommand = @"
                    SELECT
                            u.[ITEMID] AS ITEMID,
                            u.[FROMUNITID] AS FROMUNITID,
                            u.[TOUNITID] AS TOUNITID,
                            u.[FROMUOMSYMBOL] AS FROMUOMSYMBOL,
                            u.[TOUOMSYMBOL] AS TOUOMSYMBOL,
                            u.[ISBACKWARD] AS ISBACKWARD,
                            uomc.[RECID] AS RECID,
                            uomc.[DENOMINATOR] AS DENOMINATOR,
                            uomc.[FACTOR] AS FACTOR,
                            uomc.[FROMUNITOFMEASURE] AS FROMUNITOFMEASURE,
                            uomc.[INNEROFFSET] AS INNEROFFSET,
                            uomc.[NUMERATOR] AS NUMERATOR,
                            uomc.[OUTEROFFSET] AS OUTEROFFSET,
                            uomc.[PRODUCT] AS PRODUCT,
                            uomc.[ROUNDING] AS ROUNDING,
                            uomc.[TOUNITOFMEASURE] AS TOUNITOFMEASURE
                        FROM
                        (
                            SELECT
                                iuc.ITEMID,
                                iuc.FROMUNITID,
                                iuc.TOUNITID,
                                uom_from.SYMBOL AS FROMUOMSYMBOL,
                                uom_to.SYMBOL AS TOUOMSYMBOL,
                                CASE
                                    WHEN uomc1.RECID IS NOT NULL THEN 0
                                    WHEN uomc2.RECID IS NOT NULL THEN 1
                                    WHEN uomc3.RECID IS NOT NULL THEN 0
                                    WHEN uomc4.RECID IS NOT NULL THEN 1
                                END ISBACKWARD,
                                CASE
                                    WHEN uomc1.RECID IS NOT NULL THEN uomc1.RECID
                                    WHEN uomc2.RECID IS NOT NULL THEN uomc2.RECID
                                    WHEN uomc3.RECID IS NOT NULL THEN uomc3.RECID
                                    WHEN uomc4.RECID IS NOT NULL THEN uomc4.RECID
                                END RECID
                            FROM {0} iuc
                            INNER JOIN [ax].UNITOFMEASURE uom_from ON uom_from.SYMBOL = iuc.FROMUNITID
                            INNER JOIN [ax].UNITOFMEASURE uom_to ON uom_to.SYMBOL = iuc.TOUNITID
                            LEFT JOIN [ax].INVENTTABLE it ON it.ITEMID = iuc.ITEMID AND it.DATAAREAID = @nvc_DataAreaId
                            LEFT JOIN [ax].UNITOFMEASURECONVERSION uomc1
                                ON uomc1.FROMUNITOFMEASURE = uom_from.RECID
                                    AND uomc1.TOUNITOFMEASURE = uom_to.RECID
                                    AND uomc1.PRODUCT = it.PRODUCT
                            LEFT JOIN [ax].UNITOFMEASURECONVERSION uomc2
                                ON uomc2.FROMUNITOFMEASURE = uom_to.RECID
                                    AND uomc2.TOUNITOFMEASURE = uom_from.RECID
                                    AND uomc2.PRODUCT = it.PRODUCT
                            LEFT JOIN [ax].UNITOFMEASURECONVERSION uomc3
                                ON uomc3.FROMUNITOFMEASURE = uom_from.RECID
                                    AND uomc3.TOUNITOFMEASURE = uom_to.RECID
                                    AND uomc3.PRODUCT = 0
                            LEFT JOIN [ax].UNITOFMEASURECONVERSION uomc4
                                ON uomc4.FROMUNITOFMEASURE = uom_to.RECID
                                    AND uomc4.TOUNITOFMEASURE = uom_from.RECID
                                    AND uomc4.PRODUCT = 0
                        ) U
                        INNER JOIN [ax].UNITOFMEASURECONVERSION uomc ON uomc.RECID = u.RECID
    ";
    
                using (DataTable itemUnitConversionsTable = new DataTable("tvp_ItemUnitConversions"))
                {
                    itemUnitConversionsTable.Columns.Add(ItemIdColumnName, typeof(string));
                    itemUnitConversionsTable.Columns.Add(FromUnitIdColumnName, typeof(string));
                    itemUnitConversionsTable.Columns.Add(ToUnitIdColumnName, typeof(string));
    
                    foreach (ItemUnitConversion itemUnitConversion in this.request.ItemUnitConversions)
                    {
                        DataRow row = itemUnitConversionsTable.NewRow();
                        itemUnitConversionsTable.Rows.Add(row);
    
                        row[ItemIdColumnName] = itemUnitConversion.ItemId;
                        row[FromUnitIdColumnName] = itemUnitConversion.FromUnitOfMeasure;
                        row[ToUnitIdColumnName] = itemUnitConversion.ToUnitOfMeasure;
                    }
    
                    using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                    using (var itemUnitConvertionsTempTable = databaseContext.CreateTemporaryTable(itemUnitConversionsTable))
                    {
                        var sqlQuery = new SqlQuery(QueryCommand, itemUnitConvertionsTempTable.TableName);
                        sqlQuery.Parameters["@nvc_DataAreaId"] = databaseContext.DataAreaId;
    
                        result = databaseContext.ReadEntity<UnitOfMeasureConversion>(sqlQuery);
                    }
                }
    
                return new GetUnitOfMeasureConversionDataResponse(result);
            }
        }
    }
}
