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
    namespace Commerce.Runtime.DataServices.Sqlite.Product
    {
        using System.Collections.ObjectModel;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        internal sealed class GetProductsProcedure
        {
            private GetProductPartsDataRequest request;
    
            public GetProductsProcedure(GetProductPartsDataRequest request)
            {
                this.request = request;
            }
    
            public GetProductPartsDataResponse Execute()
            {
                ReadOnlyCollection<ProductIdentity> productIdentities;
                ReadOnlyCollection<ProductVariant> productVariants;
                ReadOnlyCollection<ProductProperty> productProperties;
                ReadOnlyCollection<ProductAttributeSchemaEntry> productAttributeSchemaEntries;
                ReadOnlyCollection<ProductRules> productRules;
                ReadOnlyCollection<ProductCatalog> productCatalogs;
                ReadOnlyCollection<ProductCategoryAssociation> categoryAssociations;
                ReadOnlyCollection<RelatedProduct> relatedProducts;
                ReadOnlyCollection<LinkedProduct> linkedProducts;
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                {
                    // use the channel id on the request, or context.ChannelId if none was provided in the request
                    long channelId = this.request.Criteria.Context.ChannelId.GetValueOrDefault(context.ChannelId);
    
                    using (TempTable assortedProducts = this.GetAssortedProductsTable(context, channelId, this.request.Criteria.SkipVariantExpansion))
                    {
                        // get product identities (product id, master id, variant id, item id...)
                        productIdentities = this.GetProductIdentities(context, assortedProducts);
    
                        // get variant details (dimensions, variant item id...)
                        productVariants = this.GetProductVariants(context, assortedProducts);
    
                        // get product attributes (name, variant localized names...)
                        var productAttributeProcedure = new GetProductAttributesProcedure(
                            context,
                            assortedProducts,
                            channelId,
                            this.request.LanguageId);
    
                        productAttributeProcedure.GetAttributes(out productAttributeSchemaEntries, out productProperties);
    
                        // get product rules (is serializable, should be weighted, etc...)
                        productRules = this.GetProductRules(context, assortedProducts);
    
                        productCatalogs = new ReadOnlyCollection<ProductCatalog>(new ProductCatalog[0]);
                        categoryAssociations = new ReadOnlyCollection<ProductCategoryAssociation>(new ProductCategoryAssociation[0]);
    
                        if (this.request.Criteria.DataLevel >= CommerceEntityDataLevel.Extended)
                        {
                            GetLinkedProductsProcedure getLinkedProductsProcedure = new GetLinkedProductsProcedure(context, channelId, assortedProducts);
                            linkedProducts = getLinkedProductsProcedure.Execute();
    
                            // Related products
                            relatedProducts = this.GetProductRelations(context, assortedProducts);
                        }
                        else
                        {
                            linkedProducts = new ReadOnlyCollection<LinkedProduct>(new LinkedProduct[0]);
                            relatedProducts = new ReadOnlyCollection<RelatedProduct>(new RelatedProduct[0]);
                        }
                    }
                }
    
                return new GetProductPartsDataResponse(
                    productIdentities,
                    productVariants,
                    productProperties,
                    productAttributeSchemaEntries,
                    productRules,
                    productCatalogs,
                    categoryAssociations,
                    relatedProducts,
                    linkedProducts);
            }
    
            private TempTable GetAssortedProductsTable(SqliteDatabaseContext context, long channelId, bool skipVariantsExpansion)
            {
                GetAssortedProductsProcedure assortedProductsProcedure = new GetAssortedProductsProcedure(
                    context,
                    channelId,
                    this.request.Criteria.Ids,
                    !skipVariantsExpansion,
                    this.request.QueryResultSettings.Paging);
    
                return assortedProductsProcedure.GetAssortedProducts();
            }
    
            private ReadOnlyCollection<ProductIdentity> GetProductIdentities(SqliteDatabaseContext context, TempTable assortedProducts)
            {
                string query = @"
                    -- For the filtered identifiers, we can now build the identity for each product.
                    -- For this query, LOOKUPID is either the MASTER or STANDALONE ID, PRODUCTID is the actual variantid or master/standalone when no variant is present
                    SELECT DISTINCT
    	                ap.PRODUCTID as RECID, -- id of the actual variant if present or master/standalone
                        ap.LOOKUPID  as LOOKUPID, -- master id or standalone id
                        ap.ISMASTER  as ISMASTER,
                        CASE
                            WHEN ([rk].RECID IS NULL) THEN 0
                            ELSE 1
                        END AS ISKIT,
                        0 AS ISREMOTE,
                        --CASE
                        --    WHEN ISMASTER = 0 AND ap.VARIANTID <> 0 THEN ap.PRODUCTID -- when it's not a master and variant is populated, then it's a variant and it's parentid is PRODUCTID
                        --    ELSE 0 -- otherwise it's a standalone or a master
                        --END AS MASTERPRODUCT,
                        CASE
                            WHEN ap.ISMASTER = 0 THEN ap.LOOKUPID
                            ELSE 0
                        END AS MASTERPRODUCT,  -- GETPRODUCTS always returns MASTERPRODUCT = LOOKUPID if ISMATER = 0
                        ap.ITEMID                     AS ITEMID,
                        COALESCE(idc.INVENTDIMID, '') AS INVENTDIMID,
    	                erp.DISPLAYPRODUCTNUMBER      AS DISPLAYPRODUCTNUMBER,
                        erp.SEARCHNAME                AS SEARCHNAME
    
                    FROM {0} ap
    
                    INNER JOIN [ax].ECORESPRODUCT erp ON [erp].RECID = [ap].PRODUCTID
                    LEFT OUTER JOIN [ax].INVENTDIMCOMBINATION idc ON [idc].DISTINCTPRODUCTVARIANT = [ap].PRODUCTID AND [idc].DATAAREAID = @DATAAREAID
                    LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT pv ON [pv].RECID = [ap].PRODUCTID
                    LEFT OUTER JOIN [ax].ECORESDISTINCTPRODUCTVARIANT pv2 ON [pv2].PRODUCTMASTER = [ap].PRODUCTID
                    LEFT OUTER JOIN [ax].RETAILKIT rk ON [rk].PRODUCTMASTER = [ap].PRODUCTID
                    -- Product builder will expect a specific ordering: master first, then variants
                    ORDER BY
                        ap.LOOKUPID,     -- master/variants are together in the result set
                        ap.ISMASTER DESC -- master first, then variants
    ";
                SqlQuery sqlQuery = new SqlQuery(query, assortedProducts.TableName);
                sqlQuery.Parameters["@DATAAREAID"] = context.DataAreaId;
    
                return context.ReadEntity<ProductIdentity>(sqlQuery).Results;
            }
    
            private ReadOnlyCollection<ProductVariant> GetProductVariants(SqliteDatabaseContext context, TempTable assortedProducts)
            {
                GetProductVariantsProcedure getVariantsProcedure = new GetProductVariantsProcedure(context, this.request.LanguageId, QueryResultSettings.AllRecords);
                return getVariantsProcedure.Execute(assortedProducts).Results;
            }
    
            private ReadOnlyCollection<ProductRules> GetProductRules(SqliteDatabaseContext context, TempTable assortedProducts)
            {
                // if data level is less than minimal, we don't need product rules
                if (this.request.Criteria.DataLevel < CommerceEntityDataLevel.Minimal)
                {
                    return new ReadOnlyCollection<ProductRules>(new ProductRules[0]);
                }
    
                const string Query =
                @"SELECT
                    [prv].PRODUCTID                 AS 'RECID',
                    [prv].BLOCKEDONPOS              AS BLOCKEDONPOS,
                    [prv].DATEBLOCKED               AS DATEBLOCKED,
                    [prv].DATETOACTIVATEITEM        AS DATETOACTIVATEITEM,
                    [prv].DATETOBEBLOCKED           AS DATETOBEBLOCKED,
                    [prv].KEYINGINPRICE             AS KEYINGINPRICE,
                    [prv].KEYINGINQTY               AS KEYINGINQTY,
                    [prv].MUSTKEYINCOMMENT          AS MUSTKEYINCOMMENT,
                    [prv].QTYBECOMESNEGATIVE        AS QTYBECOMESNEGATIVE,
                    [prv].SCALEITEM                 AS SCALEITEM,
                    [prv].ZEROPRICEVALID            AS ZEROPRICEVALID,
                    [prv].ISSERIALIZED              AS ISSERIALIZED,
                    [prv].ISACTIVEINSALESPROCESS    AS ISACTIVEINSALESPROCESS,
                    [prv].DEFAULTUNITOFMEASURE      AS DEFAULTUNITOFMEASURE
                FROM CRT.PRODUCTRULESVIEW prv
                INNER JOIN {0} ids ON [ids].PRODUCTID = [prv].PRODUCTID
                WHERE [prv].DATAAREAID = @DATAAREAID";
    
                SqlQuery sqlQuery = new SqlQuery(Query, assortedProducts.TableName);
                sqlQuery.Parameters["@DATAAREAID"] = context.DataAreaId;
    
                return context.ReadEntity<ProductRules>(sqlQuery).Results;
            }
    
            private ReadOnlyCollection<RelatedProduct> GetProductRelations(SqliteDatabaseContext context, TempTable assortedProducts)
            {
                const string GetRelatedProductsQueryString = @"
                    WITH RELATEDPRODUCTIDS AS
                    (
    		            SELECT DISTINCT erprt.PRODUCT1 AS RECID
                        FROM ax.ECORESPRODUCTRELATIONTABLE erprt
                        WHERE
                            erprt.PRODUCT1 IN (SELECT LOOKUPID FROM {0})
    
                        UNION
    
                        SELECT DISTINCT erprt.PRODUCT1 AS RECID
                        FROM ax.ECORESPRODUCTRELATIONTABLE erprt
                        WHERE
                            erprt.PRODUCT2 IN (SELECT LOOKUPID FROM {0})
                    )
    
                    -- Non catalog product relations
                    SELECT
                        0                   AS CATALOG,
                        erprt.PRODUCT1      AS PRODUCT,
                        erprt.PRODUCT2      AS RELATEDPRODUCT,
                        erprtype.NAME       AS PRODUCTRELATIONTYPE,
                        0                   AS EXCLUSION
                    FROM ax.ECORESPRODUCTRELATIONTABLE erprt
    	            INNER JOIN ax.ECORESPRODUCTRELATIONTYPE erprtype ON erprt.PRODUCTRELATIONTYPE = erprtype.RECID
    	            WHERE
    		            erprt.PRODUCT1 IN (SELECT RECID FROM RELATEDPRODUCTIDS)";
    
                SqlQuery sqlQuery = new SqlQuery(GetRelatedProductsQueryString, assortedProducts.TableName);
                return context.ReadEntity<RelatedProduct>(sqlQuery).Results;
            }
        }
    }
}
