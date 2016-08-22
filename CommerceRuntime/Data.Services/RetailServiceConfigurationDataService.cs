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
        /// Customer data services that contains methods to retrieve the retail service configuration.
        /// </summary>
        public class RetailServiceConfigurationDataService : IRequestHandler
        {
            private const string AuthenticationTokenIssuersSettingsName = "TENANTID";
            private const string TrialModeKeyName = "APPTOUR";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetRetailServiceConfigurationDataRequest),
                        typeof(GetRetailPlanOffersDataRequest),
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
    
                Type requestedType = request.GetType();
                Response response;
    
                if (requestedType == typeof(GetRetailServiceConfigurationDataRequest))
                {
                    response = GetRetailServiceConfiguration((GetRetailServiceConfigurationDataRequest)request);
                }
                else if (requestedType == typeof(GetRetailPlanOffersDataRequest))
                {
                    response = GetRetailPlanOffers((GetRetailPlanOffersDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the trial plan offer details.
            /// </summary>
            /// <param name="request">The data request for trial plan offer.</param>
            /// <returns>
            /// A entity data service response.
            /// </returns>
            private static SingleEntityDataServiceResponse<bool> GetRetailPlanOffers(GetRetailPlanOffersDataRequest request)
            {
                RetailServiceConfigurationDataManager dataManager = new RetailServiceConfigurationDataManager(request.RequestContext);
                ReadOnlyCollection<RetailServiceConfigurationSetting> serverConfiguration = dataManager.GetRetailServiceConfigurationSettings();
                RetailServiceConfigurationSetting trialModeSettings = serverConfiguration.SingleOrDefault(isTrialMode => string.Equals(isTrialMode.Name, TrialModeKeyName, StringComparison.OrdinalIgnoreCase));
                bool isTrialModeEnabled;
                if (trialModeSettings != null)
                {
                    var intSettingValue = Convert.ToInt32(trialModeSettings.Value.Trim());
                    isTrialModeEnabled = Convert.ToBoolean(intSettingValue);
                }
                else
                {
                    isTrialModeEnabled = false;
                }
    
                return new SingleEntityDataServiceResponse<bool>(isTrialModeEnabled);
            }
    
            /// <summary>
            /// Gets the retail service configuration.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static SingleEntityDataServiceResponse<RetailServiceConfiguration> GetRetailServiceConfiguration(GetRetailServiceConfigurationDataRequest request)
            {
                var dataManager = new RetailServiceConfigurationDataManager(request.RequestContext);
                var serverConfiguration = dataManager.GetRetailServiceConfigurationSettings();
    
                var tenantIdIssuerSettings = serverConfiguration.FirstOrDefault(x => string.Equals(x.Name, AuthenticationTokenIssuersSettingsName));
    
                var retailServerConfiguration = new RetailServiceConfiguration();
                retailServerConfiguration.AuthenticationTokenIssuers = new List<string>();
                retailServerConfiguration.ServicePrincipalNames = new List<string>();
                if (tenantIdIssuerSettings != null)
                {
                    retailServerConfiguration.AuthenticationTokenIssuers.Add(tenantIdIssuerSettings.Value);
                }
    
                return new SingleEntityDataServiceResponse<RetailServiceConfiguration>(retailServerConfiguration);
            }
        }
    }
}
