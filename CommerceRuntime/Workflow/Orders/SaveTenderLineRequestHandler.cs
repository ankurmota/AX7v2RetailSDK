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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Adds/Removes/Updates the tender line from the cart.
        /// </summary>
        /// <remarks>Upon adding/deleting/updating the tender line, the cart is saved to the database.</remarks>
        public sealed class SaveTenderLineRequestHandler : SingleRequestHandler<SaveTenderLineRequest, SaveTenderLineResponse>
        {
            /// <summary>
            /// This method processes the AddTenderLine workflow.
            /// </summary>
            /// <param name="request">The Add tender line request.</param>
            /// <returns>The Add tender line response.</returns>
            protected override SaveTenderLineResponse Process(SaveTenderLineRequest request)
            {
                ThrowIf.Null(request, "request");
    
                // Get the sales transaction
                SalesTransaction salesTransaction =
                    CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
                if (salesTransaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, request.CartId);
                }
    
                TenderLineBase tenderLineToProcess;
    
                if (request.PreprocessedTenderLine != null)
                {
                    tenderLineToProcess = request.PreprocessedTenderLine;
                }
                else if (request.TenderLine != null)
                {
                    tenderLineToProcess = request.TenderLine;
                }
                else
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Missing PreprocessedTenderLine or TenderLine");
                }
    
                // reason codes can be required during add/update or void
                this.AddOrUpdateReasonCodeLinesOnTransaction(request, salesTransaction);
    
                // Process the request.
                switch (request.OperationType)
                {
                    case TenderLineOperationType.Create:
                    case TenderLineOperationType.Update:
                    case TenderLineOperationType.Unknown:
                        CartWorkflowHelper.AddOrUpdateTenderLine(this.Context, salesTransaction, tenderLineToProcess);
                        break;
    
                    case TenderLineOperationType.Void:
                        CartWorkflowHelper.VoidTenderLine(this.Context, tenderLineToProcess, salesTransaction);
                        break;
    
                    default:
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                            string.Format("Operation {0} is not supported on tender lines.", request.OperationType));
                }
    
                // Save the updated sales transaction.
                CartWorkflowHelper.SaveSalesTransaction(this.Context, salesTransaction);
    
                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, salesTransaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
    
                return new SaveTenderLineResponse(cart);
            }
    
            /// <summary>
            /// Adds or updates the reason code lines on the sales transaction.
            /// </summary>
            /// <param name="request">The save tender line request.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            private void AddOrUpdateReasonCodeLinesOnTransaction(SaveTenderLineRequest request, SalesTransaction salesTransaction)
            {
                // Add or update any incoming reason codes on the transaction.
                if (request.ReasonCodeLines != null && request.ReasonCodeLines.Any())
                {
                    ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnTransaction(
                        salesTransaction,
                        new Cart { Id = request.CartId, ReasonCodeLines = request.ReasonCodeLines.ToList() });
                }
            }
        }
    }
}
