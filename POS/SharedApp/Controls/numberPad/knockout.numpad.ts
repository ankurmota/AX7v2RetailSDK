/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='../../Core/Core.winjs.ts'/>

/**
 * Number pad knockout binding handlers.
 *
 * numpad - binding handler for the standard/default numpad.
 * alphanumericNumpad - binding handler for the numpad that accept only alphanumeric characters.
 * cartNumpad - binding handler for the numpad used on the cart page.
 *
 * The usage follows knockout convention and the accepted options are: INumPadOptions for numpad handler, ICartNumPadOptions for cartNumpad
 * and IAlphaNumPadOptions for alphanumericNumpad.
 */
module Commerce.Controls.NumPad {
    "use strict";

    export interface INumPadResult {
        /**
         * The number pad value.
         */
        value: string;
    }

    export interface ICartNumPadResult extends INumPadResult {
        /**
         * The number pad quantity.
         */
        quantity: number;
    }

    export interface INumPadOptions {
        /**
         * The text box identifier.
         */
        textBoxId: string;

        /**
         * The function to be called when enter is pressed on the number pad or keyboard.
         */
        onEnter: (result: INumPadResult) => void;

        /**
         * The parser used to convert the text content to a value.
         */
        parser: IParser;

        /**
         * The parsed value associated with the number pad.
         */
        value: Observable<string> | string;

        /**
         * Whether or not to pre-select the text on the text box.
         */
        preSelectTextBox: boolean;

        /**
         * The supported decimal precision.
         */
        decimalPrecision: number;
    }

    export interface ICartNumPadOptions extends INumPadOptions {
        /**
         * The view name.
         */
        viewName: string;

        /**
         * The identifier of the div conatining the number pad.
         */
        containerId: string;

        /**
         * The identifier of the element containing the number pad title.
         */
        titleElementId: string;

        /**
         * The text box placeholder text.
         */
        placeholder: string;

        /**
         * The function to be called when enter is pressed on the number pad or keyboard.
         */
        onEnter: (result: ICartNumPadResult) => void;
    }

    export interface IAlphaNumPadOptions extends INumPadOptions {
        /**
         * Whether or not the decimal separator should be disabled.
         */
        disableDecimalSeparator: Observable<boolean> | boolean;
    }

    class NumPadState<T extends INumPadOptions> {
        public options: T;
        public textContent: string;
        public oldTextContent: string;
        public target: HTMLInputElement = null;

        private _callerContext: any;
        private _value: string;

        /**
         * Initializes a new instance of the NumPadState class.
         * @param {T} options The number pad options.
         * @param {any} [callerContext] The caller context for the onEnter callback.
         */
        constructor(options: T, callerContext: any) {
            this.options = options || <T>Object.create(null);
            this._callerContext = callerContext;

            this.initializeState();
        }

        /**
         * Gets the value associated with the number pad and .
         */
        public get value(): string {
            if (this.target) {
                return this.target.value = this._value;
            }

            return this._value;
        }

        /**
         * Sets the value associated with the number pad.
         */
        public set value(newValue: string) {
            this._value = newValue;
            if (this.target) {
                this.target.value = newValue;
            }
        }

        /**
         * Gets the number pad result.
         * @return {INumPadResult} The number pad result.
         */
        public getResult(): INumPadResult {
            return { value: this.value };
        }

        /**
         * Clears the number pad state.
         */
        protected clearState(): void {
            this.oldTextContent = this.textContent = StringExtensions.EMPTY;
        }

        /**
         * Initializes the number pad state.
         * @param {boolean} clearOnEnter Whether to clear the state after the onEnter callback is called.
         */
        protected initializeState(clearOnEnter: boolean = false): void {
            if (!ObjectExtensions.isNullOrUndefined(this.options.onEnter)) {
                var originalOnEnter: (result: INumPadResult) => void = this.options.onEnter;
                this.options.onEnter = (result: INumPadResult): void => {
                    originalOnEnter.call(this._callerContext, result);

                    // clears state after this
                    if (clearOnEnter) {
                        this.clearState();
                    }
                };
            }

            // Text box is selected by default
            if (ObjectExtensions.isNullOrUndefined(this.options.preSelectTextBox)) {
                this.options.preSelectTextBox = true;
            }

            var content: string;
            if (!StringExtensions.isNullOrWhitespace(this.options.textBoxId)) {
                this.target = <HTMLInputElement>$("#" + this.options.textBoxId)[0];
                if (!ObjectExtensions.isNullOrUndefined(this.target)) {
                    content = this.target.value;
                }
            }

            this.initializeContent(content);

            if (ObjectExtensions.isNullOrUndefined(this.options.decimalPrecision)) {
                this.options.decimalPrecision = Number.MAX_VALUE;
            }
        }

