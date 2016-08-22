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
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// The pricing procedure class contains the pricing SQLite queries.
        /// </summary>
        internal static class PricingProcedure
        {
            /// <summary>
            /// Gets the price group collection for the given list of price groups.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="priceGroups">The price group identifiers.</param>
            /// <returns>Returns the read-only collection of price group.</returns>
            public static ReadOnlyCollection<PriceGroup> GetPriceGroup(SqliteDatabaseContext context, IEnumerable<string> priceGroups)
            {
                using (TempTable priceGroupTempTable = PricingTempTable.CreatePriceGroup(context, priceGroups))
                {
                    const string PriceGroupQueryText = @"SELECT (CASE(pa.PRICEGROUP)
                                                    WHEN 0
                                                    THEN pg.RECID
                                                    ELSE pa.PRICEGROUP
                                                    END) PRICEGROUP
                                                FROM {0} AS pa
                                                INNER JOIN [ax].PRICEDISCGROUP pg ON pg.GROUPID = pa.GROUPID AND DATAAREAID = @nvc_DataAreaId";
    
                    var priceGroupQuery = new SqlQuery(PriceGroupQueryText, priceGroupTempTable.TableName);
                    priceGroupQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
    
                    return context.ReadEntity<PriceGroup>(priceGroupQuery).Results;
                }
            }
    
            /// <summary>
            /// Gets the affiliation price groups.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="affiliationPriceGroups">The affiliation price groups.</param>
            /// <returns>Returns the affiliation price groups.</returns>
            public static PagedResult<PriceGroup> GetAffiliationPriceGroups(SqliteDatabaseContext context, IEnumerable<AffiliationLoyaltyTier> affiliationPriceGroups)
            {
                using (TempTable priceGroupTempTable = PricingTempTable.CreateAffiliationPriceGroup(context, affiliationPriceGroups))
                {
                    const string AffiliationPriceGroupQueryText = @"SELECT pdg.GROUPID
                                                            ,rapg.PRICEDISCGROUP AS 'PRICEGROUP'
                                                            ,pdg.RETAILPRICINGPRIORITYNUMBER AS 'RETAILPRICINGPRIORITYNUMBER'
    		                                                ,rapg.RECID
                                                            FROM [ax].PRICEDISCGROUP pdg
                                                            INNER JOIN [ax].RETAILAFFILIATIONPRICEGROUP rapg ON rapg.PRICEDISCGROUP = pdg.RECID
                                                            INNER JOIN {0} alt
                                                                ON rapg.RETAILAFFILIATION = alt.AFFILIATIONID AND (rapg.RETAILLOYALTYTIER = 0 OR rapg.RETAILLOYALTYTIER = alt.LOYALTYTIERID)
                                                            INNER JOIN [ax].RETAILCHANNELTABLE AS c ON c.INVENTLOCATIONDATAAREAID = pdg.DATAAREAID AND c.RECID = @bi_ChannelId";
    
                    var priceGroupQuery = new SqlQuery(AffiliationPriceGroupQueryText, priceGroupTempTable.TableName);
                    priceGroupQuery.Parameters["@bi_ChannelId"] = context.ChannelId;
    
                    return context.ReadEntity<PriceGroup>(priceGroupQuery);
                }
            }
    
            /// <summary>
            /// Gets the item unit collection for the given price group collection.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="items">The item unit collection.</param>
            /// <returns>Returns the read only collection of item units.</returns>
            public static PagedResult<ItemUnit> GetItemsUnit(SqliteDatabaseContext context, IEnumerable<ItemUnit> items)
            {
                // IEnumerable<long> priceGroupIds = priceGroupCollection.Select(price => price.PriceGroupId);
                using (TempTable priceGroupTempTable = PricingTempTable.CreateItemIdentifier(context, items))
                {
                    const string ItemUnitQueryText = @"SELECT
                                            IFNULL(i.ITEMID, idc.ITEMID) AS 'ITEMID',
                                            idc.INVENTDIMID AS 'VARIANTINVENTDIMID',
                                            IFNULL(i.PRODUCT,0) AS PRODUCT,
                                            IFNULL(idc.DISTINCTPRODUCTVARIANT,0) AS DISTINCTPRODUCTVARIANT
                                        FROM {0} AS it
                                        LEFT JOIN [ax].INVENTTABLE i ON i.PRODUCT = it.PRODUCT AND i.DATAAREAID = @nvc_DataAreaId
                                        LEFT JOIN [ax].INVENTDIMCOMBINATION idc ON idc.DISTINCTPRODUCTVARIANT = it.DISTINCTPRODUCTVARIANT AND idc.DATAAREAID = @nvc_DataAreaId";
    
                    var priceGroupQuery = new SqlQuery(ItemUnitQueryText, priceGroupTempTable.TableName);
                    priceGroupQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
    
                    return context.ReadEntity<ItemUnit>(priceGroupQuery);
                }
            }
    
            /// <summary>
            /// Gets the retail price adjustments.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="itemUnits">The item units.</param>
            /// <param name="priceGroupCollection">The price group collection.</param>
            /// <param name="fromDate">From date time of the retail price adjustments.</param>
            /// <param name="toDate">To date time of the retail price adjustments.</param>
            /// <returns>Returns the read only collection of price adjustment.</returns>
            public static PagedResult<PriceAdjustment> GetRetailPriceAdjustments(SqliteDatabaseContext context, IEnumerable<ItemUnit> itemUnits, ReadOnlyCollection<PriceGroup> priceGroupCollection, DateTime fromDate, DateTime toDate)
            {
                IEnumerable<long> priceGroupIds = priceGroupCollection.Select(price => price.PriceGroupId);
    
                const string PriceAdjustmentQueryText = @"SELECT DISTINCT
                                                promo.OFFERID AS OFFERID,
                                                promo.VALIDFROM AS VALIDFROM,
                                                promo.VALIDTO AS VALIDTO,
                                                promo.CONCURRENCYMODE AS CONCURRENCYMODE,
                                                promo.PRICINGPRIORITYNUMBER AS PRICINGPRIORITYNUMBER,
                                                promo.DATEVALIDATIONTYPE AS DATEVALIDATIONTYPE,
                                                promo.VALIDATIONPERIODID AS VALIDATIONPERIODID,
                                                promo.CURRENCYCODE,
                                                promoOffer.DISCOUNTMETHOD AS DISCOUNTMETHOD,
                                                promoOffer.OFFERPRICE AS OFFERPRICE,
                                                promoOffer.DISCPCT AS DISCPCT,
                                                promoOffer.DISCAMOUNT AS DISCAMOUNT,
                                                promoOffer.RECID AS RECID,
                                                it.ITEMID AS ITEMID,
                                                it.VARIANTINVENTDIMID AS INVENTDIMID,
                                                it.DISTINCTPRODUCTVARIANT AS DISTINCTPRODUCTVARIANT,
    		                                    uom.SYMBOL AS SYMBOL,
                                                rgl.VARIANT AS VARIANT,
                                                rgl.PRODUCT AS PRODUCT
                                            FROM [ax].RETAILPERIODICDISCOUNT promo
                                            INNER JOIN [ax].RETAILCHANNELTABLE AS c
                                                ON c.INVENTLOCATIONDATAAREAID = promo.DATAAREAID AND c.RECID = @bi_ChannelId
                                            INNER JOIN [ax].RETAILDISCOUNTPRICEGROUP rdpg on rdpg.OFFERID = promo.OFFERID AND rdpg.DATAAREAID = promo.DATAAREAID
                                            CROSS JOIN {0} pg ON rdpg.PRICEDISCGROUP = pg.RECID
                                            INNER JOIN [ax].RETAILPERIODICDISCOUNTLINE promoLine ON promo.OFFERID = promoLine.OFFERID AND promo.DATAAREAID = promoLine.DATAAREAID
                                            INNER JOIN [ax].RETAILDISCOUNTLINEOFFER promoOffer ON promoLine.RECID = promoOffer.RECID
                                            INNER JOIN [ax].RETAILGROUPMEMBERLINE rgl ON promoLine.RETAILGROUPMEMBERLINE = rgl.RECID
                                            LEFT JOIN  [crt].RETAILPRODUCTORVARIANTCATEGORYANCESTORSVIEW rpca ON rgl.CATEGORY = rpca.CATEGORY
                                            LEFT JOIN [ax].UNITOFMEASURE uom ON uom.RECID = promoLine.UNITOFMEASURE
                                            INNER JOIN {1} it ON
                                                (
                                                    (rgl.VARIANT != 0 AND rgl.VARIANT = it.DISTINCTPRODUCTVARIANT) OR
                                                    (rgl.VARIANT = 0 AND rgl.PRODUCT != 0 AND rgl.PRODUCT = it.PRODUCT) OR
                                                    (rgl.VARIANT = 0 AND rgl.PRODUCT = 0 AND
                                                    (rpca.PRODUCT = it.PRODUCT OR rpca.PRODUCT = it.DISTINCTPRODUCTVARIANT))
                                                )
                                            WHERE promo.STATUS = 1
                                                AND promo.PERIODICDISCOUNTTYPE = 3 -- get only price adjustments
                                                AND (promo.VALIDFROM <= @fromDate OR promo.VALIDFROM <= @NoDate)
                                                AND (promo.VALIDTO >= @toDate OR promo.VALIDTO >= @Never)
    		                                    AND promo.DATAAREAID = @nvc_DataAreaId";
    
                using (TempTable priceGroupTempTable = TempTableHelper.CreateScalarTempTable(context, "RECID", priceGroupIds))
                using (TempTable itemIdTempTable = PricingTempTable.CreateItemIdentifier(context, itemUnits))
                {
                    SqlQuery priceAdjustmentQuery = new SqlQuery(PriceAdjustmentQueryText, priceGroupTempTable.TableName, itemIdTempTable.TableName);
    
                    priceAdjustmentQuery.Parameters["@bi_ChannelId"] = context.ChannelId;
                    priceAdjustmentQuery.Parameters["@NoDate"] = new DateTime(1900, 01, 01);
                    priceAdjustmentQuery.Parameters["@Never"] = new DateTime(2154, 12, 31);
                    priceAdjustmentQuery.Parameters["@fromDate"] = fromDate.Date;
                    priceAdjustmentQuery.Parameters["@toDate"] = toDate.Date;
                    priceAdjustmentQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
    
                    var priceAdjustments = context.ReadEntity<PriceAdjustment>(priceAdjustmentQuery);
    
                    return priceAdjustments;
                }
            }
    
            /// <summary>
            /// Gets the validation periods of the identifiers.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="periodIds">The validation period identifiers.</param>
            /// <returns>Returns the read only collection of validation period.</returns>
            public static PagedResult<ValidationPeriod> GetValidationPeriodsByIds(SqliteDatabaseContext context, IEnumerable<string> periodIds)
            {
                using (TempTable priceGroupTempTable = TempTableHelper.CreateScalarTempTable(context, "RECID", periodIds))
                {
                    const string ValidationPeriodQueryText = @"SELECT * FROM [crt].VALIDATIONPERIODVIEW vp
                                                            JOIN {0} period ON vp.PERIODID = period.RECID
                                                            WHERE vp.CHANNELID = @bi_ChannelId";
    
                    SqlQuery validationPeriodQuery = new SqlQuery(ValidationPeriodQueryText, priceGroupTempTable.TableName);
                    validationPeriodQuery.Parameters["@bi_ChannelId"] = context.ChannelId;
    
                    return context.ReadEntity<ValidationPeriod>(validationPeriodQuery);
                }
            }
    
            /// <summary>
            /// Gets the price trade agreements.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="itemIds">The item identifiers.</param>
            /// <param name="priceGroups">The price group identifiers.</param>
            /// <param name="customerAccount">The customer account.</param>
            /// <param name="fromDate">From date time of the price trade agreement.</param>
            /// <param name="toDate">To date time of the price trade agreement.</param>
            /// <param name="currencyCode">The currency code.</param>
            /// <returns>Returns the read only collection of trade agreement.</returns>
            public static PagedResult<TradeAgreement> GetPriceTradeAgreements(
                                                                                        SqliteDatabaseContext context,
                                                                                        IEnumerable<string> itemIds,
                                                                                        ISet<string> priceGroups,
                                                                                        string customerAccount,
                                                                                        DateTimeOffset fromDate,
                                                                                        DateTimeOffset toDate,
                                                                                        string currencyCode)
            {
                const string PriceTradeAgreementQueryText = @"SELECT
                                                ta.RECID,
                                                ta.ITEMCODE,
                                                ta.ACCOUNTCODE,
                                                ta.ITEMRELATION,
                                                ta.ACCOUNTRELATION,
                                                ta.QUANTITYAMOUNTFROM,
                                                ta.QUANTITYAMOUNTTO,
                                                ta.FROMDATE,
                                                ta.TODATE,
                                                ta.AMOUNT,
                                                ta.CURRENCY,
                                                ta.PERCENT1,
                                                ta.PERCENT2,
                                                ta.SEARCHAGAIN,
                                                ta.PRICEUNIT,
                                                ta.RELATION,
                                                ta.UNITID,
                                                ta.MARKUP,
                                                ta.ALLOCATEMARKUP,
                                                ta.INVENTDIMID,
    		                                    ta.MAXIMUMRETAILPRICE_IN AS MAXIMUMRETAILPRICEINDIA,
                                                invdim.CONFIGID,
                                                invdim.INVENTCOLORID,
                                                invdim.INVENTSIZEID,
                                                invdim.INVENTSTYLEID
                                            FROM [ax].PRICEDISCTABLE ta
    	                                    INNER JOIN [ax].RETAILCHANNELTABLE AS c
    		                                    ON c.INVENTLOCATIONDATAAREAID = ta.DATAAREAID AND c.RECID = @bi_ChannelId
                                            LEFT JOIN [ax].INVENTDIM invdim ON ta.INVENTDIMID = invdim.INVENTDIMID AND invdim.DATAAREAID = c.INVENTLOCATIONDATAAREAID
                                            WHERE
                                                -- agreement is of price sales
                                                ta.RELATION = 4
    		                                    AND ta.CURRENCY = @nvc_CurrencyCode
    
                                                -- and currently active
                                                AND ((ta.FROMDATE <= @MinDate OR ta.FROMDATE <= @NoDate)
                                                        AND (ta.TODATE >= @MaxDate OR ta.TODATE <= @NoDate))
    
                                                -- and customer/group relation matches
                                                AND
                                                (
                                                 -- account code is group and relation is in the price groups
                                                 ((ta.ACCOUNTCODE = 1) AND
                                                    (ta.ACCOUNTRELATION IN (SELECT ar.ACCOUNTRELATION FROM {0} ar)))
                                                 OR
                                                 -- or account code is customer and customer is on the agreement
                                                 ((ta.ACCOUNTCODE = 0) AND (ta.ACCOUNTRELATION = @Customer))
                                                 OR
                                                 -- or account code is ALL customers
                                                 ((ta.ACCOUNTCODE = 2))
                                                )
    
                                                -- and item/group relation matches
                                                AND
                                                (
                                                 -- item code is one of the items passed in
                                                 ((ta.ITEMCODE = 0) AND (ta.ITEMRELATION in (SELECT i.RECID FROM {1} i)))
                                                )
    
    	                                      -- and warehouse is either for all or for current channel
    	                                      AND
    	                                      (
    	                                       invdim.INVENTLOCATIONID = '' OR (invdim.INVENTLOCATIONID = c.INVENTLOCATION)
    	                                      )";
    
                using (TempTable accountRelationTempTable = PricingTempTable.CreateAccountRelation(context, priceGroups))
                using (TempTable itemIdTempTable = TempTableHelper.CreateScalarTempTable(context, "RECID", itemIds))
                {
                    var priceTradeAgreementQuery = new SqlQuery(PriceTradeAgreementQueryText, accountRelationTempTable.TableName, itemIdTempTable.TableName);
    
                    priceTradeAgreementQuery.Parameters["@bi_ChannelId"] = context.ChannelId;
                    priceTradeAgreementQuery.Parameters["@nvc_CurrencyCode"] = currencyCode;
                    priceTradeAgreementQuery.Parameters["@Customer"] = customerAccount;
                    priceTradeAgreementQuery.Parameters["@NoDate"] = new DateTime(1900, 01, 01).Date;
                    priceTradeAgreementQuery.Parameters["@MinDate"] = fromDate.Date;
                    priceTradeAgreementQuery.Parameters["@MaxDate"] = toDate.Date;
    
                    var tradeAgreements = context.ReadEntity<TradeAgreement>(priceTradeAgreementQuery);
    
                    return tradeAgreements;
                }
            }
        }
    }
}
