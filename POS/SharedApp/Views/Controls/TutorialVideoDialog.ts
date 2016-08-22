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
    interface ITutorialVideo {
        url: string;
        duration: number;
        description: string;
        name: string;
    }

    interface ITutorialVideoConfig {
        video: Array<ITutorialVideo>;
    }

    class TutorialVideo {

        url: string;
        duration: number;
        durationString: string;
        description: string;
        name: string;
        active: Observable<boolean>;

        constructor(url: string, duration: number, description: string, name: string) {
            this.url = url;
            this.description = description;
            this.duration = duration;
            this.durationString = StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_7621"), this.duration);
            this.name = name;
            this.active = ko.observable(false);
        }
    }

    export class TutorialVideoDialog extends ModalDialog<any, any> {
        private static _path: string = "Views/Controls/tutorialVideo.config.json";

        public tutorialVideoDisabled: Observable<boolean>;
        public bubbleTourDisabled: Observable<boolean>;

        private _video: HTMLVideoElement;
        private _videoList: ObservableArray<TutorialVideo>;
        private _activeVideo: TutorialVideo;

        constructor() {
            super();
            this.init();
            this.tutorialVideoDisabled = ko.observable(Commerce.UI.Tutorial.tutorialVideoDisabled());
            this.bubbleTourDisabled = ko.observable(UI.Tutorial.bubbleTourDisabled());
            this.tutorialVideoDisabled.subscribe((value: boolean) => {
                UI.Tutorial.tutorialVideoDisabled(value);
            });

            this.bubbleTourDisabled.subscribe((value: boolean) => {
                UI.Tutorial.bubbleTourDisabled(value);
            });
        }

        /**
         * Shows the modal dialog.
         * @param {any} dialogState The modal dialog state.
         */
        public afterShow(dialogState: any): void {
            this.visible(true);
            this._video = <HTMLVideoElement>document.getElementById("videoPlayer");
            this.playVideo(this._videoList()[0]);
            this.tutorialVideoDisabled(UI.Tutorial.tutorialVideoDisabled());
            this.bubbleTourDisabled(UI.Tutorial.bubbleTourDisabled());
        }

        /**
         * Hides the modal dialog.
         */
        public hideDialog(): void {
            this.hide();
            try {
                this._video.pause();
            } catch (e) {
                RetailLogger.viewsTutorialVideoDialogVideoElementThrowsError(String(e));
            }

            this.dialogResult.resolve(DialogResult.OK, null);
        }

        private playVideo(data: TutorialVideo): void {
            if (this._activeVideo) {
                this._activeVideo.active(false);
            }
            var videoUrl: string = Commerce.ViewModelAdapter.getResourceString(data.url);
            try {
                this._video.src = videoUrl;
                data.active(true);
                this._activeVideo = data;
                this._video.load();
                this._video.play();
            } catch (e) {
                RetailLogger.viewsTutorialVideoDialogVideoElementThrowsError(String(e));
                NotificationHandler.displayClientErrors([new Model.Entities.Error("string_7644")], "string_7643");
            }
        }

        private init(): void {
            this._videoList = ko.observableArray<TutorialVideo>([]);
            $.getJSON(TutorialVideoDialog._path)
                .done((data: ITutorialVideoConfig) => {
                    data.video.forEach((video: ITutorialVideo) => this._videoList.push(new TutorialVideo(
                        video.url,
                        video.duration,
                        video.description,
                        video.name
                     )));
            });
        }
    }
}
