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
    namespace Commerce.Runtime.Sample.ExtensionProperties
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Linq;
        using Messages;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Notifications;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Notification handler for the custom notification.
        /// </summary>
        public sealed class CustomNotificationHandler : NotificationHandler
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
                        typeof(CustomNotification)
                    };
                }
            }

            /// <summary>
            /// Executes the logic within the specified context.
            /// </summary>
            /// <param name="notification">The notification.</param>
            public override void Notify(Notification notification)
            {
                ThrowIf.Null(notification, "notification");
                ThrowIf.Null(notification.RequestContext, "notification.RequestContext");

                var customNotification = notification as CustomNotification;
                if (customNotification != null)
                {
                    // implement the handling code that should be executed whenever this notification is raised
                    Debug.WriteLine("The notification handler has been called with data: '{0}'", customNotification.Data);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Notification '{0}' is not supported.", notification.GetType()));
                }
            }
        }
    }
}
