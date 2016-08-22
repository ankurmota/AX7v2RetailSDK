/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {

    /**
     * Type of stock count operation types.
     */
    export enum StockCountOperationType {
        /**
         * The operation type is unknown.
         */
        Unknown = 0,

        /**
         * Create a stock count journal.
         */
        Create = 1,

        /**
         * Update a stock count journal.
         */
        Update = 2,

        /**
         * Commit a stock count journal.
         */
        Commit = 3,

        /**
         * Delete a stock count journal.
         */
        Delete = 4,

        /**
         * Get all of stock count journals.
         */
        GetAll = 5,

        /**
         * Get details of a particular stock count journal.
         */
        GetDetails = 6,

        /**
         * Synchronize all stock count journals.
         */
        SyncAll = 7,

        /**
         * Synchronize one stock count journal.
         */
        SyncOne = 8,

        /**
         * Remove a product line from a journal.
         */
        RemoveProductLine = 9
    }
}