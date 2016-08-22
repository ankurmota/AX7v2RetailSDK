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
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// The discount  procedure class contains the discount SQLite queries.
        /// </summary>
        internal class DiscountProcedure
        {
            /// <summary>
            /// Gets all the discount trade agreements.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="itemIds">The item identifiers.</param>
            /// <param name="customerAccount">The customer account.</param>
            /// <param name="minActiveDate">The min active date.</param>
            /// <param name="maxActiveDate">The max active date.</param>
            /// <param name="currencyCode">The currency code.</param>
            /// <returns>Returns the trade agreement read only collection.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "[TODO] Bug #3312124")]
            public static ReadOnlyCollection<TradeAgreement> GetAllDiscountTradeAgreements(
                SqliteDatabaseContext context,
                IEnumerable<string> itemIds,
                string customerAccount,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode)
            {
                const string DiscountTradeAgreementsQueryText = @"SELECT ta.RECID,
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
                                                LEFT JOIN [ax].INVENTDIM invdim ON ta.INVENTDIMID = invdim.INVENTDIMID AND ta.DATAAREAID = invdim.DATAAREAID
                                                WHERE
                                                    -- agreement is of one of the sales discount types
                                                    ta.RELATION IN (5, 6, 7)
    		                                        AND ta.CURRENCY = @nvc_CurrencyCode
    
                                                    -- and currently active
                                                    AND ((ta.FROMDATE <= @MinDate OR ta.FROMDATE <= @NoDate)
                                                            AND (ta.TODATE >= @MaxDate OR ta.TODATE <= @NoDate))
    
    		                                        AND ta.DATAAREAID = @nvc_DataAreaId
                                                    -- and customer/group relation matches
                                                    AND
                                                    (
                                                     -- account code is group and relation is in the price groups
                                                     ((ta.ACCOUNTCODE = 1) AND
                                                      (
                                                        (ta.RELATION = 5 AND ta.ACCOUNTRELATION = (SELECT LINEDISC FROM {1}))) OR
                                                        (ta.RELATION = 6 AND ta.ACCOUNTRELATION = (SELECT MULTILINEDISC FROM {1})) OR
                                                        (ta.RELATION = 7 AND ta.ACCOUNTRELATION = (SELECT ENDDISC FROM {1})))
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
                                                     ((ta.ITEMCODE = 0) AND (ta.ITEMRELATION IN (SELECT i.RECID FROM {0} i)))
                                                     OR
                                                     -- item code is group and trade agreement type is line discount, match item line discounts
                                                     ((ta.ITEMCODE = 1) AND (ta.RELATION = 5) AND (ta.ITEMRELATION IN (SELECT LINEDISC FROM {2})))
                                                     OR
                                                     -- item code is group and trade agreement type is multiline discount, match item line multiline discounts
                                                     ((ta.ITEMCODE = 1) AND (ta.RELATION = 6) AND (ta.ITEMRELATION IN( SELECT MULTILINEDISC FROM {3})))
                                                     OR
                                                     -- item code is all items
                                                     (ta.ITEMCODE = 2)
                                                    )
    
    	                                          -- and warehouse is either for all or for current channel
    	                                          AND
    	                                          (
    	                                           invdim.INVENTLOCATIONID = '' OR (invdim.INVENTLOCATIONID = c.INVENTLOCATION)
    	                                          )";
    
                using (TempTable items = TempTableHelper.CreateScalarTempTable(context, "RECID", itemIds))
                using (TempTable customerPriceGroupTempTable = PricingTempTable.CreateCustomerPriceGroups(context, customerAccount))
                using (TempTable itemLineDiscountTempTable = PricingTempTable.CreateItemLineDiscount(context, itemIds))
                using (TempTable itemMultiLineDiscountTempTable = PricingTempTable.CreateMultiLineDiscount(context, itemIds))
                {
                    var discountTradeAgreementsQuery = new SqlQuery(
                        DiscountTradeAgreementsQueryText,
                        items.TableName,
                        customerPriceGroupTempTable.TableName,
                        itemLineDiscountTempTable.TableName,
                        itemMultiLineDiscountTempTable.TableName);
    
                    discountTradeAgreementsQuery.Parameters["@bi_ChannelId"] = context.ChannelId;
                    discountTradeAgreementsQuery.Parameters["@Customer"] = customerAccount;
                    discountTradeAgreementsQuery.Parameters["@NoDate"] = new DateTime(1900, 01, 01);
                    discountTradeAgreementsQuery.Parameters["@MinDate"] = minActiveDate.Date;
                    discountTradeAgreementsQuery.Parameters["@MaxDate"] = maxActiveDate.Date;
    
                    return context.ReadEntity<TradeAgreement>(discountTradeAgreementsQuery).Results;
                }
            }
    
            /// <summary>
            /// Gets the periodic retail discount collection.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="itemUnits">The item unit collection.</param>
            /// <param name="priceGroupRecIds">The price group record identifiers.</param>
            /// <param name="fromDate">From date of the retail discount.</param>
            /// <param name="toDate">To date of the retail discount.</param>
            /// <param name="currencyCode">The currency code.</param>
            /// <returns>Returns the periodic discount read only collection.</returns>
            public static ReadOnlyCollection<PeriodicDiscount> GetRetailDiscount(
                SqliteDatabaseContext context,
                IEnumerable<ItemUnit> itemUnits,
                IEnumerable<long> priceGroupRecIds,
                DateTime fromDate,
                DateTime toDate,
                string currencyCode)
            {
                const string GetRetailDiscountQueryText = @"SELECT DISTINCT
                                                    pd.OFFERID AS OFFERID,
                                                    pd.NAME AS NAME,
                                                    pd.PERIODICDISCOUNTTYPE AS PERIODICDISCOUNTTYPE,
                                                    pd.CONCURRENCYMODE AS CONCURRENCYMODE,
                                                    pd.PRICINGPRIORITYNUMBER AS PRICINGPRIORITYNUMBER,
                                                    pd.ISDISCOUNTCODEREQUIRED AS ISDISCOUNTCODEREQUIRED,
                                                    pd.VALIDATIONPERIODID AS VALIDATIONPERIODID,
                                                    pd.DATEVALIDATIONTYPE AS DATEVALIDATIONTYPE,
                                                    pd.VALIDFROM AS VALIDFROM,
                                                    pd.VALIDTO AS VALIDTO,
                                                    pd.DISCOUNTTYPE AS DISCOUNTTYPE,
                                                    pd.DEALPRICEVALUE AS DEALPRICEVALUE,
                                                    pd.DISCOUNTPERCENTVALUE AS DISCOUNTPERCENTVALUE,
                                                    pd.DISCOUNTAMOUNTVALUE AS DISCOUNTAMOUNTVALUE,
                                                    pd.NOOFLEASTEXPENSIVELINES AS NOOFLEASTEXPENSIVELINES,
                                                    pd.NUMBEROFTIMESAPPLICABLE AS NUMBEROFTIMESAPPLICABLE,
                                                    pd.LINENUM AS LINENUM,
                                                    pd.DISCOUNTPERCENTORVALUE AS DISCOUNTPERCENTORVALUE,
    
                                                    IFNULL(mmol.LINEGROUP,'') AS LINEGROUP,
                                                    IFNULL(mmol.DISCOUNTTYPE,0) AS MIXANDMATCHLINEDISCOUNTTYPE,
                                                    IFNULL(mmol.NUMBEROFITEMSNEEDED,0) AS NUMBEROFITEMSNEEDED,
    
                                                    IFNULL(dol.DISCOUNTMETHOD,0) AS DISCOUNTMETHOD,
                                                    IFNULL(dol.DISCAMOUNT,0) AS DISCAMOUNT,
                                                    IFNULL(dol.DISCPCT, 0) AS DISCPCT,
                                                    IFNULL(dol.OFFERPRICE, 0) AS OFFERPRICE,
    
                                                    IFNULL(uom.SYMBOL,'') AS SYMBOL,
    
                                                    IFNULL(pd.COUNTNONDISCOUNTITEMS, 0) AS COUNTNONDISCOUNTITEMS,
                                                    it.ITEMID AS ITEMID,
                                                    it.VARIANTINVENTDIMID AS INVENTDIMID,
                                                    it.DISTINCTPRODUCTVARIANT AS DISTINCTPRODUCTVARIANT,
                                                    rgl.VARIANT AS VARIANT,
                                                    rgl.PRODUCT AS PRODUCTID
                                                FROM [crt].RETAILPERIODICDISCOUNTSFLATTENEDVIEW pd
    	                                        INNER JOIN [ax].RETAILCHANNELTABLE AS c
    		                                        ON c.INVENTLOCATIONDATAAREAID = pd.DATAAREAID AND c.RECID = @bi_ChannelId
                                                INNER JOIN [ax].RETAILDISCOUNTPRICEGROUP rdpg on rdpg.OFFERID = pd.OFFERID AND rdpg.DATAAREAID = pd.DATAAREAID
                                                INNER JOIN {0} pg ON rdpg.PRICEDISCGROUP = pg.RECID
                                                LEFT JOIN [ax].UNITOFMEASURE uom ON uom.RECID = pd.UNITOFMEASURE
                                                INNER JOIN [ax].RETAILGROUPMEMBERLINE rgl ON pd.RETAILGROUPMEMBERLINE = rgl.RECID
                                                LEFT JOIN [crt].RETAILPRODUCTORVARIANTCATEGORYANCESTORSVIEW rpca ON rgl.CATEGORY = rpca.CATEGORY
                                                LEFT JOIN [ax].RETAILDISCOUNTLINEMIXANDMATCH mmol ON pd.DISCOUNTLINEID = mmol.RECID AND pd.DATAAREAID = mmol.DATAAREAID
                                                LEFT JOIN [ax].RETAILDISCOUNTLINEOFFER dol ON pd.DISCOUNTLINEID = dol.RECID AND pd.DATAAREAID = dol.DATAAREAID
                                                INNER JOIN {1} it ON
                                                    (
                                                        (rgl.VARIANT != 0 AND rgl.VARIANT = it.DISTINCTPRODUCTVARIANT) OR
                                                        (rgl.VARIANT = 0 AND rgl.PRODUCT != 0 AND rgl.PRODUCT = it.PRODUCT) OR
                                                        (rgl.VARIANT = 0 AND rgl.PRODUCT = 0 AND
                                                        (rpca.PRODUCT = it.PRODUCT OR rpca.PRODUCT = it.DISTINCTPRODUCTVARIANT))
                                                    )
                                                WHERE (pd.STATUS = 1)
                                                    AND (pd.PERIODICDISCOUNTTYPE != 3) -- don't fetch price adjustments
                                                    AND (pd.VALIDFROM <= @MinDate OR pd.VALIDFROM <= @NoDate)
                                                    AND (pd.VALIDTO >= @MaxDate OR pd.VALIDTO <= @NoDate)
                                                    AND pd.CURRENCYCODE = @nvc_CurrencyCode
    		                                        AND pd.DATAAREAID = @nvc_DataAreaId
                                                ORDER BY pd.OFFERID, pd.LINENUM";
    
                using (TempTable priceGroupTempTable = TempTableHelper.CreateScalarTempTable(context, "RECID", priceGroupRecIds))
                using (TempTable itemIdTempTable = PricingTempTable.CreateItemIdentifier(context, itemUnits))
                {
                    SqlQuery priceAdjustmentQuery = new SqlQuery(GetRetailDiscountQueryText, priceGroupTempTable.TableName, itemIdTempTable.TableName);
    
                    priceAdjustmentQuery.Parameters["@bi_ChannelId"] = context.ChannelId;
                    priceAdjustmentQuery.Parameters["@NoDate"] = new DateTime(1900, 01, 01);
                    priceAdjustmentQuery.Parameters["@MinDate"] = fromDate.Date;
                    priceAdjustmentQuery.Parameters["@MaxDate"] = toDate.Date;
                    priceAdjustmentQuery.Parameters["@nvc_CurrencyCode"] = currencyCode;
                    priceAdjustmentQuery.Parameters["@nvc_DataAreaId"] = context.DataAreaId;
    
                    return context.ReadEntity<PeriodicDiscount>(priceAdjustmentQuery).Results;
                }
            }
        }
    }
}
