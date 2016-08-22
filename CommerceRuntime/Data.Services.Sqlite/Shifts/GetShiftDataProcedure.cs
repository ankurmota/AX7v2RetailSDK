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
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The SQLite implementation of getting shift data.
        /// </summary>
        internal sealed class GetShiftDataProcedure
        {
            private ShiftDataQueryCriteria criteria;
            private SqliteDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetShiftDataProcedure"/> class.
            /// </summary>
            /// <param name="criteria">The query criteria object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetShiftDataProcedure(ShiftDataQueryCriteria criteria, SqliteDatabaseContext databaseContext)
            {
                this.criteria = criteria;
                this.databaseContext = databaseContext;
            }
    
            public EntityDataServiceResponse<Shift> Execute()
            {
                IList<string> whereClauses = new List<string>();
    
                // Builds where clauses for query.
                if (this.criteria.ChannelId != null)
                {
                    whereClauses.Add("CHANNEL = @ChannelId");
                }
    
                if (this.criteria.Status != null)
                {
                    whereClauses.Add("STATUS = @Status");
                }
    
                if (!string.IsNullOrEmpty(this.criteria.StaffId))
                {
                    if (this.criteria.SearchByStaffId && this.criteria.SearchByCurrentStaffId)
                    {
                        whereClauses.Add("(STAFFID = @StaffId OR CURRENTSTAFFID = @StaffId)");
                    }
    
                    if (this.criteria.SearchByStaffId && !this.criteria.SearchByCurrentStaffId)
                    {
                        whereClauses.Add("STAFFID = @StaffId");
                    }
    
                    if (!this.criteria.SearchByStaffId && this.criteria.SearchByCurrentStaffId)
                    {
                        whereClauses.Add("CURRENTSTAFFID = @StaffId");
                    }
                }
    
                if (!string.IsNullOrEmpty(this.criteria.TerminalId))
                {
                    if (this.criteria.SearchByTerminalId && this.criteria.SearchByCurrentTerminalId)
                    {
                        whereClauses.Add("(TERMINALID = @TerminalId OR CURRENTTERMINALID = @TerminalId)");
                    }
    
                    if (this.criteria.SearchByTerminalId && !this.criteria.SearchByCurrentTerminalId)
                    {
                        whereClauses.Add("TERMINALID = @TerminalId");
                    }
    
                    if (!this.criteria.SearchByTerminalId && this.criteria.SearchByCurrentTerminalId)
                    {
                        whereClauses.Add("CURRENTTERMINALID = @TerminalId");
                    }
                }
    
                if (this.criteria.ShiftId != null)
                {
                    whereClauses.Add("SHIFTID = @ShiftId");
                }
    
                string queryString = string.Concat(OfflineShiftDataServiceUtilities.OfflineShiftsView, "WHERE ", string.Join(" AND ", whereClauses));
                SqlQuery query = new SqlQuery(queryString);
    
                // Sets query parameters.
                if (this.criteria.ChannelId != null)
                {
                    query.Parameters.Add("@ChannelId", this.criteria.ChannelId);
                }
    
                if (this.criteria.Status != null)
                {
                    query.Parameters.Add("@Status", this.criteria.Status);
                }
    
                if (!string.IsNullOrEmpty(this.criteria.StaffId))
                {
                    query.Parameters.Add("@StaffId", this.criteria.StaffId);
                }
    
                if (!string.IsNullOrEmpty(this.criteria.TerminalId))
                {
                    query.Parameters.Add("@TerminalId", this.criteria.TerminalId);
                }
    
                if (this.criteria.ShiftId != null)
                {
                    query.Parameters.Add("@ShiftId", this.criteria.ShiftId);
                }
    
                PagedResult<Shift> shifts = this.databaseContext.ReadEntity<Shift>(query);
                return new EntityDataServiceResponse<Shift>(shifts);
            }
        }
    }
}
