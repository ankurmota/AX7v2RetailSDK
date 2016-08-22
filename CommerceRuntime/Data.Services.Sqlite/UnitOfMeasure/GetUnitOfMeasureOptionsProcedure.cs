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
    namespace Commerce.Runtime.DataServices.Sqlite.DataServices
    {
        using System.Collections.ObjectModel;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class for getting a product's possible Unit of Measurement options.
        /// </summary>
        internal sealed class GetUnitOfMeasureOptionsProcedure
        {
            private GetProductUnitOfMeasureOptionsDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the GetUnitOfMeasureOptionsProcedure class.
            /// </summary>
            /// <param name="request">The product unit of measurement options request.</param>
            public GetUnitOfMeasureOptionsProcedure(GetProductUnitOfMeasureOptionsDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the database operation for the unit of measurement options search.
            /// NOTE: For variant products use the Master Products RECID.
            /// </summary>
            /// <returns>Returns the unit of measurement options search result.</returns>
            public GetProductUnitOfMeasureOptionsDataResponse Execute()
            {
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    using (TempTable unitOfMeasureOptionsProductsTempTable = TempTableHelper.CreateScalarTempTable(context, "RECID", this.request.ProductIds))
                    {
                        const string UnitOfMeasureOptionsQueryText = @"
                            SELECT tvppi.RECID AS PRODUCT,
                                it.ITEMID AS ITEMID,
                                uomc.[FROMUNITOFMEASURE] AS FROMUNITID,
                                uom1.[SYMBOL] AS FROMUOMSYMBOL,
                                uomc.[TOUNITOFMEASURE] AS TOUNITID,
                                uom2.[SYMBOL] AS TOUOMSYMBOL
                            FROM ax.UNITOFMEASURECONVERSION uomc
                            INNER JOIN ax.UNITOFMEASURE AS uom1 ON uom1.[RECID] = uomc.[FROMUNITOFMEASURE]
                            INNER JOIN ax.UNITOFMEASURE AS uom2 ON uom2.[RECID] = uomc.[TOUNITOFMEASURE]
                            INNER JOIN ax.INVENTTABLE AS it ON (it.[PRODUCT] = uomc.PRODUCT AND it.[DATAAREAID] = @nvc_DataAreaId) OR uomc.[PRODUCT] = 0
                            INNER JOIN {0} AS tvppi ON tvppi.[RECID] = it.[PRODUCT]
                            INNER JOIN ax.INVENTTABLEMODULE AS itm ON (itm.ITEMID = it.ITEMID AND itm.MODULETYPE = 2 AND itm.DATAAREAID = @nvc_DataAreaId) -- ModuleType = 2 is Sales
                                AND (uom1.[SYMBOL] = itm.UNITID OR uom2.[SYMBOL] = itm.UNITID)";
    
                        SqlQuery unitOfMeasureOptionsQuery = new SqlQuery(UnitOfMeasureOptionsQueryText, unitOfMeasureOptionsProductsTempTable.TableName);
                        unitOfMeasureOptionsQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
                        PagedResult<UnitOfMeasureConversion> unitOfMeasureConversions = context.ReadEntity<UnitOfMeasureConversion>(unitOfMeasureOptionsQuery);
    
                        return new GetProductUnitOfMeasureOptionsDataResponse(unitOfMeasureConversions);
                    }
                }
            }
        }
    }
}
