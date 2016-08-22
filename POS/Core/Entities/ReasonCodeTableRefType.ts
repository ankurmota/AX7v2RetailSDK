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
     * Table reference type. Maps to RBOInfocodeRefTAbleIdBase enum in AX.
     */
    export enum ReasonCodeTableRefType {
        /**
         * No type = 0.
         */
        None = 0,

        /**
         * INVENTTABLE = 1.
         */
        Item = 1,

        /**
         * RETAILCUSTTABLE = 2.
         */
        Customer = 2,

        /**
         * RETAILSTORETENDERTYPETABLE = 3.
         */
        Tender = 3,

        /**
         * RETAILSTORETENDERTYPECARDTABLE = 4.
         */
        CreditCard = 4,

        /**
         * RETAILINCOMEEXPENSEACCOUNTTABLE = 5.
         */
        IncomeExpense = 5,

        /**
         * RETAILINVENTITEMDEPARTMENT = 6.
         */
        ItemDepartment = 6,

        /**
         * RboInventItemGroup = 7.
         */
        ItemGroup = 7,

        /**
         * RBOHierarchyTable = 8.
         */
        HierarchyTable = 8,

        /**
         * InventTable = 9.
         */
        InventTable = 9,

        /**
         * RetailAffiliation = 10.
         */
        Affiliation = 10
    }
}
