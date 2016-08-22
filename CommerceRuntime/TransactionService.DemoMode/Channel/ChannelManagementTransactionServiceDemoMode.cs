/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

namespace Contoso
{
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Channel demo mode transaction service.
        /// </summary>
        public class ChannelManagementTransactionServiceDemoMode : IRequestHandler
        {
            private const string TestConnectorProperties = @"<![CDATA[<?xml version='1.0' encoding='utf-16'?>
                <ArrayOfPaymentProperty xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>AssemblyName</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>Microsoft.Dynamics.Retail.TestConnector, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>true</IsReadOnly>
                    <SequenceNumber>0</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>MerchantId</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>136e9c86-31a1-4177-b2b7-a027c63edbe0</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>1</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>ProviderId</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>467079b4-1601-4f79-83c9-f569872eb94e</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>2</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>SupportedCurrencies</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>USD;CAD</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>3</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>SupportedTenderTypes</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>Visa;MasterCard;Amex;Discover;Debit</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>4</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>IsAVSRequired</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>false</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>5</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>TestString</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>Test string 1234567890 1234567890 End.</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>6</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>TestDecimal</Name>
                    <ValueType>Decimal</ValueType>
                    <DecimalValue>12345.67</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>7</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>TestDate</Name>
                    <ValueType>DateTime</ValueType>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>2011-09-22T11:03:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>8</SequenceNumber>
                  </PaymentProperty>
                  <PaymentProperty>
                    <Namespace>MerchantAccount</Namespace>
                    <Name>ServiceAccountId</Name>
                    <ValueType>String</ValueType>
                    <StoredStringValue>20f8e810-65d3-4607-9567-90af63e5fb97</StoredStringValue>
                    <DecimalValue>0</DecimalValue>
                    <DateValue>0001-01-01T00:00:00</DateValue>
                    <SecurityLevel>None</SecurityLevel>
                    <IsEncrypted>false</IsEncrypted>
                    <IsPassword>false</IsPassword>
                    <IsReadOnly>false</IsReadOnly>
                    <SequenceNumber>1</SequenceNumber>
                  </PaymentProperty>
                </ArrayOfPaymentProperty>]]>";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(UpdateChannelPublishingStatusRealtimeRequest),
                        typeof(GetTerminalMerchantPaymentProviderDataRealtimeRequest),
                        typeof(GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(UpdateChannelPublishingStatusRealtimeRequest))
                {
                    response = UpdateChannelPublishingStatus();
                }
                else if (requestType == typeof(GetTerminalMerchantPaymentProviderDataRealtimeRequest))
                {
                    response = GetTerminalMerchantPaymentProviderData((GetTerminalMerchantPaymentProviderDataRealtimeRequest)request);
                }
                else if (requestType == typeof(GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest))
                {
                    response = GetChannelMerchantPaymentProviderData((GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Updates the publishing status and message for the given channel in AX.
            /// </summary>
            /// <returns>The null response.</returns>
            private static NullResponse UpdateChannelPublishingStatus()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "UpdateChannelPublishingStatus is not supported in demo mode.");
            }
    
            /// <summary>
            /// Get terminal merchant payment provider data.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "Always return test connector default setting in demo mode")]
            private static GetTerminalMerchantPaymentProviderDataRealtimeResponse GetTerminalMerchantPaymentProviderData(GetTerminalMerchantPaymentProviderDataRealtimeRequest request)
            {
                var paymentMerchantInformation = new PaymentMerchantInformation(TestConnectorProperties);
    
                var response = new GetTerminalMerchantPaymentProviderDataRealtimeResponse(paymentMerchantInformation);
                return response;
            }
    
            /// <summary>
            /// Get channel merchant payment provider data.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "Throw exception for demo mode")]
            private static GetOnlineChannelMerchantPaymentProviderDataRealtimeResponse GetChannelMerchantPaymentProviderData(GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest request)
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetChannelMerchantPaymentProviderData is not supported in demo mode.");
            }
        }
    }
}
