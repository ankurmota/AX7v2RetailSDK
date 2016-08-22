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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        ///  The data service that contains operations related to kit transactions.
        /// </summary>
        public class KitTransactionSqlServerDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return new[] { typeof(SaveKitTransactionDataRequest) }; }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>
            /// The outgoing response message.
            /// </returns>
            /// <exception cref="System.NotSupportedException">The request type is not supported.</exception>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(SaveKitTransactionDataRequest))
                {
                    response = Save((SaveKitTransactionDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Saves the kit transaction.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The empty response.</returns>
            private static NullResponse Save(SaveKitTransactionDataRequest request)
            {
                KitTransactionDataManager dataManager = new KitTransactionDataManager(request.RequestContext);
                dataManager.Save(request.KitTransaction);
    
                return new NullResponse();
            }
        }
    }
}
