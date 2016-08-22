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
    namespace Retail.Ecommerce.Sdk.Core.Publishing
    {
        using System;
        using System.Collections.Generic;
        using System.Data;
        using System.Data.SqlClient;
        using System.Diagnostics;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Implements the logic of retrieving and storing a product catalog, as well as the functionality to persist and
        /// retrieve the catalog from the storage proper.
        /// </summary>
        public class DataAccessor
        {
            #region constants
            /// <summary>
            /// Defines field name ListId.
            /// </summary>
            public static readonly string ListIdFieldName = "LISTID";
            private const string TagFieldName = "TAG";
            private const string ProductIdFieldName = "PRODUCTID";
            private const string ListingIdFieldName = "LISTINGRECID";
            private const string ListingLanguageIdFieldName = "LANGUAGEID";
            private const string ListingCatalogIdFieldName = "CATALOGID";
            private const string CatalogIdFieldName = "CATALOGID";
            private const string ColumnRecId = "RECID";
            private const int LanguageIdFieldSize = 7;
    
            private const string GetAllListingsSProcName = "[crt].GETALLLISTINGSMAP";
            private const string StoreListingMapSProcName = "[crt].STORELISTINGMAP";
            private const string GetNotExistingCatalogsSProcName = "[crt].GETNOTEXISTINGCATALOGS";
            private const string GetNotExistingLanguagesSProcName = "[crt].GETNOTEXISTINGLANGUAGES";
            private const string DeleteListingsByCatalogsSProcName = "[crt].DELETELISTINGSBYCATALOGS";
            private const string DeleteListingsByLanguagesSProcName = "[crt].DELETELISTINGSBYLANGUAGES";
            private const string DeleteListingsByCompositeIdsSProcName = "[crt].DELETELISTINGSBYCOMPOSITEIDS";
            #endregion
    
            #region members
            private readonly string connectionStringValue;                                  // connection string to the local channel storage
            private readonly int maxPageSize;
            private long channelIdValue;
            #endregion members
    
            #region constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="DataAccessor"/> class.
            /// Creates an object of type DataAccessor, with the specified storage connection string.
            /// </summary>
            /// <param name="channelId">Channel id for channel.</param>
            /// <param name="connectionString">Channel storage connection string.</param>
            /// <param name="maxPageSize">Max Page Size.</param>
            public DataAccessor(long channelId, string connectionString, int maxPageSize)
            {
                if (channelId == 0)
                {
                    throw new ArgumentException("channelId can not be 0", nameof(channelId));
                }
    
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentNullException(nameof(connectionString));
                }
    
                if (maxPageSize < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(maxPageSize));
                }
    
                this.connectionStringValue = connectionString;
                this.maxPageSize = maxPageSize;
    
                this.channelIdValue = channelId;
            }
    
            #endregion constructors
    
            /// <summary>
            /// Returns RecId table type.
            /// </summary>
            /// <returns>Returns DataTable.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of the method is to return valid object which may not be disposed at this time.")]
            public static DataTable GetRecIdTableType()
            {
                DataTable listingIdTable = new DataTable("RECORDIDTABLETYPE");
                listingIdTable.Locale = CultureInfo.InvariantCulture;
                listingIdTable.Columns.Add(ColumnRecId, typeof(long));
                return listingIdTable;
            }
    
            /// <summary>
            /// Returns Composite Id table type.
            /// </summary>
            /// <returns>Return data table.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of the method is to return valid object which may not be disposed at this time.")]
            public static DataTable GetCompositeIdTableType()
            {
                DataTable idTable = new DataTable("RECORDIDLANGUAGETABLETYPE");
                idTable.Locale = CultureInfo.InvariantCulture;
                idTable.Columns.Add(ColumnRecId, typeof(long));
                idTable.Columns.Add(ListingLanguageIdFieldName, typeof(string)).MaxLength = LanguageIdFieldSize;
                return idTable;
            }
    
            /// <summary>
            /// This function delete listings by composite ids.
            /// </summary>
            /// <param name="catalogId">Accepts catalog id.</param>
            /// <param name="ids">Accept ids.</param>
            public void DeleteListingsByCompositeIds(long catalogId, IEnumerable<ListingIdentity> ids)
            {
                if (ids == null)
                {
                    throw new ArgumentNullException(nameof(ids));
                }
    
                if (!ids.Any())
                {
                    return;
                }
    
                using (DataTable catalogIdsTable = GetCompositeIdTableType())
                {
                    foreach (ListingIdentity id in ids)
                    {
                        catalogIdsTable.Rows.Add(id.ProductId, id.LanguageId);
                    }
    
                    using (SqlConnection connection = new SqlConnection(this.connectionStringValue))
                    {
                        using (SqlCommand command = new SqlCommand(DeleteListingsByCompositeIdsSProcName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@bi_ChannelId", this.channelIdValue);
                            command.Parameters.AddWithValue("@bi_CatalogId", catalogId);
                            command.Parameters.AddWithValue("@tvp_ProductIds", catalogIdsTable);
    
                            connection.Open();
    
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
    
            /// <summary>
            /// Returns list of catalogs which exists in SharePoint but doesn't exist in CRT.
            /// </summary>
            /// <param name="existingCatalogs">The list of existing CRT catalogs.</param>
            /// <returns>The list of catalogs which exists in SharePoint but doesn't exist in CRT. The list is grouped by SP List name.</returns>
            internal Dictionary<string, List<long>> GetNotExistingCatalogs(IEnumerable<ProductCatalog> existingCatalogs)
            {
                using (DataTable catalogIdsTable = GetRecIdTableType())
                {
                    // Populate TVP with catalog IDs.
                    foreach (ProductCatalog existingCatalog in existingCatalogs)
                    {
                        catalogIdsTable.Rows.Add(existingCatalog.RecordId);
                    }
    
                    using (SqlConnection connection = new SqlConnection(this.connectionStringValue))
                    {
                        using (SqlCommand command = new SqlCommand(GetNotExistingCatalogsSProcName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@bi_ChannelId", this.channelIdValue);
                            command.Parameters.AddWithValue("@tvp_CatalogIds", catalogIdsTable);
    
                            connection.Open();
    
                            Dictionary<string, List<long>> notExistingCatalogsIds = new Dictionary<string, List<long>>();
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string listName = (string)reader[ListIdFieldName];
                                    long notExistingCatalogId = (long)reader[ListingCatalogIdFieldName];
    
                                    // 1. Creating map (TAG and collection of catalogs which are stored for that TAG) if it doesn't exist.
                                    List<long> currentCatalogs;
                                    if (!notExistingCatalogsIds.TryGetValue(listName, out currentCatalogs))
                                    {
                                        currentCatalogs = new List<long>();
                                        notExistingCatalogsIds.Add(listName, currentCatalogs);
                                    }
    
                                    // 2. Adding an item to the map;
                                    currentCatalogs.Add(notExistingCatalogId);
                                }
                            }
    
                            return notExistingCatalogsIds;
                        }
                    }
                }
            }
    
            /// <summary>
            /// Returns list of languages which exists in SharePoint but doesn't exist in CRT.
            /// </summary>
            /// <param name="existingLanguages">The list of existing CRT languages for the channel.</param>
            /// <returns>The list of languages which exists in SharePoint but doesn't exist in CRT. The list is grouped by SP List name.</returns>
            internal Dictionary<string, List<string>> GetNotExistingLanguages(IEnumerable<ChannelLanguage> existingLanguages)
            {
                using (DataTable languageIdsTable = GetLanguageIdTableType())
                {
                    // Populate TVP with language IDs.
                    foreach (ChannelLanguage existingLanguage in existingLanguages)
                    {
                        languageIdsTable.Rows.Add(existingLanguage.LanguageId);
                    }
    
                    using (SqlConnection connection = new SqlConnection(this.connectionStringValue))
                    {
                        using (SqlCommand command = new SqlCommand(GetNotExistingLanguagesSProcName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@bi_ChannelId", this.channelIdValue);
                            command.Parameters.AddWithValue("@tvp_LanguageIds", languageIdsTable);
    
                            connection.Open();
    
                            Dictionary<string, List<string>> notExistingLanguageIds = new Dictionary<string, List<string>>();
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string listName = (string)reader[ListIdFieldName];
                                    string notExistingLanguageId = (string)reader[ListingLanguageIdFieldName];
    
                                    // 1. Creating map (TAG and collection of languages which are stored for that TAG) if it doesn't exist.
                                    List<string> currentLanguages;
                                    if (!notExistingLanguageIds.TryGetValue(listName, out currentLanguages))
                                    {
                                        currentLanguages = new List<string>();
                                        notExistingLanguageIds.Add(listName, currentLanguages);
                                    }
    
                                    // 2. Adding an item to the map;
                                    currentLanguages.Add(notExistingLanguageId);
                                }
                            }
    
                            return notExistingLanguageIds;
                        }
                    }
                }
            }
    
            internal void DeleteListingsByCatalogs(IEnumerable<long> catalogIds)
            {
                if (!catalogIds.Any())
                {
                    return;
                }
    
                using (DataTable catalogIdsTable = GetRecIdTableType())
                {
                    foreach (long catalogId in catalogIds)
                    {
                        catalogIdsTable.Rows.Add(catalogId);
                    }
    
                    using (SqlConnection connection = new SqlConnection(this.connectionStringValue))
                    {
                        using (SqlCommand command = new SqlCommand(DeleteListingsByCatalogsSProcName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@bi_ChannelId", this.channelIdValue);
                            command.Parameters.AddWithValue("@tvp_CatalogIds", catalogIdsTable);
    
                            connection.Open();
    
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
    
            internal void DeleteListingsByLanguages(IEnumerable<string> languageIds)
            {
                if (!languageIds.Any())
                {
                    return;
                }
    
                using (DataTable languageIdsTable = GetLanguageIdTableType())
                {
                    foreach (string languageId in languageIds)
                    {
                        languageIdsTable.Rows.Add(languageId);
                    }
    
                    using (SqlConnection connection = new SqlConnection(this.connectionStringValue))
                    {
                        using (SqlCommand command = new SqlCommand(DeleteListingsByLanguagesSProcName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@bi_ChannelId", this.channelIdValue);
                            command.Parameters.AddWithValue("@tvp_LanguageIds", languageIdsTable);
    
                            connection.Open();
    
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
    
            internal Dictionary<long, List<ListingIdentity>> LoadAllListingsMap()
            {
                using (SqlConnection connection = new SqlConnection(this.connectionStringValue))
                {
                    using (SqlCommand command = new SqlCommand(GetAllListingsSProcName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@bi_ChannelId", this.channelIdValue);
    
                        Dictionary<long, List<ListingIdentity>> ids = new Dictionary<long, List<ListingIdentity>>();
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long productId = (long)reader[ListingIdFieldName];
                                string languageId = (string)reader[ListingLanguageIdFieldName];
                                long catalogId = (long)reader[ListingCatalogIdFieldName];
                                string tag = reader[ListIdFieldName].ToString();
    
                                List<ListingIdentity> currentIds;
    
                                if (!ids.TryGetValue(catalogId, out currentIds))
                                {
                                    currentIds = new List<ListingIdentity>();
                                    ids.Add(catalogId, currentIds);
                                }
    
                                // 2. Adding an item to the map;
                                currentIds.Add(new ListingIdentity
                                {
                                    ProductId = productId,
                                    LanguageId = languageId,
                                    CatalogId = catalogId,
                                    Tag = tag
                                });
                            }
                        }
    
                        return ids;
                    }
                }
            }
    
            /// <summary>
            /// Stores the listing map corresponding to this list.
            /// </summary>
            /// <param name="ids">Ids to be stored.</param>
            internal void StorePublishedIds(IEnumerable<ListingIdentity> ids)
            {
                // string message = string.Format("listing map for list {0}", list.Identifier);
                // NetTracer.Information(Messages.Listing_BeginSaving, message);
    
                // int totalRows = 0;
                Stopwatch timer = Stopwatch.StartNew();
    
                using (DataTable catalogUpsertListingMapTable = CreateDataTable_ListMapping("tvpUpsertListingMap"))
                {
                    int crtPageSize = 0;
                    foreach (ListingIdentity id in ids)
                    {
                        // traverse the list, building the listing map tables
                        DataRow newRow = catalogUpsertListingMapTable.NewRow();
                        newRow[ProductIdFieldName] = id.ProductId;
                        newRow[ListingLanguageIdFieldName] = id.LanguageId;
                        newRow[TagFieldName] = id.Tag;
                        newRow[CatalogIdFieldName] = id.CatalogId;
                        catalogUpsertListingMapTable.Rows.Add(newRow);
    
                        crtPageSize++;
    
                        // update the listing map table if either we have a full page, or both iterators have finished
                        if (crtPageSize >= this.maxPageSize)
                        {
                            this.ExecuteStoreMapping(catalogUpsertListingMapTable);
    
                            // reset the page size and data tables
                            crtPageSize = 0;
                            catalogUpsertListingMapTable.Clear();
                        }
                    }
    
                    // Making sure that we are saving batches wehre number of rows < than max allowed page size.
                    if (crtPageSize > 0)
                    {
                        this.ExecuteStoreMapping(catalogUpsertListingMapTable);
                    }
                } // using tables
    
                timer.Stop();
                //// PublishingEntry.LogTimingMessage(Messages.Listing_EndSaving, message, totalRows, timer.Elapsed);
            }
    
            /// <summary>
            /// Creates a listing map data table.
            /// </summary>
            /// <param name="tableName">Name of table to create.</param>
            /// <returns>An object of the list mapping table type.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller disposes")]
            private static DataTable CreateDataTable_ListMapping(string tableName)
            {
                DataTable catalogListMappingTable = new DataTable(tableName);
                catalogListMappingTable.Locale = CultureInfo.InvariantCulture;
                catalogListMappingTable.Columns.Add(CatalogIdFieldName, typeof(long));
                catalogListMappingTable.Columns.Add(ProductIdFieldName, typeof(long));
                catalogListMappingTable.Columns.Add(ListingLanguageIdFieldName, typeof(string));
                catalogListMappingTable.Columns.Add(TagFieldName, typeof(string));
    
                return catalogListMappingTable;
            }
    
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of the method is to return valid object which may not be disposed at this time.")]
            private static DataTable GetLanguageIdTableType()
            {
                DataTable idTable = new DataTable("LANGUAGEIDTABLETYPE");
                idTable.Locale = CultureInfo.InvariantCulture;
                idTable.Columns.Add(ListingLanguageIdFieldName, typeof(string)).MaxLength = LanguageIdFieldSize;
                return idTable;
            }
    
            private void ExecuteStoreMapping(DataTable catalogUpsertListingMapTable)
            {
                using (SqlConnection connection = new SqlConnection(this.connectionStringValue))
                {
                    using (SqlCommand command = new SqlCommand(StoreListingMapSProcName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@bi_ChannelId", this.channelIdValue);
                        command.Parameters.AddWithValue("@tvp_UpsertListingMap", catalogUpsertListingMapTable);
    
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
