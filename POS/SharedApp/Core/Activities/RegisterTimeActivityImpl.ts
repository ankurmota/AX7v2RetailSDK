/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce.Activities {
    "use strict";

    RegisterTimeActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: RegisterTimeActivity = <RegisterTimeActivity>(this);

        var timeRegistrationDialog = new Controls.TimeRegistrationDialog();
        var asyncQueue: AsyncQueue = new AsyncQueue();
        var dialogOutput: Controls.ITimeRegistrationDialogOutput;

        asyncQueue
            .enqueue((): IVoidAsyncResult => {
 
            timeRegistrationDialog.show({ employeeActivity: self.context.employeeActivity }, false /* hideOnResult */)
                .on(DialogResult.OK,(result: Controls.ITimeRegistrationDialogOutput): void => {
                    dialogOutput = result;
                }).on(DialogResult.Cancel,(result: Controls.ITimeRegistrationDialogOutput): void => {
                    timeRegistrationDialog.hide();
                    asyncQueue.cancel();
                });

                return ModalDialogHelper.toVoidAsyncResult(timeRegistrationDialog, false);
            }).enqueue((): IVoidAsyncResult => {
                if (dialogOutput.employeeActivityType === Model.Entities.EmployeeActivityType.Logbook) {
                    Commerce.ViewModelAdapter.navigate("TimeClockView");
                }
                else if ((dialogOutput.employeeActivityType === Model.Entities.EmployeeActivityType.ClockIn && !timeRegistrationDialog.clockedIn()) ||
                    (dialogOutput.employeeActivityType !== Model.Entities.EmployeeActivityType.ClockIn && timeRegistrationDialog.clockedIn())) {

                    self.response = { employeeActivityType: dialogOutput.employeeActivityType };
                    return self.responseHandler(self.response)
                        .done((registrationDateTime: Date) => {
                            timeRegistrationDialog.updateTimeClockStatus(dialogOutput.actionName, registrationDateTime);
                        }).fail((errors: Model.Entities.Error[]) => {
                            timeRegistrationDialog.hide();
                        });
                }

                timeRegistrationDialog.hide();
                return VoidAsyncResult.createResolved();
            });

        return asyncQueue.run();
    };
}