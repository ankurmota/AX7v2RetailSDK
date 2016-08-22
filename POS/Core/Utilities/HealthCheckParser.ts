/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";

    export class HealthCheckParser {

        public static HEALTH_CHECK_URL: string = "/healthcheck?testname=ping&ResultFormat=xml";
        public static DB_CHECK: string = "dbcheck";
        public static RTS_CHECK: string = "realtimeservicecheck";

        private static parseProperty(element: JQuery, propertyName: string): string {
            var resultElement: JQuery = element.find(propertyName);

            if (ObjectExtensions.isNullOrUndefined(resultElement)) {
                return null;
            }

            return resultElement.text();
        }

        /**
         * Parse the xml to become IHealthCheck[] entities.
         * @param {string} xml The xml text.
         * @returns {Model.Entities.IHealthCheck[]} The entities of IHealthCheck[].
         */
        public parse(xml: string): Model.Entities.IHealthCheck[] {
            var elements: JQuery = $(xml).find("TestResult");

            var healthCheckStatus: Model.Entities.IHealthCheck[] = [];
            if (!ObjectExtensions.isNullOrUndefined(elements) && elements.length > 0) {

                elements.each(function (): void {

                    var healthCheckResult: Model.Entities.IHealthCheck = {
                        name: HealthCheckParser.parseProperty($(this), "Name"),
                        data: HealthCheckParser.parseProperty($(this), "Data"),
                        success: HealthCheckParser.parseProperty($(this), "Success"),
                        result: HealthCheckParser.parseProperty($(this), "Result"),
                        severity: HealthCheckParser.parseProperty($(this), "Severity")
                    };

                    healthCheckStatus.push(healthCheckResult);
                });
            }

            return healthCheckStatus;
        }

        /**
         * Validate the IHealthCheck entities.
         * @param {Model.Entities.IHealthCheck[]} entities The entities to be validated.
         * @returns {boolean} True if valid, false otherwise.
         */
        public isValidEntity(entities: Model.Entities.IHealthCheck[]): boolean {
            if (!ArrayExtensions.hasElements(entities)) {
                return false;
            }

            var entity: Model.Entities.IHealthCheck;

            for (var i: number = 0; i < entities.length; i++) {
                entity = entities[i];

                if (ObjectExtensions.isNullOrUndefined(entity) ||
                    StringExtensions.isNullOrWhitespace(entity.data)) {
                    return false;
                }
            }

            return true;
        }
    }
}