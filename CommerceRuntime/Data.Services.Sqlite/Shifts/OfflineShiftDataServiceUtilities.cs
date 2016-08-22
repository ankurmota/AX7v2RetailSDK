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

        internal static class OfflineShiftDataServiceUtilities
        {
            public const string OfflineShiftsView =
                @"SELECT
                    SHIFT.SHIFTID AS SHIFTID,
                    SHIFT.CASHDRAWER AS CASHDRAWER,
                    SHIFT.CHANNEL AS CHANNEL,
                    SHIFT.CURRENTTERMINALID AS CURRENTTERMINALID,
                    NULL AS CLOSEDATE,
                    NULL AS CLOSEDATETIMEUTC,
                    NULL AS CLOSEDATETIMEUTCTZID,
                    0 AS CLOSETIME,
                    0 AS CUSTOMERSCOUNT,
                    0.0 AS DISCOUNTTOTAL,
                    0 AS LOGONSCOUNT,
                    0 AS NOSALECOUNT,
                    0.0 AS PAIDTOACCOUNTTOTAL,
                    0.0 AS RETURNSTOTAL,
                    0.0 AS ROUNDEDAMOUNTTOTAL,
                    0 AS SALESCOUNT,
                    0.0 AS SALESTOTAL,
                    SHIFT.STAFFID AS STAFFID,
                    SHIFT.CURRENTSTAFFID AS CURRENTSTAFFID,
                    NULL AS STARTDATE,
                    SHIFT.STARTDATETIMEUTC AS STARTDATETIMEUTC,
                    NULL AS STARTDATETIMEUTCTZID,
                    0 AS STARTTIME,
                    SHIFT.STATUS AS STATUS,
                    SHIFT.STATUSDATETIMEUTC AS STATUSDATETIMEUTC,
                    STORE.STORENUMBER AS STOREID,
                    0.0 AS TAXTOTAL,
                    SHIFT.TERMINALID AS TERMINALID,
                    0 AS TRANSACTIONSCOUNT,
                    0 AS VOIDSCOUNT,
                    SHIFT.DATAAREAID AS DATAAREAID,
                    SHIFT.ROWVERSION AS ROWVERSION
                  FROM crt_RETAILSHIFTSTAGINGTABLE AS SHIFT
                       INNER JOIN ax_RETAILPUBRETAILSTORETABLE AS STORE ON SHIFT.CHANNEL = STORE.STOREORIGINID ";
        }
    }
}
