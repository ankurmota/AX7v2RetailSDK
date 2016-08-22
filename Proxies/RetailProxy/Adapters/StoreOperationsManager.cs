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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Text.RegularExpressions;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Entities;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;

        internal class StoreOperationsManager : IStoreOperationsManager
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "connectionRequest", Justification = "Preserving signature of deprecated method.")]
            public Task<Employee> LogOn(ConnectionRequest connectionRequest)
            {
                throw new NotSupportedException("LogOn API is not supported anymore. Please use AuthenticationManager.Token() API to perform user authentication.");
            }

            public Task<EnvironmentConfiguration> GetEnvironmentConfiguration()
            {
                // Loading the environment configuration is currently only supported in online mode.
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_RequestTypeNotSupported);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "transactionId", Justification = "Preserving signature of deprecated method.")]
            public Task LogOff(string transactionId)
            {
                return Task.Run(() =>
                {
                    throw new NotSupportedException("Log off operation is not supported.");
                });
            }

            public Task UnlockRegister(ConnectionRequest connectionRequest)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).UnlockRegister(connectionRequest));
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "connectionRequest", Justification = "Preserving signature of deprecated method.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operationId", Justification = "Preserving signature of deprecated method.")]
            public Task<string> ElevateUser(ConnectionRequest connectionRequest, int operationId)
            {
                throw new NotSupportedException("ElevateUser API is not supported anymore. Please use AuthenticationManager.Token() API to perform user elevation.");
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "userToken", Justification = "Preserving signature of deprecated method.")]
            public Task RevertToSelf(string userToken)
            {
                throw new NotSupportedException("RevertToSelf API is not supported anymore. Please use AuthenticationManager.SetAuthenticationTokens() API to revert to a previously acquired user token.");
            }

            public Task<EmployeeActivity> GetLatestActivity()
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).GetLatestEmployeeActivity());
            }

            public Task<DeviceActivationResult> ActivateDevice(string deviceNumber, string terminalId, string deviceId, bool forceActivate, int? deviceType)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).ActivateDevice(deviceNumber, terminalId, deviceId, forceActivate, deviceType));
            }

            public Task DeactivateDevice(string transactionId)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).DeactivateDevice(transactionId));
            }

            public Task ResetPassword(string userId, string newPassword, bool mustChangePasswordAtNextLogOn)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).ResetPassword(userId, newPassword, mustChangePasswordAtNextLogOn));
            }

            public Task ChangePassword(string userId, string oldPassword, string newPassword)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).ChangePassword(userId, oldPassword, newPassword));
            }

            public Task<CreateHardwareStationTokenResult> CreateHardwareStationToken()
            {
                // Hardware Station pairing must happen through Retail Server. Otherwise, having the certificate locally
                // deployed defeats the security assumption(s) of having a trusted third party component.
                throw new NotSupportedException("Create hardware station token is only supported through Retail Server.");
            }

            public Task<ValidateHardwareStationTokenResult> ValidateHardwareStationToken(string deviceNumber, string hardwareStationToken)
            {
                throw new NotSupportedException("Validate hardware station token is only supported through Retail Server.");
            }

            public Task<PagedResult<Affiliation>> GetAffiliations(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetAffiliations(queryResultSettings));
            }

            public Task<PagedResult<DeliveryOption>> GetDeliveryOptions(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelDeliveryOptions(queryResultSettings));
            }

            public Task<Barcode> GetBarcodeById(string barcodeId)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetBarcode(new ScanInfo { ScannedText = barcodeId }));
            }

            public Task<PagedResult<ButtonGrid>> GetButtonGrids(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => LayoutManager.Create(CommerceRuntimeManager.Runtime).GetButtonGrids(queryResultSettings));
            }

            public Task<ButtonGrid> GetButtonGridById(string buttonGridId)
            {
                return Task.Run(() => LayoutManager.Create(CommerceRuntimeManager.Runtime).GetButtonGridById(buttonGridId));
            }

            public Task<PagedResult<ButtonGrid>> GetButtonGridsByIds(GetButtonGridsByIdsCriteria getButtonGridsByIdsCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => LayoutManager.Create(CommerceRuntimeManager.Runtime).GetButtonGridsByIds(getButtonGridsByIdsCriteria.ButtonGridIds, queryResultSettings));
            }

            public Task<PagedResult<CardTypeInfo>> GetCardTypes(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetCardTypes(queryResultSettings));
            }

            public Task<PagedResult<CashDeclaration>> GetCashDeclarations(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelCashDeclarations(queryResultSettings));
            }

            public Task<PagedResult<CityInfo>> GetCities(string countryRegionId, string stateProvinceId, string countyId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetCities(countryRegionId, stateProvinceId, countyId, queryResultSettings));
            }

            public Task<PagedResult<CountryRegionInfo>> GetCountryRegionsByLanguageId(string languageId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetCountries(languageId, queryResultSettings));
            }

            public Task<PagedResult<CountryRegionInfo>> GetCountryRegions(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetCountries(null, queryResultSettings));
            }

            public Task<PagedResult<CountyInfo>> GetCounties(string countryRegionId, string stateProvinceId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetCounties(countryRegionId, stateProvinceId, queryResultSettings));
            }

            public Task<CreditMemo> GetCreditMemoById(string creditMemoId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetCreditMemo(creditMemoId));
            }

            public Task<decimal> RoundAmountByTenderType(decimal amount, string tenderTypeId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).RoundAmountByTenderType(amount, tenderTypeId));
            }

            public Task<PagedResult<Currency>> GetCurrencies(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetCurrencies(queryResultSettings));
            }

            public Task<bool> GetRetailTrialPlanOffer()
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetRetailTrialPlanOffer());
            }

            public Task<PagedResult<SalesTaxGroup>> GetSalesTaxGroups(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetSalesTaxGroups(queryResultSettings));
            }

            public Task<PagedResult<CurrencyAmount>> GetCurrenciesAmount(string currencyCode, decimal amount, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelCurrenciesAmount(currencyCode, amount, queryResultSettings));
            }

            public Task<PagedResult<NumberSequenceSeedData>> GetLatestNumberSequence(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetNextNumberSequence());
            }

            public Task<CurrencyAmount> CalculateTotalCurrencyAmount(System.Collections.Generic.IEnumerable<CurrencyRequest> currenciesAmount)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).CalculateTotalCurrency(currenciesAmount));
            }

            public Task<PagedResult<CustomerGroup>> GetCustomerGroups(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).GetCustomerGroups(queryResultSettings));
            }

            public Task<DeviceConfiguration> GetDeviceConfiguration()
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetDeviceConfiguration());
            }

            public Task<PagedResult<DiscountCode>> GetDiscountCodes(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetDiscountCodes(queryResultSettings));
            }

            public Task<DiscountCode> GetDiscountCode(string discountCode)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetDiscountCodeDetails(discountCode));
            }

            public Task<PagedResult<DiscountCode>> GetDiscountCodesByOfferId(string offerId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetDiscountCodesByOfferId(offerId, queryResultSettings));
            }

            public Task<PagedResult<DiscountCode>> GetDiscountCodesByKeyword(string keyword, DateTimeOffset activeDate, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetDiscountCodesByKeyword(keyword, activeDate, queryResultSettings));
            }

            public Task<PagedResult<DistrictInfo>> GetDistricts(string countryRegionId, string stateProvinceId, string countyId, string cityName, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetDistricts(countryRegionId, stateProvinceId, countyId, cityName, queryResultSettings));
            }

            public Task<PagedResult<HardwareStationProfile>> GetHardwareStationProfiles(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetHardwareStationProfiles(queryResultSettings));
            }

            public Task<HardwareProfile> GetHardwareProfileById(string hardwareProfileId)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetHardwareProfile(hardwareProfileId));
            }

            public Task<PaymentMerchantInformation> GetPaymentMerchantInformation(string hardwareProfileId)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetPaymentMerchantInformation(hardwareProfileId));
            }

            public Task<PagedResult<LocalizedString>> GetLocalizedStrings(string languageId, int? textId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetLocalizedStrings(languageId, textId, queryResultSettings));
            }

            public Task<OperationPermission> GetOperationPermissionById(int operationId)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).GetOperationPermissionsById((RetailOperation)operationId));
            }

            public Task<PagedResult<OperationPermission>> GetOperationPermissions(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).GetOperationPermissions(queryResultSettings));
            }

            public Task<PagedResult<ReasonCode>> GetReturnOrderReasonCodes(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetReturnOrderReasonCodes(queryResultSettings));
            }

            public Task<PagedResult<ReasonCode>> GetReasonCodes(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetReasonCodes(queryResultSettings));
            }

            public Task<PagedResult<ReasonCode>> GetReasonCodesById(string reasonCodeGroupId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetReasonCodesById(reasonCodeGroupId, queryResultSettings));
            }

            public Task<ReportDataSet> SearchReportDataSet(string reportId, System.Collections.Generic.IEnumerable<CommerceProperty> parameters)
            {
                return Task.Run(() => BusinessIntelligenceManager.Create(CommerceRuntimeManager.Runtime).SearchReportDataSet(reportId, parameters));
            }

            public Task<ReportDataSet> GetReportDataSetById(string reportId)
            {
                return Task.Run(() => BusinessIntelligenceManager.Create(CommerceRuntimeManager.Runtime).GetReportDataSetById(reportId));
            }

            public Task<PagedResult<IncomeExpenseAccount>> GetIncomeExpenseAccounts(int incomeExpenseAccountType, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).GetIncomeExpenseAccount((IncomeExpenseAccountType)incomeExpenseAccountType, queryResultSettings));
            }

            public Task<PagedResult<StateProvinceInfo>> GetStateProvinces(string countryRegionId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetStateProvinces(countryRegionId, queryResultSettings));
            }

            public Task<PagedResult<TenderType>> GetTenderTypes(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelTenderTypes(queryResultSettings));
            }

            public Task<PagedResult<ZipCodeInfo>> GetZipCodes(string countryRegionId, string stateProvinceId, string countyId, string cityName, string district, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetZipCodes(countryRegionId, stateProvinceId, countyId, cityName, district, queryResultSettings));
            }

            public Task<KitTransaction> DisassembleKitTransactions(KitTransaction kitTransaction)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).SaveKitTransaction(kitTransaction));
            }

            public Task<LoyaltyCard> IssueLoyaltyCard(LoyaltyCard loyaltyCard)
            {
                return Task.Run(() => LoyaltyManager.Create(CommerceRuntimeManager.Runtime).IssueLoyaltyCard(loyaltyCard.CardNumber, loyaltyCard.CustomerAccount, loyaltyCard.CardTenderType));
            }

            public Task<LoyaltyCard> GetLoyaltyCard(string cardNumber)
            {
                return Task.Run(() => LoyaltyManager.Create(CommerceRuntimeManager.Runtime).GetLoyaltyCardStatus(cardNumber));
            }

            public Task<PagedResult<LoyaltyCard>> GetCustomerLoyaltyCards(string accountNumber, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => LoyaltyManager.Create(CommerceRuntimeManager.Runtime).GetCustomerLoyaltyCards(accountNumber, queryResultSettings));
            }

            public Task<PagedResult<Transaction>> SearchJournalTransactions(TransactionSearchCriteria searchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).SearchJournalTransactions(searchCriteria, queryResultSettings));
            }

            public Task<GiftCard> GetGiftCard(string giftCardId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetGiftCard(giftCardId));
            }

            public Task<PagedResult<SupportedLanguage>> GetLanguages(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetLanguages(queryResultSettings));
            }

            public Task<PagedResult<NonSalesTransaction>> GetNonSalesTransactions(string shiftId, string shiftTerminalId, int nonSalesTenderTypeValue, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).GetNonSaleTenderOperations((TransactionType)nonSalesTenderTypeValue, shiftId, shiftTerminalId, queryResultSettings));
            }

            public Task<NonSalesTransaction> CreateNonSalesTransaction(NonSalesTransaction nonSalesTransaction)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).SaveNonSaleTenderOperation(nonSalesTransaction));
            }

            public Task<DropAndDeclareTransaction> CreateDropAndDeclareTransaction(DropAndDeclareTransaction dropAndDeclareTransaction)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).SaveTenderDropAndDeclareOperation(dropAndDeclareTransaction));
            }

            public Task<PagedResult<TaxOverride>> GetTaxOverrides(string overrideBy, QueryResultSettings queryResultSettings)
            {
                TaxOverrideBy taxOverrideBy;

                if (!Enum.TryParse(overrideBy, out taxOverrideBy))
                {
                    throw new ArgumentException(@"Invalid value specified for overrideBy.", "overrideBy");
                }

                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelTaxOverrides(taxOverrideBy, queryResultSettings));
            }

            public Task<PagedResult<UnitOfMeasure>> GetUnitsOfMeasure(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetUnitsOfMeasure(queryResultSettings));
            }

            public Task<CustomerBalances> GetCustomerBalance(string accountNumber, string invoiceAccountNumber)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).GetBalance(accountNumber, invoiceAccountNumber));
            }

            public Task<PagedResult<LoyaltyCardTransaction>> GetLoyaltyCardTransactions(string cardNumber, string rewardPointId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.LoyaltyManager.Create(CommerceRuntimeManager.Runtime).GetLoyaltyCardTransactions(cardNumber, rewardPointId, queryResultSettings));
            }

            public Task SaveOfflineTransactions(byte[] compressedOfflineTransactions)
            {
                return Task.Run(() => Runtime.Client.OfflineTransactionManager.Create(CommerceRuntimeManager.Runtime).SaveOfflineTransactions(compressedOfflineTransactions));
            }

            public Task<PagedResult<string>> GetOfflineTransactionIds(int numberOfTransactions, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.OfflineTransactionManager.Create(CommerceRuntimeManager.Runtime).GetOfflineTransactionIds(numberOfTransactions, queryResultSettings));
            }

            public Task<byte[]> GetOfflineTransactions(IEnumerable<string> transactionIds)
            {
                return Task.Run(() => Runtime.Client.OfflineTransactionManager.Create(CommerceRuntimeManager.Runtime).GetOfflineTransactions(transactionIds));
            }

            public Task PurgeOfflineTransactions(IEnumerable<string> transactionIds)
            {
                return Task.Run(() => Runtime.Client.OfflineTransactionManager.Create(CommerceRuntimeManager.Runtime).PurgeOfflineTransactions(transactionIds));
            }

            public Task<PagedResult<OfflineSyncStatsLine>> GetOfflineSyncStatus(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).GetOfflineSyncStats(queryResultSettings).ToCRTPagedResult());
            }

            public Task<long> GetOfflinePendingTransactionCount()
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).GetOfflinePendingTransactionCount());
            }

            public Task<PagedResult<ZipCodeInfo>> GetAddressFromZipCode(string countryRegionId, string zipPostalCode, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetAddressFromZipCode(countryRegionId, zipPostalCode, queryResultSettings));
            }

            public Task<string> GetDownloadInterval(string dataStoreName)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).GetDownloadInterval(dataStoreName));
            }

            public Task<string> GetTerminalDataStoreName(string terminalId)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).GetTerminalDataStoreName(terminalId));
            }

            public Task<string> GetDownloadLink(string dataStoreName, long downloadSessionId)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).GetDownloadLink(dataStoreName, downloadSessionId));
            }

            public Task<PagedResult<DownloadSession>> GetDownloadSessions(string dataStoreName, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).GetDownloadSessions(dataStoreName, queryResultSettings));
            }

            public Task<PagedResult<string>> GetUploadJobDefinitions(string dataStoreName, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).GetUploadJobDefinitions(dataStoreName).ToCRTPagedResult());
            }

            public Task<string> GetUploadInterval(string dataStoreName)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).GetUploadInterval(dataStoreName));
            }

            public Task<bool> PostOfflineTransactions(IEnumerable<string> offlineTransactionForMPOS)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).SyncOfflineTransactions((ICollection<string>)offlineTransactionForMPOS, 0));
            }

            public Task<bool> UpdateDownloadSession(DownloadSession downloadSession)
            {
                return Task.Run(() => Runtime.Client.AsyncServiceManager.Create(CommerceRuntimeManager.Runtime).UpdateDownloadSessionStatus((DownloadSession)downloadSession));
            }

            public Task<PagedResult<string>> GetSupportedPaymentCardTypes(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetSupportedPaymentCardTypes());
            }

            public Task UpdateApplicationVersion(string appVersion)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).UpdateApplicationVersion(appVersion));
            }

            public Task StartSession(string transactionId)
            {
                return Task.Run(() => Runtime.Client.SecurityManager.Create(CommerceRuntimeManager.Runtime).StartSession(transactionId));
            }

            public Task EndSession(string transactionId)
            {
                return Task.Run(() => Runtime.Client.SecurityManager.Create(CommerceRuntimeManager.Runtime).EndSession(transactionId));
            }

            public Task<PagedResult<Device>> GetAvailableDevices(int deviceType, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.SecurityManager.Create(CommerceRuntimeManager.Runtime).GetAvailableDevices(deviceType, queryResultSettings));
            }

            public Task<LinkToExistingCustomerResult> InitiateLinkToExistingCustomer(string email, string activationToken, string emailTemplateId, IEnumerable<NameValuePair> emailProperties)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).InitiateLinkToExistingCustomer(email, activationToken, emailTemplateId, emailProperties));
            }

            public Task<LinkToExistingCustomerResult> FinalizeLinkToExistingCustomer(string email, string activationToken)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).FinalizeLinkToExistingCustomer(email, activationToken));
            }

            public Task UnlinkFromExistingCustomer()
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).UnlinkFromExistingCustomer());
            }

            public Task<StorageAccessToken> GetStorageAccessTokenForUpload()
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).GetStorageAccessTokenForUpload());
            }

            public Task<PagedResult<Framework>> GetBusinessProcessModelLibraries(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).GetBusinessProcessModelLibraries(queryResultSettings));
            }

            public Task<Framework> GetBusinessProcessModelLibrary(int businessProcessModelFrameworkId, int hierarchyDepth)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).GetBusinessProcessModelLibrary(businessProcessModelFrameworkId, hierarchyDepth));
            }

            public Task<TaskGuidesSearchResult> SearchTaskGuidesByTitle(int businessProcessModelFrameworkId, string taskGuideSearchKeyword, int queryTypeValue)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).SearchTaskGuidesByTitle(businessProcessModelFrameworkId, taskGuideSearchKeyword, queryTypeValue));
            }

            public Task<string> GenerateBusinessProcessModelPackage(Recording taskRecording)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).GenerateBusinessProcessModelPackage(taskRecording));
            }

            public Task<string> GenerateRecordingBundle(Recording taskRecording)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).GenerateRecordingBundle(taskRecording));
            }

            public Task<Recording> DownloadRecording(int businessProcessModelLineId)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).DownloadRecording(businessProcessModelLineId));
            }

            public Task<Recording> LoadRecordingFromFile(string recordingUrl)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).LoadRecordingFromFile(recordingUrl));
            }

            public Task<string> GenerateRecordingFile(Recording taskRecording)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).GenerateRecordingFile(taskRecording));
            }

            public Task<string> GenerateTrainingDocument(Recording taskRecording)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).GenerateTrainingDocument(taskRecording));
            }

            public Task UploadRecording(Recording taskRecording, int businessProcessModelLineId)
            {
                return Task.Run(() => Runtime.Client.TaskRecorderManager.Create(CommerceRuntimeManager.Runtime).UploadRecording(taskRecording, businessProcessModelLineId));
            }
        }
    }
}