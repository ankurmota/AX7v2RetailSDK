/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/ReasonCodeTableRefType.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='../Core.d.ts'/>

module Commerce {
    "use strict";

    type ReasonCodeDictionary = { [index: string]: Proxy.Entities.ReasonCode };
    type StringDictionary = { [index: string]: string };

    /**
     * The context where to get and add reason code lines.
     */
    
    export interface ReasonCodesContext {
        cart?: Proxy.Entities.Cart;
        cartLines?: Proxy.Entities.CartLine[];
        tenderLines?: Proxy.Entities.TenderLine[];
        affiliationLines?: Proxy.Entities.AffiliationLoyaltyTier[];
        nonSalesTransaction?: Proxy.Entities.NonSalesTransaction;
        dropAndDeclareTransaction?: Proxy.Entities.DropAndDeclareTransaction;
    }

    /**
     * Async action which can cause required reason codes to be asked for.
     */
    export interface RequiredReasonCodesAsyncAction {
        (context: ReasonCodesContext): IVoidAsyncResult;
    }

    /**
     * Interface used internally to define which reason codes were processed and what reason code lines were added.
     */
    interface ReasonCodesInternalContext extends ReasonCodesContext {
        processedReasonCodes: Proxy.Entities.ReasonCode[];
        addedReasonCodeLines: Proxy.Entities.ReasonCodeLine[];
    }
    

    /**
     * Indicates the target context where the reason code line will be stored.
     */
    enum TargetContext {
        Unknown,
        Cart,
        CartLine,
        DropAndDeclareTransaction,
        NonSalesTransaction
    }

