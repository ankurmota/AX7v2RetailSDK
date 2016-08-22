/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../DataJS.d.ts'/>
///<reference path='../../JQuery.d.ts'/>

module Commerce.Proxy.Common {
    "use strict";

    export class XmlHttpRequestHelper {
        /**
         * Sets up jquery ajax call parameters.
         */
        public static SetupAjaxParameters(): void {

            $.support.cors = true;

            $.ajaxSetup({
                cache: true
            });
        }

        /**
         * Parse a header string into a key value pair object.
         * @param {string} headerString the string with the headers.
         * @returns {{ [headerName: string]: string }} the key value pair object containing the headers.
         */
        public static parseXmlHttpResponseHeaders(headerString: string): { [headerName: string]: string } {
            var responseHeaders: string[] = (headerString || "").split(/\r?\n/);
            var headers: { [headerName: string]: string } = {};

            responseHeaders.forEach(function (value: string): void {
                if (value) {
                    var pair: string[] = value.split(": ");
                    headers[pair[0]] = pair[1];
                }
            });

            return headers;
        }

        public static executeRequest(
            request: IXmlHttpRequest,
            success: (response: IXmlHttpResponse) => void,
            error: (errorDetails: IXmlHttpError) => void): void {

            var xhr: XMLHttpRequest = new XMLHttpRequest();
            var timeoutHandle: number = 0;
            var handleTimeout: () => void = function (): void {
                if (xhr != null) {
                    xhr.abort();
                    xhr = null;
                    var statusText: string = "Request Timeout";
                    var response: IXmlHttpResponse = {
                        requestUri: request.requestUri,
                        statusCode: 408,
                        statusText: statusText,
                        headers: {},
                        body: ""
                    };

                    var errorResult: IXmlHttpError = {
                        message: statusText,
                        request: request,
                        response: response
                    };

                    error(errorResult);
                }
            };

            xhr.onreadystatechange = function (): void {
                if (xhr === null || xhr.readyState !== 4 /* Response ready */) {
                    return;
                }

                clearTimeout(timeoutHandle);

                // Workaround for XHR behavior on IE.
                var statusText: string = xhr.statusText;
                var statusCode: number = xhr.status;

                if (statusCode === 1223) {
                    statusCode = Commerce.Proxy.Common.HttpStatusCodes.NO_CONTENT;
                    statusText = "No Content";
                }

                var response: IXmlHttpResponse = {
                    requestUri: request.requestUri,
                    statusCode: statusCode,
                    statusText: statusText,
                    headers: XmlHttpRequestHelper.parseXmlHttpResponseHeaders(xhr.getAllResponseHeaders()),
                    body: xhr.responseText
                };

                xhr = null;

                if (Common.HttpStatusCodes.isSuccessful(statusCode)) {
                    success(response);
                } else {
                    var errorResult: IXmlHttpError = {
                        message: statusText,
                        request: request,
                        response: response
                    };

                    error(errorResult);
                }
            };

            xhr.open(request.method || "GET", request.requestUri, true);

            // controls whether cookies will be enabled for CORS requests
            xhr.withCredentials = request.useCrossDomainCookies || false; // Include cookie

            // Transfer request headers to XHR object.
            if (request.headers) {
                ObjectExtensions.forEachKeyValuePair(request.headers,
                    (headerName: string, headerValue: string): void => {
                        xhr.setRequestHeader(headerName, headerValue);
                    });
            }

            // Set timeout if configured.
            if (Commerce.Config.connectionTimeout > 0) {
                timeoutHandle = setTimeout(handleTimeout, Commerce.Config.connectionTimeout * 1000); // Configured in seconds.
            }

            try {
                xhr.send(request.body);
            } catch (error) {
                // log error
                // but let platform change status of request
                var errorMessage: string = StringExtensions.EMPTY;

                if (!ObjectExtensions.isNullOrUndefined(error) && !ObjectExtensions.isNullOrUndefined(error.message)) {
                    errorMessage = error.message;
                }

                RetailLogger.coreLogXmlHttpRequestError(request.requestUri, errorMessage);
            }
        }

        /**
         * Sets up the Odata's default http client using CORS-ready jquery calls.
         */
        public static SetupODataParameters(): void {
            OData.jsonHandler.accept = "application/json;odata=nometadata";
            OData.defaultHttpClient = {
                request: XmlHttpRequestHelper.executeRequest
            };
        }
    }
}