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
    namespace Commerce.Runtime.DataServices.Sqlite.DataServices.ReasonCodes
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The procedure helper class for getting sub reason codes.
        /// </summary>
        internal sealed class GetSubReasonCodesProcedure
        {
            private readonly string defaultLanguageId;
            private readonly string employeeLanguageId;
            private IEnumerable<string> reasonCodeIds;
            private string reasonSubCodeId;
            private QueryResultSettings settings;
            private RequestContext requestContext;
            private SqliteDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetSubReasonCodesProcedure" /> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="databaseContext">The database context.</param>
            /// <param name="reasonCodeIds">The reason code identifiers.</param>
            /// <param name="reasonSubCodeId">The sub reason code identifiers.</param>
            /// <param name="settings">The query settings.</param>
            /// <param name="defaultlanguageId">The channel default language identifier.</param>
            /// <param name="employeeLanguageId">The employee language identifier.</param>
            public GetSubReasonCodesProcedure(
                RequestContext context,
                SqliteDatabaseContext databaseContext,
                IEnumerable<string> reasonCodeIds,
                string reasonSubCodeId,
                QueryResultSettings settings,
                string defaultlanguageId,
                string employeeLanguageId)
            {
                this.requestContext = context;
                this.databaseContext = databaseContext;
                this.reasonCodeIds = reasonCodeIds;
                this.reasonSubCodeId = reasonSubCodeId;
                this.settings = settings;
                this.defaultLanguageId = defaultlanguageId;
                this.employeeLanguageId = employeeLanguageId;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            /// <returns>The collection of sub reason codes.</returns>
            public ReadOnlyCollection<ReasonSubCode> Execute()
            {
                const string GetSubReasonCodesQueryString = @"
                (
    	            	SELECT
    		                iscv.[REASONCODEID]                   AS REASONCODEID,
    		                iscv.[SUBCODEID]                    AS SUBCODEID,
    		                iscv.[RECID]                        AS RECID,
    		                iscv.[TRIGGERFUNCTION]              AS TRIGGERFUNCTION,
    		                iscv.[TRIGGERCODE]                  AS TRIGGERCODE,
    		                iscv.[NEWSALESLINE]                 AS NEWSALESLINE,
    		                iscv.[PRICETYPE]                    AS PRICETYPE,
    		                iscv.[AMOUNTPERCENT]                AS AMOUNTPERCENT,
    		                COALESCE(risct.[DESCRIPTION], risctd.[DESCRIPTION], iscv.[SUBCODEID])         AS DESCRIPTION,
    		                COALESCE(risct.[LANGUAGEID], risctd.[LANGUAGEID])                             AS LANGUAGEID
    	                FROM [crt].[INFOSUBCODEVIEW] iscv
    	                LEFT JOIN [ax].[RETAILINFORMATIONSUBCODETRANSLATION] risct
    		                ON	iscv.[RECID]			= risct.[INFOSUBCODE]
    			                AND risct.[LANGUAGEID]	= @languageId
                                AND risct.[DATAAREAID]  = @DataAreaId
    	                LEFT JOIN [ax].[RETAILINFORMATIONSUBCODETRANSLATION] risctd
    		                ON	iscv.[RECID]			= risctd.[INFOSUBCODE]
    			                AND risctd.[LANGUAGEID]	= @defaultlanguageId
                                AND risctd.[DATAAREAID] = @DataAreaId
    	                WHERE iscv.[DATAAREAID] = @DataAreaId
                )";
    
                SqlPagedQuery query = new SqlPagedQuery(this.settings)
                {
                    From = GetSubReasonCodesQueryString,
                    Aliased = true,
                    DatabaseSchema = string.Empty
                };
    
                this.BuildSubReasonCodesQuery(this.reasonCodeIds, this.reasonSubCodeId, query);
    
                ReadOnlyCollection<ReasonSubCode> reasonSubCodes;
    
                using (StringIdTableType type = new StringIdTableType(this.reasonCodeIds, "REASONCODEID"))
                {
                    query.Parameters["@TVP_INFOCODEIDTABLETYPE"] = type;
                    reasonSubCodes = this.databaseContext.ReadEntity<ReasonSubCode>(query).Results;
                }
    
                return reasonSubCodes;
            }
    
            /// <summary>
            /// Builds the query for getting sub reason codes.
            /// </summary>
            /// <param name="givenReasonCodeIds">The reason code identifiers.</param>
            /// <param name="givenReasonSubCodeId">The reason sub code identifier.</param>
            /// <param name="query">The query object.</param>
            public void BuildSubReasonCodesQuery(IEnumerable<string> givenReasonCodeIds, string givenReasonSubCodeId, SqlPagedQuery query)
            {
                ThrowIf.Null(query, "query");
    
                // Add query clause for info code ids
                if (givenReasonCodeIds.Any(givenReasonCodeId => string.IsNullOrWhiteSpace(givenReasonCodeId)))
                {
                    throw new ArgumentException("Empty reason code id(s) were encountered.", "givenReasonCodeIds");
                }
    
                var whereClauses = new List<string>();
    
                // Add query clause for info subcode id (primary key is a combination of infoCodeId and subCodeId).
                if (!string.IsNullOrWhiteSpace(givenReasonSubCodeId))
                {
                    whereClauses.Add(string.Format(@"({0} = @InfoSubcodeId)", "INFOSUBCODEID"));
                    query.Parameters["@InfoSubcodeId"] = givenReasonSubCodeId;
                }
    
                query.Parameters["@defaultlanguageId"] = this.defaultLanguageId;
                query.Parameters["@languageId"] = this.employeeLanguageId;
                query.Parameters["@DataAreaId"] = this.requestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                query.Where = string.Join(" AND ", whereClauses);
            }
        }
    }
}
