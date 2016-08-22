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
    namespace Retail.Connector.FunctionalTests
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.IO;
        using System.Linq;
        using System.Reflection;
        using System.Text;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
        using Microsoft.Dynamics.Retail.SDKManager.Portable;
        using Microsoft.VisualStudio.TestTools.UnitTesting;

        /// <summary>
        /// Helper class for testing payment connectors.
        /// </summary>
        internal sealed class PaymentHelper
        {
            /// <summary>
            /// The default locale for request and response.
            /// </summary>
            public const string DefaultLocale = "en-US";

            /// <summary>
            /// The path of portable connectors.
            /// </summary>
            public const string PortableConnectorPath = @".\Connectors.Portable";

            /// <summary>
            /// The path of desktop connectors.
            /// </summary>
            public const string DesktopConnectorPath = @".\Connectors.Desktop";

            private const string PortableConnectorType = "Portable";
            private const string DesktopConnectorType = "Desktop";

            private static readonly object LockObject = new object();
            private static string currentConnectorType = null;

            private TestContext testContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="PaymentHelper"/> class.
            /// </summary>
            /// <param name="testContext">The test context.</param>
            public PaymentHelper(TestContext testContext)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }

                this.testContext = testContext;
            }

            /// <summary>
            /// Gets the connector name from the test context.
            /// </summary>
            /// <returns>The connector name.</returns>
            public string GetConnectorName()
            {
                return this.testContext.GetStringValue(ColumnName.ConnectorName);
            }

            /// <summary>
            /// Gets a payment processor based on test context.
            /// </summary>
            /// <returns>The payment processor.</returns>
            public IPaymentProcessor GetPaymentProcessor()
            {
                string connectorType = this.testContext.GetStringValue(ColumnName.ConnectorType);
                string connectorName = this.testContext.GetStringValue(ColumnName.ConnectorName);

                lock (LockObject)
                {
                    // Only load payment processors when necessary to reduce execution time.
                    if (currentConnectorType == null
                        || !currentConnectorType.Equals(connectorType))
                    {
                        this.ClearPaymentProcessors();
                        this.LoadPaymentProcessors(connectorType);
                    }
                }

                return PaymentProcessorManager.GetPaymentProcessor(connectorName);
            }

            /// <summary>
            /// Asserts the response contains no error.
            /// </summary>
            /// <param name="response">The response.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Assert method.")]
            public void AssertResponseNoError(Response response)
            {
                this.AssertResponseNotNull(response);

                bool noError = response.Errors == null || response.Errors.Length == 0;

                if (!noError)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("The response.Errors should not contain errors. Here are the errors: ");
                    foreach (var error in response.Errors)
                    {
                        sb.AppendLine(string.Format("Error '{0}': {1}. ", error.Code, error.Message));
                    }

                    Assert.Fail(this.ErrorFormat(sb.ToString()));
                }
            }

            /// <summary>
            /// Asserts the response contains at least one errors.
            /// </summary>
            /// <param name="response">The response.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Assert method.")]
            public void AssertResponseWithError(Response response)
            {
                this.AssertResponseNotNull(response);
                bool noError = response.Errors == null || response.Errors.Length == 0;
                if (noError)
                {
                    Assert.Fail(this.ErrorFormat("The response.Errors should contain at least one error."));
                }
            }

            /// <summary>
            /// Asserts the response contains the expected error.
            /// </summary>
            /// <param name="response">The response.</param>
            /// <param name="errorCode">The expected error code.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Assert method.")]
            public void AssertResponseErrorCode(Response response, ErrorCode errorCode)
            {
                this.AssertResponseNotNull(response);
                Assert.IsNotNull(response.Errors, this.ErrorFormat("The response.Errors should not be null."));
                Assert.IsTrue(response.Errors.Length > 0, this.ErrorFormat("The response.Errors must contain at least one error."));
                Assert.IsTrue(response.Errors.Any(e => e.Code == errorCode), this.ErrorFormat("The response does not contain the expected error {0}.", errorCode));
            }

            /// <summary>
            /// Asserts the response contains the expected error.
            /// </summary>
            /// <param name="response">The response.</param>
            /// <param name="errorCode">The expected error code.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Assert method.")]
            public void AssertResponseErrorCode(Response response, string errorCode)
            {
                ErrorCode error = (ErrorCode)Enum.Parse(typeof(ErrorCode), errorCode);
                this.AssertResponseErrorCode(response, error);
            }

            /// <summary>
            /// Asserts the response contains the expected number of errors.
            /// </summary>
            /// <param name="response">The response.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Assert method.")]
            public void AssertResponseErrorCount(Response response)
            {
                this.AssertResponseNotNull(response);

                int expectedErrorCount = this.testContext.GetIntegerValue(ColumnName.ExpectedErrorCount);
                Assert.IsNotNull(response.Errors, this.ErrorFormat("Errors should not be null."));
                Assert.AreEqual(expectedErrorCount, response.Errors.Length, this.ErrorFormat("Wrong count of errors."));
            }

            /// <summary>
            /// Asserts the response is not null.
            /// </summary>
            /// <param name="response">The response.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Assert method.")]
            public void AssertResponseNotNull(Response response)
            {
                Assert.IsNotNull(response, this.ErrorFormat("The response should not be null."));
            }

            /// <summary>
            /// Initializes a new request with merchant account properties.
            /// </summary>
            /// <returns>The new request.</returns>
            public Request GetNewRequestWithMechantAccount()
            {
                var request = new Request();
                request.Locale = DefaultLocale;

                string merchantAccountXmlPath = this.testContext.GetStringValue(ColumnName.MerchantAccountXmlPath);
                string xml = File.ReadAllText(@".\TestData\" + merchantAccountXmlPath);
                request.Properties = PaymentProperty.ConvertXMLToPropertyArray(xml);

                return request;
            }

            /// <summary>
            /// Get payment properties of the payment card from the test context.
            /// </summary>
            /// <returns>The property array.</returns>
            public PaymentProperty[] GetPaymentCardProperties()
            {
                var properties = new List<PaymentProperty>();
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.AccountType, this.testContext, ColumnName.AccountType);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.AdditionalSecurityData, this.testContext, ColumnName.AdditionalSecurityData);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardEntryType, this.testContext, ColumnName.CardEntryType);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardNumber, this.testContext, ColumnName.CardNumber);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardToken, this.testContext, ColumnName.CardToken);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, this.testContext, ColumnName.CardType);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardVerificationValue, this.testContext, ColumnName.CardVerificationValue);
                this.TryAddDecimalProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CashBackAmount, this.testContext, ColumnName.CashBackAmount);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.City, this.testContext, ColumnName.City);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Country, this.testContext, ColumnName.Country);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.EncryptedPin, this.testContext, ColumnName.EncryptedPin);
                this.TryAddDecimalProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationMonth, this.testContext, ColumnName.ExpirationMonth);
                this.TryAddDecimalProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationYear, this.testContext, ColumnName.ExpirationYear);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.IssuerName, this.testContext, ColumnName.IssuerName);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.IsSwipe, this.testContext, ColumnName.IsSwipe);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits, this.testContext, ColumnName.Last4Digits);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Name, this.testContext, ColumnName.Name);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Phone, this.testContext, ColumnName.Phone);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode, this.testContext, ColumnName.PostalCode);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ProcessorTenderId, this.testContext, ColumnName.ProcessorTenderId);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ShowSameAsShippingAddress, this.testContext, ColumnName.ShowSameAsShippingAddress);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.State, this.testContext, ColumnName.State);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress, this.testContext, ColumnName.StreetAddress);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress2, this.testContext, ColumnName.StreetAddress2);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Track1, this.testContext, ColumnName.Track1);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Track2, this.testContext, ColumnName.Track2);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Track3, this.testContext, ColumnName.Track3);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Track4, this.testContext, ColumnName.Track4);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId, this.testContext, ColumnName.UniqueCardId);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.VoiceAuthorizationCode, this.testContext, ColumnName.VoiceAuthorizationCode);

                return properties.ToArray();
            }

            /// <summary>
            /// Get payment properties of the authorization transaction data from the test context.
            /// </summary>
            /// <returns>Array with properties.</returns>
            public PaymentProperty[] GetAuthorizationProperties()
            {
                var properties = new List<PaymentProperty>();
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.AllowPartialAuthorization, this.testContext, ColumnName.AllowPartialAuthorization);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.AllowVoiceAuthorization, this.testContext, ColumnName.AllowVoiceAuthorization);
                this.TryAddDecimalProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, this.testContext, ColumnName.Amount);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, this.testContext, ColumnName.CurrencyCode);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.Description, this.testContext, ColumnName.Description);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalCustomerId, this.testContext, ColumnName.ExternalCustomerId);
                this.TryAddStringPropertyWithTimeStamp(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalInvoiceNumber, this.testContext, ColumnName.ExternalInvoiceNumber);
                this.TryAddStringPropertyWithTimeStamp(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalReferenceId, this.testContext, ColumnName.ExternalReferenceId);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.IndustryType, this.testContext, ColumnName.IndustryType);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.IsTestMode, this.testContext, ColumnName.IsTestMode);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.PurchaseLevel, this.testContext, ColumnName.PurchaseLevel);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.SupportCardTokenization, this.testContext, ColumnName.SupportCardTokenization);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.TerminalId, this.testContext, ColumnName.TerminalId);
                return properties.ToArray();
            }

            /// <summary>
            /// Get payment properties of the refund transaction data from the test context.
            /// </summary>
            /// <returns>Array with properties.</returns>
            public PaymentProperty[] GetRefundProperties()
            {
                var properties = new List<PaymentProperty>();
                this.TryAddDecimalProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, this.testContext, ColumnName.Amount);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, this.testContext, ColumnName.CurrencyCode);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.Description, this.testContext, ColumnName.Description);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalCustomerId, this.testContext, ColumnName.ExternalCustomerId);
                this.TryAddStringPropertyWithTimeStamp(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalInvoiceNumber, this.testContext, ColumnName.ExternalInvoiceNumber);
                this.TryAddStringPropertyWithTimeStamp(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalReferenceId, this.testContext, ColumnName.ExternalReferenceId);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.IndustryType, this.testContext, ColumnName.IndustryType);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.IsTestMode, this.testContext, ColumnName.IsTestMode);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.PurchaseLevel, this.testContext, ColumnName.PurchaseLevel);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.SupportCardTokenization, this.testContext, ColumnName.SupportCardTokenization);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.TerminalId, this.testContext, ColumnName.TerminalId);
                this.TryAddLevel2Properties(properties);
                this.TryAddLevel3Properties(properties);
                return properties.ToArray();
            }

            /// <summary>
            /// Get payment properties of the transaction data and the payment card data for getting payment accepting point from the test context.
            /// </summary>
            /// <returns>Array with properties.</returns>
            public PaymentProperty[] GetAcceptPointProperties()
            {
                var properties = new List<PaymentProperty>();
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.IndustryType, this.testContext, ColumnName.IndustryType);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.TransactionType, this.testContext, ColumnName.TransactionType);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, this.testContext, ColumnName.CurrencyCode);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.SupportCardTokenization, this.testContext, ColumnName.SupportCardTokenization);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.SupportCardSwipe, this.testContext, ColumnName.SupportCardSwipe);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.AllowPartialAuthorization, this.testContext, ColumnName.AllowPartialAuthorization);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.AllowVoiceAuthorization, this.testContext, ColumnName.AllowVoiceAuthorization);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.HostPageOrigin, this.testContext, ColumnName.HostPageOrigin);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.TerminalId, this.testContext, ColumnName.TerminalId);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.PurchaseLevel, this.testContext, ColumnName.PurchaseLevel);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.Description, this.testContext, ColumnName.Description);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalCustomerId, this.testContext, ColumnName.ExternalCustomerId);
                this.TryAddStringPropertyWithTimeStamp(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalInvoiceNumber, this.testContext, ColumnName.ExternalInvoiceNumber);
                this.TryAddStringPropertyWithTimeStamp(properties, GenericNamespace.TransactionData, TransactionDataProperties.ExternalReferenceId, this.testContext, ColumnName.ExternalReferenceId);
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.IsTestMode, this.testContext, ColumnName.IsTestMode);

                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.AccountType, this.testContext, ColumnName.AccountType);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, this.testContext, ColumnName.CardType);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.City, this.testContext, ColumnName.City);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Country, this.testContext, ColumnName.Country);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Name, this.testContext, ColumnName.Name);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Phone, this.testContext, ColumnName.Phone);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode, this.testContext, ColumnName.PostalCode);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ShowSameAsShippingAddress, this.testContext, ColumnName.ShowSameAsShippingAddress);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.State, this.testContext, ColumnName.State);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress, this.testContext, ColumnName.StreetAddress);
                this.TryAddStringProperty(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress2, this.testContext, ColumnName.StreetAddress2);
                return properties.ToArray();
            }

            /// <summary>
            /// Get payment properties of the transaction data for retrieving payment accepting result from the test context.
            /// </summary>
            /// <returns>Array with properties.</returns>
            public PaymentProperty[] GetAcceptResultProperties()
            {
                var properties = new List<PaymentProperty>();
                this.TryAddStringProperty(properties, GenericNamespace.TransactionData, TransactionDataProperties.PaymentAcceptResultAccessCode, this.testContext, ColumnName.PaymentAcceptResultAccessCode);
                return properties.ToArray();
            }

            /// <summary>
            /// Combines multiple arrays of payment properties.
            /// </summary>
            /// <param name="propertyArray1">The first array.</param>
            /// <param name="propertyArray2">The second array.</param>
            /// <param name="morePropertyArrays">More arrays.</param>
            /// <returns>The combined array.</returns>
            public PaymentProperty[] CombineProperties(PaymentProperty[] propertyArray1, PaymentProperty[] propertyArray2, params PaymentProperty[][] morePropertyArrays)
            {
                var properties = new List<PaymentProperty>();

                if (propertyArray1 != null)
                {
                    properties.AddRange(propertyArray1);
                }

                if (propertyArray2 != null)
                {
                    properties.AddRange(propertyArray2);
                }

                if (morePropertyArrays != null)
                {
                    foreach (var propertyArray in morePropertyArrays)
                    {
                        if (propertyArray != null)
                        {
                            properties.AddRange(propertyArray);
                        }
                    }
                }

                if (properties.Count == 0)
                {
                    return null;
                }
                else
                {
                    return properties.ToArray();
                }
            }

            /// <summary>
            /// Asserts that a property can be found.
            /// </summary>
            /// <param name="actualProperties">The actual property array.</param>
            /// <param name="expectedPropertyNamespace">The namespace of the expected property.</param>
            /// <param name="expectedPropertyName">The name of the expected property.</param>
            public void AssertPropertyNotNull(Hashtable actualProperties, string expectedPropertyNamespace, string expectedPropertyName)
            {
                if (actualProperties == null)
                {
                    throw new ArgumentNullException("actualProperties");
                }

                PaymentProperty foundProperty = PaymentProperty.GetPropertyFromHashtable(actualProperties, expectedPropertyNamespace, expectedPropertyName);
                Assert.IsNotNull(foundProperty, this.ErrorFormat("Property {0}:{1} is not found.", expectedPropertyNamespace, expectedPropertyName));

                if (foundProperty.ValueType == DataType.String)
                {
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(foundProperty.StringValue), this.ErrorFormat("Property value for {0}:{1} is null or whitespaces.", expectedPropertyNamespace, expectedPropertyName));
                }
            }

            /// <summary>
            /// Asserts that a property cannot be found.
            /// </summary>
            /// <param name="actualProperties">The actual property array.</param>
            /// <param name="propertyNamespace">The namespace of the property.</param>
            /// <param name="propertyName">The name of the property.</param>
            public void AssertPropertyNull(Hashtable actualProperties, string propertyNamespace, string propertyName)
            {
                if (actualProperties == null)
                {
                    throw new ArgumentNullException("actualProperties");
                }

                PaymentProperty foundProperty = PaymentProperty.GetPropertyFromHashtable(actualProperties, propertyNamespace, propertyName);
                Assert.IsNull(foundProperty, this.ErrorFormat("Property {0}:{1} is found unexpectedly.", propertyNamespace, propertyName));
            }

            /// <summary>
            /// Asserts that no PCI properties can be found.
            /// </summary>
            /// <param name="actualProperties">The actual property array.</param>
            public void AssertNoPCIProperties(Hashtable actualProperties)
            {
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardNumber);
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardVerificationValue);
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.EncryptedPin);
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.AdditionalSecurityData);
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Track1);
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Track2);
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Track3);
                this.AssertPropertyNull(actualProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Track4);
            }

            /// <summary>
            /// Asserts that no merchant credential properties an be found.
            /// </summary>
            /// <param name="actualProperties">The actual property array.</param>
            public void AssertNoMerchantCredential(PaymentProperty[] actualProperties)
            {
                if (actualProperties == null)
                {
                    return;
                }

                foreach (var property in actualProperties)
                {
                    if (property.ValueType == DataType.PropertyList)
                    {
                        this.AssertNoMerchantCredential(property.PropertyList);
                    }
                    else if (GenericNamespace.MerchantAccount.Equals(property.Namespace, StringComparison.OrdinalIgnoreCase)
                                && !MerchantAccountProperties.AssemblyName.Equals(property.Name, StringComparison.OrdinalIgnoreCase)
                                && !MerchantAccountProperties.PortableAssemblyName.Equals(property.Name, StringComparison.OrdinalIgnoreCase)
                                && !MerchantAccountProperties.ServiceAccountId.Equals(property.Name, StringComparison.OrdinalIgnoreCase)
                                && !MerchantAccountProperties.SupportedCurrencies.Equals(property.Name, StringComparison.OrdinalIgnoreCase)
                                && !MerchantAccountProperties.SupportedTenderTypes.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Fail(this.ErrorFormat("Unexpected property {0}:{1} is found which might be a merchant credential property.", property.Namespace, property.Name));
                    }
                }
            }

            /// <summary>
            /// Asserts the two property tables contain the same property and value.
            /// </summary>
            /// <param name="propertyTable1">Property table 1.</param>
            /// <param name="propertyTable2">Property table 2.</param>
            /// <param name="propertyNamespace">The property namespace.</param>
            /// <param name="propertyName">The property name.</param>
            public void AssertPropertyMatch(Hashtable propertyTable1, Hashtable propertyTable2, string propertyNamespace, string propertyName)
            {
                this.AssertPropertyValueMatch(propertyTable1, propertyNamespace, propertyName, propertyTable2, propertyNamespace, propertyName);
            }

            /// <summary>
            /// Asserts the value of a property.
            /// </summary>
            /// <param name="actualProperties">The actual property array.</param>
            /// <param name="expectedPropertyNamespace">The namespace of the expected property.</param>
            /// <param name="expectedPropertyName">The name of the expected property.</param>
            /// <param name="expectedPropertyValue">The expected property value.</param>
            public void AssertPropertyValue(Hashtable actualProperties, string expectedPropertyNamespace, string expectedPropertyName, object expectedPropertyValue)
            {
                if (actualProperties == null)
                {
                    throw new ArgumentNullException("actualProperties");
                }

                PaymentProperty foundProperty = PaymentProperty.GetPropertyFromHashtable(actualProperties, expectedPropertyNamespace, expectedPropertyName);
                Assert.IsNotNull(foundProperty, this.ErrorFormat("Property {0}:{1} is not found.", expectedPropertyNamespace, expectedPropertyName));

                switch (foundProperty.ValueType)
                {
                    case DataType.String:
                        Assert.AreEqual(expectedPropertyValue, foundProperty.StringValue, this.ErrorFormat("Property value for {0}:{1} is different from expected.", expectedPropertyNamespace, expectedPropertyName));
                        break;

                    case DataType.DateTime:
                        Assert.AreEqual(expectedPropertyValue, foundProperty.DateValue, this.ErrorFormat("Property value for {0}:{1} is different from expected.", expectedPropertyNamespace, expectedPropertyName));
                        break;

                    case DataType.Decimal:
                        Assert.AreEqual(expectedPropertyValue, foundProperty.DecimalValue, this.ErrorFormat("Property value for {0}:{1} is different from expected.", expectedPropertyNamespace, expectedPropertyName));
                        break;

                    default:
                        throw new ArgumentException(this.ErrorFormat("Property value type '{0}' is not supported.", foundProperty.ValueType));
                }
            }

            /// <summary>
            /// Asserts the two properties contain the same value.
            /// </summary>
            /// <param name="propertyTable1">Property table 1.</param>
            /// <param name="propertyNamespace1">The property 1 namespace.</param>
            /// <param name="propertyName1">The property 1 name.</param>
            /// <param name="propertyTable2">Property table 2.</param>
            /// <param name="propertyNamespace2">The property 2 namespace.</param>
            /// <param name="propertyName2">The property 2 name.</param>
            public void AssertPropertyValueMatch(Hashtable propertyTable1, string propertyNamespace1, string propertyName1, Hashtable propertyTable2, string propertyNamespace2, string propertyName2)
            {
                if (propertyTable1 == null)
                {
                    throw new ArgumentNullException("propertyTable1");
                }

                if (propertyTable2 == null)
                {
                    throw new ArgumentNullException("propertyTable2");
                }

                PaymentProperty property1 = PaymentProperty.GetPropertyFromHashtable(propertyTable1, propertyNamespace1, propertyName1);
                PaymentProperty property2 = PaymentProperty.GetPropertyFromHashtable(propertyTable2, propertyNamespace2, propertyName2);

                // Consider a match if the property is not found in neither tables.
                if (property1 != null || property2 != null)
                {
                    Assert.IsNotNull(property1, this.ErrorFormat("The property {0}:{1} is found the second list but not the first list.", propertyNamespace1, propertyName1));
                    Assert.IsNotNull(property2, this.ErrorFormat("The property {0}:{1} is found the first list but not the second list.", propertyNamespace2, propertyName2));
                    Assert.AreEqual(property1.ValueType, property2.ValueType, this.ErrorFormat("The property type for {0}:{1} and {2}:{3} do not match.", propertyNamespace1, propertyName1, propertyNamespace2, propertyName2));

                    switch (property1.ValueType)
                    {
                        case DataType.String:
                            Assert.AreEqual(property1.StringValue, property2.StringValue, this.ErrorFormat("The property value for {0}:{1} and {2}:{3} do not match.", propertyNamespace1, propertyName1, propertyNamespace2, propertyName2));
                            break;
                        case DataType.Decimal:
                            Assert.AreEqual(property1.DecimalValue, property2.DecimalValue, this.ErrorFormat("The property value for {0}:{1} and {2}:{3} do not match.", propertyNamespace1, propertyName1, propertyNamespace2, propertyName2));
                            break;
                        case DataType.DateTime:
                            Assert.AreEqual(property1.DateValue, property2.DateValue, this.ErrorFormat("The property value for {0}:{1} and {2}:{3} do not match.", propertyNamespace1, propertyName1, propertyNamespace2, propertyName2));
                            break;
                        default:
                            throw new ArgumentException(string.Format("The property type for {0}:{1} and {2}:{3} is not supported: {4}.", propertyNamespace1, propertyName1, propertyNamespace2, propertyName2, property1.ValueType));
                    }
                }
            }

            /// <summary>
            /// Creates an authorize request from a tokenize response.
            /// </summary>
            /// <param name="tokenizeResponse">The tokenize response.</param>
            /// <returns>The authorize request.</returns>
            public Request CreateAuthorizeRequest(Response tokenizeResponse)
            {
                if (tokenizeResponse == null)
                {
                    throw new ArgumentNullException("tokenizeResponse");
                }

                // Read payment card properties from tokenize response
                var cardProperties = new List<PaymentProperty>();
                foreach (var property in tokenizeResponse.Properties)
                {
                    if (GenericNamespace.PaymentCard.Equals(property.Namespace, StringComparison.OrdinalIgnoreCase))
                    {
                        cardProperties.Add(property);
                    }
                }

                PaymentProperty[] authorizationProperties = this.GetAuthorizationProperties();

                Request authorizeRequest = this.GetNewRequestWithMechantAccount();
                authorizeRequest.Properties = this.CombineProperties(authorizeRequest.Properties, cardProperties.ToArray(), authorizationProperties);
                return authorizeRequest;
            }

            /// <summary>
            /// Creates a capture request from an authorization response.
            /// </summary>
            /// <param name="authorizeResponse">The authorization response.</param>
            /// <returns>The capture request.</returns>
            public Request CreateCaptureRequest(Response authorizeResponse)
            {
                return this.CreateCaptureRequest(authorizeResponse, -1M);
            }

            /// <summary>
            /// Creates a capture request from an authorization response.
            /// </summary>
            /// <param name="authorizeResponse">The authorization response.</param>
            /// <param name="captureAmount">The amount to capture. When negative, capture the approved amount.</param>
            /// <returns>The capture request.</returns>
            public Request CreateCaptureRequest(Response authorizeResponse, decimal captureAmount)
            {
                if (authorizeResponse == null)
                {
                    throw new ArgumentNullException("authorizeResponse");
                }

                // Get authorization response properties
                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                PaymentProperty authorizationResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties);
                decimal approvedAmount;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount, out approvedAmount);
                string currencyCode;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CurrencyCode, out currencyCode);

                var captureRequestProperties = new List<PaymentProperty>();
                captureRequestProperties.Add(authorizationResponsePropertyList);
                this.TryAddStringProperty(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.IndustryType, this.testContext, ColumnName.IndustryType);
                this.TryAddStringProperty(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.IsTestMode, this.testContext, ColumnName.IsTestMode);
                this.TryAddStringProperty(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.PurchaseLevel, this.testContext, ColumnName.PurchaseLevel);

                if (captureAmount >= 0M)
                {
                    captureRequestProperties.Add(new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.Amount, captureAmount));
                }
                else
                {
                    captureRequestProperties.Add(new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.Amount, approvedAmount));
                }

                captureRequestProperties.Add(new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, currencyCode));

                this.TryAddLevel2Properties(captureRequestProperties);
                this.TryAddLevel3Properties(captureRequestProperties);

                // Get payemnt card properties
                PaymentProperty[] cardProperties = this.GetPaymentCardProperties();

                var captureRequest = this.GetNewRequestWithMechantAccount();
                captureRequest.Locale = authorizeResponse.Locale;
                captureRequest.Properties = this.CombineProperties(captureRequest.Properties, cardProperties, captureRequestProperties.ToArray());
                return captureRequest;
            }

            /// <summary>
            /// Creates a capture request from a tokenization response and an authorization response.
            /// </summary>
            /// <param name="tokenizeResponse">The tokenization response.</param>
            /// <param name="authorizeResponse">The authorization response.</param>
            /// <returns>The capture request.</returns>
            public Request CreateCaptureRequest(Response tokenizeResponse, Response authorizeResponse)
            {
                if (tokenizeResponse == null)
                {
                    throw new ArgumentNullException("tokenizeResponse");
                }

                if (authorizeResponse == null)
                {
                    throw new ArgumentNullException("authorizeResponse");
                }

                // Get authorization response properties
                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                PaymentProperty authorizationResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties);
                decimal approvedAmount;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount, out approvedAmount);
                string currencyCode;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CurrencyCode, out currencyCode);

                var captureRequestProperties = new List<PaymentProperty>();
                captureRequestProperties.Add(authorizationResponsePropertyList);
                this.TryAddStringProperty(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.IndustryType, this.testContext, ColumnName.IndustryType);
                this.TryAddStringProperty(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.IsTestMode, this.testContext, ColumnName.IsTestMode);
                this.TryAddStringProperty(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.PurchaseLevel, this.testContext, ColumnName.PurchaseLevel);
                captureRequestProperties.Add(new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.Amount, approvedAmount));
                captureRequestProperties.Add(new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, currencyCode));

                this.TryAddLevel2Properties(captureRequestProperties);
                this.TryAddLevel3Properties(captureRequestProperties);

                // Get payemnt card properties from tokenization response
                var cardProperties = new List<PaymentProperty>();
                foreach (var property in tokenizeResponse.Properties)
                {
                    if (GenericNamespace.PaymentCard.Equals(property.Namespace, StringComparison.OrdinalIgnoreCase))
                    {
                        cardProperties.Add(property);
                    }
                }

                var captureRequest = this.GetNewRequestWithMechantAccount();
                captureRequest.Locale = authorizeResponse.Locale;
                captureRequest.Properties = this.CombineProperties(captureRequest.Properties, cardProperties.ToArray(), captureRequestProperties.ToArray());
                return captureRequest;
            }

            /// <summary>
            /// Creates a void request from an authorization response.
            /// </summary>
            /// <param name="authorizeResponse">The authorization response.</param>
            /// <returns>The void request.</returns>
            public Request CreateVoidRequest(Response authorizeResponse)
            {
                if (authorizeResponse == null)
                {
                    throw new ArgumentNullException("authorizeResponse");
                }

                // Get authorization response properties
                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                PaymentProperty authorizationResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties);

                var voidRequest = this.GetNewRequestWithMechantAccount();
                voidRequest.Locale = authorizeResponse.Locale;
                voidRequest.Properties = this.CombineProperties(voidRequest.Properties, new PaymentProperty[] { authorizationResponsePropertyList });
                return voidRequest;
            }

            /// <summary>
            /// Creates an refund request from a tokenize response.
            /// </summary>
            /// <param name="tokenizeResponse">The tokenize response.</param>
            /// <returns>The refund request.</returns>
            public Request CreateRefundRequestFromTokenizeReponse(Response tokenizeResponse)
            {
                if (tokenizeResponse == null)
                {
                    throw new ArgumentNullException("tokenizeResponse");
                }

                // Read payment card properties from tokenize response
                var cardProperties = new List<PaymentProperty>();
                foreach (var property in tokenizeResponse.Properties)
                {
                    if (GenericNamespace.PaymentCard.Equals(property.Namespace, StringComparison.OrdinalIgnoreCase))
                    {
                        cardProperties.Add(property);
                    }
                }

                PaymentProperty[] refundProperties = this.GetRefundProperties();

                Request refundRequest = this.GetNewRequestWithMechantAccount();
                refundRequest.Properties = this.CombineProperties(refundRequest.Properties, cardProperties.ToArray(), refundProperties);
                return refundRequest;
            }

            /// <summary>
            /// Creates an refund request from a capture response.
            /// </summary>
            /// <param name="captureResponse">The capture response.</param>
            /// <returns>The refund request.</returns>
            public Request CreateRefundRequestFromCaptureResponse(Response captureResponse)
            {
                if (captureResponse == null)
                {
                    throw new ArgumentNullException("captureResponse");
                }

                // Get capture response properties
                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                PaymentProperty captureResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.Properties);

                PaymentProperty[] refundProperties = this.GetRefundProperties();

                Request refundRequest = this.GetNewRequestWithMechantAccount();
                refundRequest.Properties = this.CombineProperties(refundRequest.Properties, new PaymentProperty[] { captureResponsePropertyList }, refundProperties);
                return refundRequest;
            }

            /// <summary>
            /// Prints the details of a request object when debugging.
            /// </summary>
            /// <param name="request">The request.</param>
            public void PrintRequestWhenDebug(Request request)
            {
                Debug.WriteLine("==============================");
                Debug.WriteLine(string.Format("Request content {0}", this.GetTestDescriptor()));
                if (request == null)
                {
                    Debug.WriteLine("The request is null.");
                }
                else
                {
                    Debug.WriteLine(string.Format("Request local: '{0}'", request.Locale));
                    if (request.Properties == null || request.Properties.Length == 0)
                    {
                        Debug.WriteLine("The request properties is empty.");
                    }
                    else
                    {
                        Debug.WriteLine("The request properties: ");
                        Debug.WriteLine(PaymentProperty.ConvertPropertyArrayToXML(request.Properties));
                    }
                }
            }

            /// <summary>
            /// Prints the details of a response object when debugging.
            /// </summary>
            /// <param name="response">The response.</param>
            public void PrintResponseWhenDebug(Response response)
            {
                Debug.WriteLine("==============================");
                Debug.WriteLine(string.Format("Response content {0}", this.GetTestDescriptor()));
                if (response == null)
                {
                    Debug.WriteLine("The response is null.");
                }
                else
                {
                    Debug.WriteLine(string.Format("Response local: '{0}'", response.Locale));
                    if (response.Properties == null || response.Properties.Length == 0)
                    {
                        Debug.WriteLine("The response properties is empty.");
                    }
                    else
                    {
                        Debug.WriteLine("The response properties: ");
                        Debug.WriteLine(PaymentProperty.ConvertPropertyArrayToXML(response.Properties));
                    }

                    if (response.Errors == null || response.Errors.Length == 0)
                    {
                        Debug.WriteLine("The response errors is empty.");
                    }
                    else
                    {
                        Debug.WriteLine("The response errors: ");
                        foreach (var error in response.Errors)
                        {
                            Debug.WriteLine(string.Format("{0}: {1}", error.Code, error.Message));
                        }
                    }
                }
            }

            /// <summary>
            /// Get a formatted string with provided arguments.
            /// </summary>
            /// <param name="format">The string format.</param>
            /// <param name="args">The arguments.</param>
            /// <returns>The string.</returns>
            public string ErrorFormat(string format, params object[] args)
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Empty);
                sb.AppendLine(this.GetTestDescriptor());
                sb.AppendLine(string.Format(format, args));
                return sb.ToString();
            }

            /// <summary>
            /// Clear loaded payment processors.
            /// </summary>
            private void ClearPaymentProcessors()
            {
                Type paymentProcessorManagerType = typeof(PaymentProcessorManager);
                FieldInfo field = paymentProcessorManagerType.GetField("connectorPath", BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(null, null);
                currentConnectorType = null;
            }

            private void LoadPaymentProcessors(string connectorType)
            {
                string connectorPath = null;
                switch (connectorType)
                {
                    case PortableConnectorType:
                        connectorPath = PortableConnectorPath;
                        break;
                    case DesktopConnectorType:
                        connectorPath = DesktopConnectorPath;
                        break;

                    default:
                        throw new ArgumentException(string.Format("Invalid connector type: {0}", connectorType));
                }

                connectorPath = Path.GetFullPath(connectorPath);
                PaymentProcessorManager.Create(connectorPath);
                currentConnectorType = connectorType;
            }

            private bool TryAddStringProperty(List<PaymentProperty> properties, string propertyNamespace, string propertyName, TestContext testContext, string columnName)
            {
                string value;
                if (testContext.TryGetStringValue(columnName, out value))
                {
                    properties.Add(new PaymentProperty(
                        propertyNamespace,
                        propertyName,
                        value));
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool TryAddStringPropertyWithTimeStamp(List<PaymentProperty> properties, string propertyNamespace, string propertyName, TestContext testContext, string columnName)
            {
                string value;
                if (testContext.TryGetStringValue(columnName, out value))
                {
                    value += DateTime.Now.Ticks.ToString();
                    properties.Add(new PaymentProperty(
                        propertyNamespace,
                        propertyName,
                        value));
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool TryAddDecimalProperty(List<PaymentProperty> properties, string propertyNamespace, string propertyName, TestContext testContext, string columnName)
            {
                decimal value;
                if (testContext.TryGetDecimalValue(columnName, out value))
                {
                    properties.Add(new PaymentProperty(
                        propertyNamespace,
                        propertyName,
                        value));
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool TryAddDateTimeProperty(List<PaymentProperty> properties, string propertyNamespace, string propertyName, TestContext testContext, string columnName)
            {
                DateTime value;
                if (testContext.TryGetDateTimeValue(columnName, out value))
                {
                    properties.Add(new PaymentProperty(
                        propertyNamespace,
                        propertyName,
                        value));
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private void TryAddLevel2Properties(List<PaymentProperty> properties)
            {
                var level2DataProperties = new List<PaymentProperty>();
                this.TryAddDateTimeProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.OrderDateTime, this.testContext, ColumnName.Level2DataOrderDateTime);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.OrderNumber, this.testContext, ColumnName.Level2DataOrderNumber);
                this.TryAddDateTimeProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.InvoiceDateTime, this.testContext, ColumnName.Level2DataInvoiceDateTime);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.InvoiceNumber, this.testContext, ColumnName.Level2DataInvoiceNumber);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.OrderDescription, this.testContext, ColumnName.Level2DataOrderDescription);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.SummaryCommodityCode, this.testContext, ColumnName.Level2DataSummaryCommodityCode);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantContact, this.testContext, ColumnName.Level2DataMerchantContact);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantTaxId, this.testContext, ColumnName.Level2DataMerchantTaxId);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantType, this.testContext, ColumnName.Level2DataMerchantType);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.PurchaserId, this.testContext, ColumnName.Level2DataPurchaserId);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.PurchaserTaxId, this.testContext, ColumnName.Level2DataPurchaserTaxId);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipToCity, this.testContext, ColumnName.Level2DataShipToCity);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipToCounty, this.testContext, ColumnName.Level2DataShipToCounty);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipToState_ProvinceCode, this.testContext, ColumnName.Level2DataShipToStateProvinceCode);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipToPostalCode, this.testContext, ColumnName.Level2DataShipToPostalCode);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipToCountryCode, this.testContext, ColumnName.Level2DataShipToCountryCode);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipFromCity, this.testContext, ColumnName.Level2DataShipFromCity);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipFromCounty, this.testContext, ColumnName.Level2DataShipFromCounty);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipFromState_ProvinceCode, this.testContext, ColumnName.Level2DataShipFromStateProvinceCode);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipFromPostalCode, this.testContext, ColumnName.Level2DataShipFromPostalCode);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.ShipFromCountryCode, this.testContext, ColumnName.Level2DataShipFromCountryCode);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.DiscountAmount, this.testContext, ColumnName.Level2DataDiscountAmount);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MiscCharge, this.testContext, ColumnName.Level2DataMiscCharge);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.DutyAmount, this.testContext, ColumnName.Level2DataDutyAmount);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.FreightAmount, this.testContext, ColumnName.Level2DataFreightAmount);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.IsTaxable, this.testContext, ColumnName.Level2DataIsTaxable);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.TotalTaxAmount, this.testContext, ColumnName.Level2DataTotalTaxAmount);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.TotalTaxRate, this.testContext, ColumnName.Level2DataTotalTaxRate);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantName, this.testContext, ColumnName.Level2DataMerchantName);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantStreet, this.testContext, ColumnName.Level2DataMerchantStreet);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantCity, this.testContext, ColumnName.Level2DataMerchantCity);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantState, this.testContext, ColumnName.Level2DataMerchantState);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantCounty, this.testContext, ColumnName.Level2DataMerchantCounty);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantCountryCode, this.testContext, ColumnName.Level2DataMerchantCountryCode);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.MerchantZip, this.testContext, ColumnName.Level2DataMerchantZip);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.TaxRate, this.testContext, ColumnName.Level2DataTaxRate);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.TaxAmount, this.testContext, ColumnName.Level2DataTaxAmount);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.TaxDescription, this.testContext, ColumnName.Level2DataTaxDescription);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.TaxTypeIdentifier, this.testContext, ColumnName.Level2DataTaxTypeIdentifier);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.RequesterName, this.testContext, ColumnName.Level2DataRequesterName);
                this.TryAddDecimalProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.TotalAmount, this.testContext, ColumnName.Level2DataTotalAmount);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.PurchaseCardType, this.testContext, ColumnName.Level2DataPurchaseCardType);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.AmexLegacyDescription1, this.testContext, ColumnName.Level2DataAmexLegacyDescription1);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.AmexLegacyDescription2, this.testContext, ColumnName.Level2DataAmexLegacyDescription2);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.AmexLegacyDescription3, this.testContext, ColumnName.Level2DataAmexLegacyDescription3);
                this.TryAddStringProperty(level2DataProperties, GenericNamespace.L2Data, L2DataProperties.AmexLegacyDescription4, this.testContext, ColumnName.Level2DataAmexLegacyDescription4);

                var taxDetailProperties = new List<PaymentProperty>();
                this.TryAddStringProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxTypeIdentifier, this.testContext, ColumnName.Level2DataTaxDetailsTaxTypeIdentifier);
                this.TryAddDecimalProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxRate, this.testContext, ColumnName.Level2DataTaxDetailsTaxRate);
                this.TryAddStringProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxDescription, this.testContext, ColumnName.Level2DataTaxDetailsTaxDescription);
                this.TryAddDecimalProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxAmount, this.testContext, ColumnName.Level2DataTaxDetailsTaxAmount);
                if (taxDetailProperties.Count > 0)
                {
                    var taxDetailsProperties = new List<PaymentProperty>();
                    taxDetailsProperties.Add(new PaymentProperty(GenericNamespace.TaxDetails, TaxDetailProperties.TaxDetail, taxDetailProperties.ToArray()));

                    level2DataProperties.Add(new PaymentProperty(GenericNamespace.L2Data, L2DataProperties.TaxDetails, taxDetailsProperties.ToArray()));
                }

                var miscellaneousChargeProperties = new List<PaymentProperty>();
                this.TryAddStringProperty(miscellaneousChargeProperties, GenericNamespace.MiscellaneousCharge, MiscellaneousChargeProperties.ChargeType, this.testContext, ColumnName.Level2DataMiscellaneousChargesChargeType);
                this.TryAddDecimalProperty(miscellaneousChargeProperties, GenericNamespace.MiscellaneousCharge, MiscellaneousChargeProperties.ChargeAmount, this.testContext, ColumnName.Level2DataMiscellaneousChargesChargeAmount);
                if (miscellaneousChargeProperties.Count > 0)
                {
                    var miscellaneousChargesProperties = new List<PaymentProperty>();
                    miscellaneousChargesProperties.Add(new PaymentProperty(GenericNamespace.MiscellaneousCharges, MiscellaneousChargeProperties.MiscellaneousCharge, miscellaneousChargeProperties.ToArray()));

                    level2DataProperties.Add(new PaymentProperty(GenericNamespace.L2Data, L2DataProperties.MiscellaneousCharges, miscellaneousChargesProperties.ToArray()));
                }

                if (level2DataProperties.Count > 0)
                {
                    properties.Add(new PaymentProperty(GenericNamespace.PurchaseLevelData, PurchaseLevelDataProperties.L2Data, level2DataProperties.ToArray()));
                }
            }

            private void TryAddLevel3Properties(List<PaymentProperty> properties)
            {
                var level3DataItemProperties = new List<PaymentProperty>();
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.SequenceNumber, this.testContext, ColumnName.Level3DataSequenceNumber);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.CommodityCode, this.testContext, ColumnName.Level3DataCommodityCode);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.ProductCode, this.testContext, ColumnName.Level3DataProductCode);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.ProductName, this.testContext, ColumnName.Level3DataProductName);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.ProductSKU, this.testContext, ColumnName.Level3DataProductSKU);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.Descriptor, this.testContext, ColumnName.Level3DataDescriptor);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.UnitOfMeasure, this.testContext, ColumnName.Level3DataUnitOfMeasure);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.UnitPrice, this.testContext, ColumnName.Level3DataUnitPrice);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.Discount, this.testContext, ColumnName.Level3DataDiscount);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.DiscountRate, this.testContext, ColumnName.Level3DataDiscountRate);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.Quantity, this.testContext, ColumnName.Level3DataQuantity);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.MiscCharge, this.testContext, ColumnName.Level3DataMiscCharge);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.NetTotal, this.testContext, ColumnName.Level3DataNetTotal);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.TaxAmount, this.testContext, ColumnName.Level3DataTaxAmount);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.TaxRate, this.testContext, ColumnName.Level3DataTaxRate);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.TotalAmount, this.testContext, ColumnName.Level3DataTotalAmount);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.CostCenter, this.testContext, ColumnName.Level3DataCostCenter);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.FreightAmount, this.testContext, ColumnName.Level3DataFreightAmount);
                this.TryAddDecimalProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.HandlingAmount, this.testContext, ColumnName.Level3DataHandlingAmount);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.CarrierTrackingNumber, this.testContext, ColumnName.Level3DataCarrierTrackingNumber);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.MerchantTaxID, this.testContext, ColumnName.Level3DataMerchantTaxID);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.MerchantCatalogNumber, this.testContext, ColumnName.Level3DataMerchantCatalogNumber);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.TaxCategoryApplied, this.testContext, ColumnName.Level3DataTaxCategoryApplied);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupAddress, this.testContext, ColumnName.Level3DataPickupAddress);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupCity, this.testContext, ColumnName.Level3DataPickupCity);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupState, this.testContext, ColumnName.Level3DataPickupState);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupCounty, this.testContext, ColumnName.Level3DataPickupCounty);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupZip, this.testContext, ColumnName.Level3DataPickupZip);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupCountry, this.testContext, ColumnName.Level3DataPickupCountry);
                this.TryAddDateTimeProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupDateTime, this.testContext, ColumnName.Level3DataPickupDateTime);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.PickupRecordNumber, this.testContext, ColumnName.Level3DataPickupRecordNumber);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.CarrierShipmentNumber, this.testContext, ColumnName.Level3DataCarrierShipmentNumber);
                this.TryAddStringProperty(level3DataItemProperties, GenericNamespace.L3Data, L3DataProperties.UNSPSCCode, this.testContext, ColumnName.Level3DataUNSPSCCode);

                var taxDetailProperties = new List<PaymentProperty>();
                this.TryAddStringProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxTypeIdentifier, this.testContext, ColumnName.Level3DataTaxDetailsTaxTypeIdentifier);
                this.TryAddDecimalProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxRate, this.testContext, ColumnName.Level3DataTaxDetailsTaxRate);
                this.TryAddStringProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxDescription, this.testContext, ColumnName.Level3DataTaxDetailsTaxDescription);
                this.TryAddDecimalProperty(taxDetailProperties, GenericNamespace.TaxDetail, TaxDetailProperties.TaxAmount, this.testContext, ColumnName.Level3DataTaxDetailsTaxAmount);
                if (taxDetailProperties.Count > 0)
                {
                    var taxDetailsProperties = new List<PaymentProperty>();
                    taxDetailsProperties.Add(new PaymentProperty(GenericNamespace.TaxDetails, TaxDetailProperties.TaxDetail, taxDetailProperties.ToArray()));

                    level3DataItemProperties.Add(new PaymentProperty(GenericNamespace.L3Data, L3DataProperties.TaxDetails, taxDetailsProperties.ToArray()));
                }

                var miscellaneousChargeProperties = new List<PaymentProperty>();
                this.TryAddStringProperty(miscellaneousChargeProperties, GenericNamespace.MiscellaneousCharge, MiscellaneousChargeProperties.ChargeType, this.testContext, ColumnName.Level3DataMiscellaneousChargesChargeType);
                this.TryAddDecimalProperty(miscellaneousChargeProperties, GenericNamespace.MiscellaneousCharge, MiscellaneousChargeProperties.ChargeAmount, this.testContext, ColumnName.Level3DataMiscellaneousChargesChargeAmount);
                if (miscellaneousChargeProperties.Count > 0)
                {
                    var miscellaneousChargesProperties = new List<PaymentProperty>();
                    miscellaneousChargesProperties.Add(new PaymentProperty(GenericNamespace.MiscellaneousCharges, MiscellaneousChargeProperties.MiscellaneousCharge, miscellaneousChargeProperties.ToArray()));

                    level3DataItemProperties.Add(new PaymentProperty(GenericNamespace.L3Data, L3DataProperties.MiscellaneousCharges, miscellaneousChargesProperties.ToArray()));
                }

                if (level3DataItemProperties.Count > 0)
                {
                    var level3DataProperties = new List<PaymentProperty>();
                    level3DataProperties.Add(new PaymentProperty(GenericNamespace.PurchaseLevelData, PurchaseLevelDataProperties.L3DataItems, level3DataItemProperties.ToArray()));

                    properties.Add(new PaymentProperty(GenericNamespace.PurchaseLevelData, PurchaseLevelDataProperties.L3Data, level3DataProperties.ToArray()));
                }
            }

            private string GetTestDescriptor()
            {
                string testDescriptor;
                if (!this.testContext.TryGetStringValue(ColumnName.TestDescriptor, out testDescriptor))
                {
                    testDescriptor = "NA";
                }

                return string.Format(
                    "(ConnectorName={0}, ConnectorType={1}, TestDescriptor={2})",
                    this.testContext.GetStringValue(ColumnName.ConnectorName),
                    this.testContext.GetStringValue(ColumnName.ConnectorType),
                    testDescriptor);
            }
        }
    }
}
