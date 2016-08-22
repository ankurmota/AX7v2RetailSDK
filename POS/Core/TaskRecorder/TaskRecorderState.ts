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
     * Enumerates possible Task Recorder states.
     */
    export enum TaskRecorderState {

        /**
         * The TaskRecorder session had been initialized and in read-only state.
         */
        None,

        /**
         * The Task Recorder session is in recording state.
         */
        Recording,

        /**
         * The Task Recroder session is in recording state, and reording is paused.
         */
        RecordingPaused,

        /**
         * The Task Recorder session is in recording state, and recording has been completed.
         */
        RecordingCompleted
    }
}
