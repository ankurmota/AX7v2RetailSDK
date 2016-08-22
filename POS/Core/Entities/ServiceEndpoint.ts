/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Locator.Model.Entities {
    "use strict";

    /**
     * Server Service Endpoint names.
     */
    export class ServerServiceEndPointNames {
        public static RetailServer : string = "RetailServer.RetailService"; 
    }

    /**
     * List of available Service Types.
     */
    export enum ServiceType {
        UnknownService = 0,
        Commerce = 1,
        Sites = 2,
        Locator = 3,
        Test = 4,
        PartnerPortal = 5,
        CustomerSupport = 6,
        RapidStart = 7,
        Nav = 8,
        SgManage = 9,
        Cloud = 10,
        SignUpUrl = 11,
        Telemetry = 12,
        Health = 13,
        SqlSU = 14,
        ScomSU = 15,
        DeploymentService = 16,
        Retail = 17,
        ERP = 18,
        AX = 19,
    }

    /**
    * ServiceEndpoint entity interface.
     */
    export interface ServiceEndpoint {
        Name?: string;
        UrlString?: string;
        Geography?: string;
        ServiceUnitId?: string;
        PublicUriString?: string;
        ConsumerId?: string;
        ApplicationServiceId?: string;
        TenantId?: string;
        ResourceId?: string;
        ConsumerType?: number;
        ServiceType?: number;
    }              
    
    /**
    * ServiceEndpoint entity class.
     */
    export class ServiceEndpointClass implements ServiceEndpoint {   
        public Name : string;
        public UrlString: string;
        public Geography : string;
        public ServiceUnitId : string;
        public PublicUriString : string;
        public ConsumerId : string;
        public ApplicationServiceId : string;
        public TenantId : string;
        public ResourceId : string;
        public ConsumerType : number;
        public ServiceType: number;

        /**
        * Construct an object from odata response.
        *
        * @param {any} odataObject The odata result object.
         */
        constructor(odataObject?: any) {
            odataObject = odataObject || {};
            this.Name = odataObject.Name;
            this.UrlString = odataObject.UrlString;
            this.Geography = odataObject.Geography;
            this.ServiceUnitId = odataObject.ServiceUnitId;
            this.PublicUriString = odataObject.PublicUriString;
            this.ConsumerId = odataObject.ConsumerId;
            this.ApplicationServiceId = odataObject.ApplicationServiceId;
            this.TenantId = odataObject.TenantId;
            this.ResourceId = odataObject.ResourceId;
            this.ConsumerType = odataObject.ConsumerType;
        }
    }
}