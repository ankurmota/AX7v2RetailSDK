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
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Wraps  common operations associated to database access, maintaining single database connection.
        /// </summary>
        internal class SqliteDatabaseContext : DatabaseContext
        {
            private int connectionElementUniqueIdentifier;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteDatabaseContext"/> class.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            public SqliteDatabaseContext(RequestContext requestContext)
                : base(requestContext)
            {
                ChannelConfiguration channelConfiguration = requestContext.GetChannelConfiguration();
    
                if (channelConfiguration != null)
                {
                    this.ChannelId = channelConfiguration.RecordId;
                    this.DataAreaId = channelConfiguration.InventLocationDataAreaId;
                }
    
                OrgUnit orgUnit = requestContext.GetOrgUnit();
    
                if (orgUnit != null)
                {
                    this.StoreNumber = orgUnit.OrgUnitNumber;
                }
    
                this.connectionElementUniqueIdentifier = 0;
                this.ShiftId = requestContext.GetPrincipal().ShiftId;
                this.ShiftTerminalId = requestContext.GetPrincipal().ShiftTerminalId;
    
                GetCurrentTerminalIdDataRequest dataRequest = new GetCurrentTerminalIdDataRequest();
                this.TerminalId = requestContext.Runtime.Execute<SingleEntityDataServiceResponse<string>>(dataRequest, requestContext, skipRequestTriggers: true).Entity;
            }
    
            /// <summary>
            /// Gets the store number.
            /// </summary>
            public string StoreNumber
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the current terminal identifier.
            /// </summary>
            public string TerminalId
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the shift terminal identifier.
            /// </summary>
            public string ShiftTerminalId
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the shift identifier.
            /// </summary>
            public long ShiftId
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the channel identifier for this context.
            /// </summary>
            public long ChannelId
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the data area identifier for this context.
            /// </summary>
            public string DataAreaId
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Gets the database provider.
            /// </summary>
            private new SqliteDatabaseProvider DatabaseProvider
            {
                get
                {
                    return (SqliteDatabaseProvider)base.DatabaseProvider;
                }
            }
    
            /// <summary>
            /// Gets a unique incremental number to identify a connection scope element.
            /// </summary>
            /// <returns>A a unique incremental number to identify a connection scope element.</returns>
            /// <remarks>Use this identifier to append to any database element that needs to be unique inside this context's connection.</remarks>
            public int GetNextContextIdentifier()
            {
                return this.connectionElementUniqueIdentifier++;
            }
    
            /// <summary>
            /// Creates a temporary table using this context's database connection.
            /// </summary>
            /// <param name="table">The data table to be created as a temporary table in the database.</param>
            /// <returns>The temporary table created.</returns>
            public TempTable CreateTemporaryTable(DataTable table)
            {
                return TempTable.CreateTemporaryTable(table, this.ConnectionManager.Connection);
            }
    
            /// <summary>
            /// Gets table schema info for a table in SQLite database.
            /// </summary>
            /// <param name="tableName">Table name.</param>
            /// <returns>Object that contains SQLite table schema.</returns>
            public SqliteTableSchema GetTableSchemaInfo(string tableName)
            {
                return this.DatabaseProvider.GetTableSchemaInfo(this.ConnectionManager.Connection, tableName);
            }
    
            /// <summary>
            /// Saves a data table into the database.
            /// </summary>
            /// <param name="table">The data table to be saved.</param>
            /// <remarks>Records in the <paramref name="table"/> sharing primary key with existing records in the database
            /// will update the values in the database with the existing <paramref name="table"/> values.
            /// If a column is not provided in the  <paramref name="table" />, it's assumed as a null value for update and insert purposed.</remarks>
            public void SaveTable(DataTable table)
            {
                const string InsertTable = "INSERT OR REPLACE INTO {0} ({1}) VALUES ({2});";
                table.ExecuteNonQuery(this.ConnectionManager.Connection, this.DatabaseProvider, InsertTable);
            }
        }
    }
}
