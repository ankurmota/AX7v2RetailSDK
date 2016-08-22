/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/CommerceTypes.g.ts'/>
///<reference path='../../Entities/Layout.ts'/>
///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../RegularExpressionValidations.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../ITillLayoutManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class TillLayoutManager implements Commerce.Model.Managers.ITillLayoutManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get button grids.
         * @returns {IAsyncResult<Entities.ButtonGrid[]>} The async result.
         */
        public getButtonGridsAsync(): IAsyncResult<Entities.ButtonGrid[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getButtonGrids();
            return request.execute<Entities.ButtonGrid[]>();
        }

        /**
         * Get the till layout.
         * @returns {IAsyncResult<Entities.TillLayoutProxy>} The async result.
         */
        public getTillLayoutAsync(): IAsyncResult<Entities.TillLayoutProxy> {
            var tillLayout: Entities.TillLayout;
            var tillLayoutProxy: Entities.TillLayoutProxy;

            var asyncQueue: AsyncQueue = new AsyncQueue().enqueue((): IAsyncResult<any> => {
                var request: Common.IDataServiceRequest = this._commerceContext.orgUnits().getTillLayout();
                return request.execute<Entities.TillLayout>().done((result: Entities.TillLayout) => {
                    tillLayout = result;
                });
            }).enqueue((): IAsyncResult<any> => {
                tillLayoutProxy = new Entities.TillLayoutProxy(tillLayout);

                // If zones were initialized, try get button grids.
                if (ArrayExtensions.hasElements(tillLayoutProxy.getButtonGridZones())) {
                    return this.getButtonGridsAsync().done((buttonGrids: Entities.ButtonGrid[]) => {
                        tillLayoutProxy.setButtonGrids(buttonGrids);
                    });
                }

                return VoidAsyncResult.createResolved();
            });

            return asyncQueue.run().map((result: ICancelableResult): Entities.TillLayoutProxy => { return tillLayoutProxy; });
        }
    }
}
