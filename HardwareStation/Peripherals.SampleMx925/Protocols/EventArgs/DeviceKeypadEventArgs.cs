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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols.EventArgs
    {
        using System;
    
        /// <summary>
        ///  Event arguments for keypad enter button pressed event.
        /// </summary>
        public class DeviceKeypadEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets Control Id.
            /// </summary>
            public int ControlId { get; set; }
    
            /// <summary>
            /// Gets or sets the string values entered into the control.
            /// </summary>
            public string Value { get; set; }
    
            /// <summary>
            /// Gets or sets FormName which contains the ControlId.
            /// </summary>
            public string FormName { get; set; }
    
            /// <summary>
            ///  Gets or sets a value indicating whether the user pressed cancel.
            /// </summary>
            public bool IsCanceled { get; set; }
        }
    }
}
