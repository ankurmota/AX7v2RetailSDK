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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Employee data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class EmployeeSqliteDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(LockUserAtLogOnDataRequest),
                        typeof(UnlockUserAtLogOffDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(LockUserAtLogOnDataRequest))
                {
                    response = this.LockUserAtLogOn();
                }
                else if (requestType == typeof(UnlockUserAtLogOffDataRequest))
                {
                    response = this.UnLockUserAtLogOff();
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Unlock the current user.
            /// </summary>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<bool> UnLockUserAtLogOff()
            {
                // NOTE: sqlite data service is used only in offline context and only one client can consume the database. There is no reason to unlock logging in database.
                return new SingleEntityDataServiceResponse<bool>(true);
            }
    
            /// <summary>
            /// Lock the current user, so that same user can't log into another terminal until log off from the current terminal.
            /// </summary>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<bool> LockUserAtLogOn()
            {
                // NOTE: sqlite data service is used only in offline context and only one client can consume the database. There is no reason to lock logging in database.
                return new SingleEntityDataServiceResponse<bool>(true);
            }
        }
    }
}
