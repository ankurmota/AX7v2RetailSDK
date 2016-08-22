/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Extensions/StringExtensions.ts" />
/// <reference path="../Utilities/ThrowIf.ts" />
/// <reference path="../Entities/CommerceTypes.g.ts" />
/// <reference path="../Utilities/ErrorHelper.ts" />
/// <reference path="Configuration/ITaskRecorderConfig.ts" />
/// <reference path="Model/RecordingFactory.ts" />

module Commerce.TaskRecorder {

    import Managers = Commerce.Model.Managers;

    /**
     * Represents the Task Recorder managing functionality, entry-point to the Task Recorder API.
     */
    export class TaskRecorderManager {

        private static configurationPath: string = "TaskRecorder.config.json";
        private static configuration: Configuration.ITaskRecorderConfig = null;
        public activeSession: Observable<TaskRecorderSession> = ko.observable(null);
        private inProgress: boolean = false;
        private recordingManager: Managers.IRecordingManager = Managers.Factory.getManager<Managers.IRecordingManager>(Managers.IRecordingManagerName);
        private takeScreenshotListener: EventListener;

        /**
         * Constructor.
         */
        constructor() {
            var that: any = this;
            this.takeScreenshotListener = ((event: any) => {
                that.takeScreenshot(that.activeSession().id(), event.stepId);
            });
        }

