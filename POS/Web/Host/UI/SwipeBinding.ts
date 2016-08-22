/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Host.UI {
    "use strict";

    /**
     * Identifies the direction of the swipe.
     * UpDown used for GridView layout.
     * RightLeft used for ListView layout.
     */
    enum SwipeDirection {
        RigthLeft = 1,
        UpDown = 2
    }

    /**
     * Internal class for storing the touch position.
     */
    class Position {
        x: number;
        y: number;

        constructor(x: number, y: number) {
            this.x = x;
            this.y = y;
        }

       /** 
        * calculates the difference between two positions.
        * @param {Position} pos New position of the touch.
        * @returns {Position} Position difference.
        */
        difference(pos: Position): Position {
            return new Position(
                pos.x - this.x,
                pos.y - this.y
            );
        }

    }

   /**
    * Creates crossplatform swipe event for the ListView because winjs doesn't 
    * support swipe event in browsers other than IE
    */
    export class SwipeBinding {
        /**
         * The max distance for the swipe;
         */
        private static swipeDistance: number = 30;

        /**
         *  The delta for changing element state.
         */
        private static selectionDistance: number = 15;
        private static itemContainerSelector: string = ".win-container";
        private static selectedClass: string = ".win-selected";

        /**
         * Used for getting item index by using the element;
         */
        private static itemElementSelector: string = ".win-template";

        /**
         * The class for overlapping other item containers.
         */
        private static swipeClass: string = "win-swipe";
        private static animationSpeed: string = "slow";

        private _swipeDirection: SwipeDirection;
        private _basePosition: Position;
        private _isSelectedOriginalState: boolean;
        private _winControl: any;
        private _itemIndex: number;
        private _$itemContainer: any;
        private _$grid: JQuery;

        /**
         * Initialization of object state
         * @param {HTMLElement} grid element
         */
        constructor(grid: HTMLElement) {
            if (ObjectExtensions.isNullOrUndefined(grid)) {
                throw new Error("Parameter grid can't be null or undefined.");
            }

            if (ObjectExtensions.isNullOrUndefined(grid.winControl)) {
                throw new Error("Unable to initialize swipe for non-wincontrol element");
            }

            this._$grid = $(grid);
            this._winControl = grid.winControl;
        }

        /** 
         * Identifies touch position by JQuery event.
         * TODO: [aluki] as soon as jquery is updated to 2.0 specify a strict type
         * @param {any} event JQuery event.
         * @returns {Position} touch position.
         */
        private static getEventPosition(event: any): Position {
            var touch: any = event.originalEvent.touches[0] || event.originalEvent.changedTouches[0];
            return new Position(touch.pageX, touch.pageY);
        }

        /**
         * Binds the events to the element.
         */
        public  bind(): void {
            this._$grid.bind("touchstart", this.startTouch.bind(this));
            this._$grid.bind("touchmove", this.moveTouch.bind(this));
            this._$grid.bind("touchend", this.endTouch.bind(this));
            this._swipeDirection = this._winControl.layoutType === WinJS.UI.ListLayout ? SwipeDirection.RigthLeft : SwipeDirection.UpDown;
        }

        /** 
         * Event used for proccesing start of touching.
         * @param {any} event JQuery event.
         */
        private startTouch(event: JQueryEventObject): void {
            var parentContainer: JQuery = $(event.target).parents(SwipeBinding.itemContainerSelector);
            if (parentContainer.length > 0) {
                this._swipeDirection = this._winControl.layout._inListMode ? SwipeDirection.RigthLeft
                    : SwipeDirection.UpDown;
                this._basePosition = SwipeBinding.getEventPosition(event);
                this._$itemContainer = $(parentContainer[0]);
                this._isSelectedOriginalState = this._$itemContainer.is(SwipeBinding.selectedClass);
                var itemElement: Element = this._$itemContainer.find(SwipeBinding.itemElementSelector)[0];
                this._itemIndex = this._winControl._itemsManager._recordFromElement(itemElement).item.index;
            }
        }

        /** 
         * Event used for proccesing end of touching.
         * @param {JQueryEventObject} event JQuery event.
         */
        private endTouch(event: JQueryEventObject): void {
            if (ObjectExtensions.isNullOrUndefined(this._basePosition)) {
                return;
            }
            var currentPosition: Position = SwipeBinding.getEventPosition(event);
            var moveDistance: Position = this._basePosition.difference(currentPosition);
            if (this._swipeDirection === SwipeDirection.RigthLeft && Math.abs(moveDistance.x) > 0) {
                this._$itemContainer.animate({ left: 0 }, SwipeBinding.animationSpeed);
            } else if (this._swipeDirection === SwipeDirection.UpDown && Math.abs(moveDistance.y) > 0) {
                this._$itemContainer.animate({ top: 0 }, SwipeBinding.animationSpeed);
            }
            this._basePosition = null;
            this._$itemContainer = null;
        }

        /** 
         * Event used for proccesing touch movement
         * @param {JQueryEventObject} event JQuery event.
         */
        private moveTouch(event: JQueryEventObject): void {
            if (ObjectExtensions.isNullOrUndefined(this._basePosition)) {
                return;
            }
            var currentPosition: Position = SwipeBinding.getEventPosition(event);
            var positionChange: Position = this._basePosition.difference(currentPosition);
            var distance: number = 0;
            var type: string = "left";

            if (Math.abs(positionChange.x) > 0 && (this._swipeDirection === SwipeDirection.RigthLeft)) {
                distance = positionChange.x;
            } else if (Math.abs(positionChange.y) > 0 && (this._swipeDirection === SwipeDirection.UpDown)) {
                distance = positionChange.y;
                type = "top";
            }
            if (Math.abs(distance) > 0) {
                if (Math.abs(distance) > SwipeBinding.swipeDistance) {
                    distance = distance > 0 ? SwipeBinding.swipeDistance : -SwipeBinding.swipeDistance;
                }
                this._$itemContainer.addClass(SwipeBinding.swipeClass);
                this._$itemContainer.css(type, distance + "px");
                this._$itemContainer.css("transform", ""); // removes transform scale applied by winjs onclick
            }

            var isElementSelected: boolean = this._$itemContainer.is(SwipeBinding.selectedClass);

            // the element requires to change it selection state in two situations. The distance is less 
            // than selection distance and current state is different from original
            // or the distance is bigger than selection distance and current state is the same as original. 
            // in this way achieved animation selection / deselection while you are
            // moving element.
            if ((Math.abs(distance) <= SwipeBinding.selectionDistance && isElementSelected !== this._isSelectedOriginalState) ||
            (Math.abs(distance) > SwipeBinding.selectionDistance && isElementSelected === this._isSelectedOriginalState)) {
                if (isElementSelected) {
                    this._winControl.selection.remove(this._itemIndex);
                } else {
                    this._winControl.selection.add(this._itemIndex);
                }
            }
        }
    }
}