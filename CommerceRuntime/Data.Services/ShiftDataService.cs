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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// A data service implementation associated with <see cref="Shift"/>s.
        /// </summary>
        public class ShiftDataService : IRequestHandler
        {
            private const string ShiftTenderLinesView = "SHIFTTENDERLINESVIEW";
            private const string ShiftAccountsView = "SHIFTACCOUNTSVIEW";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(GetShiftDataRequest) };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetShiftDataRequest))
                {
                    response = this.GetShift((GetShiftDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the shift.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A single entity data service response.</returns>
            private SingleEntityDataServiceResponse<Shift> GetShift(GetShiftDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.TerminalId, "request.TerminalId");
    
                long channelId = request.RequestContext.GetPrincipal().ChannelId;
    
                // Loads shift.
                ShiftDataQueryCriteria criteria = new ShiftDataQueryCriteria();
                criteria.ChannelId = channelId;
                criteria.TerminalId = request.TerminalId;
                criteria.ShiftId = request.ShiftId;
                criteria.SearchByTerminalId = true;
                criteria.IncludeSharedShifts = true;
    
                GetShiftDataDataRequest dataServiceRequest = new GetShiftDataDataRequest(criteria, QueryResultSettings.SingleRecord);
                Shift shift = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<Shift>>(dataServiceRequest, request.RequestContext).PagedEntityCollection.FirstOrDefault();
    
                if (shift != null)
                {
                    // Load shift tender lines.
                    PagedResult<ShiftTenderLine> shiftTenderLines = this.GetShiftEntity<ShiftTenderLine>(string.Empty, ShiftTenderLinesView, request.TerminalId, request.ShiftId, request, queryByPrimaryKey: false);
    
                    // Load shift account lines.
                    PagedResult<ShiftAccountLine> shiftAccountLines = this.GetShiftEntity<ShiftAccountLine>(string.Empty, ShiftAccountsView, request.TerminalId, request.ShiftId, request, queryByPrimaryKey: false);
    
                    shift.AccountLines = shiftAccountLines.Results.ToList();
                    shift.TenderLines = shiftTenderLines.Results.ToList();
                }
    
                return new SingleEntityDataServiceResponse<Shift>(shift);
            }
    
            /// <summary>
            /// Gets the shift tender lines or account lines entity.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="viewName">The view name.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="shiftId">The shift identifier.</param>
            /// <param name="request">The request.</param>
            /// <param name="queryByPrimaryKey">The query by primary key flag.</param>
            /// <typeparam name="T">The entity type.</typeparam>
            /// <returns>A collection of shift tender lines or account lines.</returns>
            private PagedResult<T> GetShiftEntity<T>(string userId, string viewName, string terminalId, long shiftId, Request request, bool queryByPrimaryKey) where T : CommerceEntity, new()
            {
                var settings = QueryResultSettings.AllRecords;
    
                if (queryByPrimaryKey)
                {
                    settings = QueryResultSettings.SingleRecord;
                }
    
                var query = new SqlPagedQuery(settings)
                {
                    From = viewName,
                };
    
                var whereClauses = new List<string>();
                whereClauses.Add("CHANNEL = @ChannelId");
                whereClauses.Add("TERMINALID = @TerminalId");
                whereClauses.Add("SHIFTID = @ShiftId");
    
                query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
                query.Parameters["@TerminalId"] = terminalId;
                query.Parameters["@ShiftId"] = shiftId;
    
                if (!string.IsNullOrEmpty(userId))
                {
                    whereClauses.Add(@"(STAFFID = @StaffId OR CURRENTSTAFFID = @StaffId)");
                    query.Parameters["@StaffId"] = userId;
                }
    
                // Compose the where clause
                if (whereClauses.Count != 0)
                {
                    query.Where = string.Join(" AND ", whereClauses);
                }
    
                // Load the shift entity
                PagedResult<T> shiftEntity = null;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    shiftEntity = databaseContext.ReadEntity<T>(query);
                }
    
                return shiftEntity;
            }
        }
    }
}
