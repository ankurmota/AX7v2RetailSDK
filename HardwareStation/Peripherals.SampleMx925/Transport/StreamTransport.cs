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
        using System.Diagnostics;
        using System.IO;
        using System.Threading;
        using System.Threading.Tasks;
    
        /// <summary>
        ///  Base class for transport layers which provide a stream.
        /// </summary>
        public abstract class StreamTransport : ITransport
        {
            /// <summary>
            ///  Retry count for sending / receiving data.
            /// </summary>
            protected const int RetryCount = 3;
            private bool disposed;
    
            /// <summary>
            ///  Gets or sets the stream to use when sending/receiving data.
            /// </summary>
            protected Stream Stream { get; set; }
    
            /// <summary>
            ///  Gets a value indicating whether the transport is connected.
            /// </summary>
            protected abstract bool IsConnected { get; }
    
            /// <summary>
            ///  Send data to the device.
            /// </summary>
            /// <param name="buffer">Buffer containing data.</param>
            /// <param name="start">Start index of data in buffer.</param>
            /// <param name="length">Length of data.</param>
            /// <param name="token">Token used to cancel the send request.</param>
            /// <returns>A task to send data to the device.</returns>
            public virtual async Task SendDataAsync(byte[] buffer, int start, int length, CancellationToken token)
            {
                await this.ExecuteWithRetry(() => this.Stream.WriteAsync(buffer, start, length, token).ContinueWith(task => Task.FromResult(true)), token);
            }
    
            /// <summary>
            ///  Receives data from the device.
            /// </summary>
            /// <param name="buffer">Buffer to use to store data.</param>
            /// <param name="start">Start index of where to write data in buffer.</param>
            /// <param name="length">Length of data to read.</param>
            /// <param name="token">Token used to cancel the receive request.</param>
            /// <returns>A task to receive data from the device.</returns>
            public virtual Task<int> ReceiveDataAsync(byte[] buffer, int start, int length, CancellationToken token)
            {
                return this.ExecuteWithRetry(() => this.Stream.ReadAsync(buffer, start, length, token), token);
            }
    
            /// <summary>
            ///  Closes the connection.
            /// </summary>
            /// <returns>A task to close the connection to the device.</returns>
            public virtual Task CloseAsync()
            {
                return Task.Run(() =>
                {
                    if (this.Stream != null)
                    {
                        this.Stream.Close();
                        this.Stream = null;
                    }
                });
            }
    
            /// <summary>
            ///  ConnectAsync to the device.
            /// </summary>
            /// <returns>A task to connect to the device.</returns>
            public abstract Task ConnectAsync();
    
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
                if (!this.disposed)
                {
                    if (disposing)
                    {
                        this.CloseAsync().Wait();
                    }
                }
    
                this.disposed = true;
            }
    
            /// <summary>
            ///  Executes a method with retry logic.
            /// </summary>
            /// <typeparam name="T">Return type.</typeparam>
            /// <param name="method">Method to execute.</param>
            /// <param name="token">Token used to cancel the receive request.</param>
            /// <returns>A task that can be awaited until execution completes.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Need to return a generic task.")]
            protected async Task<T> ExecuteWithRetry<T>(Func<Task<T>> method, CancellationToken token)
            {
                int count = 0;
                do
                {
                    try
                    {
                        if (!this.IsConnected)
                        {
                            await this.CloseAsync();
                            await this.ConnectAsync();
                        }
    
                        return await method();
                    }
                    catch (IOException)
                    {
                        // Serial port does not support cancellation but will instead throw an IO Exception when closing.
                        if (count == RetryCount)
                        {
                            throw;
                        }
    
                        token.ThrowIfCancellationRequested();
    
                        count++;
                    }
                }
                while (true);
            }
        }
    }
}
