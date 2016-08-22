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
        using Commerce.Runtime.Data.Sqlite; 
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// The address procedure class contains the address SQLite queries.
        /// </summary>
        internal static class AddressQueries
        {
            public static PagedResult<AddressFormattingInfo> GetAddressFormatInfo(SqliteDatabaseContext context, TempTable countryRegionIdsTempTable)
            {
                string queryString = @"SELECT LACR.COUNTRYREGIONID
                                           ,LAFL.ELEMENT
                                           ,CAST(LAFL.LINENUM AS INT) AS LINEINDEX
                                           ,LAFL.DATAENTRYONLY
    	                                   ,LAFL.SEPARATOR
    	                                   ,LAFL.SEPARATORCRLF
    	                                   ,LAFL.INACTIVE
    	                                   ,LAFL.EXPAND
    	                                   ,LAFL.SPECIAL
    	                                   ,LAFL.NUMOFSPACES
                                        FROM {0} CRI
                                        INNER JOIN [ax].LOGISTICSADDRESSCOUNTRYREGION LACR ON LACR.COUNTRYREGIONID = CRI.RECID
                                        INNER JOIN [ax].LOGISTICSADDRESSFORMATHEADING LAFH ON LACR.ADDRFORMAT = LAFH.ADDRFORMAT
                                        INNER JOIN [ax].LOGISTICSADDRESSFORMATLINES LAFL ON LAFH.ADDRFORMAT = LAFL.ADDRFORMAT
                                        WHERE LAFL.INACTIVE = 0
                                        ORDER BY LINEINDEX ASC";
    
                SqlQuery sqlQuery = new SqlQuery(queryString, countryRegionIdsTempTable.TableName);
    
                return context.ReadEntity<AddressFormattingInfo>(sqlQuery);
            }
    
            public static PagedResult<CountryRegionInfo> GetCountryRegionInfo(SqliteDatabaseContext context, string languageId, TempTable countryRegionIdsTempTable)
            {
                string queryString = @"SELECT LCNTRY.COUNTRYREGIONID
                                                ,LCNTRY.ISOCODE
                                                ,LCNTRY.TIMEZONE
                                                ,LCNTRY.ADDRFORMAT AS ADDRESSFORMATID
                                                ,LFORMAT.NAME AS ADDRESSFORMATNAME
                                                ,LTRANS.SHORTNAME
                                                ,LTRANS.LONGNAME
                                                ,LTRANS.LANGUAGEID
                                            FROM {0} CRI
                                            INNER JOIN [ax].LOGISTICSADDRESSCOUNTRYREGION LCNTRY ON LCNTRY.COUNTRYREGIONID = CRI.RECID
                                            INNER JOIN [ax].LOGISTICSADDRESSCOUNTRYREGIONTRANSLATION LTRANS ON LTRANS.COUNTRYREGIONID = LCNTRY.COUNTRYREGIONID
                                            INNER JOIN [ax].LOGISTICSADDRESSFORMATHEADING LFORMAT ON LFORMAT.ADDRFORMAT = LCNTRY.ADDRFORMAT
                                            WHERE (@nvc_LanguageId IS NULL OR LTRANS.LANGUAGEID = @nvc_LanguageId)";
    
                SqlQuery sqlQuery = new SqlQuery(queryString, countryRegionIdsTempTable.TableName);
    
                sqlQuery.Parameters["@nvc_LanguageId"] = languageId;
    
                return context.ReadEntity<CountryRegionInfo>(sqlQuery);
            }
    
            public static PagedResult<CountryRegionInfo> GetCountryRegionIds(SqliteDatabaseContext context, string languageId)
            {
                const string QueryString = @"SELECT DISTINCT LCNTRY.COUNTRYREGIONID
                                                FROM [ax].LOGISTICSADDRESSCOUNTRYREGION LCNTRY
                                                INNER JOIN [ax].LOGISTICSADDRESSCOUNTRYREGIONTRANSLATION LTRANS ON LTRANS.COUNTRYREGIONID = LCNTRY.COUNTRYREGIONID
                                                WHERE (@nvc_LanguageId IS NULL OR LTRANS.LANGUAGEID = @nvc_LanguageId)";
    
                SqlQuery sqlQuery = new SqlQuery(QueryString);
                sqlQuery.Parameters["@nvc_LanguageId"] = languageId;
    
                return context.ReadEntity<CountryRegionInfo>(sqlQuery);
            }
        }
    }
}
