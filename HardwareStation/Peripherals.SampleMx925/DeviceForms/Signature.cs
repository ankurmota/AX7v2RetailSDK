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
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Composition;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.Forms;
    
        /// <summary>
        ///  Device form for display customer signature screen.
        /// </summary>
        [Export("SIGNATURE", typeof(IForm))]
        public class Signature : IForm
        {
            /// <summary>
            ///  Start x coordinate of signature capture box.
            /// </summary>
            public const int StartX = 5;
    
            /// <summary>
            ///  Start y coordinate of signature capture box.
            /// </summary>
            public const int StartY = 140;
    
            /// <summary>
            ///  End x coordinate of signature capture box.
            /// </summary>
            public const int EndX = 700;
    
            /// <summary>
            ///  End y coordinate of signature capture box.
            /// </summary>
            public const int EndY = 330;
    
            private const int SignatureTextControlId = 2;
    
            /// <summary>
            ///  Gets the name of the form on the device.
            /// </summary>
            public string FormName
            {
                get { return Form.Signature; }
            }
    
            /// <summary>
            ///  Creates the form specific properties from the property name/values.
            /// </summary>
            /// <param name="properties">List of property name/values.</param>
            /// <returns>A list of form specific properties.</returns>
            public ReadOnlyCollection<DeviceFormProperty> CreateProperties(IEnumerable<FormProperty> properties)
            {
                if (properties == null)
                {
                    throw new ArgumentNullException("properties");
                }
    
                var formProperties = new List<DeviceFormProperty>(1);
    
                foreach (var property in properties)
                {
                    switch (property.Name)
                    {
                        case Form.ProvideSignatureTextProperty:
                            var formProp1 = new DeviceFormProperty
                            {
                                ControlType = ControlType.Label,
                                ControlId = SignatureTextControlId,
                                Name = StringPropertyName.Caption,
                                PropertyType = PropertyType.String,
                                Value = property.Value
                            };
                            formProperties.Add(formProp1);
                            break;
                    }
                }
    
                return formProperties.AsReadOnly();
            }
    
            /// <summary>
            ///  Converts the form specific control id to a control name.
            /// </summary>
            /// <param name="controlId">Control identifier.</param>
            /// <returns>Control name.</returns>
            public string GetControlName(int controlId)
            {
                throw new NotImplementedException();
            }
        }
    }
}
