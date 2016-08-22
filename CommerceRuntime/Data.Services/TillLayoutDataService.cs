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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Channel data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class TillLayoutDataService : IRequestHandler
        {
            private const string ButtonGridsView = "BUTTONGRIDSVIEW";
            private const string ButtonGridButtonsView = "BUTTONGRIDBUTTONSVIEW";
            private const string ButtonGridIdTableTypeParameterName = "@TVP_BUTTONGRIDIDTABLETYPE";
            private const string ButtonGridButtonIdColumn = "ID";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetTillLayoutDataRequest),
                        typeof(GetButtonGridsDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
    
                if (requestType == typeof(GetTillLayoutDataRequest))
                {
                    return this.GetTillLayout((GetTillLayoutDataRequest)request);
                }
                else if (requestType == typeof(GetButtonGridsDataRequest))
                {
                    return this.GetButtonsGrids((GetButtonGridsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
            }
    
            /// <summary>
            /// Gets till layout by given parameters in request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private SingleEntityDataServiceResponse<TillLayout> GetTillLayout(GetTillLayoutDataRequest request)
            {
                var context = request.RequestContext;
    
                var tillLayoutDataManager = this.GetTillLayoutDataManagerInstance(context);
    
                string userId;
                long terminalId;
                long channelId;
    
                if (request.IsPrincipalSpecified)
                {
                    userId = request.UserId;
                    terminalId = request.TerminalId.Value;
                    channelId = request.ChannelId.Value;
                }
                else
                {
                    var principal = context.GetPrincipal();
    
                    channelId = principal.ChannelId;
                    terminalId = principal.TerminalId;
                    userId = principal.UserId;
                }
    
                var tillLayout = tillLayoutDataManager.GetTillLayout(channelId, terminalId, userId);
                return new SingleEntityDataServiceResponse<TillLayout>(tillLayout);
            }
    
            /// <summary>
            /// Gets the till layout data manager instance.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>The till layout data manager.</returns>
            private TillLayoutDataManager GetTillLayoutDataManagerInstance(RequestContext context)
            {
                return new TillLayoutDataManager(context);
            }
    
            /// <summary>
            /// Gets a button grids by identifiers.
            /// </summary>
            /// <param name="request">The get button grids data request.</param>
            /// <returns>
            /// Collection of matching button grids.
            /// </returns>
            private EntityDataServiceResponse<ButtonGrid> GetButtonsGrids(GetButtonGridsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ButtonGridIds, "request.ButtonGridIds");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                PagedResult<ButtonGrid> buttonGrids = null;
    
                // Default query to retrieve all the button grids.
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = ButtonGridsView,
                    OrderBy = ButtonGrid.ButtonGridIdColumn
                };
    
                // Update query when only one button grid is retrieved.
                if (request.ButtonGridIds.Count() == 1)
                {
                    query.Where = string.Format("{0} = @{0}", ButtonGrid.ButtonGridIdColumn);
                    query.Parameters[string.Format("@{0}", ButtonGrid.ButtonGridIdColumn)] = request.ButtonGridIds.FirstOrDefault();
                }
    
                // Update query for retrieving multiple button grids.
                if (request.ButtonGridIds.HasMultiple())
                {
                    using (StringIdTableType buttonGridIdTableType = new StringIdTableType(request.ButtonGridIds, ButtonGrid.ButtonGridIdColumn))
                    {
                        query.Parameters[ButtonGridIdTableTypeParameterName] = buttonGridIdTableType;
    
                        // Query execution for retrieving multiple button grids.
                        buttonGrids = this.ExecuteQuery<ButtonGrid>(query, request.RequestContext);
                    }
                }
                else
                {
                    // Query execution for retrieving one or all the button grids.
                    buttonGrids = this.ExecuteQuery<ButtonGrid>(query, request.RequestContext);
                }
    
                // Get the button grid buttons.
                if (buttonGrids != null && buttonGrids.Results != null)
                {
                    var buttonGridIds = buttonGrids.Results.Select(b => b.Id);
    
                    ReadOnlyCollection<ButtonGridButton> buttons = this.GetButtonGridButtons(buttonGridIds, request.RequestContext).Results;
    
                    foreach (var buttonGrid in buttonGrids.Results)
                    {
                        buttonGrid.Buttons = buttons.Where(b => string.Equals(b.ButtonGridId, buttonGrid.Id, StringComparison.OrdinalIgnoreCase));
                    }
                }
    
                return new EntityDataServiceResponse<ButtonGrid>(buttonGrids);
            }
    
            /// <summary>
            /// Get buttons for a button grid.
            /// </summary>
            /// <param name="buttonGridIds">The button grid identifiers.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Collection of button grid buttons.</returns>
            private PagedResult<ButtonGridButton> GetButtonGridButtons(IEnumerable<string> buttonGridIds, RequestContext context)
            {
                if (buttonGridIds == null || !buttonGridIds.Any())
                {
                    return (new List<ButtonGridButton>()).AsPagedResult();
                }
    
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = ButtonGridButtonsView,
                    OrderBy = ButtonGridButtonIdColumn
                };
    
                PagedResult<ButtonGridButton> buttonGridButtons;
    
                using (StringIdTableType buttonGridIdTableType = new StringIdTableType(buttonGridIds, ButtonGrid.ButtonGridIdColumn))
                {
                    query.Parameters[ButtonGridIdTableTypeParameterName] = buttonGridIdTableType;
    
                    buttonGridButtons = this.ExecuteQuery<ButtonGridButton>(query, context);
                }
    
                return buttonGridButtons ?? new List<ButtonGridButton>().AsPagedResult();
            }
    
            private PagedResult<T> ExecuteQuery<T>(SqlPagedQuery query, RequestContext context) where T : CommerceEntity, new()
            {
                PagedResult<T> results;
    
                using (var databaseContext = new DatabaseContext(context))
                {
                    results = databaseContext.ReadEntity<T>(query);
                }
    
                return results;
            }
        }
    }
}
