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
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// SQLite table column.
        /// </summary>
        public class SqliteTableColumn
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteTableColumn" /> class.
            /// </summary>
            /// <param name="columnName">Column name.</param>
            /// <param name="sqlType">SQL type.</param>
            /// <param name="managedType">Managed type.</param>
            internal SqliteTableColumn(string columnName, SqlType sqlType, Type managedType)
            {
                this.Name = columnName;
                this.SqlType = sqlType;
                this.ManagedType = managedType;
            }

            /// <summary>
            /// Gets column name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets managed type for this column.
            /// </summary>
            public Type ManagedType { get; private set; }

            /// <summary>
            /// Gets SQL type for this column.
            /// </summary>
            internal SqlType SqlType { get; private set; }
        }
    }
}
