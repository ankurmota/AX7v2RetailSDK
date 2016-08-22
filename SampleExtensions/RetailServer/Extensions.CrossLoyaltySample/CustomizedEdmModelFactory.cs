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
    namespace RetailServer.CrossLoyaltySample
    {
        using System.Composition;
        using System.Runtime.InteropServices;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.RetailServerLibrary;

        /// <summary>
        /// The specialized commerce model factory.
        /// </summary>
        [Export(typeof(IEdmModelFactory))]
        [ComVisible(false)]
        public class CustomizedEdmModelFactory : CommerceModelFactory
        {
            /// <summary>
            /// Builds entity sets.
            /// </summary>
            protected override void BuildActions()
            {
                base.BuildActions();
                var var1 = CommerceModelFactory.BindEntitySetAction<Customer>("GetCrossLoyaltyCardDiscountAction");
                var1.Parameter<string>("LoyaltyCardNumber");
                var1.Returns<decimal>();
            }
        }
    }
}
