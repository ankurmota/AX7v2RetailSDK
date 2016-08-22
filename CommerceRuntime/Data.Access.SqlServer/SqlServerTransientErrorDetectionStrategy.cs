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
        using System.Collections.Generic;
        using System.Data.SqlClient;
        using Microsoft.Dynamics.Commerce.Runtime.Framework;

        /// <summary>
        /// A SQL Server specific error detection strategy for transient errors that can be safely retried.
        /// </summary>
        public sealed class SqlServerTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            private static readonly HashSet<int> TransientErrorCodes = new HashSet<int>
        {
            20, // The instance of SQL Server does not support encryption.
            64, // An error occurred during login.
            233, // Connection initialization error.
            10053, // A transport-level error occurred when receiving results from the server.
            10054, // A transport-level error occurred when sending the request to the server.
            10060, // Network or instance-specific error.
            40143, // Connection could not be initialized.
            40197, // The service encountered an error processing your request.
            40501, // The server is busy.
            40613, // The database is currently unavailable.
        };

            /// <summary>
            /// Determines whether the specified exception represents a transient failure that can be compensated by a retry. 
            /// </summary>
            /// <param name="ex">The exception.</param>
            /// <returns>A value indicating whether the specified exception could be retried.</returns>
            public bool IsTransient(Exception ex)
            {
                SqlException sqlException = ex as SqlException;
                if (sqlException != null)
                {
                    if (TransientErrorCodes.Contains(sqlException.ErrorCode))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}