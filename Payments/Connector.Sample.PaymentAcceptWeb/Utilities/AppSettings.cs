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
    /*
    SAMPLE CODE NOTICE
    
    THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
    OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
    THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
    NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
    */
    namespace Retail.SampleConnector.PaymentAcceptWeb.Utilities
    {
        using System;
        using System.Configuration;
        using System.Globalization;
    
        /// <summary>
        /// The application settings from Web.config file.
        /// </summary>
        public static class AppSettings
        {
            /// <summary>
            /// Gets the connector assembly name.
            /// </summary>
            public static string ConnectorAssembly
            {
                get
                {
                    return GetSetting<string>("ConnectorAssembly");
                }
            }
    
            /// <summary>
            /// Gets the connector name.
            /// </summary>
            public static string ConnectorName
            {
                get
                {
                    return GetSetting<string>("ConnectorName");
                }
            }
    
            /// <summary>
            /// Gets the valid period of the payment entry (in minutes).
            /// </summary>
            public static int PaymentEntryValidPeriodInMinutes
            {
                get
                {
                    return GetSetting<int>("PaymentEntryValidPeriodInMinutes");
                }
            }
    
            private static T GetSetting<T>(string name)
            {
                string value = ConfigurationManager.AppSettings[name];
    
                if (value == null)
                {
                    string message = string.Format("Could not find setting '{0}' in the configuration file.", name);
                    throw new ArgumentException(name, message);
                }
    
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
        }
    }
}
