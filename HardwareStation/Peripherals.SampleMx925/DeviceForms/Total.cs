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
        using System.Globalization;
    
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.Forms;
    
        /// <summary>
        ///  Device form for displaying transaction totals screen.
        /// </summary>
        [Export("TOTAL", typeof(IForm))]
        public class Total : IForm
        {
            private const int ItemListControlId = 1;
            private const int SubtotalControlId = 6;
            private const int DiscountControlId = 7;
            private const int TaxControlId = 8;
            private const int TotalControlId = 9;
    
            /// <summary>
            ///  Gets the name of the form on the device.
            /// </summary>
            public string FormName
            {
                get { return Form.Total; }
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
    
                var formProperties = new List<DeviceFormProperty>(8);
    
                foreach (var property in properties)
                {
                    DeviceFormProperty deviceFormProperty;
                    switch (property.Name)
                    {
                        case Form.ItemListProperty:
                            deviceFormProperty = new DeviceFormProperty { ControlType = ControlType.ListBox, ControlId = ItemListControlId, Value = property.Value };
                            break;
                        case Form.SubtotalProperty:
                            deviceFormProperty = new DeviceFormProperty
                            {
                                ControlType = ControlType.Label,
                                ControlId = SubtotalControlId,
                                Name = StringPropertyName.Caption,
                                PropertyType = PropertyType.String,
                                Value = property.Value
                            };
                            break;
                        case Form.DiscountProperty:
                            deviceFormProperty = new DeviceFormProperty
                            {
                                ControlType = ControlType.Label,
                                ControlId = DiscountControlId,
                                Name = StringPropertyName.Caption,
                                PropertyType = PropertyType.String,
                                Value = property.Value
                            };
                            break;
                        case Form.TaxProperty:
                            deviceFormProperty = new DeviceFormProperty
                            {
                                ControlType = ControlType.Label,
                                ControlId = TaxControlId,
                                Name = StringPropertyName.Caption,
                                PropertyType = PropertyType.String,
                                Value = property.Value
                            };
                            break;
                        case Form.TotalProperty:
                            deviceFormProperty = new DeviceFormProperty
                            {
                                ControlType = ControlType.Label,
                                ControlId = TotalControlId,
                                Name = StringPropertyName.Caption,
                                PropertyType = PropertyType.String,
                                Value = property.Value
                            };
                            break;
                        default:
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown property {0}", property.Name));
                    }
    
                    formProperties.Add(deviceFormProperty);
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
