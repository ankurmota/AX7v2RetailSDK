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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Diagnostics.CodeAnalysis;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Data contract of filtering criteria for Transaction Service 'Get' APIs.
        /// </summary>
        public class FilteringCriteria
        {
            private const string ResultsStartIndexKey = "ResultsStartIndex";
            private const string ResultsMaxCountKey = "ResultsMaxCount";
            private const string StartTimeKey = "StartTime";
            private const string StartTimeAxDateStringKey = "StartTimeAxDateString";
            private const string StartTimeAxTimeStringKey = "StartTimeAxTimeString";
            private const string EndTimeKey = "EndTime";
            private const string EndTimeAxDateStringKey = "EndTimeAxDateString";
            private const string EndTimeAxTimeStringKey = "EndTimeAxTimeString";
            private const string AxDateSequenceKey = "AxDateSequence";
            private const int DefaultAxDateSequence = 321;
            private readonly ParameterSet parameters;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="FilteringCriteria"/> class.
            /// </summary>
            public FilteringCriteria()
            {
                this.parameters = new ParameterSet();
                this.AxDateSequence = DefaultAxDateSequence;
            }
    
            /// <summary>
            /// Gets or sets the start index of the results.
            /// </summary>
            /// <value>
            /// The start index of the results.
            /// </value>
            public int ResultsStartIndex
            {
                get
                {
                    return (int)this.parameters[ResultsStartIndexKey];
                }
    
                set
                {
                    this.parameters[ResultsStartIndexKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the max count of the results.
            /// </summary>
            /// <value>
            /// The results max count.
            /// </value>
            public int ResultsMaxCount
            {
                get
                {
                    return (int)this.parameters[ResultsMaxCountKey];
                }
    
                set
                {
                    this.parameters[ResultsMaxCountKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the ax date sequence.
            /// </summary>
            /// <value>
            /// The ax date sequence.
            /// </value>
            public int AxDateSequence
            {
                get
                {
                    return (int)this.parameters[AxDateSequenceKey];
                }
    
                set
                {
                    this.parameters[AxDateSequenceKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the start time for the query.
            /// </summary>
            /// <value>
            /// From date.
            /// </value>
            /// <remarks>
            /// Regarding serializations:
            /// Just use the standard serialization for DateTime won't work for different locales in AX.
            /// We need to use a helper method to manually serialize this into 3 pieces:
            /// 1. The AX date string.
            /// 2. The AX time string.
            /// 2. The sequence number for AX date string.
            /// </remarks>
            [XmlIgnore]
            public DateTimeOffset StartTime
            {
                get
                {
                    return (DateTimeOffset)(this.parameters[StartTimeKey] ?? DateTimeOffsetExtensions.AxMinDateValue);
                }

                set
                {
                    this.parameters[StartTimeKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the start time as an AX date string.
            /// </summary>
            public string StartTimeAxDateString
            {
                get
                {
                    return Serialization.SerializationHelper.ConvertDateTimeToAXDateString(this.StartTime, this.AxDateSequence);
                }
    
                set
                {
                    this.parameters[StartTimeAxDateStringKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the start time as an AX time string.
            /// </summary>
            public string StartTimeAxTimeString
            {
                get
                {
                    return Serialization.SerializationHelper.ConvertDateTimeToAXTimeString(this.StartTime);
                }
    
                set
                {
                    this.parameters[StartTimeAxTimeStringKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the end time for the query.
            /// </summary>
            /// <value>
            /// To end date.
            /// </value>
            /// <remarks>
            /// Regarding serializations:
            /// Just use the standard serialization for DateTime won't work for different locales in AX.
            /// We need to use a helper method to manually serialize this into 3 pieces:
            /// 1. The AX date string.
            /// 2. The AX time string.
            /// 2. The sequence number for AX date string.
            /// </remarks>
            [XmlIgnore]
            public DateTimeOffset EndTime
            {
                get
                {
                    return (DateTimeOffset)(this.parameters[EndTimeKey] ?? DateTimeOffsetExtensions.AxMinDateValue);
                }
    
                set
                {
                    this.parameters[EndTimeKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the end time as an AX date string.
            /// </summary>
            public string EndTimeAxDateString
            {
                get
                {
                    return Serialization.SerializationHelper.ConvertDateTimeToAXDateString(this.EndTime, this.AxDateSequence);
                }
    
                set
                {
                    this.parameters[EndTimeAxDateStringKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the end time as an AX time string.
            /// </summary>
            public string EndTimeAxTimeString
            {
                get
                {
                    return Serialization.SerializationHelper.ConvertDateTimeToAXTimeString(this.EndTime);
                }
    
                set
                {
                    this.parameters[EndTimeAxTimeStringKey] = value;
                }
            }
    
            /// <summary>
            /// Gets or sets the value associated with the specified key.
            /// </summary>
            /// <param name="key">The specified key.</param>
            /// <returns>
            /// The value associated with the specified key or NULL if the specified key does not exist.
            /// </returns>
            public object this[string key]
            {
                get
                {
                    return this.parameters[key];
                }
    
                set
                {
                    this.parameters[key] = value;
                }
            }
        }
    }
}
