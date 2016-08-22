/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
CUSTOMER CARD
Example usage:

DYNAMICALLY CONTROLLING THE LAYOUT OF THE CUSTOMER CARD:
Simply specify the name of the template to use to generate the customer card

Example usage:
<div data-bind="customerCard: {control: _customerCardControl, template: 'customerPrimaryAddressCardTemplate'}"></div>

TEMPLATES (MAIN TEMPLATE):
    (template name | layoutData applicable?)

    'customerDetailsCardTemplate' | Yes
        - displays the full customer card, which is a combination of each of the modular templates below.

        MANUALLY CONTROL THE CUSTOMER CARD LAYOUT:
        Alternately, you can supply values manually when implementing the customer card in various places.

        Example layout data object (this is the default data given to this template if no layoutData binding is made):
            layoutData = {
                ShowImage: true,            // default if not set
                ShowName: true,             // default if not set
                ShowAccountNumber: true,    // default if not set
                ShowLoyaltyCard: true,      // default if not set
                ShowBalance: true,          // default if not set
                ShowCreditLimit: true,      // default if not set
                AddressType: 1              // primary address (default if not set)
            }

        Example usage:
        (layoutData contains AddressType: 2, which specifies the invoice address)

        <div data-bind="customerCard: {control: _customerCardControl, template: 'customerDetailsCardTemplate', layoutData: {AddressType: 2}}"></div>

        - Note: Only certain fields can be dynamically managed in *some* of the various customerCard modular templates. Because of this,
            layoutData only applies to either of these two templates:
                1. 'customerDetailsCardTemplate' (all layoutData fields are applicable)
                2. 'customerContactCardTemplate' (applicable layoutData fields are ShowImage, ShowName, ShowAccountNumber)


TEMPLATES (MODULAR TEMPLATES):
    (template name | layoutData applicable?)

    'customerContactCardTemplate' | Yes
        - displays the entire customer card (customer contact card, loyalty card, finance card, [primary|invoice|add address card]*)
        *an additional binding of "layoutData" must be applied to specify which address template to use,
        *see the ko.layoutData binding below for more details about which parameters to specify.

    'customerLoyaltyCardTemplate' | No
        - displays a single container with a LOYALTY CARD label and a corresponding value with which to interact with the customer
        loyalty card(s)*.
        *Not yet fully supported.

    'customerFinanceCardTemplate' | No
        - displays a single container with 2 fields: BALANCE and CREDIT LIMIT, with corresponding values

    'customerPrimaryAddressCardTemplate' | No
        - displays the customer's primary address, if any.
        - if no primary address is available, the control defaults to the customerAddAddressCardTemplate
        - (AddressType 1)

    'customerInvoiceAddressCardTemplate' | No
        - displays the customer's invoice address, if any.
        - if no invoice address is available, the control defaults to the customerAddAddressCardTemplate
        - (AddressType 2)

    'customerAddAddressCardTemplate' | No
        - displays a container with an add button to add a new address for the given customer.
        - This template displays by default when attempting to display the 'customerPrimaryAddressCardTemplate' or
        'customerInvoiceAddressCardTemplate', but the associated address is not available.

*/

