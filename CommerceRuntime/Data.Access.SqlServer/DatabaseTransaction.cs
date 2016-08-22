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
        using System.Data.SqlClient;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        
        /// <summary>
        /// Represents a transaction on the database.
        /// </summary>
        internal class DatabaseTransaction : IDatabaseTransaction
        {
            private SqlTransaction transaction;
    
            /// <summary>
            /// Represents whether this transaction has been finalized (committed or rolled back).
            /// </summary>
            private bool isFinalized;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DatabaseTransaction"/> class.
            /// </summary>
            /// <param name="databaseConnection">The database connection.</param>
            public DatabaseTransaction(DatabaseConnection databaseConnection)
            {
                this.transaction = databaseConnection.SqlConnection.BeginTransaction();
                this.isFinalized = false;
            }
    
            /// <summary>
            /// Commit the changes made during the transaction.
            /// </summary>
            public void Commit()
            {
                this.transaction.Commit();
                this.isFinalized = true;
            }
    
            /// <summary>
            /// Rolls back the transaction.
            /// </summary>
            public void Rollback()
            {
                this.transaction.Rollback();
                this.isFinalized = true;
            }
    
            /// <summary>
            /// Ends the transaction. If it hasn't been finalized (committed or rolled back) then it will be rolled back.
            /// </summary>
            public void Dispose()
            {
                if (!this.isFinalized && this.transaction != null)
                {
                    this.Rollback();
                }
            }
        }
    }
}
