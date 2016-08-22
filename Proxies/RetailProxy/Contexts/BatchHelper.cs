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
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Provides helper methods to execute batch operations.
        /// </summary>
        internal static class BatchHelper
        {
            /// <summary>
            /// Sends the batch request to the server and clears the cache and the batchId.
            /// </summary>
            /// <param name="context">The context that sends the request.</param>
            /// <param name="cache">The cache containing the parameters of cached methods.</param>
            /// <param name="batchId">The batchId to retrieve the cached parameters.</param>
            /// <returns>A Task.</returns>
            internal static async Task ExecuteBatch(IContext context, ParametersCache cache, ThreadLocal<Guid> batchId)
            {
                if (!BatchHelper.IsInBatchMode(batchId))
                {
                    throw new InvalidOperationException("The calling thread was trying to do batch operation but it didn't begin the batch operations before.");
                }
    
                Guid localBatchId = batchId.Value;
    
                // Clears the batchId before executing the batch operation since after do "await" the original
                // thread will be used to do ExecuteBatchAsync and a new thread will be created to do the rest work.
                // So we will not be able to clear the original thread any more.
                batchId.Value = Guid.Empty;
    
                List<ParametersGroup> requests = cache.GetParameters(localBatchId);
                List<TaskCompletionSource<object>> tasks = cache.GetTasks(localBatchId);
    
                try
                {
                    await context.ExecuteBatchAsync(requests, tasks);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    // Clear the cache for this thread.
                    cache.Clear(localBatchId);
                }
            }
    
            /// <summary>
            /// Checks if the current thread is in batch mode.
            /// </summary>
            /// <param name="batchId">The batchId of current thread.</param>
            /// <returns>True if current thread is in batch mode, otherwise false.</returns>
            internal static bool IsInBatchMode(ThreadLocal<Guid> batchId)
            {
                return batchId != null && batchId.Value != null && batchId.Value != Guid.Empty;
            }
        }
    }
}
