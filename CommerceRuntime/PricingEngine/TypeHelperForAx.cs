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
    namespace Commerce.Runtime.Services.PricingEngine
    {
        using System;
        using System.Collections.Generic;
    
        /// <summary>
        /// Type helper for Dynamics AX.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed", Justification = "This class is used by AX, so omitting any breaking changes.")]
        public class TypeHelperForAx
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="TypeHelperForAx" /> class from being created.
            /// </summary>
            private TypeHelperForAx()
            {
            }
    
            /// <summary>
            /// Create a set of string.
            /// </summary>
            /// <returns>An empty set of string.</returns>
            public static ISet<string> CreateSetOfString()
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
    
            /// <summary>
            /// Create a set of long.
            /// </summary>
            /// <returns>An empty set of long.</returns>
            public static ISet<long> CreateSetOfLong()
            {
                return new HashSet<long>();
            }
    
            /// <summary>
            /// Create a list of a type at runtime.
            /// </summary>
            /// <param name="type">The base type.</param>
            /// <returns>An empty list of the given type.</returns>
            public static object CreateListOfType(Type type)
            {
                Type listType = typeof(List<>).MakeGenericType(type);
    
                return Activator.CreateInstance(listType);
            }
        }
    }
}
