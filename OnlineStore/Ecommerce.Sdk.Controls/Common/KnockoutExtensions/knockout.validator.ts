/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../KnockoutJS.d.ts" />
/// <reference path="../Helpers/Utils.ts" />
/// <reference path="../../Resources/Resources.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
    /*
     * Validator knockout binding extension.
     *
     *
     * Validator binding usage example:
     * validator: { 
     *          data: customerAddEditViewModel.CustomerProxy,
     *          field: 'FirstName',
     *          validatorType: 'CustomerValidator'
     *          validate: function(srcElement) { 
     *              return srcElement.value > 5; // Do BL validation.
     *          }
     *        }
     *
     * @param {any} [data] - instance of entity object to bind data to. If not provided, binding context is used to bind data to given field.
     * @param {string} field - field of entity object to bind data to.
     * @param {string} validatorType - validator class name to be used to validate the entity.
     * @param {string} [validatorField] - name of validatorType class field to be used for validation. If not provided - field attribute is used instead.
     * @param {(element: Element) => {}} [validate] - custom validation method. Optional.
     *
     */
    export interface IValidatorOptions {
        data?: any;
        field: string;
        validatorType: string;
        validatorField?: string;
        validate?: (element: Element) => {}; // Customer validation method.
    }

    /*
     * Validator helper knockout binding extension.
     *
     *
     * Validator binding usage example:
     * submitIfValid: { 
     *          containerSelector: '.someJQuerySelector',
     *          submit: function(eventObject) { 
     *              // Do something.
     *          }
     *        }
     *
     * @param {string} containerSelector - JQuery selector to identify container element content of which needs to be validated.
     * @param {(eventObject: JQueryEventObject) => {}} submit - submit method to execute once validation complete.
     *
     */
    export interface ISubmitIfValidOptions {
        containerSelector: string;
        submit: (eventObject: JQueryEventObject) => {};
        validate?: (element: Element) => {}; // Customer validation method.
    }
}

