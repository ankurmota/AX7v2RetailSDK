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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Linq;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// The commerce runtime manager.
        /// </summary>
        internal sealed class CommerceRuntimeManager
        {
            private static readonly Lazy<CommerceRuntimeManager> Instance = new Lazy<CommerceRuntimeManager>(() => new CommerceRuntimeManager());
            private Func<string, CommerceRuntimeConfiguration> getCrtConfigurationByHostFunc;

            /// <summary>
            /// Prevents a default instance of the <see cref="CommerceRuntimeManager"/> class from being created.
            /// </summary>
            private CommerceRuntimeManager()
            {
                this.getCrtConfigurationByHostFunc = AdaptorCaller.GetCrtConfigurationByHostFunc;
            }

            /// <summary>
            /// Gets or sets the current commerce identity.
            /// </summary>
            public static CommerceIdentity Identity { get; internal set; }

            /// <summary>
            /// Gets or sets the locale.
            /// </summary>
            internal static string Locale { get; set; }

            /// <summary>
            /// Gets or sets the specified roles.
            /// </summary>
            internal static string[] SpecifiedRoles { get; set; }

            /// <summary>
            /// Gets the runtime.
            /// </summary>
            internal static CommerceRuntime Runtime
            {
                get
                {
                    // Constructs the CommerceIdentity if the default roles are provided.
                    if (Identity == null && SpecifiedRoles != null && SpecifiedRoles.Any())
                    {
                        Identity = GetDefaultCommerceIdentity();
                    }

                    if (Identity == null)
                    {
                        // identity missing is equivalent to non authenticated user without channel information
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "Commerce identity is not provided.");
                    }

                    CommercePrincipal principal = new CommercePrincipal(Identity);
                    return CommerceRuntime.Create(Instance.Value.getCrtConfigurationByHostFunc(AdaptorCaller.HostName), principal, CommerceRuntimeManager.Locale);
                }
            }

            /// <summary>
            /// Sets the user identity.
            /// </summary>
            /// <param name="userToken">The user token.</param>
            internal static void SetUserIdentity(UserToken userToken)
            {
                ThrowIf.Null(userToken, "userToken");
                CommerceUserToken commerceUserToken = userToken as CommerceUserToken;
                CommerceRuntimeUserToken runtimeToken = userToken as CommerceRuntimeUserToken;

                // A user token has to be either a commerce runtime token or a commerce user token which can contain a commerce runtime token, or retail server token or both.
                if (commerceUserToken == null && runtimeToken == null)
                {                    
                    throw new RetailProxy.UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidUserToken.ToString(), string.Format("Invalid token type provided {0}.", userToken.GetType()));
                }

                runtimeToken = commerceUserToken == null ? runtimeToken : commerceUserToken.CommerceRuntimeToken;

                if (runtimeToken == null)
                {
                    throw new RetailProxy.UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidUserToken.ToString(), string.Format("The provided commerce runtime token cannot be null."));
                }

                Identity = Newtonsoft.Json.JsonConvert.DeserializeObject<CommerceIdentity>(runtimeToken.Token);
            }

            /// <summary>
            /// Updates the <paramref name="targetIdentity"/> with the fields from the <paramref name="sourceIdentity"/>.
            /// </summary>
            /// <param name="targetIdentity">The identity object to be updated.</param>
            /// <param name="sourceIdentity">The identity object source for the update.</param>
            internal static void UpdateCommerceIdentity(CommerceIdentity targetIdentity, CommerceIdentity sourceIdentity)
            {
                ThrowIf.Null(targetIdentity, "targetIdentity");
                ThrowIf.Null(sourceIdentity, "sourceIdentity");

                targetIdentity.UserId = sourceIdentity.UserId;

                if (sourceIdentity.UserId != null)
                {
                    targetIdentity.Roles.Add(CommerceRoles.Employee);
                }

                if (!string.IsNullOrWhiteSpace(sourceIdentity.OriginalUserId))
                {
                    targetIdentity.OriginalUserId = sourceIdentity.OriginalUserId;
                    targetIdentity.ElevatedRetailOperation = sourceIdentity.ElevatedRetailOperation;
                    targetIdentity.UserId = sourceIdentity.UserId;

                    const string ManagerPrivilege = "MANAGERPRIVILEGES";

                    if (sourceIdentity.Roles.Contains(ManagerPrivilege))
                    {
                        targetIdentity.Roles.Add(ManagerPrivilege);
                    }
                }
                else
                {
                    foreach (var role in sourceIdentity.Roles)
                    {
                        targetIdentity.Roles.Add(role);
                    }
                }

                if (!string.IsNullOrWhiteSpace(sourceIdentity.DeviceNumber))
                {
                    targetIdentity.DeviceToken = sourceIdentity.DeviceToken;
                    targetIdentity.LogOnConfiguration = sourceIdentity.LogOnConfiguration;
                    targetIdentity.TerminalId = sourceIdentity.TerminalId;
                    targetIdentity.DeviceNumber = sourceIdentity.DeviceNumber;
                    targetIdentity.ChannelId = sourceIdentity.ChannelId;
                }
            }

            /// <summary>
            /// Clears the user identity from the <paramref name="currentToken"/> and returns a new <see cref="UserToken"/> without user identity information.
            /// </summary>
            /// <param name="currentToken">The current user token.</param>
            /// <returns>The new user token after user identity is cleared.</returns>
            internal static UserToken RemoveUserIdentityFromToken(UserToken currentToken)
            {
                CommerceIdentity commerceIdentity;

                if (currentToken == null)
                {
                    commerceIdentity = new CommerceIdentity();
                }
                else
                {
                    commerceIdentity = Newtonsoft.Json.JsonConvert.DeserializeObject<CommerceIdentity>(currentToken.Token);
                }

                commerceIdentity.UserId = null;
                commerceIdentity.OriginalUserId = null;
                commerceIdentity.ElevatedRetailOperation = RetailOperation.None;
                commerceIdentity.Roles.Clear();
                commerceIdentity.Roles.Add(CommerceRoles.Anonymous);

                return new CommerceRuntimeUserToken(commerceIdentity);
            }

            private static CommerceIdentity GetDefaultCommerceIdentity()
            {
                using (CommerceRuntime commerceRuntime = CommerceRuntime.Create(Instance.Value.getCrtConfigurationByHostFunc(AdaptorCaller.HostName), CommercePrincipal.AnonymousPrincipal))
                {
                    ChannelManager channelManager = ChannelManager.Create(commerceRuntime);
                    var defaultChannelId = channelManager.GetDefaultChannelId();

                    return new CommerceIdentity(defaultChannelId, SpecifiedRoles);
                }
            }
        }
    }
}