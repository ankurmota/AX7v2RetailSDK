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

module Commerce.Proxy {
    "use strict";

    export class XhrHelper {
        private static ODATA_TOTAL_COUNT_PROPERTY = '@odata.count';
        private static ODATA_HAS_NEXT_PAGE_PROPERTY = '@odata.nextLink';

        /**
         * Sets up jquery ajax call parameters.
         */
        public static SetupAjaxParameters() {

            $.support.cors = true;

            $.ajaxSetup({
                cache: false,
                xhrFields: {
                    // cookies on CORS
                    withCredentials: "true"
                }
            });
        }

        /**
         * Sets up the Odata's default http client using CORS-ready jquery calls.
         */
        public static SetupODataParameters() {

            OData.jsonHandler.accept = 'application/json;odata=nometadata';
            OData.defaultHttpClient = (function () {

                var readResponseHeaders = function (xhr, headers) {
                    var responseHeaders = xhr.getAllResponseHeaders().split(/\r?\n/);

                    responseHeaders.forEach(function (value) {
                        if (value) {
                            var pair = value.split(": ");
                            headers[pair[0]] = pair[1];
                        }
                    });
                };

                return {
                    request: function (request, success, error) {

                        var timeoutHandle = 0;
                        var handleTimeout = function () {
                            if (xhr != null) {
                                xhr.abort();
                                xhr = null;
                                var statusText = "Request Timeout";
                                var response = { requestUri: request.requestUri, statusCode: 408, statusText: statusText, headers: [], body: "" };

                                error({ message: statusText, request: request, response: response });
                            }
                        };

                        var xhr = new XMLHttpRequest();

                        xhr.onreadystatechange = function () {
                            if (xhr === null || xhr.readyState != 4 /* Response ready */) {
                                return;
                            }

                            clearTimeout(timeoutHandle);

                            // Workaround for XHR behavior on IE.
                            var statusText = xhr.statusText;
                            var statusCode = xhr.status;

                            if (statusCode === 1223) {
                                statusCode = 204;
                                statusText = "No Content";
                            }

                            var headers = [];
                            readResponseHeaders(xhr, headers);

                            var response = { requestUri: request.requestUri, statusCode: statusCode, statusText: statusText, headers: headers, body: xhr.responseText };

                            xhr = null;

                            if (statusCode >= 200 && statusCode <= 299) {
                                success(response);
                            } else {
                                error({ message: statusText, request: request, response: response });
                            }
                        }

                        xhr.open(request.method || "GET", request.requestUri, true);
                        xhr.withCredentials = true; // Include cookie

                        // Transfer request headers to XHR object.
                        if (request.headers) {
                            for (var name in request.headers) {
                                xhr.setRequestHeader(name, request.headers[name]);
                            }
                        }

                        //TODO anandjo to anandjo: Revisit later.
                        // Set timeout if configured.
                        //if (Commerce.Config.connectionTimeout > 0) {
                        //    timeoutHandle = setTimeout(handleTimeout, Commerce.Config.connectionTimeout * 1000); // Configured in seconds.
                        //}

                        xhr.send(request.body);
                    }
                };
            })();
        }

        /**
         * Parses the odata result returned from server.
         *
         * @param {any} data The result returned from server.
         * @param {any} resultType The entity type to parse the result as.
         */
        public static parseOdataResult(data: any, resultType: any): any {
            var result = data;

            if (data) {
                
                // If the return result is an OData PageResult object
                if (data.value) {
                    var resultSet = data.value;

                    if (data[XhrHelper.ODATA_TOTAL_COUNT_PROPERTY]) {
                        var totalCount = data[XhrHelper.ODATA_TOTAL_COUNT_PROPERTY];
                    }

                    var hasNextPage = false;
                    if (data[XhrHelper.ODATA_HAS_NEXT_PAGE_PROPERTY]) {
                        // For now we only tell if there are more pages rather than giving the exact next page link.
                        hasNextPage = true;
                    }

                    if (resultType) {
                        if (Object.prototype.toString.call(resultSet) === '[object Array]') {
                            for (var i = 0; i < resultSet.length; i++) {
                                resultSet[i] = new resultType(resultSet[i]);
                            }
                        }
                    } 

                    resultSet.totalCount = totalCount;
                    resultSet.hasNextPage = hasNextPage;
                    result = resultSet;
                }
                else { 
                    if (resultType) {
                        result = new resultType(result);
                    }
                }
            }

            return result;
        }
    }
}
