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
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
    
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.EventArgs;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.DeviceForms;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols.EventArgs;
    
        /// <summary>
        /// Interface class for the MX925Protocols class.
        /// </summary>
        public interface IDeviceProtocol : IDisposable
        {
            /// <summary>
            ///  Card Data Event.
            /// </summary>
            event EventHandler<CardSwipeEventArgs> CardSwipeEvent;
    
            /// <summary>
            /// Pin Data Event.
            /// </summary>
            event EventHandler<PinDataEventArgs> PinDataEvent;
    
            /// <summary>
            /// Enter Keypad Event.
            /// </summary>
            event EventHandler<DeviceKeypadEventArgs> EnterKeypadEvent;
    
            /// <summary>
            /// Button Press Event.
            /// </summary>
            event EventHandler<DeviceButtonPressEventArgs> ButtonPressEvent;
    
            /// <summary>
            /// Signature Event.
            /// </summary>
            event EventHandler<SignatureEventArgs> SignatureEvent;
    
            /// <summary>
            /// Open connection between PC and the PaymentDevice.
            /// </summary>
            /// <returns>Empty Task.</returns>
            Task OpenAsync();
    
            /// <summary>
            /// Close connection between PC and the PaymentDevice.
            /// </summary>
            /// <returns>Empty Task.</returns>
            Task CloseAsync();
    
            /// <summary>
            /// Shows a form on the screen.
            /// </summary>
            /// <param name="formName">Name of form to show.</param>
            /// <param name="properties">Device form properties. </param>
            /// <returns> Boolean value to indicate success.</returns>
            Task<bool> ShowFormAsync(string formName, IEnumerable<DeviceFormProperty> properties);
    
            /// <summary>
            /// Turn on the PaymentDevice lights for card swipe.
            /// </summary>
            /// <param name="state">Enabled or disabled.</param>
            /// <returns>Empty Task.</returns>
            Task<bool> SetCardSwipeAsync(DeviceState state);
    
            /// <summary>
            /// Gets the encrypted PIN block from the card.
            /// </summary>
            /// <param name="state">Enabled or disabled.</param>
            /// <param name="cardNumber">Input debit card number.</param>
            /// <returns>Boolean value to indicate success.</returns>
            Task<bool> SetPinPadAsync(DeviceState state, string cardNumber);
    
            /// <summary>
            /// Send signature commands and get signature data.
            /// </summary>
            /// <param name="startX">The start X coordinate on form to capture signature.</param>
            /// <param name="startY">The start Y coordinate on form to capture signature.</param>
            /// <param name="endX">The end X coordinate on form to capture signature.</param>
            /// <param name="endY">The end Y coordinate on form to capture signature.</param>
            /// <param name="state">Enabled or disabled.</param>
            /// <returns>
            /// Boolean indicating success or failure.
            /// </returns>
            Task<bool> SetSignatureCaptureAsync(int startX, int startY, int endX, int endY, DeviceState state);
    
            /// <summary>
            /// Resets the terminal application state.
            /// </summary>
            /// <returns>Empty Task.</returns>
            Task<bool> ResetTerminalStateAsync();
        }
    }
}
