/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.TaskRecorder {
    /**
     * Additional methods for Task Recorder.
     */
    export class TaskRecorderUtil {
        /**
         * Generate a random GUID.
         * @returns {string} The generated GUID.
         */
        public static generateGuid(): string {
            function guidPart(): string {
                return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
            }
            return guidPart() + guidPart() + "-" + guidPart() + "-" + guidPart() + "-" + guidPart() + "-" + guidPart() + guidPart() + guidPart();
        }
    }
}