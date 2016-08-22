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
        using System.Collections.Concurrent;
        using System.Collections.Generic;
        using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Caches the requests and tasks for batch operations of each calling thread.
        /// </summary>
        public class ParametersCache
        {
            private ConcurrentDictionary<string, List<ParametersGroup>> requestsCache = new ConcurrentDictionary<string, List<ParametersGroup>>();
            private ConcurrentDictionary<string, List<TaskCompletionSource<object>>> tasksCache = new ConcurrentDictionary<string, List<TaskCompletionSource<object>>>();

            /// <summary>
            /// Puts the request into the cache for the calling thread.
            /// </summary>
            /// <typeparam name="T">The type parameter.</typeparam>
            /// <param name="batchId">The batchId to group the requests into one single batch operation.</param>
            /// <param name="entitySet">The entity set of this request.</param>
            /// <param name="entitySetType">The name of entity set type.</param>
            /// <param name="functionName">The function name of this request.</param>
            /// <param name="settings">The query result settings of this request.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="operationParameters">The operation parameters of this request.</param>
            /// <returns>The TaskCompletionSource object to retrieve the result.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Need to cache tasks for requests returning different kinds of results.")]
            public TaskCompletionSource<object> PutParameters<T>(Guid batchId, string entitySet, string entitySetType, string functionName, QueryResultSettings settings, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
            {
                if (!this.IsThreadRegistered(batchId))
                {
                    throw new InvalidOperationException("The calling thread is trying to cache tasks for batch operation, but the thread has not been registered on this cache.");
                }
                else
                {
                    ParametersGroup prametersGroup = new ParametersGroup(entitySet, entitySetType, functionName, typeof(T), settings, expandProperties, operationParameters);
                    this.requestsCache[batchId.ToString()].Add(prametersGroup);
    
                    TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
                    this.tasksCache[batchId.ToString()].Add(taskCompletionSource);
                    return taskCompletionSource;
                }
            }
    
            /// <summary>
            /// Gets the cached requests for this calling thread.
            /// </summary>
            /// <param name="batchId">The batchId to group the requests into one single batch operation.</param>
            /// <returns>The list of requests associated with calling thread.</returns>
            public List<ParametersGroup> GetParameters(Guid batchId)
            {
                if (this.IsThreadRegistered(batchId))
                {
                    return this.requestsCache[batchId.ToString()];
                }
                else
                {
                    throw new InvalidOperationException("The calling thread is trying to read the cached requests for batch operations, but the thread has not been registered on this cache.");
                }
            }
    
            /// <summary>
            /// Gets the cached tasks for this calling thread.
            /// </summary>
            /// <param name="batchId">The batchId to group the requests into one single batch operation.</param>
            /// <returns>The list of TaskCompletionSource associated with calling thread.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It's necessary herer.")]
            public List<TaskCompletionSource<object>> GetTasks(Guid batchId)
            {
                if (this.IsThreadRegistered(batchId))
                {
                    return this.tasksCache[batchId.ToString()];
                }
                else
                {
                    throw new InvalidOperationException("The calling thread is trying to read the cached tasks for batch operations, but the thread has not been registered on this cache.");
                }
            }
    
            /// <summary>
            /// Clears the cache for the calling thread.
            /// </summary>
            /// <param name="batchId">The batchId to group the requests into one single batch operation.</param>
            public void Clear(Guid batchId)
            {
                if (this.IsThreadRegistered(batchId))
                {
                    List<TaskCompletionSource<object>> removedTasks = null;
                    this.tasksCache.TryRemove(batchId.ToString(), out removedTasks);
    
                    List<ParametersGroup> removedRequests = null;
                    this.requestsCache.TryRemove(batchId.ToString(), out removedRequests);
                }
                else
                {
                    throw new InvalidOperationException("The calling thread is trying to clear the cache for batch operations, but the thread has not been registered on this cache.");
                }
            }
    
            /// <summary>
            /// Registers a thread on this cache.
            /// </summary>
            /// <param name="batchId">The batchId to group the requests into one single batch operation.</param>
            public void RegisterThread(Guid batchId)
            {
                if (this.IsThreadRegistered(batchId))
                {
                    return;
                }
                else
                {
                    this.requestsCache[batchId.ToString()] = new List<ParametersGroup>();
                    this.tasksCache[batchId.ToString()] = new List<TaskCompletionSource<object>>();
                }
            }
    
            /// <summary>
            /// Determines if a thread has registered on this cache.
            /// </summary>
            /// <param name="batchId">The batchId to group the requests into one single batch operation.</param>
            /// <returns>True if the calling thread has registered on this cache, otherwise false.</returns>
            public bool IsThreadRegistered(Guid batchId)
            {
                return this.requestsCache.ContainsKey(batchId.ToString());
            }
        }
    }
}
