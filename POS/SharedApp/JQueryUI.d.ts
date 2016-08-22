/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

// Type definitions for jQueryUI 1.9

/// <reference path="../SharedApp/JQuery.d.ts"/>


// Widget //////////////////////////////////////////////////

interface Widget {

    // Methods
    destroy();
    disable();
    enable();
    option(optionName: string): any;
    option(): any;
    option(optionName: string, value: any): void;
    option(options: any): void;
    refresh(): void;
    widget(): JQuery;

    // Events
    create?: (event: Event, ui) => void;
}


// Accordion //////////////////////////////////////////////////

interface AccordionOptions {
    active?: any; // boolean or number
    animate?: any; // boolean, number, string or object
    collapsible?: boolean;
    disabled?: boolean;
    event?: string;
    header?: string;
    heightStyle?: string;
    icons?: any;
}

interface AccordionUIParams {
    newHeader: JQuery;
    oldHeader: JQuery;
    newPanel: JQuery;
    oldPanel: JQuery;
}

interface AccordionEvent {
    (event: Event, ui: AccordionUIParams): void;
}

interface AccordionEvents {
    activate?: AccordionEvent;
    beforeActivate?: AccordionEvent;
    create?: AccordionEvent;
}

interface Accordion extends AccordionOptions, AccordionEvents {
}


// Autocomplete //////////////////////////////////////////////////

interface AutocompleteOptions {
    appendTo?: any; //Selector;
    autoFocus?: boolean;
    delay?: number;
    disabled?: boolean;
    minLength?: number;
    position?: string;
    source?: any; // [], string or ()
}

interface AutocompleteUIParams {

}

interface AuotcompleteEvent {
    (event: Event, ui: AutocompleteUIParams): void;
}

interface AutocompleteEvents {
    change?: AuotcompleteEvent;
    close?: AuotcompleteEvent;
    create?: AuotcompleteEvent;
    focus?: AuotcompleteEvent;
    open?: AuotcompleteEvent;
    response?: AuotcompleteEvent;
    search?: AuotcompleteEvent;
    select?: AuotcompleteEvent;
}

interface Autocomplete extends AutocompleteOptions, AutocompleteEvents {
}


// Button //////////////////////////////////////////////////

interface ButtonOptions {
    disabled?: boolean;
    icons?: any;
    label?: string;
    text?: boolean;
}

interface Button extends ButtonOptions {
}


// Datepicker //////////////////////////////////////////////////

interface DatepickerOptions {
    altFieldType?: any; // Selecotr, jQuery or Element
    altFormat?: string;
    appendText?: string;
    autoSize?: boolean;
    beforeShow?: (input: Element, inst: any) => void;
    beforeShowDay?: (date: Date) => void;
    buttonImage?: string;
    buttonImageOnly?: boolean;
    buttonText?: string;
    calculateWeek?: () => any;
    changeMonth?: boolean;
    changeYear?: boolean;
    closeText?: string;
    constrainInput?: boolean;
    currentText?: string;
    dateFormat?: string;
    dayNames?: string[];
    dayNamesMin?: string[];
    dayNamesShort?: string[];
    defaultDateType?: any; // Date, number or string
    duration?: string;
    firstDay?: number;
    gotoCurrent?: boolean;
    hideIfNoPrevNext?: boolean;
    isRTL?: boolean;
    maxDate?: any; // Date, number or string
    minDate?: any; // Date, number or string
    monthNames?: string[];
    monthNamesShort?: string[];
    navigationAsDateFormat?: boolean;
    nextText?: string;
    numberOfMonths?: any; // number or []
    onChangeMonthYear?: (year: number, month: number, inst: any) => void;
    onClose?: (dateText: string, inst: any) => void;
    onSelect?: (dateText: string, inst: any) => void;
    prevText?: string;
    selectOtherMonths?: boolean;
    shortYearCutoff?: any; // number or string
    showAnim?: string;
    showButtonPanel?: boolean;
    showCurrentAtPos?: number;
    showMonthAfterYear?: boolean;
    showOn?: string;
    showOptions?: any; // TODO
    showOtherMonths?: boolean;
    showWeek?: boolean;
    stepMonths?: number;
    weekHeader?: string;
    yearRange?: string;
    yearSuffix?: string;
}

interface Datepicker extends Widget, DatepickerOptions {
}


