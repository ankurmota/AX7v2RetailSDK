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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Encapsulates the functionality required to fill a receipt mask.
        /// </summary>
        internal static class ReceiptMaskFiller
        {
            /// <summary>
            /// Extracts the number sequence value from the receipt mask.
            /// </summary>
            /// <param name="receiptMask">Template mask for receipt identifier.</param>
            /// <param name="receiptId">The receipt identifier to extract number sequence value.</param>
            /// <returns>Returns the number sequence value of the receipt.</returns>
            public static long GetNumberSequenceFromReceipt(string receiptMask, string receiptId)
            {
                ReadOnlyCollection<FormatBlock> formatBlocks = GetFormatBlocks(receiptMask, new[] { '#' });
    
                FormatBlock formatBlock = formatBlocks.SingleOrDefault();
    
                if (formatBlock != default(FormatBlock))
                {
                    string numberSequenceValue = receiptId.Substring(formatBlock.StartIndex, formatBlock.Length);
    
                    long numberSequence;
                    if (long.TryParse(numberSequenceValue, out numberSequence))
                    {
                        return numberSequence;
                    }
                }
                
                return 0L;
            }
    
            /// <summary>
            /// Fills in the template with mask and possible parameters.
            /// </summary>
            /// <param name="receiptMask">Template mask for receipt identifier.</param>
            /// <param name="seedValue">Receipt number, the sequential part of the receipt identifier.</param>
            /// <param name="storeId">The store identifier.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="currentDate">Date for receipt identifier.</param>
            /// <returns>
            /// Populated receipt identifier from mask.
            /// </returns>
            public static string FillMask(string receiptMask, string seedValue, string storeId, string terminalId, string staffId, DateTime currentDate)
            {
                string receiptId = receiptMask;
    
                // Get list of blocks in mask
                ReadOnlyCollection<FormatBlock> blockList = GetFormatBlocks(receiptId, new[] { 'S', 'T', 'C', '#', 'd', 'M', 'D', 'Y' });
    
                // For each block, fill with contents
                foreach (FormatBlock formatBlock in blockList)
                {
                    switch (formatBlock.BlockCharacter)
                    {
                        case 'S':
                            receiptId = FillBlock(receiptId, formatBlock, storeId);
                            break;
    
                        case 'T':
                            receiptId = FillBlock(receiptId, formatBlock, terminalId);
                            break;
    
                        case 'C':
                            receiptId = FillBlock(receiptId, formatBlock, staffId);
                            break;
    
                        case '#':
                            receiptId = FillBlock(receiptId, formatBlock, seedValue);
                            break;
    
                        case 'd':
                            // ddd is a valid block but dd isn't
                            if (formatBlock.Length >= 3)
                            {
                                receiptId = FillBlock(receiptId, formatBlock, currentDate.DayOfYear.ToString(CultureInfo.CurrentCulture));
                            }
    
                            break;
    
                        case 'D':
                            // DD is a valid block but D isn't
                            if (formatBlock.Length >= 2)
                            {
                                receiptId = FillBlock(receiptId, formatBlock, currentDate.Day.ToString(CultureInfo.CurrentCulture));
                            }
    
                            break;
    
                        case 'M':
                            if (formatBlock.Length >= 2)
                            {
                                receiptId = FillBlock(receiptId, formatBlock, currentDate.Month.ToString(CultureInfo.CurrentCulture));
                            }
    
                            break;
    
                        case 'Y':
                            if (formatBlock.Length >= 2)
                            {
                                receiptId = FillBlock(receiptId, formatBlock, currentDate.Year.ToString(CultureInfo.CurrentCulture));
                            }
    
                            break;
                    }
                }
    
                // if empty mask, default to seed number
                if (string.IsNullOrEmpty(receiptId))
                {
                    receiptId = seedValue;
                }
    
                return receiptId;
            }
    
            /// <summary>
            /// Given template mask string, and characters which indicated a formatted block, gets all formatted blocks.
            /// </summary>
            /// <param name="mask">Template mask string to search for format blocks.</param>
            /// <param name="formatChars">Array of characters valid for each format block type.</param>
            /// <returns>List of FormatBlocks containing character, start, and length of block in template.</returns>
            private static ReadOnlyCollection<FormatBlock> GetFormatBlocks(string mask, char[] formatChars)
            {
                List<FormatBlock> blockList = new List<FormatBlock>();

                int currentIdx = mask.IndexOfAny(formatChars);
                while (currentIdx < mask.Length && currentIdx >= 0)
                {
                    // Find the next template block
                    int startBlockIdx = currentIdx;
                    int endBlockIdx = FindEndOfBlock(mask, startBlockIdx);
                    int blockLength = endBlockIdx - startBlockIdx + 1;
    
                    // Add block to list
                    blockList.Add(new FormatBlock(mask[startBlockIdx], startBlockIdx, blockLength));
    
                    // Increment to next block start
                    currentIdx = mask.IndexOfAny(formatChars, endBlockIdx + 1);
                }
    
                return new ReadOnlyCollection<FormatBlock>(blockList);
            }
    
            /// <summary>
            /// Given a string, contents, and format block in the string, fills format block with contents.
            /// </summary>
            /// <param name="template">String with block to be filled.</param>
            /// <param name="formatBlock">The format of the block to be filled in the template string.</param>
            /// <param name="contents">String to put into the template block.</param>
            /// <returns>The fill block.</returns>
            private static string FillBlock(string template, FormatBlock formatBlock, string contents)
            {
                string filledTemplate = template;
    
                string trimmed = contents.PadLeft(formatBlock.Length, '0');
                trimmed = trimmed.Substring(trimmed.Length - formatBlock.Length);
    
                filledTemplate = filledTemplate.Remove(formatBlock.StartIndex, formatBlock.Length);
                filledTemplate = filledTemplate.Insert(formatBlock.StartIndex, trimmed);
    
                return filledTemplate;
            }
    
            /// <summary>
            /// Finds end of block of same character. Returns index of last occurrence of character in block.
            /// </summary>
            /// <param name="template">String to search.</param>
            /// <param name="startOfBlock">Index of beginning of block.</param>
            /// <returns>Returns index of last occurrence of character in block.</returns>
            private static int FindEndOfBlock(string template, int startOfBlock)
            {
                int currentIdx = startOfBlock;
                char blockChar = template[startOfBlock];
    
                while (currentIdx < template.Length &&
                    template[currentIdx] == blockChar)
                {
                    currentIdx += 1;
                }
    
                return currentIdx - 1;
            }
    
            /// <summary>
            /// Represents a formatting block.
            /// </summary>
            private struct FormatBlock
            {            
                /// <summary>
                /// Initializes a new instance of the <see cref="FormatBlock"/> struct.
                /// </summary>
                /// <param name="blockChar">The block character.</param>
                /// <param name="startIndex">The start index.</param>
                /// <param name="length">The length.</param>
                public FormatBlock(char blockChar, int startIndex, int length)
                    : this()
                {
                    this.BlockCharacter = blockChar;
                    this.StartIndex = startIndex;
                    this.Length = length;
                }
    
                /// <summary>
                /// Gets the block character.
                /// </summary>
                /// <value>
                /// The block character.
                /// </value>
                public char BlockCharacter { get; }
    
                /// <summary>
                /// Gets the start index.
                /// </summary>
                /// <value>
                /// The start index.
                /// </value>
                public int StartIndex { get; }
    
                /// <summary>
                /// Gets the length.
                /// </summary>
                /// <value>
                /// The length.
                /// </value>
                public int Length { get; }
    
                /// <summary>
                /// Overloads the equality of the two format block objects.
                /// </summary>
                /// <param name="fb1">The format block object 1.</param>
                /// <param name="fb2">The format block object 2.</param>
                /// <returns>Returns the boolean indicating whether the equality of format blocks.</returns>
                public static bool operator ==(FormatBlock fb1, FormatBlock fb2)
                {
                    return fb1 != null && fb1.Equals(fb2);
                }
    
                /// <summary>
                /// Overloads the non-equality of the two format block objects.
                /// </summary>
                /// <param name="fb1">The format block object 1.</param>
                /// <param name="fb2">The format block object 2.</param>
                /// <returns>Returns the boolean indicating whether the equality of format blocks.</returns>
                public static bool operator !=(FormatBlock fb1, FormatBlock fb2)
                {
                    return !(fb1 != null && fb1.Equals(fb2));
                }
                
                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj))
                    { 
                        return false; 
                    }
    
                    return obj is FormatBlock && this.Equals((FormatBlock)obj);
                }
    
                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hashCode = this.BlockCharacter.GetHashCode();
                        hashCode = (hashCode * 397) ^ this.StartIndex;
                        hashCode = (hashCode * 397) ^ this.Length;
                        return hashCode;
                    }
                }
    
                private bool Equals(FormatBlock other)
                {
                    return this.StartIndex == other.StartIndex && this.Length == other.Length;
                }
            }
        }
    }
}
