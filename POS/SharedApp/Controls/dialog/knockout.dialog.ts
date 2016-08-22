/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Core/Core.winjs.ts'/>

/*
    dialog knockout binding extension
*/
/*
* dialog binding usage example:
* dialog: {
            modal: false,
            hideOnEscape: true,
            title: 'Dialog title',
            message: 'Some message to display',
            visible: true,
            buttons: [
                { label: 'button 0', operationId: 'operation0', isPrimary: true},
                { label: 'button 1', operationId: 'operation1', disable: _disabled },
                { label: 'button 2', operationId: 'operation2', flowLeft: true },
            ],
            buttonClick: ClickCallback,
          }

    @/// <param name="dialogType" type="DialogTypes">Default: DEFAULT. Specifies the dialog type.</param>
    @/// <param name="modal" type="boolean">Default: true. Determines if this dialog box is modal. Non-modal dialog will not display a blocking background</param>
    @/// <param name="hideOnEscape" type="boolean">Default: true. Determines if pressing escape key hides the dialog box. On hide, calls the operation for button click as if the Cancel button was clicked.</param>
    @/// <param name="backButtonVisible" type="boolean">Determines if back button is visible. Default: false. Does not apply to dialogs of type SEQUENCE.</param>
    @/// <param name="backClick" type="function">Function that gets excecuted when back button is clicked. Does not apply to dialogs of type SEQUENCE.</param>
    @/// <param name="title" type="string">Dialog title</param>
    @/// <param name="subTitle" type="string">Dialog subtitle</param>
    @/// <param name="message" type="string">Dialog message</param>
    @/// <param name="titleCssClass" type="string">Dialog title extra css class</param>
    @/// <param name="subTitleCssClass" type="string">Dialog subtitle css class</param>
    @/// <param name="messageCssClass" type="string">Dialog message css class</param>
    @/// <param name="visible" type="boolean">Default: false. Determines if this dialog box is visible</param>
    @/// <param name="showProgressIndicator" type="boolean">Default: false. Shows the progess indicator. Can be an observable boolean.</param>
    @/// <param name="enableValidation" type="boolean">Default: true. Enable html5 validation in dialogs./param>
    @/// <param name="buttons" type="object collection">
    @///    Each object defines a button label, operationId, and an optional disable parameter. The operationId is passed to the buttonClick callback function.
    @///    The disable parameters sets the button enabled/disabled state and can be a boolean or an observable boolean.
    @///    <param name="label" type="string">
    @///        The text to be displayed inside the button.
    @///    </param>
    @///    <param name="operationId" type="string">
    @///        The string constant in the knockout.dialog object which corresponds directly to an event handler inside of the view controller. This is the event handler which is
    @///        called specifically for the button on which this parameter is passed. Possible values are:
    @///        Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK
    @///        Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK
    @///        Commerce.Controls.Dialog.OperationIds.CLOSE_BUTTON_CLICK
    @///        Commerce.Controls.Dialog.OperationIds.NO_BUTTON_CLICK
    @///    </param>
    @///    <param name="isPrimary" type="boolean">
    @///        Boolean flag which provides the optional ability to specify which button is "primary," in that it responds to the enter-key being pressed and takes the accent color.
    @///    </param
    @///    <param name="disable" type="boolean">
    @///        Accepts either a boolean or a function which returns a boolean to set the disabled state of the button.
    @///    </param>
    @///    <param name="visible" type="boolean">
    @///        Accepts either a boolean or a function which returns a boolean to set the visible state of the button.
    @///    </param>
    @///    <param name="cssClass" type="string">
    @///        Additional css classes to add to the button. Multiple classes are separated by a single space.
    @///    </param>
    @///    <param name="flowLeft" type="boolean">
    @///        Indicates whether the button is to show on the left side of the dialog under the content section. If false or not specified, the buttons are on the right side of the dialog under the content section.
    @///    </param>
    @/// </param>
    @/// <param name="buttonClick" type="function">Function that gets excecuted with each button click. It is passed the operationId of the clicked button.</param>

* */

module Commerce.Controls.Dialog {
    "use strict";

    /**
     * List of possible dialog types
     */
    export enum DialogTypes {
        DEFAULT,  // There is no back button support or reserved space for the back button.
        SEQUENCE  // Used to support switching between multiple dialog states showing different objects. The back button is supported.
    }

    /**
     * Private member variables of the dialog.
     */
    export interface DialogState {
        // Values that map to options/parameters sent in to the dialog
        showProgressIndicator: any;
        enableValidation: boolean;
        modal: boolean;
        isFullView: boolean;
        buttons: any[];
        viewModel: any;
        subTitleClick: () => void;
        buttonClick: (operationId: string) => void;
        afterShow: () => void;
        backClick: () => void;
        onHidden: () => void;
        keyPressHandler: (ev: Event, ...eventData: any[]) => void;

        // Dialog state
        initialized: boolean;        // Indicates whether the dialog has been initialized.
        visible: boolean;            // Indicates whether the dialog is visible or is in the process of becoming visible.
        supportBackButton: boolean;  // Indicates whether the back button is supported (there is space reserved and can be made visible).
        tabIndex: number;            // The highest tab index on the dialog.
        hasContent: boolean;         // Indicates whether the message dialog has content
        closeOnEscButton: boolean;    // Indicates whether the dialog will close on ESC press.
        primaryButtonIndex: number;   // Stores the index for primary button
        focus: boolean; // indicates whether requires focus on element;


