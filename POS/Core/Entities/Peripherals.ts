/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='CommerceTypes.g.ts'/>

module Commerce.Proxy.Entities {

    /*
     * Peripheral Type for printer, cash drawer, or keylock.
     */
    export enum PeripheralType {
        None = 0,
        OPOS = 1,
        Windows = 2,
        Device = 3, // Local to device
        Network = 4
    }

    /*
    * Peripheral Type for MSR, pinpad, line display, scanner, scale, signature capture.
    */
    export enum PeripheralDeviceType {
        None = 0,
        OPOS = 1,
        Windows = 2,
        Network = 3
    }

    /*
    * Peripheral Type for payment
    */
    export enum PeripheralPaymentType {
        None = 0,
        CardPaymentController = 1, // credit and debit card payments for manual entry, MSR and other payment peripherals
        PaymentTerminal = 2, // credit and debit card payments through payment terminal
        RetailServer = 3,
        CardPaymentAccept = 4 // credit and debit card payments for manual entry through third party payment accepting page
    }

    /*
     * Printable receipt entity.
     */
    export class PrintableReceipt {
        public ReceiptHeader: string;
        public ReceiptBody: string;
        public ReceiptFooter: string;
        public ReceiptTypeValue: number;
        public ReceiptTypeStrValue: string;
        public ReceiptLayoutId: string;
        public ReceiptName: string;
        public PrinterName: string;
        public PrinterType: number;
        public PrinterIP: string;
        public PrinterPort: number;
        public PrintBehaviorValue: number;
        public ShouldPrint: boolean;
        public ShouldPrompt: boolean;

        constructor(Receipt: Model.Entities.Receipt, Printer: Model.Entities.Printer) {
            this.ReceiptHeader = Receipt.Header;
            this.ReceiptBody = Receipt.Body;
            this.ReceiptFooter = Receipt.Footer;
            this.ReceiptTypeValue = Receipt.ReceiptTypeValue;
            this.ReceiptTypeStrValue = Receipt.ReceiptTypeStrValue;
            this.ReceiptLayoutId = Receipt.LayoutId;

            this.ReceiptName = "";
            if (!StringExtensions.isEmptyOrWhitespace(Receipt.ReceiptTitle)) {
                this.ReceiptName = Receipt.ReceiptTitle;
            } else if (!StringExtensions.isEmptyOrWhitespace(Receipt.ReceiptTypeStrValue)) {
                this.ReceiptName = Receipt.ReceiptTypeStrValue;
            }

            this.PrinterName = Printer.Name;
            this.PrinterType = Printer.PrinterType;
            this.PrintBehaviorValue = Printer.PrintBehaviorValue;
            
            switch (this.PrintBehaviorValue) {
                case PrintBehavior.Never:
                case PrintBehavior.AsRequired:
                    this.ShouldPrint = false;
                    this.ShouldPrompt = false;
                    break;
                case PrintBehavior.Always:
                    this.ShouldPrint = true;
                    this.ShouldPrompt = false;
                    break;
                case PrintBehavior.Prompt:
                default:
                    this.ShouldPrint = true;
                    this.ShouldPrompt = true;
                    break;
            }

            if (this.PrinterType === Model.Entities.PeripheralType.Network) {
                var printerConfigurations: Model.Entities.HardwareConfiguration[];
                var hardwareStation: Model.Entities.IHardwareStation = HardwareStationEndpointStorage.getActiveHardwareStation();
                if (!ObjectExtensions.isNullOrUndefined(hardwareStation) && hardwareStation.ProfileId) {
                    printerConfigurations = hardwareStation.HardwareConfigurations.PrinterConfigurations;
                } else {
                    printerConfigurations = ApplicationContext.Instance.deviceConfiguration.HardwareConfigurations.PrinterConfigurations;
                }

                if (printerConfigurations) {
                    var printerConfiguration: Model.Entities.HardwareConfiguration =
                        ArrayExtensions.firstOrUndefined(
                            printerConfigurations,
                            (p: Model.Entities.HardwareConfiguration) => (StringExtensions.compare(p.DeviceName, this.PrinterName) === 0));

                    if (printerConfiguration) {
                        this.PrinterIP = printerConfiguration.IPAddress;
                        this.PrinterPort = printerConfiguration.Port;
                    }
                }
            }
        }
    }

    /*
     * Hardware station entity basic interface.
     */
    export interface IHardwareStation {
        RecordId: number;
        Url: string;
        ProfileId: string;
        EftTerminalId: string;
        HardwareConfigurations?: HardwareConfigurations;
        Description: string;
    }

    /*
     * Hardware station entity.
     */
    export class HardwareStation implements IHardwareStation {
        public RecordId: number;
        public HostName: string;
        public Description: string;
        public Url: string;
        public IsActive: boolean;
        public IsPaired: boolean;
        public ProfileId: string;
        public EftTerminalId: string;
        HardwareConfigurations: HardwareConfigurations;
    }

    /*
     * Card information object.
     */
    export interface CardInfo {
        CardTypeId?: string;
        CardNumber?: string;
        FirstName?: string;
        LastName?: string;
        ExpirationMonth?: number;
        ExpirationYear?: number;
        Track1?: string;
        Track2?: string;
        Track3?: string;
        EncryptedPin?: string;
        AdditionalSecurityData?: string;
        CashBackAmount?: number;
        DigitalSignature?: string;
        CCID?: string;
        VoiceAuthorizationCode?: string;
        Address1?: string;
        Zip?: string;

    }

    /*
     * Information returned from pin pad
     */
    export class PinPadInfo {
        public encryptedPin: string;
        public additionalSecurityData: string;

        constructor(encryptedPin: string, additionalSecurityData: string) {
            this.encryptedPin = encryptedPin;
            this.additionalSecurityData = additionalSecurityData;
        }
    }
}
