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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Pricing SQL data service class.
        /// </summary>
        public class PricingSqliteDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(ReadPriceAdjustmentsDataRequest),
                        typeof(ReadRetailDiscountsDataRequest),
                        typeof(ReadPriceTradeAgreementsDataRequest),
                        typeof(ReadDiscountTradeAgreementsDataRequest),
                        typeof(EntityDataServiceRequest<IEnumerable<AffiliationLoyaltyTier>, PriceGroup>),
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
    
                if (requestType == typeof(ReadPriceAdjustmentsDataRequest))
                {
                    response = this.ReadPriceAdjustments((ReadPriceAdjustmentsDataRequest)request);
                }
                else if (requestType == typeof(ReadRetailDiscountsDataRequest))
                {
                    response = this.ReadRetailDiscounts((ReadRetailDiscountsDataRequest)request);
                }
                else if (requestType == typeof(ReadPriceTradeAgreementsDataRequest))
                {
                    response = this.ReadPriceTradeAgreements((ReadPriceTradeAgreementsDataRequest)request);
                }
                else if (requestType == typeof(ReadDiscountTradeAgreementsDataRequest))
                {
                    response = this.ReadDiscountTradeAgreements((ReadDiscountTradeAgreementsDataRequest)request);
                }
                else if (requestType == typeof(EntityDataServiceRequest<IEnumerable<AffiliationLoyaltyTier>, PriceGroup>))
                {
                    response = this.GetAffiliationPriceGroups((EntityDataServiceRequest<IEnumerable<AffiliationLoyaltyTier>, PriceGroup>)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            private PricingSqliteDatabaseAccessor GetDataManagerInstance(RequestContext context)
            {
                return new PricingSqliteDatabaseAccessor(context);
            }
    
            private EntityDataServiceResponse<PriceAdjustment, ValidationPeriod> ReadPriceAdjustments(ReadPriceAdjustmentsDataRequest request)
            {
                var pricingDataManager = this.GetDataManagerInstance(request.RequestContext);
                ReadOnlyCollection<ValidationPeriod> validationPeriods;
    
                var priceAdjustments = pricingDataManager.ReadPriceAdjustments(
                    request.ItemUnits,
                    request.PriceGroups as ISet<string>,
                    request.MinActiveDate,
                    request.MaxActiveDate,
                    out validationPeriods);
    
                return new EntityDataServiceResponse<PriceAdjustment, ValidationPeriod>(priceAdjustments, validationPeriods.ToArray());
            }
    
            private EntityDataServiceResponse<PeriodicDiscount, ValidationPeriod> ReadRetailDiscounts(ReadRetailDiscountsDataRequest request)
            {
                var pricingSqlDatamaanger = this.GetDataManagerInstance(request.RequestContext);
                ReadOnlyCollection<ValidationPeriod> validationPeriods;
    
                var retailDiscounts = pricingSqlDatamaanger.ReadRetailDiscounts(
                    request.ItemUnits,
                    request.PriceGroups,
                    request.MinActiveDate,
                    request.MaxActiveDate,
                    request.CurrencyCode,
                    out validationPeriods);
    
                return new EntityDataServiceResponse<PeriodicDiscount, ValidationPeriod>(retailDiscounts, validationPeriods.ToArray());
            }
    
            private EntityDataServiceResponse<TradeAgreement> ReadPriceTradeAgreements(ReadPriceTradeAgreementsDataRequest request)
            {
                var pricingSqlDatamaanger = this.GetDataManagerInstance(request.RequestContext);
    
                var retailPriceTradeAgreements = pricingSqlDatamaanger.ReadPriceTradeAgreements(
                    request.ItemId,
                    request.PriceGroups as ISet<string>,
                    request.CustomerAccount,
                    request.MinActiveDate,
                    request.MaxActiveDate,
                    request.CurrencyCode);
    
                return new EntityDataServiceResponse<TradeAgreement>(retailPriceTradeAgreements);
            }
    
            private EntityDataServiceResponse<TradeAgreement> ReadDiscountTradeAgreements(ReadDiscountTradeAgreementsDataRequest request)
            {
                var pricingSqlDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var retailDiscountTradeAgreements = pricingSqlDataManager.ReadDiscountTradeAgreements(
                    request.ItemId,
                    request.CustomerAccount,
                    request.MinActiveDate,
                    request.MaxActiveDate,
                    request.CurrencyCode);
    
                return new EntityDataServiceResponse<TradeAgreement>(retailDiscountTradeAgreements.AsPagedResult());
            }
    
            private EntityDataServiceResponse<PriceGroup> GetAffiliationPriceGroups(EntityDataServiceRequest<IEnumerable<AffiliationLoyaltyTier>, PriceGroup> request)
            {
                var pricingSqlDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                var affiliationPriceGroups = pricingSqlDataManager.GetAffiliationPriceGroups(request.RequestParameter);
    
                return new EntityDataServiceResponse<PriceGroup>(affiliationPriceGroups.AsPagedResult());
            }
        }
    }
}
