/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Retail.Ecommerce.Sdk.Core.OperationsHandlers
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Configuration;
        using System.Globalization;
        using System.IO;
        using System.Linq;
        using System.Threading.Tasks;
        using Commerce.RetailProxy;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Handler for cart operations.
        /// </summary>
        public class CartOperationsHandler : OperationsHandlerBase
        {
            /// <summary>
            /// Represents the term after which a authenticated users' cart expires.
            /// </summary>
            private const string ShoppingCartExpiryTermPropertyName = "StoreFront_ShoppingCartExpiryTerm";

            /// <summary>
            /// Initializes a new instance of the <see cref="CartOperationsHandler"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            public CartOperationsHandler(EcommerceContext ecommerceContext) : base(ecommerceContext)
            {
            }

            /// <summary>
            /// Gets the shopping cart.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <returns>
            /// The shopping cart.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId is null or empty.</exception>
            /// <exception cref="System.IO.InvalidDataException">Shopping cart was not found.</exception>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">Shopping cart {0} was not found.</exception>
            public virtual async Task<Cart> GetCart(string shoppingCartId)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = await cartManager.Read(shoppingCartId);

                if (cart == null)
                {
                    RetailLogger.Log.OnlineStoreCartNotFound(shoppingCartId);
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), shoppingCartId);
                }

                return cart;
            }

            /// <summary>
            /// Creates an empty shopping cart.
            /// </summary>
            /// <returns>
            /// The shopping cart.
            /// </returns>
            public virtual async Task<Cart> CreateEmptyCart()
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = await cartManager.Create(new Cart() { Id = string.Empty });

                return cart;
            }

            /// <summary>
            /// Adds the items to the shopping cart.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="cartLines">Represents the cartLines in the cart.</param>
            /// <param name="sessionType">Represents the type of the current sessions.</param>
            /// <returns>A shopping cart.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when listings is null.</exception>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">Shopping cart {0} was not found.</exception>
            public virtual async Task<Cart> AddItems(string shoppingCartId, IEnumerable<CartLine> cartLines, SessionType sessionType)
            {
                if (cartLines == null)
                {
                    throw new ArgumentNullException(nameof(cartLines));
                }

                Cart cart = null;

                if (sessionType == SessionType.SignedInShopping || sessionType == SessionType.SignedInCheckout)
                {
                    // Always get the active shopping cart before adding items for an authenticated user.
                    cart = await this.GetActiveShoppingCart();

                    // if active shopping cart does not exist create an new cart Id.
                    shoppingCartId = cart == null ? string.Empty : cart.Id;
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();

                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    // Create empty cart if it does not exist yet.
                    cart = await cartManager.Create(new Cart() { Id = string.Empty });
                    shoppingCartId = cart.Id;
                }

                cart = await cartManager.AddCartLines(shoppingCartId, cartLines);

                if (cart == null)
                {
                    RetailLogger.Log.OnlineStoreCartNotFound(shoppingCartId);
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), shoppingCartId);
                }

                return cart;
            }

            /// <summary>
            /// Removes the items from the shopping cart.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="lineIds">The line ids.</param>
            /// <returns>
            /// A shopping cart.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId is null.</exception>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">Shopping cart {0} was not found.</exception>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId is null.</exception>
            public virtual async Task<Cart> RemoveItems(string shoppingCartId, IEnumerable<string> lineIds)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                if (lineIds == null)
                {
                    throw new ArgumentNullException(nameof(lineIds));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = await cartManager.RemoveCartLines(shoppingCartId, lineIds);

                if (cart == null)
                {
                    RetailLogger.Log.OnlineStoreCartNotFound(shoppingCartId);
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), shoppingCartId);
                }

                return cart;
            }

            /// <summary>
            /// Sets AffiliationLoyaltyTiers on cart level.
            /// </summary>
            /// <param name="cartId">The CartId.</param>
            /// <param name="affiliationLoyaltyTierIds">Collection of AffiliationLoyaltyTier IDs.</param>
            /// <returns>Returns task.</returns>
            /// <remarks>The tiers are created with type General.</remarks>
            public virtual async Task SetCartAffiliationLoyaltyTiers(string cartId, IEnumerable<long> affiliationLoyaltyTierIds)
            {
                if (affiliationLoyaltyTierIds == null)
                {
                    throw new ArgumentNullException(nameof(affiliationLoyaltyTierIds));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = await cartManager.Read(cartId);
                cart.AffiliationLines.Clear();

                foreach (long affiliationLoyaltyTierId in affiliationLoyaltyTierIds)
                {
                    cart.AffiliationLines.Add(new AffiliationLoyaltyTier { AffiliationId = affiliationLoyaltyTierId, AffiliationTypeValue = 0 }); // RetailAffiliationType.General = 0;
                }

                await cartManager.Update(cart);
            }

            /// <summary>
            /// Gets General AffiliationLoyaltyTier IDs applied to the card.
            /// </summary>
            /// <param name="cartId">The Cart ID.</param>
            /// <returns>Collection of AffiliationLoyaltyTier IDs.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<IEnumerable<long>> GetCartAffiliationLoyaltyTiers(string cartId)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = await cartManager.Read(cartId);

                IList<long> tierIds = new List<long>();
                foreach (AffiliationLoyaltyTier tier in cart.AffiliationLines)
                {
                    if (tier.AffiliationTypeValue == 0)
                    {
                        tierIds.Add((long)tier.AffiliationId);
                    }
                }

                return tierIds;
            }

            /// <summary>
            /// Updates the items.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="cartLines">Represents the cart lines in the cart.</param>
            /// <returns>
            /// The shopping cart.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when items is null.</exception>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">
            /// Shopping cart {0} was not found.
            /// </exception>
            public virtual async Task<Cart> UpdateItems(string shoppingCartId, IEnumerable<CartLine> cartLines)
            {
                if (cartLines == null)
                {
                    throw new ArgumentNullException(nameof(cartLines));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = await cartManager.UpdateCartLines(shoppingCartId, cartLines);

                if (cart == null)
                {
                    RetailLogger.Log.OnlineStoreCartNotFound(shoppingCartId);
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), shoppingCartId);
                }

                return cart;
            }

            /// <summary>
            /// Moves the items between carts.
            /// </summary>
            /// <param name="sourceShoppingCartId">The source shopping cart identifier.</param>
            /// <param name="destinationShoppingCartId">The destination shopping cart identifier.</param>
            /// <param name="sessionType">Represents the type of the current sessions.</param>
            /// <returns>Returns cart.</returns>
            /// <exception cref="System.ArgumentNullException">
            /// SourceShoppingCartId
            /// or
            /// destinationShoppingCartId.
            /// </exception>
            public virtual async Task<Cart> MoveItemsBetweenCarts(string sourceShoppingCartId, string destinationShoppingCartId, SessionType sessionType)
            {
                if (string.IsNullOrWhiteSpace(sourceShoppingCartId))
                {
                    throw new ArgumentNullException(nameof(sourceShoppingCartId));
                }

                if (string.IsNullOrWhiteSpace(destinationShoppingCartId))
                {
                    throw new ArgumentNullException(nameof(destinationShoppingCartId));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart sourceCart = await cartManager.Read(sourceShoppingCartId);

                Cart destinationCart;
                if (sourceCart != null && sourceCart.CartLines.Any())
                {
                    destinationCart = await this.AddItems(destinationShoppingCartId, sourceCart.CartLines, sessionType);

                    // Line ids to delete from source.
                    IEnumerable<string> lineIdsToDelete = sourceCart.CartLines.Select(i => i.LineId);

                    // The api expects the line ids as a string array.
                    string[] lineIdsToDeleteAsArray = lineIdsToDelete.ToArray();
                    sourceCart = await cartManager.RemoveCartLines(sourceShoppingCartId, lineIdsToDeleteAsArray);
                }
                else
                {
                    destinationCart = await this.GetCart(destinationShoppingCartId);
                }

                return destinationCart;
            }

            /// <summary>
            /// Get the applicable promotions for the items in the cart.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <returns>The applicable promotions.</returns>
            public virtual async Task<CartPromotions> GetPromotions(string shoppingCartId)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                CartPromotions cartPromotions = await cartManager.GetPromotions(shoppingCartId);

                return cartPromotions;
            }

            /// <summary>
            /// Starts the checkout process by creating a secure cart and returning it.
            /// </summary>
            /// <param name="shoppingCartId">The current cart id to be used for the checkout process.</param>
            /// <param name="previousCheckoutCartId">The identifier for any previous checkout cart that was abandoned.</param>
            /// <returns>
            /// A new cart with a random cart id that should be used during the secure checkout process.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when shoppingCartId is null.</exception>
            public virtual async Task<Cart> CommenceCheckout(string shoppingCartId, string previousCheckoutCartId)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                // Get the saved cart
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart checkoutCart = await cartManager.Copy(shoppingCartId, (int)CartType.Checkout);

                return checkoutCart;
            }

            /// <summary>
            /// Adds or Removes the discount codes from the cart.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="promotionCode">The promotion code.</param>
            /// <param name="isAdd">Indicates whether the operation is addition or removal of discount codes.</param>
            /// <returns>
            /// A shopping cart.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when shoppingCartId or promotionCode is null.</exception>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">Shopping cart {0} was not found.</exception>
            public virtual async Task<Cart> AddOrRemovePromotionCode(string shoppingCartId, string promotionCode, bool isAdd)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                if (string.IsNullOrWhiteSpace(promotionCode))
                {
                    throw new ArgumentNullException(nameof(promotionCode));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = null;

                if (isAdd)
                {
                    cart = await cartManager.AddDiscountCode(shoppingCartId, promotionCode);
                }
                else
                {
                    cart = await cartManager.RemoveDiscountCodes(shoppingCartId, new Collection<string>() { promotionCode });
                }

                if (cart == null)
                {
                    RetailLogger.Log.OnlineStoreCartNotFound(shoppingCartId);
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), shoppingCartId);
                }

                cart = await DataAugmenter.GetAugmentedCart(this.EcommerceContext, cart);
                return cart;
            }

            /// <summary>
            /// Gets the latest modified shopping cart associated with the current user.
            /// </summary>
            /// <returns>
            /// The active shopping cart.
            /// </returns>
            public virtual async Task<Cart> GetActiveShoppingCart()
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                QueryResultSettings queryResultSettings = new QueryResultSettings() { Paging = new PagingInfo { Skip = 0, Top = 1 } };
                SortingInfo sortingInfo = new SortingInfo();
                ObservableCollection<SortColumn> sortColumns = new ObservableCollection<SortColumn>();
                SortColumn sortColumn = new SortColumn();
                sortColumn.ColumnName = "ModifiedDateTime";
                sortColumn.IsDescending = true;
                sortColumns.Add(sortColumn);
                sortingInfo.Columns = sortColumns;
                queryResultSettings.Sorting = sortingInfo;
                CartSearchCriteria cartSearchCriteria = new CartSearchCriteria();
                cartSearchCriteria.CartTypeValue = (int)CartType.Shopping;
                cartSearchCriteria.IncludeAnonymous = false;

                IEnumerable<Cart> carts = await cartManager.Search(cartSearchCriteria, queryResultSettings);

                Cart cart = carts.FirstOrDefault();
                if (cart != null)
                {
                    DateTime cartLastModifiedDate = cart.ModifiedDateTime.HasValue ? cart.ModifiedDateTime.Value.DateTime : DateTime.UtcNow;
                    if (this.HasCartExpired(cartLastModifiedDate))
                    {
                        cart = null;
                    }
                }

                return cart;
            }

            /// <summary>
            /// Claims the anonymous cart.
            /// </summary>
            /// <param name="shoppingCartIdToBeClaimed">The shopping cart identifier.</param>
            /// <returns>A shopping cart.</returns>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">Shopping cart {0} of customer {1} was not found.</exception>
            public virtual async Task<Cart> ClaimAnonymousCart(string shoppingCartIdToBeClaimed)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICustomerManager customerManager = managerFactory.GetManager<ICustomerManager>();
                Customer customer = await customerManager.Read(string.Empty);

                ICartManager cartManager = managerFactory.GetManager<ICartManager>();

                // First validate the cart to be claimed
                Cart cartToClaim = await cartManager.Read(shoppingCartIdToBeClaimed);

                // Make sure only anonymous cart can be claimed. CustomerId check is needed for defence in depth. 
                if (cartToClaim == null || !string.IsNullOrWhiteSpace(cartToClaim.CustomerId))
                {
                    return null;
                }

                Cart cart = new Cart
                {
                    Id = shoppingCartIdToBeClaimed,
                    CustomerId = customer.AccountNumber,
                    CartTypeValue = (int)CartType.Shopping
                };

                cart = await cartManager.Update(cart);

                if (cart == null)
                {
                    RetailLogger.Log.OnlineStoreCartNotFound(shoppingCartIdToBeClaimed);
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), shoppingCartIdToBeClaimed);
                }

                return cart;
            }

            /// <summary>
            /// Updates the loyalty card id.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart id.</param>
            /// <param name="loyaltyCardId">The loyalty card id.</param>
            /// <returns>
            /// A shopping cart.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when shoppingCartId is null.</exception>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">
            /// The loyalty card provided is blocked.
            /// or
            /// Invalid loyalty card number.
            /// or
            /// Shopping cart {0} of customer {1} was not found.
            /// </exception>
            public virtual async Task<Cart> UpdateLoyaltyCardId(string shoppingCartId, string loyaltyCardId)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();

                Cart cart = new Cart()
                {
                    Id = shoppingCartId,
                    LoyaltyCardId = loyaltyCardId
                };

                cart = await cartManager.Update(cart);

                cart = await DataAugmenter.GetAugmentedCart(this.EcommerceContext, cart);
                return cart;
            }

            /// <summary>
            /// Gets the applicable delivery options when the user wants to 'ship' the entire order as a single entity.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="shipToaddress">The ship to address.</param>
            /// <param name="queryResultSettings">Accepts queryResultSetting.</param>
            /// <returns>
            /// The available delivery options.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId is null or empty.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<DeliveryOption>> GetOrderDeliveryOptionsForShipping(string shoppingCartId, Address shipToaddress, QueryResultSettings queryResultSettings)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                PagedResult<DeliveryOption> deliveryOptions = await cartManager.GetDeliveryOptions(shoppingCartId, shipToaddress, queryResultSettings);

                return deliveryOptions;
            }

            /// <summary>
            /// Gets the delivery options applicable per line when the user wants the items in the cart 'shipped' to them individually.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="lineShippingAddresses">The line shipping addresses.</param>
            /// <param name="queryResultSettings">The queryResultSettings.</param>
            /// <returns>
            /// The delivery options available per item.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId is null or empty.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<SalesLineDeliveryOption>> GetLineDeliveryOptionsForShipping(
                string shoppingCartId,
                IEnumerable<LineShippingAddress> lineShippingAddresses,
                QueryResultSettings queryResultSettings)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                if (lineShippingAddresses == null || !lineShippingAddresses.Any())
                {
                    throw new ArgumentNullException(nameof(lineShippingAddresses));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();

                PagedResult<SalesLineDeliveryOption> salesLineDeliveryOptions = await cartManager.GetLineDeliveryOptions(shoppingCartId, lineShippingAddresses, queryResultSettings);

                return salesLineDeliveryOptions;
            }

            /// <summary>
            /// Gets delivery preferences applicable to the current checkout cart.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <returns>The applicable delivery preferences.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when shoppingCartId is null.</exception>
            public virtual async Task<CartDeliveryPreferences> GetDeliveryPreferences(string shoppingCartId)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                CartDeliveryPreferences cartDeliveryPreferences = await cartManager.GetDeliveryPreferences(shoppingCartId);

                return cartDeliveryPreferences;
            }

            /// <summary>
            /// Commits the selected delivery option to the cart when entire order is being 'delivered' as a single entity.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="deliverySpecification">The selected delivery option.</param>
            /// <returns>
            /// The updated shopping cart.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId or shippingOptions is null or empty.</exception>
            public virtual async Task<Cart> UpdateDeliverySpecification(string shoppingCartId, DeliverySpecification deliverySpecification)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                if (deliverySpecification == null)
                {
                    throw new ArgumentNullException(nameof(deliverySpecification));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();

                Cart cart = await cartManager.UpdateDeliverySpecification(shoppingCartId, deliverySpecification);

                cart = await DataAugmenter.GetAugmentedCart(this.EcommerceContext, cart);
                return cart;
            }

            /// <summary>
            /// Commits the selected delivery options per line when the sales line in the order are being 'delivered' individually.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="lineDeliverySpecifications">The line delivery options.</param>
            /// <returns>
            /// The updated shopping cart.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId or shippingOptions is null or empty.</exception>
            public virtual async Task<Cart> UpdateLineDeliverySpecifications(string shoppingCartId, IEnumerable<LineDeliverySpecification> lineDeliverySpecifications)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                if (lineDeliverySpecifications == null)
                {
                    throw new ArgumentNullException(nameof(lineDeliverySpecifications));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                Cart cart = await cartManager.UpdateLineDeliverySpecifications(shoppingCartId, lineDeliverySpecifications);

                cart = await DataAugmenter.GetAugmentedCart(this.EcommerceContext, cart);
                return cart;
            }

            /// <summary>
            /// Get the types of payment cards.
            /// </summary>
            /// <param name="shoppingCartId">The shopping cart identifier.</param>
            /// <param name="cartTenderLines">The cart tender lines.</param>
            /// <param name="emailAddress">The email address.</param>
            /// <returns>The channel reference identifier for the order.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the shoppingCartId or emailAddress is null or empty.  Thrown when payments is null.</exception>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.DataValidationException">Shopping cart {0} for customer {1} was not found.</exception>
            public virtual async Task<SalesOrder> CreateOrder(string shoppingCartId, IEnumerable<CartTenderLine> cartTenderLines, string emailAddress)
            {
                if (string.IsNullOrWhiteSpace(shoppingCartId))
                {
                    throw new ArgumentNullException(nameof(shoppingCartId));
                }

                if (string.IsNullOrWhiteSpace(emailAddress))
                {
                    throw new ArgumentNullException(nameof(emailAddress));
                }

                if (cartTenderLines == null)
                {
                    throw new ArgumentNullException(nameof(cartTenderLines));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();

                SalesOrder salesOrder = await cartManager.Checkout(
                    id: shoppingCartId,
                    receiptEmail: emailAddress,
                    tokenizedPaymentCard: null,
                    receiptNumberSequence: null,
                    cartTenderLines: cartTenderLines);

                return salesOrder;
            }

            /// <summary>
            /// Retrieves card payment accept point.
            /// </summary>
            /// <param name="cartId">The cardId.</param>
            /// <param name="cardPaymentAcceptSettings">The card payment accept settings.</param>
            /// <returns>Card payment accept point.</returns>
            public virtual async Task<CardPaymentAcceptPoint> GetCardPaymentAcceptPoint(string cartId, CardPaymentAcceptSettings cardPaymentAcceptSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                CardPaymentAcceptPoint cardPaymentAcceptPoint = await cartManager.GetCardPaymentAcceptPoint(cartId, cardPaymentAcceptSettings);

                return cardPaymentAcceptPoint;
            }

            /// <summary>
            /// Checks if the shopping cart of a customer has expired.
            /// </summary>
            /// <param name="cartLastModifiedDate">The last modified date of the shopping cart that is checked for expiry.</param>
            /// <returns>Boolean indicating whether the cart is expired or not.</returns>
            protected virtual bool HasCartExpired(DateTime cartLastModifiedDate)
            {
                DateTime newDate = DateTime.UtcNow;
                try
                {
                    int expiryTerm = int.Parse(ConfigurationManager.AppSettings[ShoppingCartExpiryTermPropertyName], CultureInfo.InvariantCulture);
                    TimeSpan ts = newDate - cartLastModifiedDate;

                    // Difference in days.
                    int differenceInDays = ts.Days;
                    return differenceInDays > expiryTerm;
                }
                catch (FormatException ex)
                {
                    throw new FormatException("Exception obtaining expiry term", ex);
                }
            }
        }
    }
}