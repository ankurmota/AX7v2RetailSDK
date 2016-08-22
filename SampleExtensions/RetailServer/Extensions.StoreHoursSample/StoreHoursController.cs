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
    namespace RetailServer.StoreHoursSample
    {
        using System;
        using System.Collections.Generic;
        using System.Runtime.InteropServices;
        using System.Web.Http;
        using System.Web.OData;
        using Commerce.Runtime.StoreHoursSample.Messages;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.RetailServerLibrary;
        using Microsoft.Dynamics.Retail.RetailServerLibrary.ODataControllers;
        using SampleDataModel = Commerce.Runtime.DataModel;

        /// <summary>
        /// The controller to retrieve a new entity.
        /// </summary>
        [ComVisible(false)]
        public class StoreHoursController : CommerceController<SampleDataModel.StoreDayHours, string>
        {
            /// <summary>
            /// Gets the controller name used to load extended controller.
            /// </summary>
            public override string ControllerName
            {
                get { return "StoreHours"; }
            }

            /// <summary>
            /// Gets the store hours for a given store.
            /// </summary>
            /// <param name="parameters">The parameters to this action.</param>
            /// <returns>The list of store hours.</returns>
            [HttpPost]
            [CommerceAuthorization(AllowedRetailRoles = new string[] { CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee })]
            public System.Web.OData.PageResult<SampleDataModel.StoreDayHours> GetStoreDaysByStore(ODataActionParameters parameters)
            {
                if (parameters == null)
                {
                    throw new ArgumentNullException("parameters");
                }

                var runtime = CommerceRuntimeManager.CreateRuntime(this.CommercePrincipal);

                QueryResultSettings queryResultSettings = QueryResultSettings.SingleRecord;
                queryResultSettings.Paging = new PagingInfo(10);

                var request = new GetStoreHoursDataRequest((string)parameters["StoreNumber"]) { QueryResultSettings = queryResultSettings };
                PagedResult<SampleDataModel.StoreDayHours> hours = runtime.Execute<GetStoreHoursDataResponse>(request, null).DayHours;
                return this.ProcessPagedResults(hours);
            }
        }
    }
}