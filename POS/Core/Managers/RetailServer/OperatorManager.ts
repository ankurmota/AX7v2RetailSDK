/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IOperatorManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class OperatorManager implements Commerce.Model.Managers.IOperatorManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get all employees.
         * @return {IAsyncResult<Entities.Employee[]>} The async result.
         */
        public getEmployeesAsync(): IAsyncResult<Entities.Employee[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees().read();
            return request.execute<Entities.Employee[]>();
        }

        /**
         * Get the details for a given employee identifier.
         * @param {number} employeeId The employee identifier.
         * @return {IAsyncResult<Entities.Employee>} The async result.
         */
        public getEmployeeAsync(employeeId: string): IAsyncResult<Entities.Employee> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees(employeeId).read();
            return request.execute<Entities.Employee>();
        }

        /**
         * Get the currently authenticated employee.
         * @return {IAsyncResult<Entities.Employee>} The async result.
         */
        public getCurrentEmployeeAsync(): IAsyncResult<Entities.Employee> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees().getCurrentEmployee();
            return request.execute<Entities.Employee>();
        }
    }
}
