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
    namespace Commerce.Runtime.Services.PricingEngine
    {
        using System;
        using System.Diagnostics;
        using System.Diagnostics.CodeAnalysis;
        using System.Text;
    
        /// <summary>
        /// Simple profiler.
        /// </summary>
        public sealed class SimpleProfiler : IDisposable
        {
            [ThreadStatic]
            private static bool enabled = false;
    
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Violation is caused when not in debug mode because this variable is used for debugging purposes only.")]
            private string name;
            private int indent;
            private DateTime startTime;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleProfiler"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public SimpleProfiler(string name)
                : this(name, false, 0)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleProfiler"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="indent">Indent times.</param>
            public SimpleProfiler(string name, int indent)
                : this(name, false, indent)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleProfiler"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="logStart">Log start.</param>
            /// <param name="indent">Indent times.</param>
            public SimpleProfiler(string name, bool logStart, int indent)
            {
                if (enabled)
                {
                    this.name = name;
                    this.indent = indent;
                    this.startTime = DateTime.Now;
                    if (logStart)
                    {
                        Debug.WriteLine("{0}[{1}] started: {2}", this.GetPrefixSpaces(), this.name, this.startTime);
                    }
                }
            }
    
            /// <summary>
            /// Gets or sets a value indicating whether it's enabled.
            /// </summary>
            public static bool Enabled
            {
                get { return enabled; }
                set { enabled = value; }
            }
    
            /// <summary>
            /// Dispose and log.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "timeLapsed", Justification = "The variable is used for logging.")]
            public void Dispose()
            {
                if (enabled)
                {
                    DateTime endTime = DateTime.Now;
                    TimeSpan timeLapsed = endTime.Subtract(this.startTime);
    
                    Debug.WriteLine("{0}[{1}] started: {2} ended: {3}, lasting {4}", this.GetPrefixSpaces(), this.name, this.startTime, endTime, timeLapsed);
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in debug build.")]
            private string GetPrefixSpaces()
            {
                StringBuilder prefixSpaces = new StringBuilder();
                for (int i = 0; i < this.indent; i++)
                {
                    prefixSpaces.Append("  ");
                }
    
                return prefixSpaces.ToString();
            }
        }
    }
}
