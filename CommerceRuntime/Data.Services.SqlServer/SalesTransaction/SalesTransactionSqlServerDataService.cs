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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// The data request handler for sales transaction in SQLServer.
        /// </summary>
        public sealed class SalesTransactionSqlServerDataService : IRequestHandler
        {
            private const string LocalPendingTransactionsAmountSprocName = "GETCUSTOMERPENDINGTRANSACTIONSAMOUNT";
            private const string RetailTransactionSalesTransView = "RETAILTRANSACTIONSALESTRANSVIEW";

            private const string EmailColumn = "EMAIL";
            private const string ItemIdColumn = "ITEMID";
            private const string BarcodeColumn = "BARCODE";
            private const string InventSerialIdColumn = "INVENTSERIALID";
            private const string ChannelReferenceIdColumn = "CHANNELREFERENCEID";
            private const string StaffColumn = "STAFF";
            private const string ReceiptIdColumn = "RECEIPTID";
            private const string CreatedDateTimeColumn = "CREATEDDATETIME";
            private const string TransactionIdColumn = "TRANSACTIONID";
            private const string PurgeSalesTransactionsSpProcName = "PURGESALESONTERMINAL";
            private const string SearchSalesOrdersFunctionTemplate = "GETSALESORDER(@nvc_CustomerName)";
            private const string StoreColumn = "STORE";
            private const string TerminalColumn = "TERMINAL";
            private const string SaleIdColumn = "SALESID";
            private const string TypeColumn = "TYPE";
            private const string ReceiptEmailColumn = "RECEIPTEMAIL";
            private const string EntryStatusColumn = "ENTRYSTATUS";
            private const string SalesTransactionTableTypeName = "SALESTRANSACTIONTABLETYPE";
            private const string SaveSalesTransactionSprocName = "SAVESALESTRANSACTIONS";
            private const string DeleteSalesTransactionsSprocName = "DELETESALESTRANSACTIONS";
            private const string SalesTransactionTableTypeVariableName = "@tvp_SalesTransaction";
            private const string SalesTransactionIdsTableTypeVariableName = "@tvp_SalesTransactionIds";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(SaveCartDataRequest),
                    typeof(DeleteCartDataRequest),
                    typeof(InsertSalesTransactionTablesDataRequest),
                    typeof(GetSalesTransactionDataRequest),
                    typeof(GetDiscountLinesDataRequest),
                    typeof(GetLoyaltyRewardPointLinesDataRequest),
                    typeof(GetSalesLinesDataRequest),
                    typeof(UpdateReturnQuantitiesDataRequest),
                    typeof(PurgeSalesTransactionsDataRequest),
                    typeof(GetCustomerAccountLocalPendingBalanceDataRequest),
                };
                }
            }

            /// <summary>
            /// Gets the sales transaction to be saved.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");

                Response response;

                if (request is SaveCartDataRequest)
                {
                    response = SaveCart((SaveCartDataRequest)request);
                }
                else if (request is InsertSalesTransactionTablesDataRequest)
                {
                    response = InsertSalesTransactionTables((InsertSalesTransactionTablesDataRequest)request);
                }
                else if (request is DeleteCartDataRequest)
                {
                    response = DeleteCart((DeleteCartDataRequest)request);
                }
                else if (request is GetSalesTransactionDataRequest)
                {
                    response = GetSalesTransaction((GetSalesTransactionDataRequest)request);
                }
                else if (request is GetDiscountLinesDataRequest)
                {
                    response = GetDiscountLines((GetDiscountLinesDataRequest)request);
                }
                else if (request is GetLoyaltyRewardPointLinesDataRequest)
                {
                    response = GetLoyaltyRewardPointLines((GetLoyaltyRewardPointLinesDataRequest)request);
                }
                else if (request is GetSalesLinesDataRequest)
                {
                    response = GetSalesLines((GetSalesLinesDataRequest)request);
                }
                else if (request is UpdateReturnQuantitiesDataRequest)
                {
                    response = UpdateReturnQuantities((UpdateReturnQuantitiesDataRequest)request);
                }
                else if (request is PurgeSalesTransactionsDataRequest)
                {
                    response = PurgeSalesTransactions((PurgeSalesTransactionsDataRequest)request);
                }
                else if (request is GetCustomerAccountLocalPendingBalanceDataRequest)
                {
                    response = GetCustomerLocalPendingBalance((GetCustomerAccountLocalPendingBalanceDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }

                return response;
            }

            private static SingleEntityDataServiceResponse<decimal> GetCustomerLocalPendingBalance(GetCustomerAccountLocalPendingBalanceDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.AccountNumber, "request.AccountNumber");
                decimal tenderedAmount = decimal.Zero;
                decimal depositsAmount = decimal.Zero;

                ParameterSet parameters = new ParameterSet();
                parameters["@nvc_AccountNumber"] = request.AccountNumber;
                parameters["@nvc_StoreId"] = request.RequestContext.GetOrgUnit().OrgUnitNumber;
                parameters["@nvc_PosOperation"] = (int)RetailOperation.PayCustomerAccount;
                parameters["@i_LastCounter"] = request.LastReplicationCounter;
                parameters["@nvc_DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

                ParameterSet outputParameters = new ParameterSet();
                outputParameters["@d_tenderAmount"] = tenderedAmount;
                outputParameters["@d_depositAmount"] = depositsAmount;

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                {
                    databaseContext.ExecuteStoredProcedureScalar(LocalPendingTransactionsAmountSprocName, parameters, outputParameters);
                }

                tenderedAmount = Convert.ToDecimal(outputParameters["@d_tenderAmount"]);
                depositsAmount = Convert.ToDecimal(outputParameters["@d_depositAmount"]);

                return new SingleEntityDataServiceResponse<decimal>(tenderedAmount - depositsAmount);
            }

            private static NullResponse PurgeSalesTransactions(PurgeSalesTransactionsDataRequest request)
            {
                ThrowIf.Null(request, "request");

                if (request.RetentionDays > 0)
                {
                    try
                    {
                        var parameters = new ParameterSet
                    {
                        { "@bi_ChannelId", request.ChannelId },
                        { "@vc_TerminalId", request.TerminalId },
                        { "@i_RetentionDays", request.RetentionDays },
                        { "@f_PurgeOrder", 1 } // purge customer orders first
                    };

                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                        {
                            databaseContext.ExecuteStoredProcedureNonQuery(PurgeSalesTransactionsSpProcName, parameters);
                        }

                        // now purge retail transactions
                        parameters["@f_PurgeOrder"] = 0;

                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                        {
                            databaseContext.ExecuteStoredProcedureNonQuery(PurgeSalesTransactionsSpProcName, parameters);
                        }
                    }
                    catch (Exception exception)
                    {
                        RetailLogger.Log.CrtDataAccessPurgeSalesTransactionFailure(exception);
                    }
                }

                return new NullResponse();
            }

            private static NullResponse UpdateReturnQuantities(UpdateReturnQuantitiesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.SalesLines, "request.SalesLines");

                foreach (var salesline in request.SalesLines)
                {
                    if (salesline.IsReturnByReceipt)
                    {
                        ParameterSet parameters = new ParameterSet();
                        parameters[DatabaseAccessor.ChannelIdVariableName] = salesline.ReturnChannelId;
                        parameters["@nvc_StoreNumber"] = salesline.ReturnStore;
                        parameters["@nvc_TerminalId"] = salesline.ReturnTerminalId;
                        parameters["@nvc_TransactionId"] = salesline.ReturnTransactionId;
                        parameters["@nu_LineNumber"] = salesline.ReturnLineNumber;
                        parameters["@nu_Quantity"] = salesline.Quantity;

                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                        {
                            databaseContext.ExecuteStoredProcedureNonQuery("UPDATERETURNQUANTITY", parameters);
                        }
                    }
                }

                return new NullResponse();
            }

            private static EntityDataServiceResponse<SalesOrder> GetSalesTransaction(GetSalesTransactionDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
                ThrowIf.Null(request.SearchCriteria, "request.SearchCriteria");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = SearchSalesOrdersFunctionTemplate,
                };

                if (request.QueryResultSettings.Sorting == null || request.QueryResultSettings.Sorting.Count == 0)
                {
                    query.OrderBy = new SortingInfo(CreatedDateTimeColumn, true).ToString();
                }

                var whereClauses = new List<string>();

                BuildWhereClauseForItemIdBarcodeSerialNumber(request.SearchCriteria, query, whereClauses, request.RequestContext.Runtime.Configuration.DatabaseProvider.GetDatabaseQueryBuilder());
                BuildSearchOrderWhereClause(request.SearchCriteria, query, whereClauses);

                query.Where = string.Join(" AND ", whereClauses);

                string customerName = "\"\"";

                if (!string.IsNullOrEmpty(request.SearchCriteria.CustomerFirstName) || !string.IsNullOrEmpty(request.SearchCriteria.CustomerLastName))
                {
                    customerName = request.SearchCriteria.CustomerFirstName == null ? "\"" + request.SearchCriteria.CustomerLastName + "\"" : "\"" + request.SearchCriteria.CustomerFirstName + "*" + (request.SearchCriteria.CustomerLastName ?? string.Empty) + "\"";
                }

                query.Parameters["@nvc_CustomerName"] = customerName;

                PagedResult<SalesOrder> transactions;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    transactions = sqlServerDatabaseContext.ReadEntity<SalesOrder>(query);
                }

                return new EntityDataServiceResponse<SalesOrder>(transactions);
            }

            /// <summary>
            /// Builds the WHERE clause from the search criteria for Orders.
            /// The result is the AND of the following non-empty parameters for the RetailTransactionView: ReceiptId, CustomerAccountNumber, FirstName, LastName, Store, Terminal, StaffId, StartDateTime, EndDateTime
            /// and the following non-empty parameters for the RetailTransactionSalesTransView: ItemId, Barcode.
            /// </summary>
            /// <param name="criteria">Search criteria.</param>
            /// <param name="query">The SQL query.</param>
            /// <param name="whereClauses">Where clauses to build.</param>
            private static void BuildSearchOrderWhereClause(SalesOrderSearchCriteria criteria, SqlPagedQuery query, IList<string> whereClauses)
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

            private static void BuildWhereClauseForItemIdBarcodeSerialNumber(SalesOrderSearchCriteria criteria, SqlPagedQuery query, IList<string> whereClauses, IDatabaseQueryBuilder databaseQueryBuilder)
            {
                // If an ItemId, Barcode or SerialNumber was specified we need to check whether a Sales Line exists that meets the criteria
                if (!string.IsNullOrEmpty(criteria.ItemId) || !string.IsNullOrEmpty(criteria.Barcode) || !string.IsNullOrEmpty(criteria.SerialNumber))
                {
                    if (!string.IsNullOrEmpty(criteria.ItemId))
                    {
                        whereClauses.Add(string.Format("{0} = @itemId", ItemIdColumn));
                        query.Parameters["@itemId"] = criteria.ItemId.Trim();
                    }

                    if (!string.IsNullOrEmpty(criteria.Barcode))
                    {
                        whereClauses.Add(string.Format("{0} = @barcode", BarcodeColumn));
                        query.Parameters["@barcode"] = criteria.Barcode.Trim();
                    }

                    if (!string.IsNullOrEmpty(criteria.SerialNumber))
                    {
                        whereClauses.Add(string.Format("{0} = @serial", InventSerialIdColumn));
                        query.Parameters["@serial"] = criteria.SerialNumber.Trim();
                    }

                    var salesLineClause = string.Join(" AND ", whereClauses);
                    whereClauses.Clear();

                    var existsQuery = new SqlPagedQuery(QueryResultSettings.AllRecords)
                    {
                        Select = new ColumnSet(TransactionIdColumn),
                        From = RetailTransactionSalesTransView,
                        Where = salesLineClause
                    };

                    string existsQuerySql = existsQuery.BuildQuery(databaseQueryBuilder);

                    whereClauses.Add(string.Format("{0} IN ({1})", TransactionIdColumn, existsQuerySql));
                }
            }

            private static NullResponse DeleteCart(DeleteCartDataRequest request)
            {
                ThrowIf.Null(request, "request");

                int errorCode;

                using (StringIdTableType transactionIdsTableType = new StringIdTableType(request.SalesTransactionIds, string.Empty))
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters[SalesTransactionIdsTableTypeVariableName] = transactionIdsTableType;

                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        errorCode = sqlServerDatabaseContext.ExecuteStoredProcedureNonQuery(DeleteSalesTransactionsSprocName, parameters);
                    }
                }

                if (errorCode != (int)DatabaseErrorCodes.Success)
                {
                    if (errorCode == (int)DatabaseErrorCodes.AuthorizationError)
                    {
                        throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "One or more transactions could not be deleted.");
                    }

                    throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to delete transactions.");
                }

                return new NullResponse();
            }

            private static NullResponse InsertSalesTransactionTables(InsertSalesTransactionTablesDataRequest request)
            {
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters["@tvp_Transaction"] = request.TransactionTable;
                parameters["@tvp_SalesTrans"] = request.LinesTable;
                parameters["@tvp_IncomeExpense"] = request.IncomeExpenseTable;
                parameters["@tvp_MarkupTrans"] = request.MarkupTable;
                parameters["@tvp_PaymentTrans"] = request.PaymentTable;
                parameters["@tvp_TaxTrans"] = request.TaxTable;
                parameters["@tvp_AttributeTrans"] = request.AttributeTable;
                parameters["@tvp_AddressTrans"] = request.AddressTable;
                parameters["@tvp_DiscountTrans"] = request.DiscountTable;
                parameters["@tvp_InfoCodeTrans"] = request.ReasonCodeTable;
                parameters["@tvp_SalesProperties"] = request.PropertiesTable;
                parameters["@tvp_RewardPointTrans"] = request.RewardPointTable;
                parameters["@tvp_AffiliationTrans"] = request.AffiliationsTable;
                parameters["@tvp_CustomerOrderTrans"] = request.CustomerOrderTable;
                parameters["@tvp_InvoiceTrans"] = request.InvoiceTable;
                parameters["@tvp_CustomerAccountDepositTrans"] = request.CustomerAccountDepositTable;

                int errorCode;

                using (var databaseContext = new SqlServerDatabaseContext(request))
                {
                    errorCode = databaseContext.ExecuteStoredProcedureNonQuery("INSERTSALESORDER", parameters);
                }

                if (errorCode != (int)DatabaseErrorCodes.Success)
                {
                    throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save sales order.");
                }

                return new NullResponse();
            }

            private static NullResponse SaveCart(SaveCartDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.SalesTransactions, "request.SalesTransactions");
                
                var salesTransactionDataCollection = SalesTransactionConverter.ConvertToData(request.SalesTransactions, serializeData: true);

                using (DataTable table = new DataTable(SalesTransactionTableTypeName))
                {
                    SalesTransactionData.FillSchemaForSave(table);
                    foreach (SalesTransactionData salesTransactionData in salesTransactionDataCollection)
                    {
                        table.Rows.Add(salesTransactionData.CreateDataRow(table));
                    }

                    ParameterSet parameters = new ParameterSet();
                    parameters[SalesTransactionTableTypeVariableName] = table;
                    parameters["@b_IgnoreRowVersionCheck"] = request.IgnoreRowVersionCheck;

                    try
                    {
                        int errorCode;
                        using (var databaseContext = new SqlServerDatabaseContext(request))
                        {
                            errorCode = databaseContext.ExecuteStoredProcedureNonQuery(SaveSalesTransactionSprocName, parameters);
                        }

                        if (errorCode == (int)DatabaseErrorCodes.VersionMismatchError)
                        {
                            throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectVersionMismatchError, errorCode, "Version mismatch while saving transaction.");
                        }

                        if (errorCode != (int)DatabaseErrorCodes.Success)
                        {
                            throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save transactions.");
                        }
                    }
                    catch (DatabaseException databaseException)
                    {
                        throw new StorageException(
                            StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError,
                            (int)databaseException.ErrorCode,
                            databaseException,
                            "Unable to save transactions.");
                    }
                }

                return new NullResponse();
            }

            private static EntityDataServiceResponse<DiscountLine> GetDiscountLines(GetDiscountLinesDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    GetDiscountLinesProcedure getDiscountLinesProcedure = new GetDiscountLinesProcedure(request, databaseContext);
                    return getDiscountLinesProcedure.Execute();
                }
            }

            private static EntityDataServiceResponse<LoyaltyRewardPointLine> GetLoyaltyRewardPointLines(GetLoyaltyRewardPointLinesDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    GetLoyaltyRewardPointLinesProcedure getLoyaltyRewardPointLinesProcedure = new GetLoyaltyRewardPointLinesProcedure(request, databaseContext);
                    return getLoyaltyRewardPointLinesProcedure.Execute();
                }
            }

            private static EntityDataServiceResponse<SalesLine> GetSalesLines(GetSalesLinesDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    GetSalesLinesProcedure getSalesLinesProcedure = new GetSalesLinesProcedure(request, databaseContext);
                    return getSalesLinesProcedure.Execute();
                }
            }
        }
    }
}