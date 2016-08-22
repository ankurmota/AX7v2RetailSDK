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
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The SQLite implementation of getting sales lines.
        /// </summary>
        internal sealed class GetSalesLinesProcedure
        {
            private const string LineNumColumn = "LINENUM";
    
            private GetSalesLinesDataRequest request;
            private SqliteDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetSalesLinesProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetSalesLinesProcedure(GetSalesLinesDataRequest request, SqliteDatabaseContext databaseContext)
            {
                this.request = request;
                this.databaseContext = databaseContext;
            }
    
            public EntityDataServiceResponse<SalesLine> Execute()
            {
                string queryTemplate =
                    @"SELECT
                        ST.TRANSACTIONID AS TRANSACTIONID,
                        ST.LINENUM AS LINENUM,
                        ST.ITEMID AS ITEMID,
                        ST.BARCODE AS BARCODE,
                        ST.INVENTSERIALID AS INVENTSERIALID,
                        (ST.QTY * -1.0) AS QUANTITY,
                        ST.PRICE AS PRICE,
                        ST.UNIT AS UNIT,
                        (ST.NETAMOUNT * -1.0) AS NETAMOUNT,
                        (ST.NETAMOUNTINCLTAX * -1.0) AS NETAMOUNTINCLTAX,
                        ST.TAXITEMGROUP AS ITEMTAXGROUPID,
                        ST.TAXGROUP AS SALESTAXGROUPID,
                        (ST.TAXAMOUNT * -1.0) AS TAXAMOUNT,
                        ST.DLVMODE AS DELIVERYMODE,
                        IDC.INVENTDIMID AS INVENTORYDIMENSIONID,
                        ST.INVENTLOCATIONID AS INVENTLOCATIONID,
                        ST.INVENTSITEID AS INVENTORYSITEID,
                        ST.LOGISTICSPOSTALADDRESS AS LOGISTICSPOSTALADDRESS,
                        (ST.RETURNQTY * -1.0) AS RETURNQTY,
                        ST.STORE AS STORE,
                        ST.TERMINALID AS TERMINALID,
                        ST.RETURNNOSALE AS RETURNNOSALE,
                        ST.RETURNTRANSACTIONID AS RETURNTRANSACTIONID,
                        ST.RETURNLINENUM AS RETURNLINENUM,
                        ST.RETURNSTORE AS RETURNSTORE,
                        ST.RETURNTERMINALID AS RETURNTERMINALID,
                        ST.DISCAMOUNT AS DISCOUNTAMOUNT,
                        ST.TOTALDISCAMOUNT AS TOTALDISCOUNTAMOUNT,
                        ST.TOTALDISCPCT AS TOTALDISCOUNTPERCENTAGE,
                        ST.LINEDSCAMOUNT AS LINEDISCOUNTAMOUNT,
                        ST.GIFTCARD AS GIFTCARD,
                        ST.COMMENT AS COMMENT,
                        ST.ORIGINALPRICE AS ORIGINALPRICE,
                        ST.PERIODICDISCAMOUNT AS PERIODICDISCOUNTAMOUNT,
                        ST.PERIODICPERCENTAGEDISCOUNT AS PERIODICPERCENTAGEDISCOUNT,
                        ST.LINEMANUALDISCOUNTAMOUNT AS LINEMANUALDISCOUNTAMOUNT,
                        ST.LINEMANUALDISCOUNTPERCENTAGE AS LINEMANUALDISCOUNTPERCENTAGE,
                        COALESCE(CAST(ST.LISTINGID AS BIGINT), IDC.DISTINCTPRODUCTVARIANT, IT.PRODUCT, 0)  AS LISTINGID,
                        ST.TRANSACTIONSTATUS AS TRANSACTIONSTATUS,
                        (ST.TRANSDATE + ST.TRANSTIME) AS SALESDATETIME
                      FROM {0} AS TT
                           CROSS JOIN ax_RETAILTRANSACTIONSALESTRANS AS ST ON ST.TRANSACTIONID = TT.TRANSACTIONID
                           LEFT OUTER JOIN ax_INVENTTABLE AS IT ON ST.LISTINGID = '' AND IT.DATAAREAID = ST.DATAAREAID AND ST.VARIANTID = '' AND IT.ITEMID = ST.ITEMID
                           LEFT OUTER JOIN ax_INVENTDIMCOMBINATION AS IDC ON ST.ITEMID = IDC.ITEMID AND ST.VARIANTID <> '' AND ST.VARIANTID = IDC.RETAILVARIANTID AND ST.DATAAREAID = IDC.DATAAREAID
                      ORDER BY ST.LINENUM ASC;";
    
                using (TempTable transactionIdTempTable = TempTableHelper.CreateScalarTempTable(this.databaseContext, RetailTransactionTableSchema.TransactionIdColumn, this.request.Criteria.TransactionIds))
                {
                    SqlQuery query = new SqlQuery(string.Format(queryTemplate, transactionIdTempTable.TableName));
                    PagedResult<SalesLine> salesLines = this.databaseContext.ReadEntity<SalesLine>(query);
                    return new EntityDataServiceResponse<SalesLine>(salesLines);
                }
            }
        }
    }
}
