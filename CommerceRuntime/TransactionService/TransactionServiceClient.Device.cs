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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.ServiceModel;
        using System.Threading.Tasks;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            /// <summary>
            /// The default device type.
            /// </summary>
            private const int DefaultDeviceType = -1;

            /// <summary>
            /// Error code for attempting to activate from difference physical device.
            /// </summary>
            private const string AttemptToActivateFromDifferencePhysicalDeviceErrorCode = "AttemptToActivateFromDifferentPhysicalDevice";

            // Transaction service client method names.
            private const string ActivateDeviceMethodName = "ActivateDevice";
            private const string DeactivateDeviceMethodName = "DeactivateDevice";
            private const string AuthenticateDeviceMethodName = "AuthenticateDevice";
            private const string GetNumberSequeneceSeedDataMethodName = "GetNumberSequenceSeedData";
            private const string GetMediaStorageSasKeyMethodName = "GetMediaStorageSasKey";
            private const string UpdateApplicationVersionMethodName = "UpdateApplicationVersion";
            private const string GetAvailableTerminalsMethodName = "GetAvailableTerminals";
            private const string GetAvailableDevicesMethodName = "GetAvailableDevices";
            private const int DeviceActivationResponseSize = 19;
            private const int DeviceDeactivationResponseSize = 18;
            private const int DeviceDeactivationInvalidTokenResponseSize = 19;
            private const int DeviceAuthenticationResponseSize = 18;

            /// <summary>
            /// Activate a device in AX.
            /// </summary>
            /// <param name="deviceNumber">The device number.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="deviceId">The physical device identifier.</param>
            /// <param name="forceActivate">The value indicating whether to force the Activation of a device when the physical device identifiers are different.</param>
            /// <param name="deviceType">The device type (optional).</param>
            /// <returns>The device activation result object.</returns>
            internal DeviceActivationResult ActivateDevice(string deviceNumber, string terminalId, string staffId, string deviceId, bool forceActivate, int? deviceType)
            {
                ThrowIf.Null(deviceNumber, "deviceNumber");
                ThrowIf.Null(staffId, "staffId");

                // transform the customer to parameters
                object[] parameters = new object[] { deviceNumber, terminalId, staffId, deviceId, forceActivate, deviceType.GetValueOrDefault(TransactionServiceClient.DefaultDeviceType) };
                ReadOnlyCollection<object> data = null;

                try
                {
                    data = this.InvokeMethod(ActivateDeviceMethodName, parameters);
                }
                catch (HeadquarterTransactionServiceException exception)
                {
                    string errorCode = (string)exception.HeadquartersErrorData.FirstOrDefault();
                    if (AttemptToActivateFromDifferencePhysicalDeviceErrorCode.Equals(errorCode, StringComparison.OrdinalIgnoreCase))
                    {
                        RetailLogger.Log.CrtServicesAttemptToActivateFromDifferentPhysicalDevice(staffId, deviceNumber, deviceId, terminalId);

                        throw new CommerceException(
                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AttemptToActivateFromDifferentPhysicalDevice.ToString(),
                            exception,
                            string.Format("Attempt to activate an activated device '{0}' from another physical device.", deviceNumber));
                    }
                    else
                    {
                        throw;
                    }
                }

                if (data == null || data.Count < DeviceActivationResponseSize)
                {
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                // Parse response data
                Microsoft.Dynamics.Commerce.Runtime.DataModel.Device device = this.CreateDevice(data);
                var result = new DeviceActivationResult { Device = device };
                return result;
            }

            /// <summary>
            /// Deactivate a device in AX.
            /// </summary>
            /// <param name="deviceNumber">The device number.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="deviceToken">The device token.</param>
            /// <returns>The deactivated device.</returns>
            internal DeviceDeactivationResult DeactivateDevice(string deviceNumber, string terminalId, string staffId, string deviceToken)
            {
                const int InvalidTokenError = 1;
                ThrowIf.Null(deviceNumber, "deviceNumber");
                ThrowIf.Null(terminalId, "terminalId");
                ThrowIf.Null(staffId, "staffId");
                ReadOnlyCollection<object> data;
                HeadquarterTransactionServiceException deactivationException = null;

                try
                {
                    data = this.InvokeMethod(DeactivateDeviceMethodName, new object[] { deviceNumber, terminalId, staffId, deviceToken });
                }
                catch (HeadquarterTransactionServiceException exception)
                {
                    data = exception.HeadquartersErrorData;

                    if (data.Count == DeviceDeactivationInvalidTokenResponseSize)
                    {
                        int errorCode = (int)data[DeviceDeactivationInvalidTokenResponseSize - 1];
                        if (errorCode == InvalidTokenError)
                        {
                            RetailLogger.Log.CrtServicesDeactiveDeviceFailedDueToInvalidToken(staffId, deviceNumber, terminalId);
                            deactivationException = exception;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }

                if (data == null || data.Count < DeviceDeactivationResponseSize)
                {
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                Microsoft.Dynamics.Commerce.Runtime.DataModel.Device device = this.CreateDevice(data);

                return new DeviceDeactivationResult() { Device = device, ErrorMessage = deactivationException == null ? null : deactivationException.Message };
            }

            /// <summary>
            /// Authenticate a device in AX.
            /// </summary>
            /// <param name="deviceNumber">The logical device number.</param>
            /// <param name="deviceToken">Device Token.</param>
            /// <returns>The authenticated device.</returns>
            internal Microsoft.Dynamics.Commerce.Runtime.DataModel.Device AuthenticateDevice(string deviceNumber, string deviceToken)
            {
                ThrowIf.Null(deviceToken, "deviceToken");
                Microsoft.Dynamics.Commerce.Runtime.DataModel.Device device;

                object[] parameters;
                parameters = new object[] { deviceNumber, deviceToken };

                var data = this.InvokeMethod(AuthenticateDeviceMethodName, parameters);

                if (data == null || data.Count < DeviceAuthenticationResponseSize)
                {
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                device = this.CreateDevice(data);
                device.Token = deviceToken;
                return device;
            }

            /// <summary>
            /// Gets the number sequence seed data values from headquarters.
            /// </summary>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <returns>The collection of number sequence seed values.</returns>
            internal PagedResult<NumberSequenceSeedData> GetNumberSequenceSeedData(string terminalId)
            {
                var data = this.InvokeMethod(
                    GetNumberSequeneceSeedDataMethodName,
                    terminalId);

                // Parse numbersequence seed data collection.
                string seedDataCollectionXml = (string)data[0];
                return SerializationHelper.DeserializeObjectDataContractFromXml<NumberSequenceSeedData[]>(seedDataCollectionXml).AsPagedResult();
            }

            /// <summary>
            /// Returns the media storage SAS key and its expiration date time as <see cref="DatetimeOffset"/>.
            /// </summary>
            /// <returns>The media storage SAS key details.</returns>
            internal MediaStorageSasDetails GetMediaStorageSasKeyDetails()
            {
                var data = this.InvokeMethod(GetMediaStorageSasKeyMethodName);
                string sasKey = (string)data[0];
                DateTime saskeyExpirationDateTimeInUtc = (DateTime)data[1];
                DateTimeOffset saskeyExpirationDateTimeOffsetInUtc = new DateTimeOffset(saskeyExpirationDateTimeInUtc);
                MediaStorageSasDetails mediaSasKeyDetails = new MediaStorageSasDetails();
                mediaSasKeyDetails.SasKey = sasKey;
                mediaSasKeyDetails.SasKeyExpirationDateTimeUtc = saskeyExpirationDateTimeOffsetInUtc;
                return mediaSasKeyDetails;
            }

            /// <summary>
            /// Updates client application version in AX.
            /// </summary>
            /// <param name="appVersion">The client application version.</param>
            internal void UpdateApplicationVersion(string appVersion)
            {
                string deviceNumber = this.context.GetPrincipal().DeviceNumber;

                ThrowIf.Null(appVersion, "appVersion");
                ThrowIf.NullOrWhiteSpace(deviceNumber, "deviceNumber");

                var parameters = new object[] { deviceNumber, appVersion };
                Task.Run(() => this.InvokeMethodNoDataReturn(UpdateApplicationVersionMethodName, parameters));
            }

            /// <summary>
            /// Retrieves terminal and device association information data from headquarters.
            /// </summary>
            /// <param name="orgUnitNumber">The store number.</param>
            /// <param name="deviceType">The device type value.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>The paged results of terminal info of the given store.</returns>
            internal PagedResult<Microsoft.Dynamics.Commerce.Runtime.DataModel.TerminalInfo> GetTerminalInfo(string orgUnitNumber, int deviceType, QueryResultSettings settings)
            {
                ThrowIf.NullOrWhiteSpace(orgUnitNumber, "orgUnitNumber");
                ThrowIf.Null(settings, "settings");
                ThrowIf.Null(settings.Paging, "settings.Paging");

                var terminalInfoList = new List<Microsoft.Dynamics.Commerce.Runtime.DataModel.TerminalInfo>();
                var data = this.InvokeMethodAllowNullResponse(
                    GetAvailableTerminalsMethodName,
                    new object[] { orgUnitNumber, deviceType, settings.Paging.NumberOfRecordsToFetch, settings.Paging.Skip });

                if (data != null)
                {
                    var xmlTerminalInfoList = (string)data[0];
                    var availableTerminalInfo = SerializationHelper.DeserializeObjectDataContractFromXml<Serialization.TerminalInfo[]>(xmlTerminalInfoList);

                    if (availableTerminalInfo != null)
                    {
                        terminalInfoList = availableTerminalInfo.Select(terminalInfo => new Microsoft.Dynamics.Commerce.Runtime.DataModel.TerminalInfo()
                        {
                            TerminalId = terminalInfo.TerminalId,
                            DeviceNumber = terminalInfo.DeviceNumber,
                            Name = terminalInfo.Name,
                            DeviceType = terminalInfo.DeviceType,
                            ActivationStatusValue = terminalInfo.ActivationStatusValue
                        }).ToList();
                    }
                }

                return new PagedResult<Microsoft.Dynamics.Commerce.Runtime.DataModel.TerminalInfo>(terminalInfoList.AsReadOnly(), settings.Paging);
            }

            /// <summary>
            /// Retrieves devices available for activation from headquarters.
            /// </summary>
            /// <param name="deviceType">The device type value.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>The paged results of devices matches the given device type.</returns>
            internal PagedResult<Microsoft.Dynamics.Commerce.Runtime.DataModel.Device> GetAvailableDevices(int deviceType, QueryResultSettings settings)
            {
                ThrowIf.Null(settings, "settings");
                ThrowIf.Null(settings.Paging, "settings.Paging");

                var devices = new List<Microsoft.Dynamics.Commerce.Runtime.DataModel.Device>();
                var data = this.InvokeMethodAllowNullResponse(
                    GetAvailableDevicesMethodName,
                    new object[] { deviceType, settings.Paging.NumberOfRecordsToFetch, settings.Paging.Skip });

                if (data != null)
                {
                    var xmlDevices = (string)data[0];
                    var availableDevices = SerializationHelper.DeserializeObjectDataContractFromXml<Serialization.Device[]>(xmlDevices);

                    if (availableDevices != null)
                    {
                        devices = availableDevices.Select(device => new Microsoft.Dynamics.Commerce.Runtime.DataModel.Device()
                        {
                            DeviceNumber = device.DeviceNumber,
                            DeviceType = device.DeviceType,
                            ActivationStatusValue = device.ActivationStatusValue
                        }).ToList();
                    }
                }

                return new PagedResult<Microsoft.Dynamics.Commerce.Runtime.DataModel.Device>(devices.AsReadOnly(), settings.Paging);
            }

            /// <summary>
            /// Creates the device given the data collection return by AX.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <returns>The device.</returns>
            private Microsoft.Dynamics.Commerce.Runtime.DataModel.Device CreateDevice(ReadOnlyCollection<object> data)
            {
                var device = new Microsoft.Dynamics.Commerce.Runtime.DataModel.Device();
                device.TerminalRecordId = (long)data[0];
                device.ChannelId = (long)data[1];
                device.TerminalId = (string)data[2];
                device.ChannelName = (string)data[3];
                device.DeviceTypeRecordId = (long)data[4];
                device.TokenData = (string)data[5];
                device.TokenSalt = (string)data[6];
                device.TokenAlgorithm = (string)data[7];
                device.ActivatedDateTime = new DateTimeOffset((DateTime)data[8]);
                device.ActivationStatus = (DeviceActivationStatus)data[9];
                device.DeactivateComments = (string)data[10];
                device.DeactivatedDateTime = new DateTimeOffset((DateTime)data[11]);
                device.Description = (string)data[12];
                device.TokenIssueTime = new DateTimeOffset((DateTime)data[13]);
                device.UseInMemoryDeviceDataStorage = Convert.ToBoolean((int)data[14]);
                device.DeviceId = (string)data[15];
                device.DeviceNumber = (string)data[16];
                device.RecordId = (long)data[17];
                device.Token = (string)data[18];

                return device;
            }
        }
    }
}
