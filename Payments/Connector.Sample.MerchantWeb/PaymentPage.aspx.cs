/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
namespace Contoso
{
    namespace Retail.SampleConnector.MerchantWeb
    {
        using System;
        using System.Collections.Generic;
        using System.Text;
        using System.Threading;
        using System.Web;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
        using Microsoft.Dynamics.Retail.SDKManager.Portable;

        /// <summary>
        /// The payment page where the payment card page is hosted. This page can be part of eCommerce or any retail solution.
        /// </summary>
        public partial class PaymentPage : System.Web.UI.Page
        {
            // This sample project uses TestConnector. In production, the payment service profile decides which payment connector to use. 
            // This information could also be better place in a config file.
            // Additionally, you should be using a fully qualified assembly name for stronger security. See the commented line below as an example. 
            // private const string ConnectorAssembly = "Microsoft.Dynamics.Retail.TestConnector.Portable, Version=1.0.0.0, Culture=neutral, PublicKeyToken=your public key token goes here";
            private const string ConnectorAssembly = "Microsoft.Dynamics.Retail.TestConnector.Portable";
            private const string ConnectorName = "TestConnector";

            private string industryType;
            private TransactionType transactionType;
            private string supportCardTokenization;
            private string supportCardSwipe;
            private string supportVoiceAuthorization;
            private string showSameAsShippingAddress;
            private string requestLocale;
            private string fontSize;
            private string fontFamily;
            private string labelColor;
            private string textBackgroundColor;
            private string textColor;
            private string disabledTextBackgroundColor;
            private string columnNumber;

            /// <summary>
            /// Gets the URL of the payment accepting page.
            /// </summary>
            public string PaymentAcceptUrl { get; private set; }

            /// <summary>
            /// Gets the background color of the page.
            /// </summary>
            public string PageBackgroundColor { get; private set; }

            /// <summary>
            /// Gets the width of the page.
            /// </summary>
            public string PageWidth { get; private set; }

            /// <summary>
            /// Loads the content of the page.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event data.</param>
            protected void Page_Load(object sender, EventArgs e)
            {
                // Parse query parameters
                string type = Request.QueryString["type"];
                if (!Enum.TryParse<TransactionType>(type, true, out this.transactionType))
                {
                    Response.Redirect("StartPage.aspx");
                }

                this.industryType = Request.QueryString["industrytype"];
                this.supportCardTokenization = Request.QueryString["token"];
                this.supportCardSwipe = Request.QueryString["swipe"];
                this.supportVoiceAuthorization = Request.QueryString["voice"];
                this.showSameAsShippingAddress = Request.QueryString["sameasshipping"];

                this.requestLocale = Request.QueryString["locale"];
                if (this.requestLocale == null)
                {
                    this.requestLocale = Thread.CurrentThread.CurrentUICulture.Name;
                }
                
                this.fontSize = Request.QueryString["fontsize"];
                this.fontFamily = Request.QueryString["fontfamily"];
                this.labelColor = Request.QueryString["labelcolor"];
                this.textBackgroundColor = Request.QueryString["textbackgroundcolor"];
                this.textColor = Request.QueryString["textcolor"];
                this.disabledTextBackgroundColor = Request.QueryString["disabledtextbackgroundcolor"];
                this.columnNumber = Request.QueryString["columnnumber"];

                this.PageBackgroundColor = Request.QueryString["pagebackgroundcolor"];
                this.PageWidth = Request.QueryString["pagewidth"];

                if (!Page.IsPostBack)
                {
                    this.GetPaymentAcceptPoint();
                }
                else
                {
                    this.RetrievePaymentAcceptResult();
                }
            }

            /// <summary>
            /// Initializes the page.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event data.</param>
            protected void Page_Init(object sender, EventArgs e)
            {
                this.ViewStateUserKey = new Guid().ToString();
            }

