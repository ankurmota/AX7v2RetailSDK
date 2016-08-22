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
        using System.Collections.ObjectModel;
        using System.IO;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Data Contract for return location print parameters serialization.
        /// </summary>
        [Serializable]
        public class ReturnLocationPrintParameter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReturnLocationPrintParameter"/> class.
            /// </summary>
            public ReturnLocationPrintParameter()
            {
                this.Codes = new ReturnLocationPrintParameterCodes();
            }
    
            /// <summary>
            /// Gets or sets the store identifier.
            /// </summary>
            public string StoreId { get; set; }
    
            /// <summary>
            /// Gets or sets the item identifier.
            /// </summary>
            public string ItemId { get; set; }
    
            /// <summary>
            /// Gets or sets the return codes.
            /// </summary>
            public ReturnLocationPrintParameterCodes Codes { get; set; }
    
            /// <summary>
            /// Serialize the class to xml.
            /// </summary>
            /// <returns>The serialized xml.</returns>
            public string ToXml()
            {
                var xmlString = string.Empty;
                var serializer = new XmlSerializer(typeof(ReturnLocationPrintParameter));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, this);
                    writer.Flush();
                    xmlString = writer.ToString();
                }
    
                return xmlString;
            }
        }
    }
}
