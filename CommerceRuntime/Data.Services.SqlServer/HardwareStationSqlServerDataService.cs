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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// The hardware station profile data request handler.
        /// </summary>
        public class HardwareStationSqlServerDataService : IRequestHandler
        {
            private const string GetHardwareStationsSprocName = "GETHARDWARESTATIONS";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetHardwareStationDataRequest),
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
                ThrowIf.Null(request, "request");

                Type requestType = request.GetType();
                Response response;

                if (requestType == typeof(GetHardwareStationDataRequest))
                {
                    response = this.GetHardwareStationProfile((GetHardwareStationDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            private EntityDataServiceResponse<HardwareStationProfile> GetHardwareStationProfile(GetHardwareStationDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ParameterSet parameters = new ParameterSet();
                Tuple<PagedResult<HardwareStationProfile>,
                        ReadOnlyCollection<HardwareConfiguration>,
                        ReadOnlyCollection<HardwareConfiguration>,
                        ReadOnlyCollection<HardwareConfiguration>> dataSets = null;

                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext, request.QueryResultSettings))
                {
                    dataSets = databaseContext.ExecuteStoredProcedure<HardwareStationProfile, HardwareConfiguration, HardwareConfiguration, HardwareConfiguration>(GetHardwareStationsSprocName, parameters);
                }

                PagedResult<HardwareStationProfile> hardwareStationProfiles = dataSets.Item1;
                ReadOnlyCollection<HardwareConfiguration> drawers = dataSets.Item2;
                ReadOnlyCollection<HardwareConfiguration> printers = dataSets.Item3;
                ReadOnlyCollection<HardwareConfiguration> pinpads = dataSets.Item4;

                foreach (var hardwareStationProfile in hardwareStationProfiles.Results)
                {
                    if (hardwareStationProfile != null)
                    {
                        hardwareStationProfile.HardwareConfigurations = new HardwareConfigurations();
                        if (drawers != null)
                        {
                            hardwareStationProfile.HardwareConfigurations.CashDrawerConfigurations.AddRange(
                                from d in drawers
                                where d.HardwareStationRecordId == hardwareStationProfile.RecordId && !string.IsNullOrEmpty(d.DeviceName)
                                select d);
                        }

                        if (printers != null)
                        {
                            hardwareStationProfile.HardwareConfigurations.PrinterConfigurations.AddRange(
                                from p in printers
                                where p.HardwareStationRecordId == hardwareStationProfile.RecordId && !string.IsNullOrEmpty(p.DeviceName)
                                select p);
                        }

                        if (pinpads != null)
                        {
                            hardwareStationProfile.HardwareConfigurations.PinPadConfiguration =
                                (from p in pinpads
                                 where p.HardwareStationRecordId == hardwareStationProfile.RecordId && !string.IsNullOrEmpty(p.DeviceName)
                                 select p).SingleOrDefault();
                        }
                    }
                }

                return new EntityDataServiceResponse<HardwareStationProfile>(hardwareStationProfiles);
            }
        }
    }
}
