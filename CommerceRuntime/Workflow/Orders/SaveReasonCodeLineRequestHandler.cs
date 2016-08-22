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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Adds or updates a reason code line.
        /// </summary>
        public sealed class SaveReasonCodeLineRequestHandler : SingleRequestHandler<SaveReasonCodeLineRequest, SaveReasonCodeLineResponse>
        {
            /// <summary>
            /// Processes the save reason code line workflow.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override SaveReasonCodeLineResponse Process(SaveReasonCodeLineRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.CartId, "request.CartId");
                ThrowIf.Null(request.ReasonCodeLine, "request.ReasonCodeLine");
    
                // Get the sales transaction.
                SalesTransaction salesTransaction =
                    CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
    
                if (salesTransaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, request.CartId);
                }
    
                // Find the reason code lines collection that the reason code line belongs.
                ICollection<ReasonCodeLine> reasonCodeLines = null;
    
                switch (request.ReasonCodeLineType)
                {
                    case ReasonCodeLineType.Header:
                        reasonCodeLines = salesTransaction.ReasonCodeLines;
                        break;
                    case ReasonCodeLineType.Sales:
                        // Consider all lines. We could add reason code to voided lines.
                        if (salesTransaction.SalesLines != null)
                        {
                            var salesLine = (from s in salesTransaction.SalesLines
                                             where s.LineId == request.ParentLineId
                                             select s).FirstOrDefault();
    
                            if (salesLine != null)
                            {
                                reasonCodeLines = salesLine.ReasonCodeLines;
                            }
                        }
    
                        break;
                    case ReasonCodeLineType.Payment:
                        if (salesTransaction.TenderLines != null)
                        {
                            var tenderLine = (from t in salesTransaction.TenderLines
                                              where t.TenderLineId == request.ParentLineId
                                              select t).FirstOrDefault();
    
                            if (tenderLine != null)
                            {
                                reasonCodeLines = tenderLine.ReasonCodeLines;
                            }
                        }
    
                        break;
                    default:
                        // Do nothing.
                        break;
                }
    
                if (reasonCodeLines == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, request.CartId, "Cannot find the requested parent line for the reason code line.");
                }
    
                // Add or update the reason code line.
                if (string.IsNullOrWhiteSpace(request.ReasonCodeLine.LineId))
                {
                    request.ReasonCodeLine.LineId = Guid.NewGuid().ToString("N");
                    request.ReasonCodeLine.ParentLineId = request.ParentLineId;
                    request.ReasonCodeLine.TransactionId = salesTransaction.Id;
                    request.ReasonCodeLine.LineType = request.ReasonCodeLineType;
    
                    reasonCodeLines.Add(request.ReasonCodeLine);
                }
                else 
                {
                    // Update an existing reason code line
                    var reasonCodeLineToUpdate = (from r in reasonCodeLines
                                                  where r.LineId == request.ReasonCodeLine.LineId
                                                  select r).FirstOrDefault();
                    
                    if (reasonCodeLineToUpdate != null)
                    {
                        reasonCodeLineToUpdate.CopyPropertiesFrom(request.ReasonCodeLine);
                        reasonCodeLineToUpdate.IsChanged = true;
                    }
                }
    
                // Save the updated sales transaction.
                CartWorkflowHelper.SaveSalesTransaction(this.Context, salesTransaction);
    
                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, salesTransaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
    
                return new SaveReasonCodeLineResponse(cart);
            }
        }
    }
}
