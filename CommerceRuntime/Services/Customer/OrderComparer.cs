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
    namespace Commerce.Runtime.Services
    {
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Encapsulates comparison logic of orders for equality.
        /// </summary>
        public class OrderComparer : IEqualityComparer<SalesOrder>
        {
            /// <summary>
            /// Compares the two orders for equality.
            /// </summary>
            /// <param name="x">The order object.</param>
            /// <param name="y">The another oder object.</param>
            /// <returns>True, if they are equal; otherwise, false.</returns>
            public bool Equals(SalesOrder x, SalesOrder y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                // Compares sales id first.
                if (!string.IsNullOrWhiteSpace(x.SalesId) && !string.IsNullOrWhiteSpace(y.SalesId) && x.SalesId == y.SalesId)
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(y.Id) && x.Id == y.Id)
                {
                    return true;
                }
                
                // For order created online and fulfilled or cancelled at another channel (store).
                if (!string.IsNullOrWhiteSpace(x.ChannelReferenceId) && !string.IsNullOrWhiteSpace(y.ChannelReferenceId) && x.ChannelReferenceId == y.ChannelReferenceId)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Gets the hash code for the specific order.
            /// </summary>
            /// <param name="obj">The order object.</param>
            /// <returns>The hash code.</returns>
            public int GetHashCode(SalesOrder obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return obj.GetHashCode();
            }
        }
    }
}
