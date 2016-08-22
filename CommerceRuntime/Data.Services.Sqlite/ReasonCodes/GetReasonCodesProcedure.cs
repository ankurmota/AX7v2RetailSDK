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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The procedure helper class for getting reason codes.
        /// </summary>
        internal sealed class GetReasonCodesProcedure
        {
            private readonly string defaultLanguageId;
            private readonly string employeeLanguageId;
            private GetReasonCodesDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetReasonCodesProcedure" /> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <param name="defaultlanguageId">The channel default language identifier.</param>
            /// <param name="employeeLanguageId">The employee language identifier.</param>
            public GetReasonCodesProcedure(GetReasonCodesDataRequest request, string defaultlanguageId, string employeeLanguageId)
            {
                this.request = request;
                this.defaultLanguageId = defaultlanguageId;
                this.employeeLanguageId = employeeLanguageId;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            /// <returns>The collection of reason codes.</returns>
            public PagedResult<ReasonCode> Execute()
            {
                const string GetReasonCodesQueryString = @"
                (
                    SELECT
                        icv.[REASONCODEID]                AS REASONCODEID,
                        icv.[RECID]                     AS RECID,
                        icv.[ONCEPERTRANSACTION]        AS ONCEPERTRANSACTION,
                        icv.[PRINTPROMPTONRECEIPT]      AS PRINTPROMPTONRECEIPT,
                        icv.[PRINTINPUTONRECEIPT]       AS PRINTINPUTONRECEIPT,
                        icv.[PRINTINPUTNAMEONRECEIPT]   AS PRINTINPUTNAMEONRECEIPT,
                        icv.[INPUTTYPE]                 AS INPUTTYPE,
                        icv.[MINIMUMVALUE]              AS MINIMUMVALUE,
                        icv.[MAXIMUMVALUE]              AS MAXIMUMVALUE,
                        icv.[MINIMUMLENGTH]             AS MINIMUMLENGTH,
                        icv.[MAXIMUMLENGTH]             AS MAXIMUMLENGTH,
                        icv.[INPUTREQUIRED]             AS INPUTREQUIRED,
                        icv.[LINKEDREASONCODEID]          AS LINKEDREASONCODEID,
                        icv.[RANDOMFACTOR]              AS RANDOMFACTOR,
                        icv.[RETAILUSEINFOCODE]         AS RETAILUSEINFOCODE,
                        icv.[PRIORITY]                  AS PRIORITY,
                        COALESCE(rict.[DESCRIPTION], rictd.[DESCRIPTION], icv.[REASONCODEID]) AS DESCRIPTION,
                        COALESCE(rict.[PROMPT], rictd.[PROMPT], icv.[REASONCODEID]) AS PROMPT,
                        COALESCE(rict.[LANGUAGEID], rictd.[LANGUAGEID]) AS LANGUAGEID
                    FROM [crt].[INFOCODEVIEW] icv
                    LEFT JOIN [ax].[RETAILINFOCODETRANSLATION] rict
                        ON icv.[RECID]               = rict.[INFOCODE]
                            AND icv.[DATAAREAID]    = rict.[DATAAREAID]
                            AND rict.[LANGUAGEID]   = @languageId
                    LEFT JOIN [ax].[RETAILINFOCODETRANSLATION] rictd
                        ON icv.[RECID]              = rictd.[INFOCODE]
                            AND icv.[DATAAREAID]    = rictd.[DATAAREAID]
                            AND rictd.[LANGUAGEID]  = @defaultlanguageId
                    WHERE icv.[DATAAREAID] = @DataAreaId
                            AND ((SELECT COUNT(STRINGID) FROM @tvp_groupIds) = 0 OR icv.[GROUPID] IN (SELECT STRINGID FROM @tvp_groupIds))
                )";
    
                SqlPagedQuery query = new SqlPagedQuery(this.request.QueryResultSettings)
                {
                    From = GetReasonCodesQueryString,
                    Aliased = true,
                    DatabaseSchema = string.Empty,
                    OrderBy = new SortingInfo(ReasonCode.PriorityColumn, false).ToString()
                };
    
                PagedResult<ReasonCode> reasonCodes;
    
                query.Parameters["@defaultlanguageId"] = this.defaultLanguageId;
                query.Parameters["@languageId"] = this.employeeLanguageId;
                query.Parameters["@DataAreaId"] = this.request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                using (StringIdTableType groupIds = new StringIdTableType(this.request.ReasonCodeIds, "GROUPID"))
                {
                    // the view sets the INFOCODEID to GROUPID when the reason code is not part of a group, so we always query by GROUPID
                    query.Parameters["@tvp_groupIds"] = groupIds.DataTable;
    
                    reasonCodes = context.ReadEntity<ReasonCode>(query);
    
                    if (reasonCodes.Results.Any())
                    {
                        IEnumerable<string> reasonCodeIds = reasonCodes.Results.Select(x => x.ReasonCodeId);
    
                        QueryResultSettings subCodeSettings = QueryResultSettings.AllRecords;
                        GetSubReasonCodesProcedure getSubReasonCodes = new GetSubReasonCodesProcedure(
                            this.request.RequestContext,
                            context,
                            reasonCodeIds,
                            reasonSubCodeId: null,
                            settings: subCodeSettings,
                            defaultlanguageId: this.defaultLanguageId,
                            employeeLanguageId: this.employeeLanguageId);
    
                        ILookup<string, ReasonSubCode> subcodes = getSubReasonCodes.Execute().ToLookup(x => x.ReasonCodeId);
    
                        foreach (var infoCode in reasonCodes.Results)
                        {
                            infoCode.ReasonSubCodes.Clear();
                            infoCode.ReasonSubCodes.AddRange(subcodes[infoCode.ReasonCodeId]);
                        }
                    }
                }
    
                return reasonCodes;
            }
        }
    }
}
