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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using PaymentSDK = Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    
        /// <summary>
        /// Encapsulates functionality related to card types.
        /// </summary>
        internal static class CardTypeHelper
        {
            internal static CardTypeInfo GetCardTypeConfiguration(string cardTypeId, RequestContext context)
            {
                if (string.IsNullOrWhiteSpace(cardTypeId))
                {
                    throw new ArgumentNullException("cardTypeId");
                }
    
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                // Due to the nature of sql join CARDTYPESVIEW has multiple rows for each card type (one per each number mask).
                // Since for specific card type configuration will be the same only first row is being used.
                GetCardTypeDataRequest getCardTypeRequest = new GetCardTypeDataRequest(cardTypeId, QueryResultSettings.FirstRecord);
                IEnumerable<CardTypeInfo> cardTypes = context.Execute<EntityDataServiceResponse<CardTypeInfo>>(getCardTypeRequest).PagedEntityCollection.Results;
    
                return cardTypes.SingleOrDefault();
            }
        }
    }
}
