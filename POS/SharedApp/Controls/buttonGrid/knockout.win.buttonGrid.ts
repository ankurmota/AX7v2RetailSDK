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
 *  buttonGrid knockout binding extension
 *
 *
 * buttongrid binding usage example:
 * buttonGrid: { 
 *          visible: paymentButtonGridVisible,
 *          data: paymentButtonGrid,
 *          buttonClick: ClickCallback,
 *          callbackContext: someContext,
 *        }
 *
 * @param {function} buttonClick Function that gets excecuted with each button click. It is passed the operationId of the clicked button.
 * @param {number} buttonWidth The width of a button/column.
 * @param {number} buttonHeight The height of a button/row.
 * @param {ButtonGrid} data ButtonGrid entity.
 * @param {number} margin Margin applied to the button.
 * @param {boolean} visible Determines if buttongrid is visible or not.
 *
 */

module Commerce.Controls.ButtonGrid {
    export class ButtonGridState {
        buttonClick: (action: number, actionProperty: string) => boolean;
        buttonWidth: number;
        buttonHeight: number;
        callbackContext: any;
        id: string;
        margin: number;
        $buttonsContainer: JQuery;
        $headerContainer: JQuery;
        showName: boolean;
    }

    /* Extends ButtonGridButton to allow the button grid to return data associated to a button on click of the button */
    export interface ButtonGridButtonExtended extends Commerce.Model.Entities.ButtonGridButton {
        Data?: any;
    }
}

