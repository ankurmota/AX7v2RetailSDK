/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='DateTimeFormat.ts'/>
///<reference path='IDateTimeFormatter.ts'/>
///<reference path='../../moment-timezone.d.ts'/>

module Commerce.Host.Globalization {
    "use strict";

    interface ITimeZoneInfoMap {
        windowsId: string;
        ianaId: string;
    }

    /**
     * Date time formatter based on Moment-Timezone library.
     */
    export class TimeZoneDateTimeFormatter implements IDateTimeFormatter {

        private static _timeZoneMap: Dictionary<ITimeZoneInfoMap> = TimeZoneDateTimeFormatter.getTimezoneMap();

        private _format: string;
        private _locale: string;
        private _timeZone: ITimeZoneInfoMap;

        /**
         * Constructor.
         * @param {string} format The format template. See {@link Commerce.Host.Globalization.Host.DateTimeFormat} for predefined formats.
         * @param {string} locale The locale as IEFT language tag.
         * @param {string} timeZone The Windows time zone id used to format dates.
         */
        constructor(format: string, locale?: string, timeZoneId?: string) {
            this._format = TimeZoneDateTimeFormatter.dateTimeFormat2MomentFormat(format);
            this._locale = locale || Commerce.Host.instance.globalization.getApplicationLanguage();
            this._timeZone = TimeZoneDateTimeFormatter._timeZoneMap.getItem(timeZoneId);
        }

