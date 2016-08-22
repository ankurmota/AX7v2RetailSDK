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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Rounding class.
        /// </summary>
        public static class Rounding
        {
            internal const decimal DefaultRoundingValue = 0.01M;

            /// <summary>
            /// Converts round off type to enumeration.
            /// </summary>
            /// <param name="roundOffType">Round off type as defined in AX.</param>
            /// <returns>Rounding method.</returns>
            public static RoundingMethod ConvertRoundOffTypeToRoundingMethod(int roundOffType)
            {
                RoundingMethod roundingMethod;
                switch (roundOffType)
                {
                    case 0:
                        roundingMethod = RoundingMethod.Nearest;
                        break;
                    case 1:
                        roundingMethod = RoundingMethod.Down;
                        break;
                    case 2:
                        roundingMethod = RoundingMethod.Up;
                        break;
                    default:
                        roundingMethod = RoundingMethod.Nearest;
                        break;
                }

                return roundingMethod;
            }

            /// <summary>
            /// Rounds values to nearest currency unit, i.e 16,45 kr. rounded up if the smallest coin is 10 kr will give 20 kr.
            /// or if the smallest coin is 24 aurar(0,25 kr.) then if rounded up it will give 16,50 kr.
            /// </summary>
            /// <param name="value">The currency value or value to be rounded.</param>
            /// <param name="unit">The smallest unit to be rounded to.</param>
            /// <param name="roundMethod">The method of rounding (i.e. nearest, up or down).</param>
            /// <returns>Returns a value rounded to the nearest unit.</returns>
            public static decimal RoundToUnit(decimal value, decimal unit, RoundingMethod roundMethod)
            {
                if (unit == decimal.Zero)
                {
                    unit = DefaultRoundingValue;
                }

                if (roundMethod == RoundingMethod.None)
                {
                    roundMethod = RoundingMethod.Nearest;
                }

                decimal decimalValue = value / unit;
                decimal difference = Math.Abs(decimalValue) - Math.Abs(Math.Truncate(value / unit));

                // is rounding required?
                if (difference > 0)
                {
                    switch (roundMethod)
                    {
                        case RoundingMethod.Nearest:
                            {
                                return Math.Round(Math.Round(value / unit, 0, MidpointRounding.AwayFromZero) * unit, GetNumberOfDecimals(unit), MidpointRounding.AwayFromZero);
                            }

                        case RoundingMethod.Down:
                            {
                                if (value > 0M)
                                {
                                    return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                }

                                return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                            }

                        case RoundingMethod.Up:
                            {
                                if (value > 0M)
                                {
                                    return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                }

                                return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                            }
                    }
                }
                else if (difference == 0M)
                {
                    // for scenarios like value == 69.9900000000, the unit == 0.01 and the difference == 0.000000000
                    return Math.Round(value, GetNumberOfDecimals(unit));
                }

                return value;
            }

            private static int GetNumberOfDecimals(decimal round)
            {
                int number = 0;
                if (round < 1)
                {
                    while (round != 0)
                    {
                        round = (round * 10M) % 1;
                        number++;
                    }
                }

                return number;
            }
        }
    }
}