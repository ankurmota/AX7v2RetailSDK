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
        using Commerce.Runtime.Services.Security;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Device Service.
        /// </summary>
        public class DeviceManagementService : IRequestHandler
        {
            /// <summary>
            /// The token separator (cannot be same as the character appeared in the token fields).
            /// </summary>
            private const char Separator = ':';
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(ActivateDeviceServiceRequest),
                        typeof(AuthenticateDeviceServiceRequest),
                        typeof(DeactivateDeviceServiceRequest),
                        typeof(CreateHardwareStationTokenServiceRequest),
                        typeof(ValidateHardwareStationTokenServiceRequest)
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
                if (requestType == typeof(ActivateDeviceServiceRequest))
                {
                    response = ActivateDevice((ActivateDeviceServiceRequest)request);
                }
                else if (requestType == typeof(AuthenticateDeviceServiceRequest))
                {
                    response = AuthenticateDevice((AuthenticateDeviceServiceRequest)request);
                }
                else if (requestType == typeof(DeactivateDeviceServiceRequest))
                {
                    response = DeactivateDevice((DeactivateDeviceServiceRequest)request);
                }
                else if (requestType == typeof(CreateHardwareStationTokenServiceRequest))
                {
                    response = CreateHardwareStationToken((CreateHardwareStationTokenServiceRequest)request);
                }
                else if (requestType == typeof(ValidateHardwareStationTokenServiceRequest))
                {
                    response = ValidateHardwareStationToken((ValidateHardwareStationTokenServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Activates the device and updates the activated device in the channel database.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static ActivateDeviceServiceResponse ActivateDevice(ActivateDeviceServiceRequest request)
            {
                ActivateDeviceRealtimeRequest realtimeRequest = new ActivateDeviceRealtimeRequest(request.DeviceNumber, request.TerminalId, request.StaffId, request.DeviceId, request.ForceActivation, request.DeviceType);
                ActivateDeviceRealtimeResponse realtimeResponse = request.RequestContext.Execute<ActivateDeviceRealtimeResponse>(realtimeRequest);
    
                if (realtimeResponse.DeviceActivationResult.Device != null)
                {
                    realtimeResponse.DeviceActivationResult.Device.Token = GenerateDeviceToken(realtimeResponse.DeviceActivationResult.Device);
                }
    
                // Creating or updating the activated device in the channel db.
                CreateOrUpdateDeviceDataRequest createOrUpdateDeviceDataRequest = new CreateOrUpdateDeviceDataRequest(realtimeResponse.DeviceActivationResult.Device);
                request.RequestContext.Execute<NullResponse>(createOrUpdateDeviceDataRequest);
                return new ActivateDeviceServiceResponse(realtimeResponse.DeviceActivationResult);
            }
    
            /// <summary>
            /// Deactivates the device.
            /// </summary>
            /// <param name="request">The device deactivation request.</param>
            /// <returns>The device deactivation response.</returns>
            private static DeactivateDeviceServiceResponse DeactivateDevice(DeactivateDeviceServiceRequest request)
            {
                Device device = ConstructDeviceFromToken(request.DeviceToken);
                DeactivateDeviceRealtimeRequest realtimeRequest = new DeactivateDeviceRealtimeRequest(request.DeviceNumber, request.TerminalId, request.StaffId, device.TokenData);
                DeactivateDeviceRealtimeResponse realtimeResponse = request.RequestContext.Execute<DeactivateDeviceRealtimeResponse>(realtimeRequest);
    
                // Updating the activated device in the channel db.
                if (realtimeResponse.DeactivationResult.Device != null)
                {
                    CreateOrUpdateDeviceDataRequest createOrUpdateDeviceDataRequest = new CreateOrUpdateDeviceDataRequest(realtimeResponse.DeactivationResult.Device);
                    request.RequestContext.Execute<NullResponse>(createOrUpdateDeviceDataRequest);
                }
    
                return new DeactivateDeviceServiceResponse(realtimeResponse.DeactivationResult);
            }
    
            /// <summary>
            /// Authenticates the device.
            /// </summary>
            /// <param name="request">The device authentication request.</param>
            /// <returns>The response.</returns>
            private static AuthenticateDeviceServiceResponse AuthenticateDevice(AuthenticateDeviceServiceRequest request)
            {
                Device device = new Device();
    
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    throw new ArgumentException("request.Token is not set", "request");
                }
    
                device = ConstructDeviceFromToken(request.Token);
    
                try
                {
                    // Try to validate the device token using the channel database
                    device = ValidateDeviceTokenLocally(device.DeviceNumber, device.TokenData, request.Token, request.RequestContext);
                }
                catch (DeviceAuthenticationException deviceAuthenticationException)
                {
                    RetailLogger.Log.CrtServicesDeviceManagementServiceDeviceAuthenticationInChannelDbFailure(device.ToString(), deviceAuthenticationException);
    
                    try
                    {
                        // If local authentication failed then try to contact AX for activation and refresh local data.
                        AuthenticateDeviceRealtimeRequest realtimeRequest = new AuthenticateDeviceRealtimeRequest(device);
                        device = request.RequestContext.Execute<AuthenticateDeviceRealtimeResponse>(realtimeRequest).Device;
                    }
                    catch (HeadquarterTransactionServiceException ex)
                    {
                        RetailLogger.Log.CrtServicesDeviceManagementServiceDeviceAuthenticationInAxFailure(device.ToString(), ex);
                        throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterTransactionServiceMethodCallFailure, ex, ex.Message)
                        {
                            LocalizedMessage = ex.LocalizedMessage
                        };
                    }
    
                    // Creating or updating the authenticated device in the channel db.
                    CreateOrUpdateDeviceDataRequest createOrUpdateDeviceDataRequest = new CreateOrUpdateDeviceDataRequest(device);
                    request.RequestContext.Execute<NullResponse>(createOrUpdateDeviceDataRequest);
                }
    
                return new AuthenticateDeviceServiceResponse(device);
            }
    
            /// <summary>
            /// Validate Device token.
            /// </summary>
            /// <param name="deviceId">The device identifier.</param>
            /// <param name="deviceTokenData">The device token data.</param>
            /// <param name="deviceToken">The device token.</param>
            /// <param name="requestContext">Request context.</param>
            /// <returns>Device response.</returns>
            private static Device ValidateDeviceTokenLocally(string deviceId, string deviceTokenData, string deviceToken, RequestContext requestContext)
            {
                var getTransactionServiceProfileDataRequest = new GetTransactionServiceProfileDataRequest();
                TransactionServiceProfile transactionServiceProfile = requestContext.Runtime.Execute<SingleEntityDataServiceResponse<TransactionServiceProfile>>(getTransactionServiceProfileDataRequest, requestContext).Entity;
    
                int deviceTokenExpirationInDays = transactionServiceProfile.DeviceTokenExpirationInDays;
    
                // only validate device if connecting against master database
                bool mustValidateActiveDevice = requestContext.Runtime.Configuration.IsMasterDatabaseConnectionString;
    
                // Get the device
                GetDeviceDataRequest getDeviceRequest = new GetDeviceDataRequest(deviceId, mustValidateActiveDevice);
                Device localDevice = requestContext.Execute<SingleEntityDataServiceResponse<Device>>(getDeviceRequest).Entity;
                if (localDevice == null)
                {
                    throw new DeviceAuthenticationException(
                        SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_LocalDeviceAuthenticationFailed,
                        string.Format("Device not found for device '{0}'.", deviceId));
                }
    
                localDevice.Token = deviceToken;
    
                string algorithm = localDevice.TokenAlgorithm;
    
                if (string.IsNullOrEmpty(algorithm))
                {
                    // string algorithm = transactionServiceProfile.DeviceTokenAlgorithm;
                    algorithm = "SHA256";
                }
    
                if (mustValidateActiveDevice)
                {
                    if (string.IsNullOrEmpty(localDevice.TokenSalt))
                    {
                        throw new DeviceAuthenticationException(
                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_LocalDeviceAuthenticationFailed, 
                            string.Format("TokenSalt not available for device '{0}'.", deviceId));
                    }
    
                    if (localDevice.ActivationStatus != DeviceActivationStatus.Activated)
                    {
                        throw new DeviceAuthenticationException(
                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_LocalDeviceAuthenticationFailed, 
                            string.Format("Device is not activated, actual state is '{0}'.", localDevice.ActivationStatus));
                    }
    
                    // Get hashed value for device token
                    HashDataServiceRequest hashDataServiceRequest = new HashDataServiceRequest(deviceTokenData, algorithm, localDevice.DeviceNumber, localDevice.TokenSalt);
                    string deviceTokenHash = requestContext.Execute<HashDataServiceResponse>(hashDataServiceRequest).Data;
    
                    // Validate the hashed device token with value in the database
                    if (!string.Equals(deviceTokenHash, localDevice.TokenData))
                    {
                        throw new DeviceAuthenticationException(
                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_LocalDeviceAuthenticationFailed,
                            string.Format("Device token for '{0}' record was not found.", deviceId));
                    }
    
                    if (!localDevice.TokenIssueTime.HasValue)
                    {
                        throw new ConfigurationException(
                            ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_ActivatedDeviceMissingTokenIssueDatetime,
                            string.Format("Activated device '{0}' does not have an associated token issue datetime.", deviceId));
                    }
    
                    if (DateTimeOffset.Compare(DateTimeOffset.Now, localDevice.TokenIssueTime.Value.AddDays(deviceTokenExpirationInDays)) > 0)
                    {
                        throw new DeviceAuthenticationException(
                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenExpired, 
                            string.Format("Device Token for device '{0}' expired.", deviceId));
                    }
                }
                else
                {
                    // when device comes from non-master database, it might have stale data
                    localDevice = ConstructDeviceFromToken(deviceToken);
                }
    
                return localDevice;
            }
    
            /// <summary>
            /// Creates hardware station token.
            /// </summary>
            /// <param name="request">The create authentication token request.</param>
            /// <returns>Create authentication token response.</returns>
            private static CreateHardwareStationTokenServiceResponse CreateHardwareStationToken(CreateHardwareStationTokenServiceRequest request)
            {
                // Authorization token (pairing key) for the client and hardware station.
                string pairingKey = Guid.NewGuid().ToString();
    
                // Validateion token for HS to get validated from Retail Server
                string validationToken = request.DeviceNumber + Separator + pairingKey + Separator + DateTimeOffset.UtcNow.ToFileTime();
    
                try
                {
                    CreateHardwareStationTokenResult result = new CreateHardwareStationTokenResult();

                    result.PairingKey = pairingKey;
                    result.HardwareStationToken = DeviceManagementService.EncryptData(
                        request.RequestContext,
                        validationToken,
                        request.CertificateThumbprint,
                        request.StoreName,
                        request.StoreLocation);

                    return new CreateHardwareStationTokenServiceResponse(result);
                }
                catch (Exception ex)
                {
                    throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_HardwareStationTokenCreationFailed, ex, "Failed to create a hardware station token.");
                }
            }
    
            /// <summary>
            /// Validates hardware station token.
            /// </summary>
            /// <param name="request">The validate authentication token request.</param>
            /// <returns>Validate hardware station token response.</returns>
            private static ValidateHardwareStationTokenServiceResponse ValidateHardwareStationToken(ValidateHardwareStationTokenServiceRequest request)
            {
                const int TokenExpirationInMinutes = 1;
                string deviceNumber = string.Empty;
                long tokenCreationFileTime;
                ValidateHardwareStationTokenResult result = new ValidateHardwareStationTokenResult();
    
                try
                {
                    string decryptedToken = DeviceManagementService.DecryptData(request.RequestContext, request.HardwareStationToken);
    
                    string[] fields = decryptedToken.Split(Separator);
                    deviceNumber = fields[0];
                    result.PairingKey = fields[1];
                    tokenCreationFileTime = long.Parse(fields[2], CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidHardwareStationToken, ex, "Hardware station token validation failed.");
                }
    
                if ((!request.DeviceNumber.Equals(deviceNumber, StringComparison.OrdinalIgnoreCase))
                    || (DateTimeOffset.UtcNow.CompareTo(DateTimeOffset.FromFileTime(tokenCreationFileTime).AddMinutes(TokenExpirationInMinutes)) > 0))
                {
                    throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidHardwareStationToken);
                }
    
                return new ValidateHardwareStationTokenServiceResponse(result);
            }
    
            private static string GenerateDeviceToken(Device device)
            {
                if (device == null)
                {
                    return null;
                }
    
                string token = string.Join(
                    Separator.ToString(),
                    device.DeviceNumber,
                    device.Token,
                    device.ChannelId,
                    device.TerminalRecordId);
    
                return token;
            }
    
            private static Device ConstructDeviceFromToken(string token)
            {
                Device device = null;
    
                string[] fields = token.Split(Separator);
                if (fields.Length == 4)
                {
                    device = new Device
                    {
                        DeviceNumber = fields[0],
                        TokenData = fields[1],
                        ChannelId = Convert.ToInt64(fields[2], CultureInfo.InvariantCulture),
                        TerminalRecordId = Convert.ToInt64(fields[3], CultureInfo.InvariantCulture),
                        Token = token,
                    };
                }
                else
                {
                    // Incorrect format of the token
                    throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenValidationFailed, "Incorrect device token format.");
                }
    
                return device;
            }

            /// <summary>
            /// Encrypts the data.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="data">The data.</param>
            /// <param name="certificateThumbprint">The thumbprint of the certificate used to encrypt the token.</param>
            /// <param name="storeName">The (optional) certificate store name.</param>
            /// <param name="storeLocation">The (optional) certificate store location.</param>
            /// <returns>The encrypted data.</returns>
            private static string EncryptData(RequestContext context, string data, string certificateThumbprint, string storeName, string storeLocation)
            {
                CertificateEncryptionServiceRequest encryptDataServiceRequest = new CertificateEncryptionServiceRequest(data, certificateThumbprint, storeName, storeLocation);

                string encryptedData = context.Execute<CertificateEncryptionServiceResponse>(encryptDataServiceRequest).Data;

                return encryptedData;
            }

            /// <summary>
            /// Decrypts the data.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="data">The data.</param>
            /// <returns>The decrypted data.</returns>
            private static string DecryptData(RequestContext context, string data)
            {
                CertificateDecryptionServiceRequest decryptDataServiceRequest = new CertificateDecryptionServiceRequest(data);

                string decryptedData = context.Execute<CertificateDecryptionServiceResponse>(decryptDataServiceRequest).Data;

                return decryptedData;
            }
        }
    }
}