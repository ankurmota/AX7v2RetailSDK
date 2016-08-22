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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Employee data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class EmployeeDataService : IRequestHandler
        {
            private const string StaffCredentialsView = "STAFFCREDENTIALSVIEW";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(EntityDataServiceRequest<Employee>),
                    typeof(GetEmployeeDataRequest),
                    typeof(GetEmployeeStoresFromAddressBookDataRequest),
                    typeof(GetEmployeeBreakCategoriesByJobDataRequest),
                    typeof(GetEmployeePermissionsDataRequest),
                    typeof(EmployeeLogOnStoreDataRequest),
                    typeof(GetEmployeePasswordCryptoInfoDataRequest),
                    typeof(GetOperationPermissionsDataRequest),
                    typeof(GetEmployeeBreakCategoriesByActivityDataRequest),
                    typeof(ValidateEmployeePasswordDataRequest),
                    typeof(GetEmployeeAuthorizedOnStoreDataRequest),
                    typeof(CheckEmployeeHasOpenSessionDataRequest),
                    typeof(GetUserCredentialsDataRequest),
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

                if (requestType == typeof(EntityDataServiceRequest<Employee>))
                {
                    response = this.GetAllStoreEmployees((EntityDataServiceRequest<Employee>)request);
                }
                else if (requestType == typeof(CheckEmployeeHasOpenSessionDataRequest))
                {
                    response = this.CheckEmployeeHasOpenSessionOnCurrentTerminal(request.RequestContext);
                }
                else if (requestType == typeof(GetEmployeeAuthorizedOnStoreDataRequest))
                {
                    response = this.GetAuthorizedEmployee((GetEmployeeAuthorizedOnStoreDataRequest)request);
                }
                else if (requestType == typeof(GetEmployeeDataRequest))
                {
                    response = this.GetEmployee((GetEmployeeDataRequest)request);
                }
                else if (requestType == typeof(GetEmployeeStoresFromAddressBookDataRequest))
                {
                    response = this.GetEmployeeStoresFromAddressBook((GetEmployeeStoresFromAddressBookDataRequest)request);
                }
                else if (requestType == typeof(GetEmployeeBreakCategoriesByJobDataRequest))
                {
                    response = this.GetEmployeeBreakCategoriesByJob((GetEmployeeBreakCategoriesByJobDataRequest)request);
                }
                else if (requestType == typeof(GetEmployeePermissionsDataRequest))
                {
                    response = this.GetEmployeePermissions((GetEmployeePermissionsDataRequest)request);
                }
                else if (requestType == typeof(EmployeeLogOnStoreDataRequest))
                {
                    response = this.EmployeeLogOnStore((EmployeeLogOnStoreDataRequest)request);
                }
                else if (requestType == typeof(GetEmployeePasswordCryptoInfoDataRequest))
                {
                    response = this.GetEmployeePasswordCryptoInfo((GetEmployeePasswordCryptoInfoDataRequest)request);
                }
                else if (requestType == typeof(GetOperationPermissionsDataRequest))
                {
                    response = this.GetOperationPermissions((GetOperationPermissionsDataRequest)request);
                }
                else if (requestType == typeof(GetEmployeeBreakCategoriesByActivityDataRequest))
                {
                    response = this.GetEmployeeBreakCategoriesByActivity((GetEmployeeBreakCategoriesByActivityDataRequest)request);
                }
                else if (requestType == typeof(ValidateEmployeePasswordDataRequest))
                {
                    response = this.ValidateEmployeePassword((ValidateEmployeePasswordDataRequest)request);
                }
                else if (requestType == typeof(GetUserCredentialsDataRequest))
                {
                    response = this.GetUserCredential((GetUserCredentialsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Looks up for the matching user credential against the database.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<UserCredential> GetUserCredential(GetUserCredentialsDataRequest request)
            {
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        From = StaffCredentialsView,
                        Where = "CREDENTIALID = @CREDENTIALID AND GRANTTYPE = @GRANTTYPE",
                    };
                    
                    query.Parameters["@CREDENTIALID"] = request.CredentialId;
                    query.Parameters["@GRANTTYPE"] = request.GrantType;

                    return new SingleEntityDataServiceResponse<UserCredential>(databaseContext.ReadEntity<UserCredential>(query).Results.FirstOrDefault());
                }
            }

            /// <summary>
            /// Checks whether the current employee has an open session on the current terminal.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<bool> CheckEmployeeHasOpenSessionOnCurrentTerminal(RequestContext requestContext)
            {
                const string EmployeeSessionsViewName = "EmployeeSessionsView";
                string terminalId = requestContext.GetTerminal().TerminalId;
                string staffId = requestContext.GetPrincipal().UserId;

                bool employeeHasOpenSessionOnCurrentTerminal;

                DataStoreManager.InstantiateDataStoreManager(requestContext);
                EmployeeL2CacheDataStoreAccessor accessor = new EmployeeL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], requestContext);

                if (!accessor.CheckEmployeeSessionOpenOnTerminal(terminalId, staffId, out employeeHasOpenSessionOnCurrentTerminal))
                {
                    using (DatabaseContext databaseContext = new DatabaseContext(requestContext))
                    {
                        SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                        {
                            Select = new ColumnSet("STAFFID"),
                            From = EmployeeSessionsViewName,
                            Where = "DATAAREAID = @DATAAREAID AND STAFFID = @STAFFID AND TERMINALID = @TERMINALID AND CHANNELID = @CHANNELID",
                        };

                        query.Parameters["@DATAAREAID"] = requestContext.GetChannelConfiguration().InventLocationDataAreaId;
                        query.Parameters["@STAFFID"] = staffId;
                        query.Parameters["@TERMINALID"] = terminalId;
                        query.Parameters["@CHANNELID"] = requestContext.GetPrincipal().ChannelId;

                        employeeHasOpenSessionOnCurrentTerminal = databaseContext.ExecuteScalarCollection<string>(query).Any();
                    }

                    accessor.CacheIsEmployeeSessionOpenOnTerminal(terminalId, staffId, employeeHasOpenSessionOnCurrentTerminal);
                }

                return new SingleEntityDataServiceResponse<bool>(employeeHasOpenSessionOnCurrentTerminal);
            }

            /// <summary>
            /// Gets employee authorized to current store.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<Employee> GetAuthorizedEmployee(GetEmployeeAuthorizedOnStoreDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                Employee employee = dataManager.GetAuthorizedEmployeeOnStore(request.RequestContext.GetPrincipal().ChannelId, request.StaffId);
                return new SingleEntityDataServiceResponse<Employee>(employee);
            }

            /// <summary>
            /// Gets Operation Permissions for the operation.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<OperationPermission> GetOperationPermissions(GetOperationPermissionsDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                var result = dataManager.GetOperationPermissions(request.OperationId, request.QueryResultSettings);
                return new EntityDataServiceResponse<OperationPermission>(result);
            }

            /// <summary>
            /// Gets the salt for hashing password.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The salt and the algorithm to hash password.</returns>
            private SingleEntityDataServiceResponse<EmployeePasswordCryptoInfo> GetEmployeePasswordCryptoInfo(GetEmployeePasswordCryptoInfoDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                var passwordCryptoInfo = dataManager.GetEmployeePasswordCryptoInfo(request.StaffId, request.ChannelId);
                return new SingleEntityDataServiceResponse<EmployeePasswordCryptoInfo>(passwordCryptoInfo);
            }

            /// <summary>
            /// Logs On the user in the local store database.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<Employee> EmployeeLogOnStore(EmployeeLogOnStoreDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                var result = dataManager.EmployeeLogOnStore(request.ChannelId, request.StaffId, request.PasswordHash, request.ColumnSet);

                // Clear the cache for the staff at logon.
                DataStoreManager.InstantiateDataStoreManager(request.RequestContext);
                EmployeeL2CacheDataStoreAccessor accessor = new EmployeeL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], request.RequestContext);
                accessor.ClearCacheAuthorizedEmployeeOnStore(request.ChannelId, request.StaffId);

                return new SingleEntityDataServiceResponse<Employee>(result);
            }

            /// <summary>
            /// Gets the employee permission group for the staff identifier.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<EmployeePermissions> GetEmployeePermissions(GetEmployeePermissionsDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                var result = dataManager.GetEmployeePermissions(request.StaffId, request.ColumnSet);
                return new SingleEntityDataServiceResponse<EmployeePermissions>(result);
            }

            /// <summary>
            /// Gets the employee break categories by job identifier.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<EmployeeActivity> GetEmployeeBreakCategoriesByJob(GetEmployeeBreakCategoriesByJobDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                var result = dataManager.GetEmployeeBreakCategoriesByJob(request.JobId);
                return new EntityDataServiceResponse<EmployeeActivity>(result);
            }

            /// <summary>
            /// Gets the employee stores from address book.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<OrgUnit> GetEmployeeStoresFromAddressBook(GetEmployeeStoresFromAddressBookDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                var result = dataManager.GetEmployeeStoresFromAddressBook(request.RequestContext.GetPrincipal().UserId, request.QueryResultSettings);
                return new EntityDataServiceResponse<OrgUnit>(result);
            }

            /// <summary>
            /// Gets the employee.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<Employee> GetEmployee(GetEmployeeDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                Employee result = dataManager.GetEmployee(request.StaffId, request.QueryResultSettings);
                return new SingleEntityDataServiceResponse<Employee>(result);
            }

            /// <summary>
            /// The data service method to execute the data manager to get all employees.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<Employee> GetAllStoreEmployees(EntityDataServiceRequest<Employee> request)
            {
                var employeeDataManager = this.GetDataManagerInstance(request.RequestContext);
                var employees = employeeDataManager.GetAllStoreEmployees(request.QueryResultSettings);
                return new EntityDataServiceResponse<Employee>(employees);
            }

            /// <summary>
            /// Gets the employee break categories by activity names.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<EmployeeActivity> GetEmployeeBreakCategoriesByActivity(GetEmployeeBreakCategoriesByActivityDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                var result = dataManager.GetEmployeeBreakCategoriesByActivity(request.ActivityNames);
                return new EntityDataServiceResponse<EmployeeActivity>(result);
            }

            /// <summary>
            /// Validates the employee password.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<bool> ValidateEmployeePassword(ValidateEmployeePasswordDataRequest request)
            {
                EmployeeDataManager dataManager = this.GetDataManagerInstance(request.RequestContext);
                bool result = dataManager.ValidateEmployeePassword(request.ChannelId, request.StaffId, request.PasswordHash, request.QueryResultSettings.ColumnSet);
                return new SingleEntityDataServiceResponse<bool>(result);
            }

            /// <summary>
            /// Gets the data manager instance.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of <see cref="EmployeeDataManager"/></returns>
            private EmployeeDataManager GetDataManagerInstance(RequestContext context)
            {
                return new EmployeeDataManager(context);
            }
        }
    }
}