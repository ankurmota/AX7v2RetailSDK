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
        using System.Linq;
        using System.Xml.Serialization;

        /// <summary>
        /// Class that holds all persistent storage for this application.
        /// </summary>
        [Serializable]
        public sealed class ApplicationStorage
        {
            private Dictionary<string, DeviceActivationInformation> allActivations = new Dictionary<string, DeviceActivationInformation>();

            /// <summary>
            /// Initializes a new instance of the <see cref="ApplicationStorage"/> class.
            /// </summary>
            public ApplicationStorage()
            {
            }

            /// <summary>
            /// Gets or sets the activation-related information.
            /// </summary>
            [XmlArray("ActivationInformation")]
            [XmlArrayItem("DeviceActivationInformation")]
            public DeviceActivationInformation[] ActivationInformation
            {
                get
                {
                    return this.allActivations.Values.ToArray();
                }

                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }

                    foreach (DeviceActivationInformation info in value)
                    {
                        this.allActivations.Add(info.UniqueActivationId, info);
                    }
                }
            }

            /// <summary>
            /// Initializes a new ApplicationStorage instance.
            /// </summary>
            /// <returns>The ApplicationStorage instance.</returns>
            public static ApplicationStorage Initialize()
            {
                ApplicationStorage storage = null;
                if (!Helpers.TryLoadFromXml(MainForm.AppStorageXmlFilePath, out storage))
                {
                    storage = new ApplicationStorage();
                    Helpers.ToXml(MainForm.AppStorageXmlFilePath, storage);
                }

                return storage;
            }

            /// <summary>
            /// Adds a new DeviceActivationInformation to the application storage.
            /// </summary>
            /// <param name="information">A DeviceActivationInformation to be added.</param>
            /// <returns>The DeviceActivationInformation instance that was added.</returns>
            public DeviceActivationInformation Add(DeviceActivationInformation information)
            {
                if (information == null)
                {
                    throw new ArgumentNullException("information");
                }

                this.allActivations[information.UniqueActivationId] = information;
                Helpers.ToXml(MainForm.AppStorageXmlFilePath, this);
                return information;
            }

            /// <summary>
            /// Removes a DeviceActivationInformation from the application storage.
            /// </summary>
            /// <param name="information">A DeviceActivationInformation to be removed.</param>
            public void Remove(DeviceActivationInformation information)
            {
                if (information == null)
                {
                    throw new ArgumentNullException("information");
                }

                this.allActivations.Remove(information.UniqueActivationId);
                Helpers.ToXml(MainForm.AppStorageXmlFilePath, this);
            }
        }
    }
}
