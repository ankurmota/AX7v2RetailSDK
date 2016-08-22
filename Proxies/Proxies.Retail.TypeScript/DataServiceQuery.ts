/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Entities/CommerceTypes.g.ts'/>
///<reference path='Extensions/StringExtensions.ts'/>
///<reference path='Interfaces/IDataServiceRequestFactory.ts'/>
///<reference path='Interfaces/IDataServiceRequest.ts'/>

module Commerce.Proxy.Common {
    "use strict";

    /**
     * Represents a data service query object.
     */
    export class DataServiceQuery<T> {
        private static AUTHENTICATION_TOKEN_NAME: string = "Authorization";
        private static AUTHENTICATION_TOKEN_FORMAT: string = "Bearer {0}";
        private _query: IDataServiceQueryInternal;
        private _dataServiceRequestFactory: IDataServiceRequestFactory;

        /**
         * Creates a new instance of DataServiceQuery<T>.
         * @param {IDataServiceRequestFactory} dataServiceRequestFactory The data service request factory.
         * @param {string} [entitySet] The entity set name.
         * @param {string} [entityType] The entity type name.
         * @param {any} [returntype] The return type of query.
         * @param {any} [key] Entity key.
         */
        constructor(
            dataServiceRequestFactory: IDataServiceRequestFactory,
            entitySet?: string,
            entityType?: string,
            returnType?: any,
            key?: any) {

            this._dataServiceRequestFactory = dataServiceRequestFactory;
            this._query = {
                entitySet: entitySet,
                entityType: entityType,
                key: key,
                returnType: returnType,
                tokens: {}
            };
            this._query.resultSettings = { Paging: {}, Sorting: {} };
        }

        /**
         * Query result settings.
         * 
         * @param {string} value The value of query result settings.
         */
        public resultSettings(value: Entities.QueryResultSettings): DataServiceQuery<T> {
            if (value == null) {
                this._query.resultSettings = { Paging: {}, Sorting: {} };
            }
            else {
                this._query.resultSettings = value;
            }

            return this;
        }

        /**
         * Filter Clause.
         * @param {string} value The filter value.
         */
        public filter(value: string): DataServiceQuery<T> {
            this._query.filterSettings = value;

            return this;
        }

        /**
         * Top clause
         * @param {string} value The number of top rows to select.
         */
        public top(value: number): DataServiceQuery<T> {
            this._query.resultSettings.Paging.Top = value;

            return this;
        }

        /**
         * Skip clause
         * @param {string} value The number of rows to skip
         */
        public skip(value: number): DataServiceQuery<T> {
            this._query.resultSettings.Paging.Skip = value;

            return this;
        }

        /**
         * Expand clause
         * @param {string} propertyName The property name to expand.
         */
        public expand(propertyName: string): DataServiceQuery<T> {
            if (this._query.expands == null) {
                this._query.expands = [];
            }

            this._query.expands.push(propertyName);
            return this;
        }

        /**
         * Request number of total row available.
         */
        public inlineCount(): DataServiceQuery<T> {
            this._query.inlineCount = true;

            return this;
        }

        /**
         * Order by clause.
         * @param {string} value The field name for ordering.
         */
        public orderBy(value: string): DataServiceQuery<T> {
            if (!this._query.resultSettings.Sorting.Columns) {
                this._query.resultSettings.Sorting.Columns = [];
            }

            this._query.resultSettings.Sorting.Columns.push({ ColumnName: value });

            return this;
        }

        /**
         * Transforms the request into a create request.
         * @param {string} object The object.
         * @return {IDataServiceRequest} The data service request.
         */
        public create(object: any): IDataServiceRequest {
            this._query.action = "Create";
            this._query.data = object;

            return this._createRequest();
        }

        /**
         * Transforms the request into a read request.
         * @return {IDataServiceRequest} The data service request.
         */
        public read(): IDataServiceRequest {
            if (this._query.key) {
                this._query.action = "Read";
            } else {
                this._query.action = "ReadAll";
                this._query.isReturnTypeACollection = true;
            }

            return this._createRequest();
        }

        /**
         * Transforms the request into an update request.
         * @param {string} object The object.
         * @return {IDataServiceRequest} The data service request.
         */
        public update(object: any): IDataServiceRequest {
            this._query.action = "Update";
            this._query.data = object;

            return this._createRequest();
        }

        /**
         * Transforms the request into a delete request.
         * @return {DataServiceRequest} The data service request.
         */
        public delete(): IDataServiceRequest {
            this._query.action = "Delete";

            return this._createRequest();
        }

        public setGetAllRecords(): void {
            this._query.resultSettings.Paging.Top = -1;
        }

        public isGetAllRecords(): boolean {
            return this._query.resultSettings.Paging.Top === -1;
        }

        public createDataServiceRequestForOperation(
            operationName: string,
            isAction: boolean,
            returnType: any,
            isReturnTypeACollection: string,
            data?: ODataOperationParameters): IDataServiceRequest {

            this._query.returnType = returnType;
            this._query.isAction = isAction;
            this._query.isReturnTypeACollection = isReturnTypeACollection === "true";
            this._query.action = operationName;
            this._query.data = data;

            if (this._query.isReturnTypeACollection && ObjectExtensions.isNullOrUndefined(this._query.resultSettings.Paging.Top)) {
                // for paginated requests, where we didn't specify page size, set to fetch all records
                this.setGetAllRecords();
            }

            return this._createRequest();
        }

        private _createRequest(): IDataServiceRequest {
            return this._dataServiceRequestFactory.create(this._query);
        }
    }

    /**
     * Represents collection of OData action or function parameters.
     */
    export class ODataOperationParameters {
        public parameters: any;
    }

    /**
     * Represents a data service query object for internal paramater from DataServiceQuery to DataServiceRequest.
     */
    export interface IDataServiceQueryInternal {
        entitySet: string;
        entityType: string;
        key: any;
        action?: string;
        isAction?: boolean;
        data?: any;
        returnType: any;
        isReturnTypeACollection?: boolean;
        resultSettings?: Entities.QueryResultSettings;
        filterSettings?: string;
        expands?: string[];
        inlineCount?: boolean;
        tokens: { [tokenName: string]: string };
    }
}