        /**
         * Gets the map between Windows and IANA time zone identifiers.
         * @remark The source of the map is Unicode Common Locale Data Repository.
         * @param {Dictionary<ITimeZoneInfoMap>} The timezones identifiers map.
         */
        private static getTimezoneMap(): Dictionary<ITimeZoneInfoMap> {
            var map: Dictionary<ITimeZoneInfoMap> = new Dictionary<ITimeZoneInfoMap>();

            map.setItems([
                { windowsId: "Dateline Standard Time", ianaId: "Etc/GMT+12" },
                { windowsId: "UTC-11", ianaId: "Etc/GMT+11" },
                { windowsId: "Hawaiian Standard Time", ianaId: "Pacific/Honolulu" },
                { windowsId: "Alaskan Standard Time", ianaId: "America/Anchorage" },
                { windowsId: "Pacific Standard Time (Mexico)", ianaId: "America/Santa_Isabel" },
                { windowsId: "Pacific Standard Time", ianaId: "America/Los_Angeles" },
                { windowsId: "US Mountain Standard Time", ianaId: "America/Phoenix" },
                { windowsId: "Mountain Standard Time (Mexico)", ianaId: "America/Chihuahua" },
                { windowsId: "Mountain Standard Time", ianaId: "America/Denver" },
                { windowsId: "Central America Standard Time", ianaId: "America/Guatemala" },
                { windowsId: "Central Standard Time", ianaId: "America/Chicago" },
                { windowsId: "Central Standard Time (Mexico)", ianaId: "America/Mexico_City" },
                { windowsId: "Canada Central Standard Time", ianaId: "America/Regina" },
                { windowsId: "SA Pacific Standard Time", ianaId: "America/Bogota" },
                { windowsId: "Eastern Standard Time", ianaId: "America/New_York" },
                { windowsId: "US Eastern Standard Time", ianaId: "America/Indiana/Indianapolis" },
                { windowsId: "Venezuela Standard Time", ianaId: "America/Caracas" },
                { windowsId: "Paraguay Standard Time", ianaId: "America/Asuncion" },
                { windowsId: "Atlantic Standard Time", ianaId: "America/Halifax" },
                { windowsId: "Central Brazilian Standard Time", ianaId: "America/Cuiaba" },
                { windowsId: "SA Western Standard Time", ianaId: "America/La_Paz" },
                { windowsId: "Pacific SA Standard Time", ianaId: "America/Santiago" },
                { windowsId: "Newfoundland Standard Time", ianaId: "America/St_Johns" },
                { windowsId: "E. South America Standard Time", ianaId: "America/Sao_Paulo" },
                { windowsId: "Argentina Standard Time", ianaId: "America/Argentina/Buenos_Aires" },
                { windowsId: "SA Eastern Standard Time", ianaId: "America/Cayenne" },
                { windowsId: "Greenland Standard Time", ianaId: "America/Godthab" },
                { windowsId: "Montevideo Standard Time", ianaId: "America/Montevideo" },
                { windowsId: "Bahia Standard Time", ianaId: "America/Bahia" },
                { windowsId: "UTC-02", ianaId: "Etc/GMT+2" },
                { windowsId: "Mid-Atlantic Standard Time", ianaId: "Etc/GMT+2" },
                { windowsId: "Azores Standard Time", ianaId: "Atlantic/Azores" },
                { windowsId: "Cape Verde Standard Time", ianaId: "Atlantic/Cape_Verde" },
                { windowsId: "Morocco Standard Time", ianaId: "Africa/Casablanca" },
                { windowsId: "UTC", ianaId: "Etc/UTC" },
                { windowsId: "GMT Standard Time", ianaId: "Europe/London" },
                { windowsId: "Greenwich Standard Time", ianaId: "Atlantic/Reykjavik" },
                { windowsId: "W. Europe Standard Time", ianaId: "Europe/Berlin" },
                { windowsId: "Central Europe Standard Time", ianaId: "Europe/Budapest" },
                { windowsId: "Romance Standard Time", ianaId: "Europe/Paris" },
                { windowsId: "Central European Standard Time", ianaId: "Europe/Warsaw" },
                { windowsId: "W. Central Africa Standard Time", ianaId: "Africa/Lagos" },
                { windowsId: "Namibia Standard Time", ianaId: "Africa/Windhoek" },
                { windowsId: "Jordan Standard Time", ianaId: "Asia/Amman" },
                { windowsId: "GTB Standard Time", ianaId: "Europe/Bucharest" },
                { windowsId: "Middle East Standard Time", ianaId: "Asia/Beirut" },
                { windowsId: "Egypt Standard Time", ianaId: "Africa/Cairo" },
                { windowsId: "Syria Standard Time", ianaId: "Asia/Damascus" },
                { windowsId: "E. Europe Standard Time", ianaId: "Europe/Istanbul" },
                { windowsId: "South Africa Standard Time", ianaId: "Africa/Johannesburg" },
                { windowsId: "FLE Standard Time", ianaId: "Europe/Kiev" },
                { windowsId: "Turkey Standard Time", ianaId: "Europe/Istanbul" },
                { windowsId: "Israel Standard Time", ianaId: "Asia/Jerusalem" },
                { windowsId: "Kaliningrad Standard Time", ianaId: "Europe/Kaliningrad" },
                { windowsId: "Libya Standard Time", ianaId: "Africa/Tripoli" },
                { windowsId: "Arabic Standard Time", ianaId: "Asia/Baghdad" },
                { windowsId: "Arab Standard Time", ianaId: "Asia/Riyadh" },
                { windowsId: "Belarus Standard Time", ianaId: "Europe/Moscow" },
                { windowsId: "Russian Standard Time", ianaId: "Europe/Moscow" },
                { windowsId: "E. Africa Standard Time", ianaId: "Africa/Nairobi" },
                { windowsId: "Iran Standard Time", ianaId: "Asia/Tehran" },
                { windowsId: "Arabian Standard Time", ianaId: "Asia/Dubai" },
                { windowsId: "Azerbaijan Standard Time", ianaId: "Asia/Baku" },
                { windowsId: "Russia Time Zone 3", ianaId: "Asia/Baku" },
                { windowsId: "Mauritius Standard Time", ianaId: "Indian/Mauritius" },
                { windowsId: "Georgian Standard Time", ianaId: "Asia/Tbilisi" },
                { windowsId: "Caucasus Standard Time", ianaId: "Asia/Yerevan" },
                { windowsId: "Afghanistan Standard Time", ianaId: "Asia/Kabul" },
                { windowsId: "West Asia Standard Time", ianaId: "Asia/Tashkent" },
                { windowsId: "Ekaterinburg Standard Time", ianaId: "Asia/Yekaterinburg" },
                { windowsId: "Pakistan Standard Time", ianaId: "Asia/Karachi" },
                { windowsId: "India Standard Time", ianaId: "Asia/Kolkata" },
                { windowsId: "Sri Lanka Standard Time", ianaId: "Asia/Colombo" },
                { windowsId: "Nepal Standard Time", ianaId: "Asia/Kathmandu" },
                { windowsId: "Central Asia Standard Time", ianaId: "Asia/Almaty" },
                { windowsId: "Bangladesh Standard Time", ianaId: "Asia/Dhaka" },
                { windowsId: "N. Central Asia Standard Time", ianaId: "Asia/Novosibirsk" },
                { windowsId: "Myanmar Standard Time", ianaId: "Asia/Rangoon" },
                { windowsId: "SE Asia Standard Time", ianaId: "Asia/Bangkok" },
                { windowsId: "North Asia Standard Time", ianaId: "Asia/Krasnoyarsk" },
                { windowsId: "China Standard Time", ianaId: "Asia/Shanghai" },
                { windowsId: "North Asia East Standard Time", ianaId: "Asia/Irkutsk" },
                { windowsId: "Singapore Standard Time", ianaId: "Asia/Singapore" },
                { windowsId: "W. Australia Standard Time", ianaId: "Australia/Perth" },
                { windowsId: "Taipei Standard Time", ianaId: "Asia/Taipei" },
                { windowsId: "Ulaanbaatar Standard Time", ianaId: "Asia/Ulaanbaatar" },
                { windowsId: "Tokyo Standard Time", ianaId: "Asia/Tokyo" },
                { windowsId: "Korea Standard Time", ianaId: "Asia/Seoul" },
                { windowsId: "Yakutsk Standard Time", ianaId: "Asia/Yakutsk" },
                { windowsId: "Cen. Australia Standard Time", ianaId: "Australia/Adelaide" },
                { windowsId: "AUS Central Standard Time", ianaId: "Australia/Darwin" },
                { windowsId: "E. Australia Standard Time", ianaId: "Australia/Brisbane" },
                { windowsId: "AUS Eastern Standard Time", ianaId: "Australia/Sydney" },
                { windowsId: "West Pacific Standard Time", ianaId: "Pacific/Port_Moresby" },
                { windowsId: "Tasmania Standard Time", ianaId: "Australia/Hobart" },
                { windowsId: "Magadan Standard Time", ianaId: "Asia/Magadan" },
                { windowsId: "Vladivostok Standard Time", ianaId: "Asia/Vladivostok" },
                { windowsId: "Russia Time Zone 10", ianaId: "Pacific/Guadalcanal" },
                { windowsId: "Central Pacific Standard Time", ianaId: "Pacific/Guadalcanal" },
                { windowsId: "Russia Time Zone 11", ianaId: "Pacific/Auckland" },
                { windowsId: "New Zealand Standard Time", ianaId: "Pacific/Auckland" },
                { windowsId: "UTC+12", ianaId: "Etc/GMT-12" },
                { windowsId: "Fiji Standard Time", ianaId: "Pacific/Fiji" },
                { windowsId: "Kamchatka Standard Time", ianaId: "Etc/GMT-12" },
                { windowsId: "Tonga Standard Time", ianaId: "Pacific/Tongatapu" },
                { windowsId: "Samoa Standard Time", ianaId: "Pacific/Apia" },
                { windowsId: "Line Islands Standard Time", ianaId: "Pacific/Kiritimati" }
            ], (mapItem: ITimeZoneInfoMap) => mapItem.windowsId);

            return map;
        }

