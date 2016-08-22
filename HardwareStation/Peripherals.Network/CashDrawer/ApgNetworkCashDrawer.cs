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
    namespace Commerce.HardwareStation.Peripherals
    {
        using System;
        using System.Collections.Generic;
        using System.Composition;
        using System.Net.Sockets;
        using System.Text;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// APG cash drawer with ethernet connection.
        /// </summary>
        [Export(PeripheralType.Network, typeof(ICashDrawer))]
        public sealed class ApgNetworkCashDrawer : ICashDrawer, IDisposable
        {
            private const string OpenDrawerCommand = "opendrawer\x0a";
            private const string QueryStatusCommand = "querystatus\x0a";
            private const string OpenStatus = "OPEN";

            private TcpClient tcpClient = null;

            /// <summary>
            /// Gets a value indicating whether the cash drawer is open or not.
            /// </summary>
            public bool IsOpen
            {
                get
                {
                    byte[] command = Encoding.UTF8.GetBytes(QueryStatusCommand);
                    byte[] buffer = new byte[256];

                    var networkStream = this.tcpClient.GetStream();
                    networkStream.Write(command, 0, command.Length);
                    int read = networkStream.Read(buffer, 0, buffer.Length);
                    string status = Encoding.UTF8.GetString(buffer, 0, read);

                    return string.Equals(status, OpenStatus, StringComparison.OrdinalIgnoreCase);
                }
            }

            /// <summary>
            /// Establishes a connection to the specified cash drawer.
            /// </summary>
            /// <param name="peripheralName">Name of cash drawer device to open.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                ThrowIf.NullOrWhiteSpace(peripheralName, "peripheralName");
                ThrowIf.Null(peripheralConfig, "peripheralConfig");
                ThrowIf.Null(peripheralConfig.ExtensionProperties, "peripheralConfig.ExtensionProperties");

                IDictionary<string, object> configurations = peripheralConfig.ExtensionProperties.ToObjectDictionary();
                string ip = configurations[PeripheralConfigKey.IpAddress] as string;
                int? port = configurations[PeripheralConfigKey.Port] as int?;
                if (string.IsNullOrWhiteSpace(ip))
                {
                    throw new ArgumentException(string.Format("Peripheral configuration parameter is missing: {0}.", PeripheralConfigKey.IpAddress));
                }

                if (port == null)
                {
                    throw new ArgumentException(string.Format("Peripheral configuration parameter is missing: {0}.", PeripheralConfigKey.Port));
                }

                this.tcpClient = new TcpClient();
                this.tcpClient.Connect(ip, (int)port);
            }

            /// <summary>
            /// Terminates a connection to the cash drawer.
            /// </summary>
            public void Close()
            {
                this.Dispose();
            }

            /// <summary>
            /// Causes the cash drawer to be physically opened.
            /// </summary>
            public void OpenDrawer()
            {
                byte[] command = Encoding.UTF8.GetBytes(OpenDrawerCommand);

                var networkStream = this.tcpClient.GetStream();
                networkStream.Write(command, 0, command.Length);
            }

            /// <summary>
            ///  Closes the connection to the device.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            ///  Closes the connection to the device.
            /// </summary>
            /// <param name="disposing">Whether to dispose managed resources.</param>
            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.tcpClient != null)
                    {
                        if (this.tcpClient.Connected)
                        {
                            this.tcpClient.GetStream().Close();
                        }

                        this.tcpClient.Close();
                        this.tcpClient = null;
                    }
                }
            }
        }
    }
}
