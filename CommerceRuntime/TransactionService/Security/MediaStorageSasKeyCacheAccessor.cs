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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        
        /// <summary>
        /// This class encapsulates the media storage SAS key cache accessing logic. 
        /// </summary>
        public class MediaStorageSasKeyCacheAccessor : DataCacheAccessor
        {
            private const string GetMediaStorageSasKeyMethodName = "GetMediaStorageSasKey";        
            private static readonly TimeSpan ValidityInterval = TimeSpan.FromMinutes(30);
    
            /// <summary>
            /// Initializes a new instance of the <see cref="MediaStorageSasKeyCacheAccessor"/> class.
            /// </summary>
            /// <param name="dataStore">The data store.</param>
            /// <param name="context">The request context <see cref="RequestContext"/>.</param>
            public MediaStorageSasKeyCacheAccessor(IDataStore dataStore, RequestContext context)
                : base(dataStore, context)
            {
            }
    
            /// <summary>
            /// Gets the next expiration date based on current date and <see cref="ValidityInterval"/>.
            /// </summary>
            private static DateTimeOffset NextExpirationDateTime
            {
                get
                {
                    return DateTimeOffset.Now.Add(ValidityInterval);
                }
            }
    
            /// <summary>
            /// Gets the media storage SAS key.
            /// </summary>
            /// <param name="sasKey">The media storage SAS key.</param>
            /// <returns>A value indicating whether it was a cache hit.</returns>
            public bool GetMediaStorageSasKey(out string sasKey)
            {
                string key = this.GenerateKey(GetMediaStorageSasKeyMethodName);
                return this.TryGetItem(key, out sasKey);
            }
    
            /// <summary>
            /// Sets the media storage SAS key.
            /// </summary>
            /// <param name="sasKey">The media storage SAS key.</param>
            public void PutMediaStorageSasKey(string sasKey)
            {
                string key = this.GenerateKey(GetMediaStorageSasKeyMethodName);
                this.PutItem(key, sasKey, NextExpirationDateTime);
            }
    
            /// <summary>
            /// Returns the key pattern specific to this implementation of the cache accessor.
            /// </summary>
            /// <returns>The key pattern specific to this implementation of the cache accessor.</returns>
            protected override string GetKeyPattern()
            {
                return string.Format("{0}\\", "0");            
            }
        }
    }
}
