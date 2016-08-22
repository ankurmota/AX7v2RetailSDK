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
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Trigger that performs permission checks for workflows.
        /// </summary>
        public class WorkflowsAuthorizationTrigger : IRequestTrigger
        {
            /// <summary>
            /// Gets the collection of request types supported by this trigger.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetCartRequest),
                        typeof(SaveCartRequest),
                        typeof(AddOrRemoveDiscountCodesRequest),
                        typeof(GetOrdersRequest),
                        typeof(GetAvailableShiftsRequest),
                        typeof(SaveCartLinesRequest),
                        typeof(GetXAndZReportReceiptRequest),
                        typeof(ChangeShiftStatusRequest),
                        typeof(UpdateCustomerRequest),
                        typeof(EmployeeTimeRegistrationRequest)
                   };
                }
            }

            /// <summary>
            /// Invoked before request has been processed by <see cref="IRequestHandler"/>.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            public void OnExecuting(Request request)
            {
                ThrowIf.Null(request, "request");

                Type requestType = request.GetType();

                if (requestType == typeof(GetCartRequest))
                {
                    OnGetCartExecuting((GetCartRequest)request);
                }
                else if (requestType == typeof(SaveCartRequest))
                {
                    OnSaveCartExecuting(request);
                }
                else if (requestType == typeof(GetOrdersRequest))
                {
                    OnGetOrdersExecuting((GetOrdersRequest)request);
                }
                else if (requestType == typeof(GetAvailableShiftsRequest))
                {
                    OnGetAvailableShiftsExecuting((GetAvailableShiftsRequest)request);
                }
                else if (requestType == typeof(SaveCartLinesRequest))
                {
                    OnSaveCartLinesExecuting((SaveCartLinesRequest)request);
                }
                else if (requestType == typeof(GetXAndZReportReceiptRequest))
                {
                    OnGetXAndZReportReceiptExecuting((GetXAndZReportReceiptRequest)request);
                }
                else if (requestType == typeof(ChangeShiftStatusRequest))
                {
                    OnChangeShiftStatusExecuting((ChangeShiftStatusRequest)request);
                }
                else if (requestType == typeof(UpdateCustomerRequest))
                {
                    OnUpdateCustomerExecuting((UpdateCustomerRequest)request);
                }
                else if (requestType == typeof(AddOrRemoveDiscountCodesRequest))
                {
                    OnAddOrRemoveDiscountCodesExecuting((AddOrRemoveDiscountCodesRequest)request);
                }
                else if (requestType == typeof(SaveCartRequest))
                {
                    OnSaveCartExecuting((SaveCartRequest)request);
                }
                else if (requestType == typeof(EmployeeTimeRegistrationRequest))
                {
                    OnEmployeeTimeRegistrationExecuting((EmployeeTimeRegistrationRequest)request);
                }
            }

            /// <summary>
            /// Invoked after request has been processed by <see cref="IRequestHandler"/>.
            /// </summary>
            /// <param name="request">The request message processed by handler.</param>
            /// <param name="response">The response message generated by handler.</param>
            public void OnExecuted(Request request, Response response)
            {
            }

            private static void OnSaveCartExecuting(SaveCartRequest request)
            {
                if (request.Cart.CartLines.Any(l => string.IsNullOrWhiteSpace(l.LineId) && !l.IsGiftCardLine && !l.IsVoided && l.Quantity >= 0m))
                {
                    // Checks for gift cards and returns are done separately.
                    request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ItemSale));
                }
            }

            private static void OnAddOrRemoveDiscountCodesExecuting(AddOrRemoveDiscountCodesRequest request)
            {
                if (request.RequestContext.GetPrincipal().IsInRole(CommerceRoles.Employee))
                {
                    var checkAccessRequest = new CheckAccessHasShiftServiceRequest();
                    request.RequestContext.Execute<NullResponse>(checkAccessRequest);
                }

                if (request.DiscountCodes.Any())
                {
                    request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.DiscountCodeBarcode));
                }
            }

            private static void OnUpdateCustomerExecuting(UpdateCustomerRequest request)
            {
                // Check for permissions if a new shipping address is being added.
                if (request.UpdatedCustomer.Addresses.Any(a => a.RecordId == 0))
                {
                    request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ShippingAddressAdd));
                }
            }

            private static void OnChangeShiftStatusExecuting(ChangeShiftStatusRequest request)
            {
                switch (request.ToStatus)
                {
                    case ShiftStatus.Closed:
                        bool isNonDrawerOperationCheckRequired = IsNonDrawerOperationCheckRequired(request.RequestContext, request.ShiftId);
                        request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.CloseShift, isNonDrawerOperationCheckRequired));
                        break;
                    case ShiftStatus.BlindClosed:
                        request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.BlindCloseShift));
                        break;
                    case ShiftStatus.Suspended:
                        request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SuspendShift));
                        break;
                }
            }

            private static void OnGetXAndZReportReceiptExecuting(GetXAndZReportReceiptRequest request)
            {
                switch (request.ReceiptType)
                {
                    case ReceiptType.XReport:
                        bool isNonDrawerOperationCheckRequired = IsNonDrawerOperationCheckRequired(request.RequestContext, request.ShiftId);
                        request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.PrintX, isNonDrawerOperationCheckRequired));
                        break;
                    case ReceiptType.ZReport:
                        request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.PrintZ));
                        break;
                }
            }

            private static void OnSaveCartLinesExecuting(SaveCartLinesRequest request)
            {
                switch (request.OperationType)
                {
                    case TransactionOperationType.Create:
                        request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ItemSale));
                        break;
                    case TransactionOperationType.Void:
                        request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.VoidItem));
                        break;
                }
            }

            private static void OnGetAvailableShiftsExecuting(GetAvailableShiftsRequest request)
            {
                if (request.Status == ShiftStatus.BlindClosed)
                {
                    request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ShowBlindClosedShifts));
                }
            }

            private static void OnGetOrdersExecuting(GetOrdersRequest request)
            {
                if (!string.IsNullOrWhiteSpace(request.Criteria.CustomerAccountNumber) ||
                    !string.IsNullOrWhiteSpace(request.Criteria.CustomerFirstName) ||
                    !string.IsNullOrWhiteSpace(request.Criteria.CustomerLastName))
                {
                    request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.CustomerTransactions));
                }
            }

            private static void OnSaveCartExecuting(Request request)
            {
                if (request.RequestContext.GetPrincipal().IsInRole(CommerceRoles.Employee))
                {
                    var checkAccessRequest = new CheckAccessHasShiftServiceRequest();
                    request.RequestContext.Execute<NullResponse>(checkAccessRequest);
                }
            }

            private static void OnGetCartExecuting(GetCartRequest request)
            {
                if (!string.IsNullOrEmpty(request.SearchCriteria.CustomerAccountNumber))
                {
                    var checkAccessRequest = new CheckAccessToCustomerAccountServiceRequest(request.SearchCriteria.CustomerAccountNumber);
                    request.RequestContext.Execute<NullResponse>(checkAccessRequest);
                }
            }

            private static void OnEmployeeTimeRegistrationExecuting(EmployeeTimeRegistrationRequest request)
            {
                // View time clock entries operation is identified by LogBook activity type with IsManagerLogbook set to true.
                bool isViewTimeClockEntries = (request.EmployeeActivityType == EmployeeActivityType.Logbook) && request.IsManagerLogbook;
                if (!isViewTimeClockEntries)
                {
                    // Checks for the time registration access permission if it is not view time clock entries operation.
                    request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.TimeRegistration));
                }
            }

            private static bool IsNonDrawerOperationCheckRequired(RequestContext context, long? shiftId)
            {
                // Triggers non-drawer operation check if the context is in non-drawer mode and the shift Id is not provided in the request, 
                // if the shift Id is provided (e.g. for blind-closed shift), we should skip the non-drawer operation check
                return context.GetPrincipal().ShiftId == 0 && !shiftId.HasValue;
            }
        }
    }
}