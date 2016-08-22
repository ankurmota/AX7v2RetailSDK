/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";

    /**
      * Provides support for lazy initialization.
      */
    export class Lazy<T> {
        private _valueFactory: () => T;
        private _value: T;

        /**
          * Gets the value.
          *
          * @returns {T} The value.
          */
        public get value(): T {
            if (this._value == undefined)
            {
                this._value = this._valueFactory();
            }
            return this._value;
        }
        
        /**
          * Instantiates a new instance of the Lazy<T> class. When lazy initialization occurs, the specified initialization function is used.
          *
          * @param {() => T} 
          */
        constructor(valueFactory: () => T) {
            this._valueFactory = valueFactory;
        }
    }
}