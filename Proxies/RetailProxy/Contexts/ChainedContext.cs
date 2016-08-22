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
    namespace Commerce.RetailProxy
    {
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq.Expressions;
        using System.Threading;
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Class encapsulates offline capable context.
        /// </summary>
        public class ChainedContext : IContext, IDisposable
        {
            private readonly ChainedContextStateMachine contextStateMachine;
            private ThreadLocal<Guid> batchId = new ThreadLocal<Guid>();
            private ParametersCache parametersCache = new ParametersCache();
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ChainedContext"/> class.
            /// </summary>
            /// <param name="onlineContext">The online context.</param>
            /// <param name="offlineContext">The offline context.</param>
            /// <param name="getCachedEntityFunc">The get cached entity function delegate.</param>
            /// <param name="initialState">The initial state.</param>
            /// <param name="reconnectionInterval">The online reconnection time interval.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Backward Compatibility.")]
            public ChainedContext(IContext onlineContext, IContext offlineContext, Func<Type, Task<object>> getCachedEntityFunc, ChainedContextState initialState, TimeSpan reconnectionInterval)
            {
                this.contextStateMachine = new ChainedContextStateMachine(onlineContext, offlineContext, getCachedEntityFunc, initialState, reconnectionInterval);
            }
    
            /// <summary>
            /// The offline and online status change event handler.
            /// </summary>
            public event EventHandler<ContextStatusChangedEventArgs> StatusChanged;
    
            /// <summary>
            /// Gets or sets the locale for the context.
            /// </summary>
            public string Locale
            {
                get { return this.contextStateMachine.Locale; }
                set { this.contextStateMachine.Locale = value; }
            }
    
            /// <summary>
            /// Gets a value indicating whether this instance is offline.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
            /// </value>
            public bool IsOffline
            {
                get { return this.contextStateMachine.IsOffline; }
            }
    
            /// <summary>
            /// Gets the last online connection UTC date/time.
            /// </summary>
            public DateTime LastOnlineConnectionUtcDateTime
            {
                get { return this.contextStateMachine.LastOnlineConnectionUtcDateTime; }
            }
    
            /// <summary>
            /// Gets the online context.
            /// </summary>
            internal IContext OnlineContext
            {
                get { return this.contextStateMachine.OnlineContext; }
            }
    
            /// <summary>
            /// Gets the offline context.
            /// </summary>
            internal IContext OfflineContext
            {
                get { return this.contextStateMachine.OfflineContext; }
            }
    
            /// <summary>
            /// Sets the operating unit number.
            /// </summary>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            public void SetOperatingUnitNumber(string operatingUnitNumber)
            {
                this.contextStateMachine.SetOperatingUnitNumber(operatingUnitNumber);
            }
    
            /// <summary>
            /// Gets the operating unit number.
            /// </summary>
            /// <returns>The context operating unit number.</returns>
            public string GetOperatingUnitNumber()
            {
                return this.contextStateMachine.GetOperatingUnitNumber();
            }
    
            /// <summary>
            /// Sets the device token.
            /// </summary>
            /// <param name="deviceToken">The device token.</param>
            public void SetDeviceToken(string deviceToken)
            {
                this.contextStateMachine.SetDeviceToken(deviceToken);
            }
    
            /// <summary>
            /// Gets the device token.
            /// </summary>
            /// <returns>The context device token.</returns>
            public string GetDeviceToken()
            {
                return this.contextStateMachine.GetDeviceToken();
            }
    
            /// <summary>
            /// Sets the user token.
            /// </summary>
            /// <param name="userToken">The user token.</param>
            public void SetUserToken(UserToken userToken)
            {
                this.contextStateMachine.SetUserToken(userToken);
            }
    
            /// <summary>
            /// Gets the user token.
            /// </summary>
            /// <returns>The context user token.</returns>
            public UserToken GetUserToken()
            {
                return this.contextStateMachine.GetUserToken();
            }
    
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
    
            /// <summary>
            /// Switches to online mode asynchronously.
            /// </summary>
            /// <returns>No return.</returns>
            public async Task SwitchToOnlineAsync()
            {
                await this.contextStateMachine.SwitchToOnlineAsync();
            }
    
            /// <summary>
            /// Switches to offline mode asynchronously.
            /// </summary>
            /// <returns>No return.</returns>
            public async Task SwitchToOfflineAsync()
            {
                await this.contextStateMachine.SwitchToOfflineAsync();
            }
    
            /// <summary>
            /// Creates the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entity">The entity.</param>
            /// <returns>
            /// The object of type T.
            /// </returns>
            public async Task<T> Create<T>(string entitySet, T entity)
                where T : ICommerceEntity
            {
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    throw new NotSupportedException("Create operation is not supported in batch mode, only Read, ReadAll, and Functions are supported..");
                }
    
                return await this.ExecuteAsync(entitySet, ActionNames.Create, async (context) => await context.Create<T>(entitySet, entity));
            }
    
            /// <summary>
            /// Deletes the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entity">The entity.</param>
            /// <returns>No return.</returns>
            public async Task Delete<T>(string entitySet, T entity)
                where T : ICommerceEntity
            {
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    throw new NotSupportedException("Delete operation is not supported in batch mode, only Read, ReadAll, and Functions are supported.");
                }
    
                await this.ExecuteAsync(
                    entitySet,
                    ActionNames.Delete,
                    async (context) =>
                    {
                        await context.Delete<T>(entitySet, entity);
    
                        return 0;
                    });
            }

            /// <summary>
            /// Reads the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="predicate">The predicate.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>
            /// The object of type T.
            /// </returns>
            public async Task<T> Read<T>(string entitySet, Expression<Func<T, bool>> predicate, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
                where T : ICommerceEntity
            {
                return await this.ExecuteOperationSingleResultAsync<T>(entitySet, typeof(T).Name, ActionNames.Read, false, expandProperties, operationParameters);
            }

            /// <summary>
            /// Reads all entities of type T.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>
            /// Collection of objects.
            /// </returns>
            public async Task<PagedResult<T>> ReadAll<T>(string entitySet, ICollection<string> expandProperties, QueryResultSettings queryResultSettings)
                where T : ICommerceEntity
            {
                return await this.ExecuteOperationAsync<T>(entitySet, typeof(T).Name, ActionNames.ReadAll, false, queryResultSettings, expandProperties);
            }
    
            /// <summary>
            /// Gets a single entity from the task.
            /// </summary>
            /// <typeparam name="T">The type of the result.</typeparam>
            /// <param name="task">The task returned by an individual method call such as Read.</param>
            /// <returns>The single entity.</returns>
            public T GetSingleEntity<T>(Task<object> task)
            {
                return this.ExecuteAsync<T>(
                    string.Empty,
                    string.Empty,
                    async (context) =>
                    {
                        return await Task.Run(() => context.GetSingleEntity<T>(task));
                    }).Result;
            }
    
            /// <summary>
            /// Gets a paged result of entities.
            /// </summary>
            /// <typeparam name="T">The type of entities.</typeparam>
            /// <param name="task">The task returned by an individual method call such as ReadAll.</param>
            /// <returns>The PagedResult of entities.</returns>
            public PagedResult<T> GetPagedResult<T>(Task<object> task)
            {
                return this.ExecuteAsync(
                    string.Empty,
                    string.Empty,
                    async (context) =>
                    {
                        return await Task.Run(() => context.GetPagedResult<T>(task));
                    }).Result;
            }
    
            /// <summary>
            /// Reads the stream for the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="predicate">The predicate.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>The stream for the specified entitySet.</returns>
            public async Task<Stream> ReadStream<T>(string entitySet, Expression<Func<T, bool>> predicate, params OperationParameter[] operationParameters)
                where T : ICommerceEntity
            {
                return await this.ExecuteAsync(entitySet, ActionNames.ReadStream, async (context) => await context.ReadStream<T>(entitySet, predicate, operationParameters));
            }
    
            /// <summary>
            /// Updates the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entity">The entity.</param>
            /// <returns>
            /// The object of type T.
            /// </returns>
            public async Task<T> Update<T>(string entitySet, T entity)
                where T : CommerceEntity
            {
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    throw new NotSupportedException("Update operation is not supported in batch mode, only Read, ReadAll, and Functions are supported.");
                }
    
                return await this.ExecuteAsync(entitySet, ActionNames.Update, async (context) => await context.Update<T>(entitySet, entity));
            }
    
            /// <summary>
            /// Executes the operation asynchronous with no result.
            /// </summary>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entitySetTypeName">The entity set type name.</param>
            /// <param name="operation">The operation name.</param>
            /// <param name="isAction">True, if the operation is an action; false, if the operation is a function.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>No return.</returns>
            public async Task ExecuteOperationAsync(string entitySet, string entitySetTypeName, string operation, bool isAction, params OperationParameter[] operationParameters)
            {
                await this.ExecuteAsync(
                    entitySet,
                    operation,
                    async (context) =>
                    {
                        await context.ExecuteOperationAsync(entitySet, entitySetTypeName, operation, isAction, operationParameters);
    
                        return true;
                    });
            }

            /// <summary>
            /// Executes the operation asynchronous with single result.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entitySetTypeName">Type name of the entity set.</param>
            /// <param name="operation">The operation name.</param>
            /// <param name="isAction">True, if the operation is an action; false, if the operation is a function.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>The object of type T.</returns>
            public async Task<T> ExecuteOperationSingleResultAsync<T>(string entitySet, string entitySetTypeName, string operation, bool isAction, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
            {
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    if (isAction)
                    {
                        throw new NotSupportedException("Action is not supported in batch mode, only Read, ReadAll, and Functions are supported.");
                    }
                    else
                    {
                        Guid localBatchId = this.batchId.Value;
    
                        TaskCompletionSource<object> taskCompletionSource = this.parametersCache.PutParameters<T>(localBatchId, entitySet, entitySetTypeName, operation, null, expandProperties, operationParameters);
    
                        return await taskCompletionSource.Task.ContinueWith<T>(this.GetSingleEntity<T>);
                    }
                }
                else
                {
                    return await this.ExecuteAsync(entitySet, operation, async (context) => await context.ExecuteOperationSingleResultAsync<T>(entitySet, entitySetTypeName, operation, isAction, expandProperties, operationParameters));
                }
            }

            /// <summary>
            /// Executes the authentication operation asynchronous with no result.
            /// </summary>
            /// <param name="operation">The operation name.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>No return.</returns>
            public async Task ExecuteAuthenticationOperationAsync(string operation, params OperationParameter[] operationParameters)
            {
                await this.ExecuteAsync(
                    null,
                    operation,
                    async (context) =>
                    {
                        await context.ExecuteAuthenticationOperationAsync(operation, operationParameters);

                        return true;
                    });
            }

            /// <summary>
            /// Executes the authentication operation asynchronous with a single result.
            /// </summary>
            /// <typeparam name="T">The type of the result.</typeparam>
            /// <param name="operation">The operation name.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>No return.</returns>
            public async Task<T> ExecuteAuthenticationOperationSingleResultAsync<T>(string operation, params OperationParameter[] operationParameters)
            {
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    throw new NotSupportedException("Authentication operations are not supported in batch mode.");
                }

                return await this.ExecuteAsync(null, operation, async (context) => await context.ExecuteAuthenticationOperationSingleResultAsync<T>(operation, operationParameters));
            }

            /// <summary>
            /// Executes the operation asynchronous with paged results.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entitySetTypeName">Type name of the entity set.</param>
            /// <param name="operation">The operation name.</param>
            /// <param name="isAction">True, if the operation is an action; false, if the operation is a function.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>The paged results of objects.</returns>
            public async Task<PagedResult<T>> ExecuteOperationAsync<T>(string entitySet, string entitySetTypeName, string operation, bool isAction, QueryResultSettings queryResultSettings, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
            {
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    if (isAction)
                    {
                        throw new NotSupportedException("Action is not supported in batch mode, only Read, ReadAll, and Functions are supported.");
                    }
                    else
                    {
                        Guid localBatchId = this.batchId.Value;
    
                        TaskCompletionSource<object> taskCompletionSource = this.parametersCache.PutParameters<T>(localBatchId, entitySet, entitySetTypeName, operation, queryResultSettings, expandProperties, operationParameters);
    
                        return await taskCompletionSource.Task.ContinueWith<PagedResult<T>>(this.GetPagedResult<T>);
                    }
                }
                else
                {
                    return await this.ExecuteAsync(entitySet, operation, async (context) => await context.ExecuteOperationAsync<T>(entitySet, entitySetTypeName, operation, isAction, queryResultSettings, expandProperties, operationParameters));
                }
            }
    
            /// <summary>
            /// Begins the batch operations for the calling thread.
            /// </summary>
            public void BeginBatch()
            {
                this.batchId.Value = Guid.NewGuid();
                this.parametersCache.RegisterThread(this.batchId.Value);
            }
    
            /// <summary>
            /// Submits the batch request.
            /// </summary>
            /// <returns>A Task.</returns>
            public async Task ExecuteBatchAsync()
            {
                await BatchHelper.ExecuteBatch(this, this.parametersCache, this.batchId);
            }
    
            /// <summary>
            /// Submits the batch request.
            /// </summary>
            /// <param name="requests">The requests that will be grouped into one single batch request.</param>
            /// <param name="tasks">The task where to return the results.</param>
            /// <returns>A Task.</returns>
            public async Task ExecuteBatchAsync(List<ParametersGroup> requests, List<TaskCompletionSource<object>> tasks)
            {
                await this.ExecuteAsync(
                    string.Empty,
                    "batch",
                    async (context) =>
                    {
                        await context.ExecuteBatchAsync(requests, tasks);
    
                        return true;
                    });
            }
    
            /// <summary>
            /// Disposes resources.
            /// </summary>
            /// <param name="disposing">Indicates whether or not dispose managed resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                }
    
                // Always dispose unmanaged resources.
                if (this.batchId != null)
                {
                    this.batchId.Dispose();
                }
            }
    
            private async Task<TResult> ExecuteAsync<TResult>(string entitySet, string action, Func<IContext, Task<TResult>> method)
            {
                var oldState = this.contextStateMachine.CurrentState;
    
                TResult result = default(TResult);
                try
                {
                    result = await this.contextStateMachine.MoveNextAsync(entitySet, action, method);
                }
                catch
                {
                    if (this.contextStateMachine.CurrentState != oldState && this.StatusChanged != null)
                    {
                        this.StatusChanged(this, new ContextStatusChangedEventArgs { State = this.contextStateMachine.CurrentState });
                    }
    
                    throw;
                }
    
                return result;
            }
        }
    }
}
