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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// The request handler for GetXAndZReportReceiptRequest class.
        /// </summary>
        public sealed class GetXAndZReportReceiptRequestHandler : SingleRequestHandler<GetXAndZReportReceiptRequest, GetReceiptResponse>
        {
            /// <summary>
            /// Processes the GetXZReportReceiptRequest to return the X or Z report receipts. The request should not be null.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The GetReceiptResponse.</returns>
            protected override GetReceiptResponse Process(GetXAndZReportReceiptRequest request)
            {
                ThrowIf.Null(request, "request");
                    
                var getReceiptServiceRequest = this.CreateXZReportReceiptServiceRequest(request);
                var getReceiptServiceResponse = this.Context.Execute<GetReceiptServiceResponse>(getReceiptServiceRequest);
    
                // Save the transaction log for printing X or X report
                this.LogTransaction(request);
    
                return new GetReceiptResponse(getReceiptServiceResponse.Receipts);
            }
    
            private GetReceiptServiceRequest CreateXZReportReceiptServiceRequest(GetXAndZReportReceiptRequest request)
            {
                ThrowIf.Null(request, "request");
    
                Shift shift;
    
                var shiftId = request.ShiftId;
                var receiptType = request.ReceiptType;
    
                // Validates if the request is XReport or ZReport type
                if ((receiptType != ReceiptType.XReport) && (receiptType != ReceiptType.ZReport))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReceiptTypeNotSupported, "Only receipt types for X or Z reports are expected.");
                }
    
                if (receiptType == ReceiptType.XReport)
                {
                    var terminalId = request.ShiftTerminalId;
                    GetShiftDataRequest getShiftDataRequest = new GetShiftDataRequest(terminalId, shiftId);
                    shift = this.Context.Execute<SingleEntityDataServiceResponse<Shift>>(getShiftDataRequest).Entity;
    
                    // Validates if an open or blind-closed shift of the ShiftId can be found when requesting XReport
                    if (shift == null || 
                        (shift.Status != ShiftStatus.Open && shift.Status != ShiftStatus.BlindClosed))
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ShiftNotFound,
                            string.Format("No open shift information can be found using the shift Id {0} on terminal {1} for X report.", shiftId, terminalId));
                    }
    
                    // Calculates the shift information inorder to generate the X report
                    ShiftCalculator.Calculate(this.Context, shift, shift.TerminalId, shift.ShiftId);
                }
                else
                {
                    var terminalId = this.Context.GetTerminal().TerminalId;
                    GetLastClosedShiftDataRequest getLastClosedShiftDataRequest = new GetLastClosedShiftDataRequest(terminalId);
                    shift = this.Context.Execute<SingleEntityDataServiceResponse<Shift>>(getLastClosedShiftDataRequest).Entity;
    
                    // Validates if a closed shift of the ShiftId can be found when requesting XReport
                    if (shift == null || shift.Status != ShiftStatus.Closed)
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ShiftNotFound,
                            string.Format("No closed shift information can be found using the shift Id {0} on terminal {1} for Z report.", shiftId, terminalId));
                    }
                }
    
                var getReceiptServiceRequest = new GetReceiptServiceRequest(
                    shift,
                    new List<ReceiptType>() { receiptType }.AsReadOnly(),
                    request.HardwareProfileId);
    
                return getReceiptServiceRequest;
            }
    
            private void LogTransaction(GetXAndZReportReceiptRequest request)
            {
                SaveTransactionLogServiceRequest serviceRequest = null;
    
                if (request.ReceiptType == ReceiptType.XReport)
                {
                    serviceRequest = new SaveTransactionLogServiceRequest(TransactionType.PrintX, request.TransactionId);
                }
                else if (request.ReceiptType == ReceiptType.ZReport)
                {
                    serviceRequest = new SaveTransactionLogServiceRequest(TransactionType.PrintZ, request.TransactionId);
                }
    
                if (serviceRequest != null)
                {
                    this.Context.Execute<Response>(serviceRequest);
                }
            }
        }
    }
}
