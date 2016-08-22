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
    namespace Commerce.Runtime.TestHost
    {
        using System.Configuration;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.Configuration;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// The commerce runtime manager.
        /// </summary>
        internal static class CommerceRuntimeManager
        {
            /// <summary>
            /// Gets or sets the current commerce identity.
            /// </summary>
            public static CommerceIdentity Identity { get; internal set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Sample code.")]
            internal static string Locale { get; set; }

            /// <summary>
            /// Gets or sets the specified roles.
            /// </summary>
            internal static string[] SpecifiedRoles { get; set; }

            /// <summary>
            /// Gets the runtime.
            /// </summary>
            internal static CommerceRuntime Runtime
            {
                get
                {
                    // Constructs the CommerceIdentity if the default roles are provided.
                    if (Identity == null && SpecifiedRoles != null && SpecifiedRoles.Any())
                    {
                        Identity = GetDefaultCommerceIdentity();
                    }

                    if (Identity != null)
                    {
                        CommercePrincipal principal = new CommercePrincipal(Identity);

                        return CommerceRuntime.Create(CommerceRuntimeManager.GetDefaultCommerceRuntimeConfiguration(), principal, CommerceRuntimeManager.Locale);
                    }

                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "Commerce identity is not provided.");
                }
            }

            private static CommerceIdentity GetDefaultCommerceIdentity()
            {
                using (CommerceRuntime commerceRuntime = CommerceRuntime.Create(CommerceRuntimeManager.GetDefaultCommerceRuntimeConfiguration(), CommercePrincipal.AnonymousPrincipal))
                {
                    ChannelManager channelManager = ChannelManager.Create(commerceRuntime);
                    var defaultChannelId = channelManager.GetDefaultChannelId();

                    return new CommerceIdentity(defaultChannelId, SpecifiedRoles);
                }
            }

            private static CommerceRuntimeConfiguration GetDefaultCommerceRuntimeConfiguration()
            {
                var connectionString = ConfigurationManager.ConnectionStrings["HoustonStore"].ConnectionString;
                var section = CommerceRuntimeConfigurationManager.GetConfigurationSection(CommerceRuntimeConfigurationManager.SectionName);
                return new CommerceRuntimeConfiguration(section, connectionString, null, true, true);
            }
        }
    }
}
