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
    namespace Commerce.Runtime.Data.Sqlite
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;

        /// <summary>
        /// Represents a temporary table in the database.
        /// </summary>
        public sealed class TempTable : IDisposable
        {
            private IDatabaseConnection connection;

            /// <summary>
            /// Prevents a default instance of the <see cref="TempTable"/> class from being created.
            /// </summary>
            private TempTable()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TempTable"/> class.
            /// </summary>
            /// <param name="table">The table definition for the temporary table.</param>
            /// <param name="connection">The database connection in which this table should be created.</param>
            private TempTable(DataTable table, IDatabaseConnection connection)
            {
                this.Table = table;
                this.connection = connection;
            }

            /// <summary>
            /// Gets the table name for this <see cref="TempTable"/>.
            /// </summary>
            public string TableName
            {
                get
                {
                    return this.Table.TableName;
                }
            }

            /// <summary>
            /// Gets the column definition for the data table.
            /// </summary>
            public ReadOnlyCollection<DataColumn> Columns
            {
                get
                {
                    return new ReadOnlyCollection<DataColumn>(this.Table.Columns);
                }
            }

            /// <summary>
            /// Gets or sets the table that represents the temporary table.
            /// </summary>
            private DataTable Table
            {
                get;
                set;
            }

            /// <summary>
            /// Creates a temporary table in the database and copy any rows present in <see cref="DataTable.Rows"/> to the temporary table.
            /// </summary>
            /// <param name="table">The table definition for the temporary table.</param>
            /// <param name="connection">The database connection in which this table should be created.</param>
            /// <returns>An instance of the temporary table.</returns>
            public static TempTable CreateTemporaryTable(DataTable table, IDatabaseConnection connection)
            {
                if (table == null)
                {
                    throw new ArgumentNullException("table");
                }

                if (string.IsNullOrWhiteSpace(table.TableName))
                {
                    throw new ArgumentException("table.TableName must be provided.", "table");
                }

                if (table.Columns.Count == 0)
                {
                    throw new ArgumentException("table.Columns must not be empty.", "table");
                }

                // creates instance of temp table object
                TempTable tempTable = new TempTable(table, connection);

                // materialize the temp table in the database
                tempTable.ExecuteCreateTable();

                // populate it with any input data
                tempTable.ExecutePopulateTable();

                return tempTable;
            }

            /// <summary>
            /// Drops the temporary table in the database.
            /// </summary>
            public void Dispose()
            {
                if (this.connection != null && this.Table != null)
                {
                    this.ExecuteDropTable();
                }

                if (this.Table != null)
                {
                    this.Table.Dispose();
                }

                this.connection = null;
                this.Table = null;
            }

            /// <summary>
            /// Formats the column for the create table statement.
            /// </summary>
            /// <param name="column">The column to be formatted into a string.</param>
            /// <returns>The formatted string.</returns>
            private static string FormatColumn(DataColumn column)
            {
                return string.Format("{0} {1}", column.ColumnName, SqlTypeHelper.GetSqliteTypeNameFromManagedType(column.DataType));
            }

            /// <summary>
            /// Assembles and executes a drop table statement.
            /// </summary>
            private void ExecuteDropTable()
            {
                using (var provider = new SqliteDatabaseProvider())
                {
                    var deleteTempTableQuery = new SqlQuery("DROP TABLE IF EXISTS {0};", this.Table.TableName);
                    provider.ExecuteNonQuery(this.connection, deleteTempTableQuery);
                }
            }

            /// <summary>
            /// Assembles and executes a create table statement.
            /// </summary>        
            private void ExecuteCreateTable()
            {
                const string CreateTempTable = "CREATE TEMP TABLE {0} ({1});";

                string columnString = string.Join(", ", this.Table.Columns.Select(column => FormatColumn(column)));

                using (var provider = new SqliteDatabaseProvider())
                {
                    var deleteTempTableQuery = new SqlQuery(CreateTempTable, this.Table.TableName, columnString);
                    provider.ExecuteNonQuery(this.connection, deleteTempTableQuery);
                }
            }

            /// <summary>
            /// Populates the temporary table in the database with the records presents in the <see cref="Table"/>.
            /// </summary>
            private void ExecutePopulateTable()
            {
                const string InsertTable = "INSERT INTO {0} ({1}) VALUES ({2});";

                // for multiple row insertion, begining a transaction improves performance, because sqlite doesn't need to start
                // a new transaction for every single insert.
                using (var provider = new SqliteDatabaseProvider())
                using (IDatabaseTransaction transaction = this.connection.BeginTransaction())
                {
                    this.Table.ExecuteNonQuery(this.connection, provider, InsertTable);

                    transaction.Commit();
                }
            }
        }
    }
}
