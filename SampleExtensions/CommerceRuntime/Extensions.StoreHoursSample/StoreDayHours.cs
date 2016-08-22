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
    namespace Commerce.Runtime.DataModel
    {
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Defines a simple class that holds information about opening and closing times for a particular day.
        /// </summary>
        public class StoreDayHours : CommerceEntity
        {
            private const string DayColumn = "DAY";
            private const string OpenTimeColumn = "OPENTIME";
            private const string CloseTimeColumn = "CLOSINGTIME";
            private const string IdColumn = "RECID";

            /// <summary>
            /// Initializes a new instance of the <see cref="StoreDayHours"/> class.
            /// </summary>
            public StoreDayHours()
                : base("StoreDayHours")
            {
            }

            /// <summary>
            /// Gets or sets the day of the week.
            /// </summary>
            [DataMember]
            [Column(DayColumn)]
            public int DayOfWeek
            {
                get { return (int)this[DayColumn]; }
                set { this[DayColumn] = value; }
            }

            /// <summary>
            /// Gets or sets the open time.
            /// </summary>
            [DataMember]
            [Column(OpenTimeColumn)]
            public int OpenTime
            {
                get { return (int)this[OpenTimeColumn]; }
                set { this[OpenTimeColumn] = value; }
            }

            /// <summary>
            /// Gets or sets the closing time.
            /// </summary>
            [DataMember]
            [Column(CloseTimeColumn)]
            public int CloseTime
            {
                get { return (int)this[CloseTimeColumn]; }
                set { this[CloseTimeColumn] = value; }
            }

            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            [Key]
            [DataMember]
            [Column(IdColumn)]
            public long Id
            {
                get { return (long)this[IdColumn]; }
                set { this[IdColumn] = value; }
            }
        }
    }
}