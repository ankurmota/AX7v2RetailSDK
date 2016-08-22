/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

// Knockout JavaScript library v2.1.0
// (c) Steven Sanderson - http://knockoutjs.com/
// License: MIT (http://www.opensource.org/licenses/mit-license.php)


interface IDisposable {
    dispose: () => void;
}

interface Computed<T> {
    (): T;
    subscribe: (callback: (newValue: T) => void, callerObject?: any) => IDisposable;
}

interface Observable<T> {
    (newValue: T): void;
    (): T;
    subscribe: (callback: (newValue: T) => void , callerObject?: any) => IDisposable;
    valueHasMutated: () => void;
    extend(requestedExtenders: ObservableExtender): Observable<T>;
}

interface ObservableArray<T> {
    (newValue: T[]): void;
    (): T[];
    push: (value: T) => void;
    pop: (value: T) => void;
    remove: (value: T) => void;
    removeAll: () => void;
    splice(start: number, deleteCount: number, ...items: T[]): T[];
    subscribe: (callback: (newValue: T[]) => void , callerObject?: any) => IDisposable;
    valueHasMutated: () => void;
}

interface ObservableExtender {
    notify?: string;
    rateLimit?: number;
}

interface KnockoutUtils {
    unwrapObservable: (any) => any;
    triggerEvent: (target: any, eventName: string) => void;
    registerEventHandler: (target: any, eventName: string, callback: any) => void;
    domNodeDisposal: any;
    arrayForEach: (collection: any[], callback: any) => void;
}

interface Knockout {
    observable<T>(value: T): Observable<T>;
    observableArray<T>(value: T[]): ObservableArray<T>;
    computed<T>(func: () => T, callerObj?: any): Computed<T>;
    utils: KnockoutUtils;
    applyBindings: (data: any, target?: Element) => void;
    applyBindingsToDescendants: (data: any, target?: Element) => void;
    applyBindingsToNode: (element: Element, data: any, viewModel?: any) => void;
    bindingHandlers: any;
    isObservable: (any) => boolean;
    cleanNode: (element: Node) => void;
    removeNode: (element: Node) => void;
}

interface KnockoutBindingContext {
    $parent: any;
    $parents: any[];
    $root: any;
    $data: any;
    $index?: number;
    $parentContext?: KnockoutBindingContext;

    extend(any): any;
    createChildContext(any): any;
}

interface KnockoutBindingHandler {
    init? (element: any, valueAccessor: () => any, allBindingsAccessor: () => any, viewModel: any, bindingContext: KnockoutBindingContext): void;
    update? (element: any, valueAccessor: () => any, allBindingsAccessor: () => any, viewModel: any, bindingContext: KnockoutBindingContext): void;
    options?: any;
}

declare var ko: Knockout;

