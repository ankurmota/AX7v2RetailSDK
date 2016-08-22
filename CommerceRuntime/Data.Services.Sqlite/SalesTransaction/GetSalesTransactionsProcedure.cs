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
    namespace Commerce.Runtime.DataServices.Sqlite.DataServices.SalesTransaction
    {
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class to get sales transactions.
        /// </summary>
        internal sealed class GetSalesTransactionsProcedure
        {
            private const string CreatedDateTimeColumn = "CREATEDDATETIME";
            private const string SaleIdColumn = "SALESID";
            private const string StoreColumn = "STORE";
            private const string TerminalColumn = "TERMINAL";
            private const string TypeColumn = "TYPE";
            private const string ReceiptEmailColumn = "RECEIPTEMAIL";
            private const string EntryStatusColumn = "ENTRYSTATUS";
            private const string EmailColumn = "EMAIL";
            private const string ChannelReferenceIdColumn = "CHANNELREFERENCEID";
            private const string StaffColumn = "STAFF";
            private const string ReceiptIdColumn = "RECEIPTID";
            private const string TransactionIdColumn = "TRANSACTIONID";
    
            private GetSalesTransactionDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetSalesTransactionsProcedure"/> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            public GetSalesTransactionsProcedure(GetSalesTransactionDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            /// <returns>The response message.</returns>
            public EntityDataServiceResponse<SalesOrder> Execute()
            {
                PagedResult<SalesOrder> transactions = this.GetTransactions();
                return new EntityDataServiceResponse<SalesOrder>(transactions);
            }
    
            private static string ComposeCustomerNameForSearch(SalesOrderSearchCriteria criteria)
            {
                string customerName = null;
    
                if (!string.IsNullOrWhiteSpace(criteria.CustomerFirstName) || !string.IsNullOrWhiteSpace(criteria.CustomerLastName))
                {
                    customerName = string.IsNullOrWhiteSpace(criteria.CustomerFirstName)
                        ? criteria.CustomerLastName
                        : criteria.CustomerFirstName + "%" + (criteria.CustomerLastName ?? string.Empty);
    
                    customerName = string.Format("%{0}%", customerName);
                }
    
                return customerName;
            }
    
            private PagedResult<SalesOrder> GetTransactions()
            {
                const string SelectSalesTransactions = @"
                    (
                        SELECT
                            RT.CHANNEL AS 'CHANNELID'
                            ,RT.TRANSACTIONID AS 'TRANSACTIONID'
                            ,CASE
                                    WHEN (RT.SALESORDERID <> '') THEN RT.SALESORDERID
                                    ELSE COALESCE(OS.SALESID, '')
                            END AS 'SALESID'
                            ,RT.CREATEDDATETIME AS 'CREATEDDATETIME'
                            ,(RT.GROSSAMOUNT * -1.0) AS 'GROSSAMOUNT'
                            ,CASE WHEN RT.TYPE = 7 THEN    -- TYPE = 7: TenderDeclaration
    			                    (
    				                    SELECT SUM(TD.AMOUNTTENDERED)
    				                    FROM ax_RETAILTRANSACTIONTENDERDECLARATIONTRANS TD
    				                    WHERE
    					                    TD.TRANSACTIONID = RT.TRANSACTIONID
    					                    AND TD.TERMINAL = RT.TERMINAL
    					                    AND TD.DATAAREAID = RT.DATAAREAID
    					                    AND TD.TRANSACTIONSTATUS = 0    -- 0: Normal status transaction
    					                    AND TD.CHANNEL = RT.CHANNEL
    			                    )
    		                    WHEN RT.TYPE = 16 THEN    -- TYPE = 16: BankDrop
    			                    (
    				                    SELECT SUM(B.AMOUNTTENDERED)
    				                    FROM ax_RETAILTRANSACTIONBANKEDTENDERTRANS B
    				                    WHERE
    					                    B.TRANSACTIONID = RT.TRANSACTIONID
    					                    AND B.TERMINAL = RT.TERMINAL
    					                    AND B.DATAAREAID = RT.DATAAREAID
    					                    AND B.TRANSACTIONSTATUS = 0    -- 0: Normal status transaction
    					                    AND B.CHANNEL = RT.CHANNEL
    			                    )
    		                    WHEN RT.TYPE = 17 THEN    -- TYPE = 17: SafeDrop
    			                    (
    				                    SELECT SUM(S.AMOUNTTENDERED)
    				                    FROM ax_RETAILTRANSACTIONSAFETENDERTRANS S
    				                    WHERE
    					                    S.TRANSACTIONID = RT.TRANSACTIONID
    					                    AND S.TERMINAL = RT.TERMINAL
    					                    AND S.DATAAREAID = RT.DATAAREAID
    					                    AND S.TRANSACTIONSTATUS = 0    -- 0: Normal status transaction
    					                    AND S.CHANNEL = RT.CHANNEL
    			                    )
    		                    WHEN RT.TYPE = 3 THEN    -- TYPE = 3: Payment
    			                    (
    				                    SELECT SUM(P.AMOUNTTENDERED)
    				                    FROM ax_RETAILTRANSACTIONPAYMENTTRANS P
    				                    WHERE
    					                    P.TRANSACTIONID = RT.TRANSACTIONID
    					                    AND P.TERMINAL = RT.TERMINAL
    					                    AND P.DATAAREAID  = RT.DATAAREAID
    					                    AND P.TRANSACTIONSTATUS = 0    -- 0: Normal status transaction
    					                    AND P.CHANNEL = RT.CHANNEL
    			                    )
    		                    ELSE (RT.GROSSAMOUNT * -1.0) END AS 'TOTALAMOUNT'
                            ,RT.PAYMENTAMOUNT AS 'AMOUNTPAID'
                            ,RT.TERMINAL AS 'TERMINAL'
                            ,RT.STORE AS 'STORE'
                            ,RT.STAFF AS 'STAFF'
                            ,RT.CUSTACCOUNT AS 'CUSTOMERID'
                            ,RT.RECEIPTEMAIL AS 'RECEIPTEMAIL'
                            ,RT.TYPE
                            ,RT.DISCAMOUNT AS 'DISCOUNTAMOUNT'
                            ,IFNULL(AT.DELIVERYNAME,DPT.NAME) AS 'NAME'
                            ,COALESCE((
                                SELECT SUM(TAXAMOUNT * -1.0)
                                FROM ax_RETAILTRANSACTIONSALESTRANS RTST
                                WHERE
                                    RTST.TRANSACTIONID = RT.TRANSACTIONID
                                    AND RTST.TERMINALID = RT.TERMINAL
                                    AND RTST.STORE = RT.STORE
                                    AND RTST.CHANNEL = RT.CHANNEL
    			                    AND RTST.TRANSACTIONSTATUS <> 1		-- Excludes the voided line
    			                    ), 0) AS 'TAXAMOUNT'
    	                    ,COALESCE((
                                SELECT ISINCLUDEDINPRICE
                                FROM ax_RETAILTRANSACTIONTAXTRANS TT
                                WHERE
                                    TT.TRANSACTIONID = RT.TRANSACTIONID
                                    AND TT.TERMINALID = RT.TERMINAL
                                    AND TT.STOREID = RT.STORE
                                    AND TT.CHANNEL = RT.CHANNEL LIMIT 1 ), 0) AS 'ISTAXINCLUDEDINPRICE'
                            ,COALESCE((
                                SELECT SUM(CALCULATEDAMOUNT)
                                FROM ax_RETAILTRANSACTIONMARKUPTRANS MT
                                WHERE
                                    MT.TRANSACTIONID = RT.TRANSACTIONID
                                    AND MT.TERMINALID = RT.TERMINAL
                                    AND MT.STORE = RT.STORE
                                    AND MT.CHANNEL = RT.CHANNEL), 0) AS 'CHARGEAMOUNT'
                            ,RT.CHANNELREFERENCEID AS 'CHANNELREFERENCEID'
                            ,RT.INVENTLOCATIONID AS 'INVENTLOCATIONID'
                            ,RT.INVENTSITEID AS 'INVENTSITEID'
                            ,RT.DLVMODE AS 'DELIVERYMODE'
                            ,RT.SHIPPINGDATEREQUESTED AS 'REQUESTEDDELIVERYDATE'
                            ,RT.LOGISTICSPOSTALADDRESS AS 'LOGISTICSPOSTALADDRESS'
                            ,RT.RECEIPTID AS 'RECEIPTID'
                            ,RT.LOYALTYCARDID AS 'LOYALTYCARDID'
                            ,RT.ENTRYSTATUS AS 'ENTRYSTATUS'
    	                    ,CASE
    		                        WHEN (AT.EMAIL IS NOT NULL AND AT.EMAIL <> '') THEN AT.EMAIL
                                    WHEN (RT.RECEIPTEMAIL IS NOT NULL AND RT.RECEIPTEMAIL <> '') THEN RT.RECEIPTEMAIL
                                    ELSE CT.EMAIL
                                END  AS 'EMAIL'
                            ,IFNULL(AT.PHONE,'') AS 'PHONE'
                            ,IFNULL(OS.STATUS, 0) AS 'STATUS'
                            ,RT.TRANSTIME + RT.TRANSDATE AS 'BEGINDATETIME'
                            ,RT.BUSINESSDATE
                            ,RT.STATEMENTCODE
                            ,RT.COMMENT AS 'COMMENT'
                            ,RT.INVOICECOMMENT AS 'INVOICECOMMENT'
                            ,RT.CREATEDOFFLINE AS 'ISCREATEDOFFLINE'
                            ,CO.CANCELLATIONCHARGE AS 'CANCELLATIONCHARGE'
                            ,CO.DEPOSITOVERRIDE AS 'DEPOSITOVERRIDE'
                            ,CO.REQUIREDDEPOSIT AS 'REQUIREDDEPOSIT'
                            ,CO.CALCULATEDDEPOSIT AS 'CALCULATEDDEPOSIT'
                            ,CO.PREPAYMENTPAID AS 'PREPAYMENTPAID'
                            ,CO.PREPAYMENTINVOICED AS 'PREPAYMENTINVOICED'
    	                    ,IFNULL((
    			                    SELECT 1 AS 'HASREDEEMLINE' FROM ax_RETAILTRANSACTIONLOYALTYREWARDPOINTTRANS RPT
    			                    WHERE RPT.DATAAREAID = RT.DATAAREAID
    			                    AND RPT.TRANSACTIONID = RT.TRANSACTIONID
                                    AND RPT.TERMINALID = RT.TERMINAL
                                    AND RPT.STOREID = RT.STORE
                                    AND RPT.CHANNEL = RT.CHANNEL
    			                    AND RPT.ENTRYTYPE = 2 LIMIT 1), 0) AS 'HASLOYALTYPAYMENT' -- EntryType 2 is Redeem. HASLOYALTYPAYMENT = 1 if true, 0 if false.
                    FROM ax_RETAILTRANSACTIONTABLE RT
                    LEFT JOIN ax_RETAILTRANSACTIONORDERSTATUS OS ON
                        OS.TRANSACTIONID = RT.TRANSACTIONID
                        AND OS.TERMINAL = RT.TERMINAL
                        AND OS.STORE = RT.STORE
                        AND OS.CHANNEL = RT.CHANNEL
                    LEFT JOIN ax_RETAILTRANSACTIONADDRESSTRANS AT ON
                        AT.TRANSACTIONID = RT.TRANSACTIONID
                        AND AT.TERMINAL = RT.TERMINAL
                        AND AT.STORE = RT.STORE
                        AND AT.CHANNEL = RT.CHANNEL
                        AND AT.SALELINENUM = 0      -- SaleLineNum 0 = Header level address
                    LEFT JOIN crt_CUSTOMERORDERTRANSACTION CO ON
                        CO.CHANNEL = RT.CHANNEL
                        AND CO.STORE = RT.STORE
                        AND CO.TERMINAL = RT.TERMINAL
                        AND CO.TRANSACTIONID = RT.TRANSACTIONID
                        AND CO.DATAAREAID = RT.DATAAREAID
                    LEFT JOIN crt_CUSTOMERSVIEW CT ON CT.ACCOUNTNUMBER = RT.CUSTACCOUNT
                        AND CT.DATAAREAID = RT.DATAAREAID
                    LEFT JOIN ax_DIRPARTYTABLE DPT ON CT.PARTY = DPT.RECID
                    WHERE
                        @customerName IS NULL OR (AT.DELIVERYNAME LIKE @customerName OR DPT.NAME LIKE @customerName)
                )
    ";
                var settings = this.request.QueryResultSettings;
    
                SortingInfo sorting = (settings.Sorting == null || settings.Sorting.Count == 0)
                    ? new SortingInfo(CreatedDateTimeColumn, true)
                    : settings.Sorting;
    
                var query = new SqlPagedQuery(settings)
                {
                    From = SelectSalesTransactions,
                    OrderBy = sorting.ToString(),
                    DatabaseSchema = string.Empty,
                    Aliased = true
                };
    
                query.Parameters["@customerName"] = ComposeCustomerNameForSearch(this.request.SearchCriteria);
    
                var whereClauses = new List<string>();
                this.BuildSearchOrderWhereClause(this.request.SearchCriteria, query, whereClauses);
    
                query.Where = string.Join(" AND ", whereClauses);
    
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    return databaseContext.ReadEntity<SalesOrder>(query);
                }
            }
    
            /// <summary>
            /// Builds the WHERE clause from the search criteria for Orders.
            /// The result is the AND of the following non-empty parameters for the RetailTransactionView: ReceiptId, CustomerAccountNumber, FirstName, LastName, Store, Terminal, StaffId, StartDateTime, EndDateTime
            /// and the following non-empty parameters for the RetailTransactionSalesTransView: ItemId, Barcode.
            /// </summary>
            /// <param name="criteria">Search criteria.</param>
            /// <param name="query">The SQL query.</param>
            /// <param name="whereClauses">Where clauses to build.</param>
            private void BuildSearchOrderWhereClause(SalesOrderSearchCriteria criteria, SqlPagedQuery query, IList<string> whereClauses)
            {
                ThrowIf.Null(criteria, "criteria");
                ThrowIf.Null(query, "query");
                ThrowIf.Null(whereClauses, "whereClauses");
    
                if (!string.IsNullOrEmpty(criteria.SalesId))
                {
                    whereClauses.Add(string.Format("{0} = @saleId", SaleIdColumn));
                    query.Parameters["@saleId"] = criteria.SalesId.Trim();
                }
    
                if (!string.IsNullOrEmpty(criteria.ReceiptId))
                {
                    whereClauses.Add(string.Format("{0} = @receiptId", ReceiptIdColumn));
                    query.Parameters["@receiptId"] = criteria.ReceiptId.Trim();
                }
    
                if (!string.IsNullOrEmpty(criteria.ChannelReferenceId))
                {
                    whereClauses.Add(string.Format("{0} = @channelReferenceId", ChannelReferenceIdColumn));
                    query.Parameters["@channelReferenceId"] = criteria.ChannelReferenceId.Trim();
                }
    
                if (!string.IsNullOrEmpty(criteria.CustomerAccountNumber))
                {
                    whereClauses.Add(string.Format("{0} = @custAccount", RetailTransactionTableSchema.CustomerIdColumn));
                    query.Parameters["@custAccount"] = criteria.CustomerAccountNumber.Trim();
                }
    
                if (!string.IsNullOrEmpty(criteria.StoreId))
                {
                    whereClauses.Add(string.Format("{0} = @storeId", StoreColumn));
                    query.Parameters["@storeId"] = criteria.StoreId.Trim();
                }
    
                if (!string.IsNullOrEmpty(criteria.TerminalId))
                {
                    whereClauses.Add(string.Format("{0} = @terminalId", TerminalColumn));
                    query.Parameters["@terminalId"] = criteria.TerminalId.Trim();
                }
    
                if (!string.IsNullOrEmpty(criteria.StaffId))
                {
                    whereClauses.Add(string.Format("{0} = @staffId", StaffColumn));
                    query.Parameters["@staffId"] = criteria.StaffId.Trim();
                }
    
                if (criteria.StartDateTime != null)
                {
                    whereClauses.Add(string.Format("{0} >= @startDate", CreatedDateTimeColumn));
                    query.Parameters["@startDate"] = criteria.StartDateTime.Value.UtcDateTime;
                }
    
                if (criteria.EndDateTime != null)
                {
                    whereClauses.Add(string.Format("{0} <= @endDate", CreatedDateTimeColumn));
                    whereClauses.Add(string.Format("{0} < @endDate", CreatedDateTimeColumn));
                    query.Parameters["@endDate"] = criteria.EndDateTime.Value.UtcDateTime;
                }
    
                if (!criteria.SalesTransactionTypes.IsNullOrEmpty() && criteria.SalesTransactionTypes.All(transactionType => transactionType != SalesTransactionType.None))
                {
                    query.AddInClause<SalesTransactionType>(criteria.SalesTransactionTypes.AsReadOnly(), TypeColumn, whereClauses);
                }
    
                if (!criteria.TransactionStatusTypes.IsNullOrEmpty())
                {
                    query.AddInClause<TransactionStatus>(criteria.TransactionStatusTypes.AsReadOnly(), EntryStatusColumn, whereClauses);
                }
    
                if (criteria.TransactionIds != null && criteria.TransactionIds.Any())
                {
                    query.AddInClause<string>(criteria.TransactionIds.AsReadOnly(), TransactionIdColumn, whereClauses);
                }
    
                if (!string.IsNullOrEmpty(criteria.ReceiptEmailAddress))
                {
                    whereClauses.Add(string.Format("({0} = @receiptEmailAddress OR {1} = @receiptEmailAddress)", ReceiptEmailColumn, EmailColumn));
                    query.Parameters["@receiptEmailAddress"] = criteria.ReceiptEmailAddress.Trim();
                }
    
                if (!string.IsNullOrEmpty(criteria.SearchIdentifiers))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("({0} = @searchIdentifiers", TransactionIdColumn);
                    sb.AppendFormat(" OR {0} = @searchIdentifiers", ReceiptIdColumn);
                    sb.AppendFormat(" OR {0} = @searchIdentifiers", SaleIdColumn);
                    sb.AppendFormat(" OR {0} = @searchIdentifiers", RetailTransactionTableSchema.CustomerIdColumn);
                    sb.AppendFormat(" OR {0} = @searchIdentifiers)", ChannelReferenceIdColumn);
    
                    whereClauses.Add(sb.ToString());
                    query.Parameters["@searchIdentifiers"] = criteria.SearchIdentifiers.Trim();
                }
    
                query.Where = string.Join(" AND ", whereClauses);
            }
        }
    }
}
