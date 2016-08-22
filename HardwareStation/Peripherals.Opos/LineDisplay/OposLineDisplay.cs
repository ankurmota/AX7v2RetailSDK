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
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Interop.OposConstants;
        using Interop.OposLineDisplay;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;

        /// <summary>
        /// Class implements OPOS based line display for hardware station.
        /// </summary>
        [Export(PeripheralType.Opos, typeof(ILineDisplay))]
        public sealed class OposLineDisplay : ILineDisplay, IDisposable
        {
            private const char CharacterSetListSeparator = ',';
            private bool binaryConversionEnabled;
            private int numberOfColumns;
            private WorkerThread<IOPOSLineDisplay> oposLineDisplayWorker;

            /// <summary>
            /// Gets the columns.
            /// </summary>
            /// <value>
            /// The columns.
            /// </value>
            public int Columns
            {
                get { return this.numberOfColumns; }
            }

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.Open(peripheralName, 0, false, peripheralConfig);
            }

            /// <summary>
            /// Disposes the line display worker.
            /// </summary>
            public void Dispose()
            {
                this.Close();
            }

            /// <summary>
            /// Establishes a connection to the specified line display.
            /// </summary>
            /// <param name="peripheralName">Name of scale device to open.</param>
            /// <param name="characterSet">The character set.</param>
            /// <param name="binaryConversion">If set to <c>true</c> [binary conversion].</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, int characterSet, bool binaryConversion, PeripheralConfiguration peripheralConfig)
            {
                this.oposLineDisplayWorker = new WorkerThread<IOPOSLineDisplay>(() =>
                {
                    IOPOSLineDisplay oposLineDisplay = new OPOSLineDisplay();

                    // Open
                    oposLineDisplay.Open(peripheralName);
                    OposHelper.CheckResultCode(this, oposLineDisplay.ResultCode);

                    // Claim
                    oposLineDisplay.ClaimDevice(OposHelper.ClaimTimeOut);
                    OposHelper.CheckResultCode(this, oposLineDisplay.ResultCode);

                    // Enable/Configure
                    oposLineDisplay.DeviceEnabled = true;
                    binaryConversionEnabled = binaryConversion;

                    if (characterSet != 0)
                    {
                        // If character set is not supported by device, then disable and error out.
                        if (!oposLineDisplay.CharacterSetList.Split(CharacterSetListSeparator).Any(p => p.Equals(characterSet.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)))
                        {
                            oposLineDisplay.ReleaseDevice();
                            oposLineDisplay.Close();

                            throw new PeripheralException(PeripheralException.LineDisplayCharacterSetNotSupported);
                        }

                        oposLineDisplay.CharacterSet = characterSet;
                    }

                    this.numberOfColumns = oposLineDisplay.Columns;

                    return oposLineDisplay;
                });
            }

            /// <summary>
            /// Closes a connection with line display.
            /// </summary>
            public void Close()
            {
                if (this.oposLineDisplayWorker != null)
                {
                    this.oposLineDisplayWorker.Execute((oposLineDisplay) =>
                    {
                        if (oposLineDisplay != null)
                        {
                            oposLineDisplay.ReleaseDevice();
                            oposLineDisplay.Close();
                            oposLineDisplay = null;
                        }
                    });

                    this.oposLineDisplayWorker.Dispose();
                    this.oposLineDisplayWorker = null;
                }
            }

            /// <summary>
            /// Displays the text.
            /// </summary>
            /// <param name="lines">The lines to display.</param>
            public void DisplayText(IEnumerable<string> lines)
            {
                ThrowIf.Null(lines, "lines");

                this.oposLineDisplayWorker.Execute((oposLineDisplay) =>
                {
                    var index = 0;

                    foreach (var line in lines)
                    {
                        var textToDisplay = line;

                        if (this.binaryConversionEnabled)
                        {
                            oposLineDisplay.BinaryConversion = 2;  // OposBcDecimal
                            textToDisplay = OposHelper.ConvertToBCD(textToDisplay, oposLineDisplay.CharacterSet);
                        }

                        oposLineDisplay.DisplayTextAt(index++, 0, textToDisplay, (int)OPOSLineDisplayConstants.DISP_DT_NORMAL);
                        oposLineDisplay.BinaryConversion = 0;   // OposBcNone
                    }
                });
            }

            /// <summary>
            /// Clears this display.
            /// </summary>
            public void Clear()
            {
                this.oposLineDisplayWorker.Execute((oposLineDisplay) =>
                {
                    oposLineDisplay.ClearText();
                });
            }
        }
    }
}
