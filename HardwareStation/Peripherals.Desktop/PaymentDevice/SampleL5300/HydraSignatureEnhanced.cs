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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleL5300
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Diagnostics.Contracts;
        using System.Drawing;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        internal enum SignatureFormat
        {
            Unknown,
            Legacy,
            Enhanced
        }
    
        /// <summary>
        /// Code for processing L5300 Signature (Enhanced format) bit stream and converting to UPOS point array list.
        /// </summary>
        internal class HydraSignatureEnhanced
        {
            private const byte InvalidId = 0xff;
            private const int SignatureHeaderLength = 10; // in bytes
    
            private int bitOffset;
            private BitArray data;
            private SignatureFormat signatureType;
            private int startBitOffset;
    
            // Header Data
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Needed for debugging and readability.")]
            private byte id;
            private byte headerLength;
            private int axisScaleX;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Needed for debugging and readability.")]
            private int axisScaleY;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Needed for debugging and readability.")]
            private int phySizeX;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Needed for debugging and readability.")]
            private int phySizeY;
    
            public HydraSignatureEnhanced(byte[] signatureData)
            {
                Contract.Assert(signatureData != null);
    
                this.signatureType = SignatureFormat.Unknown;
                this.id = InvalidId; // Start with an invalid ID
    
                if ((signatureData != null) && (signatureData.Length >= 1))
                {   // Must have at least 1 byte to be a valid signature
    
                    byte[] reverseSignatureData = new byte[signatureData.Length];
                    for (int i = 0; i < signatureData.Length; i++)
                    {   // Reverse all bits to match "endian"
                        reverseSignatureData[i] = ReverseByte(signatureData[i]);
                    }
    
                    this.data = new BitArray(reverseSignatureData);
                    this.bitOffset = 0;
    
                    if (this.data[0] == true)
                    {   // Legacy signature always with "Move" instruction (e.g., first bit is 1)
                        this.signatureType = SignatureFormat.Legacy;
                        this.startBitOffset = 0; // Start with bit(0) - "Move"
                    }
                    else if ((reverseSignatureData[0] == 0) && (signatureData.Length >= SignatureHeaderLength))
                    {   // Enhanced format signature (first 8 bits must alwasy be 0)
                        this.signatureType = SignatureFormat.Enhanced;
    
                        // Signature Header:
                        // 8: ID (all bits 0)
                        // 8: HeaderDataLengths (MSB first, = 8)
                        // 16: X-Axis scaled resolution (hor full-scale range up to 2048, MSB first)
                        // 16: Y-Axis scaled resolution (vert full-scale range up to 1024, MSB fist)
                        // 16: X-Axis physical size (withd of pad in 0.01 mm units, for the ICE5500 this is 6717 which corresponding to 67.17mm, MSB first)
                        // 16: Y-Axis phy size (hight of pad in 0.01 mm units, MSB first)
                        this.id = this.ReadByte(8); // Enhanced Signature first 8 bits should alwasy be zero(0)
                        this.headerLength = this.ReadByte(8);
                        this.startBitOffset = (2 /* this.id + len */ + this.headerLength) * 8 /* 8 bits in byte */;
    
                        this.axisScaleX = this.ReadInt(16, false);
                        this.axisScaleY = this.ReadInt(16, false);
                        this.phySizeX = this.ReadInt(16, false);
                        this.phySizeY = this.ReadInt(16, false);
                    }
                }
                else
                {
                    NetTracer.Warning("L5300Terminal - L5300SignatureEnhanced - null or invalid signature header");
                }
            }
    
            /// <summary>
            /// Renders the UPOS signature.
            /// </summary>
            /// <param name="targetGraphics">The target graphics.</param>
            /// <param name="drawingPen">The drawing pen.</param>
            /// <param name="points">The points.</param>
            [Conditional("DEBUG")]
            public static void RenderUpos(Graphics targetGraphics, Pen drawingPen, IList<Point> points)
            {
                Point endPoint = new Point(-1, -1);
                Point lastPoint = new Point();
    
                foreach (Point nextPoint in points)
                {
                    if (nextPoint == endPoint)
                    {   // do nothing
                    }
                    else if (lastPoint == endPoint)
                    {   // do nothing
                    }
                    else
                    {
                        targetGraphics.DrawLine(
                            drawingPen,
                            lastPoint.X,
                            lastPoint.Y,
                            nextPoint.X,
                            nextPoint.Y);
                    }
    
                    lastPoint = nextPoint;
                }
            }
    
            /// <summary>
            /// Converts the UPOS point array into a byte array.
            /// </summary>
            /// <returns>Byte array representing the signature.</returns>
            public byte[] ToByteArray()
            {
                IList<Point> points = this.ToUposPointArray();
    
                if (points == null || points.Count < 1)
                {
                    return null;
                }
    
                // Size of int times number of values. Two values per point.
                List<byte> bytes = new List<byte>(points.Count * sizeof(int) * 2);
    
                foreach (Point point in points)
                {
                    bytes.AddRange(BitConverter.GetBytes(point.X));
                    bytes.AddRange(BitConverter.GetBytes(point.Y));
                }
    
                return bytes.ToArray();
            }
    
            /// <summary>
            /// Convert data to UPOS point array.
            /// <remarks>current implementation does not apply scaling.  1:1 ratio is assumed to be configured (1024:1024).</remarks>
            /// </summary>
            /// <returns>UPOS point array.</returns>
            public IList<Point> ToUposPointArray()
            {
                IList<Point> result = new List<Point>();
                Point endPoint = new Point(-1, -1);
    
                Point lastPoint = new Point();
                Point nextPoint;
                int absX, absY;
                int deltaX, deltaY;
    
                int moveBitsX = 0;
                int moveBitsY = 0;
    
                switch (this.signatureType)
                {
                    case SignatureFormat.Legacy:
                        Debug.WriteLine("L5300 Signature - Legacy");
                        moveBitsX = 10;
                        moveBitsY = 7;
                        break;
                    case SignatureFormat.Enhanced:
                        switch (this.axisScaleX)
                        {
                            case 1024:
                                Debug.WriteLine("L5300 Signature - Extended: 10");
                                moveBitsX = 10;
                                moveBitsY = 10;
                                break;
                            case 2048:
                                Debug.WriteLine("L5300 Signature - Extended: 11");
                                moveBitsX = 11;
                                moveBitsY = 10;
                                break;
                        }
    
                        break;
                }
    
                if (((moveBitsX > 0) && (moveBitsY > 0)) &&
                    ((this.signatureType == SignatureFormat.Legacy) || (this.signatureType == SignatureFormat.Enhanced)))
                {   // Suppored algorihtm
    
                    this.bitOffset = this.startBitOffset;
    
                    while (this.bitOffset < this.data.Length)
                    {
                        if (this.ReadBit())
                        {   // Move
    
                            if (this.bitOffset + moveBitsX + moveBitsY >= this.data.Length)
                            {   // End of bitstream
                                break;
                            }
    
                            absX = this.ReadInt(moveBitsX, false);
                            absY = this.ReadInt(moveBitsY, false);
    
                            lastPoint = new Point(absX, absY);
                            result.Add(endPoint);
                            result.Add(lastPoint);
    
                            Debug.WriteLine(string.Format("MoveTo: {0}, {1}", absX, absY));
                        }
                        else
                        {   // Draw
                            if (this.bitOffset + 6 + 6 >= this.data.Length)
                            {
                                break;
                            }
    
                            deltaX = this.ReadInt(6, true);
                            deltaY = this.ReadInt(6, true);
    
                            nextPoint = new Point(lastPoint.X + deltaX, lastPoint.Y + deltaY);
    
                            result.Add(nextPoint);
    
                            Debug.WriteLine(string.Format("DrawTo: {0}, {1}", nextPoint.X, nextPoint.Y));
    
                            lastPoint = nextPoint;
                        }
                    }
    
                    result.Add(endPoint);
                }
    
                return result;
            }
    
            private static byte ReverseByte(byte source)
            {
                int result = source;
                result = ((result & 0x55) << 1) | ((result & 0xAA) >> 1); // swap odd and even bits
                result = ((result & 0x33) << 2) | ((result & 0xCC) >> 2); // Swap consecutive pairs
                result = ((result & 0x0F) << 4) | ((result & 0xF0) >> 4); // swap nibbles
    
                return (byte)result;
            }
    
            /// <summary>
            /// Read a byte from the # of specified bits.  NOTE: MSB is first.
            /// </summary>
            /// <param name="noBits">Number of bits.</param>
            /// <returns>Byte read.</returns>
            private byte ReadByte(int noBits)
            {
                Contract.Assert((noBits > 0) && (noBits <= 8));
    
                byte result = 0;
    
                if (this.bitOffset + noBits < this.data.Length)
                {   // We have not gone over the buffer
                    for (int i = this.bitOffset; i < this.bitOffset + noBits; i++)
                    {
                        result <<= 1; // Shift
                        if (this.data[i])
                        {
                            result |= 1;
                        }
                    }
                }
    
                this.bitOffset += noBits;
    
                return result;
            }
    
            private bool ReadBit()
            {
                bool result = this.data[this.bitOffset];
    
                this.bitOffset += 1;
    
                return result;
            }
    
            private int ReadInt(int noBits, bool signBit)
            {
                int sign = 1;
                int result = 0;
    
                if (this.bitOffset + noBits < this.data.Length)
                {   // We have not gone over the buffer
                    for (int i = this.bitOffset; i < this.bitOffset + noBits; i++)
                    {
                        sign <<= 1;
                        result <<= 1; // Shift
                        if (this.data[i])
                        {
                            result |= 1;
                        }
                    }
                }
                else
                {
                    Debug.Fail("Invalid bitstream data");
                }
    
                this.bitOffset += noBits;
    
                // 2s complement hack
                if (signBit)
                {
                    if (result > (sign >> 1))
                    {
                        result -= sign; // 2^noBits
                    }
                }
    
                return result;
            }
        }
    }
}
