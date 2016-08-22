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
    
        /// <summary>
        /// Procedure class to get product attributes.
        /// </summary>
        internal sealed class GetProductAttributesProcedure
        {
            private SqliteDatabaseContext context;
            private long channelId;
            private string languageId;
            private TempTable assortedProductsTempTable;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProductAttributesProcedure"/> class.
            /// </summary>
            /// <param name="context">The database context.</param>
            /// <param name="assortedProductsTempTable">The temporary table holding the assorted products used as input for the attribute retrieval.</param>
            /// <param name="channelId">The channel id in which to execute the operation.</param>
            /// <param name="languageId">The language in which the attribute will be.</param>
            public GetProductAttributesProcedure(SqliteDatabaseContext context, TempTable assortedProductsTempTable, long channelId, string languageId)
            {
                this.context = context;
                this.channelId = channelId;
                this.languageId = languageId;
                this.assortedProductsTempTable = assortedProductsTempTable;
            }
    
            /// <summary>
            /// Gets the attributes' schema and value.
            /// </summary>
            /// <param name="attributeSchema">Products attribute schema.</param>
            /// <param name="attributeValues">Products attribute values.</param>
            public void GetAttributes(
                out ReadOnlyCollection<ProductAttributeSchemaEntry> attributeSchema,
                out ReadOnlyCollection<ProductProperty> attributeValues)
            {
                attributeSchema = this.GetAttributeSchema();
                attributeValues = this.GetAttributeValues();
            }
    
            /// <summary>
            /// Gets the attribute schema.
            /// </summary>
            /// <returns>The attribute schema.</returns>
            private ReadOnlyCollection<ProductAttributeSchemaEntry> GetAttributeSchema()
            {
                string query = @"
                WITH ATTRIBUTESCHEMA AS
                (
                    SELECT
                        rppacm.ATTRIBUTE,
                        0 AS PRODUCT,
                        0 AS CATALOG,
                        0 AS ATTRIBUTEGROUP,
                        0 AS ATTRIBUTEGROUPTYPE
                    FROM [ax].RETAILPUBPRODUCTATTRIBUTECHANNELMETADATA rppacm
                    WHERE
                        rppacm.ATTRIBUTERELATIONTYPE != 1  -- NonCategory
                        AND rppacm.HOSTCHANNEL = @CHANNELID
                )
    
                SELECT
                    [pas].ATTRIBUTE             AS ATTRIBUTE,
                    [pamv].KEYNAME              AS KEYNAME,
                    [pamv].DATATYPE             AS DATATYPE,
                    [pas].PRODUCT               AS PRODUCT,
                    [pas].[CATALOG]             AS CATALOG,
                    [pas].ATTRIBUTEGROUP        AS ATTRIBUTEGROUP,
                    [pas].ATTRIBUTEGROUPTYPE    AS ATTRIBUTEGROUPTYPE
                FROM ATTRIBUTESCHEMA AS pas
                INNER JOIN [crt].PRODUCTATTRIBUTEMETADATAVIEW pamv ON [pamv].ATTRIBUTE = [pas].ATTRIBUTE
                WHERE
                    [pamv].CHANNEL = @CHANNELID
    ";
    
                var attributeSchemaQuery = new SqlQuery(query);
                attributeSchemaQuery.Parameters["@CHANNELID"] = this.channelId;
                return this.context.ReadEntity<ProductAttributeSchemaEntry>(attributeSchemaQuery).Results;
            }
    
            /// <summary>
            /// Gets the attribute values.
            /// </summary>
            /// <returns>The attribute values.</returns>
            private ReadOnlyCollection<ProductProperty> GetAttributeValues()
            {
                string query = @"
                    SELECT
                        PAV.ATTRIBUTE           AS ATTRIBUTE,
                        PAV.VALUE               AS VALUE,
                        PAV.ISREFERENCE         AS ISREFERENCE,
                        PAV.PRODUCT             AS PRODUCT,
                        PAV.CATEGORY            AS CATEGORY,
                        PAV.CATALOG             AS CATALOG,
                        PAV.DISTANCE            AS DISTANCE,
                        PAV.SOURCE              AS SOURCE,
                        PAV.BOOLEANVALUE        AS BOOLEANVALUE,
                        PAV.CURRENCYVALUE       AS CURRENCYVALUE,
                        PAV.CURRENCYCODE        AS CURRENCYCODE,
                        PAV.DATETIMEVALUE       AS DATETIMEVALUE,
                        PAV.DATETIMEVALUETZID   AS DATETIMEVALUETZID,
                        PAV.FLOATVALUE          AS FLOATVALUE,
                        PAV.FLOATUNITSYMBOL     AS FLOATUNITSYMBOL,
                        PAV.INTVALUE            AS INTVALUE,
                        PAV.INTUNITSYMBOL       AS INTUNITSYMBOL,
                        PAV.TEXTVALUE           AS TEXTVALUE,
                        PAV.LANGUAGE            AS LANGUAGE,
                        PAV.TRANSLATION         AS TRANSLATION
                    FROM crt.PRODUCTATTRIBUTESVIEW PAV
                    WHERE
                        (PAV.PRODUCT IN (SELECT IDS.PRODUCTID FROM {0} IDS) OR PAV.PRODUCT = 0)
                        AND PAV.CHANNEL = @CHANNELID
                        AND (PAV.LANGUAGE = '' OR PAV.LANGUAGE IS NULL OR PAV.LANGUAGE = @LANGUAGEID)
    ";
    
                var sqlQuery = new SqlQuery(query, this.assortedProductsTempTable.TableName);
                sqlQuery.Parameters["@CHANNELID"] = this.channelId;
                sqlQuery.Parameters["@LANGUAGEID"] = this.languageId;
    
                return this.context.ReadEntity<ProductProperty>(sqlQuery).Results;
            }
        }
    }
}
