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
    namespace Commerce.Runtime.Data.Sqlite
    {
        using System; 
        using System.Globalization; 
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Data.Interfaces;

        /// <summary>
        /// Represents a SQLite database query builder that can build final SQL syntax from the <see cref="DatabaseQueryParts"/>.
        /// </summary>
        internal sealed class SqliteDatabaseQueryBuilder : AnsiDatabaseQueryBuilder
        {
            /// <summary>
            /// Writes parts to a builder based on a specified culture.
            /// </summary>
            /// <param name="builder">A string builder which aggregates a final SQL query.</param>
            /// <param name="parts">A database query parts.</param>
            /// <param name="builderCulture">A string builder culture.</param>
            protected override void BuildCore(StringBuilder builder, DatabaseQueryParts parts, CultureInfo builderCulture)
            {
                ThrowIf.Null(builder, "builder");
                ThrowIf.Null(parts, "parts");

                this.BuildColumnsPart(builder, parts);
                this.BuildFromJoinPart(builder, parts);
                this.BuildWherePart(builder, parts);
                this.BuildOrderByPart(builder, parts.OrderBy);

                bool hasTop = parts.Top.HasValue;

                if (hasTop)
                {
                    builder.AppendFormat(builderCulture, "LIMIT {0} ", parts.Top);
                }

                if (parts.Skip.HasValue)
                {
                    if (!hasTop)
                    {
                        builder.Append("LIMIT -1 ");
                    }

                    builder.AppendFormat(builderCulture, "OFFSET {0} ", parts.Skip);
                }
            }
        }
    }
}
