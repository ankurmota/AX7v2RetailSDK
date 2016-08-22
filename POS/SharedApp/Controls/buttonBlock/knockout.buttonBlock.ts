/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='../../Core/Core.winjs.ts' />

/*
    buttonBlock control.
 *
 * The buttonBlock control contains a button that is styled by an icon class and an optional label either to the left or right of the button.
 * The buttonBlock provide styling for focus and behavior on enter or click. The control is specified in a containing div element and will
 * replace the containing element.
 */
/*
* dialog binding usage example:
* <div data-bind="buttonBlock: { 
            click: doSomething,
            buttonClass: buttonIcon,
            labelTextResx: 'string_1153'
          }
  </div>

   @/// <param name="buttonClass" type="string">The button class (ex. buttonIcon) to apply to the button. If ButtonBlockSizes is set to None, then the icon as specified is used, otherwise the icon name will have the size extension added to it (ex. Big will be added to iconEdit).</param>
   @/// <param name="click" type="() => void"> (Optional) The method to call on click or enter.</param>
   @/// <param name="labelTextResx" type="string">(Optional) The resource string id of the text to apply to the label. If not specified the label is not displayed.</param>
   @/// <param name="size" type="ButtonBlockSizes"> (Optional) Allows specification of a size for the button. Currently specifies the size of the label text of the button.</param>
   @/// <param name="maxWidth" type="number"> (Optional) Maximum width of the button block in ems. Used to compute max width of the label to prevent overflow.</param>
   @/// <param name="tabIndex" type="number"> (Optional) The tab index of the control. If not specified, defaults to 0.</param>
   @/// <param name="ariaLabelResx" type="string"> (Optional) The resource string of the aria label to display. If not specified, defaults to the label text.</param>
   @/// <param name="labelValue" type="string"> (Optional) The string that is directly used for label if the resource is not available.</param>
   @/// <param name="loading" type="boolean"> (Optional) The flag that indicates that loader animation should be shown instead of icon. </param>
   @/// <param name="blockType" type="ButtonBlockType"> (Optional) Indicates the type of button block. Might be text or button.</param>
* */

module Commerce {
    "use strict";


    export module ButtonBlock {
        /**
         * List of button block size specifications.
         */
        export enum ButtonBlockSizes {
            Standard, // Specifies 5em height. Uses a H4 tag for the label. Best used with normal sized icons. This is the default size.
            Big, // Specifies 6em height. Uses a H3 tag for the label. Best used with big sized icons.
            None // Do not specify any sizing changes. H3 tag will be used for the label and any sizing specifications for the containing div will be used.
        }

        /**
         * The type of button block.
         */
        export enum ButtonBlockType {
            Text = 1, //Removes cursor pointer. Removes padding left/right 1em depending on orientation. 
            Button = 2 // General button block. 
        }

        export interface IButtonBlockOptions {
            buttonClass: string | Observable<string>;
            click?: () => void;
            labelTextResx?: string;
            size?: ButtonBlockSizes;
            maxWidth?: number;
            tabIndex?: number;
            ariaLabelResx?: string;
            labelValue?: string | Observable<string>;
            loading?: Observable<boolean>;
            blockType?: ButtonBlockType;
        }
    }
}


/**
 * Binding handlers
 */

/**
 * buttonBlock
 *
 * Displays a button block.
 */
ko.bindingHandlers.buttonBlock = {

    /**
     * Creates after render event handler.
     * @param click. Button block click event handler
     * @param viewModel Binding viewModel of button block.
     * @returns {} 
     */
    createAfterRender(click: () => void, viewModel: any): (element: HTMLElement) => void {
        if (Commerce.ObjectExtensions.isNullOrUndefined(click)) {
            return null;
        }

        return (element: HTMLElement) => {
            var $element = $(element);

            // Set the behavior of click on the button block
            $element.click((event: Event) => {
                click.call(viewModel);
                event.preventDefault();
                event.stopImmediatePropagation();
            });

            // Add the keydown event handler
            $element.keydown((event: KeyboardEvent) => {
                var anyKeyPressHandled: boolean = false;

                // Handle the enter keyboard event
                if ((event.keyCode === 13 || event.keyCode === 108)) { // 13 is ENTER key and 108 is ENTER key on the numpad.
                    $element.click();
                    anyKeyPressHandled = true;
                }

                // If any operation handled an event then don't execute the default behavior for the event
                if (anyKeyPressHandled) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                }
            });
        };
    },

    init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value: Commerce.ButtonBlock.IButtonBlockOptions = ko.utils.unwrapObservable(valueAccessor()) || {};
        value.blockType = value.blockType || Commerce.ButtonBlock.ButtonBlockType.Button;

        // Get the sizing information
        var size: Commerce.ButtonBlock.ButtonBlockSizes = value.size || Commerce.ButtonBlock.ButtonBlockSizes.Standard;
        var buttonPostfix:string = size === Commerce.ButtonBlock.ButtonBlockSizes.Big ? "Big" : Commerce.StringExtensions.EMPTY;
        var buttonClass: Computed<string> = ko.computed<string>(() => {
            return ko.utils.unwrapObservable(value.buttonClass) + buttonPostfix;
        });
        
        // Get the label
        var label: any;
        var maxLabelWidthClass:string = "";
        if (value.labelTextResx) {
            label = Commerce.ViewModelAdapter.getResourceString(value.labelTextResx);

            // Add the maximum width of the label to the label parent element to limit the maximum label width
            if (value.maxWidth) {
                maxLabelWidthClass = "maxWidth" + (value.maxWidth - 7).toString(); // 7 is computed by... 1 pad at the start and end of the button block + 4 for icon width + 1 pad between the icon and the text
            }
        }

        // Get the aria-label
        var ariaLabel: string;
        if (value.ariaLabelResx) {
            ariaLabel = Commerce.ViewModelAdapter.getResourceString(value.ariaLabelResx);
        } else if (label) {
            ariaLabel = label;
        }

        if (!label && !Commerce.ObjectExtensions.isNullOrUndefined(value.labelValue)) {
            label = value.labelValue;
        }

        // Set the data for binding
        var buttonBlockData = {
            tabIndex: value.tabIndex || 0,
            buttonClass: buttonClass,
            label: label || "",
            labelVisible: !Commerce.ObjectExtensions.isNullOrUndefined(label),
            standardButtonBlock: size === Commerce.ButtonBlock.ButtonBlockSizes.Standard,
            bigButtonBlock: size === Commerce.ButtonBlock.ButtonBlockSizes.Big,
            maxLabelWidthClass: maxLabelWidthClass,
            ariaLabel: ariaLabel || "",
            loading: value.loading || ko.observable(false),
            blockType: value.blockType
        };

        var afterRender: (element: HTMLElement) => void =
            value.blockType === Commerce.ButtonBlock.ButtonBlockType.Text ? null :
                ko.bindingHandlers.buttonBlock.createAfterRender(value.click, viewModel);

        ko.applyBindingsToNode(element, {
            template: {
                name: "buttonBlockTemplate",
                data: buttonBlockData,
                afterRender: afterRender
            }
        });

        return { controlsDescendantBindings: true };
    }
};