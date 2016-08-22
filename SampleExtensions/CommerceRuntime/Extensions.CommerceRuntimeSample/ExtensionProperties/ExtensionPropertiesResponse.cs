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
    namespace Commerce.Runtime.Sample.ExtensionProperties.Messages
    {
        using System.Collections.ObjectModel;
        using System.Runtime.Serialization;
        using Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Defines a simple response class.
        /// </summary>
        [DataContract]
        public sealed class ExtensionPropertiesResponse : Response
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExtensionPropertiesResponse"/> class.
            /// </summary>
            /// <param name="entity">The entity.</param>
            public ExtensionPropertiesResponse(ExtensionPropertyEntity entity)
            {
                this.Entity = entity;
            }

            /// <summary>
            /// Gets the found store entity.
            /// </summary>
            [DataMember]
            public ExtensionPropertyEntity Entity { get; private set; }
        }
    }
}