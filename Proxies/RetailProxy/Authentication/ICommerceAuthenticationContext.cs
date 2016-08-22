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
    namespace Commerce.RetailProxy.Authentication
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Reflection;
        using System.Threading.Tasks;

        /// <summary>
        /// Interface for the Commerce Authentication providers.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Dynamics.Retail.StyleCop.Rules.FileNameAnalyzer", "SR1704:FileNameDoesNotMatchElementInside", Justification = "Will be removed once file is renamed.")]
        public abstract class CommerceAuthenticationProvider
        {
            internal const string AcquireTokenActionName = "AcquireToken";

            /// <summary>
            /// Gets or sets the device token.
            /// </summary>
            internal string DeviceToken { get; set; }

            /// <summary>
            /// Gets or sets the user token.
            /// </summary>
            internal UserToken UserToken { get; set; }

            /// <summary>
            /// Gets or sets the locale for the context.
            /// </summary>
            internal string Locale { get; set; }
            
            /// <summary>
            /// Acquires the user token.
            /// </summary>
            /// <param name="userName">Name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="commerceAuthenticationParameters">The additional commerce authentication parameters.</param>
            /// <returns>The user token.</returns>
            internal abstract Task<UserToken> AcquireToken(string userName, string password, CommerceAuthenticationParameters commerceAuthenticationParameters);
    
            /// <summary>
            /// Changes the password.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="oldPassword">The current password.</param>
            /// <param name="newPassword">The new password.</param>
            /// <returns>A task.</returns>
            internal abstract Task ChangePassword(string userId, string oldPassword, string newPassword);
    
            /// <summary>
            /// Resets the password of the user <param name="userId"/>.
            /// </summary>
            /// <param name="userId">The id of the user having the password changed.</param>
            /// <param name="newPassword">The newPassword.</param>
            /// <param name="mustChangePasswordAtNextLogOn">Whether the password needs to be changed at the next logon.</param>
            /// <returns>A Task.</returns>
            internal abstract Task ResetPassword(string userId, string newPassword, bool mustChangePasswordAtNextLogOn);

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credential">The user credential.</param>
            /// <param name="extraParameters">The extra parameters.</param>
            /// <returns>A task.</returns>
            internal abstract Task EnrollUserCredentials(string userId, string grantType, string credential, IDictionary<string, object> extraParameters);

            /// <summary>
            /// Removes user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <returns>A task.</returns>
            internal abstract Task UnenrollUserCredentials(string userId, string grantType);

            /// <summary>
            /// Executes the operation asynchronous with no result.
            /// </summary>
            /// <typeparam name="T">The type of the returned result.</typeparam>
            /// <param name="operation">The operation name.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            /// <returns>No return.</returns>
            internal virtual async Task<T> ExecuteAuthenticationSingleResultOperationAsync<T>(string operation, params OperationParameter[] operationParameters)
            {
                Type managerType = this.GetType();
                object result = null;
                if (managerType != null)
                {
                    var methodInfos = managerType.GetRuntimeMethods().Where(m => m.Name.Equals(operation, StringComparison.OrdinalIgnoreCase));

                    foreach (MethodInfo methodInfo in methodInfos)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();

                        // Make sure the actual method parameter count matches with parameters passed in thru query string.
                        if (parameters.Length == operationParameters.Length)
                        {
                            Task task;
                            if (parameters.Length == 0)
                            {
                                task = (Task)methodInfo.Invoke(this, null);
                            }
                            else
                            {
                                List<object> parametersList = new List<object>();

                                foreach (ParameterInfo parameter in parameters)
                                {
                                    var parameterValue = operationParameters.First(x => string.Equals(x.Name, parameter.Name)).Value;
                                    parametersList.Add(parameterValue);
                                }

                                task = (Task)methodInfo.Invoke(this, parametersList.ToArray());
                            }

                            await task;

                            // For async methods that return non generic 'Task' the returned result for this method will be 'null'
                            if (methodInfo.ReturnType.GetTypeInfo().IsGenericType)
                            {
                                var property = task.GetType().GetTypeInfo().GetDeclaredProperty("Result");
                                if (property != null)
                                {
                                    result = property.GetValue(task);
                                }
                            }
                        }
                    }
                }

                return (T)result;
            }
        }
    }
}
