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
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// This class encapsulates the helper routines related to the media storage SAS key.
        /// </summary>
        internal class SasKeyHelper
        {
            /// <summary>
            /// This routine adds the SAS key with the images by obtaining the SAS key from the transaction service.
            /// </summary>
            /// <param name="mediaDetails">The RichMediaLocation details.</param>
            /// <param name="context">The request context.</param>
            public static void AddSaskeyWithMedia(IDictionary<string, RichMediaLocations> mediaDetails, RequestContext context)
            {
                if (!context.Runtime.Configuration.IsMasterDatabaseConnectionString)
                {
                    return; // Do not need to add the SAS key for the offline mode.
                }
    
                Dictionary<string, RichMediaLocations> selfHostedImages = new Dictionary<string, RichMediaLocations>(StringComparer.OrdinalIgnoreCase);
                foreach (string key in mediaDetails.Keys)
                {
                    if (!(mediaDetails[key] != null && mediaDetails[key].Items != null && mediaDetails[key].Items.Any()))
                    {
                        continue;
                    }
    
                    // Adding the defult image in the collection.
                    var defaultImageOfEmployee = mediaDetails[key].Items.Where(item => item.isDefault == true).Select(item => new RichMediaLocationsRichMediaLocation() { AltText = item.AltText, isDefault = item.isDefault, IsSelfHosted = item.IsSelfHosted, Url = item.Url }).ToArray();
                    if (defaultImageOfEmployee != null && defaultImageOfEmployee.Any())
                    {
                        mediaDetails[key].Items = defaultImageOfEmployee;
                    }
    
                    if (mediaDetails[key].Items.Any(item => item.IsSelfHosted == true && item.IsSasKeyAlreadyAdded == false))
                    {
                        selfHostedImages.Add(key, mediaDetails[key]); // there are self-hosted images hence the storage SAS key need to be appended with these image urls.
                    }
                }
    
                if (selfHostedImages != null && selfHostedImages.Any())
                {
                    var mediaStorageSecurityServiceRequest = new AssociateSasKeyRealtimeRequest(selfHostedImages);
                    var mediaStorageSecurityServiceResponse = context.Execute<AssociateSasKeyRealtimeResponse>(mediaStorageSecurityServiceRequest);
                    selfHostedImages = mediaStorageSecurityServiceResponse.MediaDictionary;
                }
    
                foreach (string key in selfHostedImages.Keys)
                {
                    if (selfHostedImages.ContainsKey(key) && selfHostedImages[key] != null && selfHostedImages[key].Items.Any())
                    {
                        mediaDetails[key] = selfHostedImages[key];
                    }
                }
            }
        }
    }
}