        // Handle to elements/controls on the dialog
        $element: JQuery;
        $background: JQuery;
        $title: JQuery;
        $subTitle: JQuery;
        $message: JQuery;
        $dialogContainer: JQuery;
        $backButton: JQuery;
        $topSectionSeperatorSpace: JQuery;
    }

    /**
     * Different click operation ids for the OK, Cancel, and Close button.
     */
    export class OperationIds {
        public static OK_BUTTON_CLICK = "okButtonClick";
        public static CANCEL_BUTTON_CLICK = "cancelButtonClick";
        public static CLOSE_BUTTON_CLICK = "closeButtonClick";
        public static NO_BUTTON_CLICK = "noButtonClick";
    }

    /**
     * Dialog class used by all dialogs.
     */
    export class DialogHandler {

        static _fadeDuration: number = 200; // fade in/out duration in ms for a modal dialog
        static _dialogStateKey: string = "dialogStateKey";
        static _visibleDialogs: DialogState[] = [];

        // Event handlers

        /**
         * This handles clicks from dialog buttons and invokes the buttonClick callback on the view model/controller if one is provided.
         *
         * @param {DialogState} dialogState The dialog options containing the handle to the button click callback method.
         * @param {string} operationId The operation identifier (ok, cancel, etc...).
         * @param {any} viewModel The view model containing the button click callback method.
         * @param {Event} [e] The event info.
         */
        static buttonClick(dialogState: DialogState, operationId: string, viewModel: any, e?: Event): void {
            if (e) {
                e.stopImmediatePropagation();
            }

            if (dialogState.buttonClick && typeof (dialogState.buttonClick) === "function") {
                dialogState.buttonClick.call(viewModel, operationId);
            }
        }

        /**
         * Click handler for when the subtitle is clicked.
         *
         * @param {DialogState} dialogState The dialog options containing the handle to the subTitleClick callback method.
         * @param {any} viewModel The view model containing the subTitleClick callback method.
         * @param {Event} e The event info.
         */
        static subTitleClick(dialogState: DialogState, viewModel: any, e: Event): void {
            if (e) {
                e.stopImmediatePropagation();
            }

            if (dialogState.subTitleClick && typeof (dialogState.subTitleClick) === "function") {
                dialogState.subTitleClick.call(viewModel);
            }
        }

        /**
         * Click handler for when the back button is clicked.
         *
         * @param {DialogState} dialogState The dialog options containing the handle to the subTitleClick callback method.
         * @param {any} viewModel The view model containing the subTitleClick callback method.
         * @param {Event} e The event info.
         */
        static backClick(dialogState: DialogState, viewModel: any, e: Event): void {
            if (e) {
                e.stopImmediatePropagation();
            }

            if (dialogState.backClick && typeof (dialogState.backClick) === "function") {
                dialogState.backClick.call(viewModel);
            }
        }

        /**
         * Click handler for when the background is clicked. Will close the dialog and execute the operation for the Cancel button click.
         *
         * @param {DialogState} dialogState The dialog options containing the handle to the subTitleClick callback method.
         * @param {any} viewModel The view model containing the subTitleClick callback method.
         * @param {Event} e The event info.
         */
        static backgroundClicked(dialogState: DialogState, viewModel: any, e: Event): void {
            if (dialogState == null) {
                return;
            }

            if (e) {
                e.stopImmediatePropagation();
            }

            DialogHandler.hide(dialogState);
            DialogHandler.buttonClick(dialogState, Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK, viewModel, e);
        }

        /**
         * Click handler for when the key is pressed.
         *  On Enter fires primary button event
         *  On Esc fires CANCEL_BUTTON event
         *
         * @param {DialogState} dialogState The dialog options containing the handle to the subTitleClick callback method.
         * @param {any} viewModel The view model containing the subTitleClick callback method.
         * @param {KeyboardEvent} e The event info.
         * @return {boolean} Returns "true" for event propogation.
         */
        static keyPressed(dialogState: DialogState, viewModel: any, e: JQueryKeyEventObject): boolean {
            // Casting to any as TypeScript is preventing compilation otherwise
            DialogHandler.stopPropagation(e);
            if (e.target.nodeName && e.target.nodeName === "BUTTON" && (e.keyCode !== 27 && e.keyCode !== 9))
                return true;

            // Prevents unexpected dialog closing on key press
            if (dialogState == null) {

                return true;
            }

            var handleEvent = false; // indicates whteher to fire button event.
            var operationId = Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK; // Default operation id.

            switch (e.keyCode) {
                case 27: // 27 - ESC key
                    if (dialogState.closeOnEscButton) {
                        handleEvent = true;
                        DialogHandler.hide(dialogState);
                    }
                    break;
                case 13: // Enter key
                    // Primary button exists, not disabled and source element cant handle it.
                    if ((dialogState.primaryButtonIndex >= 0)
                        && dialogState.buttons[dialogState.primaryButtonIndex].element.is(':enabled')) {

                        var sourceElement: JQuery = $(e.target);
                        if (!sourceElement.is('textarea') && !sourceElement.is('select')) {
                            var hasFocus: boolean = sourceElement.is('input');

                            // Forces knockout to update observable value if input doesn't have valueUpdate attribute.
                            if (hasFocus) {
                                sourceElement.trigger('change');
                            }

                            // if validation is enabled and focus is on the textarea/input, the enter key is handled by the form submit event
                            handleEvent = !(hasFocus && dialogState.enableValidation);
                            operationId = dialogState.buttons[dialogState.primaryButtonIndex].operationId;
                        }
                    }
                    break;
                case 9: // TAB key
                    DialogHandler.tabKeyPressedHandler(dialogState, e);
                    break;
            }

            if (handleEvent) {
                DialogHandler.buttonClick(dialogState, operationId, viewModel, e);
            }

            return true;
        }