ko.bindingHandlers.customerCard = (() => {

    // Render method handles which template to load and allocates the appropriate data for that template
    function render(customer, data, templateName, templateElement) {

        // find the customer's primary address and populate the primary address observable in
        // the customerCard controller
        data.customerPrimaryAddress(
            Commerce.ArrayExtensions.firstOrUndefined(customer.Addresses,
                (address: Commerce.Model.Entities.Address): boolean => {
                    return address.IsPrimary;
                }));
        // find the customer's invoice address and populate the invoice address observable in
        // the customerCard controller
        data.customerInvoiceAddress(
            Commerce.ArrayExtensions.firstOrUndefined(customer.Addresses,
            (address: Commerce.Model.Entities.Address): boolean => {
                return address.AddressTypeValue === Commerce.Model.Entities.AddressType.Invoice;
            }));

        if ((templateName === "customerPrimaryAddressCardTemplate"
            && Commerce.ObjectExtensions.isNullOrUndefined(data.customerPrimaryAddress()))
            || (templateName === "customerInvoiceAddressCardTemplate"
            && Commerce.ObjectExtensions.isNullOrUndefined(data.customerInvoiceAddress()))) {
            templateName = "customerAddAddressCardTemplate";
        }

        // wrap the data to bind in a new observable
        var templateData = ko.observable(data);

        // apply template binding after customer object is successfully retrieved
        ko.applyBindingsToNode(templateElement, {
            template: { name: templateName, data: templateData }
        });
    }
    return {
        init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var templateElement = element;
            var data = value.data;
            var templateName = value.template;
            if (Commerce.ObjectExtensions.isNullOrUndefined(data)) {
                // fail gently
                Commerce.RetailLogger.viewsControlsKnockoutCustomerCardDataPropertyRequired();
                return { controlsDescendantBindings: true };
            }
  
            var $element = $(element);

            // only 'customerDetailsCardTemplate' or 'customerContactCardTemplate' are usable with layoutData
            if (templateName === 'customerDetailsCardTemplate'
                || templateName === 'customerContactCardTemplate') {
                var layoutData = null;
                if (!Commerce.ObjectExtensions.isNullOrUndefined(data.layoutData)) { // if layout was passed through data
                    layoutData = data.layoutData;
                } else if (!Commerce.ObjectExtensions.isNullOrUndefined(value.layoutData)){ // if layout was explicitly set on element
                    layoutData = value.layoutData;
                }

                if (!Commerce.ObjectExtensions.isNullOrUndefined(layoutData)) {
                    $element.attr('layoutData', '');
                    $element.data('layoutData', layoutData);
                }
            }
            var customer = ko.utils.unwrapObservable(data.customer);
            if (!Commerce.ObjectExtensions.isNullOrUndefined(customer)) {
                render(customer, data, templateName, templateElement);
            }
            
            if (ko.isObservable(data.customer)) {
                // set up a subscription to the customerCard controller to wait for
                // the customer observable to be updated before proceeding with the template binding
                data.customer.subscribe((updatedCustomer) => {
                    if (!Commerce.ObjectExtensions.isNullOrUndefined(updatedCustomer)
                        && !Commerce.StringExtensions.isNullOrWhitespace(updatedCustomer.Name)) {
                        render(updatedCustomer, data, templateName, templateElement);
                    }
                }, this);
            }

            // prevent the template from being bound again for nested templates
            return { controlsDescendantBindings: true };
        }
    }
})();

ko.bindingHandlers.customerLoyaltyCard = (() => {
    function render(viewModel, loyaltyCardDataArray, $element) {

        var loyaltyCardCount: number = Commerce.ArrayExtensions.hasElements(loyaltyCardDataArray) ? loyaltyCardDataArray.length : 0;
        var isLabelActionable: boolean = false;
        var loyaltyCardActionLabel: string;

        // determine whether the loyalty card data could be actionable
        if (!viewModel.isLoyaltyCardDataReadOnly()) {

            // Check whether there is a loyalty card associated with the cart
            var loyaltyCardIdOnCart: string = Commerce.Session.instance ? (Commerce.Session.instance.cart ? Commerce.Session.instance.cart.LoyaltyCardId : null) : null;

            // Clears any binding and action
            $element.unbind("click");
            viewModel.isLoyaltyCardLabelActionable(false);

            if (loyaltyCardCount > 1) {
                // If the customer has multiple loyalty cards, set the action to "Choose loyalty card"
                loyaltyCardActionLabel = Commerce.ViewModelAdapter.getResourceString("string_3269");  // Choose loyalty card

                if (!Commerce.ObjectExtensions.isNullOrUndefined(viewModel.chooseCardClick) && typeof (viewModel.chooseCardClick) == 'function') {
                    $element
                        .click(function () {
                            // Display the select loyalty card dialog
                            viewModel.chooseCardClick(loyaltyCardIdOnCart);
                        });
                    viewModel.isLoyaltyCardLabelActionable(true);
                }
            } else if (loyaltyCardCount == 0 && Commerce.StringExtensions.isNullOrWhitespace(loyaltyCardIdOnCart)) {
                // If the customer has no loyalty cards and no anonymous card is associated to the cart, set the action to "Issue loyalty card"
                loyaltyCardActionLabel = Commerce.ViewModelAdapter.getResourceString("string_3264");  // Issue loyalty card
                $element
                    .click(function () {
                        var options: Commerce.Operations.IIssueLoyaltyCardOperationOptions = {
                            customer: viewModel.customer()
                        };
                        // Issue a new loyalty card
                        Commerce.Operations.OperationsManager.instance.runOperation(Commerce.Operations.RetailOperation.LoyaltyIssueCard, options);
                    });
                viewModel.isLoyaltyCardLabelActionable(true);
            }

            // If there is a customer/anonymous loyalty card associated, always show its number (even if not actionable)
            if (!Commerce.StringExtensions.isNullOrWhitespace(loyaltyCardIdOnCart)) {
                viewModel.customerLoyaltyCardLabel(loyaltyCardIdOnCart);
            } else {
                viewModel.customerLoyaltyCardLabel(loyaltyCardActionLabel);
            }

        } else {
            var labelResourceString = loyaltyCardCount === 1 ?
                Commerce.ViewModelAdapter.getResourceString("string_3263") :  // {0} loyalty card
                Commerce.ViewModelAdapter.getResourceString("string_3268");   // {0} loyalty cards

            viewModel.customerLoyaltyCardLabel(Commerce.StringExtensions.format(labelResourceString, loyaltyCardCount));
            viewModel.isLoyaltyCardLabelActionable(false);
        }
    }
    return {
        init: (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) => {
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var data = value.data;
            var $element = $(element);
            // render loyalty card bindings the first time regardless of loyalty card state
            render(data, data.customerLoyaltyCards(), $element);
        },

        update: (element, valueAccessor) => {
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var data = value.data;
            var $element = $(element);

            if (!Commerce.ObjectExtensions.isNullOrUndefined(data) &&
                !Commerce.ObjectExtensions.isNullOrUndefined(data.customerLoyaltyCards)) {
                // render loyalty card bindings when the observables change
                render(data, data.customerLoyaltyCards(), $element);
            }
        }
    };
})();


