/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Peripherals.Native {
    "use strict";

    export class MagneticStripeReader implements IMagneticStripeReader, IInitializable {

        private _magneticStripeReader: Windows.Devices.PointOfService.MagneticStripeReader;
        private _claimedMagneticStripeReader: Windows.Devices.PointOfService.ClaimedMagneticStripeReader;
        private _deviceId: string;


        /**
         * Ctor.
         *
         * @param {string} deviceId The deviceId.
         */
        constructor(deviceId: string) {
            this._deviceId = deviceId;
        }

        /**
         * Initialize MSR.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(): IVoidAsyncResult {

            var asyncResult = new VoidAsyncResult();
            var self = this;

            try {
            Windows.Devices.PointOfService.MagneticStripeReader.fromIdAsync(this._deviceId)
                    .then((reader: Windows.Devices.PointOfService.MagneticStripeReader) => {
                        if (reader != null) {
                            self._magneticStripeReader = reader;
                        }
                        asyncResult.resolve();
                    }, (e) => {
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_MSR_NOTFOUND)]);
                    });
            }
            catch (error) {
                RetailLogger.peripheralsMagneticStripeReaderInitializeFailed(error.message);
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Enable MSR device for scan.
         *
         * @param {(cardInfo: Model.Entities.CardInfo) => void} readerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(readerMsgEventHandler: (cardInfo: Model.Entities.CardInfo) => void): IVoidAsyncResult {
            return VoidAsyncResult.retryOnFailure(
                () => this.enableAsyncInternal(readerMsgEventHandler),
                () => this.initializeAsync());
        }

        private enableAsyncInternal(readerMsgEventHandler: (cardInfo: Model.Entities.CardInfo) => void): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();

            if (this._magneticStripeReader != null) {
                var self = this;

                this.disableAsync();
                this._magneticStripeReader.claimReaderAsync()
                    .done((claimedReader: Windows.Devices.PointOfService.ClaimedMagneticStripeReader) => {
                        if (claimedReader) {
                            this._claimedMagneticStripeReader = claimedReader;
                            claimedReader.isDecodeDataEnabled = true;
                            claimedReader.tracksToRead = Windows.Devices.PointOfService.MagneticStripeReaderTrackIds.track1 | Windows.Devices.PointOfService.MagneticStripeReaderTrackIds.track2;
                            claimedReader.onbankcarddatareceived = ((args) => {
                                if (readerMsgEventHandler) {
                                    readerMsgEventHandler(
                                        <Model.Entities.CardInfo>{
                                            CardNumber: args.accountNumber,
                                            FirstName: args.firstName,
                                            LastName: args.surname,
                                            ExpirationMonth: parseInt(args.expirationDate.substring(2)),
                                            ExpirationYear: parseInt(args.expirationDate.substring(0, 2)),
                                            Track1: args.report.track1.data ? Windows.Storage.Streams.DataReader.fromBuffer(args.report.track1.data).readString(args.report.track1.data.length) : StringExtensions.EMPTY,
                                            Track2: args.report.track2.data ? Windows.Storage.Streams.DataReader.fromBuffer(args.report.track2.data).readString(args.report.track2.data.length) : StringExtensions.EMPTY,
                                            Track3: args.report.track3.data ? Windows.Storage.Streams.DataReader.fromBuffer(args.report.track3.data).readString(args.report.track3.data.length) : StringExtensions.EMPTY
                                        });
                                }
                            });

                            claimedReader.enableAsync().done(() => {
                                asyncResult.resolve();
                            }, (e) => {
                                RetailLogger.peripheralsMagneticStripeReaderEnableFailed(e.message);
                                asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_MSR_ENABLE_FAILED)]);
                            });
                        } else {
                            asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED)]);
                        }
                    }, (e) => {
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED)]);
                    });
            }
            else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Disable MSR device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        disableAsync(): IVoidAsyncResult {
            if (this._magneticStripeReader && this._claimedMagneticStripeReader) {
                this._claimedMagneticStripeReader.onbankcarddatareceived = null;
                this._claimedMagneticStripeReader.close();
                this._claimedMagneticStripeReader = null;
            }
            return VoidAsyncResult.createResolved();
        }
    }
}
