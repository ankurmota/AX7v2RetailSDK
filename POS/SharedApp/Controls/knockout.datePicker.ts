/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
 * Wrapper for WinJS.UI.DatePicker.
 * The purpose for creating it is that WinJS.UI controls doesn't work 
 * when using knockout templates.
 * see WinJS.UI.DatePicker http://msdn.microsoft.com/en-us/library/windows/apps/br211675.aspx
 * 
 * 
 * datePicker binding usage examples:
 * datePicker:{ datePattern: '{day.integer(2)} {dayofweek.full}', current: EndDateTime, disabled: !$parent._isEndDateEnabled }
 *
 * Parameters shared by all numpad types:
 * @param {string} datePattern date format.
 * @param {Date} current. Value for the controls
 * @param {boolean} disabled. Indicates whether the control disabled.
 */
ko.bindingHandlers.datePicker = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = ko.utils.unwrapObservable(valueAccessor()) || {};
        var dateControl = new WinJS.UI.DatePicker(element);
        ko.applyBindingsToNode(element, { winControl: { disabled: value.disabled, current: value.current, datePattern: value.datePattern } }, viewModel);
    }
} 