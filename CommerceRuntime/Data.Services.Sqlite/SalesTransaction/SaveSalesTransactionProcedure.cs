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
        using System;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The SQLite implementation of sales transaction save.
        /// </summary>
        internal sealed class SaveSalesTransactionProcedure
        {
            private const string SalesTransactionTableName = "crt.SALESTRANSACTION";
            private const string BeginDateTimeColumName = "BEGINDATETIME";
            private const string CreatedDateTimeColumName = "CREATEDDATETIME";
            private const string ModifiedDateTimeColumnName = "MODIFIEDDATETIME";
            private const string RowversionColumnName = "ROWVERSION";
            private SaveCartDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SaveSalesTransactionProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            public SaveSalesTransactionProcedure(SaveCartDataRequest request)
            {
                this.request = request;
            }
    
            public void Execute()
            {
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                using (var transaction = databaseContext.BeginTransaction())
                {
                    foreach (SalesTransaction salesTransaction in this.request.SalesTransactions)
                    {
                        if (!this.request.IgnoreRowVersionCheck)
                        {
                            // no out of the box rowversion support in sqlite, check row version before performing operation
                            DateTime transactionCreatedDateTime = RetrieveTransactionCreatedDateOrThrowOnInvalidRowVersion(databaseContext, salesTransaction.Id, salesTransaction.Version);
    
                            // updates the created datetime on the entity, since the update query will replace all columns in the db with the values present in the entity
                            salesTransaction.SetProperty(CreatedDateTimeColumName, new DateTimeOffset(transactionCreatedDateTime));
                        }
                    }
    
                    var salesTransactionDataCollection = SalesTransactionConverter.ConvertToData(
                        this.request.SalesTransactions,
                        serializeData: true);
    
                    using (DataTable table = CreateSalesTransactionDataTable())
                    {
                        foreach (SalesTransactionData salesTransactionData in salesTransactionDataCollection)
                        {
                            DateTimeOffset createdDateTimeOffset = this.request.SalesTransactions.First(t => t.Id == salesTransactionData.Id).BeginDateTime;
    
                            AddSalesTransactionDataRow(table, salesTransactionData, createdDateTimeOffset.DateTime);
                        }
    
                        // saves or updates records
                        databaseContext.SaveTable(table);
                    }
    
                    transaction.Commit();
                }
            }
    
            /// <summary>
            /// Checks whether the row version for a sales transaction is valid or not.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="transactionId">The sales transaction identifier.</param>
            /// <param name="rowversion">The row version value.</param>
            /// <remarks>An exception is thrown for a invalid row version.</remarks>
            /// <returns>Transaction's created date time.</returns>
            private static DateTime RetrieveTransactionCreatedDateOrThrowOnInvalidRowVersion(SqliteDatabaseContext context, string transactionId, byte[] rowversion)
            {
                const string GetRowVersionQueryString =
                    "SELECT {0}, {1} FROM [crt].SALESTRANSACTION WHERE TRANSACTIONID = @TRANSACTIONID;";
    
                var query = new SqlQuery(GetRowVersionQueryString, RowversionColumnName, CreatedDateTimeColumName);
                query.Parameters["@TRANSACTIONID"] = transactionId;
    
                byte[] savedRowverion = null;
    
                // default created time as utc now
                DateTime createdDateTime = DateTime.UtcNow;
    
                SalesTransactionData transactionData = context.ReadEntity<SalesTransactionData>(query).FirstOrDefault();
                if (transactionData != null)
                {
                    savedRowverion = transactionData.Version;
    
                    // update created date time value in case it exists in the db
                    createdDateTime = transactionData.CreatedDateTime.DateTime;
                }
    
                // null is fine since the record does not exist yet, otherwise compare to see if they match
                if (savedRowverion != null && !RowVersionHelper.AreEquals(rowversion, savedRowverion))
                {
                    throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectVersionMismatchError, "Version mismatch while saving transaction.");
                }
    
                return createdDateTime;
            }
    
            /// <summary>
            /// Creates a data table for sales transaction.
            /// </summary>
            /// <returns>The data table for the sales transaction table.</returns>
            private static DataTable CreateSalesTransactionDataTable()
            {
                DataTable table = new DataTable(SalesTransactionTableName);
    
                SalesTransactionData.FillSchemaForSave(table);
    
                // add fields not present in the default schema
                DataColumn createdDateTimeColumn = new DataColumn(CreatedDateTimeColumName, typeof(DateTime))
                {
                    DefaultValue = DateTime.UtcNow
                };
    
                table.Columns.Add(createdDateTimeColumn);
                table.Columns.Add(ModifiedDateTimeColumnName, typeof(DateTime));
    
                return table;
            }
    
            /// <summary>
            /// Creates and adds a data row based on the sales transaction data contents.
            /// </summary>
            /// <param name="table">The data table to be populated.</param>
            /// <param name="salesTransactionData">The sales transaction data.</param>
            /// <param name="createdDateTime">The transaction's created date time.</param>
            private static void AddSalesTransactionDataRow(DataTable table, SalesTransactionData salesTransactionData, DateTime createdDateTime)
            {
                // no out of the box rowversion support in sqlite, increment value here
                salesTransactionData.Version = RowVersionHelper.Increment(salesTransactionData.Version);
    
                DataRow row = salesTransactionData.CreateDataRow(table);
    
                // all fields must be set otherwise the insert or update query will clear them out
                row[CreatedDateTimeColumName] = createdDateTime;
                row[ModifiedDateTimeColumnName] = DateTime.UtcNow;
    
                table.Rows.Add(row);
            }
        }
    }
}
