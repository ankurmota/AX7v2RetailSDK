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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        internal sealed class GetShiftDataProcedure
        {
            private const string ShiftsView = "SHIFTSVIEW";
    
            private GetShiftDataDataRequest request;
            private SqlServerDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetShiftDataProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetShiftDataProcedure(GetShiftDataDataRequest request, SqlServerDatabaseContext databaseContext)
            {
                this.request = request;
                this.databaseContext = databaseContext;
            }
    
            public EntityDataServiceResponse<Shift> Execute(QueryResultSettings queryResultSettings)
            {
                SqlPagedQuery query = new SqlPagedQuery(queryResultSettings)
                {
                    From = ShiftsView,
                    OrderBy = "SHIFTID",
                };
    
                IList<string> whereClauses = new List<string>();
    
                // Builds where clauses for query.
                if (this.request.Criteria.ChannelId != null)
                {
                    whereClauses.Add("CHANNEL = @ChannelId");
                }
    
                if (this.request.Criteria.Status != null)
                {
                    whereClauses.Add("STATUS = @Status");
                }
    
                if (!string.IsNullOrEmpty(this.request.Criteria.StaffId))
                {
                    if (this.request.Criteria.SearchByStaffId && this.request.Criteria.SearchByCurrentStaffId)
                    {
                        whereClauses.Add("(STAFFID = @StaffId OR CURRENTSTAFFID = @StaffId)");
                    }
    
                    if (this.request.Criteria.SearchByStaffId && !this.request.Criteria.SearchByCurrentStaffId)
                    {
                        whereClauses.Add("STAFFID = @StaffId");
                    }
    
                    if (!this.request.Criteria.SearchByStaffId && this.request.Criteria.SearchByCurrentStaffId)
                    {
                        whereClauses.Add("CURRENTSTAFFID = @StaffId");
                    }
                }
    
                if (!string.IsNullOrEmpty(this.request.Criteria.TerminalId))
                {
                    if (this.request.Criteria.SearchByTerminalId && this.request.Criteria.SearchByCurrentTerminalId)
                    {
                        whereClauses.Add("(TERMINALID = @TerminalId OR CURRENTTERMINALID = @TerminalId)");
                    }
    
                    if (this.request.Criteria.SearchByTerminalId && !this.request.Criteria.SearchByCurrentTerminalId)
                    {
                        whereClauses.Add("TERMINALID = @TerminalId");
                    }
    
                    if (!this.request.Criteria.SearchByTerminalId && this.request.Criteria.SearchByCurrentTerminalId)
                    {
                        whereClauses.Add("CURRENTTERMINALID = @TerminalId");
                    }
                }
    
                if (this.request.Criteria.ShiftId != null)
                {
                    whereClauses.Add("SHIFTID = @ShiftId");
                }
    
                query.Where = string.Join(" AND ", whereClauses);
    
                // Build query for shared shifts.
                if (!this.request.Criteria.IncludeSharedShifts)
                {
                    query.Where += " AND (ISSHARED IS NULL OR ISSHARED <> 1)";
                }
    
                // Sets query parameters.
                if (this.request.Criteria.ChannelId != null)
                {
                    query.Parameters.Add("@ChannelId", this.request.Criteria.ChannelId);
                }
    
                if (this.request.Criteria.Status != null)
                {
                    query.Parameters.Add("@Status", this.request.Criteria.Status);
                }
    
                if (!string.IsNullOrEmpty(this.request.Criteria.StaffId))
                {
                    query.Parameters.Add("@StaffId", this.request.Criteria.StaffId);
                }
    
                if (!string.IsNullOrEmpty(this.request.Criteria.TerminalId))
                {
                    query.Parameters.Add("@TerminalId", this.request.Criteria.TerminalId);
                }
    
                if (this.request.Criteria.ShiftId != null)
                {
                    query.Parameters.Add("@ShiftId", this.request.Criteria.ShiftId);
                }
    
                PagedResult<Shift> shifts = this.databaseContext.ReadEntity<Shift>(query);
                return new EntityDataServiceResponse<Shift>(shifts);
            }
        }
    }
}
