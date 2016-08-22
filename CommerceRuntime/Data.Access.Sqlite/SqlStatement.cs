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
    namespace Commerce.Runtime.DataAccess.Sqlite
    {
        using System;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using SQLitePCL;

        /// <summary>
        /// Represents a SQL statement.
        /// </summary>
        internal sealed class SqlStatement : IDisposable
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SqlStatement"/> class.
            /// </summary>
            /// <param name="statement">The SQLite statement.</param>
            /// <param name="connection">The database connection.</param>
            public SqlStatement(ISQLiteStatement statement, DatabaseConnection connection)
            {
                this.SqliteStatement = statement;
                this.Connection = connection;
            }

            /// <summary>
            /// Gets the SQLite statement.
            /// </summary>
            public ISQLiteStatement SqliteStatement
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the database connection.
            /// </summary>
            public DatabaseConnection Connection
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether this statement is being used or not.
            /// </summary>
            public bool InUse
            {
                get;
                set;
            }

            /// <summary>
            /// Disposes the underlying SQLite statement.
            /// </summary>
            public void Dispose()
            {
                // do not dispose the connection since it has a different lifetime than the statement
                this.Connection = null;

                if (this.SqliteStatement != null)
                {
                    this.SqliteStatement.Dispose();
                    this.SqliteStatement = null;
                }
            }
        }
    }
}
