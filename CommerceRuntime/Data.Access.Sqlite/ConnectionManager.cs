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
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;

        /// <summary>
        /// Manages the SQLite connection.
        /// </summary>
        internal sealed class ConnectionManager : IDisposable
        {
            private readonly SqliteConfiguration configuration;
            private object syncLock = new object();
    
            /// <summary>
            /// The dictionary of all currently opened database connections.
            /// </summary>
            private Dictionary<string, LinkedList<DatabaseConnection>> connectionDictionary;
    
            /// <summary>
            /// Represents the total number of connections active in our pool.
            /// </summary>
            private volatile int activeConnectionNumber;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
            /// </summary>
            /// <param name="configuration">The configuration object.</param>
            internal ConnectionManager(SqliteConfiguration configuration)
            {
                this.configuration = configuration;
                this.connectionDictionary = new Dictionary<string, LinkedList<DatabaseConnection>>(StringComparer.OrdinalIgnoreCase);
            }
    
            /// <summary>
            /// Finalizes an instance of the <see cref="ConnectionManager"/> class.
            /// </summary>
            /// <remarks>Disposes all database connections being held by this manager.</remarks>
            ~ConnectionManager()
            {
                this.Dispose();
            }
    
            /// <summary>
            /// Gets a value indicating the number of active connections in the pool.
            /// </summary>
            internal int ActiveConnectionNumber
            {
                get
                {
                    lock (this.syncLock)
                    {
                        return this.activeConnectionNumber;
                    }
                }
            }
    
            /// <summary>
            /// Returns a SQLite connection for the connection string. If the connection string has been used previously.
            /// </summary>
            /// <param name="connectionString">The connection string to created / fetch the connection for.</param>
            /// <returns>A SQLite connection.</returns>
            public DatabaseConnection GetConnection(string connectionString)
            {
                DatabaseConnection databaseConnection = null;
    
                // when trying to get a connection, we need to serialize the access to the connection pool
                lock (this.syncLock)
                {
                    LinkedList<DatabaseConnection> databaseConnections;
    
                    // try getting a list of connections in the pool for our connection string
                    if (!this.connectionDictionary.TryGetValue(connectionString, out databaseConnections))
                    {
                        // create list for that connection pool if it doesn't exist
                        databaseConnections = new LinkedList<DatabaseConnection>();
                        this.connectionDictionary.Add(connectionString, databaseConnections);
                    }
    
                    // searches for a connection not in use in the pool                
                    if (!this.TryGetFreeConnection(databaseConnections, out databaseConnection))
                    {
                        // if we didn't find a connection to use
                        // check if we can create create a new connection in the pool
                        if (this.activeConnectionNumber < this.configuration.ConnectionPoolSize)
                        {
                            // if we can, then just create a new connection for this connection string and add it to the pool
                            databaseConnection = this.CreateConnectionAndAddToConnectionPool(connectionString);
                        }
                        else
                        {
                            DatabaseConnection connectionToBeRemoved = null;
                            LinkedList<DatabaseConnection> connectionsBucket = null;
    
                            // if we don't have space to create a new connection, we need to free space
                            // search for connections not in use for other connection strings
                            foreach (LinkedList<DatabaseConnection> connections in this.connectionDictionary.Values)
                            {
                                // skip the collection that uses same connection string as the requested one,
                                // since we already checked it before
                                if (connections == databaseConnections)
                                {
                                    continue;
                                }
    
                                // search for a free connection to be removed from the pool
                                if (this.TryGetFreeConnection(connections, out connectionToBeRemoved))
                                {
                                    connectionsBucket = connections;
                                    break;
                                }
                            }
    
                            if (connectionToBeRemoved != null)
                            {
                                // dispose the connection
                                this.RemoveConnectionFromPool(connectionToBeRemoved, connectionsBucket);
    
                                // now we have space in the pool to add our new connection
                                // create it and add it to the pool
                                databaseConnection = this.CreateConnectionAndAddToConnectionPool(connectionString);
                            }
                            else
                            {
                                // the pool is full and all connections are in use
                                throw new DatabaseException("The database connection pool is full and all connections are currently in use." +
                                                            "It was not possible to reserve a connection for this request.");
                            }
                        }
                    }
    
                    // marks the connection as in use
                    databaseConnection.Reserve();
                }
    
                return databaseConnection;
            }
    
            /// <summary>
            /// Clears the pool of connections for a specific connection string.
            /// </summary>
            /// <param name="connectionString">The connection string.</param>
            /// <returns>Returns true if all connections with <paramref name="connectionString"/> were removed from the pool, otherwise false.</returns>
            public bool ClearPool(string connectionString)
            {
                ThrowIf.NullOrWhiteSpace(connectionString, "connectionString");
    
                lock (this.syncLock)
                {
                    LinkedList<DatabaseConnection> databaseConnections;
    
                    if (!this.connectionDictionary.TryGetValue(connectionString, out databaseConnections))
                    {
                        return true;
                    }
    
                    bool allDisposed = true;
    
                    // NOTE: database connections are copied to the array because we are removing them from the same linked list on the enumeration.
                    DatabaseConnection[] databaseConnectionsArray = databaseConnections.ToArray();
    
                    foreach (DatabaseConnection databaseConnection in databaseConnectionsArray)
                    {
                        if (databaseConnection.InUse)
                        {
                            databaseConnection.MarkedForRemoval = true;
                        }
                        else
                        {
                            this.RemoveConnectionFromPool(databaseConnection, databaseConnections);
                        }
    
                        allDisposed &= databaseConnection.IsDisposed;
                    }
    
                    return allDisposed;
                }
            }
    
            /// <summary>
            /// Disposes the connection pool and all it's resources.
            /// </summary>
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "An aggregate exception thrown instead.")]
            public void Dispose()
            {
                if (this.connectionDictionary != null)
                {
                    List<Exception> exceptions = new List<Exception>();
    
                    foreach (LinkedList<DatabaseConnection> databaseConnections in this.connectionDictionary.Values)
                    {
                        foreach (DatabaseConnection connection in databaseConnections)
                        {
                            try
                            {
                                if (connection != null)
                                {
                                    // Instead of .Dispose() which contains pool logic we immediately release all resources taken by the connection.
                                    connection.ReleaseResources();
                                }
                            }
                            catch (Exception exception)
                            {
                                exceptions.Add(exception);
                            }
                        }
                    }
    
                    this.connectionDictionary = null;
    
                    if (exceptions.Count > 0)
                    {
                        throw new AggregateException("Exceptions were thrown during disposal. See details in the inner exception collection.", exceptions);
                    }
                }
    
                GC.SuppressFinalize(this);
            }
    
            /// <summary>
            /// Releases a connection from use and return it to the pool so it's available for other requests.
            /// </summary>
            /// <param name="databaseConnection">The database connection to be released.</param>
            internal void ReleaseConnection(DatabaseConnection databaseConnection)
            {
                // since releasing the connection affects the ability of others to get a connection
                // we need to serialize the access to it
                lock (this.syncLock)
                {
                    if (databaseConnection.MarkedForRemoval)
                    {
                        this.RemoveConnectionFromPool(databaseConnection);
                    }
                    else
                    {
                        databaseConnection.Release();
                    }
                }
            }
    
            /// <summary>
            /// Creates a new database connection and adds it to the pool.
            /// </summary>
            /// <param name="connectionString">The connection string to be used when creating the connection.</param>
            /// <returns>The created database connection.</returns>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "To be disposed by caller.")]
            private DatabaseConnection CreateConnectionAndAddToConnectionPool(string connectionString)
            {
                DatabaseConnection databaseConnection;
    
                // check if we can create create a new connection in the pool
                if (this.activeConnectionNumber < this.configuration.ConnectionPoolSize)
                {
                    // if we can, then just create a new connection for this connection string
                    databaseConnection = new DatabaseConnection(
                        connectionString,
                        this.configuration.StatementCachingPoolSize,
                        this.configuration.BusyTimeout,
                        this);
    
                    // add the connection to the ppol
                    this.connectionDictionary[connectionString].AddLast(databaseConnection);
    
                    // increase the counter for connections in the pool
                    this.activeConnectionNumber++;
                }
                else
                {
                    string message = string.Format(
                        "There are no slots available in the connection pool at this moment (pool size: {0})",
                        this.configuration.ConnectionPoolSize);
                    throw new DatabaseException(message);
                }
    
                return databaseConnection;
            }
    
            /// <summary>
            /// Removes a connection from a pool.
            /// </summary>
            /// <param name="connection">A connection to remove.</param>
            private void RemoveConnectionFromPool(DatabaseConnection connection)
            {
                string connectionString = connection.ConnectionString;
    
                LinkedList<DatabaseConnection> databaseConnections;
    
                if (!this.connectionDictionary.TryGetValue(connectionString, out databaseConnections))
                {
                    return;
                }
    
                this.RemoveConnectionFromPool(connection, databaseConnections);
            }
    
            /// <summary>
            /// Removes a connection from a specific connection bucket of the pool.
            /// </summary>
            /// <param name="connection">A connection to remove.</param>
            /// <param name="connectionBucket">A bucket to which the connection belongs.</param>
            private void RemoveConnectionFromPool(DatabaseConnection connection, ICollection<DatabaseConnection> connectionBucket)
            {
                string connectionString = connection.ConnectionString;
    
                connectionBucket.Remove(connection);
    
                this.activeConnectionNumber--;
    
                if (connectionBucket.Count == 0)
                {
                    this.connectionDictionary.Remove(connectionString);
                }
    
                connection.ReleaseResources();
            }
    
            /// <summary>
            /// Searches for a database connection that is not in use in the <paramref name="connections"/> and returns it. 
            /// </summary>
            /// <param name="connections">The collection of connections to be searched for.</param>
            /// <param name="databaseConnection">The free connection available, if any.</param>
            /// <returns>True if a free connection was found, false otherwise.</returns>
            private bool TryGetFreeConnection(IEnumerable<DatabaseConnection> connections, out DatabaseConnection databaseConnection)
            {
                databaseConnection = null;
    
                // searches for a connection not in use in the pool
                foreach (DatabaseConnection connection in connections)
                {
                    if (!connection.InUse)
                    {
                        // if a connection was found, break
                        databaseConnection = connection;
                        break;
                    }
                }
    
                return databaseConnection != null;
            }
        }
    }
}