        /**
         * Tab key pressed handler.
         * @param {DialogState} dialogState The dialog options containing the handle to the subTitleClick callback method.
         * @param {KeyboardEvent} e The event info.
         */
        static tabKeyPressedHandler(dialogState: DialogState, e: JQueryKeyEventObject): void {
            var focusableElements: JQuery = Commerce.Controls.Dialog.DialogHandler.getFocusableChildren(dialogState.$element);

            if (focusableElements.length > 0) {
                var focusedElement: HTMLElement = $(':focus')[0];
                var focusedElementIndex: number = $.inArray(focusedElement, <any>focusableElements);
                var nextFocusedElementIndex: number = 0;
                var lastFocusableElementIndex: number = focusableElements.length - 1;

                if (!e.shiftKey) {
                    nextFocusedElementIndex = focusedElementIndex >= lastFocusableElementIndex ? 0 : focusedElementIndex + 1;
                } else {
                    nextFocusedElementIndex = focusedElementIndex <= 0 ? lastFocusableElementIndex : focusedElementIndex - 1;
                }

                if (nextFocusedElementIndex >= 0) {
                    focusableElements[nextFocusedElementIndex].focus();
                }
            }

            e.preventDefault();
        }

        // Helper methods

        /**
         * Stop the propagation of the specified event.
         *
         * @param {JQueryEventObject} e The event to stop the propagation.
         */
        static stopPropagation(e: JQueryKeyEventObject, ...eventData: any[]): void {
            e.stopPropagation();
        }

        /**
         * Add the css classes to the primary and secondary buttons.
         *
         * @param {any} button The button to set state.
         * @param {JQuery} $button The JQuery handle to the button.
         * @param {DialogState} dialogState The dialog state.
         * @param {any} viewModel The view model containing the button click callback method.
         * @param {boolean} isPrimaryButton The button is the primary button.
         * @param {boolean} isDisabled The button is in the disabled state.e
         * @return {JQuery} The JQuery handle to the button.
         */
        static addPrimarySecondaryButtonClassNames(button: any, $button: JQuery, dialogState: DialogState, viewModel: any, isPrimaryButton: boolean, isDisabled: boolean): JQuery {

            $button.addClass(isPrimaryButton ? "primaryButton" : "secondaryButton");

            return $button;
        }

        /**
         * Gets the focusable children elements of an element.
         *
         * @param {JQuery} $parent The JQuery parent element to get the focusable children elements.
         * @return {JQuery} The JQuery object with the list of focusable children elements.
         */
        static getFocusableChildren($parent: JQuery): JQuery {
            if ($parent == null) {
                return null;
            }

            var tabbableElements: JQuery = $parent.find(':tabbable');

            return this.sortByTabindex(tabbableElements);
        }

        /**
         * Focus on the specified element. If the element contains children, will focus on the first child element.
         *
         * @param {DialogState} dialogState The dialog state.
         * @param {JQuery} $element The element to get focus.
         */
        static focusOnElement(dialogState: DialogState, $element: JQuery): void {
            //focus
            DialogHandler.getFocusableChildren($element).first().focus();

            //check to make sure focus is within element. If not, it means no focusable element exists, make element focusable
            var $focused = $(':focus');
            if ($focused.closest($element).length == 0) {
                $element.attr("tabindex", dialogState.tabIndex++);
                $element.focus();
            }
        }

        /**
         * Focus on the specified element if it is visible.
         *
         * @param {JQuery} $element The element to get focus.
         */
        static keepFocus(element: JQuery): void {
            if (element.is(":visible")) {
                var $focused: JQuery = $(':focus');
                if ($focused.closest(element).length == 0) {
                    element.focus();
                }
            }
        }

