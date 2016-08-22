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
        using System.Configuration;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.Configuration;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Represents utility class for the Commerce Runtime.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Crt", Justification = "Crt is well known term.")]
        public static class CrtUtilities
        {
            /// <summary>
            /// Represents an invalid value for the default channel identifier which is used as a seed value here.
            /// It is being assumed that a valid channel identifier would never be this value.
            /// </summary>
            private const long InvalidDefaultChannelId = 0;
    
            /// <summary>
            /// Identifies a configuration key to access CRT DB Connection string.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Crt", Justification = "Crt is well known CommerceRunTime")]
            private const string CrtConnectionStringKeyName = "CommerceRuntimeConnectionString";
    
            /// <summary>
            /// Caches the default channel identifier between web service calls.
            /// </summary>
            /// <remarks>This variable is not thread safe.</remarks>
            private static long defaultChannelIdentifer = InvalidDefaultChannelId;
    
            /// <summary>
            /// Caches the commerce runtime configuration between web service calls.
            /// </summary>
            /// <remarks>This variable is not thread safe.</remarks>
            private static CommerceRuntimeConfiguration crtConfiguration = null;
    
            /// <summary>
            /// Gets the CommerceRuntime instance initialized by using the currently executing application's config.
            /// </summary>
            /// <returns>Commerce runtime instance.</returns>
            /// <remarks>
            /// Caches the commerce runtime configuration and default channel identifier.
            /// </remarks>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The method might be called by other implementations.")]
            public static CommerceRuntime GetCommerceRuntime()
            {
                if (crtConfiguration == null)
                {
                    crtConfiguration = GetCrtConfiguration();
                }
    
                CommerceRuntime runtime = GetCommerceRuntime(crtConfiguration);
    
                return runtime;
            }
    
            /// <summary>
            /// Gets the CommerceRuntime instance initialized by using the provided application configuration.
            /// </summary>
            /// <param name="appConfiguration">The application configuration.</param>
            /// <returns>Commerce runtime instance.</returns>
            /// <remarks>Caches the default channel identifier.</remarks>
            public static CommerceRuntime GetCommerceRuntime(Configuration appConfiguration)
            {
                string initialConnectionString = CommerceRuntimeConfigurationManager.GetInitialConnectionString(appConfiguration);
                CommerceRuntimeSection section = CommerceRuntimeConfigurationManager.GetConfigurationSection(appConfiguration, CommerceRuntimeConfigurationManager.SectionName);
                Dictionary<string, string> connectionStrings = CommerceRuntimeConfigurationManager.GetStorageLookupConnectionStrings(appConfiguration);
                CommerceRuntimeConfiguration commerceRuntimeConfiguration = new CommerceRuntimeConfiguration(section, initialConnectionString, connectionStrings);
                CommerceRuntime runtime = GetCommerceRuntime(commerceRuntimeConfiguration);
    
                return runtime;
            }
    
            /// <summary>
            /// Gets the commerce runtime configuration by using the currently executing application's config..
            /// </summary>
            /// <returns>The commerce runtime configuration.</returns>
            internal static CommerceRuntimeConfiguration GetCrtConfiguration()
            {
                string initialConnectionString = CommerceRuntimeConfigurationManager.GetInitialConnectionString();
                CommerceRuntimeSection section = CommerceRuntimeConfigurationManager.GetConfigurationSection(CommerceRuntimeConfigurationManager.SectionName);
                Dictionary<string, string> connectionStrings = CommerceRuntimeConfigurationManager.GetStorageLookupConnectionStrings();
                CommerceRuntimeConfiguration commerceRuntimeConfiguration = new CommerceRuntimeConfiguration(section, initialConnectionString, connectionStrings);
    
                return commerceRuntimeConfiguration;
            }
    
            /// <summary>
            /// Gets the commerce runtime based on the passed in commerce runtime configuration.
            /// </summary>
            /// <param name="commerceRuntimeConfiguration">The commerce runtime configuration.</param>
            /// <returns>An instance of commerce runtime.</returns>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.ConfigurationException">The default channel identifier cannot be zero.</exception>
            private static CommerceRuntime GetCommerceRuntime(CommerceRuntimeConfiguration commerceRuntimeConfiguration)
            {
                if (defaultChannelIdentifer == InvalidDefaultChannelId)
                {
                    using (CommerceRuntime commerceRuntime = CommerceRuntime.Create(commerceRuntimeConfiguration, CommercePrincipal.AnonymousPrincipal))
                    {
                        ChannelManager channelManager = ChannelManager.Create(commerceRuntime);
                        defaultChannelIdentifer = channelManager.GetDefaultChannelId();
                    }
    
                    if (defaultChannelIdentifer == InvalidDefaultChannelId)
                    {
                        string message = string.Format(CultureInfo.InvariantCulture, "The default channel identifier {0} was returned from CRT. Please ensure that a default operating unit number has been specified as part of the <commerceRuntime> configuration section.", defaultChannelIdentifer);
                        throw new Microsoft.Dynamics.Commerce.Runtime.ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration, message);
                    }
                }
    
                CommercePrincipal principal = new CommercePrincipal(new CommerceIdentity(defaultChannelIdentifer, new string[] { CommerceRoles.Storefront }));
                CommerceRuntime runtime = CommerceRuntime.Create(commerceRuntimeConfiguration, principal);
    
                return runtime;
            }
        }
    }
}
