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
        using System.Collections.Generic;
        using System.Configuration;
        using System.Globalization;
        using System.Linq;
        using System.Threading.Tasks;
        using Commerce.RetailProxy;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// The configuration related utilities.
        /// </summary>
        public partial class Utilities
        {
            private const string RetailConfigurationSectionName = "retailConfiguration";

            /// <summary>
            /// The cached operating unit configuration.
            /// </summary>
            private static Dictionary<string, OperatingUnitConfiguration> cachedOperatingUnitConfig = new Dictionary<string, OperatingUnitConfiguration>();

            private static int? retailServerMaximumPageSize = null;

            private static bool? shouldNonCataloguedProductsBeIncludedByDefault = null;

            private static QueryResultSettings defaultQuerySettingsValue;

            private static object syncRoot = new object();

            /// <summary>
            /// Gets a value for default query settings.
            /// </summary>
            public static QueryResultSettings DefaultQuerySettings
            {
                get
                {
                    if (defaultQuerySettingsValue == null)
                    {
                        defaultQuerySettingsValue = new QueryResultSettings() { Paging = new PagingInfo { Skip = 0, Top = Utilities.GetMaxPageSize() } };
                    }

                    return defaultQuerySettingsValue;
                }
            }

            /// <summary>
            /// Gets factory initialized by the id_token.
            /// </summary>
            /// <param name="ecommerceContext">The eCommerce context.</param>
            /// <returns>The factory.</returns>
            public static ManagerFactory GetManagerFactory(EcommerceContext ecommerceContext)
            {
                if (ecommerceContext == null)
                {
                    throw new ArgumentNullException(nameof(ecommerceContext));
                }

                if (string.IsNullOrEmpty(ecommerceContext.OperatingUnitId))
                {
                    RetailLogger.Log.OnlineStoreOperatingUnitNumberNotSetInEcommerceContext();
                    var exception = new NotSupportedException("The operating unit number must be set in the eCommerce context.");
                    throw exception;
                }

                Uri retailServerUri = null;
                OperatingUnitConfiguration operatingUnitConfig = null;
                if (!cachedOperatingUnitConfig.TryGetValue(ecommerceContext.OperatingUnitId, out operatingUnitConfig))
                {
                    lock (syncRoot)
                    {
                        if (!cachedOperatingUnitConfig.TryGetValue(ecommerceContext.OperatingUnitId, out operatingUnitConfig))
                        {
                            retailServerUri = new Uri(ConfigurationManager.AppSettings["RetailServerRoot"]);
                            operatingUnitConfig = new OperatingUnitConfiguration()
                            {
                                RetailServerUri = retailServerUri
                            };

                            cachedOperatingUnitConfig.Add(ecommerceContext.OperatingUnitId, operatingUnitConfig);
                        }
                    }
                }
                else
                {
                    retailServerUri = operatingUnitConfig.RetailServerUri;
                }

                ManagerFactory managerFactory = null;
                RetailServerContext context = null;

                if (string.IsNullOrEmpty(ecommerceContext.AuthenticationToken))
                {
                    context = RetailServerContext.Create(retailServerUri, ecommerceContext.OperatingUnitId);
                }
                else
                {
                    if (ecommerceContext.IdentityProviderType == IdentityProviderType.OpenIdConnect)
                    {
                        context = RetailServerContext.Create(retailServerUri, ecommerceContext.OperatingUnitId, ecommerceContext.AuthenticationToken);
                    }
                    else if (ecommerceContext.IdentityProviderType == IdentityProviderType.ACS)
                    {
                        context = RetailServerContext.Create(retailServerUri, ecommerceContext.OperatingUnitId, new AcsToken(ecommerceContext.AuthenticationToken));
                    }
                    else
                    {
                        RetailLogger.Log.OnlineStoreUnsupportedIdentityProviderTypeEncountered(ecommerceContext.IdentityProviderType.ToString());
                        string message = string.Format("The specified identity provider type [{0}] set in the eCommerce context is not supported.", ecommerceContext.IdentityProviderType);
                        var exception = new NotSupportedException(message);
                        throw exception;
                    }
                }

                context.Locale = ecommerceContext.Locale;
                managerFactory = ManagerFactory.Create(context);

                return managerFactory;
            }

            /// <summary>
            /// Gets the channel identifier.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            /// <returns>The channel identifier.</returns>
            public static async Task<long> GetChannelId(EcommerceContext ecommerceContext)
            {
                OperatingUnitConfiguration operatingUnitConfig = null;
                if (cachedOperatingUnitConfig.TryGetValue(ecommerceContext.OperatingUnitId, out operatingUnitConfig))
                {
                    if (operatingUnitConfig.ChannelId != 0)
                    {
                        return operatingUnitConfig.ChannelId;
                    }
                }

                ManagerFactory factory = Utilities.GetManagerFactory(ecommerceContext);
                IOrgUnitManager orgUnitManager = factory.GetManager<IOrgUnitManager>();
                ChannelConfiguration channelConfiguration = await orgUnitManager.GetOrgUnitConfiguration();

                // Update cache
                Utilities.cachedOperatingUnitConfig[ecommerceContext.OperatingUnitId].ChannelId = channelConfiguration.RecordId;

                return channelConfiguration.RecordId;
            }

            /// <summary>
            /// Gets the channel currency string template.
            /// </summary>
            /// <param name="ecommerceContext">The eCommerce context.</param>
            /// <returns>The template for channel currency string.</returns>
            public static async Task<string> GetChannelCurrencyStringTemplate(EcommerceContext ecommerceContext)
            {
                OperatingUnitConfiguration operatingUnitConfig = null;
                if (cachedOperatingUnitConfig.TryGetValue(ecommerceContext.OperatingUnitId, out operatingUnitConfig))
                {
                    if (!string.IsNullOrEmpty(operatingUnitConfig.CurrencyStringTemplate))
                    {
                        return operatingUnitConfig.CurrencyStringTemplate;
                    }
                }

                ManagerFactory factory = Utilities.GetManagerFactory(ecommerceContext);
                IOrgUnitManager orgUnitManager = factory.GetManager<IOrgUnitManager>();
                ChannelConfiguration channelConfiguration = await orgUnitManager.GetOrgUnitConfiguration();
                string currencySymbol = Utilities.GetChannelCurrencySymbol(channelConfiguration.Currency);

                CultureInfo cultureInfo = Utilities.GetCultureInfo(ecommerceContext.Locale);
                NumberFormatInfo nfi = cultureInfo.NumberFormat;
                bool symbolToTheRight = (nfi.CurrencyPositivePattern % 2 == 0) ? false : true;

                string currencyTemplate = symbolToTheRight ? "{0}" + currencySymbol : currencySymbol + "{0}";

                // Update cache
                Utilities.cachedOperatingUnitConfig[ecommerceContext.OperatingUnitId].CurrencySymbol = currencySymbol;
                Utilities.cachedOperatingUnitConfig[ecommerceContext.OperatingUnitId].CurrencyStringTemplate = currencyTemplate;

                return currencyTemplate;
            }

            /// <summary>
            /// Gets the identity provider from configuration.
            /// </summary>
            /// <param name="name">The name of identity provider.</param>
            /// <returns>The identity provider client configuration element.</returns>
            public static IdentityProviderClientConfigurationElement GetIdentityProviderFromConfiguration(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                RetailConfigurationSection retailConfiguration = (RetailConfigurationSection)ConfigurationManager.GetSection(Utilities.RetailConfigurationSectionName);
                foreach (IdentityProviderClientConfigurationElement provider in retailConfiguration.IdentityProviders)
                {
                    if (provider.Name == name)
                    {
                        return provider;
                    }
                }

                return null;
            }

            /// <summary>
            /// Updates the cached operating unit configuration. Should be used as a callback for whenever Retail Proxy throws a redirect exception.
            /// </summary>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            /// <param name="redirectedRetailServerUriString">The retail server URI string.</param>
            internal static void UpdateCachedOperatingUnitConfig(string operatingUnitNumber, string redirectedRetailServerUriString)
            {
                lock (syncRoot)
                {
                    Utilities.cachedOperatingUnitConfig[operatingUnitNumber].RetailServerUri = new Uri(redirectedRetailServerUriString);
                }
            }

            internal static int GetMaxPageSize()
            {
                if (Utilities.retailServerMaximumPageSize == null)
                {
                    int maxPageSize = 0;

                    if (!int.TryParse(ConfigurationManager.AppSettings["RetailServerMaxPageSize"], out maxPageSize))
                    {
                        RetailLogger.Log.OnlineStorePropertyNotSetInAppConfig("RetailServerMaxPageSize");
                        var exception = new NotSupportedException("Max page size not found in app config.");
                        throw exception;
                    }

                    Utilities.retailServerMaximumPageSize = maxPageSize;
                }

                return (int)Utilities.retailServerMaximumPageSize;
            }

            internal static bool AreNonCataloguedProductsIncludedByDefault()
            {
                if (Utilities.shouldNonCataloguedProductsBeIncludedByDefault == null)
                {
                    bool shouldNonCataloguedProductsBeIncludedByDefault = false;

                    if (!bool.TryParse(ConfigurationManager.AppSettings["IncludeNonCataloguedProductsByDefault"], out shouldNonCataloguedProductsBeIncludedByDefault))
                    {
                        RetailLogger.Log.OnlineStorePropertyNotSetInAppConfig("IncludeNonCataloguedProductsByDefault");
                        var exception = new NotSupportedException("Setting for configruing inclusion of non catalogued products is not set in app config.");
                        throw exception;
                    }

                    Utilities.shouldNonCataloguedProductsBeIncludedByDefault = shouldNonCataloguedProductsBeIncludedByDefault;
                }

                return (bool)Utilities.shouldNonCataloguedProductsBeIncludedByDefault;
            }

            /// <summary>
            /// Gets the currency configuration.
            /// </summary>
            /// <param name="currencyCode">The currency code.</param>
            /// <returns>A Tuple that consists of the currency symbol as string and a boolean
            /// that defines whether the symbol should be displayed as prefix or suffix.</returns>
            private static string GetChannelCurrencySymbol(string currencyCode)
            {
                Dictionary<string, string> currencySymbolsByCode;

                currencySymbolsByCode = new Dictionary<string, string>();

                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                List<CultureInfo> culturesList = new List<CultureInfo>();
                culturesList.AddRange(cultures);

                var regions = culturesList.Select(x => new RegionInfo(x.LCID));

                foreach (var region in regions)
                {
                    if (!currencySymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
                    {
                        currencySymbolsByCode.Add(region.ISOCurrencySymbol, region.CurrencySymbol);
                    }
                }

                string currencySymbol = string.Empty;

                if (!currencySymbolsByCode.TryGetValue(currencyCode, out currencySymbol))
                {
                    var message = string.Format("No currency symbol could be found for currency code {0}.", currencyCode);
                    var exception = new NotSupportedException(message);
                    RetailLogger.Log.OnlineStoreCurrencySymbolForCurrencyCodeNotFound(currencyCode);
                    throw exception;
                }

                return currencySymbol;
            }
        }
    }
}
