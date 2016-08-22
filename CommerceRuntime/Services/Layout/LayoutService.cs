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
        using System.Linq;
        using Commerce.Runtime.Services.Layout;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// The layout service.
        /// </summary>
        public class LayoutService : IRequestHandler
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
                        typeof(GetButtonGridsServiceRequest),
                        typeof(GetButtonGridByIdServiceRequest),
                        typeof(GetButtonGridsByIdsServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Implementation of button grid service.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>The response object.</returns>
            public Response Execute(Request request)
            {
                Response response;
    
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                ServicesHelper.ValidateInboundRequest(request);
                Type requestType = request.GetType();
    
                if (requestType == typeof(GetButtonGridByIdServiceRequest))
                {
                    response = GetButtonGridById((GetButtonGridByIdServiceRequest)request);
                }
                else if (requestType == typeof(GetButtonGridsServiceRequest))
                {
                    response = GetButtonGrids((GetButtonGridsServiceRequest)request);
                }
                else if (requestType == typeof(GetButtonGridsByIdsServiceRequest))
                {
                    response = GetButtonGridByIds((GetButtonGridsByIdsServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType().FullName));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets a button grid.
            /// </summary>
            /// <param name="request">Request instance.</param>
            /// <returns>Response instance.</returns>
            private static GetButtonGridByIdServiceResponse GetButtonGridById(GetButtonGridByIdServiceRequest request)
            {
                var butonGridIds = new List<string> { request.ButtonGridId };
                var getButtonGridDataRequest = new GetButtonGridsDataRequest(butonGridIds, request.QueryResultSettings);
                ButtonGrid buttonGrid = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ButtonGrid>>(getButtonGridDataRequest, request.RequestContext).PagedEntityCollection.FirstOrDefault();
    
                return new GetButtonGridByIdServiceResponse(buttonGrid);
            }
    
            /// <summary>
            /// Gets all button grids.
            /// </summary>
            /// <param name="request">Request instance.</param>
            /// <returns>Response instance.</returns>
            private static GetButtonGridsServiceResponse GetButtonGrids(GetButtonGridsServiceRequest request)
            {
                // resolves the layout based on the principal (user, terminal, channel) and get the button grid ids for those
                var getTillLayoutRequest = new GetTillLayoutDataRequest();
                TillLayout layout = request.RequestContext.Execute<SingleEntityDataServiceResponse<TillLayout>>(getTillLayoutRequest).Entity;
    
                if (layout == null)
                {
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_LayoutNotFound, "Could not find layout associated with the request.");
                }
    
                // get all button grids
                var getButtonGridDataRequest = new GetButtonGridsDataRequest(QueryResultSettings.AllRecords);
                PagedResult<ButtonGrid> buttonGrids = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ButtonGrid>>(getButtonGridDataRequest, request.RequestContext).PagedEntityCollection;
    
                // get all button grids that participate on the layout (top level)
                var topLevelButtonGridIdsPertainingToLayout = layout.ButtonGridZones.Select(buttonGridZone => buttonGridZone.ButtonGridId);
    
                // get filter only button grids pertaining to the layout
                var layoutFilteredButtonGrids = new ButtonGridFilter(topLevelButtonGridIdsPertainingToLayout, buttonGrids.Results).GetButtonGrids();
    
                // paginate result according to client settings
                PagedResult<ButtonGrid> buttonGridPagedResult = new PagedResult<ButtonGrid>(
                    layoutFilteredButtonGrids,
                    request.QueryResultSettings.Paging,
                    layoutFilteredButtonGrids.Count);
                return new GetButtonGridsServiceResponse(buttonGridPagedResult);
            }
    
            /// <summary>
            /// Gets button grids by identifiers.
            /// </summary>
            /// <param name="request">Request instance.</param>
            /// <returns>Response instance.</returns>
            private static GetButtonGridsByIdsServiceResponse GetButtonGridByIds(GetButtonGridsByIdsServiceRequest request)
            {
                var getButtonGridDataRequest = new GetButtonGridsDataRequest(request.ButtonGridIds, request.QueryResultSettings);
                PagedResult<ButtonGrid> buttonGrids = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ButtonGrid>>(getButtonGridDataRequest, request.RequestContext).PagedEntityCollection;
    
                return new GetButtonGridsByIdsServiceResponse(buttonGrids);
            }
        }
    }
}
