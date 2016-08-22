/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.TaskRecorder.Configuration {

    export interface ITaskRecorderConfig {
        events: ITaskRecorderEvent[];
    }

    export interface ITaskRecorderEvent {
        eventName: string;
        rules: ITaskRecorderEventRule[];
        custom?: ITaskRecorderCustomEvent;
    }

    export interface ITaskRecorderElementRule {
        keyCode?: number;       // apply this rule for element with this key code only. Uses only if eventName is 'keypress'
        dataAxBubble?: string;  // apply this rule for element with this data-ax-bubble attribute only
        dataAction?: number;    // apply this rule for element with this data-action attribute only
        tagName?: string;       // apply this rule for element with this tag name only
        types?: string[];       // apply this rule for element which has one of this types
        classNames?: string[];  // apply this rule only for element which has one of this classes
        ids?: string[];         // apply this rule only for element with id which contains in this list
        roles?: string[];       // apply this rule only for element with role which contains in this list
        checkParents?: boolean; // try to find element matched this rule from parent's chain
        ignore?: boolean;       // ignore element which matches by rule

        parent?: ITaskRecorderElementRule; // apply this rule only if parent mathed this parent rule
    }

    export interface ITaskRecorderEventRule extends ITaskRecorderElementRule {
        actionData: ITaskRecorderActionData;
        ignore?: boolean;
        handleParent?: ITaskRecorderEventRule; // apply this rule only if parent should be handled as element of event
        handleChild?: ITaskRecorderEventRule; // apply this rule only if child should be handled as element of event
    }

    export interface ITaskRecorderActionData {
        strategies?: string[];
        defaultValue?: string; // value if none of strategy was appropriate
        text: string; // should contains "{0}", which we'll have replaced to value calculated by strategy
        deferred: boolean; // deferred invoke action
        compositionOrder?: number;
    }

    export interface ITaskRecorderEventAction extends ITaskRecorderActionData {
        element: HTMLElement;
    }

    export interface ITaskRecorderCustomEvent {
        startEvents: string[]; // contains names of start events, if we get one of them we will determine that event is started
        endEvents: string[]; // contains names of end events, if we get one of them we will determine that event is ended
        elementEvents?: ITaskRecorderAcceptEvent; // if we get decline event - set runAction to false, if we get accept event - set runAction to true
        runAction?: boolean; // specifies should action be run for this event
        startPhase?: boolean; // true if we got start event but we didn't get end event, false if we got end event
        internalEvents?: ITaskRecorderElementEvent[]; // contains internal events for this custom event
        actions?: ITaskRecorderEventAction[]; // actions for this event
    }

    export interface ITaskRecorderAcceptEvent {
        acceptEvents: string[];
        declineEvents: string[];
    }

    export interface ITaskRecorderElementEvent {
        name: string;
        listener: any;
        element?: HTMLElement;
    }
}