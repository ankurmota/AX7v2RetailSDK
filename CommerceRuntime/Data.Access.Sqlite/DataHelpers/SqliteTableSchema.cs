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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// SQLite table schema.
        /// </summary>
        public class SqliteTableSchema
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteTableSchema" /> class.
            /// </summary>
            /// <param name="tableName">The SQLite table name.</param>
            /// <param name="columnsByColumnName">Column dictionary. Key is column name, value is table column.</param>
            public SqliteTableSchema(string tableName, IDictionary<string, SqliteTableColumn> columnsByColumnName)
            {
                this.Name = tableName;
                this.ColumnsByColumnName = columnsByColumnName;
            }

            /// <summary>
            /// Gets table name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets column dictionary. Key is column name, value is table column.
            /// </summary>
            public IDictionary<string, SqliteTableColumn> ColumnsByColumnName { get; private set; }
        }
    }
}
