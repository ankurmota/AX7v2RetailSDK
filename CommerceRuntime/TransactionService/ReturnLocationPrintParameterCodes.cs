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
        /// Data Contract for return location print parameters codes.
        /// </summary>
        [Serializable]
        public class ReturnLocationPrintParameterCodes
        {
            /// <summary>
            /// Gets or sets the info code identifier.
            /// </summary>
            public string InfocodeId { get; set; }
    
            /// <summary>
            /// Gets or sets the sub code identifier.
            /// </summary>
            public string SubcodeId { get; set; }
        }
    }
}