        /**
         * Shows the dialog
         * Will not show the dialog if already shown unless the forceShow parameter is set to true.
         *
         * @param {DialogState} dialogState The dialog state.
         * @param {boolean} [forceShow] Set to true to force the method to show indeterminate whether the dialog might already be shown.
         */
        static show(dialogState: DialogState, forceShow?: boolean): void {

            // Check the parameters
            if (ObjectExtensions.isNullOrUndefined(dialogState)) {
                return;
            }

            // Set the default value for the parameters
            forceShow = forceShow || false;

            // Get a handle to the element being shown
            var $element = dialogState.$element;

            // We shouldn't be performing animations when the visibility does not change (it is
            // already visible or in the process of being visible)
            if (!forceShow && dialogState.visible) {
                return;
            }

            // Set the state of the dialog to the expected target state of hidden
            dialogState.visible = true;

            // Add the state of the dialog to the visible dialogs array.
            DialogHandler._visibleDialogs.push(dialogState);

            $element.css("visibility", "visible"); //using css visibility to avoid problems with WinJS bindings

            // attach event handlers
            $element.on("keydown", dialogState.keyPressHandler);
            $element.on("keyup", DialogHandler.stopPropagation);
            $element.on("keypress", DialogHandler.stopPropagation);

            if (dialogState.modal) {
                dialogState.$dialogContainer.fadeIn(DialogHandler._fadeDuration, () => {
                    dialogState.$dialogContainer.css("visibility", "visible");
                    //focus
                    if (dialogState.focus) {
                        DialogHandler.focusOnElement(dialogState, $element);
                    }
                    if (dialogState.afterShow) {
                        dialogState.afterShow();
                    }
                });
            } else {
                dialogState.$dialogContainer.css("visibility", "visible");
                //focus
                if (dialogState.focus) {
                    DialogHandler.focusOnElement(dialogState, $element);
                }
                if (dialogState.afterShow) {
                    dialogState.afterShow();
                }
            }
        }

        /**
         * Hide the dialog.
         * Will not hide the dialog if already hidden unless the forceHide parameter is set to true.
         *
         * @param {DialogState} dialogState The dialog state.
         * @param {boolean} [forceShow] Set to true to force the method to show indeterminate whether the dialog might already be shown.
         */
        static hide(dialogState: DialogState, forceHide?: boolean): void {
            // Check the parameters
            if (dialogState == null) {
                return;
            }
            // Detach DOM event handlers.
            dialogState.$element.off("keydown", dialogState.keyPressHandler);
            dialogState.$element.off("keyup", DialogHandler.stopPropagation);
            dialogState.$element.off("keypress", DialogHandler.stopPropagation);

            // Set the parameter default values
            forceHide = forceHide || false;

            // Get a handler to the element being shown
            var $element = dialogState.$element;

            // Set the state of the dialog
            if (dialogState) {
                // We shouldn't be performing animations when the visibility does not change (it is
                // already hidden or in the process of being hidden)
                if (!forceHide && !dialogState.visible) {
                    return;
                }

                // Set the state of the dialog to the expected target state of hidden
                dialogState.visible = false;

                // Remove the state of the dialog from the visible dialogs array.
                var index = DialogHandler._visibleDialogs.indexOf(dialogState);
                if (index > -1) {
                    DialogHandler._visibleDialogs.splice(index, 1);
                }
            }


            // We don't want fading if the control is hiding as part of initialization
            if (dialogState.modal && dialogState.initialized) {
                dialogState.$dialogContainer.fadeOut(DialogHandler._fadeDuration, () => {
                    $element.css("visibility", "hidden");
                    dialogState.$dialogContainer.css("visibility", "hidden");

                    dialogState.onHidden();
                });
            } else {
                $element.css("visibility", "hidden");
                dialogState.$dialogContainer.css("visibility", "hidden");
            }
        }

        /**
         * Hide all visible dialogs.
         */
        public static hideAll(): void {
            while (ArrayExtensions.hasElements(DialogHandler._visibleDialogs)) {
                var dialogState = ArrayExtensions.lastOrUndefined(DialogHandler._visibleDialogs);

                if (!ObjectExtensions.isNullOrUndefined(dialogState)) {
                    DialogHandler.hide(dialogState);
                    DialogHandler.buttonClick(dialogState, OperationIds.CANCEL_BUTTON_CLICK, dialogState.viewModel);
                } else {
                    DialogHandler._visibleDialogs.pop();
                }
            }
        }

