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
        using Microsoft.Dynamics.Commerce.Tests.Utilities.Constants;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
        using Microsoft.VisualStudio.TestTools.UnitTesting;
        using MS.Dynamics.TestTools.Metadata;

        /// <summary>
        /// This class has the functional tests for the payment connectors.
        /// </summary>
        [TestClass]
        [AreaPath(RetailTestAreaPathConstants.PaymentProcessingVerifoneConnector)]
        [Granularity(Granularity.Component)]
        [DeploymentItem("Microsoft.Dynamics.Retail.PaymentSDK.Extensions.dll")]
        [DeploymentItem("Microsoft.Dynamics.Retail.TestConnector.Portable.dll")]
        [DeploymentItem("Microsoft.Dynamics.Retail.TestConnector.Portable.dll", "Connectors.Portable")]
        [DeploymentItem("Microsoft.Dynamics.Retail.TestConnector.dll", "Connectors.Desktop")]
        [DeploymentItem(@"TestData\MerchantAccount_TestConnectorPortable.xml", "TestData")]
        [DeploymentItem(@"TestData\MerchantAccount_TestConnectorDesktop.xml", "TestData")]
        [DeploymentItem(@"TestData\MerchantAccount_TestConnectorPortable_InvalidProperties.xml", "TestData")]
        [DeploymentItem(@"TestData\MerchantAccount_TestConnectorDesktop_InvalidProperties.xml", "TestData")]
        [DeploymentItem(@"%testroot%\frameworks\retail\components\platform\libraries\paymentsdk\connector.functionaltests")]
        [DeploymentItem(@"%testroot%\frameworks\retail\components\platform\libraries\paymentsdk\connector.functionaltests\Microsoft.Dynamics.Retail.TestConnector.Portable.dll", "Connectors.Portable")]
        [DeploymentItem(@"%testroot%\frameworks\retail\components\platform\libraries\paymentsdk\connector.functionaltests\Microsoft.Dynamics.Retail.TestConnector.dll", "Connectors.Desktop")]
        public class PaymentProcessorFunctionalTests
        {
            #region Properties

            private TestContext testContext;
            private PaymentHelper helper;

            /// <summary>
            /// Gets or sets the test context.
            /// </summary>
            public TestContext TestContext
            {
                get
                {
                    return this.testContext;
                }

                set
                {
                    this.testContext = value;
                    this.helper = new PaymentHelper(value);
                }
            }

            #endregion

            #region GetPaymentProcessor tests

            /// <summary>
            /// Validates creating the payment processors using portable manager, empty path, expect exception.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("6A707298-7694-46C7-BED8-0BEC295F23DF")]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentProcessor_Create_PortableEmptyPath()
            {
                // Arrange.
                // Nothing

                // Act.
                Microsoft.Dynamics.Retail.SDKManager.Portable.PaymentProcessorManager.Create(string.Empty);

                // Assert.
                // Expect exception
            }

            /// <summary>
            /// Validates creating the payment processors using desktop manager, empty path, expect exception.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("F9EB8FF4-C846-40E9-8416-47B7F6AE5D84")]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentProcessor_Create_DesktopEmptyPath()
            {
                // Arrange.
                // Nothing

                // Act.
                Microsoft.Dynamics.Retail.SDKManager.PaymentProcessorManager.Create(string.Empty);

                // Assert.
                // Expect exception
            }

            /// <summary>
            /// Validates getting the payment processor using portable manager, success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("1EBF53D9-1127-4BF6-A9F2-2D791A4A1248")]
            [DeploymentItem(@"TestData\PaymentProcessor_GetPaymentProcessor.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GetPaymentProcessor.xml", "PaymentProcessor_GetPaymentProcessor_PortableSuccess", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GetPaymentProcessor_PortableSuccess()
            {
                // Arrange.
                // Things are arranged in helper.GetPaymentProcessor()

                // Act.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Assert.
                Assert.IsNotNull(processor, this.ErrorFormat("Processor should not be null."));
            }

            /// <summary>
            /// Validates getting the payment processor using desktop manager, success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("09828DF1-77F5-42D3-A1A5-04016F83E094")]
            [DeploymentItem(@"TestData\PaymentProcessor_GetPaymentProcessor.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GetPaymentProcessor.xml", "PaymentProcessor_GetPaymentProcessor_DesktopSuccess", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GetPaymentProcessor_DesktopSuccess()
            {
                // Arrange.
                Microsoft.Dynamics.Retail.SDKManager.PaymentProcessorManager.Create(PaymentHelper.DesktopConnectorPath);
                string connectorName = this.TestContext.GetStringValue(ColumnName.ConnectorName);

                // Act.
                Microsoft.Dynamics.Retail.PaymentSDK.IPaymentProcessor processor = Microsoft.Dynamics.Retail.SDKManager.PaymentProcessorManager.GetPaymentProcessor(connectorName);

                // Assert.
                Assert.IsNotNull(processor, this.ErrorFormat("Processor should not be null."));
            }

            /// <summary>
            /// Validates getting the payment processor using portable manager, invalid processor name, expect exception.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("B05FE965-411C-4F1F-AD63-9A3B16704CFC")]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PaymentProcessor_GetPaymentProcessor_PortableInvalidName()
            {
                // Arrange.
                Microsoft.Dynamics.Retail.SDKManager.Portable.PaymentProcessorManager.Create(PaymentHelper.PortableConnectorPath);

                // Act.
                Microsoft.Dynamics.Retail.SDKManager.Portable.PaymentProcessorManager.GetPaymentProcessor("DoesNotExist");

                // Assert.
                // Expect exception
            }

            /// <summary>
            /// Validates getting the payment processor using desktop manager, invalid processor name, expect exception.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("CCCF8F6B-F565-4260-AB25-42DDC1DA1BB6")]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PaymentProcessor_GetPaymentProcessor_DesktopInvalidName()
            {
                // Arrange.
                Microsoft.Dynamics.Retail.SDKManager.PaymentProcessorManager.Create(PaymentHelper.DesktopConnectorPath);

                // Act.
                Microsoft.Dynamics.Retail.SDKManager.PaymentProcessorManager.GetPaymentProcessor("DoesNotExist");

                // Assert.
                // Expect exception
            }

            #endregion

            #region Name tests

            /// <summary>
            /// Validates getting the name of the payment processor, success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("7868EC26-7A42-4D06-A42B-10BB76D666F4")]
            [DeploymentItem(@"TestData\PaymentProcessor_Name.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Name.xml", "PaymentProcessor_Name_Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Name_Success()
            {
                // Arrange.
                string expectedConnectorName = this.helper.GetConnectorName();
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                string actualConnectorName = processor.Name;

                // Assert.
                Assert.IsNotNull(actualConnectorName, this.ErrorFormat("The actualConnectorName should not be null."));
                Assert.AreEqual(expectedConnectorName, actualConnectorName, this.ErrorFormat("Wrong connector name."));
            }

            #endregion

            #region Copyright tests

            /// <summary>
            /// Validates getting the copyright of the payment processor, success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("846EA85C-F065-45D6-942B-677862A6B67C")]
            [DeploymentItem(@"TestData\PaymentProcessor_Copyright.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Copyright.xml", "PaymentProcessor_Copyright_Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Copyright_Success()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                string copyright = processor.Copyright;

                // Assert.
                Assert.IsTrue(!string.IsNullOrWhiteSpace(copyright), this.ErrorFormat("Copyright should not be null or white spaces."));
            }

            #endregion

            #region SupportedCountries tests

            /// <summary>
            /// Validates getting the copyright of the payment processor, success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("D3AC69AA-F417-4D66-88D0-CDDF63B4F008")]
            [DeploymentItem(@"TestData\PaymentProcessor_SupportedCountries.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_SupportedCountries.xml", "PaymentProcessor_SupportedCountries_Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_SupportedCountries_Success()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                ArrayList countries = processor.SupportedCountries;

                // Assert.
                Assert.IsNotNull(countries, this.ErrorFormat("The supported countries should not be null."));
                Assert.IsTrue(countries.Count > 0, this.ErrorFormat("The supported countries should not be empty."));
                foreach (var country in countries)
                {
                    Assert.IsTrue(country is string, this.ErrorFormat("The supported country must be string type: {0}", country));
                    string countryCode = country as string;
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(countryCode), this.ErrorFormat("The supported country must not be null or white spaces: {0}.", countryCode));
                    Assert.IsTrue(countryCode.Length == 2, this.ErrorFormat("Invalid country code: {0}.", countryCode));
                }
            }

            #endregion

            #region GetMerchantAccountPropertyMetadata tests

            /// <summary>
            /// Validates getting the merchant property metadata, success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("785808A7-F8A4-4204-A4DB-CD2BB4C9B887")]
            [DeploymentItem(@"TestData\PaymentProcessor_GetMerchantAccountPropertyMetadata.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GetMerchantAccountPropertyMetadata.xml", "PaymentProcessor_GetMerchantAccountPropertyMetadata_Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GetMerchantAccountPropertyMetadata_Success()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request request = new Request();
                request.Locale = PaymentHelper.DefaultLocale;
                this.helper.PrintRequestWhenDebug(request);

                // Act.
                Response response = processor.GetMerchantAccountPropertyMetadata(request);

                // Assert.
                this.helper.PrintResponseWhenDebug(response);
                this.helper.AssertResponseNoError(response);
                Assert.IsNotNull(response.Properties, this.ErrorFormat("The response.Properties should not be null."));
                Assert.IsTrue(response.Properties.Length > 0, this.ErrorFormat("The response.Properties should not be empty."));
                foreach (var property in response.Properties)
                {
                    Assert.IsNotNull(property, this.ErrorFormat("The property should not be null."));
                    Assert.AreEqual(GenericNamespace.MerchantAccount, property.Namespace, this.ErrorFormat("Wrong property namespace."));
                }
            }

            /// <summary>
            /// Validates getting the merchant property metadata with null request, expects error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("EB143D86-CA29-4900-B5FC-12E170DDBB25")]
            [DeploymentItem(@"TestData\PaymentProcessor_GetMerchantAccountPropertyMetadata.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GetMerchantAccountPropertyMetadata.xml", "PaymentProcessor_GetMerchantAccountPropertyMetadata_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GetMerchantAccountPropertyMetadata_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.GetMerchantAccountPropertyMetadata(null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            #endregion

            #region ValidateMerchantAccount tests

            /// <summary>
            /// Tests validating the merchant properties, success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("B2ADE1C9-3B68-44C8-AD27-63DD7AAB582E")]
            [DeploymentItem(@"TestData\PaymentProcessor_ValidateMerchantAccount.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_ValidateMerchantAccount.xml", "PaymentProcessor_ValidateMerchantAccount_Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_ValidateMerchantAccount_Success()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request request = this.helper.GetNewRequestWithMechantAccount();
                this.helper.PrintRequestWhenDebug(request);

                // Act.
                Response response = processor.ValidateMerchantAccount(request);

                // Assert.
                this.helper.PrintResponseWhenDebug(response);
                this.helper.AssertResponseNoError(response);
            }

            /// <summary>
            /// Tests validating the merchant properties with null request, expect error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("16CEAA86-2CD4-45A1-8D64-40D5369F3518")]
            [DeploymentItem(@"TestData\PaymentProcessor_ValidateMerchantAccount.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_ValidateMerchantAccount.xml", "PaymentProcessor_ValidateMerchantAccount_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_ValidateMerchantAccount_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.ValidateMerchantAccount(null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            /// <summary>
            /// Tests validating the merchant properties with invalid properties, expect error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("6D7701F0-D6E4-4AB6-B312-32A522902342")]
            [DeploymentItem(@"TestData\PaymentProcessor_ValidateMerchantAccount.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_ValidateMerchantAccount.xml", "PaymentProcessor_ValidateMerchantAccount_InvalidProperties", DataAccessMethod.Sequential)]
            public void PaymentProcessor_ValidateMerchantAccount_InvalidProperties()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request request = this.helper.GetNewRequestWithMechantAccount();
                this.helper.PrintRequestWhenDebug(request);

                // Act.
                Response response = processor.ValidateMerchantAccount(request);

                // Assert.
                this.helper.PrintResponseWhenDebug(response);
                this.helper.AssertResponseErrorCount(response);
            }

            #endregion

            #region GenerateCardToken tests

            /// <summary>
            /// Validates tokenization, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("50E76E14-8948-41FD-BB5E-7B725CE79920")]
            [DeploymentItem(@"TestData\PaymentProcessor_GenerateCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GenerateCardToken.xml", "PaymentProcessor_GenerateCardToken_Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GenerateCardToken_Success()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request request = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                request.Properties = this.helper.CombineProperties(request.Properties, cardProperties);
                this.helper.PrintRequestWhenDebug(request);

                // Act.
                Response response = processor.GenerateCardToken(request, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(response);
                this.helper.AssertResponseNoError(response);

                Hashtable responseProperties = PaymentProperty.ConvertToHashtable(response.Properties);
                this.helper.AssertPropertyNotNull(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId);
                this.helper.AssertPropertyNotNull(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardToken);
                this.helper.AssertPropertyNotNull(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits);
                this.helper.AssertNoMerchantCredential(response.Properties);
                this.helper.AssertNoPCIProperties(responseProperties);

                Hashtable requestProperties = PaymentProperty.ConvertToHashtable(request.Properties);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Name);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress2);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.City);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.State);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Country);
                this.helper.AssertPropertyMatch(requestProperties, responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Phone);
            }

            /// <summary>
            /// Validates tokenization with null request, expects error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("F6565B27-2D33-4762-BF6B-6A78CC7F32ED")]
            [DeploymentItem(@"TestData\PaymentProcessor_GenerateCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GenerateCardToken.xml", "PaymentProcessor_GenerateCardToken_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GenerateCardToken_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.GenerateCardToken(null, null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            /// <summary>
            /// Validates tokenization with invalid card data, expects error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("C5D66D00-7E63-48B7-AE5E-0070CFE5D5AD")]
            [DeploymentItem(@"TestData\PaymentProcessor_GenerateCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GenerateCardToken.xml", "PaymentProcessor_GenerateCardToken_InvalidInput", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GenerateCardToken_InvalidInput()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request request = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                request.Properties = this.helper.CombineProperties(request.Properties, cardProperties);
                this.helper.PrintRequestWhenDebug(request);

                // Act.
                Response response = processor.GenerateCardToken(request, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(response);
                this.helper.AssertResponseWithError(response);
                if (response.Properties != null)
                {
                    Hashtable responseProperties = PaymentProperty.ConvertToHashtable(response.Properties);
                    this.helper.AssertNoPCIProperties(responseProperties);
                }
            }

            #endregion

            #region Authorize tests

            /// <summary>
            /// Validates basic authorization, manual or swipe, various industries, various card types, expects success. 
            /// Validates capture, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("5885D479-4F3D-4E3C-8FED-CADDE2E6CA32")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_Basic.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_Basic.xml", "PaymentProcessor_Authorize_Basic", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_Basic()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseNoError(authorizeResponse);

                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                Hashtable authorizeRequestProperties = PaymentProperty.ConvertToHashtable(authorizeRequest.Properties);
                this.helper.AssertNoMerchantCredential(authorizeResponse.Properties);
                this.helper.AssertNoPCIProperties(authorizeResponseProperties);
                this.helper.AssertPropertyMatch(authorizeRequestProperties, authorizeResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, AuthorizationResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CardType);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount);
                this.helper.AssertPropertyNotNull(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CurrencyCode);
                this.helper.AssertPropertyNotNull(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Last4Digits);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.IsSwipe, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.IsSwiped);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.TransactionType, TransactionType.Authorize.ToString());

                // Debit card authorization and some credit cards (gift card) have available balance returned
                decimal expectedAvailableBalance;
                if (this.testContext.TryGetDecimalValue(ColumnName.ExpectedAvailableBalance, out expectedAvailableBalance))
                {
                    decimal actualAvailableBalance;
                    if (PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AvailableBalance, out actualAvailableBalance))
                    {
                        Assert.AreEqual(expectedAvailableBalance, actualAvailableBalance, this.ErrorFormat("The available balance is not expected."));
                    }
                    else
                    {
                        Assert.Fail(this.ErrorFormat("Available balance is not found from the authorization response."));
                    }
                }

                // Arrange 2.
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act 2. 
                Response captureResponse = processor.Capture(captureRequest);

                // Assert 2.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseNoError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                Hashtable captureRequestProperties = PaymentProperty.ConvertToHashtable(captureRequest.Properties);
                this.helper.AssertNoMerchantCredential(captureResponse.Properties);
                this.helper.AssertNoPCIProperties(captureResponseProperties);
                this.helper.AssertPropertyMatch(captureRequestProperties, captureResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(captureRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardType);
                this.helper.AssertPropertyNotNull(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CurrencyCode);
                this.helper.AssertPropertyNotNull(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.Last4Digits);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.TransactionType, TransactionType.Capture.ToString());
            }

            /// <summary>
            /// Validates authorization with SupportCardTokenization = True, various card types, expects success.
            /// Validates capture, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("C4CD53EC-095F-430B-AD70-5D0896D2522D")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_SupportCardTokenization.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_SupportCardTokenization.xml", "PaymentProcessor_Authorize_SupportCardTokenization", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_SupportCardTokenization()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseNoError(authorizeResponse);

                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, AuthorizationResult.Success.ToString());
                this.helper.AssertPropertyNotNull(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CardToken);
                this.helper.AssertPropertyNotNull(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.UniqueCardId);

                // Arrange 2.
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act 2. 
                Response captureResponse = processor.Capture(captureRequest);

                // Assert 2.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseNoError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Success.ToString());
                this.helper.AssertPropertyNotNull(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardToken);
                this.helper.AssertPropertyNotNull(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.UniqueCardId);
            }

            /// <summary>
            /// Validates authorization with voice authorization code, various card types, expects success.
            /// Validates capture, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("E7611626-32F0-4702-8381-7166D76B697B")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_VoiceAuthorization.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_VoiceAuthorization.xml", "PaymentProcessor_Authorize_VoiceAuthorization", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_VoiceAuthorization()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseNoError(authorizeResponse);

                Hashtable authorizeRequestProperties = PaymentProperty.ConvertToHashtable(authorizeRequest.Properties);
                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, AuthorizationResult.Success.ToString());
                this.helper.AssertPropertyNotNull(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.VoiceAuthorizationCode);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.VoiceAuthorizationCode, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.VoiceAuthorizationCode);

                // Arrange 2.
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act 2. 
                Response captureResponse = processor.Capture(captureRequest);

                // Assert 2.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseNoError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Success.ToString());
            }

            /// <summary>
            /// Validates partial authorization (when allowed), various card types, expects success.
            /// Validates capture, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("BE41BE84-85DB-4F81-9FF2-4B99C8A0D1B2")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_PartialAuthorization.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_PartialAuthorization.xml", "PaymentProcessor_Authorize_PartialAuthorizationAllowed", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_PartialAuthorizationAllowed()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseNoError(authorizeResponse);

                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, AuthorizationResult.PartialAuthorization.ToString());

                Hashtable authorizeRequestProperties = PaymentProperty.ConvertToHashtable(authorizeRequest.Properties);
                decimal amount;
                PaymentProperty.GetPropertyValue(authorizeRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, out amount);
                decimal approvedAmount;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount, out approvedAmount);
                Assert.IsTrue(amount > approvedAmount, this.ErrorFormat("The approved amount({0}) must be less than the request amount({1}).", approvedAmount, amount));

                // Arrange 2.
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act 2. 
                Response captureResponse = processor.Capture(captureRequest);

                // Assert 2.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseNoError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Success.ToString());
            }

            /// <summary>
            /// Validates partial authorization (when not allowed), various card types, expects failure.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("DF20F1AA-8116-4685-81AA-3D1DB04385EB")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_PartialAuthorization.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_PartialAuthorization.xml", "PaymentProcessor_Authorize_PartialAuthorizationNotAllowed", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_PartialAuthorizationNotAllowed()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseWithError(authorizeResponse);

                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, AuthorizationResult.Failure.ToString());

                decimal approvedAmount;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount, out approvedAmount);
                Assert.AreEqual(0M, approvedAmount, this.ErrorFormat("The approved amount must be zero."));
            }

            /// <summary>
            /// Validates address verification service (AVS) in authorization, expects different AVS results.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("E329CD7E-58AD-4D2D-A758-58D35E941563")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_AVS.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_AVS.xml", "PaymentProcessor_Authorize_AVS", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_AVS()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request request = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                var authProperties = this.helper.GetAuthorizationProperties();
                request.Properties = this.helper.CombineProperties(request.Properties, cardProperties, authProperties);
                this.helper.PrintRequestWhenDebug(request);

                // Act.
                Response response = processor.Authorize(request, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(response);
                this.helper.AssertResponseNotNull(response);
                this.helper.AssertResponseNoError(response);

                Hashtable responseProperties = PaymentProperty.ConvertToHashtable(response.Properties);
                string actualAVSResult;
                PaymentProperty.GetPropertyValue(responseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AVSResult, out actualAVSResult);
                string actualAVSDetail;
                PaymentProperty.GetPropertyValue(responseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AVSDetail, out actualAVSDetail);
                string expectedAVSResult = this.testContext.GetStringValue(ColumnName.ExpectedAVSResult);
                string expectedAVSDetail = this.testContext.GetStringValue(ColumnName.ExpectedAVSDetail);
                Assert.AreEqual(expectedAVSResult, actualAVSResult, this.ErrorFormat("The AVS result is not expected."));
                Assert.AreEqual(expectedAVSDetail, actualAVSDetail, this.ErrorFormat("The AVS detail is not expected."));
            }

            /// <summary>
            /// Validates CVV2 verification in authorization, expects different CVV2 results.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("3B3430DB-8F5F-482D-ACB2-927E87E1A96A")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_CVV2.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_CVV2.xml", "PaymentProcessor_Authorize_CVV2", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_CVV2()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request request = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                var authProperties = this.helper.GetAuthorizationProperties();
                request.Properties = this.helper.CombineProperties(request.Properties, cardProperties, authProperties);
                this.helper.PrintRequestWhenDebug(request);

                // Act.
                Response response = processor.Authorize(request, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(response);
                this.helper.AssertResponseNotNull(response);
                this.helper.AssertResponseNoError(response);

                Hashtable responseProperties = PaymentProperty.ConvertToHashtable(response.Properties);
                string actualCVV2Result;
                PaymentProperty.GetPropertyValue(responseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CVV2Result, out actualCVV2Result);
                string expectedCVV2Result = this.testContext.GetStringValue(ColumnName.ExpectedCVV2Result);
                Assert.AreEqual(expectedCVV2Result, actualCVV2Result, this.ErrorFormat("The CVV2 result is not expected."));
            }

            /// <summary>
            /// Validates debit cash back in authorization, expects different results.
            /// Validates capture if authorization succeeds, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("9C024685-C102-4A34-874F-37F62DB507A8")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_Cashback.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_Cashback.xml", "PaymentProcessor_Authorize_Cashback", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_Cashback()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);

                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                string actualAuthorizationResult;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, out actualAuthorizationResult);
                string expectedAuthorizationResult = this.testContext.GetStringValue(ColumnName.ExpectedAuthorizationResult);
                Assert.AreEqual(expectedAuthorizationResult, actualAuthorizationResult, this.ErrorFormat("The authorization result is not expected."));

                Hashtable authorizeRequestProperties = PaymentProperty.ConvertToHashtable(authorizeRequest.Properties);
                if (AuthorizationResult.Success.ToString().Equals(expectedAuthorizationResult, StringComparison.OrdinalIgnoreCase))
                {
                    this.helper.AssertResponseNoError(authorizeResponse);
                    this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CashBackAmount, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CashBackAmount);
                }
                else
                {
                    this.helper.AssertResponseWithError(authorizeResponse);
                    this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CashBackAmount, 0M);
                }

                // Only capture if authorization is success.
                if (!AuthorizationResult.Success.ToString().Equals(expectedAuthorizationResult, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Arrange 2.
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act 2. 
                Response captureResponse = processor.Capture(captureRequest);

                // Assert 2.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseNoError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Success.ToString());
            }

            /// <summary>
            /// Validates authorization using a card token, various industries, various card types, expects success.
            /// Validates capture, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("75DA49D5-721C-4E53-8F73-3966DFB6F004")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_WithCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_WithCardToken.xml", "PaymentProcessor_Authorize_WithCardToken", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_WithCardToken()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request tokenizeRequest = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                tokenizeRequest.Properties = this.helper.CombineProperties(tokenizeRequest.Properties, cardProperties);
                this.helper.PrintRequestWhenDebug(tokenizeRequest);

                Response tokenizeResponse = processor.GenerateCardToken(tokenizeRequest, null);
                this.helper.PrintResponseWhenDebug(tokenizeResponse);

                Request authorizeRequest = this.helper.CreateAuthorizeRequest(tokenizeResponse);
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseNoError(authorizeResponse);

                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                Hashtable authorizeRequestProperties = PaymentProperty.ConvertToHashtable(authorizeRequest.Properties);
                this.helper.AssertNoMerchantCredential(authorizeResponse.Properties);
                this.helper.AssertNoPCIProperties(authorizeResponseProperties);
                this.helper.AssertPropertyMatch(authorizeRequestProperties, authorizeResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, AuthorizationResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CardType);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount);
                this.helper.AssertPropertyNotNull(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CurrencyCode);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Last4Digits);
                this.helper.AssertPropertyValueMatch(authorizeRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId, authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.UniqueCardId);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.TransactionType, TransactionType.Authorize.ToString());

                // Arrange 2.
                Request captureRequest = this.helper.CreateCaptureRequest(tokenizeResponse, authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act 2. 
                Response captureResponse = processor.Capture(captureRequest);

                // Assert 2.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseNoError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                Hashtable captureRequestProperties = PaymentProperty.ConvertToHashtable(captureRequest.Properties);
                this.helper.AssertNoMerchantCredential(captureResponse.Properties);
                this.helper.AssertNoPCIProperties(captureResponseProperties);
                this.helper.AssertPropertyMatch(captureRequestProperties, captureResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(captureRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardType);
                this.helper.AssertPropertyNotNull(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(captureRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CurrencyCode);
                this.helper.AssertPropertyValueMatch(captureRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits, captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.Last4Digits);
                this.helper.AssertPropertyValueMatch(captureRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId, captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.UniqueCardId);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.TransactionType, TransactionType.Capture.ToString());
            }

            /// <summary>
            /// Validates authorization failures, expects different failure results.
            /// Validates capture, expects error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("EB0E11DD-ECAA-4C77-9866-C226C4A33DFC")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_FailureResult.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_FailureResult.xml", "PaymentProcessor_Authorize_FailureResult", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_FailureResult()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseWithError(authorizeResponse);

                Hashtable authorizeResponseProperties = PaymentProperty.ConvertToHashtable(authorizeResponse.Properties);
                this.helper.AssertPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount, 0M);

                string actualAuthorizationResult;
                PaymentProperty.GetPropertyValue(authorizeResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, out actualAuthorizationResult);
                string expectedAuthorizationResult = this.testContext.GetStringValue(ColumnName.ExpectedAuthorizationResult);
                Assert.AreEqual(expectedAuthorizationResult, actualAuthorizationResult, this.ErrorFormat("The authorization result is not expected."));

                // Arrange 2.
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act 2. 
                Response captureResponse = processor.Capture(captureRequest);

                // Assert 2.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseWithError(captureResponse);
            }

            /// <summary>
            /// Validates authorization with invalid inputs, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("EB0E11DD-ECAA-4C77-9866-C226C4A33DFC")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_InvalidInput.xml", "PaymentProcessor_Authorize_InvalidInput", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_InvalidInput()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                this.helper.PrintRequestWhenDebug(authorizeRequest);

                // Act.
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(authorizeResponse);
                this.helper.AssertResponseNotNull(authorizeResponse);
                this.helper.AssertResponseWithError(authorizeResponse);
            }

            /// <summary>
            /// Validates authorization with null request, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("51F11635-5A72-4CE5-B834-CCFAF1D9F965")]
            [DeploymentItem(@"TestData\PaymentProcessor_Authorize_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Authorize_InvalidInput.xml", "PaymentProcessor_Authorize_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Authorize_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.Authorize(null, null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            #endregion

            #region Capture tests

            /*
             Most capture tests are covered in authorize tests.
             */

            /// <summary>
            /// Validates level 2/level 3 capture, various levels, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("C3D3783A-2F2E-42DF-B1E1-F49E0A6D3758")]
            [DeploymentItem(@"TestData\PaymentProcessor_Capture_Level2Level3.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Capture_Level2Level3.xml", "PaymentProcessor_Capture_Level2Level3Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Capture_Level2Level3Success()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act.
                Response captureResponse = processor.Capture(captureRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseNoError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Success.ToString());
            }

            /// <summary>
            /// Validates capture failures, expects different failure results.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("328F3CA7-8216-4C7A-A043-177E9B529867")]
            [DeploymentItem(@"TestData\PaymentProcessor_Capture_FailureResult.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Capture_FailureResult.xml", "PaymentProcessor_Capture_FailureResult", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Capture_FailureResult()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act.
                Response captureResponse = processor.Capture(captureRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseWithError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                string actualCaptureResult;
                PaymentProperty.GetPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, out actualCaptureResult);
                string expectedCaptureResult = this.testContext.GetStringValue(ColumnName.ExpectedCaptureResult);
                Assert.AreEqual(expectedCaptureResult, actualCaptureResult, this.ErrorFormat("The capture result is not expected."));

                string expectedErrorCode = this.testContext.GetStringValue(ColumnName.ExpectedErrorCode);
                if (!string.IsNullOrWhiteSpace(expectedErrorCode))
                {
                    this.helper.AssertResponseErrorCode(captureResponse, expectedErrorCode);
                }
            }

            /// <summary>
            /// Validates capture with invalid amount, expect error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("2072C745-2A32-4E39-86D6-54AD8E6EA671")]
            [DeploymentItem(@"TestData\PaymentProcessor_Capture_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Capture_InvalidInput.xml", "PaymentProcessor_Capture_InvalidAmount", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Capture_InvalidAmount()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);
                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse, 99.99M);
                this.helper.PrintRequestWhenDebug(captureRequest);

                // Act.
                Response captureResponse = processor.Capture(captureRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(captureResponse);
                this.helper.AssertResponseNotNull(captureResponse);
                this.helper.AssertResponseWithError(captureResponse);

                Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponse.Properties);
                this.helper.AssertPropertyValue(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, CaptureResult.Failure.ToString());
            }

            /// <summary>
            /// Validates capture with null request, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("6524A78C-AA50-4114-9DE9-244AA40DF7DA")]
            [DeploymentItem(@"TestData\PaymentProcessor_Capture_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Capture_InvalidInput.xml", "PaymentProcessor_Capture_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Capture_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.Capture(null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            #endregion

            #region Void tests

            /// <summary>
            /// Authorizes a payment, validates voiding it (basic), manual or swipe, various industries, various card types, expects success. 
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("B62275DB-48C1-460E-98CE-1005E49F4EB7")]
            [DeploymentItem(@"TestData\PaymentProcessor_Void_Basic.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Void_Basic.xml", "PaymentProcessor_Void_Basic", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Void_Basic()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                Request voidRequest = this.helper.CreateVoidRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(voidRequest);

                // Act.
                Response voidResponse = processor.Void(voidRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(voidResponse);
                this.helper.AssertResponseNotNull(voidResponse);
                this.helper.AssertResponseNoError(voidResponse);

                Hashtable voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponse.Properties);
                Hashtable voidRequestProperties = PaymentProperty.ConvertToHashtable(voidRequest.Properties);
                this.helper.AssertNoMerchantCredential(voidResponse.Properties);
                this.helper.AssertNoPCIProperties(voidResponseProperties);
                this.helper.AssertPropertyMatch(voidRequestProperties, voidResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, VoidResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(voidRequestProperties, GenericNamespace.AuthorizationResponse, PaymentCardProperties.CardType, voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CardType);
                this.helper.AssertPropertyNotNull(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(voidRequestProperties, GenericNamespace.AuthorizationResponse, TransactionDataProperties.CurrencyCode, voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CurrencyCode);
                this.helper.AssertPropertyNotNull(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.Last4Digits);
                this.helper.AssertPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.TransactionType, TransactionType.Void.ToString());
            }

            /// <summary>
            /// Authorizes with voice authorization code, validates voiding it, various card types, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("13B499F1-4203-4A80-BDA3-CE8DDB9BE3E0")]
            [DeploymentItem(@"TestData\PaymentProcessor_Void_VoiceAuthorization.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Void_VoiceAuthorization.xml", "PaymentProcessor_Void_VoiceAuthorization", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Void_VoiceAuthorization()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                Request voidRequest = this.helper.CreateVoidRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(voidRequest);

                // Act.
                Response voidResponse = processor.Void(voidRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(voidResponse);
                this.helper.AssertResponseNotNull(voidResponse);
                this.helper.AssertResponseNoError(voidResponse);

                Hashtable voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponse.Properties);
                this.helper.AssertPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, VoidResult.Success.ToString());
            }

            /// <summary>
            /// Authorizes with partial authorization, validates voiding it, various card types, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("9AB1ECEA-2145-4D5C-A047-0C6E23B37B30")]
            [DeploymentItem(@"TestData\PaymentProcessor_Void_PartialAuthorization.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Void_PartialAuthorization.xml", "PaymentProcessor_Void_PartialAuthorization", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Void_PartialAuthorization()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                Request voidRequest = this.helper.CreateVoidRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(voidRequest);

                // Act.
                Response voidResponse = processor.Void(voidRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(voidResponse);
                this.helper.AssertResponseNotNull(voidResponse);
                this.helper.AssertResponseNoError(voidResponse);

                Hashtable voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponse.Properties);
                this.helper.AssertPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, VoidResult.Success.ToString());
            }

            /// <summary>
            /// Authorizes with debit cash back, validates voiding it, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("BE23D2E0-91A3-4041-96A0-ABF25F76506E")]
            [DeploymentItem(@"TestData\PaymentProcessor_Void_Cashback.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Void_Cashback.xml", "PaymentProcessor_Void_Cashback", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Void_Cashback()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                Request voidRequest = this.helper.CreateVoidRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(voidRequest);

                // Act.
                Response voidResponse = processor.Void(voidRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(voidResponse);
                this.helper.AssertResponseNotNull(voidResponse);
                this.helper.AssertResponseNoError(voidResponse);

                Hashtable voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponse.Properties);
                this.helper.AssertPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, VoidResult.Success.ToString());
            }

            /// <summary>
            /// Tokenizes a card, authorizes with the token, validates voiding it, various industries, various card types, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("311F5DF1-37BE-4DD1-8936-C6DEFCC1B37E")]
            [DeploymentItem(@"TestData\PaymentProcessor_Void_WithCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Void_WithCardToken.xml", "PaymentProcessor_Void_WithCardToken", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Void_WithCardToken()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request tokenizeRequest = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                tokenizeRequest.Properties = this.helper.CombineProperties(tokenizeRequest.Properties, cardProperties);
                this.helper.PrintRequestWhenDebug(tokenizeRequest);

                Response tokenizeResponse = processor.GenerateCardToken(tokenizeRequest, null);
                this.helper.PrintResponseWhenDebug(tokenizeResponse);

                Request authorizeRequest = this.helper.CreateAuthorizeRequest(tokenizeResponse);
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                Request voidRequest = this.helper.CreateVoidRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(voidRequest);

                // Act.
                Response voidResponse = processor.Void(voidRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(voidResponse);
                this.helper.AssertResponseNotNull(voidResponse);
                this.helper.AssertResponseNoError(voidResponse);

                Hashtable voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponse.Properties);
                Hashtable voidRequestProperties = PaymentProperty.ConvertToHashtable(voidRequest.Properties);
                this.helper.AssertNoMerchantCredential(voidResponse.Properties);
                this.helper.AssertNoPCIProperties(voidResponseProperties);
                this.helper.AssertPropertyMatch(voidRequestProperties, voidResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, VoidResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(voidRequestProperties, GenericNamespace.AuthorizationResponse, PaymentCardProperties.CardType, voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CardType);
                this.helper.AssertPropertyValueMatch(voidRequestProperties, GenericNamespace.AuthorizationResponse, PaymentCardProperties.UniqueCardId, voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.UniqueCardId);
                this.helper.AssertPropertyNotNull(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(voidRequestProperties, GenericNamespace.AuthorizationResponse, TransactionDataProperties.CurrencyCode, voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CurrencyCode);
                this.helper.AssertPropertyNotNull(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.Last4Digits);
                this.helper.AssertPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.TransactionType, TransactionType.Void.ToString());
            }

            /// <summary>
            /// Validates void failures, expects different failure results.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("1FD837F8-B70F-4DC6-865C-037A75E670F4")]
            [DeploymentItem(@"TestData\PaymentProcessor_Void_FailureResult.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Void_FailureResult.xml", "PaymentProcessor_Void_FailureResult", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Void_FailureResult()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);
                Request voidRequest = this.helper.CreateVoidRequest(authorizeResponse);
                this.helper.PrintRequestWhenDebug(voidRequest);

                // Act.
                Response voidResponse = processor.Void(voidRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(voidResponse);
                this.helper.AssertResponseNotNull(voidResponse);
                this.helper.AssertResponseWithError(voidResponse);

                Hashtable voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponse.Properties);
                string actualVoidResult;
                PaymentProperty.GetPropertyValue(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, out actualVoidResult);
                string expectedVoidResult = this.testContext.GetStringValue(ColumnName.ExpectedVoidResult);
                Assert.AreEqual(expectedVoidResult, actualVoidResult, this.ErrorFormat("The void result is not expected."));

                string expectedErrorCode = this.testContext.GetStringValue(ColumnName.ExpectedErrorCode);
                if (!string.IsNullOrWhiteSpace(expectedErrorCode))
                {
                    this.helper.AssertResponseErrorCode(voidResponse, expectedErrorCode);
                }
            }

            /// <summary>
            /// Validates void with null request, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("6AE216BA-1F40-4C98-8749-3BE2E475DF4A")]
            [DeploymentItem(@"TestData\PaymentProcessor_Void_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Void_InvalidInput.xml", "PaymentProcessor_Void_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Void_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.Void(null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            #endregion

            #region Refund tests

            /// <summary>
            /// Validates basic refund (not linked refund), manual or swipe, various industries, various card types, expects success. 
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("90AB0543-B7A3-4260-B6C7-C5BD3C123404")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_Basic.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_Basic.xml", "PaymentProcessor_Refund_Basic", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_Basic()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request refundRequest = this.GetRefundRequest();
                this.helper.PrintRequestWhenDebug(refundRequest);

                // Act.
                Response refundResponse = processor.Refund(refundRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(refundResponse);
                this.helper.AssertResponseNotNull(refundResponse);
                this.helper.AssertResponseNoError(refundResponse);

                Hashtable refundResponseProperties = PaymentProperty.ConvertToHashtable(refundResponse.Properties);
                Hashtable refundRequestProperties = PaymentProperty.ConvertToHashtable(refundRequest.Properties);
                this.helper.AssertNoMerchantCredential(refundResponse.Properties);
                this.helper.AssertNoPCIProperties(refundResponseProperties);
                this.helper.AssertPropertyMatch(refundRequestProperties, refundResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, RefundResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardType);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovedAmount);
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CurrencyCode);
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.Last4Digits);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionType, TransactionType.Refund.ToString());
            }

            /// <summary>
            /// Validates refund with SupportCardTokenization = True, various card types, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("92AC09DB-A692-4AF4-9F1D-84709F060F4E")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_SupportCardTokenization.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_SupportCardTokenization.xml", "PaymentProcessor_Refund_SupportCardTokenization", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_SupportCardTokenization()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request refundRequest = this.GetRefundRequest();
                this.helper.PrintRequestWhenDebug(refundRequest);

                // Act.
                Response refundResponse = processor.Refund(refundRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(refundResponse);
                this.helper.AssertResponseNotNull(refundResponse);
                this.helper.AssertResponseNoError(refundResponse);

                Hashtable refundResponseProperties = PaymentProperty.ConvertToHashtable(refundResponse.Properties);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, RefundResult.Success.ToString());
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardToken);
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.UniqueCardId);
            }

            /// <summary>
            /// Validates refund using a card token (not linked refund), various industries, various card types, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("F60B7A48-F8FF-46DB-BF96-FB164B18AA86")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_WithCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_WithCardToken.xml", "PaymentProcessor_Refund_WithCardToken", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_WithCardToken()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request tokenizeRequest = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                tokenizeRequest.Properties = this.helper.CombineProperties(tokenizeRequest.Properties, cardProperties);
                this.helper.PrintRequestWhenDebug(tokenizeRequest);

                Response tokenizeResponse = processor.GenerateCardToken(tokenizeRequest, null);
                this.helper.PrintResponseWhenDebug(tokenizeResponse);

                Request refundRequest = this.helper.CreateRefundRequestFromTokenizeReponse(tokenizeResponse);
                this.helper.PrintRequestWhenDebug(refundRequest);

                // Act.
                Response refundResponse = processor.Refund(refundRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(refundResponse);
                this.helper.AssertResponseNotNull(refundResponse);
                this.helper.AssertResponseNoError(refundResponse);

                Hashtable refundResponseProperties = PaymentProperty.ConvertToHashtable(refundResponse.Properties);
                Hashtable refundRequestProperties = PaymentProperty.ConvertToHashtable(refundRequest.Properties);
                this.helper.AssertNoMerchantCredential(refundResponse.Properties);
                this.helper.AssertNoPCIProperties(refundResponseProperties);
                this.helper.AssertPropertyMatch(refundRequestProperties, refundResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, RefundResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardType);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovedAmount);
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CurrencyCode);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.Last4Digits);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.UniqueCardId);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionType, TransactionType.Refund.ToString());
            }

            /// <summary>
            /// Validates linked refund (without card token), manual or swipe, various industries, various card types, expects success. 
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("FFE11E4F-A819-4353-B869-7DC4DEE202D3")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_LinkedRefundWithoutCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_LinkedRefundWithoutCardToken.xml", "PaymentProcessor_Refund_LinkedRefundWithoutCardToken", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_LinkedRefundWithoutCardToken()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request authorizeRequest = this.GetAuthorizeRequest();
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                Request captureRequest = this.helper.CreateCaptureRequest(authorizeResponse);
                Response captureResponse = processor.Capture(captureRequest);

                Request refundRequest = this.helper.CreateRefundRequestFromCaptureResponse(captureResponse);
                this.helper.PrintRequestWhenDebug(refundRequest);

                // Act.
                Response refundResponse = processor.Refund(refundRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(refundResponse);
                this.helper.AssertResponseNotNull(refundResponse);
                this.helper.AssertResponseNoError(refundResponse);

                Hashtable refundResponseProperties = PaymentProperty.ConvertToHashtable(refundResponse.Properties);
                Hashtable refundRequestProperties = PaymentProperty.ConvertToHashtable(refundRequest.Properties);
                this.helper.AssertNoMerchantCredential(refundResponse.Properties);
                this.helper.AssertNoPCIProperties(refundResponseProperties);
                this.helper.AssertPropertyMatch(refundRequestProperties, refundResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, RefundResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardType, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardType);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovedAmount);
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CurrencyCode);
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.Last4Digits);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionType, TransactionType.Refund.ToString());
            }

            /// <summary>
            /// Validates linked refund using a card token, various industries, various card types, expects success.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("6865A851-58F3-4A5C-82AF-BDE4C668E174")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_LinkedRefundWithCardToken.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_LinkedRefundWithCardToken.xml", "PaymentProcessor_Refund_LinkedRefundWithCardToken", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_LinkedRefundWithCardToken()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request tokenizeRequest = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                tokenizeRequest.Properties = this.helper.CombineProperties(tokenizeRequest.Properties, cardProperties);
                Response tokenizeResponse = processor.GenerateCardToken(tokenizeRequest, null);

                Request authorizeRequest = this.helper.CreateAuthorizeRequest(tokenizeResponse);
                Response authorizeResponse = processor.Authorize(authorizeRequest, null);

                Request captureRequest = this.helper.CreateCaptureRequest(tokenizeResponse, authorizeResponse);
                Response captureResponse = processor.Capture(captureRequest);

                Request refundRequest = this.helper.CreateRefundRequestFromCaptureResponse(captureResponse);
                this.helper.PrintRequestWhenDebug(refundRequest);

                // Act.
                Response refundResponse = processor.Refund(refundRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(refundResponse);
                this.helper.AssertResponseNotNull(refundResponse);
                this.helper.AssertResponseNoError(refundResponse);

                Hashtable refundResponseProperties = PaymentProperty.ConvertToHashtable(refundResponse.Properties);
                Hashtable refundRequestProperties = PaymentProperty.ConvertToHashtable(refundRequest.Properties);
                this.helper.AssertNoMerchantCredential(refundResponse.Properties);
                this.helper.AssertNoPCIProperties(refundResponseProperties);
                this.helper.AssertPropertyMatch(refundRequestProperties, refundResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, RefundResult.Success.ToString());
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardType, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardType);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovedAmount);
                this.helper.AssertPropertyNotNull(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderTransactionId);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CurrencyCode);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.Last4Digits, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.Last4Digits);
                this.helper.AssertPropertyValueMatch(refundRequestProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.UniqueCardId, refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.UniqueCardId);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionType, TransactionType.Refund.ToString());
            }

            /// <summary>
            /// Validates level 2/level 3 refund, various purchase levels, expects success. 
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("B3DE30D9-BBCE-40D3-8842-617F04C84EAE")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_Level2Level3.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_Level2Level3.xml", "PaymentProcessor_Refund_Level2Level3Success", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_Level2Level3Success()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request refundRequest = this.GetRefundRequest();
                this.helper.PrintRequestWhenDebug(refundRequest);

                // Act.
                Response refundResponse = processor.Refund(refundRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(refundResponse);
                this.helper.AssertResponseNotNull(refundResponse);
                this.helper.AssertResponseNoError(refundResponse);

                Hashtable refundResponseProperties = PaymentProperty.ConvertToHashtable(refundResponse.Properties);
                this.helper.AssertPropertyValue(refundResponseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, RefundResult.Success.ToString());
            }

            /// <summary>
            /// Validates refund with invalid inputs, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("5566CCAE-B40B-42F5-A989-E952B4936937")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_InvalidInput.xml", "PaymentProcessor_Refund_InvalidInput", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_InvalidInput()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request refundRequest = this.GetRefundRequest();
                this.helper.PrintRequestWhenDebug(refundRequest);

                // Act.
                Response refundResponse = processor.Refund(refundRequest, null);

                // Assert.
                this.helper.PrintResponseWhenDebug(refundResponse);
                this.helper.AssertResponseNotNull(refundResponse);
                this.helper.AssertResponseWithError(refundResponse);
            }

            /// <summary>
            /// Validates refund with null request, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("8899282A-9B14-472F-87FB-21E57D4CEA41")]
            [DeploymentItem(@"TestData\PaymentProcessor_Refund_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_Refund_InvalidInput.xml", "PaymentProcessor_Refund_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_Refund_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.Refund(null, null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            #endregion

            #region GetPaymentAcceptPoint tests

            /*
             GetPaymentAcceptPoint is part of the payment accepting feature. It requires UX tests.
             The functional tests here only covers part of the scenario. 
             Please use Sample.MerchantWeb to validate the payment accepting feature.
             */

            /// <summary>
            /// Validates basic GetPaymentAcceptPoint, various industries, various transaction type, expects success. 
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("975EAB09-839A-4E2F-8D7E-1CF7A7B18794")]
            [DeploymentItem(@"TestData\PaymentProcessor_GetPaymentAcceptPoint_Basic.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GetPaymentAcceptPoint_Basic.xml", "PaymentProcessor_GetPaymentAcceptPoint_Basic", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GetPaymentAcceptPoint_Basic()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request acceptPointRequest = this.helper.GetNewRequestWithMechantAccount();
                var acceptPointProperties = this.helper.GetAcceptPointProperties();
                acceptPointRequest.Properties = this.helper.CombineProperties(acceptPointRequest.Properties, acceptPointProperties);
                this.helper.PrintRequestWhenDebug(acceptPointRequest);

                // Act.
                Response acceptPointResponse = processor.GetPaymentAcceptPoint(acceptPointRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(acceptPointResponse);
                this.helper.AssertResponseNotNull(acceptPointResponse);
                this.helper.AssertResponseNoError(acceptPointResponse);

                Hashtable acceptPointResponseProperties = PaymentProperty.ConvertToHashtable(acceptPointResponse.Properties);
                Hashtable acceptPointRequestProperties = PaymentProperty.ConvertToHashtable(acceptPointRequest.Properties);
                this.helper.AssertNoMerchantCredential(acceptPointResponse.Properties);
                this.helper.AssertNoPCIProperties(acceptPointResponseProperties);
                this.helper.AssertPropertyMatch(acceptPointRequestProperties, acceptPointResponseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                this.helper.AssertPropertyNotNull(acceptPointResponseProperties, GenericNamespace.TransactionData, TransactionDataProperties.PaymentAcceptUrl);
            }

            /// <summary>
            /// Validates GetPaymentAcceptPoint with invalid inputs, expects error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("85B0EE16-2E24-4157-9386-0E437CAEFFFC")]
            [DeploymentItem(@"TestData\PaymentProcessor_GetPaymentAcceptPoint_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GetPaymentAcceptPoint_InvalidInput.xml", "PaymentProcessor_GetPaymentAcceptPoint_InvalidInput", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GetPaymentAcceptPoint_InvalidInput()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request acceptPointRequest = this.helper.GetNewRequestWithMechantAccount();
                var acceptPointProperties = this.helper.GetAcceptPointProperties();
                acceptPointRequest.Properties = this.helper.CombineProperties(acceptPointRequest.Properties, acceptPointProperties);
                this.helper.PrintRequestWhenDebug(acceptPointRequest);

                // Act.
                Response acceptPointResponse = processor.GetPaymentAcceptPoint(acceptPointRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(acceptPointResponse);
                this.helper.AssertResponseNotNull(acceptPointResponse);
                this.helper.AssertResponseWithError(acceptPointResponse);
            }

            /// <summary>
            /// Validates GetPaymentAcceptPoint with null request, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("2A05B0C3-BA9D-4304-85D1-7536203AA89C")]
            [DeploymentItem(@"TestData\PaymentProcessor_GetPaymentAcceptPoint_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_GetPaymentAcceptPoint_InvalidInput.xml", "PaymentProcessor_GetPaymentAcceptPoint_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_GetPaymentAcceptPoint_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.GetPaymentAcceptPoint(null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            #endregion

            #region RetrievePaymentAcceptResult tests

            /*
             RetrievePaymentAcceptResult is part of the payment accepting feature. It requires UX tests.
             The functional tests here only covers part of the scenario. 
             Please use Sample.MerchantWeb to validate the payment accepting feature.
             */

            /// <summary>
            /// Validates RetrievePaymentAcceptResult with invalid inputs, expects error.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P1)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("D5D4C658-04AD-48AA-88E4-89C88C67357E")]
            [DeploymentItem(@"TestData\PaymentProcessor_RetrievePaymentAcceptResult_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_RetrievePaymentAcceptResult_InvalidInput.xml", "PaymentProcessor_RetrievePaymentAcceptResult_InvalidInput", DataAccessMethod.Sequential)]
            public void PaymentProcessor_RetrievePaymentAcceptResult_InvalidInput()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();
                Request acceptResultRequest = this.helper.GetNewRequestWithMechantAccount();
                var acceptPointProperties = this.helper.GetAcceptResultProperties();
                acceptResultRequest.Properties = this.helper.CombineProperties(acceptResultRequest.Properties, acceptPointProperties);
                this.helper.PrintRequestWhenDebug(acceptResultRequest);

                // Act.
                Response acceptResultResponse = processor.RetrievePaymentAcceptResult(acceptResultRequest);

                // Assert.
                this.helper.PrintResponseWhenDebug(acceptResultResponse);
                this.helper.AssertResponseNotNull(acceptResultResponse);
                this.helper.AssertResponseWithError(acceptResultResponse);
            }

            /// <summary>
            /// Validates RetrievePaymentAcceptResult with null request, expects errors.
            /// </summary>
            [TestMethod]
            [TestOwner("rexu")]
            [TestStatus(TestStatus.Stabilizing)]
            [TestPriority(Priority.P2)]
            [ExecutionGroup(ExecutionGroup.BAT)]
            [TestKey("AD79ACBA-EABC-48D5-9253-3C43AC6AB937")]
            [DeploymentItem(@"TestData\PaymentProcessor_RetrievePaymentAcceptResult_InvalidInput.xml", "TestData")]
            [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"TestData\PaymentProcessor_RetrievePaymentAcceptResult_InvalidInput.xml", "PaymentProcessor_RetrievePaymentAcceptResult_NullRequest", DataAccessMethod.Sequential)]
            public void PaymentProcessor_RetrievePaymentAcceptResult_NullRequest()
            {
                // Arrange.
                IPaymentProcessor processor = this.helper.GetPaymentProcessor();

                // Act.
                Response response = processor.RetrievePaymentAcceptResult(null);

                // Assert.
                this.helper.AssertResponseErrorCode(response, ErrorCode.InvalidRequest);
            }

            #endregion

            #region Helper methods

            private string ErrorFormat(string format, params object[] args)
            {
                return this.helper.ErrorFormat(format, args);
            }

            private Request GetAuthorizeRequest()
            {
                Request authorizeRequest = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                var authProperties = this.helper.GetAuthorizationProperties();
                authorizeRequest.Properties = this.helper.CombineProperties(authorizeRequest.Properties, cardProperties, authProperties);
                return authorizeRequest;
            }

            private Request GetRefundRequest()
            {
                Request refundRequest = this.helper.GetNewRequestWithMechantAccount();
                var cardProperties = this.helper.GetPaymentCardProperties();
                var authProperties = this.helper.GetRefundProperties();
                refundRequest.Properties = this.helper.CombineProperties(refundRequest.Properties, cardProperties, authProperties);
                return refundRequest;
            }

            #endregion
        }
    }
}