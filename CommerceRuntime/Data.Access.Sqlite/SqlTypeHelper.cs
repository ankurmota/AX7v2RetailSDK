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
        using System.Diagnostics.CodeAnalysis;
        using System.Reflection;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using SQLitePCL;

        /// <summary>
        /// Helper class for type conversions.
        /// </summary>
        internal static class SqlTypeHelper
        {
            private const string StringLiteral = "TEXT";
            private const string IntegerLiteral = "INTEGER";
            private const string NumberLiteral = "REAL";
            private const string BlobLiteral = "BLOB";
            private const string DateTimeLiteral = "DATETIME";

            /// <summary>
            /// The minimum number of seconds that a date can be from <see cref="BaseDateTime"/>.
            /// </summary>
            private const long MinSecondsFromBaseDateTime = 0;

            /// <summary>
            /// Represents the base time from which the DateTime's number of seconds will be stored in the database.
            /// </summary>
            private static readonly DateTime BaseDateTime = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Unspecified);

            /// <summary>
            /// The maximum number of seconds that a date can be from <see cref="BaseDateTime"/>.
            /// </summary>
            private static readonly long MaxSecondsFromBaseDateTime = (long)DateTime.MaxValue.Subtract(BaseDateTime).TotalSeconds;

            /// <summary>
            /// Dictionary mapping the conversion functions between managed types and SQLite types.
            /// </summary>
            private static readonly Dictionary<Type, ConvertManagedToSqliteType> ManagedToSqliteTypeConversionDictionary = new Dictionary<Type, ConvertManagedToSqliteType>();

            /// <summary>
            /// Dictionary mapping the managed type to SQLite type literals.
            /// </summary>
            private static readonly Dictionary<Type, string> ManagedToSqliteTypeLiteralsDictionary = new Dictionary<Type, string>();

            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Improved readability.")]
            static SqlTypeHelper()
            {
                // Managed to SQLite 
                ManagedToSqliteTypeConversionDictionary.Add(typeof(DateTime), ConvertDateTimeToSql);
                ManagedToSqliteTypeConversionDictionary.Add(typeof(DateTime?), ConvertDateTimeToSql);
                ManagedToSqliteTypeConversionDictionary.Add(typeof(DateTimeOffset), ConvertDateTimeOffsetToSql);
                ManagedToSqliteTypeConversionDictionary.Add(typeof(DateTimeOffset?), ConvertDateTimeOffsetToSql);
                ManagedToSqliteTypeConversionDictionary.Add(typeof(decimal), ConvertDecimalToSql);
                ManagedToSqliteTypeConversionDictionary.Add(typeof(bool), ConvertIntegersToSql);
                ManagedToSqliteTypeConversionDictionary.Add(typeof(short), ConvertIntegersToSql);

                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(string), StringLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(char), StringLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(int), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(long), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(byte), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(sbyte), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(short), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(ushort), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(uint), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(ulong), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(bool), IntegerLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(float), NumberLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(double), NumberLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(decimal), NumberLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(DateTime), DateTimeLiteral);
                ManagedToSqliteTypeLiteralsDictionary.Add(typeof(DateTimeOffset), DateTimeLiteral);
            }

            /// <summary>
            /// Delegates to convert the managed type into a SQLite friendly.
            /// </summary>
            /// <param name="managedValue">The managed value.</param>
            /// <returns>The SQLite converted value.</returns>
            private delegate object ConvertManagedToSqliteType(object managedValue);

            /// <summary>
            /// Maps a SQLite exception to <see cref="DatabaseException"/> and throws it.
            /// </summary>
            /// <param name="sqlException">The SQL exception.</param>
            public static void ThrowDatabaseException(SQLiteException sqlException)
            {
                throw new DatabaseException(DatabaseErrorCode.Unknown, sqlException.Message, sqlException);
            }

            /// <summary>
            /// Converts a managed type into a SQLite type.
            /// </summary>
            /// <param name="managedValue">The managed value.</param>
            /// <returns>The SQLite converted value.</returns>
            public static object ConvertManagedValueToSqlValue(object managedValue)
            {
                if (managedValue != null)
                {
                    Type type = managedValue.GetType();
                    ConvertManagedToSqliteType convertionFunction;
                    if (ManagedToSqliteTypeConversionDictionary.TryGetValue(type, out convertionFunction))
                    {
                        return convertionFunction(managedValue);
                    }
                    else if (managedValue is Enum)
                    {
                        // Enum special case handling
                        return ConvertIntegersToSql(managedValue);
                    }
                }

                // if no specific convertion is provided, just uses same value type
                return managedValue;
            }

            /// <summary>
            /// Converts a SQL value into managed value.
            /// </summary>
            /// <param name="sqlValue">The SQL value.</param>
            /// <param name="managedType">The desired managed value.</param>
            /// <returns>The converted value into the managed type.</returns>
            public static object ConvertSqlValueToManagedValue(object sqlValue, Type managedType)
            {
                if ((sqlValue != null) && (managedType != typeof(object)))
                {
                    if (managedType == typeof(DateTime))
                    {
                        return ConvertSqlDateTimeToManaged(sqlValue);
                    }
                    else
                    {
                        // Enums need special case handling
                        TypeInfo typeInfo = managedType.GetTypeInfo();

                        if (typeInfo.IsEnum)
                        {
                            return Convert.ChangeType(sqlValue, Enum.GetUnderlyingType(managedType));
                        }
                        else if (typeInfo.IsGenericType)
                        {
                            if (typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                Type underlyingType = Nullable.GetUnderlyingType(managedType);
                                return ConvertSqlValueToManagedValue(sqlValue, underlyingType);
                            }
                        }
                    }

                    // Default conversion if no specific conversion was defined
                    return Convert.ChangeType(sqlValue, managedType);
                }

                return sqlValue;
            }

            /// <summary>
            /// Gets the SQLite type literal based on a managed type.
            /// </summary>
            /// <param name="type">The managed type.</param>
            /// <returns>The SQLite literal name.</returns>
            public static string GetSqliteTypeNameFromManagedType(Type type)
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }

                string literalType;

                if (!ManagedToSqliteTypeLiteralsDictionary.TryGetValue(type, out literalType))
                {
                    literalType = BlobLiteral;
                }

                return literalType;
            }

            /// <summary>
            /// Converts <see cref="SqlType" /> to managed data type.
            /// </summary>
            /// <param name="sqlType">SQL type.</param>
            /// <returns>Managed data type.</returns>
            public static Type ConvertSqlTypeToManagedType(SqlType sqlType)
            {
                switch (sqlType)
                {
                    case SqlType.BigInt:
                        return typeof(long);

                    case SqlType.Binary:
                    case SqlType.Image:
                    case SqlType.Timestamp:
                    case SqlType.VarBinary:
                    case SqlType.VarBinaryMax:
                        return typeof(byte[]);

                    case SqlType.Bit:
                        return typeof(bool);

                    case SqlType.Char:
                    case SqlType.NChar:
                    case SqlType.NText:
                    case SqlType.NVarChar:
                    case SqlType.NVarCharMax:
                    case SqlType.Text:
                    case SqlType.VarChar:
                    case SqlType.VarCharMax:
                    case SqlType.Xml:
                        return typeof(string);

                    case SqlType.DateTime:
                    case SqlType.SmallDateTime:
                    case SqlType.Date:
                    case SqlType.Time:
                    case SqlType.DateTime2:
                        return typeof(DateTime);

                    case SqlType.Decimal:
                    case SqlType.Numeric:
                    case SqlType.Money:
                    case SqlType.SmallMoney:
                        return typeof(decimal);

                    case SqlType.Float:
                        return typeof(double);

                    case SqlType.Int:
                        return typeof(int);

                    case SqlType.Real:
                        return typeof(float);

                    case SqlType.UniqueIdentifier:
                        return typeof(Guid);

                    case SqlType.SmallInt:
                        return typeof(short);

                    case SqlType.TinyInt:
                        return typeof(byte);

                    case SqlType.DateTimeOffset:
                        return typeof(DateTimeOffset);

                    // For unsupported data types.
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("SqlType {0} is not supported.", sqlType));
                }
            }

            /// <summary>
            /// Converts string representation of a SQL type to the enumeration representation of a SQL type, ignoring case.
            /// </summary>
            /// <param name="stringRepresentationOfSqlType">String representation of a SQL type.</param>
            /// <returns>Enumeration representation of a SQL type.</returns>
            public static SqlType ConvertStringToSqlType(string stringRepresentationOfSqlType)
            {
                SqlType result;
                bool isParsed = Enum.TryParse<SqlType>(stringRepresentationOfSqlType, true, out result);

                if (!isParsed)
                {
                    throw new DatabaseException(string.Format("Cannot parse SQL type {0}.", stringRepresentationOfSqlType));
                }

                return result;
            }

            #region Managed type to SQLite conversions

            /// <summary>
            /// Converts <see cref="DateTime"/> into a SQLite friendly type.
            /// </summary>
            /// <param name="managedValue">The managed value to be converted.</param>
            /// <returns>The SQLite converted value.</returns>
            private static object ConvertDateTimeToSql(object managedValue)
            {
                DateTime? datetimeValue = (DateTime?)managedValue;

                // Take the total number of seconds from datetimeValue to BaseDateTime
                if (datetimeValue.HasValue)
                {
                    return Math.Max((long)datetimeValue.Value.Subtract(BaseDateTime).TotalSeconds, MinSecondsFromBaseDateTime);
                }

                return null;
            }

            /// <summary>
            /// Converts <see cref="DateTimeOffset"/> into a SQLite friendly type.
            /// </summary>
            /// <param name="managedValue">The managed value to be converted.</param>
            /// <returns>The SQLite converted value.</returns>
            private static object ConvertDateTimeOffsetToSql(object managedValue)
            {
                // For date time offset, just persist the date time portion
                // this is same behavior as SQLServer and is needed so CRT can handle
                // the offset in a single manner
                DateTimeOffset? datetimeOffsetValue = (DateTimeOffset?)managedValue;
                return ConvertDateTimeToSql(datetimeOffsetValue.HasValue ? (DateTime?)datetimeOffsetValue.Value.DateTime : null);
            }

            /// <summary>
            /// Converts <see cref="Decimal"/> into a SQLite friendly type.
            /// </summary>
            /// <param name="managedValue">The managed value to be converted.</param>
            /// <returns>The SQLite converted value.</returns>
            private static object ConvertDecimalToSql(object managedValue)
            {
                return Convert.ToDouble(managedValue);
            }

            /// <summary>
            /// Converts <see cref="short"/> and <see cref="int"/> into a SQLite friendly type.
            /// </summary>
            /// <param name="managedValue">The managed value to be converted.</param>
            /// <returns>The SQLite converted value.</returns>
            private static object ConvertIntegersToSql(object managedValue)
            {
                return Convert.ToInt64(managedValue);
            }

            #endregion

            #region SQLite to Managed type conversions

            /// <summary>
            /// Converts a SQLite value into the managed type <see cref="DateTime"/>.
            /// </summary>
            /// <param name="sqlValue">The SQLite value to be converted.</param>
            /// <returns>The value converted to the managed type.</returns>
            private static object ConvertSqlDateTimeToManaged(object sqlValue)
            {
                if (sqlValue == null)
                {
                    return null;
                }

                if (sqlValue is long)
                {
                    long secondsSinceBase = (long)sqlValue;

                    if (secondsSinceBase <= MinSecondsFromBaseDateTime)
                    {
                        return DateTime.MinValue;
                    }
                    else if (secondsSinceBase >= MaxSecondsFromBaseDateTime)
                    {
                        return DateTime.MaxValue;
                    }

                    // This is the number of seconds since BaseDateTime
                    return BaseDateTime.AddSeconds((long)secondsSinceBase);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Cannot convert type {0} into DateTime.", sqlValue.GetType()));
                }
            }

            #endregion
        }
    }
}
