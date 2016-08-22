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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Framework;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Represents an implementation of the store operation service.
        /// </summary>
        public class StoreOperationService : IRequestHandler
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
                    typeof(SaveNonSaleTenderServiceRequest),
                    typeof(SaveDropAndDeclareServiceRequest),
                    typeof(GetNonSaleTenderServiceRequest),
                    typeof(SearchJournalTransactionsServiceRequest)
                };
                }
            }

            /// <summary>
            /// Executes the specified service request.
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

                if (requestType == typeof(SaveNonSaleTenderServiceRequest))
                {
                    response = SaveNonSaleTenderTransactions((SaveNonSaleTenderServiceRequest)request);
                }
                else if (requestType == typeof(GetNonSaleTenderServiceRequest))
                {
                    response = GetNonSaleTenderTransactions((GetNonSaleTenderServiceRequest)request);
                }
                else if (requestType == typeof(SaveDropAndDeclareServiceRequest))
                {
                    response = SaveDropAndDeclareTransactions((SaveDropAndDeclareServiceRequest)request);
                }
                else if (requestType == typeof(SearchJournalTransactionsServiceRequest))
                {
                    response = SearchJournalTransactions((SearchJournalTransactionsServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Merges two collections of transaction records.
            /// </summary>
            /// <param name="remoteResults">The remote transaction collection.</param>
            /// <param name="localResults">The local transaction collection.</param>
            /// <returns>A merged transaction collection.</returns>
            internal static IEnumerable<Transaction> MergeTransactionRecords(IEnumerable<Transaction> remoteResults, IEnumerable<Transaction> localResults)
            {
                IEnumerable<Transaction> mergedTransactions = MultiDataSourcesPagingHelper.MergeResults(
                        remoteResults,
                        localResults,
                        GetTransactionMergingKey);

                return mergedTransactions;
            }

            /// <summary>
            /// Invoke the method to save non sale tender type transactions.
            /// </summary>
            /// <param name="request">Request for non sale tender transactions.</param>
            /// <returns>Returns the non sale tender transactions.</returns>
            private static SaveNonSaleTenderServiceResponse SaveNonSaleTenderTransactions(SaveNonSaleTenderServiceRequest request)
            {
                NonSalesTransaction nonSalesTransaction = StoreOperationServiceHelper.ConvertToNonSalesTenderTransaction(request.RequestContext, request);

                // If the previously created tender transaction response did not get received due to network connection issue.
                // On client retry, check if it was already saved. If true, returns saved object.
                var getCurrentShiftNonSalesTransactionsdataServiceRequest = new GetCurrentShiftNonSalesTransactionsDataRequest(nonSalesTransaction, request.TransactionId);
                NonSalesTransaction savedNonSalesTransaction = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<NonSalesTransaction>>(getCurrentShiftNonSalesTransactionsdataServiceRequest, request.RequestContext).PagedEntityCollection.FirstOrDefault();

                if (savedNonSalesTransaction == null)
                {
                    var saveNonSalesTransactionsdataServiceRequest = new SaveNonSalesTransactionDataRequest(nonSalesTransaction);
                    savedNonSalesTransaction = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<NonSalesTransaction>>(saveNonSalesTransactionsdataServiceRequest, request.RequestContext).Entity;
                }

                return new SaveNonSaleTenderServiceResponse(savedNonSalesTransaction);
            }

            /// <summary>
            /// Invoke the method to save drop and declare transactions.
            /// </summary>
            /// <param name="request">Request context.</param>
            /// <returns>Returns response for save drop and declare.</returns>
            private static SaveDropAndDeclareServiceResponse SaveDropAndDeclareTransactions(SaveDropAndDeclareServiceRequest request)
            {
                StoreOperationServiceHelper.ValidateTenderDeclarationCountingDifference(request);

                DropAndDeclareTransaction tenderDropAndDeclare = StoreOperationServiceHelper.ConvertTenderDropAndDeclareTransaction(request);

                // If the previously created drop transaction response did not get received due to network connection issue.
                // On client retry, check if it was already saved. If true, returns saved object.
                var getDropAndDeclareTransactionDataRequest = new GetDropAndDeclareTransactionDataRequest(tenderDropAndDeclare.Id, QueryResultSettings.SingleRecord);
                DropAndDeclareTransaction transaction = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<DropAndDeclareTransaction>>(getDropAndDeclareTransactionDataRequest, request.RequestContext).PagedEntityCollection.FirstOrDefault();

                if (transaction != null)
                {
                    var getDropAndDeclareTransactionTenderDetailsDataRequest = new GetDropAndDeclareTransactionTenderDetailsDataRequest(tenderDropAndDeclare.Id, QueryResultSettings.AllRecords);
                    transaction.TenderDetails = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<TenderDetail>>(getDropAndDeclareTransactionTenderDetailsDataRequest, request.RequestContext).PagedEntityCollection.Results;
                }
                else
                {
                    var saveDropAndDeclareTransactionDataRequest = new SaveDropAndDeclareTransactionDataRequest(tenderDropAndDeclare);
                    transaction = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<DropAndDeclareTransaction>>(saveDropAndDeclareTransactionDataRequest, request.RequestContext).Entity;
                }

                return new SaveDropAndDeclareServiceResponse(transaction);
            }

            /// <summary>
            /// Invoke the method to get non sale tender transaction list for the given non sale tender type.
            /// </summary>
            /// <param name="request">Request for non sale tender service.</param>
            /// <returns>Returns the response for non sale tender operation get request.</returns>
            private static GetNonSaleTenderServiceResponse GetNonSaleTenderTransactions(GetNonSaleTenderServiceRequest request)
            {
                NonSalesTransaction tenderTransaction = StoreOperationServiceHelper.ConvertToNonSalesTenderTransaction(request.RequestContext, request.ShiftId, request.ShiftTerminalId, request.TransactionType);
                var getCurrentShiftNonSalesTransactionsdataServiceRequest = new GetCurrentShiftNonSalesTransactionsDataRequest(tenderTransaction, request.TransactionId);

                PagedResult<NonSalesTransaction> nonSaleOperationList = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<NonSalesTransaction>>(getCurrentShiftNonSalesTransactionsdataServiceRequest, request.RequestContext).PagedEntityCollection;

                return new GetNonSaleTenderServiceResponse(nonSaleOperationList);
            }

            /// <summary>
            /// Get transactions using the request criteria.
            /// </summary>
            /// <param name="request">Request containing the criteria used to retrieve transactions.</param>
            /// <returns>SearchJournalTransactionsServiceResponse object.</returns>
            private static SearchJournalTransactionsServiceResponse SearchJournalTransactions(SearchJournalTransactionsServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                NetTracer.Information("TransactionService.SearchJournalTransactions()");

                if (request.QueryResultSettings.Sorting == null)
                {
                    request.QueryResultSettings.Sorting = new SortingInfo();
                }

                if (!request.QueryResultSettings.Sorting.IsSpecified)
                {
                    request.QueryResultSettings.Sorting.Add(new SortColumn(RetailTransactionTableSchema.CreatedDateTimeColumn, isDescending: true));
                    request.QueryResultSettings.Sorting.Add(new SortColumn(RetailTransactionTableSchema.BeginDateTimeColumn, isDescending: true));
                }

                if (request.Criteria.SearchLocationType == SearchLocation.Local)
                {
                    // Search local only.
                    PagedResult<Transaction> results = SearchJournalTransactionsLocally(request.Criteria, request.QueryResultSettings, request.RequestContext);

                    return new SearchJournalTransactionsServiceResponse(results);
                }
                else if (request.Criteria.SearchLocationType == SearchLocation.Remote)
                {
                    // Search remote only.
                    PagedResult<Transaction> results = SearchJournalTransactionsRemotely(request.Criteria, request.QueryResultSettings, request.RequestContext);

                    return new SearchJournalTransactionsServiceResponse(results);
                }
                else
                {
                    // Search all.
                    // Adjust the paging.
                    QueryResultSettings settings = request.QueryResultSettings;
                    PagingInfo adjustedPaging = MultiDataSourcesPagingHelper.GetAdjustedPaging(settings.Paging);
                    QueryResultSettings adjustedQueryResultSettings = new QueryResultSettings(settings.ColumnSet, adjustedPaging, settings.Sorting, settings.ChangeTracking);

                    // Get local results.
                    IEnumerable<Transaction> localTransactions = SearchJournalTransactionsLocally(request.Criteria, adjustedQueryResultSettings, request.RequestContext).Results;

                    // Getremote results.
                    IEnumerable<Transaction> remoteTransactions = Enumerable.Empty<Transaction>();
                    try
                    {
                        remoteTransactions = SearchJournalTransactionsRemotely(request.Criteria, adjustedQueryResultSettings, request.RequestContext).Results;
                    }
                    catch (Exception e)
                    {
                        // Eats the exception since search remote is optional and log the error details as a warning.
                        RetailLogger.Log.CrtServicesStoreOperationServiceServiceRemoteTransactionSearchFailed(e);
                    }

                    // Merge results.
                    IEnumerable<Transaction> mergedTransactions = MultiDataSourcesPagingHelper.MergeResults(
                        remoteTransactions,
                        localTransactions,
                        GetTransactionMergingKey);

                    // Sorts merged transactions.
                    PagedResult<Transaction> mergedResults = MultiDataSourcesPagingHelper.GetPagedResult(mergedTransactions, settings.Paging, settings.Sorting);
                    return new SearchJournalTransactionsServiceResponse(mergedResults);
                }
            }

            private static PagedResult<Transaction> SearchJournalTransactionsLocally(TransactionSearchCriteria criteria, QueryResultSettings settings, RequestContext context)
            {
                var searchTransactionsDataRequest = new SearchJournalTransactionsDataRequest(criteria, settings);
                EntityDataServiceResponse<Transaction> searchJournalTransactions = context.Runtime
                    .Execute<EntityDataServiceResponse<Transaction>>(searchTransactionsDataRequest, context);

                return searchJournalTransactions.PagedEntityCollection;
            }

            private static PagedResult<Transaction> SearchJournalTransactionsRemotely(TransactionSearchCriteria criteria, QueryResultSettings settings, RequestContext context)
            {
                var remoteSearchRequest = new SearchJournalTransactionsRealtimeRequest(criteria, settings);
                SearchJournalTransactionsRealtimeResponse journalTransactionsServiceResponse = context.Execute<SearchJournalTransactionsRealtimeResponse>(remoteSearchRequest);

                return journalTransactionsServiceResponse.Transactions;
            }

            /// <summary>
            /// Gets the key used when merging two transactions.
            /// </summary>
            /// <param name="transaction">The transaction to merge.</param>
            /// <returns>The key.</returns>
            private static string GetTransactionMergingKey(Transaction transaction)
            {
                if (string.IsNullOrWhiteSpace(transaction.Id))
                {
                    return null;
                }
                else
                {
                    return transaction.Id;
                }
            }
        }
    }
}