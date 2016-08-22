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
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Data types defined in SQLite databases. This is consistent with types defined in <c>Microsoft.SqlServer.Management.Smo.SqlDataType</c>.
        /// </summary>
        internal enum SqlType
        {
            None = 0,
            BigInt,
            Binary,
            Bit,
            Char,
            Date,
            DateTime,
            DateTime2,
            DateTimeOffset,
            Decimal,
            Float,
            Geography,
            Geometry,
            HierarchyId,
            Image,
            Int,
            Money,
            NChar,
            NText,
            Numeric,
            NVarChar,
            NVarCharMax,
            Real,
            SmallDateTime,
            SmallInt,
            SmallMoney,
            SysName,
            Text,
            Time,
            Timestamp,
            TinyInt,
            UniqueIdentifier,
            UserDefinedDataType,
            UserDefinedTableType,
            UserDefinedType,
            VarBinary,
            VarBinaryMax,
            VarChar,
            VarCharMax,
            Variant,
            Xml
        }
    }
}
