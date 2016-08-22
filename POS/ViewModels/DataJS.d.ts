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

/**
 * OData request interface.
 */
interface ODataRequest {

    /**
     * OData endpoint URI
     */
    requestUri: string;

    /**
     * HTTP method (GET, POST, PUT, DELETE)
     */
    method: string;

    /**
     * Payload of the request (in intermediate format)
     */
    data: any;

    /**
     * Object that contains HTTP headers as name value pairs
     */
    headers?: Object;

    /**
     * (Optional) Username to send for BASIC authentication
     */
    user?: string;

    /**
     * (Optional) Password to send for BASIC authentication
     */
    password?: string;

    /**
     * (Optional) Whether or not to use cross domain cookies.
     */
    useCrossDomainCookies?: boolean;
}

/**
 * OData metadata interface.
 */
interface ODataMetadata {
    /**
     * Object holding all the properties on this metadata.
     * Basically it is a dictionary of Property -> Property metadata.
     */
    properties: Object;

    /**
     * One to one mapping for data and metada on a collection.
     */
    elements?: ODataMetadata[];

    /**
     * The data type;
     */
    type: string;
}

/**
 * OData interface.
 */
interface OData {

    /**
     * Odata read.
     * @param {string} uri A string containing the URL to which the request is sent.
     * @param {any} success A callback function that is executed if the request succeeds, taking the processed data and the server response.
     * @param {any} error (Optional) A callback function that is executed if the request fails, taking an error object.
     */
    read(uri: string, success, error?, handler?);

    /**
     * Odata request.
     * @param {ODataRequest} request An object that represents the HTTP request to be sent.
     * @param {any} success A callback function that is executed if the request succeeds, taking the processed data and the server response.
     * @param {any} error (Optional) A callback function that is executed if the request fails, taking an error object.
     * @param {any} handler (Optional) Batch request handler function.
     */
    request(request: ODataRequest, success, error?, handler?);

    /**
     * Default http client.
     */
    defaultHttpClient;

    /**
     * Default request handler.
     */
    defaultHandler;

    /**
     * Default metadata.
     */
    defaultMetadata;

    /**
     * Batch request handler.
     */
    batchHandler;

    /**
     * JSON Handler
     */
    jsonHandler;

    /**
     * metadata handler.
     */
    metadataHandler;

    /**
     * json propery value reader.
     */
    jsonLightReadStringPropertyValue
}


/**
 * DataJS cache options interface.
 */
interface DataJSCacheOptions {

    /**
     * Name of the cache (used by the underlying store)
     */
    name: string;

    /**
     * URI for the collection (without $top or $skip options)
     */
    source: string;

    /**
     * Estimated maximum size of cached data in bytes.
     */
    cacheSize?: number;

    /**
     * Page size for requests and prefetching
     */
    pageSize?: number;

    /**
     * Number of items to prefetch ahead of any request in a deferred manner, -1 to scan until the end of the collection, 0 to disable.
     */
    prefetchSize?: number;

    /**
     * Initial value for the onidle event.
     */
    idle?: number;

    /**
     * If specified, indicates the store mechanism to use.
     * Values : "best", "memory", "dom" and "indexeddb".
     */
    mechanism?: string;

    /**
     * httpClient, user, password, enableJsonpCallback, callbackParameterName, formatQueryString, httpClient: when the source is an OData URI, these values will be applied.
     */
    metadata?: string;
}

/**
 * DataJS cache interface.
 */
interface DataJSCache {

    /**
     * Read the specifie number of records.
     * @param {number} startIndex Start index of data range.
     * @param {number} count Number of records.
     * @return {DataJSDeferredResult} Deferred result.
     */
    readRange(startIndex: number, count: number): DataJSDeferredResult;

    /**
     * This returns a deferred result. When the result completes, the success handler is invoked with the count of items in the collection.
     * @return {DataJSDeferredResult} Deferred result.
     */
    count(): DataJSDeferredResult;

    /**
     * This returns a deferred result. When the result completes, all data in the cache will have been cleared.
     */
    clear();
}

/**
 * DataJS store interface.
 */
interface DataJSStore {

    /**
     * Adds a new key/value pair, fails if the key was already in the store.
     * @param {string} key The key.
     * @param {any} value The value
     * @param {(key: string, value: any) => void} successCallback The success Callback.
     * @param {(error: any) => void} errorCallback (Optional) The error Callback.
     */
    add(key: string, value: any, successCallback: (key: string, value: any) => void, errorCallback?: (error: any) => void);

    /**
     * Adds a new key/value pair, updates the existing value if the key was already in the store. 
     * @param {any} key The key.
     * @param {any} value The value
     * @param {(key: string, value: any) => void} successCallback The success Callback.
     * @param {(error: any) => void} errorCallback (Optional) The error Callback.
     */
    addOrUpdate(key: any, value: any, successCallback: (key: string, value: any) => void, errorCallback?: (error: any) => void);

    /**
     * Checks whether the key is in the store.
     * @param {any} key The key.
     * @param {(result: boolean) => void} successCallback The success Callback.
     * @param {(error: any) => void} errorCallback (Optional) The error Callback.
     */
    contains(key: any, successCallback: (result: boolean) => void, errorCallback?: (error: any) => void);

    /**
     * Returns an array with all key values.
     * @param {(array: any) => void} successCallback The success Callback.
     * @param {(error: any) => void} errorCallback (Optional) The error Callback.
     */
    getAllKeys(successCallback: (array: any) => void, errorCallback?: (error: any) => void);

    /**
     * Gets a key/value back; fails if the key is not present.
     * @param {any} key The key.
     * @param {(key: string, value: any) => void} successCallback The success Callback.
     * @param {(error: any) => void} errorCallback (Optional) The error Callback.
     */
    read(key: any, successCallback: (key: string, value: any) => void, errorCallback?: (error: any) => void);

    /**
     * Removes a key and its value from the store if found; the success callback argument is true if the key was found, false otherwise.
     * @param {any} key The key.
     * @param {(result: boolean) => void} successCallback The success Callback.
     * @param {(error: any) => void} errorCallback (Optional) The error Callback.
     */
    remove(key: any, successCallback: (result: boolean) => void, errorCallback?: (error: any) => void);

    /**
     * Cleans up all data use and invalidates the store.
     * @param {() => void} successCallback The success Callback.
     * @param {(error: any) => void} errorCallback (Optional) The error Callback.
     */
    clear(successCallback: () => void, errorCallback?: (error: any) => void);

    /**
     * Closes any resources that the store may be using.
     */
    close();
}

/**
 * DataJS deferred result interface.
 */
interface DataJSDeferredResult {

    /**
     * Register success and failure callbacks.
     * @param {any} successCallback The success Callback.
     * @param {any} errorCallback (Optional) The error Callback.
     */
    then(successCallback, errorCallback?);

    /**
     * Attempt to abort the operation.If the operation hasn't completed, the fail callback will be invoked, otherwise nothing will happen.
     */
    abort();
}


/**
 * DataJS interface.
 */
interface DataJS {

    /**
     * Create data cache.
     * @param {DataJSCacheOptions} options An object that represents data js cache options.
     */
    createDataCache(options: DataJSCacheOptions): DataJSCache;

    /**
     * Create store of specified type.
     * @param {string} name A string used to distinguish one store from another.
     * @param {string} mechanism Store mechanism [Possible values: "best", "memory", "dom" and "indexeddb"]
     */
    createStore(name: string, mechanism?: string);
}

declare var OData: OData;
declare var datajs: DataJS;