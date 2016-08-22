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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the implementation of the service to save transaction for certain operations.
        /// </summary>
        public class TransactionLogService : IRequestHandler
        {
            /// <summary>
            /// The transaction id of the transaction that will not be saved into the channel database.
            /// </summary>
            private const string SkippedTransactionId = "-1";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(SaveTransactionLogServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(SaveTransactionLogServiceRequest))
                {
                    response = this.SaveTransactionLog((SaveTransactionLogServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static void SaveTransaction(RequestContext context, TransactionType transactionType, string transactionId)
            {
                string terminalId;
                string storeId;
    
                if (context.GetPrincipal() != null && context.GetTerminal() != null)
                {
                    terminalId = context.GetTerminal().TerminalId;
                    var getStoreDataServiceRequest = new SearchOrgUnitDataRequest(context.GetPrincipal().ChannelId);
                    OrgUnit store = context.Runtime.Execute<EntityDataServiceResponse<OrgUnit>>(getStoreDataServiceRequest, context).PagedEntityCollection.SingleOrDefault();
    
                    storeId = store.OrgUnitNumber;
                }
                else
                {
                    return;
                }
    
                TransactionLog transaction = new TransactionLog()
                {
                    TransactionType = transactionType,
                    Id = transactionId,
                    StaffId = context.GetPrincipal().UserId,
                    TerminalId = terminalId,
                    StoreId = storeId
                };
    
                TransactionLogService.LogTransaction(context, transaction);
            }
    
            /// <summary>
            /// Logs the transaction.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="log">The log.</param>
            private static void LogTransaction(RequestContext context, TransactionLog log)
            {
                SaveTransactionLogDataRequest request = new SaveTransactionLogDataRequest(log);
                context.Runtime.Execute<NullResponse>(request, context);
            }
    
            /// <summary>
            /// Saves the transaction to channel database.
            /// </summary>
            /// <param name="request">The request to save transaction log.</param>
            /// <returns>The empty response.</returns>
            private NullResponse SaveTransactionLog(SaveTransactionLogServiceRequest request)
            {
                switch (request.TransactionType)
                {
                    case TransactionType.LogOn:
                    case TransactionType.LogOff:
                        TransactionLogService.SaveTransaction(request.RequestContext, request.TransactionType, request.TransactionId);
                        break;
    
                    case TransactionType.PrintX:
                    case TransactionType.PrintZ:
                        TransactionLogService.SaveTransaction(request.RequestContext, request.TransactionType, request.TransactionId);
                        break;
    
                    default:
                        throw new NotSupportedException(
                            string.Format("The transaction type {0} is not supported in TransactionLogService.", request.TransactionType.ToString()));
                }
    
                return new NullResponse();
            }
        }
    }
}
