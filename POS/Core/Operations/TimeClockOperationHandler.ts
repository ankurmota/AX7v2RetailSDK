/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the TimeClock operation.
     */
    export interface ITimeClockOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the TimeClock operation.
     */
    export class TimeClockOperationHandler extends OperationHandlerBase {
        /**
         * Executes the TimeClock operation.
         *
         * @param {ITimeClockOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ITimeClockOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            if (!this.applicationContext.deviceConfiguration.EnableTimeRegistration) {
                RetailLogger.operationTimeClockNotEnabled();

                var errors: Model.Entities.Error[] = [new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_TIME_CLOCK_DISABLED)];
                return VoidAsyncResult.createRejected(errors);
            }

            var asyncQueue: Commerce.AsyncQueue = new AsyncQueue();
            var employeeActivity: Model.Entities.EmployeeActivity = null;
            var activity: Commerce.Activities.RegisterTimeActivity;
            asyncQueue.enqueue((): Commerce.IVoidAsyncResult => {
                var asyncResult: VoidAsyncResult = new VoidAsyncResult();
                this.storeOperationsManager.getCurrentTimeRegistrationStatus()
                    .done((result: Model.Entities.EmployeeActivity): void => {
                        employeeActivity = result;
                    }).always((): void => {
                        asyncResult.resolve();
                    });

                return asyncResult;
            }).enqueue((): Commerce.IVoidAsyncResult => {
                activity = new Activities.RegisterTimeActivity({ employeeActivity: employeeActivity });
                activity.responseHandler = (response: Activities.RegisterTimeActivityResponse): IAsyncResult<Date> => {
                    return this.storeOperationsManager.performTimeRegistration(Session.instance.CurrentEmployee.StaffId, response.employeeActivityType);
                };

                return activity.execute();
            });

            return asyncQueue.run();
        }
    }
}