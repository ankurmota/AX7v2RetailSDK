/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialog.ts'/>

module Commerce.Controls {
    "use strict";

    export class TimeClockDialogViewMode {
        static Options: string = "options";
        static RegistrationDetails: string = "registrationDetails";
    }

    export class TimeClockActivityType {
        static ClockIn: string = "ClockIn";
        static ClockOut: string = "ClockOut";
        static Break: string = "BreakFlowStart";
        static Journal: string = "Journal";
    }

    export class TimeClockBreakType {
        static WorkBreak: string = "DailyBrks";
        static LunchBreak: string = "LunchBrk";
    }

    export interface ITimeRegistrationDialogState {
        employeeActivity: Model.Entities.EmployeeActivity;
    }

    export interface ITimeRegistrationDialogOutput {
        employeeActivityType: Model.Entities.EmployeeActivityType;
        actionName: string;
    }

    export class TimeRegistrationDialog extends ModalDialog<ITimeRegistrationDialogState, ITimeRegistrationDialogOutput> {

        public lastActivityStatus: Observable<string>;
        public lastActivityDate: Observable<string>;
        public lastActivityDateLabel: Observable<string>;
        public viewMode: Observable<string>;

        public statusLabelsVisible: Observable<boolean>;
        public clockedIn: Observable<boolean>;
        public optionButtons: ObservableArray<any>;

        /**
         * Initializes a new instance of the TimeRegistrationDialog class.
         */
        constructor() {
            super();

            // Control objects
            this.title(Commerce.ViewModelAdapter.getResourceString('string_4082'));
            this.viewMode = ko.observable(TimeClockDialogViewMode.Options);
            this.lastActivityStatus = ko.observable(StringExtensions.EMPTY);
            this.lastActivityDate = ko.observable(StringExtensions.EMPTY);
            this.lastActivityDateLabel = ko.observable(Commerce.ViewModelAdapter.getResourceString('string_4090'));

            this.statusLabelsVisible = ko.observable(false);
            this.clockedIn = ko.observable(false);
            this.optionButtons = ko.observableArray<any>([]);
        }

        /**
         * Shows the dialog.
         *
         * @param {any} dialogState The dialog state.
         */
        public onShowing(dialogState: ITimeRegistrationDialogState) {
            this.indeterminateWaitVisible(true);
            if (!ObjectExtensions.isNullOrUndefined(dialogState.employeeActivity)) {
                var formatter: Host.IDateTimeFormatter = Host.instance.globalization.getDateTimeFormatter(Host.Globalization.DateTimeFormat.DATE_TIME);
                this.lastActivityDate(formatter.format(dialogState.employeeActivity.ActivityDateTimeOffset));

                this.setActivityStatus(dialogState.employeeActivity.Activity, dialogState.employeeActivity.BreakCategory);
                this.statusLabelsVisible(true);
            } else {
                this.setActivityStatus(StringExtensions.EMPTY, StringExtensions.EMPTY);
            };

            this.visible(true);
        }
        
        /**
         * Hides the dialog
         */
        public hide(): IVoidAsyncResult {
            return super.hide();
        }

        public updateTimeClockStatus(actionName: string, registrationDateTime: Date): void {
            switch (actionName) {
                case Commerce.ViewModelAdapter.getResourceString("string_4084"):
                    actionName = Commerce.ViewModelAdapter.getResourceString("string_4148");
                    break;
                case Commerce.ViewModelAdapter.getResourceString("string_4085"):
                    actionName = Commerce.ViewModelAdapter.getResourceString("string_4149");
                    break;
                case Commerce.ViewModelAdapter.getResourceString("string_4086"):
                    actionName = Commerce.ViewModelAdapter.getResourceString("string_4152");
                    break;
                case Commerce.ViewModelAdapter.getResourceString("string_4087"):
                    actionName = Commerce.ViewModelAdapter.getResourceString("string_4153");
                    break;
            }

            this.lastActivityStatus(actionName);

            var formatter: Host.IDateTimeFormatter = Host.instance.globalization.getDateTimeFormatter(Host.Globalization.DateTimeFormat.DATE_TIME);
            this.lastActivityDate(formatter.format(registrationDateTime));
            this.viewMode(TimeClockDialogViewMode.RegistrationDetails);
            this.statusLabelsVisible(true);
            this.lastActivityDateLabel(Commerce.ViewModelAdapter.getResourceString('string_4123'));
            this.indeterminateWaitVisible(false);
        }

        /**
         * Sets the current activity status and initializes the option list
         *
         * @param {string} activityType The current activity type
         * @param {string} breakType The current break type
         */
        private setActivityStatus(activityType: string, breakType: string) {
            if (activityType == TimeClockActivityType.ClockIn) {
                this.clockedIn(true);
                this.lastActivityStatus(Commerce.ViewModelAdapter.getResourceString("string_4148"));
            }
            else if (activityType == TimeClockActivityType.ClockOut) {
                this.lastActivityStatus(Commerce.ViewModelAdapter.getResourceString("string_4149"));
            }
            else if (activityType == TimeClockActivityType.Break && breakType == TimeClockBreakType.WorkBreak) {
                this.lastActivityStatus(Commerce.ViewModelAdapter.getResourceString("string_4152"));
            }
            else if (activityType == TimeClockActivityType.Break && breakType == TimeClockBreakType.LunchBreak) {
                this.lastActivityStatus(Commerce.ViewModelAdapter.getResourceString("string_4153"));
            }

            this.optionButtons.push({ action: Model.Entities.EmployeeActivityType.ClockIn, actionName: Commerce.ViewModelAdapter.getResourceString("string_4084"), disabled: this.clockedIn(), cssClass: 'iconCloseShiftBig' });
            this.optionButtons.push({ action: Model.Entities.EmployeeActivityType.ClockOut, actionName: Commerce.ViewModelAdapter.getResourceString("string_4085"), disabled: !this.clockedIn(), cssClass: 'iconOpenShiftBig' });
            this.optionButtons.push({ action: Model.Entities.EmployeeActivityType.BreakFromWork, actionName: Commerce.ViewModelAdapter.getResourceString("string_4086"), disabled: !this.clockedIn(), cssClass: 'iconTakeBreakBig' });
            this.optionButtons.push({ action: Model.Entities.EmployeeActivityType.BreakForLunch, actionName: Commerce.ViewModelAdapter.getResourceString("string_4087"), disabled: !this.clockedIn(), cssClass: 'iconLunchBreakBig' });
            this.optionButtons.push({ action: Model.Entities.EmployeeActivityType.Logbook, actionName: Commerce.ViewModelAdapter.getResourceString("string_4088"), disabled: false, cssClass: 'iconBulletListBig' });

            this.indeterminateWaitVisible(false);
        }

        /**
         * Handles a selection from the option list
         *
         * @param {Commerce.TileList.IItemInvokedArgs} eventArgs The event data
         */
        private handleTimeClockAction(eventArgs: Commerce.TileList.IItemInvokedArgs) {
            // Do this to prevent actions being clicked multiple times 
            // such as two different clock - ins back to back
            this.indeterminateWaitVisible(true);
            var action: Model.Entities.EmployeeActivityType = eventArgs.data.action;
            var actionName: string = eventArgs.data.actionName;

            this.dialogResult.resolve(DialogResult.OK, { employeeActivityType: action, actionName: actionName });
        }

        /**
         * Button click handler.
         * @param {string} buttonId The identifier of the button.
         */
        private buttonClickHandler(buttonId: string) {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}