// Dialog //////////////////////////////////////////////////

interface DialogOptions {
    autoOpen?: boolean;
    buttons?: any; // object or []
    closeOnEscape?: boolean;
    closeText?: string;
    dialogClass?: string;
    disabled?: boolean;
    draggable?: boolean;
    height?: any; // number or string
    maxHeight?: number;
    maxWidth?: number;
    minHeight?: number;
    minWidth?: number;
    modal?: boolean;
    position?: any; // object, string or []
    resizable?: boolean;
    show?: any; // number, string or object
    stack?: boolean;
    title?: string;
    width?: number;
    zIndex?: number;
}

interface DialogUIParams {
}

interface DialogEvent {
    (event: Event, ui: DialogUIParams): void;
}

interface DialogEvents {
    beforeClose?: DialogEvent;
    close?: DialogEvent;
    create?: DialogEvent;
    drag?: DialogEvent;
    dragStart?: DialogEvent;
    dragStop?: DialogEvent;
    focus?: DialogEvent;
    open?: DialogEvent;
    resize?: DialogEvent;
    resizeStart?: DialogEvent;
    resizeStop?: DialogEvent;
}

interface Dialog extends DialogOptions, DialogEvents {


}


// Draggable //////////////////////////////////////////////////

interface DraggableEventUIParams {
    helper: JQuery;
    position: { top: number; left: number; };
    offset: { top: number; left: number; };
}

interface DraggableEvent {
    (event: Event, ui: DraggableEventUIParams): void;
}

interface DraggableOptions {
    disabled?: boolean;
    addClasses?: boolean;
    appendTo?: any;
    axis?: string;
    cancel?: string;
    connectToSortable?: string;
    containment?: any;
    cursor?: string;
    cursorAt?: any;
    delay?: number;
    distance?: number;
    grid?: number[];
    handle?: any;
    helper?: any;
    iframeFix?: any;
    opacity?: number;
    refreshPositions?: boolean;
    revert?: any;
    revertDuration?: number;
    scope?: string;
    scroll?: boolean;
    scrollSensitivity?: number;
    scrollSpeed?: number;
    snap?: any;
    snapMode?: string;
    snapTolerance?: number;
    stack?: string;
    zIndex?: number;
}

interface DraggableEvents {
    create?: DraggableEvent;
    start?: DraggableEvent;
    drag?: DraggableEvent;
    stop?: DraggableEvent;
}

interface Draggable extends Widget, DraggableOptions, DraggableEvent {
}


// Droppable //////////////////////////////////////////////////

interface DroppableEventUIParam {
    draggable: JQuery;
    helper: JQuery;
    position: { top: number; left: number; };
    offset: { top: number; left: number; };
}

interface DroppableEvent {
    (event: Event, ui: DroppableEventUIParam): void;
}

interface DroppableOptions {
    disabled?: boolean;
    accept?: any;
    activeClass?: string;
    greedy?: boolean;
    hoverClass?: string;
    scope?: string;
    tolerance?: string;
}

interface DroppableEvents {
    create?: DroppableEvent;
    activate?: DroppableEvent;
    deactivate?: DroppableEvent;
    over?: DroppableEvent;
    out?: DroppableEvent;
    drop?: DroppableEvent;
}

interface Droppable extends DroppableOptions, DroppableEvents {
}

// TODO ?
interface JQueryDatePickerDefaults {
    closeText: string;
    prevText: string;
    nextText: string;
    currentText: string;
    monthNames: string[];
    monthNamesShort: string[];
    dayNames: string[];
    dayNamesShort: string[];
    dayNamesMin: string[];
    weekHeader: string;
    dateFormat: string;
    firstDay: number;
    isRTL: boolean;
    showMonthAfterYear: boolean;
    yearSuffix: string;
}

interface JQueryDatePicker {
    regional: any;
    setDefaults(JQueryDatePickerDefaults);
}


// Menu //////////////////////////////////////////////////

interface MenuOptions {
    disabled?: boolean;
    icons?: any;
    menus?: string;
    position?: any; // TODO
    role?: string;
}

interface MenuUIParams {
}

interface MenuEvent {
    (event: Event, ui: MenuUIParams): void;
}

interface MenuEvents {
    blur?: MenuEvent;
    create?: MenuEvent;
    focus?: MenuEvent;
    select?: MenuEvent;
}

