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
        public class DeviceManagementTransactionServiceDemoMode : IRequestHandler
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
            /// Executes the specified request.
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
                    response = GetNumberSequence();
                }
                else if (requestType == typeof(GetTerminalInfoRealtimeRequest))
                {
                    response = GetTerminalInfo();
                }
                else if (requestType == typeof(GetAvailableDevicesRealtimeRequest))
                {
                    response = GetAvailableDevices();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Retrieves number sequence seed data locally.
            /// </summary>
            /// <returns>The response message.</returns>
            private static GetNumberSequenceSeedDataRealtimeResponse GetNumberSequence()
            {
                return new GetNumberSequenceSeedDataRealtimeResponse(new List<NumberSequenceSeedData>().AsPagedResult());
            }

            /// <summary>
            /// Activates the device.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static ActivateDeviceRealtimeResponse ActivateDevice(ActivateDeviceRealtimeRequest request)
            {
                var getDeviceRequest = new GetDeviceDataRequest(request.DeviceNumber, isActivatedOnly: false);
                Device device = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<Device>>(getDeviceRequest, request.RequestContext).Entity;
                device.ActivatedDateTime = device.ActivatedDateTime ?? DateTimeOffset.UtcNow;
                device.DeactivateComments = device.DeactivateComments ?? string.Empty;
                device.DeactivatedDateTime = device.DeactivatedDateTime ?? DateTimeOffset.MinValue;
                device.TokenIssueTime = device.TokenIssueTime ?? DateTimeOffset.UtcNow;

                var result = new DeviceActivationResult();
                if (device == null)
                {
                    // Device is not found, throws exception.
                    string message = string.Format("The input device number '{0}' does not exist in demo database.", request.DeviceNumber);
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceConfigurationNotFound, message);
                }
                else if (string.IsNullOrWhiteSpace(device.TerminalId))
                {
                    // If device is found but the terminal associated with the device is not found, try to find the terminal using input terminal id.
                    var columnSet = new ColumnSet(Terminal.RecordIdColumn, Terminal.TerminalIdColumn, Terminal.ChannelIdColumn);
                    var settings = new QueryResultSettings(columnSet, PagingInfo.AllRecords);
                    var getTerminalRequest = new GetTerminalDataRequest(request.TerminalId, settings);
                    Terminal terminal = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<Terminal>>(getTerminalRequest, request.RequestContext).Entity;

                    if (terminal == null)
                    {
                        string message = string.Format("The input device number '{0}' and terminal identifier '{1}' do not exist in demo database.", request.DeviceNumber, request.TerminalId);
                        throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceConfigurationNotFound, message);
                    }

                    result.Device = new Device
                    {
                        DeviceId = request.DeviceId,
                        DeviceNumber = request.DeviceNumber,
                        TerminalRecordId = terminal.RecordId,
                        TerminalId = terminal.TerminalId,
                        ChannelId = terminal.ChannelId,
                        DeviceTypeRecordId = device.DeviceTypeRecordId,
                        ActivatedDateTime = device.ActivatedDateTime,
                        TokenIssueTime = device.TokenIssueTime,
                        DeactivatedDateTime = device.DeactivatedDateTime,
                        DeactivateComments = device.DeactivateComments,
                        TokenData = device.TokenData,
                        TokenSalt = device.TokenSalt
                    };
                }
                else
                {
                    // Both the device and associated terminal are found.
                    device.DeviceNumber = request.DeviceNumber;
                    device.DeviceId = request.DeviceId;
                    result.Device = device;
                }

                var response = new ActivateDeviceRealtimeResponse(result);
                return response;
            }

            /// <summary>
            /// Deactivates the device.
            /// </summary>
            /// <param name="request">The device deactivation request.</param>
            /// <returns>The device deactivation response.</returns>
            private static DeactivateDeviceRealtimeResponse DeactivateDevice(DeactivateDeviceRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");

                var getDeviceRequest = new GetDeviceDataRequest(request.DeviceNumber, isActivatedOnly: false);
                Device device = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<Device>>(getDeviceRequest, request.RequestContext).Entity;

                device.ActivationStatus = DeviceActivationStatus.Deactivated;
                device.ActivatedDateTime = device.ActivatedDateTime ?? DateTimeOffset.UtcNow;
                device.DeactivateComments = device.DeactivateComments ?? string.Empty;
                device.DeactivatedDateTime = device.DeactivatedDateTime ?? DateTimeOffset.MinValue;
                device.TokenIssueTime = device.TokenIssueTime ?? DateTimeOffset.UtcNow;
                return new DeactivateDeviceRealtimeResponse(new DeviceDeactivationResult() { Device = device });
            }

            /// <summary>
            /// Authenticates the device.
            /// </summary>
            /// <param name="request">The device authentication request.</param>
            /// <returns>The response.</returns>
            private static AuthenticateDeviceRealtimeResponse AuthenticateDevice(AuthenticateDeviceRealtimeRequest request)
            {
                var getDeviceRequest = new GetDeviceDataRequest(request.Device.DeviceNumber, isActivatedOnly: false);
                Device device = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<Device>>(getDeviceRequest, request.RequestContext).Entity;

                if (device == null)
                {
                    // Device is not found, throws exception.
                    string message = string.Format("The input device number '{0}' does not exist in demo database.", request.Device.DeviceNumber);
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceConfigurationNotFound, message);
                }

                device.ActivatedDateTime = device.ActivatedDateTime ?? DateTimeOffset.UtcNow;
                device.DeactivateComments = device.DeactivateComments ?? string.Empty;
                device.DeactivatedDateTime = device.DeactivatedDateTime ?? DateTimeOffset.MinValue;
                device.TokenIssueTime = device.TokenIssueTime ?? DateTimeOffset.UtcNow;

                return new AuthenticateDeviceRealtimeResponse(device);
            }

            /// <summary>
            /// Retrieves terminal and device association information data from headquarters.
            /// </summary>
            /// <returns>The paged results of terminal info of the given store.</returns>
            private static EntityDataServiceResponse<Device> GetTerminalInfo()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetTerminalInfo is not supported in demo mode.");
            }

            /// <summary>
            /// Retrieves devices available for activation from headquarters.
            /// </summary>
            /// <returns>The paged results of devices matches the given device type.</returns>
            private static EntityDataServiceResponse<Device> GetAvailableDevices()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetAvailableDevices is not supported in demo mode.");
            }
        }
    }
}