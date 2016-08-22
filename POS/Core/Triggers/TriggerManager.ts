/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ITrigger.ts'/>
///<reference path='TriggerType.ts'/>

module Commerce.Triggers {
    "use strict";

    /**
     * Class for managing triggers.
     * This class is implemented as a singleton.
     */
    export class TriggerManager {
        private static _instance: TriggerManager = null;
        private _triggerMap: { [triggerType: string]: ITrigger[] };

        constructor() {
            this._triggerMap = {};
        }

        /**
         * Gets the instance of trigger manager.
         */
        public static get instance(): TriggerManager {
            if (ObjectExtensions.isNullOrUndefined(TriggerManager._instance)) {
                TriggerManager._instance = new TriggerManager();
            }

            return TriggerManager._instance;
        }

        /**
         * Executes the triggers that have registed for the specified trigger type.
         * @param {CancelableTriggerType | NonCancelableTriggerType} triggerType The trigger type for which the registered triggers should be run.
         * @param {ITriggerOptions} options The options for the specified trigger.
         * @return {IAsyncResult<ICancelableResult> | IVoidAsyncResult} The combined async result of the trigger execution.
         */
        public execute(triggerType: CancelableTriggerType, options: ITriggerOptions): IAsyncResult<ICancelableResult>;
        public execute(triggerType: NonCancelableTriggerType, options: ITriggerOptions): IVoidAsyncResult;
        public execute(triggerType: CancelableTriggerType | NonCancelableTriggerType, options: ITriggerOptions): IVoidAsyncResult {
            if (ObjectExtensions.isNullOrUndefined(triggerType)) {
                throw new Error("TriggerManager::execute - Invalid trigger execution parameters: The trigger type cannot be null.");
            }

            var triggerName: string = triggerType.toString();
            var triggersToExecute: ITrigger[] = this._triggerMap[triggerName];

            // If there are no triggers registered for this event return a resolved async result.
            if (!ArrayExtensions.hasElements(triggersToExecute)) {
                return AsyncResult.createResolved<ICancelableResult>({ canceled: false });
            }

            RetailLogger.coreTriggerExecutionStarted(triggerName);

            var isCancelableTriggerType: boolean = triggerType instanceof CancelableTriggerType;
            var triggerQueue: AsyncQueue = this.triggerQueue(triggersToExecute, options, isCancelableTriggerType);

            return triggerQueue.run()
                .done((result: ICancelableResult): void => {
                    if (result.canceled) {
                        RetailLogger.coreTriggerExecutionCanceled(triggerName);
                    } else {
                        RetailLogger.coreTriggerExecutionCompleted(triggerName);
                    }
                }).fail((errors: Model.Entities.Error[]): void => {
                    RetailLogger.coreTriggerExecutionFailed(triggerName, ErrorHelper.getErrorMessages(errors));
                });
        }

        /**
         * Registers the trigger for the provided TriggerType.
         * @param {TriggerType} triggerType The trigger type for which the trigger should be registered.
         * @param {ITrigger} trigger The trigger to register.
         */
        public register(triggerType: ITriggerType, trigger: ITrigger): void {
            if (ObjectExtensions.isNullOrUndefined(triggerType) || ObjectExtensions.isNullOrUndefined(trigger)) {
                throw new Error("TriggerManager::register - Invalid trigger registration: The trigger type and trigger cannot be null.");
            }

            if (ObjectExtensions.isNullOrUndefined(this._triggerMap[triggerType.toString()])) {
                this._triggerMap[triggerType.toString()] = [];
            }

            this._triggerMap[triggerType.toString()].push(trigger);
        }

        /**
         * Creates a cancelable async queue to execute the triggers provided in a sequential manner.
         * @param {ITrigger[]} triggers The triggers to execute.
         * @param {ITriggerOptions} options The options to use when running the trigger.
         * @param {boolean} isCancelable The value indicating if the trigger queue can be canceled by a trigger result.
         * @returns {AsyncQueue} The async queue.
         */
        private triggerQueue(triggers: ITrigger[], options: ITriggerOptions, isCancelable: boolean): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            triggers.forEach((trigger: ITrigger): void => {
                asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                    var triggerResult: IAsyncResult<ICancelableResult> = this.executeTrigger(trigger, options);
                    return isCancelable ? asyncQueue.cancelOn(triggerResult) : triggerResult;
                });
            });

            return asyncQueue;
        }

        /**
         * Executes the trigger.
         * @param {ITrigger} trigger The trigger to execute.
         * @param {ITriggerOptions} options The options to run the trigger with.
         * @returns {IAsyncResult<ICancelableResult>} The result of the trigger execution.
         */
        private executeTrigger(trigger: ITrigger, options: ITriggerOptions): IAsyncResult<ICancelableResult> {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            asyncQueue.enqueue((): IVoidAsyncResult => {
                var triggerResult: IAsyncResult<ICancelableResult>;
                try {
                    triggerResult = trigger.execute(options);
                } catch (error) {
                    triggerResult = AsyncResult.createRejected<ICancelableResult>([new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
                }

                return asyncQueue.cancelOn(triggerResult);
            });

            return asyncQueue.run();
        }
    }
}  