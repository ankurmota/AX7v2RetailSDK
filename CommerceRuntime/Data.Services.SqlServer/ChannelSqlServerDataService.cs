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
        using System.Collections.ObjectModel;
        using System.Diagnostics;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Channel SQL server data service class.
        /// </summary>
        public class ChannelSqlServerDataService : IRequestHandler
        {
            private const string ChannelCategoryAttributeViewName = "CHANNELCATEGORYATTRIBUTEVIEW";
            private const string StorageLookupView = "STORAGELOOKUPVIEW";
            private const string ChannelIdColumnName = "CHANNELID";
            private const string UpdateOnlineChannelPublishStatusSprocName = "UPDATEONLINECHANNELPUBLISHSTATUS";
            private const string GetDownloadingDataSprocName = "GETDOWNLOADINGDATA";
            private const string SaveChannelPropertySprocName = "SAVECHANNELPROPERTY";
            private const string GetDeviceConfigurationSprocName = "GETDEVICECONFIGURATION";

            private const string SqlParamChannelId = "@channelId";
            private const string TerminalIdVariableName = "@nvc_TerminalId";
            private const string IncludeImagesVariableName = "@b_IncludeImages";

            private const int MaxCachedCollectionSize = 500;
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new Type[]
                    {
                        typeof(GetDeviceConfigurationDataRequest),
                        typeof(GetChannelCategoryAttributesDataRequest),
                        typeof(ResolveOperatingUnitNumberDataRequest),
                        typeof(UpdateChannelPropertiesByChannelIdDataRequest),
                        typeof(UpdateOnlineChannelPublishStatusDataRequest),
                        typeof(GetDownloadingDataSetDataRequest)
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
    
                if (requestType == typeof(GetDeviceConfigurationDataRequest))
                {
                    response = this.GetDeviceConfiguration((GetDeviceConfigurationDataRequest)request);
                }
                else if (requestType == typeof(GetChannelCategoryAttributesDataRequest))
                {
                    response = this.GetChannelCategoryAttributes((GetChannelCategoryAttributesDataRequest)request);
                }
                else if (requestType == typeof(ResolveOperatingUnitNumberDataRequest))
                {
                    response = this.ResolveOperatingUnitNumber((ResolveOperatingUnitNumberDataRequest)request);
                }
                else if (requestType == typeof(UpdateChannelPropertiesByChannelIdDataRequest))
                {
                    response = this.UpdateChannelPropertiesByChannelId((UpdateChannelPropertiesByChannelIdDataRequest)request);
                }
                else if (requestType == typeof(UpdateOnlineChannelPublishStatusDataRequest))
                {
                    response = this.UpdatePublishStatus((UpdateOnlineChannelPublishStatusDataRequest)request);
                }
                else if (requestType == typeof(GetDownloadingDataSetDataRequest))
                {
                    response = this.GetDownloadingDataSet((GetDownloadingDataSetDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
            
            /// <summary>
            /// Gets a downloading dataset of a given data group.
            /// </summary>
            /// <param name="request">The get downloading data set data request.</param>
            /// <returns>A dataset.</returns>
            public SingleEntityDataServiceResponse<DataSet> GetDownloadingDataSet(GetDownloadingDataSetDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.DataGroupName, "request.DataGroupName");
    
                Stopwatch processTimer = Stopwatch.StartNew();
    
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                parameters["@vc_DataGroupName"] = request.DataGroupName;
    
                DataSet result = null;
    
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    result = sqlServerDatabaseContext.ExecuteStoredProcedureDataSet(GetDownloadingDataSprocName, parameters);
                }
    
                processTimer.Stop();
                NetTracer.Information("** timer info: GetDownloadingDataSet completed in {0} ms", processTimer.ElapsedMilliseconds);
    
                return new SingleEntityDataServiceResponse<DataSet>(result);
            }

            /// <summary>
            /// The data service method to execute the data manager to get the device configuration.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<DeviceConfiguration> GetDeviceConfiguration(GetDeviceConfigurationDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                ICommercePrincipal principal = request.RequestContext.GetPrincipal();
                if (principal.IsChannelAgnostic || principal.IsTerminalAgnostic || string.IsNullOrWhiteSpace(principal.DeviceNumber))
                {
                    throw new InvalidOperationException("Current request context is not associated to a device.");
                }

                Terminal terminal = request.RequestContext.GetTerminal();

                ParameterSet parameters = new ParameterSet();
                Tuple<PagedResult<DeviceConfiguration>,
                        ReadOnlyCollection<HardwareConfiguration>,
                        ReadOnlyCollection<HardwareConfiguration>,
                        ReadOnlyCollection<HardwareConfiguration>> dataSets = null;

                parameters[DatabaseAccessor.ChannelIdVariableName] = terminal.ChannelId;
                parameters[TerminalIdVariableName] = terminal.TerminalId;
                parameters[IncludeImagesVariableName] = request.IncludeImages;

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext, request.QueryResultSettings))
                {
                    dataSets = databaseContext.ExecuteStoredProcedure<DeviceConfiguration, HardwareConfiguration, HardwareConfiguration, HardwareConfiguration>(GetDeviceConfigurationSprocName, parameters);
                }

                DeviceConfiguration deviceConfiguration = dataSets.Item1.SingleOrDefault();
                ReadOnlyCollection<HardwareConfiguration> drawers = dataSets.Item2;
                ReadOnlyCollection<HardwareConfiguration> printers = dataSets.Item3;
                ReadOnlyCollection<HardwareConfiguration> pinpads = dataSets.Item4;

                if (deviceConfiguration != null)
                {
                    deviceConfiguration.HardwareConfigurations = new HardwareConfigurations();
                    deviceConfiguration.HardwareConfigurations.CashDrawerConfigurations.AddRange(drawers);
                    deviceConfiguration.HardwareConfigurations.PrinterConfigurations.AddRange(printers);
                    deviceConfiguration.HardwareConfigurations.PinPadConfiguration = pinpads.SingleOrDefault();
                }

                GetDeviceDataRequest getDeviceRequest = new GetDeviceDataRequest(principal.DeviceNumber);
                Device device = request.RequestContext.Execute<SingleEntityDataServiceResponse<Device>>(getDeviceRequest).Entity;

                if (deviceConfiguration != null && device != null)
                {
                    deviceConfiguration.UseInMemoryDeviceDataStorage = device.UseInMemoryDeviceDataStorage;
                }

                return new SingleEntityDataServiceResponse<DeviceConfiguration>(deviceConfiguration);
            }

            /// <summary>
            /// Updates the publishing status of the channel.
            /// </summary>
            /// <param name="request">The update online channel publish status data request.</param>
            /// <returns>Result that indicates whether the update is successful.</returns>
            private SingleEntityDataServiceResponse<bool> UpdatePublishStatus(UpdateOnlineChannelPublishStatusDataRequest request)
            {
                ThrowIf.Null(request, "request");
    
                Stopwatch processTimer = Stopwatch.StartNew();
    
                ParameterSet parameters = new ParameterSet();
    
                parameters["@bi_ChannelId"] = request.ChannelId;
                parameters["@i_PublishStatus"] = request.PublishStatus;
                parameters["@nvc_PublishStatusMessage"] = request.PublishStatusMessage;
    
                int errorCode;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    errorCode = sqlServerDatabaseContext.ExecuteStoredProcedureNonQuery(UpdateOnlineChannelPublishStatusSprocName, parameters);
                }
    
                processTimer.Stop();
                NetTracer.Information("** timer info: UpdatePublishStatus completed in {0} ms", processTimer.ElapsedMilliseconds);
    
                return new SingleEntityDataServiceResponse<bool>(errorCode == (int)DatabaseErrorCodes.Success);
            }
    
            /// <summary>
            /// Sets the channel-specific properties of the specified channel.
            /// </summary>
            /// <param name="request">The update channel properties by channel id data request.</param>
            /// <returns>A null response.</returns>
            private NullResponse UpdateChannelPropertiesByChannelId(UpdateChannelPropertiesByChannelIdDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ChannelProperties, "request.ChannelProperties");
    
                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);
    
                bool updateL2Cache = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss
                                     && request.ChannelProperties.Count() < MaxCachedCollectionSize;
    
                Stopwatch processTimer = Stopwatch.StartNew();
    
                foreach (ChannelProperty channelProperty in request.ChannelProperties)
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters["@bi_ChannelId"] = request.ChannelId;
                    parameters["@nvc_PropertyName"] = channelProperty.Name;
                    parameters["@nvc_PropertyValue"] = channelProperty.Value;
    
                    int errorCode;
    
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        errorCode = sqlServerDatabaseContext.ExecuteStoredProcedureNonQuery(SaveChannelPropertySprocName, parameters);
                    }
    
                    if (errorCode != (int)DatabaseErrorCodes.Success)
                    {
                        throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to update channel properties.");
                    }
                }
    
                processTimer.Stop();
                NetTracer.Information("** timer info: UpdateChannelPropertiesByChannelId completed in {0} ms", processTimer.ElapsedMilliseconds);
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChannelPropertiesByChannelId(request.ChannelId, QueryResultSettings.AllRecords, request.ChannelProperties.AsPagedResult());
                }
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Resolves operating unit number.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The channel Id corresponding to the Operating Unit Number.</returns>
            private SingleEntityDataServiceResponse<long> ResolveOperatingUnitNumber(ResolveOperatingUnitNumberDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);
                bool foundInCache;
                bool updateCache;
    
                const long InvalidChannelId = 0;
                long channelId = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChannelIdByOperatingUnitNumber(request.OperatingUnitNumber), InvalidChannelId, out foundInCache, out updateCache);
    
                if (!foundInCache)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        Select = new ColumnSet(ChannelIdColumnName),
                        From = StorageLookupView,
                        Where = "ISPUBLISHED = 1 AND OPERATINGUNITNUMBER = @oun",
                    };
    
                    query.Parameters["@oun"] = request.OperatingUnitNumber;
    
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        channelId = sqlServerDatabaseContext.ExecuteScalar<long>(query);
                    }
                }
    
                if (updateCache)
                {
                    level2CacheDataAccessor.PutChannelIdByOperatingUnitNumber(request.OperatingUnitNumber, channelId);
                }
    
                return new SingleEntityDataServiceResponse<long>(channelId);
            }
    
            private EntityDataServiceResponse<ChannelCategoryAttribute> GetChannelCategoryAttributes(GetChannelCategoryAttributesDataRequest request)
            {
                PagedResult<ChannelCategoryAttribute> channelCategoryAttributes;
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = ChannelCategoryAttributeViewName,
                    Where = "HOSTCHANNEL = " + SqlParamChannelId,
                    OrderBy = "HOSTCHANNEL, CATEGORY, KEYNAME"
                };
    
                using (RecordIdTableType categoryRecordIds = new RecordIdTableType(request.CategoryIds, "CATEGORY"))
                using (SqlServerDatabaseContext context = new SqlServerDatabaseContext(request))
                {
                    query.Parameters[SqlParamChannelId] = request.ChannelId;
                    query.Parameters["@TVP_RECIDTABLETYPE"] = categoryRecordIds;
    
                    channelCategoryAttributes = context.ReadEntity<ChannelCategoryAttribute>(query);
                }
    
                return new EntityDataServiceResponse<ChannelCategoryAttribute>(channelCategoryAttributes);
            }
    
            /// <summary>
            /// Gets the cache accessor for the channel data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="ChannelL2CacheDataStoreAccessor"/> class.</returns>
            private ChannelL2CacheDataStoreAccessor GetChannelL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new ChannelL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
        }
    }
}
