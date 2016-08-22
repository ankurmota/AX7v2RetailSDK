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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Pricing data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class PricingDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            /// <remarks>This covers common accessors shared by SQL and SQLite.</remarks>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(EntityDataServiceRequest<RetailDiscount>),
                        typeof(GetDiscountCodesDataRequest),
                        typeof(GetCustomerPriceGroupDataRequest),
                        typeof(EntityDataServiceRequest<IEnumerable<long>, CatalogPriceGroup>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, DiscountCode>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, Item>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, ProductVariant>),
                        typeof(EntityDataServiceRequest<IEnumerable<long>, RetailCategoryMember>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, MixAndMatchLineGroup>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, QuantityDiscountLevel>),
                        typeof(EntityDataServiceRequest<PriceGroup>),
                        typeof(EntityDataServiceRequest<PriceParameters>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, RetailDiscountPriceGroup>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, ThresholdDiscountTier>),
                        typeof(EntityDataServiceRequest<IEnumerable<string>, ValidationPeriod>),
                        typeof(EntityDataServiceRequest<ChannelPriceConfiguration>),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(EntityDataServiceRequest<RetailDiscount>))
                {
                    response = this.GetAllRetailDiscounts((EntityDataServiceRequest<RetailDiscount>)request);
                }
                else if (requestType == typeof(GetDiscountCodesDataRequest))
                {
                    response = this.GetDiscountCodes((GetDiscountCodesDataRequest)request);
                }
                else if (requestType == typeof(GetCustomerPriceGroupDataRequest))
                {
                    response = this.GetCustomerPriceGroup((GetCustomerPriceGroupDataRequest)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<long>, CatalogPriceGroup>))
                {
                    response = this.GetCatalogPriceGroups((EntityDataServiceRequest<IEnumerable<long>, CatalogPriceGroup>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, DiscountCode>))
                {
                    response = this.GetDiscountCodesByOfferId((EntityDataServiceRequest<IEnumerable<string>, DiscountCode>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, Item>))
                {
                    response = this.GetItems((EntityDataServiceRequest<IEnumerable<string>, Item>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, ProductVariant>))
                {
                    response = this.GetVariantDimensionsByItemIds((EntityDataServiceRequest<IEnumerable<string>, ProductVariant>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<long>, RetailCategoryMember>))
                {
                    response = this.GetRetailCategoryMembersForItems((EntityDataServiceRequest<IEnumerable<long>, RetailCategoryMember>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, MixAndMatchLineGroup>))
                {
                    response = this.GetMixAndMatchLineGroupsByOfferIds((EntityDataServiceRequest<IEnumerable<string>, MixAndMatchLineGroup>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, QuantityDiscountLevel>))
                {
                    response = this.GetMultipleBuyDiscountLinesByOfferIds((EntityDataServiceRequest<IEnumerable<string>, QuantityDiscountLevel>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<PriceGroup>))
                {
                    response = this.GetPriceGroups((EntityDataServiceRequest<PriceGroup>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<PriceParameters>))
                {
                    response = this.GetPriceParameters((EntityDataServiceRequest<PriceParameters>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, RetailDiscountPriceGroup>))
                {
                    response = this.GetRetailDiscountPriceGroups((EntityDataServiceRequest<IEnumerable<string>, RetailDiscountPriceGroup>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, ThresholdDiscountTier>))
                {
                    response = this.GetThresholdTiersByOfferIds((EntityDataServiceRequest<IEnumerable<string>, ThresholdDiscountTier>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<string>, ValidationPeriod>))
                {
                    response = this.GetValidationPeriodsByIds((EntityDataServiceRequest<IEnumerable<string>, ValidationPeriod>)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<ChannelPriceConfiguration>))
                {
                    response = this.GetChannelPriceConfiguration((EntityDataServiceRequest<ChannelPriceConfiguration>)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            private PricingDataManager GetDataManagerInstance(RequestContext context)
            {
                return new PricingDataManager(context);
            }
    
            private EntityDataServiceResponse<RetailDiscount> GetAllRetailDiscounts(EntityDataServiceRequest<RetailDiscount> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var retailDiscounts = pricingDataManager.GetAllRetailDiscounts();
    
                return new EntityDataServiceResponse<RetailDiscount>(retailDiscounts.AsPagedResult());
            }
    
            private EntityDataServiceResponse<DiscountCode> GetDiscountCodes(GetDiscountCodesDataRequest request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var discountCodes = pricingDataManager.GetDiscountCodes(request.OfferId, request.DiscountCode, request.Keyword, request.MinActiveDate.DateTime, request.QueryResultSettings);
    
                return new EntityDataServiceResponse<DiscountCode>(discountCodes);
            }
    
            private EntityDataServiceResponse<PriceGroup> GetCustomerPriceGroup(GetCustomerPriceGroupDataRequest request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                PriceGroup priceGroup = pricingDataManager.GetCustomerPriceGroup(request.CustomerPriceGroupId);
    
                return new EntityDataServiceResponse<PriceGroup>(new ReadOnlyCollection<PriceGroup>(new[] { priceGroup }).AsPagedResult());
            }
    
            private EntityDataServiceResponse<CatalogPriceGroup> GetCatalogPriceGroups(EntityDataServiceRequest<IEnumerable<long>, CatalogPriceGroup> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var catalogPriceGroups = pricingDataManager.GetCatalogPriceGroups(request.RequestParameter as ISet<long>);
    
                return new EntityDataServiceResponse<CatalogPriceGroup>(catalogPriceGroups.AsPagedResult());
            }
    
            private EntityDataServiceResponse<DiscountCode> GetDiscountCodesByOfferId(EntityDataServiceRequest<IEnumerable<string>, DiscountCode> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var discountCodes = pricingDataManager.GetDiscountCodesByOfferId(request.RequestParameter, new ColumnSet());
    
                return new EntityDataServiceResponse<DiscountCode>(discountCodes);
            }
    
            private EntityDataServiceResponse<Item> GetItems(EntityDataServiceRequest<IEnumerable<string>, Item> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var items = pricingDataManager.GetItems(request.RequestParameter);
    
                return new EntityDataServiceResponse<Item>(items.AsPagedResult());
            }
    
            private EntityDataServiceResponse<ProductVariant> GetVariantDimensionsByItemIds(EntityDataServiceRequest<IEnumerable<string>, ProductVariant> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var productVariants = pricingDataManager.GetVariantDimensionsByItemIds(request.RequestParameter);
    
                return new EntityDataServiceResponse<ProductVariant>(productVariants.AsPagedResult());
            }
    
            private EntityDataServiceResponse<RetailCategoryMember> GetRetailCategoryMembersForItems(EntityDataServiceRequest<IEnumerable<long>, RetailCategoryMember> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var retailCategoryMembers = pricingDataManager.GetRetailCategoryMembersForItems(request.RequestParameter as ISet<long>);
    
                return new EntityDataServiceResponse<RetailCategoryMember>(retailCategoryMembers.AsPagedResult());
            }
    
            private EntityDataServiceResponse<MixAndMatchLineGroup> GetMixAndMatchLineGroupsByOfferIds(EntityDataServiceRequest<IEnumerable<string>, MixAndMatchLineGroup> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var mixAndMatchLineGroups = pricingDataManager.GetMixAndMatchLineGroupsByOfferIds(request.RequestParameter);
    
                return new EntityDataServiceResponse<MixAndMatchLineGroup>(mixAndMatchLineGroups.AsPagedResult());
            }
    
            private EntityDataServiceResponse<QuantityDiscountLevel> GetMultipleBuyDiscountLinesByOfferIds(EntityDataServiceRequest<IEnumerable<string>, QuantityDiscountLevel> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var qtyDiscountLevels = pricingDataManager.GetMultipleBuyDiscountLinesByOfferIds(request.RequestParameter);
    
                return new EntityDataServiceResponse<QuantityDiscountLevel>(qtyDiscountLevels.AsPagedResult());
            }
    
            private EntityDataServiceResponse<PriceGroup> GetPriceGroups(EntityDataServiceRequest<PriceGroup> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var priceGroups = pricingDataManager.GetPriceGroups(QueryResultSettings.AllRecords);
    
                return new EntityDataServiceResponse<PriceGroup>(priceGroups);
            }
    
            private EntityDataServiceResponse<PriceParameters> GetPriceParameters(EntityDataServiceRequest<PriceParameters> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var priceParameters = new ReadOnlyCollection<PriceParameters>(new[] { pricingDataManager.GetPriceParameters(new ColumnSet()) });
    
                return new EntityDataServiceResponse<PriceParameters>(priceParameters.AsPagedResult());
            }
    
            private EntityDataServiceResponse<RetailDiscountPriceGroup> GetRetailDiscountPriceGroups(EntityDataServiceRequest<IEnumerable<string>, RetailDiscountPriceGroup> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var retailDiscountPriceGroups = pricingDataManager.GetRetailDiscountPriceGroups(request.RequestParameter as ISet<string>);
    
                return new EntityDataServiceResponse<RetailDiscountPriceGroup>(retailDiscountPriceGroups.AsPagedResult());
            }
    
            private EntityDataServiceResponse<ThresholdDiscountTier> GetThresholdTiersByOfferIds(EntityDataServiceRequest<IEnumerable<string>, ThresholdDiscountTier> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var thresholdDiscountTiers = pricingDataManager.GetThresholdTiersByOfferIds(request.RequestParameter);
    
                return new EntityDataServiceResponse<ThresholdDiscountTier>(thresholdDiscountTiers.AsPagedResult());
            }
    
            private EntityDataServiceResponse<ValidationPeriod> GetValidationPeriodsByIds(EntityDataServiceRequest<IEnumerable<string>, ValidationPeriod> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var validationPeriods = pricingDataManager.GetValidationPeriodsByIds(request.RequestParameter, new ColumnSet());
    
                return new EntityDataServiceResponse<ValidationPeriod>(validationPeriods.AsPagedResult());
            }

            private EntityDataServiceResponse<ChannelPriceConfiguration> GetChannelPriceConfiguration(EntityDataServiceRequest<ChannelPriceConfiguration> request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);

                var channelPriceConfigurations = new ReadOnlyCollection<ChannelPriceConfiguration>(new[] { pricingDataManager.ChannelPriceConfiguration });

                return new EntityDataServiceResponse<ChannelPriceConfiguration>(channelPriceConfigurations.AsPagedResult());
            }
        }
    }
}
