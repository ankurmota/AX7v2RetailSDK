/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../JQuery.d.ts'/>
///<reference path='../KnockoutJS.d.ts'/>

module Commerce {
    "use strict";

    export class ObjectExtensions {
        /**
         * Verifies whether the object is null.
         * Remarks: if the object is undefined, this returns false.
         * @param {any} object The object.
         * @return {boolean} True if the object is null, false otherwise.
         */
        public static isNull(object: any): boolean {
            return (object === null);
        }

        /**
         * Verifies whether the object is undefined.
         * Remarks: if the object is null, this returns false.
         * @param {any} object The object.
         * @return {boolean} True if the object is undefined, false otherwise.
         */
        public static isUndefined(object: any): boolean {
            return (typeof object === "undefined");
        }

        /**
         * Verifies whether the object is of the given type.
         * Remarks: if the object is undefined, this returns false.
         * @param {any} object The object.
         * @param {any} type The type to check against.
         * @return {boolean} True if the object is of the given type, false otherwise.
         */
        public static isOfType(object: any, type: any): boolean {
            return !ObjectExtensions.isUndefined(object) && object instanceof type;
        }

        /**
         * Verifies whether the object is null or undefined.
         * @param {any} object The object.
         * @return {boolean} True if the object is null or undefined, false otherwise.
         */
        public static isNullOrUndefined(object: any): boolean {
            return ObjectExtensions.isNull(object)
                || ObjectExtensions.isUndefined(object);
        }

        /**
         * Verifies whether the object is a function.
         * @param {any} object The object.
         * @return {boolean} True if the object is a function, false otherwise.
         */
        public static isFunction(object: any): boolean {
            return typeof object === "function";
        }

        /**
         * Verifies whether the variable is of type object.
         * @param {any} variable The variable.
         * @return {boolean} True if the variable is an object, false otherwise.
         */
        public static isObject(variable: any): boolean {
            return typeof variable === "object";
        }

        /**
         * Verifies whether the variable is of type number.
         * @param {any} variable The variable.
         * @return {boolean} True if the variable is a number, false otherwise.
         */
        public static isNumber(variable: any): boolean {
            return typeof variable === "number";
        }

        /**
         * Verifies whether the variable is of type string.
         * @param {any} variable The variable.
         * @return {boolean} True if the variable is a string, false otherwise.
         */
        public static isString(variable: any): boolean {
            return typeof variable === "string";
        }

        /**
         * Verifies whether the variable is of type boolean.
         * @param {any} variable The variable.
         * @return {boolean} True if the variable is a number, false otherwise.
         */
        public static isBoolean(variable: any): boolean {
            return typeof variable === "boolean";
        }

        /**
         * Converts the object to an observable proxy object.
         * Returns the same object that is provided as parameter.
         * Every property that of the object will become an observable.
         * e.g. for an object like this:
         * obj = {
         *     simpleProperty: "a",
         *     complexProperty: {
         *         someProperty: "b"
         *     }
         * }
         * we will have:
         *    obj.simpleProperty()   <- observable
         *    obj.complexProperty <- object
         *    obj.complexProperty.someProperty() <- observable
         * To update an proxy use obj.update(newObjectValue).
         * This will update every observable in your obj with the values found in the newObjectValue.
         * @param {any} obj. Object that needs to converted to observable.
         * @param {()=> {}} changeCallback. Function that will be called whenever the proxy object has changed. Will not be persisted to the non-proxy object. 
         */
        public static convertToObservableProxyObject(obj: any, changeCallback: () => void = null): any {
            if (ObjectExtensions.isNullOrUndefined(obj) || obj.__isObservableProxy ||
                typeof obj === "string" || typeof obj === "number" || typeof obj === "boolean") {
                return obj;
            }

            obj.__isObservableProxy = true;

            // get all properties from the object
            var keys: string[] = Object.keys(obj);

            // for each property
            for (var i: number = 0; i < keys.length; i++) {
                var key: string = keys[i];
                var keyValue: any = obj[key];
                var keyType: string = typeof keyValue;

                // if of specified type
                if (keyType === "string"
                    || keyType === "number"
                    || keyType === "boolean"
                    || keyType === "undefined"
                    || keyValue === null
                    || keyValue instanceof Date
                    || key === "CreatedDateTime") {
                    obj[key] = ko.observable(keyValue);
                    if (!this.isNullOrUndefined(changeCallback)) {
                        obj[key].subscribe(() => {
                            changeCallback();
                        });
                    }
                } else if ($.isArray(keyValue)) {
                    keyValue.forEach((val: any) => ObjectExtensions.convertToObservableProxyObject(val, changeCallback));
                    obj[key] = ko.observableArray(keyValue);
                    if (!this.isNullOrUndefined(changeCallback)) {
                        obj[key].subscribe(() => {
                            changeCallback();
                        });
                    }
                } else if (keyType === "object") {
                    obj[key] = keyValue || null;
                    ObjectExtensions.convertToObservableProxyObject(obj[key], changeCallback);
                }
            }

            obj.update = ObjectExtensions.updateObservableProxyObject;

            return obj;
        }

