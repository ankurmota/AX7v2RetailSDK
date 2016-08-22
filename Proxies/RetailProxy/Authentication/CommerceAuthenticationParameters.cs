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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// The commerce authentication parameters.
        /// </summary>
        public class CommerceAuthenticationParameters : Dictionary<string, object>
        {
            /// <summary>
            /// The grant type parameter name.
            /// </summary>
            private const string GrantTypeParameterName = "grant_type";
    
            /// <summary>
            /// The client identifier parameter name.
            /// </summary>
            private const string ClientIdParameterName = "client_id";
    
            /// <summary>
            /// The retail operation identifier parameter name.
            /// </summary>
            private const string RetailOperationIdentifierParamaterName = "operation_id";

            /// <summary>
            /// The authentication credential.
            /// </summary>
            private const string CredentialParameterName = "credential";

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceAuthenticationParameters"/> class.
            /// </summary>
            /// <param name="grantType">Type of the grant.</param>
            /// <param name="clientId">The client identifier.</param>
            public CommerceAuthenticationParameters(string grantType, string clientId) : this(grantType, clientId, null, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceAuthenticationParameters"/> class.
            /// </summary>
            /// <param name="grantType">Type of the grant.</param>
            /// <param name="clientId">The client identifier.</param>
            /// <param name="retailOperation">The retail operation.</param>
            /// <param name="credential">The user credential.</param>
            public CommerceAuthenticationParameters(string grantType, string clientId, RetailOperation? retailOperation, string credential)
            {
                ThrowIf.NullOrWhiteSpace(grantType, "grantType");
                ThrowIf.NullOrWhiteSpace(clientId, "clientId");
    
                this.GrantType = grantType;
                this.ClientId = clientId;
                this.RetailOperation = retailOperation;
                this.Credential = credential;
            }
    
            /// <summary>
            /// Gets or sets the type of the grant.
            /// </summary>
            public string GrantType
            {
                get
                {
                    object grantType;
                    this.TryGetValue(GrantTypeParameterName, out grantType);
                    return (string)grantType;
                }

                set
                {
                    this[GrantTypeParameterName] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the client identifier.
            /// </summary>
            public string ClientId
            {
                get { return (string)this[ClientIdParameterName]; }
                set { this[ClientIdParameterName] = value; }
            }

            /// <summary>
            /// Gets or sets the user credential.
            /// </summary>
            public string Credential
            {
                get
                {
                    object credential;
                    this.TryGetValue(CredentialParameterName, out credential);
                    return (string)credential;
                }

                set
                {
                    this[CredentialParameterName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the targeted retail operation.
            /// </summary>
            public RetailOperation? RetailOperation
            {
                get { return (RetailOperation?)this[RetailOperationIdentifierParamaterName]; }
                set { this[RetailOperationIdentifierParamaterName] = value; }
            }
        }
    }
}
