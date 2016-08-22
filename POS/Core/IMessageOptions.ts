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
     * Represents the options that can be used to display a message
     */
    export interface IMessageOptions {
        // General message options
        title?: string;                          // The message box title.
        additionalInfo?: string;                 // The additional information.
        messageType?: MessageType;               // Type of the message to be shown. The default is MessageType.Info.
        messageButtons?: MessageBoxButtons;      // Type of the buttons to be shown. The default is MessageBoxButtons.Default.
        primaryButtonIndex?: number;             // Allows the primary action to be specified.

        // Checkbox related options
        displayMessageCheckbox?: boolean;        // Indicates whether to display the checkbox and the checkbox label. 
                                                 // True to display, false to not display. It is false by default.
        messageCheckboxChecked?: boolean;        // The initial state of the checkbox. True for checked, false for not checked. 
                                                 // It is false be default.
        messageCheckboxLabelResourceID?: string; // The resource ID of the text to display for the label. 
                                                 // By default, it is "Do not tell me again".
    }

    export class MessageOptions implements IMessageOptions {
        // General message options
        title: string;                                                  // The message box title.
        additionalInfo: string;                                         // The additional information.
        messageType: MessageType = MessageType.Info;                    // Type of the message to be shown.
        messageButtons: MessageBoxButtons = MessageBoxButtons.Default;  // Type of the buttons to be shown. 
                                                                        // The default is MessageBoxButtons.Default.
        primaryButtonIndex: number;                                     // Allows the primary action to be specified.

        // Checkbox related options
        displayMessageCheckbox: boolean = false;         // Indicates whether to display the checkbox and the checkbox label. 
                                                         // True to display, false to not display.
        messageCheckboxChecked: boolean = false;         // The initial state of the checkbox. True for checked, false for not checked.
        messageCheckboxLabelResourceID: string;          // The resource ID of the text to display for the label. 
                                                         // By default, it is "Do not tell me again".
    }
}