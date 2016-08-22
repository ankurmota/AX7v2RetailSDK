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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols
    {
        using System.Globalization;
    
        /// <summary>
        /// This class has all the special characters needed by <c>VeriFone</c> MX925 REQ and RSP sequence.
        /// </summary>
        public static class ProtocolConstants
        {
            /// <summary>
            ///     Byte constant definition from MX925 device, ACK byte.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ack", Justification = "Packet name.")]
            public const byte Ack = 6;
    
            /// <summary>
            ///     String constant to signify adding a list box item to the end of the list.
            /// </summary>
            public const string DoNotCache = "0";
    
            /// <summary>
            ///     Ending byte of an input or output sequence (ETX).
            /// </summary>
            public const byte EndMessage = 3;
    
            /// <summary>
            ///     String constant to signify adding a list box item to the end of the list.
            /// </summary>
            public const string EndOfList = "-1";
    
            /// <summary>
            ///     Close connection.
            /// </summary>
            public const byte EndOfTransmission = 4;
    
            /// <summary>
            ///     Byte constant definition from MX925 device, separator char.
            /// </summary>
            public const byte FS = 28;
    
            /// <summary>
            ///     Byte constant definition from MX925 device, NAK byte.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nak", Justification = "Packet name.")]
            public const byte Nak = 21;
    
            /// <summary>
            ///     Starting byte of an input or output sequence (STX).
            /// </summary>
            public const byte StartMessage = 2;
    
            /// <summary>
            ///     Command response was success.
            /// </summary>
            public const string Success = "1";
    
            /// <summary>
            ///     String representation of FS byte.
            /// </summary>
            public static readonly string FieldSeparator = ((char)FS).ToString(CultureInfo.InvariantCulture);
        }
    }
}
