/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Core {
    "use strict";

    /**
     * Class for validate a value from regular expressions.
     */
    export class RegularExpressionValidations {

        public static EMAIL_REGEX: string = "(^[a-zA-Z0-9\\.\\-\\_]+@[a-zA-Z0-9\\-\\_]+\\.([a-zA-Z\\-\\_]+\\.)*[a-zA-Z]+$)|(^[\u00C0-\u1FFF\u2C00-\uD7FF\w\\.\\-\\_]+@[a-zA-Z0-9\\-\\_]+\\.([a-zA-Z\\-\\_]+\\.)*[a-zA-Z]+$)";

        public static validateEmail(email: string): boolean {
            var regex: RegExp = new RegExp(RegularExpressionValidations.EMAIL_REGEX);
            return regex.test(email);
        }

        public static validateUrl(url: string): boolean {
            var regex: RegExp = new RegExp("^((http|https|ftp)://)?([^@]+)//.(.+)$");
            return regex.test(url);
        }

        public static validatePhone(phone: string) {
            var regex: RegExp = new RegExp("^\\+?([0-9]|\\(|\\)|\\-|\\ )+$");
            return regex.test(phone);
        }

        public static validateNameFieldForCustomer(value: string): boolean {
            var regex: RegExp = new RegExp("^[A-Za-z]+$");
            return regex.test(value);
        }
    }
}