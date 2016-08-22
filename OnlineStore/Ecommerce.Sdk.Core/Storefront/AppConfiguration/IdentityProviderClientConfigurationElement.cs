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
    namespace Retail.Ecommerce.Sdk.Core
    {
        using System;
        using System.Configuration;

        /// <summary>
        /// Defines IdentityProvider configuration element.
        /// </summary>
        public class IdentityProviderClientConfigurationElement : ConfigurationElement
        {
            private const string PropName = "name";
            private const string PropIssuer = "issuer";
            private const string PropClientId = "clientId";
            private const string PropClientSecret = "clientSecret";
            private const string PropRedirectUrl = "redirectUrl";
            private const string PropLogOffUrl = "logOffUrl";
            private const string PropProviderType = "providerType";
            private const string PropDomainHint = "domainHint";
            private const string PropImageUrl = "imageUrl";
            private const string PropDisplayIndex = "displayIndex";

            /// <summary>
            /// Gets the identity Provider's Name.
            /// </summary>
            [ConfigurationProperty(PropName)]
            public string Name
            {
                get
                {
                    return (string)this[PropName];
                }
            }

            /// <summary>
            /// Gets the issuer.
            /// </summary>
            [ConfigurationProperty(PropIssuer)]
            public Uri Issuer
            {
                get
                {
                    return (Uri)this[PropIssuer];
                }
            }

            /// <summary>
            /// Gets the Client ID registered with the Identity Provider.
            /// </summary>
            [ConfigurationProperty(PropClientId)]
            public string ClientId
            {
                get
                {
                    return (string)this[PropClientId];
                }
            }

            /// <summary>
            /// Gets the Client Secret issued by the Identity Provider.
            /// </summary>
            [ConfigurationProperty(PropClientSecret)]
            public string ClientSecret
            {
                get
                {
                    return (string)this[PropClientSecret];
                }
            }

            /// <summary>
            /// Gets the Redirect URL registered with the Identity Provider.
            /// </summary>
            [ConfigurationProperty(PropRedirectUrl)]
            public Uri RedirectUrl
            {
                get
                {
                    return (Uri)this[PropRedirectUrl];
                }
            }

            /// <summary>
            /// Gets the identity provider type.
            /// </summary>
            [ConfigurationProperty(PropProviderType)]
            public IdentityProviderType ProviderType
            {
                get
                {
                    return (IdentityProviderType)this[PropProviderType];
                }
            }

            /// <summary>
            /// Gets the Domain Hint.
            /// </summary>
            /// <remarks>Identifies social identity provider.</remarks>
            [ConfigurationProperty(PropDomainHint)]
            public string DomainHint
            {
                get
                {
                    return (string)this[PropDomainHint];
                }
            }

            /// <summary>
            /// Gets the External provider's log off URL.
            /// </summary>
            [ConfigurationProperty(PropLogOffUrl)]
            public Uri LogOffUrl
            {
                get
                {
                    return (Uri)this[PropLogOffUrl];
                }
            }

            /// <summary>
            /// Gets the provider image url.
            /// </summary>
            [ConfigurationProperty(PropImageUrl)]
            public Uri ImageUrl
            {
                get
                {
                    return (Uri)this[PropImageUrl];
                }
            }

            /// <summary>
            /// Gets the display index which is supposed to be taken into account while figuring out an order of rendering in case multiple providers exist.
            /// </summary>
            [ConfigurationProperty(PropDisplayIndex)]
            public int DisplayIndex
            {
                get
                {
                    return (int)this[PropDisplayIndex];
                }
            }
        }
    }
}