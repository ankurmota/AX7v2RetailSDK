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
    namespace Commerce.HardwareStation.WebHost
    {
        using System;
        using System.Net;
        using System.Web;
        using System.Web.Configuration;
        using System.Web.Http;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Instrumentation;
        using Microsoft.Dynamics.Commerce.HardwareStation.WebApi;
        using Microsoft.Dynamics.Retail.Diagnostics;
        
        /// <summary>
        /// Hardware station Web host application class.
        /// </summary>
        public class WebApiApplication : HttpApplication
        {
            private const string RetailServerConfigKey = "retailServer";

            /// <summary>
            /// Handles initialization logic on application start.
            /// </summary>
            protected void Application_Start()
            {
                HandleSecurityProtocolVersion();
                GlobalConfiguration.Configure(WebApiConfig.Register);
                string retailServerUrl = WebConfigurationManager.AppSettings[RetailServerConfigKey];
                ServiceLocator.Initialize(retailServerUrl);
                InstrumentationInitializer.Initialize();
            }

            /// <summary>
            /// Handles the BeginRequest event for the incoming requests.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs" />The event data.</param>
            protected void Application_BeginRequest(object sender, EventArgs e)
            {
                string environmentId = InstrumentationInitializer.EnvironmentConfig != null ? InstrumentationInitializer.EnvironmentConfig.EnvironmentId : string.Empty;
                RetailLogger.Log.SetSessionInfo(new SessionInfo(Guid.NewGuid(), environmentId));
                CorsSupport.HandlePreflightRequest();
            }

            private static void HandleSecurityProtocolVersion()
            {
                // Set the Security protocol version to Tls1.2.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
        }
    }
}
