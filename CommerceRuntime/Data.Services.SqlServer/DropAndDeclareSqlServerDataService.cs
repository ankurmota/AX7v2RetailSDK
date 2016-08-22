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
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The tender drop and declare operations SQL server data service.
        /// </summary>
        public class DropAndDeclareSqlServerDataService : IRequestHandler
        {
            private const string DropAndDeclareTransactionsView = "DROPANDDECLARETRANSACTIONSVIEW";
            private const string DropAndDeclareTendersView = "DROPANDDECLARETENDERSVIEW";
    
            private const string AmountCurColumn = "AMOUNTCUR";
            private const string AmountMSTColumn = "AMOUNTMST";
            private const string AmountTenderedColumn = "AMOUNTTENDERED";
            private const string CurrencyColumn = "CURRENCY";
            private const string ExchangeColumn = "EXCHRATE";
            private const string ExchangeMSTColumn = "EXCHRATEMST";
            private const string LineNumColumn = "LINENUM";
            private const string PosCurrencyColumn = "POSCURRENCY";
            private const string QTYColumn = "QTY";
            private const string StaffColumn = "STAFF";
            private const string StoreColumn = "STORE";
            private const string TenderTypeColumn = "TENDERTYPE";
            private const string TerminalColumn = "TERMINAL";
            private const string TransactionIDColumn = "TRANSACTIONID";
            private const string TransTimeColumn = "TRANSTIME";
            private const string DataAreaIdColumn = "DATAAREAID";
    
            // BankDrop, SafeDrop
            private const string BankBagNumColumn = "NANKBAGNUM";
            private const string AmountCurPOSColumn = "AMOUNTCURPOS";
            private const string AmountTenderedPOSColumn = "AMOUNTTENDEREDPOS";
            private const string AmountMSTPOSColumn = "AMOUNTMSTPOS";
            private const string StatusTypeColumn = "STATUSTYPE";
            private const string TypeColumn = "TYPE";

            // Reason code details
            private const string ReasonCodeIdColumn = "INFOCODEID";
            private const string InformationColumn = "INFORMATION";
            private const string InformationAmountColumn = "INFOAMOUNT";
            private const string ItemTenderColumn = "ITEMTENDER";
            private const string AmountColumn = "AMOUNT";
            private const string InputTypeColumn = "INPUTTYPE";
            private const string SubReasonCodeIdColumn = "SUBINFOCODEID";
            private const string SourceCodeColumn = "SOURCECODE";
            private const string SourceCode2Column = "SOURCECODE2";
            private const string SourceCode3Column = "SOURCECODE3";
            private const string ParentLineNumColumn = "PARENTLINENUM";
            private const string TransDateColumn = "TRANSDATE";
            private const string StatementCodeColumn = "STATEMENTCODE";

            private const string InsertTenderDropAndDeclareSprocName = "INSERTTENDERDROPANDDECLARE";
            private const string RetailTransactionTableType = "RETAILTRANSACTIONTABLETYPE";
            private const string TenderDeclarationTransType = "TENDERDECLARATIONTRANSTYPE";
            private const string TenderDropTransType = "TENDERDROPTRANSTYPE";
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
                        typeof(SaveDropAndDeclareTransactionDataRequest),
                        typeof(GetDropAndDeclareTransactionDataRequest),
                        typeof(GetDropAndDeclareTransactionTenderDetailsDataRequest),
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
    
                if (requestType == typeof(SaveDropAndDeclareTransactionDataRequest))
                {
                    response = this.SaveDropAndDeclareTransaction((SaveDropAndDeclareTransactionDataRequest)request);
                }
                else if (requestType == typeof(GetDropAndDeclareTransactionDataRequest))
                {
                    response = this.GetDropAndDeclareTransactions((GetDropAndDeclareTransactionDataRequest)request);
                }
                else if (requestType == typeof(GetDropAndDeclareTransactionTenderDetailsDataRequest))
                {
                    response = this.GetDropAndDeclareTransactionTenderDetails((GetDropAndDeclareTransactionTenderDetailsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Save drop and declare transaction.
            /// </summary>
            /// <param name="request">Drop and declare transaction data service request.</param>
            /// <returns>A SingleEntityDataServiceResponse containing the saved DropAndDeclareTransaction object.</returns>
            private SingleEntityDataServiceResponse<DropAndDeclareTransaction> SaveDropAndDeclareTransaction(SaveDropAndDeclareTransactionDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.DropAndDeclareTransaction, "request.DropAndDeclareTransaction");
    
                DropAndDeclareTransaction dropAndDeclareTransaction = request.DropAndDeclareTransaction;
    
                Tuple<PagedResult<DropAndDeclareTransaction>, ReadOnlyCollection<TenderDetail>> results;
    
                using (var transactionTable = new DataTable(RetailTransactionTableType))
                using (var tenderDeclarationTable = new DataTable(TenderDeclarationTransType))
                using (var tenderDropTable = new DataTable(TenderDropTransType))
                using (var reasonCodeTable = new DataTable(RetailTransactionInfoCodeTransTableType))
                {
                    TransactionLog transaction = this.Convert(dropAndDeclareTransaction);

                    RetailTransactionTableSchema.PopulateSchema(transactionTable);
                    TenderDropAndDeclareTableSchema.PopulateTenderDeclarationSchema(tenderDeclarationTable);
                    TenderDropAndDeclareTableSchema.PopulateBankTenderDropSchema(tenderDropTable);
                    TenderDropAndDeclareTableSchema.PopulateReasonCodeSchema(reasonCodeTable);

                    // Fill the transaction header details.
                    TransactionLogDataManager.FillData(request.RequestContext, transaction, transactionTable);

                    // File the transaction line details.
                    if (dropAndDeclareTransaction.TransactionType == TransactionType.BankDrop ||
                        dropAndDeclareTransaction.TransactionType == TransactionType.SafeDrop)
                    {
                        this.FillTenderDataTable(transaction.Id, dropAndDeclareTransaction, tenderDropTable, request.RequestContext);
                    }
                    else if (dropAndDeclareTransaction.TransactionType == TransactionType.TenderDeclaration)
                    {
                        this.FillTenderDataTable(transaction.Id, dropAndDeclareTransaction, tenderDeclarationTable, request.RequestContext);

                        // Fill captured reason code details for [Tender Declaration] store operation
                        this.FillReasonCodeLines(transaction.Id, dropAndDeclareTransaction, reasonCodeTable, -1m, request.RequestContext);
                    }

                    var parameters = new ParameterSet();

                    parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                    parameters["@tvp_Transaction"] = transactionTable;
                    parameters["@tvp_TenderDeclareTrans"] = tenderDeclarationTable;
                    parameters["@tvp_TenderDropTrans"] = tenderDropTable;
                    parameters["@tvp_ReasoncodeLine"] = reasonCodeTable;

                    int errorCode;
                    using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        results = sqlServerDatabaseContext.ExecuteStoredProcedure<DropAndDeclareTransaction, TenderDetail>(InsertTenderDropAndDeclareSprocName, parameters, null, out errorCode);
                    }

                    if (errorCode != (int)DatabaseErrorCodes.Success)
                    {
                        throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save tender drop and declare operation.");
                    }
                }
    
                DropAndDeclareTransaction savedTransaction = results.Item1.FirstOrDefault();
                if (savedTransaction == null)
                {
                    throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, "Unable to retrieve newly created tender drop and declare operation after save.");
                }
    
                savedTransaction.TenderDetails = results.Item2;
    
                return new SingleEntityDataServiceResponse<DropAndDeclareTransaction>(savedTransaction);
            }
    
            private EntityDataServiceResponse<DropAndDeclareTransaction> GetDropAndDeclareTransactions(GetDropAndDeclareTransactionDataRequest request)
            {
                ThrowIf.NullOrWhiteSpace(request.DropAndDeclareTransactionId, "request.DropAndDeclareTransactionId");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = DropAndDeclareTransactionsView,
                    Where = "TRANSACTIONID = @TransactionId",
                };
    
                query.Parameters["@TransactionId"] = request.DropAndDeclareTransactionId;
    
                PagedResult<DropAndDeclareTransaction> dropAndDeclareTransaction;
                using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    dropAndDeclareTransaction = sqlServerDatabaseContext.ReadEntity<DropAndDeclareTransaction>(query);
                }
    
                return new EntityDataServiceResponse<DropAndDeclareTransaction>(dropAndDeclareTransaction);
            }
    
            private EntityDataServiceResponse<TenderDetail> GetDropAndDeclareTransactionTenderDetails(GetDropAndDeclareTransactionTenderDetailsDataRequest request)
            {
                ThrowIf.NullOrWhiteSpace(request.DropAndDeclareTransactionId, "request.DropAndDeclareTransactionId");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = DropAndDeclareTendersView,
                    Where = "TRANSACTIONID = @TransactionId",
                    OrderBy = "LINENUM",
                };
    
                query.Parameters["@TransactionId"] = request.DropAndDeclareTransactionId;
    
                PagedResult<TenderDetail> results;
                using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    results = sqlServerDatabaseContext.ReadEntity<TenderDetail>(query);
                }
    
                return new EntityDataServiceResponse<TenderDetail>(results);
            }
    
            /// <summary>
            /// Fill data to the data table for TenderDeclaration tender lines.
            /// </summary>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="transaction">Tender drop and declare transactions.</param>
            /// <param name="tenderDeclarationTable">Tender drop and declare data table.</param>
            /// <param name="context">Request context.</param>
            private void FillTenderDataTable(string transactionId, DropAndDeclareTransaction transaction, DataTable tenderDeclarationTable, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(tenderDeclarationTable, "tenderDeclarationTable");
    
                int lineNumber = 1;
    
                foreach (TenderDetail tenderDetail in transaction.TenderDetails)
                {
                    bool isTenderDeclaration = transaction.TransactionType == TransactionType.TenderDeclaration;
                    bool isBankDrop = transaction.TransactionType == TransactionType.BankDrop;
                    bool isSafeDrop = transaction.TransactionType == TransactionType.SafeDrop;
    
                    DataRow row = tenderDeclarationTable.NewRow();
                    row[TransactionIDColumn] = transactionId;
                    row[TenderTypeColumn] = tenderDetail.TenderTypeId;
                    row[AmountTenderedColumn] = tenderDetail.Amount;
                    row[LineNumColumn] = lineNumber;
                    row[QTYColumn] = 1;
                    row[CurrencyColumn] = tenderDetail.ForeignCurrency;
                    row[AmountCurColumn] = tenderDetail.AmountInForeignCurrency;
                    row[ExchangeColumn] = tenderDetail.ForeignCurrencyExchangeRate;
                    row[AmountMSTColumn] = tenderDetail.AmountInCompanyCurrency;
                    row[ExchangeMSTColumn] = tenderDetail.CompanyCurrencyExchangeRate;
                    row[StaffColumn] = transaction.StaffId;
                    row[StoreColumn] = transaction.StoreId;
                    row[TerminalColumn] = transaction.TerminalId;
                    row[DataAreaIdColumn] = context.GetChannelConfiguration().InventLocationDataAreaId; // transaction.DataAreaId should also be ok
                    row[TransTimeColumn] = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds;
    
                    if (isTenderDeclaration)
                    {
                        row[PosCurrencyColumn] = transaction.ChannelCurrency;
                    }
                    else if (isBankDrop)
                    {
                        row[TypeColumn] = (int)transaction.TransactionType;
                        row[AmountCurPOSColumn] = tenderDetail.AmountInForeignCurrency;
                        row[AmountTenderedPOSColumn] = tenderDetail.Amount;
                        row[AmountMSTPOSColumn] = tenderDetail.AmountInCompanyCurrency;
                        row[StatusTypeColumn] = 1;  // Always to 1 by the POS
                        row[BankBagNumColumn] = tenderDetail.BankBagNumber;
                    }
                    else if (isSafeDrop)
                    {
                        row[TypeColumn] = (int)transaction.TransactionType;
                        row[AmountCurPOSColumn] = tenderDetail.AmountInForeignCurrency;
                        row[AmountTenderedPOSColumn] = tenderDetail.Amount;
                        row[AmountMSTPOSColumn] = tenderDetail.AmountInCompanyCurrency;
                        row[StatusTypeColumn] = 1;  // Always to 1 by the POS
                    }
    
                    tenderDeclarationTable.Rows.Add(row);
                    lineNumber++;
                }
            }

            /// <summary>
            /// Saves the reason code line for [Tender Declaration] store operation.
            /// </summary>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="reasonCodeTable">The reason code table.</param>
            /// <param name="parentLineNumber">The parent line number.</param>
            /// <param name="context">The request context.</param>
            private void FillReasonCodeLines(string transactionId, DropAndDeclareTransaction transaction, DataTable reasonCodeTable, decimal parentLineNumber, RequestContext context)
            {
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(reasonCodeTable, "reasonCodeTable");

                int lineNumber = 1;
                DateTimeOffset transactionDate = context.GetNowInChannelTimeZone();

                foreach (ReasonCodeLine reasonCodeLine in transaction.ReasonCodeLines)
                {
                    DataRow row = reasonCodeTable.NewRow();
                    row[TransactionIDColumn] = transactionId;
                    row[TransDateColumn] = transactionDate.Date;

                    // trans time is stored as seconds (integer) in the database
                    row[TransTimeColumn] = (int)transactionDate.TimeOfDay.TotalSeconds;
                    row[LineNumColumn] = lineNumber;
                    row[DataAreaIdColumn] = context.GetChannelConfiguration().InventLocationDataAreaId;
                    row[TypeColumn] = (int)reasonCodeLine.LineType;
                    row[ReasonCodeIdColumn] = StringDataHelper.TruncateString(reasonCodeLine.ReasonCodeId, 10);
                    row[InformationColumn] = StringDataHelper.TruncateString(reasonCodeLine.Information, 100);
                    row[InformationAmountColumn] = reasonCodeLine.InformationAmount;
                    row[StoreColumn] = transaction.StoreId;
                    row[TerminalColumn] = transaction.TerminalId ?? string.Empty;
                    row[StaffColumn] = StringDataHelper.TruncateString(transaction.StaffId, 25);
                    row[ItemTenderColumn] = StringDataHelper.TruncateString(reasonCodeLine.ItemTender, 10);
                    row[AmountColumn] = reasonCodeLine.Amount;
                    row[InputTypeColumn] = (int)reasonCodeLine.InputType;
                    row[SubReasonCodeIdColumn] = StringDataHelper.TruncateString(reasonCodeLine.SubReasonCodeId, 10);
                    row[StatementCodeColumn] = StringDataHelper.TruncateString(reasonCodeLine.StatementCode, 25);
                    row[SourceCodeColumn] = StringDataHelper.TruncateString(reasonCodeLine.SourceCode, 20);
                    row[SourceCode2Column] = StringDataHelper.TruncateString(reasonCodeLine.SourceCode2, 20);
                    row[SourceCode3Column] = StringDataHelper.TruncateString(reasonCodeLine.SourceCode3, 20);
                    row[ParentLineNumColumn] = parentLineNumber;

                    reasonCodeTable.Rows.Add(row);
                    lineNumber++;
                }
            }

            /// <summary>
            /// Converts from non sale tender operation type to transaction log.
            /// </summary>
            /// <param name="tenderTransaction">Non sale tender transaction.</param>
            /// <returns>Returns the transaction log entity.</returns>
            private TransactionLog Convert(DropAndDeclareTransaction tenderTransaction)
            {
                ThrowIf.NullOrWhiteSpace(tenderTransaction.Id, "tenderTransaction.Id");
    
                var transaction = new TransactionLog();
    
                transaction.Id = tenderTransaction.Id;
                transaction.StoreId = tenderTransaction.StoreId;
                transaction.TransactionType = (TransactionType)tenderTransaction.TransactionType;
                transaction.StaffId = tenderTransaction.StaffId;
                transaction.ShiftId = tenderTransaction.ShiftId;
                transaction.ShiftTerminalId = tenderTransaction.ShiftTerminalId;
                transaction.TerminalId = tenderTransaction.TerminalId;
                transaction.ChannelCurrencyExchangeRate = tenderTransaction.ChannelCurrencyExchangeRate;
                transaction.ChannelCurrency = tenderTransaction.ChannelCurrency;
                transaction.Description = tenderTransaction.Description;
    
                return transaction;
            }
        }
    }
}
