/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Requests {
    "use strict";

    /**
     * Class represents chained request factory.
     */
    export class ChainedRequestFactory implements Common.IDataServiceRequestFactory {

        private _onlineScanGraceInterval: number = 1; // 1 minute.
        private _defaultRetryInterval: number = 5; // Default is 5 minutes.
        private _nextOnlineScan: number = 0; // Default is off
        private _onlineDataServiceRequestFactory: Common.IDataServiceRequestFactory;
        private _offlineDataServiceRequestFactory: Common.IDataServiceRequestFactory;

        constructor(onlineDataServiceRequestFactory: Common.IDataServiceRequestFactory, offlineDataServiceRequestFactory: Common.IDataServiceRequestFactory) {
            this._onlineDataServiceRequestFactory = onlineDataServiceRequestFactory;
            this._offlineDataServiceRequestFactory = offlineDataServiceRequestFactory;
            this.setNextOnlineScanInterval(0);
        }

        /**
         * Gets the locale for current request.
         */
        public get locale(): string {
            return this._onlineDataServiceRequestFactory.locale;
        }

        /**
         * Create a request.
         * @param {DataServiceQueryInternal} dataServiceQuery The data service query.
         * @return {IDataServiceRequest} The data service request.
         */
        public create(dataServiceQuery: Common.IDataServiceQueryInternal): Common.IDataServiceRequest {
            return new ChainedRequest(this._onlineDataServiceRequestFactory, this._offlineDataServiceRequestFactory, this, dataServiceQuery, this.locale);
        }

        /**
         * Switch the connection.
         * @param {newConnectionStatus} ConnectionStatusType New connection status.
         * @param {boolean} manualSwitchToOnline If switch type is manual switch to online.
         * @return {IVoidAsyncResult} The async result.
         */
        public switchConnection(newConnectionStatus: ConnectionStatusType, manualSwitchToOnline: boolean): IVoidAsyncResult {
            var currentConnectionStatus: ConnectionStatusType = Session.instance.connectionStatus;

            if (currentConnectionStatus === newConnectionStatus) {
                return VoidAsyncResult.createResolved();
            }

            var asyncResult: VoidAsyncResult = new VoidAsyncResult(this);

            switch (newConnectionStatus) {
                // Switching to online
                case ConnectionStatusType.Online:
                    if (Session.instance.isCartInProgress) {
                        // If switching to online is due and Transaction is in progress), then skip scan for a grace period.
                        this.setNextOnlineScanInterval(this._onlineScanGraceInterval);
                        if (manualSwitchToOnline) {
                            asyncResult.reject([new Model.Entities.Error(
                                ErrorTypeEnum.CANNOT_SWITCH_ONLINE_CART_IN_PROGRESS)]);
                        } else {
                            asyncResult.resolve();
                        }
                    } else {
                        RetailLogger.modelManagersChainedRequestFactorySwitchingToOnline();

                        this.transferShiftToOnline()
                            .done(() => {
                                Session.instance.connectionStatus = newConnectionStatus;
                                asyncResult.resolve();
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                if (errors[0].ErrorCode.toUpperCase() === ErrorTypeEnum.SERVICE_UNAVAILABLE.toUpperCase()) {
                                    this.setNextOnlineScanInterval();
                                    // If there is no connection to Retail Server, keeps in offline.
                                    if (manualSwitchToOnline) {
                                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.SERVICE_UNAVAILABLE)]);
                                    } else {
                                        asyncResult.resolve();
                                    }
                                } else if (errors[0].ErrorCode.toUpperCase()
                                    === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AUTHENTICATIONFAILED.serverErrorCode) {
                                    // If user token expired, do not display error message. Navigate to logon page directly.
                                    asyncResult.resolve();
                                } else {
                                    asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.CANNOT_TRANSFER_SHIFT_TO_ONLINE)]);
                                }
                            });
                    }
                    break;

                // Switching to offline
                case ConnectionStatusType.SeamlessOffline:
                case ConnectionStatusType.ManualOffline:
                    if (!Session.instance.isOfflineAvailable || Session.instance.offlineParameters.offlineModeDisabled) {
                        asyncResult.reject([new Model.Entities.Error(
                            ErrorTypeEnum.CANNOT_SWITCH_OFFLINE_REQUIRE_RELOGIN)]);
                    } else {
                        var asyncQueue: AsyncQueue = new AsyncQueue();

                        asyncQueue.enqueue(() => this.transferShift(this._offlineDataServiceRequestFactory));
                        asyncQueue.enqueue(() => this.transferCartToOffline());

                        asyncQueue.run()
                            .done(() => {
                                RetailLogger.modelManagersChainedRequestFactorySwitchingToOffline();
                                Session.instance.connectionStatus = newConnectionStatus;
                                this.setNextOnlineScanInterval();
                                asyncResult.resolve();
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                asyncResult.reject(errors);
                            });
                    }
                    break;
            }

            return asyncResult;
        }

        /**
         * Switch the connection to online if due.
         */
        public switchConnectionToOnlineIfDue(): IVoidAsyncResult {
            // Try to go online after specified period if we automatically switched to offline.
            if (Session.instance.connectionStatus === ConnectionStatusType.SeamlessOffline
                && this._nextOnlineScan
                && this._nextOnlineScan <= Date.now()) {
                return this.switchConnection(ConnectionStatusType.Online, false);
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Transfer shift from offline to online.
         */
        public transferShiftToOnline(): IVoidAsyncResult {
            if (Session.instance.Shift && Session.instance.Shift.ShiftId) {
                var shiftTransferToOnlineQuery: Proxy.ShiftsDataServiceQuery
                    = new Proxy.CommerceContext(this._onlineDataServiceRequestFactory).shifts();
                var shiftTransferToOfflineQuery: Proxy.ShiftsDataServiceQuery
                    = new Proxy.CommerceContext(this._offlineDataServiceRequestFactory).shifts();

                var asyncQueue: any = new AsyncQueue()
                    .enqueue(() => {
                        return shiftTransferToOnlineQuery.create(Session.instance.Shift).execute()
                            .fail((errors: Model.Entities.Error[]) => {
                                // Do not log error if transfer to online failed due to service unavailable.
                                if (errors[0].ErrorCode.toUpperCase() !== ErrorTypeEnum.SERVICE_UNAVAILABLE.toUpperCase()) {
                                    RetailLogger.modelManagersChainedRequestFactoryShiftTransferToOnlineCreateFailed(errors[0].ErrorCode,
                                        ErrorHelper.formatErrorMessage(errors[0]));
                                }
                            });
                    });

                asyncQueue
                    .enqueue(() => {
                        return shiftTransferToOfflineQuery.delete(Session.instance.Shift).execute()
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.modelManagersChainedRequestFactoryShiftTransferToOnlineDeleteFailed(errors[0].ErrorCode,
                                    ErrorHelper.formatErrorMessage(errors[0]));
                            });
                    });

                return asyncQueue.run();
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        private transferShift(targetFactory: Common.IDataServiceRequestFactory): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            if (Session.instance.Shift && Session.instance.Shift.ShiftId) {
                var shiftTransferQuery: Proxy.ShiftsDataServiceQuery = new Proxy.CommerceContext(targetFactory).shifts();

                shiftTransferQuery.create(Session.instance.Shift).execute()
                    .done((transferdShift: Model.Entities.Shift) => {
                        asyncResult.resolve();
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        var currentState: string = Session.instance.connectionStatusAsString();
                        RetailLogger.modelManagersChainedRequestFactoryShiftTransferFailed(currentState, errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.CANNOT_SWITCH_TRANSFER_FAILED)]);
                    });
            } else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        private transferCartToOffline(): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            if (Session.instance.isCartInProgress) {
                var cartToTransfer: Model.Entities.Cart = ObjectExtensions.clone(Session.instance.cart);
                var cartTransferQuery: Proxy.CartsDataServiceQuery = new Proxy.CommerceContext(this._offlineDataServiceRequestFactory).carts();

                cartTransferQuery.create(cartToTransfer).execute()
                    .done((transferedCart: Model.Entities.Cart) => {
                        Session.instance.cart = cartToTransfer;
                        asyncResult.resolve();
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.modelManagersChainedRequestFactoryCartTransferToOfflineFailed(errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.CANNOT_SWITCH_TRANSFER_FAILED)]);
                    });
            } else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        private setNextOnlineScanInterval(interval?: number): void {
            if (Commerce.ApplicationContext.Instance.deviceConfiguration) {
                var configuredInterval: number = Commerce.ApplicationContext.Instance.deviceConfiguration.ReconnectToOnlineInterval;

                if (configuredInterval > 0) {
                    this._nextOnlineScan = Date.now() + ((interval >= 0 ? interval : configuredInterval) * 60 * 1000); // Interval is configured in minutes.
                } else {
                    this._nextOnlineScan = Date.now() + this._defaultRetryInterval * 60 * 1000;
                }
            }
        }
    }
}