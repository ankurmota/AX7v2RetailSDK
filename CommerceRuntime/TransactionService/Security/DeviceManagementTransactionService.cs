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
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// Device Service.
        /// </summary>
        public class DeviceManagementTransactionService : IRequestHandler
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
                    typeof(ActivateDeviceRealtimeRequest),
                    typeof(DeactivateDeviceRealtimeRequest),
                    typeof(AuthenticateDeviceRealtimeRequest),
                    typeof(GetNumberSequenceSeedDataRealtimeRequest),
                    typeof(GetTerminalInfoRealtimeRequest),
                    typeof(GetAvailableDevicesRealtimeRequest)
                };
                }
            }

            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(ActivateDeviceRealtimeRequest))
                {
                    response = ActivateDevice((ActivateDeviceRealtimeRequest)request);
                }
                else if (requestType == typeof(DeactivateDeviceRealtimeRequest))
                {
                    response = DeactivateDevice((DeactivateDeviceRealtimeRequest)request);
                }
                else if (requestType == typeof(AuthenticateDeviceRealtimeRequest))
                {
                    response = AuthenticateDevice((AuthenticateDeviceRealtimeRequest)request);
                }
                else if (requestType == typeof(GetNumberSequenceSeedDataRealtimeRequest))
                {
                    response = GetNumberSequenceFromHeadquarters((GetNumberSequenceSeedDataRealtimeRequest)request);
                }
                else if (requestType == typeof(GetTerminalInfoRealtimeRequest))
                {
                    response = GetTerminalInfo((GetTerminalInfoRealtimeRequest)request);
                }
                else if (requestType == typeof(GetAvailableDevicesRealtimeRequest))
                {
                    response = GetAvailableDevices((GetAvailableDevicesRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Retrieves number sequence seed data from headquarters.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            private static GetNumberSequenceSeedDataRealtimeResponse GetNumberSequenceFromHeadquarters(GetNumberSequenceSeedDataRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                PagedResult<NumberSequenceSeedData> seedData = transactionService.GetNumberSequenceSeedData(request.TerminalId);
                return new GetNumberSequenceSeedDataRealtimeResponse(seedData);
            }

            /// <summary>
            /// Activates the device.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static ActivateDeviceRealtimeResponse ActivateDevice(ActivateDeviceRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                // Call transaction service to activate the device.
                var activationResult = transactionService.ActivateDevice(request.DeviceNumber, request.TerminalId, request.StaffId, request.DeviceId, request.ForceActivation, request.DeviceType);
                var response = new ActivateDeviceRealtimeResponse(activationResult);
                return response;
            }

            /// <summary>
            /// Deactivates the device.
            /// </summary>
            /// <param name="request">The device deactivation request.</param>
            /// <returns>The device deactivation response.</returns>
            private static DeactivateDeviceRealtimeResponse DeactivateDevice(DeactivateDeviceRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                DeviceDeactivationResult deviceDeactivationResult;

                try
                {
                    // Call transaction service to deactivate the device.
                    deviceDeactivationResult = transactionService.DeactivateDevice(request.DeviceNumber, request.TerminalId, request.StaffId, request.DeviceToken);
                }
                catch (Exception ex)
                {
                    throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceDeactivationFailed, ex, ex.Message);
                }

                return new DeactivateDeviceRealtimeResponse(deviceDeactivationResult);
            }

            /// <summary>
            /// Authenticates the device.
            /// </summary>
            /// <param name="request">The device authentication request.</param>
            /// <returns>The response.</returns>
            private static AuthenticateDeviceRealtimeResponse AuthenticateDevice(AuthenticateDeviceRealtimeRequest request)
            {
                ThrowIf.Null(request.Device, "request.Device");

                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                Device device = transactionService.AuthenticateDevice(request.Device.DeviceNumber, request.Device.TokenData);
                return new AuthenticateDeviceRealtimeResponse(device);
            }

            /// <summary>
            /// Retrieves terminal and device association information data from headquarters.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            private static EntityDataServiceResponse<TerminalInfo> GetTerminalInfo(GetTerminalInfoRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                PagedResult<TerminalInfo> terminalInfo = transactionService.GetTerminalInfo(request.OrgUnitNumber, request.DeviceType, request.QueryResultSettings);
                return new EntityDataServiceResponse<TerminalInfo>(terminalInfo);
            }

            /// <summary>
            /// Retrieves devices available for activation from headquarters.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            private static EntityDataServiceResponse<Device> GetAvailableDevices(GetAvailableDevicesRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                PagedResult<Device> devices = transactionService.GetAvailableDevices(request.DeviceType, request.QueryResultSettings);
                return new EntityDataServiceResponse<Device>(devices);
            }
        }
    }
}