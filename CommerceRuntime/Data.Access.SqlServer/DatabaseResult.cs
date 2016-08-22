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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Represents the result of a database operation.
        /// </summary>
        internal sealed class DatabaseResult : IDatabaseResult
        {
            private Guid monitoringCorrelationId;

            /// <summary>
            /// Initializes a new instance of the <see cref="DatabaseResult"/> class.
            /// </summary>
            /// <param name="reader">The SQL server reader.</param>            
            public DatabaseResult(SqlDataReader reader)
            {
                ThrowIf.Null(reader, "reader");    
                this.SqlReader = reader;
            }
    
            /// <summary>
            /// Gets the number of fields available in the current result set.
            /// </summary>
            public int FieldCount
            {
                get { return this.SqlReader.FieldCount; }
            }
    
            /// <summary>
            /// Gets the number of reads executed against this result.
            /// </summary>
            internal long MonitoringNumberOfReads
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets the underlying SQL reader used to compose this result.
            /// </summary>
            private SqlDataReader SqlReader
            {
                get;
                set;
            }

            /// <summary>
            /// Gets a value indicating whether monitoring is enabled or not.
            /// </summary>
            private bool IsMonitoringEnabled
            {
                get
                {
                    return this.monitoringCorrelationId != null;
                }
            }

            /// <summary>
            /// Configures this <paramref name="DataResult"/> with monitoring event parameters so when the result is consumed, it records a termination event.
            /// </summary>
            /// <param name="databaseEventCorrelationId">The <see cref="Guid"/> representing the correction identifier for the database event.</param>
            public void ConfigureMonitoringEvent(Guid databaseEventCorrelationId)
            {
                this.monitoringCorrelationId = databaseEventCorrelationId;
                this.MonitoringNumberOfReads = 0;
            }
    
            /// <summary>
            /// Moves to the next result set.
            /// </summary>
            /// <returns>Whether new result set exists or not.</returns>
            /// <remarks>No call to this method is require to read the initial result set.</remarks>
            public bool NextResult()
            {
                return this.SqlReader.NextResult();
            }
    
            /// <summary>
            /// Reads the next result set row.
            /// </summary>
            /// <returns>Whether new result set row exists or not.</returns>
            /// <remarks>To read the first row in the result set, as well as all subsequent rows, a call to this method is necessary for each row to be read.</remarks>
            public bool Read()
            {
                this.MonitoringNumberOfReads++;
                return this.SqlReader.Read();
            }
    
            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <typeparam name="T">The expected type of the field being read.</typeparam>
            /// <param name="index">The index of the field to be read.</param>
            /// <returns>The field value read.</returns>
            public T GetValue<T>(int index)
            {
                return (T)this.GetValue(index, typeof(T));            
            }
    
            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <param name="index">The index of the field to be read.</param>
            /// <param name="valueType">The expected type of the field being read.</param>
            /// <returns>The field value read.</returns>
            public object GetValue(int index, System.Type valueType)
            {
                object value = this.SqlReader.GetValue(index);
                return value == System.DBNull.Value ? null : value;
            }
    
            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <typeparam name="T">The expected type of the field being read.</typeparam>
            /// <param name="fieldName">The name of the field to be read.</param>
            /// <returns>The field value read.</returns>
            public T GetValue<T>(string fieldName)
            {
                int fieldIndex = this.GetFieldIndex(fieldName);
                return this.GetValue<T>(fieldIndex);
            }
    
            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <param name="fieldName">The name of the field to be read.</param>
            /// <param name="valueType">The expected type of the field being read.</param>
            /// <returns>The field value read.</returns>
            public object GetValue(string fieldName, Type valueType)
            {
                int fieldIndex = this.GetFieldIndex(fieldName);
                return this.GetValue(fieldIndex, valueType);
            }
    
            /// <summary>
            /// Gets the name of the field in a specific index.
            /// </summary>
            /// <param name="index">The index of the field being queried.</param>
            /// <returns>The name of the field.</returns>
            public string GetName(int index)
            {
                return this.SqlReader.GetName(index);
            }
    
            /// <summary>
            /// Gets the index of the field by a specific name.
            /// </summary>
            /// <param name="fieldName">The name of the field.</param>
            /// <returns>The index of the field.</returns>
            public int GetFieldIndex(string fieldName)
            {
                ThrowIf.Null(fieldName, "fieldName");
    
                try
                {
                    // We don't need to cache it ourselves, because SqlDataReader caches it itself
                    return this.SqlReader.GetOrdinal(fieldName);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new DatabaseException(string.Format("The field '{0}' was not found in the result set.", fieldName), ex);
                }
            }
    
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                try
                {
                    this.SqlReader.Dispose();
                }
                finally
                {
                    // if we have monitoring information, use it to record end of dataaccess event
                    if (this.IsMonitoringEnabled)
                    {
                        RetailLogger.Log.CrtDataAccessExecuteQueryFinished(this.MonitoringNumberOfReads, wasSuccessful: true, correlationId: this.monitoringCorrelationId);
                    }                    
                }
            }
        }
    }
}