        /**
         * This will be called when the binding is first applied to an element.
         * Set up any initial state, event handlers, etc. here.
         *
         * @param {any} element The DOM element involved in this binding.
         * @param {any} valueAccessor A JavaScript function that you can call to get the current model property that is involved in this binding. Call this without passing any parameters (i.e., call valueAccessor()) to get the current model property value. To easily accept both observable and plain values, call ko.unwrap on the returned value.
         * @param {any} allBindingsAccessor A JavaScript object that you can use to access all the model values bound to this DOM element. Call allBindingsAccessor.get('name') to retrieve the value of the name binding (returns undefined if the binding doesn’t exist); or allBindingsAccessor.has('name') to determine if the name binding is present for the current element.
         * @param {any} viewModel The view model. This parameter is deprecated in Knockout 3.x. Use bindingContext.$data or bindingContext.$rawData to access the view model instead.
         * @param {any} bindingContext An object that holds the binding context available to this element’s bindings. This object includes special properties including $parent, $parents, and $root that can be used to access data that is bound against ancestors of this context.
         */
        static init(element: any, valueAccessor: any, allBindingsAccessor: any, viewModel: any, bindingContext: any): void {
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var $element = $(element);

            $element.addClass('commerceDialog');
            $element.attr("tabindex", -1);

            //set defaults and copy options
            var afterShow: any;
            if (viewModel.afterShow) {
                afterShow = () => {
                    viewModel.afterShow.call(viewModel);
                };
            }

            var onHidden = () => {
                if (viewModel && Commerce.ObjectExtensions.isOfType(viewModel, Commerce.Controls.ModalDialog)) {
                    viewModel.onHidden();
                }
            };

            var dialogState: DialogState = {
                buttons: value.buttons || {},
                viewModel: viewModel,
                subTitleClick: value.subTitleClick || null,
                buttonClick: value.buttonClick || null,
                backClick: value.backClick || null,
                afterShow: afterShow || null,
                onHidden: onHidden || null,
                modal: value.modal == null ? true : value.modal,
                showProgressIndicator: value.showProgressIndicator,
                enableValidation: ObjectExtensions.isBoolean(value.enableValidation) ? value.enableValidation : true,
                initialized: false,
                visible: false, // The tracking state of the visibility of the dialog. Not tracked in css as there can be a delay (due to fadein/fadeout) between a show/hide method and when the css visibility setting occurs.
                focus: ObjectExtensions.isNullOrUndefined(value.focus)? true : value.focus,
                supportBackButton: false,
                tabIndex: value.tabIndexButton || 0,
                hasContent: false,
                isFullView: value.isFullView,
                $element: $element,
                $background: null,
                $title: null,
                $message: null,
                $dialogContainer: null,
                $subTitle: null,
                $backButton: null,
                $topSectionSeperatorSpace: null,
                closeOnEscButton: false,
                primaryButtonIndex: -1,
                keyPressHandler: null
            };

            // Init Key Press Handler
            dialogState.keyPressHandler =  (e: JQueryKeyEventObject) => { DialogHandler.keyPressed(dialogState, viewModel, e); };

            // Set the state variables based on dialog type
            var dialogType: DialogTypes = value.dialogType || DialogTypes.DEFAULT;
            switch (dialogType) {
                case DialogTypes.DEFAULT:
                    dialogState.supportBackButton = false;
                    break;
                case DialogTypes.SEQUENCE:
                    dialogState.supportBackButton = true;
                    break;
            }

            // Attach the options to the element as there is one static dialog instance
            $element.data(DialogHandler._dialogStateKey, dialogState);

            //obtain initial content reference
            var $initialContent: JQuery = $element.contents();
            var $dialogContainer = $('<div tabindex="-1" class="dialogContainer"></div>');

            if (!dialogState.isFullView) {

                // Container 1
                // The container centers the strip containing the dialog and provides the background outside the modal dialog.
                $dialogContainer.addClass('centerY');
                dialogState.$dialogContainer = $dialogContainer;
                $element.append($dialogContainer);

                // Container 2 contains the dialog contents in 5 sections
                // The container provides the white background
                // Section 1 is white background to the left of the dialog contents and Section 2
                // Section 2 contains the back button to the left of the dialog contents
                // Section 3/Dialog contains the dialog contents
                // Section 4 is white background to the right of the dialog contents (should be the same size as Section 2)
                // Section 5 is white background to the right of the dialog contents (should be same size as Section 1)
                // Note:
                // Sections 2 and 4 are not included if the back button is not supported for the dialog type
                var $dialogContainer2 = $('<div tabindex="-1" class="dialogContainer2 row"></div>');
                $dialogContainer.append($dialogContainer2);

                // Prevent the click events for the container to go to Container 1... prevent unintended dialog closing
                $dialogContainer2.click((e: MouseEvent) => {
                    e.stopImmediatePropagation();
                });

                // Setup the sections
                var $dialogSection1 = $('<div class="col grow marginTop2"></div>');
                var $dialogSection2 = $('<div class="col grow marginTop2 width6"></div>');
                var $dialog = $('<div class="col grow marginTop2 width68"></div>');
                var $dialogSection4 = $('<div class="dialogSection4 col grow marginTop2 width6"></div>');
                var $dialogSection5 = $('<div class="col grow marginTop2"></div>');

                // Add the sections to Container 2
                // Only add the sections for the back button (2 and 4) if the back button is supported for the dialog type
                $dialogContainer2.append($dialogSection1);
                if (dialogState.supportBackButton) {
                    $dialogContainer2.append($dialogSection2);
                }
                $dialogContainer2.append($dialog);
                if (dialogState.supportBackButton) {
                    $dialogContainer2.append($dialogSection4);
                }
                $dialogContainer2.append($dialogSection5);

                // Add the back button to Container 2
                if (dialogState.supportBackButton) {
                    // Add back button to dialogSection2
                    var $backButtonWrappingDiv = $('<div tabindex="-1" class="height4 center"></div>');
                    var $backButton = $('<button class="iconNavBack backButton" aria-label="Back"></button>');
                    $backButton.click((e: Event) => { DialogHandler.backClick(dialogState, viewModel, e); });
                    dialogState.$backButton = $backButton;
                    $dialogSection2.append($backButtonWrappingDiv);
                    $backButtonWrappingDiv.append($backButton);
                }

                //
                // Add the content to the dialog
                // Content consists of:
                // Title
                // Subtitle (optional)
                // Message (optional)
                // Content (will contain the content defined in the content class of the html for the dialog)
                // Buttons (OK, Cancel, etc...)
                //

                // Add title
                var $titleWrappingDiv = $('<div class="marginTop1"></div>');
                var $title = $('<h2 class="title"></h2>');
                if (value.titleCssClass) {
                    $title.addClass(value.titleCssClass);
                }
                $dialog.append($titleWrappingDiv);
                $titleWrappingDiv.append($title);
                dialogState.$title = $title;

                // Add subtitle
                var $subTitleWrappingDiv = $('<div class="marginTop1"></div>'); // The visibility for this div is set in the update method
                var $subTitle = $('<h4></h4>');
                $subTitle.click((e: Event) => { DialogHandler.subTitleClick(dialogState, viewModel, e); });
                $subTitle.addClass(ko.utils.unwrapObservable(value.subTitleCssClass) || "secondaryFontColor");
                $dialog.append($subTitleWrappingDiv);
                $subTitleWrappingDiv.append($subTitle);
                dialogState.$subTitle = $subTitle;

                // Add message
                var $messageWrappingDiv = $('<div class="marginTop1" style="display: none"></div>'); // The visibility for this div is set in the update method
                var $message = $('<h4 class="message"></h4>');
                if (value.messageCssClass) {
                    $message.addClass(value.messageCssClass);
                }
                $dialog.append($messageWrappingDiv);
                $messageWrappingDiv.append($message);
                dialogState.$message = $message;

                // Add the spacing between the top section and the content
                var $topSectionSeperatorSpace = $('<div class="marginTop1"></div>');
                dialogState.$topSectionSeperatorSpace = $topSectionSeperatorSpace;
                $dialog.append($topSectionSeperatorSpace);

                var $dialogForm: JQuery = null;

                if (dialogState.enableValidation) {
                    var $div: JQuery = $("<div></div>");
                    $dialogForm = $("<form autocomplete='off'></form>");
                    $dialogForm.on("submit", (e) => {
                        // prevent the form from doing a submit
                        e.preventDefault();
                        return false;
                    });
                    $div.append($dialogForm);
                    $dialog.append($div);
                }

                // Re-add content, if exists with data, wrapped
                var $contentWrapper: JQuery = $("<div class='marginBottom3'></div>");
                dialogState.hasContent = true;
                if (($initialContent.length == 0) || (($initialContent.length === 1) && ($initialContent[0].firstChild == null) && ($initialContent[0].nodeName === "#text"))) {
                    $contentWrapper = $("<div></div>");
                    dialogState.hasContent = false;
                }

                if ($dialogForm != null) {
                    $dialogForm.append($contentWrapper);
                } else {
                    $dialog.append($contentWrapper);
                }

                $initialContent.appendTo($contentWrapper);

                // Add buttons
                if (dialogState.buttons && dialogState.buttons.length > 0) {
                    var $buttonsContainer: JQuery = $('<div class="buttonsContainer row marginBottom3"></div>');

                    // Determine the index of the primary button
                    // For LTR, buttons are set to flowLeft (show on the left side of the dialog under the content section) or show on the right side of the dialog under the content section.
                    // If no button is specified as primary, the first button (on the left side of the right side of the dialog) not specified as flowLeft is set to primary by default
                    // (if multiple buttons are specified as primary, every button after the first one is ignored).
                    // If all buttons are explicitly set to not be primary, then no button is marked as primary.
                    var primaryButtonIndex = -1;
                    var allButtonsAreSecondary = true;
                    var buttonPrimaryInRightSection = true;
                    var buttonsLeft: any[] = [];
                    var buttonsRight: any[] = [];
                    dialogState.buttons.forEach((button, index) => {
                        // Add the button to the left or right button array
                        var buttonRight: boolean = ObjectExtensions.isNullOrUndefined(button.flowLeft) || !button.flowLeft;
                        if (buttonRight) {
                            buttonsRight.push(button);
                        } else {
                            buttonsLeft.push(button);
                        }

                        if ((button.isPrimary) && (primaryButtonIndex === -1)) {
                            buttonPrimaryInRightSection = buttonRight;
                            primaryButtonIndex = (buttonRight ? buttonsRight.length : buttonsLeft.length) - 1;
                            allButtonsAreSecondary = false;
                        } else if (ObjectExtensions.isNullOrUndefined(button.isPrimary)) {
                            allButtonsAreSecondary = false;
                        }
                    });

                    // Set the default index of the primary button if not set
                    if (!allButtonsAreSecondary && (primaryButtonIndex === -1)) {
                        buttonPrimaryInRightSection = buttonsRight.length > 0;
                        primaryButtonIndex = 0;
                    }

                    // Init primary button index
                    dialogState.primaryButtonIndex = primaryButtonIndex;

                    // Function to create the button based on the button information parameter
                    var createButton = ((button, $buttonDivPadding, isButtonPrimary) => {
                        var $buttonContainer = $('<div class="buttonContainer col no-shrink"></div>'); // Specifies the dimensions of the button
                        var buttonType: string = (button.type) ? button.type : (button.operationId === OperationIds.OK_BUTTON_CLICK ? "submit" : "button");
                        var $button = $('<button></button>');
                        $button.prop("type", buttonType);

                        button.element = $button; // Stores the reference to the Dom element;
                        $button.attr("tabindex", dialogState.tabIndex++);

                        // Supplementary class
                        if (<string>button.cssClass != null) {
                            $button.addClass(<string>button.cssClass);
                        }
                        // Supplementary id
                        if (<string>button.id != null) {
                            $button.attr("id", <string>button.id);
                        }

                        // Set the visibility state of the button
                        if (!ObjectExtensions.isNullOrUndefined(button.visible)) {
                            var isButtonVisible: boolean = true;
                            if (button.visible && button.visible.subscribe && (typeof button.visible === "function")) {
                                isButtonVisible = button.visible();
                                button.visible.subscribe((newValue: boolean) => {
                                    if (newValue) {
                                        if ($buttonDivPadding) {
                                            $buttonDivPadding.show();
                                        }
                                        $button.show();
                                    } else {
                                        if ($buttonDivPadding) {
                                            $buttonDivPadding.hide();
                                        }
                                        $button.hide();
                                    }
                                });
                            } else if (typeof button.visible === "boolean") {
                                isButtonVisible = button.visible;
                            }

                            if (isButtonVisible) {
                                if ($buttonDivPadding) {
                                    $buttonDivPadding.show();
                                }
                                $button.show();
                            } else {
                                if ($buttonDivPadding) {
                                    $buttonDivPadding.hide();
                                }
                                $button.hide();
                            }
                        }

                        // Set the disabled state of the button
                        var isButtonDisabled = button.disable;
                        if (button.disable) {
                            var firstButton: HTMLButtonElement = <HTMLButtonElement>$button[0];
                            if (button.disable && button.disable.subscribe && (typeof button.disable === "function")) {
                                firstButton.disabled = button.disable();
                                isButtonDisabled = button.disable();
                                button.disable.subscribe((newValue: boolean) => {
                                    firstButton.disabled = newValue;

                                    // set the button classes based on its disabled state
                                    $button = DialogHandler.addPrimarySecondaryButtonClassNames(button, $button, dialogState, viewModel, isButtonPrimary, newValue);
                                });
                            } else if (typeof button.disable === "boolean") {
                                firstButton.disabled = button.disable;

                                // set the button classes based on its disabled state
                                isButtonDisabled = button.disable;
                            }
                        } else {
                            isButtonDisabled = false;
                        }

                        $button = DialogHandler.addPrimarySecondaryButtonClassNames(button, $button, dialogState, viewModel, isButtonPrimary, isButtonDisabled);

                        // Set the button label
                        ko.applyBindingsToNode($button[0], { text: button.label });

                        $button.click((e: Event) => {
                            if (button.operationId === OperationIds.OK_BUTTON_CLICK && $dialogForm != null) {
                                var form: HTMLFormElement = <HTMLFormElement>$dialogForm.get(0);
                                if (!ObjectExtensions.isNullOrUndefined(form) && !form.checkValidity()) {
                                    return;
                                }
                            }

                            DialogHandler.buttonClick(dialogState, button.operationId, viewModel, e);
                        });

                        $buttonsContainer.append($buttonContainer);
                        $buttonContainer.append($button);
                    });

                    // Add the left buttons
                    buttonsLeft.forEach((button, index) => {
                        var $buttonDivPadding = null;
                        if (index > 0) {
                            $buttonDivPadding = $('<div class="col width2"></div>');
                            $buttonsContainer.append($buttonDivPadding);
                        }
                        createButton(button, $buttonDivPadding, !buttonPrimaryInRightSection && (index === primaryButtonIndex));
                    });

                    // Add the section between the buttons. Also acts as padding if buttons are either on the left or right
                    // to push the buttons to the expected side
                    $buttonsContainer.append($('<div class="col grow"></div>'));

                    // Add the right buttons
                    buttonsRight.forEach((button, index) => {
                        var $buttonDivPadding = null;
                        if (index > 0) {
                            $buttonDivPadding = $('<div class="col width2"></div>');
                            $buttonsContainer.append($buttonDivPadding);
                        }
                        createButton(button, $buttonDivPadding, buttonPrimaryInRightSection && (index === primaryButtonIndex));
                    });

                    if ($dialogForm != null) {
                        $dialogForm.append($buttonsContainer);
                    } else {
                        $dialog.append($buttonsContainer);
                    }
                }
            } else {
                // Container 1
                // The container centers the strip containing the dialog and provides the background outside the modal dialog.
                $dialogContainer.addClass('center');

                dialogState.$dialogContainer = $dialogContainer;
                $element.append($dialogContainer);

                var $dialogContainer2 = $('<div tabindex="-1" class="fullView row"></div>');
                $dialogContainer.append($dialogContainer2);

                // Prevent the click events for the container to go to Container 1... prevent unintended dialog closing
                $dialogContainer2.click((e: MouseEvent) => {
                    e.stopImmediatePropagation();
                });


                $dialogContainer2.append($initialContent);
            }


            // add indeterminate wait/progress indicator elements
            var $progressIndicatorArea = $('<div tabindex="-1"></div>');
            $element.append($progressIndicatorArea);
            ko.applyBindingsToNode($progressIndicatorArea[0], { loader: { visible: value.showProgressIndicator, type: Commerce.Controls.LoaderType.Dialog } });
            dialogState.showProgressIndicator = value.showProgressIndicator;
        }

