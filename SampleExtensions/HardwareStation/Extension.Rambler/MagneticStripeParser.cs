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
        using System;
        using System.Text;

        /// <summary>
        /// Enumerates supported magnetic card tracks.
        /// </summary>
        public enum MagneticTrackKind
        {
            /// <summary>
            /// The track1 data of the magnetic swipe reader.
            /// </summary>
            Track1,

            /// <summary>
            /// The track2 data of the magnetic swipe reader.
            /// </summary>
            Track2
        }

        /// <summary>
        /// Provides methods for extraction of card data from the raw magnetic card track.
        /// </summary>
        public static class MagneticStripeParser
        {
            private const char Track1StartSentinel = '%';
            private const char Track1EndSentinel = '?';

            private const char Track2StartSentinel = ';';
            private const char Track2EndSentinel = '?';

            private const char Track1StartIndexCardNumber = '^';
            private const char Track2StartIndexCardNumber = '=';

            private const int CardNumberMaxLength = 19;

            /// <summary>
            /// Extracts payment card track of given kind from a given raw magnetic stipe track.
            /// </summary>
            /// <param name="rawTrack">The raw magnetic stripe track.</param>
            /// <param name="offset">The offset in the raw track.</param>
            /// <param name="length">The length of the track.</param>
            /// <param name="trackKind">The kind of the track.</param>
            /// <param name="excludeChecksum">True to exclude trailing checksum from the extracted track; otherwise - false.</param>
            /// <returns>The extracted track.</returns>
            public static string ExtractTrack(byte[] rawTrack, int offset, int length, MagneticTrackKind trackKind, bool excludeChecksum = true)
            {
                if (rawTrack == null)
                {
                    throw new ArgumentNullException("rawTrack");
                }

                if (length == 0)
                {
                    return null;
                }
                
                var track = AsciiStringConverter.Ascii8BitToString(rawTrack, offset, length);

                CheckTrack(track, trackKind, !excludeChecksum);

                return track;
            }

            /// <summary>
            /// Extracts card number from a given track 1.
            /// </summary>
            /// <param name="track">The track of a payment card.</param>
            /// <param name="trackKind">The kind of the track.</param>
            /// <returns>The extracted card number.</returns>
            /// <remarks>
            /// Only format B for track 1 is supported.
            /// </remarks>
            public static string ExtractCardNumber(string track, MagneticTrackKind trackKind)
            {
                if (track == null)
                {
                    throw new ArgumentNullException("track");
                }

                CheckTrack(track, trackKind);

                switch (trackKind)
                {
                    case MagneticTrackKind.Track1:
                        return ExtractTrack1BCardNumber(track);

                    case MagneticTrackKind.Track2:
                        return ExtractTrack2CardNumber(track);

                    default:
                        throw new NotSupportedException(string.Format("Retrieval of card number from track '{0}' not supported.", trackKind));
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "length--", Justification = "Fxcop errors in gates even if the input is validated.")]
            private static void CheckTrack(string track, MagneticTrackKind trackKind, bool containsChecksum = false)
            {
                int length = track.Length;

                if (containsChecksum)
                {                    
                    length--;
                }

                char sentineStart;
                char sentineEnd;

                switch (trackKind)
                {
                    case MagneticTrackKind.Track1:
                        sentineStart = MagneticStripeParser.Track1StartSentinel;
                        sentineEnd = MagneticStripeParser.Track1EndSentinel;
                        break;

                    case MagneticTrackKind.Track2:
                        sentineStart = MagneticStripeParser.Track2StartSentinel;
                        sentineEnd = MagneticStripeParser.Track2EndSentinel;
                        break;

                    default:
                        throw new NotSupportedException(string.Format("Retrieval of track '{0}' not supported.", trackKind));
                }

                // Checking for track sentines
                if (length < 2 || track[0] != sentineStart || track[length - 1] != sentineEnd)
                {
                    throw new FormatException("The track doesn't have start and end sentines.");
                }
            }

            private static string ExtractTrack1BCardNumber(string track)
            {
                int offsetExclSentines = 1;
                int lengthExclSentines = track.Length - 2;

                // Checking for supported track format
                if (lengthExclSentines < 1)
                {
                    throw new FormatException("The track doesn't have format identfier.");
                }

                char trackFormat = track[offsetExclSentines];

                // Other formats are proprietary.
                if (trackFormat != 'B')
                {
                    throw new NotSupportedException(string.Format("The format of track '{0}' not supported.", trackFormat));
                }

                int cardNumberOffset = offsetExclSentines + 1;
                int cardNumberSearchLength = Math.Min(lengthExclSentines - 1, CardNumberMaxLength + 1);

                // Finding card number
                int fieldSeparator = track.IndexOf(MagneticStripeParser.Track1StartIndexCardNumber, cardNumberOffset, cardNumberSearchLength);

                // Card number not found or zero digits.
                if (fieldSeparator < cardNumberOffset + 1)
                {
                    throw new FormatException("The track doesn't have card number.");
                }

                // Extracting card number
                var cardNumber = new StringBuilder(fieldSeparator - cardNumberOffset);

                for (int i = cardNumberOffset; i < fieldSeparator; i++)
                {
                    var ch = track[i];

                    if (ch >= '0' && ch <= '9')
                    {
                        // Adding digits to card number
                        cardNumber.Append(ch);
                        continue;
                    }

                    if (ch == ' ')
                    {
                        // Skipping spaces
                        continue;
                    }

                    throw new FormatException("The card number has invalid characters.");
                }

                if (cardNumber.Length == 0)
                {
                    throw new FormatException("The card number is empty.");
                }

                return cardNumber.ToString();
            }

            private static string ExtractTrack2CardNumber(string track)
            {
                int offsetExclSentines = 1;
                int lengthExclSentines = track.Length - 2;

                int cardNumberOffset = offsetExclSentines;
                int cardNumberSearchLength = Math.Min(lengthExclSentines, CardNumberMaxLength + 1);

                // Finding card number
                int fieldSeparator = track.IndexOf(MagneticStripeParser.Track2StartIndexCardNumber, cardNumberOffset, cardNumberSearchLength);

                // Card number not found or zero digits.
                if (fieldSeparator < cardNumberOffset + 1)
                {
                    throw new FormatException("The track doesn't have card number.");
                }

                // Extracting card number
                string cardNumber = track.Substring(cardNumberOffset, fieldSeparator - cardNumberOffset);
                
                return cardNumber;
            }
        }
    }
}