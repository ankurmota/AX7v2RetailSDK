/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IOperatorManagerName: string = "IOperatorManager";
    

    export interface IOperatorManager {
        /**
         * Get all employees.
         * @return {IAsyncResult<Entities.Employee[]>} The async result.
         */
        getEmployeesAsync(): IAsyncResult<Entities.Employee[]>;

        /**
         * Get the details for a given employee identifier.
         * @param {number} employeeId The employee identifier.
         * @return {IAsyncResult<Entities.Employee>} The async result.
         */
        getEmployeeAsync(employeeId: string): IAsyncResult<Entities.Employee>;

        /**
         * Get the currently authenticated employee.
         * @return {IAsyncResult<Entities.Employee>} The async result.
         */
        getCurrentEmployeeAsync(): IAsyncResult<Entities.Employee>;
    }
}
