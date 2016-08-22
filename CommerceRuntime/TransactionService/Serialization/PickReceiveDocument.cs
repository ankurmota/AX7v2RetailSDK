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
    namespace Commerce.Runtime.TransactionService.Serialization
    {
        using System;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Data Contract for pick receive information result serialization.
        /// </summary>
        [Serializable]
        public class PickReceiveDocument : SerializableObject
        {
            /// <summary>
            /// Gets or sets the picking or receiving document as xml.
            /// </summary>
            public string XMLDocument { get; set; }
    
            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns>Deserialized PickReceiveDocument instance from source.</returns>
            public static PickReceiveDocument Deserialize(string source)
            {
                ThrowIf.NullOrWhiteSpace(source, "source");
    
                return SerializationHelper.DeserializeObjectFromXml<PickReceiveDocument>(source);
            }
    
            /// <summary>
            /// Serializes the specified picking receiving document.
            /// </summary>
            /// <param name="pickReceiveDocument">The pick receive document.</param>
            /// <returns>Serialized string for the pick receive document.</returns>
            public static string Serialize(PickReceiveDocument pickReceiveDocument)
            {
                ThrowIf.Null(pickReceiveDocument, "pickReceiveDocument");
    
                return SerializationHelper.SerializeObjectToXml<PickReceiveDocument>(pickReceiveDocument);
            }
        }
    }
}
