/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../KnockoutJS.d.ts'/>

/*
 * Validator knockout binding extension.
 *
 *
 * Validator binding usage example:
 * validator: { 
 *          data: customerAddEditViewModel.CustomerProxy,
 *          field: FirstName,
 *          validatorType: CustomerValidator
 *        }
 *
 * @param {any} [data] - instance of entity object to bind data to. If not provided, binding context is used to bind data to given field.
 * @param {string} field - field of entity object to bind data to.
 * @param {string} validatorType - validator class name to be used to validate the entity.
 * @param {string} [validatorField] - name of validatorType class field to be used for validation. If not provided - field attribute is used instead.
 *
 */

module Commerce.Controls.Validator {
    export interface IValidatorOptions {
        data?: any;
        field: string;
        validatorType: string;
        validatorField?: string;
    }
}

ko.bindingHandlers.validator = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var binding: Commerce.Controls.Validator.IValidatorOptions = ko.utils.unwrapObservable(valueAccessor()) || {};

        // Optional, in case user wants to combine input value binding with validation.
        if (!Commerce.StringExtensions.isNullOrWhitespace(binding.field)) {
            // Apply value data-bind.
            // if "data" attribute is provided we use it's field directly, otherwise use field of the object from binding context.
            var valueObject = binding.data ? binding.data[binding.field] : bindingContext.$data[binding.field];
            if (Commerce.ObjectExtensions.isNullOrUndefined(valueObject)) {
                throw "Unable to get '" + binding.field + "' property. Make sure that validator binding has either data attribute provided or has proper context.";
            }

            ko.applyBindingsToNode(element, { value: valueObject, valueUpdate: 'afterkeydown' });
        }

        // Set validation attributes.
        if (Commerce.ObjectExtensions.isNullOrUndefined(binding.validatorType)) {
            throw "validatorType attribute is not provided for validator binding.";
        }

        var validator = Object.create(Commerce.Model.Entities[binding.validatorType].prototype);
        validator.constructor.apply(validator);

        var field = binding.validatorField ? binding.validatorField : binding.field;
        validator.setValidationAttributes(element, field);
    }
}
