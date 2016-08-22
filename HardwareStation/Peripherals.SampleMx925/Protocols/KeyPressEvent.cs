/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

/*
 IMPORTANT!!!
 THIS IS SAMPLE CODE ONLY.
 THE CODE SHOULD BE UPDATED TO WORK WITH THE APPROPRIATE PAYMENT PROVIDERS.
 PROPER MESASURES SHOULD BE TAKEN TO ENSURE THAT THE PA-DSS AND PCI DSS REQUIREMENTS ARE MET.
*/
namespace Contoso
{
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols
    {
        /// <summary>
        ///  Type of key press events.
        /// </summary>
        public enum KeyPressEvent
        {
            /// <summary>
            ///  Invalid option.
            /// </summary>
            Error = 0,
    
            /// <summary>
            ///  A key press event.
            /// </summary>
            KeyPressed = 1,
    
            /// <summary>
            ///  The 'Enter' key was pressed.
            /// </summary>
            EnterPressed = 2,
    
            /// <summary>
            ///  The user pressed cancel.
            /// </summary>
            Cancel = 3,
    
            /// <summary>
            ///  Timeout for entry has occurred.
            /// </summary>
            Timeout = 4,
    
            /// <summary>
            ///  The 'Clear' key was pressed.
            /// </summary>
            Clear = 5
        }
    }
}
