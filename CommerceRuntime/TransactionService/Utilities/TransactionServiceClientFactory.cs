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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Net;
        using System.Net.Security;
        using System.Reflection;
        using System.Security.Cryptography.X509Certificates;
        using System.ServiceModel;
        using System.ServiceModel.Channels;
        using System.ServiceModel.Description;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.IdentityModel.Claims;
        using Microsoft.IdentityModel.Protocols.WSTrust;
        using Microsoft.IdentityModel.SecurityTokenService;
        using Microsoft.IdentityModel.Tokens;
        using Microsoft.IdentityModel.Tokens.Saml2;
        using Retail.TransactionServices.ClientProxy;

        /// <summary>
        /// Represents the transaction service client factory.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Tracked in TFS1051915")]
        internal sealed class TransactionServiceClientFactory : ITransactionServiceClientFactory
        {
            private static readonly Lazy<string> ClientType = new Lazy<string>(() => string.Format("AX2012.CommerceRuntime.{0}", TransactionServiceClientFactory.GetApplicationVersion()));
            private readonly string dataAreaId;
            private readonly string requestLanguageId;
            private TransactionServiceProfile transactionServiceProfile;
            private ChannelFactory<RetailRealTimeServiceContractChannel> transactionServiceChannelFactory = null;
            private bool refreshRequestedTransactionService = true;
            private System.IdentityModel.Tokens.SecurityToken securityToken = null;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="TransactionServiceClientFactory"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public TransactionServiceClientFactory(Microsoft.Dynamics.Commerce.Runtime.RequestContext context)
            {
                ThrowIf.Null(context, "context");
    
                this.dataAreaId = context.GetChannelConfiguration() != null
                                      ? context.GetChannelConfiguration().InventLocationDataAreaId
                                      : string.Empty;
    
                var getTransactionServiceProfileDataRequest = new GetTransactionServiceProfileDataRequest();
                this.transactionServiceProfile = context.Runtime.Execute<SingleEntityDataServiceResponse<TransactionServiceProfile>>(getTransactionServiceProfileDataRequest, context).Entity;
                this.transactionServiceProfile = GetTransactionServiceProfile(context);
                this.SetRealTimeServiceProfileConfigSettings(context);
                this.requestLanguageId = this.transactionServiceProfile.LanguageId;
                this.ValidateTransactionProfile();
            }
    
            /// <summary>
            /// Gets password hash algorithm in channel configuration.
            /// </summary>
            public string PasswordHashAlgorithm
            {
                get { return this.transactionServiceProfile.StaffPasswordHash; }
            }
    
            /// <summary>
            /// Gets the channel factory for transaction service.
            /// </summary>
            private ChannelFactory<RetailRealTimeServiceContractChannel> ChannelFactory
            {
                get
                {
                    lock (this)
                    {
                        if (this.refreshRequestedTransactionService)
                        {
                            this.DoRefreshTransactionService();
                        }
    
                        return this.transactionServiceChannelFactory;
                    }
                }
            }
    
            /// <summary>
            /// Gets the security token for transaction service.
            /// </summary>
            private System.IdentityModel.Tokens.SecurityToken SecurityToken
            {
                get
                {
                    // Create a new token if the existing token is null or expired.
                    if (this.securityToken == null || this.securityToken.ValidTo <= DateTime.UtcNow)
                    {
                        this.securityToken = this.CreateToken();
                    }
    
                    return this.securityToken;
                }
            }
    
            /// <summary>
            /// Creates the RequestInfo based on the transaction service profile from database.
            /// </summary>
            /// <returns>The RequestInfo instance.</returns>
            public RetailTransactionServiceRequestInfo CreateRequestInfo()
            {
                return new RetailTransactionServiceRequestInfo
                {
                    Company = this.dataAreaId,
                    Language = this.requestLanguageId,
                    ProfileId = this.transactionServiceProfile.ProfileId,
                    ClientType = ClientType.Value
                };
            }
    
            /// <summary>
            /// Creates the transaction service client instance for common services.
            /// </summary>
            /// <returns>The common services client instance.</returns>
            public RetailRealTimeServiceContractChannel CreateTransactionServiceClient()
            {
                return this.ChannelFactory.CreateChannelWithIssuedToken<RetailRealTimeServiceContractChannel>(this.SecurityToken);
            }
    
            /// <summary>
            /// Request to refresh the transaction service channel factory (load the latest configurations).
            /// </summary>
            /// <remarks>
            /// Refreshing the channel factory resets connection for all the open clients. Use it with caution in multi-threaded environment.
            /// </remarks>
            public void Refresh()
            {
                this.refreshRequestedTransactionService = true;
            }
    
            /// <summary>
            /// Retrieves the transaction service profile.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The transaction service profile.</returns>
            private static TransactionServiceProfile GetTransactionServiceProfile(Microsoft.Dynamics.Commerce.Runtime.RequestContext context)
            {
                GetTransactionServiceProfileDataRequest request = new GetTransactionServiceProfileDataRequest();
                Microsoft.Dynamics.Commerce.Runtime.RequestContext getTransactionServiceProfileRequestContext;
    
                // Transaction service profile is associated with channel
                // Decide what context is to be used to retrieve it
                if (context.GetPrincipal().IsChannelAgnostic)
                {
                    // If the context is channel agnostic (no channel information), then use any channel available to retrieve the profile
                    GetChannelIdServiceRequest getChannelRequest = new GetChannelIdServiceRequest();
                    GetChannelIdServiceResponse getChannelResponse = context.Execute<GetChannelIdServiceResponse>(getChannelRequest);
                    long anyChannelId = getChannelResponse.ChannelId;
    
                    var anonymousIdentity = new CommerceIdentity()
                    {
                        ChannelId = anyChannelId
                    };
    
                    getTransactionServiceProfileRequestContext = new Microsoft.Dynamics.Commerce.Runtime.RequestContext(context.Runtime);
                    getTransactionServiceProfileRequestContext.SetPrincipal(new CommercePrincipal(anonymousIdentity));
                }
                else
                {
                    // If the request has channel information, then use current context to retrieve the transaction service profile
                    getTransactionServiceProfileRequestContext = context;
                }
    
                return getTransactionServiceProfileRequestContext.Execute<SingleEntityDataServiceResponse<TransactionServiceProfile>>(request).Entity;
            }
    
            /// <summary>
            /// Gets Application version number from assembly info.
            /// </summary>
            /// <returns>
            /// The assembly version.
            /// </returns>
            private static string GetApplicationVersion()
            {
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                string result = string.Empty;
    
                if (currentAssembly != null)
                {
                    object[] attributes = currentAssembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                    if (attributes.Length > 0)
                    {
                        AssemblyFileVersionAttribute versionAttribute = (AssemblyFileVersionAttribute)attributes[0];
                        if (!string.IsNullOrEmpty(versionAttribute.Version))
                        {
                            result = versionAttribute.Version;
                        }
                    }
                    else
                    {
                        // Fallback to assembly version, if 'FileVersion' attribute not found.
                        result = currentAssembly.GetName().Version.ToString();
                    }
                }
    
                return result;
            }
    
            /// <summary>
            /// Ignores the certificate validation.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="certificate">The certificate.</param>
            /// <param name="chain">The certificate chain.</param>
            /// <param name="errors">The policy errors.</param>
            /// <returns>The validation callback result.</returns>
            private static bool HttpsCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                if (errors == SslPolicyErrors.None)
                {
                    return true;
                }
                else
                {
                    if (certificate != null)
                    {
                        RetailLogger.Log.CrtTransactionServiceHttpsCertificateValidationFailedError(errors.ToString(), certificate.Subject);
                    }
                    else
                    {
                        RetailLogger.Log.CrtTransactionServiceHttpsCertificateValidationFailedError(errors.ToString(), string.Empty);
                    }

                    return false;
                }
            }

            /// <summary>
            /// Does the refresh for transaction service.
            /// </summary>
            private void DoRefreshTransactionService()
            {
                if (this.transactionServiceChannelFactory != null)
                {
                    if (this.transactionServiceChannelFactory.State == CommunicationState.Faulted)
                    {
                        this.transactionServiceChannelFactory.Abort();
                    }
                    else if (this.transactionServiceChannelFactory.State == CommunicationState.Opened)
                    {
                        this.transactionServiceChannelFactory.Close();
                    }
                }
    
                Binding binding = this.CreateBinding();
    
                // Create transaction service channel.
                ServiceEndpoint endpointTransactionService = this.CreateEndPoint(binding, typeof(RetailRealTimeServiceContract));
                this.transactionServiceChannelFactory = new ChannelFactory<RetailRealTimeServiceContractChannel>(endpointTransactionService);
                this.transactionServiceChannelFactory.Credentials.SupportInteractive = false;
                this.transactionServiceChannelFactory.ConfigureChannelFactory<RetailRealTimeServiceContractChannel>();
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(HttpsCertificateValidationCallback);
                this.refreshRequestedTransactionService = false;
            }
    
            /// <summary>
            /// Creates the binding based on the configurations.
            /// </summary>
            /// <returns>The binding configurations.</returns>
            private Binding CreateBinding()
            {
                WS2007FederationHttpBinding fedBinding = new WS2007FederationHttpBinding("SamlBearerTokenBindingConfig");
                fedBinding.Security.Message.IssuedTokenType = this.transactionServiceProfile.IssuedTokenType;
                return fedBinding;
            }
    
            /// <summary>
            /// Creates the endpoint based on passed binding, channel factory and service area.
            /// </summary>
            /// <param name="fedBinding">The service binding.</param>
            /// <param name="channelFactoryType">The channel factory.</param>
            /// <returns>The service endpoint.</returns>
            private ServiceEndpoint CreateEndPoint(Binding fedBinding, Type channelFactoryType)
            {
                string endPointAddress = string.Format("{0}/Services/{1}", this.transactionServiceProfile.ServiceHostUrl, this.transactionServiceProfile.ServiceName);
                ContractDescription contract = ContractDescription.GetContract(channelFactoryType);
                EndpointAddress address = new EndpointAddress(endPointAddress);
                return new ServiceEndpoint(contract, fedBinding, address);
            }
    
            /// <summary>
            /// Creates a token for transaction service channel factory.
            /// </summary>
            /// <returns>Token for transaction service.</returns>
            private System.IdentityModel.Tokens.SecurityToken CreateToken()
            {
                ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                Claim claim;
                claim = new Claim(ClaimTypes.Email, this.transactionServiceProfile.UserId);
                claimsIdentity.Claims.Add(claim);
                claim = new Claim(this.transactionServiceProfile.IdentityProviderClaim, this.transactionServiceProfile.IdentityProvider);
                claimsIdentity.Claims.Add(claim);
                claim = new Claim(ClaimTypes.NameIdentifier, this.transactionServiceProfile.UserId);
                claimsIdentity.Claims.Add(claim);
                Saml2SecurityTokenHandler tokenHandler = new Saml2SecurityTokenHandler();
                tokenHandler.SamlSecurityTokenRequirement = new SamlSecurityTokenRequirement();
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor();
                tokenDescriptor.TokenType = Microsoft.IdentityModel.Tokens.SecurityTokenTypes.Saml2TokenProfile11;
                tokenDescriptor.TokenIssuerName = this.transactionServiceProfile.IssuerUri;
                tokenDescriptor.Subject = claimsIdentity;
                DateTime currentUtcTime = DateTime.UtcNow;
                tokenDescriptor.Lifetime = new Microsoft.IdentityModel.Protocols.WSTrust.Lifetime(currentUtcTime, currentUtcTime.AddHours(24));
                tokenDescriptor.AppliesToAddress = this.transactionServiceProfile.AudienceUrn;
                X509Certificate2 signingCert = this.FindCertificate();
                if (signingCert != null)
                {
                    tokenDescriptor.SigningCredentials = new X509SigningCredentials(signingCert);
                }
                else
                {
                    throw new ArgumentException("Error locating certificate by thumbprint");
                }
    
                return tokenHandler.CreateToken(tokenDescriptor);
            }
    
            /// <summary>
            /// Finds the certificate.
            /// </summary>
            /// <returns>The certificate.</returns>
            private X509Certificate2 FindCertificate()
            {
                X509Certificate2 signingCert = null;
                StoreLocation location = (StoreLocation)Enum.Parse(typeof(StoreLocation), this.transactionServiceProfile.StoreLocation);
                X509Store certStore = new X509Store(this.transactionServiceProfile.StoreName, location);
                certStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, this.transactionServiceProfile.SigningCertificateThumbprint, true);
                if (certCollection.Count > 0)
                {
                    // If we find more than 1, return the first.
                    signingCert = certCollection[0];
                }
    
                return signingCert;
            }
    
            /// <summary>
            /// Sets real time service transaction profile configuration settings.
            /// </summary>
            /// <param name="context">Request context.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "[TODO] Bug #3312148")]
            private void SetRealTimeServiceProfileConfigSettings(Microsoft.Dynamics.Commerce.Runtime.RequestContext context)
            {
                this.transactionServiceProfile.SigningCertificateThumbprint = context.Runtime.Configuration.RealtimeService.Certificate.Thumbprint;
                this.transactionServiceProfile.StoreName = context.Runtime.Configuration.RealtimeService.Certificate.StoreName;
                this.transactionServiceProfile.StoreLocation = context.Runtime.Configuration.RealtimeService.Certificate.StoreLocation;
            }
    
            /// <summary>
            /// Verify if related fields in transaction profile are null or empty.
            /// </summary>
            private void ValidateTransactionProfile()
            {
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.UserId, "TSUserID");
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.ServiceHostUrl, "TSServiceHostUrl");
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.IssuerUri, "TSIssuerUri");
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.IdentityProvider, "TSIdentityProvider");
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.IdentityProviderClaim, "TSIdentityProviderClaimType");
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.AudienceUrn, "TSAudienceUrn");
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.IssuedTokenType, "TSIssuedTokenType");
                ThrowIf.NullOrWhiteSpace(this.transactionServiceProfile.LanguageId, "LanguageId");
            }
        }
    }
}