        /**
         * This will be called when the dialog is updated.
         *
         * @param {any} element The DOM element involved in this binding.
         * @param {any} valueAccessor A JavaScript function that you can call to get the current model property that is involved in this binding. Call this without passing any parameters (i.e., call valueAccessor()) to get the current model property value. To easily accept both observable and plain values, call ko.unwrap on the returned value.
         * @param {any} allBindingsAccessor A JavaScript object that you can use to access all the model values bound to this DOM element. Call allBindingsAccessor.get('name') to retrieve the value of the name binding (returns undefined if the binding doesn’t exist); or allBindingsAccessor.has('name') to determine if the name binding is present for the current element.
         * @param {any} viewModel The view model. This parameter is deprecated in Knockout 3.x. Use bindingContext.$data or bindingContext.$rawData to access the view model instead.
         * @param {any} bindingContext An object that holds the binding context available to this element’s bindings. This object includes special properties including $parent, $parents, and $root that can be used to access data that is bound against ancestors of this context.
         */
        static update(element: any, valueAccessor: any, allBindingsAccessor: any, viewModel: any, bindingContext: any): void {
            var $element = $(element);
            var dialogState: DialogState = <DialogState>(<any>$element.data(DialogHandler._dialogStateKey));
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};

            // update values accordingly

