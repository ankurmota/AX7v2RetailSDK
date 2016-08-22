/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Retail.SampleConnector.Portable
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

        internal abstract class RequestBase
        {
            internal RequestBase()
                : base()
            {
            }

            internal string Locale { get; set; }

            internal string AssemblyName { get; set; }

            internal string ServiceAccountId { get; set; }

            internal string MerchantId { get; set; }

            internal string ProviderId { get; set; }

            internal string Environment { get; set; }

            internal string TestString { get; set; }

            internal decimal? TestDecimal { get; set; }

            internal DateTime? TestDate { get; set; }

            internal string SupportedCurrencies { get; set; }

            internal string SupportedTenderTypes { get; set; }

            internal string IndustryType { get; set; }

            internal bool? IsTestMode { get; set; }

            protected void ReadBaseProperties(Request request, List<PaymentError> errors)
            {
                if (request == null)
                {
                    throw new SampleException(ErrorCode.InvalidRequest, "Request is null.");
                }

                if (string.IsNullOrWhiteSpace(request.Locale))
                {
                    throw new SampleException(ErrorCode.InvalidRequest, "Locale is null or whitespaces.");
                }
                else
                {
                    this.Locale = request.Locale;
                }

                if (request.Properties == null || request.Properties.Length == 0)
                {
                    throw new SampleException(ErrorCode.InvalidRequest, "Request properties is null or empty.");
                }

                Hashtable properties = PaymentProperty.ConvertToHashtable(request.Properties);
                this.AssemblyName = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.AssemblyName,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.ServiceAccountId = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.ServiceAccountId,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.MerchantId = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.MerchantId,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.ProviderId = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    SampleMerchantAccountProperty.ProviderId,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.Environment = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    SampleMerchantAccountProperty.Environment);
                this.TestString = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    SampleMerchantAccountProperty.TestString,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.TestDecimal = PaymentUtilities.GetPropertyDecimalValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    SampleMerchantAccountProperty.TestDecimal,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.TestDate = PaymentUtilities.GetPropertyDateTimeValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    SampleMerchantAccountProperty.TestDate,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.SupportedCurrencies = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.SupportedCurrencies,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.SupportedTenderTypes = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.SupportedTenderTypes,
                    errors,
                    ErrorCode.InvalidMerchantProperty);
                this.IndustryType = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.IndustryType);
                this.IsTestMode = PaymentUtilities.GetPropertyBooleanValue(
                    properties,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.IsTestMode);
            }
        }
    }
}
