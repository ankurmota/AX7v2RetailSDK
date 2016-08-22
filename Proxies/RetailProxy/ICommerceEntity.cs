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
    namespace Commerce.RetailProxy
    {
        using System;
        using System.ComponentModel;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Interface for commerce entities.
        /// </summary>
        public interface ICommerceEntity : INotifyPropertyChanged
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance is notification disabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is notification disabled; otherwise, <c>false</c>.
            /// </value>
            bool IsNotificationDisabled { get; set; }
        }
    }
}
