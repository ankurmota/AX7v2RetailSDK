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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Extended authentication service for barcode.
        /// </summary>
        public class BarcodeExtendedAuthenticationService : UniqueSecretExtendedAuthenticationService
        {
            /// <summary>
            /// Gets the unique name for this request handler.
            /// </summary>
            public override string HandlerName
            {
                get
                {
                    return "auth://example.auth.contoso.com/barcode";
                }
            }

            /// <summary>
            /// Gets a value indicating whether the service is enabled.
            /// </summary>
            /// <param name="deviceConfiguration">The device configuration.</param>
            /// <returns>A value indicating whether the service is enabled.</returns>
            protected override bool IsServiceEnabled(DeviceConfiguration deviceConfiguration)
            {
                ThrowIf.Null(deviceConfiguration, "deviceConfiguration");
                return deviceConfiguration.StaffBarcodeLogOn;
            }

            /// <summary>
            /// Gets a value indicating whether the service is requires the user password as a second factor authentication.
            /// </summary>
            /// <param name="deviceConfiguration">The device configuration.</param>
            /// <returns>A value indicating whether the service is requires the user password as a second factor authentication.</returns>
            protected override bool IsPasswordRequired(DeviceConfiguration deviceConfiguration)
            {
                ThrowIf.Null(deviceConfiguration, "deviceConfiguration");
                return deviceConfiguration.StaffBarcodeLogOnRequiresPassword;
            }
        }
    }
}
