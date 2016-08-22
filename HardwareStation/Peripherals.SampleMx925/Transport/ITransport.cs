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
        using System.Threading;
        using System.Threading.Tasks;
    
        /// <summary>
        ///  Interface for device transport.
        /// </summary>
        public interface ITransport : IDisposable
        {
            /// <summary>
            ///  Connect to the device.
            /// </summary>
            /// <returns>A task that connects to the device.</returns>
            Task ConnectAsync();
    
            /// <summary>
            ///  Closes the connection.
            /// </summary>
            /// <returns>A task that closes the connection to the device.</returns>
            Task CloseAsync();
    
            /// <summary>
            ///  Send data to the device.
            /// </summary>
            /// <param name="buffer">Buffer containing data.</param>
            /// <param name="start">Start index of data in buffer.</param>
            /// <param name="length">Length of data.</param>
            /// <param name="token">Token used to cancel the send request.</param>
            /// <returns>A task that sends data to the device.</returns>
            Task SendDataAsync(byte[] buffer, int start, int length, CancellationToken token);
    
            /// <summary>
            ///  Receives data from the device.
            /// </summary>
            /// <param name="buffer">Buffer to use to store data.</param>
            /// <param name="start">Start index of where to store data in buffer.</param>
            /// <param name="length">Length of data to read.</param>
            /// <param name="token">Token used to cancel the receive request.</param>
            /// <returns>A task that receives data from the device.</returns>
            Task<int> ReceiveDataAsync(byte[] buffer, int start, int length, CancellationToken token);
        }
    }
}
