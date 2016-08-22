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
        /// The shifts SQL server data service.
        /// </summary>
        public class ShiftSqlServerDataService : IRequestHandler
        {
            // Type names
            private const string ShiftTableTypeName = "SHIFTTABLETYPE";
            private const string ShiftTenderLineTableTypeName = "SHIFTTENDERLINETABLETYPE";
            private const string ShiftAccountLineTableTypeName = "SHIFTACCOUNTLINETYPE";
    
            // Table type parameters
            private const string ShiftTableTypeVariableName = "@TVP_SHIFTTABLETYPE";
            private const string ShiftTenderLineTableTypeVariableName = "@TVP_SHIFTTENDERLINETABLETYPE";
            private const string ShiftAccountLineTableTypeVariableName = "@TVP_SHIFTACCOUNTLINETABLETYPE";
    
            // View names
            private const string ShiftsView = "SHIFTSVIEW";
            private const string ShiftTenderLinesView = "SHIFTTENDERLINESVIEW";
            private const string ShiftAccountsView = "SHIFTACCOUNTSVIEW";
    
            // Stored procedure names
            private const string InsertShiftSprocName = "INSERTSHIFT";
            private const string GetShiftSalesDataSprocName = "GETSHIFTSALESDATA";
            private const string GetShiftTransactionsSprocName = "GETSHIFTRANSACTIONS";
            private const string GetShiftTenderedAmountSprocName = "GETSHIFTTENDEREDAMOUNT";
            private const string UpsertShiftStagingSprocName = "UPSERTSHIFTSTAGING";
            private const string UpdateShiftStagingSprocName = "UPDATESHIFTSTAGING";
            private const string DeleteShiftStagingSprocName = "DELETESHIFTSTAGING";
            private const string DeleteShiftStagingAndInsertShiftSprocName = "DELETESHIFTSTAGINGANDINSERTSHIFT";
            private const string GetShiftRequiredAmountsPerTenderSprocName = "GETSHIFTREQUIREDAMOUNTSPERTENDER";
    
            // Parameter names
            private const string TerminalIdVariableName = "@nvc_TerminalId";
            private const string ShiftIdVariableName = "@bi_ShiftId";
            private const string TaxInclusiveVariableName = "@i_TaxInclusive";
            private const string RowVersionViarableName = "@rv_RowVersion";
            private const string DataAreaIdVariableName = "@nvc_DataAreaId";
    
            // Column names
            private const string CashDrawerColumn = "CASHDRAWER";
            private const string ChannelRecordIdColumn = "CHANNEL";
            private const string CloseDateColumn = "CLOSEDATE";
            private const string CloseDateTimeUtcColumn = "CLOSEDATETIMEUTC";
            private const string CloseDateTimeUtcTimeZoneIdColumn = "CLOSEDATETIMEUTCTZID";
            private const string CloseTimeColumn = "CLOSETIME";
            private const string ClosedAtTerminalIdColumn = "CLOSEDATTERMINAL";
            private const string CurrentTerminalIdColumn = "CURRENTTERMINALID";
            private const string CustomerCountColumn = "CUSTOMERSCOUNT";
            private const string DataAreaIdColumn = "DATAAREAID";
            private const string IsSharedColumn = "ISSHARED";
            private const string DiscountTotalColumn = "DISCOUNTTOTAL";
            private const string LogOnTransactionCountColumn = "LOGONSCOUNT";
            private const string NoSaleCountColumn = "NOSALECOUNT";
            private const string PaidToAccountTotalColumn = "PAIDTOACCOUNTTOTAL";
            private const string PostedColumn = "POSTED";
            private const string ReturnsTotalColumn = "RETURNSTOTAL";
            private const string RoundedAmountTotalColumn = "ROUNDEDAMOUNTTOTAL";
            private const string SaleTransactionCountColumn = "SALESCOUNT";
            private const string SalesTotalColumn = "SALESTOTAL";
            private const string ShiftIdColumn = "SHIFTID";
            private const string StaffIdColumn = "STAFFID";
            private const string CurrentStaffIdColumn = "CURRENTSTAFFID";
            private const string StartDateColumn = "STARTDATE";
            private const string StartDateTimeUtcColumn = "STARTDATETIMEUTC";
            private const string StartDateTimeUtcTimeZoneIdColumn = "STARTDATETIMEUTCTZID";
            private const string StartTimeColumn = "STARTTIME";
            private const string StatusColumn = "STATUS";
            private const string StatusDateTimeUtcColumn = "STATUSDATETIMEUTC";
            private const string StoreIdColumn = "STOREID";
            private const string TaxTotalColumn = "TAXTOTAL";
            private const string TerminalIdColumn = "TERMINALID";
            private const string TransactionCountColumn = "TRANSACTIONSCOUNT";
            private const string VoidTransactionCountColumn = "VOIDSCOUNT";
            private const string AddToTenderAmountColumnName = "ADDTOTENDERAMOUNT";
            private const string AddToTenderAmountCurColumnName = "ADDTOTENDERAMOUNTCUR";
            private const string BankDropAmountColumnName = "BANKDROPAMOUNT";
            private const string BankDropAmountCurColumnName = "BANKDROPAMOUNTCUR";
            private const string CardTypeIdColumnName = "CARDTYPEID";
            private const string ChangeAmountColumnName = "CHANGEAMOUNT";
            private const string ChangeAmountCurColumnName = "CHANGEAMOUNTCUR";
            private const string CountColumnName = "COUNT";
            private const string CountingRequiredColumnName = "COUNTINGREQUIRED";
            private const string CurrencyColumnName = "CURRENCY";
            private const string DeclareTenderAmountColumnName = "DECLARETENDERAMOUNT";
            private const string DeclareTenderAmountCurColumnName = "DECLARETENDERAMOUNTCUR";
            private const string RemoveTenderAmountColumnName = "REMOVETENDERAMOUNT";
            private const string RemoveTenderAmountCurColumnName = "REMOVETENDERAMOUNTCUR";
            private const string SafeDropAmountColumnName = "SAFEDROPAMOUNT";
            private const string SafeDropAmountCurColumnName = "SAFEDROPAMOUNTCUR";
            private const string StartingAmountColumnName = "STARTINGAMOUNT";
            private const string StartingAmountCurColumnName = "STARTINGAMOUNTCUR";
            private const string TenderedAmountColumnName = "TENDEREDAMOUNT";
            private const string TenderedAmountCurColumnName = "TENDEREDAMOUNTCUR";
            private const string TenderTypeIdColumnName = "TENDERTYPEID";
            private const string AccountNumberColumn = "INCOMEEXEPENSEACCOUNT";
            private const string AccountTypeColumn = "ACCOUNTTYPE";
            private const string AmountColumn = "AMOUNT";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(UpdateShiftStagingTableDataRequest),
                        typeof(CreateShiftDataRequest),
                        typeof(GetShiftDataDataRequest),
                        typeof(DeleteShiftDataRequest),
                        typeof(GetShiftRequiredAmountsPerTenderDataRequest),
                        typeof(GetLastClosedShiftDataRequest),
                        typeof(GetEndOfDayShiftDetailsDataRequest),
                        typeof(GetShiftTransactionsCountDataRequest),
                        typeof(GetShiftTenderedAmountDataRequest),
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
    
                if (requestType == typeof(UpdateShiftStagingTableDataRequest))
                {
                    response = this.UpdateShiftStaging((UpdateShiftStagingTableDataRequest)request);
                }
                else if (requestType == typeof(CreateShiftDataRequest))
                {
                    response = this.InsertShiftStagingTable((CreateShiftDataRequest)request);
                }
                else if (requestType == typeof(GetShiftDataDataRequest))
                {
                    response = this.GetShiftData((GetShiftDataDataRequest)request);
                }
                else if (requestType == typeof(DeleteShiftDataRequest))
                {
                    response = this.DeleteShiftStagingTable((DeleteShiftDataRequest)request);
                }
                else if (requestType == typeof(GetShiftRequiredAmountsPerTenderDataRequest))
                {
                    response = this.GetShiftRequiredAmountsPerTender((GetShiftRequiredAmountsPerTenderDataRequest)request);
                }
                else if (requestType == typeof(GetLastClosedShiftDataRequest))
                {
                    response = this.GetLastClosedShift((GetLastClosedShiftDataRequest)request);
                }
                else if (requestType == typeof(GetEndOfDayShiftDetailsDataRequest))
                {
                    response = this.GetEndOfDayshiftDetails((GetEndOfDayShiftDetailsDataRequest)request);
                }
                else if (requestType == typeof(GetShiftTransactionsCountDataRequest))
                {
                    response = this.GetShiftTransactionsCounts((GetShiftTransactionsCountDataRequest)request);
                }
                else if (requestType == typeof(GetShiftTenderedAmountDataRequest))
                {
                    response = this.GetShiftTenderedAmount((GetShiftTenderedAmountDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// The data service method to get the shift data.
            /// </summary>
            /// <param name="request">The get shift data request.</param>
            /// <returns>A response.</returns>
            private Response GetShiftData(GetShiftDataDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    GetShiftDataProcedure procedure = new GetShiftDataProcedure(request, databaseContext);
                    return procedure.Execute(request.QueryResultSettings);
                }
            }
    
            /// <summary>
            /// The data service method to insert the shift into staging table if the shift status is not closed.
            /// </summary>
            /// <param name="request">The create shift data request.</param>
            /// <returns>A null response.</returns>
            private NullResponse InsertShiftStagingTable(CreateShiftDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Shift, "request.Shift");
    
                Shift shift = request.Shift;
    
                if (shift.Status == ShiftStatus.Closed)
                {
                    this.ConvertShiftToTableVariableParametersAndCallStoredProcedure(InsertShiftSprocName, request, isShiftStagingTableRow: false);
                }
                else
                {
                    this.ConvertShiftToTableVariableParametersAndCallStoredProcedure(UpsertShiftStagingSprocName, request);
                }
    
                return new NullResponse();
            }
    
            /// <summary>
            /// The data service method to update the shift staging table.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>A null response.</returns>
            private NullResponse UpdateShiftStaging(UpdateShiftStagingTableDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Shift, "request.Shift");
    
                Shift shift = request.Shift;
    
                if (shift.Status == ShiftStatus.Closed)
                {
                    this.ConvertShiftToTableVariableParametersAndCallStoredProcedure(DeleteShiftStagingAndInsertShiftSprocName, request, isShiftStagingTableRow: false);
                }
                else
                {
                    this.ConvertShiftToTableVariableParametersAndCallStoredProcedure(UpdateShiftStagingSprocName, request);
                }
    
                return new NullResponse();
            }
    
            /// <summary>
            /// The data service method to retrieve the required tender declaration amounts of a shift per tender type.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The entity data service response.</returns>
            private EntityDataServiceResponse<ShiftTenderLine> GetShiftRequiredAmountsPerTender(GetShiftRequiredAmountsPerTenderDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.TerminalId, "request.TerminalId");
    
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters[TerminalIdVariableName] = request.TerminalId;
                parameters[ShiftIdVariableName] = request.ShiftId;
                parameters[DataAreaIdVariableName] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                PagedResult<ShiftTenderLine> pagedResults;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    pagedResults = sqlServerDatabaseContext.ExecuteStoredProcedure<ShiftTenderLine>(GetShiftRequiredAmountsPerTenderSprocName, parameters);
                }
    
                return new EntityDataServiceResponse<ShiftTenderLine>(pagedResults);
            }
    
            /// <summary>
            /// Delete the shift from shift staging table.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A null response.</returns>
            private NullResponse DeleteShiftStagingTable(DeleteShiftDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Shift, "request.Shift");
    
                Shift shift = request.Shift;
    
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters[TerminalIdVariableName] = shift.TerminalId;
                parameters[ShiftIdVariableName] = shift.ShiftId;
                parameters[RowVersionViarableName] = shift.RowVersion;
    
                int errorCode;
                using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    errorCode = sqlServerDatabaseContext.ExecuteStoredProcedureNonQuery(DeleteShiftStagingSprocName, parameters);
                }
    
                if (errorCode != (int)DatabaseErrorCodes.Success)
                {
                    throw new StorageException(
                        StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError,
                        errorCode,
                        string.Format("Unable to execute the stored procedure {0}.", DeleteShiftStagingSprocName));
                }
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets the last closed shift.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A single entity data service response.</returns>
            private SingleEntityDataServiceResponse<Shift> GetLastClosedShift(GetLastClosedShiftDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.TerminalId, "request.TerminalId");
    
                long channelId = request.RequestContext.GetPrincipal().ChannelId;
    
                var query = new SqlPagedQuery(QueryResultSettings.FirstRecord)
                {
                    From = ShiftsView,
                    Where = "CHANNEL = @ChannelId AND CLOSEDATTERMINAL = @TerminalId AND STATUS = @Status",
                    OrderBy = "CLOSEDATETIMEUTC DESC"
                };
    
                query.Parameters["@ChannelId"] = channelId;
                query.Parameters["@TerminalId"] = request.TerminalId;
                query.Parameters["@Status"] = (int)ShiftStatus.Closed;
    
                Shift lastShift = null;
    
                using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    lastShift = sqlServerDatabaseContext.ReadEntity<Shift>(query).Results.FirstOrDefault();
                }
    
                if (lastShift != null)
                {
                    GetShiftDataRequest getShiftDataRequest = new GetShiftDataRequest(lastShift.TerminalId, lastShift.ShiftId);
                    lastShift = request.RequestContext.Execute<SingleEntityDataServiceResponse<Shift>>(getShiftDataRequest).Entity;
    
                    // Convert UTC time to channel time.
                    if (lastShift.StartDateTime != null)
                    {
                        lastShift.StartDateTime = new DateTimeOffset(lastShift.StartDateTime.Value.DateTime, new TimeSpan(0));
                    }
    
                    if (lastShift.StatusDateTime != null)
                    {
                        lastShift.StatusDateTime = new DateTimeOffset(lastShift.StatusDateTime.Value.DateTime, new TimeSpan(0));
                    }
    
                    if (lastShift.CloseDateTime != null)
                    {
                        lastShift.CloseDateTime = new DateTimeOffset(lastShift.CloseDateTime.Value.DateTime, new TimeSpan(0));
                    }
                }
    
                return new SingleEntityDataServiceResponse<Shift>(lastShift);
            }
    
            /// <summary>
            /// Get shift transactions.
            /// </summary>
            /// <param name="request">The get shift transaction counts data request.</param>
            /// <returns>The Shift collection object.</returns>
            private SingleEntityDataServiceResponse<Shift> GetShiftTransactionsCounts(GetShiftTransactionsCountDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.TerminalId, "request.TerminalId");
    
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters[TerminalIdVariableName] = request.TerminalId;
                parameters[ShiftIdVariableName] = request.ShiftId;
    
                Shift shift;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    shift = sqlServerDatabaseContext.ExecuteStoredProcedure<Shift>(GetShiftTransactionsSprocName, parameters).Results.FirstOrDefault();
                }
    
                return new SingleEntityDataServiceResponse<Shift>(shift);
            }
    
            /// <summary>
            /// Converts shift To table variable parameters and calls stored procedure.
            /// </summary>
            /// <param name="storedProcedureName">The stored procedure name.</param>
            /// <param name="request">The create shift data request.</param>
            /// <param name="isShiftStagingTableRow">A boolean value indicating if the row is from shift staging table or Pos batch table.</param>
            private void ConvertShiftToTableVariableParametersAndCallStoredProcedure(string storedProcedureName, ShiftDataRequest request, bool isShiftStagingTableRow = true)
            {
                string inventLocationDataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                Shift shift = request.Shift;
                var parameters = new ParameterSet();
    
                using (DataTable shiftTable = new DataTable(ShiftTableTypeName))
                using (DataTable shiftTenderLineTable = new DataTable(ShiftTenderLineTableTypeName))
                using (DataTable shiftAccountLineTable = new DataTable(ShiftAccountLineTableTypeName))
                {
                    this.SetShiftTableTypeSchema(shiftTable);
                    shiftTable.Rows.Add(this.ConvertToShiftDataRow(shiftTable, shift, inventLocationDataAreaId, isShiftStagingTableRow));
                    parameters[ShiftTableTypeVariableName] = shiftTable;
    
                    if (shift.Status == ShiftStatus.Closed)
                    {
                        // Only closed shift contains tenderline information.
                        this.SetShiftTenderLineTableTypeSchema(shiftTenderLineTable);
                        foreach (DataRow row in this.ConvertToShiftTenderLineDataRows(shiftTenderLineTable, shift, inventLocationDataAreaId))
                        {
                            shiftTenderLineTable.Rows.Add(row);
                        }
    
                        parameters[ShiftTenderLineTableTypeVariableName] = shiftTenderLineTable;
    
                        // Insert account (income /expense) lines
                        this.SetShiftAccountLineTableTypeSchema(shiftAccountLineTable);
                        foreach (DataRow row in this.ConvertToShiftAccountLineDataRows(shiftAccountLineTable, shift, inventLocationDataAreaId))
                        {
                            shiftAccountLineTable.Rows.Add(row);
                        }
    
                        parameters[ShiftAccountLineTableTypeVariableName] = shiftAccountLineTable;
                    }
    
                    var inputOutputParameters = new ParameterSet();
                    inputOutputParameters[RowVersionViarableName] = shift.RowVersion;
                    int errorCode;
    
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        errorCode = sqlServerDatabaseContext.ExecuteStoredProcedureScalar(storedProcedureName, parameters, inputOutputParameters);
                    }
    
                    if (errorCode != (int)DatabaseErrorCodes.Success)
                    {
                        throw new StorageException(
                            StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError,
                            errorCode,
                            string.Format("Unable to execute the stored procedure {0}.", storedProcedureName));
                    }
    
                    shift.RowVersion = inputOutputParameters[RowVersionViarableName] as byte[];
                }
            }
    
            /// <summary>
            /// Loads the shift transactions data.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A single entity data service response.</returns>
            private SingleEntityDataServiceResponse<Shift> GetEndOfDayshiftDetails(GetEndOfDayShiftDetailsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.TerminalId, "request.TerminalId");
    
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters[TerminalIdVariableName] = request.TerminalId;
                parameters[ShiftIdVariableName] = request.ShiftId;
                parameters[TaxInclusiveVariableName] = Convert.ToInt32(request.IsTaxInclusive);
    
                Shift shift;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    var result = sqlServerDatabaseContext.ExecuteStoredProcedure<Shift, ShiftTenderLine, ShiftAccountLine>(GetShiftSalesDataSprocName, parameters);
                    shift = result.Item1.SingleOrDefault();
                    shift.TenderLines = result.Item2;
                    shift.AccountLines = result.Item3;
                }
    
                return new SingleEntityDataServiceResponse<Shift>(shift);
            }
    
            /// <summary>
            /// Get tendered amount details of the shift.
            /// </summary>
            /// <param name="request">The get shift tendered amount data request.</param>
            /// <returns>A entity data service response.</returns>
            private EntityDataServiceResponse<ShiftTenderLine> GetShiftTenderedAmount(GetShiftTenderedAmountDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.TerminalId, "request.TerminalId");
    
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters[TerminalIdVariableName] = request.TerminalId;
                parameters[ShiftIdVariableName] = request.ShiftId;
    
                PagedResult<ShiftTenderLine> pagedResults;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    pagedResults = sqlServerDatabaseContext.ExecuteStoredProcedure<ShiftTenderLine>(GetShiftTenderedAmountSprocName, parameters);
                }
    
                return new EntityDataServiceResponse<ShiftTenderLine>(pagedResults);
            }
    
            /// <summary>
            /// Sets shift table schema.
            /// </summary>
            /// <param name="table">The shift table.</param>
            private void SetShiftTableTypeSchema(DataTable table)
            {
                ThrowIf.Null(table, "table");
    
                // NOTE: The order of colums here MUST match the ShiftTableType TVP.
                table.Columns.Add(CashDrawerColumn, typeof(string));
                table.Columns.Add(ChannelRecordIdColumn, typeof(long));
                table.Columns.Add(CloseDateColumn, typeof(DateTime));
                table.Columns.Add(CloseDateTimeUtcColumn, typeof(DateTime));
                table.Columns.Add(CloseDateTimeUtcTimeZoneIdColumn, typeof(int));
                table.Columns.Add(CloseTimeColumn, typeof(int));
                table.Columns.Add(ClosedAtTerminalIdColumn, typeof(string));
                table.Columns.Add(CurrentTerminalIdColumn, typeof(string));
                table.Columns.Add(CustomerCountColumn, typeof(int));
                table.Columns.Add(DiscountTotalColumn, typeof(decimal));
                table.Columns.Add(LogOnTransactionCountColumn, typeof(int));
                table.Columns.Add(NoSaleCountColumn, typeof(int));
                table.Columns.Add(PaidToAccountTotalColumn, typeof(decimal));
                table.Columns.Add(PostedColumn, typeof(int));
                table.Columns.Add(ReturnsTotalColumn, typeof(decimal));
                table.Columns.Add(RoundedAmountTotalColumn, typeof(decimal));
                table.Columns.Add(SaleTransactionCountColumn, typeof(int));
                table.Columns.Add(SalesTotalColumn, typeof(decimal));
                table.Columns.Add(ShiftIdColumn, typeof(long));
                table.Columns.Add(StaffIdColumn, typeof(string));
                table.Columns.Add(CurrentStaffIdColumn, typeof(string));
                table.Columns.Add(StartDateColumn, typeof(DateTime));
                table.Columns.Add(StartDateTimeUtcColumn, typeof(DateTime));
                table.Columns.Add(StartDateTimeUtcTimeZoneIdColumn, typeof(int));
                table.Columns.Add(StartTimeColumn, typeof(int));
                table.Columns.Add(StatusColumn, typeof(int));
                table.Columns.Add(StatusDateTimeUtcColumn, typeof(DateTime));
                table.Columns.Add(StoreIdColumn, typeof(string));
                table.Columns.Add(TaxTotalColumn, typeof(decimal));
                table.Columns.Add(TerminalIdColumn, typeof(string));
                table.Columns.Add(TransactionCountColumn, typeof(int));
                table.Columns.Add(VoidTransactionCountColumn, typeof(int));
                table.Columns.Add(IsSharedColumn, typeof(bool));
                table.Columns.Add(DataAreaIdColumn, typeof(string));
            }
    
            /// <summary>
            /// Convert the shift object to a data row to insert into RetailPosBatchTable or RetailShiftStagingTable.
            /// </summary>
            /// <param name="table">The data table.</param>
            /// <param name="shift">The shift object.</param>
            /// <param name="inventLocationDataAreaId">The invent location area id.</param>
            /// <param name="isShiftStagingTableRow">True, if the shift info is to be put into RetailShiftStagingTable.</param>
            /// <returns>The DataRow object containing the shift info.</returns>
            private DataRow ConvertToShiftDataRow(DataTable table, Shift shift, string inventLocationDataAreaId, bool isShiftStagingTableRow = true)
            {
                DateTime? startShiftDateTime, closeShiftDateTime, statusShiftDateTime;
                DataRow row = table.NewRow();
    
                if (isShiftStagingTableRow)
                {
                    startShiftDateTime = DateTimeOffsetDataHelper.GetDbNullableDateTime(shift.StartDateTime);
                    closeShiftDateTime = DateTimeOffsetDataHelper.GetDbNullableDateTime(shift.CloseDateTime);
                    statusShiftDateTime = DateTimeOffsetDataHelper.GetDbNullableDateTime(shift.StatusDateTime);
                }
                else
                {
                    startShiftDateTime = DateTimeOffsetDataHelper.GetDbNullableUtcDateTime(shift.StartDateTime);
                    closeShiftDateTime = DateTimeOffsetDataHelper.GetDbNullableUtcDateTime(shift.CloseDateTime);
                    statusShiftDateTime = DateTimeOffsetDataHelper.GetDbNullableUtcDateTime(shift.StatusDateTime);
                }
    
                row[CashDrawerColumn] = shift.CashDrawer;
                row[ChannelRecordIdColumn] = shift.StoreRecordId;
                row[CloseDateColumn] = DateTimeOffsetDataHelper.GetDbNullableDate(shift.CloseDateTime);
                row[CloseDateTimeUtcColumn] = closeShiftDateTime;
                row[CloseDateTimeUtcTimeZoneIdColumn] = 0; // Not assigned in POS.
                row[CloseTimeColumn] = DateTimeOffsetDataHelper.GetDbNullableTimeInSeconds(shift.CloseDateTime);
                row[ClosedAtTerminalIdColumn] = shift.ClosedAtTerminalId;
                row[CurrentTerminalIdColumn] = shift.CurrentTerminalId;
                row[CustomerCountColumn] = shift.CustomerCount;
                row[DiscountTotalColumn] = shift.DiscountTotal;
                row[LogOnTransactionCountColumn] = shift.LogOnTransactionCount;
                row[NoSaleCountColumn] = shift.NoSaleTransactionCount;
                row[PaidToAccountTotalColumn] = shift.PaidToAccountTotal;
                row[PostedColumn] = 0; // Not assigned in POS.
                row[ReturnsTotalColumn] = shift.ReturnsTotal;
                row[RoundedAmountTotalColumn] = shift.RoundedAmountTotal;
                row[SaleTransactionCountColumn] = shift.SaleTransactionCount;
                row[SalesTotalColumn] = shift.SalesTotal;
                row[ShiftIdColumn] = shift.ShiftId;
                row[StaffIdColumn] = shift.StaffId;
                row[CurrentStaffIdColumn] = shift.CurrentStaffId;
                row[StartDateColumn] = DateTimeOffsetDataHelper.GetDbNullableDateTime(shift.StartDateTime);
                row[StartDateTimeUtcColumn] = startShiftDateTime;
                row[StartDateTimeUtcTimeZoneIdColumn] = 0; // Not assigned in POS.
                row[StartTimeColumn] = DateTimeOffsetDataHelper.GetDbNullableTimeInSeconds(shift.StartDateTime);
                row[StatusColumn] = (int)shift.Status;
                row[StatusDateTimeUtcColumn] = statusShiftDateTime;
                row[StoreIdColumn] = shift.StoreId;
                row[TaxTotalColumn] = shift.TaxTotal;
                row[TerminalIdColumn] = shift.TerminalId;
                row[TransactionCountColumn] = shift.TransactionCount;
                row[VoidTransactionCountColumn] = shift.VoidTransactionCount;
                row[IsSharedColumn] = shift.IsShared;
                row[DataAreaIdColumn] = inventLocationDataAreaId;
    
                return row;
            }
    
            /// <summary>
            /// Sets the shift tender line table.
            /// </summary>
            /// <param name="table">The shift tender line table.</param>
            private void SetShiftTenderLineTableTypeSchema(DataTable table)
            {
                ThrowIf.Null(table, "table");
    
                // NOTE: The order of colums here MUST match the ShiftTenderLineTableType TVP.
                table.Columns.Add(AddToTenderAmountColumnName, typeof(decimal));
                table.Columns.Add(AddToTenderAmountCurColumnName, typeof(decimal));
                table.Columns.Add(BankDropAmountColumnName, typeof(decimal));
                table.Columns.Add(BankDropAmountCurColumnName, typeof(decimal));
                table.Columns.Add(CardTypeIdColumnName, typeof(string));
                table.Columns.Add(ChangeAmountColumnName, typeof(decimal));
                table.Columns.Add(ChangeAmountCurColumnName, typeof(decimal));
                table.Columns.Add(ChannelRecordIdColumn, typeof(long));
                table.Columns.Add(CountColumnName, typeof(int));
                table.Columns.Add(CountingRequiredColumnName, typeof(int));
                table.Columns.Add(CurrencyColumnName, typeof(string));
                table.Columns.Add(DeclareTenderAmountColumnName, typeof(decimal));
                table.Columns.Add(DeclareTenderAmountCurColumnName, typeof(decimal));
                table.Columns.Add(RemoveTenderAmountColumnName, typeof(decimal));
                table.Columns.Add(RemoveTenderAmountCurColumnName, typeof(decimal));
                table.Columns.Add(SafeDropAmountColumnName, typeof(decimal));
                table.Columns.Add(SafeDropAmountCurColumnName, typeof(decimal));
                table.Columns.Add(ShiftIdColumn, typeof(long));
                table.Columns.Add(StartingAmountColumnName, typeof(decimal));
                table.Columns.Add(StartingAmountCurColumnName, typeof(decimal));
                table.Columns.Add(StoreIdColumn, typeof(string));
                table.Columns.Add(TenderedAmountColumnName, typeof(decimal));
                table.Columns.Add(TenderedAmountCurColumnName, typeof(decimal));
                table.Columns.Add(TenderTypeIdColumnName, typeof(string));
                table.Columns.Add(TerminalIdColumn, typeof(string));
                table.Columns.Add(DataAreaIdColumn, typeof(string));
            }
    
            /// <summary>
            /// Sets the shift account line table.
            /// </summary>
            /// <param name="table">The shift account line table.</param>
            private void SetShiftAccountLineTableTypeSchema(DataTable table)
            {
                ThrowIf.Null(table, "table");
    
                // NOTE: The order of colums here MUST match the ShiftAccountLineTableType.
                table.Columns.Add(AccountNumberColumn, typeof(string));
                table.Columns.Add(AccountTypeColumn, typeof(int));
                table.Columns.Add(AmountColumn, typeof(decimal));
                table.Columns.Add(ShiftIdColumn, typeof(long));
                table.Columns.Add(StoreIdColumn, typeof(string));
                table.Columns.Add(TerminalIdColumn, typeof(string));
                table.Columns.Add(ChannelRecordIdColumn, typeof(long));
                table.Columns.Add(DataAreaIdColumn, typeof(string));
            }
    
            /// <summary>
            /// Converts to shift tender line data rows.
            /// </summary>
            /// <param name="table">The shift tender line table.</param>
            /// <param name="shift">The current shift.</param>
            /// <param name="inventLocationDataAreaId">The invent location area id.</param>
            /// <returns>Tender line data row.</returns>
            private IEnumerable<DataRow> ConvertToShiftTenderLineDataRows(DataTable table, Shift shift, string inventLocationDataAreaId)
            {
                List<DataRow> rows = new List<DataRow>();
                foreach (ShiftTenderLine tenderLine in shift.TenderLines)
                {
                    DataRow row = table.NewRow();
    
                    // The inconsistent mapping was taken from legacy POS code.
                    row[AddToTenderAmountColumnName] = tenderLine.FloatingEntryAmountOfStoreCurrency;
                    row[AddToTenderAmountCurColumnName] = tenderLine.FloatingEntryAmountOfTenderCurrency;
    
                    row[BankDropAmountColumnName] = tenderLine.BankDropAmountOfStoreCurrency;
                    row[BankDropAmountCurColumnName] = tenderLine.BankDropAmountOfTenderCurrency;
                    row[CardTypeIdColumnName] = tenderLine.CardTypeId;
                    row[ChangeAmountColumnName] = tenderLine.ChangeAmountOfStoreCurrency;
                    row[ChangeAmountCurColumnName] = tenderLine.ChangeAmountOfTenderCurrency;
                    row[ChannelRecordIdColumn] = shift.StoreRecordId;
                    row[CountColumnName] = tenderLine.Count;
                    row[CountingRequiredColumnName] = Convert.ToInt32(tenderLine.CountingRequired);
                    row[CurrencyColumnName] = tenderLine.TenderCurrency;
                    row[DeclareTenderAmountColumnName] = tenderLine.DeclareTenderAmountOfStoreCurrency;
                    row[DeclareTenderAmountCurColumnName] = tenderLine.DeclareTenderAmountOfTenderCurrency;
                    row[RemoveTenderAmountColumnName] = tenderLine.RemoveFromTenderAmountOfStoreCurrency;
                    row[RemoveTenderAmountCurColumnName] = tenderLine.RemoveFromTenderAmountOfTenderCurrency;
                    row[SafeDropAmountColumnName] = tenderLine.SafeDropAmountOfStoreCurrency;
                    row[SafeDropAmountCurColumnName] = tenderLine.SafeDropAmountOfTenderCurrency;
                    row[ShiftIdColumn] = shift.ShiftId;
                    row[StartingAmountColumnName] = tenderLine.StartingAmountOfStoreCurrency;
                    row[StartingAmountCurColumnName] = tenderLine.StartingAmountOfTenderCurrency;
                    row[StoreIdColumn] = shift.StoreId;
                    row[TenderedAmountColumnName] = tenderLine.TenderedAmountOfStoreCurrency;
                    row[TenderedAmountCurColumnName] = tenderLine.TenderedAmountOfTenderCurrency;
                    row[TenderTypeIdColumnName] = tenderLine.TenderTypeId;
                    row[TerminalIdColumn] = shift.TerminalId;
                    row[DataAreaIdColumn] = inventLocationDataAreaId;
    
                    rows.Add(row);
                }
    
                return rows;
            }
    
            /// <summary>
            /// Converts to shift account line table rows.
            /// </summary>
            /// <param name="table">The shift account line table.</param>
            /// <param name="shift">The current shift.</param>
            /// <param name="inventLocationDataAreaId">The invent location area id.</param>
            /// <returns>Account lines data row.</returns>
            private IEnumerable<DataRow> ConvertToShiftAccountLineDataRows(DataTable table, Shift shift, string inventLocationDataAreaId)
            {
                List<DataRow> rows = new List<DataRow>();
                foreach (ShiftAccountLine accountLine in shift.AccountLines)
                {
                    DataRow row = table.NewRow();
    
                    row[AccountNumberColumn] = accountLine.AccountNumber;
                    row[AccountTypeColumn] = accountLine.AccountTypeValue;
                    row[AmountColumn] = accountLine.Amount;
                    row[ShiftIdColumn] = shift.ShiftId;
                    row[StoreIdColumn] = shift.StoreId;
                    row[TerminalIdColumn] = shift.TerminalId;
                    row[ChannelRecordIdColumn] = shift.StoreRecordId;
                    row[DataAreaIdColumn] = inventLocationDataAreaId;
    
                    rows.Add(row);
                }
    
                return rows;
            }
        }
    }
}
