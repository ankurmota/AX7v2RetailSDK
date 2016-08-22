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
    namespace Commerce.Runtime.DataAccess.SqlServer
    {
        using System;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Data.Interfaces;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Represents a SQL server database query builder that can build final SQL syntax from the <see cref="DatabaseQueryParts"/>.
        /// </summary>
        internal sealed class SqlServerDatabaseQueryBuilder : AnsiDatabaseQueryBuilder
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
                if (parts.Skip.HasValue && parts.Skip != 0 && string.IsNullOrWhiteSpace(parts.OrderBy))
                {
                    var errorMessage = string.Format(
                        "Predefined sort order is mandatory when skip is defined for {0}.{1}", 
                        parts.Schema ?? string.Empty,
                        parts.FromSource);
                    RetailLogger.Log.CrtDataAccessSqlServerError("SqlServerDatabaseQueryBuilder::BuildCore", errorMessage);
                    throw new DatabaseException(errorMessage);
                }
    
                // Use "TOP" sql-part only if nothing needed to skip, otherwise top goes to fetch part of the "ORDER BY" statement.
                if (parts.Top.HasValue && (!parts.Skip.HasValue || parts.Skip == 0))
                {
                    builder.AppendFormat(builderCulture, "TOP ({0}) ", parts.Top);
                }
    
                this.BuildColumnsPart(builder, parts);
                this.BuildFromJoinPart(builder, parts);
                this.BuildWherePart(builder, parts);
                this.BuildOrderByPart(builder, parts.OrderBy);
                if (!string.IsNullOrWhiteSpace(parts.OrderBy))
                {
                    if (parts.Skip.HasValue && parts.Skip.Value != 0)
                    {
                        builder.AppendFormat(builderCulture, "OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY ", parts.Skip, parts.Top);
                    }
                }

                if (!parts.Hints.IsNullOrEmpty())
                {
                    builder.AppendFormat(builderCulture, "OPTION ({0}) ", string.Join(", ", parts.Hints));
                }
            }
        }
    }
}
