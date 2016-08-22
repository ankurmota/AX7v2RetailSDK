/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Entities = Proxy.Entities;

    export class ActivationViewModel extends ViewModelBase {

        // Health check statuses
        public dbConnectivityStatus: Observable<Entities.HealthCheckConnectivityStatus>;
        public rtsConnectivityStatus: Observable<Entities.HealthCheckConnectivityStatus>;

        constructor() {
            super();

            // Initialize health check
            this.dbConnectivityStatus = ko.observable(Entities.HealthCheckConnectivityStatus.None);
            this.rtsConnectivityStatus = ko.observable(Entities.HealthCheckConnectivityStatus.None);
        }

        /**
         * Ping the health check url.
         * @param {string} healthCheckUrl The health check Url.
         */
        public pingHealthCheck(healthCheckUrl: string): void {

            this.setAllHealthCheckStatuses(Entities.HealthCheckConnectivityStatus.Connecting);

            this.authenticationManager.checkServerHealthAsync(healthCheckUrl)
                .done((healthCheckEntities: Proxy.Entities.IHealthCheck[]) => {

                    // If no entity at all found on the async result, set the connection
                    // for all entities as a failure.
                    if (!ArrayExtensions.hasElements(healthCheckEntities)) {
                        this.setAllHealthCheckStatuses(Entities.HealthCheckConnectivityStatus.Failed);
                        return;
                    }

                    for (var i: number = 0; i < healthCheckEntities.length; i++) {

                        var entity: Proxy.Entities.IHealthCheck = healthCheckEntities[i];

                        if (ObjectExtensions.isNullOrUndefined(entity) ||
                            StringExtensions.isNullOrWhitespace(entity.data)) {
                            continue;
                        }

                        // Update connection status if health entities found.
                        if (entity.data.toLowerCase() === HealthCheckParser.DB_CHECK) {
                            this.dbConnectivityStatus(entity.success.toLowerCase() === true.toString() ?
                                Entities.HealthCheckConnectivityStatus.Succeeded : Entities.HealthCheckConnectivityStatus.Failed);
                        } else if (entity.data.toLowerCase() === HealthCheckParser.RTS_CHECK) {
                            this.rtsConnectivityStatus(entity.success.toLowerCase() === true.toString() ?
                                Entities.HealthCheckConnectivityStatus.Succeeded : Entities.HealthCheckConnectivityStatus.Failed);
                        }
                    }

                    // In case any health entity isn't found on the Async result, set connection status to unknown.
                    this.setUnknownHealthCheck(this.dbConnectivityStatus);
                    this.setUnknownHealthCheck(this.rtsConnectivityStatus);

                }).fail((errors: Proxy.Entities.Error[]) => {
                    this.setAllHealthCheckStatuses(Entities.HealthCheckConnectivityStatus.Failed);
                });
        }

        /**
         * Set a status on all health check properties.
         * @param {Entities.HealthCheckConnectivityStatus} newStatus The new health check status.
         */
        public setAllHealthCheckStatuses(newStatus: Entities.HealthCheckConnectivityStatus): void {
            this.dbConnectivityStatus(newStatus);
            this.rtsConnectivityStatus(newStatus);
        }

        private setUnknownHealthCheck(connectivityStatus: Observable<Entities.HealthCheckConnectivityStatus>): void {
            if (connectivityStatus() === Entities.HealthCheckConnectivityStatus.Connecting) {
                connectivityStatus(Entities.HealthCheckConnectivityStatus.Unknown);
            }
        }
    }
}