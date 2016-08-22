/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ObjectExtensions.ts'/>
///<reference path='../Tracer.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
     * Delegate for equality comparers
     */
    export interface IEqualityComparer<T> {
        (left: T, right: T): boolean
    }

    export class ArrayExtensions {

        /**
         * Verifies whether the object array has elements.
         *
         * @param {any[]} array The array.
         * @return {boolean} True if the object has elements, false otherwise.
         */
        public static hasElements(array: any[]): boolean {
            return !ObjectExtensions.isNullOrUndefined(array) && array.length > 0;
        }

        /**
         * Verifies whether the array has element.
         *
         * @param {T[]} array The array.
         * @param {T} element The element
         * @param {IEqualityComparer<T>} [equalityComparer] An equality comparer to compare values.
         * @return {boolean} True if the object has element, false otherwise.
         */
        public static hasElement<T>(array: T[], element: T, equalityComparer?: IEqualityComparer<T>): boolean {
            if (!ArrayExtensions.hasElements(array)) {
                return false;
            }
            var equals: IEqualityComparer<T> = ArrayExtensions._getEqualityComparer(equalityComparer);

            for (var i in array) {
                if (equals(array[i], element))
                    return true;
            }
            return false;
        }


        //#region Set operations

        /**
         * Returns distinct elements from an array.
         *
         * @param {T[]} array An array to remove duplicate elements from.
         * @param {IEqualityComparer<T>} [equalityComparer] An equality comparer to compare values.
         * @return {T[]} An array that contains distinct elements from the source array.
         */
        public static distinct<T>(array: T[], equalityComparer?: IEqualityComparer<T>): T[] {
            if (!ArrayExtensions.hasElements(array)) {
                return array;
            }
            var equals: IEqualityComparer<T> = ArrayExtensions._getEqualityComparer(equalityComparer);

            var distinct: T[] = [];
            o: for (var i = 0, n = array.length; i < n; i++) {
                for (var x = 0, y = distinct.length; x < y; x++) {
                    if (equals(distinct[x], array[i])) {
                        continue o;
                    }
                }
                distinct.push(array[i]);
            }

            return distinct;
        }

        /**
         * Produces the set intersection of two arrays.
         *
         * @param {T[]} left An array whose distinct elements that also appear in right will be returned.
         * @param {T[]} right An array whose distinct elements that also appear in left will be returned.
         * @param {IEqualityComparer<T>} [equalityComparer] An equality comparer to compare values.
         * @return {T[]} An array that contains the elements that form the set intersection of two arrays.
         */
        public static intersect<T>(left: T[], right: T[], equalityComparer?: IEqualityComparer<T>): T[] {
            return ArrayExtensions._differenceOrIntersect(left, right, false, equalityComparer);
        }

        /**
         * Produces the set intersection of multiple arrays.
         *
         * @param {IEqualityComparer<T>} equalityComparer An equality comparer to compare values. (null for defaultEqualityComparer '==')
         * @param {...T[]} arrays Arrays whose distinct elements that also appear in all other arrays will be returned.
         * @return {T[]} An array that contains the elements that form the set intersection of arrays.
         */
        public static intersectMultiple<T>(equalityComparer: IEqualityComparer<T>, ...arrays: T[][]): T[] {

            if (!ObjectExtensions.isNullOrUndefined(arrays)) {
                return [];
            }
            if (arrays.length == 1) {
                return arrays[0];
            }

            var result: T[] = arrays[0];

            for (var i = 1; i < arrays.length; i++) {
                result = ArrayExtensions.intersect(result, arrays[i], equalityComparer);
            }

            return result;
        }

        /**
         * Produces the set difference of two arrays.
         *
         * @param {T[]} left An array whose elements that are not also in second will be returned.
         * @param {T[]} right whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.
         * @param {IEqualityComparer<T>} [equalityComparer] An equality comparer to compare values.
         * @return {T[]} A sequence that contains the set difference of the elements of two sequences.
         */
        public static difference<T>(left: T[], right: T[], equalityComparer?: IEqualityComparer<T>): T[] {
            return ArrayExtensions._differenceOrIntersect(left, right, true, equalityComparer);
        }

        /**
         * Produces the set union of two arrays.
         *
         * @param {T[]} left An array whose distinct elements form the first set for the union.
         * @param {T[]} right An array whose distinct elements form the second set for the union.
         * @param {IEqualityComparer<T>} [equalityComparer] An equality comparer to compare values.
         * @return {T[]} An array that contains the elements from both input sequences, excluding duplicates.
         */
        public static union<T>(left: T[], right: T[], equalityComparer?: IEqualityComparer<T>): T[] {
            if (ObjectExtensions.isNullOrUndefined(left) || ObjectExtensions.isNullOrUndefined(right)) {
                return null;
            }

            return ArrayExtensions.distinct(left.concat(right), equalityComparer);
        }

        /**
         * Produces the symmetrical difference of two arrays.
         *
         * @param {T[]} left An array whose distinct elements form the first set for the symmetric difference.
         * @param {T[]} right An array whose distinct elements form the second set for the symmetric difference.
         * @param {IEqualityComparer<T>} [equalityComparer] An equality comparer to compare values.
         * @return {T[]} An array that contains the elements in either of the arrays and not in their intersection.
         */
        public static symmetricDifference<T>(left: T[], right: T[], equalityComparer?: IEqualityComparer<T>): T[] {
            if (!ArrayExtensions.hasElements(left)) {
                return ArrayExtensions.distinct(right, equalityComparer);
            }

            if (!ArrayExtensions.hasElements(right)) {
                return ArrayExtensions.distinct(left, equalityComparer);
            }

            var union: T[] = ArrayExtensions.union(left, right, equalityComparer);
            var intersect: T[] = ArrayExtensions.intersect(left, right);

            return ArrayExtensions.difference(union, intersect);
        }

        //#endregion

        //#region LINQ-like operations

        /**
         * Returns the first element of an array, or undefined if the sequence contains no elements.
         *
         * @param {T[]} array The array.
         * @param {(element: T) => boolean} [predicate] A function to test each element for a condition.
         * @return {boolean} undefined if array is empty or if no element passes the test specified by predicate; otherwise, the first element in source that passes the test specified by predicate.
         */
        public static firstOrUndefined<T>(array: T[], predicate?: (element: T) => boolean): T {

            if (!ArrayExtensions.hasElements(array)) {
                return undefined;
            }

            if (ObjectExtensions.isFunction(predicate)) {

                for (var i = 0; i < array.length; i++) {
                    if (predicate(array[i])) {
                        return array[i];
                    }
                }

                return undefined;
            } else {
                return array[0];
            }
        }

        /**
         * Returns the last element of an array, or undefined if the sequence contains no elements.
         *
         * @param {T[]} array The array.
         * @param {(element: T) => boolean} [predicate] A function to test each element for a condition.
         * @return {boolean} undefined if array is empty or if no element passes the test specified by predicate; otherwise, the last element in source that passes the test specified by predicate.
         */
        public static lastOrUndefined<T>(array: T[], predicate?: (element: T) => boolean): T {

            if (!ArrayExtensions.hasElements(array)) {
                return undefined;
            }

            if (ObjectExtensions.isFunction(predicate)) {

                for (var i = array.length - 1; i >= 0; i--) {
                    if (predicate(array[i])) {
                        return array[i];
                    }
                }

                return undefined;
            } else {
                return array[array.length - 1];
            }
        }

        /**
         * Filters an array of values based on a predicate.
         *
         * @param {T[]} array The array.
         * @param {(element: T) => boolean} predicate A function to test each element for a condition.
         * @return {T[]} An array that contains elements from the input sequence that satisfy the condition.
         */
        public static where<T>(array: T[], predicate: (element: T) => boolean): T[] {

            if (!ArrayExtensions.hasElements(array)) {
                return [];
            }

            return array.filter(predicate);
        }

        /**
         * Computes the sum of the array of numeric values that are obtained by invoking a transform function on each element of the array.
         *
         * @param {T[]} array The array.
         * @param {(element: T) => number} selector A transform function to apply to each element.
         * @param {(element: T) => boolean} [predicate] A function to test each element for a condition.
         * @return {number} The sum of the projected values, 0 if array is empty or undefined if selector is not a function.
         */
        public static sum<T>(array: T[], selector: (element: T) => number, predicate?: (element: T) => boolean): number {

            var usePredicate: boolean = false;

            if (predicate) {
                usePredicate = ObjectExtensions.isFunction(predicate);
            }

            if (!ObjectExtensions.isFunction(selector)) {
                throw "Selector is not a Function";
            }

            if (!ArrayExtensions.hasElements(array)) {
                return 0;
            }

            return array.reduce<number>((accumulator: number, element: T): number => {
                var elementValue: number;

                if (usePredicate) {
                    elementValue = predicate(element) ? selector(element) : 0;
                } else {
                    elementValue = selector(element);
                }

                return accumulator + elementValue;
            }, 0);
        }

        //#endregion

        // #region Private members

        private static _differenceOrIntersect<T>(left: T[], right: T[], difference: boolean, equalityComparer?: IEqualityComparer<T>): T[] {
            if (ObjectExtensions.isNullOrUndefined(left) || ObjectExtensions.isNullOrUndefined(right)) {
                return null;
            }

            return left.filter((value: T): boolean => {
                var existsInRight = ArrayExtensions.hasElement(right, value, equalityComparer);
                return difference ? !existsInRight : existsInRight;
            });
        }

        private static _getDefaultEqualityComparer<T>(): IEqualityComparer<T> {
            return (left: T, right: T): boolean => left === right;
        }

        private static _getEqualityComparer<T>(equalityComparer?: IEqualityComparer<T>): IEqualityComparer<T> {
            return (equalityComparer) ? equalityComparer : ArrayExtensions._getDefaultEqualityComparer();
        }

        // #endregion
    }
}