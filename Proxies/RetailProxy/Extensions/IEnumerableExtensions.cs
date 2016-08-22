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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using CRT = Microsoft.Dynamics.Commerce.Runtime;
    
        internal static class IEnumerableExtensions
        {
            /// <summary>
            /// Converts the IEnumerable to ObservableCollection.
            /// </summary>
            /// <typeparam name="T">The collection type.</typeparam>
            /// <param name="source">The source.</param>
            /// <returns>The ObservableCollection object.</returns>
            public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source) where T : class
            {
                return new ObservableCollection<T>(source);
            }
    
            /// <summary>
            /// Converts IEnumerable to CRT PagedResult.
            /// </summary>
            /// <typeparam name="T">The collection type.</typeparam>
            /// <param name="source">The source.</param>
            /// <returns>The PagedResult object.</returns>
            public static CRT.PagedResult<T> ToCRTPagedResult<T>(this ReadOnlyCollection<T> source) where T : class
            {
                return new CRT.PagedResult<T>(source);
            }
    
            /// <summary>
            /// Converts IEnumerable to CRT PagedResult.
            /// </summary>
            /// <typeparam name="T">The collection type.</typeparam>
            /// <param name="source">The source.</param>
            /// <returns>The PagedResult object.</returns>
            public static CRT.PagedResult<T> ToCRTPagedResult<T>(this IEnumerable<T> source) where T : class
            {
                var collection = new ReadOnlyCollection<T>(new List<T>(source));
                return new CRT.PagedResult<T>(collection);
            }
        }
    }
}
