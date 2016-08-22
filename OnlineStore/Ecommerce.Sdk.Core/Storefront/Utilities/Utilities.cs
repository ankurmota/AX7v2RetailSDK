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
        using System.Globalization;
        using System.Text.RegularExpressions;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Utilities class for Ecommerce Core.
        /// </summary>
        public static partial class Utilities
        {
            /// <summary>
            /// Formats decimal value to represent currency.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="ecommerceContext">The eCommerce context.</param>
            /// <returns>Amount as a currency string.</returns>
            public static async Task<string> ToCurrencyString(this decimal amount, EcommerceContext ecommerceContext)
            {
                string currencyStringTemplate = await Utilities.GetChannelCurrencyStringTemplate(ecommerceContext);
                CultureInfo cultureInfo = Utilities.GetCultureInfo(ecommerceContext.Locale);

                string amountWithoutSymbol = amount.ToString("N2", cultureInfo);

                string returnedAmount = string.Format(currencyStringTemplate, amountWithoutSymbol);

                return returnedAmount;
            }

            /// <summary>
            /// Sanitizes the search text.
            /// </summary>
            /// <param name="searchText">The search text.</param>
            /// <returns>Search text that has been stripped of unwanted characters.</returns>
            public static string GetSanitizedSearchText(string searchText)
            {
                string sanitizedString = Regex.Replace(searchText, @"[^0-9a-zA-Z\s]+", "''");
                return sanitizedString.Trim();
            }

            /// <summary>
            /// Gets the culture info object for the given culture name.
            /// </summary>
            /// <param name="cultureName">Name of the culture.</param>
            /// <returns>The culture info object.</returns>
            public static CultureInfo GetCultureInfo(string cultureName)
            {
                CultureInfo cultureInfo = null;
                if (!string.IsNullOrWhiteSpace(cultureName))
                {
                    try
                    {
                        cultureInfo = new CultureInfo(cultureName);
                    }
                    catch (CultureNotFoundException ex)
                    {
                        RetailLogger.Log.OnlineStoreCultureIdProvidedIsNotSupported(cultureName, ex);
                        cultureInfo = CultureInfo.InvariantCulture;
                    }
                }
                else
                {
                    cultureInfo = CultureInfo.InvariantCulture;
                }

                return cultureInfo;
            } 
        }
    }
}