        /**
         * Converts the clone of an object to observable proxy object.
         * @param {any} object The object.
         * @return {any} clone of an object to observable proxy object.
         */
        public static cloneToObservableProxyObject(obj: any): any {
            if (ObjectExtensions.isNullOrUndefined(obj)) {
                return obj;
            }
            var clone: any = ObjectExtensions.clone(obj);
            return ObjectExtensions.convertToObservableProxyObject(clone);
        }

        /**
         * Unwraps an proxy observable unwrapping all inner observables.
         * Any callbacks on the proxy object will not be persisted to the non-proxy object.
         */
        public static unwrapObservableProxyObject(proxy: any): any {
            if (!proxy.__isObservableProxy) {
                return proxy;
            }

            var unwrappedObject: any = <any>{};

            // get all properties from the object
            var keys: string[] = Object.keys(proxy);

            // for each property
            for (var i: number = 0; i < keys.length; i++) {
                var key: string = keys[i];
                var keyValue: any = proxy[key];

                if (keyValue == null) {
                    continue;
                } else if (ko.isObservable(keyValue)) {
                    // if property is a observable
                    // unwrap it
                    unwrappedObject[key] = keyValue();

                    // if property is array, it may contain observable proxies as well, so  
                    // unwrap all elements of it
                    if ($.isArray(unwrappedObject[key])) {
                        var array: any[] = unwrappedObject[key];
                        for (var j: number = 0; j < array.length; j++) {
                            array[j] = ObjectExtensions.unwrapObservableProxyObject(array[j]);
                        }
                    }
                } else if (typeof keyValue === "object" && keyValue.__isObservableProxy) {
                    // if it's a proxy inside a proxy, unwrap as well
                    unwrappedObject[key] = ObjectExtensions.unwrapObservableProxyObject(keyValue);
                } else {
                    // just a property, let's copy it
                    unwrappedObject[key] = keyValue;
                }
            }

            delete unwrappedObject.__isObservableProxy;
            delete unwrappedObject.update;

            return unwrappedObject;
        }

        /**
         * Makes a copy of an object.
         * Alternative for jquery (does not handle arrays correctly and it will break calls with Odata):
         * jquery example call that this method replaces: <any >$.extend(true, { }, <your object to clone >);
         * @param {T} origObject The original object to clone.
         * @return {T} The cloned object.
         */
        public static clone<T>(origObject: T): T {
            return <T>ObjectExtensions.safeClone(origObject, []);
        }

        /**
         * Help execute async calls in a sequence.
         * @param {any[]} array The elements array.
         * @param {(entity, () => void) => void)} iterator Async iterator callback.
         * @param {any} [then] Success callback.
         */
        public static forEachAsync<T>(array: T[], iterator: (element: T, next: () => void, index?: number) => void, then?: () => void): void {

            function next(i?: number): void {
                if (i < array.length) {
                    iterator(array[i], () => { next(i + 1); }, i);
                } else if (then) {
                    then();
                }
            };

            next(0);
        }

        /**
         * Iterates over each key value pair in an object.
         * @param {any} object to iterate over its keys.
         * @param {(key: string, value: TValue) => boolean|void} iterator function that is called for each key value pair in the obejct.
         * The iterator function might return FALSE to  break the loop.
         */
        public static forEachKeyValuePair<TValue>(object: any, iterator: (key: string, value: TValue) => boolean|void): void {
            if (ObjectExtensions.isNullOrUndefined(object)) {
                return;
            }

            var keys: string[] = Object.keys(object);
            var stop: boolean = false;
            for (var i: number = 0; !stop && i < keys.length; i++) {
                var key: string = keys[i];
                stop = iterator(key, object[key]) === false;
            }
        }

        /**
         * Creates a grouped array from a given array based on key.
         * @param {T[]} array The elements array.
         * @param {(T) => any} keySelector The key selector function.
         * @return {T[][]} The grouped array.
         */
        public static groupBy<T>(inputArray: T[], keySelector: (value: T) => any): T[][] {
            var groupedArray: T[][] = [];

            if (!ArrayExtensions.hasElements(inputArray)) {
                return groupedArray;
            }

            inputArray.forEach((element: T) => {
                var groupKey: any = keySelector(element);

                if (typeof (groupedArray[groupKey]) === "undefined") {
                    groupedArray[groupKey] = [];
                }

                groupedArray[groupKey].push(element);
            });

            return groupedArray;
        }

