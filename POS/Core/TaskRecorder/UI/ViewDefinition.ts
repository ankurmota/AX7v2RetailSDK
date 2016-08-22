/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../TaskRecorderManager.ts" />
/// <reference path="../ITaskRecorderController.ts" />

module Commerce.TaskRecorder {

    export interface IViewModel {
        load?(state?: any): void;
        dispose?(): void;
    }

    export interface IViewModelConstructor {
        new (taskRecorderController: ITaskRecorderController, taskRecorderManager: TaskRecorderManager, options: any);
    }

    export interface IViewDefinition {
        viewUri?: string;
        viewModelType: IViewModelConstructor;
    }

    export interface IViewDefinitionMap {
        [viewName: string]: IViewDefinition;
    }

    export interface IViewViewModel {
        element: HTMLDivElement;
        viewModel: IViewModel;
    }
}
