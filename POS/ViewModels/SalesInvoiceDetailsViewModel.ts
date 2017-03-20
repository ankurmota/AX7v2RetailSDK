/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Entities = Proxy.Entities;

    /**
     * Class containing the cart line data and related product information for UI binding.
     */
    export class CartLineForDisplay extends Entities.CartLineClass {
        private _product: Entities.Product;

        /**
         * Creates a new instance of the CartLineForDisplay class.
         * @param {Entities.CartLine} cartLine The cart line.
         * @param {Entities.Product} product The product associated with the cart line.
         */
        constructor(cartLine: Entities.CartLine, product: Entities.Product) {
            super(cartLine);

            this._product = product;
        }

        /**
         * Property exposing the name of the product.
         * @return {string} The name of the product.
         */
        public get ProductName(): string {
            return ObjectExtensions.isNullOrUndefined(this._product) ? StringExtensions.EMPTY : this._product.ProductName;
        }
    }

    /**
     * Represents the sales invoice details view model.
     */
    export class SalesInvoiceDetailsViewModel extends ViewModelBase {
        // Binding Properties
        private _cart: Observable<Entities.Cart>;
        private _cartLinesForDisplay: ObservableArray<CartLineForDisplay>;
        private _selectedCartLines: ObservableArray<CartLineForDisplay>;
        private _returnSalesInvoiceDisabled: Computed<boolean>;

        // Event Handlers
        private _handleSelection: (selectedLines: CartLineForDisplay[]) => void;

        // Internal State
        private _productsInCart: Dictionary<Entities.Product>;

        constructor() {
            super();
            this._cart = ko.observable<Entities.Cart>(new Entities.CartClass({ CartLines: [] }));
            this._cartLinesForDisplay = ko.observableArray<CartLineForDisplay>([]);
            this._selectedCartLines = ko.observableArray<CartLineForDisplay>([]);
            this._handleSelection = this.onSelectionChanged.bind(this);
            this._returnSalesInvoiceDisabled = ko.computed(() => {
                return !Commerce.ArrayExtensions.hasElements(this._selectedCartLines());
            }, this);

            this._productsInCart = new Dictionary<Entities.Product>();
        }

        /**
         * Gets a cart by invoice identifier.
         *
         * @param {string} invoiceId The sales invoice identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        public recallCartByInvoiceId(invoiceId: string): IVoidAsyncResult {
            var cart: Entities.Cart;
            var asyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    return this.cartManager.recallSalesInvoice(invoiceId)
                        .done((result) => { cart = result; });
                }).enqueue(() => {
                    var productIdsNotInCart = cart.CartLines.map(c => c.ProductId);

                    if (!ArrayExtensions.hasElements(productIdsNotInCart)) {
                        return VoidAsyncResult.createResolved();
                    }

                    return this.productManager.getProductDetailsAsync(productIdsNotInCart)
                        .done((products: Entities.Product[]) => {
                            products.forEach((p: Entities.Product): void => {
                                // caches by record identifier
                                this._productsInCart.setItem(p.RecordId, p);

                                // caches variants, if any
                                if (p.IsMasterProduct) {
                                    var variants = p.CompositionInformation.VariantInformation.Variants;
                                    variants.forEach((v: Entities.ProductVariant): void => {
                                        this._productsInCart.setItem(v.DistinctProductVariantId, p);
                                    });
                                }
                            });
                        });
                });

            return asyncQueue.run().done((): void => {
                this._cart(cart);
                var displayLines: CartLineForDisplay[] = cart.CartLines.map((cartLine: Entities.CartLine): CartLineForDisplay => {
                    var product: Entities.Product = this._productsInCart.getItem(cartLine.ProductId);
                    return new CartLineForDisplay(cartLine, product);
                });

                this._cartLinesForDisplay(displayLines);
            });
        }

        //DEMO4 NEW //AM 
        //Create and return new async queue and update cart lines with new price for Invoiced orders
        private priceOverrideAsyncQueue(cartLines: Entities.CartLine[], salesOrderStatus: number,hasDiscounts:boolean): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (salesOrderStatus === 4 && hasDiscounts) {
                cartLines.forEach((cartLine: Entities.CartLine) => {
                    let price: number = 0;
                    switch (cartLine.ItemId) {
                    case "0003":
                        price = 79.20; //TODO: Change this per ENV
                        break;
                    case "0005":
                        price = 5.39; //TODO: Change this per ENV
                        break;
                    case "0006":
                        price = 4.49; //TODO: Change this per ENV
                        break;
                    case "0009":
                        price = 32.40; //TODO: Change this per ENV
                        break;
                    case "0021":
                        price = 359.10; //TODO: Change this per ENV
                        break;
                    case "0004":
                        price = 809.10; //TODO: Change this per ENV
                        break;
                    }
                    if (price !== 0) {
                        cartLine.Price = price;
                    }
                    asyncQueue.enqueue(() => {
                        var query: Proxy.CartsDataServiceQuery = this.cartManager
                            .getCartByCartIdForKitAsync(Session.instance.cart.Id);

                        return query.overrideCartLinePrice(cartLine.LineId,
                                cartLine.Price)
                            .execute<Entities.Cart>()
                            .done((updatedCart: Entities.Cart): void => {
                                Commerce.Session.instance.cart = updatedCart;
                            });
                    });
                });


            };
            return asyncQueue;
        }

        //DEMO4 END

        /**
         * Returns the selected cart lines.
         *
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public returnCartLines(salesOrderStatus?:number): IAsyncResult<ICancelableResult> {
            // copy the cart, in order to keep the sales invoice details
            Session.instance.cart = new Entities.CartClass(this._cart());
            //DEMO4 Ankur start
            let hasDiscounts: boolean = false;
            var properties = this._cart().ExtensionProperties.filter((property) => {
                return property.Key === "HasReturns";
            });
            if (ArrayExtensions.hasElements(properties)) {
                let hasReturnProperty = properties[0];

                if (hasReturnProperty.Value) {
                    hasDiscounts = hasReturnProperty.Value.BooleanValue;
                }
            }

            var asyncQueue = this.priceOverrideAsyncQueue(this._selectedCartLines(),salesOrderStatus,hasDiscounts);//new AsyncQueue();
            //var asyncQueue = new AsyncQueue();
            //DEMO4 END
            
            asyncQueue
                .enqueue(() => {
                    // select lines not being returned
                    var nonReturnedLines: Entities.CartLine[] = Session.instance.cart.CartLines.filter(
                        cartLine => !this._selectedCartLines().some(c => c.LineId === cartLine.LineId));

                    // lines not being returned must have their quantity set to zero
                    nonReturnedLines = nonReturnedLines.map(c => {
                        return <Entities.CartLine>{ LineId: c.LineId, Quantity: 0 };
                    });

                    if (ArrayExtensions.hasElements(nonReturnedLines)) {
                        return this.cartManager.updateCartLinesOnCartAsync(nonReturnedLines);
                    }

                    return null;
                }).enqueue(() => {
                    // return selected lines
                    var options: Operations.IReturnProductOperationOptions = {
                        customerId: Session.instance.cart.CustomerId,
                        productReturnDetails: this._selectedCartLines()
                            .map((cartLineForDisplay: CartLineForDisplay): Entities.ProductReturnDetails => {
                                var cartLineForDisplayAsCartLine: Entities.CartLine =
                                    cartLineForDisplay;

                                //Call cart manager to update this
                                return <Entities.ProductReturnDetails>
                                    { cartLine: cartLineForDisplayAsCartLine };
                            })
                    };

                    var operationResult = this.operationsManager.runOperation(
                        Operations.RetailOperation.ReturnItem,
                        options);

                    return asyncQueue.cancelOn(operationResult);
                });

            return asyncQueue.run()
                .done((result) => {
                    if (result.canceled) {
                        // clears cart if canceled
                        Session.instance.clearCart();
                    } else {
                        this._cart(Session.instance.cart);
                    }
                }).fail((errors) => {
                    // clears cart if an error occurred
                    Session.instance.clearCart();
                });
        }

        /**
         * Cart line selection change handler.
         * @param {CartLineForDisplay[]} selectedLines The currently selected cart lines.
         */
        private onSelectionChanged(selectedLines: CartLineForDisplay[]): void {
            this._selectedCartLines(selectedLines);
        }
    }
}