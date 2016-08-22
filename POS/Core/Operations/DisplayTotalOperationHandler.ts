/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the DisplayTotal operation.
     */
    export interface IDisplayTotalOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the DisplayTotal operation.
     */
    export class DisplayTotalOperationHandler extends OperationHandlerBase {
        /**
         * Executes the DisplayTotal operation.
         * @param {IDisplayTotalOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IDisplayTotalOperationOptions): IAsyncResult<IOperationResult> {
            var result: VoidAsyncResult = new VoidAsyncResult();
            Commerce.Peripherals.HardwareStation.LineDisplayHelper.displayBalance(Session.instance.cart.TotalAmount, Session.instance.cart.AmountDue)
                .done((): void => {
                    result.resolve();
                }).fail((errors: Model.Entities.Error[]): void => {
                    var error: Model.Entities.Error = new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_LINEDISPLAY_ERROR);
                    if (ObjectExtensions.isNullOrUndefined(errors)) {
                        errors = new Model.Entities.Error[0];
                    }

                    errors.unshift(error);
                    result.reject(errors);
                });

            return result;
        }
    }
}