        private static dateTimeFormat2MomentFormat(format: string): string {
            switch (format) {
                case DateTimeFormat.SHORT_DATE:
                    return "L"; // Localized Date - 09/04/1986
                case DateTimeFormat.DATE_TIME:
                    return "L LT"; // Localized date and time - 09/04/1986 8:30 PM
                case DateTimeFormat.SHORT_TIME:
                    return "LT"; // Localized time - 8:30 PM
                case DateTimeFormat.LONG_TIME:
                    return "hh:mm:ss A"; // Time with seconds - 08:30:12 PM
                case DateTimeFormat.MONTH_FULL:
                    return "MMMM"; // January February ... November December
                default:
                    throw new Error("The format is not supported."); // note: add a new format to Commerce.Host.Globalization.DateTimeFormat.
            }
        }

        /**
         * Returns the formatted date.
         * @param {Date} value The date.
         * @return {string} The date as a formatted string.
         */
        public format(value: Date): string {

            if (ObjectExtensions.isNullOrUndefined(value)) {
                return StringExtensions.EMPTY;
            }

             var momentDate: moment.Moment = moment(value);

            if (this._locale) {
                momentDate = momentDate.locale(this._locale);
            }

            if (this._timeZone) {
                momentDate = momentDate.tz(this._timeZone.ianaId);
            }

            return momentDate.format(this._format);
        }
    }
}