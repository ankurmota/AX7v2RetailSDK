/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Stock count demo mode transaction service.
        /// </summary>
        public class StockCountRealtimeServiceDemoMode : IRequestHandler
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
                        typeof(GetStockCountJournalsRealtimeRequest),
                        typeof(GetStockCountJournalTransactionsRealtimeRequest),
                        typeof(CommitStockCountJournalRealtimeRequest),
                        typeof(CreateStockCountJournalRealtimeRequest),
                        typeof(DeleteStockCountJournalRealtimeRequest)
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
                if (requestType == typeof(CreateStockCountJournalRealtimeRequest))
                {
                    response = CreateStockCountJournal();
                }
                else if (requestType == typeof(DeleteStockCountJournalRealtimeRequest))
                {
                    response = DeleteStockCountJournal();
                }
                else if (requestType == typeof(GetStockCountJournalsRealtimeRequest))
                {
                    response = GetStockCountJournals();
                }
                else if (requestType == typeof(GetStockCountJournalTransactionsRealtimeRequest))
                {
                    response = GetStockCountJournalTransactions();
                }
                else if (requestType == typeof(CommitStockCountJournalRealtimeRequest))
                {
                    response = CommitStockCountJournal();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets stock count journals in AX by location id.
            /// </summary>
            /// <returns>The collection of <see cref="StockCountJournal"/> items.</returns>
            private static EntityDataServiceResponse<StockCountJournal> GetStockCountJournals()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetStockCountJournals is not supported in demo mode.");
            }
    
            /// <summary>
            /// Commits stock count journals in AX.
            /// </summary>
            /// <returns>The <see cref="StockCountJournal"/> journal.</returns>
            private static SingleEntityDataServiceResponse<StockCountJournal> CommitStockCountJournal()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "CommitStockCountJournal is not supported in demo mode.");
            }
    
            /// <summary>
            /// Deletes stock count journal in AX.
            /// </summary>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse DeleteStockCountJournal()
            {
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets stock count journals with transactions from AX by location and journal ids.
            /// </summary>
            /// <returns>The collection of <see cref="StockCountJournalTransaction"/> items.</returns>
            private static EntityDataServiceResponse<StockCountJournalTransaction> GetStockCountJournalTransactions()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetStockCountJournalTransactions is not supported in demo mode.");
            }
    
            /// <summary>
            /// Creates stock count journal in AX by location id and description.
            /// </summary>
            /// <returns>The collection of <see cref="StockCountJournal"/> items.</returns>
            private static EntityDataServiceResponse<StockCountJournal> CreateStockCountJournal()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "CreateStockCountJournal is not supported in demo mode.");
            }
        }
    }
}
