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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Shifts SQLite database accessor class.
        /// </summary>
        public class ShiftsSqliteDatabaseAccessor : DataStoreAccessor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ShiftsSqliteDatabaseAccessor"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public ShiftsSqliteDatabaseAccessor(RequestContext context)
            {
                this.Context = context;
            }
    
            /// <summary>
            /// Inserts the shift into the staging table.
            /// </summary>
            /// <param name="shift">The shift object.</param>
            public void InsertShiftStaging(Shift shift)
            {
                using (var context = new SqliteDatabaseContext(this.Context))
                {
                    ShiftsQueries.InsertShift(context, shift);
                }
            }
    
            /// <summary>
            /// Updated the shift staging table.
            /// </summary>
            /// <param name="shift">The shift.</param>
            public void UpdateShiftStaging(Shift shift)
            {
                using (var context = new SqliteDatabaseContext(this.Context))
                {
                    ShiftsQueries.UpdateShift(context, shift);
                }
            }
    
            /// <summary>
            /// Delete the shift staging table.
            /// </summary>
            /// <param name="shift">The shift.</param>
            public void DeleteShiftStaging(Shift shift)
            {
                using (var context = new SqliteDatabaseContext(this.Context))
                {
                    ShiftsQueries.DeleteShift(context, shift);
                }
            }
        }
    }
}
