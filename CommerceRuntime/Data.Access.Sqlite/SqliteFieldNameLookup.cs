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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using SQLitePCL;

        /// <summary>
        /// A helper class for retrieval of field indexes by their names for SQLite.
        /// </summary>
        /// <remarks>
        /// Support for access to fields by names implemented in upcoming version of SQLite PCL.
        /// This class is intended to mimic the behavior of SQLite PCL library.
        /// <a href="https://sqlitepcl.codeplex.com/SourceControl/changeset/3c8d99daa1ebc5d6345adc91affea34f507d2608" />.
        /// </remarks>
        internal sealed class SqliteFieldNameLookup
        {
            // (commit 3c8d99daa1ebc5d6345adc91affea34f507d2608 06/10/2014).
            private readonly IDictionary<string, int> fieldIndices;

            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteFieldNameLookup"/> class.
            /// </summary>
            /// <param name="statement">The SQLite statement.</param>
            public SqliteFieldNameLookup(ISQLiteStatement statement)
            {
                ThrowIf.Null(statement, "statement");

                int fieldCount = statement.ColumnCount;

                this.fieldIndices = new Dictionary<string, int>(fieldCount);

                for (int i = 0; i < fieldCount; i++)
                {
                    string fieldName = statement.ColumnName(i);

                    if (!string.IsNullOrEmpty(fieldName) && !this.fieldIndices.ContainsKey(fieldName))
                    {
                        this.fieldIndices.Add(fieldName, i);
                    }
                }
            }

            /// <summary>
            /// Gets the index of the field by a specific name.
            /// </summary>
            /// <param name="fieldName">The name of the field.</param>
            /// <returns>The index of the field.</returns>
            public int GetOrdinal(string fieldName)
            {
                ThrowIf.Null(fieldName, "fieldName");

                int fieldIndex;

                if (this.fieldIndices.TryGetValue(fieldName, out fieldIndex))
                {
                    return fieldIndex;
                }
                else
                {
                    // Throwing the same exception as SQLite PCL in upcoming version
                    throw new SQLiteException("Unable to find column with the specified name: " + fieldName);
                }
            }
        }
    }
}
