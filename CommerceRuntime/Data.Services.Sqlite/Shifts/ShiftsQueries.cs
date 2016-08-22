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
    
        internal class ShiftsQueries
        {
            /// <summary>
            /// Updates the shift in shift staging table.
            /// </summary>
            /// <param name="context">The SQLite database context.</param>
            /// <param name="shift">The shift object.</param>
            public static void UpdateShift(SqliteDatabaseContext context, Shift shift)
            {
                const string UpdateQuery = @"UPDATE [crt].RETAILSHIFTSTAGINGTABLE
                                 SET
                                 STAFFID                = @StaffId,
                                 CURRENTSTAFFID         = @CurrentStaffId,
                                 [STATUS]               = @Status,
                                 CURRENTTERMINALID      = @CurrentTerminalId,
                                 STATUSDATETIMEUTC      = @StatusDateTimeUTC,
                                 CASHDRAWER             = @CashDrawer
                                    WHERE
                                 [crt].RETAILSHIFTSTAGINGTABLE.CHANNEL = @ChannelId AND
                                 [crt].RETAILSHIFTSTAGINGTABLE.TERMINALID = @TerminalId AND
                                 [crt].RETAILSHIFTSTAGINGTABLE.SHIFTID = @ShiftId";
    
                var sqlQuery = new SqlQuery(UpdateQuery);
    
                sqlQuery.Parameters["@StaffId"] = shift.StaffId;
                sqlQuery.Parameters["@CurrentStaffId"] = shift.CurrentStaffId;
                sqlQuery.Parameters["@Status"] = shift.Status;
                sqlQuery.Parameters["@CurrentTerminalId"] = shift.CurrentTerminalId;
                sqlQuery.Parameters["@StatusDateTimeUTC"] = shift.StatusDateTime;
                sqlQuery.Parameters["@CashDrawer"] = shift.CashDrawer;
                sqlQuery.Parameters["@ChannelId"] = context.ChannelId;
                sqlQuery.Parameters["@TerminalId"] = context.TerminalId;
                sqlQuery.Parameters["@ShiftId"] = context.ShiftId;
    
                context.ExecuteNonQuery(sqlQuery);
            }
    
            /// <summary>
            /// Inserts the shift into shift staging table.
            /// </summary>
            /// <param name="context">The SQLite database context.</param>
            /// <param name="shift">The shift object.</param>
            public static void InsertShift(SqliteDatabaseContext context, Shift shift)
            {
                const string InsertQuery = @"INSERT OR REPLACE INTO [crt].RETAILSHIFTSTAGINGTABLE
                                            (
                                                CHANNEL
                                                ,STOREID
                                                ,TERMINALID
                                                ,SHIFTID		
                                                ,STAFFID
    		                                    ,CURRENTSTAFFID
                                                ,[STATUS]
                                                ,CURRENTTERMINALID
                                                ,STARTDATETIMEUTC
                                                ,STATUSDATETIMEUTC
                                                ,DATAAREAID
                                                ,CASHDRAWER
                                            )
                                            VALUES
                                            (
                                                @ChannelId
                                                ,@StoreId
                                                ,@TerminalId
                                                ,@ShiftId
                                                ,@StaffId
    		                                    ,@CurrentStaffId
                                                ,@Status
                                                ,@CurrentTerminalId
                                                ,@StatusDateTimeUTC
                                                ,@StatusDateTimeUTC
                                                ,@DataAreaId
                                                ,@CashDrawer
                                            )";
    
                var sqlQuery = new SqlQuery(InsertQuery);
    
                sqlQuery.Parameters["@ChannelId"] = context.ChannelId;
                sqlQuery.Parameters["@StoreId"] = shift.StoreId;
                sqlQuery.Parameters["@TerminalId"] = shift.TerminalId;
                sqlQuery.Parameters["@ShiftId"] = shift.ShiftId;
                sqlQuery.Parameters["@StaffId"] = shift.StaffId;
                sqlQuery.Parameters["@CurrentStaffId"] = shift.CurrentStaffId;
                sqlQuery.Parameters["@Status"] = shift.Status;
                sqlQuery.Parameters["@CurrentTerminalId"] = shift.CurrentTerminalId;
                sqlQuery.Parameters["@StatusDateTimeUTC"] = shift.StartDateTime;
                sqlQuery.Parameters["@DataAreaId"] = context.DataAreaId;
                sqlQuery.Parameters["@CashDrawer"] = shift.CashDrawer;
    
                context.ExecuteNonQuery(sqlQuery);
            }

            /// <summary>
            /// Deletes the shift in shift staging table.
            /// </summary>
            /// <param name="context">The SQLite database context.</param>
            /// <param name="shift">The shift object.</param>
            public static void DeleteShift(SqliteDatabaseContext context, Shift shift)
            {
                const string DeleteQuery = @"DELETE FROM [crt].RETAILSHIFTSTAGINGTABLE
                                    WHERE
                                 [crt].RETAILSHIFTSTAGINGTABLE.CHANNEL = @ChannelId AND
                                 [crt].RETAILSHIFTSTAGINGTABLE.TERMINALID = @TerminalId AND
                                 [crt].RETAILSHIFTSTAGINGTABLE.SHIFTID = @ShiftId";

                var sqlQuery = new SqlQuery(DeleteQuery);

                sqlQuery.Parameters["@ChannelId"] = context.ChannelId;
                sqlQuery.Parameters["@TerminalId"] = shift.TerminalId;
                sqlQuery.Parameters["@ShiftId"] = shift.ShiftId;

                context.ExecuteNonQuery(sqlQuery);
            }
        }
    }
}
