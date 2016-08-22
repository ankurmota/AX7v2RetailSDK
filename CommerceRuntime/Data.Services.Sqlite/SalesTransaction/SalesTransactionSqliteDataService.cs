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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using DataServices.SalesTransaction;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The data request handler for sales transaction in SQLite.
        /// </summary>
        public sealed class SalesTransactionSqliteDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(SaveCartDataRequest),
                        typeof(DeleteCartDataRequest),
                        typeof(InsertSalesTransactionTablesDataRequest),
                        typeof(GetSalesTransactionDataRequest),
                        typeof(GetDiscountLinesDataRequest),
                        typeof(GetLoyaltyRewardPointLinesDataRequest),
                        typeof(GetSalesLinesDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Gets the sales transaction to be saved.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
    
                if (request is SaveCartDataRequest)
                {
                    response = SaveSalesTransaction((SaveCartDataRequest)request);
                }
                else if (request is InsertSalesTransactionTablesDataRequest)
                {
                    response = InsertSalesTransactionTables((InsertSalesTransactionTablesDataRequest)request);
                }
                else if (request is GetSalesTransactionDataRequest)
                {
                    response = GetSalesTransaction((GetSalesTransactionDataRequest)request);
                }
                else if (request is DeleteCartDataRequest)
                {
                    response = DeleteCart((DeleteCartDataRequest)request);
                }
                else if (request is GetDiscountLinesDataRequest)
                {
                    response = GetDiscountLines((GetDiscountLinesDataRequest)request);
                }
                else if (request is GetLoyaltyRewardPointLinesDataRequest)
                {
                    response = GetLoyaltyRewardPointLines((GetLoyaltyRewardPointLinesDataRequest)request);
                }
                else if (request is GetSalesLinesDataRequest)
                {
                    response = GetSalesLines((GetSalesLinesDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static EntityDataServiceResponse<SalesOrder> GetSalesTransaction(GetSalesTransactionDataRequest request)
            {
                GetSalesTransactionsProcedure getSalesTransactionsProcedure = new GetSalesTransactionsProcedure(request);
                return getSalesTransactionsProcedure.Execute();
            }
    
            private static NullResponse DeleteCart(DeleteCartDataRequest request)
            {
                var deleteCartProcedure = new DeleteCartProcedure(request);
                deleteCartProcedure.Execute();
                return new NullResponse();
            }
    
            private static NullResponse SaveSalesTransaction(SaveCartDataRequest request)
            {
                var saveSalesTransactionProcedure = new SaveSalesTransactionProcedure(request);
                saveSalesTransactionProcedure.Execute();
    
                return new NullResponse();
            }
    
            private static NullResponse InsertSalesTransactionTables(InsertSalesTransactionTablesDataRequest request)
            {
                var procedure = new InsertSalesTransactionTablesProcedure(request);
                procedure.Execute();
    
                return new NullResponse();
            }
    
            private static EntityDataServiceResponse<DiscountLine> GetDiscountLines(GetDiscountLinesDataRequest request)
            {
                using (SqliteDatabaseContext databaseContext = new SqliteDatabaseContext(request.RequestContext))
                {
                    GetDiscountLinesProcedure getDiscountLinesProcedure = new GetDiscountLinesProcedure(request, databaseContext);
                    return getDiscountLinesProcedure.Execute();
                }
            }
    
            private static EntityDataServiceResponse<LoyaltyRewardPointLine> GetLoyaltyRewardPointLines(GetLoyaltyRewardPointLinesDataRequest request)
            {
                using (SqliteDatabaseContext databaseContext = new SqliteDatabaseContext(request.RequestContext))
                {
                    GetLoyaltyRewardPointLinesProcedure getLoyaltyRewardPointLinesProcedure = new GetLoyaltyRewardPointLinesProcedure(request, databaseContext);
                    return getLoyaltyRewardPointLinesProcedure.Execute();
                }
            }
    
            private static EntityDataServiceResponse<SalesLine> GetSalesLines(GetSalesLinesDataRequest request)
            {
                using (SqliteDatabaseContext databaseContext = new SqliteDatabaseContext(request.RequestContext))
                {
                    GetSalesLinesProcedure getSalesLinesProcedure = new GetSalesLinesProcedure(request, databaseContext);
                    return getSalesLinesProcedure.Execute();
                }
            }
        }
    }
}
