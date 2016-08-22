/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Entities/CommerceTypes.g.ts'/>
///<reference path='Entities/IOperationsMap.ts'/>
///<reference path='Extensions/ObjectExtensions.ts'/>
///<reference path='Utilities/Dictionary.ts'/>

module Commerce {
    "use strict";

    /**
     * Extending tender type map with utility functions
     */
    export class TenderTypeMap extends Dictionary<Proxy.Entities.TenderType[]> {
        /**
         * Returns a mapping from the operation type identifier to the tender type.
         *
         * @param {Operations.RetailOperation} operationId The operation identifier.
         * @return {Proxy.Entities.TenderType} The tender type of the operation of null if one does not exist.
         */
        public getTenderTypeByOperationId(operationId: Operations.RetailOperation): Proxy.Entities.TenderType {
            if (ObjectExtensions.isNullOrUndefined(operationId)) {
                return null;
            }

            var tenderType: Proxy.Entities.TenderType = null;

            // for quick cash operation, we don't have a tender mapping
            if (operationId === Operations.RetailOperation.PayCashQuick) {
                // we should use same as cash
                operationId = Operations.RetailOperation.PayCash;
            }

            if (this.hasItem(operationId)) {
                tenderType = this.getItem(operationId)[0];
            } else {
                RetailLogger.coreTenderTypeMapOperationHasNoTenderType(operationId);
            }

            return tenderType;
        }

        /**
         * Returns a mapping from the tender type identifier to the tender type.
         *
         * @param {string} typeId The tender type identifier.
         * @return {Proxy.Entities.TenderType} The tender type or null if one does not exist.
         */
        public getTenderByTypeId(typeId: string): Proxy.Entities.TenderType {
            if (StringExtensions.isNullOrWhitespace(typeId)) {
                return null;
            }

            var tenderTypes: Proxy.Entities.TenderType[] = [];
            this.getItems().forEach((value: Proxy.Entities.TenderType[]) => {
                value.forEach((tenderType: Proxy.Entities.TenderType) => {
                    if (tenderType.TenderTypeId === typeId) {
                        tenderTypes.push(tenderType);
                    }
                });
            });

            // Tender type not found return null.
            if (tenderTypes.length < 1) {
                RetailLogger.coreTenderTypeMapTenderTypeNotFound(typeId);
                return null;
            }

            if (tenderTypes.length > 1) {
                RetailLogger.coreTenderTypeMapMultipleTendersOfSameType();
            }

            return tenderTypes[0];
        }
    }
}