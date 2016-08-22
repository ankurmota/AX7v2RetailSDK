/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='../KnockoutJS.d.ts'/>

/*
 *  InvokeValidate knockout binding extension to enable HTML5 form validation.
 *  Must be bind to the button which submits the form.
 *  Button must be enclosed to the form for this binding extension to work.
 *
 *  Following binding example makes button click to perform following:
 *      1. Invoke HTML5 validation before the form submission.
 *      2. Invoke "SaveCustomer" method on the form submission.
 *
 *  <form>
 *      ...
 *      <button class="iconSaveBig" data-bind="invokeValidate: SaveCustomer" />
 *  </form>
 */

ko.bindingHandlers.invokeValidate = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var methodToInvoke: () => void = ko.utils.unwrapObservable(valueAccessor());
        if (typeof methodToInvoke !== 'function') {
            throw "invokeValidate binding value must be a function.";
        }

        var formElementQuery: JQuery = $(element).closest("form");
        if (formElementQuery == null || formElementQuery[0] == null) {
            throw "Button with invokeValidate binding must be enclosed into a form.";
        }

        // In order to enable HTML5 validation, button with "type=submit" enclosed to the form
        // must be clicked. So button type is explicitly changed here.
        element.type = "submit";

        // Set event for button click.
        $(element).click(function () {
            var formElement: HTMLElement = formElementQuery[0];

            // Set onsubmit handler to function provided in binding.
            // If HTML5 validation fails, onsubmit callback is NOT called.
            formElement.onsubmit = function () {
                methodToInvoke.call(viewModel);

                // Must always return false not to submit the form.
                return false;
            };
        });
    }
}
