/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Handles workflow for setting the delivery options on a cart.
        /// </summary>
        public sealed class UpdateDeliverySpecificationsRequestHandler : SingleRequestHandler<UpdateDeliverySpecificationsRequest, UpdateDeliverySpecificationsResponse>
        {
            /// <summary>
            /// Execute method to be overridden by each derived class.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The response.
            /// </returns>
            protected override UpdateDeliverySpecificationsResponse Process(UpdateDeliverySpecificationsRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.CartId, "request.CartId");
    
                SalesTransaction transaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
    
                if (transaction == null)
                {
                    string message = string.Format("Cart with identifer {0} was not found.", request.CartId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, message);
                }
    
                if (request.UpdateOrderLevelDeliveryOptions)
                {
                    transaction = this.UpdateOrderLevelDeliverySpecification(transaction, request.DeliverySpecification);
                }
                else
                {
                    transaction = this.UpdateLineLevelDeliveryOptions(transaction, request.LineDeliverySpecifications);
                }
    
                // Validate and resolve addresses.
                ShippingHelper.ValidateAndResolveAddresses(this.Context, transaction);
    
                // Updating the shipping information should only affect charges, taxes, amount due and totals.
                CartWorkflowHelper.Calculate(this.Context, transaction, CalculationModes.Charges | CalculationModes.Taxes | CalculationModes.AmountDue | CalculationModes.Totals);
    
                CartWorkflowHelper.SaveSalesTransaction(this.Context, transaction);
    
                Cart updatedCart = CartWorkflowHelper.ConvertToCart(this.Context, transaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(updatedCart);
    
                return new UpdateDeliverySpecificationsResponse(updatedCart);
            }

            /// <summary>
            /// Updates the electronic delivery information.
            /// </summary>
            /// <param name="salesLine">The sales line being updated.</param>
            /// <param name="electronicDeliveryEmailAddress">The electronic delivery email address.</param>
            /// <param name="electronicDeliveryEmailContent">Content of the electronic delivery email.</param>
            private static void UpdateElectronicDeliveryInfo(SalesLine salesLine, string electronicDeliveryEmailAddress, string electronicDeliveryEmailContent)
            {
                salesLine.ElectronicDeliveryEmailAddress = electronicDeliveryEmailAddress;
                salesLine.ElectronicDeliveryEmailContent = electronicDeliveryEmailContent;
            }
    
            /// <summary>
            /// Clears the line level delivery values.
            /// </summary>
            /// <param name="salesLines">The sales lines being updated.</param>
            private static void ClearLineLevelDeliveryValues(IEnumerable<SalesLine> salesLines)
            {
                foreach (SalesLine salesLine in salesLines)
                {
                    ClearLineLevelDeliveryValues(salesLine);
                }
            }
    
            /// <summary>
            /// Clears the line level delivery values.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            private static void ClearLineLevelDeliveryValues(SalesLine salesLine)
            {
                if (salesLine != null)
                {
                    salesLine.DeliveryMode = null;
                    salesLine.ShippingAddress = null;
                    salesLine.FulfillmentStoreId = null;
                }
            }

            /// <summary>
            /// Sets the order level delivery options.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction being updated.</param>
            /// <param name="deliverySpecification">The selected delivery option.</param>
            /// <returns>Updated sales transaction.</returns>
            private SalesTransaction UpdateOrderLevelDeliverySpecification(SalesTransaction salesTransaction, DeliverySpecification deliverySpecification)
            {
                ClearLineLevelDeliveryValues(salesTransaction.SalesLines);

                this.ValidateDeliveryMode(deliverySpecification.DeliveryModeId);

                salesTransaction.DeliveryMode = deliverySpecification.DeliveryModeId;
    
                switch (deliverySpecification.DeliveryPreferenceType)
                {
                    case DeliveryPreferenceType.ShipToAddress:
                        salesTransaction.ShippingAddress = deliverySpecification.DeliveryAddress;
                        break;
    
                    case DeliveryPreferenceType.PickupFromStore:
                        salesTransaction.ShippingAddress = deliverySpecification.DeliveryAddress;
                        foreach (SalesLine salesLine in salesTransaction.SalesLines)
                        {
                            salesLine.FulfillmentStoreId = deliverySpecification.PickUpStoreId;
                        }
    
                        break;
                    case DeliveryPreferenceType.ElectronicDelivery:
                        foreach (SalesLine salesLine in salesTransaction.SalesLines)
                        {
                            UpdateElectronicDeliveryInfo(salesLine, deliverySpecification.ElectronicDeliveryEmailAddress, deliverySpecification.ElectronicDeliveryEmailContent);
                        }
    
                        break;
                    default:
                        var message = string.Format("Unsupported delivery preference type [{0}] is specified.", deliverySpecification.DeliveryPreferenceType);
                        throw new InvalidOperationException(message);
                }
    
                return salesTransaction;
            }
    
            /// <summary>
            /// Sets the line level delivery options.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction being updated.</param>
            /// <param name="lineDeliverySpecifications">The delivery specifications at the line level.</param>
            /// <returns>Updated sales transaction.</returns>
            private SalesTransaction UpdateLineLevelDeliveryOptions(SalesTransaction salesTransaction, IEnumerable<LineDeliverySpecification> lineDeliverySpecifications)
            {
                ThrowIf.Null(lineDeliverySpecifications, "lineDeliverySpecifications");
    
                // Clear header level delivery information.
                salesTransaction.DeliveryMode = null;
                salesTransaction.ShippingAddress = null;
    
                Dictionary<string, LineDeliverySpecification> lineDeliverySpecificationsByLineId = lineDeliverySpecifications.ToDictionary(k => k.LineId);
    
                foreach (var salesLine in salesTransaction.SalesLines)
                {
                    if (salesLine != null)
                    {
                        LineDeliverySpecification currentLineDeliverySpecification;
    
                        if (lineDeliverySpecificationsByLineId.TryGetValue(salesLine.LineId, out currentLineDeliverySpecification))
                        {
                            ClearLineLevelDeliveryValues(salesLine);
    
                            DeliverySpecification currentDeliverySpecification = currentLineDeliverySpecification.DeliverySpecification;

                            this.ValidateDeliveryMode(currentDeliverySpecification.DeliveryModeId);

                            salesLine.DeliveryMode = currentDeliverySpecification.DeliveryModeId;
    
                            switch (currentDeliverySpecification.DeliveryPreferenceType)
                            {
                                case DeliveryPreferenceType.ShipToAddress:
                                    salesLine.ShippingAddress = currentDeliverySpecification.DeliveryAddress;
                                    break;
    
                                case DeliveryPreferenceType.PickupFromStore:
                                    salesLine.ShippingAddress = currentDeliverySpecification.DeliveryAddress;
                                    salesLine.FulfillmentStoreId = currentDeliverySpecification.PickUpStoreId;
                                    break;
    
                                case DeliveryPreferenceType.ElectronicDelivery:
                                    UpdateElectronicDeliveryInfo(
                                        salesLine,
                                        currentDeliverySpecification.ElectronicDeliveryEmailAddress,
                                        currentDeliverySpecification.ElectronicDeliveryEmailContent);
                                    break;
    
                                default:
                                    var message = string.Format("Unsupported delivery preference type [{0}] is specified.", currentDeliverySpecification.DeliveryPreferenceType);
                                    throw new InvalidOperationException(message);
                            }
                        }
                    }
                }
    
                return salesTransaction;
            }

            /// <summary>
            /// Verifies whether the delivery mode is valid or not throwing <see cref="DataValidationException"></see> if not valid.
            /// </summary>
            /// <param name="deliveryModeId">The identifier of the delivery mode to be verified.</param>
            private void ValidateDeliveryMode(string deliveryModeId)
            {
                var deliveryOptionDataRequest = new GetDeliveryOptionDataRequest(deliveryModeId, new QueryResultSettings(new ColumnSet("CODE"), PagingInfo.AllRecords));
                var deliveryOptionDataResponse = this.Context.Execute<EntityDataServiceResponse<DeliveryOption>>(deliveryOptionDataRequest);
                DeliveryOption deliveryOption = deliveryOptionDataResponse.PagedEntityCollection.FirstOrDefault();

                if (deliveryOption == null)
                {
                    RetailLogger.Log.CrtWorkflowUpdateDeliverySpecificationRequestHandlerDeliveryModeNotFound(deliveryModeId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidDeliveryMode, "The provided delivery mode if was not found.");
                }
            }
        }
    }
}
