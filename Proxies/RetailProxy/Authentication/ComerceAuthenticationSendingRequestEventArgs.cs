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
    namespace Commerce.RetailProxy.Authentication
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;

        /// <summary>
        /// Event argument for the event of sending a commerce authentication request.
        /// </summary>
        public class ComerceAuthenticationSendingRequestEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ComerceAuthenticationSendingRequestEventArgs"/> class.
            /// </summary>
            public ComerceAuthenticationSendingRequestEventArgs()
            {
                this.Headers = new Dictionary<string, string>();
            }

            /// <summary>
            /// Gets the headers being used by the request.
            /// </summary>
            public IDictionary<string, string> Headers { get; private set; }
        }
    }
}