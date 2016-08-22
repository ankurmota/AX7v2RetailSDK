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
        using Microsoft.Dynamics.Commerce.Runtime;

        internal class ActionNames
        {
            /// <summary>
            /// CRUD action names defined by proxy.
            /// </summary>
            /// <remarks>
            /// The following action names can not be retrieved from OData metadata.
            /// </remarks>
            public const string Create = "Create";
            public const string Read = "Read";
            public const string ReadAll = "ReadAll";
            public const string ReadStream = "ReadStream";
            public const string Update = "Update";
            public const string Delete = "Delete";
        }
    }
}
