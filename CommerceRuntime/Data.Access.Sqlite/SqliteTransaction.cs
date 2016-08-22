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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;

        /// <summary>
        /// Represents a transaction in the SQLite database.
        /// </summary>
        /// <remarks>
        /// The nested transaction implementation follows the constraints described here <a href="http://technet.microsoft.com/en-us/library/ms189336(v=sql.105).aspx"/>.
        /// This class provides no support for multithreading.
        /// </remarks>
        internal sealed class SqliteTransaction : IDatabaseTransaction
        {
            private const string BeginTransactionQueryString = "BEGIN TRANSACTION;";
            private const string CommitTransactionQueryString = "COMMIT TRANSACTION;";
            private const string RollbackTransactionQueryString = "ROLLBACK TRANSACTION;";

            /// <summary>
            /// Represents whether this transaction has been finalized (committed or rolled back).
            /// </summary>
            private bool isFinalized;

            /// <summary>
            /// Represents whether the transaction in the connection has been initialed on this transaction object or not.
            /// </summary>
            private bool transactionInitiated;

            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteTransaction"/> class.
            /// </summary>
            /// <param name="databaseConnection">The database connection.</param>
            internal SqliteTransaction(DatabaseConnection databaseConnection)
            {
                if (databaseConnection == null)
                {
                    throw new ArgumentNullException("databaseConnection");
                }

                this.DatabaseConnection = databaseConnection;
                this.isFinalized = false;
                this.BeginTransaction();
            }

            /// <summary>
            /// Gets or sets the database connection.
            /// </summary>
            private DatabaseConnection DatabaseConnection
            {
                get;
                set;
            }

            /// <summary>
            /// Commit the changes made during the transaction.
            /// </summary>
            public void Commit()
            {
                // if the connection is not under transaction, it means a rollback has happened
                if (!this.DatabaseConnection.IsUnderTransaction)
                {
                    throw new DatabaseException(DatabaseErrorCode.Unknown, "An inner transaction was rolled back, this transaction cannot be committed.");
                }

                // if this transaction object initialized the connection, then it should be the one issuing a commit statement
                if (this.transactionInitiated)
                {
                    this.ExecuteQuery(CommitTransactionQueryString);
                    this.DatabaseConnection.IsUnderTransaction = false;
                }

                this.isFinalized = true;
            }

            /// <summary>
            /// Rolls back the transaction.
            /// </summary>
            public void Rollback()
            {
                // if rollback happens, we need to issue the rollback statement
                if (this.DatabaseConnection.IsUnderTransaction)
                {
                    this.ExecuteQuery(RollbackTransactionQueryString);
                    this.DatabaseConnection.IsUnderTransaction = false;
                }

                this.isFinalized = true;
            }

            /// <summary>
            /// Ends the transaction. If it hasn't been finalized (committed or rolled back) then it will be rolled back.
            /// </summary>
            public void Dispose()
            {
                if (!this.isFinalized)
                {
                    this.Rollback();
                }
            }

            /// <summary>
            /// Begins the transaction.
            /// </summary>
            private void BeginTransaction()
            {
                if (!this.DatabaseConnection.IsUnderTransaction)
                {
                    // actually initializes the transaction
                    this.ExecuteQuery(BeginTransactionQueryString);

                    // marks that the connection is now under transaction
                    this.DatabaseConnection.IsUnderTransaction = true;

                    // marks that the transaction has been initialized inside this transaction object
                    this.transactionInitiated = true;
                }
            }

            /// <summary>
            /// Executes a query string in the database.
            /// </summary>
            /// <param name="query">The query string to be executed.</param>
            private void ExecuteQuery(string query)
            {
                var provider = new SqliteDatabaseProvider();
                provider.ExecuteNonQuery(this.DatabaseConnection, new SqlQuery(query));
            }
        }
    }
}