        /**
         * Loads Task Recorder configuration async from the config file.
         */
        public static loadConfigurationAsync(): IVoidAsyncResult {
            if (this.configuration) {
                return VoidAsyncResult.createResolved();
            }

            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            $.getJSON(this.configurationPath).done((config: Configuration.ITaskRecorderConfig) => {
                this.configuration = config;
                asyncResult.resolve();
            }).fail(() => {
                asyncResult.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_CONFIGURATION_ERROR)]);
            });

            return asyncResult;
        }

        /**
         * Starts a new recording session with given name and description.
         * @param name {string} The name of the recording.
         * @param description {string} The description of the recording.
         * @returns {IVoidAsyncResult} A promise which resolves when the new session opened.
         */
        public startNewSession(name: string, description: string): IVoidAsyncResult {
            ThrowIf.argumentIsNotString(name, "name");
            ThrowIf.argumentIsNotStringOrNull(description, "description");

            if (this.inProgress) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY)]);
            }

            // Cancel existing session.
            this.cancelSession();

            this.inProgress = true;

            // Make sure configuration has been loaded.
            var asyncResult: IVoidAsyncResult = TaskRecorderManager.loadConfigurationAsync().done(() => {

                // Create a new empty recording.
                var recording: Proxy.Entities.Recording = Model.RecordingFactory.createNew(name, description);

                // Create a session.
                this.activeSession(new TaskRecorderSession(recording, TaskRecorderManager.configuration));
            });

            return asyncResult.always(() => {
                this.inProgress = false;
            });
        }

        /**
         * Opens a existing recording session.
         * @returns {IVoidAsyncResult} A promise which resolves when the existing session opened.
         */
        public openExistingSession(): IVoidAsyncResult {
            if (this.inProgress) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY)]);
            }

            // Cancel existing session.
            this.cancelSession();

            this.inProgress = true;

            var asyncResult: IVoidAsyncResult = VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.NOT_IMPLEMENTED)]);

            return asyncResult.always(() => {
                this.inProgress = false;
            });
        }

        /**
         * Save session as XML file.
         */
        public saveSessionAsXml(): IVoidAsyncResult {
            if (this.inProgress) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY)]);
            }

            if (ObjectExtensions.isNullOrUndefined(this.activeSession())) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_NO_ACTIVE_SESSION)]);
            }

            this.inProgress = true;

            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            this.activeSession().getRecordingAsync().done((recording: Proxy.Entities.Recording) => {
                this.recordingManager.generateRecordingFile(recording).done((url: string) => {
                    this.saveFile(url)
                        .done(() => {
                            asyncResult.resolve();
                        })
                        .fail((errors: Proxy.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                })
                .fail((errors: Proxy.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });
            })
            .fail((errors: Proxy.Entities.Error[]) => {
                asyncResult.reject(errors);
            });

            return asyncResult.always(() => {
                this.inProgress = false;
            });
        }

        /**
         * Save session as business process model package.
         */
        public saveBusinessProcessModelPackage(): IVoidAsyncResult {
            if (this.inProgress) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY)]);
            }

            if (ObjectExtensions.isNullOrUndefined(this.activeSession())) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_NO_ACTIVE_SESSION)]);
            }

            this.inProgress = true;

            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            this.activeSession().getRecordingAsync().done((recording: Commerce.Model.Entities.Recording) => {
                this.recordingManager.generateBusinessProcessModelPackage(recording).done((url: string) => {
                    this.saveFile(url)
                        .done(() => {
                            asyncResult.resolve();
                        })
                        .fail((errors: Commerce.Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    })
                    .fail((errors: Commerce.Model.Entities.Error[]) => {
                        asyncResult.reject(errors);
                    });
                })
                .fail((errors: Commerce.Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

            return asyncResult.always(() => {
                this.inProgress = false;
            });
        }

        /**
         * Save session as recording bundle.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public saveSessionAsRecordingBundle(): IAsyncResult<ICancelableResult> {

            if (this.inProgress) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY)]);
            }

            if (ObjectExtensions.isNullOrUndefined(this.activeSession())) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_NO_ACTIVE_SESSION)]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();

            this.inProgress = true;

            var savingRecording: Proxy.Entities.Recording;

            asyncQueue.enqueue(() => {
                return this.activeSession().getRecordingAsync().done((recording: Proxy.Entities.Recording) => {
                    savingRecording = recording;
                });
            });

            asyncQueue.enqueue(() => {
                return this.saveRecordingAsRecordingBundle(savingRecording);
            });

            return asyncQueue.run()
                .always(() => {
                    this.inProgress = false;
                });
        }

        /**
         * Save recording as recording bundle.
         * @param {Proxy.Entities.Recording} recording The recording.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public saveRecordingAsRecordingBundle(recording: Proxy.Entities.Recording): IAsyncResult<ICancelableResult> {

            var asyncQueue: AsyncQueue = new AsyncQueue();

            var savingUrl: string;

            asyncQueue.enqueue(() => {
                return this.recordingManager.generateRecordingBundle(recording).done((url: string) => {
                    savingUrl = url;
                });
            });

            asyncQueue.enqueue(() => {
                return this.saveFile(savingUrl);
            });

            return asyncQueue.run();
        }

        /**
         * Save session to Lifecycle Services.
         */
        public saveToLifecycleServices(): IVoidAsyncResult {
            if (this.inProgress) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY)]);
            }

            if (ObjectExtensions.isNullOrUndefined(this.activeSession())) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_NO_ACTIVE_SESSION)]);
            }

            this.inProgress = true;

            var asyncResult: IVoidAsyncResult = VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.NOT_IMPLEMENTED)]);

            return asyncResult.always(() => {
                this.inProgress = false;
            });
        }

        /**
         * Save session as Word Document file.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public saveSessionAsWordDocument(): IAsyncResult<ICancelableResult> {

            if (this.inProgress) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY)]);
            }

            if (ObjectExtensions.isNullOrUndefined(this.activeSession())) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_NO_ACTIVE_SESSION)]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();

            this.inProgress = true;

            var savingRecording: Proxy.Entities.Recording;

            asyncQueue.enqueue(() => {
                return this.activeSession().getRecordingAsync().done((recording: Proxy.Entities.Recording) => {
                    savingRecording = recording;
                });
            });

            asyncQueue.enqueue(() => {
                return this.saveRecordingAsWordDocument(savingRecording);
            });

            return asyncQueue.run()
                .always(() => {
                    this.inProgress = false;
                });
        }

        /**
         * Save recording as Word Document file.
         * @param {Proxy.Entities.Recording} recording The recording.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public saveRecordingAsWordDocument(recording: Proxy.Entities.Recording): IAsyncResult<ICancelableResult> {

            var asyncQueue: AsyncQueue = new AsyncQueue();

            var savingUrl: string;

            asyncQueue.enqueue(() => {
                return this.recordingManager.generateTrainingDocument(recording).done((url: string) => {
                    savingUrl = url;
                });
            });

            asyncQueue.enqueue(() => {
                return this.saveFile(savingUrl);
            });

            return asyncQueue.run();
        }

        /**
         * Searches task guide by title.
         * @param {string} taskGuideSearchText The search text.
         * @return {IAsyncResult<ViewModel.TaskGuideViewModel[]>} The async result.
         */
        public searchTaskGuidesByTitle(taskGuideSearchText: string): IAsyncResult<Model.TaskGuideModel[]> {

            var asyncResult: AsyncResult<Model.TaskGuideModel[]> = new AsyncResult<Model.TaskGuideModel[]>();

            this.recordingManager.getBusinessProcessModelLibraries()
                .done((bpmLibraries: Proxy.Entities.Framework[]) => {

                    var result: Model.TaskGuideModel[] = [];
                    var asyncQueue: AsyncQueue = new AsyncQueue();

                    // search in all BPM libraries
                    bpmLibraries.forEach((bpmLibrary: Proxy.Entities.Framework) => {
                        asyncQueue.enqueue(() => {
                            return this.recordingManager.searchTaskGuidesByTitle(bpmLibrary.Id,
                                taskGuideSearchText,
                                Proxy.Entities.QueryType.NameAndDescription)
                                .done((taskGuides: Proxy.Entities.TaskGuidesSearchResult) => {
                                    taskGuides.Lines.forEach((taskGuideLine: Proxy.Entities.Line) => {
                                        result.push(new Model.TaskGuideModel(taskGuideLine, bpmLibrary.Publisher));
                                    });
                                })
                                .fail(() => {
                                    asyncQueue.cancel();
                                });
                        });
                    });

                    asyncQueue.run()
                        .done(() => {
                            asyncResult.resolve(result);
                        })
                        .fail((errors: Proxy.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                })
                .fail((errors: Proxy.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

            return asyncResult;
        }

        /**
         * Downloads a recording object.
         * @param {number} bpmLineId The recording ID.
         * @return {IAsyncResult<ViewModel.TaskRecorderSessionViewModel>} The async result.
         */
        public downloadRecording(bpmLineId: number): IAsyncResult<ViewModel.TaskRecorderSessionViewModel> {

            var asyncResult: AsyncResult<ViewModel.TaskRecorderSessionViewModel> = new AsyncResult<ViewModel.TaskRecorderSessionViewModel>();

            this.recordingManager.downloadRecording(bpmLineId)
                .done((recording: Proxy.Entities.Recording) => {
                    try {
                        asyncResult.resolve(new ViewModel.TaskRecorderSessionViewModel(recording));
                    } catch (error) {
                        asyncResult.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_COULDNT_DOWNLOAD_RECORDING,
                            false, StringExtensions.EMPTY, null, bpmLineId, JSON.stringify(error))]);
                    }
                })
                .fail((errors: Proxy.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

            return asyncResult;
        }

        /**
         * Cancels the active Task Recorder session and rejects all unsaved changes.
         */
        public cancelSession(): void {
            if (this.inProgress) {
                throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_MANAGER_BUSY);
            }

            if (this.activeSession()) {
                this.activeSession().eventListener.dispose();
                this.activeSession().eventListener = null;

                this.activeSession(null);
            }
        }

        /**
         * Enable event for screen capturing
         */
        public enableScreenCapture(): void {
            window.addEventListener("screenshot", this.takeScreenshotListener, true);
        }

        /**
         * Disable event for screen capturing
         */
        public disableScreenCapture(): void {
            window.removeEventListener("screenshot", this.takeScreenshotListener, true);
        }

        /**
         * Take screenshot method.
         */
        public takeScreenshot(sessionId: string, stepId: string): void {
            Microsoft.Dynamics.Commerce.ClientBroker.ScreenCapture.takeScreenshotAsync(sessionId, stepId).done((result: string) => {
                if (StringExtensions.isEmpty(result)) {
                    throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_COULDNT_TAKE_SCREENSHOT);
                }

                this.activeSession().updateScreenshotUri(stepId, result);
            });
        }

        private saveFile(url: string): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            TaskRecorderDownloader.downloadFile(url)
                .done((isCanceled: boolean) => {
                    if (isCanceled) {
                        Commerce.RetailLogger.taskRecorderSavingFileCanceled(url);
                    } else {
                        Commerce.RetailLogger.taskRecorderSavingFileFinished(url);
                    }

                    asyncResult.resolve();
                })
                .fail((errors: Proxy.Entities.Error[]) => {
                    Commerce.RetailLogger.taskRecorderSavingFileFailed(ErrorHelper.getErrorMessages(errors));

                    asyncResult.reject(errors);
                });

            return asyncResult;
        }
    }
}