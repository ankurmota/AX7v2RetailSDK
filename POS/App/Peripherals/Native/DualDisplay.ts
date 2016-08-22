/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Peripherals.Native {
    "use strict";

    /**
     * With TypeScript compiler version 1.5, the MSApp had some methods removed.
     * This interface is not public on purpose.
     */
    interface IExtendedMSApp extends MSApp {
        createNewView(uri: string): IMSAppView;
    }

    /**
     * With TypeScript compiler version 1.5, the MSAppView was removed.
     * This interface is not public on purpose.
     */
    interface IMSAppView {
        viewId: number;
        close(): void;
        postMessage(message: any, targetOrigin: string, ports?: any): void;
    }

    interface IInteropMessage {
        type: string;
        args?: any;
    }

    export class DualDisplay implements IDualDisplay {

        private static MESSAGE_TYPE_DISPLAY_TRANSACTION: string = "displayTransaction";
        private static MESSAGE_TYPE_INITIALIZE: string = "initialize";
        private static VIEW_PATH: string = "ms-appx:///Views/Device/DualDisplayView.html";

        private _projectionStarted: boolean;
        private _viewManagement: any;
        private _secondaryView: IMSAppView;
        private _deviceConfiguration: Model.Entities.DeviceConfiguration;

        constructor() {
            this._projectionStarted = false;
            this._viewManagement = Windows.UI.ViewManagement;
            window.onmessage = this.handleMessage.bind(this);
        }

        /**
         * Initializes the dual display.
         * @param Model.Entities.DeviceConfiguration} deviceConfiguration The device configuration.
         */
        public initialize(deviceConfiguration: Model.Entities.DeviceConfiguration): void {
            this._deviceConfiguration = deviceConfiguration;
            this.displayTransaction(new Model.Entities.CartClass({
                Id: StringExtensions.EMPTY
            }));
        }

        /**
         * Displays a transaction on the dual display
         * If display isn't shown yet, it creates the secondary window; otherwise, the transaction is updated
         * @param {Commerce.Model.Entities.Cart} cart Cart to be displayed.
         */
        public displayTransaction(cart: Model.Entities.Cart): void {
            // Dual display should show only if it's configured in AX and if another display is available
            if (Commerce.ApplicationContext.Instance
                && Commerce.ApplicationContext.Instance.hardwareProfile.DualDisplayActive
                && this._viewManagement.ProjectionManager.projectionDisplayAvailable) {
                if (this._projectionStarted) {
                    // Send the current cart to the secondary view so the transaction information can be displayed
                    this.sendMessage(DualDisplay.MESSAGE_TYPE_DISPLAY_TRANSACTION, cart);
                } else {
                    // Start projection
                    this._secondaryView = (<IExtendedMSApp>MSApp).createNewView(DualDisplay.VIEW_PATH);
                    this.startProjection(cart);
                }
            }
        }

        /**
         * Starts the display of the dual screen
         * @param {Commerce.Model.Entities.Cart} cart Cart to be displayed.
         */
        private startProjection(cart: Model.Entities.Cart): void {
            // Start projection using the previously created secondary view.
            if (!this._projectionStarted && this._viewManagement.ProjectionManager.projectionDisplayAvailable) {
                this._viewManagement.ProjectionManager.startProjectingAsync(
                    this._secondaryView.viewId,
                    this._viewManagement.ApplicationView.getForCurrentView().id
                    ).done(() => {
                        this._projectionStarted = true;

                        // Initialize by sending the cart information
                        this.sendMessage(DualDisplay.MESSAGE_TYPE_INITIALIZE, this._deviceConfiguration);
                        this.sendMessage(DualDisplay.MESSAGE_TYPE_DISPLAY_TRANSACTION, cart);
                    }, () => {
                        Commerce.NotificationHandler.displayClientErrors([
                            new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_DUALDISPLAY_ERROR)
                        ]);
                    });
            }
        }

        /**
         * Sends a message to the secondary view containing the current cart information
         * @param {Commerce.Model.Entities.Cart} cart New cart to be added.
         */
        private sendMessage(type: string, data: any): void {
            var msgObj: IInteropMessage = { type: type, args: data };
            this._secondaryView.postMessage(JSON.stringify(msgObj), document.location.protocol + "//" + document.location.host);
        }

        /**
         * Handles messages received from the secondary view
         * @param {MessageEvent} message The message received.
         */
        private handleMessage(message: MessageEvent): void {
            var data: IInteropMessage = JSON.parse(message.data);

            switch (data.type) {
                // Handle the case of the secondary view being closed
                case "close":
                    this._projectionStarted = false;
                    break;
                default:
                    break;
            }
        }

    }
}