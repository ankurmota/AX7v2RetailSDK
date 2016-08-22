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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /* ****************************  DESTINATION BASED TAXATION ALGORITHM (in a nutshell) ***************************************
         * Each filter is a stage in DBT algorithm depicted by the functional spec. It is a waterfall process flowing from -> to
         * ZipFilter -> DistrictFilter -> CityFilter -> CountyFilter -> StateFilter -> CountryFilter
         * There is mainly two phases of the algorithms:
         * (Phase-A) Filter Determination : This is the stage in which the approprite filter is selected, based on the existence of
         *                                  various address components in the input
         *
         * (Phase-B) Finding a Matching   : Based on the filter picked up in the previous phase, the DB is queried with the predicates and
         *              If there is a match Phase B succeeds.
         *              If not, the cycle from Phase-A to Phase-B repeated starting with the next filter.
         * ***************************************************************************************************************************/
    
        /// <summary>
        /// Returns whether or not address can be processed based on the type of filter.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>A value indicating whether the address can be handled.</returns>
        public delegate bool CanHandle(Address address);
    
        /// <summary>
        /// Populates the predicates.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="predicates">The predicates.</param>
        public delegate void Handle(Address address, IDictionary<string, string> predicates);
    
        /// <summary>
        /// Base class for all the filters implemented (and for those which might come later in development.
        /// </summary>
        internal sealed class DestinationFilterHandler
        {
            private const string CountryComponentName = "COUNTRYREGIONID";
            private const string StateComponentName = "STATEID";
            private const string CountyComponentName = "COUNTYID";
            private const string CityComponentName = "CITY";
            private const string DistrictComponentName = "DISTRICT";
            private const string ZipPostalCodeComponentName = "ZIPCODE";
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DestinationFilterHandler"/> class.
            /// </summary>
            /// <param name="canHandleDelegate">The can handle delegate.</param>
            /// <param name="handleDelegate">The handle delegate.</param>
            public DestinationFilterHandler(CanHandle canHandleDelegate, Handle handleDelegate)
            {
                this.CanHandle = canHandleDelegate;
                this.Handle = handleDelegate;
            }
    
            /// <summary>
            /// Gets the delegate that determines whether this filter type can handle the given address or not.
            /// </summary>
            /// <remarks>
            /// This boolean result will yield either a Handle() call or invoking the next filter in the chain to see if that one satisfies the need.
            /// </remarks>
            public CanHandle CanHandle { get; private set; }
    
            /// <summary>
            /// Gets the value of the address component.
            /// </summary>
            public Handle Handle { get; private set; }
    
            /// <summary>
            /// Builds instance of country filter.
            /// </summary>
            /// <returns>The destination filter handler.</returns>
            internal static DestinationFilterHandler BuildCountryFilter()
            {
                return new DestinationFilterHandler(
                    (Address address) =>
                    {
                        if (string.IsNullOrWhiteSpace(address.ThreeLetterISORegionName) &&
                            string.IsNullOrWhiteSpace(address.TwoLetterISORegionName))
                        {
                            throw new ArgumentException("CountryRegionId is not set"); // Country is mandatory field.
                        }
    
                        return true;
                    },
                    (Address address, IDictionary<string, string> predicates) =>
                    {
                        if (!string.IsNullOrWhiteSpace(address.ThreeLetterISORegionName))
                        {
                            predicates.Add(CountryComponentName, address.ThreeLetterISORegionName);
                        }
                        else
                        {
                            predicates.Add(CountryComponentName, address.TwoLetterISORegionName);
                        }
                    });
            }
    
            /// <summary>
            /// Builds instance of state filter.
            /// </summary>
            /// <returns>
            /// The destination filter handler.
            /// </returns>
            internal static DestinationFilterHandler BuildStateFilter()
            {
                return new DestinationFilterHandler(
                    (Address address) =>
                    {
                        return !string.IsNullOrWhiteSpace(address.State);
                    },
                    (Address address, IDictionary<string, string> predicates) =>
                    {
                        predicates.Add(StateComponentName, address.State);
                    });
            }
    
            /// <summary>
            /// Builds instance of county filter.
            /// </summary>
            /// <returns>
            /// The destination filter handler.
            /// </returns>
            internal static DestinationFilterHandler BuildCountyFilter()
            {
                return new DestinationFilterHandler(
                    (Address address) =>
                    {
                        return !string.IsNullOrWhiteSpace(address.County);
                    },
                    (Address address, IDictionary<string, string> predicates) =>
                    {
                        predicates.Add(CountyComponentName, address.County);
                    });
            }
    
            /// <summary>
            /// Builds instance of city filter.
            /// </summary>
            /// <returns>
            /// The destination filter handler.
            /// </returns>
            internal static DestinationFilterHandler BuildCityFilter()
            {
                return new DestinationFilterHandler(
                    (Address address) =>
                    {
                        return !string.IsNullOrWhiteSpace(address.City);
                    },
                    (Address address, IDictionary<string, string> predicates) =>
                    {
                        predicates.Add(CityComponentName, address.City);
                    });
            }
    
            /// <summary>
            /// Builds the district filter.
            /// </summary>
            /// <returns>
            /// The destination filter handler.
            /// </returns>
            internal static DestinationFilterHandler BuildDistrictFilter()
            {
                return new DestinationFilterHandler(
                    (Address address) =>
                    {
                        return !string.IsNullOrWhiteSpace(address.DistrictName);
                    },
                    (Address address, IDictionary<string, string> predicates) =>
                    {
                        predicates.Add(DistrictComponentName, address.DistrictName);
                    });
            }
    
            /// <summary>
            /// Builds instance of postal code filter.
            /// </summary>
            /// <returns>The destination filter handler.</returns>
            internal static DestinationFilterHandler BuildZipPostalCodeFilter()
            {
                return new DestinationFilterHandler(
                    (Address address) =>
                    {
                        return !string.IsNullOrWhiteSpace(address.ZipCode);
                    },
                    (Address address, IDictionary<string, string> predicates) =>
                    {
                        predicates.Add(ZipPostalCodeComponentName, address.ZipCode);
                    });
            }
        }
    }
}
