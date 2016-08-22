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
        using Commerce.Runtime.TransactionService;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Represents an implementation of the commerce list service.
        /// </summary>
        public sealed class CommerceListRealtimeService : IRequestHandler
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
                        typeof(GetCommerceListRealtimeRequest),
                        typeof(SaveCommerceListRealtimeRequest),
                        typeof(AcceptCommerceListInvitationRealtimeRequest)
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
    
                if (requestType == typeof(GetCommerceListRealtimeRequest))
                {
                    response = GetCommerceLists((GetCommerceListRealtimeRequest)request);
                }
                else if (requestType == typeof(SaveCommerceListRealtimeRequest))
                {
                    response = SaveCommerceList((SaveCommerceListRealtimeRequest)request);
                }
                else if (requestType == typeof(AcceptCommerceListInvitationRealtimeRequest))
                {
                    response = AcceptInvitation((AcceptCommerceListInvitationRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Invokes the Real-time service to get commerce lists from AX.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Returns the response that contains the result.</returns>
            private static GetCommerceListRealtimeResponse GetCommerceLists(GetCommerceListRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
                var clists = transactionService.GetCommerceLists(request.Id, request.CustomerAccountNumber, request.FavoriteFilter, request.PublicFilter);
                return new GetCommerceListRealtimeResponse(clists);
            }
    
            /// <summary>
            /// Invokes the Real-time service to save a commerce list on AX.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Returns the response that contains the result.</returns>
            private static SaveCommerceListRealtimeResponse SaveCommerceList(SaveCommerceListRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
    
                CommerceList commerceList = null;
                string filterAccountNumber = null;
                if (request.RequestContext.GetPrincipal().IsCustomer)
                {
                    filterAccountNumber = request.RequestContext.GetPrincipal().UserId;
                }
    
                switch (request.OperationType)
                {
                    case CommerceListOperationType.Create:
                        commerceList = transactionService.CreateCommerceList(request.CommerceList);
                        break;
    
                    case CommerceListOperationType.Delete:
                        transactionService.DeleteCommerceList(request.CommerceList.Id, filterAccountNumber);
                        break;
    
                    case CommerceListOperationType.Update:
                        commerceList = transactionService.UpdateCommerceList(request.CommerceList, filterAccountNumber);
                        break;
    
                    case CommerceListOperationType.AddLine:
                        foreach (var line in request.CommerceList.CommerceListLines)
                        {
                            commerceList = transactionService.CreateCommerceListLine(line, filterAccountNumber);
                        }
    
                        break;
    
                    case CommerceListOperationType.RemoveLine:
                        foreach (var line in request.CommerceList.CommerceListLines)
                        {
                            commerceList = transactionService.DeleteCommerceListLine(line.LineId, filterAccountNumber);
                        }
    
                        break;
    
                    case CommerceListOperationType.UpdateLine:
                        foreach (var line in request.CommerceList.CommerceListLines)
                        {
                            commerceList = transactionService.UpdateCommerceListLine(line, filterAccountNumber);
                        }
    
                        break;
    
                    case CommerceListOperationType.MoveLine:
                        commerceList = transactionService.MoveCommerceListLines(request.CommerceList.CommerceListLines, request.DestinationCommerceListId, filterAccountNumber);
                        break;
    
                    case CommerceListOperationType.CopyLine:
                        commerceList = transactionService.CopyCommerceListLines(request.CommerceList.CommerceListLines, request.DestinationCommerceListId, filterAccountNumber);
                        break;
    
                    case CommerceListOperationType.AddContributor:
                        commerceList = transactionService.CreateCommerceListContributors(request.CommerceList.Id, request.CommerceList.CommerceListContributors, filterAccountNumber);
                        break;
    
                    case CommerceListOperationType.RemoveContributor:
                        commerceList = transactionService.DeleteCommerceListContributors(request.CommerceList.Id, request.CommerceList.CommerceListContributors, filterAccountNumber);
                        break;
    
                    case CommerceListOperationType.CreateInvitation:
                        commerceList = transactionService.CreateCommerceListInvitations(request.CommerceList.Id, request.CommerceList.CommerceListInvitations, filterAccountNumber);
                        break;
    
                    default:
                        throw new NotSupportedException("Cannot save a commerce list with an unspecified operation type.");
                }
    
                return new SaveCommerceListRealtimeResponse(commerceList);
            }
    
            /// <summary>
            /// Accepts an invitation to commerce list.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static NullResponse AcceptInvitation(AcceptCommerceListInvitationRealtimeRequest request)
            {
                TransactionServiceClient transactionService = new TransactionServiceClient(request.RequestContext);
    
                transactionService.AcceptCommerceListInvitation(request.InvitationToken, request.CustomerId);
                return new NullResponse();
            }
        }
    }
}
