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
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;

        /// <summary>
        /// Provides a simple way to interpret a connection string.
        /// </summary>
        public sealed class SqliteConnectionStringBuilder
        {
            private const string MainDatabasePathKey = "DATA SOURCE";
            private const string AttachDatabaseKey = "ATTACH";

            private const char ValuePairSeparator = '=';
            private const string ValuePairEntrySeparator = ";";
            private static readonly string[] ValuePairEntrySeparatorList = new string[] { ValuePairEntrySeparator };

            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteConnectionStringBuilder"/> class.
            /// </summary>
            /// <param name="connectionString">The connection string.</param>
            public SqliteConnectionStringBuilder(string connectionString)
            {
                ThrowIf.NullOrWhiteSpace(connectionString, "connectionString");
                this.Initialize(connectionString);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteConnectionStringBuilder"/> class.
            /// </summary>
            /// <param name="mainDatabase">The main database name.</param>
            /// <param name="attachDatabases">The collection of databases to be attached to the connection.</param>
            public SqliteConnectionStringBuilder(string mainDatabase, IEnumerable<string> attachDatabases)
            {
                ThrowIf.NullOrWhiteSpace(mainDatabase, "connectionString");
                ThrowIf.Null(attachDatabases, "attachDatabases");

                StringBuilder builder = new StringBuilder();

                // attach = main database
                builder.AppendFormat("{0} {1} {2}{3}", MainDatabasePathKey, ValuePairSeparator, mainDatabase, ValuePairEntrySeparator);

                foreach (var attachDatabase in attachDatabases)
                {
                    builder.AppendFormat("{0} {1} {2}{3}", AttachDatabaseKey, ValuePairSeparator, attachDatabase, ValuePairEntrySeparator);
                }

                this.Initialize(builder.ToString());
            }

            /// <summary>
            /// Gets the connection string.
            /// </summary>
            public string ConnectionString
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the database path.
            /// </summary>
            public string MainDatabase
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the collection of attached databases' path.
            /// </summary>
            public IEnumerable<string> AttachedDatabases
            {
                get;
                private set;
            }

            /// <summary>
            /// Throws a <see cref="DatabaseException"/> for a invalid connection string.
            /// </summary>
            /// <param name="errorMessage">The error message.</param>
            /// <param name="arguments">Optional arguments to format the error message.</param>
            private static void ThrowInvalidConnectionString(string errorMessage, params string[] arguments)
            {
                throw new DatabaseException(string.Format("Malformed connection string. {0}", string.Format(errorMessage, arguments)));
            }

            /// <summary>
            /// Initializes this connection string object.
            /// </summary>
            /// <param name="connectionString">The connection string.</param>
            private void Initialize(string connectionString)
            {
                this.ConnectionString = connectionString;
                var attachedDatabases = new List<string>();
                this.AttachedDatabases = attachedDatabases;

                string[] valuePairs = connectionString.Split(ValuePairEntrySeparatorList, StringSplitOptions.RemoveEmptyEntries);

                if (valuePairs.Length == 0)
                {
                    ThrowInvalidConnectionString("Connection string cannot be empty.");
                }

                foreach (string valuePairString in valuePairs)
                {
                    string[] valuePair = valuePairString.Split(ValuePairSeparator);

                    if (valuePair.Length != 2)
                    {
                        ThrowInvalidConnectionString("Pair or value missing.");
                    }

                    string key = valuePair[0].Trim().ToUpperInvariant();
                    string value = valuePair[1].Trim();

                    switch (key)
                    {
                        case MainDatabasePathKey:
                            this.MainDatabase = value;
                            break;

                        case AttachDatabaseKey:
                            attachedDatabases.Add(value);
                            break;

                        default:
                            throw new NotSupportedException(string.Format("Connection string key '{0}' is not supported.", key));
                    }
                }

                if (string.IsNullOrWhiteSpace(this.MainDatabase))
                {
                    ThrowInvalidConnectionString("Value for '{0}' is missing.", MainDatabasePathKey);
                }
            }
        }
    }
}