        /**
         * Tries to run dispose function on the object.
         * @param {IDisposable} disposableObject The object to use for dispose
         * @return {boolean} Returns true if dispose function found and executed. False otherwise.  
         */
        public static tryDispose(disposableObject: IDisposable): boolean {
            var disposedKey: string = "_dax_disposed"; // flag for already disposed objects
            if (!ObjectExtensions.isNullOrUndefined(disposableObject)
                && ObjectExtensions.isFunction(disposableObject.dispose)
                && !disposableObject[disposedKey]) {
                // To break circular references we need to mark already disposed objects. It needs to be set before dispose.
                disposableObject[disposedKey] = true;
                disposableObject.dispose();
                return true;
            }

            return false;
        }

        /**
         * Tries to run dispose function on all properties of the object and reset to null to singal GC it is safe to collect.
         * @param {IDisposable} obj The object to use for dispose.
         */
        public static disposeAllProperties(obj: IDisposable): void {
            if (!ObjectExtensions.isNullOrUndefined(obj) && ObjectExtensions.isObject(obj)) {
                for (var propertyKey in obj) {
                    if (obj.hasOwnProperty(propertyKey)) {
                        var property: any = obj[propertyKey];

                        // Dispose arrays explicityly (to handle instances when we have control collection in controller)
                        // Do not reset array properties to null. We have instances of simple entity arrays that are shared (ApplicationContext).
                        if (Array.isArray(property)) {
                            for (var itemIndex in property) {
                                if (property.hasOwnProperty(itemIndex)) {
                                    ObjectExtensions.tryDispose(property[itemIndex]);
                                }
                            }
                        } else {
                            ObjectExtensions.tryDispose(property);
                        }

                        obj[propertyKey] = null; // clear reference
                    }
                }
            }
        }

        /**
         * Do not use this function directly. Call update method on your proxy object.
         */
        private static updateObservableProxyObject(newValue: any): void {
            if (ObjectExtensions.isNullOrUndefined(newValue)) {
                return;
            }

            // get all properties from the object
            var keys: string[] = Object.keys(this);

            // for each property
            for (var i: number = 0; i < keys.length; i++) {
                var key: string = keys[i];
                var keyValue: any = this[key];

                if (keyValue == null) {
                    continue;
                } else if ("subscribe" in keyValue) {
                    // if property is a observable
                    // update observable with new value
                    keyValue(newValue[key]);
                } else if (typeof keyValue === "object" && newValue[key] != null) {
                    // if it's another complex object, update it as a if it were a new proxy
                    keyValue.update(newValue[key]);
                }
            }
        }

        /**
         * Makes a copy of an object
         * Alternative for jquery (does not handle arrays correctly and it will break calls with Odata):
         * @param {any} origObject The original object to clone
         * @param {any} cloneMap A map of the objects that are being cloned to identify circular references
         * @return {any} The cloned object
         */
        private static safeClone(origObject: any, cloneMap: any[]): any {
            if (ObjectExtensions.isNullOrUndefined(origObject)) {
                return origObject;
            }

            var newObj: any;
            if (origObject instanceof Array) {
                // Use the array reference in the object map if it exists
                if (!cloneMap.some((val: any): boolean => {
                    if (val.id === origObject) {
                        newObj = val.value;
                        return true;
                    }

                    return false;
                })) {
                    // Add the cloned array reference to the object map and clone the array
                    newObj = [];
                    cloneMap.push({ id: origObject, value: newObj });
                    for (var i: number = 0; i < origObject.length; i++) {
                        if (typeof origObject[i] === "object") {
                            newObj.push(ObjectExtensions.safeClone(origObject[i], cloneMap));
                        } else {
                            newObj.push(origObject[i]);
                        }
                    }
                }
            } else if (origObject instanceof Date) {
                newObj = new Date((<Date> origObject).valueOf());
            } else if (origObject instanceof Object) {
                // Use the object reference in the object map if it exists
                if (!cloneMap.some((val: any): boolean => {
                    if (val.id === origObject) {
                        newObj = val.value;
                        return true;
                    }

                    return false;
                })) {
                    // Add the cloned object reference to the object map and clone the object
                    newObj = <any >$.extend(false, {}, origObject);
                    cloneMap.push({ id: origObject, value: newObj });
                    for (var property in newObj) {
                        if (newObj.hasOwnProperty(property)) {
                            if (typeof newObj[property] === "object") {
                                if (property === "__metadata") {
                                    newObj[property] = <any >$.extend(false, {}, origObject[property]);
                                } else {
                                    newObj[property] = ObjectExtensions.safeClone(origObject[property], cloneMap);
                                }
                            }
                        }
                    }
                }
            } else {
                newObj = origObject;
            }

            return newObj;
        }
    }
}