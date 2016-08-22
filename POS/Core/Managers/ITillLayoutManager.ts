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

    
    export var ITillLayoutManagerName: string = "ITillLayoutManager";
    

    export interface ITillLayoutManager {
        /**
         * Get button grids.
         * @returns {IAsyncResult<Entities.ButtonGrid[]>} The async result.
         */
        getButtonGridsAsync(): IAsyncResult<Entities.ButtonGrid[]>;

        /**
         * Get the till layout.
         * @returns {IAsyncResult<Entities.TillLayout>} The async result.
         */
        getTillLayoutAsync(): IAsyncResult<Entities.TillLayout>;
    }
}