            // Show/hide the back button
            if (dialogState.$backButton) {
                var backButtonVisible: boolean = ko.utils.unwrapObservable(value.backButtonVisible) || false;
                if (backButtonVisible) {
                    dialogState.$backButton.show();
                } else {
                    dialogState.$backButton.hide();
                }
            }

            // title
            var hasTopContent: boolean = false;
            var title: string = ko.utils.unwrapObservable(value.title) || '';

            if (dialogState.$title != null) {
                dialogState.$title.text(title);

                // If there is no text for the title, then do not show the title or the wrapping space for the title
                var $wrappingTitleDiv = dialogState.$title.parent();
                if (title.length == 0) {
                    $wrappingTitleDiv.hide();
                } else {
                    $wrappingTitleDiv.show();
                    hasTopContent = true;
                }
            }

            // subTitle
            var subTitle: string = ko.utils.unwrapObservable(value.subTitle) || '';

            if (dialogState.$subTitle != null) {
                dialogState.$subTitle.text(subTitle);

                // If there is no text for the subtitle, then do not show the subtitle or the wrapping space for the subtitle
                var $wrappingSubtitleDiv = dialogState.$subTitle.parent();
                if (subTitle.length == 0) {
                    $wrappingSubtitleDiv.hide();
                } else {
                    $wrappingSubtitleDiv.show();
                    hasTopContent = true;
                }
            }

