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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System.Collections.Generic;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Pricing and discount temp tables.
        /// </summary>
        internal class PricingTempTable
        {
            /// <summary>
            /// Creates the price group temp table.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="priceGroups">The price group identifiers.</param>
            /// <returns>Returns the price group instance of the temp table.</returns>
            public static TempTable CreatePriceGroup(SqliteDatabaseContext context, IEnumerable<string> priceGroups)
            {
                const string PriceGroupTempTable = "PRICEGROUPS";
    
                // Create temp table for price groups.
                var priceGroupDataTable = new DataTable(PriceGroupTempTable);
    
                priceGroupDataTable.Columns.Add("RECID", typeof(long));
                priceGroupDataTable.Columns.Add("PRICEGROUP", typeof(long));
                priceGroupDataTable.Columns.Add("GROUPID", typeof(string));
    
                foreach (var pg in priceGroups)
                {
                    priceGroupDataTable.Rows.Add(0, 0, pg);
                }
    
                return context.CreateTemporaryTable(priceGroupDataTable);
            }
    
            /// <summary>
            /// Creates the affiliation price group temp table.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="affiliationPriceGroups">The affiliation price groups.</param>
            /// <returns>Returns the affiliation price groups.</returns>
            public static TempTable CreateAffiliationPriceGroup(SqliteDatabaseContext context, IEnumerable<AffiliationLoyaltyTier> affiliationPriceGroups)
            {
                const string AffiliationPriceGroupTempTable = "AFFILIATIONLOYALTYTIERS";
    
                // Create temp table for price groups.
                var affiliationPriceGroupDataTable = new DataTable(AffiliationPriceGroupTempTable);
    
                affiliationPriceGroupDataTable.Columns.Add("AFFILIATIONID", typeof(long));
                affiliationPriceGroupDataTable.Columns.Add("LOYALTYTIERID", typeof(long));
    
                foreach (var pg in affiliationPriceGroups)
                {
                    affiliationPriceGroupDataTable.Rows.Add(pg.AffiliationId, pg.LoyaltyTierId);
                }
    
                return context.CreateTemporaryTable(affiliationPriceGroupDataTable);
            }
    
            /// <summary>
            /// Creates the temp table for item units.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="itemUnits">The item unit collection.</param>
            /// <returns>Returns the temp table instance of item units.</returns>
            public static TempTable CreateItemIdentifier(SqliteDatabaseContext context, IEnumerable<ItemUnit> itemUnits)
            {
                const string ItemIdTempTable = "ITEMIDS";
    
                // Create temp table for item identifiers.
                var itemIdDataTable = new DataTable(ItemIdTempTable);
    
                itemIdDataTable.Columns.Add("ITEMID", typeof(string));
                itemIdDataTable.Columns.Add("VARIANTINVENTDIMID", typeof(string));
                itemIdDataTable.Columns.Add("PRODUCT", typeof(long));
                itemIdDataTable.Columns.Add("DISTINCTPRODUCTVARIANT", typeof(long));
    
                foreach (var item in itemUnits)
                {
                    itemIdDataTable.Rows.Add(item.ItemId, item.VariantInventoryDimensionId, item.Product, item.DistinctProductVariant);
                }
    
                return context.CreateTemporaryTable(itemIdDataTable);
            }
    
            /// <summary>
            /// Creates the temp table for account relation.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="accountRelations">The account relation identifiers.</param>
            /// <returns>Returns the instance of account relation temp table.</returns>
            public static TempTable CreateAccountRelation(SqliteDatabaseContext context, IEnumerable<string> accountRelations)
            {
                const string AccountRelationTempTable = "ACCOUNTRELATIONS";
    
                // Create temp table for account relations.
                var accountRelationsDataTable = new DataTable(AccountRelationTempTable);
    
                accountRelationsDataTable.Columns.Add("ACCOUNTRELATION", typeof(string));
    
                foreach (var accountRelation in accountRelations)
                {
                    accountRelationsDataTable.Rows.Add(accountRelation);
                }
    
                return context.CreateTemporaryTable(accountRelationsDataTable);
            }
    
            /// <summary>
            /// Creates the temp table for item line discount.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="itemIds">The item identifiers.</param>
            /// <returns>Returns the instance of item line discount temp table.</returns>
            public static TempTable CreateItemLineDiscount(SqliteDatabaseContext context, IEnumerable<string> itemIds)
            {
                const string ItemLineDiscountGroupTempTableName = "ITEMLINEDISCGROUPS";
    
                // Create temp table for item line discount groups.
                var itemLineDiscountGroupsDataTable = new DataTable(ItemLineDiscountGroupTempTableName);
    
                itemLineDiscountGroupsDataTable.Columns.Add("LINEDISC", typeof(string));
    
                TempTable itemLineDiscountGroupsTempTable = context.CreateTemporaryTable(itemLineDiscountGroupsDataTable);
    
                using (TempTable items = TempTableHelper.CreateScalarTempTable(context, "RECID", itemIds))
                {
                    const string InsertQuery = @"INSERT INTO {0} (LINEDISC)
                                            SELECT DISTINCT it.LINEDISC FROM [ax].[INVENTTABLEMODULE] it
                                            INNER JOIN {1} i ON it.ITEMID = i.RECID
    		                                WHERE it.MODULETYPE = 2 AND it.DATAAREAID = @nvc_DataAreaId";
    
                    var sqlQuery = new SqlQuery(InsertQuery, itemLineDiscountGroupsTempTable.TableName, items.TableName);
                    sqlQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
    
                    context.ExecuteNonQuery(sqlQuery);
                }
    
                return itemLineDiscountGroupsTempTable;
            }
    
            /// <summary>
            /// Creates the temp table for multiline discount.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="itemIds">The item identifiers.</param>
            /// <returns>Returns the instance of multiline discount temp table.</returns>
            public static TempTable CreateMultiLineDiscount(SqliteDatabaseContext context, IEnumerable<string> itemIds)
            {
                const string ItemMultiLineDiscountGroups = "ItemMultilineDiscGroups";
    
                // Create temp table for item line discount groups.
                var itemMultiLineDiscountGroupsDataTable = new DataTable(ItemMultiLineDiscountGroups);
    
                itemMultiLineDiscountGroupsDataTable.Columns.Add("MULTILINEDISC", typeof(string));
    
                TempTable itemLineDiscountGroupsTempTable = context.CreateTemporaryTable(itemMultiLineDiscountGroupsDataTable);
    
                using (TempTable items = TempTableHelper.CreateScalarTempTable(context, "RECID", itemIds))
                {
                    const string InsertQuery = @"INSERT INTO {0}
                                                (MULTILINEDISC)
                                                SELECT DISTINCT it.MULTILINEDISC FROM [ax].[INVENTTABLEMODULE] it
                                                INNER JOIN {1} i ON it.ITEMID = i.RECID
    		                                    WHERE it.MODULETYPE = 2 AND it.DATAAREAID = @nvc_DataAreaId";
    
                    var sqlQuery = new SqlQuery(InsertQuery, itemLineDiscountGroupsTempTable.TableName, items.TableName);
                    sqlQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
    
                    context.ExecuteNonQuery(sqlQuery);
                }
    
                return itemLineDiscountGroupsTempTable;
            }
    
            /// <summary>
            /// Creates the temp table for customer price groups.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="customer">The customer account.</param>
            /// <returns>Returns the instance of customer price group temp table.</returns>
            public static TempTable CreateCustomerPriceGroups(SqliteDatabaseContext context, string customer)
            {
                const string CustomerPriceGroupTempTableName = "CUSTOMERPRICEGROUPS";
    
                // Create temp table for customer price groups.
                var customerPriceGroupDataTable = new DataTable(CustomerPriceGroupTempTableName);
    
                customerPriceGroupDataTable.Columns.Add("LINEDISC", typeof(string));
                customerPriceGroupDataTable.Columns.Add("MULTILINEDISC", typeof(string));
                customerPriceGroupDataTable.Columns.Add("ENDDISC", typeof(string));
    
                var customerPriceGroupTempTable = context.CreateTemporaryTable(customerPriceGroupDataTable);
    
                const string InsertQuery = @"INSERT INTO {0} (LINEDISC, MULTILINEDISC, ENDDISC)
                                            SELECT LINEDISC, MULTILINEDISC, ENDDISC
                                            FROM [ax].CUSTTABLE WHERE ACCOUNTNUM = @Customer AND DATAAREAID = @nvc_DataAreaId";
    
                var sqlQuery = new SqlQuery(InsertQuery, customerPriceGroupTempTable.TableName);
    
                sqlQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
                sqlQuery.Parameters["@Customer"] = customer;
    
                context.ExecuteNonQuery(sqlQuery);
    
                return customerPriceGroupTempTable;
            }
        }
    }
}
