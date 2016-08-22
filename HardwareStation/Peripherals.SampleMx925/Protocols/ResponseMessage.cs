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
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
    
        /// <summary>
        ///  Class to hold/parse the response message from the device.
        /// </summary>
        public class ResponseMessage
        {
            /// <summary>
            ///  Gets a value indicating whether the message is valid.
            /// </summary>
            public bool IsValid { get; private set; }
    
            /// <summary>
            ///  Gets a value indicating whether the message is an <c>ack</c>.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ack", Justification = "Packet name.")]
            public bool IsAck { get; private set; }
    
            /// <summary>
            ///  Gets a value indicating whether the message is a <c>nak</c>.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nak", Justification = "Packet name.")]
            public bool IsNak { get; private set; }
    
            /// <summary>
            ///  Gets the message command.
            /// </summary>
            public string Command { get; private set; }
    
            /// <summary>
            ///  Gets the message response parameters.
            /// </summary>
            public IList<string> ResponseValues { get; private set; }
    
            /// <summary>
            ///  Gets the raw buffer.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Data buffer")]
            public byte[] RawData { get; private set; }
    
            /// <summary>
            ///  Parse a message from a buffer.
            /// </summary>
            /// <param name="data">Buffer containing message.</param>
            /// <param name="length">Length of message in buffer.</param>
            /// <returns>A message object.</returns>
            public static ResponseMessage Parse(byte[] data, int length)
            {
                if (data == null)
                {
                    throw new ArgumentNullException("data");
                }
    
                var message = new ResponseMessage();
                if (data[0] == ProtocolConstants.Ack)
                {
                    message.IsAck = true;
                    message.IsValid = true;
                    return message;
                }
    
                if (data[0] == ProtocolConstants.Nak)
                {
                    message.IsNak = true;
                    message.IsValid = true;
                    return message;
                }
    
                message.IsValid = ProtocolUtilities.ValidateEnvelope(data, length);
                if (!message.IsValid)
                {
                    return message;
                }
    
                // Handle signature data packets which do not have a field seperator.
                if (HeaderEquals(data, 0, ProtocolCommands.SignatureStart))
                {
                    // Format is "S01xxxxxyyyyzerrr"
                    message.Command = Encoding.Default.GetString(data, 1, 3);
                    message.ResponseValues = new[]
                    {
                        Encoding.Default.GetString(data, 4, 5),
                        Encoding.Default.GetString(data, 9, 4),
                        Encoding.Default.GetString(data, 13, 1),
                        Encoding.Default.GetString(data, 14, 1),
                        Encoding.Default.GetString(data, 15, 3)
                    };
                }
                else if (HeaderEquals(data, 0, ProtocolCommands.SignatureDataReceived))
                {
                    // Format is "S02Muuuu{sigdata}"
                    message.Command = Encoding.Default.GetString(data, 1, 3);
                    message.ResponseValues = new[]
                    {
                        Encoding.Default.GetString(data, 4, 1),
                        Encoding.Default.GetString(data, 5, 4)
                    };
                    message.RawData = data;
                }
                else if (HeaderEquals(data, 0, ProtocolCommands.CreditCardDataReceived))
                {
                    // Format is "81.<Track1><FS><Track2><FS><Track3><FS>M"
                    message.Command = Encoding.Default.GetString(data, 1, 3);
                    if (length > 6)
                    {
                        string responseString = Encoding.Default.GetString(data, 4, length - 6);
                        var responseValues = responseString.Split(new[] { ProtocolConstants.FieldSeparator }, StringSplitOptions.None);
                        message.ResponseValues = responseValues.ToArray();
                    }
                }
                else if (HeaderEquals(data, 0, ProtocolCommands.PinDataReceived))
                {
                    // Format is "73.00000KKKKKKKKKKKKKKKKKKKKPPPPPPPPPPPPPPPP"
                    message.Command = Encoding.Default.GetString(data, 1, 3);
                    if (length > 26)
                    {
                        string pinLength = Encoding.Default.GetString(data, 4, 5);
                        string key = Encoding.Default.GetString(data, 9, length - 27);
                        string pinData = Encoding.Default.GetString(data, length - 18, 16);
                        message.ResponseValues = new[] { pinLength, key, pinData };
                    }
                }
                else if (HeaderEquals(data, 0, ProtocolCommands.PinDataError1) || HeaderEquals(data, 0, ProtocolCommands.PinDataError2))
                {
                    message.Command = ProtocolCommands.PinDataReceived;
                    message.ResponseValues = new string[0];
                }
                else
                {
                    if (length > 3)
                    {
                        // Format is "<Command><FS><Response1><FS><Response2>..."
                        string responseString = Encoding.Default.GetString(data, 1, length - 3);
                        var responseValues = responseString.Split(new[] { ProtocolConstants.FieldSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        message.Command = responseValues[0];
                        message.ResponseValues = responseValues.Skip(1).ToArray();
                    }
                }
    
                message.RawData = data;
    
                return message;
            }
    
            /// <summary>
            ///  Gets the expected packet length for a given message.
            /// </summary>
            /// <param name="buffer">Buffer containing packet.</param>
            /// <param name="start">Start index in buffer.</param>
            /// <param name="end">Last byte received in buffer.</param>
            /// <returns>Length of expected message.</returns>
            public static int GetPacketLength(byte[] buffer, int start, int end)
            {
                if (buffer == null || buffer.Length < end || start > end || start < 0)
                {
                    throw new InvalidOperationException("Invalid buffer.");
                }
    
                int expectedLength = end + 1;
    
                if (HeaderEquals(buffer, start, ProtocolCommands.SignatureDataReceived))
                {
                    // Format is "S02Muuuu{sigdata}"
                    string sigLength = Encoding.Default.GetString(buffer, 5 + start, 4);
                    if (int.TryParse(sigLength, out expectedLength))
                    {
                        // Add length of <STX>S02Muuuu<ETX><LRC>
                        expectedLength += start + 11;
                    }
                }
                else
                {
                    for (int i = start; i < end; i++)
                    {
                        if (buffer[i] == ProtocolConstants.Ack || buffer[i] == ProtocolConstants.Nak)
                        {
                            expectedLength = i + 1;
                            break;
                        }
                        else if (buffer[i] == ProtocolConstants.EndMessage)
                        {
                            expectedLength = i + 2;
                            break;
                        }
                    }
                }
    
                return expectedLength;
            }
    
            /// <summary>
            ///  Checks to see if the message header is equal to the value.
            /// </summary>
            /// <param name="data">Message buffer.</param>
            /// <param name="start">Start index of data.</param>
            /// <param name="value">Header value.</param>
            /// <returns>Whether the message header matches.</returns>
            private static bool HeaderEquals(byte[] data, int start, string value)
            {
                for (int i = start; i < value.Length + start; i++)
                {
                    if (data[i + 1] != (byte)value[i - start])
                    {
                        return false;
                    }
                }
    
                return true;
            }
        }
    }
}
