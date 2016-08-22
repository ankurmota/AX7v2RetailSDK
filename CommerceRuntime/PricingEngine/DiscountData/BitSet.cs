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
    namespace Commerce.Runtime.Services.PricingEngine.DiscountData
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
    
        /// <summary>
        /// Represents an array of bits that allows performing logical operations and optimized methods of testing for equality and finding the number of non-zero bits.
        /// This class is similar to the BitArray class, except that it can be tested for equality with another BitSet and with zero, and can track the first non-zero bit.
        /// </summary>
        internal class BitSet
        {
            /// <summary>
            /// Indicates that the next non-zero bit is unknown or does not exist.
            /// </summary>
            public const int UnknownBit = -1;
    
            private const int BitsInLong = sizeof(long) * 8;
            private const string BitSetSizesDoNotMatchOperationCannotBePerformed = "BitSet sizes do not match, the requested operation cannot be performed.";
    
            /// <summary>
            /// Static array of bit values for each bit position in a long, used for fast checking of whether or not a bit is set.
            /// </summary>
            private static ulong[] bitValues = new[]
                                                {
                                                    1UL, 1UL << 1, 1UL << 2, 1UL << 3, 1UL << 4, 1UL << 5, 1UL << 6, 1UL << 7,
                                                    1UL << 8, 1UL << 9, 1UL << 10, 1UL << 11, 1UL << 12, 1UL << 13, 1UL << 14, 1UL << 15,
                                                    1UL << 16, 1UL << 17, 1UL << 18, 1UL << 19, 1UL << 20, 1UL << 21, 1UL << 22, 1UL << 23,
                                                    1UL << 24, 1UL << 25, 1UL << 26, 1UL << 27, 1UL << 28, 1UL << 29, 1UL << 30, 1UL << 31,
                                                    1UL << 32, 1UL << 33, 1UL << 34, 1UL << 35, 1UL << 36, 1UL << 37, 1UL << 38, 1UL << 39,
                                                    1UL << 40, 1UL << 41, 1UL << 42, 1UL << 43, 1UL << 44, 1UL << 45, 1UL << 46, 1UL << 47,
                                                    1UL << 48, 1UL << 49, 1UL << 50, 1UL << 51, 1UL << 52, 1UL << 53, 1UL << 54, 1UL << 55,
                                                    1UL << 56, 1UL << 57, 1UL << 58, 1UL << 59, 1UL << 60, 1UL << 61, 1UL << 62, 1UL << 63
                                                };
    
            private uint size;
            private ulong[] values;
            private uint numberOfNonZeroBits;
            private int firstNonZeroBit;
            private bool areBitsKnown;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="BitSet"/> class.
            /// </summary>
            /// <param name="size">The number of bits in the BitSet.</param>
            public BitSet(uint size)
                : this(size, false)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="BitSet"/> class with the specified initial value.
            /// </summary>
            /// <param name="size">The number of bits in the BitSet.</param>
            /// <param name="initialValue">The value to set each initial bit to.</param>
            public BitSet(uint size, bool initialValue)
            {
                this.size = size;
                uint arrayLength = size / (uint)BitsInLong;
                uint extraIndex = 0;
    
                if (size % BitsInLong != 0)
                {
                    extraIndex = 1;
                }
    
                this.values = new ulong[arrayLength + extraIndex];
    
                if (initialValue)
                {
                    for (int x = 0; x < arrayLength; x++)
                    {
                        this.values[x] = ulong.MaxValue;
                    }
    
                    if (extraIndex == 1)
                    {
                        this.values[arrayLength] = ulong.MaxValue >> (BitsInLong - (int)(size % BitsInLong));
                    }
    
                    this.numberOfNonZeroBits = size;
                    this.firstNonZeroBit = 0;
                }
                else
                {
                    this.firstNonZeroBit = UnknownBit;
                }
    
                this.areBitsKnown = true;
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="BitSet"/> class with the specified initial values.
            /// </summary>
            /// <param name="initialValues">The initial values.</param>
            public BitSet(bool[] initialValues)
                : this((uint)initialValues.Length)
            {
                for (int x = 0; x < initialValues.Length; x++)
                {
                    if (initialValues[x])
                    {
                        this[x] = true;
                    }
                }
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="BitSet"/> class as a copy of the specified BitSet.
            /// </summary>
            /// <param name="initialValue">The BitSet to copy for this BitSet.</param>
            public BitSet(BitSet initialValue)
                : this(initialValue.size)
            {
                for (int x = 0; x < this.values.Length; x++)
                {
                    this.values[x] = initialValue.values[x];
                }
    
                this.areBitsKnown = initialValue.areBitsKnown;
                this.firstNonZeroBit = initialValue.firstNonZeroBit;
                this.numberOfNonZeroBits = initialValue.numberOfNonZeroBits;
            }
    
            /// <summary>
            /// Gets the length of the BitSet.
            /// </summary>
            public int Length
            {
                get
                {
                    return (int)this.size;
                }
            }
    
            /// <summary>
            /// Gets or sets the bit value at the specified position.
            /// </summary>
            /// <param name="x">The position of the bit.</param>
            /// <returns>True if the bit is set, false otherwise.</returns>
            public bool this[int x]
            {
                get
                {
                    if (x > this.size)
                    {
                        throw new ArgumentOutOfRangeException("x");
                    }
    
                    return (this.values[x / BitsInLong] & bitValues[x % BitsInLong]) != 0;
                }
    
                set
                {
                    if (x > this.size)
                    {
                        throw new ArgumentOutOfRangeException("x");
                    }
    
                    // Only change the value if the value is different than the current value, so that we can handle changing the number of non-zero bits and first non-zero bit.
                    if (this[x] != value)
                    {
                        if (value)
                        {
                            this.values[x / BitsInLong] |= bitValues[x % BitsInLong];
                            this.numberOfNonZeroBits++;
                            if (x < this.firstNonZeroBit || this.firstNonZeroBit == UnknownBit)
                            {
                                this.firstNonZeroBit = x;
                            }
                        }
                        else
                        {
                            this.values[x / BitsInLong] ^= bitValues[x % BitsInLong];
                            this.numberOfNonZeroBits--;
    
                            // If we are clearing the first non-zero bit, we no longer know the first bit, so we will reset the areBitsKnown flag and firstNonZeroBit value.
                            if (x == this.firstNonZeroBit)
                            {
                                this.areBitsKnown = false;
                                this.firstNonZeroBit = UnknownBit;
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// Gets the number of non-zero bits in the BitSet.  This method also finds the first non-zero bit for subsequent calls to GetFirstNonZeroBit().
            /// This method takes exactly 64 * ceil(size / 64.0) comparisons for the first run, and is O(1) for any subsequent calls.
            /// </summary>
            /// <returns>The number of non-zero bits.</returns>
            public int GetNumberOfNonzeroBits()
            {
                if (this.areBitsKnown)
                {
                    return (int)this.numberOfNonZeroBits;
                }
                else
                {
                    this.numberOfNonZeroBits = 0;
                    this.firstNonZeroBit = UnknownBit;
    
                    // Loop backwards so that the first non-zero bit will be the last one we encounter.
                    for (int x = this.values.Length - 1; x >= 0; x--)
                    {
                        for (int y = BitsInLong - 1; y >= 0; y--)
                        {
                            if ((this.values[x] & bitValues[y]) != 0)
                            {
                                this.numberOfNonZeroBits++;
                                this.firstNonZeroBit = (x * BitsInLong) + y;
                            }
                        }
                    }
    
                    this.areBitsKnown = true;
                    return (int)this.numberOfNonZeroBits;
                }
            }
    
            /// <summary>
            /// Gets the position of the first non-zero bit in the BitSet.  This method calls GetNumberOfNonzeroBits() internally if it has not already been called.
            /// </summary>
            /// <returns>The first non-zero bit, or UnknownBit if none exists.</returns>
            public int GetFirstNonZeroBit()
            {
                if (this.areBitsKnown)
                {
                    return this.firstNonZeroBit;
                }
                else
                {
                    this.GetNumberOfNonzeroBits();
                    return this.firstNonZeroBit;
                }
            }
    
            /// <summary>
            /// Gets the next non-zero bit in the BitSet, starting at the specified index.
            /// </summary>
            /// <param name="startIndex">The starting index in the BitSet to check.</param>
            /// <returns>The index value of the next non-zero bit, or UnknownBit if none exists.</returns>
            public int GetNextNonZeroBit(int startIndex)
            {
                for (int x = startIndex; x < this.size; x++)
                {
                    if (this[x])
                    {
                        return x;
                    }
                }
    
                return UnknownBit;
            }
    
            /// <summary>
            /// Determines if the BitSet value is zero.  If the number of non-zero bits is known from GetNumberOfNonzeroBits(), it checks that value
            /// to see if it is zero.  Otherwise, it examines each of the values in the internal array to determine if they are all zero, leading to a best
            /// case of O(1) if GetNumberOfNonzeroBits() has been called, and a worst-case of O(ceil(size / 64.0)) if it has not been called.
            /// </summary>
            /// <returns>True if all bits in the BitSet are zero, otherwise false.</returns>
            public bool IsZero()
            {
                // As a slight performance improvement, if we know the bits, we can just test the number of non-zero bits.
                // Otherwise, instead of counding the non-zero bits, we will very quickly test each value in the array instead of counting the number of bits set, making this o(n/64) instead of o(n).
                if (this.areBitsKnown)
                {
                    return this.numberOfNonZeroBits == 0;
                }
                else
                {
                    for (int x = 0; x < this.values.Length; x++)
                    {
                        if (this.values[x] != 0)
                        {
                            return false;
                        }
                    }
    
                    return true;
                }
            }
    
            /// <summary>
            /// Performs a logical "AND" between two BitSet values, and produces a new BitSet containing the result.
            /// </summary>
            /// <param name="value">The BitSet to perform the operation on along with this BitSet.</param>
            /// <returns>The BitSet resulting from the logical operation.</returns>
            public BitSet And(BitSet value)
            {
                if (value.size != this.size)
                {
                    throw new ArgumentException(BitSetSizesDoNotMatchOperationCannotBePerformed);
                }
    
                BitSet result = new BitSet(this.size);
    
                for (int x = 0; x < this.values.Length; x++)
                {
                    result.values[x] = this.values[x] & value.values[x];
                }
    
                return result;
            }
    
            /// <summary>
            /// Performs a logical "OR" between two BitSet values, and produces a new BitSet containing the result.
            /// </summary>
            /// <param name="value">The BitSet to perform the operation on along with this BitSet.</param>
            /// <returns>The BitSet resulting from the logical operation.</returns>
            public BitSet Or(BitSet value)
            {
                if (value.size != this.size)
                {
                    throw new ArgumentException(BitSetSizesDoNotMatchOperationCannotBePerformed);
                }
    
                BitSet result = new BitSet(this.size);
    
                for (int x = 0; x < this.values.Length; x++)
                {
                    result.values[x] = this.values[x] | value.values[x];
                }
    
                return result;
            }
    
            /// <summary>
            /// Performs a logical "XOR" (exclusive OR) between two BitSet values, and produces a new BitSet containing the result.
            /// </summary>
            /// <param name="value">The BitSet to perform the operation on along with this BitSet.</param>
            /// <returns>The BitSet resulting from the logical operation.</returns>
            public BitSet Xor(BitSet value)
            {
                if (value.size != this.size)
                {
                    throw new ArgumentException(BitSetSizesDoNotMatchOperationCannotBePerformed);
                }
    
                BitSet result = new BitSet(this.size);
    
                for (int x = 0; x < this.values.Length; x++)
                {
                    result.values[x] = this.values[x] ^ value.values[x];
                }
    
                return result;
            }
    
            /// <summary>
            /// Performs a logical "NOT" (bitwise complement) on the BitSet, and produces a new BitSet containing the result.
            /// </summary>
            /// <returns>The BitSet resulting from the logical operation.</returns>
            public BitSet Not()
            {
                BitSet result = new BitSet(this.size);
    
                for (int x = 0; x < this.values.Length; x++)
                {
                    result.values[x] = ~this.values[x];
                }
    
                // Zero out any remaining bits in the last value in the array if not all bits belong to the BitSet.
                int remainderAmount = (int)this.size % BitsInLong;
                if (remainderAmount != 0)
                {
                    result.values[this.values.Length - 1] = result.values[this.values.Length - 1] << (BitsInLong - remainderAmount);
                    result.values[this.values.Length - 1] = result.values[this.values.Length - 1] >> (BitsInLong - remainderAmount);
                }
    
                return result;
            }
    
            /// <summary>
            /// Overrides object.Equals() to allow comparison between two BitSet values to determine if they contain the same values.
            /// </summary>
            /// <param name="obj">The object to compare with this object.</param>
            /// <returns>True if the object is also a BitSet and contains the same values, false otherwise.</returns>
            public override bool Equals(object obj)
            {
                // If the other object is null, it cannot be equal.
                if (obj == null)
                {
                    return false;
                }
    
                // If the other object is not a BitSet, it cannot be equal.
                if (obj.GetType() != typeof(BitSet))
                {
                    return false;
                }
    
                BitSet otherBitSet = (BitSet)obj;
    
                // If the sizes of the BitSets do not match, they cannot be equal.
                if (this.size != otherBitSet.size)
                {
                    return false;
                }
    
                // Otherwise, two BitSets are equal if all values are identical.
                for (int x = 0; x < this.values.Length; x++)
                {
                    if (this.values[x] != otherBitSet.values[x])
                    {
                        return false;
                    }
                }
    
                return true;
            }
    
            /// <summary>
            /// Overrides object.GetHashCode() to provide a hash code based on the contents of the BitSet such that two equal BitSets will have the same hash code value.
            /// The implementation of this method performs an exclusive-OR on the values of the internal array, converted down to an integer to be returned.
            /// </summary>
            /// <returns>The hash code value.</returns>
            public override int GetHashCode()
            {
                ulong result = 0;
    
                for (int x = 0; x < this.values.Length; x++)
                {
                    result ^= this.values[x];
                }
    
                return unchecked((int)((result & uint.MaxValue) ^ ((result >> (BitsInLong / 2)) & uint.MaxValue)));
            }
        }
    }
}
