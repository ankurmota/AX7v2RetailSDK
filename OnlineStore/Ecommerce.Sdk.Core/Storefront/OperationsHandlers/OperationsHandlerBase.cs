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
    namespace Retail.Ecommerce.Sdk.Core.OperationsHandlers
    {
        /// <summary>
        /// Base class for operation handlers.
        /// </summary>
        public abstract class OperationsHandlerBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OperationsHandlerBase"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            protected OperationsHandlerBase(EcommerceContext ecommerceContext)
            {
                this.EcommerceContext = ecommerceContext;
            }

            /// <summary>
            /// Gets or sets the ecommerce context.
            /// </summary>
            /// <value>
            /// The ecommerce context.
            /// </value>
            public EcommerceContext EcommerceContext { get; protected set; }

            /// <summary>
            /// Sets eCommerce context.
            /// </summary>
            /// <param name="ecommerceContext">The eCommerce context.</param>
            public void SetEcommerceContext(EcommerceContext ecommerceContext)
            {
                this.EcommerceContext = ecommerceContext;
            }
        }
    }
}
