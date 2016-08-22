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
        using System.Text;
    
        /// <summary>
        /// This class has all the device commands needed by <c>VeriFone</c> MX925 request / response sequence.
        /// </summary>
        public static class ProtocolCommands
        {
            /// <summary>
            /// <c>InitForm</c> command header to the device.
            /// </summary>
            public const string InitForm = "XIFM";
    
            /// <summary>
            /// ShowForm command header to the device.
            /// </summary>
            public const string ShowForm = "XSFM";
    
            /// <summary>
            /// ShowForm command header to the device.
            /// </summary>
            public const string ShowIdleForm = "XZZZ";
    
            /// <summary>
            /// SetPropValue command header to the device.
            /// </summary>
            public const string SetPropValue = "XSPV";
    
            /// <summary>
            /// AddListBoxItem command header to the device.
            /// </summary>
            public const string AddListBoxItem = "XALI";
    
            /// <summary>
            /// ClearScreen command header to the device.
            /// </summary>
            public const string ClearScreen = "XCLS";
    
            /// <summary>
            /// Resets the terminal application state.
            /// </summary>
            public const string ResetTerminalState = "72";
    
            /// <summary>
            /// FormEvent command header to the device.
            /// </summary>
            public const string FormEvent = "XEVT";
    
            /// <summary>
            /// Requests the track2 data from the card reader.
            /// </summary>
            public const string EnableCreditCardTrack123 = "Q14";
    
            /// <summary>
            ///  Request pin entry screen to be displayed.
            /// </summary>
            public const string EnablePinPadCommandFormat = "Z60.{0}";
    
            /// <summary>
            /// Enables signature capture.
            /// </summary>
            public const string EnableSignatureCapture = "S00";
    
            /// <summary>
            ///  Sets the signature capture parameters.
            /// </summary>
            public const string SetSignatureCaptureParametersFormat = "S03{0:000}{1:000}00{2}00";
    
            /// <summary>
            ///  Sets the signature capture area.
            /// </summary>
            public const string SetSignatureCaptureAreaFormat = "S04{0:000}{1:000}{2:000}{3:000}0";
    
            /// <summary>
            /// Enables signature capture.
            /// </summary>
            public const string SignatureStart = "S01";
    
            /// <summary>
            ///  Event for signature data received.
            /// </summary>
            public const string SignatureDataReceived = "S02";
    
            /// <summary>
            ///  Event for credit card data received.
            /// </summary>
            public const string CreditCardDataReceived = "81.";
    
            /// <summary>
            ///  Event for pin data received.
            /// </summary>
            public const string PinDataReceived = "73.";
    
            /// <summary>
            ///  Event for pin data error.
            /// </summary>
            public static readonly string PinDataError1 = Encoding.UTF8.GetString(new[] { ProtocolConstants.StartMessage, ProtocolConstants.EndOfTransmission, ProtocolConstants.EndMessage });
    
            /// <summary>
            ///  Event for pin data error.
            /// </summary>
            public static readonly string PinDataError2 = Encoding.UTF8.GetString(new[] { ProtocolConstants.EndOfTransmission });
        }
    }
}