ko.bindingHandlers.buttonGrid = (() => {
    "use strict";

    var _buttonGridStateKey: string = "commerceButtonGridState";
    var _buttonOptionsKey: string = "commerceButtonGridButtonOptions";
    var _buttonWidth: number = 80; // width of a buttongrid button in pixels
    var _buttonHeight: number = 80; // height of a buttongrid button in pixels
    var _margin: number = 10; // buttongrid button right and bottom margins in pixels
    var _scrollBarOverlapOffset: number = 40;

    /* This function handles clicks from buttongrid buttons and invokes the buttonClick callback on the view model/controller if one was provided */
    var buttonClick = function (e: JQueryEventObject): void {
        e.stopPropagation();

        var viewModel: any = this;

        var $button = $(e.currentTarget);
        var $element = $button.closest('.commerceButtonGrid');
        var buttonGridState: Commerce.Controls.ButtonGrid.ButtonGridState = <any>$element.data(_buttonGridStateKey);
        var buttonData: Commerce.Controls.ButtonGrid.ButtonGridButtonExtended = <Commerce.Controls.ButtonGrid.ButtonGridButtonExtended>$button.data(_buttonOptionsKey);

        // Unfocus from button after pressed. (Required to properly handle keyboard wedge based MSR)
        $button.blur();

        if (buttonGridState == null || buttonData == null) return;

        var handled: boolean = false;
        if (typeof (buttonGridState.buttonClick) == 'function') {
            handled = buttonGridState.buttonClick.call(viewModel, buttonData.Action, buttonData.ActionProperty, buttonData.Data);
        }

        if (!handled) {
            handled = commonOperationsHander(buttonData.Action, buttonData.ActionProperty, buttonData, buttonGridState, $element);

            if (!handled) {
                Commerce.NotificationHandler.displayErrorMessage("string_29802");

            }
        }
    };

    /*
    * Common operations handler.
    *
    * @param {number} action This is the operation identifier or action.
    * @param {string} actionProperty Action property.
    * @param {boolean} handled Indicates that the action/operation has been handled by the user.
    * @returns {boolean} True if this function handles the action/operation, false otherwise.
    */
    var commonOperationsHander = (action: number, actionProperty: string, buttonData: Commerce.Model.Entities.ButtonGridButton, buttonGridState: Commerce.Controls.ButtonGrid.ButtonGridState, $element: any): boolean => {
        switch (action) {
        case Commerce.Operations.RetailOperation.Submenu:
            var buttonGrid = Commerce.ApplicationContext.Instance.tillLayoutProxy.getButtonGridById(actionProperty);
            if (buttonGrid) {
                render($element, buttonGridState, buttonGrid, buttonGridState.showName, true);
            }
            return true;
        }

        return false;
    }

    var render = ($element: any, buttonGridState: Commerce.Controls.ButtonGrid.ButtonGridState, buttonGrid: Commerce.Model.Entities.ButtonGrid, showName: boolean, visible: boolean): void => {
        if (visible) {
            // header
            buttonGridState.$headerContainer.empty();
            var $header = $("<h2>").text(buttonGrid.Name);
            buttonGridState.$headerContainer.append($header);
            if (showName && !Commerce.StringExtensions.isNullOrWhitespace(buttonGrid.Name)) {
                buttonGridState.$headerContainer.removeClass("hide");
            }

            var index = 0;

            // render buttons
            buttonGridState.$buttonsContainer.empty();
            var columns = 1; // grid max rows
            var rows = 1; // grid max columns
            buttonGrid.Buttons.forEach((button: Commerce.Proxy.Entities.ButtonGridButton) => {
                // create button
                var $button = $('<button></button>');
                $button.data(_buttonOptionsKey, button);
                var buttonClass = 'button' + index;

                $button.addClass(Commerce.StringExtensions.format("accentBackground highContrastBorder pad0 margin0 positionAbsolute {0}", buttonClass));
                $button.attr('data-action', button.Action);
                // text content
                var $displayText = $('<div class="left1 bottom05 textLeft padRight1"><h4 class="margin0"></h4></div>');
                $displayText.find("h4").text(button.DisplayText);
                $button.append($displayText);

                // Work around background color. The '!important' flag cannot be set usign jQuery directly. Using cssText as a work around.
                // Background properties should be combined into one string.
                var cssText = ''; 

                // image, applied to button background
                if (!Commerce.StringExtensions.isNullOrWhitespace(button.PictureAsBase64)) {
                    cssText += " background-image:url('data:image;base64," + button.PictureAsBase64 + "');";
                }

                // background color
                if (!Commerce.ObjectExtensions.isNullOrUndefined(button.BackColorAsARGB) &&
                    button.BackColorAsARGB.A !== 0) {
                    cssText += ' background-color:' + Commerce.CSSHelpers.colorToRGBAStyle(button.BackColorAsARGB) + ' !important;'; // jQuery removes !important, cssText is the work around.
                }

                $button.css('cssText', cssText);

                // position
                button.Column = button.Column || 1;
                button.Row = button.Row || 1;
                button.ColumnSpan = button.ColumnSpan || 1;
                button.RowSpan = button.RowSpan || 1;
                var directionStyleName = !Commerce.CSSHelpers.isRightToLeft() ? "left" : "right";

                var styles = {
                    top: (button.Row - 1) * buttonGridState.buttonHeight,
                    width: (buttonGridState.buttonWidth * button.ColumnSpan) - buttonGridState.margin,
                    height: (buttonGridState.buttonHeight * button.RowSpan) - buttonGridState.margin
                }
                styles[directionStyleName] = (button.Column - 1) * buttonGridState.buttonWidth;

                $button.css(styles);

                // get max columns and rows for the grid
                var buttonColumn: number = button.Column + (button.ColumnSpan - 1);
                var buttonRow: number = button.Row + (button.RowSpan - 1);
                columns = buttonColumn > columns ? buttonColumn : columns;
                rows = buttonRow > rows ? buttonRow : rows;

                // click
                $button.click(buttonClick.bind(buttonGridState.callbackContext));

                buttonGridState.$buttonsContainer.append($button);
                $button.attr("data-ax-bubble", Commerce.StringExtensions.format("{0}_{1}", buttonGridState.id, index));
                index++;
            }); // end of buttons loop
            
            //don't include margin for the last button row/column
            var containerHeight: number = buttonGridState.buttonHeight * rows  - buttonGridState.margin;
            var containerWidth: number = buttonGridState.buttonWidth * columns - buttonGridState.margin;

            var containerStyles = {
                height: containerHeight,
                width: containerWidth
            }
            buttonGridState.$buttonsContainer.css(containerStyles);
            // show control
            $element.show();
        } else {
            $element.hide();
        }
    }

    return {
        init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var $element = $(element);
            $element.addClass('commerceButtonGrid'); // control root CSS class
            $element.empty();
            // id attribute must be set on the control
            if (value.id) {
                $element.attr('id', value.id);
            }
            var id = $element.attr('id');
            if (!id) {
                Commerce.ViewModelAdapter.displayMessage("buttonGrid control requires a unique Id", Commerce.MessageType.Error);
                return;
            }

            // set defaults and copy options 
            var buttonGridState: Commerce.Controls.ButtonGrid.ButtonGridState = {
                buttonClick: value.buttonClick,
                buttonWidth: value.buttonWidth || _buttonWidth,
                buttonHeight: value.buttonHeight || _buttonHeight,
                callbackContext: value.callbackContext || bindingContext.$root,
                id: id,
                margin: value.margin || _margin,
                $buttonsContainer: null,
                $headerContainer: null,
                showName: value.showName
            };

            // header container
            var $headerContainer = $('<div></div>').addClass("titleAboveControl padBottom1 hide");
            $element.append($headerContainer);
            buttonGridState.$headerContainer = $headerContainer;

            // buttons container
            var $buttonsContainer = $('<div class="buttonsContainer positionRelative"></div>');
            $element.append($buttonsContainer);
            buttonGridState.$buttonsContainer = $buttonsContainer;

            $element.data(_buttonGridStateKey, buttonGridState);
        },
        update(element, valueAccessor, allBindingsAccessor, viewModel) {
            var $element = $(element);
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var buttonGrid: Commerce.Model.Entities.ButtonGrid = <Commerce.Model.Entities.ButtonGrid>ko.utils.unwrapObservable(value.data);

            if (Commerce.ObjectExtensions.isNullOrUndefined(buttonGrid)) return;

            var buttonGridState: Commerce.Controls.ButtonGrid.ButtonGridState = <any>$element.data(_buttonGridStateKey);
            if (Commerce.ObjectExtensions.isNullOrUndefined(buttonGridState) ||
                Commerce.ObjectExtensions.isNullOrUndefined(buttonGridState.id)) {
                return;
            }

            var visible: boolean = value.visible == null ? true : ko.utils.unwrapObservable(value.visible);

            value.showName = value.showName || (<any>buttonGrid).DisplayTitleAboveControl;

            render($element, buttonGridState, buttonGrid, value.showName, visible);
        }
    };

})();
