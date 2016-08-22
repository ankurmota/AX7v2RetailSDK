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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Encapsulates the functionality required to fill an order number mask.
        /// </summary>
        internal static class OrderNumberMaskFiller
        {
            /// <summary>
            /// Apply the mask to the identifier that is passed in.
            /// </summary>
            /// <param name="identifier">The identifier to which the mask will be applied.</param>
            /// <param name="mask">The mask.</param>
            /// <param name="characterPool">The pool of characters that will be used for populating the non-"#" component of the order number mask.</param>
            /// <returns>The filled out mask.</returns>
            public static string ApplyMask(string identifier, string mask, string characterPool)
            {
                StringBuilder orderNumber = new StringBuilder();
                string filteredCharacterPool = FilterAlphaNumericCharacters(characterPool);
                int nextCharacterPoolIndex = 0;
    
                for (int i = 0; i < mask.Length; i++)
                {
                    char characterToAppend;
    
                    switch (mask[i])
                    {
                        case '#':
                            characterToAppend = identifier[i];
                            break;
                        case '@':
                            if (nextCharacterPoolIndex > filteredCharacterPool.Length)
                            {
                                characterToAppend = identifier[i];
                            }
                            else
                            {
                                characterToAppend = filteredCharacterPool[nextCharacterPoolIndex++];
                            }
    
                            break;
                        default:
                            characterToAppend = mask[i];
                            break;
                    }
    
                    orderNumber.Append(char.ToUpperInvariant(characterToAppend));
                }
    
                return orderNumber.ToString();
            }
    
            /// <summary>
            /// Returns a string that contains only letters or digits from the passed in character pool.
            /// </summary>
            /// <param name="characterPool">The pool of characters.</param>
            /// <returns>The filtered character pool.</returns>
            private static string FilterAlphaNumericCharacters(string characterPool)
            {
                StringBuilder filteredCharacterPool = new StringBuilder();
    
                if (!string.IsNullOrWhiteSpace(characterPool))
                {
                    for (int i = 0; i < characterPool.Length; i++)
                    {
                        if (char.IsLetterOrDigit(characterPool[i]))
                        {
                            filteredCharacterPool.Append(characterPool[i]);
                        }
                    }
                }
    
                return filteredCharacterPool.ToString();
            }
        }
    }
}