/*
CUSTOMER PANEL
Example usage:

DYNAMICALLY CONTROLLING THE LAYOUT OF THE CUSTOMER CARD:
A valid TillLayout > CustomerLayout JSON object can be bound on the *parent* element of the template.

Example tillLayoutData:
    {
        __type: CustomerPanel:#Microsoft.Dynamics.Retail.TillLayoutDesigner.Contracts.Controls,
        ID: CustomerPanel,
        Type: Microsoft.Dynamics.Retail.TillLayoutDesigner.Contracts.Controls.ICustomerPanel,
        Height: 180,
        Width: 320,
        Top: 490,
        Left: 0,
        Title: Customer card,
        ShowImage: true,
        ShowName: true,
        ShowAccountNumber: true,
        ShowLoyaltyCard: false,
        ShowBalance: true,
        ShowCreditLimit: true,
        AddressType: 0 // no address to show
    }
Example usage:
<div id="CustomerPanel" data-bind="tillLayoutItem: { view: 'transactionScreenLayout', id: 'CustomerPanel' }">
    <!-- dynamic customer card template -->
    <div data-bind="template: { name: 'customerDetailsCardTemplate', data: [yourViewModel] }"></div>
</div>

MANUALLY CONTROL THE CUSTOMER CARD LAYOUT:
Alternately, you can supply values manually when implementing the customer card in various places.

Example custom-defined layout data:
    layoutData: {
        ShowImage: true,
        ShowName: true,
        ShowAccountNumber: true,
        ShowLoyaltyCard: false,
        ShowBalance: true,
        ShowCreditLimit: true,
        AddressType: 0 // no address to show
    }

Example usage when not using till layout data:

<div layoutData="{AddressType: 1}" data-bind="template: { name: 'customerDetailsCardTemplate', data: [yourViewModel] }"></div>

*/
enum CustomerPanelAddressTypeEnum {
    Primary = 1,
    Invoice = 2,
}

