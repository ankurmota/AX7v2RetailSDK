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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        /// <summary>
        /// Formatter class for free text search queries.
        /// </summary>
        internal class FreeTextSearchFormatter
        {
            private const string FuzzySearchTextFormat = "\"{0}*\"";
            private readonly string searchText;

            /// <summary>
            /// Initializes a new instance of the <see cref="FreeTextSearchFormatter"/> class.
            /// </summary>
            /// <param name="searchText">The search text being used.</param>
            public FreeTextSearchFormatter(string searchText)
            {
                this.searchText = searchText ?? string.Empty;
            }

            /// <summary>
            /// Gets or sets a value indicating whether fuzzy search is to be used.
            /// </summary>
            public bool UseFuzzySearch
            {
                get;
                set;
            }

            /// <summary>
            /// Formats <paramref name="searchText"/> for fuzzy search.
            /// </summary>
            /// <param name="searchText">The search text being used.</param>
            /// <returns>The search text escaped and formatted.</returns>
            public static string FormatFuzzySearch(string searchText)
            {
                return new FreeTextSearchFormatter(searchText)
                {
                    UseFuzzySearch = true
                }.GetFormattedSearchText();
            }

            /// <summary>
            /// Gets the search text escaped and formatted.
            /// </summary>
            /// <returns>The search text escaped and formatted.</returns>
            public string GetFormattedSearchText()
            {
                string formattedSearchText = this.searchText.Replace("\"", "\"\"");

                if (this.UseFuzzySearch)
                {
                    formattedSearchText = string.Format(FuzzySearchTextFormat, formattedSearchText);
                }

                return formattedSearchText;
            }
        }
    }
}