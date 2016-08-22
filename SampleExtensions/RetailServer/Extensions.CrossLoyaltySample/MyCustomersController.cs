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
        using System;
        using System.Web.Http;
        using System.Web.OData;
        using Commerce.Runtime.CrossLoyaltySample.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.Dynamics.Retail.RetailServerLibrary;
        using Microsoft.Dynamics.Retail.RetailServerLibrary.ODataControllers;

        /// <summary>
        /// A customized CustomersController.
        /// </summary>
        public class MyCustomersController : CustomersController
        {
            /// <summary>
            /// The action to get the cross loyalty card discount.
            /// </summary>
            /// <param name="parameters">The OData action parameters.</param>
            /// <returns>The discount value.</returns>
            [HttpPost]
            [CommerceAuthorization(AllowedRetailRoles = new string[] { CommerceRoles.Customer, CommerceRoles.Employee })]
            public decimal GetCrossLoyaltyCardDiscountAction(ODataActionParameters parameters)
            {
                if (parameters == null)
                {
                    throw new ArgumentNullException("parameters");
                }

                var runtime = CommerceRuntimeManager.CreateRuntime(this.CommercePrincipal);
                string loyaltyCardNumber = (string)parameters["LoyaltyCardNumber"];

                GetCrossLoyaltyCardResponse resp = runtime.Execute<GetCrossLoyaltyCardResponse>(new GetCrossLoyaltyCardRequest(loyaltyCardNumber), null);

                string logMessage = "GetCrossLoyaltyCardAction successfully handled with card number '{0}'. Returned discount '{1}'.";
                RetailLogger.Log.ExtendedInformationalEvent(logMessage, loyaltyCardNumber, resp.Discount.ToString());
                return resp.Discount;
            }
        }
    }
}