        private initializeContent(content: string): void {
            // clear out any undefined
            if (StringExtensions.isNullOrWhitespace(content)) {
                content = StringExtensions.EMPTY;
            }

            this._value = this.oldTextContent = this.textContent = content;
        }
    }

    enum CartNumPadModes {
        QuantityOrProduct = 0,  // The NumPadKnockoutHandler object can accept a quantity or a product
        Product = 1             // The NumPadKnockoutHandler object can accept a product
    }

    class CartNumPadState extends NumPadState<ICartNumPadOptions> {
        public quantity: number;
        public numPadMode: CartNumPadModes;

        /**
         * Initializes the number pad state.
         * @param {boolean} clearOnEnter Whether to clear the state after the onEnter callback is called.
         */
        protected initializeState(clearOnEnter: boolean = false): void {
            super.initializeState(true);

            this.quantity = undefined;
            this.numPadMode = CartNumPadModes.QuantityOrProduct;
        }

        /**
         * Gets the number pad result.
         * @return {ICartNumPadResult} The number pad result.
         */
        public getResult(): ICartNumPadResult {
            return { value: this.value, quantity: this.quantity };
        }

        /**
         * Clears the number pad state.
         */
        protected clearState(): void {
            super.clearState();

            this.quantity = undefined;
            this.numPadMode = CartNumPadModes.QuantityOrProduct;
        }
    }

