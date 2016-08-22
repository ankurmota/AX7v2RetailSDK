/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce.Controls {
    "use strict";

    /**
     * Base class for user controls.
     */
    export class UserControl {
        private _children: UserControl[];
        private _element: HTMLDivElement;

        // The _viewPath variable is dynamically set during application load and should never be changed.
        private _viewPath: string;

        /**
         * Constructs a new instance of the UserControl class.
         */
        constructor() {
            this._element = null;
            this._children = null;
        }

        /**
         * Gets the div element where the user control is displayed on.
         * @return {HTMLElement} The div element where the user control is displayed on.
         */
        public get element(): HTMLElement {
            if (this._element == null) {
                this._element = document.createElement("div");
                this._element.innerHTML = StringExtensions.EMPTY;
            }

            return this._element;
        }

        /**
         * Gets the children of this control.
         * @return {UserControl[]} The children of this control.
         */
        public get children(): UserControl[] {
            if (this._children == null) {
                this._children = [];
            }

            return this._children;
        }

        /**
         * This method is called when the control has been loaded into the DOM.
         */
        protected onLoaded(): void {
            // this method is supposed to be implemented in deriving controls
        }

        /**
         * Adds a child control to this control.
         * @param {UserControl} control The user control do add.
         */
        public addControl(control: UserControl): void {
            if (control) {
                this.children.push(control);

                if (this._element != null) {
                    this._element.appendChild(control.element);
                }
            }
        }

        /**
         * This method renders the control asynchronously.
         * @return {IVoidAsyncResult} The async result for when the control is rendered.
         */
        public render(): IVoidAsyncResult {
            if (this._viewPath !== null && StringExtensions.isNullOrWhitespace(this.element.innerHTML)) {
                var asyncResult: VoidAsyncResult = new VoidAsyncResult();
                WinJS.UI.Pages.render(this._viewPath, this.element, this)
                    .done(() => { asyncResult.resolve(); });

                return asyncResult;
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Sets the focus on the control.
         */
        public focus(): void {
            this.element.focus();
        }

        /**
         * This method is called when the element that contains the control has been created.
         * It can be used to load more controls inside this control.
         */
        
        private onCreated(): void {
            this.children.forEach((control: UserControl): void => {
                this.element.appendChild(control.element);
            });
        }
        
    }
}