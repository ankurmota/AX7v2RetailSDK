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
    /*
    SAMPLE CODE NOTICE
    
    THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
    OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
    THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
    NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
    */
    namespace Retail.SampleConnector.PaymentAcceptWeb.Utilities
    {
        using System;
        using System.Collections.Generic;
        using System.Text;
    
        /// <summary>
        /// Utility methods for card types.
        /// </summary>
        public static class CardTypes
        {
            /// <summary>
            /// American Express card.
            /// </summary>
            public const string Amex = "Amex";
    
            /// <summary>
            /// Discovery card.
            /// </summary>
            public const string Discover = "Discover";
    
            /// <summary>
            /// Master Card.
            /// </summary>
            public const string MasterCard = "MasterCard";
    
            /// <summary>
            /// VISA card.
            /// </summary>
            public const string Visa = "Visa";
    
            private static string[] supportedCardTypes = { Amex, Discover, MasterCard, Visa };
            private static char separator = ';';
    
            /// <summary>
            /// Converts a string of card types to an array.
            /// </summary>
            /// <param name="cardTypes">The input.</param>
            /// <returns>The output.</returns>
            public static string[] ToArray(string cardTypes)
            {
                string[] cardTypeArray = null;
                if (!string.IsNullOrWhiteSpace(cardTypes))
                {
                    cardTypeArray = cardTypes.Split(separator);
                }
    
                return cardTypeArray;
            }
    
            /// <summary>
            /// Converts an array of card types to a string.
            /// </summary>
            /// <param name="cardTypes">The input.</param>
            /// <returns>The output.</returns>
            public static string ToString(string[] cardTypes)
            {
                var sb = new StringBuilder();
                if (cardTypes != null)
                {
                    for (int i = 0; i < cardTypes.Length; i++)
                    {
                        sb.Append(cardTypes[i]);
    
                        if (i != cardTypes.Length - 1)
                        {
                            sb.Append(separator);
                        }
                    }
                }
    
                return sb.ToString();
            }
    
            /// <summary>
            /// Gets the supported card types based on a provided card type list. If no card type list is provided, return the full supported list.
            /// </summary>
            /// <param name="chosenCardTypes">The provided card types.</param>
            /// <returns>The supported card types.</returns>
            public static string GetSupportedCardTypes(string chosenCardTypes)
            {
                string[] chosenCardTypeArray = CardTypes.ToArray(chosenCardTypes);
    
                if (chosenCardTypeArray == null || chosenCardTypeArray.Length == 0)
                {
                    // Return the full list of supported card types
                    return CardTypes.ToString(CardTypes.supportedCardTypes);
                }
                else
                {
                    // Filter unsupported card types
                    var supportedChosenCardTypes = new List<string>();
    
                    foreach (var chosenCardType in chosenCardTypeArray)
                    {
                        foreach (var supportedCardType in CardTypes.supportedCardTypes)
                        {
                            if (supportedCardType.Equals(chosenCardType, StringComparison.OrdinalIgnoreCase))
                            {
                                supportedChosenCardTypes.Add(supportedCardType);
                            }
                        }
                    }
    
                    return CardTypes.ToString(supportedChosenCardTypes.ToArray());
                }
            }
        }
    }
}
