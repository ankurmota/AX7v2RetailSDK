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
        using System.Threading.Tasks;
        using Commerce.RetailProxy;

        /// <summary>
        /// Handler for wish list operations.
        /// </summary>
        public class WishListOperationsHandler : OperationsHandlerBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WishListOperationsHandler"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            public WishListOperationsHandler(EcommerceContext ecommerceContext) : base(ecommerceContext)
            {
            }

            /// <summary>
            /// Gets the wish list.
            /// </summary>
            /// <param name="wishListId">The wish list identifier.</param>
            /// <returns>
            /// The wish list.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            /// Throws when WishListId is null.
            /// </exception>
            public async virtual Task<CommerceList> GetWishList(long wishListId)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();

                CommerceList wishList = await commerceListManager.Read(wishListId);
                return wishList;
            }

            /// <summary>
            /// Deletes the wish list.
            /// </summary>
            /// <param name="wishListId">The wish list identifier.</param>
            /// <returns>Returns task.</returns>
            public virtual async Task DeleteWishList(long wishListId)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();

                CommerceList wishList = await commerceListManager.Read(wishListId);
                await commerceListManager.Delete(wishList);
            }

            /// <summary>
            /// Creates the wish list.
            /// </summary>
            /// <param name="wishListName">The wish list to create.</param>
            /// <returns>
            /// The wish list.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            /// WishList
            /// or
            /// customerId.
            /// </exception>
            public virtual async Task<CommerceList> CreateWishList(string wishListName)
            {
                if (string.IsNullOrWhiteSpace(wishListName))
                {
                    throw new ArgumentNullException(nameof(wishListName));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();

                ICustomerManager customerManager = managerFactory.GetManager<ICustomerManager>();
                Customer customer = await customerManager.Read(string.Empty);

                CommerceList wishList = new CommerceList();
                wishList.Name = wishListName;
                wishList.CustomerId = customer.AccountNumber;
                wishList = await commerceListManager.Create(wishList);

                return wishList;
            }

            /// <summary>
            /// Adds items to wish list.
            /// </summary>
            /// <param name="wishListId">The wish list identifier.</param>
            /// <param name="wishListLines">The items to add to the wish list.</param>
            /// <returns>
            /// The wish list.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            /// WishListId
            /// or
            /// customerId
            /// or
            /// listings.
            /// </exception>
            public virtual async Task<CommerceList> AddLinesToWishList(long wishListId, IEnumerable<CommerceListLine> wishListLines)
            {
                if (wishListLines == null)
                {
                    throw new ArgumentNullException(nameof(wishListLines));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();

                ICustomerManager customerManager = managerFactory.GetManager<ICustomerManager>();
                Customer customer = await customerManager.Read(string.Empty);

                foreach (CommerceListLine wishListLine in wishListLines)
                {
                    if (wishListLine != null)
                    {
                        wishListLine.CommerceListId = wishListId;
                        wishListLine.CustomerId = customer.AccountNumber;
                    }
                }

                CommerceList wishList = await commerceListManager.AddLines(wishListId, wishListLines);
                return wishList;
            }

            /// <summary>
            /// Update items on wish list.
            /// </summary>
            /// <param name="wishListId">The wish list identifier.</param>
            /// <param name="wishListLines">The item to update on the wish list.</param>
            /// <returns>
            /// The wish list.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            /// WishListId
            /// or
            /// customerId
            /// or
            /// listings.
            /// </exception>
            public virtual async Task<CommerceList> UpdateLinesOnWishList(long wishListId, IEnumerable<CommerceListLine> wishListLines)
            {
                if (wishListLines == null)
                {
                    throw new ArgumentNullException(nameof(wishListLines));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();

                ICustomerManager customerManager = managerFactory.GetManager<ICustomerManager>();
                Customer customer = await customerManager.Read(string.Empty);

                foreach (CommerceListLine wishListLine in wishListLines)
                {
                    if (wishListLine != null)
                    {
                        wishListLine.CommerceListId = wishListId;
                        wishListLine.CustomerId = customer.AccountNumber;
                    }
                }

                CommerceList wishList = await commerceListManager.UpdateLines(wishListId, wishListLines);
                return wishList;
            }

            /// <summary>
            /// Update wish list properties.
            /// </summary>
            /// <param name="wishList">The wish list to update.</param>
            /// <returns>
            /// The updated wish list.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            /// WishList
            /// or
            /// customerId.
            /// </exception>
            public virtual async Task<CommerceList> UpdateWishListProperties(CommerceList wishList)
            {
                if (wishList == null)
                {
                    throw new ArgumentNullException(nameof(wishList));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();

                wishList = await commerceListManager.Update(wishList);
                return wishList;
            }

            /// <summary>
            /// Remove item from wish list.
            /// </summary>
            /// <param name="wishListId">The wish list identifier.</param>
            /// <param name="productIds">The items to remove from the wish list.</param>
            /// <returns>Returns the updated wish list.</returns>
            public virtual async Task<CommerceList> RemoveItemFromWishList(long wishListId, IEnumerable<long> productIds)
            {
                if (productIds == null)
                {
                    throw new ArgumentNullException(nameof(productIds));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();
                ICustomerManager customerManager = managerFactory.GetManager<ICustomerManager>();
                Customer customer = await customerManager.Read(string.Empty);

                Collection<CommerceListLine> wishListLines = new Collection<CommerceListLine>();
                foreach (long productId in productIds)
                {
                    CommerceListLine wishListLine = new CommerceListLine()
                    {
                        CommerceListId = wishListId,
                        CustomerId = customer.AccountNumber,
                        ProductId = productId
                    };

                    wishListLines.Add(wishListLine);
                }

                CommerceList updatedWishList = await commerceListManager.RemoveLines(wishListId, wishListLines);
                return updatedWishList;
            }
        }
    }
}