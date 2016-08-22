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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Channel data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class ChannelDataService : IRequestHandler
        {
            private const string DeviceViewName = "DEVICESVIEW";
            private const string DeviceConfigurationViewName = "DEVICECONFIGURATIONSVIEW";
            private const string JournalTransactionView = "JOURNALTRANSACTIONVIEW";
            private const string RetailTransactionSalesTransView = "RETAILTRANSACTIONSALESTRANSVIEW";
            private const string CurrenciesViewName = "CURRENCIESVIEW";
            private const string ChannelViewName = "CHANNELVIEW";
            private const string OnlineChannelViewName = "ONLINECHANNELVIEW";
            private const string OrgUnitsView = "ORGUNITSVIEW";
            private const string ChannelConfigurationViewName = "CHANNELCONFIGURATIONVIEW_V2";
            private const string TransactionServiceProfileViewName = "TRANSACTIONSERVICEPROFILEVIEW";
            private const string DefaultTransactionServiceProfileViewName = "DEFAULTTRANSACTIONSERVICEPROFILEVIEW";
            private const string ChannelPropertiesViewName = "CHANNELPROPERTIESVIEW";
            private const string CurrentChannelLanguagesName = "CHANNELLANGUAGESVIEW";
            private const string ChannelProfileViewName = "CHANNELPROFILEVIEW";
            private const string ChannelProfilePropertyViewName = "CHANNELPROFILEPROPERTYVIEW";
            private const string RetailProductHierarchyViewName = "RETAILPRODUCTCATEGORYHIERARCHYVIEW";
            private const string ChannelCategoryHierarchyImageViewName = "CHANNELCATEGORYHIERARCHYIMAGEVIEW";
            private const string ChannelProductAttributeViewName = "CHANNELPRODUCTATTRIBUTEVIEW";
            private const string ChannelAttributeViewName = "CHANNELATTRIBUTEVIEW";
            private const string ChannelTenderTypeViewName = "CHANNELTENDERTYPEVIEW";
            private const string CardTypesViewName = "CARDTYPESVIEW";
            private const string StoreCashDeclarationViewName = "STORECASHDECLARATIONVIEW";
            private const string AttributeNameTranslationsViewName = "ATTRIBUTENAMETRANSLATIONSVIEW";
            private const string CategoryTranslationsViewName = "CATEGORYNAMETRANSLATIONSVIEW";
            private const string TextValueTranslationsViewName = "TEXTVALUETRANSLATIONSVIEW";
            private const string LocalizedStringsViewName = "LOCALIZEDSTRINGSVIEW";
            private const string TimeZoneViewName = "TIMEZONEVIEW";
            private const string RetailImagesView = "RETAILIMAGESVIEW";

            private const string CategoryNameTranslationsCategoryId = "CATEGORY";
            private const string ImagePropertyName = "IMAGE";

            private const string StoreNumberColumn = "STORENUMBER";
            private const string InventLocationColumn = "INVENTLOCATION";
            private const string RecIdColumn = "RECID";
            private const string SqlParamChannelId = "@channelId";
            private const string SqlParamCountingRequired = "@CountingRequired";

            private const string ChannelReferenceIdColumn = "CHANNELREFERENCEID";
            private const string StaffColumn = "STAFF";
            private const string SalesIdColumn = "SALESID";
            private const string ReceiptIdColumn = "RECEIPTID";
            private const string TransactionIdColumn = "TRANSACTIONID";
            private const string EmailColumn = "EMAIL";
            private const string ItemIdColumn = "ITEMID";
            private const string BarcodeColumn = "BARCODE";
            private const string InventSerialIdColumn = "INVENTSERIALID";
            private const string AttributeColumn = "ATTRIBUTE";
            private const string LanguageIdColumn = "LANGUAGEID";

            private const int MaxCachedCollectionSize = 500;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "False positive.")]
            private const decimal DefaultRoundingValue = 0.01M;

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(GetDeviceDataRequest),
                    typeof(GetChannelConfigurationDataRequest),
                    typeof(GetChannelTenderTypesDataRequest),
                    typeof(GetDefaultLanguageIdDataRequest),
                    typeof(GetCardTypeDataRequest),
                    typeof(SearchJournalTransactionsDataRequest),
                    typeof(GetTransactionServiceProfileDataRequest),
                    typeof(GetChannelProfileByChannelIdDataRequest),
                    typeof(GetChannelPropertiesByChannelIdDataRequest),
                    typeof(GetChannelLanguagesByChannelIdDataRequest),
                    typeof(GetCurrenciesDataRequest),
                    typeof(GetChannelCategoriesDataRequest),
                    typeof(GetDirectChildCategoriesDataRequest),
                    typeof(GetOnlineChannelByIdDataRequest),
                    typeof(GetChannelByIdDataRequest),
                    typeof(SearchOrgUnitDataRequest),
                    typeof(GetChannelCategoryAttributesByChannelIdDataRequest),
                    typeof(GetChannelProductAttributesByChannelIdDataRequest),
                    typeof(GetChannelProductAttributeByIdDataRequest),
                    typeof(GetChannelAttributesByChannelIdDataRequest),
                    typeof(GetChannelCashDeclarationDataRequest),
                    typeof(GetLocalizedStringsDataRequest),
                    typeof(GetImageByImageIdDataRequest)
                };
                }
            }

            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestType = request.GetType();
                Response response;

                if (requestType == typeof(GetDeviceDataRequest))
                {
                    response = this.GetDeviceByDeviceId((GetDeviceDataRequest)request);
                }
                else if (requestType == typeof(GetChannelConfigurationDataRequest))
                {
                    response = this.GetChannelConfiguration((GetChannelConfigurationDataRequest)request);
                }
                else if (requestType == typeof(GetChannelTenderTypesDataRequest))
                {
                    response = this.GetChannelTenderTypes((GetChannelTenderTypesDataRequest)request);
                }
                else if (requestType == typeof(GetDefaultLanguageIdDataRequest))
                {
                    response = this.GetDefaultLanguageId((GetDefaultLanguageIdDataRequest)request);
                }
                else if (requestType == typeof(GetCardTypeDataRequest))
                {
                    response = this.GetCardType((GetCardTypeDataRequest)request);
                }
                else if (requestType == typeof(SearchJournalTransactionsDataRequest))
                {
                    response = this.SearchJournalTransactions((SearchJournalTransactionsDataRequest)request);
                }
                else if (requestType == typeof(GetTransactionServiceProfileDataRequest))
                {
                    response = this.GetTransactionServiceProfile((GetTransactionServiceProfileDataRequest)request);
                }
                else if (requestType == typeof(GetChannelProfileByChannelIdDataRequest))
                {
                    response = this.GetChannelProfileByChannelId((GetChannelProfileByChannelIdDataRequest)request);
                }
                else if (requestType == typeof(GetChannelPropertiesByChannelIdDataRequest))
                {
                    response = this.GetChannelPropertiesByChannelId((GetChannelPropertiesByChannelIdDataRequest)request);
                }
                else if (requestType == typeof(GetChannelLanguagesByChannelIdDataRequest))
                {
                    response = this.GetChannelLanguagesByChannelId((GetChannelLanguagesByChannelIdDataRequest)request);
                }
                else if (requestType == typeof(GetCurrenciesDataRequest))
                {
                    response = this.GetCurrencies((GetCurrenciesDataRequest)request);
                }
                else if (requestType == typeof(GetChannelCategoriesDataRequest))
                {
                    response = this.GetChannelCategories((GetChannelCategoriesDataRequest)request);
                }
                else if (requestType == typeof(GetDirectChildCategoriesDataRequest))
                {
                    response = this.GetDirectChildCategories((GetDirectChildCategoriesDataRequest)request);
                }
                else if (requestType == typeof(GetOnlineChannelByIdDataRequest))
                {
                    response = this.GetOnlineChannelById((GetOnlineChannelByIdDataRequest)request);
                }
                else if (requestType == typeof(GetChannelByIdDataRequest))
                {
                    response = this.GetChannelById((GetChannelByIdDataRequest)request);
                }
                else if (requestType == typeof(SearchOrgUnitDataRequest))
                {
                    response = this.SearchOrgUnitByGivenCriteria((SearchOrgUnitDataRequest)request);
                }
                else if (requestType == typeof(GetChannelCategoryAttributesByChannelIdDataRequest))
                {
                    response = this.GetChannelCategoryAttributesByChannelId((GetChannelCategoryAttributesByChannelIdDataRequest)request);
                }
                else if (requestType == typeof(GetChannelProductAttributesByChannelIdDataRequest))
                {
                    response = this.GetChannelProductAttributesByChannelId((GetChannelProductAttributesByChannelIdDataRequest)request);
                }
                else if (requestType == typeof(GetChannelProductAttributeByIdDataRequest))
                {
                    response = this.GetChannelProductAttributeById((GetChannelProductAttributeByIdDataRequest)request);
                }
                else if (requestType == typeof(GetChannelAttributesByChannelIdDataRequest))
                {
                    response = this.GetChannelAttributesByChannelId((GetChannelAttributesByChannelIdDataRequest)request);
                }
                else if (requestType == typeof(GetChannelCashDeclarationDataRequest))
                {
                    response = this.GetChannelCashDeclaration((GetChannelCashDeclarationDataRequest)request);
                }
                else if (requestType == typeof(GetLocalizedStringsDataRequest))
                {
                    response = this.GetLocalizedStrings((GetLocalizedStringsDataRequest)request);
                }
                else if (requestType == typeof(GetImageByImageIdDataRequest))
                {
                    response = this.GetImageByImageId((GetImageByImageIdDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Populates orgUnits' addresses based on their property bag values.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="orgUnits">OrgUnits for the update.</param>
            private static void PopulateOrgUnitsAddress(RequestContext context, IEnumerable<OrgUnit> orgUnits)
            {
                IEnumerable<long> channelIds = orgUnits.Select(orgunit => orgunit.RecordId);

                var getOrgUnitContactsDataRequest = new GetOrgUnitContactsDataRequest(channelIds);
                IEnumerable<OrgUnitContact> contacts = context.Execute<EntityDataServiceResponse<OrgUnitContact>>(getOrgUnitContactsDataRequest).PagedEntityCollection.Results;

                var getOrgUnitAddressDataRequest = new GetOrgUnitAddressDataRequest(channelIds);
                IEnumerable<OrgUnitAddress> addresses = context.Execute<EntityDataServiceResponse<OrgUnitAddress>>(getOrgUnitAddressDataRequest).PagedEntityCollection.Results;

                // map the orgunit contacts to the orgunit.
                orgUnits = orgUnits.GroupJoin<OrgUnit, OrgUnitContact, long, OrgUnit>(
                    contacts,
                    orgUnit => orgUnit.RecordId,
                    orgUnitContact => orgUnitContact.ChannelId,
                    (orgUnit, orgUnitContacts) =>
                    {
                        orgUnit.Contacts = orgUnitContacts.AsReadOnly();
                        return orgUnit;
                    });

                // map the orgunit address to the orgunit.
                orgUnits = orgUnits.Join<OrgUnit, OrgUnitAddress, long, OrgUnit>(
                    addresses,
                    orgUnit => orgUnit.RecordId,
                    orgUnitAddress => orgUnitAddress.ChannelId,
                    (orgUnit, orgUnitAddress) =>
                    {
                        orgUnit.OrgUnitAddress = orgUnitAddress as Address;
                        return orgUnit;
                    });

                foreach (OrgUnit orgUnit in orgUnits)
                {
                    PopulateOrgUnitContact(orgUnit);
                }
            }

            /// <summary>
            /// Populates <see cref="OrgUnit.OrgUnitAddress"/>.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="orgUnit">The <see cref="OrgUnit"/> instance for the update.</param>
            private static void PopulateOrgUnitAddress(RequestContext context, OrgUnit orgUnit)
            {
                PopulateOrgUnitsAddress(context, new OrgUnit[] { orgUnit });
            }

            /// <summary>
            /// Populates organization unit address according to organization unit contacts.
            /// </summary>
            /// <param name="orgUnit">The <see cref="OrgUnit"/> instance for the update.</param>
            private static void PopulateOrgUnitContact(OrgUnit orgUnit)
            {
                // in case of that an orgunit has no postal address.
                if (orgUnit.OrgUnitAddress == null)
                {
                    orgUnit.OrgUnitAddress = new Address();
                }

                // Phone
                OrgUnitContact phone = GetOrgUnitContactByType(orgUnit.Contacts, ContactInfoType.Phone);
                if (phone != null)
                {
                    orgUnit.OrgUnitAddress.Phone = phone.Locator;
                    orgUnit.OrgUnitAddress.PhoneExt = phone.LocatorExtension;
                    orgUnit.OrgUnitAddress.PhoneRecordId = phone.LocationRecordId;
                }

                // Email
                OrgUnitContact email = GetOrgUnitContactByType(orgUnit.Contacts, ContactInfoType.Email);
                if (email != null)
                {
                    orgUnit.OrgUnitAddress.Email = email.Locator;
                    orgUnit.OrgUnitAddress.EmailRecordId = email.LocationRecordId;
                }

                // Url
                OrgUnitContact url = GetOrgUnitContactByType(orgUnit.Contacts, ContactInfoType.Url);
                if (url != null)
                {
                    orgUnit.OrgUnitAddress.Url = url.Locator;
                    orgUnit.OrgUnitAddress.UrlRecordId = url.LocationRecordId;
                }
            }

            /// <summary>
            /// Gets the organization unit contact by contact type.
            /// </summary>
            /// <param name="contacts">The collection of contacts.</param>
            /// <param name="type">The desired type.</param>
            /// <returns>The organization unit contact.</returns>
            private static OrgUnitContact GetOrgUnitContactByType(IEnumerable<OrgUnitContact> contacts, ContactInfoType type)
            {
                if (contacts == null)
                {
                    return null;
                }
                else
                {
                    // Gets the primary contact first.
                    OrgUnitContact contact = contacts.FirstOrDefault(orgUnitContact => orgUnitContact.ContactType == type && orgUnitContact.IsPrimary);

                    // If cannot get primary contact, then try to get a non-primary one.
                    if (contact == null)
                    {
                        contact = contacts.FirstOrDefault(orgUnitContact => orgUnitContact.ContactType == type);
                    }

                    return contact;
                }
            }

            /// <summary>
            /// Get retail image by image identifier.
            /// </summary>
            /// <param name="request">The get image by image id data request.</param>
            /// <returns>The retail image.</returns>
            private SingleEntityDataServiceResponse<RetailImage> GetImageByImageId(GetImageByImageIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                RetailImage result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetPictureByPictureId(request.ImageId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = RetailImagesView,
                        Where = "PICTUREID = @pictureId",
                        IsQueryByPrimaryKey = true,
                        OrderBy = RecIdColumn
                    };

                    query.Parameters["@pictureId"] = request.ImageId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<RetailImage>(query).Results.SingleOrDefault();
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetPictureByPictureId completed in {0} ms", processTimer.ElapsedMilliseconds);
                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutPictureByPictureId(request.ImageId, request.QueryResultSettings, result);
                }

                return new SingleEntityDataServiceResponse<RetailImage>(result);
            }

            /// <summary>
            /// Gets the localized string for a specified language and text identifier.
            /// </summary>
            /// <param name="request">The get localized strings data request.</param>
            /// <returns>
            /// A collection containing the localized string record for the language and text identifiers specified.
            /// </returns>
            private EntityDataServiceResponse<LocalizedString> GetLocalizedStrings(GetLocalizedStringsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<LocalizedString> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetLocalizedStrings(request.LanguageId, request.TextId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = LocalizedStringsViewName,
                        IsQueryByPrimaryKey = false,
                        OrderBy = RecIdColumn
                    };

                    if (!string.IsNullOrWhiteSpace(request.LanguageId))
                    {
                        if (request.TextId.HasValue)
                        {
                            query.Where = string.Format("{0} = @LanguageId AND {1} = @TextId AND DATAAREAID = @DataAreaId", LocalizedString.LanguageIdColumn, LocalizedString.TextIdColumn);
                            query.Parameters["@TextId"] = request.TextId;
                        }
                        else
                        {
                            query.Where = string.Format("{0} = @LanguageId AND DATAAREAID = @DataAreaId", LocalizedString.LanguageIdColumn);
                        }

                        query.Parameters["@LanguageId"] = request.LanguageId;
                        query.Parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    }

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<LocalizedString>(query);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetLocalizedStrings completed in {0} ms", processTimer.ElapsedMilliseconds);
                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutLocalizedStrings(request.LanguageId, request.TextId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<LocalizedString>(result);
            }

            /// <summary>
            /// Gets the cash declarations.
            /// </summary>
            /// <param name="request">The get channel cash declaration data request.</param>
            /// <returns>
            /// A collection of cash declarations.
            /// </returns>
            private EntityDataServiceResponse<CashDeclaration> GetChannelCashDeclaration(GetChannelCashDeclarationDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<CashDeclaration> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelCashDeclaration(request.ChannelId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        Where = "CHANNELID = " + SqlParamChannelId,
                        OrderBy = "AMOUNT, CURRENCY",
                        From = StoreCashDeclarationViewName,
                        IsQueryByPrimaryKey = false,
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<CashDeclaration>(query);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelCashDeclaration completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelCashDeclaration(request.ChannelId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<CashDeclaration>(result);
            }

            /// <summary>
            /// Gets the channel attributes of a given channel.
            /// </summary>
            /// <param name="request">The get channel attributes by channel id data request.</param>
            /// <returns>
            /// A collection of channel attributes.
            /// </returns>
            private EntityDataServiceResponse<ChannelAttribute> GetChannelAttributesByChannelId(GetChannelAttributesByChannelIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<ChannelAttribute> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelAttributesByChannelId(request.ChannelId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = ChannelAttributeViewName,
                        Where = "CHANNEL = " + SqlParamChannelId,
                        IsQueryByPrimaryKey = false,
                        OrderBy = RecIdColumn
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    PagedResult<ChannelAttributeView> channelAttributeView;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        channelAttributeView = databaseContext.ReadEntity<ChannelAttributeView>(query);
                    }

                    var channelAttributes = new List<ChannelAttribute>();

                    foreach (var caview in channelAttributeView.Results)
                    {
                        // Convert ChannelAttributeView to ChannelAttribute
                        var ca = caview.ToChannelAttribute();

                        // Read name translations
                        ca.NameTranslations = this.GetAttributeNameTranslations(ca.RecordId, request.RequestContext);

                        // Read text value translations
                        if (ca.ChannelAttributeValue != null && ca.ChannelAttributeValue is AttributeTextValue)
                        {
                            // Pull all available translations for this channel text attribute.
                            var translationQuery = new SqlPagedQuery(QueryResultSettings.AllRecords)
                            {
                                From = TextValueTranslationsViewName,
                                Where = "TEXTVALUETABLE = @AttributeValueId",
                                IsQueryByPrimaryKey = false,
                            };

                            translationQuery.Parameters["@AttributeValueId"] = ca.AttributeValueRecordId;

                            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                            {
                                (ca.ChannelAttributeValue as AttributeTextValue).TextValueTranslations = databaseContext.ReadEntity<TextValueTranslation>(translationQuery).Results;
                            }
                        }

                        channelAttributes.Add(ca);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelAttributesByChannelId completed in {0} ms", processTimer.ElapsedMilliseconds);

                    result = channelAttributes.AsPagedResult();

                    updateL2Cache &= result != null
                                 && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelAttributesByChannelId(request.ChannelId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<ChannelAttribute>(result);
            }

            /// <summary>
            /// Gets a list of attribute name translation of a given attribute.
            /// </summary>
            /// <param name="attributeId">The attribute identifier.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of attribute name translation.</returns>
            private ReadOnlyCollection<TextValueTranslation> GetAttributeNameTranslations(long attributeId, RequestContext context)
            {
                Stopwatch processTimer = Stopwatch.StartNew();

                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = AttributeNameTranslationsViewName,
                    Where = "ATTRIBUTE = @Attibute",
                    IsQueryByPrimaryKey = false,
                    OrderBy = AttributeColumn
                };

                query.Parameters["@Attibute"] = attributeId;

                PagedResult<AttributeNameTranslation> attributeNameTranslations;
                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    attributeNameTranslations = databaseContext.ReadEntity<AttributeNameTranslation>(query);
                }

                var textValueTransaltions = attributeNameTranslations.Results.Select(t => new TextValueTranslation { Language = t.Language, Text = t.FriendlyName });

                processTimer.Stop();
                NetTracer.Information("** timer info: GetAttributeNameTranslations completed in {0} ms", processTimer.ElapsedMilliseconds);

                return textValueTransaltions.AsReadOnly();
            }

            /// <summary>
            /// Gets the product attribute of a given attribute identifier.
            /// </summary>
            /// <param name="request">The get channel product attribute by id data request.</param>
            /// <returns>The product attribute.</returns>
            private SingleEntityDataServiceResponse<AttributeProduct> GetChannelProductAttributeById(GetChannelProductAttributeByIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Columns, "request.Columns");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                AttributeProduct result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelProductAttributeById(request.AttributeId, request.Columns), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        Select = request.Columns,
                        From = ChannelProductAttributeViewName,
                        Where = "HOSTCHANNEL = " + SqlParamChannelId + " AND ATTRIBUTE = @attributeId",
                        IsQueryByPrimaryKey = true,
                        OrderBy = RecIdColumn,
                    };

                    query.Parameters[SqlParamChannelId] = request.RequestContext.GetPrincipal().ChannelId;
                    query.Parameters["@attributeId"] = request.AttributeId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<AttributeProduct>(query).SingleOrDefault();
                    }

                    if (result != null)
                    {
                        result.NameTranslations = this.GetAttributeNameTranslations(result.RecordId, request.RequestContext);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelProductAttributeById completed in {0} ms", processTimer.ElapsedMilliseconds);
                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelProductAttributeById(request.AttributeId, request.Columns, result);
                }

                return new SingleEntityDataServiceResponse<AttributeProduct>(result);
            }

            /// <summary>
            /// Gets the product attributes of a given channel.
            /// </summary>
            /// <param name="request">The get channel product attributes by channel id data request.</param>
            /// <returns>
            /// A collection of product attribute.
            /// </returns>
            private EntityDataServiceResponse<AttributeProduct> GetChannelProductAttributesByChannelId(GetChannelProductAttributesByChannelIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<AttributeProduct> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelProductAttributesByChannelId(request.ChannelId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = ChannelProductAttributeViewName,
                        Where = "HOSTCHANNEL = " + SqlParamChannelId,
                        IsQueryByPrimaryKey = false,
                        OrderBy = RecIdColumn
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<AttributeProduct>(query);
                    }

                    foreach (var pa in result.Results)
                    {
                        pa.NameTranslations = this.GetAttributeNameTranslations(pa.RecordId, request.RequestContext);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelProductAttributesByChannelId completed in {0} ms", processTimer.ElapsedMilliseconds);
                    updateL2Cache &= result != null
                                 && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelProductAttributesByChannelId(request.ChannelId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<AttributeProduct>(result);
            }

            /// <summary>
            /// Gets the category attributes of a given channel category.
            /// </summary>
            /// <param name="request">The get channel category attributes by channel id data request.</param>
            /// <returns>
            /// A collection of category attributes.
            /// </returns>
            private EntityDataServiceResponse<AttributeCategory> GetChannelCategoryAttributesByChannelId(GetChannelCategoryAttributesByChannelIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<AttributeCategory> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelCategoryAttributesByChannelId(request.ChannelId, request.CategoryIds, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    ReadOnlyCollection<ChannelCategoryAttribute> channelCategoryAttributes;
                    Stopwatch processTimer = Stopwatch.StartNew();

                    GetDefaultLanguageIdDataRequest getDefaultLanguageIdDataRequest = new GetDefaultLanguageIdDataRequest();
                    string defaultChannelLanguageId = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<string>>(getDefaultLanguageIdDataRequest, request.RequestContext).Entity;

                    GetChannelCategoryAttributesDataRequest getChannelCategoryAttributesDataRequest = new GetChannelCategoryAttributesDataRequest(request.ChannelId, request.CategoryIds, request.QueryResultSettings);
                    EntityDataServiceResponse<ChannelCategoryAttribute> response = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ChannelCategoryAttribute>>(getChannelCategoryAttributesDataRequest, request.RequestContext);
                    channelCategoryAttributes = response.PagedEntityCollection.Results;

                    var categoryAttributes = new List<AttributeCategory>();

                    foreach (var caview in channelCategoryAttributes)
                    {
                        AttributeCategory ca;
                        if (caview.DataType == AttributeDataType.Image)
                        {
                            ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
                            string categoryDefaultImageTemplate = channelConfiguration != null ? channelConfiguration.CategoryDefaultImageTemplate : string.Empty;
                            ca = caview.ToAttributeCategory(defaultChannelLanguageId, categoryDefaultImageTemplate);
                        }
                        else
                        {
                            ca = caview.ToAttributeCategory(defaultChannelLanguageId);
                        }

                        // Read name translations
                        ca.NameTranslations = this.GetAttributeNameTranslations(ca.RecordId, request.RequestContext);

                        // Read text value translations
                        if (ca.CategoryAttributeValue != null && ca.CategoryAttributeValue is AttributeTextValue)
                        {
                            // Pull all available translations for this text category attribute.
                            var translationQuery = new SqlPagedQuery(QueryResultSettings.AllRecords)
                            {
                                From = TextValueTranslationsViewName,
                                Where = "TEXTVALUETABLE = @AttributeValueId",
                                IsQueryByPrimaryKey = false,
                                OrderBy = RecIdColumn
                            };

                            translationQuery.Parameters["@AttributeValueId"] = ca.AttributeValueRecordId;

                            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                            {
                                (ca.CategoryAttributeValue as AttributeTextValue).TextValueTranslations = databaseContext.ReadEntity<TextValueTranslation>(translationQuery).Results;
                            }
                        }

                        categoryAttributes.Add(ca);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelCategoryAttributesByChannelId completed in {0} ms", processTimer.ElapsedMilliseconds);

                    result = categoryAttributes.AsPagedResult();

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelCategoryAttributesByChannelId(request.ChannelId, request.CategoryIds, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<AttributeCategory>(result);
            }

            /// <summary>
            /// Gets the channel by channel identifier.
            /// </summary>
            /// <param name="request">The get channel by id data request.</param>
            /// <returns>The channel.</returns>
            private SingleEntityDataServiceResponse<Channel> GetChannelById(GetChannelByIdDataRequest request)
            {
                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                Channel result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelById(request.ChannelId), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        From = ChannelViewName,
                        Where = "RECID = " + SqlParamChannelId,
                        IsQueryByPrimaryKey = true,
                        OrderBy = RecIdColumn,
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<Channel>(query).SingleOrDefault();
                    }

                    if (result == null)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ChannelIsNotPublished, "The specified channel is not published.");
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelById completed in {0} ms", processTimer.ElapsedMilliseconds);
                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelById(request.ChannelId, result);
                }

                return new SingleEntityDataServiceResponse<Channel>(result);
            }

            private IEnumerable<string> SetFilterClause(SearchStoreCriteria storeSearchCriteria, SqlPagedQuery sqlQuery, bool isOnline)
            {
                var whereClauses = new Collection<string>();

                if (!string.IsNullOrEmpty(storeSearchCriteria.StoreNumber) && isOnline != true)
                {
                    whereClauses.Add(string.Format("{0} = @StoreNumber", StoreNumberColumn));
                    sqlQuery.Parameters["@StoreNumber"] = storeSearchCriteria.StoreNumber;
                }

                if (!string.IsNullOrEmpty(storeSearchCriteria.InventoryLocationId))
                {
                    whereClauses.Add(string.Format("{0} = @InventLocation", InventLocationColumn));
                    sqlQuery.Parameters["@InventLocation"] = storeSearchCriteria.InventoryLocationId;
                }

                if (storeSearchCriteria.ChannelId != 0)
                {
                    whereClauses.Add(string.Format("{0} = " + SqlParamChannelId, RecIdColumn));
                    sqlQuery.Parameters[SqlParamChannelId] = storeSearchCriteria.ChannelId;
                }

                return whereClauses;
            }

            /// <summary>
            /// Gets the online channel by channel identifier.
            /// </summary>
            /// <param name="request">The get online channel by id data request.</param>
            /// <returns>The online channel.</returns>
            private SingleEntityDataServiceResponse<OnlineChannel> GetOnlineChannelById(GetOnlineChannelByIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ColumnSet, "request.ColumnSet");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                OnlineChannel result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetOnlineChannelById(request.ChannelId, request.ColumnSet), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        Select = request.ColumnSet,
                        From = OnlineChannelViewName,
                        Where = "RECID = " + SqlParamChannelId,
                        IsQueryByPrimaryKey = true,
                        OrderBy = RecIdColumn,
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<OnlineChannel>(query).SingleOrDefault();
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetOnlineChannelById completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutOnlineChannelById(request.ChannelId, request.ColumnSet, result);
                }

                return new SingleEntityDataServiceResponse<OnlineChannel>(result);
            }

            /// <summary>
            /// Gets the child categories.
            /// </summary>
            /// <param name="request">The get directC child categories data request.</param>
            /// <returns>
            /// A collection of categories.
            /// </returns>
            private EntityDataServiceResponse<Category> GetDirectChildCategories(GetDirectChildCategoriesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<Category> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetDirectChildCategories(request.ChannelId, request.ParentCategoryId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        Where = "PARENTCATEGORY = @ParentCategoryId",
                        IsQueryByPrimaryKey = false,
                    };

                    query.Parameters["@ParentCategoryId"] = request.ParentCategoryId;

                    if (request.ChannelId != 0)
                    {
                        query.From = ChannelCategoryHierarchyImageViewName;
                        query.Where = string.Concat("CHANNELID = " + SqlParamChannelId + " AND ", query.Where);
                        query.Parameters[SqlParamChannelId] = request.ChannelId;
                    }
                    else
                    {
                        // The category hierarchy for the warehouse is the retail product category hierarchy.
                        query.From = RetailProductHierarchyViewName;
                    }

                    query.OrderBy = RecIdColumn;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<Category>(query);
                    }

                    // Fill the image values
                    this.FillCategoryImageProperty(request.ChannelId, result.Results, request.RequestContext);

                    // Fill in the category name translations
                    this.FillCategoryNameTranslations(request.ChannelId, result.Results, request.RequestContext);

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetDirectChildCategories completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutDirectChildCategories(request.ChannelId, request.ParentCategoryId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<Category>(result);
            }

            /// <summary>
            /// Fill image values for categories.
            /// </summary>
            /// <param name="channelId">Channel Identifier.</param>
            /// <param name="categories">Categories to be associated.</param>
            /// <param name="context">The request context.</param>
            private void FillCategoryImageProperty(long channelId, ReadOnlyCollection<Category> categories, RequestContext context)
            {
                if (categories != null &&
                    categories.Any())
                {
                    // get category attributes and associated
                    GetChannelCategoryAttributesByChannelIdDataRequest getChannelAttributesByChannelIdDataRequest = new GetChannelCategoryAttributesByChannelIdDataRequest(channelId, categories.Select(c => c.RecordId), QueryResultSettings.AllRecords);
                    PagedResult<AttributeCategory> categoryAttributes = context.Runtime.Execute<EntityDataServiceResponse<AttributeCategory>>(getChannelAttributesByChannelIdDataRequest, context).PagedEntityCollection;

                    if (categoryAttributes != null &&
                        categoryAttributes.Results.Any())
                    {
                        // figure out category image attributes and assign them to relevant category object
                        var imageAttributes = categoryAttributes.Results.Where(ca => string.Compare(ca.Name, ImagePropertyName, System.StringComparison.OrdinalIgnoreCase) == 0);

                        if (imageAttributes != null &&
                            imageAttributes.Any())
                        {
                            var categoriesDictionary = categories.ToDictionary(c => c.RecordId);

                            foreach (var imageAttribute in imageAttributes)
                            {
                                categoriesDictionary[imageAttribute.Category].Images = (imageAttribute.CategoryAttributeValue as AttributeMediaLocationValue).Value;
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Populate the name translations for the categories.
            /// </summary>
            /// <param name="channelId">Channel identifier.</param>
            /// <param name="categories">The category set that needs to be filled with name translations.</param>
            /// <param name="context">The request context.</param>
            private void FillCategoryNameTranslations(long channelId, IEnumerable<Category> categories, RequestContext context)
            {
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = CategoryTranslationsViewName,
                    Where = "CHANNEL = @Channel",
                    IsQueryByPrimaryKey = false,
                    OrderBy = "FRIENDLYNAME"
                };

                query.Parameters["@Channel"] = channelId;

                Dictionary<long, IList<Category>> categoriesById = new Dictionary<long, IList<Category>>();
                foreach (var category in categories)
                {
                    if (!categoriesById.ContainsKey(category.RecordId))
                    {
                        categoriesById[category.RecordId] = new List<Category>();
                    }

                    categoriesById[category.RecordId].Add(category);
                }

                using (var recordIdTableType = new RecordIdTableType(categoriesById.Keys, CategoryNameTranslationsCategoryId))
                {
                    query.Parameters["@TVP_RECIDTABLETYPE"] = recordIdTableType;

                    // Execute the query
                    PagedResult<CategoryNameTranslation> results;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        results = databaseContext.ReadEntity<CategoryNameTranslation>(query);
                    }

                    var nameTranslationsDictionary = new Dictionary<long, IList<TextValueTranslation>>();

                    foreach (CategoryNameTranslation categoryNameTranslation in results.Results)
                    {
                        var currentCategoryId = (long)categoryNameTranslation.GetProperty(CategoryNameTranslationsCategoryId);
                        if (!nameTranslationsDictionary.ContainsKey(currentCategoryId))
                        {
                            nameTranslationsDictionary[currentCategoryId] = new List<TextValueTranslation>();
                        }

                        nameTranslationsDictionary[currentCategoryId].Add(new TextValueTranslation { Language = categoryNameTranslation.LanguageId, Text = categoryNameTranslation.FriendlyName });
                    }

                    // Populate the Category Name Translations in Category
                    foreach (Category category in categories)
                    {
                        if (nameTranslationsDictionary.ContainsKey(category.RecordId))
                        {
                            category.NameTranslations = nameTranslationsDictionary[category.RecordId];
                        }
                    }
                }
            }

            /// <summary>
            /// The data service method to execute the data manager to get the channel tender types.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<TenderType> GetChannelTenderTypes(GetChannelTenderTypesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);
                bool found;
                bool updateL2Cache;
                PagedResult<TenderType> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelTenderTypes(request.ChannelId, request.CountingRequired, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings);
                    string whereClause = string.Format("CHANNEL={0}", SqlParamChannelId);

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    if (request.CountingRequired != null)
                    {
                        query.Parameters[SqlParamCountingRequired] = request.CountingRequired == null ? null : (int?)Convert.ToInt32(request.CountingRequired.Value);
                        whereClause = string.Format("CHANNEL={0} AND COUNTINGREQUIRED={1}", SqlParamChannelId, SqlParamCountingRequired);
                    }

                    query.From = ChannelTenderTypeViewName;
                    query.Where = whereClause;
                    query.IsQueryByPrimaryKey = false;
                    query.OrderBy = RecIdColumn;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<TenderType>(query);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelTenderTypes completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelTenderTypes(request.ChannelId, request.CountingRequired, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<TenderType>(result);
            }

            /// <summary>
            /// Gets the channel configuration by channel identifier.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private SingleEntityDataServiceResponse<ChannelConfiguration> GetChannelConfiguration(GetChannelConfigurationDataRequest request)
            {
                long channelId = request.ChannelId;

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                ChannelConfiguration result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelConfiguration(channelId), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        From = ChannelConfigurationViewName,
                        Where = "RECID =" + SqlParamChannelId + " AND @utcdate BETWEEN VALIDFROM AND VALIDTO",
                        IsQueryByPrimaryKey = true,
                        OrderBy = RecIdColumn
                    };

                    query.Parameters[SqlParamChannelId] = channelId;
                    query.Parameters["@utcdate"] = DateTime.UtcNow;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ChannelConfiguration>(query).Results.SingleOrDefault();
                    }

                    if (result == null)
                    {
                        string message = "No channel configuration was found. "
                            + "Please verify that if the channel is created and published in AX, channel data group has been set correctly and channel related data has been completely synced to the channel database.";
                        throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration, ExceptionSeverity.Warning, message);
                    }

                    result.TimeZoneRecords = this.GetTimeZones(result.TimeZoneCode, request.RequestContext);

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelConfiguration completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null;

                    // Populate transaction service profile on channel configuration.
                    var getTransactionServiceProfileRequest = new GetTransactionServiceProfileDataRequest(channelId);
                    TransactionServiceProfile transactionServiceProfile = request.RequestContext.Execute<SingleEntityDataServiceResponse<TransactionServiceProfile>>(getTransactionServiceProfileRequest).Entity;
                    if (transactionServiceProfile != null)
                    {
                        result.TransactionServiceProfile = transactionServiceProfile;
                    }

                    // Populate channel profile on channel configuration.
                    var getChannelProfileRequest = new GetChannelProfileByChannelIdDataRequest(channelId, QueryResultSettings.SingleRecord);
                    ChannelProfile channelProfile = request.RequestContext.Execute<SingleEntityDataServiceResponse<ChannelProfile>>(getChannelProfileRequest).Entity;
                    if (channelProfile != null)
                    {
                        result.SetProfileProperties(channelProfile.ProfileProperties);
                    }

                    // Populate channel properties on channel configuration.
                    var getChannelPropertiesRequest = new GetChannelPropertiesByChannelIdDataRequest(channelId, QueryResultSettings.AllRecords);
                    ReadOnlyCollection<ChannelProperty> properties = request.RequestContext.Execute<EntityDataServiceResponse<ChannelProperty>>(getChannelPropertiesRequest).PagedEntityCollection.Results;
                    if (!properties.IsNullOrEmpty())
                    {
                        result.SetProperties(properties);
                    }

                    // Populate channel languages on channel configuration.
                    var getChannelLanguagesRequest = new GetChannelLanguagesByChannelIdDataRequest(channelId, QueryResultSettings.AllRecords);
                    ReadOnlyCollection<ChannelLanguage> languages = request.RequestContext.Execute<EntityDataServiceResponse<ChannelLanguage>>(getChannelLanguagesRequest).PagedEntityCollection.Results;
                    if (!languages.IsNullOrEmpty())
                    {
                        result.SetLanguages(languages);
                    }
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelConfiguration(result, channelId);
                }

                return new SingleEntityDataServiceResponse<ChannelConfiguration>(result);
            }

            /// <summary>
            /// Gets the list of time zones by time zone code.
            /// </summary>
            /// <param name="timeZoneCode">The time zone code.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Collection of <see cref="TimeZoneInterval"/> time zone records.</returns>
            private List<TimeZoneInterval> GetTimeZones(int timeZoneCode, RequestContext context)
            {
                if (timeZoneCode == 0)
                {
                    NetTracer.Warning("Time Zone Code is unknown for channel: {0}", context.GetPrincipal().ChannelId);
                    return new List<TimeZoneInterval>();
                }

                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = TimeZoneViewName,
                    Where = "[TIMEZONEID] = @timeZoneCode",
                    OrderBy = TimeZoneInterval.StartDateColumn
                };

                query.Parameters["@timeZoneCode"] = timeZoneCode;

                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    return databaseContext.ReadEntity<TimeZoneInterval>(query).Results.ToList();
                }
            }

            /// <summary>
            /// Gets the channel categories.
            /// </summary>
            /// <param name="request">The get channel categories data request.</param>
            /// <returns>
            /// A collection of categories.
            /// </returns>
            private EntityDataServiceResponse<Category> GetChannelCategories(GetChannelCategoriesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<Category> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelCategories(request.ChannelId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        IsQueryByPrimaryKey = false
                    };

                    if (request.ChannelId != 0)
                    {
                        query.From = ChannelCategoryHierarchyImageViewName;
                        query.Where = "CHANNELID = " + SqlParamChannelId;
                        query.Parameters[SqlParamChannelId] = request.ChannelId;
                    }
                    else
                    {
                        // The category hierarchy for the warehouse is the retail product category hierarchy.
                        query.From = RetailProductHierarchyViewName;
                    }

                    query.OrderBy = query.OrderBy ?? "NAME";

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<Category>(query);
                    }

                    // Fill the image values
                    this.FillCategoryImageProperty(request.ChannelId, result.Results, request.RequestContext);

                    // Fill in the category name translations
                    this.FillCategoryNameTranslations(request.ChannelId, result.Results, request.RequestContext);

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelCategories(request.ChannelId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<Category>(result);
            }

            private SingleEntityDataServiceResponse<string> GetDefaultLanguageId(GetDefaultLanguageIdDataRequest request)
            {
                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                string result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelDefaultLanguageId(), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        From = CurrentChannelLanguagesName,
                        Where = "CHANNEL = " + SqlParamChannelId + " AND ISDEFAULT = 1",
                        IsQueryByPrimaryKey = true,
                        OrderBy = LanguageIdColumn
                    };

                    query.Parameters[SqlParamChannelId] = request.RequestContext.GetPrincipal().ChannelId;

                    ChannelLanguage channelLanguage;
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        channelLanguage = databaseContext.ReadEntity<ChannelLanguage>(query).FirstOrDefault();
                    }

                    if (channelLanguage == null || string.IsNullOrWhiteSpace(channelLanguage.LanguageId))
                    {
                        throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration, ExceptionSeverity.Warning, "No default language or empty default language id found in channel configuration.");
                    }

                    result = channelLanguage.LanguageId;

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelDefaultLanguageId completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelDefaultLanguageId(result);
                }

                return new SingleEntityDataServiceResponse<string>(result);
            }

            private EntityDataServiceResponse<CardTypeInfo> GetCardType(GetCardTypeDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<CardTypeInfo> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetCardTypes(request.RequestContext.GetPrincipal().ChannelId, request.CardTypeId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = CardTypesViewName,
                        IsQueryByPrimaryKey = false,
                        Where = "CHANNELID = " + SqlParamChannelId,
                        OrderBy = RecIdColumn
                    };

                    query.Parameters[SqlParamChannelId] = request.RequestContext.GetPrincipal().ChannelId;

                    if (!string.IsNullOrWhiteSpace(request.CardTypeId))
                    {
                        query.Where += " and CARDTYPEID = @CardTypeId";
                        query.Parameters["@CardTypeId"] = request.CardTypeId;
                    }

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<CardTypeInfo>(query);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetCardTypes completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null
                                 && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutCardTypes(request.CardTypeId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<CardTypeInfo>(result);
            }

            private EntityDataServiceResponse<Transaction> SearchJournalTransactions(SearchJournalTransactionsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.SearchCriteria, "request.SearchCriteria");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                QueryResultSettings settings = request.QueryResultSettings;
                TransactionSearchCriteria criteria = request.SearchCriteria;

                var query = new SqlPagedQuery(settings)
                {
                    From = JournalTransactionView,
                    OrderBy = settings.Sorting.ToString(),
                };

                if (settings.Sorting == null || settings.Sorting.Count == 0)
                {
                    query.OrderBy = new SortingInfo(RetailTransactionTableSchema.CreatedDateTimeColumn, true).ToString();
                }

                var whereClauses = new List<string>();

                this.BuildWhereClauseForItemIdBarcodeSerialNumber(criteria, query, whereClauses, request.RequestContext.Runtime.Configuration.DatabaseProvider.GetDatabaseQueryBuilder());
                this.BuildSearchOrderWhereClause(criteria, query, whereClauses);

                if (!string.IsNullOrEmpty(criteria.CustomerFirstName) || !string.IsNullOrEmpty(criteria.CustomerLastName))
                {
                    string customerName = criteria.CustomerFirstName == null ? "%" + criteria.CustomerLastName : criteria.CustomerFirstName + "%" + criteria.CustomerLastName;

                    whereClauses.Add(string.Format("{0} like @customerName", SalesTransaction.NameColumn));
                    query.Parameters["@customerName"] = customerName;
                }

                // Make sure to query the current channel only.
                whereClauses.Add(string.Format("{0} = @channelId", RetailTransactionTableSchema.ChannelIdColumn));
                query.Parameters["@channelId"] = request.RequestContext.GetPrincipal().ChannelId;

                query.Where = string.Join(" AND ", whereClauses);

                PagedResult<Transaction> transactions = null;
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    transactions = databaseContext.ReadEntity<Transaction>(query);
                }

                return new EntityDataServiceResponse<Transaction>(transactions);
            }

            /// <summary>
            /// Gets the channel profile.
            /// </summary>
            /// <param name="request">The get channel profile by channel id data request.</param>
            /// <returns>The channel profile.</returns>
            private SingleEntityDataServiceResponse<ChannelProfile> GetChannelProfileByChannelId(GetChannelProfileByChannelIdDataRequest request)
            {
                ThrowIf.Null(request, "request");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                ChannelProfile channelProfile = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelProfileByChannelId(request.ChannelId, request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);

                if (!found)
                {
                    // Sql paged query for channel profile property.
                    SqlPagedQuery channelProfilePropertyQuery = this.GetChannelProfileQuery(request.ChannelId, QueryResultSettings.AllRecords, ChannelProfilePropertyViewName);
                    PagedResult<ChannelProfileProperty> channelProfileProperties;

                    // Sql paged query for channel profile.
                    SqlPagedQuery channelProfileQuery = this.GetChannelProfileQuery(request.ChannelId, request.QueryResultSettings, ChannelProfileViewName);
                    channelProfileQuery.IsQueryByPrimaryKey = true;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        channelProfileProperties = databaseContext.ReadEntity<ChannelProfileProperty>(channelProfilePropertyQuery);
                        channelProfile = databaseContext.ReadEntity<ChannelProfile>(channelProfileQuery).Results.SingleOrDefault();
                    }

                    if (channelProfile != null)
                    {
                        channelProfile.SetProfileProperties(channelProfileProperties.Results);
                    }

                    updateL2Cache &= channelProfile != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelProfileByChannelId(request.ChannelId, request.QueryResultSettings.ColumnSet, channelProfile);
                }

                return new SingleEntityDataServiceResponse<ChannelProfile>(channelProfile);
            }

            private SqlPagedQuery GetChannelProfileQuery(long channelId, QueryResultSettings settings, string viewName)
            {
                var query = new SqlPagedQuery(settings)
                {
                    Select = settings.ColumnSet,
                    From = viewName,
                    Where = "CHANNEL = " + SqlParamChannelId,
                    OrderBy = RecIdColumn
                };

                query.Parameters[SqlParamChannelId] = channelId;

                return query;
            }

            /// <summary>
            /// Gets the channel-specific properties.
            /// </summary>
            /// <param name="request">The get channel properties by channel id data request.</param>
            /// <returns>
            /// A collections of channel properties.
            /// </returns>
            private EntityDataServiceResponse<ChannelProperty> GetChannelPropertiesByChannelId(GetChannelPropertiesByChannelIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<ChannelProperty> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelPropertiesByChannelId(request.ChannelId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = ChannelPropertiesViewName,
                        Where = "CHANNEL = " + SqlParamChannelId,
                        IsQueryByPrimaryKey = false,
                        OrderBy = RecIdColumn
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ChannelProperty>(query);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelPropertiesByChannelId completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelPropertiesByChannelId(request.ChannelId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<ChannelProperty>(result);
            }

            /// <summary>
            /// Gets the channel languages.
            /// </summary>
            /// <param name="request">The get channel languages by channel id data request.</param>
            /// <returns>
            /// A collection of channel languages.
            /// </returns>
            private EntityDataServiceResponse<ChannelLanguage> GetChannelLanguagesByChannelId(GetChannelLanguagesByChannelIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<ChannelLanguage> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelLanguagesByChannelId(request.ChannelId, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = CurrentChannelLanguagesName,
                        Where = "CHANNEL = " + SqlParamChannelId,
                        OrderBy = LanguageIdColumn,
                        IsQueryByPrimaryKey = false,
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ChannelLanguage>(query);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetChannelLanguagesByChannelId completed in {0} ms", processTimer.ElapsedMilliseconds);
                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelLanguagesByChannelId(request.ChannelId, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<ChannelLanguage>(result);
            }

            /// <summary>
            /// Get the the transaction service profile.
            /// </summary>
            /// <param name="request">Get transaction service profile data request.</param>
            /// <returns>Instance of <see cref="TransactionServiceProfile"/>.</returns>
            private SingleEntityDataServiceResponse<TransactionServiceProfile> GetTransactionServiceProfile(GetTransactionServiceProfileDataRequest request)
            {
                TransactionServiceProfile profile;

                if (request.ChannelId == null)
                {
                    profile = this.GetDefaultTransactionServiceProfile(request.RequestContext);
                }
                else
                {
                    // Caching is not required in this case because result is already cached as part of ChannelConfiguration object.
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = TransactionServiceProfileViewName,
                        IsQueryByPrimaryKey = true,
                        Where = "CHANNELID = " + SqlParamChannelId,
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        profile = databaseContext.ReadEntity<TransactionServiceProfile>(query).SingleOrDefault();
                    }
                }

                return new SingleEntityDataServiceResponse<TransactionServiceProfile>(profile);
            }

            private TransactionServiceProfile GetDefaultTransactionServiceProfile(RequestContext context)
            {
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();

                TransactionServiceProfile profile = null;
                if (channelConfiguration != null)
                {
                    profile = channelConfiguration.TransactionServiceProfile;
                }

                if (!TransactionServiceProfile.HasValidProfileId(profile))
                {
                    ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(context);

                    bool found;
                    bool updateL2Cache;
                    TransactionServiceProfile result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetDefaultTransactionServiceProfile(), out found, out updateL2Cache);

                    if (!found)
                    {
                        Stopwatch processTimer = Stopwatch.StartNew();

                        var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                        {
                            From = DefaultTransactionServiceProfileViewName,
                            IsQueryByPrimaryKey = true,
                            OrderBy = "TSPROFILEID"
                        };

                        using (DatabaseContext databaseContext = new DatabaseContext(context))
                        {
                            result = databaseContext.ReadEntity<TransactionServiceProfile>(query).SingleOrDefault();
                        }

                        if (result == null)
                        {
                            throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration, ExceptionSeverity.Warning, "No default transaction service profile was found.");
                        }

                        processTimer.Stop();
                        NetTracer.Information("** timer info: GetDefaultTransactionServiceProfile completed in {0} ms", processTimer.ElapsedMilliseconds);

                        updateL2Cache &= result != null;
                    }

                    if (updateL2Cache)
                    {
                        level2CacheDataAccessor.PutDefaultTransactionServiceProfile(result);
                    }

                    profile = result;
                }

                if (!TransactionServiceProfile.HasValidProfileId(profile))
                {
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration, ExceptionSeverity.Warning, "Can not find the real-time service profile.");
                }

                return profile;
            }

            /// <summary>
            /// Gets the currencies.
            /// </summary>
            /// <param name="request">The get currencies data request.</param>
            /// <returns>
            /// A collection of currencies.
            /// </returns>
            private EntityDataServiceResponse<Currency> GetCurrencies(GetCurrenciesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<Currency> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetCurrencies(request.CurrencyCode, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = CurrenciesViewName,
                        IsQueryByPrimaryKey = false,
                        OrderBy = "CURRENCYCODE",
                    };

                    if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
                    {
                        query.Where = "CURRENCYCODE = @CurrencyCode";
                        query.Parameters["@CurrencyCode"] = request.CurrencyCode;
                    }

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<Currency>(query);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetCurrencies completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                foreach (Currency currency in result.Results)
                {
                    currency.NumberOfDecimals = this.GetNumberOfDecimals(currency.RoundOffPrice);
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutCurrencies(request.CurrencyCode, request.QueryResultSettings, result);
                }

                return new EntityDataServiceResponse<Currency>(result);
            }

            /// <summary>
            /// Get the No.Of decimals in currency amount.
            /// </summary>
            /// <param name="roundOffValue">Round Off Price Value of Currency.</param>
            /// <returns>No. Of Decimals in currency amount.</returns>
            private short GetNumberOfDecimals(decimal roundOffValue)
            {
                short decimalCount = 0;

                if (roundOffValue == 0m)
                {
                    roundOffValue = DefaultRoundingValue;
                }

                // If value is not equal to zero, multiply by 10 until greater than zero, counting as we go
                while (roundOffValue != 0)
                {
                    // Remove any whole numbers
                    roundOffValue = roundOffValue % 1.0M;

                    // If we are still non-zero, it is fractional
                    if (roundOffValue != 0)
                    {
                        // Count and shift right
                        decimalCount++;
                        roundOffValue *= 10M;
                    }
                }

                return decimalCount;
            }

            /// <summary>
            /// Gets the cache accessor for the channel data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="ChannelL2CacheDataStoreAccessor"/> class.</returns>
            private ChannelL2CacheDataStoreAccessor GetChannelL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new ChannelL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }

            private void BuildWhereClauseForItemIdBarcodeSerialNumber(TransactionSearchCriteria criteria, SqlPagedQuery query, IList<string> whereClauses, IDatabaseQueryBuilder databaseQueryBuilder)
            {
                // If an ItemId, Barcode or SerialNumber was specified we need to check whether a Sales Line exists that meets the criteria
                if (!string.IsNullOrEmpty(criteria.ItemId) || !string.IsNullOrEmpty(criteria.Barcode) || !string.IsNullOrEmpty(criteria.SerialNumber))
                {
                    if (!string.IsNullOrEmpty(criteria.ItemId))
                    {
                        whereClauses.Add(string.Format("{0} = @itemId", ItemIdColumn));
                        query.Parameters["@itemId"] = criteria.ItemId.Trim();
                    }

                    if (!string.IsNullOrEmpty(criteria.Barcode))
                    {
                        whereClauses.Add(string.Format("{0} = @barcode", BarcodeColumn));
                        query.Parameters["@barcode"] = criteria.Barcode.Trim();
                    }

                    if (!string.IsNullOrEmpty(criteria.SerialNumber))
                    {
                        whereClauses.Add(string.Format("{0} = @serial", InventSerialIdColumn));
                        query.Parameters["@serial"] = criteria.SerialNumber.Trim();
                    }

                    var salesLineClause = string.Join(" AND ", whereClauses);
                    whereClauses.Clear();

                    var existsQuery = new SqlPagedQuery(QueryResultSettings.AllRecords)
                    {
                        Select = new ColumnSet(TransactionIdColumn),
                        From = RetailTransactionSalesTransView,
                        Where = salesLineClause,
                        IsQueryByPrimaryKey = false,
                    };

                    string existsQuerySql = existsQuery.BuildQuery(databaseQueryBuilder);

                    whereClauses.Add(string.Format("{0} IN ({1})", TransactionIdColumn, existsQuerySql));
                }
            }

            /// <summary>
            /// Builds the WHERE clause from the search criteria for Orders.
            /// The result is the AND of the following non-empty parameters for the RetailTransactionView: ReceiptId, CustomerAccountNumber, FirstName, LastName, Store, Terminal, StaffId, StartDateTime, EndDateTime
            /// and the following non-empty parameters for the RetailTransactionSalesTransView: ItemId, Barcode.
            /// </summary>
            /// <param name="criteria">Search criteria.</param>
            /// <param name="query">The SQL query.</param>
            /// <param name="whereClauses">Where clauses to build.</param>
            private void BuildSearchOrderWhereClause(TransactionSearchCriteria criteria, SqlPagedQuery query, IList<string> whereClauses)
            {
                ThrowIf.Null(criteria, "criteria");
                ThrowIf.Null(query, "query");
                ThrowIf.Null(whereClauses, "whereClauses");

                if (!string.IsNullOrEmpty(criteria.ReceiptId))
                {
                    whereClauses.Add(string.Format("{0} = @receiptId", ReceiptIdColumn));
                    query.Parameters["@receiptId"] = criteria.ReceiptId.Trim();
                }

                if (!string.IsNullOrEmpty(criteria.ChannelReferenceId))
                {
                    whereClauses.Add(string.Format("{0} = @channelReferenceId", ChannelReferenceIdColumn));
                    query.Parameters["@channelReferenceId"] = criteria.ChannelReferenceId.Trim();
                }

                if (!string.IsNullOrEmpty(criteria.CustomerAccountNumber))
                {
                    whereClauses.Add(string.Format("{0} = @custAccount", RetailTransactionTableSchema.CustomerIdColumn));
                    query.Parameters["@custAccount"] = criteria.CustomerAccountNumber.Trim();
                }

                if (!string.IsNullOrEmpty(criteria.StoreId))
                {
                    whereClauses.Add(string.Format("{0} = @storeId", RetailTransactionTableSchema.StoreColumn));
                    query.Parameters["@storeId"] = criteria.StoreId.Trim();
                }

                if (!string.IsNullOrEmpty(criteria.TerminalId))
                {
                    whereClauses.Add(string.Format("{0} = @terminalId", RetailTransactionTableSchema.TerminalColumn));
                    query.Parameters["@terminalId"] = criteria.TerminalId.Trim();
                }

                if (!string.IsNullOrEmpty(criteria.StaffId))
                {
                    whereClauses.Add(string.Format("{0} = @staffId", StaffColumn));
                    query.Parameters["@staffId"] = criteria.StaffId.Trim();
                }

                if (!string.IsNullOrEmpty(criteria.SalesId))
                {
                    whereClauses.Add(string.Format("{0} = @salesId", SalesIdColumn));
                    query.Parameters["@salesId"] = criteria.SalesId.Trim();
                }

                if (criteria.StartDateTime != null)
                {
                    whereClauses.Add(string.Format("{0} >= @startDate", RetailTransactionTableSchema.CreatedDateTimeColumn));
                    query.Parameters["@startDate"] = criteria.StartDateTime.Value.UtcDateTime;
                }

                if (criteria.EndDateTime != null)
                {
                    whereClauses.Add(string.Format("{0} <= @endDate", RetailTransactionTableSchema.CreatedDateTimeColumn));
                    query.Parameters["@endDate"] = criteria.EndDateTime.Value.UtcDateTime;
                }

                if (criteria.TransactionIds != null && criteria.TransactionIds.Any())
                {
                    query.AddInClause<string>(criteria.TransactionIds.AsReadOnly(), RetailTransactionTableSchema.TransactionIdColumn, whereClauses);
                }

                if (!string.IsNullOrEmpty(criteria.ReceiptEmailAddress))
                {
                    whereClauses.Add(string.Format("({0} = @receiptEmailAddress OR {1} = @receiptEmailAddress)", RetailTransactionTableSchema.ReceiptEmailColumn, EmailColumn));
                    query.Parameters["@receiptEmailAddress"] = criteria.ReceiptEmailAddress.Trim();
                }

                if (!string.IsNullOrEmpty(criteria.SearchIdentifiers))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("({0} = @searchIdentifiers", RetailTransactionTableSchema.TransactionIdColumn);
                    sb.AppendFormat(" OR {0} = @searchIdentifiers", RetailTransactionTableSchema.ReceiptIdColumn);
                    sb.AppendFormat(" OR {0} = @searchIdentifiers", RetailTransactionTableSchema.CustomerIdColumn);
                    sb.AppendFormat(" OR {0} = @searchIdentifiers)", RetailTransactionTableSchema.ChannelReferenceIdColumn);

                    whereClauses.Add(sb.ToString());
                    query.Parameters["@searchIdentifiers"] = criteria.SearchIdentifiers.Trim();
                }

                query.Where = string.Join(" AND ", whereClauses);
            }

            /// <summary>
            /// Search store information by using search criteria.
            /// </summary>
            /// <param name="request">Search org unit data request.</param>
            /// <returns>Returns the store collection.</returns>
            private EntityDataServiceResponse<OrgUnit> SearchOrgUnitByGivenCriteria(SearchOrgUnitDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
                PagedResult<OrgUnit> result = null;

                if (request.StoreSearchCriteria != null)
                {
                    // Search when store search criteria is given.
                    result = this.SearchOrgUnitByStoreSearchCriteria(request);
                }
                else if (request.ChannelId != null)
                {
                    // Search when channel id is given.
                    OrgUnit store = this.GetStoreByChannelId(request);

                    var storeList = new List<OrgUnit> { store };
                    result = storeList.AsPagedResult();
                }
                else if (request.StoreNumbers != null)
                {
                    // Serach when store numbers are provided.
                    result = this.GetStoresbyStoreNumbers(request);
                }
                            
                return new EntityDataServiceResponse<OrgUnit>(result);
            }

            /// <summary>
            /// Gets a collection of available stores.
            /// </summary>
            /// <param name="request">The data request.</param>
            /// <returns>
            /// The stores associated with the current channel.
            /// </returns>
            private PagedResult<OrgUnit> GetStoresbyStoreNumbers(SearchOrgUnitDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.StoreNumbers, "request.StoreNumbers");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;

                // Order store numbers for pagination purposes - take distinct ones
                var distinctStoreNumbers = request.StoreNumbers.Distinct().OrderBy(s => s).ToList();

                PagedResult<OrgUnit> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetStoresByStoreNumbers(distinctStoreNumbers, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    // Default query.
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = OrgUnitsView,
                        IsQueryByPrimaryKey = false,
                        OrderBy = "STORENUMBER"
                    };

                    if (request.StoreNumbers.Any())
                    {
                        // Get stores by given store numbers.
                        using (StringIdTableType storeNumbersTable = new StringIdTableType(distinctStoreNumbers, "STORENUMBER"))
                        {
                            query.Parameters["@TVP_STORENUMBERS"] = storeNumbersTable;

                            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                            {
                                result = databaseContext.ReadEntity<OrgUnit>(query);
                            }
                        }
                    }
                    else
                    {
                        // Get all stores.
                        using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                        {
                            result = databaseContext.ReadEntity<OrgUnit>(query);
                        }
                    }

                    PopulateOrgUnitsAddress(request.RequestContext, result.Results);

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetStores completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutStores(request.QueryResultSettings, result);
                }

                return result;
            }

            /// <summary>
            /// Search store information by using search criteria.
            /// </summary>
            /// <param name="request">Search org unit data request.</param>
            /// <returns>Returns the store collection.</returns>
            private PagedResult<OrgUnit> SearchOrgUnitByStoreSearchCriteria(SearchOrgUnitDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.StoreSearchCriteria, "request.StoreSearchCriteria");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                PagedResult<OrgUnit> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.SearchOrgUnit(request.StoreSearchCriteria, request.QueryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    // Invoke store information view
                    var storeQuery = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = OrgUnitsView,
                        IsQueryByPrimaryKey = false,
                        OrderBy = RecIdColumn
                    };

                    var storeQueryFilter = this.SetFilterClause(request.StoreSearchCriteria, storeQuery, false);
                    storeQuery.Where = string.Join(" AND ", storeQueryFilter);
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<OrgUnit>(storeQuery);
                    }

                    if (result.Results.Any())
                    {
                        PopulateOrgUnitsAddress(request.RequestContext, result.Results);
                    }
                    else
                    {
                        // Search online information only if no stores are found for the given search query.
                        var onlineQuery = new SqlPagedQuery(request.QueryResultSettings)
                        {
                            From = OnlineChannelViewName,
                            IsQueryByPrimaryKey = false,
                        };

                        var onlineQueryFilter = this.SetFilterClause(request.StoreSearchCriteria, onlineQuery, true);
                        onlineQuery.Where = string.Join(" AND ", onlineQueryFilter);
                        PagedResult<OnlineChannel> onlineChannels;

                        using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                        {
                            onlineChannels = databaseContext.ReadEntity<OnlineChannel>(onlineQuery);
                        }

                        // Merge the online channel into OrgUnit.
                        var orgUnitList = new List<OrgUnit>();

                        foreach (var onlineChannel in onlineChannels.Results)
                        {
                            var orgUnit = new OrgUnit(onlineChannel.RecordId)
                            {
                                OrgUnitType = onlineChannel.OrgUnitType,
                                OrgUnitNumber = onlineChannel.InventoryLocationId,
                                OrgUnitName = onlineChannel.OnlineChannelName
                            };

                            orgUnitList.Add(orgUnit);
                        }

                        result = orgUnitList.AsPagedResult();
                    }

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutSearchOrgUnit(request.StoreSearchCriteria, request.QueryResultSettings, result);
                }

                return result;
            }

            /// <summary>
            /// Gets the device by device identifier.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private SingleEntityDataServiceResponse<Device> GetDeviceByDeviceId(GetDeviceDataRequest request)
            {
                DeviceL2CacheDataStoreAccessor levelL2CacheDataAccessor = this.GetDeviceL2CacheDataStoreAccessor(request.RequestContext);

                Device device;

                if (!levelL2CacheDataAccessor.GetDeviceById(request.DeviceNumber, out device))
                {
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                        {
                            From = DeviceViewName,
                            Where = "DEVICEID = @deviceId",
                            IsQueryByPrimaryKey = true
                        };

                        query.Parameters["@deviceId"] = request.DeviceNumber;

                        if (request.IsActivatedDeviceOnly)
                        {
                            query.Where += " AND ACTIVATIONSTATUS = @activationStatus";
                            query.Parameters["@activationStatus"] = 1;  // 1 represents Activated status
                        }

                        device = databaseContext.ReadEntity<Device>(query).Results.SingleOrDefault();

                        if (device != null)
                        {
                            levelL2CacheDataAccessor.CacheDeviceById(device.DeviceNumber, device);
                        }
                    }
                }

                return new SingleEntityDataServiceResponse<Device>(device);
            }

            /// <summary>
            /// The data service method to execute the data manager to get the channel configuration.
            /// Gets the store by identifier.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private OrgUnit GetStoreByChannelId(SearchOrgUnitDataRequest request)
            {
                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                OrgUnit result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetStoreById((long)request.ChannelId), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        From = OrgUnitsView,
                        Where = "RECID = " + SqlParamChannelId,
                        IsQueryByPrimaryKey = true,
                        OrderBy = RecIdColumn
                    };

                    query.Parameters[SqlParamChannelId] = request.ChannelId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<OrgUnit>(query).SingleOrDefault();
                    }

                    if (result != null)
                    {
                        PopulateOrgUnitAddress(request.RequestContext, result);
                    }

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetStoreById completed in {0} ms", processTimer.ElapsedMilliseconds);

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutStoreById((long)request.ChannelId, result);
                }

                return result;
            }

            /// <summary>
            /// Gets the cache accessor for the device data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="DeviceL2CacheDataStoreAccessor"/> class.</returns>
            private DeviceL2CacheDataStoreAccessor GetDeviceL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new DeviceL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
        }
    }
}