interface Menu extends MenuOptions, MenuEvents {
}


// Progressbar //////////////////////////////////////////////////

interface ProgressbarOptions {
    disabled?: boolean;
    //value?: number;
}

interface ProgressbarUIParams {
}

interface ProgressbarEvent {
    (event: Event, ui: ProgressbarUIParams): void;
}

interface ProgressbarEvents {
    change?: ProgressbarEvent;
    complete?: ProgressbarEvent;
    create?: ProgressbarEvent;
}

interface Progressbar extends ProgressbarOptions, ProgressbarEvents {
}


// Resizable //////////////////////////////////////////////////

interface ResizableOptions {
    alsoResize?: any; // Selector, JQuery or Element
    animate?: boolean;
    animateDuration?: any; // number or string
    animateEasing?: string;
    aspectRatio?: any; // boolean or number
    autoHide?: boolean;
    cancel?: string;
    containment?: any; // Selector, Element or string
    delay?: number;
    disabled?: boolean;
    distance?: number;
    ghost?: boolean;
    grid?: any;
    handles?: any; // string or object
    helper?: string;
    maxHeight?: number;
    maxWidth?: number;
    minHeight?: number;
    minWidth?: number;
}

interface ResizableUIParams {
    element: JQuery;
    helper: JQuery;
    originalElement: JQuery;
    originalPosition: any;
    originalSize: any;
    position: any;
    size: any;
}

interface ResizableEvent {
    (event: Event, ui: ResizableUIParams): void;
}

interface ResizableEvents {
    resize?: ResizableEvent;
    start?: ResizableEvent;
    stop?: ResizableEvent;
}

interface Resizable extends Widget, ResizableOptions, ResizableEvents {
}


// Selectable //////////////////////////////////////////////////

interface SelectableOptions {
    autoRefresh?: boolean;
    cancel?: string;
    delay?: number;
    disabled?: boolean;
    distance?: number;
    filter?: string;
    tolerance?: string;
}

interface SelectableEvents {
    selected? (event: Event, ui: { selected?: Element; }): void;
    selecting? (event: Event, ui: { selecting?: Element; }): void;
    start? (event: Event, ui: any): void;
    stop? (event: Event, ui: any): void;
    unselected? (event: Event, ui: { unselected: Element; }): void;
    unselecting? (event: Event, ui: { unselecting: Element; }): void;
}

interface Selectable extends Widget, SelectableOptions, SelectableEvents {
}

// Slider //////////////////////////////////////////////////

interface SliderOptions {
    animate?: any; // boolean, string or number
    disabled?: boolean;
    max?: number;
    min?: number;
    orientation?: string;
    range?: any; // boolean or string
    step?: number;
    // value?: number;
    // values?: number[];
}

interface SliderUIParams {
}

interface SliderEvent {
    (event: Event, ui: SliderUIParams): void;
}

interface SliderEvents {
    change?: SliderEvent;
    create?: SliderEvent;
    slide?: SliderEvent;
    start?: SliderEvent;
    stop?: SliderEvent;
}

interface Slider extends SliderOptions, SliderEvents {
}


// Sortable //////////////////////////////////////////////////

interface SortableOptions {
    appendTo?: any; // jQuery, Element, Selector or string
    axis?: string;
    cancel?: string;
    connectWith?: string;
    containment?: any; // Element, Selector or string
    cursor?: string;
    cursorAt?: any;
    delay?: number;
    disabled?: boolean;
    distance?: number;
    dropOnEmpty?: boolean;
    forceHelperSize?: boolean;
    forcePlaceholderSize?: boolean;
    grid?: number[];
    handle?: any; // Selector or Element
    items?: any; // Selector
    opacity?: number;
    placeholder?: string;
    revert?: any; // boolean or number
    scroll?: boolean;
    scrollSensitivity?: number;
    scrollSpeed?: number;
    tolerance?: string;
    zIndex?: number;
}

interface SortableUIParams {
    helper: JQuery;
    item: JQuery;
    offset: any;
    position: any;
    originalPosition: any;
    sender: JQuery;
}

interface SortableEvent {
    (event: Event, ui: SortableUIParams): void;
}

interface SortableEvents {
    activate?: SortableEvent;
    beforeStop?: SortableEvent;
    change?: SortableEvent;
    deactivate?: SortableEvent;
    out?: SortableEvent;
    over?: SortableEvent;
    receive?: SortableEvent;
    remove?: SortableEvent;
    sort?: SortableEvent;
    start?: SortableEvent;
    stop?: SortableEvent;
    update?: SortableEvent;
}

