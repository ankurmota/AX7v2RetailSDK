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
    namespace Commerce.HardwareStation.Peripherals
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Text.RegularExpressions;
    
        /// <summary>
        /// Parses a receipt into separate regions of content for printing.
        /// </summary>
        public sealed class TextLogoParser
        {
            private const string LogoStart = "<L:";
            private const string LogoEnd = ">";
            private const string LogoRegex = @"(?<logo>\<L\:[A-Za-z0-9\+\/\=]*?\>)|(?<legacyLogo>\<L\>)";
    
            /// <summary>
            /// Prevents a default instance of the TextLogoParser class from being created.
            /// </summary>
            private TextLogoParser()
            {
            }
    
            /// <summary>
            /// Extracts the base64 encoded images bytes from a tag with &lt;L:base64&gt;.
            /// </summary>
            /// <param name="logoText">Logo tag containing Base64-encoded image data.</param>
            /// <returns>Bytes extracted from the logo image data.</returns>
            public static byte[] GetLogoImageBytes(string logoText)
            {
                if (!string.IsNullOrWhiteSpace(logoText) && logoText.StartsWith(LogoStart, StringComparison.Ordinal) && logoText.EndsWith(LogoEnd, StringComparison.Ordinal))
                {
                    string base64 = logoText.Substring(LogoStart.Length, logoText.Length - LogoStart.Length - LogoEnd.Length);
                    byte[] image = Convert.FromBase64String(base64);
                    return image;
                }
    
                return new byte[] { };
            }
    
            /// <summary>
            /// Splits a string into parts that are text, and parts that are embedded logos of the form
            /// &lt;L:Base64&gt;.
            /// </summary>
            /// <param name="text">Text to parse.</param>
            /// <returns>List of either text or logo strings.</returns>
            public static ReadOnlyCollection<TextPart> Parse(string text)
            {
                List<TextPart> textParts = new List<TextPart>();
                if (string.IsNullOrEmpty(text))
                {
                    textParts.Add(new TextPart(TextType.Text, text));
                    return textParts.AsReadOnly();
                }
    
                List<Group> logos = new List<Group>();
                Regex.Replace(
                    text,
                    LogoRegex,
                    (match) =>
                    {
                        if (null != match && match.Success && match.Groups != null)
                        {
                            var group = match.Groups["logo"];
                            if (null == group || false == group.Success)
                            {
                                group = match.Groups["legacyLogo"];
                            }
    
                            if (null != group && group.Success)
                            {
                                logos.Add(group);
                            }
                        }
    
                        return null;
                    });
    
                int index = 0;
                foreach (var logo in logos)
                {
                    if (logo.Index > index)
                    {
                        textParts.Add(new TextPart(TextType.Text, text.Substring(index, logo.Index - index)));
                        AddLogo(textParts, logo);
                    }
                    else
                    {
                        AddLogo(textParts, logo);
                    }
    
                    index = logo.Index + logo.Length;
                }
    
                if (index < text.Length)
                {
                    textParts.Add(new TextPart(TextType.Text, text.Substring(index, text.Length - index)));
                }
    
                return textParts.AsReadOnly();
            }
    
            private static void AddLogo(List<TextPart> textParts, Group logo)
            {
                textParts.Add(new TextPart(logo.Value == "<L>" ? TextType.LegacyLogo : TextType.LogoWithBytes, logo.Value));
            }
        }
    }
}
