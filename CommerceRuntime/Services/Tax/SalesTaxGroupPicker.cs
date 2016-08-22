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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Encapsulates the business logic to pick up the correct sales tax group based on channel settings.
        /// </summary>
        public class SalesTaxGroupPicker
        {
            private readonly ICollection<ITaxGroupPolicy> policies;
    
            private SalesTaxGroupPicker(ICollection<ITaxGroupPolicy> policies)
            {
                this.policies = policies;
            }
    
            /// <summary>
            /// Defines the common tax group policy interface.
            /// </summary>
            private interface ITaxGroupPolicy
            {
                string GetTaxGroup();
            }
    
            /// <summary>
            /// Gets the SalesTaxGroup based on business rules.
            /// </summary>
            /// <value> The SalesTaxGroup setting. </value>
            public string SalesTaxGroup
            {
                get
                {
                    string taxGroup = null;
    
                    // Finds the first valid setting by iterating the policies in predefined order.
                    foreach (ITaxGroupPolicy policy in this.policies)
                    {
                        taxGroup = policy.GetTaxGroup();
    
                        if (!string.IsNullOrEmpty(taxGroup))
                        {
                            return taxGroup;
                        }
                    }
    
                    return taxGroup;
                }
            }
    
            /// <summary>
            /// Factory method to construct the SalesTaxGroupPicker.
            /// </summary>
            /// <param name="channel">The current channel.</param>
            /// <param name="context">The request context.</param>
            /// <param name="address">The destination address object.</param>
            /// <param name="deliveryMode">The delivery mode used.</param>
            /// <param name="fulfillmentStoreId">The store that fulfills the purchase (pick up from).</param>
            /// <param name="shippingFromInventLocation">The invent location that item shipped from.</param>
            /// <param name="customerId">Customer account number.</param>
            /// <returns>
            /// The SalesTaxGroupPicker object.
            /// </returns>
            public static SalesTaxGroupPicker Create(Channel channel, RequestContext context, Address address, string deliveryMode, string fulfillmentStoreId, string shippingFromInventLocation, string customerId)
            {
                ThrowIf.Null(channel, "channel");
                ThrowIf.Null(context, "context");
    
                if (channel.OrgUnitType != RetailChannelType.RetailStore)
                {
                    return CreatePickerForOnlineChannel(context, address, deliveryMode, fulfillmentStoreId, shippingFromInventLocation);
                }
    
                Customer customer = null;
                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    var getCustomerDataRequest = new GetCustomerDataRequest(customerId);
                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest, context);
                    customer = getCustomerDataResponse.Entity;
                }
    
                OrgUnit store = channel as OrgUnit;
                return CreatePickerForStore(store, customer, context, address, deliveryMode, fulfillmentStoreId, shippingFromInventLocation);
            }
    
            /// <summary>
            /// Constructs SalesTaxGroupPicker for online channel.
            /// </summary>
            /// <param name="context">The current CRT request context object.</param>
            /// <param name="deliveryAddress">The delivery address.</param>
            /// <param name="deliveryMode">The delivery mode used.</param>
            /// <param name="fulfillmentStoreId">The store that fulfills the purchase (pick up from).</param>
            /// <param name="shippingFromInventLocation">The invent location that item shipped from.</param>
            /// <returns>
            /// The SalesTaxGroupPicker object.
            /// </returns>
            private static SalesTaxGroupPicker CreatePickerForOnlineChannel(RequestContext context, Address deliveryAddress, string deliveryMode, string fulfillmentStoreId, string shippingFromInventLocation)
            {
                var policies = new Collection<ITaxGroupPolicy>();
                bool pickUpFromStore = false;
                OrgUnit store = null;
    
                if (!string.IsNullOrWhiteSpace(deliveryMode))
                {
                    pickUpFromStore = deliveryMode.Equals(context.GetChannelConfiguration().PickupDeliveryModeCode, StringComparison.OrdinalIgnoreCase);
    
                    if (!string.IsNullOrWhiteSpace(fulfillmentStoreId))
                    {
                        var getStoresByStoreNumbersDataRequest = new SearchOrgUnitDataRequest(new string[] { fulfillmentStoreId }, QueryResultSettings.SingleRecord);
                        store = context.Runtime.Execute<EntityDataServiceResponse<OrgUnit>>(getStoresByStoreNumbersDataRequest, context).PagedEntityCollection.Results.SingleOrDefault();
                    }
                }
    
                if (pickUpFromStore && store != null)
                {
                    policies.Add(new StoreTaxGroupPolicy(store));
                }
                else
                {
                    if (deliveryAddress != null)
                    {
                        switch (context.GetChannelConfiguration().CountryRegionISOCode)
                        {
                            case CountryRegionISOCode.IN:
                                policies.Add(new AddressTaxGroupPolicyIndia(context, deliveryAddress, shippingFromInventLocation));
                                break;
                            default:
                                policies.Add(new AddressTaxGroupPolicy(context, deliveryAddress));
                                break;
                        }
                    }
                }
    
                return new SalesTaxGroupPicker(policies);
            }
    
            /// <summary>
            /// Constructs the SalesTaxGroupPicker for store.
            /// </summary>
            /// <param name="store">The organization unit.</param>
            /// <param name="customer">The customer object.</param>
            /// <param name="context">The request context.</param>
            /// <param name="deliveryAddress">The delivery address.</param>
            /// <param name="deliveryMode">The delivery mode used.</param>
            /// <param name="fulfillmentStoreId">The store that fulfills the purchase (pick up from).</param>
            /// <param name="shippingFromInventLocation">The invent location that item shipped from.</param>
            /// <returns>
            /// The SalesTaxGroupPicker object.
            /// </returns>
            private static SalesTaxGroupPicker CreatePickerForStore(
                OrgUnit store,
                Customer customer,
                RequestContext context,
                Address deliveryAddress,
                string deliveryMode,
                string fulfillmentStoreId,
                string shippingFromInventLocation)
            {
                var policies = new Collection<ITaxGroupPolicy>();
    
                bool pickUpFromStore = false;
    
                if (!string.IsNullOrWhiteSpace(deliveryMode))
                {
                    pickUpFromStore = deliveryMode.Equals(context.GetChannelConfiguration().PickupDeliveryModeCode, StringComparison.OrdinalIgnoreCase);
    
                    if (!string.IsNullOrWhiteSpace(fulfillmentStoreId))
                    {
                        // same store pickup or not?
                        if (!fulfillmentStoreId.Equals(store.OrgUnitNumber, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var getStoresByStoreNumbersDataRequest = new SearchOrgUnitDataRequest(new string[] { fulfillmentStoreId }, QueryResultSettings.SingleRecord);
                            store = context.Runtime.Execute<EntityDataServiceResponse<OrgUnit>>(getStoresByStoreNumbersDataRequest, context).PagedEntityCollection.Results.SingleOrDefault();
                        }
                    }
                }
    
                // Constructs the policy checking chain in the priority of : Shipping address -> Customer -> Store.
                // There should be no tax fallback calculations between types of store taxes.
                // For example, if the store is using Customer based tax and no tax group found,
                // should return 0 tax instead of trying to calculate using Store based tax.
                if (store.UseDestinationBasedTax && !pickUpFromStore && deliveryAddress != null)
                {
                    switch (context.GetChannelConfiguration().CountryRegionISOCode)
                    {
                        case CountryRegionISOCode.IN:
                            policies.Add(new AddressTaxGroupPolicyIndia(context, deliveryAddress, shippingFromInventLocation));
                            break;
                        default:
                            policies.Add(new AddressTaxGroupPolicy(context, deliveryAddress));
                            break;
                    }
                }
                else if (store.UseCustomerBasedTax && customer != null)
                {
                    policies.Add(new CustomerTaxGroupPolicy(customer));
                }
                else
                {
                    policies.Add(new StoreTaxGroupPolicy(store));
                }
    
                return new SalesTaxGroupPicker(policies);
            }
    
            /// <summary>
            /// Implements store based tax group policy.
            /// </summary>
            private class StoreTaxGroupPolicy : ITaxGroupPolicy
            {
                private readonly OrgUnit store;
    
                internal StoreTaxGroupPolicy(OrgUnit store)
                {
                    this.store = store;
                }
    
                /// <summary>
                /// Gets the tax group setting.
                /// </summary>
                /// <returns>The tax group setting.</returns>
                public string GetTaxGroup()
                {
                    return this.store.TaxGroup;
                }
            }
    
            /// <summary>
            /// Implements customer based tax group policy.
            /// </summary>
            private class CustomerTaxGroupPolicy : ITaxGroupPolicy
            {
                private readonly Customer customer;
    
                internal CustomerTaxGroupPolicy(Customer customer)
                {
                    this.customer = customer;
                }
    
                /// <summary>
                /// Gets the tax group setting.
                /// </summary>
                /// <returns>The tax group setting.</returns>
                public string GetTaxGroup()
                {
                    return this.customer.TaxGroup;
                }
            }
    
            /// <summary>
            /// Implements destination based tax group policy.
            /// </summary>
            private class AddressTaxGroupPolicy : ITaxGroupPolicy
            {
                private readonly RequestContext context;
                private readonly Address address;
    
                internal AddressTaxGroupPolicy(RequestContext context, Address address)
                {
                    this.context = context;
                    this.address = address;
                }
    
                /// <summary>
                /// Gets the tax group setting.
                /// </summary>
                /// <returns>The tax group setting.</returns>
                public string GetTaxGroup()
                {
                    return TaxService.GetTaxRegime(this.context, this.address);
                }
            }
    
            /// <summary>
            /// Implements destination based tax group policy for India.
            /// </summary>
            private class AddressTaxGroupPolicyIndia : ITaxGroupPolicy
            {
                private readonly RequestContext context;
                private readonly Address address;
                private readonly string shippingFromInventLocation;
    
                internal AddressTaxGroupPolicyIndia(RequestContext context, Address address, string shippingFromInventLocation)
                {
                    this.context = context;
                    this.address = address;
                    this.shippingFromInventLocation = shippingFromInventLocation;
                }
    
                /// <summary>
                /// Gets the tax group setting.
                /// </summary>
                /// <returns>The tax group setting.</returns>
                public string GetTaxGroup()
                {
                    string inventLocationId = string.IsNullOrWhiteSpace(this.shippingFromInventLocation) ? this.context.GetChannelConfiguration().InventLocation : this.shippingFromInventLocation;
                    bool isInterStateTrans = false;
    
                    string taxRegime = TaxService.GetInterStateTaxRegimeIndia(this.context, inventLocationId, this.address, out isInterStateTrans);
    
                    if (isInterStateTrans)
                    {
                        return taxRegime;
                    }
                    else
                    {
                        return TaxService.GetTaxRegime(this.context, this.address);
                    }
                }
            }
        }
    }
}
