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
    namespace Retail.Ecommerce.Sdk.Core.Publishing
    {
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Encapsulates set of callbacks used to complete a channel publishing.
        /// </summary>
        public interface IChannelPublisher
        {
            /// <summary>
            /// Provides the channel's parameters which can be used to publish a channel to the target.
            /// </summary>
            /// <param name="parameters">Available channel's parameters.</param>
            /// <param name="isPublishingRequested">True if channel's publishing was requested from AX. False otherwise.</param>
            /// <remarks>The implementer can use this method to, for instance, instantiate a navigational hierarchy in the target channel.</remarks>
            void OnChannelInformationAvailable(PublishingParameters parameters, bool isPublishingRequested);
    
            /// <summary>
            /// Provides a way for the implementer to validate the channel's attributes.
            /// </summary>
            /// <param name="attributes">Set of attributes available for the channel.</param>
            /// <remarks>The method should throw exception if validation fails.</remarks>
            void OnValidateProductAttributes(IEnumerable<AttributeProduct> attributes);
        }
    }
}
