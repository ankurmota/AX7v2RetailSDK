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
        /// Helper class for retrieving kit definition.
        /// </summary>
        internal sealed class GetKitDefinitionProcedure
        {
            private GetProductKitDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetKitDefinitionProcedure"/> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            public GetKitDefinitionProcedure(GetProductKitDataRequest request)
            {
                this.request = request;
            }
    
            public ReadOnlyCollection<KitDefinition> Execute()
            {
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    long channelId = context.ChannelId;
    
                    GetAssortedProductsProcedure assortedProductsProcedure = new GetAssortedProductsProcedure(
                        context,
                        channelId,
                        this.request.KitMasterProductIds,
                        true, // skipVariantsExpansion,
                        this.request.QueryResultSettings.Paging);
    
                    using (TempTable assortedProducts = assortedProductsProcedure.GetAssortedProducts())
                    {
                        const string GetKitDefinitionQueryString = @"
                            SELECT DISTINCT
                                KPM.PRODUCTID                    AS KITPRODUCTMASTERLISTING,
                                @bi_ChannelId                    AS CHANNEL,
                                RK.DISASSEMBLYATREGISTERALLOWED,
                                RK.RECID                         AS KITRECID
                            FROM {0} AS KPM
                            INNER JOIN [ax].RETAILKIT RK ON KPM.PRODUCTID = RK.PRODUCTMASTER";
    
                        SqlQuery query = new SqlQuery(GetKitDefinitionQueryString, assortedProducts.TableName);
                        query.Parameters["@bi_ChannelId"] = channelId;
    
                        return context.ReadEntity<KitDefinition>(query).Results;
                    }
                }
            }
        }
    }
}
