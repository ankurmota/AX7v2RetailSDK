/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='../Core/Core.winjs.ts'/>

/**
*
* Rearranges fields in multiple columns
*
* totalsPanel binding usage example:
*            <div id="TotalsPanel1" data-bind="totalsPanel: { view: 'transactionScreenLayout' }">
*                <div class="background"></div>
*                <div class="content">
*                    <div class="fields">
*                       <!-- Same field on left and right panes necessary to support columns -->
*                       <div class="left">
*                           <div id="SomeField"></div>
*                       </div>
*                       <div class="right">
*                           <div id="SomeField"></div>
*                       </div>
*                    </div>
*                </div>
*            </div>
*/
ko.bindingHandlers.totalsPanel = (function () {
    "use strict";

    return {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var leftFieldsClass = ".left";
            var rightFieldsClass = ".right";
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var $element = $(element);

            // id attribute must be set on the control
            var id = $element.attr('id');
            if (!id) {
                Commerce.ViewModelAdapter.displayMessage("tab control requires a unique Id", Commerce.MessageType.Error);
                return;
            }

            if (value.view) {
                var $leftPanel = $element.find(leftFieldsClass);
                var $rightPanel = $element.find(rightFieldsClass);

                var item = Commerce.ApplicationContext.Instance.tillLayoutProxy.getLayoutItem(value.view, id);
                if (!Commerce.ObjectExtensions.isNullOrUndefined(item)) {   
                                    
                    if (item.LeftSelectedTotalsFields.length > 0) {
                        $leftPanel.show();
                        item.LeftSelectedTotalsFields.forEach((item: any, index: number) => {
                            var $field = $leftPanel.find("#" + item.ID);
                            $field.css({
                                visibility: "visible",
                                position: "relative"
                            });
                            $leftPanel.append($field); // Move element to put them in pproper order.
                        });
                    } else {
                        $leftPanel.hide();
                    }

                    if (item.RightSelectedTotalsFields.length > 0) {
                        $rightPanel.show();
                        item.RightSelectedTotalsFields.forEach((item: any, index: number) => {
                            var $field = $leftPanel.find("#" + item.ID);
                            $field.css({
                                visibility: "visible",
                                position: "relative"
                            });
                            $rightPanel.append($field); // Move element from left panel to right.
                        });
                    } else {
                        $rightPanel.hide();
                    }
                }

                // Add orientation switch tracker for cleanup and re-bind.
                var orientationChangedHandler = (args) => {
                    $rightPanel.children().appendTo($leftPanel);
                    $leftPanel.children().removeAttr("style"); // Remove styles in case styles were set by the control.
                    // On orientation switch remove listener just in case.
                    // Re-bind will add new one.
                    Commerce.ApplicationContext.Instance.tillLayoutProxy.removeOrientationChangedHandler(element, orientationChangedHandler);

                    // Re-bind the control with new orientation
                    ko.applyBindings(viewModel, element);
                };

                Commerce.ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, orientationChangedHandler);
            }
        }
    };
})();