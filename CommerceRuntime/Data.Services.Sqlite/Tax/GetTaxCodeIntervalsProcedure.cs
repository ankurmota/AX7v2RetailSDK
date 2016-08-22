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
        /// Tax SQLite database accessor class.
        /// </summary>
        public class GetTaxCodeIntervalsProcedure : DataStoreAccessor
        {
            private DateTime dateBoundary = new DateTime(1900, 01, 01);
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetTaxCodeIntervalsProcedure"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public GetTaxCodeIntervalsProcedure(RequestContext context)
            {
                this.Context = context;
            }
    
            /// <summary>
            /// Get the tax code intervals.
            /// </summary>
            /// <param name="salesTaxGroup">The sales tax group.</param>
            /// <param name="itemTaxGroup">The item tax group.</param>
            /// <param name="transactionDate">The transaction date.</param>
            /// <returns>The tax code intervals.</returns>
            public PagedResult<TaxCodeInterval> GetTaxCodeIntervals(string salesTaxGroup, string itemTaxGroup, DateTimeOffset transactionDate)
            {
                string query = @"
                    SELECT DISTINCT
                        toi.TAXITEMGROUP,
                        toi.TAXCODE,
                        IFNULL(td.TAXVALUE, 0.0) AS TAXVALUE,
                        IFNULL(td.TAXLIMITMIN, 0.0) AS TAXLIMITMIN,
                        IFNULL(td.TAXLIMITMAX, 0.0) AS TAXLIMITMAX,
                        tgd.EXEMPTTAX,
                        tgh.TAXGROUPROUNDING,
                        tt.TAXCURRENCYCODE,
                        tt.TAXBASE,
                        tt.TAXLIMITBASE,
                        tt.TAXCALCMETHOD,
                        tt.TAXONTAX,
                        tt.TAXUNIT,
                        IFNULL(tcl.TAXMAX,0) AS TAXMAX,
                        IFNULL(tcl.TAXMIN,0) AS TAXMIN
                    FROM [ax].TAXGROUPHEADING tgh
                    INNER JOIN [ax].TAXGROUPDATA tgd ON tgh.TAXGROUP = tgd.TAXGROUP AND tgh.DATAAREAID = tgd.DATAAREAID
                    INNER JOIN [ax].TAXONITEM toi ON tgd.TAXCODE = toi.TAXCODE AND tgd.DATAAREAID = toi.DATAAREAID
                    INNER JOIN [ax].TAXDATA td ON toi.TAXCODE = td.TAXCODE AND toi.DATAAREAID = td.DATAAREAID
                    INNER JOIN [ax].TAXTABLE tt ON tt.TAXCODE = td.TAXCODE AND tt.DATAAREAID = td.DATAAREAID
                    LEFT JOIN [ax].TAXCOLLECTLIMIT tcl ON
                        tcl.TAXCODE = td.TAXCODE
                        AND (tcl.TAXFROMDATE IS NULL
                            OR @dt_TransactionDate >= tcl.TAXFROMDATE
                            OR tcl.TAXFROMDATE = @dt_NoDateBoundary)
                        AND (tcl.TAXTODATE IS NULL
                            OR @dt_TransactionDateYesterday < td.TAXTODATE
                            OR tcl.TAXTODATE = @dt_NoDateBoundary)
                    WHERE
                        tgh.DATAAREAID = @nvc_DataAreaId
                        AND toi.TAXITEMGROUP = @nvc_ItemTaxGroup
                        AND tgh.TAXGROUP = @nvc_SalesTaxGroup
                        AND ((@dt_TransactionDate >= td.TAXFROMDATE OR td.TAXFROMDATE = @dt_NoDateBoundary)
                        AND (@dt_TransactionDateYesterday < td.TAXTODATE OR td.TAXTODATE = @dt_NoDateBoundary))";
    
                var attributeSchemaQuery = new SqlQuery(query);
    
                attributeSchemaQuery.Parameters["@nvc_SalesTaxGroup"] = salesTaxGroup;
                attributeSchemaQuery.Parameters["@nvc_DataAreaId"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;
                attributeSchemaQuery.Parameters["@nvc_ItemTaxGroup"] = itemTaxGroup;
                attributeSchemaQuery.Parameters["@dt_TransactionDate"] = transactionDate.DateTime;
    
                // The parameter below is used so that sqlite uses integers to comapare dates.
                attributeSchemaQuery.Parameters["@dt_TransactionDateYesterday"] = transactionDate.DateTime.AddDays(-1);
                attributeSchemaQuery.Parameters["@dt_NoDateBoundary"] = this.dateBoundary.Date;
    
                using (var databaseContext = new SqliteDatabaseContext(this.Context))
                {
                    return databaseContext.ReadEntity<TaxCodeInterval>(attributeSchemaQuery);
                }
            }
        }
    }
}
