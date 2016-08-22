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
    namespace Commerce.Runtime.DataAccess.SqlServer
    {
        using System;
        using System.Collections.Concurrent;
        using System.Data;
        using System.Data.SqlClient;
        using System.Data.SqlTypes;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using DataColumn = Microsoft.Dynamics.Commerce.Runtime.Data.Types.DataColumn;
        using DataRow = Microsoft.Dynamics.Commerce.Runtime.Data.Types.DataRow;
        using DataSet = Microsoft.Dynamics.Commerce.Runtime.Data.Types.DataSet;
        using DataTable = Microsoft.Dynamics.Commerce.Runtime.Data.Types.DataTable;

        /// <summary>
        /// Helper class for type conversions.
        /// </summary>
        public static class SqlTypeHelper
        {
            /// <summary>
            /// Minimum value of <see cref="DateTimeOffset"/> that can be stored in SQL server.
            /// </summary>
            internal static readonly DateTimeOffset MinSqlDateTimeOffset = new DateTimeOffset(SqlDateTime.MinValue.Value, TimeSpan.Zero);
    
            /// <summary>
            /// Maximum value of <see cref="DateTimeOffset"/> that can be stored in SQL server.
            /// </summary>
            internal static readonly DateTimeOffset MaxSqlDateTimeOffset = new DateTimeOffset(SqlDateTime.MaxValue.Value, TimeSpan.Zero);
            
            /// <summary>
            /// Cached schema data table collection of SQL Server User Defined Table Type(UDTT).
            /// </summary>
            private static ConcurrentDictionary<string, System.Data.DataTable> cachedEmptySystemTables = new ConcurrentDictionary<string, System.Data.DataTable>();
    
            /// <summary>
            /// Converts a <see cref="Data.Types.DataTable"/> into a <see cref="System.Data.DataTable"/>.
            /// </summary>
            /// <param name="connection">The database connection.</param>
            /// <param name="dataTable">The data table to be converted.</param>
            /// <returns>The converted data table.</returns>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "To be disposed by caller.")]
            public static System.Data.DataTable ConvertToSystemDataTable(SqlConnection connection, DataTable dataTable)
            {
                ThrowIf.Null(connection, "connection");
                ThrowIf.Null(dataTable, "dataTable");
    
                System.Data.DataTable convertedDataTable = GetConvertedSystemDataTable(connection, dataTable);
    
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    var row = convertedDataTable.NewRow();
    
                    for (int i = 0; i < convertedDataTable.Columns.Count; i++)
                    {
                        // manage managed to sql conversions
                        row[i] = ConvertToDatabaseValue(dataRow[i]);
                    }
    
                    convertedDataTable.Rows.Add(row);
                }
    
                return convertedDataTable;
            }
    
            /// <summary>
            /// Maps a SQL server exception to <see cref="DatabaseException"/> and throws it.
            /// </summary>
            /// <param name="sqlException">The SQL exception.</param>
            internal static void HandleException(SqlException sqlException)
            {
                if (IsConnectionError(sqlException))
                {
                    throw new DatabaseException(DatabaseErrorCode.ConnectionError, sqlException.Message, sqlException);
                }
    
                // Clean the cached table schema since the target database schema has been changed.
                // This could happen in N+1 schema upgrade scenario.
                if (IsSchemaMismatchedError(sqlException))
                {
                    cachedEmptySystemTables.Clear();
                }
    
                throw new DatabaseException(DatabaseErrorCode.Unknown, sqlException.Message, sqlException);
            }
    
            /// <summary>
            /// Converts a SQL Server value into a managed value.
            /// </summary>
            /// <param name="databaseValue">The SQL server value.</param>
            /// <returns>The managed value.</returns>
            internal static object ConvertToManagedValue(object databaseValue)
            {
                if (databaseValue == DBNull.Value)
                {
                    return null;
                }
    
                return databaseValue;
            }
    
            /// <summary>
            /// Converts a managed value into SQL Server value.
            /// </summary>
            /// <param name="managedValue">The managed value.</param>
            /// <returns>The SQL Server value.</returns>
            internal static object ConvertToDatabaseValue(object managedValue)
            {
                if (managedValue == null)
                {
                    return DBNull.Value;
                }
                else if (managedValue is DateTime)
                {
                    DateTime dateTime = (DateTime)managedValue;
                    if (dateTime < SqlDateTime.MinValue.Value)
                    {
                        dateTime = SqlDateTime.MinValue.Value;
                    }
                    else if (dateTime > SqlDateTime.MaxValue.Value)
                    {
                        dateTime = SqlDateTime.MaxValue.Value;
                    }
    
                    return dateTime;
                }
                else if (managedValue is DateTimeOffset)
                {
                    DateTimeOffset dateTime = (DateTimeOffset)managedValue;
                    if (dateTime < MinSqlDateTimeOffset)
                    {
                        dateTime = MinSqlDateTimeOffset;
                    }
                    else if (dateTime > MaxSqlDateTimeOffset)
                    {
                        dateTime = MaxSqlDateTimeOffset;
                    }
    
                    return dateTime;
                }
    
                return managedValue;
            }
    
            /// <summary>
            /// Tries to convert <paramref name="value"/> into a <see cref="Data.Types.DataTable"/>.
            /// </summary>
            /// <param name="value">The value object to be tested.</param>
            /// <param name="table">The output of value as a <see cref="Data.Types.DataTable"/>.</param>
            /// <returns>Whether <paramref name="value"/> can be converted to a <see cref="Data.Types.DataTable"/> or not.</returns>
            internal static bool TryGetTable(object value, out Microsoft.Dynamics.Commerce.Runtime.Data.Types.DataTable table)
            {
                TableType type = value as TableType;
    
                if (type != null)
                {
                    table = type.DataTable;
                    return true;
                }
    
                table = value as Microsoft.Dynamics.Commerce.Runtime.Data.Types.DataTable;
                return table != null;
            }
    
            private static bool IsConnectionError(SqlException exception)
            {
                var connectionErrorEvents = SqlErrorCodes.GetAllEventIdsByType(SqlErrorType.ConnectionError).ToList();
    
                return connectionErrorEvents.Contains(exception.Number) || 
                    exception.ContainsInnerException<SqlException>(e => connectionErrorEvents.Contains(e.Number));
            }
    
            /// <summary>
            /// Gets a flag indicating whether the SQL exception object contains database schema mismatched error type.
            /// </summary>
            /// <param name="exception">The SQL exception object.</param>
            /// <returns>A boolean value.</returns>
            private static bool IsSchemaMismatchedError(SqlException exception)
            {
                var connectionErrorEvents = SqlErrorCodes.GetAllEventIdsByType(SqlErrorType.SchemaMismatched).ToList();
    
                return connectionErrorEvents.Contains(exception.Number) ||
                    exception.ContainsInnerException<SqlException>(e => connectionErrorEvents.Contains(e.Number));
            }
    
            /// <summary>
            /// Gets system data table contains schema definition only.
            /// </summary>
            /// <param name="connection">The SQL connection object.</param>
            /// <param name="dataTable">The object as a <see cref="Data.Types.DataTable"/>.</param>
            /// <returns>The system data table contains schema definition.</returns>
            private static System.Data.DataTable GetConvertedSystemDataTable(SqlConnection connection, DataTable dataTable)
            {
                if (cachedEmptySystemTables.ContainsKey(dataTable.TableName))
                {
                    return cachedEmptySystemTables[dataTable.TableName].Clone();
                }
    
                System.Data.DataTable systemDataTable;
                using (var command = connection.CreateCommand())
                {
                    systemDataTable = new System.Data.DataTable(dataTable.TableName);
                    systemDataTable.Locale = CultureInfo.InvariantCulture;
    
                    command.Parameters.AddWithValue("@schemaName", SqlServerDatabaseProvider.CrtDatabaseSchemaName);
                    command.Parameters.AddWithValue("@tableName", systemDataTable.TableName);
                    
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT * FROM [crt].[GetUdttColumns](@schemaName, @tableName)";
    
                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                var type = SqlTypeHelper.GetClrType((string)dataReader["TABLE_TYPE_COL_DATATYPE"]);
                                var name = (string)dataReader["TABLE_TYPE_COL_NAME"];
                                var dataColumn = new System.Data.DataColumn(name, type);
    
                                systemDataTable.Columns.Add(dataColumn);
                            }
                        }
                    }
                }
    
                cachedEmptySystemTables[dataTable.TableName] = systemDataTable;
                return systemDataTable.Clone();
            }
    
            /// <summary>
            /// Gets the matched CLR type for a given SQL database type.
            /// </summary>
            /// <param name="sqlDbType">The SQL database type.</param>
            /// <returns>A CLR type.</returns>
            private static Type GetClrType(string sqlDbType)
            {
                SqlDbType converted;
    
                // Handles "numeric" type, since it does not exist in enum SqlDbType by default.
                if (sqlDbType.Equals("numeric", StringComparison.OrdinalIgnoreCase))
                {
                    converted = SqlDbType.Decimal;
                }
                else
                {
                    if (!Enum.TryParse<SqlDbType>(sqlDbType, true, out converted))
                    {
                        throw new ArgumentOutOfRangeException(string.Format("SqlDbType string: {0} is not a member of the SqlDbType enumeration.", sqlDbType));
                    }
                }
    
                switch (converted)
                {
                    case SqlDbType.BigInt:
                        return typeof(long);
    
                    case SqlDbType.Binary:
                    case SqlDbType.Image:
                    case SqlDbType.Timestamp:
                    case SqlDbType.VarBinary:
                        return typeof(byte[]);
    
                    case SqlDbType.Bit:
                        return typeof(bool);
    
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Text:
                    case SqlDbType.VarChar:
                    case SqlDbType.Xml:
                        return typeof(string);
    
                    case SqlDbType.DateTime:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.Date:
                    case SqlDbType.Time:
                    case SqlDbType.DateTime2:
                        return typeof(DateTime);
    
                    case SqlDbType.Decimal:
                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                        return typeof(decimal);
    
                    case SqlDbType.Float:
                        return typeof(double);
    
                    case SqlDbType.Int:
                        return typeof(int);
    
                    case SqlDbType.Real:
                        return typeof(float);
    
                    case SqlDbType.UniqueIdentifier:
                        return typeof(Guid);
    
                    case SqlDbType.SmallInt:
                        return typeof(short);
    
                    case SqlDbType.TinyInt:
                        return typeof(byte);
    
                    case SqlDbType.Variant:
                    case SqlDbType.Udt:
                        return typeof(object);
    
                    case SqlDbType.Structured:
                        return typeof(DataTable);
    
                    case SqlDbType.DateTimeOffset:
                        return typeof(DateTimeOffset);
    
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("SqlDbType: {0} is not supported.", converted));
                }
            }
        }
    }
}
