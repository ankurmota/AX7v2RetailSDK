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
        using System.Composition;
        using System.Drawing;
        using System.Threading.Tasks;
        using Interop.OposConstants;
        using Interop.OposSigCap;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Class implements OPOS based signature capture (SigCap) for hardware station.
        /// </summary>
        [Export(PeripheralType.Opos, typeof(ISignatureCapture))]
        public sealed class OposSignatureCapture : ISignatureCapture
        {
            private OPOSSigCap oposSigCap;

            /// <summary>
            /// Occurs when [entry complete event].
            /// </summary>
            public event EventHandler<SignatureCaptureNotification> EntryCompleteEvent;

            /// <summary>
            /// Gets a value indicating whether [cap user terminated].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [cap user terminated]; otherwise, <c>false</c>.
            /// </value>
            /// <remarks>
            /// This property is initialized by the Load method.
            /// </remarks>
            public bool CapUserTerminated
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the maximum horizontal coordinate of the signature capture device.
            /// <remarks>This property is initialized by the Load method.</remarks>
            /// </summary>
            public int MaximumX
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the maximum vertical coordinate of the signature capture device.
            /// <remarks>This property is initialized by the Load method.</remarks>
            /// </summary>
            public int MaximumY
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets the last results.
            /// </summary>
            /// <value>
            /// The last results.
            /// </value>
            public SignatureCaptureResults LastResult
            {
                get;
                set;
            }

            /// <summary>
            /// Convert collection of points into byte array base 64 string.
            /// </summary>
            /// <param name="points">Collection of points.</param>
            /// <returns>Encoded string.</returns>
            public static string ToByteArrayString(ICollection<Point> points)
            {
                if (points == null || points.Count < 1)
                {
                    return null;
                }

                return Convert.ToBase64String(ToByteArray(points));
            }

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                if (this.oposSigCap == null)
                {
                    this.oposSigCap = new OPOSSigCap();

                    this.oposSigCap.DataEvent += this.OnSigCapDataEvent;
                    this.oposSigCap.ErrorEvent += this.OnSigCapErrorEvent;

                    try
                    {
                        // Open
                        Task.Run(() => this.oposSigCap.Open(peripheralName)).Wait(OposHelper.ClaimTimeOut);
                        OposHelper.CheckResultCode(this, this.oposSigCap.ResultCode);

                        // Claim
                        this.oposSigCap.ClaimDevice(OposHelper.ClaimTimeOut);
                        OposHelper.CheckResultCode(this, this.oposSigCap.ResultCode);

                        // Configure
                        this.CapUserTerminated = this.oposSigCap.CapUserTerminated;
                        this.MaximumX = this.oposSigCap.MaximumY;
                        this.MaximumY = this.oposSigCap.MaximumY;
                    }
                    catch
                    {
                        this.Close();
                        throw;
                    }
                }

                // NOTE: Hardware station does not support "ReatlTimeDataEnabled" property.
            }

            /// <summary>
            /// Closes the peripheral.
            /// </summary>
            public void Close()
            {
                if (this.oposSigCap != null)
                {
                    try
                    {
                        this.oposSigCap.DataEvent -= this.OnSigCapDataEvent;
                        this.oposSigCap.ErrorEvent -= this.OnSigCapErrorEvent;

                        this.oposSigCap.Close(); // Releases device and closes resources
                    }
                    finally
                    {
                        this.oposSigCap = null;
                    }
                }
            }

            /// <summary>
            /// Enable device for capture.
            /// </summary>
            /// <param name="formName">Form name (null or empty string will use default).</param>
            public void BeginCapture(string formName)
            {
                this.LastResult = null;

                if (this.oposSigCap != null)
                {
                    if (string.IsNullOrEmpty(formName))
                    {   // Default to empty string if value is not provided.
                        formName = string.Empty;
                    }

                    this.oposSigCap.DeviceEnabled = true;
                    this.oposSigCap.DataEventEnabled = true;

                    this.oposSigCap.BeginCapture(formName);
                    OposHelper.CheckResultCode(this, this.oposSigCap.ResultCode);
                }
            }

            /// <summary>
            /// Ends the capture.
            /// </summary>
            public void EndCapture()
            {
                SignatureCaptureResults results = null;

                if (this.oposSigCap != null)
                {
                    int resultCode = this.oposSigCap.EndCapture();
                    OposHelper.CheckResultCode(this, resultCode);

                    results = ParsePointArray(this.oposSigCap.PointArray);

                    this.oposSigCap.DeviceEnabled = false;
                    this.oposSigCap.DataEventEnabled = false;
                }

                this.LastResult = results;
            }

            /// <summary>
            /// Stops the capture.
            /// </summary>
            public void StopCapture()
            {
                if ((this.oposSigCap != null) && (this.EntryCompleteEvent != null))
                {
                    SignatureCaptureNotification notification = new SignatureCaptureNotification() { TerminatedSignatureSource = SignatureTerminationSource.CashierTerminaited };

                    this.EntryCompleteEvent(this, notification);
                }
            }

            #region Helpers

            /// <summary>
            /// Convert collection of points into byte array.
            /// </summary>
            /// <param name="points">Collection of points.</param>
            /// <returns>Byte array.</returns>
            private static byte[] ToByteArray(ICollection<Point> points)
            {
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

            private static Point GetPoint(char lowXchar, char highXchar, char lowYchar, char highYchar)
            {
                int x;
                int y;

                int loX = Microsoft.VisualBasic.Strings.Asc(lowXchar);
                int hiX = Microsoft.VisualBasic.Strings.Asc(highXchar);
                int loY = Microsoft.VisualBasic.Strings.Asc(lowYchar);
                int hiY = Microsoft.VisualBasic.Strings.Asc(highYchar);

                // NOTE: all values are unsigned
                x = (hiX << 8) | loX; // same as: hiX * 256 + loX;
                y = (hiY << 8) | loY; // same as: hiY * 256 + loY;

                if ((x == 0xffff) && (y == 0xffff))
                {   // End point
                    x = -1;
                    y = -1;
                }

                Point thePoint = new Point(x, y);

                return thePoint;
            }

            /// <summary>
            /// Convert point array string into array of points.
            /// </summary>
            /// <param name="pointArray">Point array string.</param>
            /// <returns>Returns ISignatureCaptureInfo.</returns>
            private static SignatureCaptureResults ParsePointArray(string pointArray)
            {
                SignatureCaptureResults signatureCaptureInfo = new SignatureCaptureResults();

                if (!string.IsNullOrWhiteSpace(pointArray))
                {
                    Point point;
                    int step = 4; // process 4 characters each step

                    List<Point> points = new List<Point>(pointArray.Length / step);

                    // Each point is represented by four characters: x(low 8 bits), x(hight 8 bits), y(low 8 bits), y(hight 8 bits)
                    for (int i = 0; i + step <= pointArray.Length; i += step)
                    {
                        point = GetPoint(pointArray[i], pointArray[i + 1], pointArray[i + 2], pointArray[i + 3]);
                        points.Add(point);
                    }

                    signatureCaptureInfo.Signature = ToByteArrayString(points);
                }

                return signatureCaptureInfo;
            }

            #endregion

            #region Event Handlers

            private void OnSigCapErrorEvent(int resultCode, int resultCodeExtended, int errorLocus, ref int errorResponseRef)
            {
                var ex = new PeripheralException(PeripheralException.PeripheralEventError, "Error Event from Peripheral with error code {0}", resultCode);
                RetailLogger.Log.HardwareStationPeripheralError("OposSignatureCapture", resultCode, resultCodeExtended, ex);
                throw ex;
            }

            private void OnSigCapDataEvent(int status)
            {
                if ((this.oposSigCap != null) && (this.EntryCompleteEvent != null))
                {
                    SignatureCaptureNotification notification = new SignatureCaptureNotification() { TerminatedSignatureSource = SignatureTerminationSource.UserTerminated };

                    this.EntryCompleteEvent(this, notification);
                }
            }

            #endregion
        }
    }
}
