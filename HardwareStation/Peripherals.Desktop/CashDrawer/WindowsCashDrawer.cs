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
        using System.Collections.Generic;
        using System.Composition;
        using System.IO.Ports;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
    
        /// <summary>
        /// Class implements Windows based cash drawer driver for hardware station.
        /// </summary>
        [Export(PeripheralType.Windows, typeof(ICashDrawer))]
        public class WindowsCashDrawer : ICashDrawer
        {
            // <ESC>p\0dd  -> 1B 70 5C 30 64 64
            private const string OpenDrawerSequence = "\x1B\x70\x5C\x30\x64\x64";
    
            /// <summary>
            /// Gets the name of the cash drawer device to open.
            /// </summary>
            public string PortName { get; private set; }
    
            /// <summary>
            /// Gets a value indicating whether the cash drawer is open or not.
            /// </summary>
            public bool IsOpen
            {
                get
                {
                    // Currently no mechanism for determining drawer status via serial port.
                    return false;
                }
            }

            /// <summary>
            /// Establishes a connection to the specified cash drawer.
            /// </summary>
            /// <param name="peripheralName">Name of cash drawer device to open.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.PortName = peripheralName;
            }
    
            /// <summary>
            /// Terminates a connection to the cash drawer.
            /// </summary>
            public void Close()
            {
            }
    
            /// <summary>
            /// Causes the cash drawer to be physically opened.
            /// </summary>
            public void OpenDrawer()
            {
                using (SerialPort port = new SerialPort(this.PortName, 9600, Parity.None, 8, StopBits.One))
                {
                    port.Open();
                    port.Write(OpenDrawerSequence);
                }
            }
        }
    }
}
