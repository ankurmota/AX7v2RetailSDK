/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";

    /**
     * Retail Transaction Type enum.
     */
    export enum ReceiptTransactionType {
        None = 1,
        Sale = 1,
        Return = 2,
        Payment = 5,
        SalesOrder = 6,
        Quote = 7
    }

    /**
     * Class for number sequence stoage.
     */
    export class NumberSequence {

        private static TransactionIdFormat: string = "{0}-{1}-{2}"; // StoreId-TerminalId-NextSeed;

        // Client driven number sequence can be enabled by the host, if required.
        public static Enabled = false;

        /**
         * Gets the specified item from local storage specified by storageId.
         *
         * @param {Model.Entities.NumberSequenceSeedType} numberSequenceType The type of the value to be retrieved.
         * @return {number} The next sequence for the given type.
         */
        public static GetNextValue(numberSequenceType: Model.Entities.NumberSequenceSeedType): number {
            if (!NumberSequence.Enabled) {
                return null;
            }

            var numberSequences = NumberSequence.Load();
            var value = numberSequences[numberSequenceType];

            if (!value) {
                value = 1;
            }

            numberSequences[numberSequenceType] = value + 1;
            NumberSequence.Save(numberSequences);

            return value;
        }

        /**
         * Gets the next formatted transaction Id.
         *
         * @return {string} The next transaction Id.
         */
        public static GetNextTransactionId(): string {
            if (!NumberSequence.Enabled) {
                return StringExtensions.EMPTY;
            }

            var value = NumberSequence.GetNextValue(Model.Entities.NumberSequenceSeedType.TransactionId);

            return StringExtensions.format(NumberSequence.TransactionIdFormat,
                Commerce.ApplicationContext.Instance.deviceConfiguration.StoreNumber,
                Commerce.ApplicationContext.Instance.deviceConfiguration.TerminalId,
                value);
        }

        /**
         * Gets the next receipt number sequence for the given type.
         *
         * @param {Proxy.Entities.Cart} cart The target cart object.
         * @return {string} The next sequence for the given receipt type.
         */
        public static GetNextReceiptId(cart: Proxy.Entities.Cart): string {
            if (!NumberSequence.Enabled
                || ObjectExtensions.isNullOrUndefined(cart)
                || StringExtensions.isNullOrWhitespace(cart.Id)) {
                return null;
            }

            // We cache the MRU receipt number sequence until transaction hasn't changed.
            var receiptId = NumberSequence.LoadReceiptNumberSequence(cart.Id);

            if (!StringExtensions.isNullOrWhitespace(receiptId)) {
                return receiptId;
            }

            var numberSequenceType = null;

            switch (cart.ReceiptTransactionTypeValue) {
                case ReceiptTransactionType.Sale:
                    numberSequenceType = Model.Entities.NumberSequenceSeedType.ReceiptSale;
                    break;

                case ReceiptTransactionType.Return:
                    numberSequenceType = Model.Entities.NumberSequenceSeedType.ReceiptReturn;
                    break;

                case ReceiptTransactionType.SalesOrder:
                    numberSequenceType = Model.Entities.NumberSequenceSeedType.ReceiptSalesOrder;
                    break;

                case ReceiptTransactionType.Quote:
                    numberSequenceType = Model.Entities.NumberSequenceSeedType.ReceiptSalesOrder;
                    break;

                case ReceiptTransactionType.Payment:
                    numberSequenceType = Model.Entities.NumberSequenceSeedType.ReceiptPayment;
                    break;

                default:
                    numberSequenceType = Model.Entities.NumberSequenceSeedType.ReceiptDefault;
            }

            receiptId = NumberSequence.GetNextValue(numberSequenceType).toString();

            NumberSequence.SaveReceiptNumberSequence(cart.Id, receiptId);
            return receiptId;
        }

        /**
         * Updates the number sequenes seed data.
         *
         * @param {NumberSequenceSeedData[]} numberSequencesCollection The number sequence seed data collection.
         */
        public static Update(numberSequencesCollection: Model.Entities.NumberSequenceSeedData[]): void {
            var numberSequences = [];

            numberSequencesCollection.forEach((numberSequence) => {
                numberSequences[numberSequence.DataTypeValue] = numberSequence.DataValue;
            });

            NumberSequence.Save(numberSequences);
        }

        private static Load(): number[]{
            var numberSequencesData = ApplicationStorage.getItem(ApplicationStorageIDs.NUMBER_SEQUENCES_KEY);

            return numberSequencesData ? JSON.parse(numberSequencesData) : [];
        }

        private static Save(numberSequences: number[]): void {
            ApplicationStorage.setItem(ApplicationStorageIDs.NUMBER_SEQUENCES_KEY, JSON.stringify(numberSequences));
        }

        private static LoadReceiptNumberSequence(cartId: string): string {
            var mruReceiptNumberData = ApplicationStorage.getItem(ApplicationStorageIDs.CART_RECEIPT_NUMBER_SEQUENCE_KEY);
            var mruReceiptNumber = mruReceiptNumberData ? JSON.parse(mruReceiptNumberData) : {};

            return mruReceiptNumber[cartId];
        }

        private static SaveReceiptNumberSequence(cartId: string, numberSequence: string): void {
            var mruReceiptNumber = {};

            mruReceiptNumber[cartId] = numberSequence;
            ApplicationStorage.setItem(ApplicationStorageIDs.CART_RECEIPT_NUMBER_SEQUENCE_KEY, JSON.stringify(mruReceiptNumber));
        }

        private static padLeft(value: number, length: number): string {
            if (value.toString().length >= length) {
                return value.toString();
            }

            return (Math.pow(10, length) + Math.floor(value)).toString().substring(1);
        }
    }
}