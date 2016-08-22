/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Context {

    /**
     * The context for performing authentication calls against the Commerce Authentication endpoint.
     */
    export class CommerceAuthenticationContext {
        private requestFactory: Common.IDataServiceRequestFactory;

        /**
         * Initializes a new instance of the CommerceAuthenticationContext class.
         */
        constructor(requestFactory: Common.IDataServiceRequestFactory) {
            this.requestFactory = requestFactory;
        }

        /**
         * Gets or sets Locale for the current data service factory instance.
         */
        public get locale(): string {
            return "";
        }

        /**
         * Requests a token for the user.
         */
        public token(logonRequest: Entities.Authentication.ILogonRequest): Common.IDataServiceRequest {
            return this.createRequest("token", logonRequest);
        }

        /**
         * Enrolls user credentials.
         */
        public enrollUserCredentials(request: Entities.Authentication.IEnrollRequest): Common.IDataServiceRequest {
            return this.createRequest("enrollUserCredentials", request, Common.MimeTypes.APPLICATION_JSON);
        }

        /**
         * Disenrolls user credentials.
         */
        public disenrollUserCredentials(request: Entities.Authentication.IDisenrollRequest): Common.IDataServiceRequest {
            return this.createRequest("unenrollUserCredentials", request, Common.MimeTypes.APPLICATION_JSON);
        }

        /**
         * Changes user password.
         */
        public changePassword(request: Entities.Authentication.IChangePasswordRequest): Common.IDataServiceRequest {
            return this.createRequest("changePassword", request, Common.MimeTypes.APPLICATION_JSON);
        }

        /**
         * Resets user password.
         */
        public resetPassword(request: Entities.Authentication.IResetPasswordRequest): Common.IDataServiceRequest {
            return this.createRequest("resetPassword", request, Common.MimeTypes.APPLICATION_JSON);
        }

        /**
         * Creates a request.
         * @param {string} action the action name.
         * @param {any} data the data to be sent on the request.
         * @param {string} dataType the type of the data.
         * @returns {Common.IDataServiceRequest} the data service request created.
         */
        private createRequest(action: string, data: any, dataType?: string): Common.IDataServiceRequest {
            var query: Common.IDataServiceQueryInternal = {
                entityType: "Authentication",
                action: action,
                data: data,
                dataType: dataType
            };

            return this.requestFactory.create(query);
        }
    }
}