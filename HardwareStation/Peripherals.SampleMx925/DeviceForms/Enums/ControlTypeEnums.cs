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
        /// Enumerate values of control types returned from XEVT form events.
        /// </summary>
        public enum ControlType
        {
            /// <summary>
            ///  Keypad control type.
            /// </summary>
            Keypad = 0,
    
            /// <summary>
            ///  TextBox control type.
            /// </summary>
            TextBox = 1,
    
            /// <summary>
            /// Button control type.
            /// </summary>
            Button = 2,
    
            /// <summary>
            ///  Image control type.
            /// </summary>
            Image = 3,
    
            /// <summary>
            ///  FunctionKeyButton control type.
            /// </summary>
            FunctionKeyButton = 4,
    
            /// <summary>
            ///  Label control type.
            /// </summary>
            Label = 5,
    
            /// <summary>
            ///  CheckBox control type.
            /// </summary>
            CheckBox = 6,
    
            /// <summary>
            ///  ListBox control type.
            /// </summary>
            ListBox = 7,
    
            /// <summary>
            ///  Animation control type.
            /// </summary>
            Animation = 8,
    
            /// <summary>
            ///  Box control type.
            /// </summary>
            Box = 9,
    
            /// <summary>
            ///  Marquee control type.
            /// </summary>
            Marquee = 10,
    
            /// <summary>
            ///  EditField control type.
            /// </summary>
            EditField = 11,
    
            /// <summary>
            ///  RadioButton control type.
            /// </summary>
            RadioButton = 12,
    
            /// <summary>
            ///  Video control type.
            /// </summary>
            Video = 17,
        }
    }
}
