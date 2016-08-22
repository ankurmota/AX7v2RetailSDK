/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {
    /**
     * Wraps the basic functionalities for AddressType enumeration data type.
     */
    export class AddressTypeHelper {
        /**
         * Gets the description string based on the AddressType value.
         * @param {AddressType} value The address type.
         * @return {string} The mapped description string based on AddressType value.
         */
        public static getDescription(value: AddressType): string {
            var textValue: string = null;

            switch (value) {

                case AddressType.AltDlv:
                    textValue = "string_4812"; // AltDlv
                    break;

                case AddressType.Business:
                    textValue = "string_4813"; // Business
                    break;

                case AddressType.Consignment_IN:
                    textValue = "string_4814"; // Consignment_IN
                    break;

                case AddressType.Delivery:
                    textValue = "string_4815"; // Delivery
                    break;

                case AddressType.FixedAsset:
                    textValue = "string_4816"; // FixedAsset
                    break;

                case AddressType.Home:
                    textValue = "string_4817"; // Home
                    break;

                case AddressType.Invoice:
                    textValue = "string_4818"; // Invoice
                    break;

                case AddressType.Lading_W:
                    textValue = "string_4819"; // Lading_W
                    break;

                case AddressType.None:
                    textValue = "string_4820"; // None
                    break;

                case AddressType.Onetime:
                    textValue = "string_4821"; // Onetime
                    break;

                case AddressType.Other:
                    textValue = "string_4822"; // Other
                    break;

                case AddressType.Payment:
                    textValue = "string_4823"; // Payment
                    break;

                case AddressType.Recruit:
                    textValue = "string_4824"; // Recruit
                    break;

                case AddressType.RemitTo:
                    textValue = "string_4825"; // RemitTo
                    break;

                case AddressType.Service:
                    textValue = "string_4826"; // Service
                    break;

                case AddressType.ShipCarrierThirdPartyShipping:
                    textValue = "string_4827"; // ShipCarrierThirdPartyShipping
                    break;

                case AddressType.SMS:
                    textValue = "string_4828"; // SMS
                    break;

                case AddressType.Statement:
                    textValue = "string_4829"; // Statement
                    break;

                case AddressType.SWIFT:
                    textValue = "string_4830"; // SWIFT
                    break;

                case AddressType.Unlading_W:
                    textValue = "string_4831"; // Unlading_W
                    break;

                default:
                    RetailLogger.coreHelpersUnknownAddressType(value);
                    break;
            }

            return Commerce.ViewModelAdapter.getResourceString(textValue);
        }

    }

}