    /**
     * This class is used to help get the required reason codes from an async error or from a reason code identifier.
     */
    export class ReasonCodesHelper {
        /**
         * Gets the required reason codes lines from the given reason code identifier, by getting them and adding on the appropriate context.
         * Afterwards, handles the async action for required reason codes, by getting them,
         * adding on the appropriate context and retrying the action, until no more reason codes are required.
         * @param {ReasonCodesContext} context The context to add reason codes to.
         * @param {RequiredReasonCodesAsyncAction} asyncAction The async action.
         * @param {Proxy.Entities.ReasonCodeSourceType} [sourceType] The optional reason code source type, used for functionality profile reason codes.
         * @param {boolean} [isManualReturn] The optional flag used for returns and checked against customer orders.
         * @return {AsyncQueue} The async queue.
         */
        public static handleRequiredReasonCodesAsyncQueue(
            context: ReasonCodesContext,
            asyncAction: RequiredReasonCodesAsyncAction,
            sourceType: Proxy.Entities.ReasonCodeSourceType = undefined,
            isManualReturn: boolean = false): AsyncQueue {

            var internalContext: ReasonCodesInternalContext = <ReasonCodesInternalContext>context;
            internalContext.processedReasonCodes = [];
            internalContext.addedReasonCodeLines = [];

            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (!ObjectExtensions.isNullOrUndefined(sourceType)) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    var reasonCodeQueue: AsyncQueue = ReasonCodesHelper.getFunctionalityProfileReasonCodesAsyncQueue(
                        internalContext, sourceType, isManualReturn);
                    return asyncQueue.cancelOn(reasonCodeQueue.run());
                });
            }

            if (!ObjectExtensions.isNullOrUndefined(asyncAction)) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.getReasonCodesFromAsyncActionAsyncQueue(internalContext, asyncAction).run();
                    return asyncQueue.cancelOn(result);
                });
            }

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleTriggeredReasonSubCodesAsyncQueue(internalContext).run();

                // clear internal state
                delete internalContext.addedReasonCodeLines;
                delete internalContext.processedReasonCodes;

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue;
        }

        /**
         * Calls the async action and get required reason codes, if any. Then, re-tries the async action, until not more reason codes are required.
         * @param {ReasonCodesInternalContext} context The context to add reason codes to.
         * @param {RequiredReasonCodesAsyncAction} asyncAction The async action.
         * @return {AsyncQueue} The async queue.
         */
        private static getReasonCodesFromAsyncActionAsyncQueue(
            context: ReasonCodesInternalContext,
            asyncAction: RequiredReasonCodesAsyncAction): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();
            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var result: IVoidAsyncResult = asyncAction(context);
                if (ObjectExtensions.isNullOrUndefined(result)) {
                    return VoidAsyncResult.createResolved();
                }

                return result.recoverOnFailure((errors: Proxy.Entities.Error[]): IVoidAsyncResult => {
                    // we get reason codes and retry the async action
                    var retryQueueResult: IAsyncResult<ICancelableResult> = ReasonCodesHelper.retryOnRequiredReasonCodesAsyncQueue(
                        ArrayExtensions.firstOrUndefined(errors), context,
                        () => { return ReasonCodesHelper.getReasonCodesFromAsyncActionAsyncQueue(context, asyncAction); }).run();

                    return asyncQueue.cancelOn(retryQueueResult);
                });
            });
        }

        /**
         * Creates an async queue for handling required reason codes reported on the error.
         * After getting all required reason codes, it calls the retry function.
         * If the error does not have any reason code requirement, it fails the async queue with the original error.
         * @param {Proxy.Entities.Error} error The error to get reason code requirements from.
         * @param {ReasonCodesContext} context The context to add reason codes to.
         * @param {() => AsyncQueue} retryFunction The function to be called after the reason codes are selected.
         * @return {AsyncQueue} The async queue.
         */
        private static retryOnRequiredReasonCodesAsyncQueue(
            error: Proxy.Entities.Error,
            context: ReasonCodesInternalContext,
            retryFunction: () => AsyncQueue): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();
            return asyncQueue.enqueue((): IAsyncResult<any> => {
                // get required reason codes
                var reasonCodesQueue: AsyncQueue = ReasonCodesHelper.getReasonCodesFromErrorAsyncQueue(error, context);
                return asyncQueue.cancelOn(reasonCodesQueue.run());
            }).enqueue((): IAsyncResult<any> => {
                // retry
                if (retryFunction) {
                    var retryQueue: AsyncQueue = retryFunction();
                    return asyncQueue.cancelOn(retryQueue.run());
                }

                return null;
            });
        }

        /**
         * Creates an async queue with the business logic of how to:
         * 1. Get a reason code information from the error;
         * 2. Ask for reason codes for a cart, cart lines and tender lines.
         * @param {Proxy.Entities.Error} error The error to get reason codes from.
         * @param {ReasonCodesContext} context The context to add reason codes to.
         * @return {AsyncQueue} The async queue that must be executed in order to get proper reason codes from the error, if any required.
         * @remarks If no reason code is required, this functions returns a failed queue with the original error.
         */
        private static getReasonCodesFromErrorAsyncQueue(error: Proxy.Entities.Error, context: ReasonCodesInternalContext): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            // rejects the error if the commerce exception cannot be handled here
            if (ObjectExtensions.isNullOrUndefined(error.commerceException)
                || !(error.commerceException instanceof Proxy.Entities.MissingRequiredReasonCodeExceptionClass)) {

                return asyncQueue.enqueue((): IVoidAsyncResult => {
                    return VoidAsyncResult.createRejected([error]);
                });
            }

            var missingRequiredReasonCodeException: Proxy.Entities.MissingRequiredReasonCodeException
                = <Proxy.Entities.MissingRequiredReasonCodeException>error.commerceException;
            var requiredReasonCodes: Proxy.Entities.ReasonCode[] = missingRequiredReasonCodeException.RequiredReasonCodes;
            var reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[] = missingRequiredReasonCodeException.ReasonCodeRequirements;
            var requiredReasonCodeIds: string[] = missingRequiredReasonCodeException.TransactionRequiredReasonCodeIds;

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var reasonCodeQueue: AsyncQueue = ReasonCodesHelper.getReasonCodesFromRequirementsAsyncQueue(
                    context, requiredReasonCodes, requiredReasonCodeIds, reasonCodeRequirements);
                return asyncQueue.cancelOn(reasonCodeQueue.run());
            });
        }

        /**
         * Creates an async queue with the business logic of how to ask for reason codes for a cart, cart lines and tender lines.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The required reason codes returned from server.
         * @param {string[]} requiredReasonCodeIds The required reason code identifiers.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} reasonCodeRequirements The reason code requirements.
         * @param {ReasonCodesContext} context The context to add reason codes to.
         * @return {AsyncQueue} The async queue that must be executed in order to get proper reason codes from the requirements, if any required.
         */
        private static getReasonCodesFromRequirementsAsyncQueue(
            context: ReasonCodesInternalContext,
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            requiredReasonCodeIds: string[],
            reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[]): AsyncQueue {

            var reasonCodeQueue: AsyncQueue = new AsyncQueue();
            return reasonCodeQueue.enqueue((): IAsyncResult<any> => {
                var reasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.getRequiredReasonCodesForCart(
                    requiredReasonCodes, requiredReasonCodeIds, context.cart);
                var cartQueue: AsyncQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(
                    context, reasonCodes, true, { reasonCodes: null, cart: context.cart });
                return reasonCodeQueue.cancelOn(cartQueue.run());
            }).enqueue((): IAsyncResult<any> => {
                var cartLineQueue: AsyncQueue = ReasonCodesHelper.getRequiredReasonCodesForCartLinesAsyncQueue(
                    context, requiredReasonCodes, reasonCodeRequirements, context.cartLines);
                return reasonCodeQueue.cancelOn(cartLineQueue.run());
            }).enqueue((): IAsyncResult<any> => {
                var tenderLineQueue: AsyncQueue = ReasonCodesHelper.getRequiredReasonCodesForTenderLinesAsyncQueue(
                    context, requiredReasonCodes, reasonCodeRequirements, context.tenderLines);
                return reasonCodeQueue.cancelOn(tenderLineQueue.run());
            }).enqueue((): IAsyncResult<any> => {
                var affiliationLineQueue: AsyncQueue = ReasonCodesHelper.getRequiredReasonCodesForAffiliationLinesAsyncQueue(
                    context, requiredReasonCodes, reasonCodeRequirements, context.affiliationLines);
                return reasonCodeQueue.cancelOn(affiliationLineQueue.run());
            });
        }

        /**
         * Creates an async queue with the business logic of how to get required reason code lines for the cart lines.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The required reason codes.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} reasonCodeRequirements The reason code requirements.
         * @param {Proxy.Entities.CartLine[]} cartLines The cart lines to add the reason code lines to.
         * @return {AsyncQueue} The async queue that must be executed in order to get proper reason codes from the requirements, if any required.
         */
        private static getRequiredReasonCodesForCartLinesAsyncQueue(
            context: ReasonCodesInternalContext,
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[],
            cartLines: Proxy.Entities.CartLine[]): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!ArrayExtensions.hasElements(cartLines)) {
                return asyncQueue;
            }

            // for each cart line, if there are reason codes to get, get them and add to the cart line.
            cartLines.forEach((cartLine: Proxy.Entities.CartLine) => {
                var reasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.getRequiredReasonCodesForCartLine(
                    requiredReasonCodes, reasonCodeRequirements, cartLine);
                if (ArrayExtensions.hasElements(reasonCodes)) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        var reasonCodeQueue: AsyncQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(
                            context, reasonCodes, true, { reasonCodes: null, cartLine: cartLine });
                        return asyncQueue.cancelOn(reasonCodeQueue.run());
                    });
                }
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue with the business logic of how to get required reason code lines for the tender lines.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The required reason codes.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} reasonCodeRequirements The reason code requirements.
         * @param {Proxy.Entities.TenderLine[]} tenderLines The tender lines to add the reason code lines to.
         * @return {AsyncQueue} The async queue that must be executed in order to get proper reason codes from the requirements, if any required.
         */
        private static getRequiredReasonCodesForTenderLinesAsyncQueue(
            context: ReasonCodesInternalContext,
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[],
            tenderLines: Proxy.Entities.TenderLine[]): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!ArrayExtensions.hasElements(tenderLines)) {
                return asyncQueue;
            }

            // for each tender line, if there are reason codes to get, get them and add to the tender line.
            tenderLines.forEach((tenderLine: Proxy.Entities.TenderLine) => {
                var reasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.getRequiredReasonCodesForTenderLine(
                    requiredReasonCodes, reasonCodeRequirements, tenderLine);
                if (ArrayExtensions.hasElements(reasonCodes)) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        var reasonCodeQueue: AsyncQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(
                            context, reasonCodes, true, { reasonCodes: null, tenderLine: tenderLine });
                        return asyncQueue.cancelOn(reasonCodeQueue.run());
                    });
                }
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue with the business logic of how to get required reason code lines for the affiliation lines.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The required reason codes.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} reasonCodeRequirements The reason code requirements.
         * @param {Proxy.Entities.AffiliationLoyaltyTier[]} affiliationLines The affiliation lines to add the reason code lines to.
         * @return {AsyncQueue} The async queue that must be executed in order to get proper reason codes from the requirements, if any required.
         */
        private static getRequiredReasonCodesForAffiliationLinesAsyncQueue(
            context: ReasonCodesInternalContext,
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[],
            affiliationLines: Proxy.Entities.AffiliationLoyaltyTier[]): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!ArrayExtensions.hasElements(affiliationLines)) {
                return asyncQueue;
            }

            // for each affiliation line, if there are reason codes to get, get them and add to the affiliation line.
            affiliationLines.forEach((affiliationLine: Proxy.Entities.AffiliationLoyaltyTier) => {
                var reasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.getRequiredReasonCodesForAffiliationLine(
                    requiredReasonCodes, reasonCodeRequirements, affiliationLine);
                if (ArrayExtensions.hasElements(reasonCodes)) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        var reasonCodeQueue: AsyncQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(
                            context, reasonCodes, true, { reasonCodes: null, affiliationLine: affiliationLine });
                        return asyncQueue.cancelOn(reasonCodeQueue.run());
                    });
                }
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue with the business logic of getting reason code lines out of reason codes for the given context.
         * @param {ReasonCodesContext} context The context to get reason codes for.
         * @param {Proxy.Entities.ReasonCodeSourceType} [sourceType] The optional reason code source type, used for functionality profile reason codes.
         * @param {boolean} [isManualReturn] The optional flag used for returns and checked against customer orders.
         * @return {AsyncQueue} The async queue that must be executed in order to get reason code lines out of reason codes.
         */
        private static getFunctionalityProfileReasonCodesAsyncQueue(
            context: ReasonCodesInternalContext,
            sourceType: Proxy.Entities.ReasonCodeSourceType,
            isManualReturn: boolean = false): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var reasonCodeId: string = ReasonCodesHelper.getReasonCodeIdFromSourceType(sourceType);
            var targetContext: TargetContext = ReasonCodesHelper.getTargetContextFromSourceType(sourceType);

            if (StringExtensions.isNullOrWhitespace(reasonCodeId)
                || (targetContext === TargetContext.Cart && ObjectExtensions.isNullOrUndefined(context.cart))
                || (targetContext === TargetContext.CartLine && !ArrayExtensions.hasElements(context.cartLines))) {
                return asyncQueue;
            }

            var reasonCodes: Proxy.Entities.ReasonCode[] = [];

            // checks for reason codes
            asyncQueue.enqueue((): IAsyncResult<any> => {
                // customer orders have a special case
                if (Proxy.Entities.CartType.CustomerOrder === Session.instance.cart.CartTypeValue && isManualReturn) {
                    return ApplicationContext.Instance.returnOrderReasonCodesAsCompositeSubcodesAsync
                        .done((reasonCode: Proxy.Entities.ReasonCode) => { reasonCodes = [reasonCode]; });
                }

                var salesOrderManager: Model.Managers.ISalesOrderManager = Model.Managers.Factory.GetManager(Model.Managers.ISalesOrderManagerName, null);
                return salesOrderManager.getReasonCodesByIdAsync(reasonCodeId)
                    .done((result: Proxy.Entities.ReasonCode[]) => { reasonCodes = result; });
            });

            // get reason code lines
            asyncQueue.enqueue((): IAsyncResult<any> => {
                var reasonCodesQueue: AsyncQueue;
                if (targetContext === TargetContext.Cart) {
                    reasonCodesQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(context, reasonCodes, false, { reasonCodes: null, cart: context.cart });
                } else if (targetContext === TargetContext.CartLine) {
                    reasonCodesQueue = new AsyncQueue();
                    context.cartLines.forEach((line: Proxy.Entities.CartLine) => {
                        reasonCodesQueue.enqueue((): IAsyncResult<any> => {
                            var cartLineQueue: AsyncQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(
                                context, reasonCodes, false, { reasonCodes: null, cartLine: line });
                            return reasonCodesQueue.cancelOn(cartLineQueue.run());
                        });
                    });
                } else if (targetContext === TargetContext.DropAndDeclareTransaction) {
                    reasonCodesQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(
                        context, reasonCodes, false, { reasonCodes: null, dropAndDeclareTransaction: context.dropAndDeclareTransaction });
                } else if (targetContext === TargetContext.NonSalesTransaction) {
                    reasonCodesQueue = ReasonCodesHelper.getReasonCodeLinesAsyncQueue(
                        context, reasonCodes, false, { reasonCodes: null, nonSalesTransaction: context.nonSalesTransaction });
                } else {
                    reasonCodesQueue = new AsyncQueue();
                }

                return asyncQueue.cancelOn(reasonCodesQueue.run());
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue with the business logic of getting reason code lines out of reason codes.
         * @param {Proxy.Entities.ReasonCode[]} reasonCodes The collection of reason codes to get reason code lines out of.
         * @param {boolean} skipFrequencyTest If the reason code should always be asked for.
         * @param {Proxy.Entities.Cart} [cartContext] The optional cart context.
         * @param {Proxy.Entities.CartLine} [cartLineContext] The optional cart line context.
         * @param {Proxy.Entities.TenderLine} [tenderLineContext] The optional tender line context.
         * @param {Proxy.Entities.AffiliationLoyaltyTier} [affiliationLineContext] The optional affiliation line context.
         * @param {Proxy.Entities.NonSalesTransaction} [nonSalesTransactionContext] The optional non sales transaction (Open Drawer) context.
         * @return {AsyncQueue} The async queue that must be executed in order to get reason code lines out of reason codes.
         */
        private static getReasonCodeLinesAsyncQueue(
            context: ReasonCodesInternalContext,
            reasonCodes: Proxy.Entities.ReasonCode[],
            skipFrequencyTest: boolean,
            activityContext: Activities.GetReasonCodeLinesActivityContext): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var processedReasonCodes: Proxy.Entities.ReasonCode[] = [];

            // gets linked reason codes
            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var reasonCodeIds: StringDictionary = Object.create(null);
                return ReasonCodesHelper.getLinkedReasonCodesForArrayQueue(reasonCodeIds, processedReasonCodes, reasonCodes, skipFrequencyTest).run();
            }).enqueue((): IAsyncResult<any> => {
                processedReasonCodes = ArrayExtensions.distinct(processedReasonCodes,
                    (r1: Proxy.Entities.ReasonCode, r2: Proxy.Entities.ReasonCode): boolean => r1.ReasonCodeId === r2.ReasonCodeId);

                if (activityContext.cart && activityContext.cart.Id !== Session.instance.cart.Id) {
                    activityContext.reasonCodes = processedReasonCodes;
                } else {
                    activityContext.reasonCodes = processedReasonCodes.filter(
                        (r: Proxy.Entities.ReasonCode) => !(r.OncePerTransaction && ReasonCodesHelper.cartContainsReasonCode(r)));
                }

                if (!ArrayExtensions.hasElements(activityContext.reasonCodes)) {
                    return VoidAsyncResult.createResolved();
                }

                var activity: Activities.GetReasonCodeLinesActivity = new Activities.GetReasonCodeLinesActivity(activityContext);
                return activity.execute().done((): void => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }

                    var reasonCodesContainer: Proxy.Entities.Cart | Proxy.Entities.CartLine | Proxy.Entities.TenderLine | Proxy.Entities.AffiliationLoyaltyTier;
                    reasonCodesContainer = activity.context.cart ? activity.context.cart
                        : activity.context.cartLine ? activity.context.cartLine
                        : activity.context.tenderLine ? activity.context.tenderLine
                        : activity.context.affiliationLine ? activity.context.affiliationLine
                        : activity.context.nonSalesTransaction ? activity.context.nonSalesTransaction
                        : activity.context.dropAndDeclareTransaction ? activity.context.dropAndDeclareTransaction
                        : null;

                    var addedReasonCodeLines: Proxy.Entities.ReasonCodeLine[] = activity.response.reasonCodeLines;

                    if (reasonCodesContainer) {
                        reasonCodesContainer.ReasonCodeLines = reasonCodesContainer.ReasonCodeLines || [];
                        reasonCodesContainer.ReasonCodeLines = reasonCodesContainer.ReasonCodeLines.concat(addedReasonCodeLines);

                        context.processedReasonCodes = context.processedReasonCodes.concat(processedReasonCodes);
                        context.addedReasonCodeLines = context.addedReasonCodeLines.concat(addedReasonCodeLines);
                    }
                });
            });
        }

        /**
         * Verifies if the session cart contains the reason code.
         * @param {Proxy.Entities.ReasonCode} reasonCode The reason code.
         * @return {boolean} True if the reason code is present on the cart, false otherwise.
         */
        private static cartContainsReasonCode(reasonCode: Proxy.Entities.ReasonCode): boolean {
            var cart: Proxy.Entities.Cart = Session.instance.cart;
            var containsReasonCode: boolean = false;

            if (ArrayExtensions.hasElements(cart.ReasonCodeLines)) {
                containsReasonCode = cart.ReasonCodeLines.some((rcl: Proxy.Entities.ReasonCodeLine) => rcl.ReasonCodeId === reasonCode.ReasonCodeId);
            }

            if (!containsReasonCode && ArrayExtensions.hasElements(cart.AffiliationLines)) {
                containsReasonCode = cart.AffiliationLines
                    .filter((a: Proxy.Entities.SalesAffiliationLoyaltyTier) => ArrayExtensions.hasElements(a.ReasonCodeLines))
                    .some((a: Proxy.Entities.SalesAffiliationLoyaltyTier) => {
                        return a.ReasonCodeLines.some((rcl: Proxy.Entities.ReasonCodeLine) => rcl.ReasonCodeId === reasonCode.ReasonCodeId);
                    });
            }

            if (!containsReasonCode && ArrayExtensions.hasElements(cart.CartLines)) {
                containsReasonCode = cart.CartLines.filter((c: Proxy.Entities.CartLine) => ArrayExtensions.hasElements(c.ReasonCodeLines))
                    .some((c: Proxy.Entities.CartLine) => {
                        return c.ReasonCodeLines.some((rcl: Proxy.Entities.ReasonCodeLine) => rcl.ReasonCodeId === reasonCode.ReasonCodeId);
                    });
            }

            if (!containsReasonCode && ArrayExtensions.hasElements(cart.TenderLines)) {
                containsReasonCode = cart.TenderLines.filter((t: Proxy.Entities.TenderLine) => ArrayExtensions.hasElements(t.ReasonCodeLines))
                    .some((t: Proxy.Entities.TenderLine) => {
                        return t.ReasonCodeLines.some((rcl: Proxy.Entities.ReasonCodeLine) => rcl.ReasonCodeId === reasonCode.ReasonCodeId);
                    });
            }

            return containsReasonCode;
        }

        /**
         * Gets the functionality profile reason code identifier associated with source type.
         * @param {Proxy.Entities.ReasonCodeSourceType} sourceType The source type.
         * @return {string} The reason code identifier associated with the given source type.
         */
        private static getReasonCodeIdFromSourceType(sourceType: Proxy.Entities.ReasonCodeSourceType): string {
            var deviceConfiguration: Proxy.Entities.DeviceConfiguration = ApplicationContext.Instance.deviceConfiguration;
            if (ObjectExtensions.isNullOrUndefined(deviceConfiguration)) {
                return undefined;
            }

            switch (sourceType) {
                case Proxy.Entities.ReasonCodeSourceType.AddSalesperson:
                    return deviceConfiguration.SalesPerson;
                case Proxy.Entities.ReasonCodeSourceType.EndOfTransaction:
                    return deviceConfiguration.EndOfTransaction;
                case Proxy.Entities.ReasonCodeSourceType.ItemDiscount:
                    return deviceConfiguration.ProductDiscount;
                case Proxy.Entities.ReasonCodeSourceType.ItemNotOnFile:
                    return deviceConfiguration.ItemNotOnFile;
                case Proxy.Entities.ReasonCodeSourceType.LineItemTaxChange:
                    return deviceConfiguration.LineItemTaxChange;
                case Proxy.Entities.ReasonCodeSourceType.Markup:
                    return deviceConfiguration.MarkUp;
                case Proxy.Entities.ReasonCodeSourceType.NegativeAdjustment:
                    return undefined;
                case Proxy.Entities.ReasonCodeSourceType.NfcEContingencyModeEnabled:
                    return deviceConfiguration.NfcEContingencyModeEnabled;
                case Proxy.Entities.ReasonCodeSourceType.NfcEVoided:
                    return deviceConfiguration.NfcEVoided;
                case Proxy.Entities.ReasonCodeSourceType.None:
                    return undefined;
                case Proxy.Entities.ReasonCodeSourceType.OpenDrawer:
                    return deviceConfiguration.OpenDrawer;
                case Proxy.Entities.ReasonCodeSourceType.OverridePrice:
                    return deviceConfiguration.OverridePrice;
                case Proxy.Entities.ReasonCodeSourceType.ReturnItem:
                    return deviceConfiguration.ReturnProduct;
                case Proxy.Entities.ReasonCodeSourceType.ReturnTransaction:
                    return deviceConfiguration.RefundSale;
                case Proxy.Entities.ReasonCodeSourceType.SerialNumber:
                    return deviceConfiguration.SerialNumber;
                case Proxy.Entities.ReasonCodeSourceType.StartOfTransaction:
                    return deviceConfiguration.StartOfTransaction;
                case Proxy.Entities.ReasonCodeSourceType.TenderDeclaration:
                    return deviceConfiguration.TenderDeclaration;
                case Proxy.Entities.ReasonCodeSourceType.TotalDiscount:
                    return deviceConfiguration.DiscountAtTotal;
                case Proxy.Entities.ReasonCodeSourceType.TransactionTaxChange:
                    return deviceConfiguration.TransactionTaxChange;
                case Proxy.Entities.ReasonCodeSourceType.VoidItem:
                    return deviceConfiguration.VoidItem;
                case Proxy.Entities.ReasonCodeSourceType.VoidPayment:
                    return deviceConfiguration.VoidPayment;
                case Proxy.Entities.ReasonCodeSourceType.VoidTransaction:
                    return deviceConfiguration.VoidTransaction;
                default:
                    return undefined;
            }
        }

        /**
         * Gets the target context associated with the given source type.
         * @param {Proxy.Entities.ReasonCodeSourceType} sourceType The source type.
         * @return {TargetContext} The target context associated with the given source type.
         */
        private static getTargetContextFromSourceType(sourceType: Proxy.Entities.ReasonCodeSourceType): TargetContext {
            switch (sourceType) {
                case Proxy.Entities.ReasonCodeSourceType.AddSalesperson:
                case Proxy.Entities.ReasonCodeSourceType.EndOfTransaction:
                case Proxy.Entities.ReasonCodeSourceType.ItemNotOnFile:
                case Proxy.Entities.ReasonCodeSourceType.NegativeAdjustment:
                case Proxy.Entities.ReasonCodeSourceType.StartOfTransaction:
                case Proxy.Entities.ReasonCodeSourceType.TotalDiscount:
                case Proxy.Entities.ReasonCodeSourceType.TransactionTaxChange:
                case Proxy.Entities.ReasonCodeSourceType.VoidPayment:
                case Proxy.Entities.ReasonCodeSourceType.VoidTransaction:
                    return TargetContext.Cart;
                case Proxy.Entities.ReasonCodeSourceType.ItemDiscount:
                case Proxy.Entities.ReasonCodeSourceType.LineItemTaxChange:
                case Proxy.Entities.ReasonCodeSourceType.Markup:
                case Proxy.Entities.ReasonCodeSourceType.OverridePrice:
                case Proxy.Entities.ReasonCodeSourceType.ReturnTransaction:
                case Proxy.Entities.ReasonCodeSourceType.ReturnItem:
                case Proxy.Entities.ReasonCodeSourceType.SerialNumber:
                case Proxy.Entities.ReasonCodeSourceType.VoidItem:
                    return TargetContext.CartLine;
                case Proxy.Entities.ReasonCodeSourceType.TenderDeclaration:
                    return TargetContext.DropAndDeclareTransaction;
                case Proxy.Entities.ReasonCodeSourceType.OpenDrawer:
                    return TargetContext.NonSalesTransaction;
                case Proxy.Entities.ReasonCodeSourceType.NfcEContingencyModeEnabled:
                case Proxy.Entities.ReasonCodeSourceType.NfcEVoided:
                case Proxy.Entities.ReasonCodeSourceType.None:
                    return TargetContext.Unknown;
                default:
                    return TargetContext.Unknown;
            }
        }

        /**
         * Gets the cart required reason codes from the requirements.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The required reason codes.
         * @param {string[]} requiredReasonCodeIds The required reason code identifiers.
         * @param {Proxy.Entities.Cart} cart The cart.
         * @return {Proxy.Entities.ReasonCode[]} The required reason codes.
         */
        private static getRequiredReasonCodesForCart(
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            requiredReasonCodeIds: string[],
            cart: Proxy.Entities.Cart): Proxy.Entities.ReasonCode[] {

            if (!ObjectExtensions.isNullOrUndefined(cart)) {
                var filteredRequiredReasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.filterReasonCodes(
                    requiredReasonCodes, requiredReasonCodeIds);
                return ReasonCodesHelper.filterUniqueReasonCodes(filteredRequiredReasonCodes, cart.ReasonCodeLines);
            }

            return [];
        }

        /**
         * Gets the cart line required reason codes from the requirements.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The reason codes returned from server.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} reasonCodeRequirements The reason codes requirements returned from server.
         * @param {Proxy.Entities.CartLine} cartLine The cart line.
         * @return {Proxy.Entities.ReasonCode[]} The required reason codes.
         */
        private static getRequiredReasonCodesForCartLine(
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[],
            cartLine: Proxy.Entities.CartLine): Proxy.Entities.ReasonCode[] {

            if (!ObjectExtensions.isNullOrUndefined(cartLine) && ArrayExtensions.hasElements(requiredReasonCodes)) {
                var reasonCodeIds: string[] = ReasonCodesHelper.getRequiredReasonCodeIds(
                    reasonCodeRequirements, cartLine.ProductId.toString(), Proxy.Entities.ReasonCodeTableRefType.Item);

                var filteredRequiredReasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.filterReasonCodes(
                    requiredReasonCodes, reasonCodeIds);
                return ReasonCodesHelper.filterUniqueReasonCodes(filteredRequiredReasonCodes, cartLine.ReasonCodeLines);
            }

            return [];
        }

        /**
         * Gets the tender line required reason codes from the requirements.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The required reason codes.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} reasonCodeRequirements The reason code requirements.
         * @param {Proxy.Entities.TenderLine} tenderLine The tender line.
         * @return {Proxy.Entities.ReasonCode[]} The required reason codes.
         */
        private static getRequiredReasonCodesForTenderLine(
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[],
            tenderLine: Proxy.Entities.TenderLine): Proxy.Entities.ReasonCode[] {

            if (!ObjectExtensions.isNullOrUndefined(tenderLine) && ArrayExtensions.hasElements(requiredReasonCodes)) {
                var reasonCodeIds: string[] = ReasonCodesHelper.getRequiredReasonCodeIds(
                    reasonCodeRequirements, tenderLine.TenderTypeId, Proxy.Entities.ReasonCodeTableRefType.Tender);
                var filteredRequiredReasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.filterReasonCodes(requiredReasonCodes, reasonCodeIds);

                return ReasonCodesHelper.filterUniqueReasonCodes(filteredRequiredReasonCodes, tenderLine.ReasonCodeLines);
            }

            return [];
        }

        /**
         * Gets the affiliation line required reason codes from the requirements.
         * @param {Proxy.Entities.ReasonCode[]} requiredReasonCodes The required reason codes.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} reasonCodeRequirements The reason code requirements.
         * @param {Proxy.Entities.SalesAffiliationLoyaltyTier} affiliationLine The affiliation line.
         * @return {Proxy.Entities.ReasonCode[]} The required reason codes.
         */
        private static getRequiredReasonCodesForAffiliationLine(
            requiredReasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeRequirements: Proxy.Entities.ReasonCodeRequirement[],
            affiliationLine: Proxy.Entities.SalesAffiliationLoyaltyTier): Proxy.Entities.ReasonCode[] {

            if (!ObjectExtensions.isNullOrUndefined(affiliationLine) && ArrayExtensions.hasElements(requiredReasonCodes)) {
                var reasonCodeIds: string[] = ReasonCodesHelper.getRequiredReasonCodeIds(
                    reasonCodeRequirements, affiliationLine.AffiliationId.toString(), Proxy.Entities.ReasonCodeTableRefType.Affiliation);
                var filteredRequiredReasonCodes: Proxy.Entities.ReasonCode[] = ReasonCodesHelper.filterReasonCodes(requiredReasonCodes, reasonCodeIds);

                return ReasonCodesHelper.filterUniqueReasonCodes(filteredRequiredReasonCodes, affiliationLine.ReasonCodeLines);
            }

            return [];
        }

        /**
         * Gets the required reason code identifiers given the source identifier and table reference type.
         * @param {Proxy.Entities.ReasonCodeRequirement[]} requirements The collection of reason code requirements.
         * @param {string} sourceId The source identifier.
         * @param {Proxy.Entities.ReasonCodeTableRefType} tabeRefType The table reference type.
         * @return {string[]} The required reason code identifiers.
         */
        private static getRequiredReasonCodeIds(
            requirements: Proxy.Entities.ReasonCodeRequirement[],
            sourceId: string,
            tableRefType: Proxy.Entities.ReasonCodeTableRefType): string[] {

            return requirements.filter((value: Proxy.Entities.ReasonCodeRequirement) => {
                return value.SourceId === sourceId && value.TableRefTypeValue === tableRefType;
            }).map((value: Proxy.Entities.ReasonCodeRequirement) => { return value.ReasonCodeId; });
        }

        /**
         * Filter the reason codes by reason code identifier.
         * @param {Proxy.Entities.ReasonCode[]} reasonCodes The reason codes to be filtered.
         * @param {string[]} reasonCodeIds The reason code identifiers used to filter.
         * @return {Proxy.Entities.ReasonCode[]} The filtered reason codes.
         */
        private static filterReasonCodes(
            reasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeIds: string[]): Proxy.Entities.ReasonCode[] {

            return reasonCodes.filter((value: Proxy.Entities.ReasonCode) => {
                for (var i: number = 0; i < reasonCodeIds.length; i++) {
                    if (value.ReasonCodeId === reasonCodeIds[i]) {
                        return true;
                    }
                }

                return false;
            });
        }

        /**
         * Gets the unique reason codes not present on the collection of reason code lines.
         * @param {Proxy.Entities.ReasonCode[]} reasonCodes The reason codes to be filtered.
         * @param {Proxy.Entities.ReasonCodeLine[]} presentReasonCodes The collection of reason code lines used to filter.
         * @return {Proxy.Entities.ReasonCode[]} The filtered reason codes.
         */
        private static filterUniqueReasonCodes(
            reasonCodes: Proxy.Entities.ReasonCode[],
            presentReasonCodes: Proxy.Entities.ReasonCodeLine[]): Proxy.Entities.ReasonCode[] {

            return reasonCodes.filter((value: Proxy.Entities.ReasonCode) => {
                if (ArrayExtensions.hasElements(presentReasonCodes)) {
                    for (var i: number = 0; i < presentReasonCodes.length; i++) {
                        if (value.ReasonCodeId === presentReasonCodes[i].ReasonCodeId) {
                            return false;
                        }
                    }
                }

                return true;
            });
        }

        /**
         * Gets the associated linked reason codes on a depth-first search manner until all reason codes are present.
         * @param {StringDictionary} reasonCodeIds The dictionary containing the processed reason code identifiers.
         * @param {Proxy.Entities.ReasonCode[]} reasonCodes The collection of processed reason codes, i.e. the reason codes with the frequency test performed.
         * @param {Proxy.Entities.ReasonCode[]} reasonCodesToProcess The collection of reason codes to process, i.e.
         * to have the frequency test performed and linked reason codes queried.
         * @param {boolean} [skipFrequencyTest] If the reason code should always be asked for.
         * @return {AsyncQueue} The async queue that must be executed in order to get reason codes and linked reason codes.
         */
        private static getLinkedReasonCodesForArrayQueue(
            reasonCodeIds: StringDictionary,
            reasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodesToProcess: Proxy.Entities.ReasonCode[],
            skipFrequencyTest: boolean = false): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();

            reasonCodesToProcess.forEach((r: Proxy.Entities.ReasonCode) => {
                if (reasonCodeIds[r.ReasonCodeId] !== undefined) {
                    return;
                }

                var randomValue: number = Math.random() * 100;
                var shouldAsk: boolean = skipFrequencyTest
                    || ObjectExtensions.isNullOrUndefined(r.RandomFactor)
                    || (r.RandomFactor === 0)
                    || (r.RandomFactor > randomValue);

                if (shouldAsk) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        return ReasonCodesHelper.getLinkedReasonCodesQueue(reasonCodeIds, reasonCodes, r).run();
                    });
                }
            });

            return asyncQueue;
        }

        /**
         * Gets the associated linked reason codes on a depth-first search manner until all reason codes are present.
         * @param {StringDictionary} reasonCodeIds The dictionary containing the processed reason code identifiers.
         * @param {Proxy.Entities.ReasonCode[]} reasonCodes The collection of processed reason codes, i.e. the reason codes with the frequency test performed.
         * @param {Proxy.Entities.ReasonCode} reasonCodeToProcess The reason code to process, i.e.
         * to have the frequency test performed and linked reason codes queried.
         * @return {AsyncQueue} The async queue that must be executed in order to get reason codes and linked reason codes.
         */
        private static getLinkedReasonCodesQueue(
            reasonCodeIds: StringDictionary,
            reasonCodes: Proxy.Entities.ReasonCode[],
            reasonCodeToProcess: Proxy.Entities.ReasonCode): AsyncQueue {

            var asyncQueue: AsyncQueue = new AsyncQueue();

            // add to the list of reason codes
            reasonCodeIds[reasonCodeToProcess.ReasonCodeId] = reasonCodeToProcess.ReasonCodeId;
            reasonCodes.push(reasonCodeToProcess);

            if (StringExtensions.isNullOrWhitespace(reasonCodeToProcess.LinkedReasonCodeId)) {
                return asyncQueue;
            }

            var linkedReasonCodes: Proxy.Entities.ReasonCode[] = [];
            asyncQueue.enqueue((): IAsyncResult<any> => {
                var salesOrderManager: Model.Managers.ISalesOrderManager = Model.Managers.Factory.getManager<Model.Managers.ISalesOrderManager>(
                    Model.Managers.ISalesOrderManagerName);

                return salesOrderManager.getReasonCodesByIdAsync(reasonCodeToProcess.LinkedReasonCodeId)
                    .done((result: Proxy.Entities.ReasonCode[]) => { linkedReasonCodes = result; });
            }).enqueue((): IAsyncResult<any> => {
                if (!ArrayExtensions.hasElements(linkedReasonCodes)) {
                    return VoidAsyncResult.createResolved();
                }

                return ReasonCodesHelper.getLinkedReasonCodesForArrayQueue(reasonCodeIds, reasonCodes, linkedReasonCodes).run();
            });

            return asyncQueue;
        }

        /**
         * If any reason sub code selected has a trigger code associated, triggers the sub code appropriately.
         * @param {ReasonCodesInternalContext} context The reason codes context.
         * @return {AsyncQueue} The async queue.
         */
        private static handleTriggeredReasonSubCodesAsyncQueue(context: ReasonCodesInternalContext): AsyncQueue {
            var reasonSubCodesQueue: AsyncQueue = new AsyncQueue();

            if (ArrayExtensions.hasElements(context.processedReasonCodes)) {
                var reasonSubCodes: Proxy.Entities.ReasonSubCode[] = ReasonCodesHelper.getTriggeredReasonSubCodesFromContext(context);
                reasonSubCodes.forEach((r: Proxy.Entities.ReasonSubCode) => {
                    reasonSubCodesQueue.enqueue((): IVoidAsyncResult => { return ReasonCodesHelper.triggerReasonSubCodeAsyncQueue(r).run(); });
                });
            }

            return reasonSubCodesQueue;
        }

        /**
         * Gets the triggered reason sub codes from the given context.
         * @param {ReasonCodesInternalContext} context The reason codes context.
         * @return {Proxy.Entities.ReasonSubCode[]} The collection of reason sub codes that were triggered.
         */
        private static getTriggeredReasonSubCodesFromContext(context: ReasonCodesInternalContext): Proxy.Entities.ReasonSubCode[] {
            var reasonCodesById: ReasonCodeDictionary = Object.create(null);
            context.processedReasonCodes.forEach((r: Proxy.Entities.ReasonCode) => reasonCodesById[r.ReasonCodeId] = r);

            var reasonSubCodes: Proxy.Entities.ReasonSubCode[] = [];
            context.addedReasonCodeLines.forEach((rcl: Proxy.Entities.ReasonCodeLine) => {
                if (StringExtensions.isNullOrWhitespace(rcl.SubReasonCodeId)) {
                    return;
                }

                var subCode: Proxy.Entities.ReasonSubCode = ArrayExtensions.firstOrUndefined(reasonCodesById[rcl.ReasonCodeId].ReasonSubCodes,
                    (rsc: Proxy.Entities.ReasonSubCode) => rsc.SubCodeId === rcl.SubReasonCodeId);

                reasonSubCodes.push(subCode);
            });

            return reasonSubCodes;
        }

        /**
         * Triggers the reason sub code.
         * @param {Proxy.Entities.ReasonSubCode} reasonSubCode The reason sub code to be triggered.
         * @return {AsyncQueue} The async queue.
         */
        private static triggerReasonSubCodeAsyncQueue(reasonSubCode: Proxy.Entities.ReasonSubCode): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            // if no trigger code configured, just continue
            if (reasonSubCode.TriggerFunctionTypeValue !== Proxy.Entities.TriggerFunctionType.Item
                || StringExtensions.isNullOrWhitespace(reasonSubCode.TriggerCode)) {
                return asyncQueue;
            }

            var operationsManager: Operations.OperationsManager = Operations.OperationsManager.instance;
            var oldCartLines: Proxy.Entities.CartLine[] = Session.instance.cart.CartLines;

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var options: Operations.IItemSaleOperationOptions = { productSaleDetails: [{ productId: reasonSubCode.ProductId, quantity: 1 }] };
                var result: IAsyncResult<ICancelableResult> = operationsManager.runOperation(Operations.RetailOperation.ItemSale, options);
                return asyncQueue.cancelOn(result);
            }).enqueue((): IVoidAsyncResult => {
                var cartLines: Proxy.Entities.CartLine[] = CartLineHelper.getModifiedCartLines(oldCartLines, Session.instance.cart.CartLines);
                var cartLine: Proxy.Entities.CartLine = ArrayExtensions.firstOrUndefined(cartLines,
                    (c: Proxy.Entities.CartLine) => c.ItemId === reasonSubCode.TriggerCode);

                if (ObjectExtensions.isNullOrUndefined(cartLine)) {
                    return VoidAsyncResult.createResolved();
                }

                var result: IAsyncResult<ICancelableResult>;
                switch (reasonSubCode.PriceTypeValue) {
                    case Proxy.Entities.PriceType.Percent:
                        var lineDiscountOptions: Operations.ILineDiscountOperationOptions = {
                            cartLineDiscounts: [{ cartLine: cartLine, discountValue: reasonSubCode.AmountPercent }]
                        };

                        result = operationsManager.runOperation(Operations.RetailOperation.LineDiscountPercent, lineDiscountOptions);
                        break;

                    case Proxy.Entities.PriceType.Price:
                        var priceOverrideOptions: Operations.IPriceOverrideOperationOptions = {
                            cartLinePrices: [{ cartLine: cartLine, price: reasonSubCode.AmountPercent }]
                        };

                        result = operationsManager.runOperation(Operations.RetailOperation.PriceOverride, priceOverrideOptions);
                        break;
                };

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue;
        }
    }
}