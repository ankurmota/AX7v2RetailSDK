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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The non sales tender operations SQL server data service.
        /// </summary>
        public class NonSalesTransactionSqlServerDataService : IRequestHandler
        {
            private const string InsertNonSaleTenderSprocName = "INSERTNONSALETENDER";
            private const string RetailTransactionTableType = "RETAILTRANSACTIONTABLETYPE";
            private const string RetailTransactionPaymentTransTableType = "RETAILTRANSACTIONPAYMENTTRANSTABLETYPE";
            private const string RetailTransactionInfoCodeTransTableType = "RETAILTRANSACTIONINFOCODETRANSTABLETYPE";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(SaveNonSalesTransactionDataRequest),
                        typeof(GetCurrentShiftNonSalesTransactionsDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(SaveNonSalesTransactionDataRequest))
                {
                    response = this.SaveNonSalesTransaction((SaveNonSalesTransactionDataRequest)request);
                }
                else if (requestType == typeof(GetCurrentShiftNonSalesTransactionsDataRequest))
                {
                    response = this.GetCurrentShiftNonSalesTransactions((GetCurrentShiftNonSalesTransactionsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private EntityDataServiceResponse<NonSalesTransaction> GetCurrentShiftNonSalesTransactions(GetCurrentShiftNonSalesTransactionsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.NonSalesTransaction, "request.NonSalesTransaction");
    
                string transactionId = request.NonSalesTransactionId;
    
                if (string.IsNullOrWhiteSpace(request.NonSalesTransactionId))
                {
                    transactionId = string.Empty;
                }
    
                var transaction = request.NonSalesTransaction;
                var parameters = new ParameterSet();
    
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters["@nvc_TRANSACTIONTYPE"] = transaction.TransactionTypeValue;
                parameters["@nvc_TENDERTYPE"] = transaction.TenderTypeId;
                parameters["@nvc_BATCHTERMINALID"] = transaction.ShiftTerminalId;
                parameters["@bi_BATCHID"] = transaction.ShiftId;
                parameters["@nvc_TransactionId"] = transactionId;
    
                PagedResult<NonSalesTransaction> results;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    results = sqlServerDatabaseContext.ExecuteStoredProcedure<NonSalesTransaction>("GETCURRENTSHIFTNONSALETENDERS", parameters);
                }
    
                return new EntityDataServiceResponse<NonSalesTransaction>(results);
            }
    
            /// <summary>
            /// Save non sales transaction.
            /// </summary>
            /// <param name="request">Non sales transaction data service request.</param>
            /// <returns>A SingleEntityDataServiceResponse containing the saved NonSalesTransaction object.</returns>
            private SingleEntityDataServiceResponse<NonSalesTransaction> SaveNonSalesTransaction(SaveNonSalesTransactionDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.NonSalesTransaction, "request.NonSalesTransaction");
    
                var nonSalesTransaction = request.NonSalesTransaction;
    
                NonSalesTransaction savedTransaction = null;
    
                using (var transactionTable = new DataTable(RetailTransactionTableType))
                using (var paymentTable = new DataTable(RetailTransactionPaymentTransTableType))
                using (var reasonCodeTable = new DataTable(RetailTransactionInfoCodeTransTableType))
                {
                    TransactionLog transaction = this.Convert(nonSalesTransaction);

                    RetailTransactionTableSchema.PopulateSchema(transactionTable);

                    TransactionLogDataManager.FillData(request.RequestContext, transaction, transactionTable);

                    var parameters = new ParameterSet();
                    parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                    parameters["@tvp_Transaction"] = transactionTable;

                    if (!nonSalesTransaction.TransactionType.Equals(Microsoft.Dynamics.Commerce.Runtime.DataModel.TransactionType.OpenDrawer))
                    {
                        RetailTransactionPaymentSchema.PopulatePaymentSchema(paymentTable);
                        this.FillPaymentDataTable(nonSalesTransaction.Id, nonSalesTransaction, paymentTable, request.RequestContext);
                        parameters["@tvp_PaymentTrans"] = paymentTable;
                    }
                    else
                    {
                        RetailTransactionPaymentSchema.PopulateReasonCodeSchema(reasonCodeTable); // Populate reason code schema
                        this.FillReasonCodeLines(transaction.Id, nonSalesTransaction, reasonCodeTable, -1m, request.RequestContext); // Fill captured reason code details
                        parameters["@tvp_ReasoncodeLine"] = reasonCodeTable; // Reason code parameter for [Open Drawer] store operation
                    }

                    int errorCode;
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        savedTransaction = sqlServerDatabaseContext.ExecuteStoredProcedure<NonSalesTransaction>(InsertNonSaleTenderSprocName, parameters, out errorCode).FirstOrDefault();
                    }

                    if (errorCode != (int)DatabaseErrorCodes.Success)
                    {
                        throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save non sale tender operation.");
                    }
                }
    
                return new SingleEntityDataServiceResponse<NonSalesTransaction>(savedTransaction);
            }
    
            /// <summary>
            /// Fills the data for tender removal payment data table.
            /// </summary>
            /// <param name="dataAreaId">Data area identifier.</param>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="lineNumber">Payment row Line number.</param>
            /// <param name="transaction">Non sale tender transaction.</param>
            /// <param name="paymentTable">Data table for Payments.</param>
            /// <param name="context">Request context.</param>
            /// <returns>Returns the tender removal tender type data row.</returns>
            private DataRow FillTenderRemovalPaymentType(string dataAreaId, string transactionId, int lineNumber, NonSalesTransaction transaction, DataTable paymentTable, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(paymentTable, "paymentTable");
    
                bool isRemoveTender = transaction.TransactionType == TransactionType.RemoveTender;
                DataRow row = paymentTable.NewRow();
    
                row[RetailTransactionTableSchema.DataAreaIdColumn] = dataAreaId;
                row[RetailTransactionTableSchema.TransactionIdColumn] = transactionId;
                row[RetailTransactionPaymentSchema.LineNumColumn] = lineNumber;
                row[RetailTransactionPaymentSchema.AmountTenderedColumn] = isRemoveTender ? transaction.Amount : decimal.Negate(transaction.Amount);
                row[RetailTransactionPaymentSchema.ForeignCurrencyAmountColumn] = isRemoveTender ? transaction.AmountInForeignCurrency : decimal.Negate(transaction.AmountInForeignCurrency);
                row[RetailTransactionPaymentSchema.ForeignCurrencyExchangeRateTableColumn] = transaction.ForeignCurrencyExchangeRate;
                row[RetailTransactionPaymentSchema.CompanyCurrencyAmountColumn] = isRemoveTender ? transaction.AmountInCompanyCurrency : decimal.Negate(transaction.AmountInCompanyCurrency);
                row[RetailTransactionPaymentSchema.CompanyCurrencyExchangeRateColumn] = transaction.CompanyCurrencyExchangeRate;
                row[RetailTransactionPaymentSchema.ForeignCurrencyTableColumn] = transaction.ForeignCurrency;
                row[RetailTransactionPaymentSchema.TenderTypeTableColumn] = this.GetTenderRemovalTypeIdentifier(context);
                row[RetailTransactionTableSchema.StaffIdColumn] = transaction.StaffId;
                row[RetailTransactionTableSchema.TerminalColumn] = string.IsNullOrWhiteSpace(transaction.TerminalId) ? context.GetTerminal().TerminalId : transaction.TerminalId;
                row[RetailTransactionTableSchema.StoreColumn] = transaction.StoreId;
    
                return row;
            }
    
            /// <summary>
            /// Fills the data for non sales tender operation.
            /// </summary>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="transaction">Non sale tender transaction.</param>
            /// <param name="paymentTable">Data table for Payments.</param>
            /// <param name="context">Request context.</param>
            private void FillPaymentDataTable(string transactionId, NonSalesTransaction transaction, DataTable paymentTable, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(paymentTable, "paymentTable");
    
                int lineNumber = 1; // Line number 1 for cash tender line
                bool isRemoveTender = transaction.TransactionType == TransactionType.RemoveTender;
                DataRow row = paymentTable.NewRow();
    
                row[RetailTransactionTableSchema.TransactionIdColumn] = transactionId;
                row[RetailTransactionPaymentSchema.LineNumColumn] = lineNumber;
                row[RetailTransactionPaymentSchema.ForeignCurrencyTableColumn] = transaction.ForeignCurrency;
                row[RetailTransactionPaymentSchema.TenderTypeTableColumn] = transaction.TenderTypeId;
                row[RetailTransactionPaymentSchema.DataAreaIdColumn] = context.GetChannelConfiguration().InventLocationDataAreaId;
                row[RetailTransactionTableSchema.StaffIdColumn] = transaction.StaffId;
                row[RetailTransactionTableSchema.TerminalColumn] = string.IsNullOrWhiteSpace(transaction.TerminalId) ? context.GetTerminal().TerminalId : transaction.TerminalId;
                row[RetailTransactionTableSchema.StoreColumn] = transaction.StoreId;
                row[RetailTransactionPaymentSchema.AmountTenderedColumn] = isRemoveTender ? decimal.Negate(transaction.Amount) : transaction.Amount;
                row[RetailTransactionPaymentSchema.ForeignCurrencyAmountColumn] = isRemoveTender ? decimal.Negate(transaction.AmountInForeignCurrency) : transaction.AmountInForeignCurrency;
                row[RetailTransactionPaymentSchema.ForeignCurrencyExchangeRateTableColumn] = transaction.ForeignCurrencyExchangeRate;
                row[RetailTransactionPaymentSchema.CompanyCurrencyAmountColumn] = isRemoveTender ? decimal.Negate(transaction.AmountInCompanyCurrency) : transaction.AmountInCompanyCurrency;
                row[RetailTransactionPaymentSchema.CompanyCurrencyExchangeRateColumn] = transaction.CompanyCurrencyExchangeRate;
    
                paymentTable.Rows.Add(row);
    
                lineNumber = 2; // Line number 2 for balance tender line
    
                // Add tender removal type for every non sale tender operation.
                DataRow tenderRemovalRow = this.FillTenderRemovalPaymentType(context.GetChannelConfiguration().InventLocationDataAreaId, transactionId, lineNumber, transaction, paymentTable, context);
    
                paymentTable.Rows.Add(tenderRemovalRow);
            }

            /// <summary>
            /// Saves the reason code line.
            /// </summary>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="parentLineNumber">The parent line number.</param>
            /// <param name="context">The request context.</param>
            private void FillReasonCodeLines(string transactionId, NonSalesTransaction transaction, DataTable reasonCodeTable, decimal parentLineNumber, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(reasonCodeTable, "reasonCodeTable");

                int lineNumber = 1;
                DateTimeOffset transactionDate = context.GetNowInChannelTimeZone();

                foreach (ReasonCodeLine reasonCodeLine in transaction.ReasonCodeLines)
                {
                    DataRow row = reasonCodeTable.NewRow();
                    row[RetailTransactionPaymentSchema.TransactionIdColumn] = transactionId;
                    row[RetailTransactionPaymentSchema.TransDateColumn] = transactionDate.Date;

                    // trans time is stored as seconds (integer) in the database
                    row[RetailTransactionPaymentSchema.TransTimeColumn] = (int)transactionDate.TimeOfDay.TotalSeconds;
                    row[RetailTransactionPaymentSchema.LineNumColumn] = lineNumber;
                    row[RetailTransactionPaymentSchema.DataAreaIdColumn] = context.GetChannelConfiguration().InventLocationDataAreaId;
                    row[RetailTransactionPaymentSchema.TypeColumn] = (int)ReasonCodeLineType.NoSale;

                    row[RetailTransactionPaymentSchema.ReasonCodeIdColumn] = StringDataHelper.TruncateString(reasonCodeLine.ReasonCodeId, 10);
                    row[RetailTransactionPaymentSchema.InformationColumn] = StringDataHelper.TruncateString(reasonCodeLine.Information, 100);
                    row[RetailTransactionPaymentSchema.InformationAmountColumn] = reasonCodeLine.InformationAmount;
                    row[RetailTransactionPaymentSchema.StoreColumn] = transaction.StoreId;
                    row[RetailTransactionPaymentSchema.TerminalColumn] = transaction.TerminalId ?? string.Empty;
                    row[RetailTransactionPaymentSchema.StaffIdColumn] = StringDataHelper.TruncateString(transaction.StaffId, 25);
                    row[RetailTransactionPaymentSchema.ItemTenderColumn] = StringDataHelper.TruncateString(reasonCodeLine.ItemTender, 10);
                    row[RetailTransactionPaymentSchema.AmountColumn] = reasonCodeLine.Amount;
                    row[RetailTransactionPaymentSchema.InputTypeColumn] = (int)reasonCodeLine.InputType;
                    row[RetailTransactionPaymentSchema.SubReasonCodeIdColumn] = StringDataHelper.TruncateString(reasonCodeLine.SubReasonCodeId, 10);
                    row[RetailTransactionPaymentSchema.StatementCodeColumn] = StringDataHelper.TruncateString(reasonCodeLine.StatementCode, 25);
                    row[RetailTransactionPaymentSchema.SourceCodeColumn] = StringDataHelper.TruncateString(reasonCodeLine.SourceCode, 20);
                    row[RetailTransactionPaymentSchema.SourceCode2Column] = StringDataHelper.TruncateString(reasonCodeLine.SourceCode2, 20);
                    row[RetailTransactionPaymentSchema.SourceCode3Column] = StringDataHelper.TruncateString(reasonCodeLine.SourceCode3, 20);
                    row[RetailTransactionPaymentSchema.ParentLineNumColumn] = parentLineNumber;

                    reasonCodeTable.Rows.Add(row);
                    lineNumber++;
                }
            }

            /// <summary>
            /// Converts from non sale tender operation type to transaction log.
            /// </summary>
            /// <param name="nonSalesTransaction">Non sale tender transaction.</param>
            /// <returns>Returns the transaction log entity.</returns>
            private TransactionLog Convert(NonSalesTransaction nonSalesTransaction)
            {
                ThrowIf.NullOrWhiteSpace(nonSalesTransaction.Id, "nonSalesTransaction.Id");
    
                var transaction = new TransactionLog();
    
                transaction.Id = nonSalesTransaction.Id;
                transaction.StoreId = nonSalesTransaction.StoreId;
                transaction.TransactionType = (TransactionType)nonSalesTransaction.TransactionTypeValue;
                transaction.StaffId = nonSalesTransaction.StaffId;
                transaction.ShiftId = nonSalesTransaction.ShiftId;
                transaction.ShiftTerminalId = nonSalesTransaction.ShiftTerminalId;
                transaction.TerminalId = nonSalesTransaction.TerminalId;
                transaction.Description = nonSalesTransaction.Description;
                transaction.ChannelCurrencyExchangeRate = nonSalesTransaction.ChannelCurrencyExchangeRate;
                transaction.ChannelCurrency = nonSalesTransaction.ChannelCurrency;
    
                return transaction;
            }
    
            /// <summary>
            /// Retrieves the tender removal tender type identifier.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <returns>The tender type identifier for tender removal type.</returns>
            private string GetTenderRemovalTypeIdentifier(RequestContext context)
            {
                string tenderRemovalTypeId = "-1";  // initialize tender removal type id to -1 as EPOS does
                int function = 4;                   // initialize function to 4 as EPOS does
    
                var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var tenderTypes = context.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, context).PagedEntityCollection;
    
                if (tenderTypes != null)
                {
                    tenderRemovalTypeId = tenderTypes.Results.Where(tenderType => tenderType.Function == function).Select(tenderType => tenderType.TenderTypeId).FirstOrDefault();
                }
    
                return tenderRemovalTypeId;
            }
        }
    }
}
