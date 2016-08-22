/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

/*
 IMPORTANT!!!
 THIS IS SAMPLE CODE ONLY.
 THE CODE SHOULD BE UPDATED TO WORK WITH THE APPROPRIATE PAYMENT PROVIDERS.
 PROPER MESASURES SHOULD BE TAKEN TO ENSURE THAT THE PA-DSS AND PCI DSS REQUIREMENTS ARE MET.
*/
namespace Contoso
{
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Transport
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Net;
        using System.Net.Sockets;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.HardwareStation.Configuration;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;

        /// <summary>
        ///  Transport class to connect to <c>VeriFone</c> device over ethernet.
        /// </summary>
        public class TcpTransport : StreamTransport
        {
            /// <summary>
            ///  Initializes a new instance of the <see cref="TcpTransport" /> class.
            /// </summary>
            /// <param name="config">Case insensitive configuration parameters.</param>
            public TcpTransport(IDictionary<string, string> config)
            {
                IPAddress endpointAddress = config.GetValueOrDefault(PeripheralConfigKey.IpAddress, IPAddress.None, IPAddress.TryParse);
                int port = config.GetValueOrDefault(PeripheralConfigKey.Port, 0, int.TryParse);
    
                if (endpointAddress.Equals(IPAddress.None) || port <= 0)
                {
                    throw new InvalidOperationException("Invalid internet protocol address or port specified for tcp transport");
                }
    
                this.Endpoint = new IPEndPoint(endpointAddress, port);
            }
    
            /// <summary>
            ///  Gets or sets the client.
            /// </summary>
            protected TcpClient TcpClient { get; set; }
    
            /// <summary>
            ///  Gets or sets the device endpoint.
            /// </summary>
            protected IPEndPoint Endpoint { get; set; }
    
            /// <summary>
            ///  Gets a value indicating whether the transport is connected.
            /// </summary>
            protected override bool IsConnected
            {
                get
                {
                    return this.TcpClient != null && this.TcpClient.Connected;
                }
            }
    
            /// <summary>
            ///  Connect to the device.
            /// </summary>
            /// <returns>A task that connects to the device.</returns>
            public override Task ConnectAsync()
            {
                this.TcpClient = new TcpClient();
                return this.TcpClient.ConnectAsync(this.Endpoint.Address, this.Endpoint.Port).ContinueWith(task =>
                {
                    this.Stream = this.TcpClient.GetStream();
                });
            }
    
            /// <summary>
            ///  Closes the connection.
            /// </summary>
            /// <returns>A task that closes the connection to the device.</returns>
            public override async Task CloseAsync()
            {
                await base.CloseAsync();
    
                if (this.TcpClient != null)
                {
                    this.TcpClient.Close();
                    this.TcpClient = null;
                }
            }
        }
    }
}
