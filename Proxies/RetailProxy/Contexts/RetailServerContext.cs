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
        using System.Collections;
        using System.Collections.Generic;
        using System.Globalization;
        using System.IO;
        using System.Linq;
        using System.Linq.Expressions;
        using System.Net;
        using System.Reflection;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Adapters;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.OData.Client;
        using Microsoft.OData.Core;

        /// <summary>
        /// Class encapsulates retail server context.
        /// </summary>
        public partial class RetailServerContext : DataServiceContext, IContext, IDisposable
        {
            private const string OperatingUnitNumberHeaderName = "OUN";
            private const string DeviceTokenHeaderName = "DeviceToken";
            private const string AuthorizationHeaderName = "Authorization";
            private const string ApiVersionParameterName = "api-version";
            private const string CrtRuntimeNamespace = "Microsoft.Dynamics.Commerce.Runtime";
            private const string CrtNotificationNamespace = "Microsoft.Dynamics.Commerce.Runtime.Notifications";
            private const string CrtDataModelNamespace = "Microsoft.Dynamics.Commerce.Runtime.DataModel";
            private CookieContainer cookieContainer = new CookieContainer();
            private string operatingUnitNumber;
            private string deviceToken;
            private string locale;
            private UserToken userToken;
            private CommerceAuthenticationProvider authenticationProvider;
            private ThreadLocal<Guid> batchId = new ThreadLocal<Guid>();
            private ParametersCache parametersCache = new ParametersCache();
            private PropertyInfo applyingChangesProperty;

            /// <summary>
            /// Initializes a new instance of the <see cref="RetailServerContext"/> class.
            /// </summary>
            /// <param name="serviceRoot">The service root.</param>
            /// <param name="authenticationProvider">The authentication provider.</param>
            public RetailServerContext(Uri serviceRoot, CommerceAuthenticationProvider authenticationProvider) :
                base(serviceRoot, ODataProtocolVersion.V4)
            {
                this.ResolveName = new global::System.Func<global::System.Type, string>(this.ResolveNameFromType);
                this.ResolveType = new global::System.Func<string, global::System.Type>(this.ResolveTypeFromName);
                this.BuildingRequest += this.OnCommerceContextBuildingRequest;
                this.SendingRequest2 += this.OnCommerceContextSendingRequest;
                this.MergeOption = MergeOption.NoTracking;
                this.Format.UseJson(RetailServerContext.GetEdmModel());
                this.AddAndUpdateResponsePreference = DataServiceResponsePreference.IncludeContent;
                this.authenticationProvider = authenticationProvider;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RetailServerContext"/> class.
            /// </summary>
            /// <param name="serviceRoot">The service root.</param>
            public RetailServerContext(Uri serviceRoot) : this(serviceRoot, null)
            {
            }

            /// <summary>
            /// Gets or sets the locale for the context.
            /// </summary>
            public string Locale
            {
                get
                {
                    return this.locale;
                }

                set
                {
                    this.locale = value;

                    if (this.authenticationProvider != null)
                    {
                        this.authenticationProvider.Locale = this.locale;
                    }
                }
            }

            /// <summary>
            /// Creates a new instance of the <see cref="RetailServerContext"/> class.
            /// </summary>
            /// <param name="serviceUrl">The service root.</param>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            /// <returns>New instance of the <see cref="RetailServerContext"/> class.</returns>
            public static RetailServerContext Create(Uri serviceUrl, string operatingUnitNumber)
            {
                return Create(serviceUrl, null, operatingUnitNumber: operatingUnitNumber);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="RetailServerContext"/> class.
            /// </summary>
            /// <param name="serviceUrl">The URL of the service root.</param>
            /// <param name="authenticationProvider">The authentication provider.</param>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            /// <returns>New instance of the <see cref="RetailServerContext"/> class.</returns>
            public static RetailServerContext Create(Uri serviceUrl, CommerceAuthenticationProvider authenticationProvider, string operatingUnitNumber)
            {
                RetailServerContext context = new RetailServerContext(serviceUrl, authenticationProvider);
                context.operatingUnitNumber = operatingUnitNumber;
                return context;
            }

            /// <summary>
            /// Creates a new instance of the <see cref="RetailServerContext"/> class.
            /// </summary>
            /// <param name="serviceUrl">The service root.</param>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            /// <param name="token">Id Token for OpenId Connect authentication.</param>
            /// <returns>New instance of the <see cref="RetailServerContext"/> class.</returns>
            public static RetailServerContext Create(Uri serviceUrl, string operatingUnitNumber, UserToken token)
            {
                RetailServerContext context = RetailServerContext.Create(serviceUrl, operatingUnitNumber);
                context.SetUserToken(token);

                return context;
            }

            /// <summary>
            /// Creates a new instance of the <see cref="RetailServerContext"/> class.
            /// </summary>
            /// <param name="serviceUrl">The service root.</param>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            /// <param name="token">Id Token for OpenId Connect authentication.</param>
            /// <returns>New instance of the <see cref="RetailServerContext"/> class.</returns>
            public static RetailServerContext Create(Uri serviceUrl, string operatingUnitNumber, string token)
            {
                RetailServerContext context = RetailServerContext.Create(serviceUrl, operatingUnitNumber, new UserIdToken(token));
                return context;
            }

            /// <summary>
            /// Sets the operating unit number.
            /// </summary>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            public void SetOperatingUnitNumber(string operatingUnitNumber)
            {
                this.operatingUnitNumber = operatingUnitNumber;
            }

            /// <summary>
            /// Gets the operating unit number.
            /// </summary>
            /// <returns>The operating unit number.</returns>
            public string GetOperatingUnitNumber()
            {
                return this.operatingUnitNumber;
            }

            /// <summary>
            /// Sets the device token.
            /// </summary>
            /// <param name="deviceToken">The device token.</param>
            public void SetDeviceToken(string deviceToken)
            {
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
                return this.deviceToken;
            }

            /// <summary>
            /// Sets the user token.
            /// </summary>
            /// <param name="userToken">The user token.</param>
            public void SetUserToken(UserToken userToken)
            {
                this.userToken = userToken;

                if (this.authenticationProvider != null)
                {
                    this.authenticationProvider.UserToken = this.userToken;
                }
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
            /// Dispose both managed and unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Begins the batch operations for the calling thread.
            /// </summary>
            public void BeginBatch()
            {
                this.batchId.Value = Guid.NewGuid();
                this.parametersCache.RegisterThread(this.batchId.Value);
            }

            #region Actions and Functions

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
                // batch mode
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    if (isAction)
                    {
                        throw new NotSupportedException("Action is not supported in batch mode, only Read, ReadAll, and Functions are supported.");
                    }
                    else
                    {
                        // Batch Function
                        Guid localBatchId = this.batchId.Value;

                        TaskCompletionSource<object> taskCompletionSource = this.parametersCache.PutParameters<T>(localBatchId, entitySet, entitySetTypeName, operation, queryResultSettings, expandProperties, operationParameters);

                        return await taskCompletionSource.Task.ContinueWith<PagedResult<T>>(this.GetPagedResult<T>);
                    }
                }
                else
                {
                    Uri requestUri = this.GetRequestUri(entitySet, operation, false, queryResultSettings, expandProperties, operationParameters);
                    Microsoft.OData.Client.OperationParameter[] parameters = GetOperationParameters(isAction, operationParameters);
                    string httpMethod = isAction ? ODataConstants.MethodPost : ODataConstants.MethodGet;

                    var results = await RetailServerContext.ExecuteWithExceptionHandlingAsync(
                                        async () => await Task.Factory.FromAsync<Uri, IEnumerable<T>>(
                                                (uri, callback, state) => this.BeginExecute<T>(uri, callback, state, httpMethod, false, parameters),
                                                this.EndExecute<T>,
                                                requestUri,
                                                null));

                    return new PagedResult<T>(results as QueryOperationResponse<T>);
                }
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
                // batch mode
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    if (isAction)
                    {
                        throw new NotSupportedException("Action is not supported in batch mode, only Read, ReadAll, and Functions are supported.");
                    }
                    else
                    {
                        // Batch Function
                        Guid localBatchId = this.batchId.Value;

                        TaskCompletionSource<object> taskCompletionSource = this.parametersCache.PutParameters<T>(localBatchId, entitySet, entitySetTypeName, operation, null, expandProperties, operationParameters);

                        return await taskCompletionSource.Task.ContinueWith<T>(this.GetSingleEntity<T>);
                    }
                }
                else
                {
                    Uri requestUri = this.GetRequestUri(entitySet, operation, false, null, expandProperties, operationParameters);
                    Microsoft.OData.Client.OperationParameter[] parameters = GetOperationParameters(isAction, operationParameters);
                    string httpMethod = isAction ? ODataConstants.MethodPost : ODataConstants.MethodGet;

                    var result = await RetailServerContext.ExecuteWithExceptionHandlingAsync(
                        async () => await Task.Factory.FromAsync<Uri, IEnumerable<T>>(
                                (uri, callback, state) => this.BeginExecute<T>(uri, callback, state, httpMethod, true, parameters),
                                this.EndExecute<T>,
                                requestUri,
                                null));

                    // When OData.Null is returned, the IEnumerable class System.Data.Services.Client.QueryOperationResponse
                    // cannot handel FirstOrDedault() method call correctly. We have to use try/catch to workaround this.
                    try
                    {
                        var entity = result.FirstOrDefault();
                        if (entity != null)
                        {
                            this.Detach(entity);
                        }

                        return entity;
                    }
                    catch (NullReferenceException)
                    {
                        return default(T);
                    }
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
                if (BatchHelper.IsInBatchMode(this.batchId))
                {
                    throw new NotSupportedException("Action is not supported in batch mode, only Read, ReadAll, and Functions are supported.");
                }

                Uri requestUri = this.GetRequestUri(entitySet, operation, false, null, null, operationParameters);
                Microsoft.OData.Client.OperationParameter[] parameters = GetOperationParameters(isAction, operationParameters);
                string httpMethod = isAction ? ODataConstants.MethodPost : ODataConstants.MethodGet;

                await RetailServerContext.ExecuteWithExceptionHandlingAsync(
                                    async () => await Task.Factory.FromAsync(
                                            (uri, callback, state) => this.BeginExecute(uri, callback, state, httpMethod, parameters),
                                            result => this.EndExecute(result),
                                            requestUri,
                                            null));
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
            /// Executes the operation asynchronous with a single result.
            /// </summary>
            /// <typeparam name="T">The type of the returned result.</typeparam>
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
            
            #endregion

            #region CRUD Operations

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

                this.AddObject(entitySet, entity);

                try
                {
                    await RetailServerContext.ExecuteWithExceptionHandlingAsync(
                        async () =>
                            await
                                Task<DataServiceResponse>.Factory.FromAsync(this.BeginSaveChanges, this.EndSaveChanges, null));
                }
                finally
                {
                    // Newly created object need to be removed from current context.
                    // So that, next creation request for the same entity type can be sent correctly.
                    this.Detach(entity);
                }

                return entity;
            }

            /// <summary>
            /// Reads the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="predicate">The predicate.</param>
            /// <param name="expandProperties">The navigation property names to be expanded.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>
            /// The object of type T.
            /// </returns>
            public async Task<T> Read<T>(string entitySet, Expression<Func<T, bool>> predicate, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
                where T : ICommerceEntity
            {
                return await this.ExecuteOperationSingleResultAsync<T>(entitySet, null, null, false, expandProperties, operationParameters);
            }

            /// <summary>
            /// Commits the batch request.
            /// </summary>
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
                    throw new ArgumentException("The count to cached requets does not equal to the count of cached tasks.");
                }

                // Prepare the DataServiceRequest.
                DataServiceRequest[] dataServiceRequests = new DataServiceRequest[requests.Count];
                int i = 0;
                foreach (ParametersGroup request in requests)
                {
                    Uri uri = this.GetRequestUri(request.EntitySet, request.OperationName, true, request.QueryResultSettings, request.ExpandProperties, request.OperationParameters);

                    // Use reflection to call the templated constructor of DataServiceRequest
                    MethodInfo createDataServiceRequest = typeof(RetailServerContext).GetTypeInfo().GetDeclaredMethod("CreateDataServiceRequest").MakeGenericMethod(request.TypeParameter);
                    dataServiceRequests[i++] = (DataServiceRequest)createDataServiceRequest.Invoke(null, new object[] { uri });
                }

                // Send the DataServiceRequests and get the response.
                DataServiceResponse response = await ExecuteWithExceptionHandlingAsync<DataServiceResponse>(
                    async () => await Task<DataServiceResponse>.Factory.FromAsync(
                        (cb, state) => this.BeginExecuteBatch(cb, state, dataServiceRequests), this.EndExecuteBatch, null));

                // Unpack the response and set each individual response to corresponding task.
                this.UnpackBatchResponse(response, tasks);
            }

            /// <summary>
            /// Reads the stream for the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="predicate">The predicate.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>
            /// The stream for the entity set.
            /// </returns>
            public async Task<Stream> ReadStream<T>(string entitySet, Expression<Func<T, bool>> predicate, params OperationParameter[] operationParameters)
                where T : ICommerceEntity
            {
                var entity = await this.Read(entitySet, predicate, null, operationParameters);

                var streamResponse =
                    await RetailServerContext.ExecuteWithExceptionHandlingAsync(
                                 async () => await Task<DataServiceStreamResponse>.Factory.FromAsync(
                                            (cb, state) => this.BeginGetReadStream(entity, new DataServiceRequestArgs(), cb, state),
                                            this.EndGetReadStream,
                                            null));
                return streamResponse.Stream;
            }

            /// <summary>
            /// Reads all.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>
            /// The collection of entity.
            /// </returns>
            public async Task<PagedResult<T>> ReadAll<T>(string entitySet, ICollection<string> expandProperties, QueryResultSettings queryResultSettings)
                where T : ICommerceEntity
            {
                return await this.ExecuteOperationAsync<T>(entitySet, null, null, false, queryResultSettings, expandProperties);
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

                var entityToBeUpdated = entity.Clone() as T;
                this.AttachTo(entitySet, entityToBeUpdated);

                DataServiceCollection<T> entities = null;
                bool applying = this.ApplyingChanges;
                try
                {
                    ((ICommerceEntity)entityToBeUpdated).IsNotificationDisabled = true;
                    entities = new DataServiceCollection<T>(this, entitySet, null, null) { entityToBeUpdated };

                    // Partial payload for update will only be sent out if the ApplyingChanges property is set to false.
                    // Because of that, we have to enforce the logic here, since some operations, like EndExecute, EndBatchExecute, etc., can leave that flag as true.
                    this.SetApplyingChanges(false);

                    // Assign changed property again to trigger change tracking on the current context.
                    this.AssignChangedProperties(entityToBeUpdated, entities[0]);
                    await RetailServerContext.ExecuteWithExceptionHandlingAsync(
                            async () => await Task<DataServiceResponse>.Factory.FromAsync(
                                this.BeginSaveChanges,
                                (result) =>
                                {
                                // Detach the dataservice collection of T to avoid the complex type is already being tracked error.
                                entities.Detach();
                                    return this.EndSaveChanges(result);
                                },
                                null));
                }
                finally
                {
                    this.SetApplyingChanges(applying);
                    this.Detach(entityToBeUpdated);
                    entities.Clear();
                    ((ICommerceEntity)entityToBeUpdated).IsNotificationDisabled = false;
                }

                return entityToBeUpdated;
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

                this.AttachTo(entitySet, entity);

                try
                {
                    this.DeleteObject(entity);
                    await RetailServerContext.ExecuteWithExceptionHandlingAsync(
                        async () => await Task<DataServiceResponse>.Factory.FromAsync(this.BeginSaveChanges, this.EndSaveChanges, null));
                }
                finally
                {
                    this.Detach(entity);
                }
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
                return RetailServerContext.ExecuteWithExceptionHandlingAsync<T>(
                    async () => await Task<T>.Run(
                        () =>
                        {
                            if (task.Exception != null &&
                                task.Exception.InnerException != null)
                            {
                                throw task.Exception.InnerException;
                            }

                            QueryOperationResponse<T> response = task.Result as QueryOperationResponse<T>;
                            IEnumerator enumerator = response.GetEnumerator();
                            enumerator.MoveNext();
                            return (T)enumerator.Current;
                        })).Result;
            }

            /// <summary>
            /// Gets a paged result of entities.
            /// </summary>
            /// <typeparam name="T">The type of entities.</typeparam>
            /// <param name="task">The task returned by an individual method call such as ReadAll.</param>
            /// <returns>The PagedResult of entities.</returns>
            public PagedResult<T> GetPagedResult<T>(Task<object> task)
            {
                if (task == null)
                {
                    throw new ArgumentNullException("task");
                }

                return RetailServerContext.ExecuteWithExceptionHandlingAsync<PagedResult<T>>(
                    async () => await Task.Run<PagedResult<T>>(
                        () =>
                        {
                            if (task.Exception != null &&
                                task.Exception.InnerException != null)
                            {
                                throw task.Exception.InnerException;
                            }

                            QueryOperationResponse<T> response = task.Result as QueryOperationResponse<T>;
                            return new PagedResult<T>(response);
                        })).Result;
            }

            /// <summary>
            /// Parses the inner exception.
            /// </summary>
            /// <param name="ex">The ex.</param>
            /// <exception cref="CommunicationException">Query failed due to server not found.
            /// or
            /// Query failed due to server internal error.
            /// or
            /// Query failed due to request timeout error.
            /// or
            /// Query failed due to bad request error.
            /// or
            /// Query failed due to ambiguous error.</exception>
            internal static void ParseInnerException(Exception ex)
            {
                var innerException = ex.InnerException as DataServiceClientException;
                if (innerException != null)
                {
                    // First, tries to parse and throw the exception for an OData error, if any.
                    CommunicationExceptionHelper.ThrowAsCommerceException(ex);

                    // If exception is not mapped to a commerce runtime exception, throws the local exception based on HTTP status code.
                    CommunicationExceptionHelper.ThrowAsRetailProxyExceptionOnHttpStatuCode(ex, innerException.StatusCode);
                }
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

            /// <summary>
            /// Since the namespace configured for this service reference
            /// in Visual Studio is different from the one indicated in the
            /// server schema, use type-mappers to map between the two.
            /// </summary>
            /// <param name="typeName">Name of the type.</param>
            /// <returns>The resolved type.</returns>
            protected Type ResolveTypeFromName(string typeName)
            {
                Type resolvedType = this.DefaultResolveType(typeName, RetailServerContext.CrtDataModelNamespace, this.GetType().Namespace);
                if (resolvedType != null)
                {
                    return resolvedType;
                }

                resolvedType = this.DefaultResolveType(typeName, RetailServerContext.CrtNotificationNamespace, this.GetType().Namespace);

                if (resolvedType != null)
                {
                    return resolvedType;
                }

                resolvedType = this.DefaultResolveType(typeName, RetailServerContext.CrtRuntimeNamespace, this.GetType().Namespace);

                if (resolvedType != null)
                {
                    return resolvedType;
                }

                return null;
            }

            #endregion

            /// <summary>
            /// Since the namespace configured for this service reference
            /// in Visual Studio is different from the one indicated in the
            /// server schema, use type-mappers to map between the two.
            /// </summary>
            /// <param name="clientType">Type of the client.</param>
            /// <returns>The type name.</returns>
            protected string ResolveNameFromType(Type clientType)
            {
                if (clientType == null)
                {
                    throw new ArgumentNullException("clientType");
                }

                if (clientType.Namespace.Equals(this.GetType().Namespace, StringComparison.Ordinal))
                {
                    return string.Concat(RetailServerContext.CrtDataModelNamespace, ".", clientType.Name);
                }

                return null;
            }

            /// <summary>
            /// Executes an action with exception handling asynchronously.
            /// </summary>
            /// <typeparam name="TResult">The type of the result.</typeparam>
            /// <param name="method">The method.</param>
            /// <returns>The object of type TResult.</returns>
            /// <exception cref="CommunicationException">Query failed due to server not found.
            /// or
            /// Query failed due to server internal error.
            /// or
            /// Query failed due to request timeout error.
            /// or
            /// Query failed due to bad request error.
            /// or
            /// Query failed due to ambiguous error.
            /// or
            /// Query failed.</exception>
            private static async Task<TResult> ExecuteWithExceptionHandlingAsync<TResult>(Func<Task<TResult>> method)
            {
                try
                {
                    return await method();
                }
                catch (DataServiceQueryException ex)
                {
                    RetailServerContext.ParseInnerException(ex);

                    throw new CommunicationException(
                        ClientCommunicationErrors.NotFound,
                        ex.Message,
                        ex);
                }
                catch (DataServiceRequestException ex)
                {
                    RetailServerContext.ParseInnerException(ex);

                    throw new CommunicationException(
                        ClientCommunicationErrors.BadRequest,
                        ex.Message,
                        ex);
                }
                catch (DataServiceClientException ex)
                {
                    RetailServerContext.ParseInnerException(ex);

                    throw new CommunicationException(
                        ClientCommunicationErrors.BadRequest,
                        ex.Message,
                        ex);
                }
            }

            /// <summary>
            /// Converts action or function parameters to the expected OData type of operation parameter array.
            /// </summary>
            /// <param name="isAction">True, if the operation is an action; false, if the operation is a function.</param>
            /// <param name="operationParameters">The parameter list of an action or a function.</param>
            /// <returns>The array of <see cref="OperationParameter"/></returns>
            private static Microsoft.OData.Client.OperationParameter[] GetOperationParameters(bool isAction, OperationParameter[] operationParameters)
            {
                IEnumerable<OperationParameter> parameters = operationParameters.Where(p => !p.IsKey);

                if (isAction)
                {
                    // Converts the proxy operation parameters into body parameters for POST action.
                    return parameters == null ? null
                        : parameters.Select(parameter => new BodyOperationParameter(parameter.Name, parameter.Value)).ToArray();
                }
                else
                {
                    // Converts the proxy operation parameters into Uri parameters for GET function.
                    return parameters == null ? null
                        : parameters.Select(parameter => new UriOperationParameter(parameter.Name, parameter.Value)).ToArray();
                }
            }

            private static DataServiceRequest<T> CreateDataServiceRequest<T>(Uri uri)
            {
                return new DataServiceRequest<T>(uri);
            }

            private static Uri GetRequestUriWithApiVersion(Uri origRequestUri, string requestApiVersion)
            {
                // Handles the api-version query argument to specify the retail server api version.
                HttpValueCollection queryArguments = HttpUtility.ParseQueryString(origRequestUri.Query);

                if (string.IsNullOrWhiteSpace(queryArguments[ApiVersionParameterName]))
                {
                    queryArguments[ApiVersionParameterName] = requestApiVersion;
                }

                UriBuilder uriBuilder = new UriBuilder(origRequestUri);
                uriBuilder.Query = queryArguments.ToString();

                return uriBuilder.Uri;
            }

            private static void AppendQueryDelimiter(StringBuilder uri)
            {
                uri.Append(uri.ToString().Contains("?") ? '&' : '?');
            }

            private Uri GetRequestUri(string entity, string operation, bool inlineParameters, QueryResultSettings queryResultSettings, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
            {
                StringBuilder uri = new StringBuilder();
                string formattedKeys = null;

                Dictionary<string, object> entityKeysDictionary = operationParameters.Where(p => p.IsKey).ToDictionary(p => p.Name, p => p.Value, StringComparer.Ordinal);

                if (entityKeysDictionary != null && entityKeysDictionary.Any())
                {
                    formattedKeys = Serializer.GetKeyString(this, entityKeysDictionary);
                }

                // Possible Uris.
                // <server>/[Entities[(key)]]/[Action][?Top=&Skip]
                // <server>/[Action][?Top=&Skip]
                if (!string.IsNullOrWhiteSpace(entity))
                {
                    uri.Append(entity);

                    if (!string.IsNullOrWhiteSpace(formattedKeys))
                    {
                        uri.Append('(' + formattedKeys + ')');
                    }
                }

                // For Read and ReadAll we don't add operation name to the URL
                if (operation == ActionNames.Read || operation == ActionNames.ReadAll)
                {
                    operation = null;
                }

                // For batch operation, we need full request uri,
                // so we need to append parameters after the operation name
                if (!string.IsNullOrWhiteSpace(operation))
                {
                    uri.AppendFormat("/{0}", operation);

                    if (inlineParameters)
                    {
                        string inlineParams = Serializer.GetParameterString(this, GetOperationParameters(false, operationParameters));
                        uri.Append(inlineParams);
                    }
                }

                this.AppendExpandProperties(uri, expandProperties);
                this.AppendQueryResultSettings(uri, queryResultSettings);
                return new Uri(uri.ToString(), UriKind.Relative);
            }

            private void AppendExpandProperties(StringBuilder uri, IEnumerable<string> expandProperties)
            {
                if (expandProperties == null)
                {
                    return;
                }

                AppendQueryDelimiter(uri);

                uri.Append(string.Format(
                    CultureInfo.InvariantCulture,
                    "$expand={0}",
                    string.Join(",", expandProperties.Where(s => !string.IsNullOrWhiteSpace(s)))));
            }

            private void AppendQueryResultSettings(StringBuilder uri, QueryResultSettings queryResultSettings)
            {
                if (queryResultSettings != null && (queryResultSettings.Paging != null || (queryResultSettings.Sorting != null && queryResultSettings.Sorting.Columns.Count > 0)))
                {
                    AppendQueryDelimiter(uri);

                    if (queryResultSettings.Paging != null)
                    {
                        uri.Append(string.Format(
                            CultureInfo.InvariantCulture,
                            "$skip={0}&$top={1}",
                            queryResultSettings.Paging.Skip,
                            queryResultSettings.Paging.Top));
                    }

                    if (queryResultSettings.Sorting != null && queryResultSettings.Sorting.Columns.Count > 0)
                    {
                        AppendQueryDelimiter(uri);

                        uri.Append("$orderby=");
                        foreach (SortColumn sc in queryResultSettings.Sorting.Columns)
                        {
                            if (sc.IsDescending.HasValue && sc.IsDescending.Value)
                            {
                                uri.AppendFormat("{0}%20{1}", sc.ColumnName, "desc");
                            }
                            else
                            {
                                uri.AppendFormat("{0}", sc.ColumnName, "asc");
                            }
                        }
                    }
                }
            }

            private void OnCommerceContextBuildingRequest(object sender, BuildingRequestEventArgs args)
            {
                // Adds the api version as a query argument.
                args.RequestUri = GetRequestUriWithApiVersion(args.RequestUri, this.ApiVersion);
            }

            private void OnCommerceContextSendingRequest(object sender, SendingRequest2EventArgs args)
            {
                var httpWebRequestMessage = args.RequestMessage as HttpWebRequestMessage;

                if (httpWebRequestMessage != null)
                {
                    if (this.operatingUnitNumber != null)
                    {
                        httpWebRequestMessage.SetHeader(OperatingUnitNumberHeaderName, this.operatingUnitNumber);
                    }

                    if (this.userToken != null)
                    {
                        httpWebRequestMessage.SetHeader(AuthorizationHeaderName, string.Format("{0} {1}", this.userToken.SchemeName, this.userToken.Token));
                    }

                    if (this.deviceToken != null)
                    {
                        httpWebRequestMessage.SetHeader(DeviceTokenHeaderName, this.deviceToken);
                    }

                    if (!string.IsNullOrWhiteSpace(this.Locale))
                    {
                        httpWebRequestMessage.SetHeader("Accept-Language", this.Locale);
                    }

                    httpWebRequestMessage.HttpWebRequest.CookieContainer = this.cookieContainer;
                }
            }

            /// <summary>
            /// Retrieve the entities and exceptions from the DataServiceResponse.
            /// </summary>
            /// <param name="response">The DataServiceResponse returned by batch read operation.</param>
            /// <param name="batchCompletion">The list of TaskCompletionSource to retrieve the result.</param>
            private void UnpackBatchResponse(DataServiceResponse response, List<TaskCompletionSource<object>> batchCompletion)
            {
                int i = 0;
                IEnumerator<OperationResponse> enumerator = response.GetEnumerator();

                // foreach (OperationResponse operationResponse in response)
                while (enumerator.MoveNext())
                {
                    OperationResponse operationResponse = enumerator.Current;

                    // This means this request failed.
                    if (operationResponse.Error != null)
                    {
                        // Set the exception as the result so that we can analyze it later.
                        batchCompletion[i++].SetException(operationResponse.Error);
                        continue;
                    }

                    batchCompletion[i++].SetResult((QueryOperationResponse)operationResponse);
                }
            }

            private void AssignChangedProperties(CommerceEntity source, CommerceEntity target)
            {
                if (source == null || target == null)
                {
                    return;
                }

                foreach (var propertyName in source.ChangedProperties)
                {
                    target[propertyName] = source[propertyName];
                }
            }

            /// <summary>
            /// Sets the property value of ApplyingChanges.
            /// </summary>
            /// <param name="applying">The flag indicates if the context is applying changes.</param>
            private void SetApplyingChanges(bool applying)
            {
                if (this.applyingChangesProperty == null)
                {
                    this.applyingChangesProperty = this.GetType().GetRuntimeProperty("ApplyingChanges");
                }

                this.applyingChangesProperty.SetValue(this, applying, null);
            }
        }
    }
}
