/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Requests {
    "use strict";

    /**
     *  Base class for OData requests.
     */
    export class ODataRequestBase implements Common.IDataServiceRequest {
        private static ODATA_TOTAL_COUNT_PROPERTY: string = "@odata.count";
        private static ODATA_HAS_NEXT_PAGE_PROPERTY: string = "@odata.nextLink";

        private static ODATA_METADATA_EDM_PROPERTY_NAME: string = "__edmType";
        private static ODATA_METADATA_OFFSET_PROPERTY_NAME: string = "__offset";

        private static HTTP_ACCEPT_LANGUAGE_HEADER_NAME: string = "Accept-Language";
        private static BATCH_ENDPOINT_POSTFIX: string = "/$batch";

        private static HEXDECIMAL_RADIX: number = 16;

        // Accoding to ODATA V4 URI syntax rule based on RFC3986, the following characters should also be encoded in request URI.
        private static SINGLE_QUOTE_REGEX: RegExp = /\'/g;
        private static ADDITIONAL_RFC3986_RESERVED_CHAR_REGEX: RegExp = /[!()*]/g;

        protected query: Common.IDataServiceQueryInternal;
        protected enableCrossDomainCookies: boolean;
        private _locale: string;
        private _id: number;
        private _requestUri: string;
        private method: string;
        private hasOperationName: boolean;
        private serverUri: string;

        /**
         * Initializes a new instance of the ODataRequestBase class.
         * @param {string} serverUri the server uri.
         * @param {IDataServiceQueryInternal} the protocol agnostic query.
         * @param {string} locale the locale to be set for the request.
         */
        constructor(serverUri: string, query: Common.IDataServiceQueryInternal, locale: string = "") {
            this.serverUri = serverUri;
            this.query = query;
            this.hasOperationName = false;
            this.enableCrossDomainCookies = true;
            this._locale = locale;

            switch (query.action) {
                case "Read":
                case "ReadAll":
                    this.method = Common.HttpVerbs.GET;
                    break;

                case "Create":
                    this.method = Common.HttpVerbs.POST;
                    break;

                case "Update":
                    this.method = Common.HttpVerbs.PATCH;
                    break;

                case "Delete":
                    this.method = Common.HttpVerbs.DELETE;
                    break;

                default:
                    if (query.isAction) {
                        // Sets the HTTP method to be POST for actions
                        this.method = Common.HttpVerbs.POST;
                    } else {
                        // Sets the HTTP method to be GET for functions
                        this.method = Common.HttpVerbs.GET;
                    }

                    this.hasOperationName = true;
                    break;
            }

            this._requestUri = this.getRequestUri();
        }

        /**
         * Gets the locale for current request.
         */
        public get locale(): string {
            return this._locale;
        }

        /**
         * Gets the locale for current request.
         */
        protected get requestUri(): string {
            return this._requestUri;
        }

        /**
         * Parses the odata result returned from server.
         * @param {any} data The result returned from server.
         * @param {any} resultType The entity type to parse the result as.
         */
        public static parseODataResult(data: any, resultType: any): any {
            var result: any = data;

            if (!ObjectExtensions.isNullOrUndefined(data)) {

                // If the return result is an OData PageResult object
                // data.value is from OData, data.Results is from CRT in offline mode
                var resultSet: any = ObjectExtensions.isNullOrUndefined(data.value) ? data.Results : data.value;
                if (!ObjectExtensions.isNullOrUndefined(resultSet)) {

                    // No need to process further if object type is one of the primitives (number, string, or boolean)
                    if (ObjectExtensions.isNumber(resultSet) || ObjectExtensions.isString(resultSet)
                        || ObjectExtensions.isBoolean(resultSet)) {
                        return resultSet;
                    }

                    var totalCount: number = data[ODataRequestBase.ODATA_TOTAL_COUNT_PROPERTY] || data.TotalCount;

                    var hasNextPage: boolean = data[ODataRequestBase.ODATA_HAS_NEXT_PAGE_PROPERTY] || data.HasNextPage;
                    if (ObjectExtensions.isUndefined(hasNextPage)) {
                        // For now we only tell if there are more pages rather than giving the exact next page link.
                        hasNextPage = false;
                    }


                    if (resultType) {
                        if (Object.prototype.toString.call(resultSet) === "[object Array]") {
                            for (var i: number = 0; i < resultSet.length; i++) {
                                resultSet[i] = new resultType(resultSet[i]);
                            }
                        }
                    }

                    resultSet.totalCount = totalCount;
                    resultSet.hasNextPage = hasNextPage;
                    result = resultSet;
                } else {
                    if (resultType) {
                        result = new resultType(result);
                    }
                }
            }

            return result;
        }

        /**
         * Gets the Request Identifier.
         * Auto-generated unique identifier if executed in batch. Used to fetch the response from batch result array.
         */
        public id(): number {
            return this._id;
        }

        /**
         * Execute the request.
         * @return {IAsyncResult<T>} The async result.
         */
        public execute<T>(): IAsyncResult<T> {
            var requestHeaders: { [headerName: string]: string };
            var result: T;

            return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                return this.getRequestHeaders().done((headers: { [headerName: string]: string }): void => {
                    requestHeaders = headers;
                });
            }).enqueue((): IVoidAsyncResult => {
                var asyncResult: AsyncResult<T> = new AsyncResult<T>();

                var request: ODataRequest = {
                    requestUri: this.getRequestUri(),
                    method: this.method,
                    data: this.toJson(this.query.data),
                    headers: requestHeaders,
                    useCrossDomainCookies: this.enableCrossDomainCookies
                };

                // This is the pipe for all OData API calls.
                OData.request(request,
                    (response: Common.IXmlHttpRequest) => {
                        this.logRequestResponseAsync(RetailLogger.modelManagersRetailServerOdataRequestResponse, request, response);
                        asyncResult.resolve(ODataRequestBase.parseODataResult(response, this.query.returnType));
                    },
                    (errorResult: Common.IXmlHttpError) => {
                        this.logRequestResponseAsync(RetailLogger.modelManagersRetailServerOdataRequestErrorResponse, request, errorResult);
                        asyncResult.reject(this.parseErrorResponse(errorResult));
                    },
                    OData.jsonHandler);

                return asyncResult.done((response: T): void => {
                    result = response;
                });
            }).run().map((): T => {
                return result;
            });
        }

        /**
         * Executes the batch requests.
         * @param {IDataServiceRequest[]} requests The collection of requests to execute.
         * @return {IAsyncResult<any[]>} The async result. Responses at index I correlates to request with identifier I.
         */
        public executeBatch(requests: ODataRequestBase[]): IAsyncResult<any[]> {
            var batchRequestUri: string = this.serverUri + ODataRequestBase.BATCH_ENDPOINT_POSTFIX;
            var batchRequest: ODataRequest;
            var result: any[] = null;

            return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                return this.buildBatchRequest(batchRequestUri, requests).done((_batchRequest: ODataRequest) => {
                    batchRequest = _batchRequest;
                });
            }).enqueue(() => {
                var asyncResult: AsyncResult<any[]> = new AsyncResult<any[]>();

                OData.request(batchRequest,
                    (data: any) => {
                        var responses: any[] = new Array();
                        var errors: Model.Entities.Error[] = this.parseBatchResponses(data.__batchResponses, requests, responses);

                        if (errors.length === 0) {
                            RetailLogger.modelManagersODataExecuteBatchSuccess(batchRequestUri);
                            asyncResult.resolve(responses);
                        } else {
                            RetailLogger.modelManagersODataExecuteBatchFailed(batchRequestUri, errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                            asyncResult.reject(errors);
                        }
                    },
                    (error: Common.IXmlHttpError) => {
                        RetailLogger.modelManagersODataExecuteBatchFailed(batchRequestUri, null, null);
                        asyncResult.reject(this.parseErrorResponse(error));
                    },
                    OData.batchHandler);

                return asyncResult.done((_result: any[]) => {
                    result = _result;
                });
            }).run().map((): any[] => {
                return result;
            });
        }

        /**
         * Parses the error result.
         * @param {Common.IXmlHttpError} errorResponse the error response.
         * @param {Model.Entities.Error[]} the errors from the response.
         */
        protected parseErrorResponse(errorResponse: Common.IXmlHttpError): Model.Entities.Error[] {
            return Context.ErrorParser.parseErrorMessage(errorResponse.response);
        }

        /**
         * Gets the headers to be added to the request.
         * @returns {IAsyncResult<{ [headerName: string]: string }>} a promise for an object whose properties represent header values.
         */
        protected getRequestHeaders(): IAsyncResult<{ [headerName: string]: string }> {

            var headers: { [headerName: string]: string } = {};

            // add any headers comming from query
            var headerNames: string[] = Object.keys(this.query.headers);
            for (var i: number = 0; i < headerNames.length; i++) {
                var headerName: string = headerNames[i];
                var headerValue: string = this.query.headers[headerName];

                if (!StringExtensions.isNullOrWhitespace(headerValue)) {
                    headers[headerName] = headerValue;
                }
            }

            // Add accept-language header to send user preferred locale to server.
            if (!StringExtensions.isNullOrWhitespace(this._locale)) {
                headers[ODataRequestBase.HTTP_ACCEPT_LANGUAGE_HEADER_NAME] = this._locale;
            }

            return AsyncResult.createResolved(headers);
        }

        /**
         * Adds a parameter to the requestUri and returns a new requestUri with the new parameter added.
         * @param {string} requestUri the current request uri.
         * @param {string} key the parameter name.
         * @param {string} vakye the parameter value.
         */
        protected addParameter(requestUri: string, key: string, value: any): string {
            if (value) {
                var parameterSeparator: string = "?";

                if (requestUri.indexOf(parameterSeparator) > 0) {
                    parameterSeparator = "&";
                }

                return requestUri + parameterSeparator + key + "=" + value;
            } else {
                return requestUri;
            }
        }

        /**
         * Gets the Request Uri for this request.
         * @returns {string} the request uri.
         */
        protected getRequestUri(): string {
            var requestUri: string = this.serverUri + (this.query.entitySet
                ? "/" + this.query.entitySet + (this.query.key
                    ? "(" + this.formatKey(this.query.key) + ")"
                    : StringExtensions.EMPTY)
                : StringExtensions.EMPTY);

            // Appends the action (POST) or function (GET) name to the Uri
            if (this.hasOperationName) {
                requestUri += "/" + this.query.action;

                // Construct OData function Url with parameters. [e.g. Func(), Func(param1='',param2='')]
                if (!this.query.isAction && this.query.data instanceof Common.ODataOperationParameters) {
                    requestUri += "(" + this.formatFunctionParameter(this.query.data.parameters) + ")";
                }
            }

            if (!StringExtensions.isNullOrWhitespace(this.query.filterSettings)) {
                requestUri = this.addParameter(requestUri, "$filter", this.query.filterSettings);
            }

            // Check for resultSettings
            if (!ObjectExtensions.isNullOrUndefined(this.query.resultSettings)) {
                // Check for Paging
                if (!ObjectExtensions.isNullOrUndefined(this.query.resultSettings.Paging)) {
                    var paging: Entities.PagingInfo = this.query.resultSettings.Paging;

                    // Check for TOP
                    if (!ObjectExtensions.isNullOrUndefined(paging.Top)) {
                        requestUri = this.addParameter(requestUri, "$top", paging.Top);
                    }

                    // Check for SKIP
                    if (!ObjectExtensions.isNullOrUndefined(paging.Skip)) {
                        requestUri = this.addParameter(requestUri, "$skip", paging.Skip);
                    }
                }

                // Check for Sorting
                if (!ObjectExtensions.isNullOrUndefined(this.query.resultSettings.Sorting)
                    && ArrayExtensions.hasElements(this.query.resultSettings.Sorting.Columns)) {
                    this.query.resultSettings.Sorting.Columns.forEach((column: Entities.SortColumn) => {
                        requestUri = this.addParameter(requestUri, "$orderby", column.ColumnName);
                    });
                }
            }

            if (this.query.inlineCount) {
                requestUri = this.addParameter(requestUri, "$inlinecount", "allpages");
            }

            return requestUri;
        }

        private buildBatchRequest(batchRequestUri: string, requests: ODataRequestBase[]): IAsyncResult<ODataRequest> {
            var requestId: number = 1;
            var requestHeaders: { [headerName: string]: string };
            var odataBatchRequest: ODataRequest;

            return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                return this.getRequestHeaders().done((headers: { [headerName: string]: string }): void => {
                    requestHeaders = headers;
                });
            }).enqueue((): IVoidAsyncResult => {
                // Creates an empty batch request.
                odataBatchRequest = {
                    requestUri: batchRequestUri,
                    method: Common.HttpVerbs.POST,
                    data: { __batchRequests: [] },
                    headers: requestHeaders
                };

                var batchRequests: any[] = odataBatchRequest.data.__batchRequests;
                var changeRequests: ODataRequest[];

                requests.forEach((request: ODataRequestBase) => {
                    request._id = requestId++;

                    var odataRequest: ODataRequest = {
                        requestUri: request.requestUri,
                        method: request.method,
                        data: this.toJson(request.query.data),
                        headers: { "Content-ID": request.id(), "Prefer": "return=representation" }
                    };

                    if (request.method === Common.HttpVerbs.GET) {
                        batchRequests.push(odataRequest);
                    } else {
                        if (!changeRequests) {
                            changeRequests = [];
                            batchRequests.push({ __changeRequests: changeRequests });
                        }
                        changeRequests.push(odataRequest);
                    }
                });

                return VoidAsyncResult.createResolved();
            }).run().map((): ODataRequest => {
                return odataBatchRequest;
            });
        }

        /**
         * Parse the batch responses.
         * @param {any[]} batchResponses The server responses.
         * @param {ODataRequestBase[]} requests The request objects.
         * @param {any[]} responses The responses passed down to the success callback.
         */
        private parseBatchResponses(batchResponses: any[], requests: ODataRequestBase[], responses: any[]): Model.Entities.Error[] {
            var errors: Model.Entities.Error[] = new Array();

            batchResponses.forEach((response: any) => {
                if (response.__changeResponses) {
                    errors = errors.concat(this.parseBatchResponses(response.__changeResponses, requests, responses));
                } else {
                    if (ObjectExtensions.isNullOrUndefined(response.message)) {
                        var requestId: number = parseInt(response.headers["Content-ID"], 10);

                        responses[requestId] = ODataRequestBase.parseODataResult(response.data, requests[requestId - 1].query.returnType);
                    } else {
                        errors = errors.concat(this.parseErrorResponse(response.response));
                    }
                }
            });

            return errors;
        }

        /**
         * Helper function to convert data into a stringified JSON object.
         * @param {any} parent The container of the element.
         * @param {any} data The data to be converted to a stringified JSON object.
         * @param {boolean} [modifyDataInPlace] Whether the object passed should be modified in place. If not, a clone is created instead.
         * The default value is false.
         * @return {any} The stringified JSON object.
         */
        private toJson(data: any, parent?: any, modifyDataInPlace: boolean = false): any {
            var clone: Object = data;

            if (data === null) {
                return null;
            } else if (typeof data === "undefined") {
                return data;
            } else if (typeof (data) !== "object") {
                // if data type is primitive type, no need to process further
                // as calling jquery extend or calling foreach prop on string type
                // will transform string to array of chars which will be incorrect result.
                return data;
            }

            if (data instanceof Common.ODataOperationParameters) {
                data = data.parameters;
                parent = null;
            } else {
                parent = data;
            }

            // clones the data object instead of changing it, if no modifications in place
            if (!modifyDataInPlace) {
                clone = <any>$.extend({}, data);
            }

            var originalKeys: string[] = Object.keys(data);

            Object.keys(clone).forEach((propertyName: string) => {
                if (originalKeys.indexOf(propertyName) === -1) {
                    delete clone[propertyName];
                    return;
                }

                var property: any = clone[propertyName];
                var isUndefined: boolean = ObjectExtensions.isUndefined(property);

                // only non-undefined properties are added to the object
                if (!isUndefined) {
                    var value: any;

                    var isNull: boolean = ObjectExtensions.isNull(property);
                    var isObject: boolean = ObjectExtensions.isObject(property);

                    if (isNull) {
                        value = null;
                    } else if (isObject) {
                        if (property instanceof Array) {
                            value = this.arrayToJson(<any[]>property, parent, modifyDataInPlace);
                        } else if (property instanceof Date) {
                            value = this.toDateTimeOffset(property);
                        } else {
                            // recursively stringifies objects
                            value = this.toJson(property, clone, modifyDataInPlace);
                        }
                    } else {
                        value = property;
                    }

                    clone[propertyName] = value;
                }
            });

            return clone;
        }

        /**
         * Helper function to convert array into a stringified JSON object following OData specifications.
         * @param {Array} array The data to be converted to a stringified JSON object.
         * @param {any} parent The container of the array.
         * @param {boolean} [modifyDataInPlace] Whether the object passed should be modified in place. If not, a clone is created instead.
         * The default value is false.
         * @return {string[]} An array of JSON strings.
         */
        private arrayToJson(array: any[], parent: any, modifyDataInPlace: boolean = false): string[] {
            var data: string[] = [];
            for (var i: number = 0; i < array.length; i++) {
                data[i] = this.toJson(array[i], array, modifyDataInPlace);
            }

            // if array is not a top level data on an action
            return data;
        }

        /**
         * Logs the request and response into a single json string to the specified logMethod if in debug mode.
         * @param {(val: string) => void} logMethod The log method to log the request and response.
         * @param {ODataRequest} request The request.
         * @param {any} response The response.
         */
        private logRequestResponseAsync(logMethod: (clientRequest: string, serverResponse: string) => void, request: ODataRequest, response: any): void {
            if (Commerce.Config.isDebugMode) {
                // Asynchronously log the result.
                Commerce.Host.instance.timers.setImmediate(() => {
                    var requestString: string;
                    var responseString: string;

                    try {
                        requestString = JSON.stringify(request);
                    } catch (error) {
                        var requestUri: string = ObjectExtensions.isNullOrUndefined(request) ? StringExtensions.EMPTY
                                                                                             : (request.requestUri || StringExtensions.EMPTY);
                        requestString = StringExtensions.format("Unable to stringify request for requestUri {0}", requestUri);
                    }

                    try {
                        responseString = JSON.stringify(response);
                    } catch (error) {
                        responseString = "Unable to stringify response";
                    }

                    logMethod(requestString, responseString);
                });
            }
        }

        /**
         * Add offset fields to Date object for serialization purposes.
         * @param {Date} dateTime The date.
         * @return {Date} The date with __edmType and __offset fields injected.
         */
        private toDateTimeOffset(dateTime: Date): Date {
            if (!dateTime[ODataRequestBase.ODATA_METADATA_EDM_PROPERTY_NAME]
                || !dateTime[ODataRequestBase.ODATA_METADATA_OFFSET_PROPERTY_NAME]) {
                var pad: Function = (val: number) => val < 10 ? "0" + val : val.toString();
                var sign: string = (dateTime.getTimezoneOffset() > 0) ? "-" : "+";
                var offset: number = Math.abs(dateTime.getTimezoneOffset());
                var hours: string = pad(Math.floor(offset / 60));
                var minutes: string = pad(offset % 60);

                dateTime[ODataRequestBase.ODATA_METADATA_EDM_PROPERTY_NAME] = "Edm.DateTimeOffset";
                dateTime[ODataRequestBase.ODATA_METADATA_OFFSET_PROPERTY_NAME] = sign + hours + ":" + minutes;
            }

            return dateTime;
        }

        /**
         * Foramts function parameters in request Uri.
         * @param {any} parameters The parameter dictionary.
         * @return {string} The formated and encoded string of the parameters.
         */
        private formatFunctionParameter(parameters: any): string {
            var result: string = StringExtensions.EMPTY;

            if (parameters) {
                
                for (var parameter in parameters) {
                    var paramName: string = parameter;
                    var paramValue: any = parameters[paramName];

                    if (!StringExtensions.isEmpty(result)) {
                        result += ",";
                    }

                    if (ObjectExtensions.isUndefined(paramValue)) {
                        // handles undefined value
                        continue;
                    } else if (typeof paramValue === "string") {
                        // handles string type value
                        result += paramName + "='" + this.encodeODataURIParameterValue(paramValue) + "'";
                    } else {
                        // handles null and other types value
                        result += paramName + "=" + this.encodeODataURIParameterValue(paramValue);
                    }
                }
                
            }

            return result;
        }


        /**
         * Encodes the ODATA parameter in request Uri.
         * @param {any} literal The ODATA parameter's literal value.
         * @return {string} The encoded value.
         */
        private encodeODataURIParameterValue(literal: any): string {
            return encodeURIComponent(literal)
                .replace(ODataRequestBase.SINGLE_QUOTE_REGEX, "\'\'")
                .replace(ODataRequestBase.ADDITIONAL_RFC3986_RESERVED_CHAR_REGEX, (ch: string): string => {
                    return "%" + ch.charCodeAt(0).toString(ODataRequestBase.HEXDECIMAL_RADIX);
                });
        }

        /**
         * Foramts entity key(s) in request Uri.
         * @param {any} key The key dictionary.
         * @return {string} The formated string of the key(s).
         */
        private formatKey(key: any): string {
            var result: string = null;

            if (key) {
                var formattedKey: string[] = [];
                var propertyName: string;

                for (var property in key) {
                    if (key.hasOwnProperty(property)) {
                        propertyName = property;
                        if (typeof key[propertyName] === "string") {
                            formattedKey.push(propertyName + "='" + key[propertyName] + "'");
                        } else {
                            formattedKey.push(propertyName + "=" + key[propertyName]);
                        }
                    }
                }

                if (formattedKey.length === 1) {
                    if (typeof key[propertyName] === "string") {
                        result = "'" + key[propertyName] + "'";
                    } else {
                        result = key[propertyName];
                    }
                } else {
                    result = formattedKey.join();
                }
            }

            return result;
        }
    }
}
