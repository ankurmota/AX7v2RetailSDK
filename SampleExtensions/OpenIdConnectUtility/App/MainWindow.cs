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
    namespace Retail.Tools.OpendIdConnectUtility
    {
        using System;
        using System.Collections.Specialized;
        using System.Configuration;
        using System.Diagnostics;
        using System.IO;
        using System.Web;
        using System.Windows;
        using System.Windows.Controls;
        using System.Windows.Navigation;
        using System.Windows.Threading;
        using mshtml;

        /// <summary>
        /// Interaction logic for MainWindow.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This is the only window of the application, if it is closed then the application is closed as well therefore no resource leaks.")]
        public class MainWindow : Window
        {
            private const string RequestFormatString = "{0}?client_id={1}&redirect_uri={2}&state={3}&scope={4}&response_type={5}&nonce={6}";

            private const int ParamIndexFilePath = 1;
            private const int ParamIndexEmail = 2;
            private const int ParamIndexPassword = 3;

            private const int ErrorNoConsent = -1;
            private const int ErrorUnknownResponse = -2;
            private const int ErrorIncorrectCredentials = -4;
            private const int ErrorUnsupportedFlow = -5;

            private const int MaxBrowserNavigations = 6;

            private static readonly string AuthenticationEndpoint = ConfigurationManager.AppSettings["AuthenticationEndpoint"].TrimEnd('/');
            private static readonly string ClientId = ConfigurationManager.AppSettings["ClientId"];
            private static readonly string RedirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            private static readonly string AuthorizationRequest = string.Format(RequestFormatString, AuthenticationEndpoint, ClientId, RedirectUri, "myState", "openid", "code id_token", "MyNonce");
            private static readonly string LogOffEndPoint = ConfigurationManager.AppSettings["LogOffEndPoint"].TrimEnd('/');

            private WebBrowser browser;

            /// <summary>
            /// Counter to calculate how many times the browser control completed a navigation process.
            /// </summary>
            /// <remarks>The maximum number is 6 in scenario where a user is signed in: 
            /// 1. Click log out
            /// 2. Redirect to authentication end point
            /// 3. Click on 'sign in with different account'
            /// 4. Click on 'add account'
            /// 5. Click on 'sign in'
            /// 6. Click on 'Allow' offline access
            /// In other scenarios it will be less than 6. 
            /// This counter is needed to avoid infinite loop in case something (let's say UI control's IDs or some other authentication details) is changed on provider's side,
            /// If that happens the utility will just fail rather than enter infinite loop.
            /// </remarks>
            private int browserNavigatedCounter = 0;

            /// <summary>
            /// Boolean value to indicate whether navigation is from log off url. 
            /// </summary>
            /// <remarks>
            /// If navigation is from log off url then user will have to be redirected to the authentication endpoint.
            /// </remarks>
            private bool isLogOff = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="MainWindow" /> class.
            /// </summary>
            public MainWindow()
            {
                this.WindowStyle = WindowStyle.None;
                this.Height = 0;
                this.Width = 0;
                this.browser = new WebBrowser();
                this.browser.Navigated += this.Browser_Navigated;
                this.Content = this.browser;
                this.browser.Navigate(AuthorizationRequest);
            }

            private string Email
            {
                get
                {
                    return GetConfigurableParam(ParamIndexEmail, "email");
                }
            }

            private string Password
            {
                get
                {
                    return GetConfigurableParam(ParamIndexPassword, "password");
                }
            }

            /// <summary>
            /// Gets parameter value either from app parameter or from config file.
            /// </summary>
            /// <param name="argumentIndex">The application argument's index.</param>
            /// <param name="configKey">The application configuration's key name.</param>
            /// <returns>The parameter value.</returns>
            private static string GetConfigurableParam(int argumentIndex, string configKey)
            {
                string result;
                string[] args = Environment.GetCommandLineArgs();
                int argsCount = args.Length;
                if (argsCount == HostApplication.ParamNumDefaultAccount)
                {
                    result = ConfigurationManager.AppSettings[configKey];
                }
                else
                {
                    result = args[argumentIndex];
                }

                return result;
            }

            /// <summary>
            /// Is fired each time when browser completed a navigation process.
            /// </summary>
            /// <param name="sender">Reference to the browser.</param>
            /// <param name="e">Event arguments.</param>
            private void Browser_Navigated(object sender, NavigationEventArgs e)
            {
                if (this.isLogOff)
                {
                    // If user is redirected because of log off, redirect user to authentication end point.
                    this.isLogOff = false;
                    this.browser.Navigate(AuthorizationRequest);
                }
                else
                {
                    if (++this.browserNavigatedCounter > MaxBrowserNavigations)
                    {
                        Environment.Exit(ErrorUnsupportedFlow);
                    }

                    this.StartTimer(this.OnBrowserNavigatedDelayed);
                }
            }

            /// <summary>
            /// Is called with a delay after a browser completed a navigation process.
            /// </summary>
            /// <param name="sender">Reference to the timer created the delay.</param>
            /// <param name="e">Event arguments.</param>
            private void OnBrowserNavigatedDelayed(object sender, EventArgs e)
            {
                IHTMLDocument3 document = this.StopTimerAndGetDocument(sender);

                IHTMLElement errorMessage = document.getElementById("errormsg_0_Passwd");
                if (errorMessage != null)
                {
                    Trace.TraceError("The credentials provided are not correct. email='{0}'; password='{1}'", this.Email, this.Password);
                    Environment.Exit(ErrorIncorrectCredentials);
                }

                // Get all elements required from the page.
                string emailReauthId = "account-" + this.Email;
                IHTMLElement allowOfflineAccessButton = document.getElementById("submit_approve_access") as IHTMLElement;
                IHTMLInputElement emailTextBox = document.getElementById("Email") as IHTMLInputElement;
                IHTMLElement reauthEmailText = document.getElementById("reauthEmail") as IHTMLElement;
                IHTMLElement signInWithDifferentAccountLink = document.getElementById("account-chooser-link") as IHTMLElement;
                IHTMLElement emailList = document.getElementById(emailReauthId) as IHTMLElement;
                IHTMLElement addAccountLink = document.getElementById("account-chooser-add-account") as IHTMLElement;

                if (allowOfflineAccessButton != null)
                {
                    // If in the allow offline access page.
                    // When navigating to the authorization end point url if user is already signed in, navigate to log off url
                    // Otherwise, click on allow offline access
                    if (this.browserNavigatedCounter == 1)
                    {
                        this.isLogOff = true;
                        this.browser.Navigate(LogOffEndPoint);
                    }
                    else
                    {
                        this.StartTimer(this.OnConsentDisplayed);
                    }
                }
                else if (emailTextBox != null && !emailTextBox.readOnly)
                {
                    // Check for email text box, if present enter email, password and click sign in
                    // User is prompted for email id and password if logging in for first time
                    IHTMLInputElement textBoxPassword = document.getElementById("Passwd") as IHTMLInputElement;
                    IHTMLElement buttonSignIn = document.getElementById("signIn") as IHTMLElement;

                    emailTextBox.value = this.Email;
                    textBoxPassword.value = this.Password;
                    buttonSignIn.click();
                }
                else if (reauthEmailText != null && signInWithDifferentAccountLink != null)
                {
                    // If the single or default email from the session is chosen already and user is request for password
                    // Check if the email matches the current email. If yes, enter password and click sign in
                    // If no, click on sign in with different account
                    if (this.Email.Equals(reauthEmailText.innerText, StringComparison.OrdinalIgnoreCase))
                    {
                        IHTMLInputElement textBoxPassword = document.getElementById("Passwd") as IHTMLInputElement;
                        IHTMLElement buttonSignIn = document.getElementById("signIn") as IHTMLElement;

                        textBoxPassword.value = this.Password;
                        buttonSignIn.click();
                    }
                    else
                    {
                        signInWithDifferentAccountLink.click();
                    }
                }
                else if (emailList != null)
                {
                    // If accounts are displayed as a list, check if current email matches with one of them.
                    // If yes, click the account, enter password and click sign in
                    // If not click "Add acount", enter email and password and click sign in
                    emailList.children[0].click();
                }
                else if (addAccountLink != null)
                {
                    addAccountLink.click();
                }
                else
                {
                    string fileName = Environment.GetCommandLineArgs()[ParamIndexFilePath];
                    string fileContent;

                    dynamic doc = this.browser.Document;
                    string title = doc.Title;
                    if (title.StartsWith("Success", StringComparison.OrdinalIgnoreCase))
                    {
                        NameValueCollection parameters = HttpUtility.ParseQueryString(title);
                        fileContent = parameters["id_token"];
                        Trace.TraceInformation("id_token was successfully received and being saved into the file " + fileName);
                    }
                    else
                    {
                        if (title.StartsWith("Denied", StringComparison.OrdinalIgnoreCase))
                        {
                            Trace.TraceWarning("id_token was not recevied because consent was not acquired");
                            Environment.ExitCode = ErrorNoConsent;
                            fileContent = title;
                        }
                        else
                        {
                            Trace.TraceError("id_token was not received because of unknown response: " + title);
                            Environment.ExitCode = ErrorUnknownResponse;
                            fileContent = "Uknown response: " + title;
                        }
                    }

                    File.WriteAllText(fileName, fileContent);
                    this.Close();
                }
            }

            /// <summary>
            /// Stops the timer and reads the DOM document.
            /// </summary>
            /// <param name="timer">The timer.</param>
            /// <returns>The document.</returns>
            private IHTMLDocument3 StopTimerAndGetDocument(object timer)
            {
                (timer as DispatcherTimer).Stop();
                IHTMLDocument3 document = this.browser.Document as IHTMLDocument3;
                return document;
            }

            /// <summary>
            /// Callback to indicate that consent screen has been rendered and is ready to be analyzed.
            /// </summary>
            /// <param name="sender">Reference to the browser.</param>
            /// <param name="args">Event arguments.</param>
            private void OnConsentDisplayed(object sender, EventArgs args)
            {
                IHTMLDocument3 document = this.StopTimerAndGetDocument(sender);
                IHTMLElement buttonAccept = document.getElementById("submit_approve_access") as IHTMLElement;
                buttonAccept.click();
            }

            /// <summary>
            /// The timer is needed to make sure that requested operation is executed in asynchronous way.
            /// </summary>
            /// <param name="handler">The handler to be executed once the timer expires.</param>
            private void StartTimer(EventHandler handler)
            {
                new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, handler, this.Dispatcher);
            }
        }
    }
}