            // closeoOnEscButton
            dialogState.closeOnEscButton = ko.utils.unwrapObservable(value.closeOnEscButton) || false;

            // message
            var message: string = ko.utils.unwrapObservable(value.message) || '';

            if (dialogState.$message != null) {
                dialogState.$message.text(message);

                // If there is no text for the message, then do not show the message or the wrapping space for the message
                var $wrappingMessageDiv = dialogState.$message.parent();
                if (message.length == 0) {
                    $wrappingMessageDiv.hide();
                } else {
                    $wrappingMessageDiv.show();
                    hasTopContent = true;
                }
            }

            // Set the visibility state of the spacing between the top section and the content
            if (dialogState.$topSectionSeperatorSpace != null) {
                var $topSectionSeperatorSpace: any = dialogState.$topSectionSeperatorSpace;
                if (hasTopContent && dialogState.hasContent) {
                    $topSectionSeperatorSpace.show();
                } else {
                    $topSectionSeperatorSpace.hide();
                }
            }

            // Set the visibility state of the dialog
            var shouldBeVisible: boolean = ko.utils.unwrapObservable(value.visible) || false;
            if (shouldBeVisible) {
                DialogHandler.show(dialogState, !dialogState.initialized);
            } else {
                DialogHandler.hide(dialogState, !dialogState.initialized);
            }

            // hideOnEscape
            var hideOnEscape: boolean = ko.utils.unwrapObservable(value.hideOnEscape);
            if (ObjectExtensions.isNullOrUndefined(hideOnEscape)) {
                hideOnEscape = true;
            }
            if (hideOnEscape && dialogState.modal) {
                $element.keypress((event) => {
                    if (event.keyCode == 27) { // esc key is pressed
                        DialogHandler.backgroundClicked(dialogState, viewModel, event);
                    }
                });
            }

            dialogState.initialized = true;
        }

        /**
         * Sorts the elements by tabindex
         * @param {JQuery} jquery elements.
         * @return {JQuery} Sorted Jquery elements.
         */
        private static sortByTabindex(array: JQuery): JQuery {
            if (!array || array.length <= 1) {
                return array;
            }

            return (<any>array).sort((a: Element, b: Element) => {
                var tabindex1 = Number(a.getAttribute('tabindex'));
                var tabindex2 = Number(b.getAttribute('tabindex'));
                if (isNaN(tabindex1) || tabindex1 < 0)
                    return 1;
                else if (isNaN(tabindex2) || tabindex2 < 0)
                    return -1;
                else
                    return tabindex1 - tabindex2;
            });
        }

    }
}


/**
 * Binding handlers
 */

/**
 * DefaultDialog
 *
 * Displays a dialog.
 * There is no back button or reserved space for the back button
 */
ko.bindingHandlers.dialog = {
    init: function (element: any, valueAccessor: any, allBindingsAccessor: any, viewModel: any, bindingContext: any): any {
        ko.applyBindingsToDescendants(bindingContext, element);
        Commerce.Controls.Dialog.DialogHandler.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        return { controlsDescendantBindings: true };
    },
    update: function (element: any, valueAccessor: any, allBindingsAccessor: any, viewModel: any, bindingContext: any): any {
        Commerce.Controls.Dialog.DialogHandler.update(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        return { controlsDescendantBindings: true };
    }
};