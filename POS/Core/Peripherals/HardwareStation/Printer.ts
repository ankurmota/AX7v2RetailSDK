/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../IPrinter.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class Printer implements IPrinter {
        /**
         * Prints the receipt.
         *
         * @param {PrintableReceipt[]} printableReceipt[] The receipt objects.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public printAsync(printableReceipts: Model.Entities.PrintableReceipt[], callerContext?: any): IVoidAsyncResult {

            var self = this;
            var printRequests: PrintRequest[] = [];

            printableReceipts.forEach((printableReceipt) => {

                var printerProfile: Model.Entities.HardwareProfilePrinter = self.getPrinterProfile(printableReceipt);

                var printRequest: PrintRequest = <PrintRequest> {
                    DeviceName: printableReceipt.PrinterName,
                    DeviceType: Model.Entities.PeripheralType[printableReceipt.PrinterType],
                    CharacterSet: printerProfile.CharacterSet,
                    BinaryConversion: printerProfile.BinaryConversion,
                    Header: printableReceipt.ReceiptHeader,
                    Lines: printableReceipt.ReceiptBody,
                    Footer: printableReceipt.ReceiptFooter
                };

                if (printableReceipt.PrinterType == Model.Entities.PeripheralType.Network) {

                    var ipConfig = <Model.Entities.CommerceProperty> {
                        Key: PeripheralConfigKey.IpAddress,
                        Value: <Model.Entities.CommercePropertyValue> {
                            StringValue: printableReceipt.PrinterIP
                        }
                    };
                    var portConfig = <Model.Entities.CommerceProperty> {
                        Key: PeripheralConfigKey.Port,
                        Value: <Model.Entities.CommercePropertyValue> {
                            IntegerValue: printableReceipt.PrinterPort
                        }
                    };
                    printRequest.DeviceConfig = <PeripheralConfiguration> {
                        ExtensionProperties: [ipConfig, portConfig]
                    };
                }

                printRequests.push(printRequest);
            });

            return Peripherals.HardwareStation.HardwareStationContext.instance.peripheral('Printer').execute('Print', printRequests);
        }

        private getPrinterProfile(printableReceipt): Model.Entities.HardwareProfilePrinter {

            var result: Model.Entities.HardwareProfilePrinter = {};

            var printerProfiles: Model.Entities.HardwareProfilePrinter[] = ApplicationContext.Instance.hardwareProfile.Printers.filter((profile) => {
                if (StringExtensions.compare(profile.DeviceName, printableReceipt.PrinterName) === 0
                    && profile.DeviceTypeValue === printableReceipt.PrinterType) {
                    return true;
                }
            });

            if (ArrayExtensions.hasElements(printerProfiles)) {
                result = printerProfiles[0];
            }

            return result;
        }
    }
}