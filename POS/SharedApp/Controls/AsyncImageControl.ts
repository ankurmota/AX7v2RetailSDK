/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Core/Converters.ts'/>

module Commerce {
    "use strict";

    /**
     * Options for async image control constructor.
     */
    export class AsyncImageControlOptions {
        offlineBinaryImageHandlerProperty: string;
        defaultImage: string;
    }

    /**
     * Extends the functionality of img HTML element to load the online image or offline image source asynchronously.
     */
    export class AsyncImageControl {
        private _data: any;
        private _onlineImage: string;
        private _options: AsyncImageControlOptions;

        private _element: HTMLElement;
        private _imageElement: HTMLImageElement;

        /**
         * Sets the data entity containing the offline image handler.
         * @param {any} data The data entity.
         */
        public set data(data: any) {
            this._data = data;
            this.setImage();
        }

        /**
         * Sets the online image.
         * @param {string} image The online image.
         */
        public set onlineImage(image: string) {
            this._onlineImage = image;
            this.setImage();
        }

        /**
         * Sets the image alternative.
         * @param {string} alt The image alternative.
         */
        public set alt(alt: string) {
            this._imageElement.alt = alt;
        }

        /**
         * @param {HTMLElement} element The HTML image element being bound to the AsyncImageControl
         * @param {AsyncImageControlOptions} options Options passed to the AsyncImageControl through the data-win-options attribute.
         * @return {AsyncImageControl} Returns the AsyncImageControl.
         */
        constructor(element: HTMLElement, options: AsyncImageControlOptions) {
            this._options = options || { offlineBinaryImageHandlerProperty: null, defaultImage: null };
            element.winControl = this;
            this._element = element;

            this._imageElement = document.createElement("img");
            this._imageElement.className = this._element.className;

            if (!StringExtensions.isNullOrWhitespace(this._options.defaultImage)) {
                this._imageElement.src = this._options.defaultImage;
                this._imageElement.addEventListener("error", (() => {
                    Commerce.BindingHandlers.SetDefaultImageOnError(this._imageElement, this._options.defaultImage);
                }).bind(this));
            } else {
                RetailLogger.viewsAsyncImageControlInvalidDefaultImage();
            }

            this._element.appendChild(this._imageElement);

            return this;
        }

        /**
         * Sets the appropriate image depending on connection status.
         */
        private setImage(): void {
            if (Commerce.Session.instance.connectionStatus === ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(this._onlineImage)) {
                    this._imageElement.src = Commerce.Formatters.ImageUrlFormatter(this._onlineImage);
                }
            } else {
                if (!ObjectExtensions.isNullOrUndefined(this._data) &&
                    !StringExtensions.isNullOrWhitespace(this._options.offlineBinaryImageHandlerProperty)) {

                    var imageHandler: ((currentItem: any) => IAsyncResult<string>) = this._data[this._options.offlineBinaryImageHandlerProperty];
                    if (ObjectExtensions.isFunction(imageHandler)) {
                        imageHandler(this._data).done((imageSource: string) => {
                            if (!StringExtensions.isNullOrWhitespace(imageSource)) {
                                this._imageElement.src = Commerce.Formatters.ImageBinaryFormatter(imageSource);
                            }
                        });
                    }
                }
            }
        }
    }

    // Required for WinJS to process this control as a WinJS control
    WinJS.Utilities.markSupportedForProcessing(AsyncImageControl);
}