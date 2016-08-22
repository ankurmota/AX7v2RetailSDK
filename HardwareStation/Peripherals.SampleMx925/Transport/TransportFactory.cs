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
        using Microsoft.Dynamics.Commerce.HardwareStation.Configuration;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;

        /// <summary>
        ///  Factory class to instantiate the correct transport.
        /// </summary>
        public static class TransportFactory
        {
            /// <summary>
            ///  Gets the transport object.
            /// </summary>
            /// <param name="config">Device configuration.</param>
            /// <returns>Transport object.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "It will be disposed when MX925TcpTransport gets disposed")]
            public static ITransport GetTransport(IDictionary<string, string> config)
            {
                ITransport transport = null;
                string transportType = config.GetValueOrDefault(PeripheralConfigKey.TransportType, string.Empty);
    
                // Default to serial
                if (string.IsNullOrWhiteSpace(transportType) || string.Equals(transportType, TransportType.SerialTransport, StringComparison.OrdinalIgnoreCase))
                {
                    transport = new MX925BufferedTransport(new SerialTransport(config));
                }
                else if (string.Equals(transportType, TransportType.TcpTransport, StringComparison.OrdinalIgnoreCase))
                {
                    transport = new MX925TcpTransport(new TcpTransport(config));
                }
                else if (string.Equals(transportType, TransportType.TcpTlsTransport, StringComparison.OrdinalIgnoreCase))
                {
                    transport = new MX925TcpTransport(new TcpTlsTransport(config));
                }
                else
                {
                    throw new InvalidOperationException("No suitable transport provider found");
                }
    
                return transport;
            }
        }
    }
}
