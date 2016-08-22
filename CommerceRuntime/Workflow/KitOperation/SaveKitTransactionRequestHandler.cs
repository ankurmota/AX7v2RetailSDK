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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Save all incoming kit (disassembly) operation transactions.
        /// </summary>
        public class SaveKitTransactionRequestHandler : SingleRequestHandler<SaveKitTransactionRequest, SaveKitTransactionResponse>
        {
            /// <summary>
            /// Executes the workflow to save  transactions from kit (disassembly) operation.
            /// </summary>
            /// <param name="request">Instance of <see cref="SaveKitTransactionRequest"/>.</param>
            /// <returns>Instance of <see cref="SaveKitTransactionResponse"/>.</returns>
            protected override SaveKitTransactionResponse Process(SaveKitTransactionRequest request)
            {
                ThrowIf.Null(request, "request");
    
                SaveKitTransactionResponse response;
    
                KitTransaction transaction = this.GetKitTransaction(request);
                this.ValidateTransactiondata(transaction);
    
                SaveKitTransactionDataRequest dataRequest = new SaveKitTransactionDataRequest(transaction);
                request.RequestContext.Execute<NullResponse>(dataRequest);
    
                response = new SaveKitTransactionResponse(transaction);
    
                return response;
            }
    
            /// <summary>
            /// Gets transaction object of the kit operation from the request and sets context related information.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>Returns the kit transaction object.</returns>
            private KitTransaction GetKitTransaction(SaveKitTransactionRequest request)
            {
                var kitTransaction = new KitTransaction();
    
                kitTransaction.Id = request.KitTransaction.Id;
                kitTransaction.ShiftId = request.KitTransaction.ShiftId;
                kitTransaction.StoreId = this.Context.GetOrgUnit().OrgUnitNumber;
                kitTransaction.StaffId = this.Context.GetPrincipal().UserId;
                kitTransaction.TerminalId = this.Context.GetTerminal().TerminalId;
                kitTransaction.TransactionType = request.KitTransaction.TransactionType;
    
                foreach (var kitTransLine in request.KitTransaction.KitTransactionLines)
                {
                    var transLine = new KitTransactionLine();
                    transLine.Quantity = kitTransLine.Quantity;
                    transLine.ItemId = kitTransLine.ItemId;
                    transLine.InventoryDimensionId = kitTransLine.InventoryDimensionId;
    
                    kitTransaction.KitTransactionLines.Add(transLine);
                }
    
                return kitTransaction;
            }
    
            private void ValidateTransactiondata(KitTransaction kitTransaction)
            {
                ThrowIf.Null(kitTransaction.KitTransactionLines, "kitTransaction.KitTransactionLines");
    
                if (!kitTransaction.KitTransactionLines.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Transaction does not contain any kit transaction line.");
                }
    
                // Validate transaction contains a valid kit variant
                foreach (KitTransactionLine kitline in kitTransaction.KitTransactionLines)
                {
                    if (string.IsNullOrWhiteSpace(kitline.InventoryDimensionId))
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "InventoryDimensionId is not set for the Kit line transaction.");
                    }

                    var productLookupClauses = new List<ProductLookupClause> { new ProductLookupClause(kitline.ItemId, kitline.InventoryDimensionId) };
                    var request = new GetProductsServiceRequest(this.Context.GetChannelConfiguration().RecordId, productLookupClauses, QueryResultSettings.AllRecords);
                    var results = this.Context.Runtime.Execute<GetProductsServiceResponse>(request, this.Context).Products.Results;

                    if (results == null)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindListing, string.Format("Kit with itemId {0} and inventoryDimensionId {1} is not found.", kitline.ItemId, kitline.InventoryDimensionId));
                    }

                    if (results.HasMultiple())
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MultipleItemsForItemId, string.Format("ItemId {0} and inventoryDimensionId {1}  returned multiple kit records. The ids do not represent a single kit product.", kitline.ItemId, kitline.InventoryDimensionId));
                    }

                    SimpleProduct product = results.Single();
                    if (product.ProductType != ProductType.KitVariant)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, string.Format("ItemId {0} is not a kit product.", kitline.ItemId));
                    }

                    if (!product.Behavior.IsKitDisassemblyAllowed)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, string.Format("Kit product '{0}' is not allowed to be disassembled at a register.", kitline.ItemId));
                    }
                }
            }
        }
    }
}
