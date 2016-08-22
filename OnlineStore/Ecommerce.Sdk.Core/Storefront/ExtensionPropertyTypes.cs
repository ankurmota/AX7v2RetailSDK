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
    namespace Retail.Ecommerce.Sdk.Core
    {
        /// <summary>
        /// The types of values that can be stored in extension properties.
        /// </summary>
        public enum ExtensionPropertyTypes
        {
            /// <summary>
            /// No type specified.
            /// </summary>
            None = 0,

            /// <summary>
            /// A boolean type value.
            /// </summary>
            Boolean = 1,

            /// <summary>
            /// A byte type value.
            /// </summary>
            Byte = 2,

            /// <summary>
            /// A date time offset type value. 
            /// </summary>
            DateTimeOffset = 3,

            /// <summary>
            /// A decimal type value.
            /// </summary>
            Decimal = 4,

            /// <summary>
            /// An integer type value.
            /// </summary>
            Integer = 5,

            /// <summary>
            /// A long type value.
            /// </summary>
            Long = 6,

            /// <summary>
            /// A string type value.
            /// </summary>
            String = 7
        }
    }
}
