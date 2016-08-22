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
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;

        /// <summary>
        /// The rambler magnetic swipe card data entity class.
        /// This class parses and loads the card properties in <see cref="MagneticCardSwipeInfo"/>. entity.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "DR1717:AssertTypesAreInExpectedNamespace", Justification = "The namespace is set different for retail Sdk classes.")]
        public class RamblerMagneticStripeCardData : MagneticCardSwipeInfo
        {
            private const string RamblerFormatId = "formatID";
            private const string RamblerEncodedTrack = "encTrack";
            private const string RamblerTrack1Length = "track1Length";
            private const string RamblerTrack2Length = "track2Length";
            private const string RamblerCardNumberMaskedField = "maskedPAN";
            private const string RamblerCardholderField = "cardholderName";
            private const string RamblerExpirationDateField = "expiryDate";

            private const int RamblerFormatIdUnencrypted = 32;
            private const int RamblerFormatIdEncrypted = 38;

            /// <summary>
            /// Gets or sets a value indicating whether the card is encrypted.
            /// </summary>
            public bool IsEncrypted { get; set; }

            /// <summary>
            /// Gets or sets the Encrypted track if the card data is in encrypted format.
            /// </summary>
            public string EncryptedTrack { get; set; }

            /// <summary>
            /// Parses data received from the Rambler device and returns filled up card <see cref="MagneticCardSwipeInfo"/>.
            /// </summary>
            /// <param name="cardData">The Rambler data structure with data of swiped card.</param>
            /// <exception cref="ArgumentNullException">Throws if decodeData is null.</exception>
            /// <exception cref="FormatException">Throws if the card data is invalid.</exception>
            /// <exception cref="NotSupportedException">Throws if the card format not supported.</exception>
            /// <remarks>
            /// For compliance with Windows Store Runtime, returns all not present fields as empty strings.
            /// </remarks>
            public void ParseCard(IDictionary<string, string> cardData)
            {
                if (cardData == null)
                {
                    throw new ArgumentNullException("cardData");
                }

                string track1;
                string track2;
                bool encrypted;
                string encryptedTrack;

                ExtractTracks(cardData, out track1, out track2, out encrypted, out encryptedTrack);

                this.IsEncrypted = encrypted;
                this.EncryptedTrack = encryptedTrack;

                this.ParseTracks(track1, track2);
            }

            private static void ExtractTracks(IDictionary<string, string> decodeData, out string track1, out string track2, out bool encrypted, out string encryptedTrack)
            {
                track1 = null;
                track2 = null;
                encrypted = false;
                encryptedTrack = null;

                int formatId = GetFieldAsInt(decodeData, RamblerFormatId);
                var encodedTrack = GetFieldAsString(decodeData, RamblerEncodedTrack);

                if (formatId == RamblerFormatIdUnencrypted)
                {
                    // If the card is unencrypted, decoding track
                    var track1Length = GetFieldAsInt(decodeData, RamblerTrack1Length);
                    var track2Length = GetFieldAsInt(decodeData, RamblerTrack2Length);

                    try
                    {
                        var rawTrackBytes = HexStringConverter.ToByteArray(encodedTrack, true);

                        if (track1Length > 0)
                        {
                            track1 = MagneticStripeParser.ExtractTrack(rawTrackBytes, 0, track1Length - 1, MagneticTrackKind.Track1);
                        }

                        if (track2Length > 0)
                        {
                            track2 = MagneticStripeParser.ExtractTrack(rawTrackBytes, track1Length, track2Length - 1, MagneticTrackKind.Track2);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new FormatException("The card has invalid track.", ex);
                    }
                }
                else if (formatId == RamblerFormatIdEncrypted)
                {
                    encrypted = true;
                    encryptedTrack = encodedTrack;
                }
                else
                {
                    throw new NotSupportedException(string.Format("The Rambler data format '{0}' not supported.", formatId));
                }
            }

            private static string GetFieldAsString(IDictionary<string, string> decodeData, string fieldName, bool requiredNotEmpty = true)
            {
                if (!decodeData.ContainsKey(fieldName))
                {
                    throw new FormatException(string.Format("The Rambler data doesn't have field '{0}'.", fieldName));
                }

                string value = decodeData[fieldName];

                if (value != null)
                {
                    // Trim trailing spaces
                    value = value.TrimEnd(' ');

                    if (value.Length == 0)
                    {
                        value = null;
                    }
                }

                if (requiredNotEmpty && value == null)
                {
                    throw new FormatException(string.Format("The Rambler data has empty field '{0}'.", fieldName));
                }

                return value;
            }

            private static int GetFieldAsInt(IDictionary<string, string> decodeData, string fieldName)
            {
                string str = GetFieldAsString(decodeData, fieldName);

                int value;

                if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    throw new FormatException(string.Format("The Rambler data has invalid field '{0}'.", fieldName));
                }

                return value;
            }
        }
    }
}