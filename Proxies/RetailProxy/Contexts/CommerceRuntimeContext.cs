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
        using System.Globalization;
        using System.IO;
        using System.Linq.Expressions;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Adapters;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Class encapsulates commerce runtime context.
        /// </summary>
        public class CommerceRuntimeContext : IContext, IDisposable
        {
            private static readonly Uri BaseUrl = new Uri("crt://localhost/");
            private ThreadLocal<Guid> batchId = new ThreadLocal<Guid>();
            private ParametersCache parametersCache = new ParametersCache();

            private UserToken userToken;
            private string deviceToken;
            private CommerceAuthenticationProvider authenticationProvider;

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceRuntimeContext"/> class.
            /// </summary>
            /// <param name="getCrtConfigByHostFunc">The action to load CommerceRuntimeConfiguration object for a specific host.</param>
            public CommerceRuntimeContext(Func<string, CommerceRuntimeConfiguration> getCrtConfigByHostFunc)
                : this(getCrtConfigByHostFunc, null, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceRuntimeContext"/> class.
            /// </summary>
            /// <param name="getCrtConfigByHostFunc">The action to load CommerceRuntimeConfiguration object for a specific host.</param>
            /// <param name="authenticationProvider">The authentication provider.</param>
            public CommerceRuntimeContext(Func<string, CommerceRuntimeConfiguration> getCrtConfigByHostFunc, CommerceAuthenticationProvider authenticationProvider)
                : this(getCrtConfigByHostFunc, null, authenticationProvider)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceRuntimeContext"/> class.
            /// </summary>
            /// <param name="getCrtConfigByHostFunc">The action to get CommerceRuntimeConfiguration object for a specific host.</param>
            /// <param name="specifiedRoles">The specified role collection.</param>
            /// <remarks>Once specified role collection is provided, the commerce runtime will be constructed by using these roles plus default unit number from the configuration file.</remarks>
            public CommerceRuntimeContext(Func<string, CommerceRuntimeConfiguration> getCrtConfigByHostFunc, string[] specifiedRoles)
                : this(getCrtConfigByHostFunc, specifiedRoles, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceRuntimeContext"/> class.
            /// </summary>
            /// <param name="getCrtConfigByHostFunc">The action to get CommerceRuntimeConfiguration object for a specific host.</param>
            /// <param name="specifiedRoles">The specified role collection.</param>
            /// <param name="authenticationProvider">The authentication provider.</param>
            /// <remarks>Once specified role collection is provided, the commerce runtime will be constructed by using these roles plus default unit number from the configuration file.</remarks>
            public CommerceRuntimeContext(Func<string, CommerceRuntimeConfiguration> getCrtConfigByHostFunc, string[] specifiedRoles, CommerceAuthenticationProvider authenticationProvider)
            {
                AdaptorCaller.SetGetConfigurationFunc(getCrtConfigByHostFunc);
                CommerceRuntimeManager.SpecifiedRoles = specifiedRoles;
                this.userToken = null;
                this.authenticationProvider = authenticationProvider;
            }

            /// <summary>
            /// Gets or sets the locale for the context.
            /// </summary>
            public string Locale
            {
                get
                {
                    return CommerceRuntimeManager.Locale;
                }

                set
                {
                    CommerceRuntimeManager.Locale = value;
                    if (this.authenticationProvider != null)
                    {
                        this.authenticationProvider.Locale = value;
                    }
                }
            }

            /// <summary>
            /// Sets the operating unit number.
            /// </summary>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            public void SetOperatingUnitNumber(string operatingUnitNumber)
            {
                throw new InvalidOperationException("Operating unit number cannot be set in the Commerce Runtime context.");
            }

            /// <summary>
            /// Gets the operating unit number.
            /// </summary>
            /// <returns>The context operating unit number.</returns>
            public string GetOperatingUnitNumber()
            {
                throw new InvalidOperationException("Operating unit number cannot be retrieved from Commerce Runtime context.");
            }

            /// <summary>
            /// Sets the device token.
            /// </summary>
            /// <param name="deviceToken">The device token.</param>
            public void SetDeviceToken(string deviceToken)
            {
                // A Commerce Runtime Acquire Token call will already include the device context in the returned token, the device token does not need to be set in the Commerce Runtime Manager.
                this.deviceToken = deviceToken;

                if (this.authenticationProvider != null)
                {
                    this.authenticationProvider.DeviceToken = this.deviceToken;
                }
            }

            /// <summary>
            /// Gets the device token.
            /// </summary>
            /// <returns>The context device token.</returns>
            public string GetDeviceToken()
            {
                // A Commerce Runtime Acquire Token call will already include the device context in the returned token, the device token does not need to be set in the Commerce Runtime Manager.
                return this.deviceToken;
            }

            /// <summary>
            /// Sets the user token.
            /// </summary>
            /// <param name="userToken">The user token.</param>
            public void SetUserToken(UserToken userToken)
            {
                UserToken userTokenForUpdate;

                if (userToken == null)
                {
                    userTokenForUpdate = CommerceRuntimeManager.RemoveUserIdentityFromToken(this.userToken);
                }
                else
                {
                    userTokenForUpdate = userToken;
                }

                CommerceRuntimeManager.SetUserIdentity(userTokenForUpdate);
                this.userToken = userToken;
            }

            /// <summary>
            /// Gets the user token.
            /// </summary>
            /// <returns>The context user token.</returns>
            public UserToken GetUserToken()
            {
                return this.userToken;
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
                    string result = await Task.Run(async () => await AdaptorCaller.ExecuteAsync(GetRequestUri(entitySetTypeName, operation, queryResultSettings, operationParameters)));
                    result = AdaptorCaller.RemoveCommerceRuntimePrefix(result);

                    TryThrowAsCommerceException(result);
                    PagedResult<T> pagedResult = result.DeserializeJsonObject<PagedResult<T>>();

                    return pagedResult;
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
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    throw new NotSupportedException("Authentication operations are not supported in batch mode.");
                }

                if (this.authenticationProvider == null)
                {
                    throw new NotSupportedException("Authentication provider is not set.");
                }

                await this.authenticationProvider.ExecuteAuthenticationSingleResultOperationAsync<object>(operation, operationParameters);
            }

            /// <summary>
            /// Executes the authentication operation asynchronous with no result.
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

                if (this.authenticationProvider == null)
                {
                    throw new NotSupportedException("Authentication provider is not set.");
                }

                T result = await this.authenticationProvider.ExecuteAuthenticationSingleResultOperationAsync<T>(operation, operationParameters);

                if (string.Equals(operation, CommerceAuthenticationProvider.AcquireTokenActionName))
                {
                    this.SetUserToken(result as UserToken);
                }

                return result;
            }

            /// <summary>
            /// Executes the operation asynchronous with single result.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entitySetTypeName">Type name of the entity set.</param>
            /// <param name="operation">The operation name.</param>
            /// <param name="isAction">True, if the operation is an action; false, if the operation is a function.</param>
            /// <param name="expandProperties">The navigation property names to be expanded.</param>
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

                        TaskCompletionSource<object> taskCompletionSource = this.parametersCache.PutParameters<T>(localBatchId, entitySet, entitySetTypeName, operation, null, null, operationParameters);

                        return await taskCompletionSource.Task.ContinueWith<T>(this.GetSingleEntity<T>);
                    }
                }
                else
                {
                    string result = await Task.Run(async () => await AdaptorCaller.ExecuteAsync(GetRequestUri(entitySetTypeName, operation, operationParameters)));
                    result = AdaptorCaller.RemoveCommerceRuntimePrefix(result);

                    TryThrowAsCommerceException(result);
                    return result.DeserializeJsonObject<T>();
                }
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
                string result = await Task.Run(async () => await AdaptorCaller.ExecuteAsync(GetRequestUri(entitySetTypeName, operation, operationParameters)));
                result = AdaptorCaller.RemoveCommerceRuntimePrefix(result);

                TryThrowAsCommerceException(result);
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

                var result = await Task.Run(async () => await AdaptorCaller.ExecuteAsync(GetRequestUri(typeof(T).Name, "Create", new OperationParameter() { Name = "entity", Value = entity })));
                result = AdaptorCaller.RemoveCommerceRuntimePrefix(result);

                TryThrowAsCommerceException(result);
                return result.DeserializeJsonObject<T>();
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
            public async Task<RetailProxy.PagedResult<T>> ReadAll<T>(string entitySet, ICollection<string> expandProperties, QueryResultSettings queryResultSettings)
                where T : ICommerceEntity
            {
                return await this.ExecuteOperationAsync<T>(entitySet, typeof(T).Name, ActionNames.ReadAll, false, queryResultSettings, expandProperties);
            }

            /// <summary>
            /// Reads the stream for the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="predicate">The predicate.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>
            /// The stream for the specified entitySet.
            /// </returns>
            /// <exception cref="System.NotImplementedException">Not implemented for this context.</exception>
            public Task<Stream> ReadStream<T>(string entitySet, Expression<Func<T, bool>> predicate, params OperationParameter[] operationParameters)
                where T : ICommerceEntity
            {
                throw new NotImplementedException();
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

                string result = await Task.Run(async () => await AdaptorCaller.ExecuteAsync(GetRequestUri(typeof(T).Name, "Update", new OperationParameter() { Name = "entity", Value = entity })));
                result = AdaptorCaller.RemoveCommerceRuntimePrefix(result);

                TryThrowAsCommerceException(result);
                return result.DeserializeJsonObject<T>();
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

                string result = await Task.Run(async () => await AdaptorCaller.ExecuteAsync(GetRequestUri(typeof(T).Name, "Delete", new OperationParameter() { Name = "entity", Value = entity })));
                result = AdaptorCaller.RemoveCommerceRuntimePrefix(result);

                TryThrowAsCommerceException(result);
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
            /// <remarks>
            /// This method does not really submit the batch request, it does nothing since
            /// all requests have been submitted before.
            /// </remarks>
            /// <returns>A Task.</returns>
            public async Task ExecuteBatchAsync()
            {
                await BatchHelper.ExecuteBatch(this, this.parametersCache, this.batchId);
            }

            /// <summary>
            /// Commits the batch request.
            /// </summary>
            /// <param name="requests">The cached requests.</param>
            /// <param name="tasks">The cached tasks to return back the result.</param>
            /// <returns>A Task.</returns>
            public async Task ExecuteBatchAsync(List<ParametersGroup> requests, List<TaskCompletionSource<object>> tasks)
            {
                if (requests == null)
                {
                    throw new ArgumentNullException("requests");
                }

                if (tasks == null)
                {
                    throw new ArgumentNullException("tasks");
                }

                if (requests.Count != tasks.Count)
                {
                    throw new ArgumentException("The count of cached requests does not equal to the count of cached tasks.");
                }

                for (int i = 0; i < requests.Count; ++i)
                {
                    string result = await Task.Run(async () => await AdaptorCaller.ExecuteAsync(GetRequestUri(requests[i].EntitySetType, requests[i].OperationName, requests[i].QueryResultSettings, requests[i].OperationParameters)));
                    tasks[i].SetResult(AdaptorCaller.RemoveCommerceRuntimePrefix(result));
                }
            }

            /// <summary>
            /// Gets a paged result of entities.
            /// </summary>
            /// <typeparam name="T">The type of entities.</typeparam>
            /// <param name="task">The task returned by an individual method call such as ReadAll.</param>
            /// <returns>The PagedResult of entities.</returns>
            public RetailProxy.PagedResult<T> GetPagedResult<T>(Task<object> task)
            {
                if (task == null)
                {
                    throw new ArgumentNullException("task");
                }

                // Get the result from the task.
                string result = task.Result as string;
                TryThrowAsCommerceException(result);
                return result.DeserializeJsonObject<RetailProxy.PagedResult<T>>();
            }

            /// <summary>
            /// Gets a single entity from the task.
            /// </summary>
            /// <typeparam name="T">The type of the result.</typeparam>
            /// <param name="task">The task returned by an individual method call such as Read.</param>
            /// <returns>The single entity.</returns>
            public T GetSingleEntity<T>(Task<object> task)
            {
                if (task == null)
                {
                    throw new ArgumentNullException("task");
                }

                // Get the result from the task.
                string result = task.Result as string;
                TryThrowAsCommerceException(result);
                return result.DeserializeJsonObject<T>();
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

            private static string GetRequestUri(string entitySet, string action, params OperationParameter[] actionParameters)
            {
                return GetRequestUri(entitySet, action, null, actionParameters);
            }

            private static string GetRequestUri(string entitySet, string action, QueryResultSettings settings, params OperationParameter[] actionParameters)
            {
                if (string.IsNullOrWhiteSpace(action))
                {
                    throw new ArgumentNullException("action");
                }

                UriBuilder uriBuilder = new UriBuilder(BaseUrl);

                uriBuilder.Path = string.Format(CultureInfo.InvariantCulture, "{0}Manager\\{1}", entitySet, action);
                uriBuilder.Query = GetUriParameters(settings, actionParameters);

                return uriBuilder.ToString();
            }

            private static string GetUriParameters(QueryResultSettings settings, params OperationParameter[] actionParameters)
            {
                List<string> sections = new List<string>();

                if (settings != null)
                {
                    string str = string.Format(
                        "{0}={1}",
                        "queryResultSettings",
                        CommerceRuntimeContext.EscapeLongDataString(settings.SerializeToJsonObject()));
                    sections.Add(str);
                }

                foreach (var parameter in actionParameters)
                {
                    string str = string.Format(
                        "{0}={1}",
                        parameter.Name,
                        CommerceRuntimeContext.EscapeLongDataString(parameter.Value.SerializeToJsonObject()));
                    sections.Add(str);
                }

                return string.Join("&", sections);
            }

            /// <summary>
            /// Tries to throws a commerce exception, if the result could be serialized as a commerce exception; Proceeds silently, otherwise.
            /// </summary>
            /// <param name="result">The serialized return result.</param>
            private static void TryThrowAsCommerceException(string result)
            {
                if (!string.IsNullOrWhiteSpace(result))
                {
                    RetailProxyException crtException = null;

                    if (DefaultExceptionHandlingBehavior.TryDeserializeFromJsonString(result, out crtException)
                        && crtException != null)
                    {
                        throw crtException;
                    }
                }
            }

            /// <summary>
            /// Escapes string which could be longer than the default limit of Uri.EscapeDataString() (32766 characters).
            /// </summary>
            /// <param name="stringToBeEscaped">The string to be escaped.</param>
            /// <returns>An escaped string.</returns>
            private static string EscapeLongDataString(string stringToBeEscaped)
            {
                if (string.IsNullOrEmpty(stringToBeEscaped))
                {
                    return stringToBeEscaped;
                }

                const int Limit = 30000;

                StringBuilder sb = new StringBuilder();
                int loops = stringToBeEscaped.Length / Limit;

                for (int i = 0; i <= loops; i++)
                {
                    if (i < loops)
                    {
                        sb.Append(Uri.EscapeDataString(stringToBeEscaped.Substring(Limit * i, Limit)));
                    }
                    else
                    {
                        sb.Append(Uri.EscapeDataString(stringToBeEscaped.Substring(Limit * i)));
                    }
                }

                return sb.ToString();
            }
        }
    }
}