ko.bindingHandlers.customerPanelField = {
    init: (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) => {
        var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);

        // for proper binding when using the customer card in the till layout scenario,
        // the template's PARENT element must have a tillLayoutItem knockout binding applied.
        // this binding will search for the closest div with a tillLayoutItem attribute BY DEFAULT
        var boundLayoutData: any = $element.closest('div[tillLayoutItem]').data('tillLayoutItem') ||
            $element.closest('div[layoutData]').data('layoutData');

        var layoutData;

        if (Commerce.ObjectExtensions.isNullOrUndefined(boundLayoutData)) {
            // set default values. If no data passed, show everything in the given template by default.
            layoutData = {
                ShowImage: true,
                ShowName: true,
                ShowAccountNumber: true,
                ShowLoyaltyCard: true,
                ShowBalance: true,
                ShowCreditLimit: true,
                AddressType: 1 // primary address default
            };
        } else {
            layoutData = {
                ShowImage: boundLayoutData.ShowImage,
                ShowName: boundLayoutData.ShowName,
                ShowAccountNumber: boundLayoutData.ShowAccountNumber,
                ShowLoyaltyCard: boundLayoutData.ShowLoyaltyCard,
                ShowBalance: boundLayoutData.ShowBalance,
                ShowCreditLimit: boundLayoutData.ShowCreditLimit,
                AddressType: boundLayoutData.AddressType
            };
        }

        if (layoutData) {
            var cssBindings = {};
            for (var field in value) {
                var unwrappedField = ko.utils.unwrapObservable(value[field]);

                // handle finance fields separately.
                if (field === 'financeVisibleField') {
                    // given the string "ShowLoyaltyCard|ShowBalance|ShowCreditLimit",
                    // check if any of those fields are true; if so, display the finance container
                    var financeFieldsArray = unwrappedField.split("|");
                    for (var i = 0; i < financeFieldsArray.length; i++) {
                        if (layoutData[financeFieldsArray[i]]) {
                            $element.removeClass("hide");
                            break;
                        }
                    }
                }

                // handle the rest normally.
                var layoutDataField = layoutData[unwrappedField];
                if (!Commerce.ObjectExtensions.isNullOrUndefined(layoutDataField)) {
                    if (field === 'visibilityField') {
                        if (typeof layoutDataField === "boolean" && layoutDataField) {
                            $element.removeClass("hide");
                        }
                    }
                    else if (field === 'heightField') {
                        cssBindings["height"] = layoutDataField;
                    }
                    else if (field === 'widthField') {
                        cssBindings["width"] = layoutDataField;
                    }
                    else if (field === 'addressVisibleField') {
                        // filter out the appropriate value
                        var customerPanelAddressType = layoutDataField;

                        // address portion of the customer card is *already hidden*. It will only be shown if the address type == 1 or 2.
                        // 1 = customerPanelAddressTypeEnum.Primary
                        // 2 = customerPanelAddressTypeEnum.Invoice
                        switch (customerPanelAddressType) {
                        case CustomerPanelAddressTypeEnum.Primary:
                            // primary - display primary address only, regardless of AddressTypeValue
                            if ($element.hasClass('customerPanelPrimaryAddress')
                                && !Commerce.ObjectExtensions.isNullOrUndefined(viewModel.customerPrimaryAddress())) {
                                $element.removeClass("hide");
                            } else if ($element.hasClass('customerPanelAddAddress')
                                && Commerce.ObjectExtensions.isNullOrUndefined(viewModel.customerPrimaryAddress())) {
                                $element.removeClass("hide");
                            }
                            break;
                        case CustomerPanelAddressTypeEnum.Invoice:
                            // invoice - display invoice address only, regardless of whether it's the primary address
                            if ($element.hasClass('customerPanelInvoiceAddress')
                                && !Commerce.ObjectExtensions.isNullOrUndefined(viewModel.customerInvoiceAddress())) {
                                $element.removeClass("hide");
                            } else if ($element.hasClass('customerPanelAddAddress')
                                && Commerce.ObjectExtensions.isNullOrUndefined(viewModel.customerInvoiceAddress())) {
                                $element.removeClass("hide");
                            }
                            break;
                        }
                    }
                }
            }
            $element.css(cssBindings);
        }
    }
};

/*
 * Used to incapsulate orientation change for the tillLayout
 */
ko.bindingHandlers.customerTillLayoutCard = {
    init: (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) => {
        var value = valueAccessor();
        var $container: JQuery;

        var render = () => {
            if(!Commerce.ObjectExtensions.isNullOrUndefined($container)) {
                ko.removeNode($container[0]);
            };

            $container = $("<div />");
            $(element).append($container);

            value.data.layoutData =
            Commerce.ApplicationContext.Instance.tillLayoutProxy.getLayoutItem(value.screenName, value.panelName);
            ko.applyBindingsToNode($container[0], {
                customerCard: {
                    template: "customerDetailsCardTemplate",
                    data: value.data
                }
            }, viewModel);
        }

        render();

        Commerce.ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, () => {
            render();
        });
        return { controlsDescendantBindings: true };
    }
}
