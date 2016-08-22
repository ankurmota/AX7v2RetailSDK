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
        /// Represents the configuration values for SQLite data access.
        /// </summary>
        public sealed class SqliteConfiguration
        {
            private const string ConnectionPoolSizeName = "connectionPoolSize";
            private const string StatementCachingPoolSizeName = "statementCachingPoolSize";
            private const string BusyTimeoutName = "busyTimeout";

            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteConfiguration"/> class.
            /// </summary>
            /// <remarks>This uses the default configuration values.</remarks>
            public SqliteConfiguration()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteConfiguration"/> class.
            /// </summary>
            /// <param name="configurationValues">The configuration values for the SQLite data access.</param>
            public SqliteConfiguration(IDictionary<string, string> configurationValues)
            {
                ThrowIf.Null(configurationValues, "configurationValues");

                this.ConnectionPoolSize = GetIntegerFromConfigurationValues(configurationValues, ConnectionPoolSizeName);
                this.StatementCachingPoolSize = GetIntegerFromConfigurationValues(configurationValues, StatementCachingPoolSizeName);
            }

            /// <summary>
            /// Gets or sets the limit size of the connection pool.
            /// </summary>
            /// <remarks>The connection pool allows connections to be reused.</remarks>
            /// <value>This limits the number of concurrent connections opened against the database.</value>
            public int ConnectionPoolSize { get; set; }

            /// <summary>
            /// Gets or sets the limit size, per database connection, of the statement caching pool.
            /// </summary>
            /// <remarks>The statement cache allows statements to be reused, reducing the statement compilation overhead during runtime.</remarks>
            /// <value>This limits the size of the statement caching pool.</value>
            public int StatementCachingPoolSize { get; set; }

            /// <summary>
            /// Gets or sets the timeout in milliseconds for waiting on a statement execution that needs to acquire a lock on a database element.
            /// </summary>
            /// <value>Busy timeout in milliseconds.</value>
            public int BusyTimeout { get; set; }

            /// <summary>
            /// Checks that the configuration values are valid.
            /// </summary>
            public void Validate()
            {
                if (this.ConnectionPoolSize < 1)
                {
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidConfigurationKeyFormat, "Connection pool size must be positive.");
                }

                if (this.StatementCachingPoolSize < 1)
                {
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidConfigurationKeyFormat, "Connection statement caching size must be positive.");
                }

                if (this.BusyTimeout < 0)
                {
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidConfigurationKeyFormat, "Busy timeout must be greater or equal to 0.");
                }
            }

            private static int GetIntegerFromConfigurationValues(IDictionary<string, string> configurationValues, string key)
            {
                int value;
                string stringValue;

                if (configurationValues.TryGetValue(key, out stringValue))
                {
                    if (!int.TryParse(stringValue, out value))
                    {
                        string message = string.Format(
                            "The value '{0}' for the configuration key <dataAccess\\{1}> must be an integer.",
                            stringValue,
                            key);

                        throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidConfigurationKeyFormat, message);
                    }
                }
                else
                {
                    string message = string.Format(
                            "Configuration key <dataAccess\\{0}> not found.",
                            key);
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidConfigurationKeyFormat, message);
                }

                return value;
            }
        }
    }
}
