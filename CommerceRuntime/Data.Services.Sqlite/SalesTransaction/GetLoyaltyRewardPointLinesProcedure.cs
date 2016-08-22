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
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The SQLite implementation of getting loyalty reward point lines.
        /// </summary>
        internal sealed class GetLoyaltyRewardPointLinesProcedure
        {
            private GetLoyaltyRewardPointLinesDataRequest request;
            private SqliteDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetLoyaltyRewardPointLinesProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetLoyaltyRewardPointLinesProcedure(GetLoyaltyRewardPointLinesDataRequest request, SqliteDatabaseContext databaseContext)
            {
                this.request = request;
                this.databaseContext = databaseContext;
            }
    
            public EntityDataServiceResponse<LoyaltyRewardPointLine> Execute()
            {
                string queryTemplate =
                    @"SELECT
                              RTLRPT.TRANSACTIONID AS TRANSACTIONID,
                              RTLRPT.LINENUM AS LINENUM,
                              RTLRPT.AFFILIATION AS AFFILIATION,
                              RTLRPT.LOYALTYTIER AS LOYALTYTIER,
                              RTLRPT.CARDNUMBER AS CARDNUMBER,
                              RTLRPT.CUSTACCOUNT AS CUSTACCOUNT,
                              RTLRPT.ENTRYDATE AS ENTRYDATE,
                              RTLRPT.ENTRYTIME AS ENTRYTIME,
                              RTLRPT.ENTRYTYPE AS ENTRYTYPE,
                              RTLRPT.EXPIRATIONDATE AS EXPIRATIONDATE,
                              RTLRPT.REWARDPOINT AS REWARDPOINT,
                              RTLRPT.REWARDPOINTAMOUNTQTY AS REWARDPOINTAMOUNTQTY,
                              RLRP.REWARDPOINTID AS REWARDPOINTID,
                              RLRP.REWARDPOINTTYPE AS REWARDPOINTTYPE,
                              RLRP.REWARDPOINTCURRENCY AS REWARDPOINTCURRENCY,
                              RLRP.REDEEMABLE AS REWARDPOINTREDEEMABLE
                          FROM {0} AS TT
                               CROSS JOIN ax_RETAILTRANSACTIONLOYALTYREWARDPOINTTRANS AS RTLRPT ON RTLRPT.TRANSACTIONID = TT.TRANSACTIONID
                               INNER JOIN ax_RETAILLOYALTYREWARDPOINT AS RLRP ON RLRP.RECID = RTLRPT.REWARDPOINT;";
    
                using (TempTable transactionIdTempTable = TempTableHelper.CreateScalarTempTable(this.databaseContext, RetailTransactionTableSchema.TransactionIdColumn, this.request.Criteria.TransactionIds))
                {
                    SqlQuery query = new SqlQuery(string.Format(queryTemplate, transactionIdTempTable.TableName));
                    PagedResult<LoyaltyRewardPointLine> loyaltyRewardPointLines = this.databaseContext.ReadEntity<LoyaltyRewardPointLine>(query);
                    return new EntityDataServiceResponse<LoyaltyRewardPointLine>(loyaltyRewardPointLines);
                }
            }
        }
    }
}
