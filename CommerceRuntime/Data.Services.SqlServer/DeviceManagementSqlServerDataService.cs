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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The device management SQL server data service.
        /// </summary>
        public class DeviceManagementSqlServerDataService : IRequestHandler
        {
            private const string CreateUpdateDeviceSprocName = "CREATEUPDATEDEVICE";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(CreateOrUpdateDeviceDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(CreateOrUpdateDeviceDataRequest))
                {
                    response = this.CreateOrUpdateDevice((CreateOrUpdateDeviceDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Creates or Updates the retail device.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private NullResponse CreateOrUpdateDevice(CreateOrUpdateDeviceDataRequest request)
            {
                ThrowIf.Null(request.Device, "request.Device");
    
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters["@dt_ActivatedDateTime"] = request.Device.ActivatedDateTime;
                    parameters["@i_ActivationStatus"] = request.Device.ActivationStatus;
                    parameters["@nvc_DeactivateComments"] = request.Device.DeactivateComments;
                    parameters["@dt_DeactivatedDateTime"] = request.Device.DeactivatedDateTime;
                    parameters["@nvc_Description"] = request.Device.Description;
                    parameters["@nvc_DeviceId"] = request.Device.DeviceNumber;
                    parameters["@nvc_Terminal"] = request.Device.TerminalId;
                    parameters["@bi_TypeRecordId"] = request.Device.DeviceTypeRecordId;
                    parameters["@nvc_DeviceTokenData"] = request.Device.TokenData;
                    parameters["@nvc_DeviceTokenSalt"] = request.Device.TokenSalt;
                    parameters["@nvc_DeviceTokenAlgorithm"] = request.Device.TokenAlgorithm;
                    parameters["@dt_DeviceTokenIssueTime"] = request.Device.TokenIssueTime;
                    parameters["@i_UseInMemoryDeviceDataStorage"] = request.Device.UseInMemoryDeviceDataStorage;
                    parameters["@bi_RecordId"] = request.Device.RecordId;
    
                    databaseContext.ExecuteStoredProcedureNonQuery(CreateUpdateDeviceSprocName, parameters);
                }
    
                DeviceL2CacheDataStoreAccessor levelL2CacheDataAccessor = this.GetDeviceL2CacheDataStoreAccessor(request.RequestContext);
                levelL2CacheDataAccessor.ClearCacheDeviceById(request.Device.DeviceNumber);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets the cache accessor for the device data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="DeviceL2CacheDataStoreAccessor"/> class.</returns>
            private DeviceL2CacheDataStoreAccessor GetDeviceL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new DeviceL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
        }
    }
}
