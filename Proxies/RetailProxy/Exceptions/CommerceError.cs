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
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Represents the error response object when <see cref="CommerceException"/> is captured, serialized and sent out by the retail service.
        /// </summary>
        /// <remarks>
        /// The <see cref="Type"/> property specified the type of the exception which is serialized as the <see cref="SerializedException"/> string.
        /// This class should be in sync with the definition in ..\..\..\..\..\Services\Web\RetailServer\Core\CommerceError.cs.
        /// </remarks>
        [DataContract]
        public sealed class CommerceError
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceError"/> class.
            /// </summary>
            /// <param name="typeName">The name of the exception's type to be serialized.</param>
            /// <param name="serializedException">The serialized exception in string.</param>
            public CommerceError(string typeName, string serializedException)
            {
                this.TypeName = typeName;
                this.Exception = serializedException;
            }
    
            /// <summary>
            /// Gets the class name of the exception instance in the serialized string.
            /// </summary>
            [DataMember]
            public string TypeName { get; private set; }
    
            /// <summary>
            /// Gets the string represented serialized exception.
            /// </summary>
            [DataMember]
            public string Exception { get; private set; }
        }
    }
}
