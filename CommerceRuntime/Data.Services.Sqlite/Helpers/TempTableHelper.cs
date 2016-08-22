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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System.Collections.Generic;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
    
        /// <summary>
        /// Helper class to handle temporary table operations.
        /// </summary>
        internal static class TempTableHelper
        {
            /// <summary>
            /// Creates a temporary table that represents a collection of identifiers.
            /// </summary>
            /// <typeparam name="T">The type of the temp table field/ column.</typeparam>
            /// <param name="context">The database context in which to create the temporary table.</param>
            /// <param name="idColumnName">Identifier column name.</param>
            /// <param name="ids">The collection of identifiers.</param>
            /// <returns>The temporary table created.</returns>
            public static TempTable CreateScalarTempTable<T>(SqliteDatabaseContext context, string idColumnName, IEnumerable<T> ids)
            {
                int tempTableId = context.GetNextContextIdentifier();
                DataTable table = new DataTable(idColumnName + "TempTable" + tempTableId);
    
                table.Columns.Add(idColumnName, typeof(T));
    
                foreach (T id in ids)
                {
                    var row = table.NewRow();
                    row[idColumnName] = id;
                    table.Rows.Add(row);
                }
    
                return context.CreateTemporaryTable(table);
            }
        }
    }
}
