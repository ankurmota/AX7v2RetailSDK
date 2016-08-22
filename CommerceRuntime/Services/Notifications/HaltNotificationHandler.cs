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
    namespace Commerce.Runtime.Notifications
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Notifications;

        /// <summary>
        /// A simple notification handler that blocks the execution flow.
        /// </summary>
        public sealed class HaltNotificationHandler : NotificationHandler
        {
            /// <summary>
            /// Gets the collection of notification types supported by this handler.
            /// </summary>
            public override IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(EmptyOrderDeliveryOptionSetNotification),
                        typeof(EmptyProductDeliveryOptionSetNotification),
                        typeof(ProductDiscontinuedFromChannelNotification),
                        typeof(ProductMasterPageNotification),
                    };
                }
            }

            /// <summary>
            /// Represents the logic to handle notification.
            /// </summary>
            /// <param name="notification">The incoming notification message.</param>
            public override void Notify(Notification notification)
            {
                ThrowIf.Null(notification, "notification");

                if (notification is ProductDiscontinuedFromChannelNotification)
                {
                    NotifyProductDiscontinuedFromChannelNotification((ProductDiscontinuedFromChannelNotification)notification);
                }
                else if (notification is EmptyOrderDeliveryOptionSetNotification)
                {
                    NotifyEmptyOrderDeliveryOptionSetNotification((EmptyOrderDeliveryOptionSetNotification)notification);
                }
                else if (notification is EmptyProductDeliveryOptionSetNotification)
                {
                    NotifyEmptyProductDeliveryOptionSetNotification((EmptyProductDeliveryOptionSetNotification)notification);
                }
                else if (notification is ProductMasterPageNotification)
                {
                    NotifyProductMasterPageNotification((ProductMasterPageNotification)notification);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Notification '{0}' is not supported.", notification.GetType()));
                }
            }

            private static void NotifyProductDiscontinuedFromChannelNotification(ProductDiscontinuedFromChannelNotification notification)
            {
                IEnumerable<string> notAvailableItemIds = notification.LinesWithUnavailableProducts.Values.SelectMany(c => c.Select(l => l.ItemId));
                throw new ItemDiscontinuedException(
                    "Item(s) {0} have been discontinued from the channel or have not been synchronized to the channel database.",
                    string.Join(", ", notAvailableItemIds));
            }

            private static void NotifyEmptyOrderDeliveryOptionSetNotification(EmptyOrderDeliveryOptionSetNotification notification)
            {
                throw new ConfigurationException(
                    ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindDeliveryOptions,
                    string.Format("No common delivery option found for the order. Order id: {0}", notification.OrderId));
            }

            private static void NotifyEmptyProductDeliveryOptionSetNotification(EmptyProductDeliveryOptionSetNotification notification)
            {
                throw new ConfigurationException(
                    ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindDeliveryOptions,
                    string.Format("No delivery option found for the the product. Ensure the required job is run properly. ItemId: {0}, InventoryDimensionId: {1}, Address: {2}", notification.ItemId, notification.InventoryDimensionId, notification.Address));
            }

            private static void NotifyProductMasterPageNotification(ProductMasterPageNotification notification)
            {
                throw new DataValidationException(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ProductMasterPageRequired,
                    string.Format(CultureInfo.CurrentUICulture, "The variant information for Item Id is not present, please load the product master page for product: {0}", notification.ProductId));
            }
        }
    }
}