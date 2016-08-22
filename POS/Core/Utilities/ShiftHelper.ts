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
    export class ShiftHelper {
        /**
         * Saves the cash drawer with the given name on the application storage.
         * If the cash drawer cannot be found, clears the cache.
         *
         * @param {string} cashDrawerName The cash drawer name.
         */
        public static saveCashDrawerOnStorage(cashDrawerName: string): void {
            var drawer: Proxy.Entities.HardwareProfileCashDrawer = ArrayExtensions.firstOrUndefined(
                ApplicationContext.Instance.hardwareProfile.CashDrawers,
                (c: Proxy.Entities.HardwareProfileCashDrawer) => StringExtensions.compare(c.DeviceName, cashDrawerName) === 0);

            var drawerName: string = StringExtensions.EMPTY;
            var drawerType: string = Proxy.Entities.PeripheralType.None.toString();

            if (drawer) {
                drawerName = drawer.DeviceName;
                drawerType = drawer.DeviceTypeValue.toString();
            }

            ApplicationStorage.setItem(ApplicationStorageIDs.CASH_DRAWER_NAME, drawerName);
            ApplicationStorage.setItem(ApplicationStorageIDs.CASH_DRAWER_TYPE, drawerType);
        }
    }
}