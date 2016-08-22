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

    /*
     * The dialog state to set on showing
     *
     */
    export interface SignatureCaptureDialogState {
        verifyOnly: boolean;        // Indicates whether the dialog is in "verification" only mode and a signature cannot be entered.
        allowSkip: boolean;         // Indicates whether to allow skipping the signature.
        signatureData: string;      // The signature to display. Set to null or empty if there is no signature data to display.
        paymentAmount: number;      // The amount to display for the payment value above the signature area
    }

    /*
     * The dialog state to set on showing
     *
     */
    export interface ImageScaleAndOffset {
        scale: number;        // The scale/multiplier to convert the image for display.
        offsetX: number;      // The offset on the X axis.
        offsetY: number;      // The offset on the Y axis.
    }

    /*
     * The points on the rectangle
     */
    export interface Rectangle {
        leftX: number;
        topY: number;
        rightX: number;
        bottomY: number;
        height: number;
        width: number;
    }

    /*
     * Dialog to capture and/or approve a signature
     */
    export class SignatureCaptureDialog extends ModalDialog<SignatureCaptureDialogState, any> {
        // Signature Capture objects
        private paint = false;
        private startX = 0;
        private startY = 0;
        private canvasX = 0;
        private canvasY = 0;
        private signatureCanvas: HTMLCanvasElement;
        private _points: Commerce.Model.Entities.Point[] = [];
        private _endPoint: Commerce.Model.Entities.Point = new Commerce.Model.Entities.Point(0xFFFFFFFF, 0xFFFFFFFF); // End point
        private allowSkip: Observable<boolean>;
        private allowCancel: Observable<boolean>;
        private allowReject: Observable<boolean>;
        private allowClear: Observable<boolean>;
        private hasNoSignatureData: Observable<boolean>;
        private allowSignatureEntry: Observable<boolean>;
        private paymentAmount: Observable<number>;

        private SIGNATURE_CANVAS_ID: string = "signaturecanvas";
        private PADDING: number = 20;

        /**
         * Initializes a new instance of the SignatureCaptureDialog class.
         */
        constructor() {
            super();

            this.allowSkip = ko.observable(false);
            this.allowCancel = ko.observable(false);
            this.allowReject = ko.observable(false);
            this.allowClear = ko.observable(false);
            this.hasNoSignatureData = ko.observable(true);
            this.allowSignatureEntry = ko.observable(false);
            this.paymentAmount = ko.observable(0);
        }

        /**
         * Shows the dialog.
         *
         * @param {SignatureCaptureDialogState} dialogState The dialog state to set on showing.
         */
        public onShowing(dialogState: SignatureCaptureDialogState) {
            this.initializeCanvas();

            if (!dialogState) {
                dialogState = {
                    verifyOnly: false,
                    allowSkip: false,
                    signatureData: null,
                    paymentAmount: 0
                };
            }

            // Set the signature
            if (!ObjectExtensions.isNullOrUndefined(dialogState.signatureData) && (dialogState.signatureData.length > 0)) {
                var imageDataByteArray = SerializationHelpers.fromBase64String(dialogState.signatureData);

                var retVal: { error: Commerce.Model.Entities.Error; points: Commerce.Model.Entities.Point[] } = this.getByteArrayAsPoints(imageDataByteArray);
                if (retVal.error) {
                    Commerce.NotificationHandler.displayClientErrors([retVal.error])
                        .done(() => {
                            this.signatureCaptureDialogButtonClickHandler(Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK);
                        }).fail(() => {
                            this.signatureCaptureDialogButtonClickHandler(Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK);
                        });
                    return;
                } else {
                    this._points = retVal.points;
                }
            }

            // Set the dialog state
            this.paymentAmount(dialogState.paymentAmount);

            // Set the enabled state of the buttons
            this.allowCancel(!dialogState.verifyOnly);
            this.allowReject(dialogState.verifyOnly);
            this.allowSkip(dialogState.allowSkip);
            this.allowClear(!dialogState.verifyOnly);
            this.allowSignatureEntry(!dialogState.verifyOnly);

            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            // Draw the signature
            this.signatureCanvas.getContext("2d").clearRect(0, 0, this.signatureCanvas.width, this.signatureCanvas.height);
            if (ArrayExtensions.hasElements(this._points)) {
                this.drawPoints(this._points, this.PADDING);
            }
        }

        //
        // Canvas/paint methods
        //

        /*
         * Clear the canvas and bing the events to the canvas
         */
        public initializeCanvas() {
            this.signatureCanvas = <HTMLCanvasElement>document.getElementById(this.SIGNATURE_CANVAS_ID);

            this.mousedown = this.mousedown.bind(this);
            this.mousemove = this.mousemove.bind(this);
            this.mouseup = this.mouseup.bind(this);
            this.mouseout = this.mouseout.bind(this);
            this.clearCanvas = this.clearCanvas.bind(this);

            this.signatureCanvas.addEventListener("mousedown", this.mousedown, false);
            this.signatureCanvas.addEventListener("mousemove", this.mousemove, false);
            this.signatureCanvas.addEventListener("mouseup", this.mouseup, false);
            this.signatureCanvas.addEventListener("mouseout", this.mouseout, false);

            // Device touch
            this.signatureCanvas.addEventListener("touchstart", this.mousedown, true);
            this.signatureCanvas.addEventListener("touchmove", this.mousemove, true);
            this.signatureCanvas.addEventListener("touchend", this.mouseup, true);
            this.signatureCanvas.addEventListener("touchcancel", this.mouseout, true);

            // Initialize the member variables
            this._points = [];
        }

        /*
         * Clear the canvas
         */
        public clearCanvas() {
            this.signatureCanvas.getContext("2d").clearRect(0, 0, this.signatureCanvas.width, this.signatureCanvas.height);
            this._points = [];
            this.hasNoSignatureData(true);
        }

        /*
         * Draws points, representing a set of vectors, onto the canvas.
         *
         * @param {Commerce.Model.Entities.Point[]} points - The points representing a set of vectors.
         * @param {number} [padding] - The drawing padding.
         */
        public drawPoints(points: Commerce.Model.Entities.Point[], padding: number = 0): void {
            // Check the parameter
            if (!ArrayExtensions.hasElements(points)) {
                this.clearCanvas();
                return;
            }

            // Get the scale and offset
            var imageBounds: Commerce.Controls.Rectangle = this.getImageBounds(points);
            var imageScaleAndOffset: Commerce.Controls.ImageScaleAndOffset = this.getScaleAndOffset(imageBounds, padding);

            // Draw the vectors
            var drawingContext: CanvasRenderingContext2D = this.signatureCanvas.getContext("2d");
            var startPoint: Commerce.Model.Entities.Point = null;
            var pointNotDrawn: boolean = false; // Tracks whether the last point has been drawn. This is to handle the case where the point array did not end with an endpoint.
            drawingContext.beginPath();
            drawingContext.strokeStyle = this.getLineColor();
            drawingContext.globalAlpha = 0.7;
            points.forEach((point: Commerce.Model.Entities.Point) => {
                // If an end point, then reset the vector
                if (this.isEndpoint(point)) {
                    // Draw a single point if not an endpoint and not drawn as part of a vector
                    if (pointNotDrawn) {
                        drawingContext.moveTo(startPoint.x, startPoint.y);
                        drawingContext.lineTo(startPoint.x, startPoint.y);
                        drawingContext.stroke();
                    }

                    startPoint = null;
                    pointNotDrawn = false;
                    // Start a new point in the vector
                } else if (startPoint == null) {
                    startPoint = {
                        x: Math.floor(((point.x - imageBounds.leftX) * imageScaleAndOffset.scale) + imageScaleAndOffset.offsetX),
                        y: Math.floor(((point.y - imageBounds.topY) * imageScaleAndOffset.scale) + imageScaleAndOffset.offsetY)
                    }
                    pointNotDrawn = true;
                    // Draw the vector
                } else {
                    var endPoint: Commerce.Model.Entities.Point = {
                        x: Math.floor(((point.x - imageBounds.leftX) * imageScaleAndOffset.scale) + imageScaleAndOffset.offsetX),
                        y: Math.floor(((point.y - imageBounds.topY) * imageScaleAndOffset.scale) + imageScaleAndOffset.offsetY)
                    };
                    drawingContext.moveTo(startPoint.x, startPoint.y);
                    drawingContext.lineTo(endPoint.x, endPoint.y);
                    drawingContext.stroke();
                    startPoint = endPoint;
                    pointNotDrawn = false;
                }
            });

            // Draw the last point if not an endpoint and not drawn as part of a vector
            if (pointNotDrawn) {
                drawingContext.moveTo(startPoint.x, startPoint.y);
                drawingContext.lineTo(startPoint.x, startPoint.y);
                drawingContext.stroke();
            }

            drawingContext.closePath();
            this.hasNoSignatureData(false);
        }

        /*
         * Get the position of the element on the canvas for mouse events.
         *
         * @param {any} element - The element.
         * @return {any} The position of the element on the canvas.
         */
        private getOffsetPosition(element: any): any {
            var currentLeft = 0;
            var currentTop = 0;

            // Current object offsets
            if (element.offsetLeft) {
                currentLeft += element.offsetLeft;
            }

            if (element.offsetTop) {
                currentTop += element.offsetTop;
            }

            // ScrollTop
            if (element.scrollTop && element.scrollTop > 0) {
                currentTop -= element.scrollTop;
            }

            if (element.offsetParent) {
                var position = this.getOffsetPosition(element.offsetParent);
                currentLeft += position[0];
                currentTop += position[1];
            }

            return [currentLeft, currentTop];
        }

        /*
         * Action to take on mouse button down.
         *
         * @param {any} ev - The mouse event.
         */
        private mousedown(ev: any): void {
            if (this.signatureCanvas.width != this.signatureCanvas.offsetWidth) {
                this.signatureCanvas.width = this.signatureCanvas.offsetWidth;
            }

            if (this.signatureCanvas.height != this.signatureCanvas.offsetHeight) {
                this.signatureCanvas.height = this.signatureCanvas.offsetHeight;
            }

            var position = this.getOffsetPosition(this.signatureCanvas);
            this.canvasX = position[0];
            this.canvasY = position[1];

            ev = SignatureCaptureDialog.getEvent(ev);
            this.paint = true;
            this.startX = ev.pageX - this.canvasX;
            this.startY = ev.pageY - this.canvasY;

            this._points.push(new Commerce.Model.Entities.Point(this.startX, this.startY));
            this.hasNoSignatureData(false);
        }

        /*
         * Action to take on mouse move.
         *
         * @return {string} The line color
         */
        private getLineColor(): string {
            var lineColor: string = "#000";
            if (matchMedia("screen and (-ms-high-contrast)").matches) {
                // A high-contrast theme is active - use system color for line color
                lineColor = "WindowText";
            }

            return lineColor;
        }


        /*
         * Action to take on mouse move.
         *
         * @param {any} ev - The mouse event.
         */
        private mousemove(ev: any): void {
            ev = SignatureCaptureDialog.getEvent(ev);

            if (this.paint) {
                var context = this.signatureCanvas.getContext("2d");

                context.beginPath();

                // Set line color
                context.strokeStyle = this.getLineColor();

                context.moveTo(this.startX, this.startY);
                context.lineTo(ev.pageX - this.canvasX, ev.pageY - this.canvasY);
                context.closePath();
                context.stroke();

                this.startX = ev.pageX - this.canvasX;
                this.startY = ev.pageY - this.canvasY;
                this._points.push(new Commerce.Model.Entities.Point(this.startX, this.startY));
                this.hasNoSignatureData(false);
            }
        }

        /*
         * Action to take on mouse up.
         *
         * @param {any} ev - The mouse event.
         */
        private mouseup(ev: any): void {
            if (this.paint) {
                this.paint = false;
                this._points.push(new Commerce.Model.Entities.Point(this.startX, this.startY));
                this._points.push(this._endPoint);
                this.hasNoSignatureData(false);
            }
        }

        /*
         * Action to take on mouse out.
         *
         * @param {any} ev - The mouse event.
         */
        private mouseout(ev: any): void {
            if (this.paint) {
                this.paint = false;
                this._points.push(new Commerce.Model.Entities.Point(this.startX, this.startY));
                this._points.push(this._endPoint);
                this.hasNoSignatureData(false);
            }
        }

        /*
         * Converts the touch event to a mouse event.
         *
         * @param {any} ev - The touch event.
         * @return {any} The mouse event.
         */
        private static getEvent(ev: any): any {
            // Convert touch to MouseEvent
            if (Commerce.ArrayExtensions.hasElements(ev.touches)) {
                ev.preventDefault();
                return ev.touches[0];
            } else if (Commerce.ArrayExtensions.hasElements(ev.changedTouches)) {
                ev.preventDefault();
                return ev.changedTouches[0];
            }

            return ev;
        }

        //
        // Point utility methods
        //

        /*
         * Checks whether a point is an endpoint.
         *
         * @param {Commerce.Model.Entities.Point} point - The points representing a set of vectors.
         * @return True if it is an endpoint, false otherwise
         */
        private isEndpoint(point: Commerce.Model.Entities.Point): boolean {
            var isEndpoint: boolean = false;

            if (point) {
                isEndpoint = (point.x === this._endPoint.x) || (point.y === this._endPoint.y);
            }

            return isEndpoint;
        }

        /*
         * Converts a byte array to Points.
         *
         * @param {Uint8Array} byteArray - The byte array as points.
         * @return {error: Commerce.Model.Entities.Error, points: Commerce.Model.Entities.Point[]} The error that occured or the points. The value error is null if no error occurred.
         */
        private getByteArrayAsPoints(byteArray: Uint8Array): { error: Commerce.Model.Entities.Error; points: Commerce.Model.Entities.Point[] } {
            var retVal: { error: Commerce.Model.Entities.Error; points: Commerce.Model.Entities.Point[] } = {
                error: null,
                points: []
            }

            // Check whether the system supports the bit operations used to manipulate the data
            if (!SerializationHelpers.isSystemLittleEndian()) {
                retVal.error = new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_VALID_FOR_BIG_ENDIAN_SYSTEM);
                return retVal;
            }

            // Check that the array contains a size that can be converted to points
            if (ObjectExtensions.isNullOrUndefined(byteArray) || (byteArray.length === 0) || ((byteArray.length % 8) != 0)) {
                retVal.error = new Model.Entities.Error(ErrorTypeEnum.SIGNATURE_INVALID_FORMAT);
                return retVal;
            }

            // Convert a byte array of 64 bit (32 bit for x and 32 bit for y) blocks to points
            for (var imageDataByteArrayIndex: number = 0; imageDataByteArrayIndex < byteArray.length; imageDataByteArrayIndex += 8) {
                // Each point is represented by four characters: x (Low 16 bits), x (high 16 bits), y (low 16 bits), y (high 16 bits)
                // Convert x
                var x: number = 0;
                x = byteArray[imageDataByteArrayIndex] & 0x000000FF;
                x = x | ((byteArray[imageDataByteArrayIndex + 1] << 8) & 0x0000FF00);
                x = x | ((byteArray[imageDataByteArrayIndex + 2] << 16) & 0x00FF0000);
                x = x | ((byteArray[imageDataByteArrayIndex + 3] << 24) & 0xFF000000);
                x = x >>> 0;

                // Convert y
                var y: number = 0;
                y = byteArray[imageDataByteArrayIndex + 4];
                y = y | (byteArray[imageDataByteArrayIndex + 5] << 8);
                y = y | (byteArray[imageDataByteArrayIndex + 6] << 16);
                y = y | (byteArray[imageDataByteArrayIndex + 7] << 24);
                y = y >>> 0;

                retVal.points.push(new Commerce.Model.Entities.Point(x, y));
            }

            return retVal;
        }

        /*
         * Gets the points drawn on the image as a byte array.
         *
         * @return The byte array as points.
         */
        public getPointsAsByteArray(): Uint8Array {
            // Check whether the system supports the bit operations used to manipulate the data
            if (!SerializationHelpers.isSystemLittleEndian()) {
                throw new Error(Commerce.ErrorTypeEnum.OPERATION_NOT_VALID_FOR_BIG_ENDIAN_SYSTEM);
            }

            // Build a collection of Points that is sparsely populated with the points with specified pixel data
            var points: Commerce.Model.Entities.Point[] = this._points;

            // Convert points to a byte array of 64 bit (32 bit for x and 32 bit for y)
            var imageDataByteArray: Uint8Array = new Uint8Array(points.length * 4 * 2);
            var imageDataByteArrayIndex: number = 0;
            points.forEach((point: Commerce.Model.Entities.Point) => {
                // Each point is represented by four characters: x (Low 16 bits), x (high 16 bits), y (low 16 bits), y (high 16 bits)
                // Convert x
                imageDataByteArray[imageDataByteArrayIndex] = point.x & 0x000000FF;
                imageDataByteArray[imageDataByteArrayIndex + 1] = (point.x >>> 8) & 0x000000FF;
                imageDataByteArray[imageDataByteArrayIndex + 2] = (point.x >>> 16) & 0x000000FF;
                imageDataByteArray[imageDataByteArrayIndex + 3] = (point.x >>> 24) & 0x000000FF;

                // Convert y
                imageDataByteArray[imageDataByteArrayIndex + 4] = point.y & 0x000000FF;
                imageDataByteArray[imageDataByteArrayIndex + 5] = (point.y >>> 8) & 0x000000FF;
                imageDataByteArray[imageDataByteArrayIndex + 6] = (point.y >>> 16) & 0x000000FF;
                imageDataByteArray[imageDataByteArrayIndex + 7] = (point.y >>> 24) & 0x000000FF;

                imageDataByteArrayIndex += 8;
            });

            return imageDataByteArray;
        }

        //
        // Image scaling methods
        //

        /**
         * For a set of points, get the rectangle that bounds the points
         *
         * @param {Commerce.Controls.Rectangle} imageBounds The points of a rectangle that bounds the image.
         * @param {number} padding The padding that should be applied to the image.
         * @return {Commerce.Controls.ImageScaleAndOffset} The image scale and offset.
         */
        private getScaleAndOffset(imageBounds: Commerce.Controls.Rectangle, padding: number): Commerce.Controls.ImageScaleAndOffset {
            // Get scale and take into the account drawing offset.
            var scaleX: number = (this.signatureCanvas.width - (padding * 2)) / imageBounds.width;
            var scaleY: number = (this.signatureCanvas.height - (padding * 2)) / imageBounds.height;
 
            // Get default scale and calculate offsets for Graphics position.
            // Center image vertically or horizontally depending on which dimension used for scale.
            if (scaleX > scaleY) {
                return {
                    scale: scaleY,
                    offsetX: Math.floor((this.signatureCanvas.width - imageBounds.width * scaleY) / 2), // Center graphics horizontally. 
                    offsetY: padding
                };
            }
            else {
                return {
                    scale: scaleX,
                    offsetX: padding,
                    offsetY: Math.floor((this.signatureCanvas.height - imageBounds.height * scaleX) / 2), // Center graphics vertically.
                };
            }
        }


        /**
         * For a set of points, get the rectangle that bounds the points
         *
         * @param {Commerce.Model.Entities.Point[]} points The points.
         * @return {Commerce.Controls.Rectangle} The rectange that bounds the points.
         */
        private getImageBounds(points: Commerce.Model.Entities.Point[]): Commerce.Controls.Rectangle {
            var imageBounds: Commerce.Controls.Rectangle = {
                leftX: Number.MAX_VALUE,
                topY: Number.MAX_VALUE,
                rightX: Number.MIN_VALUE,
                bottomY: Number.MIN_VALUE,
                height: 0,
                width: 0
            };

            var allEndpoints: boolean = true;
            if (ArrayExtensions.hasElements(points)) {
                points.forEach((point: Commerce.Model.Entities.Point) => {
                    if (!this.isEndpoint(point)) {
                        allEndpoints = false;
                        imageBounds.leftX = point.x < imageBounds.leftX ? point.x : imageBounds.leftX;
                        imageBounds.topY = point.y < imageBounds.topY ? point.y : imageBounds.topY;
                        imageBounds.rightX = point.x > imageBounds.rightX ? point.x : imageBounds.rightX;
                        imageBounds.bottomY = point.y > imageBounds.bottomY ? point.y : imageBounds.bottomY;
                    }
                });

                if (!allEndpoints) {
                    imageBounds.height = imageBounds.bottomY - imageBounds.topY;
                    imageBounds.width = imageBounds.rightX - imageBounds.leftX;
                }
            }

            return imageBounds;
        }

        //
        // Dialog methods
        //

        /**
          * Cancel the dialog.
          */
        private cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        /**
         * Method called when a card type dialog control button is clicked
         *
         * @param {string} operationId The id of the button clicked. No - skip signature, OK - accept signature - Cancel - reject signature - Clear - clear signature.
         */
        public signatureCaptureDialogButtonClickHandler(operationId: string): void {
            switch (operationId) {
                case 'skip':
                    this.dialogResult.resolve(DialogResult.No);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    try {
                        var imageDataByteArray: Uint8Array = this.getPointsAsByteArray();
                        var signatureData = SerializationHelpers.toBase64String(imageDataByteArray);
                    } catch (error) {
                        if (error && error.message && error.message == Commerce.ErrorTypeEnum.OPERATION_NOT_VALID_FOR_BIG_ENDIAN_SYSTEM) {
                            this.dialogResult.reject([new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_VALID_FOR_BIG_ENDIAN_SYSTEM)]);
                            return;
                        }
                    }

                    this.dialogResult.resolve(DialogResult.OK, signatureData);
                    break;
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.cancelDialog();
                    break;
                case 'clear':
                    this.clearCanvas();
                    break;
            }
        }
    }
}