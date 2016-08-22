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
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.IO;
        using System.IO.Compression;
        using System.Linq;
        using System.Xml.Linq;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Client offline transaction data service class.
        /// </summary>
        public class ClientOfflineTransactionSqliteDataService : IRequestHandler
        {
            internal static readonly string MainRetailTransactionTableName = "AX_RETAILTRANSACTIONTABLE";
    
            private const string NumberOfTransactionsParameter = "@NumberOfTransactions";
            private const string TransactionIdParameter = "@TransactionId";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetOfflineTransactionIdsDataRequest),
                        typeof(GetOfflineTransactionsDataRequest),
                        typeof(PurgeOfflineTransactionsDataRequest),
                        typeof(GetOfflineTransactionCountDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            /// <exception cref="System.NotSupportedException">The request type is not supported.</exception>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                using (SqliteDatabaseContext databaseContext = new SqliteDatabaseContext(request.RequestContext))
                {
                    if (requestType == typeof(GetOfflineTransactionIdsDataRequest))
                    {
                        response = this.GetOfflineTransactionIds(databaseContext, ((GetOfflineTransactionIdsDataRequest)request).NumberOfTransactions);
                    }
                    else if (requestType == typeof(GetOfflineTransactionsDataRequest))
                    {
                        response = this.GetOfflineTransactions(databaseContext, ((GetOfflineTransactionsDataRequest)request).TransactionIds);
                    }
                    else if (requestType == typeof(PurgeOfflineTransactionsDataRequest))
                    {
                        response = this.PurgeOfflineTransactions(databaseContext, ((PurgeOfflineTransactionsDataRequest)request).TransactionIds);
                    }
                    else if (requestType == typeof(GetOfflineTransactionCountDataRequest))
                    {
                        response = this.GetOfflineTransactionCount(databaseContext);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                    }
                }
    
                return response;
            }
    
            #region Private methods
    
            private static byte[] CompressString(string sourceString)
            {
                if (sourceString == null)
                {
                    return null;
                }
    
                MemoryStream outputStream = new MemoryStream();
                GZipStream compressionStream = new GZipStream(outputStream, CompressionMode.Compress);
                using (StreamWriter sw = new StreamWriter(compressionStream))
                {
                    sw.Write(sourceString);
                }
    
                return outputStream.ToArray();
            }
    
            private NullResponse PurgeOfflineTransactions(SqliteDatabaseContext databaseContext, IEnumerable<string> transactionIds)
            {
                using (var transaction = databaseContext.BeginTransaction())
                {
                    string purgeMainTransactionTableStatement = string.Format("DELETE FROM {0} WHERE TRANSACTIONID = {1} COLLATE NOCASE;", MainRetailTransactionTableName, TransactionIdParameter);
                    foreach (string transactionId in transactionIds)
                    {
                        SqlQuery query = new SqlQuery(purgeMainTransactionTableStatement);
                        query.Parameters.Add(TransactionIdParameter, transactionId);
                        databaseContext.ExecuteNonQuery(query);
                    }
    
                    foreach (string tableName in this.GetSatelliteRetailTransactionTableList(databaseContext))
                    {
                        string purgeSatelliteTransactionTableStatement = string.Format("DELETE FROM {0} WHERE TRANSACTIONID = {1} COLLATE NOCASE;", tableName, TransactionIdParameter);
                        foreach (string transactionId in transactionIds)
                        {
                            SqlQuery query = new SqlQuery(purgeSatelliteTransactionTableStatement);
                            query.Parameters.Add(TransactionIdParameter, transactionId);
                            databaseContext.ExecuteNonQuery(query);
                        }
                    }
    
                    transaction.Commit();
                }
    
                return new NullResponse();
            }
    
            private GetOfflineTransactionIdsDataResponse GetOfflineTransactionIds(SqliteDatabaseContext databaseContext, int numberOfTransactions)
            {
                ReadOnlyCollection<string> transactionIds;
    
                string queryString = string.Format("SELECT TRANSACTIONID FROM {0} LIMIT {1};", MainRetailTransactionTableName, NumberOfTransactionsParameter);
    
                SqlQuery query = new SqlQuery(queryString);
                query.Parameters.Add(NumberOfTransactionsParameter, numberOfTransactions);
    
                transactionIds = databaseContext.ExecuteScalarCollection<string>(query);
    
                GetOfflineTransactionIdsDataResponse response = new GetOfflineTransactionIdsDataResponse(new ReadOnlyCollection<string>(transactionIds));
                return response;
            }
    
            private GetOfflineTransactionsDataResponse GetOfflineTransactions(SqliteDatabaseContext databaseContext, IEnumerable<string> transactionIds)
            {
                XElement root = new XElement("OfflineTransactions");
    
                root.Add(this.ReadTransactionTable(databaseContext, MainRetailTransactionTableName, transactionIds));
                foreach (string tableName in this.GetSatelliteRetailTransactionTableList(databaseContext))
                {
                    root.Add(this.ReadTransactionTable(databaseContext, tableName, transactionIds));
                }
    
                GetOfflineTransactionsDataResponse response = new GetOfflineTransactionsDataResponse(CompressString(root.ToString()));
                return response;
            }
    
            private XElement ReadTransactionTable(SqliteDatabaseContext databaseContext, string tableName, IEnumerable<string> transactionIds)
            {
                XElement table = new XElement(tableName);
                XElement tableRow;
    
                SqliteTableSchema tableSchemaInfo = databaseContext.GetTableSchemaInfo(tableName);
    
                foreach (string transactionId in transactionIds)
                {
                    // To prevent SQL injection, we need to use parameterized SQL as followed.
                    string queryTransactionStatement = string.Format("SELECT * FROM {0} WHERE TRANSACTIONID = {1} COLLATE NOCASE;", tableName, TransactionIdParameter);
                    SqlQuery query = new SqlQuery(queryTransactionStatement);
                    query.Parameters.Add(TransactionIdParameter, transactionId);
    
                    try
                    {
                        using (var resultSet = databaseContext.ExecuteQuery(query))
                        {
                            while (resultSet.Read())
                            {
                                List<XAttribute> attributes = new List<XAttribute>();
                                for (int i = 0; i < resultSet.FieldCount; ++i)
                                {
                                    if (resultSet.GetValue<object>(i) == null)
                                    {
                                        attributes.Add(new XAttribute(resultSet.GetName(i).ToUpperInvariant(), string.Empty));
                                    }
                                    else
                                    {
                                        Type expectedManagedType = tableSchemaInfo.ColumnsByColumnName[resultSet.GetName(i).ToUpperInvariant()].ManagedType;
                                        attributes.Add(new XAttribute(resultSet.GetName(i).ToUpperInvariant(), resultSet.GetValue(i, expectedManagedType)));
                                    }
                                }
    
                                tableRow = new XElement("ROW", attributes);
                                table.Add(tableRow);
                            }
                        }
                    }
                    catch (DatabaseException ex)
                    {
                        throw new StorageException(
                            StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError,
                            (int)ex.ErrorCode,
                            ex,
                            "Cannot read transaction data from transaction tables in the underlying SQLite database. See inner exception for details.");
                    }
                }
    
                return table;
            }
    
            private GetOfflineTransactionCountDataResponse GetOfflineTransactionCount(SqliteDatabaseContext databaseContext)
            {
                string queryString = string.Format("SELECT COUNT(TRANSACTIONID) FROM {0};", MainRetailTransactionTableName);
                int offlineTransactionsCount = (int)databaseContext.ExecuteScalar<int>(new SqlQuery(queryString));
    
                return new GetOfflineTransactionCountDataResponse(offlineTransactionsCount);
            }
    
            private IEnumerable<string> GetSatelliteRetailTransactionTableList(SqliteDatabaseContext databaseContext)
            {
                IEnumerable<string> tablesInOfflineTransactionDb = this.GetTablesInOfflineTransactionDatabase(databaseContext);
                IEnumerable<string> tablesInUploadSyncScopes = this.GetTablesInUploadSyncScopes(databaseContext);
    
                // For Windows Phone, tables to be uploaded from offline transaction database to retail server database are a subset of tables in upload sync scopes.
                // These tables satisfy a property: there is one main transaction table which stores header information; while all other tables stores satellite data and linked
                // to the main transaction table by transaction identifier.
                List<string> satelliteRetailTransactionTableList = tablesInUploadSyncScopes.Intersect(tablesInOfflineTransactionDb).ToList();
    
                // Removes main transaction table from this list.
                satelliteRetailTransactionTableList.Remove(MainRetailTransactionTableName);
    
                // Sorts table names in alphabetical order.
                satelliteRetailTransactionTableList.Sort();
    
                return satelliteRetailTransactionTableList;
            }
    
            private IEnumerable<string> GetTablesInOfflineTransactionDatabase(SqliteDatabaseContext databaseContext)
            {
                const int IndexOfName = 1;
                List<string> databaseList = new List<string>();
                SqlQuery queryDatabaseList = new SqlQuery("PRAGMA database_list;");
    
                try
                {
                    using (var result = databaseContext.ExecuteQuery(queryDatabaseList))
                    {
                        while (result.Read())
                        {
                            string databaseName = result.GetValue<string>(IndexOfName);
    
                            if (string.Compare(databaseName, "MAIN", StringComparison.OrdinalIgnoreCase) != 0 &&
                                string.Compare(databaseName, "TEMP", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                databaseList.Add(databaseName);
                            }
                        }
                    }
                }
                catch (DatabaseException exception)
                {
                    throw new StorageException(
                        StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError,
                        (int)exception.ErrorCode,
                        exception,
                        "Failed to read from the database. See inner exception for details");
                }
    
                // There should be only 1 attached database associated with the connection. Test AssertSqliteDatabaseIntegrity checks this property.
                string offlineTransactionDatabaseAlias = databaseList[0];
    
                SqlQuery queryTableList = new SqlQuery(string.Format("SELECT NAME FROM [{0}].[SQLITE_MASTER] WHERE TYPE = 'table';", offlineTransactionDatabaseAlias));
    
                ReadOnlyCollection<string> tableList = databaseContext.ExecuteScalarCollection<string>(queryTableList);
                List<string> formalizedTableList = new List<string>();
                foreach (string tableName in tableList)
                {
                    formalizedTableList.Add(tableName.ToUpperInvariant());
                }
    
                return formalizedTableList;
            }
    
            private IEnumerable<string> GetTablesInUploadSyncScopes(SqliteDatabaseContext databaseContext)
            {
                string queryString = string.Format(
                    @"SELECT t6.TABLENAME AS TABLENAME
                      FROM ax_RETAILSTORETABLE AS t1 INNER JOIN
                           ax_RETAILOFFLINEPROFILE AS t2 ON t1.STORENUMBER = '{0}' AND t1.OFFLINEPROFILE = t2.RECID INNER JOIN
                           ax_RETAILOFFLINEPROFILESCOPES AS t3 ON t2.RECID = t3.PROFILEID INNER JOIN
                           ax_RETAILOFFLINESCOPE AS t4 ON t4.RECID = t3.SCOPEID AND t4.SYNCDIRECTION = 1 INNER JOIN
                           ax_RETAILOFFLINESCOPETABLES AS t5 ON t4.RECID = t5.SCOPEID INNER JOIN
                           ax_RETAILOFFLINETABLE AS t6 ON t5.SYNCTABLEID = t6.RECID",
                    databaseContext.StoreNumber);
    
                ReadOnlyCollection<string> tableList = databaseContext.ExecuteScalarCollection<string>(new SqlQuery(queryString));
                List<string> result = new List<string>();
                foreach (string tableName in tableList)
                {
                    result.Add(tableName.Replace("[", string.Empty)
                                        .Replace("]", string.Empty)
                                        .Replace(".", "_"));
                }
    
                return result;
            }
    
            #endregion
        }
    }
}
