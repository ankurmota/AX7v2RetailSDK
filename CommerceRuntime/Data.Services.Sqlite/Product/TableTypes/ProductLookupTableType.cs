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
    namespace Commerce.Runtime.DataServices.Sqlite.Product
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
    
        /// <summary>
        /// Represents a table type for product look up.
        /// </summary>
        internal sealed class ProductLookupTableType : TableType
        {
            /// <summary>
            /// Represents the product identifier.
            /// </summary>
            internal const string ProductIdColumnName = "PRODUCTID";
    
            /// <summary>
            /// Represents the variant product identifier. This column should evaluate to 0 if the record is a master or standalone.
            /// </summary>
            internal const string VariantIdColumnName = "VARIANTID";
    
            /// <summary>
            /// Represents the lookup product identifier. This column should evaluate to PRODUCTID is the product is a master or standalone and to PRODUCTMASTERID if the product is a variant.
            /// </summary>
            internal const string LookupIdColumnName = "LOOKUPID";
    
            /// <summary>
            /// Represents whether the product is a master. This column should evaluate to TRUE if the product is a master product and to FALSE otherwise.
            /// </summary>
            internal const string IsMasterColumnName = "ISMASTER";
    
            /// <summary>
            /// Represents the item identifier value for the product.
            /// </summary>
            internal const string ItemIdColumnName = "ITEMID";
    
            public ProductLookupTableType(string tableName)
                : base(tableName)
            {
                this.CreateTableSchema();
            }
    
            protected override void CreateTableSchema()
            {
                this.DataTable.Columns.Add(ProductIdColumnName, typeof(long));
                this.DataTable.Columns.Add(VariantIdColumnName, typeof(long));
                this.DataTable.Columns.Add(LookupIdColumnName, typeof(long));
                this.DataTable.Columns.Add(IsMasterColumnName, typeof(bool));
                this.DataTable.Columns.Add(ItemIdColumnName, typeof(string));
            }
        }
    }
}
