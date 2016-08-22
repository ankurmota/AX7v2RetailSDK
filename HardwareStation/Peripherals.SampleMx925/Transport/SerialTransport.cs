/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
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
        using System.IO.Ports;
        using System.Threading.Tasks;

        using Microsoft.Dynamics.Commerce.HardwareStation.Configuration;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        ///  Transport class to connect to <c>VeriFone</c> device over USB.
        /// </summary>
        public class SerialTransport : StreamTransport
        {
            private const string PortNameKey = "PortName";
            private const string BaudRateKey = "BaudRate";
            private const string ParityKey = "Parity";
            private const string DataBitsKey = "DataBits";
            private const string StopBitsKey = "StopBits";
    
            private const string DefaultPortName = "COM9";
            private const int DefaultBaudRate = 115200;
            private const Parity DefaultParity = Parity.None;
            private const int DefaultDataBits = 8;
            private const StopBits DefaultStopBits = StopBits.One;
    
            private readonly string portName;
            private readonly int baudRate;
            private readonly Parity parity;
            private readonly int dataBits;
            private readonly StopBits stopBits;
    
            private SerialPort serialPort;
    
            /// <summary>
            ///  Initializes a new instance of the <see cref="SerialTransport" /> class.
            /// </summary>
            /// <param name="config">Case insensitive configuration parameters.</param>
            public SerialTransport(IDictionary<string, string> config)
            {
                this.portName = config.GetValueOrDefault(PortNameKey, DefaultPortName);
                this.baudRate = config.GetValueOrDefault(BaudRateKey, DefaultBaudRate, int.TryParse);
                this.parity = config.GetValueOrDefault(ParityKey, DefaultParity, Enum.TryParse);
                this.dataBits = config.GetValueOrDefault(DataBitsKey, DefaultDataBits, int.TryParse);
                this.stopBits = config.GetValueOrDefault(StopBitsKey, DefaultStopBits, Enum.TryParse);
            }
    
            /// <summary>
            ///  Gets a value indicating whether the transport is connected.
            /// </summary>
            protected override bool IsConnected
            {
                get
                {
                    return this.serialPort != null && this.serialPort.IsOpen;
                }
            }
    
            /// <summary>
            ///  ConnectAsync to the device.
            /// </summary>
            /// <returns>A task that connects to the device.</returns>
            public override async Task ConnectAsync()
            {
                int count = 0;
                bool success = false;
                do
                {
                    if (this.serialPort != null)
                    {
                        await this.CloseAsync();
                        await Task.Delay(1000);
                    }
    
                    this.serialPort = new SerialPort(this.portName, this.baudRate, this.parity, this.dataBits, this.stopBits);
    
                    try
                    {
                        this.serialPort.Open();
                        this.Stream = this.serialPort.BaseStream;
                        success = true;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // This could happen when accessing the device on a specific port where HW station service does not have access. 
                        RetailLogger.Log.HardwareStationActionFailure("UnAuthorized access exception occured.", ex);

                        count++;
                    }
                }
                while (count < StreamTransport.RetryCount && !success);
            }
    
            /// <summary>
            ///  Closes the connection.
            /// </summary>
            /// <returns>A task that closes the connection to the device.</returns>
            public override async Task CloseAsync()
            {
                if (this.serialPort != null)
                {
                    this.serialPort.Close();
                    this.serialPort.Dispose();
                    this.serialPort = null;
                }
    
                await base.CloseAsync();
            }
        }
    }
}