            private void GetPaymentAcceptPoint()
            {
                // Get payment processor
                PaymentProcessorManager.Create(new string[] { ConnectorAssembly });
                IPaymentProcessor processor = PaymentProcessorManager.GetPaymentProcessor(ConnectorName);

                // Prepare payment request
                var request = new Request();
                request.Locale = this.requestLocale;

                var properties = new List<PaymentProperty>();
                this.AddMerchantProperties(properties);

                PaymentProperty property;
                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardType,
                    "Visa;MasterCard;Amex;Discover;Debit");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.ShowSameAsShippingAddress,
                    this.showSameAsShippingAddress);
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.StreetAddress,
                    "1 Microsoft Way");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.City,
                    "Redmond");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.State,
                    "WA");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.PostalCode,
                    "98052");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Country,
                    "US");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.TransactionType,
                    this.transactionType.ToString());
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.SupportCardSwipe,
                    this.supportCardSwipe);
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.SupportCardTokenization,
                    this.supportCardTokenization);
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.HostPageOrigin,
                    HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
                properties.Add(property);

                property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.IndustryType,
                        this.industryType);
                properties.Add(property);

                if (this.transactionType == TransactionType.Authorize || this.transactionType == TransactionType.Capture)
                {
                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.PurchaseLevel,
                        PurchaseLevel.Level1.ToString());
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.AllowPartialAuthorization,
                        "true");
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.AllowVoiceAuthorization,
                        this.supportVoiceAuthorization);
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.CurrencyCode,
                        "USD");
                    properties.Add(property);
                }

                request.Properties = properties.ToArray();

                // Call 
                Response response = processor.GetPaymentAcceptPoint(request);

                if (response != null && response.Errors == null && response.Properties != null)
                {
                    Hashtable responseProperties = PaymentProperty.ConvertToHashtable(response.Properties);

                    string paymentAcceptUrl;
                    PaymentProperty.GetPropertyValue(
                        responseProperties,
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.PaymentAcceptUrl,
                        out paymentAcceptUrl);

                    if (!string.IsNullOrEmpty(paymentAcceptUrl))
                    {
                        this.PaymentAcceptUrl = string.Format(
                            "{0}&fontsize={1}&fontfamily={2}&labelcolor={3}&textbackgroundcolor={4}&textcolor={5}&disabledtextbackgroundcolor={6}&columnnumber={7}",
                            paymentAcceptUrl,
                            HttpUtility.UrlEncode(this.fontSize),
                            HttpUtility.UrlEncode(this.fontFamily),
                            HttpUtility.UrlEncode(this.labelColor),
                            HttpUtility.UrlEncode(this.textBackgroundColor),
                            HttpUtility.UrlEncode(this.textColor),
                            HttpUtility.UrlEncode(this.disabledTextBackgroundColor),
                            HttpUtility.UrlEncode(this.columnNumber));
                        this.CardPageOriginHiddenField.Value = new Uri(paymentAcceptUrl).GetLeftPart(UriPartial.Authority);
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to retrieve card page. Payment accepting URL is null or empty.");
                    }
                }
                else
                {
                    string errorMessage = string.Empty;
                    if (response == null)
                    {
                        errorMessage = "Response is null. ";
                    }
                    else
                    {
                        if (response.Properties == null)
                        {
                            errorMessage += "Response properties is null. ";
                        }

                        if (response.Errors != null)
                        {
                            errorMessage += "Response contains error(s). ";
                            foreach (var error in response.Errors)
                            {
                                errorMessage += string.Format("{0}: {1}.", error.Code, error.Message);
                            }
                        }
                    }

                    throw new InvalidOperationException("Failed to retrieve card page. " + errorMessage);
                }
            }

            private void RetrievePaymentAcceptResult()
            {
                // Get payment processor
                PaymentProcessorManager.Create(new string[] { ConnectorAssembly });
                IPaymentProcessor processor = PaymentProcessorManager.GetPaymentProcessor(ConnectorName);

                // Prepare payment request
                var request = new Request();
                request.Locale = this.requestLocale;

                var properties = new List<PaymentProperty>();
                this.AddMerchantProperties(properties);

                PaymentProperty property;
                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PaymentAcceptResultAccessCode,
                    this.ResultAccessCodeHiddenField.Value);
                properties.Add(property);

                request.Properties = properties.ToArray();

                // Call 
                Response response = processor.RetrievePaymentAcceptResult(request);

                string cardTokenResult = "N/A";
                string authorizationResult = "N/A";
                string captureResult = "N/A";
                string voidResult = "N/A";
                string errors = "None";
                if (response != null && response.Properties != null)
                {
                    Hashtable responseProperties = PaymentProperty.ConvertToHashtable(response.Properties);

                    // Read card token
                    string token;
                    PaymentProperty.GetPropertyValue(
                        responseProperties,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardToken,
                        out token);

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Store token or use token.
                        // In this sample, we send the token to the next page to display. Do not do this in production.
                        cardTokenResult = token;
                    }

                    // Read authorize result
                    if (this.transactionType == TransactionType.Authorize || this.transactionType == TransactionType.Capture)
                    {
                        PaymentProperty innerAuthorizeResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                            responseProperties,
                            GenericNamespace.AuthorizationResponse,
                            AuthorizationResponseProperties.Properties);

                        if (innerAuthorizeResponseProperty != null)
                        {
                            var innerAuthorizeResponseProperties = PaymentProperty.ConvertToHashtable(innerAuthorizeResponseProperty.PropertyList);

                            string authorizationResultOut = null;
                            PaymentProperty.GetPropertyValue(
                                innerAuthorizeResponseProperties,
                                GenericNamespace.AuthorizationResponse,
                                AuthorizationResponseProperties.AuthorizationResult,
                                out authorizationResultOut);

                            if (!string.IsNullOrEmpty(authorizationResultOut))
                            {
                                authorizationResult = authorizationResultOut;
                            }
                        }
                    }

                    // Read capture result
                    if (this.transactionType == TransactionType.Capture)
                    {
                        PaymentProperty innerCaptureResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                                responseProperties,
                                GenericNamespace.CaptureResponse,
                                CaptureResponseProperties.Properties);

                        if (innerCaptureResponseProperty != null)
                        {
                            var innerCaptureResponseProperties = PaymentProperty.ConvertToHashtable(innerCaptureResponseProperty.PropertyList);

                            string captureResultOut = null;
                            PaymentProperty.GetPropertyValue(
                                innerCaptureResponseProperties,
                                GenericNamespace.CaptureResponse,
                                CaptureResponseProperties.CaptureResult,
                                out captureResultOut);

                            if (!string.IsNullOrEmpty(captureResultOut))
                            {
                                captureResult = captureResultOut;
                            }
                        }
                    }

                    // Read void result
                    if (this.transactionType == TransactionType.Authorize || this.transactionType == TransactionType.Capture)
                    {
                        PaymentProperty innerVoidResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                                responseProperties,
                                GenericNamespace.VoidResponse,
                                VoidResponseProperties.Properties);

                        if (innerVoidResponseProperty != null)
                        {
                            var innerVoidResponseProperties = PaymentProperty.ConvertToHashtable(innerVoidResponseProperty.PropertyList);

                            string voidResultOut = null;
                            PaymentProperty.GetPropertyValue(
                                innerVoidResponseProperties,
                                GenericNamespace.VoidResponse,
                                VoidResponseProperties.VoidResult,
                                out voidResultOut);

                            if (!string.IsNullOrEmpty(voidResultOut))
                            {
                                voidResult = voidResultOut;
                            }
                        }
                    }
                }

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in response.Errors)
                    {
                        sb.AppendLine(string.Format("{0}: {1}", error.Code, error.Message));
                    }

                    errors = sb.ToString();
                }

                Server.Transfer(
                    string.Format(
                        "ResultPage.aspx?cardTokenResult={0}&authorizationResult={1}&captureResult={2}&voidResult={3}&errors={4}",
                        HttpUtility.UrlEncode(cardTokenResult),
                        HttpUtility.UrlEncode(authorizationResult),
                        HttpUtility.UrlEncode(captureResult),
                        HttpUtility.UrlEncode(voidResult),
                        HttpUtility.UrlEncode(errors)));
            }

            private void AddMerchantProperties(List<PaymentProperty> properties)
            {
                PaymentProperty property;
                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.AssemblyName,
                    ConnectorAssembly);
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.ServiceAccountId,
                    "136e9c86-31a1-4177-b2b7-a027c63edbe0");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.MerchantId,
                    "136e9c86-31a1-4177-b2b7-a027c63edbe0");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    "ProviderId",
                    "467079b4-1601-4f79-83c9-f569872eb94e");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    "Environment",
                    "ONEBOX");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.SupportedCurrencies,
                    "USD;CAD");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.SupportedTenderTypes,
                    "Visa;MasterCard;Amex;Discover;Debit");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    "TestString",
                    "Test string 1234567890 1234567890 End.");
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    "TestDecimal",
                    12345.67M);
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    "TestDate",
                    new DateTime(2011, 9, 22, 11, 3, 0));
                properties.Add(property);
            }
        }
    }
}