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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The hardware profile common data request handler.
        /// </summary>
        public class HardwareProfileDataService : IRequestHandler
        {
            /// <summary>
            /// The hardware profile view names.
            /// </summary>
            private const string TerminalCashDrawersViewName = "TERMINALCASHDRAWERSVIEW";
            private const string HardwareProfilesViewName = "HARDWAREPROFILESVIEW";
            private const string HardwareProfilePrintersViewName = "HARDWAREPROFILEPRINTERSVIEW";
            private const string HardwareProfileScannersViewName = "HARDWAREPROFILESCANNERSVIEW";
            private const string HardwareProfileCashDrawersViewName = "HARDWAREPROFILECASHDRAWERSVIEW";
    
            private const string HardwareProfileColumnName = "HARDWAREPROFILE";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetHardwareProfileDataRequest),
                        typeof(GetHardwareProfileCashDrawersDataRequest),
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
                Response response;
    
                if (requestType == typeof(GetHardwareProfileDataRequest))
                {
                    response = this.GetHardwareProfile((GetHardwareProfileDataRequest)request);
                }
                else if (requestType == typeof(GetHardwareProfileCashDrawersDataRequest))
                {
                    response = this.GetHardwareProfileCashDrawers((GetHardwareProfileCashDrawersDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private SingleEntityDataServiceResponse<HardwareProfile> GetHardwareProfile(GetHardwareProfileDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
                ThrowIf.NullOrWhiteSpace(request.ProfileId, "request.ProfileId");
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = HardwareProfilesViewName
                };
    
                // Add query clause for profile id
                query.IsQueryByPrimaryKey = true;
    
                query.Where = @"(PROFILEID = @ProfileId)";
                query.Parameters["@ProfileId"] = request.ProfileId;
    
                // Load hardware profile
                HardwareProfile hardwareProfile;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    hardwareProfile = databaseContext.ReadEntity<HardwareProfile>(query).SingleOrDefault();
                }
    
                if (hardwareProfile != null)
                {
                    GetHardwareProfileCashDrawersDataRequest getCashDrawersDataRequest = new GetHardwareProfileCashDrawersDataRequest(request.ProfileId, null, QueryResultSettings.AllRecords);
                    PagedResult<HardwareProfileCashDrawer> hardwareProfileCashDrawers = request.RequestContext.Execute<EntityDataServiceResponse<HardwareProfileCashDrawer>>(getCashDrawersDataRequest).PagedEntityCollection;
                    hardwareProfile.SetupDevices(
                        this.GetHardwareProfilePrinters(request.ProfileId, request.RequestContext).Results,
                        this.GetHardwareProfileScanners(request.ProfileId, request.RequestContext).Results,
                        hardwareProfileCashDrawers.Results);
                }
    
                return new SingleEntityDataServiceResponse<HardwareProfile>(hardwareProfile);
            }
    
            private PagedResult<HardwareProfilePrinter> GetHardwareProfilePrinters(string profileId, RequestContext context)
            {
                ThrowIf.NullOrWhiteSpace(profileId, "profileId");
    
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = HardwareProfilePrintersViewName,
                    IsQueryByPrimaryKey = false
                };
    
                // Add query clause for profile id
                query.Where = @"(PROFILEID = @ProfileId)";
                query.Parameters["@ProfileId"] = profileId;
    
                PagedResult<HardwareProfilePrinter> hardwareProfilePrinters;
                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    hardwareProfilePrinters = databaseContext.ReadEntity<HardwareProfilePrinter>(query);
                }
    
                return hardwareProfilePrinters;
            }
    
            private PagedResult<HardwareProfileScanner> GetHardwareProfileScanners(string profileId, RequestContext context)
            {
                ThrowIf.NullOrWhiteSpace(profileId, "profileId");
    
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = HardwareProfileScannersViewName,
                    IsQueryByPrimaryKey = false
                };
    
                // Add query clause for profile id
                query.Where = @"(PROFILEID = @ProfileId)";
                query.Parameters["@ProfileId"] = profileId;
    
                PagedResult<HardwareProfileScanner> hardwareProfileScanners;
                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    hardwareProfileScanners = databaseContext.ReadEntity<HardwareProfileScanner>(query);
                }
    
                return hardwareProfileScanners;
            }
    
            private EntityDataServiceResponse<HardwareProfileCashDrawer> GetHardwareProfileCashDrawers(GetHardwareProfileCashDrawersDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (string.IsNullOrWhiteSpace(request.ProfileId) && string.IsNullOrWhiteSpace(request.TerminalId))
                {
                    throw new ArgumentException("Either profileId or TerminalId should be provided.");
                }
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    IsQueryByPrimaryKey = false
                };
    
                if (!string.IsNullOrWhiteSpace(request.ProfileId))
                {
                    query.From = HardwareProfileCashDrawersViewName;
    
                    // Add query clause for profile id
                    query.Where = @"(PROFILEID = @ProfileId)";
                    query.Parameters["@ProfileId"] = request.ProfileId;
                }
                else if (!string.IsNullOrWhiteSpace(request.TerminalId))
                {
                    query.From = TerminalCashDrawersViewName;
    
                    // Add query clause for profile id
                    query.Where = @"(TERMINALID = @TerminalId)";
                    query.Parameters["@TerminalId"] = request.TerminalId;
                }
    
                PagedResult<HardwareProfileCashDrawer> hardwareProfileCashDrawers;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    hardwareProfileCashDrawers = databaseContext.ReadEntity<HardwareProfileCashDrawer>(query);
                }
    
                return new EntityDataServiceResponse<HardwareProfileCashDrawer>(hardwareProfileCashDrawers);
            }
        }
    }
}
