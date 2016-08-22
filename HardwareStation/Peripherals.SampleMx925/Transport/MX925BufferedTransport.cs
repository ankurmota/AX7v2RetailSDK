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
    
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols;
    
        /// <summary>
        ///  Transport class to implement a <c>VeriFone</c> specific transport.
        /// </summary>
        public class MX925BufferedTransport : ITransport
        {
            private readonly byte[] responseBuffer = new byte[VerifoneDeviceProtocol.DefaultBufferSize * 4];
            private readonly ITransport transport;
            private int responseStart;
            private int totalBytesReceived;
    
            /// <summary>
            ///  Initializes a new instance of the <see cref="MX925BufferedTransport" /> class.
            /// </summary>
            /// <param name="transport">TCP transport class to wrap.</param>
            public MX925BufferedTransport(ITransport transport)
            {
                this.transport = transport;
            }
    
            /// <summary>
            ///  Gets or sets the length of the header for this transport.
            /// </summary>
            public int HeaderLength { get; protected set; }
    
            /// <summary>
            ///  Connect to the device.
            /// </summary>
            /// <returns>A task that connects to the device.</returns>
            public Task ConnectAsync()
            {
                return this.transport.ConnectAsync();
            }
    
            /// <summary>
            ///  Closes the connection.
            /// </summary>
            /// <returns>A task that closes the connection to the device.</returns>
            public Task CloseAsync()
            {
                return this.transport.CloseAsync();
            }
    
            /// <summary>
            ///  Send data to the device.
            /// </summary>
            /// <param name="buffer">Buffer containing data.</param>
            /// <param name="start">Start index of data in buffer.</param>
            /// <param name="length">Length of data.</param>
            /// <param name="token">Token used to cancel the send request.</param>
            /// <returns>A task to send data to the device.</returns>
            public virtual Task SendDataAsync(byte[] buffer, int start, int length, CancellationToken token)
            {
                return this.transport.SendDataAsync(buffer, start, length, token);
            }
    
            /// <summary>
            ///  Receives data from the device.
            /// </summary>
            /// <param name="buffer">Buffer to use to store data.</param>
            /// <param name="start">Start index of where to write data in buffer.</param>
            /// <param name="length">Length of data to read.</param>
            /// <param name="token">Token used to cancel the receive request.</param>
            /// <returns>A task to receive data from the device.</returns>
            public virtual async Task<int> ReceiveDataAsync(byte[] buffer, int start, int length, CancellationToken token)
            {
                if (this.responseBuffer.Length < this.responseStart + length)
                {
                    throw new InvalidOperationException("Not enough buffer");
                }
    
                do
                {
                    int messageLength = this.ExtractMessage(buffer);
                    if (messageLength != 0)
                    {
                        return messageLength;
                    }
    
                    int bytesRecieved = await this.transport.ReceiveDataAsync(this.responseBuffer, this.responseStart, length, token);
                    this.totalBytesReceived = this.responseStart + bytesRecieved;
                }
                while (true);
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
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.transport.Dispose();
                }
            }
    
            /// <summary>
            ///  Extract message from the stored response buffer.
            /// </summary>
            /// <param name="buffer">Buffer to copy the stored message to.</param>
            /// <returns>Message length.</returns>
            private int ExtractMessage(byte[] buffer)
            {
                if (this.totalBytesReceived == 0)
                {
                    return 0;
                }
    
                int messageLength = ResponseMessage.GetPacketLength(this.responseBuffer, this.HeaderLength, this.totalBytesReceived);
    
                if (messageLength == this.totalBytesReceived)
                {
                    Array.Copy(this.responseBuffer, 0, buffer, 0, messageLength);
                    this.responseStart = 0;
                    this.totalBytesReceived = 0;
                    return messageLength;
                }
                else if (messageLength < this.totalBytesReceived)
                {
                    int packetLength = messageLength;
                    Array.Copy(this.responseBuffer, 0, buffer, 0, messageLength);
                    Array.Copy(this.responseBuffer, packetLength, this.responseBuffer, 0, this.totalBytesReceived - packetLength);
                    this.responseStart = packetLength;
                    this.totalBytesReceived -= packetLength;
                    return messageLength;
                }
                else
                {
                    this.responseStart = this.totalBytesReceived;
                }
    
                return 0;
            }
        }
    }
}
