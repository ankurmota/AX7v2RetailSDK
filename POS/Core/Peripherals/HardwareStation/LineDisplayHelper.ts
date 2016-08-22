/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/Peripherals.ts'/>
///<reference path='../../IAsyncResult.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    /**
     * Type containing the elements of a cartline need for line display.
     */
    type CartLineForDisplay = {
        displayPrice: string,
        displayQuantity: string,
        description: string,
        unitOfMeasureSymbol: string
    };

    export class LineDisplayHelper {

        private static Space = ' ';

        /**
         * Displays the idle text on the line display.
         */
        public static displayIdleText() {
            if (Peripherals.instance && Peripherals.instance.lineDisplay) {
                // Display the idle message
                var lines: string[] = [];
                lines.push(Commerce.ApplicationContext.Instance.deviceConfiguration.CustomerDisplayText1);
                lines.push(Commerce.ApplicationContext.Instance.deviceConfiguration.CustomerDisplayText2);

                Peripherals.instance.lineDisplay.displayLines(lines);
            }
        }

        /**
         * Displays the terminal closed message on the line display.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public static displayTerminalClosedText(): IVoidAsyncResult {
            if (Peripherals.instance && Peripherals.instance.lineDisplay && Commerce.ApplicationContext.Instance.hardwareProfile.LineDisplayDisplayTerminalClosed) {
                // Display the terminal closed message
                var lines: string[] = [];

                lines.push(Commerce.ApplicationContext.Instance.hardwareProfile.LineDisplayClosedLine1);
                lines.push(Commerce.ApplicationContext.Instance.hardwareProfile.LineDisplayClosedLine2);

                return Peripherals.instance.lineDisplay.displayLines(lines);
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Displays line items on the line display.
         * Assumes the device has already been locked at beginning of transaction.
         *
         * @param {Proxy.Entities.Cart} originalCart The cart before the lines were added.
         * @param {Proxy.Entities.Cart} updatedCart The cart after the lines were added.
         */
        public static displayLineItems(originalCart: Proxy.Entities.Cart, updatedCart: Proxy.Entities.Cart): void {
            var deviceConfiguration: Proxy.Entities.DeviceConfiguration = ApplicationContext.Instance.deviceConfiguration;
            var allowAggregation: boolean = !ObjectExtensions.isNullOrUndefined(deviceConfiguration) && deviceConfiguration.AllowItemsAggregation;

            var originalCartLinesByLineId: { [lineId: string]: Proxy.Entities.CartLine } = Object.create(null);
            originalCart.CartLines.forEach((c: Commerce.Model.Entities.CartLine) => originalCartLinesByLineId[c.LineId] = c);

            // if the line is aggregate, quantity must have changed, if it is not aggregate, line should not be present
            var displayLines: CartLineForDisplay[] = [];
            for (var i: number = 0; i < updatedCart.CartLines.length; ++i) {
                var cartLine: Proxy.Entities.CartLine = updatedCart.CartLines[i];
                var originalCartLine: Proxy.Entities.CartLine = originalCartLinesByLineId[cartLine.LineId];
                // If the cart line didn't exist previously or was modified add it to be displayed.
                // The logic used here to check if the cartline was modified is similar to the logic used in CartLineHelper::getModifiedCartlines
                // If you make a change to the logic here please check to see if it is needed there as well.
                // Reflect the changes in line display for unit of measure changes.
                if (ObjectExtensions.isNullOrUndefined(originalCartLine)
                    || (allowAggregation && originalCartLine.Quantity !== cartLine.Quantity)
                    || originalCartLine.UnitOfMeasureSymbol !== cartLine.UnitOfMeasureSymbol) {
                    var quantity: number = cartLine.Quantity;
                    var price: number = cartLine.ExtendedPrice;

                    // Update the quantity only for set quantity operation.
                    if (allowAggregation && !ObjectExtensions.isNullOrUndefined(originalCartLine) && originalCartLine.Quantity !== cartLine.Quantity) {
                        // If the cart line id existed previously calculate the difference in price and quantity.
                        quantity -= originalCartLine.Quantity;
                        price -= originalCartLine.ExtendedPrice;
                    }

                    var displayLine: CartLineForDisplay = {
                        displayQuantity: UnitOfMeasureHelper.roundToDisplay(quantity, cartLine.UnitOfMeasureSymbol),
                        displayPrice: NumberExtensions.formatCurrency(price),
                        description: cartLine.Description,
                        unitOfMeasureSymbol: cartLine.UnitOfMeasureSymbol
                    };

                    displayLines.push(displayLine);

                    // If we are not displaying additional products on the line display, we show only the first item
                    if (!Commerce.ApplicationContext.Instance.hardwareProfile.LineDisplayDisplayLinkedItem) {
                        break;
                    }
                }
            }

            displayLines.forEach((line: CartLineForDisplay, index: number): void => {
                // Show the next added item after the number of seconds specified from AX
                setTimeout(() => {
                    var lines: string[] = [];

                    lines.push(line.description);
                    lines.push(LineDisplayHelper.formatLine(line.displayQuantity + LineDisplayHelper.Space + line.unitOfMeasureSymbol, line.displayPrice));

                    Peripherals.instance.lineDisplay.displayLines(lines);
                }, Commerce.ApplicationContext.Instance.hardwareProfile.LineDisplayDelayForLinkedItems * 1000 * index); // Converting seconds to milliseconds and multiplying by index to stagger the calls correctly
            });
        }

        /**
         * Displays balance information (total and balance) on the line display.
         * Assumes the device has already been locked at beginning of transaction.
         *
         * @param {number} [total] The total amount.
         * @param {number} [balance] The balance amount.
         * @return {IVoidAsyncResult} The async result.
         */
        public static displayBalance(total: number, balance: number): IVoidAsyncResult {
            var lines: string[] = [];

            lines.push(LineDisplayHelper.formatLine(ApplicationContext.Instance.hardwareProfile.LineDisplayTotalText, NumberExtensions.formatCurrency(total)));
            lines.push(LineDisplayHelper.formatLine(ApplicationContext.Instance.hardwareProfile.LineDisplayBalanceText, NumberExtensions.formatCurrency(balance)));

            return Peripherals.instance.lineDisplay.displayLines(lines);
        }

        /**
         * Formats a line to be displayed.
         * The format used is one string left aligned and one string right aligned to be displayed on one line.
         *
         * @param {string} [leftString] The string to be left aligned on the line.
         * @param {string} [rightString] The string to be right aligned on the line.
         * @return {string} The formatted string.
         */
        private static formatLine(leftString: string, rightString: string): string {
            var returnLine: string = StringExtensions.EMPTY;
            var spaceString: string = StringExtensions.EMPTY;
            var contentLength: number = 0;

            contentLength = leftString.length + rightString.length;

            if (contentLength < Peripherals.instance.lineDisplay.lineLength) {
                spaceString = StringExtensions.padRight(spaceString, LineDisplayHelper.Space, Peripherals.instance.lineDisplay.lineLength - contentLength);
            }

            return leftString + spaceString + rightString;
        }
    }
}