ko.bindingHandlers.validator = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var binding: Contoso.Retail.Ecommerce.Sdk.Controls.IValidatorOptions = ko.utils.unwrapObservable(valueAccessor()) || {};        

        // Optional, in case user wants to combine input value binding with validation.
        if (!Contoso.Retail.Ecommerce.Utils.isNullOrWhiteSpace(binding.field)) {
            // Apply value data-bind.
            // if "data" attribute is provided we use it's field directly, otherwise use field of the object from binding context.
            var valueObject = binding.data ? binding.data[binding.field] : bindingContext.$data[binding.field];

            var observableValueObject;
            if (ko.isObservable(valueObject)) {
                observableValueObject = valueObject;
            }
            else {
                // To handle not initialized field we create new observable to set value.
                observableValueObject = ko.observable(valueObject);

                observableValueObject.subscribe((newValue) => {
                    if (Contoso.Retail.Ecommerce.Utils.isNullOrUndefined(binding.data)) {
                        bindingContext.$data[binding.field] = newValue;
                    }
                    else {
                        binding.data[binding.field] = newValue;
                    }
                });
            }     

            ko.applyBindingsToNode(element, { value: observableValueObject });
        }

        // Set validation attributes.
        if (Contoso.Retail.Ecommerce.Utils.isNullOrUndefined(binding.validatorType)) {
            throw Contoso.Retail.Ecommerce.Sdk.Controls.Resources.String_71;
        }

        var validator = Object.create(Contoso.Retail.Ecommerce.Sdk.Controls[binding.validatorType].prototype);
        validator.constructor.apply(validator);

        var field = binding.validatorField ? binding.validatorField : binding.field;
        validator.setValidationAttributes(element, field);

        var $element = $(element);
        $element.attr("msax-isValid", true);

        // In HTML5 forms not allowed to be placed in another form. 
        // With ASP.NET page form tag already exists on the host page.
        // Since we do not have a form tag wrapping fields that are groupped together
        // we manually have to find the wrapping container and validate.
        $element.change((eventObject) => {

            // If the current target element does not have the check validity function add the function.
            if (!('checkValidity' in eventObject.currentTarget)) {
                eventObject.currentTarget.checkValidity = function () {
                    var valid = true,
                        required = eventObject.currentTarget.getAttribute("required"),
                        minLength = eventObject.currentTarget.getAttribute("minlength"),
                        maxLength = eventObject.currentTarget.getAttribute("maxlength"),
                        pattern = eventObject.currentTarget.getAttribute("pattern"),
                        value = eventObject.currentTarget.value,
                        type = eventObject.currentTarget.getAttribute("type"),
                        option = (type === "checkbox" || type === "radio");

                    // Do not validate if the field is disabled.
                    if (eventObject.currentTarget.disabled) {
                        return valid;
                    }

                    // Check if the required attribute is set and if it is then check if value exists.
                    valid = valid && (!required ||
                    (option && eventObject.currentTarget.checked) ||
                    (!option && value !== ""));

                    // Check if minlength or maxlength attributes are set and check value length accordingly.
                    valid = valid && (option ||
                    ((!minLength || value.length >= minLength) &&
                    (!maxLength || value.length <= maxLength)));

                    // Check if pattern attribute exists and test pattern if it does.
                    if (valid && pattern) {
                        pattern = new RegExp(pattern);
                        valid = pattern.test(value);
                    }

                    return valid;
                }
            }

            // Invoke HTML5 validation.
            var isValid = eventObject.currentTarget.checkValidity();

            // For dropdowns when an option is selected for the first time HTML5 validation fails. To prevent this we check for selected index manually.
            // Index 0 is the default value and change event is not triggered when it is selected the first time 
            // and if '0' selected again later then HTML5 validation would have passed already so this check does not affect it.
            if (eventObject.currentTarget.type === "select-one" && eventObject.currentTarget.selectedIndex != 0) {
                isValid = true;
            }
           
            // Custom validation.
            if (isValid && binding.validate) {
                try {
                    isValid = binding.validate.call(viewModel, eventObject.currentTarget);
                }
                catch (ex) {
                    // The validation should fail if there is any error.
                    isValid = false;
                }
            }

            $element.attr("msax-isValid", isValid);

            // For radio button style for invalid field will be done by label.
            if (eventObject.currentTarget.type === "radio") {
                var $label = $element.parent().find("[for=" + eventObject.currentTarget.id + "]");
                $label.attr("msax-isValid", isValid);
            }

            // Sets title attribute to an element if the validation on element fails.
            // The title attribute will indicate what the expected value for element is.
            if (!Contoso.Retail.Ecommerce.Utils.isNullOrWhiteSpace(validator[field])) {
                validator[field].setTitleAttributeIfInvalid(element);
            }

            // Let other handlers to execute to support checkedValue for radio buttons.
            return isValid;
        });

    }
};

ko.bindingHandlers.submitIfValid = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var binding: Contoso.Retail.Ecommerce.Sdk.Controls.ISubmitIfValidOptions = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);

        $element.click((eventObject: JQueryEventObject) => {
            eventObject.preventDefault();

            var container: string;
            // Get the latest value if the container selector is an observable else use the string directly.
            if (Contoso.Retail.Ecommerce.Utils.isNullOrWhiteSpace(binding.containerSelector) || binding.containerSelector.length == 0) {
                var containerObservable: any = binding.containerSelector;
                container = containerObservable();
            }
            else {
                container = binding.containerSelector;
            }

            // Try find in the parent stack.
            var $wrapper = $element.closest(container);

            // If element is not one of the parents, search entire page.
            if ($wrapper.length === 0) {
                $wrapper = $(container);
            }

            // Trigger change to invoke validators.
            $wrapper.find("input,select").each((index, elem) => {
                $(elem).change();
            });

            // Find all invalid fields
            var $invalidFields = $wrapper.find("[msax-isValid=false]");

            // Select first invalid field.
            $invalidFields.first().focus();
            $invalidFields.first().select();

            // Call submit method if there are no invalid fields.
            if ($invalidFields.length === 0) {
                var isValid = true;
                if (binding.validate) {
                    isValid = binding.validate.call(viewModel, $wrapper);
                }

                if (isValid) {
                    binding.submit.call(viewModel, eventObject);
                }
            }
        });
    }
};