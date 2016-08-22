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
    namespace Retail.Ecommerce.Web.Storefront.Controllers
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.Specialized;
        using System.Configuration;
        using System.IdentityModel.Tokens;
        using System.IO;
        using System.Linq;
        using System.Security.Claims;
        using System.Text;
        using System.Threading.Tasks;
        using System.Web;
        using System.Web.Mvc;
        using System.Web.Security;
        using System.Xml;
        using Commerce.RetailProxy;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.IdentityModel.Protocols;
        using Microsoft.Owin.Security;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;
        using Retail.Ecommerce.Web.Storefront.ViewModels;

        /// <summary>
        /// The SignIn Controller.
        /// </summary>
        public class SignInController : ActionControllerBase
        {
            /// <summary>
            /// The controller name.
            /// </summary>
            public const string ControllerName = "SignIn";

            /// <summary>
            /// The ACS redirect action name.
            /// </summary>
            public const string AcsRedirectActionName = "AcsRedirect";

            /// <summary>
            /// The account link up pending action name.
            /// </summary>
            public const string AccountLinkUpPendingActionName = "AccountLinkUpPending";

            /// <summary>
            /// The OpenIdV2 authentication redirect action name.
            /// </summary>
            public const string OAuthV2RedirectActionName = "OAuthV2Redirect";

            /// <summary>
            /// The finalize account link up action name.
            /// </summary>
            public const string FinalizeAccountLinkUpActionName = "FinalizeAccountLinkUp";

            /// <summary>
            /// The default action name.
            /// </summary>
            public const string DefaultActionName = "Index";

            /// <summary>
            /// The provider selected action name.
            /// </summary>
            public const string ProviderSelectedActionName = "ProviderSelected";

            /// <summary>
            /// The sign out action name.
            /// </summary>
            public const string SignOutActionName = "SignOut";

            /// <summary>
            /// The sign in action name.
            /// </summary>
            public const string FinishSignUpActionName = "FinishSignUp";

            /// <summary>
            /// The start sign up action name.
            /// </summary>
            public const string StartSignUpActionName = "StartSignUp";

            private const string SignUpViewName = "OAuthV2Redirect";
            private const string SignInViewName = "SignIn";
            private const string SignOutViewName = "SignOut";
            private const string UserPendingActivationViewName = "UserPendingActivation";

            /// <summary>
            /// Image url.
            /// </summary>
            private const string ImageUrl = "ImageUrl";

            /// <summary>
            /// Sequence index.
            /// </summary>
            private const string DisplayIndex = "DisplayIndex";

            /// <summary>
            /// New account.
            /// </summary>
            private const string NewAccount = "NewAccount";

            /// <summary>
            /// Cache of registered authentication providers.
            /// </summary>
            private static IOrderedEnumerable<AuthenticationDescription> authenticationProviders = null;

            /// <summary>
            /// Default view of the sign-in Controller.
            /// </summary>
            /// <returns>The default result for the sign-in Controller.</returns>
            [HttpGet]
            public ActionResult Index()
            {
                bool isCheckoutFlow = (bool)(TempData["IsCheckoutFlow"] ?? false);
                bool isActivationFlow = (bool)(TempData["IsActivationFlow"] ?? false);

                AuthenticationProvidersViewModel authenticationProvidersViewModel = new AuthenticationProvidersViewModel();
                authenticationProvidersViewModel.AuthenticationDescriptions = SignInController.GetAuthenticationProviders(this.HttpContext);
                authenticationProvidersViewModel.IsCheckoutFlow = isCheckoutFlow;

                if (isActivationFlow)
                {
                    string existingAccount = (string)(TempData["ActivatedEmail"] ?? string.Empty);
                    string externalIdProvider = (string)(TempData["IdProvider"] ?? string.Empty);
                    string message1 = string.Format(
                        "Congratulations! We were able to succesfully link your '{0}' account with the Contoso account belonging to '{1}'",
                        externalIdProvider,
                        existingAccount);

                    string message2 = "Please sign in to access your account.";

                    authenticationProvidersViewModel.Messages = new string[] { message1, message2 };
                    authenticationProvidersViewModel.IsActivationFlow = true;
                }

                return this.View(SignInController.SignInViewName, authenticationProvidersViewModel);
            }

            /// <summary>
            /// Action for signing out.
            /// </summary>
            /// <returns>The view after signing out.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web.Configuration", "CA3147:MarkVerbHandlersWithValidateAntiforgeryToken", MessageId = "#ValidateAntiForgeryTokenAttributeDefaultMissing", Justification = "Support for anti-forgery token will be added once the controls are redesigned to follow MVC pattern.")]
            public ActionResult SignOut()
            {
                Uri externalLogOffUri = null;

                if (this.Request.IsAuthenticated)
                {
                    var ctx = Request.GetOwinContext();
                    ctx.Authentication.SignOut(CookieConstants.ApplicationCookieAuthenticationType);

                    IdentityProviderClientConfigurationElement providerClient = OpenIdConnectUtilities.GetCurrentProviderSettings();
                    ////ctx.Authentication.SignOut(providerClient.Name);

                    // Clean up openId nonce cookie. This is just a workaround. Ideally, we should be calling 'ctx.Authentication.SignOut(providerClient.Name)'
                    //// Begin workaround.
                    foreach (string cookieName in ControllerContext.HttpContext.Request.Cookies.AllKeys)
                    {
                        if (cookieName.StartsWith("OpenIdConnect.nonce.", StringComparison.OrdinalIgnoreCase))
                        {
                            OpenIdConnectUtilities.RemoveCookie(cookieName);
                            break;
                        }
                    }

                    //// End workaround.

                    externalLogOffUri = providerClient.LogOffUrl;
                }

                OpenIdConnectUtilities.RemoveCookie(OpenIdConnectUtilities.CookieCurrentProvider);
                ServiceUtilities.CleanUpOnSignOutOrAuthFailure(this.HttpContext);

                return this.View(SignInController.SignOutViewName, externalLogOffUri);
            }

            /// <summary>
            /// Action invoked once the providers are selected.
            /// </summary>
            /// <returns>A redirect response to gather user credentials.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web", "CA3007:ReviewCodeForOpenRedirectVulnerabilities", Justification = "Under investigation.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web.Configuration", "CA3147:MarkVerbHandlersWithValidateAntiforgeryToken", MessageId = "#ValidateAntiForgeryTokenAttributePostMissing", Justification = "Support for anti-forgery token will be added once the controls are redesigned to follow MVC pattern.")]
            [HttpPost]
            public ActionResult ProviderSelected()
            {
                string authenticationProviderName = this.Request.Form["SelectedAuthenticationProviderName"];

                IdentityProviderClientConfigurationElement provider = Utilities.GetIdentityProviderFromConfiguration(authenticationProviderName);
                switch (provider.ProviderType)
                {
                    case IdentityProviderType.OpenIdConnect:
                        ControllerContext.HttpContext.GetOwinContext().Authentication.Challenge(provider.Name);
                        return new HttpUnauthorizedResult();

                    case IdentityProviderType.ACS:
                        // Storing cookie with current provider (used in Logoff).
                        HttpCookie cookie = new HttpCookie(OpenIdConnectUtilities.CookieCurrentProvider, provider.Name);
                        cookie.Expires = DateTime.MaxValue;
                        cookie.HttpOnly = true;
                        cookie.Secure = true;
                        this.HttpContext.Response.Cookies.Add(cookie);

                        string url = string.Format("{0}v2/wsfederation?wa=wsignin1.0&wtrealm={1}", provider.Issuer, provider.RedirectUrl);
                        Response.Redirect(url, true);
                        break;

                    default:
                        RetailLogger.Log.OnlineStoreUnsupportedIdentityProviderTypeEncountered(provider.ProviderType.ToString());
                        SecurityException securityException = new SecurityException(string.Format("The identity provider type {0} is not supported", provider.ProviderType));
                        throw securityException;
                }

                return null;
            }

            /// <summary>
            /// Starts the sign up process.
            /// </summary>
            /// <returns>The view for entering sign up information.</returns>
            [HttpGet]
            public ActionResult StartSignUp()
            {
                string authenticationToken = (string)(this.TempData["AuthToken"] ?? string.Empty);

                IdentityProviderType selectedAuthenticationProviderType;
                Enum.TryParse((this.TempData["ExternalIdProviderType"] ?? string.Empty).ToString(), out selectedAuthenticationProviderType);

                if (string.IsNullOrEmpty(authenticationToken) || selectedAuthenticationProviderType == IdentityProviderType.None)
                {
                    string message = "Both the authentication token and authentication provider type must be set on request. Redirecting to sign in page.";
                    System.Diagnostics.Trace.TraceWarning(message);
                    return this.RedirectToAction(SignInController.DefaultActionName, SignInController.ControllerName);
                }

                string emailAddressFromExternalId = (string)(this.TempData["LogOnEmail"] ?? string.Empty);
                bool isRequestToLinkToExistingCustomerPending = (bool)(this.TempData["IsActivationPending"] ?? false);
                string invalidEmailProvidedForLinkUp = (string)(this.TempData["BadLinkUpEmail"] ?? string.Empty);

                SignUpViewModel signUpViewModel = new SignUpViewModel()
                {
                    AuthenticationToken = authenticationToken,
                    ExternalIdentityProviderType = selectedAuthenticationProviderType,
                    LogOnEmailAddress = emailAddressFromExternalId
                };

                if (isRequestToLinkToExistingCustomerPending == true)
                {
                    signUpViewModel.ErrorMessage = "A previous request to link this user to an exiting Contoso account is already pending. Any new actions will override the previous request.";
                }
                else if (!string.IsNullOrEmpty(invalidEmailProvidedForLinkUp))
                {
                    string message = string.Format("No Contoso account associated with '{0}' could be located.", invalidEmailProvidedForLinkUp);
                    signUpViewModel.ErrorMessage = message;
                }

                return this.View(SignInController.SignUpViewName, signUpViewModel);
            }

            /// <summary>
            /// Action invoked on user sign up.
            /// </summary>
            /// <returns>View after creating new account or linking to existing account.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web.Configuration", "CA3147:MarkVerbHandlersWithValidateAntiforgeryToken", MessageId = "#ValidateAntiForgeryTokenAttributePostMissing", Justification = "Support for anti-forgery token will be added once the controls are redesigned to follow MVC pattern.")]
            [HttpPost]
            public async Task<ActionResult> FinishSignUp()
            {
                IdentityProviderType selectedAuthenticationProviderType;
                Enum.TryParse(this.Request.Form["SelectedAuthenticationProviderType"], out selectedAuthenticationProviderType);

                string selectedAuthenticationProviderToken = this.Request.Form["SelectedAuthenticationProviderToken"];
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext, selectedAuthenticationProviderToken, selectedAuthenticationProviderType);

                string signUpRadioButtonSelection = this.Request.Form["SignUpRadioButton"];

                if (string.Equals(signUpRadioButtonSelection, NewAccount, StringComparison.OrdinalIgnoreCase))
                {
                    string firstName = this.Request.Form["FirstName"];
                    string lastName = this.Request.Form["LastName"];
                    string email = this.Request.Form["Email"];

                    Customer customer = new Customer();
                    customer.Email = email;
                    customer.AccountNumber = string.Empty;
                    customer.FirstName = firstName;
                    customer.LastName = lastName;

                    CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                    customer = await customerOperationsHandler.Create(customer);

                    this.UpdateClaims(
                        this.HttpContext,
                        selectedAuthenticationProviderToken,
                        selectedAuthenticationProviderType,
                        email,
                        customer.AccountNumber,
                        customer.FirstName,
                        customer.LastName);

                    return this.RedirectToAction(HomeController.DefaultActionName, HomeController.ControllerName);
                }
                else
                {
                    string emailOfExistingCustomer = this.Request.Form["EmailOfExistingCustomer"];
                    LinkToExistingCustomerResult linkToExistingCustomerResult = null;
                    try
                    {
                        CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                        linkToExistingCustomerResult = await customerOperationsHandler.InitiateLinkExternalIdToExistingCustomer(emailOfExistingCustomer, "emailTemplateId", null);
                    }
                    catch (DataValidationException ex)
                    {
                        if (string.Equals(ex.ErrorResourceId, DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerNotFound.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            RetailLogger.Log.OnlineStoreCustomerForLinkingNotFound(Utilities.GetMaskedEmailAddress(emailOfExistingCustomer), ex, ex.InnerException);

                            this.TempData["BadLinkUpEmail"] = emailOfExistingCustomer;
                            this.TempData["AuthToken"] = selectedAuthenticationProviderToken;
                            this.TempData["ExternalIdProviderType"] = selectedAuthenticationProviderType;
                            this.TempData["LogOnEmail"] = this.Request.Form["LogOnEmail"];

                            return this.RedirectToAction(SignInController.StartSignUpActionName, SignInController.ControllerName);
                        }
                        else
                        {
                            throw ex;
                        }
                    }

                    string maskedEmailAddress = Utilities.GetMaskedEmailAddress(linkToExistingCustomerResult.EmailAddress);

                    ////Clean up auth cookies completely. We need to be signed out.

                    this.TempData["IsSignUpFlow"] = true;
                    this.TempData["MaskedEmail"] = maskedEmailAddress;

                    return this.RedirectToAction(SignInController.AccountLinkUpPendingActionName, SignInController.ControllerName);
                }
            }

            /// <summary>
            /// Action invoked on being redirected from open identity provider.
            /// </summary>
            /// <returns>View after being redirected from open identity provider.</returns>
            /// <exception cref="System.NotSupportedException">Thrown when email claim does not exist.</exception>
            public async Task<ActionResult> OAuthV2Redirect()
            {
                IdentityProviderClientConfigurationElement currentProvider = OpenIdConnectUtilities.GetCurrentProviderSettings();

                // Check whether provider returned an error which could be a case if a user rejected a consent.
                string errorCode = ControllerContext.HttpContext.Request.Params["error"];
                if (errorCode != null)
                {
                    string message = string.Format(
                    "The provider {0} returned error code {1} while processing user's credentials.", currentProvider.Name, errorCode);
                    System.Diagnostics.Trace.TraceWarning(message);
                    this.Response.Redirect("~", false);
                    this.HttpContext.ApplicationInstance.CompleteRequest();
                    return null;
                }

                string authorizationCode = OpenIdConnectUtilities.ValidateRequestAndGetAuthorizationCode(this.HttpContext);

                if (authorizationCode == null)
                {
                    SecurityException securityException = new SecurityException("Unable to find the authorization code for the login request.");
                    RetailLogger.Log.OnlineStoreAuthorizationCodeNotFoundForLogOnRequest(securityException);
                    throw securityException;
                }

                string bodyParameters = string.Format(
                    "grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}",
                    authorizationCode,
                    currentProvider.RedirectUrl,
                    currentProvider.ClientId,
                    currentProvider.ClientSecret);

                OpenIdConnectConfiguration providerDiscoveryDocument = OpenIdConnectUtilities.GetDiscoveryDocument(currentProvider.Issuer);

                string returnValuesJson = OpenIdConnectUtilities.HttpPost(new Uri(providerDiscoveryDocument.TokenEndpoint), bodyParameters);

                TokenEndpointResponse tokenResponse = OpenIdConnectUtilities.DeserilizeJson<TokenEndpointResponse>(returnValuesJson);

                JwtSecurityToken token = OpenIdConnectUtilities.GetIdToken(tokenResponse.IdToken);

                Claim emailClaim = token.Claims.SingleOrDefault(c => string.Equals(c.Type, CookieConstants.Email, StringComparison.OrdinalIgnoreCase));

                if (emailClaim == null)
                {
                    RetailLogger.Log.OnlineStoreClaimNotFound(CookieConstants.Email, "Required for sign up using OpenIdAuth");
                    throw new SecurityException("Email claim does not exist.");
                }

                RedirectToRouteResult redirectResult = await this.GetRedirectionBasedOnAssociatedCustomer(this.HttpContext, tokenResponse.IdToken, currentProvider.ProviderType, emailClaim.Value);
                return redirectResult;
            }

            /// <summary>
            /// Action invoked on being redirected from ACS identity provider.
            /// </summary>
            /// <returns>View after being redirected from ACS identity provider.</returns>
            /// <exception cref="System.NotSupportedException">Thrown when email claim does not exist.</exception>
            public async Task<ActionResult> AcsRedirect()
            {
                string documentContents;
                using (Stream receiveStream = this.HttpContext.Request.InputStream)
                {
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    documentContents = readStream.ReadToEnd();
                }

                string acsToken = GetAcsToken(documentContents);

                JwtSecurityToken token = new JwtSecurityToken(acsToken);
                var emailClaim = token.Claims.FirstOrDefault(t => t.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

                string email = null;

                // Not all providers provide the claim, for instance, Windows Live ID does not.
                if (emailClaim != null)
                {
                    email = emailClaim.Value;
                }

                return await this.GetRedirectionBasedOnAssociatedCustomer(this.HttpContext, acsToken, IdentityProviderType.ACS, email);
            }

            /// <summary>
            /// Action invoked to complete link up of an existing customer with an external identity.
            /// </summary>
            /// <returns>View for entering activation code to finalize account link up.</returns>
            public async Task<ActionResult> FinalizeAccountLinkUp()
            {
                string emailAddressOfExistingCustomer = this.Request.Form["Email"];
                string activationCode = this.Request.Form["ActivationCode"];

                if (string.IsNullOrEmpty(emailAddressOfExistingCustomer) || string.IsNullOrEmpty(activationCode))
                {
                    var message = "Both the email address and associated activation code must be provided.";
                    RetailLogger.Log.OnlineStoreInvalidAccountLinkUpRequest(Utilities.GetMaskedEmailAddress(emailAddressOfExistingCustomer), activationCode, new NotSupportedException(message));
                    CustomerLinkUpPendingViewModel viewModel = new CustomerLinkUpPendingViewModel()
                    {
                        ErrorMessage = message,
                        EmailAddressOfExistingCustomer = emailAddressOfExistingCustomer,
                        ActivationCode = activationCode
                    };

                    return this.View(SignInController.UserPendingActivationViewName, viewModel);
                }

                LinkToExistingCustomerResult result;
                try
                {
                    RetailOperationsHandler retailOperationsHandler = new RetailOperationsHandler(ServiceUtilities.GetEcommerceContext(this.HttpContext));
                    result = await retailOperationsHandler.FinalizeLinkToExistingCustomer(emailAddressOfExistingCustomer, activationCode);
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.OnlineStoreInvalidAccountLinkUpRequest(Utilities.GetMaskedEmailAddress(emailAddressOfExistingCustomer), activationCode, ex);
                    var message = "We were unable to process this activation code. Please try entering the code again.";
                    CustomerLinkUpPendingViewModel viewModel = new CustomerLinkUpPendingViewModel()
                    {
                        ErrorMessage = message,
                        EmailAddressOfExistingCustomer = emailAddressOfExistingCustomer,
                        ActivationCode = activationCode
                    };

                    return this.View(SignInController.UserPendingActivationViewName, viewModel);
                }

                this.TempData["IsActivationFlow"] = true;
                this.TempData["ActivatedEmail"] = result.EmailAddress;
                this.TempData["IdProvider"] = GetAuthenticationProviderName(result.ExternalIdentityProvider);

                var authProviders = SignInController.GetAuthenticationProviders(this.HttpContext);

                return this.RedirectToAction(SignInController.DefaultActionName, SignInController.ControllerName);
            }

            /// <summary>
            /// Action invoked to bring up screen to enter activation code to finalize account link-up.
            /// </summary>
            /// <returns>
            /// The action.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web.Configuration", "CA3147:MarkVerbHandlersWithValidateAntiforgeryToken", MessageId = "#ValidateAntiForgeryTokenAttributeDefaultMissing", Justification = "Support for anti-forgery token will be added once the controls are redesigned to follow MVC pattern.")]
            public ActionResult AccountLinkUpPending()
            {
                CustomerLinkUpPendingViewModel viewModel = new CustomerLinkUpPendingViewModel();

                bool isSignUpFlow = (bool)(this.TempData["IsSignUpFlow"] ?? false);
                if (isSignUpFlow)
                {
                    string maskedEmailAddressOfExistingCustomer = (string)(this.TempData["MaskedEmail"] ?? string.Empty);
                    string message = string.Format("Congratulations! We were able to locate your account, and have sent an activation code to {0}.", maskedEmailAddressOfExistingCustomer);
                    viewModel.Messages = new string[] { message };
                }
                else
                {
                    viewModel.EmailAddressOfExistingCustomer = this.Request.QueryString["email"];
                    viewModel.ActivationCode = this.Request.QueryString["code"];
                }

                return this.View(SignInController.UserPendingActivationViewName, viewModel);
            }

            /// <summary>
            /// Gets the ACS token.
            /// </summary>
            /// <param name="queryString">The query string.</param>
            /// <returns>The ACS token.</returns>
            /// <exception cref="SecurityException">Thrown when the token is not found.</exception>
            private static string GetAcsToken(string queryString)
            {
                NameValueCollection collection = HttpUtility.ParseQueryString(queryString);
                string wresult = collection.Get("wresult");
                TextReader textReader = new StringReader(wresult);

                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.XmlResolver = null;

                XmlReader xmlReader = XmlReader.Create(textReader, xmlReaderSettings);
                if (!xmlReader.ReadToFollowing("wsse:BinarySecurityToken"))
                {
                    var securityException = new SecurityException("Could not find the token.");
                    RetailLogger.Log.OnlineStoreAcsAuthTokenNotFound(securityException);
                    throw securityException;
                }

                string encodedToken = xmlReader.ReadElementContentAsString();
                byte[] bytes = System.Convert.FromBase64String(encodedToken);
                string decodedToken = System.Text.Encoding.UTF8.GetString(bytes);
                return decodedToken;
            }

            private static string GetAuthenticationProviderName(string authenticationProviderUrl)
            {
                IDictionary<string, IdentityProviderClientConfigurationElement> identityProviderDictionary = GetIdentityProvidersFromConfig();

                IdentityProviderClientConfigurationElement providerConfig = identityProviderDictionary.Values.Where(v => string.Equals(v.Issuer.OriginalString, authenticationProviderUrl, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();

                if (providerConfig != null)
                {
                    return providerConfig.Name;
                }
                else
                {
                    RetailLogger.Log.OnlineStoreUnsupportedIdentityProviderTypeEncountered(authenticationProviderUrl);
                    return authenticationProviderUrl;
                }
            }

            private static IEnumerable<AuthenticationDescription> GetAuthenticationProviders(HttpContextBase httpContextBase)
            {
                if (SignInController.authenticationProviders == null)
                {
                    IDictionary<string, IdentityProviderClientConfigurationElement> identityProviderDictionary = GetIdentityProvidersFromConfig();
                    List<AuthenticationDescription> authDescriptions = new List<AuthenticationDescription>();

                    foreach (AuthenticationDescription openIdConnectDescription in httpContextBase.GetOwinContext().Authentication.GetAuthenticationTypes().Where(t => t.Properties.ContainsKey(CookieConstants.Caption)))
                    {
                        KeyValuePair<string, object> authenticationType = openIdConnectDescription.Properties.Single(p => p.Key == CookieConstants.AuthenticationType);
                        KeyValuePair<string, object> caption = openIdConnectDescription.Properties.Single(p => p.Key == CookieConstants.Caption);
                        AuthenticationDescription authDescription = new AuthenticationDescription()
                        {
                            AuthenticationType = authenticationType.Value.ToString(),
                            Caption = caption.Value.ToString()
                        };

                        IdentityProviderClientConfigurationElement identityProvider = identityProviderDictionary[authDescription.AuthenticationType];
                        authDescription.Properties.Add(SignInController.ImageUrl, identityProvider.ImageUrl);
                        authDescription.Properties.Add(SignInController.DisplayIndex, identityProvider.DisplayIndex);

                        authDescriptions.Add(authDescription);
                    }

                    foreach (IdentityProviderClientConfigurationElement identityProvider in identityProviderDictionary.Values)
                    {
                        if (identityProvider.ProviderType == IdentityProviderType.ACS)
                        {
                            AuthenticationDescription authDescription = new AuthenticationDescription()
                            {
                                AuthenticationType = identityProvider.Name,
                                Caption = identityProvider.Name
                            };

                            authDescription.Properties.Add(SignInController.ImageUrl, identityProvider.ImageUrl);
                            authDescription.Properties.Add(SignInController.DisplayIndex, identityProvider.DisplayIndex);
                            authDescriptions.Add(authDescription);
                        }
                    }

                    SignInController.authenticationProviders = authDescriptions.OrderBy(ad => ad.Properties[DisplayIndex]);
                }

                return SignInController.authenticationProviders;
            }

            private static IDictionary<string, IdentityProviderClientConfigurationElement> GetIdentityProvidersFromConfig()
            {
                IDictionary<string, IdentityProviderClientConfigurationElement> identityProvierLookUp = new Dictionary<string, IdentityProviderClientConfigurationElement>();

                RetailConfigurationSection retailConfiguration = (RetailConfigurationSection)ConfigurationManager.GetSection(OpenIdConnectUtilities.ConfigurationSectionName);
                foreach (IdentityProviderClientConfigurationElement provider in retailConfiguration.IdentityProviders)
                {
                    identityProvierLookUp.Add(provider.Name, provider);
                }

                return identityProvierLookUp;
            }

            /// <summary>
            /// Gets result to redirect the caller based on whether a customer account is associated with the logged in user or not.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <param name="authToken">The authentication token.</param>
            /// <param name="identityProviderType">Type of the identity provider.</param>
            /// <param name="email">The email.</param>
            /// <returns>A redirect to route result..</returns>
            private async Task<RedirectToRouteResult> GetRedirectionBasedOnAssociatedCustomer(HttpContextBase httpContextBase, string authToken, IdentityProviderType identityProviderType, string email)
            {
                Tuple<Customer, bool> customerResult = await this.GetExistingCustomer(httpContextBase, authToken, identityProviderType);
                Customer customer = customerResult.Item1;
                if (customer != null)
                {
                    this.UpdateClaims(httpContextBase, authToken, identityProviderType, customer.Email, customer.AccountNumber, customer.FirstName, customer.LastName);
                    return this.RedirectToAction(HomeController.DefaultActionName, HomeController.ControllerName);
                }
                else
                {
                    this.TempData["AuthToken"] = authToken;
                    this.TempData["ExternalIdProviderType"] = identityProviderType;
                    this.TempData["LogOnEmail"] = email;
                    bool isRequestToLinkToExistingCustomerPending = customerResult.Item2;
                    this.TempData["IsActivationPending"] = isRequestToLinkToExistingCustomerPending;

                    return this.RedirectToAction(SignInController.StartSignUpActionName, SignInController.ControllerName);
                }
            }

            /// <summary>
            /// Updates claims with authentication information.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <param name="authToken">The authentication token.</param>
            /// <param name="identityProviderType">Type of the identity provider.</param>
            /// <param name="email">The email.</param>
            /// <param name="accountNumber">The account number.</param>
            /// <param name="firstName">The first name.</param>
            /// <param name="lastName">The last name.</param>
            private void UpdateClaims(HttpContextBase httpContextBase, string authToken, IdentityProviderType identityProviderType, string email, string accountNumber, string firstName, string lastName)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(CookieConstants.Email, email));

                // Split up auth token to prevent exceeding of browser cookie size limitation.
                int midIndex = authToken.Length / 2;
                string authTokenPart1 = authToken.Substring(0, midIndex);
                claims.Add(new Claim(CookieConstants.ExternalTokenPart1, authTokenPart1));

                string authTokenPart2 = authToken.Substring(midIndex);
                this.SaveToEncryptedCookie(httpContextBase, authTokenPart2, CookieConstants.ExternalTokenPart2);

                claims.Add(new Claim(CookieConstants.IdentityProviderType, identityProviderType.ToString()));
                claims.Add(new Claim(CookieConstants.CustomerAccountNumber, accountNumber));
                claims.Add(new Claim(CookieConstants.FirstName, firstName));
                claims.Add(new Claim(CookieConstants.LastName, lastName));
                var id = new ClaimsIdentity(claims, CookieConstants.ApplicationCookieAuthenticationType);
                var ctx = httpContextBase.Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                authenticationManager.SignIn(id);

                OpenIdConnectUtilities.RemoveCookie(OpenIdConnectUtilities.CookieState);
                OpenIdConnectUtilities.RemoveCookie(OpenIdConnectUtilities.CookieNonce);
            }

            /// <summary>
            /// Saves to encrypted cookie.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <param name="userData">The user data.</param>
            /// <param name="cookieName">Name of the cookie.</param>
            private void SaveToEncryptedCookie(HttpContextBase httpContextBase, string userData, string cookieName)
            {
                byte[] encryptedData = MachineKey.Protect(Encoding.ASCII.GetBytes(userData));
                string userDataEncrypted = Convert.ToBase64String(encryptedData);
                HttpCookie cookie = new HttpCookie(cookieName, userDataEncrypted);
                cookie.HttpOnly = true;
                cookie.Secure = true;
                httpContextBase.Response.Cookies.Add(cookie);
            }

            /// <summary>
            /// Gets existing customer.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <param name="idToken">The identifier token.</param>
            /// <param name="identityProviderType">Type of the identity provider.</param>
            /// <returns>The customer associated with the token.</returns>
            private async Task<Tuple<Customer, bool>> GetExistingCustomer(HttpContextBase httpContextBase, string idToken, IdentityProviderType identityProviderType)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(httpContextBase, idToken, identityProviderType);
                Customer currentCustomer = null;
                bool isRequestToLinkToExistingCustomerPending = false;
                try
                {
                    CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                    currentCustomer = await customerOperationsHandler.GetCustomer();
                }
                catch (UserAuthorizationException ex)
                {
                    if (ex.ErrorResourceId == AuthenticationErrors.CommerceIdentityNotFound)
                    {
                        var message = "Customer read failed. No customer is associated with external identity. It is okay to create a new customer.";
                        RetailLogger.Log.OnlineStoreNoCustomerAssociatedWithExternalId(ecommerceContext.IdentityProviderType.ToString(), message, ex);
                    }
                    else if (ex.ErrorResourceId == AuthenticationErrors.UserNotActivated)
                    {
                        var message = "Customer read failed. There is a pending request for linking the current external id to a customer account.";
                        RetailLogger.Log.OnlineStoreInactiveLinkBetweenAnExistingCustomerAndExternalId(ecommerceContext.IdentityProviderType.ToString(), message, ex);
                        isRequestToLinkToExistingCustomerPending = true;
                    }
                    else
                    {
                        throw;
                    }
                }

                return new Tuple<Customer, bool>(currentCustomer, isRequestToLinkToExistingCustomerPending);
            }
        }
    }
}