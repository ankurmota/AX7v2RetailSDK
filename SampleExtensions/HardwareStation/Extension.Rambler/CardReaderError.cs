/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.HardwareStation.RamblerSample
    {
        /// <summary>
        /// Enumerates possible error types from a card reader device.
        /// </summary>
        public enum CardReaderError
        {
            /// <summary>
            /// Other (uncategorized) error.
            /// Error message has description of the error.
            /// </summary>
            Error,

            /// <summary>
            /// The swipe failed.
            /// This could be caused by very fast/slow swipes, or partial swipes,
            /// There could be a problem with the card itself.
            /// </summary>
            SwipeFailed,

            /// <summary>
            /// The CRC checksum is invalid.
            /// This could be caused by broken card. 
            /// </summary>
            CardCrcInvalid,

            /// <summary>
            /// Format of the card is invalid or not supported.
            /// This could be caused by broken card or format of card is unknown.
            /// </summary>
            CardFormatInvalid
        }
    }
}