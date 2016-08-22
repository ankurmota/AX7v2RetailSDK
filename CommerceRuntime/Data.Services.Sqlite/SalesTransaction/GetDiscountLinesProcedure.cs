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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The SQLite implementation of getting discount lines.
        /// </summary>
        internal sealed class GetDiscountLinesProcedure
        {
            private GetDiscountLinesDataRequest request;
            private SqliteDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetDiscountLinesProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetDiscountLinesProcedure(GetDiscountLinesDataRequest request, SqliteDatabaseContext databaseContext)
            {
                this.request = request;
                this.databaseContext = databaseContext;
            }
    
            public EntityDataServiceResponse<DiscountLine> Execute()
            {
                string discountLinesQueryStatement =
                    @"SELECT
                        DT.[AMOUNT] AS AMOUNT,
                        DT.[CUSTOMERDISCOUNTTYPE] AS CUSTOMERDISCOUNTTYPE,
                        DT.[DISCOUNTCODE] AS DISCOUNTCODE,
                        DT.[DEALPRICE] AS DEALPRICE,
                        DT.[DISCOUNTAMOUNT] AS DISCOUNTAMOUNT,
                        DT.[DISCOUNTORIGINTYPE] AS DISCOUNTLINETYPE,
                        DT.[LINENUM] AS LINENUM,
                        DT.[MANUALDISCOUNTTYPE] AS MANUALDISCOUNTTYPE,
                        DT.[PERCENTAGE] AS PERCENTAGE,
                        DT.[PERIODICDISCOUNTOFFERID] AS PERIODICDISCOUNTOFFERID,
                        RPD.[NAME] AS OFFERNAME,
                        DT.[SALELINENUM] AS SALELINENUM,
                        DT.[TRANSACTIONID] AS TRANSACTIONID
                      FROM ax_RETAILTRANSACTIONDISCOUNTTRANS AS DT
                           LEFT JOIN ax_RETAILPERIODICDISCOUNT AS RPD ON RPD.OFFERID = DT.PERIODICDISCOUNTOFFERID
                      WHERE TRANSACTIONID = @TransactionId AND SALELINENUM = @SaleLineNumber";
    
                SqlQuery discountLinesQuery = new SqlQuery(discountLinesQueryStatement);
                discountLinesQuery.Parameters.Add("@TransactionId", this.request.Criteria.TransactionId);
                discountLinesQuery.Parameters.Add("@SaleLineNumber", this.request.Criteria.LineNumber);
    
                PagedResult<DiscountLine> discountLines = this.databaseContext.ReadEntity<DiscountLine>(discountLinesQuery);
                return new EntityDataServiceResponse<DiscountLine>(discountLines);
            }
        }
    }
}
