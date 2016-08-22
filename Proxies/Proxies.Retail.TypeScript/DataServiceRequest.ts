/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='DataJS.d.ts'/>
///<reference path='JQuery.d.ts'/>
///<reference path='Exceptions/ProxyError.ts'/>
///<reference path='Exceptions/NotificationHandler.ts'/>
///<reference path='Exceptions/ErrorParser.ts'/>
///<reference path='Extensions/ObjectExtensions.ts'/>
///<reference path='Extensions/StringExtensions.ts'/>
///<reference path='Tracer.ts'/>
///<reference path='CommerceContext.g.ts'/>
///<reference path='DataServiceQuery.ts'/>
///<reference path='Diagnostics.TypeScriptCore.d.ts'/>
//////<reference path='../../Random.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
     * Represents a data service request object.
     */
    export class DataServiceRequest implements Common.IDataServiceRequest {
        public static GET: string = "GET";
        public static POST: string = "POST";
        public static PATCH: string = "PATCH";
        public static DELETE: string = "DELETE";

        private static DEFAULT_HEADER: { [headerName: string]: string } = {
            "Prefer": "return=representation"
        };

        private static OPERATINGUNITNUMBER_HEADERNAME = "OUN";
        private static AUTHORIZATION_HEADERNAME = "Authorization";
        private static ACCEPT_LANGUAGE = "Accept-Language";

        private _query: Common.IDataServiceQueryInternal;
        private _serverUri: string;
        private _requestUri: string;
        private _method: string;
        private _id: number;
        private _operatingUnitNumber: string;
        private _authToken: string;
        private _hasOperationName: boolean;
        private _locale: string;

        private get batchRequestUri(): string {
            return this._serverUri + "/$batch";
        }

        /**
         * Gets Locale for the current data service factory instance.
         */
        public get locale(): string {
            return this._locale;
        }

        /**
         * Sets Locale for the current data service factory instance.
         */
        public set locale(locale: string) {
            this._locale = locale;
        }

        /**
         * Gets the Request Identifier.
         * Auto-generated unique identifier if executed in batch. Used to fetch the response from batch result array.
         */
        public id(): number {
            return this._id;
        }

        constructor(serverUri: string, query: Common.IDataServiceQueryInternal, operatingUnitNumber: string, authToken: string, locale: string = "") {
            Commerce.Proxy.Tracer.Information("DataServiceRequest.constructor()");

            this._serverUri = serverUri;
            this._query = query;
            this._operatingUnitNumber = operatingUnitNumber;
            this._authToken = authToken;
            this._hasOperationName = false;
            this._locale = locale;

            switch (query.action) {
                case "Read":
                case "ReadAll":
                    this._method = DataServiceRequest.GET;
                    break;

                case "Create":
                    this._method = DataServiceRequest.POST;
                    break;

                case "Update":
                    this._method = DataServiceRequest.PATCH;
                    break;

                case "Delete":
                    this._method = DataServiceRequest.DELETE;
                    break;

                default:
                    if (query.isAction) {
                        // Sets the HTTP method to be POST for actions
                        this._method = DataServiceRequest.POST;
                    } else {
                        // Sets the HTTP method to be GET for functions
                        this._method = DataServiceRequest.GET;
                    }

                    this._hasOperationName = true;
                    break;
            }

            this._requestUri = this.getRequestUri(query);
        }

        /**
         * Gets the Request Uri.
         */
        public getRequestUri(query: Common.IDataServiceQueryInternal): string {
            var requestUri: string = this._serverUri + (query.entitySet
                ? "/" + query.entitySet + (query.key
                ? "(" + DataServiceRequest.formatKey(query.key) + ")"
                : StringExtensions.EMPTY)
                : StringExtensions.EMPTY);
            
            // Appends the action (POST) or function (GET) name to the Uri
            if (this._hasOperationName) {
                requestUri += "/" + query.action;

                // Construct OData function Url with parameters. [e.g. Func(), Func(param1='',param2='')]
                if (!query.isAction && query.data instanceof Common.ODataOperationParameters) {
                    requestUri += "(" + DataServiceRequest.formatFunctionParameter(query.data.parameters) + ")";
                }
            }

            if (query.isReturnTypeACollection) {
                requestUri = DataServiceRequest.addParameter(requestUri, "$top",
                    query.resultSettings ? query.resultSettings.Paging.Top : 1000); //Config.defaultPageSize);
                requestUri = DataServiceRequest.addParameter(requestUri, "$skip", query.resultSettings ? query.resultSettings.Paging.Skip : 0);
                requestUri = DataServiceRequest.addParameter(requestUri, "$inlinecount", query.inlineCount ? "allpages" : null);

                requestUri = DataServiceRequest.addParameter(requestUri, "$filter", query.filterSettings ? query.filterSettings : 0);

                if (query.resultSettings && query.resultSettings.Sorting.Columns) {
                    query.resultSettings.Sorting.Columns.forEach((column: Entities.SortColumn) => {
                        requestUri = DataServiceRequest.addParameter(requestUri, "$orderby", column.ColumnName);
                    });
                }
            }

            if (ArrayExtensions.hasElements(query.expands)) {
                requestUri = DataServiceRequest.addParameter(requestUri, "$expand", query.expands.join());
            }

            if (!StringExtensions.isNullOrWhitespace(Proxy.CommerceContext.ApiVersion)) {
                requestUri = DataServiceRequest.addParameter(requestUri, "api-version", Proxy.CommerceContext.ApiVersion);
            }

            return requestUri;
        }

        /**
         * Executes the request.
         * @param {any} callerContext The caller context.
         * @return {IAsyncResult<T>} The async result.
         */
        public execute<T>(callerContext: any): IAsyncResult<T> {
            var asyncResult = new AsyncResult<T>(callerContext);
            var headers: { [headerName: string]: string } = this.buildQueryHeader();

            if (!this._query.isReturnTypeACollection || this._query.resultSettings.Paging.Top >= 0) {
                this.loadPage<T>(asyncResult, headers);
            } else {
                this.getAllPages<T>(asyncResult, headers, 0);
            }

            return asyncResult;
        }

        /**
         * Executes paginated requests for all entities.
         * @param {AsyncResult{T}} asyncResult The async result.
         * @param {{ [headerName: string]: string }} headerName the headers for the request.
         * @param {number} skip The offset.
         * @param {Array<T>} The array result.
         */
        private getAllPages<T>(asyncResult: AsyncResult<T>, headers: { [headerName: string]: string }, skip: number, results?: Array<T>): void {
            skip = skip || 0;
            var top = 1000; //Config.defaultPageSize;
            var getPageAsyncResult: AsyncResult<T> = new AsyncResult<T>();
            this._query.resultSettings.Paging.Top = top;
            results = results || [];

            getPageAsyncResult.done((pageResult: T) => {
                if (pageResult) {
                    if (typeof(<any>pageResult).length === "number") { // The pageResult is an array
                        results = results.concat(<any>pageResult);

                        if ((<any>pageResult).length < top) {  // This page contains less items than top
                            if ((<any>pageResult).hasNextPage) {  // This means the pagesize defined on server side is smaller than the top
                                skip += (<any>pageResult).length;
                                this.getAllPages<T>(asyncResult, headers, skip, results);  // Try to fetch the next page
                            } else {  // No more pages need to fetch
                                asyncResult.resolve(<T><any>results);
                            }
                        } else {  // This page contains exact amount of item as the top, should not contain more
                            skip += top;
                            this.getAllPages<T>(asyncResult, headers, skip, results);  // Try to fetch the next page
                        }
                    } else {   // The pageResult is a single item
                        results.push(pageResult);
                        asyncResult.resolve(<T><any>results);
                    }
                }
            }).fail((errors: ProxyError[]) => {
                    asyncResult.reject(errors);
                });

            this.loadPage(getPageAsyncResult, headers, skip);
        }

        /**
         * Builds the header to be sent as part of the request.
         * @returns {{ [headerName: string]: string }} a key value pair collection of header names and header values.
         */
        private buildQueryHeader(): { [headerName: string]: string } {
            var headers: { [headerName: string]: string } = {};

            // copy default values from default header
            for (var headerName in DataServiceRequest.DEFAULT_HEADER) {
                headers[headerName] = DataServiceRequest.DEFAULT_HEADER[headerName];
            }

            // Copy operating unit number if present for C2 (if an operation is done by a customer or by anonymous user) scenarios.
            if (!StringExtensions.isNullOrWhitespace(this._operatingUnitNumber)) {
                headers[DataServiceRequest.OPERATINGUNITNUMBER_HEADERNAME] = this._operatingUnitNumber;
            }

            // Add accept-language header to send user preferred locale to server.
            headers[DataServiceRequest.ACCEPT_LANGUAGE] = this._locale;
           
            // add any tokens as part of the header
            for (var tokenName in this._query.tokens) {
                var tokenValue: string = this._query.tokens[tokenName];

                if (!StringExtensions.isNullOrWhitespace(tokenValue)) {
                    headers[tokenName] = tokenValue;
                }
            }

            // Now check whether authentication token is present or not, if present it will be added into Authorization header.
            if (!StringExtensions.isNullOrWhitespace(this._authToken)) {
                headers[DataServiceRequest.AUTHORIZATION_HEADERNAME] = this._authToken;
            }

            return headers;
        }

        /**
         * Executes request for one page.
         * @param {AsyncResult<T>} The async result.
         * @param {{ [headerName: string]: string }} headerName the headers for the request.
         */
        private loadPage<T>(asyncResult: AsyncResult<T>, headers: { [headerName: string]: string }, skip?: number): void {
            if (!ObjectExtensions.isNullOrUndefined(skip)) {
                this._query.resultSettings.Paging.Skip = skip;
            }

            var requestId = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();
            RetailLogger.modelManagersRetailServerRequestStarted(requestId, this._requestUri);

            // This is the pipe for all OData API calls.
            OData.request({
                requestUri: this.getRequestUri(this._query),
                method: this._method,
                data: DataServiceRequest.toJson(this._query.data),
                headers: headers,
            },
                (data) => {
                    RetailLogger.modelManagersRetailServerRequestFinished(requestId, this._requestUri);
                    asyncResult.resolve(XhrHelper.parseOdataResult(data, this._query.returnType));
                },
                (error: any) => {
                    RetailLogger.modelManagersRetailServerRequestError(requestId, this._requestUri, error.message);
                    asyncResult.reject(ErrorParser.parseErrorMessage(error));
                },
                OData.jsonHandler);
        }

        /**
         * Executes the batch requests.
         * @param {DataServiceRequest[]} requests The collection of requests to execute.
         * @param {any} [callerContext] The caller context.
         * @return {IAsyncResult<Array>} The async result.  Responses at index I correlates to request with identifier I.
         */
        public executeBatch(requests: DataServiceRequest[], callerContext?: any): IAsyncResult<any[]> {
            var asyncResult = new AsyncResult<any[]>(callerContext);

            OData.request(DataServiceRequest.buildBatchRequest(this.batchRequestUri, requests),
                (data) => {
                    var responses: any[] = new Array();
                    var errors: ProxyError[] = DataServiceRequest.parseBatchResponses(data.__batchResponses, requests, responses);

                    if (errors.length == 0) {
                        Commerce.Proxy.Tracer.Information(this.batchRequestUri + " success");
                        asyncResult.resolve(responses);
                    } else {
                        Commerce.Proxy.Tracer.Errors(errors, this.batchRequestUri + " failed.");
                        asyncResult.reject(errors);
                    }
                },
                (error: any) => {
                    Commerce.Proxy.Tracer.Error(this.batchRequestUri + " failed");
                    asyncResult.reject(ErrorParser.parseErrorMessage(error));
                },
                OData.batchHandler
                );

            return asyncResult;
        }

        private static buildBatchRequest(batchRequestUri: string, requests: DataServiceRequest[]): ODataRequest {
            var requestId = 1;

            // Creates an empty batch request.
            var odataBatchRequest: ODataRequest = {
                requestUri: batchRequestUri,
                method: DataServiceRequest.POST,
                data: { __batchRequests: [] }
            };

            var batchRequests = odataBatchRequest.data.__batchRequests;
            var changeRequests;

            requests.forEach((request) => {
                request._id = requestId++;

                var odataRequest: ODataRequest = {
                    requestUri: request._requestUri,
                    method: request._method,
                    data: DataServiceRequest.toJson(request._query.data),
                    headers: { "Content-ID": request.id(), "Prefer": "return=representation" }
                };

                if (request._method == DataServiceRequest.GET) {
                    batchRequests.push(odataRequest);
                }
                else {
                    if (!changeRequests) {
                        changeRequests = [];
                        batchRequests.push({ __changeRequests: changeRequests });
                    }
                    changeRequests.push(odataRequest);
                }
            });

            return odataBatchRequest;
        }

        /**
         * Parse the batch responses.
         * @param {any[]} batchResponses The server responses.
         * @param {any[]} requests The request objects.
         * @param {any[]} responses The responses passed down to the success callback.
         */
        private static parseBatchResponses(batchResponses: any[], requests: any[], responses: any[]): ProxyError[] {
            var errors: ProxyError[] = new Array();

            batchResponses.forEach((response) => {
                if (response.__changeResponses) {
                    errors = errors.concat(DataServiceRequest.parseBatchResponses(response.__changeResponses, requests, responses));
                }
                else {
                    if (ObjectExtensions.isNullOrUndefined(response.message)) {
                        var requestId = parseInt(response.headers["Content-ID"]);

                        responses[requestId] = XhrHelper.parseOdataResult(response.data, requests[requestId - 1]._query.returnType);
                    } else {
                        errors = errors.concat(ErrorParser.parseErrorMessage(response));
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
         */
        private static toJson(data: any, parent?: any, modifyDataInPlace?: boolean) {
            var clone = data;

            if (data == null) {
                return null;
            } else if (typeof (data) !== "object") {
                //if data type is primitive type, no need to process further
                //as calling jquery extend or calling foreach prop on string type
                //will transform string to array of chars which will be incorrect result.
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

            for (var prop in clone) {
                if (clone.hasOwnProperty(prop)) {
                    var property = clone[prop];
                    var isUndefined = ObjectExtensions.isUndefined(property);

                    // only non-undefined properties are added to the object
                    if (!isUndefined) {
                        var value;

                        var isNull = ObjectExtensions.isNull(property);
                        var isObject = ObjectExtensions.isObject(property);

                        if (isNull) {
                            value = null;
                        } else if (isObject) {
                            if (property instanceof Array) {
                                value = DataServiceRequest.arrayToJson(<any[]>property, parent, modifyDataInPlace);
                            } else if (property instanceof Date) {
                                value = DataServiceRequest.toDateTimeOffset(property);
                            } else {
                                // recursively stringifies objects
                                value = DataServiceRequest.toJson(property, clone, modifyDataInPlace);
                            }
                        } else {
                            value = property;
                        }

                        clone[prop] = value;
                    }
                }
            }

            return clone;
        }

        /**
        * Helper function to convert array into a stringified JSON object following OData specifications.
        * @param {Array} array The data to be converted to a stringified JSON object.
        * @param {any} parent The container of the array.
        * @param {boolean} [modifyDataInPlace] Whether the object passed should be modified in place. If not, a clone is created instead.
        */
        private static arrayToJson(array: any[], parent: any, modifyDataInPlace?: boolean): any {
            var data = [];
            for (var i = 0; i < array.length; i++) {
                data[i] = DataServiceRequest.toJson(array[i], array, modifyDataInPlace);
            }

            // if array is not a top level data on an action            
            return data;
        }

        /**
         * Add offset fields to Date object for serialization purposes.
         * @param {Date} dateTime The date.
         * @return {Date} The date with __edmType and __offset fields injected.
         */
        private static toDateTimeOffset(dateTime: Date): Date {
            if (!dateTime["__edmType"] || !dateTime["__offset"]) {
                var pad: Function = (val: number) => val < 10 ? "0" + val : val.toString();
                var sign: string = (dateTime.getTimezoneOffset() > 0) ? "-" : "+";
                var offset: number = Math.abs(dateTime.getTimezoneOffset());
                var hours: string = pad(Math.floor(offset / 60));
                var minutes: string = pad(offset % 60);

                dateTime["__edmType"] = "Edm.DateTimeOffset";
                dateTime["__offset"] = sign + hours + ":" + minutes;
            }

            return dateTime;
        }

        /**
         * Foramts entity key(s) in request Uri.
         * @param {any} key The key dictionary.
         * @return {string} The formated string of the key(s).
         */
        private static formatKey(key: any): string {
            var result: string = null;

            if (key) {
                var formattedKey = [];
                var propertyName: string;

                for (var property in key) {
                    if (key.hasOwnProperty(property)) {
                        propertyName = property;
                        if (typeof key[propertyName] == 'string') {
                            formattedKey.push(propertyName + "='" + key[propertyName] + "'");
                        }
                        else {
                            formattedKey.push(propertyName + "=" + key[propertyName]);
                        }
                    }
                }

                if (formattedKey.length == 1) {
                    if (typeof key[propertyName] == 'string') {
                        result = "'" + key[propertyName] + "'";
                    }
                    else {
                        result = key[propertyName];
                    }
                }
                else {
                    result = formattedKey.join();
                }
            }

            return result;
        }

        /**
         * Foramts function parameters in request Uri.
         * @param {any} parameters The parameter dictionary.
         * @return {string} The formated and encoded string of the parameters.
         */
        private static formatFunctionParameter(parameters: any): string {
            var result: string = StringExtensions.EMPTY;

            if (parameters) {
                for (var parameter in parameters) {
                    var paramName = parameter;
                    var paramValue = parameters[paramName];

                    if (!StringExtensions.isEmpty(result)) {
                        result += ",";
                    }

                    if (ObjectExtensions.isUndefined(paramValue)) {
                        // handles undefined value 
                        continue;
                    } else if (typeof paramValue == 'string') {
                        // handles string type value
                        result += paramName + "='" + encodeURIComponent(paramValue) + "'";
                    } else {
                        // handles null and other types value
                        result += paramName + "=" + encodeURIComponent(paramValue);
                    }
                }
            }

            return result;
        }

        private static addParameter(requestUri: string, key: string, value: any): string {
            if (value) {
                var parameterSeparator = "?";

                if (requestUri.indexOf(parameterSeparator) > 0) {
                    parameterSeparator = "&";
                }

                return requestUri + parameterSeparator + key + "=" + value;
            } else {
                return requestUri;
            }
        }
    }
}