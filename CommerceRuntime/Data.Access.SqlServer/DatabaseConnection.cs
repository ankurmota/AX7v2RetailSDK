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
        using System.Data.SqlClient;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Represents a database connection.
        /// </summary>
        internal sealed class DatabaseConnection : IDatabaseConnection
        {
            private const string HiddenConnectionStringValue = "[hidden]";
            private string connectionStringForTracing;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DatabaseConnection"/> class.
            /// </summary>
            /// <param name="sqlConnection">The SQL server connection.</param>
            public DatabaseConnection(SqlConnection sqlConnection)
            {
                ThrowIf.Null(sqlConnection, "sqlConnection");            
                this.SqlConnection = sqlConnection;
    
                SqlConnectionStringBuilder connectionStringParser = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
                this.DatabaseServer = connectionStringParser.DataSource;
                this.DatabaseName = connectionStringParser.InitialCatalog;
    
                if (!string.IsNullOrWhiteSpace(connectionStringParser.UserID))
                {
                    connectionStringParser.UserID = HiddenConnectionStringValue;
                }
    
                if (!string.IsNullOrWhiteSpace(connectionStringParser.Password))
                {
                    connectionStringParser.Password = HiddenConnectionStringValue;
                }
    
                this.connectionStringForTracing = connectionStringParser.ToString();
            }
    
            /// <summary>
            /// Gets the database server name.
            /// </summary>
            public string DatabaseServer
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the database name.
            /// </summary>
            public string DatabaseName
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the actual underlying connection implementation.
            /// </summary>
            internal SqlConnection SqlConnection
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Disposes the connection.
            /// </summary>
            public void Dispose()
            {
                if (this.SqlConnection == null)
                {
                    return;
                }
    
                try
                {
                    this.SqlConnection.Close();
                    this.SqlConnection.Dispose();
                    this.SqlConnection = null;
                }
                catch (SqlException sqlException)
                {
                    SqlTypeHelper.HandleException(sqlException);
                }
            }
    
            /// <summary>
            /// Opens the connection on the database.
            /// </summary>
            /// <remarks>This operation must be performed on the connection before any other action can be executed using the connection.
            /// The connection should be disposed calling <see cref="Dispose"/> on this object.</remarks>
            public void Open()
            {
                Guid correlationId = Guid.NewGuid();
                RetailLogger.Log.CrtDataOpenConnectionStart(correlationId, this.connectionStringForTracing);
    
                try
                {
                    this.SqlConnection.Open();
                    RetailLogger.Log.CrtDataOpenConnectionEnd(correlationId);
                }
                catch (SqlException sqlException)
                {
                    RetailLogger.Log.CrtDataOpenConnectionFailure(correlationId, sqlException);
                    SqlTypeHelper.HandleException(sqlException);
                }
            }
    
            /// <summary>
            /// Begins a transaction on this connection.
            /// </summary>
            /// <returns>The transaction object.</returns>
            public IDatabaseTransaction BeginTransaction()
            {
                return new DatabaseTransaction(this);
            }
        }
    }
}
