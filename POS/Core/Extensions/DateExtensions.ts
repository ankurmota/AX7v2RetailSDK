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

    export class DateExtensions {
        /**
         * Sets the date time to be the last hour, minute and seconds of the day.
         * @param {Date} date the date to be updated.
         */
        public static setTimeToLastSecondOfDay(date: Date): void {
            date.setHours(23, 59, 59);
        }

        /**
         * Validate if a date given is today's or future date.
         *
         * @param {Date} date The date to be validated.
         * @returns {boolean} True if date is today or in the future, false otherwise.
         */
        public static isTodayOrFutureDate(date: Date): boolean {
            return DateExtensions.isTodayDate(date) || DateExtensions.isFutureDate(date);
        }

        /**
         * Validate if a date given is future date.
         *
         * @param {Date} date The date to be validated.
         * @returns {boolean} True if date is in the future, false otherwise.
         */
        public static isFutureDate(date: Date): boolean {
            if (ObjectExtensions.isNullOrUndefined(date)) {
                return false;
            }

            var yearFutureDate: number = date.getFullYear();
            var monthFutureDate: number = date.getMonth();
            var dayOfMonthFutureDate: number = date.getDate();

            var now: Date = DateExtensions.now;
            var yearNow: number = now.getFullYear();
            var monthNow: number = now.getMonth();
            var dayOfMonthNow: number = now.getDate();

            if (yearFutureDate > yearNow) {
                return true;
            } else if (yearFutureDate === yearNow && monthFutureDate > monthNow) {
                return true;
            } else if (yearFutureDate === yearNow && monthFutureDate === monthNow && dayOfMonthFutureDate > dayOfMonthNow) {
                return true;
            }

            return false;
        }

        /**
         * Validate if a date given is today's date.
         *
         * @param {Date} date The date to be validated.
         * @returns {boolean} True if date is today, false otherwise.
         */
        public static isTodayDate(date: Date): boolean {
            if (ObjectExtensions.isNullOrUndefined(date)) {
                return false;
            }

            var year: number = date.getFullYear();
            var month: number = date.getMonth();
            var dayOfMonth: number = date.getDate();

            var now: Date = DateExtensions.now;

            return now.getFullYear() === year && now.getMonth() === month && now.getDate() === dayOfMonth;
        }

        /**
         * Removes the time portion of a date time object. Returns only the date portion of it.
         *
         * @param {date} [dateTime] A date time object. If null, then return current date.
         * @return {date} The date portion of the date time object.
         */
        public static getDate(dateTime?: Date): Date {
            if (dateTime == null) {
                dateTime = new Date();
            }

            dateTime.setHours(0, 0, 0, 0);

            return dateTime;
        }

        /**
         * Returns a new Date that adds the specified number of days to the value of this instance.
         *
         * @param {date} dateTime Date to count from. If null then current date is used.
         * @param {number} days A number of days. The value parameter can be negative or positive.
         * @return {date} The date portion of the date time object.
         */
        public static addDays(dateTime: Date, days: number): Date {
            if (ObjectExtensions.isNullOrUndefined(dateTime)) {
                dateTime = DateExtensions.now;
            }

            if (ObjectExtensions.isNullOrUndefined(days) || days === 0) {
                return dateTime;
            }

            var newDate: Date = new Date(dateTime.getTime());
            newDate.setDate(dateTime.getDate() + days);
            return newDate;
        }

        /**
         * Gets a Date that is set to the current date and time on this computer, expressed as the local time.
         *
         * @return {date} A Date object whose value is the current local date and time.
         */
        public static get now(): Date {
            return new Date();
        }

        /**
         * Returns a value indicating whether two Date objects have the same value.
         *
         * @param {date} left The first Date to compare.
         * @param {date} right The second Date to compare.
         * @return {boolean} true if the two values are equal; otherwise, false.
         */
        public static areEqual(left: Date, right: Date): boolean {
            if (ObjectExtensions.isNullOrUndefined(left) || ObjectExtensions.isNullOrUndefined(right)) {
                return false;
            }

            return left.getTime() === right.getTime();
        }

        /**
         * Gets a minimum Date.
         *
         * @return {date} A Date object whose value is 1901-1-1 in the current time zone.
         */
        public static getMinDate(): Date {
            return new Date(1, 1, 1);
        }

        /**
         * Returns a value indicating whether the object is a valid date or not.
         *
         * @param {any} The valid object.
         * @return {boolean} true if the object is a valid date; otherwise, false.
         */
        public static isValidDate(object: any): boolean {
            if (ObjectExtensions.isNullOrUndefined(object)) {
                return false;
            }

            return (Date.parse(object.toString()) !== NaN);
        }
    }
}