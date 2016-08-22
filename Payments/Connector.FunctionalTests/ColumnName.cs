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
        /// <summary>
        /// The constants for column names in test data.
        /// </summary>
        internal static class ColumnName
        {
            /// <summary>
            /// Column name.
            /// </summary>
            public const string TestDescriptor = "TestDescriptor";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ConnectorType = "ConnectorType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ConnectorName = "ConnectorName";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string MerchantAccountXmlPath = "MerchantAccountXmlPath";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedErrorCount = "ExpectedErrorCount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedErrorCode = "ExpectedErrorCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedAVSResult = "ExpectedAVSResult";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedAVSDetail = "ExpectedAVSDetail";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedCVV2Result = "ExpectedCVV2Result";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedAvailableBalance = "ExpectedAvailableBalance";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedAuthorizationResult = "ExpectedAuthorizationResult";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedCaptureResult = "ExpectedCaptureResult";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpectedVoidResult = "ExpectedVoidResult";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ServiceAccountId = "ServiceAccountId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string AccountType = "AccountType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string AdditionalSecurityData = "AdditionalSecurityData";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string CardEntryType = "CardEntryType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string CardNumber = "CardNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string CardToken = "CardToken";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string CardType = "CardType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string CardVerificationValue = "CardVerificationValue";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string CashBackAmount = "CashBackAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string City = "City";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Country = "Country";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string EncryptedPin = "EncryptedPin";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpirationMonth = "ExpirationMonth";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExpirationYear = "ExpirationYear";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string IssuerName = "IssuerName";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string IsSwipe = "IsSwipe";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Last4Digits = "Last4Digits";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Name = "Name";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Phone = "Phone";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string PostalCode = "PostalCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ProcessorTenderId = "ProcessorTenderId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ShowSameAsShippingAddress = "ShowSameAsShippingAddress";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string State = "State";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string StreetAddress = "StreetAddress";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string StreetAddress2 = "StreetAddress2";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Track1 = "Track1";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Track2 = "Track2";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Track3 = "Track3";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Track4 = "Track4";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string UniqueCardId = "UniqueCardId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string VoiceAuthorizationCode = "VoiceAuthorizationCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string IndustryType = "IndustryType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string AllowVoiceAuthorization = "AllowVoiceAuthorization";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Amount = "Amount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string SupportCardTokenization = "SupportCardTokenization";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string PurchaseLevel = "PurchaseLevel";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string AllowPartialAuthorization = "AllowPartialAuthorization";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string CurrencyCode = "CurrencyCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string TerminalId = "TerminalId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Description = "Description";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExternalCustomerId = "ExternalCustomerId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExternalInvoiceNumber = "ExternalInvoiceNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ExternalReferenceId = "ExternalReferenceId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string IsTestMode = "IsTestMode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string ResponseCode = "ResponseCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string TransactionType = "TransactionType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string SupportCardSwipe = "SupportCardSwipe";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string HostPageOrigin = "HostPageOrigin";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string PaymentAcceptResultAccessCode = "PaymentAcceptResultAccessCode";

            #region Level 2 data columns

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataOrderDateTime = "Level2DataOrderDateTime";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataOrderNumber = "Level2DataOrderNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataInvoiceDateTime = "Level2DataInvoiceDateTime";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataInvoiceNumber = "Level2DataInvoiceNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataOrderDescription = "Level2DataOrderDescription";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataSummaryCommodityCode = "Level2DataSummaryCommodityCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantContact = "Level2DataMerchantContact";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantTaxId = "Level2DataMerchantTaxId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantType = "Level2DataMerchantType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataPurchaserId = "Level2DataPurchaserId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataPurchaserTaxId = "Level2DataPurchaserTaxId";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipToCity = "Level2DataShipToCity";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipToCounty = "Level2DataShipToCounty";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipToStateProvinceCode = "Level2DataShipToStateProvinceCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipToPostalCode = "Level2DataShipToPostalCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipToCountryCode = "Level2DataShipToCountryCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipFromCity = "Level2DataShipFromCity";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipFromCounty = "Level2DataShipFromCounty";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipFromStateProvinceCode = "Level2DataShipFromStateProvinceCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipFromPostalCode = "Level2DataShipFromPostalCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataShipFromCountryCode = "Level2DataShipFromCountryCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataDiscountAmount = "Level2DataDiscountAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMiscCharge = "Level2DataMiscCharge";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataDutyAmount = "Level2DataDutyAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataFreightAmount = "Level2DataFreightAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataIsTaxable = "Level2DataIsTaxable";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTotalTaxAmount = "Level2DataTotalTaxAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTotalTaxRate = "Level2DataTotalTaxRate";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantName = "Level2DataMerchantName";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantStreet = "Level2DataMerchantStreet";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantCity = "Level2DataMerchantCity";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantState = "Level2DataMerchantState";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantCounty = "Level2DataMerchantCounty";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantCountryCode = "Level2DataMerchantCountryCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMerchantZip = "Level2DataMerchantZip";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxRate = "Level2DataTaxRate";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxAmount = "Level2DataTaxAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxDescription = "Level2DataTaxDescription";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxTypeIdentifier = "Level2DataTaxTypeIdentifier";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataRequesterName = "Level2DataRequesterName";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTotalAmount = "Level2DataTotalAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataPurchaseCardType = "Level2DataPurchaseCardType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataAmexLegacyDescription1 = "Level2DataAmexLegacyDescription1";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataAmexLegacyDescription2 = "Level2DataAmexLegacyDescription2";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataAmexLegacyDescription3 = "Level2DataAmexLegacyDescription3";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataAmexLegacyDescription4 = "Level2DataAmexLegacyDescription4";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxDetailsTaxTypeIdentifier = "Level2DataTaxDetailsTaxTypeIdentifier";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxDetailsTaxRate = "Level2DataTaxDetailsTaxRate";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxDetailsTaxDescription = "Level2DataTaxDetailsTaxDescription";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataTaxDetailsTaxAmount = "Level2DataTaxDetailsTaxAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMiscellaneousChargesChargeType = "Level2DataMiscellaneousChargesChargeType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level2DataMiscellaneousChargesChargeAmount = "Level2DataMiscellaneousChargesChargeAmount";

            #endregion

            #region Level 3 data columns

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataSequenceNumber = "Level3DataSequenceNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataCommodityCode = "Level3DataCommodityCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataProductCode = "Level3DataProductCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataProductName = "Level3DataProductName";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataProductSKU = "Level3DataProductSKU";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataDescriptor = "Level3DataDescriptor";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataUnitOfMeasure = "Level3DataUnitOfMeasure";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataUnitPrice = "Level3DataUnitPrice";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataDiscount = "Level3DataDiscount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataDiscountRate = "Level3DataDiscountRate";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataQuantity = "Level3DataQuantity";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataMiscCharge = "Level3DataMiscCharge";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataNetTotal = "Level3DataNetTotal";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTaxAmount = "Level3DataTaxAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTaxRate = "Level3DataTaxRate";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTotalAmount = "Level3DataTotalAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataCostCenter = "Level3DataCostCenter";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataFreightAmount = "Level3DataFreightAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataHandlingAmount = "Level3DataHandlingAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataCarrierTrackingNumber = "Level3DataCarrierTrackingNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataMerchantTaxID = "Level3DataMerchantTaxID";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataMerchantCatalogNumber = "Level3DataMerchantCatalogNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTaxCategoryApplied = "Level3DataTaxCategoryApplied";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupAddress = "Level3DataPickupAddress";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupCity = "Level3DataPickupCity";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupState = "Level3DataPickupState";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupCounty = "Level3DataPickupCounty";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupZip = "Level3DataPickupZip";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupCountry = "Level3DataPickupCountry";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupDateTime = "Level3DataPickupDateTime";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataPickupRecordNumber = "Level3DataPickupRecordNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataCarrierShipmentNumber = "Level3DataCarrierShipmentNumber";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataUNSPSCCode = "Level3DataUNSPSCCode";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTaxDetailsTaxTypeIdentifier = "Level3DataTaxDetailsTaxTypeIdentifier";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTaxDetailsTaxRate = "Level3DataTaxDetailsTaxRate";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTaxDetailsTaxDescription = "Level3DataTaxDetailsTaxDescription";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataTaxDetailsTaxAmount = "Level3DataTaxDetailsTaxAmount";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataMiscellaneousChargesChargeType = "Level3DataMiscellaneousChargesChargeType";

            /// <summary>
            /// Column name.
            /// </summary>
            public const string Level3DataMiscellaneousChargesChargeAmount = "Level3DataMiscellaneousChargesChargeAmount";

            #endregion
        }
    }
}
