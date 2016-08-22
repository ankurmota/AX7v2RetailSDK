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
        using System;
    
        /// <summary>
        /// Represents unique ID of the listing.
        /// </summary>
        public sealed class ListingIdentity : IEquatable<ListingIdentity>
        {
            /// <summary>
            /// Gets or sets Product Id.
            /// </summary>
            public long ProductId
            {
                get;
                set;
            }
    
            /// <summary>
            /// Gets or sets Language Id.
            /// </summary>
            public string LanguageId
            {
                get;
                set;
            }
    
            /// <summary>
            /// Gets or sets Catalog Id.
            /// </summary>
            public long CatalogId
            {
                get;
                set;
            }
    
            /// <summary>
            /// Gets or sets Tag.
            /// </summary>
            public string Tag
            {
                get;
                set;
            }
    
            /// <summary>
            /// Equals operation with listing identity.
            /// </summary>
            /// <param name="other">Other listing identity.</param>
            /// <returns>Returns a value indicating whether they are equal.</returns>
            public bool Equals(ListingIdentity other)
            {
                if (other == null)
                {
                    return false;
                }
    
                return
                    this.ProductId == other.ProductId &&
                    this.LanguageId == other.LanguageId &&
                    this.CatalogId == other.CatalogId &&
                    this.Tag == other.Tag;
            }
    
            /// <summary>
            /// Equals operation with object.
            /// </summary>
            /// <param name="obj">Accepts object.</param>
            /// <returns>Return a value indicating whether they are equal.</returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
    
                ListingIdentity otherIdentity = obj as ListingIdentity;
                if (otherIdentity == null)
                {
                    return false;
                }
    
                return otherIdentity.Equals(this);
            }
    
            /// <summary>
            /// Get hash code.
            /// </summary>
            /// <returns>Returns hash code.</returns>
            public override int GetHashCode()
            {
                int tagIdHash = this.Tag == null ? 0 : this.Tag.GetHashCode();
                int languageHash = this.LanguageId == null ? 0 : this.LanguageId.GetHashCode();
                return this.ProductId.GetHashCode() ^ languageHash ^ this.CatalogId.GetHashCode() ^ tagIdHash;
            }
        }
    }
}
