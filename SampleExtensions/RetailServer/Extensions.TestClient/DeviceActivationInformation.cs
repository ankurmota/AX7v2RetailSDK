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
    namespace RetailServer.TestClient
    {
        using System;
        using System.Collections.Generic;
        using System.Xml.Serialization;

        /// <summary>
        /// Holds information about a device activation.
        /// </summary>
        [Serializable]
        public sealed class DeviceActivationInformation
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DeviceActivationInformation"/> class.
            /// </summary>
            public DeviceActivationInformation()
            {
            }

            internal DeviceActivationInformation(string retailServerUrl, string registerId, string storeId, string deviceToken, string deviceId, DateTime activationDateTime)
            {
                this.RetailServerUrl = retailServerUrl;
                this.RegisterId = registerId;
                this.StoreId = storeId;
                this.DeviceToken = deviceToken;
                this.DeviceId = deviceId;
                this.ActivationDateTime = activationDateTime;
            }

            /// <summary>
            /// Gets or sets the register Id.
            /// </summary>
            public string RegisterId { get; set; }

            /// <summary>
            /// Gets or sets the store Id.
            /// </summary>
            public string StoreId { get; set; }

            /// <summary>
            /// Gets or sets the device token.
            /// </summary>
            public string DeviceToken { get; set; }

            /// <summary>
            /// Gets or sets the device Id.
            /// </summary>
            public string DeviceId { get; set; }

            /// <summary>
            /// Gets or sets the time of activation.
            /// </summary>
            public DateTime ActivationDateTime { get; set; }

            /// <summary>
            /// Gets or sets the url of RetailServer.
            /// </summary>
            public string RetailServerUrl { get; set; }

            /// <summary>
            /// Gets the unique activation Id.
            /// </summary>
            [XmlIgnore]
            public string UniqueActivationId
            {
                get
                {
                    return string.Format("{0} - {1}", this.RetailServerUrl, this.DeviceId);
                }
            }

            /// <summary>
            /// Returns a string representation of this object.
            /// </summary>
            /// <returns>The string representation.</returns>
            public override string ToString()
            {
                List<string> list = new List<string>();
                list.Add(string.Format("RetailServer url: {0}", this.RetailServerUrl));
                list.Add(string.Format("Activation time: {0}", this.ActivationDateTime));
                list.Add(string.Format("Device Id: {0}", this.DeviceId));
                list.Add(string.Format("Register Id: {0}", this.RegisterId));
                list.Add(string.Format("Store Id: {0}", this.StoreId));
                list.Add(string.Format("Device Token: {0}...", this.DeviceToken.Substring(0, 20)));
                return string.Join(Environment.NewLine, list);
            }
        }
    }
}
