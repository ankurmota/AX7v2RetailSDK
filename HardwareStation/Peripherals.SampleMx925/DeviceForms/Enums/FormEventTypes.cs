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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.DeviceForms
    {
        /// <summary>
        /// Enumerates of form event types (XEVT).
        /// </summary>
        public enum FormEventType
        {
            /// <summary>
            /// Form event comes from Keypad control.
            /// </summary>
            Keypad = 0,
    
            /// <summary>
            /// Form event comes from Button control.
            /// </summary>
            Button = 2,
    
            /// <summary>
            /// Form event comes from Image control.
            /// </summary>
            Image = 3,
    
            /// <summary>
            /// Form event comes from CheckBox control.
            /// </summary>
            CheckBox = 6,
    
            /// <summary>
            /// Form event comes from ListBox control.
            /// </summary>
            ListBox = 7,
    
            /// <summary>
            /// Form event comes from Animation control.
            /// </summary>
            Animation = 8,
    
            /// <summary>
            /// Form event comes from Video control.
            /// </summary>
            Video = 17,
    
            /// <summary>
            /// Form event comes from Smartcard event.
            /// </summary>
            Smartcard = 50,
    
            /// <summary>
            /// Form event comes from NFC event.
            /// </summary>
            NearFieldCommunication = 60
        }
    }
}
