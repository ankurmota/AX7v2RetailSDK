/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

// Use #undef to suppress writing to file if the actual device is implemented.
#define CASHDISPENSER_SAMPLE

namespace Contoso
{
    namespace Commerce.HardwareStation.CashDispenserSample
    {
        using System;
        using System.Composition;
        using System.IO;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// The cash dispenser device of windows type.
        /// </summary>
        [Export("WINDOWS", typeof(ICashDispenser))]
        public class CashDispenserDevice : ICashDispenser
        {
            private const string CashDispenserTestName = "CashDispenserTest";

            /// <summary>
            /// Gets or sets the cash dispenser name.
            /// </summary>
            public string CashDispenserName { get; set; }

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">The peripheral configuration.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.CashDispenserName = peripheralName;

                // Open the cash dispenser. 
                NetTracer.Information("Cash dispenser is opened for device '{0}'.", peripheralName);

                return;
            }

            /// <summary>
            /// Collect the change in the cash dispenser.
            /// </summary>
            /// <param name="changeValue">The change value in decimal.</param>
            /// <param name="currency">The currency type.</param>
            /// <returns>Returns success if the change is collected.</returns>
            public bool CollectChange(decimal changeValue, string currency)
            {
                // Send the cash change into the cash dispenser.
#if CASHDISPENSER_SAMPLE
                if (this.CashDispenserName == CashDispenserDevice.CashDispenserTestName)
                {
                    const string CashDispenserTestFile = @"Microsoft Dynamics AX\Retail Hardware Station\Devices\CashDispenser.txt";

                    string commonApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string cashDispenserFile = Path.Combine(commonApplicationData, CashDispenserTestFile);

                    // Create the directory if it does not exist.
                    var fileInfo = new FileInfo(cashDispenserFile);
                    if (!Directory.Exists(fileInfo.DirectoryName))
                    {
                        fileInfo.Directory.Create();
                    }

                    // Write the data to cash dispenser file.
                    using (StreamWriter sw = new StreamWriter(cashDispenserFile, true))
                    {
                        sw.WriteLine(string.Format("Device: {0}, Currency: {1}, Change:{2}", this.CashDispenserName, currency, changeValue));
                    }
                }
#endif

                return true;
            }

            /// <summary>
            /// Closes the cash dispenser device.
            /// </summary>
            public void Close()
            {
                // Close the cash dispenser. 
                NetTracer.Information("Close cash dispenser device.");

                return;
            }            
        }
    }
}