/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='ErrorHelper.ts'/>

module Commerce {
    "use strict";

    export class SerializationHelpers {
        private static _isLittleEndian: boolean;

        /*
         * Returns whether a system is little endian
         *
         * @return boolean True if the system is little endian, False otherwise
        */
        public static isSystemLittleEndian(): boolean {
            if (ObjectExtensions.isNullOrUndefined(SerializationHelpers._isLittleEndian)) {
                var b: ArrayBuffer = new ArrayBuffer(4);
                var a: Uint32Array = new Uint32Array(b);
                var c: Uint8Array = new Uint8Array(b);
                a[0] = 0xdeadbeef;

                SerializationHelpers._isLittleEndian = (c[0] === 0xef);
            }

            return SerializationHelpers._isLittleEndian;

        }

        /*
         * Encodes a byte array as a Base64 string
         *
         * @param {Uint8Array} byteArray The byte array to encode
         * @return string The byte array as a Base64 string
        */
        public static toBase64String(byteArray: Uint8Array): string {
            if (ObjectExtensions.isNullOrUndefined(byteArray)) {
                return null;
            }

            var len: number = byteArray.byteLength;
            var byteCharacterArray: string[] = [];
            for (var i: number = 0; i < len; i++) {
                byteCharacterArray[i] = String.fromCharCode(byteArray[i]);
            }
            var byteString: string = byteCharacterArray.join("");
            var encodedString: string = window.btoa(byteString);

            return encodedString;
        }

        /*
         * Encodes a byte array as a Base64 string
         *
         * @param {string} base64String The string to decode
         * @return Uint8Array The Base64 string as a byte array
         */
        public static fromBase64String(base64String: string): Uint8Array {
            if (ObjectExtensions.isNullOrUndefined(base64String)) {
                return null;
            }

            var decodedString: string = window.atob(base64String);
            var len: number = decodedString.length;
            var byteArray: Uint8Array = new Uint8Array(len);
            for (var i: number = 0; i < len; i++) {
                byteArray[i] = decodedString[i].charCodeAt(0);
            }

            return byteArray;
        }
    }
}