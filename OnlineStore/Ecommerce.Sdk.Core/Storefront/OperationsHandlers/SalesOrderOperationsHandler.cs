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
        using System;
        using System.Threading.Tasks;
        using Commerce.RetailProxy;

        /// <summary>
        /// Handler for sales order operations.
        /// </summary>
        public class SalesOrderOperationsHandler : OperationsHandlerBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SalesOrderOperationsHandler"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            public SalesOrderOperationsHandler(EcommerceContext ecommerceContext) : base(ecommerceContext)
            {
            }

            /// <summary>
            /// Get collection of SalesOrders as PagedResult.
            /// </summary>
            /// <param name="salesOrderSearchCriteria">The salesOrderSearchCriteria.</param>
            /// <param name="queryResultSettings">The queryResultSettings.</param>
            /// <returns>Sales Orders by search criteria.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public async Task<PagedResult<SalesOrder>> GetSalesOrder(SalesOrderSearchCriteria salesOrderSearchCriteria, QueryResultSettings queryResultSettings)
            {
                if (salesOrderSearchCriteria == null)
                {
                    throw new ArgumentNullException(nameof(salesOrderSearchCriteria));
                }

                if (queryResultSettings == null)
                {
                    throw new ArgumentNullException(nameof(queryResultSettings));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ISalesOrderManager salesOrderManager = managerFactory.GetManager<ISalesOrderManager>();

                PagedResult<SalesOrder> salesOrders = await salesOrderManager.Search(salesOrderSearchCriteria, queryResultSettings);

                salesOrders = await DataAugmenter.GetAugmentedSalesOrders(this.EcommerceContext, salesOrders);
                return salesOrders;
            }
        }
    }
}