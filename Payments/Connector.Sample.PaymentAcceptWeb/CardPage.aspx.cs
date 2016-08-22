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
    namespace Retail.SampleConnector.PaymentAcceptWeb
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Web;
        using System.Web.UI;
        using System.Web.UI.WebControls;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
        using Microsoft.Dynamics.Retail.SDKManager.Portable;
        using Newtonsoft.Json;
        using Resources;
        using SampleConnector.PaymentAcceptWeb.Data;
        using SampleConnector.PaymentAcceptWeb.Models;
        using SampleConnector.PaymentAcceptWeb.Utilities;

        /// <summary>
        /// The card payment page.
        /// </summary>
        public partial class CardPage : System.Web.UI.Page
        {
            private string entryId;
            private CardPaymentEntry entry;
            private bool isSwipe;

            private string track1;
            private string track2;
            private string cardType;
            private string cardNumber;
            private int cardExpirationMonth;
            private int cardExpirationYear;
            private string cardSecurityCode;
            private string voiceAuthorizationCode;
            private string cardHolderName;
            private string cardStreet1;
            private string cardCity;
            private string cardStateOrProvince;
            private string cardPostalCode;
            private string cardCountryOrRegion;
            private decimal paymentAmount;
            private decimal approvedAmount;

            /// <summary>
            /// Gets the custom styles.
            /// </summary>
            public CustomStyles CustomStyles { get; private set; }

            /// <summary>
            /// Gets the text direction.
            /// </summary>
            public string TextDirection { get; private set; }

            /// <summary>
            /// Gets the error message for invalid track data.
            /// </summary>
            public string InvalidCardTrackDataMessage
            {
                get { return WebResources.CardPage_InvalidCardTrackData; }
            }

            /// <summary>
            /// Gets the error message for communication error.
            /// </summary>
            public string CommunicationErrorMessage
            {
                get { return WebResources.CardPage_CommunicationError; }
            }

            /// <summary>
            /// Initializes culture (language) of the page.
            /// </summary>
            protected override void InitializeCulture()
            {
                this.entryId = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(this.entryId))
                {
                    // Find card payment entry by entry ID.
                    var dataManager = new DataManager();
                    this.entry = dataManager.GetCardPaymentEntryByEntryId(this.entryId);

                    if (this.entry != null && !this.entry.Used)
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(this.entry.EntryLocale);
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.entry.EntryLocale);
                    }
                }

                base.InitializeCulture();
            }

            /// <summary>
            /// Initializes the page.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event data.</param>
            protected void Page_Init(object sender, EventArgs e)
            {
                if (this.entry != null)
                {
                    this.ViewStateUserKey = this.entry.EntryId;
                }
            }

            /// <summary>
            /// Loads the content of the page.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event data.</param>
            protected void Page_Load(object sender, EventArgs e)
            {
                // Enable Right-to-left for some cultures
                this.TextDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "ltr";

                // Load custom styles
                this.LoadCustomStyles();

                // Check payment entry ID
                if (string.IsNullOrEmpty(this.entryId))
                {
                    this.ShowErrorMessage(WebResources.CardPage_Error_MissingRequestId);
                    return;
                }

                // Check payment entry
                if (this.entry == null)
                {
                    this.ShowErrorMessage(WebResources.CardPage_Error_InvalidRequestId);
                    return;
                }
                else if (this.entry.Used)
                {
                    this.ShowErrorMessage(WebResources.CardPage_Error_UsedRequest);
                    return;
                }
                else if (this.entry.IsExpired)
                {
                    this.ShowErrorMessage(WebResources.CardPage_Error_RequestTimedOut);
                    return;
                }

                if (!Page.IsPostBack)
                {
                    this.InitilizePageControls();
                }
                else
                {
                    this.SubmitPayment();
                }
            }

            /// <summary>
            /// Initializes the state of page controls based on the entry.
            /// </summary>
            private void InitilizePageControls()
            {
                bool isRetail = IndustryType.Retail.ToString().Equals(this.entry.IndustryType, StringComparison.OrdinalIgnoreCase);
                this.CardDetailsHeaderPanel.Visible = !isRetail;

                // Load card entry modes
                this.CardEntryModePanel.Visible = this.entry.SupportCardSwipe || this.entry.AllowVoiceAuthorization;
                if (this.CardEntryModePanel.Visible)
                {
                    if (this.entry.SupportCardSwipe)
                    {
                        this.CardEntryModeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardEntryModeDropDownList_Swipe, "swipe"));
                    }

                    this.CardEntryModeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardEntryModeDropDownList_Manual, "manual"));

                    if (this.entry.AllowVoiceAuthorization)
                    {
                        this.CardEntryModeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardEntryModeDropDownList_Voice, "voice"));
                    }
                }

                this.CardHolderNamePanel.Visible = !isRetail;
                this.CardTypePanel.Visible = !isRetail;

                // Load card types
                if (this.CardTypePanel.Visible)
                {
                    this.CardTypeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardTypeDropDownList_PleaseSelect, string.Empty));
                    string[] cardTypes = CardTypes.ToArray(this.entry.CardTypes);
                    foreach (var cardType in cardTypes)
                    {
                        if (CardTypes.Amex.Equals(cardType, StringComparison.OrdinalIgnoreCase))
                        {
                            this.CardTypeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardTypeDropDownList_AmericanExpress, CardTypes.Amex));
                        }
                        else if (CardTypes.Discover.Equals(cardType, StringComparison.OrdinalIgnoreCase))
                        {
                            this.CardTypeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardTypeDropDownList_Discover, CardTypes.Discover));
                        }
                        else if (CardTypes.MasterCard.Equals(cardType, StringComparison.OrdinalIgnoreCase))
                        {
                            this.CardTypeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardTypeDropDownList_MasterCard, CardTypes.MasterCard));
                        }
                        else if (CardTypes.Visa.Equals(cardType, StringComparison.OrdinalIgnoreCase))
                        {
                            this.CardTypeDropDownList.Items.Add(new ListItem(WebResources.CardPage_CardTypeDropDownList_Visa, CardTypes.Visa));
                        }
                    }
                }

                // Load month list
                this.ExpirationMonthDropDownList.Items.Add(new ListItem(WebResources.CardPage_ExpirationMonthDropDownList_PleaseSelect, "0"));
                string[] monthNames = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
                for (int i = 1; i <= 12; i++)
                {
                    this.ExpirationMonthDropDownList.Items.Add(new ListItem(monthNames[i - 1], string.Format("{0}", i)));
                }

                // Load year list
                this.ExpirationYearDropDownList.Items.Add(new ListItem(WebResources.CardPage_ExpirationYearDropDownList_PleaseSelect));
                int currentYear = DateTime.UtcNow.Year;
                for (int i = 0; i < 20; i++)
                {
                    this.ExpirationYearDropDownList.Items.Add(new ListItem(string.Format("{0}", currentYear + i)));
                }

                // Show/hide security code and voice authorization code
                this.SecurityCodePanel.Visible = false;
                this.VoiceAuthorizationCodePanel.Visible = false;
                TransactionType transactionType = (TransactionType)Enum.Parse(typeof(TransactionType), this.entry.TransactionType, true);
                if (transactionType == TransactionType.Authorize || transactionType == TransactionType.Capture)
                {
                    this.SecurityCodePanel.Visible = true;

                    if (this.entry.AllowVoiceAuthorization)
                    {
                        this.VoiceAuthorizationCodePanel.Visible = true;
                    }
                }

                this.ZipPanel.Visible = isRetail;
                this.BillingAddressPanel.Visible = !isRetail;
                if (this.BillingAddressPanel.Visible)
                {
                    // Load country list
                    // Note: the value of country/region must be two-letter ISO code.
                    // TO DO: Filter the countries down to the list you support.
                    this.CountryRegionDropDownList.Items.Add(new ListItem(WebResources.CardPage_CountryRegionDropDownList_PleaseSelect, string.Empty));
                    var dataManager = new DataManager();
                    IEnumerable<CountryOrRegion> countries = dataManager.GetCountryRegionListByLocale(this.entry.EntryLocale);
                    countries = countries.OrderBy(c => c.LongName).ToList();
                    foreach (var country in countries)
                    {
                        this.CountryRegionDropDownList.Items.Add(new ListItem(country.LongName, country.TwoLetterCode));
                    }

                    // Populate default values
                    if (this.CardHolderNamePanel.Visible)
                    {
                        this.CardHolderNameTextBox.Text = this.entry.DefaultCardHolderName;
                    }

                    if (this.entry.ShowSameAsShippingAddress)
                    {
                        this.SameAsShippingPanel.Visible = true;
                        this.DefaultStreetHiddenField.Value = this.entry.DefaultStreet1;
                        this.DefaultCityHiddenField.Value = this.entry.DefaultCity;
                        this.DefaultStateProvinceHiddenField.Value = this.entry.DefaultStateOrProvince;
                        this.DefaultZipHiddenField.Value = this.entry.DefaultPostalCode;
                        this.DefaultCountryRegionHiddenField.Value = this.entry.DefaultCountryCode;
                    }
                    else
                    {
                        this.SameAsShippingPanel.Visible = false;
                        this.StreetTextBox.Text = this.entry.DefaultStreet1;
                        this.CityTextBox.Text = this.entry.DefaultCity;
                        this.StateProvinceTextBox.Text = this.entry.DefaultStateOrProvince;
                        this.ZipTextBox1.Text = this.entry.DefaultPostalCode;
                        this.CountryRegionDropDownList.SelectedValue = this.entry.DefaultCountryCode;
                    }
                }

                this.HostPageOriginHiddenField.Value = this.entry.HostPageOrigin;
            }

            /// <summary>
            /// Submits the payment transaction e.g. tokenization, authorization, capture.
            /// </summary>
            private void SubmitPayment()
            {
                bool.TryParse(this.IsSwipeHiddenField.Value, out this.isSwipe);

                // Validate inputs
                if (this.ValidateInputs())
                {
                    if (this.isSwipe)
                    {
                        this.track1 = this.CardTrack1HiddenField.Value;
                        this.track2 = this.CardTrack2HiddenField.Value;
                        this.cardNumber = this.CardNumberHiddenField.Value;
                    }
                    else
                    {
                        this.cardNumber = this.CardNumberTextBox.Text.Trim();
                    }

                    if (this.CardTypePanel.Visible)
                    {
                        this.cardType = this.CardTypeDropDownList.SelectedItem.Value;
                    }
                    else
                    {
                        this.cardType = this.CardTypeHiddenField.Value;
                    }

                    this.cardExpirationMonth = int.Parse(this.ExpirationMonthDropDownList.SelectedItem.Value);
                    this.cardExpirationYear = int.Parse(this.ExpirationYearDropDownList.SelectedItem.Text);
                    this.cardSecurityCode = this.SecurityCodeTextBox.Text.Trim();
                    this.voiceAuthorizationCode = this.VoiceAuthorizationCodeTextBox.Text.Trim();

                    if (this.CardHolderNamePanel.Visible)
                    {
                        this.cardHolderName = this.CardHolderNameTextBox.Text.Trim();
                    }
                    else
                    {
                        this.cardHolderName = this.CardHolderNameHiddenField.Value.Trim();
                    }

                    this.cardStreet1 = this.StreetTextBox.Text.Trim();
                    this.cardCity = this.CityTextBox.Text.Trim();
                    this.cardStateOrProvince = this.StateProvinceTextBox.Text.Trim();

                    if (this.BillingAddressPanel.Visible)
                    {
                        this.cardPostalCode = this.ZipTextBox1.Text.Trim();
                    }
                    else
                    {
                        this.cardPostalCode = this.ZipTextBox2.Text.Trim();
                    }

                    this.cardCountryOrRegion = this.CountryRegionDropDownList.SelectedValue;

                    if (!string.IsNullOrEmpty(this.PaymentAmountHiddenField.Value))
                    {
                        this.paymentAmount = decimal.Parse(this.PaymentAmountHiddenField.Value, NumberStyles.Number, Thread.CurrentThread.CurrentCulture);
                    }

                    if (!string.IsNullOrEmpty(this.ApprovedAmountHiddenField.Value))
                    {
                        this.approvedAmount = decimal.Parse(this.ApprovedAmountHiddenField.Value, NumberStyles.Number, Thread.CurrentThread.CurrentCulture);
                    }

                    // Process payment, e.g. tokenize, authorize, capture.
                    try
                    {
                        this.ProcessPayment();
                    }
                    catch (CardPaymentException ex)
                    {
                        // Return the errors from UX
                        this.InputErrorsHiddenField.Value = JsonConvert.SerializeObject(ex.PaymentErrors);
                    }
                }
            }

            /// <summary>
            /// Loads custom styles to change look and feel of the page.
            /// </summary>
            private void LoadCustomStyles()
            {
                // Encode style value to prevent XSS attack
                this.CustomStyles = CustomStyles.Default;

                string fontSize = Request.QueryString["fontsize"];
                if (!string.IsNullOrWhiteSpace(fontSize))
                {
                    this.CustomStyles.FontSize = HttpUtility.HtmlEncode(fontSize);
                }

                string fontFamily = Request.QueryString["fontfamily"];
                if (!string.IsNullOrWhiteSpace(fontFamily))
                {
                    // Add the default font as backup in case that the asked font is not supported.
                    fontFamily = string.Format("{0},{1}", fontFamily, CustomStyles.Default.FontFamily);
                    fontFamily = HttpUtility.HtmlEncode(fontFamily);

                    // Decode single quotations and double quotations to make fonts like "Times New Roman" work.
                    fontFamily = fontFamily.Replace("&#39;", "'").Replace("&quot;", "\"");

                    this.CustomStyles.FontFamily = fontFamily;
                }
                
                string labelColor = Request.QueryString["labelcolor"];
                if (!string.IsNullOrWhiteSpace(labelColor))
                {
                    this.CustomStyles.LabelColor = HttpUtility.HtmlEncode(labelColor);
                }

                string textBackgroundColor = Request.QueryString["textbackgroundcolor"];
                if (!string.IsNullOrWhiteSpace(textBackgroundColor))
                {
                    this.CustomStyles.TextBackgroundColor = HttpUtility.HtmlEncode(textBackgroundColor);
                }

                string textColor = Request.QueryString["textcolor"];
                if (!string.IsNullOrWhiteSpace(textColor))
                {
                    this.CustomStyles.TextColor = HttpUtility.HtmlEncode(textColor);
                }

                string disabledTextBackgroundColor = Request.QueryString["disabledtextbackgroundcolor"];
                if (!string.IsNullOrWhiteSpace(disabledTextBackgroundColor))
                {
                    this.CustomStyles.DisabledTextBackgroundColor = HttpUtility.HtmlEncode(disabledTextBackgroundColor);
                }

                string columnnumber = Request.QueryString["columnnumber"];
                if (!string.IsNullOrWhiteSpace(columnnumber))
                {
                    int number;
                    if (int.TryParse(columnnumber, out number) && number >= 1 && number <= 2)
                    {
                        this.CustomStyles.ColumnNumber = number;
                    }
                }

                // Change the page to one column by applying a CSS style
                if (this.CustomStyles.ColumnNumber == 1)
                {
                    this.CardPanel.CssClass += " msax-DisableTable";
                    this.CardRowPanel.CssClass += " msax-DisableRow";
                    this.CardDetailsPanel.CssClass += " msax-DisableCell";
                    this.BillingAddressPanel.CssClass += " msax-DisableCell";
                }
            }

            /// <summary>
            /// Shows error message on the web page.
            /// </summary>
            /// <param name="error">The error message.</param>
            private void ShowErrorMessage(string error)
            {
                // Hide other controls
                this.CardPanel.Visible = false;

                // Show error message
                this.ErrorMessageLabel.Text = error;
                this.ErrorPanel.Visible = true;
            }

            /// <summary>
            /// Process the card payment, e.g. Tokenize, Authorize, Capture.
            /// </summary>
            private void ProcessPayment()
            {
                // Get payment processor
                PaymentProcessorManager.Create(new string[] { AppSettings.ConnectorAssembly });
                IPaymentProcessor processor = PaymentProcessorManager.GetPaymentProcessor(AppSettings.ConnectorName);

                // Prepare payment request properties
                List<PaymentProperty> requestProperties = this.GetCommonPaymentRequestProperties();

                bool isPartialAuthorizationPending = bool.TrueString.Equals(this.IsPartialAuthorizationPendingHiddenField.Value, StringComparison.OrdinalIgnoreCase);
                if (!isPartialAuthorizationPending)
                {
                    this.ProcessNewPayment(processor, requestProperties);
                }
                else
                {
                    this.ProcessPendingPartialAuthorization(processor, requestProperties);
                }
            }

            private void ProcessNewPayment(IPaymentProcessor processor, List<PaymentProperty> requestProperties)
            {
                // A brank new payment...
                // Tokenize the card if requested
                Response tokenizeResponse = null;
                if (this.entry.SupportCardTokenization)
                {
                    tokenizeResponse = this.Tokenize(processor, requestProperties);
                }

                // Authorize and Capture if requested
                // Do not authorize if tokenization failed.
                Response authorizeResponse = null;
                Response captureResponse = null;
                Response voidResponse = null;
                TransactionType transactionType = (TransactionType)Enum.Parse(typeof(TransactionType), this.entry.TransactionType, true);
                if (transactionType == TransactionType.Authorize || transactionType == TransactionType.Capture)
                {
                    authorizeResponse = this.Authorize(processor, requestProperties);

                    // Check authorization result
                    bool isAuthorizeFailed = false;
                    PaymentProperty innerAuthorizeResponseProperty = null;
                    Hashtable innerAuthorizeResponseProperties = null;
                    if (authorizeResponse == null
                        || authorizeResponse.Properties == null
                        || (authorizeResponse.Errors != null && authorizeResponse.Errors.Length > 0))
                    {
                        isAuthorizeFailed = true;
                    }
                    else
                    {
                        var authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                        innerAuthorizeResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                            authorizeResponseProperties,
                            GenericNamespace.AuthorizationResponse,
                            AuthorizationResponseProperties.Properties);

                        innerAuthorizeResponseProperties = PaymentProperty.ConvertToHashtable(innerAuthorizeResponseProperty.PropertyList);

                        string authorizationResult = null;
                        PaymentProperty.GetPropertyValue(
                            innerAuthorizeResponseProperties,
                            GenericNamespace.AuthorizationResponse,
                            AuthorizationResponseProperties.AuthorizationResult,
                            out authorizationResult);

                        // TO DO: In this sample, we only check the authorization results. CVV2 result and AVS result are ignored. 
                        if (!AuthorizationResult.Success.ToString().Equals(authorizationResult, StringComparison.OrdinalIgnoreCase)
                            && !AuthorizationResult.PartialAuthorization.ToString().Equals(authorizationResult, StringComparison.OrdinalIgnoreCase))
                        {
                            isAuthorizeFailed = true;
                        }
                    }

                    if (!isAuthorizeFailed)
                    {
                        // Authorize success or partial authorization success...
                        // Get authorized amount
                        decimal authorizedAmount = 0m;
                        PaymentProperty.GetPropertyValue(
                            innerAuthorizeResponseProperties,
                            GenericNamespace.AuthorizationResponse,
                            AuthorizationResponseProperties.ApprovedAmount,
                            out authorizedAmount);

                        if (this.paymentAmount != authorizedAmount)
                        {
                            // Partial authorization need user confirmation
                            this.IsPartialAuthorizationPendingHiddenField.Value = bool.TrueString;
                            this.ApprovedAmountHiddenField.Value = authorizedAmount.ToString();
                        }
                        else
                        {
                            // Full authorization
                            if (transactionType == TransactionType.Capture)
                            {
                                this.Capture(processor, requestProperties, innerAuthorizeResponseProperty, out captureResponse, out voidResponse);
                            }
                        }
                    }
                    else
                    {
                        // Authorization failure, Throw an exception and stop the payment.
                        var errors = new List<PaymentError>();
                        errors.Add(new PaymentError(ErrorCode.AuthorizationFailure, "Authorization failure."));
                        throw new CardPaymentException("Authorization failure.", errors);
                    }
                }

                // Combine responses into one.
                Response paymentResponse = this.CombineResponses(tokenizeResponse, authorizeResponse, captureResponse, voidResponse);
                if (paymentResponse != null)
                {
                    // Success
                    paymentResponse.Properties = PaymentProperty.RemoveDataEncryption(paymentResponse.Properties);

                    // Save result
                    var result = new CardPaymentResult();
                    result.EntryId = this.entry.EntryId;
                    result.ResultAccessCode = CommonUtility.NewGuid().ToString();
                    result.ResultData = JsonConvert.SerializeObject(paymentResponse);
                    result.Retrieved = false;
                    result.ServiceAccountId = this.entry.ServiceAccountId;

                    var dataManager = new DataManager();
                    dataManager.CreateCardPaymentResult(result);

                    // Mark the entry as used if there is no partial authorization pending
                    if (!bool.TrueString.Equals(this.IsPartialAuthorizationPendingHiddenField.Value))
                    {
                        dataManager.UpdateCardPaymentEntryAsUsed(this.entry.ServiceAccountId, this.entry.EntryId);
                    }

                    // Set request access code in hidden field for return
                    this.ResultAccessCodeHiddenField.Value = result.ResultAccessCode;
                }
                else
                {
                    this.InputErrorsHiddenField.Value = WebResources.CardPage_Error_InvalidCard;
                }
            }

            private void ProcessPendingPartialAuthorization(IPaymentProcessor processor, List<PaymentProperty> requestProperties)
            {
                // Add transaction amount
                var property = new PaymentProperty(
                            GenericNamespace.TransactionData,
                            TransactionDataProperties.Amount,
                            this.approvedAmount);
                requestProperties.Add(property);

                // Read the pending result which contains the authorization response.
                var dataManager = new DataManager();
                CardPaymentResult result = dataManager.GetCardPaymentResultByResultAccessCode(this.entry.ServiceAccountId, this.ResultAccessCodeHiddenField.Value);
                Response authorizeResponse = JsonConvert.DeserializeObject<Response>(result.ResultData);
                var authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                PaymentProperty innerAuthorizeResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                    authorizeResponseProperties,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.Properties);

                Response captureResponse = null;
                Response voidResponse = null;
                bool isPartialAuthorizationConfirmed = bool.TrueString.Equals(this.IsPartialAuthorizationConfirmedHiddenField.Value, StringComparison.OrdinalIgnoreCase);
                if (isPartialAuthorizationConfirmed)
                {
                    // User agrees to partial authorization.
                    // Continue to capture payment if needed
                    TransactionType transactionType = (TransactionType)Enum.Parse(typeof(TransactionType), this.entry.TransactionType, true);
                    if (transactionType == TransactionType.Capture)
                    {
                        this.Capture(processor, requestProperties, innerAuthorizeResponseProperty, out captureResponse, out voidResponse);
                    }
                }
                else
                {
                    // User disagrees to partial authorization.
                    // Void the partial authorization.
                    property = new PaymentProperty(
                        GenericNamespace.AuthorizationResponse,
                        AuthorizationResponseProperties.Properties,
                        innerAuthorizeResponseProperty.PropertyList);
                    requestProperties.Add(property);

                    var paymentRequest = new Request();
                    paymentRequest.Locale = this.entry.EntryLocale;
                    paymentRequest.Properties = requestProperties.ToArray();
                    voidResponse = processor.Void(paymentRequest);
                }

                // Combine responses into one
                Response paymentResponse = this.CombineResponses(null, authorizeResponse, captureResponse, voidResponse);
                paymentResponse.Properties = PaymentProperty.RemoveDataEncryption(paymentResponse.Properties);

                // Update result
                string newResultData = JsonConvert.SerializeObject(paymentResponse);
                dataManager.UpdateCardPaymentResultData(result.ServiceAccountId, result.ResultAccessCode, newResultData);

                // Mark the entry as used if there is no partial authorization pending
                dataManager.UpdateCardPaymentEntryAsUsed(this.entry.ServiceAccountId, this.entry.EntryId);

                // Set request access code in hidden field for return
                this.ResultAccessCodeHiddenField.Value = result.ResultAccessCode;

                // Since result is already created, once the pending flag is reset, the result access code will be sent to host page.
                this.IsPartialAuthorizationPendingHiddenField.Value = bool.FalseString;
            }

            private List<PaymentProperty> GetCommonPaymentRequestProperties()
            {
                var requestProperties = new List<PaymentProperty>();

                // Get payment properties from payment entry which contains the merchant information.
                Request entryData = JsonConvert.DeserializeObject<Request>(this.entry.EntryData);
                PaymentProperty[] entryPaymentProperties = entryData.Properties;

                // Filter payment card properties (they are default card data, not final card data)
                foreach (var entryPaymentProperty in entryPaymentProperties)
                {
                    if (entryPaymentProperty.Namespace != GenericNamespace.PaymentCard)
                    {
                        requestProperties.Add(entryPaymentProperty);
                    }
                }

                // Add final card data
                PaymentProperty property;
                if (this.isSwipe)
                {
                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardEntryType,
                        CardEntryTypes.MagneticStripeRead.ToString());
                    requestProperties.Add(property);

                    if (!string.IsNullOrWhiteSpace(this.track1))
                    {
                        property = new PaymentProperty(
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.Track1,
                            this.track1);
                        requestProperties.Add(property);
                    }

                    if (!string.IsNullOrWhiteSpace(this.track2))
                    {
                        property = new PaymentProperty(
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.Track2,
                            this.track2);
                        requestProperties.Add(property);
                    }
                }
                else
                {
                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardEntryType,
                        CardEntryTypes.ManuallyEntered.ToString());
                    requestProperties.Add(property);
                }

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardType,
                    this.cardType);
                requestProperties.Add(property);

                property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardNumber,
                        this.cardNumber);
                requestProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.ExpirationMonth,
                    this.cardExpirationMonth);
                requestProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.ExpirationYear,
                    this.cardExpirationYear);
                requestProperties.Add(property);

                if (!string.IsNullOrWhiteSpace(this.cardSecurityCode))
                {
                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.AdditionalSecurityData,
                        this.cardSecurityCode);
                    requestProperties.Add(property);
                }

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Name,
                    this.cardHolderName);
                requestProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.StreetAddress,
                    this.cardStreet1);
                requestProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.City,
                    this.cardCity);
                requestProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.State,
                    this.cardStateOrProvince);
                requestProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.PostalCode,
                    this.cardPostalCode);
                requestProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Country,
                    this.cardCountryOrRegion);
                requestProperties.Add(property);

                return requestProperties;
            }

            private Response Tokenize(IPaymentProcessor processor, List<PaymentProperty> requestProperties)
            {
                var paymentRequest = new Request();
                paymentRequest.Locale = this.entry.EntryLocale;
                paymentRequest.Properties = requestProperties.ToArray();
                Response tokenizeResponse = processor.GenerateCardToken(paymentRequest, null);
                if (tokenizeResponse.Errors != null && tokenizeResponse.Errors.Any())
                {
                    // Tokenization failure, Throw an exception and stop the payment.
                    throw new CardPaymentException("Tokenization failure.", tokenizeResponse.Errors);
                }

                return tokenizeResponse;
            }

            private Response Authorize(IPaymentProcessor processor, List<PaymentProperty> requestProperties)
            {
                PaymentProperty property = null;

                // Add request properties for Authorize
                if (!string.IsNullOrWhiteSpace(this.voiceAuthorizationCode))
                {
                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.VoiceAuthorizationCode,
                        this.voiceAuthorizationCode);
                    requestProperties.Add(property);
                }

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.Amount,
                    this.paymentAmount);
                requestProperties.Add(property);

                // Authorize payment
                var paymentRequest = new Request();
                paymentRequest.Locale = this.entry.EntryLocale;
                paymentRequest.Properties = requestProperties.ToArray();
                Response authorizeResponse = processor.Authorize(paymentRequest, null);
                if (authorizeResponse.Errors != null && authorizeResponse.Errors.Any())
                {
                    // Authorization failure, Throw an exception and stop the payment.
                    throw new CardPaymentException("Authorization failure.", authorizeResponse.Errors);
                }

                return authorizeResponse;
            }

            private void Capture(IPaymentProcessor processor, List<PaymentProperty> requestProperties, PaymentProperty innerAuthorizeResponseProperty, out Response captureResponse, out Response voidResponse)
            {
                // Capture payment
                var property = new PaymentProperty(
                        GenericNamespace.AuthorizationResponse,
                        AuthorizationResponseProperties.Properties,
                        innerAuthorizeResponseProperty.PropertyList);
                requestProperties.Add(property);

                var paymentRequest = new Request();
                paymentRequest.Locale = this.entry.EntryLocale;
                paymentRequest.Properties = requestProperties.ToArray();
                captureResponse = processor.Capture(paymentRequest);

                // Check capture result
                bool isCaptureFailed = false;
                if (captureResponse == null
                    || captureResponse.Properties == null
                    || (captureResponse.Errors != null && captureResponse.Errors.Length > 0))
                {
                    isCaptureFailed = true;
                }
                else
                {
                    var captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                    PaymentProperty innerCaptureResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                        captureResponseProperties,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.Properties);

                    var innerCaptureResponseProperties = PaymentProperty.ConvertToHashtable(innerCaptureResponseProperty.PropertyList);

                    string captureResult = null;
                    PaymentProperty.GetPropertyValue(
                        innerCaptureResponseProperties,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.CaptureResult,
                        out captureResult);

                    if (!CaptureResult.Success.ToString().Equals(captureResult, StringComparison.OrdinalIgnoreCase))
                    {
                        isCaptureFailed = true;
                    }
                }

                voidResponse = null;
                if (isCaptureFailed)
                {
                    // Capture failure, we have to void authorization and return the payment result.
                    voidResponse = processor.Void(paymentRequest);
                }
            }

            /// <summary>
            /// Combines various responses into one.
            /// </summary>
            /// <param name="tokenizeResponse">The tokenize response.</param>
            /// <param name="authorizeResponse">The authorize response.</param>
            /// <param name="captureResponse">The capture response.</param>
            /// <param name="voidResponse">The void response.</param>
            /// <returns>The combined response.</returns>
            private Response CombineResponses(Response tokenizeResponse, Response authorizeResponse, Response captureResponse, Response voidResponse)
            {
                Response paymentResponse = new Response();
                var properties = new List<PaymentProperty>();
                var errors = new List<PaymentError>();

                if (tokenizeResponse != null)
                {
                    // Start with tokenize response
                    paymentResponse.Locale = tokenizeResponse.Locale;

                    if (tokenizeResponse.Properties != null)
                    {
                        properties.AddRange(tokenizeResponse.Properties);
                    }

                    if (tokenizeResponse.Errors != null)
                    {
                        errors.AddRange(tokenizeResponse.Errors);
                    }

                    // Merge with authorize response
                    if (authorizeResponse != null)
                    {
                        if (authorizeResponse.Properties != null)
                        {
                            var authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                            PaymentProperty innerAuthorizeResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                                authorizeResponseProperties,
                                GenericNamespace.AuthorizationResponse,
                                AuthorizationResponseProperties.Properties);
                            properties.Add(innerAuthorizeResponseProperty);
                        }

                        if (authorizeResponse.Errors != null)
                        {
                            errors.AddRange(authorizeResponse.Errors);
                        }
                    }
                }
                else if (authorizeResponse != null)
                {
                    // Start with Authorize response
                    paymentResponse.Locale = authorizeResponse.Locale;

                    if (authorizeResponse.Properties != null)
                    {
                        properties.AddRange(authorizeResponse.Properties);
                    }

                    if (authorizeResponse.Errors != null)
                    {
                        errors.AddRange(authorizeResponse.Errors);
                    }
                }

                // Merge with authorize response
                if (captureResponse != null)
                {
                    if (captureResponse.Properties != null)
                    {
                        var captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                        PaymentProperty innerCaptureResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                            captureResponseProperties,
                            GenericNamespace.CaptureResponse,
                            CaptureResponseProperties.Properties);
                        properties.Add(innerCaptureResponseProperty);
                    }

                    if (captureResponse.Errors != null)
                    {
                        errors.AddRange(captureResponse.Errors);
                    }
                }

                // Merge with void response
                if (voidResponse != null)
                {
                    if (voidResponse.Properties != null)
                    {
                        var voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponse.Properties);
                        PaymentProperty innerVoidResponseProperty = PaymentProperty.GetPropertyFromHashtable(
                            voidResponseProperties,
                            GenericNamespace.VoidResponse,
                            VoidResponseProperties.Properties);
                        properties.Add(innerVoidResponseProperty);
                    }

                    if (voidResponse.Errors != null)
                    {
                        errors.AddRange(voidResponse.Errors);
                    }
                }

                if (properties.Count > 0)
                {
                    paymentResponse.Properties = properties.ToArray();
                }

                if (errors.Count > 0)
                {
                    paymentResponse.Errors = errors.ToArray();
                }

                return paymentResponse;
            }

            /// <summary>
            /// Validates the user inputs.
            /// </summary>
            /// <returns>The error message if validation failed, otherwise null.</returns>
            private bool ValidateInputs()
            {
                var errors = new List<PaymentError>();

                bool isEcommerce = IndustryType.Ecommerce.ToString().Equals(this.entry.IndustryType, StringComparison.OrdinalIgnoreCase);
                if (isEcommerce && string.IsNullOrWhiteSpace(this.CardHolderNameTextBox.Text))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidCardholderNameLastNameRequired, WebResources.CardPage_Error_MissingCardHolderLastName));
                    errors.Add(new PaymentError(ErrorCode.InvalidCardholderNameFirstNameRequired, WebResources.CardPage_Error_MissingCardHolderFirstName));
                }

                string cardType = null;
                if (this.CardTypePanel.Visible)
                {
                    cardType = this.CardTypeDropDownList.SelectedItem.Value;
                }
                else
                {
                    cardType = this.CardTypeHiddenField.Value;
                }

                if (string.IsNullOrWhiteSpace(cardType))
                {
                    errors.Add(new PaymentError(ErrorCode.CardTypeVerificationError, WebResources.CardPage_Error_MissingCardType));
                }

                if (!string.IsNullOrWhiteSpace(this.entry.CardTypes) && !this.entry.CardTypes.ToUpperInvariant().Contains(cardType.ToUpperInvariant()))
                {
                    errors.Add(new PaymentError(ErrorCode.CardTypeVerificationError, WebResources.CardPage_Error_InvalidCardType));
                }

                string cardNumber = null;
                if (this.isSwipe)
                {
                    cardNumber = this.CardNumberHiddenField.Value;
                }
                else
                {
                    cardNumber = this.CardNumberTextBox.Text.Trim();
                }

                if (!HelperUtilities.ValidateBankCardNumber(cardNumber))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidCardNumber, WebResources.CardPage_Error_InvalidCardNumber));
                }
                else
                {
                    // TO DO: THE CODE IS ONLY FOR TEST PURPOSE. REMOVE IT IN PRODUCTION.
                    // To prevent real credit card numbers entering our system, we only allow card numbers from a whitelist.
                    var cardNumberWhitelist = new List<string>();
                    cardNumberWhitelist.Add("4111111111111111"); // Visa
                    cardNumberWhitelist.Add("5555555555554444"); // MC
                    cardNumberWhitelist.Add("378282246310005"); // Amex
                    cardNumberWhitelist.Add("6011111111111117"); // Discover
                    if (cardNumberWhitelist.FindIndex(c => c.Equals(cardNumber)) < 0)
                    {
                        // errors.Add(new PaymentError(ErrorCode.InvalidCardNumber, "This card number is not allowed for testing purpose."));
                    }
                }

                if (this.ExpirationMonthDropDownList.SelectedIndex == 0)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidExpirationDate, WebResources.CardPage_Error_MissingExpirationMonth));
                }

                if (this.ExpirationYearDropDownList.SelectedIndex == 0)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidExpirationDate, WebResources.CardPage_Error_MissingExpirationYear));
                }

                // Validate security code and amount only when authorize or capture.
                TransactionType transactionType = (TransactionType)Enum.Parse(typeof(TransactionType), this.entry.TransactionType, true);
                if (transactionType == TransactionType.Authorize || transactionType == TransactionType.Capture)
                {
                    string securityCode = this.SecurityCodeTextBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(securityCode)
                        && (securityCode.Length < 3 || securityCode.Length > 4))
                    {
                        errors.Add(new PaymentError(ErrorCode.InvalidCVV2, WebResources.CardPage_Error_InvalidSecurityCode));
                    }

                    string amountStr = this.PaymentAmountHiddenField.Value.Trim();
                    decimal paymentAmount;
                    if (string.IsNullOrEmpty(amountStr)
                       || !decimal.TryParse(amountStr, NumberStyles.Number, Thread.CurrentThread.CurrentCulture, out paymentAmount)
                       || paymentAmount < 0)
                    {
                        errors.Add(new PaymentError(ErrorCode.InvalidAmount, WebResources.CardPage_Error_InvalidAmount));
                    }
                }

                if (isEcommerce
                    && (this.CountryRegionDropDownList.SelectedIndex == 0
                        || string.IsNullOrWhiteSpace(this.StreetTextBox.Text)
                        || string.IsNullOrWhiteSpace(this.CityTextBox.Text)
                        || string.IsNullOrWhiteSpace(this.StateProvinceTextBox.Text)
                        || string.IsNullOrWhiteSpace(this.ZipTextBox1.Text)))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidAddress, WebResources.CardPage_Error_InvalidAddress));
                }

                if (errors.Count > 0)
                {
                    this.InputErrorsHiddenField.Value = JsonConvert.SerializeObject(errors);
                }
                else
                {
                    this.InputErrorsHiddenField.Value = string.Empty;
                }

                return errors.Count == 0;
            }
        }
    }
}