interface Sortable extends Widget, SortableOptions, SortableEvents {
}


// Spinner //////////////////////////////////////////////////

interface SpinnerOptions {
    culture?: string;
    disabled?: boolean;
    icons?: any;
    incremental?: any; // boolean or ()
    max?: any; // number or string
    min?: any; // number or string
    numberFormat?: string;
    page?: number;
    step?: any; // number or string
}

interface SpinnerUIParams {
}

interface SpinnerEvent {
    (event: Event, ui: SpinnerUIParams): void;
}

interface SpinnerEvents {
    spin?: SpinnerEvent;
    start?: SpinnerEvent;
    stop?: SpinnerEvent;
}

interface Spinner extends Widget, SpinnerOptions, SpinnerEvents {
}


// Tabs //////////////////////////////////////////////////

interface TabsOptions {
    active?: any; // boolean or number
    collapsible?: boolean;
    disabled?: any; // boolean or []
    event?: string;
    heightStyle?: string;
    hide?: any; // boolean, number, string or object
    show?: any; // boolean, number, string or object
}

interface TabsUIParams {
}

interface TabsEvent {
    (event: Event, ui: TabsUIParams): void;
}

interface TabsEvents {
    activate?: TabsEvent;
    beforeActivate?: TabsEvent;
    beforeLoad?: TabsEvent;
    load?: TabsEvent;
}

interface Tabs extends Widget, TabsOptions, TabsEvents {
}


// Tooltip //////////////////////////////////////////////////

interface TooltipOptions {
    content?: any; // () or string
    disabled?: boolean;
    hide?: any; // boolean, number, string or object
    items?: string;
    position?: any; // TODO
    show?: any; // boolean, number, string or object
    tooltipClass?: string;
    track?: boolean;
}

interface TooltipUIParams {
}

interface TooltipEvent {
    (event: Event, ui: TooltipUIParams): void;
}

interface TooltipEvents {
    close?: TooltipEvent;
    open?: TooltipEvent;
}

interface Tooltip extends Widget, TooltipOptions, TooltipEvents {
}


////////////////////////////////////////////////////////////////////////////////////////////////////

interface JQuery {

    accordion(): JQuery;
    accordion(methodName: string): JQuery;
    accordion(options: AccordionOptions): JQuery;
    accordion(optionLiteral: string, optionName: string): any;
    accordion(optionLiteral: string, options: AccordionOptions): any;
    accordion(optionLiteral: string, optionName: string, optionValue: any): JQuery;


