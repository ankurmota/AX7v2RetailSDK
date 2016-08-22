/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {
    export interface IHealthCheck {
        name: string;
        data: string;
        success: string;
        result: string;
        severity: string;
    }

    export enum HealthCheckConnectivityStatus {
        None, // Not yet connected to health check Url.
        Connecting, // Trying to connect to health check Url.
        Succeeded, // Health check status succeeded.
        Failed, // Health check status not succeeded.
        Unknown // Health check entity contains no data to be reported.
    }
}