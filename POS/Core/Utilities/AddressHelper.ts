/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='../ApplicationContext.ts'/>
///<reference path='../Core.d.ts'/>

module Commerce {
    "use strict";

    export class AddressHelper {

        /**
         * Construct the address entity to be formatted based from address format lines information we got from server.
         * @param {Model.Entities.Address} address The address entity to be formatted.
         * @param {boolean} isOneLiner True if address formatter needed is one liner (array of string contains 1 element), false otherwise.
         * @returns an array of address lines
         */
        public static getFormattedAddress(address: Model.Entities.Address, isOneLiner: boolean = false): string[] {

            var addressIndex: number = ApplicationContext.Instance.countriesIndexMap.getItem(address.ThreeLetterISORegionName);

            if (ObjectExtensions.isNullOrUndefined(addressIndex)) {
                return [];
            }

            var countryRegion: Model.Entities.CountryRegionInfo = ApplicationContext.Instance.Countries[addressIndex];
            var addressFormatLines: Model.Entities.AddressFormattingInfo[] = countryRegion.AddressFormatLines;

            var addressLines: string[] = [];
            var addressFormatLine: Model.Entities.AddressFormattingInfo;
            var addressLine: string = StringExtensions.EMPTY;
            var localizedWhitespace: string = ViewModelAdapter.getResourceString("string_2408");
            var lastNumberOfSpaces: number = 0;
            var lastSeparator: string = StringExtensions.EMPTY;

            //the array should already be sorted by number of address elements.
            for (var i = 0; i < addressFormatLines.length; i++) {
                addressFormatLine = addressFormatLines[i];

                if (addressFormatLine.Inactive || addressFormatLine.IsDataEntryOnly) {
                    continue;
                }

                var addressPart = AddressHelper.getAddressPart(addressFormatLine, address, countryRegion);

                //if current address part is empty then there is no need to add separators and spaces
                if (!StringExtensions.isNullOrWhitespace(addressPart)) {
                    if (!StringExtensions.isNullOrWhitespace(lastSeparator)) {
                        addressLine += lastSeparator;
                    }

                    var numberOfSpaces = lastNumberOfSpaces;
                    while (numberOfSpaces-- > 0) {
                        addressLine += " ";
                    }

                    // Add a localized whitespace if the address format line does not have either separator or number of spaces.
                    // Do this for one liner address.
                    if (isOneLiner && !StringExtensions.isNullOrWhitespace(addressLine) &&
                        StringExtensions.isNullOrWhitespace(lastSeparator) && lastNumberOfSpaces <= 0) {
                        addressLine += localizedWhitespace;
                    }

                    addressLine += addressPart;

                    lastNumberOfSpaces = addressFormatLine.NumberOfSpaces;
                    lastSeparator = addressFormatLine.Separator;
                }

                if (!isOneLiner && addressFormatLine.NewLine) {
                    lastSeparator = StringExtensions.EMPTY;
                    lastNumberOfSpaces = 0;

                    addressLines.push(addressLine);
                    addressLine = StringExtensions.EMPTY;
                }
            }

            addressLines.push(addressLine);
            return addressLines;
        }

        /**
         * Gets the next address part for the address entity which is currently being formatted.
         * @param {Model.Entities.AddressFormattingInfo} addressFormatLine Address format line information we got from server.
         * @param {Model.Entities.Address} address The address entity to be formatted.
         * @param {Model.Entities.CountryRegionInfo} countryRegion The country/region entity which corresponds to the address to be formatted.
         * @returns the next address part to be added to formatted address entity as a string.
         */
        private static getAddressPart(addressFormatLine: Model.Entities.AddressFormattingInfo, address: Model.Entities.Address, countryRegion: Model.Entities.CountryRegionInfo): string {
            switch (addressFormatLine.AddressComponentNameValue) {
                case Model.Entities.AddressFormatLineType.ZipCode:
                    return address.ZipCode;

                case Model.Entities.AddressFormatLineType.City:
                    return address.City;

                case Model.Entities.AddressFormatLineType.County:

                    if (addressFormatLine.Expand) {
                        return address.CountyName;
                    } else {
                        return address.County;
                    }

                case Model.Entities.AddressFormatLineType.State:

                    if (addressFormatLine.Expand) {
                        return address.StateName;
                    } else {
                        return address.State;
                    }

                case Model.Entities.AddressFormatLineType.CountryRegion:

                    if (addressFormatLine.Expand) {
                        return countryRegion.LongName;
                    } else {
                        return address.ThreeLetterISORegionName;
                    }

                case Model.Entities.AddressFormatLineType.StreetName:
                    return address.Street;

                case Model.Entities.AddressFormatLineType.District:
                    return address.DistrictName;

                case Model.Entities.AddressFormatLineType.StreetNumber:
                    return address.StreetNumber;

                case Model.Entities.AddressFormatLineType.BuildingCompliment:
                    return address.BuildingCompliment;

                case Model.Entities.AddressFormatLineType.Postbox:
                    return address.Postbox;

                default:
                    RetailLogger.coreHelpersUnrecognizedAddressComponent(addressFormatLine.AddressComponentNameValue);
                    break;
            }

            return StringExtensions.EMPTY;
        }
    }
}