    autocomplete(): JQuery;
    autocomplete(methodName: string): JQuery;
    autocomplete(options: AutocompleteOptions): JQuery;
    autocomplete(optionLiteral: string, optionName: string): any;
    autocomplete(optionLiteral: string, options: AutocompleteOptions): any;
    autocomplete(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    button(): JQuery;
    button(methodName: string): JQuery;
    button(options: ButtonOptions): JQuery;
    button(optionLiteral: string, optionName: string): any;
    button(optionLiteral: string, options: ButtonOptions): any;
    button(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    buttonset(): JQuery;
    buttonset(methodName: string): JQuery;
    buttonset(options: ButtonOptions): JQuery;
    buttonset(optionLiteral: string, optionName: string): any;
    buttonset(optionLiteral: string, options: ButtonOptions): any;
    buttonset(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    datepicker(): JQuery;
    datepicker(methodName: string): JQuery;
    datepicker(options: DatepickerOptions): JQuery;
    datepicker(optionLiteral: string, optionName: string): any;
    datepicker(optionLiteral: string, options: DatepickerOptions): any;
    datepicker(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    dialog(): JQuery;
    dialog(methodName: string): JQuery;
    dialog(options: DialogOptions): JQuery;
    dialog(optionLiteral: string, optionName: string): any;
    dialog(optionLiteral: string, options: DialogOptions): any;
    dialog(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    draggable(): JQuery;
    draggable(methodName: string): JQuery;
    draggable(options: DraggableOptions): JQuery;
    draggable(optionLiteral: string, optionName: string): any;
    draggable(optionLiteral: string, options: DraggableOptions): any;
    draggable(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    droppable(): JQuery;
    droppable(methodName: string): JQuery;
    droppable(options: DroppableOptions): JQuery;
    droppable(optionLiteral: string, optionName: string): any;
    droppable(optionLiteral: string, options: DraggableOptions): any;
    droppable(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    menu(): JQuery;
    menu(methodName: string): JQuery;
    menu(options: MenuOptions): JQuery;
    menu(optionLiteral: string, optionName: string): any;
    menu(optionLiteral: string, options: MenuOptions): any;
    menu(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    progressbar(): JQuery;
    progressbar(methodName: string): JQuery;
    progressbar(options: Progressbar): JQuery;
    progressbar(optionLiteral: string, optionName: string): any;
    progressbar(optionLiteral: string, options: Progressbar): any;
    progressbar(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    resizable(): JQuery;
    resizable(methodName: string): JQuery;
    resizable(options: ResizableOptions): JQuery;
    resizable(optionLiteral: string, optionName: string): any;
    resizable(optionLiteral: string, options: ResizableOptions): any;
    resizable(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    selectable(): JQuery;
    selectable(methodName: string): JQuery;
    selectable(options: SelectableOptions): JQuery;
    selectable(optionLiteral: string, optionName: string): any;
    selectable(optionLiteral: string, options: SelectableOptions): any;
    selectable(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    slider(): JQuery;
    slider(methodName: string): JQuery;
    slider(options: SliderOptions): JQuery;
    slider(optionLiteral: string, optionName: string): any;
    slider(optionLiteral: string, options: SliderOptions): any;
    slider(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    sortable(): JQuery;
    sortable(methodName: string): JQuery;
    sortable(options: SortableOptions): JQuery;
    sortable(optionLiteral: string, optionName: string): any;
    sortable(optionLiteral: string, options: SortableOptions): any;
    sortable(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    disableSelection();

    spinner(): JQuery;
    spinner(methodName: string): JQuery;
    spinner(options: SpinnerOptions): JQuery;
    spinner(optionLiteral: string, optionName: string): any;
    spinner(optionLiteral: string, options: SpinnerOptions): any;
    spinner(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    tabs(): JQuery;
    tabs(methodName: string): JQuery;
    tabs(options: TabsOptions): JQuery;
    tabs(optionLiteral: string, optionName: string): any;
    tabs(optionLiteral: string, options: TabsOptions): any;
    tabs(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    tooltip(): JQuery;
    tooltip(methodName: string): JQuery;
    tooltip(options: TooltipOptions): JQuery;
    tooltip(optionLiteral: string, optionName: string): any;
    tooltip(optionLiteral: string, options: TooltipOptions): any;
    tooltip(optionLiteral: string, optionName: string, optionValue: any): JQuery;

    show(selectedEffect?: any, options?: any, duration?: string, callback?: any): JQuery;
    hide(selectedEffect?: any, options?: any, duration?: string, callback?: any): JQuery;

}

interface MouseOptions {
    cancel?: string;
    delay?: number;
    distance?: number;
}

interface KeyCode {
    BACKSPACE: number;
    COMMA: number;
    DELETE: number;
    DOWN: number;
    END: number;
    ENTER: number;
    ESCAPE: number;
    HOME: number;
    LEFT: number;
    NUMPAD_ADD: number;
    NUMPAD_DECIMAL: number;
    NUMPAD_DIVIDE: number;
    NUMPAD_ENTER: number;
    NUMPAD_MULTIPLY: number;
    NUMPAD_SUBTRACT: number;
    PAGE_DOWN: number;
    PAGE_UP: number;
    PERIOD: number;
    RIGHT: number;
    SPACE: number;
    TAB: number;
    UP: number;
}

interface UI {
    mouse(method: string): JQuery;
    mouse(options: MouseOptions): JQuery;
    mouse(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    mouse(optionLiteral: string, optionValue: any): any;

    accordion: Accordion;
    autocomplete: Autocomplete;
    button: Button;
    buttonset: Button;
    datepicker: Datepicker;
    dialog: Dialog;
    keyCode: KeyCode;
    menu: Menu;
    progressbar: Progressbar;
    slider: Slider;
    spinner: Spinner;
    tabs: Tabs;
    tooltip: Tooltip;
    version: string;
}

interface JQueryStatic {
    datepicker: JQueryDatePicker;
    ui: UI;
}