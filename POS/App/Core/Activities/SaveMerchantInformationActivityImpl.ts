/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Activities {
    "use strict";

    SaveMerchantInformationActivity.prototype.execute = function (): IVoidAsyncResult {
        if (Session.instance.connectionStatus !== ConnectionStatusType.Online ||
            ObjectExtensions.isNullOrUndefined(this.hardwareProfile) ||
            this.hardwareProfile.EftTypeId !== 2) {
            // NOTE: skip for offline modes or hardware profile isn't using PaymentSDK (2).
            return VoidAsyncResult.createResolved();
        }

        var paymentMerchantInfoAsyncQueue: AsyncQueue = new AsyncQueue();

        var hardwareProfileId: string = this.hardwareProfile.ProfileId;
        var paymentMerchantInformation: Model.Entities.PaymentMerchantInformation = null;

        paymentMerchantInfoAsyncQueue.enqueue(() => {
            var channelManager: Model.Managers.IChannelManager =
                Model.Managers.Factory.getManager<Model.Managers.IChannelManager>(Model.Managers.IChannelManagerName);
            return channelManager.getPaymentMerchantInformationAsync(hardwareProfileId)
                .done((paymentMerchant: Model.Entities.PaymentMerchantInformation) => {
                    paymentMerchantInformation = paymentMerchant;
                });
        }).enqueue(() => {
            var paymentMerchantInfoRequest: Peripherals.HardwareStation.SavePaymentMerchantInformationRequest = {
                HardwareProfileId: hardwareProfileId,
                PaymentMerchantInformation: paymentMerchantInformation.PaymentConnectorPropertiesXml
            };

            return MerchantInformationActivityHelper.saveMerchantInformationLocalAsync(paymentMerchantInfoRequest, hardwareProfileId)
                .fail((errors: Proxy.Entities.Error[]) => {
                    if (ArrayExtensions.hasElements(errors) && errors[0].ErrorCode === ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_COMMUNICATION_FAILED) {
                        errors.splice(0, 1, new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_CLIENTBROKER_COMMUNICATION_ERROR.serverErrorCode));
                    }

                    return VoidAsyncResult.createRejected(errors);
                });
        });

        return paymentMerchantInfoAsyncQueue.run();
    };
}