    type NumPadInputHandler = <T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number) => boolean;

    class InputHandlers {
        /**
         * Handles when enter is either pressed on the number pad or typed on the keyboard.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static enter<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            if (key === "enter" || keyCode === 13) {
                RetailLogger.librariesNumpadEnterKey(state.value);

                if (!ObjectExtensions.isNullOrUndefined(state.options.onEnter)) {
                    state.options.onEnter(state.getResult());
                }

                return true;
            }

            return false;
        }

        /**
         * Toggles the minus sign on the number pad value.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static toggleMinus<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            if ((key === "-") || (String.fromCharCode(keyCode) === "-")) {
                if (state.textContent.charAt(0) !== "-") {
                    state.textContent = "-" + state.textContent;
                } else {
                    state.textContent = state.textContent.substr(1);
                }

                return true;
            }

            return false;
        }

        /**
         * Clears the content of the number pad.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static clear<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            if (key === "clear") {
                state.textContent = StringExtensions.EMPTY;
                return true;
            }

            return false;
        }

        /**
         * Handles when backspace is either pressed on the number pad or typed on the keyboard.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static backspace<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            if ((key === "backspace") || (keyCode === 8)) {
                if (state.textContent.length >= 1) {
                    state.textContent = state.textContent.substr(0, state.textContent.length - 1);
                }

                return true;
            }

            return false;
        }

        /**
         * Handles when the decimal separator is either pressed on the number pad or typed on the keyboard.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static decimalSeparator<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            var separator: string = NumberExtensions.decimalSeparator;

            if ((key === "decimal") || (String.fromCharCode(keyCode) === separator)) {
                if (state.textContent.indexOf(separator) === -1 && state.options.decimalPrecision !== 0) {
                    state.textContent += separator;
                }

                return true;
            }

            return false;
        }

        /**
         * Handles when the group separator is either pressed on the number pad or typed on the keyboard.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static groupSeparator<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            var separator: string = NumberExtensions.groupSeparator;
            if ((key === separator) || (String.fromCharCode(keyCode) === separator)) {
                // prevent for adding the group separator
                return true;
            }

            return false;
        }

        /**
         * Handles when numbers are either pressed on the number pad or typed on the keyboard.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static numbers<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            var decimalSeparatorIndex: number = state.textContent.indexOf(NumberExtensions.decimalSeparator);
            var currentPrecision: number = decimalSeparatorIndex !== -1 ? (state.textContent.length - decimalSeparatorIndex - 1) : -1;
            var valueToAdd: string;
            var algarism: string = key || String.fromCharCode(keyCode);

            if (algarism >= "0" && algarism <= "9") {
                valueToAdd = algarism;
            }

            if (!ObjectExtensions.isUndefined(valueToAdd)) {
                // replaces the selected text with the value to be added
                if (state.target.selectionStart !== state.target.selectionEnd) {
                    state.textContent = state.target.value.substr(0, state.target.selectionStart)
                        + valueToAdd + state.target.value.substr(state.target.selectionEnd);
                } else if (currentPrecision < state.options.decimalPrecision) {
                    state.textContent += valueToAdd;
                }

                return true;
            }

            return false;
        }

        /**
         * Handles when any key is either pressed on the number pad or typed on the keyboard.
         * @param {NumPadState<T>} state The number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static anyText<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            var valueToAdd: string = key || String.fromCharCode(keyCode);

            if (state.target === document.activeElement) {
                state.textContent = state.textContent.substr(0, state.target.selectionStart)
                    + valueToAdd + state.textContent.substr(state.target.selectionEnd);
            } else {
                state.textContent += valueToAdd;
            }

            return true;
        }

        /**
         * Handles when the multiplier key is either pressed on the number pad or typed on the keyboard.
         * @param {CartNumPadState} state The cart number pad state.
         * @param {string} key The value of the key pressed.
         * @param {number} keyCode The key code, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        public static quantity(state: CartNumPadState, key: string, keyCode: number): boolean {
            if (key === "*" || String.fromCharCode(keyCode) === "*") {
                // Handle the behavior when the entry field can be a quantity or product to
                // pull the quantity from the field
                if (state.numPadMode === CartNumPadModes.QuantityOrProduct) {
                    var value: number = NumberExtensions.parseNumber(state.textContent);
                    if (isNaN(value)) {
                        NotificationHandler.displayErrorMessage("string_29823"); // The value entered is not valid
                    } else {
                        state.quantity = value;
                        state.numPadMode = CartNumPadModes.Product;
                        state.textContent = StringExtensions.EMPTY;
                    }
                } else {
                    // Handle the behavior when the entry field can only be a product and the
                    // button is clicked to indicate that it is a quantity. This will reset the form to initial state
                    // and remove the quantity value.
                    state.quantity = null;
                    state.numPadMode = CartNumPadModes.QuantityOrProduct;
                }

                return true;
            }

            return false;
        }
    }

    class NumPadKnockoutHandler {
        protected static STATE_KEY_NAME: string = "BaseNumPadKnockoutHandlerState";
        private _handlers: NumPadInputHandler[] = [];
        private _disabledKeys: string[] = [];

        /**
         * Initializes a new instance of the BaseNumPadKnockoutHandler class.
         */
        constructor() {
            // this is necessary in order to have the BaseNumPadKnockoutHandler as pseudo-static class
            this.init = this.init.bind(this);
            this.update = this.update.bind(this);
        }

        /**
         * Adds a number pad input handler.
         */
        protected addHandler(handler: NumPadInputHandler): void {
            if (this._handlers.some((h: NumPadInputHandler) => h === handler)) {
                return;
            }

            this._handlers.push(handler);
        }

        /**
         * Registers the input handlers.
         */
        protected registerInputHandlers(): void {
            // adds default handler
            this.addHandler(this.ignoreDisabledKeys);
        }

        /**
         * Creates the number pad state given the options and caller context.
         * @param {INumPadOptions} options The number pad options.
         * @param {any} callerContext The caller context used for the onEnter callback.
         * @return {NumPadState<INumPadOptions>} The number pad state.
         */
        protected createState(options: INumPadOptions, callerContext: any): NumPadState<INumPadOptions> {
            return new NumPadState<INumPadOptions>(options, callerContext);
        }

        /**
         * Gets an array containing the disabled keys.
         * @return {string[]} The array of disabled keys.
         */
        protected getDisabledKeys(): string[] {
            return [];
        }

        /**
         * Whether should ignore the keyboard event.
         * @param {KeyboardEvent} event The keyboard event.
         * @return {boolean} True, if the event should be ignored, or false otherwise.
         */
        protected ignoreEvent(event: KeyboardEvent): boolean {
            return (event.charCode === 0 && event.keyCode !== 8) // ignore special keys on key down except backspace
                || (event.keyCode === undefined); // ignore special keys on key press
        }

        /**
         * The init implementation of the knockout binding handler.
         */
        public init(element: Element, valueAccessor: () => any, allBindingsAccessor: any, viewModel: any, bindingContext: any): any {
            var $element: JQuery = $(element);
            var options: INumPadOptions = ko.utils.unwrapObservable(valueAccessor() || Object.create(null));

            var state: NumPadState<INumPadOptions> = this.createState(options, viewModel);
            $(element).data(NumPadKnockoutHandler.STATE_KEY_NAME, state);

            this.registerInputHandlers();
            this.disableButtons($element);
            this.attachToButtonEvent($element);
            this.attachToKeyboardEvent($element, state.target);

            // Update the numpad value and preselect the text
            this.updateValues(state);
            this.selectText(state);
        }

        /**
         * The update implementation of the knockout binding handler.
         */
        public update(element: Element, valueAccessor: () => any, allBindingsAccessor: any, viewModel: any, bindingContext: any): void {
            var $element: JQuery = $(element);
            var state: NumPadState<INumPadOptions> = <any>$element.data(NumPadKnockoutHandler.STATE_KEY_NAME);

            this.updateValues(state);
        }

        /**
         * Updates the numpad values.
         * @param {NumPadState<INumPadOptions>} state The number pad state to be updated.
         */
        private updateValues(state: NumPadState<INumPadOptions>): void {
            var decimalPrecision: number = ko.utils.unwrapObservable(state.options.decimalPrecision);
            if (!ObjectExtensions.isNumber(decimalPrecision)) {
                decimalPrecision = Number.MAX_VALUE;
            }

            state.options.decimalPrecision = decimalPrecision;

            var newValue: string = ko.utils.unwrapObservable(state.options.value);
            if (ObjectExtensions.isNullOrUndefined(newValue)) {
                newValue = StringExtensions.EMPTY;
            }

            var parsedValue: string = this.parse(state.options.parser, newValue);
            var parsedTextContent: string = this.parse(state.options.parser, state.textContent);

            if (parsedTextContent !== parsedValue) {
                state.value = state.oldTextContent = state.textContent = parsedValue;
            }
        }

        /**
         * Selects the text in the numpad textbox if the textbox is controlled by the numpad and if preSelectTextBox is true.
         * @param {NumPadState<INumPadOptions>} state The number pad state to be updated.
         */
        private selectText(state: NumPadState<INumPadOptions>): void {
            if (!ObjectExtensions.isNullOrUndefined(state.target) && state.options.preSelectTextBox) {
                if (state.target.type === "text") {
                    var textLength: number = state.target.value ? state.target.value.length : 0;
                    state.target.setSelectionRange(textLength, textLength);
                }

                // We have a functional requirement to NOT show the on-screen keyboard. However,
                // when invoking "select" on a text input, the on-screen keyboard is opened by default.
                // To work around this behavior, a "readonly" attribute is set on the input immediately
                // prior to selecting the text, which causes the system to ignore the text selection,
                // and not open the on-screen keyboard. Then, the readonly attribute is removed to enable
                // text editing once again.
                $(state.target).attr("readonly", "readonly");
                state.target.select();
                $(state.target).removeAttr("readonly");
            }
        }

        /**
         * Disables buttons on the number pad, based on the disabled keys array.
         */
        private disableButtons($element: JQuery): void {
            this._disabledKeys = this.getDisabledKeys() || [];

            // Disabled the specified buttons by button value
            var buttonsToDisableList: JQuery = $element.find("button").filter((index: number, element: Element): boolean => {
                var value: string = $(element).val();
                return this._disabledKeys.some((disabledKey: string) => value === disabledKey);
            });

            for (var i: number = 0; i < buttonsToDisableList.length; i++) {
                $(buttonsToDisableList[i]).attr("disabled", "disabled");
            }
        }

        /**
         * Attaches to number pad buttons mouse down event.
         */
        private attachToButtonEvent($element: JQuery): void {
            // handles mouse clicks on the num pad
            $element.find("button").mousedown((event: JQueryEventObject): void => {
                var $buttonElement: JQuery = $(event.target);
                if ($buttonElement.is(":button") === false) {
                    $buttonElement = $buttonElement.closest(":button");
                }

                var state: NumPadState<INumPadOptions> = <any>$element.data(NumPadKnockoutHandler.STATE_KEY_NAME);
                var key: string = $buttonElement.val();

                var handled: boolean = this._handlers.some((handler: NumPadInputHandler) => handler(state, key, null));
                if (handled) {
                    this.updateState(state);
                }
            });
        }

        /**
         * Attaches to keyboard key down and input events.
         */
        private attachToKeyboardEvent($element: JQuery, textInput: HTMLInputElement): void {
            if (ObjectExtensions.isNullOrUndefined(textInput)) {
                return;
            }

            var $textInput: JQuery = $(textInput);

            // handles key down input for backspace, shift, etc.
            $textInput.keydown((event: KeyboardEvent) => {
                this.handleKeyboarEvent($element, event);
            });

            // handles key press for characters
            $textInput.keypress((event: KeyboardEvent) => {
                this.handleKeyboarEvent($element, event);
            });

            // handles cut, paste, clear, etc.
            $textInput.bind("input", (event: JQueryEventObject): void => {
                var state: NumPadState<INumPadOptions> = <any>$element.data(NumPadKnockoutHandler.STATE_KEY_NAME);
                state.textContent = $(event.target).val();

                this.updateState(state);

                // mark this event as handled
                event.preventDefault();
                event.stopImmediatePropagation();
            });
        }

        /**
         * Handles a keyboard event.
         * @param {JQuery} $element The jQuery element containing the state.
         * @param {KeyboardEvent} event The keyboard event to handle.
         */
        private handleKeyboarEvent($element: JQuery, event: KeyboardEvent): void {
            if (this.ignoreEvent(event)) {
                return;
            }

            var state: NumPadState<INumPadOptions> = <any>$element.data(NumPadKnockoutHandler.STATE_KEY_NAME);
            var handled: boolean = this._handlers.some((handler: NumPadInputHandler) => handler(state, null, event.which));

            if (handled) {
                this.updateState(state);
            }

            // mark this event as handled
            event.preventDefault();
            event.stopImmediatePropagation();
        }

        /**
         * Updates the number pad state.
         * @param {NumPadState<INumPadOptions>} state The number pad state to be updated.
         */
        private updateState(state: NumPadState<INumPadOptions>): void {
            var parsedValue: string = this.parse(state.options.parser, state.textContent);

            // text content was changed, but produced an invalid value, so ignore changes and re-update values
            if (!StringExtensions.isNullOrWhitespace(state.textContent) && StringExtensions.isNullOrWhitespace(parsedValue)) {
                state.textContent = state.oldTextContent;
                parsedValue = this.parse(state.options.parser, state.textContent);
            }

            var updateCaretPosition: boolean = !ObjectExtensions.isNullOrUndefined(state.target);
            var originalLength: number = state.value.length;
            var originalCaretPositionRelativeToEnd: number = updateCaretPosition ? (originalLength - state.target.selectionEnd) : -1;

            state.oldTextContent = state.textContent;

            var value: string | Observable<string> = state.options.value;
            if (!ObjectExtensions.isNullOrUndefined(value)) {
                if (typeof value === "string") {
                    value = parsedValue;
                } else {
                    (<Observable<string>>value)(parsedValue);
                }
            }

            state.value = parsedValue;

            // updates caret position and value if the text input exists and is in focus
            if (updateCaretPosition && state.target === document.activeElement) {
                var newLength: number = parsedValue.length;
                var newCaretPositionRelativeToEnd: number = newLength - originalCaretPositionRelativeToEnd;
                state.target.setSelectionRange(newCaretPositionRelativeToEnd, newCaretPositionRelativeToEnd);
            }
        }

        /**
         * Parses the given value based on the given parser, if any.
         * @param {IParser} parser The parser.
         * @param {string} valueToParse The value to be parsed.
         * @return {string} The parsed value, or the original value, if no parser provided.
         */
        private parse(parser: IParser, valueToParse: string): string {
            if (ObjectExtensions.isNullOrUndefined(parser)) {
                return valueToParse;
            }

            return parser.parse(valueToParse);
        }

        /**
         * Handles when an ignored key is either pressed on the number pad or typed on the keyboard.
         * @param {CartNumPadState} state The cart number pad state.
         * @param {string} key The value of the key pressed.
         * @param {KeyboardEvent} event The keyboard event, if any.
         * @return {boolean} True if the event is considered handled, false otherwise.
         */
        private ignoreDisabledKeys<T extends INumPadOptions>(state: NumPadState<T>, key: string, keyCode: number): boolean {
            return this._disabledKeys.some((disabledKey: string) => (key === disabledKey) || (disabledKey === String.fromCharCode(keyCode)));
        }
    }

    class StandardNumPadKnockoutHandler extends NumPadKnockoutHandler {
        /**
         * Registers the input handlers.
         */
        protected registerInputHandlers(): void {
            super.addHandler(InputHandlers.backspace);
            super.addHandler(InputHandlers.clear);
            super.addHandler(InputHandlers.decimalSeparator);
            super.addHandler(InputHandlers.enter);
            super.addHandler(InputHandlers.groupSeparator);
            super.addHandler(InputHandlers.toggleMinus);
            super.addHandler(InputHandlers.numbers);
        }
    }

    class AlphanumericNumPadKnockoutHandler extends NumPadKnockoutHandler {
        private static _disabledKeys: string[] = ["*", "-"];

        /**
         * Registers the input handlers.
         */
        protected registerInputHandlers(): void {
            super.addHandler(InputHandlers.backspace);
            super.addHandler(InputHandlers.clear);
            super.addHandler(InputHandlers.enter);
            super.addHandler(InputHandlers.anyText);
        }

        /**
         * Gets an array containing the disabled keys.
         * @return {string[]} The array of disabled keys.
         */
        protected getDisabledKeys(): string[] {
            return AlphanumericNumPadKnockoutHandler._disabledKeys;
        }

        /**
         * The init implementation of the knockout binding handler.
         */
        public init(element: Element, valueAccessor: () => any, allBindingsAccessor: any, viewModel: any, bindingContext: any): any {
            var options: IAlphaNumPadOptions = ko.utils.unwrapObservable(valueAccessor() || Object.create(null));
            var disableDecimalSeparator: boolean = ko.utils.unwrapObservable(options.disableDecimalSeparator || false);

            if (disableDecimalSeparator && !AlphanumericNumPadKnockoutHandler._disabledKeys.some((k: string) => k === "decimal")) {
                AlphanumericNumPadKnockoutHandler._disabledKeys.push("decimal");
            }

            super.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        }
    }

    class CartNumPadKnockoutHandler extends NumPadKnockoutHandler {
        /**
         * Registers the input handlers.
         */
        protected registerInputHandlers(): void {
            super.addHandler(InputHandlers.backspace);
            super.addHandler(InputHandlers.clear);
            super.addHandler(InputHandlers.decimalSeparator);
            super.addHandler(InputHandlers.enter);
            super.addHandler(InputHandlers.quantity);
            super.addHandler(InputHandlers.anyText);
        }

        /**
         * Creates the number pad state given the options and caller context.
         * @param {INumPadOptions} options The number pad options.
         * @param {any} callerContext The caller context used for the onEnter callback.
         * @return {NumPadState<INumPadOptions>} The number pad state.
         */
        protected createState(options: ICartNumPadOptions, callerContext: any): CartNumPadState {
            return new CartNumPadState(options, callerContext);
        }

        /**
         * The init implementation of the knockout binding handler.
         */
        public init(element: Element, valueAccessor: () => any, allBindingsAccessor: any, viewModel: any, bindingContext: any): any {
            super.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);

            var $element: JQuery = $(element);
            ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, (eventArgs: string) => { this.updateControlLayout($element); });
            this.updateControlLayout($element);
        }

        /**
         * Updates the number pad layout when the orientation changes.
         */
        private updateControlLayout($element: JQuery): void {
            var state: CartNumPadState = <any>$element.data(NumPadKnockoutHandler.STATE_KEY_NAME);
            var options: ICartNumPadOptions = state.options;
            var layout: Proxy.Entities.Layout;

            if (options.viewName) {
                if (options.containerId) {
                    layout = ApplicationContext.Instance.tillLayoutProxy.getLayoutItem(options.viewName, options.containerId);
                } else {
                    ViewModelAdapter.displayMessage("NumPad control requires a unique identifier as a parameter.", MessageType.Error);
                }
            }

            if (ObjectExtensions.isNullOrUndefined(layout)) {
                return;
            }

            // Hide number pad title as per layout.
            if (!ObjectExtensions.isNullOrUndefined(options.titleElementId)) {
                var $title: JQuery = $("#" + options.titleElementId);
                if (!layout.DisplayTitleAboveControl) {
                    $title.hide();

                    // Set placeholder text in text box when title is not shown.
                    if (!ObjectExtensions.isNullOrUndefined(options.placeholder)) {
                        state.target.placeholder = ViewModelAdapterWinJS.getResourceString(options.placeholder);
                        state.target.blur(); // To force placeholder text to show. Not shown when page is loaded.
                    }
                } else {
                    $title.show();
                    state.target.placeholder = StringExtensions.EMPTY;
                }
            }

            // Hide buttons as per layout.
            if (layout.HideButtons) {
                $element.hide();
            } else {
                $element.show();
            }
        }
    }

    ko.bindingHandlers.numpad = new StandardNumPadKnockoutHandler();
    ko.bindingHandlers.alphanumericNumpad = new AlphanumericNumPadKnockoutHandler();
    ko.bindingHandlers.cartNumpad = new CartNumPadKnockoutHandler();
}