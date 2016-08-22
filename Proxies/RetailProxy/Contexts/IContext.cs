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
        using System.Diagnostics.CodeAnalysis;
        using System.IO;
        using System.Linq.Expressions;
        using System.Threading;
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Interface to commerce context.
        /// </summary>
        public interface IContext
        {
            /// <summary>
            /// Gets or sets the locale for the context.
            /// </summary>
            string Locale { get; set; }
    
            /// <summary>
            /// Sets the operating unit number.
            /// </summary>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            void SetOperatingUnitNumber(string operatingUnitNumber);
    
            /// <summary>
            /// Gets the operating unit number.
            /// </summary>
            /// <returns>The context operating unit number.</returns>
            string GetOperatingUnitNumber();
    
            /// <summary>
            /// Sets the device token.
            /// </summary>
            /// <param name="deviceToken">The device token.</param>
            void SetDeviceToken(string deviceToken);
    
            /// <summary>
            /// Gets the device token.
            /// </summary>
            /// <returns>The context device token.</returns>
            string GetDeviceToken();
    
            /// <summary>
            /// Sets the user token.
            /// </summary>
            /// <param name="userToken">The user token.</param>
            void SetUserToken(UserToken userToken);
    
            /// <summary>
            /// Gets the user token.
            /// </summary>
            /// <returns>The context user token.</returns>
            UserToken GetUserToken();
    
            /// <summary>
            /// Creates the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entity">The entity.</param>
            /// <returns>The object of type T.</returns>
            Task<T> Create<T>(string entitySet, T entity)
                where T : ICommerceEntity;
    
            /// <summary>
            /// Deletes the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entity">The entity.</param>
            /// <returns>No return.</returns>
            Task Delete<T>(string entitySet, T entity)
                where T : ICommerceEntity;

            /// <summary>
            /// Reads the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="predicate">The predicate.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>The object of type T.</returns>
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Backward Compatibility.")]
            Task<T> Read<T>(string entitySet, Expression<Func<T, bool>> predicate, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
                where T : ICommerceEntity;

            /// <summary>
            /// Reads the stream for the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="predicate">The predicate.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>The stream for the specified entitySet.</returns>
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Backward Compatibility.")]
            Task<Stream> ReadStream<T>(string entitySet, Expression<Func<T, bool>> predicate, params OperationParameter[] operationParameters)
                where T : ICommerceEntity;

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
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is by design for Async methods.")]
            Task<PagedResult<T>> ReadAll<T>(string entitySet, ICollection<string> expandProperties, QueryResultSettings queryResultSettings)
                where T : ICommerceEntity;
    
            /// <summary>
            /// Updates the specified entity set.
            /// </summary>
            /// <typeparam name="T">The type of of entity.</typeparam>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entity">The entity.</param>
            /// <returns>The object of type T.</returns>
            Task<T> Update<T>(string entitySet, T entity)
                where T : CommerceEntity;
    
            /// <summary>
            /// Executes the operation asynchronous with no result.
            /// </summary>
            /// <param name="entitySet">The entity set.</param>
            /// <param name="entitySetTypeName">The entity set type name.</param>
            /// <param name="operation">The operation name.</param>
            /// <param name="isAction">True, if the operation is an action; false, if the operation is a function.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>No return.</returns>
            Task ExecuteOperationAsync(string entitySet, string entitySetTypeName, string operation, bool isAction, params OperationParameter[] operationParameters);

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
            Task<T> ExecuteOperationSingleResultAsync<T>(string entitySet, string entitySetTypeName, string operation, bool isAction, ICollection<string> expandProperties, params OperationParameter[] operationParameters);

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
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is by design for Async methods.")]
            Task<PagedResult<T>> ExecuteOperationAsync<T>(string entitySet, string entitySetTypeName, string operation, bool isAction, QueryResultSettings queryResultSettings, ICollection<string> expandProperties, params OperationParameter[] operationParameters);

            /// <summary>
            /// Executes the operation asynchronous with no result.
            /// </summary>
            /// <param name="operation">The operation name.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>No return.</returns>
            Task ExecuteAuthenticationOperationAsync(string operation, params OperationParameter[] operationParameters);

            /// <summary>
            /// Executes the operation asynchronous with no result.
            /// </summary>
            /// <typeparam name="T">The type of the result.</typeparam>
            /// <param name="operation">The operation name.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>No return.</returns>
            Task<T> ExecuteAuthenticationOperationSingleResultAsync<T>(string operation, params OperationParameter[] operationParameters);

            /// <summary>
            /// Begins the batch operations for the calling thread.
            /// </summary>
            void BeginBatch();
    
            /// <summary>
            /// Submits the batch request.
            /// </summary>
            /// <returns>A Task.</returns>
            Task ExecuteBatchAsync();
    
            /// <summary>
            /// Submits the batch request.
            /// </summary>
            /// <param name="requests">The requests that will be grouped into one single batch request.</param>
            /// <param name="tasks">The task where to return the results.</param>
            /// <returns>A Task.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It's necessary here.")]
            Task ExecuteBatchAsync(List<ParametersGroup> requests, List<TaskCompletionSource<object>> tasks);
    
            /// <summary>
            /// Gets a single entity from the task.
            /// </summary>
            /// <typeparam name="T">The type of the result.</typeparam>
            /// <param name="task">The task returned by an individual method call such as Read.</param>
            /// <returns>The single entity.</returns>
            T GetSingleEntity<T>(Task<object> task);
    
            /// <summary>
            /// Gets a paged result of entities.
            /// </summary>
            /// <typeparam name="T">The type of entities.</typeparam>
            /// <param name="task">The task returned by an individual method call such as ReadAll.</param>
            /// <returns>The PagedResult of entities.</returns>
            PagedResult<T> GetPagedResult<T>(Task<object> task);
        }
    }
}
