/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.UI.HelpBubbles {
    "use strict";

    interface IBubbleConfig {
        global: Array<IBubble>;
        byPage: any;
        totalCount: number;
        stopTourBubble: IBubble;
        intro: Array<IBubble>;
    }

    interface IBubble {
        targetElement: string;
        title: Array<string>;
        rotation: number;
        size: string;
        position: IBubblePosition;
        isArrowVisible: boolean;
    }

    interface IBubblePosition {
        top?: number;
        left?: number;
        bottom?: number;
        right?: number;
        anchor: string;
    }

    export class BubbleControl {


        private static _instance: BubbleControl;

        // #region Constants
        private static HIDE_DELAY: number = 7000; // delay before the bubble will hide
        private static TEXT_DELAY: number = 5000; // delay before the text will switch
        private static _delay: number = 0; // delay before bubbles shows on the page
        private static _arrowLeftClass: string = "arrow-left";
        private static _arrowRightClass: string = "arrow-right";
        private static _arrowBottomClass: string = "arrow-bottom";
        private static _viewPortAnchorDistance: string = "50px";
        private static _path: string = "Controls/Bubbles/bubble.config.json";
        private static _bubbleElementSelector: string = ".bubble";
        private static _bubbleContainerSelector: string = ".bubble-container";
        private static _bubbleTextElementSelector: string = ".bubble-text-container";
        private static _rotateContainer: string = ".arrow-rotate-container";
        private static _templateName: string = "bubble-template";
        private static _searhByAttributeSelector: string = "[data-ax-bubble='{0}']";
        private static _rotateFormatter: string = "rotate({0}deg)";
        private static _centerFormatting: string = "calc(50% - {0}px)";
        // #endregion

        /**
         * Indicates that tour is in progress.
         */
        public isInProgress: Observable<boolean>;

        private _arrowColor: string;
        private _bubbleConfig: IBubbleConfig;
        private _activePage: string;
        private _timer: number;
        private _textTimer: number;
        private _activeTitle: Observable<string>;
        private _activeBubble: IBubble;
        private _activeTextIndex: number;
        private _targetHandler: (event: JQueryEventObject) => any;
        private _$target: JQuery;
        private _$container: JQuery;
        private _$pageContainer: JQuery;

        constructor() {
            this.isInProgress = ko.observable(false);
            this._activeTitle = ko.observable(null);
        }

        /**
         * Set instance of BubbleControl.
         */
        public static instance(): BubbleControl {
            if (ObjectExtensions.isNullOrUndefined(this._instance)) {
                this._instance = new BubbleControl();
            }

            return this._instance;
        }

        /**
         * Starts the bubble tour from beginning.
         */
        public reset(location: string, element: HTMLElement, arrowColor: string): void {
            this._arrowColor = arrowColor;
            $.getJSON(BubbleControl._path)
                .done(((data: IBubbleConfig) => {
                    this._bubbleConfig = data;
                    this._$container = $(BubbleControl._bubbleContainerSelector);
                    this._targetHandler = ((event: JQueryEventObject) => { this.moveNext(true); }).bind(this);
                    this.isInProgress(true);
                    this.onAfterNavigate(location, element);
                }).bind(this));
        }

        /**
         * Stop the bubble tour from beginning.
         */
        public stopTour(showHowToEnable: boolean): void {
            if (!ObjectExtensions.isNullOrUndefined(this._bubbleConfig.stopTourBubble) && showHowToEnable) {
                // clear tour bubbles
                this._bubbleConfig.intro = [];
                this._bubbleConfig.global = [this._bubbleConfig.stopTourBubble];
                this._bubbleConfig.totalCount = 1;
                this._bubbleConfig.byPage = {};
                this.moveNext(true);
            } else {
                this.hideActiveBubble();
            }

            this.isInProgress(false);
        }

        /**
         * Happens before the navigation happens to hide current bubble.
         */
        public onBeforeNavigate(): void {
            this._activePage = null;
            this.hideActiveBubble();
        }

        /**
         * Should be called when navigation happens.
         */
        public onAfterNavigate(location: string, element: HTMLElement): void {
            this._activePage = location;
            this._$pageContainer = $(element);
            this._timer = window.setTimeout(this.moveNext.bind(this), BubbleControl._delay);
        }

        private textSwitch(): boolean {
            if (ObjectExtensions.isNullOrUndefined(this._activeBubble)) {
                return false;
            }
            if (this._activeBubble.title.length > this._activeTextIndex) {
                this._activeTitle(Commerce.ViewModelAdapter.getResourceString(this._activeBubble.title[this._activeTextIndex]));
                if (this._activeTextIndex + 1 < this._activeBubble.title.length) {
                    this._textTimer = window.setTimeout(this.moveNext.bind(this), BubbleControl.TEXT_DELAY);
                } else {
                    return false;
                }

                return true;
            }

            return false;
        }

        /**
         * Moves to next bubble.
         * @param {boolean} isTargetClink. Identifies whether the target element 
         * was clicked for bubble to decide whether to change the text
         * or move to the next bubble.
         */
        private moveNext(isTargetClick: boolean = false): void {
            this.clearTimers();
            if (!isTargetClick) {
                if (this.textSwitch()) {
                    return;
                }
            }

            this.hideActiveBubble();
            this._timer = window.setTimeout(this.showBubble.bind(this), BubbleControl._delay);
        }

        /**
         * Shows active bubble.
         */
        private showBubble(): void {
            var bubble: IBubble = this.getNextBubble();
            if (ObjectExtensions.isNullOrUndefined(bubble)) {
                return;
            }

            if (!ObjectExtensions.isNullOrUndefined(this._$target)) {
                this._$target.bind("click", this._targetHandler);
            }
            this._activeTextIndex = 0;
            this._activeTitle(Commerce.ViewModelAdapter.getResourceString(bubble.title[this._activeTextIndex]));
            this._activeBubble = bubble;
            bubble.isArrowVisible =
                 ObjectExtensions.isNullOrUndefined(bubble.isArrowVisible) ? true : bubble.isArrowVisible;

            ko.applyBindingsToNode(this._$container[0], {
                template: {
                    name: BubbleControl._templateName,
                    data: {
                        title: this._activeTitle,
                        size: bubble.size,
                        isArrowVisible: bubble.isArrowVisible,
                        arrowColor: this._arrowColor
                    }
                },
                click: this.moveNext.bind(this)
            });

            var $bubbleElement: JQuery = this._$container.find(BubbleControl._bubbleElementSelector);
            this.setPosition(this._$target, bubble, $bubbleElement);
            if (bubble.title.length > 1) {
                this._textTimer = window.setTimeout(this.textSwitch.bind(this), BubbleControl.TEXT_DELAY);
            } else {
                this._timer = window.setTimeout(this.moveNext.bind(this), BubbleControl.HIDE_DELAY);
            }
        }

        private setRotation($bubbleElement: JQuery, rotation: number): void {
            var $rotateContainer: JQuery = $bubbleElement.find(BubbleControl._rotateContainer);
            $rotateContainer.css("transform", StringExtensions.format(BubbleControl._rotateFormatter, rotation + 45));
        }

        private getNextBubble(): IBubble {
            if (this._bubbleConfig.totalCount === 0) {
                this.isInProgress(false);
                return null;
            }
            var bubble: IBubble = null;
            var bubbleList: Array<IBubble>;
            var pageBubbles: Array<IBubble> = this._bubbleConfig.byPage[this._activePage];
            if (this._bubbleConfig.intro.length > 0) {
                bubbleList = this._bubbleConfig.intro;
            } else if (pageBubbles && pageBubbles.length > 0) {
                bubbleList = pageBubbles;
            } else if (this._bubbleConfig.global.length > 0) {
                bubbleList = this._bubbleConfig.global;
            }
            if (bubbleList) {
                bubble = bubbleList[bubbleList.length - 1];
                if (bubble.targetElement !== "viewport") {
                    var targetElement: JQuery =
                        this._$pageContainer.find(StringExtensions.format(BubbleControl._searhByAttributeSelector, bubble.targetElement));
                    if (targetElement.length === 0 || targetElement.is(":hidden")) {
                        // continue the loop untill the element will appear.
                        this._timer = window.setTimeout(this.moveNext.bind(this), BubbleControl._delay);
                        return null;
                    } else {
                        this._$target = targetElement;
                    }
                } else {
                    this._$target = null;
                }
                bubbleList.pop();
                this._bubbleConfig.totalCount--;
            }

            return bubble;
        }

        /**
         * Calculates the position for the current element.
         * @param {Jquery} $target. Target element.
         * @param {IBubble} bubble. Bubble configuration.
         * @param {JQuery} $bubbleElem. Bubble element
         */
        private setPosition($target: JQuery, bubble: IBubble, $bubbleElem: JQuery): void {
            var top: number = 0;
            var left: number = 0;
            var position: IBubblePosition = bubble.position;
            this.setRotation($bubbleElem, bubble.rotation);

            if (bubble.targetElement === "viewport") {
                this.positionInViewPort(bubble, $bubbleElem);
                return;
            }

            if (ObjectExtensions.isNullOrUndefined(position.top)) {
                top = $target.offset().top + $target.outerHeight() - position.bottom;
            } else {
                top = $target.offset().top + position.top;
            }

            if (ObjectExtensions.isNullOrUndefined(position.left)) {
                left = $target.offset().left + $target.outerWidth() - position.right;
            } else {
                left = $target.offset().left + position.left;
            }

            var elementPosition: { top: number; left: number } =
                this.fixPosition($bubbleElem, bubble, top, left);

            $bubbleElem.css("top", elementPosition.top + "px");
            $bubbleElem.css("left", elementPosition.left + "px");
        }

       /**
        * Positions the bubble if the target is viewport
        * @param {IBubble} bubble. Bubble configuration.
        * @param {JQuery} $bubbleElem. Bubble element
        */
        private positionInViewPort(bubble: IBubble, $bubbleElem: JQuery): void {
            // dont need top fix top and left position.
            this.fixPosition($bubbleElem, bubble, 0, 0);
            $bubbleElem.css("position", "fixed");
            if (bubble.position.anchor === "center") {
                // because this is square the height and width are equal.
                var elementSize: number = $bubbleElem.height();
                var position: string = StringExtensions.format(BubbleControl._centerFormatting, elementSize / 2);
                $bubbleElem.css("top", position);
                $bubbleElem.css("left", position);
                return;
            }

            if (bubble.position.anchor.indexOf("left") >= 0) {
                $bubbleElem.css("left", BubbleControl._viewPortAnchorDistance);
            }

            if (bubble.position.anchor.indexOf("top") >= 0) {
                $bubbleElem.css("top", BubbleControl._viewPortAnchorDistance);
            }

            if (bubble.position.anchor.indexOf("bottom") >= 0) {
                $bubbleElem.css("bottom", BubbleControl._viewPortAnchorDistance);
            }

            if (bubble.position.anchor.indexOf("right") >= 0) {
                $bubbleElem.css("right", BubbleControl._viewPortAnchorDistance);
            }
        }

       /**
        * Fix bubble and text position according to arrow direction
        * @param {Jquery} $target. Target element.
        * @param {IBubble} bubble. Bubble configuration.
        * @param {JQuery} $bubbleElem. Bubble element
        * @param {number} top. Current top position
        * @param {number} left. Current left positi0n
        */
        private fixPosition($bubbleElem: JQuery, bubble: IBubble, top: number, left: number): { top: number; left: number; } {
            var elementHeight: number = $bubbleElem.height();
            var elementWidth: number = $bubbleElem.width();
            var $textElement: JQuery = $bubbleElem.find(BubbleControl._bubbleTextElementSelector);
            var paddingBottom: number = (<any>window).parseInt($textElement.css("padding-bottom"));

            if (!bubble.isArrowVisible) {
                $textElement.css("padding-top", paddingBottom + "px");
                return null;
            }

            // fix bubble position according to arrow direction
            if (bubble.rotation > 0) {
                left -= elementWidth;
            }


            if (bubble.rotation === -90 || bubble.rotation === 90) {
                top -= elementHeight / 2;
                var paddingClass: string = bubble.rotation > 0 ? BubbleControl._arrowRightClass : BubbleControl._arrowLeftClass;
                $textElement.addClass(paddingClass);
            }

            if (bubble.rotation >= 135 || bubble.rotation <= -135) {
                top -= elementHeight;
                $textElement.addClass(BubbleControl._arrowBottomClass);
            }

            return { top: top, left: left };
        }

        /**
         * Clears current times.
         */
        private clearTimers(): void {
            window.clearTimeout(this._timer);
            window.clearTimeout(this._textTimer);
        }

        /**
         * Removes active Dom events and bindings.
         */
        private hideActiveBubble(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._$target)) {
                this._$target.unbind("click", this._targetHandler);
            }

            if (this._bubbleConfig.totalCount <= 0) {
                this.isInProgress(false);
            }

            if (!ObjectExtensions.isNullOrUndefined(this._activeBubble)) {
                (<any>ko).cleanNode(this._$container[0]);
                this._$container.empty();
                this._activeBubble = null;
                this._activeTextIndex = -1;
                this.clearTimers();
            }
        }
    }
}