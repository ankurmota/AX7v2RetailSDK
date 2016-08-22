/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Common/Helpers/EcommerceTypes.ts" />
/// <reference path="../Resources/Resources.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
    "use strict";

    export class CustomerAddress {

        // DOM elements.
        private _addressView: any;
        private _loadingDialog: any;
        private _loadingText: any;
        private _errorPanel: any;

        // Observables.
        private errorMessages: ObservableArray<string>;
        private addresses: ObservableArray<CommerceProxy.Entities.Address>;
        private areAddressesLoaded: Observable<boolean>;
        private addressDisplayEnabled: Computed<boolean>;

        constructor(element) {
    
            // Initialize DOM elements.
            this._addressView = $(element);
            this._errorPanel = this._addressView.find(" > .msax-ErrorPanel");
            this._loadingDialog = this._addressView.find('.msax-Loading');
            this._loadingText = this._loadingDialog.find('.msax-LoadingText');
            LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);
    
            // Initialize observables.
            this.errorMessages = ko.observableArray<string>([]);
            this.addresses = ko.observableArray<CommerceProxy.Entities.Address>(null);
            this.areAddressesLoaded = ko.observable<boolean>(false);
    
            // Computed observables.
            this.addressDisplayEnabled = ko.computed(() => {
                return Utils.hasElements(this.addresses());
            });
    
            // Invoke service.
            this.getCustomer();
        }

        /**
         * Closes loading dialog and displays error.
         */
        private closeDialogAndDisplayError(errorMessages: string[], isError: boolean) {
            LoadingOverlay.CloseLoadingDialog();
            this.showError(errorMessages, isError);
        }

        /**
         * Displays the error panel.
         */
        private showError(errorMessages: string[], isError: boolean) {
            this.errorMessages(errorMessages);

            if (isError) {
                this._errorPanel.addClass("msax-Error");
            }
            else if (this._errorPanel.hasClass("msax-Error")) {
                this._errorPanel.removeClass("msax-Error");
            }

            this._errorPanel.show();
            $(window).scrollTop(0);
        }

        // Service calls

        /**
         * Invokes get customer service and handles response from the service.
         */
        private getCustomer() {
            CommerceProxy.RetailLogger.customerServiceGetCustomerStarted();
            LoadingOverlay.ShowLoadingDialog();

            CustomerWebApi.GetCustomer(this)
                .done((customerResponse: CommerceProxy.Entities.Customer) => {
                    if (Utils.isNullOrUndefined(customerResponse) || Utils.isNullOrUndefined(customerResponse)) {
                        this.showError([Resources.String_209], true); // Sorry, something went wrong. An error occurred while retrieving signed-in customer's information. Please refresh the page and try again.
                    }
                    else {
                        this.addresses(customerResponse.Addresses);
                        this.areAddressesLoaded(true);
                    }

                    CommerceProxy.RetailLogger.customerServiceGetCustomerFinished();
                    LoadingOverlay.CloseLoadingDialog();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.customerServiceGetCustomerError, errors, Resources.String_209); // Sorry, something went wrong. An error occurred while retrieving signed-in customer's information. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }
    }
}