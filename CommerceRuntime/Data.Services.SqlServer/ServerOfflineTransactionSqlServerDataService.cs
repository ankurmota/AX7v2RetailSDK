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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.IO.Compression;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Server offline transaction data service class.
        /// </summary>
        public class ServerOfflineTransactionSqlServerDataService : IRequestHandler
        {
            private const string OfflineTransactionsParameter = "@offlineTransactions";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(SaveOfflineTransactionsDataRequest) };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            /// <exception cref="System.NotSupportedException">The request type is not supported.</exception>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    if (requestType == typeof(SaveOfflineTransactionsDataRequest))
                    {
                        response = this.SaveOfflineTransactions(databaseContext, ((SaveOfflineTransactionsDataRequest)request).CompressedTransactions);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                    }
                }
    
                return response;
            }
    
            private static string DecompressTransactions(byte[] compressedTransactions)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream decompressionStream = new GZipStream(new MemoryStream(compressedTransactions), CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(memoryStream);
                    }
    
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return new StreamReader(memoryStream).ReadToEnd();
                }
            }
    
            private NullResponse SaveOfflineTransactions(SqlServerDatabaseContext databaseContext, byte[] compressedTransactions)
            {
                string offlineTransactionsInXmlString = DecompressTransactions(compressedTransactions);
    
                ParameterSet parameters = new ParameterSet();
                parameters[OfflineTransactionsParameter] = offlineTransactionsInXmlString;
                int errorCode = databaseContext.ExecuteStoredProcedureNonQuery("UpsertOfflineTransactions", parameters);
    
                if (errorCode != (int)DatabaseErrorCodes.Success)
                {
                    throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save offline transactions.");
                }
    
                return new NullResponse();
            }
        }
    }
}
