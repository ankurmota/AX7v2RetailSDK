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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols
    {
        using System;
        using System.Text;
    
        /// <summary>
        ///  Contains helper methods for creating / validating device envelopes.
        /// </summary>
        public static class ProtocolUtilities
        {
            private const int EmptyEnvelopeLength = 3;
    
            /// <summary>
            ///  Creates an envelope from the command string.
            /// </summary>
            /// <param name="commandValue">Command to send.</param>
            /// <returns>Byte array containing envelope.</returns>
            public static byte[] CreateEnvelope(string commandValue)
            {
                if (string.IsNullOrEmpty(commandValue))
                {
                    throw new ArgumentNullException("commandValue");
                }
    
                byte[] command = Encoding.Default.GetBytes(commandValue);
    
                byte[] envelope = new byte[command.Length + EmptyEnvelopeLength];
    
                envelope[0] = ProtocolConstants.StartMessage;
                Array.Copy(command, 0, envelope, 1, command.Length);
                envelope[command.Length + 1] = ProtocolConstants.EndMessage;
                envelope[command.Length + 2] = ProtocolUtilities.GetLrc(command);
    
                return envelope;
            }
    
            /// <summary>
            ///  Validates the envelope format is valid and the LRC matches.
            /// </summary>
            /// <param name="envelope">Buffer containing message envelope.</param>
            /// <param name="length">Length of envelope in buffer.</param>
            /// <returns>True if valid, false otherwise.</returns>
            public static bool ValidateEnvelope(byte[] envelope, int length)
            {
                if (envelope == null)
                {
                    throw new ArgumentNullException("envelope");
                }
    
                if (envelope.Length <= 3 || length < 3)
                {
                    return false;
                }
    
                // Ignore STX, ETX and LRC bytes.
                byte lrc = ProtocolUtilities.GetLrc(envelope, 1, length - 3);
                return lrc == envelope[length - 1] && envelope[0] == ProtocolConstants.StartMessage && envelope[length - 2] == ProtocolConstants.EndMessage;
            }
    
            /// <summary>
            ///  Calculates the LRC for a command.
            /// </summary>
            /// <param name="command">Command bytes without STX/ETX bits.</param>
            /// <returns>Checksum byte.</returns>
            private static byte GetLrc(byte[] command)
            {
                return GetLrc(command, 0, command.Length);
            }
    
            /// <summary>
            ///  Calculates the LRC for a command.
            /// </summary>
            /// <param name="buffer">Buffer containing command.</param>
            /// <param name="start">Start of command in buffer after STX byte.</param>
            /// <param name="length">Length of command not including ETX byte.</param>
            /// <returns>Checksum byte.</returns>
            private static byte GetLrc(byte[] buffer, int start, int length)
            {
                byte lrc = 0;
    
                for (int i = start; i < start + length; i++)
                {
                    lrc ^= buffer[i];
                }
    
                return (byte)(lrc ^ ProtocolConstants.EndMessage);
            }
        }
    }
}
