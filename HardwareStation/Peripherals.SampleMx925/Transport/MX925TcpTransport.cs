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
        using System.Threading;
        using System.Threading.Tasks;
    
        /// <summary>
        ///  Transport class to implement a <c>VeriFone</c>specific TCP transport.
        /// </summary>
        public class MX925TcpTransport : MX925BufferedTransport
        {
            private const int MX925TcpHeaderLength = 2;
    
            /// <summary>
            ///  Initializes a new instance of the <see cref="MX925TcpTransport" /> class.
            /// </summary>
            /// <param name="transport">TCP transport class to wrap.</param>
            public MX925TcpTransport(ITransport transport) : base(transport)
            {
                this.HeaderLength = MX925TcpHeaderLength;
            }
    
            /// <summary>
            ///  Send data to the device.
            /// </summary>
            /// <param name="buffer">Buffer containing data.</param>
            /// <param name="start">Start index of data in buffer.</param>
            /// <param name="length">Length of data.</param>
            /// <param name="token">Token used to cancel the send request.</param>
            /// <returns>A task to send data to the device.</returns>
            public override Task SendDataAsync(byte[] buffer, int start, int length, CancellationToken token)
            {
                if (length >= int.MaxValue - MX925TcpHeaderLength)
                {
                    throw new InvalidOperationException("Invalid length");
                }
    
                // TCP packets have a 2-byte message length header.
                byte[] envelope = new byte[length + MX925TcpHeaderLength];
                envelope[0] = (byte)(length / 256);
                envelope[1] = (byte)(length % 256);
                Array.Copy(buffer, start, envelope, MX925TcpHeaderLength, length);
    
                return base.SendDataAsync(envelope, 0, envelope.Length, token);
            }
    
            /// <summary>
            ///  Receives data from the device.
            /// </summary>
            /// <param name="buffer">Buffer to use to store data.</param>
            /// <param name="start">Start index of where to write data in buffer.</param>
            /// <param name="length">Length of data to read.</param>
            /// <param name="token">Token used to cancel the receive request.</param>
            /// <returns>A task to receive data from the device.</returns>
            public override async Task<int> ReceiveDataAsync(byte[] buffer, int start, int length, CancellationToken token)
            {
                var responseBuffer = new byte[length + MX925TcpHeaderLength];
                int messageLength = await base.ReceiveDataAsync(responseBuffer, start, length + MX925TcpHeaderLength, token);
                Array.Copy(responseBuffer, MX925TcpHeaderLength, buffer, 0, messageLength - MX925TcpHeaderLength);
    
                int expectedLength = (responseBuffer[0] * 256) + responseBuffer[1];
                int actualLength = messageLength - MX925TcpHeaderLength;
                if (messageLength <= MX925TcpHeaderLength || actualLength != expectedLength)
                {
                    throw new InvalidOperationException("Invalid message received.");
                }
    
                return actualLength;
            }
        }
    }
}
