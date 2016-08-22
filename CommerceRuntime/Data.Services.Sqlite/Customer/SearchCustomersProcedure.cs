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
        /// Helper procedure class to search customers.
        /// </summary>
        public sealed class SearchCustomersProcedure
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SearchCustomersProcedure"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public SearchCustomersProcedure(RequestContext context)
            {
                this.Context = context;
            }
    
            private RequestContext Context { get; set; }
    
            /// <summary>
            /// Gets the customers.
            /// </summary>
            /// <param name="keyword">Optional record identifier of the customer to retrieve.</param>
            /// <param name="onlyCurrentCompany">If set to <c>true</c> [only current company].</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>
            /// A collection of customers.
            /// </returns>
            public PagedResult<GlobalCustomer> SearchCustomers(string keyword, bool onlyCurrentCompany, QueryResultSettings settings)
            {
                ThrowIf.Null(settings, "settings");
                ThrowIf.Null(keyword, "keyword");
    
                PagedResult<GlobalCustomer> customers = this.SearchCustomersSproc(keyword, onlyCurrentCompany, settings);
    
                return customers;
            }
    
            private PagedResult<GlobalCustomer> SearchCustomersSproc(string keyword, bool onlyCurrentCompany, QueryResultSettings settings)
            {
                using (SqliteDatabaseContext databaseContext = new SqliteDatabaseContext(this.Context))
                {
                    // Use the "LIKE" to search for customer related table fields
                    // Based on the pre-populated materialized view crt_CUSTOMERSEARCHABLEFIELDSVIEW
                    string queryText = @"WITH UNIQUEPARTYIDS AS
                                    (
                                        SELECT DISTINCT csfv.PARTY
                                        FROM crt_CUSTOMERSEARCHABLEFIELDSVIEW csfv
                                            INNER JOIN (
                                                SELECT PARTY, ADDRESSBOOK
                                                FROM AX_DIRADDRESSBOOKPARTY
                                                WHERE PARTY NOT IN (SELECT RECID FROM AX_OMINTERNALORGANIZATION)) dap ON csfv.PARTY = dap.PARTY
                                            INNER JOIN AX_RETAILSTOREADDRESSBOOK rsab ON dap.ADDRESSBOOK = rsab.ADDRESSBOOK
                                        WHERE rsab.STORERECID = @bi_ChannelId AND csfv.FIELD LIKE @nvc_SearchCondition
                                    )
                                    SELECT PARTYNUMBER,
                                            CASE
                                                WHEN gcv.DATAAREAID <> '' THEN gcv.ACCOUNTNUMBER
                                                ELSE ''
                                            END AS ACCOUNTNUMBER,
                                            NAME,
                                            FULLADDRESS,
                                            PHONE,
                                            EMAIL,
                                            INSTANCERELATIONTYPE as CUSTOMERTYPE,
                                            ORGID,
                                            RECORDID
                                    FROM CRT_GLOBALCUSTOMERSVIEW gcv
                                        INNER JOIN UNIQUEPARTYIDS pid ON gcv.RECORDID = pid.PARTY
                                    WHERE (@b_SearchCrossCompany <> 1 AND ACCOUNTNUMBER != '' AND gcv.DATAAREAID = @nvc_DataAreaId)
    	                                OR (@b_SearchCrossCompany = 1)
                                    LIMIT @i_Top, @i_Skip;";
    
                    var parameters = new ParameterSet();
                    parameters["@bi_ChannelId"] = databaseContext.ChannelId;
                    parameters["@nvc_DataAreaId"] = databaseContext.DataAreaId;
                    parameters["@nvc_SearchCondition"] = string.Format("%{0}%", keyword);
                    parameters["@b_SearchCrossCompany"] = Convert.ToInt32(onlyCurrentCompany);
                    parameters["@i_Top"] = settings.Paging.Top;
                    parameters["@i_Skip"] = settings.Paging.Skip;
                    var query = new SqlQuery(queryText, parameters);
                    PagedResult<GlobalCustomer> customers = databaseContext.ReadEntity<GlobalCustomer>(query);
    
                    return customers;
                }
            }
        }
    }
}
