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
    namespace Retail.Ecommerce.Publishing
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Retail.Ecommerce.Sdk.Core.Publishing;
    
        internal class ChannelPublisher : IChannelPublisher
        {
            /// <summary>
            /// Provides the channel's parameters which can be used to publish a channel to the target.
            /// </summary>
            /// <param name="parameters">Available channel's parameters.</param>
            /// <param name="isPublishingRequested">True if channel's publishing was requested from AX. False otherwise.</param>
            /// <remarks>The implementer can use this method to, for instance, instantiate a navigational hierarchy in the target channel.</remarks>
            public void OnChannelInformationAvailable(PublishingParameters parameters, bool isPublishingRequested)
            {
                if (parameters == null)
                {
                    throw new ArgumentNullException("parameters");
                }
    
                Trace.TraceInformation(
                    "Channel publishing information is available. Publishing requested={0}, DefaultCulture={1}, Number of Categories={2}, Number of Attributes={3}",
                    isPublishingRequested,
                    parameters.ChannelDefaultCulture,
                    parameters.Categories.Count(),
                    parameters.CategoriesAttributes.Count);
    
                StringBuilder builder = new StringBuilder();
                Trace.TraceInformation("The following categories are available:");
                foreach (Category category in parameters.Categories)
                {
                    builder.AppendFormat("Name={0}; ID={1}; Parent={2}\r\n", category.Name, category.ParentCategory, category.RecordId);
                }
    
                Trace.TraceInformation(builder.ToString());
            }
    
            /// <summary>
            /// Provides a way for the implementer to validate the channel's attributes.
            /// </summary>
            /// <param name="attributes">Set of attributes available for the channel.</param>
            /// <remarks>The method should throw exception if validation fails.</remarks>
            public void OnValidateProductAttributes(IEnumerable<AttributeProduct> attributes)
            {
                if (attributes == null)
                {
                    throw new ArgumentNullException("attributes");
                }
    
                Trace.TraceInformation("Validating attributes ...");
                StringBuilder builder = new StringBuilder();
                foreach (AttributeProduct attribute in attributes)
                {
                    builder.AppendFormat("Key={0}; Type={1}\r\n", attribute.KeyName, attribute.DataType);
                }
    
                Trace.TraceInformation(builder.ToString());
    
                Trace.TraceInformation("Attribute validation completed.\r\n");
            }
        }
    }
}
