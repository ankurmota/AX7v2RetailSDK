/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Controls/UserControl.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class ViewControllerBase implements IDisposable {
        private _saveInHistory: boolean = true;
        private _children: Controls.UserControl[];
        private  _element: HTMLElement;

        public get saveInHistory() { return this._saveInHistory; }

        constructor(saveInHistory: boolean) {
            this._saveInHistory = saveInHistory;
            this._children = [];
            this._element = null;
        }

        /**
         * Adds a child control to this page.
         */
        public addControl(control: Controls.UserControl) {
            if (control) {
                this._children.push(control);

                if (this._element != null) {
                    this._element.appendChild(control.element);
                }
            }
        }
        
        public getViewContainer(): HTMLElement {
            if (ObjectExtensions.isNullOrUndefined(this._element))
                throw "Element is available only after page is created";

            return this._element;
        }


        /**
         * Occurs when the element of the page is created.
         *
         * @param {HTMLElement} element DOM element.
         */
        public onCreated(element: HTMLElement) {
            this._element = element;
            this._children.forEach((control) => {
                element.appendChild(control.element);
            });
        }

        /**
         * Called when the page is supposed to be unloaded.
         */
        public unload() {
            this._element = null;
            this._children = null;
        }

        /**
         * Called when the page is disposing the resources.
         */
        public dispose(): void {
            ObjectExtensions.disposeAllProperties(this);
        }
    }
}