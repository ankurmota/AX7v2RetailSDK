/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Common {
    "use strict";

    /**
     * Represents a data service query object for internal paramater from DataServiceQuery to DataServiceRequest.
     */
    export interface IDataServiceQueryInternal {
        entitySet?: string;
        entityType?: string;
        key?: any;
        action?: string;
        isAction?: boolean;
        data?: any;
        dataType?: string;
        returnType?: any;
        isReturnTypeACollection?: boolean;
        resultSettings?: Entities.QueryResultSettings;
        filterSettings?: string;
        expands?: string[];
        inlineCount?: boolean;
        headers?: { [headerName: string]: string };
    }
}