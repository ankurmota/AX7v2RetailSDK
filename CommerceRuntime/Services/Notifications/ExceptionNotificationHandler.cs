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
    namespace Commerce.Runtime.Exceptions
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Notifications;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Generic exception notification handler that processes exception caught when executing a commerce runtime request.
        /// </summary>
        public sealed class ExceptionNotificationHandler : NotificationHandler
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
                        typeof(ExceptionNotification)
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
    
                var exceptionNotification = notification as ExceptionNotification;
                if (exceptionNotification != null)
                {
                    LogRequestExecutionFailure(exceptionNotification.Exception, exceptionNotification.CorrelationId);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Notification '{0}' is not supported.", notification.GetType()));
                }
            }

            /// <summary>
            /// Logs exception event.
            /// </summary>
            /// <param name="exception">Exception instance to log.</param>
            /// <param name="correlationId">The correlation identifier.</param>
            private static void LogRequestExecutionFailure(Exception exception, Guid correlationId)
            {
                string exceptionTypeName = exception.GetType().FullName;
                CommerceException commerceException = exception as CommerceException;

                if (commerceException == null)
                {
                    // Tracing low-level exception as 'Error' because it is either a bug (NullReferenceException, ArgumentNullException etc.)
                    // or external dependency that is not wrapped by try/catch block.
                    RetailLogger.Log.CrtExecuteRequestErrorFailure(correlationId, exception, exceptionTypeName, errorResourceId: string.Empty);
                }
                else
                {
                    switch (commerceException.Severity)
                    {
                        case ExceptionSeverity.Informational:
                            {
                                RetailLogger.Log.CrtExecuteRequestInformationalFailure(correlationId, commerceException, exceptionTypeName, commerceException.ErrorResourceId);
                                break;
                            }

                        case ExceptionSeverity.Warning:
                            {
                                RetailLogger.Log.CrtExecuteRequestWarningFailure(correlationId, commerceException, exceptionTypeName, commerceException.ErrorResourceId);
                                break;
                            }

                        case ExceptionSeverity.Critical:
                            {
                                RetailLogger.Log.CrtExecuteRequestCriticalFailure(correlationId, commerceException, exceptionTypeName, commerceException.ErrorResourceId);
                                break;
                            }

                        default:
                            {
                                // For levels 'Error' and 'None' use default trace level 'Error'.
                                RetailLogger.Log.CrtExecuteRequestErrorFailure(correlationId, commerceException, exceptionTypeName, commerceException.ErrorResourceId);
                                break;
                            }
                    }
                }
            }
        }
    }
}
