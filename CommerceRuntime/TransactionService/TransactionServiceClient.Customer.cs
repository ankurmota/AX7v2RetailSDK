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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.IO;
        using System.Linq;
        using System.Runtime.Serialization;
        using System.Xml;
        using System.Xml.Linq;
        using System.Xml.Serialization;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;

        /// <summary>
        /// Transaction Service Commerce Runtime Client APIs for customer.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string DeactivateAddressMethodName = "DeactivateAddress";
            private const string GetCustomerDataPackageMethodName = "getCustomerDataPackage";
            private const string GetPartyDataPackageMethodName = "getPartyDataPackage";
            private const string NewCustomerMethodName = "NewCustomer";
            private const string NewCustomerExtMethodName = "NewCustomerExt";
            private const string NewCustomerFromDirPartyMethodName = "NewCustomerFromDirParty";
            private const string UpdateCustomerMethodName = "UpdateCustomer";
            private const string UpdateCustomerExtMethodName = "UpdateCustomerExt";
            private const string SearchCustomersMethodName = "searchCustomers";
            private const string SendEmailMethodName = "SendEmail";
            private const string UpdateAddressMethodName = "UpdateAddress";
            private const string UpdateAddressExtMethodName = "UpdateAddressExt";
            private const string CreateAddressMethodName = "CreateAddress";
            private const string CreateAddressExtMethodName = "CreateAddressExt";
            private const string GetCustomerBalanceMethodName = "GetCustomerBalance";
            private const string GetOrderHistoryListMethodName = "GetOrderHistoryList";
            private const string GetPurchaseHistoryMethodName = "GetPurchaseHistory";

            // Create new customer constants.
            private const int CreateNewCustomerAccountNumberIndex = 0;
            private const int CreateNewCustomerSalesTaxGroupIndex = 1;
            private const int CreateNewCustomerPartyIdIndex = 2;
            private const int CreateNewCustomerRecordIdIndex = 3;
            private const int CreateNewCustomerRetailCustTableRecordIdIndex = 4;
            private const int CreateNewCustomerDirPartyTablePartyNumberIndex = 50;
            private const int CreateNewCustomerDirPersonRecordIdIndex = 58;
            private const int CreateNewCustomerDirPartyNameIndex = 59;
            private const int CreateNewCustomerAffiliationsIndex = 60;

            private const int CreateNewCustomerPostalAddressRecordIdIndex = 5;            // RecId of LogisticsPostalAddress
            private const int CreateNewCustomerPostalLocationRecordIdIndex = 6;           // RecId of LogisticsLocation
            private const int CreateNewCustomerPostalPartyLocationRecordIdIndex = 7;      // RecId of DirPartyLocation
            private const int CreateNewCustomerPostalPartyLocationRoleRecordIdIndex = 8;  // RecId of DirPartyLocationRole
            private const int CreateNewCustomerPostalPartyLocationRoleIndex = 9;          // LocationRole column of DirPartyLocationRole (used by AddressType)
            private const int CreateNewCustomerPostalLogisticLocationIdIndex = 57;        // LocationId column of LogisticsLocation
            private const int CreateNewCustomerAddressTaxLocationExtIdIndex = 10;

            private const int CreateNewCustomerEmailRecordIdIndex = 15;
            private const int CreateNewCustomerEmailLogisticsLocationRecordId = 16;
            private const int CreateNewCustomerEmailDirPartyLocationRecordId = 17;
            private const int CreateNewCustomerEmailDirPartyLocationRoleRecordId = 18;
            private const int CreateNewCustomerEmailLogisticsLocationRoleRecordId = 19;
            private const int CreateNewCustomerEmailLogisticsLocationId = 51;

            private const int CreateNewCustomerPhoneRecordIdIndex = 20;
            private const int CreateNewCustomerPhoneLogisticsLocationRecordId = 21;
            private const int CreateNewCustomerPhoneDirPartyLocationRecordId = 22;
            private const int CreateNewCustomerPhoneDirPartyLocationRoleRecordId = 23;
            private const int CreateNewCustomerPhoneLogisticsLocationRoleRecordId = 24;
            private const int CreateNewCustomerPhoneLogisticsLocationId = 53;

            private const int CreateNewCustomerUrlRecordIdIndex = 25;
            private const int CreateNewCustomerUrlLogisticsLocationRecordId = 26;
            private const int CreateNewCustomerUrlDirPartyLocationRecordId = 27;
            private const int CreateNewCustomerUrlDirPartyLocationRoleRecordId = 28;
            private const int CreateNewCustomerUrlLogisticsLocationRoleRecordId = 29;
            private const int CreateNewCustomerUrlLogisticsLocationId = 52;

            private const int CreateNewCustomerMobileRecordIdIndex = 30;
            private const int CreateNewCustomerMobileLogisticsLocationRecordId = 31;
            private const int CreateNewCustomerMobileDirPartyLocationRecordId = 32;
            private const int CreateNewCustomerMobileDirPartyLocationRoleRecordId = 33;
            private const int CreateNewCustomerMobileLogisticsLocationRoleRecordId = 34;
            private const int CreateNewCustomerMobileLogisticsLocationId = 54;

            private const int CreateNewCustomerAddressPhoneRecordIdIndex = 35;
            private const int CreateNewCustomerAddressPhoneLogisticsLocationRecordId = 36;
            private const int CreateNewCustomerAddressPhoneLogisticsLocationId = 37;

            private const int CreateNewCustomerAddressEmailRecordIdIndex = 40;
            private const int CreateNewCustomerAddressEmailLogisticsLocationRecordId = 41;
            private const int CreateNewCustomerAddressEmailLogisticsLocationId = 42;

            private const int CreateNewCustomerAddressUrlRecordIdIndex = 45;
            private const int CreateNewCustomerAddressUrlLogisticsLocationRecordId = 46;
            private const int CreateNewCustomerAddressUrlLogisticsLocationId = 47;
            private const int CreateNewCustomerFullAddressIndex = 61;
            private const int CreateNewCustomerAddressBookDataIndex = 62;
            private const int CreateNewCustomerIsCustomerTaxInclusiveIndex = 63;

            // Update customers constants.
            private const int UpdateCustomerEmailLogisticsLocationId = 35;
            private const int UpdateCustomerPhoneLogisticsLocationId = 36;
            private const int UpdateCustomerUrlLogisticsLocationId = 37;
            private const int UpdateCustomerMobileLogisticsLocationId = 38;
            private const int UpdateCustomerDirPersonRecordIdIndex = 39;
            private const int UpdateCustomerDirPersonNameIndex = 40;
            private const int UpdateCustomerAffiliationsIndex = 41;

            // The first 2 numbers (0,1) are as in the new customer, then the offset is the below number.
            private const int CountOfAdditionalFields = 40;
            private const int CustomerDetailsContainerIndex = 64;

            // Create/Update address constants.
            private const int CreateUdpateAddressDirPartyTableRecordId = 0;
            private const int CreateUpdateAddressRecordIdIndex = 3;
            private const int CreateUpdateAddressLogisticsLocationId = 33;
            private const int CreateUpdateAddressLogisticsLocationRecordId = 4;
            private const int CreateUpdateAddressDirPartyLocationRecordId = 5;
            private const int CreateUpdateAddressDirPartyLocationRoleRecordId = 6;
            private const int CreateUpdateAddressLogisticsLocationRoleRecordId = 7;
            private const int CreateUdpateAddressTaxLocationExtId = 8;
            private const int CreateUpdateAddressEmailRecordIdIndex = 13;
            private const int CreateUpdateAddressEmailLogisticsLocationRecordIdIndex = 14;
            private const int CreateUpdateAddressEmailLogisticsLocationIdIndex = 15;
            private const int CreateUpdateAddressPhoneRecordIdIndex = 18;
            private const int CreateUpdateAddressPhoneLogisticsLocationRecordIdIndex = 19;
            private const int CreateUpdateAddressPhoneLogisticsLocationIdIndex = 20;
            private const int CreateUpdateAddressUrlRecordIdIndex = 23;
            private const int CreateUpdateAddressUrlLogisticsLocationRecordIdIndex = 24;
            private const int CreateUpdateAddressUrlLogisticsLocationIdIndex = 25;
            private const int CreateUpdateAddressFullAddressIndex = 34;

            // Get customer balance constants.
            private const int CustomerBalanceIndex = 0;
            private const int CreditLimitIndex = 1;
            private const int InvoiceAccountBalanceIndex = 2;
            private const int InvoiceAccountCreditLimitIndex = 3;
            private const int TransactionMaxReplicationCounterIndex = 4;

            /// <summary>
            /// Deactivate an Address in AX.
            /// </summary>
            /// <param name="addressId">The address identifier.</param>
            /// <param name="customerId">The customer identifier.</param>
            public void DeactivateAddress(long addressId, long customerId)
            {
                this.InvokeMethodNoDataReturn(
                    DeactivateAddressMethodName,
                    new object[] { addressId, customerId });
            }

            /// <summary>
            /// Perform a keyword search in AX for customers.
            /// </summary>
            /// <param name="keywords">The keywords to search using.</param>
            /// <param name="paging">How the search results should be paged.</param>
            /// <returns>A list of GlobalCustomer objects matching the keywords provided.</returns>
            public PagedResult<GlobalCustomer> SearchCustomers(string keywords, PagingInfo paging)
            {
                ThrowIf.NullOrWhiteSpace(keywords, "Keywords");
                ThrowIf.Null(paging, "paging");

                if (paging.Skip < 0)
                {
                    throw new ArgumentOutOfRangeException("paging", paging.Skip, "paging.Skip must be >= 0");
                }

                if (paging.Top <= 0)
                {
                    throw new ArgumentOutOfRangeException("paging", paging.Top, "paging.Top must be > 0");
                }

                ReadOnlyCollection<object> response = this.InvokeMethod(
                                                            SearchCustomersMethodName,
                                                            keywords,
                                                            paging.Skip,
                                                            paging.NumberOfRecordsToFetch);

                string globalCustomersXml = (string)response[0].ToString();
                GlobalCustomer[] globalCustomers = SerializationHelper.DeserializeObjectDataContractFromXml<GlobalCustomer[]>(globalCustomersXml);

                return new PagedResult<GlobalCustomer>(new ReadOnlyCollection<GlobalCustomer>(globalCustomers), paging);
            }

            /// <summary>
            /// Find and retrieve a customer from AX.
            /// </summary>
            /// <param name="recordId">Optional: The RecId of the customer to get.</param>
            /// <param name="accountNumber">Optional: The account number of the customer to get.</param>
            /// <param name="directoryPartyRecId">Optional: The directoryParty RecId of the customer to get.</param>
            /// <param name="storeId">The store id.</param>
            /// <returns>
            /// A base64 encoded CDX data package containing the customer's information.
            /// </returns>
            public string GetCustomerDataPackage(long recordId, string accountNumber, long directoryPartyRecId, long storeId)
            {
                ReadOnlyCollection<object> data = this.InvokeMethod(
                                                        GetCustomerDataPackageMethodName,
                                                        recordId,
                                                        accountNumber,
                                                        directoryPartyRecId,
                                                        storeId);

                return (string)data[0];
            }

            /// <summary>
            /// Find and retrieve a party from AX.
            /// </summary>
            /// <param name="partyNumber">The desired party's party number.</param>
            /// <param name="storeId">The store id.</param>
            /// <returns>
            /// A base64 encoded CDX data package containing the customer's information.
            /// </returns>
            public string GetPartyDataPackage(string partyNumber, long storeId)
            {
                ReadOnlyCollection<object> data = this.InvokeMethod(
                                                        GetPartyDataPackageMethodName,
                                                        partyNumber,
                                                        storeId);

                return (string)data[0];
            }

            /// <summary>
            /// Create a new customer in AX.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <param name="storeId">The store id.</param>
            public void NewCustomer(ref Customer customer, long storeId)
            {
                ThrowIf.Null(customer, "customer");

                // transform the customer to parameters
                object[] parameters = GetNewCustomerTransactionServiceParameters(customer, storeId);

                ReadOnlyCollection<object> data = this.TryNewMethodOrFallback(NewCustomerExtMethodName, CreateExtensionPropertiesParameter(customer.ExtensionProperties), NewCustomerMethodName, parameters);

                // Parse response data
                customer = ParseNewCustomerResposeCustomerData(customer, data);

                // when a new customer is created; if it has an address then it is the primary one.
                Address address = (Address)customer.GetPrimaryAddress();

                if (address != null)
                {
                    address = ParseNewCustomerResponseAddressData(customer, data, address);
                }
            }

            /// <summary>
            /// Create a new customer in AX.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <param name="storeId">The store id.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Follows naming in AX")]
            public void NewCustomerFromDirParty(ref Customer customer, long storeId)
            {
                ThrowIf.Null(customer, "customer");
                var data = this.InvokeMethod(
                    NewCustomerFromDirPartyMethodName,
                    customer.PartyNumber,
                    storeId);

                object[] customerDetailsData = (object[])data[CustomerDetailsContainerIndex];

                customer.Name = (string)customerDetailsData[0];
                customer.FirstName = (string)customerDetailsData[2];
                customer.MiddleName = (string)customerDetailsData[3];
                customer.LastName = (string)customerDetailsData[4];
                customer.CustomerGroup = (string)customerDetailsData[5];
                customer.CurrencyCode = (string)customerDetailsData[6];
                customer.Language = (string)customerDetailsData[7];
                customer.Phone = (string)customerDetailsData[12];
                customer.Cellphone = (string)customerDetailsData[13];
                customer.Email = (string)customerDetailsData[14];
                customer.Url = (string)customerDetailsData[15];
                customer.MultilineDiscountGroup = (string)customerDetailsData[16];
                customer.TotalDiscountGroup = (string)customerDetailsData[17];
                customer.LineDiscountGroup = (string)customerDetailsData[18];
                customer.PriceGroup = (string)customerDetailsData[19];
                customer.TaxGroup = (string)customerDetailsData[20];
                customer.OrganizationId = (string)customerDetailsData[21];
                customer.VatNumber = (string)customerDetailsData[22];
                customer.IdentificationNumber = (string)customerDetailsData[24];
                customer.Blocked = false;
                customer.ReceiptSettings = (int)customerDetailsData[33];
                customer.CustomerType = (CustomerType)int.Parse(customerDetailsData[34].ToString());
                customer.UsePurchaseRequest = (bool)customerDetailsData[36];
                customer.MandatoryCreditLimit = (bool)customerDetailsData[37];
                customer.CreditLimit = (decimal)customerDetailsData[38];

                customer = ParseNewCustomerResposeCustomerData(customer, data);

                // update the default address record
                Address address = new Address()
                {
                    IsPrimary = true,
                    Street = (string)customerDetailsData[8],
                    ZipCode = (string)customerDetailsData[9],
                    State = (string)customerDetailsData[10],
                    County = (string)customerDetailsData[11],
                    ThreeLetterISORegionName = (string)customerDetailsData[39],
                    Name = (string)customerDetailsData[28],
                    Phone = (string)customerDetailsData[29],
                    Email = (string)customerDetailsData[30],
                    Url = (string)customerDetailsData[31],
                    TaxGroup = (string)customerDetailsData[32],
                    AddressType = (AddressType)customerDetailsData[35],
                };

                address.FullAddress = string.Format(
                        "{0} {1} {2} {3} {4}",
                        address.Street,
                        address.City,
                        address.State,
                        address.ZipCode,
                        address.ThreeLetterISORegionName);

                // Parse response data
                address = ParseNewCustomerResponseAddressData(customer, data, address);

                if (address.RecordId > 0)
                {
                    customer.Addresses = new Address[] { address };
                }
            }

            /// <summary>
            /// Update Customer in AX.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <returns>An updated customer.</returns>
            public Customer UpdateCustomer(Customer customer)
            {
                ThrowIf.Null(customer, "customerData");

                object[] parameters = GetUpdateCustomerTransactionServiceParameters(customer);

                if (customer.CustomerType == CustomerType.Person)
                {
                    customer.Name = StringDataHelper.JoinStrings(" ", customer.FirstName, customer.MiddleName, customer.LastName);
                }

                ReadOnlyCollection<object> data = this.TryNewMethodOrFallback(
                    UpdateCustomerExtMethodName,
                    CreateExtensionPropertiesParameter(customer.ExtensionProperties),
                    UpdateCustomerMethodName,
                    parameters);

                // Parse response data
                customer.AccountNumber = (string)data[CreateNewCustomerAccountNumberIndex];
                customer.RecordId = long.Parse(data[CreateNewCustomerRecordIdIndex].ToString());
                customer.TaxGroup = (string)data[CreateNewCustomerSalesTaxGroupIndex];
                customer.DirectoryPartyRecordId = long.Parse(data[CreateNewCustomerPartyIdIndex].ToString());
                customer.RetailCustomerTableRecordId = long.Parse(data[CreateNewCustomerRetailCustTableRecordIdIndex].ToString());
                customer.PersonNameId = long.Parse(data[UpdateCustomerDirPersonRecordIdIndex].ToString());
                customer.Name = (string)data[UpdateCustomerDirPersonNameIndex];

                customer.PhoneRecordId = long.Parse(data[CreateNewCustomerPhoneRecordIdIndex].ToString());
                customer.PhoneLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerPhoneLogisticsLocationRecordId].ToString());
                customer.PhoneLogisticsLocationId = (string)data[UpdateCustomerPhoneLogisticsLocationId];

                customer.EmailRecordId = long.Parse(data[CreateNewCustomerEmailRecordIdIndex].ToString());
                customer.EmailLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerEmailLogisticsLocationRecordId].ToString());
                customer.EmailLogisticsLocationId = (string)data[UpdateCustomerEmailLogisticsLocationId];

                customer.UrlRecordId = long.Parse(data[CreateNewCustomerUrlRecordIdIndex].ToString());
                customer.UrlLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerUrlLogisticsLocationRecordId].ToString());
                customer.UrlLogisticsLocationId = (string)data[UpdateCustomerUrlLogisticsLocationId];

                customer.CellphoneRecordId = long.Parse(data[CreateNewCustomerMobileRecordIdIndex].ToString());
                customer.CellphoneLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerMobileLogisticsLocationRecordId].ToString());
                customer.CellphoneLogisticsLocationId = (string)data[UpdateCustomerMobileLogisticsLocationId];
                customer.CustomerAffiliations = ConvertXmlStringToCustomerAffiliations(data[UpdateCustomerAffiliationsIndex].ToString());

                return customer;
            }

            /// <summary>
            /// Sends an email to a customer using the provided emailId in AX.
            /// </summary>
            /// <param name="emailAddress">The email address of the recipient.</param>
            /// <param name="language">The language of the email to send.</param>
            /// <param name="emailId">The email Id of the template to use as defined in AX.</param>
            /// <param name="mappings">Optional. The collection of name/value pairs to replace in the message and subject of the email template.</param>
            /// <param name="xmlData">Optional. The XML the will be used in the transformation of the email if the template is so configured. If this object is a string no serialization is performed.
            /// If it is not a string serialization is performed prior to sending to AX.</param>
            public void SendEmail(string emailAddress, string language, string emailId, ICollection<NameValuePair> mappings, object xmlData)
            {
                ThrowIf.Null(emailAddress, "emailAddress");
                ThrowIf.Null(language, "language");
                ThrowIf.Null(emailId, "emailId");

                object[] parameters = GetSendEmailTransactionServiceParameters(emailId, language, emailAddress, mappings, xmlData, false, true);

                this.InvokeMethodNoDataReturn(
                    SendEmailMethodName,
                    parameters);
            }

            /// <summary>
            /// Update a postal address in AX.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <param name="address">The address data.</param>
            public void UpdateAddress(ref Customer customer, ref Address address)
            {
                ThrowIf.Null(customer, "customer");
                ThrowIf.Null(address, "address");

                object[] parameters = GetUpdateAddressTransactionServiceParameters(customer, address);
                long originalRecordId = address.RecordId;

                ReadOnlyCollection<object> data = this.TryNewMethodOrFallback(UpdateAddressExtMethodName, CreateExtensionPropertiesParameter(address.ExtensionProperties), UpdateAddressMethodName, parameters);

                // Parse response data
                address.RecordId = long.Parse(data[CreateUpdateAddressRecordIdIndex].ToString());

                // if the address was update and new record was created then expire the old one in CRT DB.
                if (originalRecordId != address.RecordId)
                {
                    address.ExpireRecordId = originalRecordId;
                }
                else
                {
                    address.ExpireRecordId = address.RecordId;
                }

                address.LogisticsLocationId = (string)data[CreateUpdateAddressLogisticsLocationId];
                address.LogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressLogisticsLocationRecordId].ToString());
                address.LogisticsLocationRoleRecordId = long.Parse(data[CreateUpdateAddressLogisticsLocationRoleRecordId].ToString());
                address.LogisticsLocationExtRecordId = long.Parse(data[CreateUdpateAddressTaxLocationExtId].ToString());

                address.DirectoryPartyTableRecordId = long.Parse(data[CreateUdpateAddressDirPartyTableRecordId].ToString());
                address.DirectoryPartyLocationRecordId = long.Parse(data[CreateUpdateAddressDirPartyLocationRecordId].ToString());
                address.DirectoryPartyLocationRoleRecordId = long.Parse(data[CreateUpdateAddressDirPartyLocationRoleRecordId].ToString());

                address.PhoneRecordId = long.Parse(data[CreateUpdateAddressPhoneRecordIdIndex].ToString());
                address.PhoneLogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressPhoneLogisticsLocationRecordIdIndex].ToString());
                address.PhoneLogisticsLocationId = (string)data[CreateUpdateAddressPhoneLogisticsLocationIdIndex];

                address.EmailRecordId = long.Parse(data[CreateUpdateAddressEmailRecordIdIndex].ToString());
                address.EmailLogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressEmailLogisticsLocationRecordIdIndex].ToString());
                address.EmailLogisticsLocationId = (string)data[CreateUpdateAddressEmailLogisticsLocationIdIndex];

                address.UrlRecordId = long.Parse(data[CreateUpdateAddressUrlRecordIdIndex].ToString());
                address.UrlLogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressUrlLogisticsLocationRecordIdIndex].ToString());
                address.UrlLogisticsLocationId = (string)data[CreateUpdateAddressUrlLogisticsLocationIdIndex];

                address.FullAddress = (string)data[CreateUpdateAddressFullAddressIndex];
            }

            /// <summary>
            /// Create a new postal address in AX.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <param name="address">The address data.</param>
            public void CreateAddress(Customer customer, ref Address address)
            {
                ThrowIf.Null(customer, "customer");
                ThrowIf.Null(address, "address");

                object[] parameters = GetCreateAddressTransactionServiceParameters(customer, address);

                ReadOnlyCollection<object> data = this.TryNewMethodOrFallback(CreateAddressExtMethodName, CreateExtensionPropertiesParameter(address.ExtensionProperties), CreateAddressMethodName, parameters);

                // Parse response data
                address.RecordId = long.Parse(data[CreateUpdateAddressRecordIdIndex].ToString());
                address.ExpireRecordId = address.RecordId;

                address.LogisticsLocationId = (string)data[CreateUpdateAddressLogisticsLocationId];
                address.LogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressLogisticsLocationRecordId].ToString());
                address.LogisticsLocationRoleRecordId = long.Parse(data[CreateUpdateAddressLogisticsLocationRoleRecordId].ToString());
                address.LogisticsLocationExtRecordId = long.Parse(data[CreateUdpateAddressTaxLocationExtId].ToString());

                address.DirectoryPartyTableRecordId = long.Parse(data[CreateUdpateAddressDirPartyTableRecordId].ToString());
                address.DirectoryPartyLocationRecordId = long.Parse(data[CreateUpdateAddressDirPartyLocationRecordId].ToString());
                address.DirectoryPartyLocationRoleRecordId = long.Parse(data[CreateUpdateAddressDirPartyLocationRoleRecordId].ToString());

                address.PhoneRecordId = long.Parse(data[CreateUpdateAddressPhoneRecordIdIndex].ToString());
                address.PhoneLogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressPhoneLogisticsLocationRecordIdIndex].ToString());
                address.PhoneLogisticsLocationId = (string)data[CreateUpdateAddressPhoneLogisticsLocationIdIndex];

                address.EmailRecordId = long.Parse(data[CreateUpdateAddressEmailRecordIdIndex].ToString());
                address.EmailLogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressEmailLogisticsLocationRecordIdIndex].ToString());
                address.EmailLogisticsLocationId = (string)data[CreateUpdateAddressEmailLogisticsLocationIdIndex];

                address.UrlRecordId = long.Parse(data[CreateUpdateAddressUrlRecordIdIndex].ToString());
                address.UrlLogisticsLocationRecordId = long.Parse(data[CreateUpdateAddressUrlLogisticsLocationRecordIdIndex].ToString());
                address.UrlLogisticsLocationId = (string)data[CreateUpdateAddressUrlLogisticsLocationIdIndex];

                address.FullAddress = (string)data[CreateUpdateAddressFullAddressIndex];
            }

            /// <summary>
            /// Gets the customer balance.
            /// </summary>
            /// <param name="accountNumber">The account number.</param>
            /// <param name="currencyCode">The currency code.</param>
            /// <param name="storeId">The store identifier.</param>
            /// <returns>A customer balance object.</returns>
            public CustomerBalances GetCustomerBalance(string accountNumber, string currencyCode, string storeId)
            {
                ThrowIf.NullOrWhiteSpace(accountNumber, "accountNumber");
                ThrowIf.NullOrWhiteSpace(currencyCode, "currencyCode");
                ThrowIf.NullOrWhiteSpace(storeId, "storeId");

                CustomerBalances customerBalance = new CustomerBalances();

                object[] parameters = new object[]
                {
                accountNumber,
                currencyCode,
                storeId,
                };

                var data = this.InvokeMethod(
                    GetCustomerBalanceMethodName,
                    parameters);

                customerBalance.Balance = Convert.ToDecimal(data[CustomerBalanceIndex]);
                customerBalance.CreditLimit = Convert.ToDecimal(data[CreditLimitIndex]);
                customerBalance.InvoiceAccountBalance = Convert.ToDecimal(data[InvoiceAccountBalanceIndex]);
                customerBalance.InvoiceAccountCreditLimit = Convert.ToDecimal(data[InvoiceAccountCreditLimitIndex]);
                customerBalance.PendingTransactionsAnchor = Convert.ToInt64(data[TransactionMaxReplicationCounterIndex]);

                return customerBalance;
            }

            /// <summary>
            /// Gets the customer purchase history from AX.
            /// </summary>
            /// <param name="accountNumber">The account number of the customer.</param>
            /// <param name="languageId">The language to localize the purchases data.</param>
            /// <param name="startDateTime">The starting date time to fetch data.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>A collection of <see cref="PurchaseHistory"/>.</returns>
            public PagedResult<PurchaseHistory> GetPurchaseHistory(string accountNumber, string languageId, DateTimeOffset startDateTime, QueryResultSettings settings)
            {
                ThrowIf.NullOrWhiteSpace(accountNumber, "accountNumber");
                ThrowIf.Null(settings, "settings");

                var searchCriteria = new AxPurchaseHistorySearchCriteria(accountNumber, languageId, startDateTime.UtcDateTime.ToString(), settings.Paging);
                string parameters = SerializationHelper.SerializeObjectToXml(searchCriteria);
                var data = this.InvokeMethod(GetPurchaseHistoryMethodName, parameters);

                // Parse transactions from results, the last value is the items for all orders.
                string purchaseHistoryXML = (string)data[0];
                PurchaseHistory[] purchaseHistory = SerializationHelper.DeserializeObjectDataContractFromXml<PurchaseHistory[]>(purchaseHistoryXML);
                return new PagedResult<PurchaseHistory>(new ReadOnlyCollection<PurchaseHistory>(purchaseHistory));
            }

            /// <summary>
            ///  Gets order history for a given customer in AX.
            /// </summary>
            /// <param name="accountNumber">The account number..</param>
            /// <param name="startDateTime">The starting date time to fetch data.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>A collection of sales order.</returns>
            public PagedResult<SalesOrder> GetOrderHistory(string accountNumber, DateTimeOffset startDateTime, QueryResultSettings settings)
            {
                ThrowIf.Null(settings, "settings");

                var headQuarterCritera = new AxOrderHistorySearchCriteria();
                headQuarterCritera.CustomerAccountNumber = accountNumber;
                headQuarterCritera.IncludeDetails = true;
                headQuarterCritera.PagingInfo = settings.Paging;
                headQuarterCritera.StartDateTime = startDateTime.UtcDateTime.ToString();

                try
                {
                    if (!MethodsNotFoundInAx.Value.ContainsKey(GetOrderHistoryListMethodName))
                    {
                        ReadOnlyCollection<object> transactionData = this.InvokeMethod(
                            GetOrderHistoryListMethodName,
                            SerializationHelper.SerializeObjectToXml(headQuarterCritera));

                        // No matching orders were found.
                        if (transactionData == null)
                        {
                            return Enumerable.Empty<SalesOrder>().AsPagedResult();
                        }

                        if (transactionData.Count != 1)
                        {
                            throw new InvalidOperationException(
                                "TransactionServiceClient.GetOrderHistory returned an invalid result.");
                        }

                        // Parse transactions from results, the last value is the items for all orders.
                        string ordersXml = (string)transactionData[0];
                        var salesOrders = SerializationHelper.DeserializeObjectDataContractFromXml<SalesOrder[]>(ordersXml);
                        return salesOrders.OrderByDescending(x => x.CreatedDateTime).AsPagedResult();
                    }
                }
                catch (Exception)
                {
                    if (!MethodsNotFoundInAx.Value.ContainsKey(GetOrderHistoryListMethodName))
                    {
                        throw;
                    }

                    // Need to trigger fallback logic.
                }

                // Fallback to existing SearchOrders() call to meet SE deployment requirement.
                var criteria = new SalesOrderSearchCriteria();
                criteria.CustomerAccountNumber = accountNumber;
                criteria.IncludeDetails = true;
                criteria.SalesTransactionTypes = new[]
                {
                SalesTransactionType.CustomerOrder,
                SalesTransactionType.Sales,
                SalesTransactionType.PendingSalesOrder
            };
                criteria.SearchType = OrderSearchType.SalesOrder;

                var maxNumberOfResults = settings.Paging.Top + settings.Paging.Skip;
                return this.SearchOrders(criteria, maxNumberOfResults);
            }

            private static object[] GetNewCustomerTransactionServiceParameters(Customer customer, long storeId)
            {
                if ((customer.CustomerType == CustomerType.Person) && string.IsNullOrWhiteSpace(customer.Name))
                {
                    customer.Name = StringDataHelper.JoinStrings(" ", customer.FirstName, customer.MiddleName, customer.LastName);
                }

                Address primaryAddress = customer.GetPrimaryAddress();

                if (primaryAddress == null)
                {
                    // do this locally so we simplify null checks for the code bellow.
                    primaryAddress = new Address();
                }

                return new object[]
                {
                customer.Name,                                  // customer name
                customer.CustomerGroup,                         // customer group
                customer.CurrencyCode,                              // currency
                customer.Language,                              // language id
                primaryAddress.Street,                          // street name
                primaryAddress.ZipCode,                         // zip code
                primaryAddress.State,                           // state name
                primaryAddress.County,                          // county name
                customer.Phone,                                 // phone
                customer.Cellphone,                             // cellular phone
                customer.Email,                                 // email
                customer.Url,                                   // url
                customer.MultilineDiscountGroup,                // CustMultiLineDiscCode
                customer.TotalDiscountGroup,                    // CustEndDiscCode
                customer.LineDiscountGroup,                     // CustLineDiscCode
                customer.PriceGroup,                            // CustPriceGroup
                customer.TaxGroup,                         // TaxGroup
                customer.CreditLimit,                           // CustCreditMaxMST
                Convert.ToInt32(customer.Blocked),              // CustBlocked
                customer.OrganizationId ?? string.Empty,        // OrgID
                customer.UsePurchaseRequest,                    // RetailUsePurchRequest
                customer.VatNumber,                             // VATNum
                customer.InvoiceAccount ?? string.Empty,        // CustInvoiceAccount
                customer.MandatoryCreditLimit,                  // MandatoryCreditLimit
                string.Empty,                                   // ContactPersonId
                customer.UseOrderNumberReference,               // RetailUseOrderNumberReference
                (int)customer.ReceiptSettings,                  // RetailReceiptOption
                string.IsNullOrWhiteSpace(customer.ReceiptEmail) ? customer.Email : customer.ReceiptEmail,        // RetailReceiptEmail
                primaryAddress.City,                            // AddressCity
                primaryAddress.ThreeLetterISORegionName,        // AddressCountryRegionId
                customer.IdentificationNumber,                  // CustIdentificationNumber
                storeId,                                        // storeRecId
                (int)customer.CustomerType,                     // RelationType
                primaryAddress.StreetNumber,                    // street number
                primaryAddress.DistrictName,                    // districtName
                primaryAddress.BuildingCompliment,              // buildingCompliment
                customer.CNPJCPFNumber,                         // cnpjCpfNum_BR
                primaryAddress.Name,                            // addressName
                (int)primaryAddress.AddressType,                // addressType
                primaryAddress.Phone,                           // addressPhone
                primaryAddress.Email,                           // addressEmail
                primaryAddress.Url,                             // addressUrl
                primaryAddress.TaxGroup,                        // addressTaxGroup
                customer.FirstName,                             // firstName
                customer.MiddleName,                            // middleName
                customer.LastName,                              // lastName
                customer.PhoneExt,                              // Phone extension
                ConvertCustomerAffiliationsToXml(customer.CustomerAffiliations) // Customer Affiliations
                };
            }

            private static object[] GetUpdateCustomerTransactionServiceParameters(Customer customer)
            {
                ThrowIf.Null<Customer>(customer, "customer");

                if (string.IsNullOrWhiteSpace(customer.Name))
                {
                    customer.Name = StringDataHelper.JoinStrings(" ", customer.FirstName, customer.MiddleName, customer.LastName);
                }

                return new object[]
                {
                customer.RecordId,                              // cust table rec id
                customer.Name,                                      // cust name
                customer.CustomerGroup,                         // cust group id
                customer.CurrencyCode,                              // currency
                customer.Language,                              // language id
                customer.Phone,        // phone
                customer.PhoneRecordId,                         // phone recid
                customer.Cellphone,                                   // cellular phone
                customer.Email,                                 // email
                customer.EmailRecordId,                         // email recid
                customer.Url,                                   // url
                customer.UrlRecordId,                           // url recid
                customer.MultilineDiscountGroup,                // CustMultiLineDiscCode
                customer.TotalDiscountGroup,                    // CustEndDiscCode
                customer.LineDiscountGroup,                     // CustLineDiscCode
                customer.PriceGroup,                            // CustPriceGroup
                customer.TaxGroup,                         // TaxGroup
                customer.CreditLimit,                           // CustCreditMaxMST
                Convert.ToInt32(customer.Blocked),              // CustBlocked
                customer.OrganizationId ?? string.Empty,        // OrgID
                customer.UsePurchaseRequest,                    // RetailUsePurchRequest
                customer.VatNumber,                             // VATNum
                customer.InvoiceAccount ?? string.Empty,        // CustInvoiceAccount
                customer.MandatoryCreditLimit,                  // MandatoryCreditLimit
                string.Empty,                                   // ContactPersonId
                customer.UseOrderNumberReference,               // RetailUseOrderNumberReference
                (int)customer.ReceiptSettings,                  // RetailReceiptOption
                string.IsNullOrWhiteSpace(customer.ReceiptEmail) ? customer.Email : customer.ReceiptEmail,        // RetailReceiptEmail
                customer.IdentificationNumber ?? string.Empty,  // CustIdentificationNumber
                customer.FirstName,                             // firstName
                customer.MiddleName,                            // middleName
                customer.LastName,                              // lastName
                customer.PhoneExt,                              // phoneExtension
                customer.CellphoneRecordId,                     // cellphoneRecId
                ConvertCustomerAffiliationsToXml(customer.CustomerAffiliations) // Customer Affiliations
                };
            }

            private static object[] GetCreateAddressTransactionServiceParameters(
                Customer customer,
                Address address)
            {
                // workaround, AX requires at least one address field to be filled in, if country is not supplied, default it
                if (string.IsNullOrWhiteSpace(address.ThreeLetterISORegionName))
                {
                    address.ThreeLetterISORegionName = "USA";
                }

                return new object[]
                {
                customer.AccountNumber,
                address.Name,
                address.Street,
                address.City,
                address.County,
                address.State,
                address.ZipCode,
                address.ThreeLetterISORegionName,
                address.Phone,
                address.Email,
                address.Url,
                customer.TaxGroup,
                (int)address.AddressType,
                address.StreetNumber,
                address.DistrictName,
                address.BuildingCompliment,
                address.IsPrimary,
                };
            }

            private static object[] GetUpdateAddressTransactionServiceParameters(
                Customer customer,
                Address address)
            {
                return new object[]
                {
                address.RecordId,
                address.Name,
                address.Street,
                address.City,
                address.County,
                address.State,
                address.ZipCode,
                address.ThreeLetterISORegionName,
                address.Phone,
                address.Email,
                address.Url,
                customer.TaxGroup ?? string.Empty,
                (int)address.AddressType,
                address.PhoneRecordId,
                address.EmailRecordId,
                address.UrlRecordId,
                address.StreetNumber,
                address.DistrictName,
                address.BuildingCompliment,
                address.IsPrimary,
                };
            }

            private static object[] GetSendEmailTransactionServiceParameters(string emailId, string language, string emailAddress, object mappings, object xmlData, bool isTraceable, bool isWithRetries)
            {
                string serializedMappings = null;
                string serializedXmlData = string.Empty;

                if (mappings != null)
                {
                    // if we are pass a string already, assume it has been serialized externally
                    string mappingsAsString = mappings as string;
                    if (mappingsAsString != null)
                    {
                        serializedMappings = mappingsAsString;
                    }
                    else
                    {
                        serializedMappings = SerializationHelper.SerializeObjectToXml(mappings);
                    }
                }

                if (xmlData != null)
                {
                    string xmlDataAsString = xmlData as string;
                    if (xmlDataAsString != null)
                    {
                        serializedXmlData = xmlDataAsString;
                    }
                    else
                    {
                        serializedXmlData = SerializationHelper.SerializeObjectToXml(xmlData);
                    }
                }

                return new object[]
                {
                emailId,
                language,
                emailAddress,
                serializedMappings,
                serializedXmlData,
                isTraceable,
                isWithRetries,
                };
            }

            private static Customer ParseNewCustomerResposeCustomerData(Customer customer, ReadOnlyCollection<object> data)
            {
                customer.AccountNumber = (string)data[CreateNewCustomerAccountNumberIndex];
                customer.RecordId = long.Parse(data[CreateNewCustomerRecordIdIndex].ToString());
                customer.DirectoryPartyRecordId = long.Parse(data[CreateNewCustomerPartyIdIndex].ToString());
                customer.TaxGroup = (string)data[CreateNewCustomerSalesTaxGroupIndex];
                customer.RetailCustomerTableRecordId = long.Parse(data[CreateNewCustomerRetailCustTableRecordIdIndex].ToString());
                customer.PartyNumber = (string)data[CreateNewCustomerDirPartyTablePartyNumberIndex];
                customer.PersonNameId = long.Parse(data[CreateNewCustomerDirPersonRecordIdIndex].ToString());
                customer.PhoneRecordId = long.Parse(data[CreateNewCustomerPhoneRecordIdIndex].ToString());
                customer.PhoneLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerPhoneLogisticsLocationRecordId].ToString());
                customer.PhoneLogisticsLocationId = (string)data[CreateNewCustomerPhoneLogisticsLocationId];
                customer.PhonePartyLocationRecId = long.Parse(data[CreateNewCustomerPhoneDirPartyLocationRecordId].ToString());
                customer.Name = data[CreateNewCustomerDirPartyNameIndex].ToString();

                customer.EmailRecordId = long.Parse(data[CreateNewCustomerEmailRecordIdIndex].ToString());
                customer.EmailLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerEmailLogisticsLocationRecordId].ToString());
                customer.EmailLogisticsLocationId = (string)data[CreateNewCustomerEmailLogisticsLocationId];
                customer.EmailPartyLocationRecId = long.Parse(data[CreateNewCustomerEmailDirPartyLocationRecordId].ToString());
                if (string.IsNullOrEmpty(customer.ReceiptEmail) && (customer.Email.Length < 80))
                {
                    customer.ReceiptEmail = customer.Email;
                }

                customer.UrlRecordId = long.Parse(data[CreateNewCustomerUrlRecordIdIndex].ToString());
                customer.UrlLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerUrlLogisticsLocationRecordId].ToString());
                customer.UrlLogisticsLocationId = (string)data[CreateNewCustomerUrlLogisticsLocationId];
                customer.UrlPartyLocationRecId = long.Parse(data[CreateNewCustomerUrlDirPartyLocationRecordId].ToString());
                customer.CellphoneRecordId = long.Parse(data[CreateNewCustomerMobileRecordIdIndex].ToString());
                customer.CellphoneLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerMobileLogisticsLocationRecordId].ToString());
                customer.CellphoneLogisticsLocationId = (string)data[CreateNewCustomerMobileLogisticsLocationId];
                customer.CellphonePartyLocationRecId = long.Parse(data[CreateNewCustomerMobileDirPartyLocationRecordId].ToString());
                customer.CustomerAffiliations = ConvertXmlStringToCustomerAffiliations(data[CreateNewCustomerAffiliationsIndex].ToString());
                customer.AddressBooks = ConvertXmlStringToAddressBookPartyData(data[CreateNewCustomerAddressBookDataIndex].ToString());
                customer.IsCustomerTaxInclusive = Convert.ToBoolean(data[CreateNewCustomerIsCustomerTaxInclusiveIndex]);

                return customer;
            }

            private static Address ParseNewCustomerResponseAddressData(Customer customer, ReadOnlyCollection<object> data, Address address)
            {
                address.RecordId = long.Parse(data[CreateNewCustomerPostalAddressRecordIdIndex].ToString());
                address.DirectoryPartyTableRecordId = customer.DirectoryPartyRecordId;
                address.DirectoryPartyLocationRecordId = long.Parse(data[CreateNewCustomerPostalPartyLocationRecordIdIndex].ToString());
                address.DirectoryPartyLocationRoleRecordId = long.Parse(data[CreateNewCustomerPostalPartyLocationRoleRecordIdIndex].ToString());
                address.PartyNumber = customer.PartyNumber;

                address.LogisticsLocationId = (string)data[CreateNewCustomerPostalLogisticLocationIdIndex];
                address.LogisticsLocationRecordId = long.Parse(data[CreateNewCustomerPostalLocationRecordIdIndex].ToString());
                address.LogisticsLocationRoleRecordId = long.Parse(data[CreateNewCustomerPostalPartyLocationRoleIndex].ToString());
                address.LogisticsLocationExtRecordId = long.Parse(data[CreateNewCustomerAddressTaxLocationExtIdIndex].ToString());

                address.PhoneRecordId = long.Parse(data[CreateNewCustomerAddressPhoneRecordIdIndex].ToString());
                address.PhoneLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerAddressPhoneLogisticsLocationRecordId].ToString());
                address.PhoneLogisticsLocationId = (string)data[CreateNewCustomerAddressPhoneLogisticsLocationId];

                address.EmailRecordId = long.Parse(data[CreateNewCustomerAddressEmailRecordIdIndex].ToString());
                address.EmailLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerAddressEmailLogisticsLocationRecordId].ToString());
                address.EmailLogisticsLocationId = (string)data[CreateNewCustomerAddressEmailLogisticsLocationId];

                address.UrlRecordId = long.Parse(data[CreateNewCustomerAddressUrlRecordIdIndex].ToString());
                address.UrlLogisticsLocationRecordId = long.Parse(data[CreateNewCustomerAddressUrlLogisticsLocationRecordId].ToString());
                address.UrlLogisticsLocationId = (string)data[CreateNewCustomerAddressUrlLogisticsLocationId];

                address.FullAddress = data[CreateNewCustomerFullAddressIndex].ToString();

                return address;
            }

            /// <summary>
            /// Convert customer affiliations object data to xml.
            /// </summary>
            /// <param name="customerAffiliations">The customer affiliations.</param>
            /// <returns>The xml string.</returns>
            private static string ConvertCustomerAffiliationsToXml(IList<CustomerAffiliation> customerAffiliations)
            {
                if (customerAffiliations != null)
                {
                    CustomerAffiliationsInfo customerAffiliationsInfo = new CustomerAffiliationsInfo();
                    foreach (CustomerAffiliation customerAffiliation in customerAffiliations)
                    {
                        CustomerAffiliationInfo customerAffiliationInfo = new CustomerAffiliationInfo()
                        {
                            RecordId = customerAffiliation.RecordId,
                            CustAccountNum = customerAffiliation.CustAccountNum,
                            RetailAffiliationId = customerAffiliation.RetailAffiliationId
                        };

                        customerAffiliationsInfo.CustomerAffiliationItems.Add(customerAffiliationInfo);
                    }

                    return SerializationHelper.SerializeObjectToXml(customerAffiliationsInfo);
                }

                return string.Empty;
            }

            /// <summary>
            /// Converts the XML string to address book party data.
            /// </summary>
            /// <param name="addressBookPartyXml">The address book party XML.</param>
            /// <returns>A collection of address book data.</returns>
            private static IList<AddressBookPartyData> ConvertXmlStringToAddressBookPartyData(string addressBookPartyXml)
            {
                var serializer = new XmlSerializer(typeof(AddressBookPartyData[]));
                AddressBookPartyData[] result;

                using (TextReader stringReader = new StringReader(addressBookPartyXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.XmlResolver = null;
                    XmlReader reader = XmlReader.Create(stringReader, settings);
                    result = (AddressBookPartyData[])serializer.Deserialize(reader);
                }

                return new List<AddressBookPartyData>(result);
            }

            /// <summary>
            /// Convert customer affiliations xml string to CustomerAffiliation object collection.
            /// </summary>
            /// <param name="stringCustomerAffiliations">The customer affiliations xml string.</param>
            /// <returns>The CustomerAffiliation collection.</returns>
            private static IList<CustomerAffiliation> ConvertXmlStringToCustomerAffiliations(string stringCustomerAffiliations)
            {
                IList<CustomerAffiliation> customerAffiliations = null;

                if (!string.IsNullOrWhiteSpace(stringCustomerAffiliations))
                {
                    customerAffiliations = new Collection<CustomerAffiliation>();
                    CustomerAffiliationsInfo customerAffiliationsInfo = SerializationHelper.DeserializeObjectFromXml<CustomerAffiliationsInfo>(stringCustomerAffiliations);

                    foreach (CustomerAffiliationInfo info in customerAffiliationsInfo.CustomerAffiliationItems)
                    {
                        CustomerAffiliation customerAffiliation = new CustomerAffiliation()
                        {
                            RetailAffiliationId = info.RetailAffiliationId,
                            RecordId = info.RecordId,
                            CustAccountNum = info.CustAccountNum
                        };

                        customerAffiliations.Add(customerAffiliation);
                    }
                }

                return customerAffiliations;
            }
        }
    }
}
