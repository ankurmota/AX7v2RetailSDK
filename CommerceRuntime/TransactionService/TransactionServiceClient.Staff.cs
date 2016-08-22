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
        using System.IO;
        using System.Linq;
        using System.Runtime.Serialization;
        using System.ServiceModel;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string StaffLogonMethodName = "StaffLogOn";
            private const string StaffLogOffMethodName = "StaffLogOff";
            private const string StaffResetPasswordMethodName = "UpdateStaffPassword";
            private const string StaffChangePasswordMethodName = "StaffChangePassword";
            private const string StaffIsPasswordValid = "StaffIsPasswordValid";
            private const string RetailServerStaffIsPasswordValidForStaffMethodName = "RetailServerStaffIsPasswordValidForStaff";
            private const string RetailServerStaffLogOnMethodName = "RetailServerStaffLogOn";
            private const string RetailServerStaffLogOnRenewalMethodName = "RetailServerStaffInfo";
            private const string GetRetailServerStaffByExternalIdentityMethodName = "RetailServerStaffInfoByExternalIdentity";
            private const string RetailServerStaffLogOffMethodName = "RetailServerStaffLogOff";
            private const string GetEmployeeStoresFromAddressBookMethodName = "GetEmployeeStoresFromAddressBook";
            private const string EnrollUserCredentialsMethodName = "enrollUserCredentials";
            private const string UnenrollUserCredentialsMethodName = "unenrollUserCredentials";
            private const string GetUserCredentialsMethodName = "getUserCredential";

            // Employee object constants
            private const int StaffIdIndex = 0;
            private const int NameOnReceiptIndex = 1;
            private const int ImageIndex = 2;
            private const int NameIndex = 3;
            private const int MaximumDiscountPercentageIndex = 4;
            private const int MaximumLineDiscountAmountIndex = 5;
            private const int MaximumLineReturnAmountIndex = 6;
            private const int MaximumTotalDiscountAmountIndex = 7;
            private const int MaximumTotalDiscountPercentageIndex = 8;
            private const int MaximumTotalReturnAmountIndex = 9;
            private const int AllowBlindCloseIndex = 10;
            private const int AllowChangeNoVoidIndex = 11;
            private const int AllowCreateOrderIndex = 12;
            private const int AllowEditOrderIndex = 13;
            private const int AllowFloatingTenderDeclarationIndex = 14;
            private const int AllowMultipleLoginsIndex = 15;
            private const int AllowMUltipleShiftLogOnIndex = 16;
            private const int AllowOpenDrawerIndex = 17;
            private const int AllowPriceOverrideIndex = 18;
            private const int AllowRetrieveOrderIndex = 19;
            private const int AllowSalesTaxChangeIndex = 20;
            private const int AllowTenderDeclarationIndex = 21;
            private const int AllowTransactionSuspensionIndex = 22;
            private const int AllowTransactionVoidingIndex = 23;
            private const int AllowXReportPrintingIndex = 24;
            private const int AllowZReportPrintingIndex = 25;
            private const int AllowKitDisassemblyIndex = 26;
            private const int AllowChangePeripheralStationIndex = 27;
            private const int ManageDeviceIndex = 28;
            private const int AllowManagerPrivilegesIndex = 29;
            private const int AllowPasswordChangeIndex = 30;
            private const int AllowResetPasswordIndex = 31;
            private const int ContinueOnTSErrorsIndex = 32;
            private const int CultureNameIndex = 33;
            private const int ChangePasswordIndex = 34;
            private const int PasswordLastChangedDateTimeIndex = 35;
            private const int AllowUseSharedShiftIndex = 36;
            private const int AllowManageSharedShiftIndex = 37;
            private const int EmployeeObjectSize = 38;
            private const int ChangePasswordObjectSize = 5;
            private const int PasswordComplexityValidationFailed = 3;
            private const int PasswordHistoryValidationFailed = 4;

            /// <summary>
            /// Logs the specified user on.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="storeId">The store identifier.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="password">The password.</param>
            public void StaffLogOn(string staffId, string storeId, string terminalId, string password)
            {
                this.InvokeMethodNoDataReturn(
                    StaffLogonMethodName,
                    new object[] { staffId, storeId, terminalId, password });
            }

            /// <summary>
            /// Logs the specified user off.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="storeId">The store identifier.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            public void StaffLogOff(string staffId, string storeId, string terminalId)
            {
                this.InvokeMethodNoDataReturn(
                    StaffLogOffMethodName,
                    new object[] { staffId, storeId, terminalId });
            }

            /// <summary>
            /// Reset password for a specified user.
            /// </summary>
            /// <param name="targetUserId">The target user id.</param>
            /// <param name="newPassword">The new password.</param>
            /// <param name="changePassword">The change password parameter.</param>
            /// <param name="newPasswordHash">The new password hash as output parameter.</param>
            /// <param name="newPasswordSalt">The new password salt as output parameter.</param>
            /// <param name="newPasswordHashAlgorithm">The new password hash algorithm as output parameter.</param>
            /// <param name="newPasswordLastChangedDateTime">The new UTC date and time at which the password was changed.</param>
            /// <param name="passwordLastUpdatedOperation">The authentication operation for the last password update.</param>
            public void StaffResetPassword(
                string targetUserId,
                string newPassword,
                bool changePassword,
                out string newPasswordHash,
                out string newPasswordSalt,
                out string newPasswordHashAlgorithm,
                out DateTimeOffset newPasswordLastChangedDateTime,
                out AuthenticationOperation passwordLastUpdatedOperation)
            {
                const int StaffNotFoundErrorCode = 1;
                const int RecordSavingFailedErrorCode = 2;
                ReadOnlyCollection<object> results = null;
                try
                {
                    results = this.InvokeMethod(StaffResetPasswordMethodName, new object[] { targetUserId, newPassword, changePassword ? 1 : 0 });
                    GetChangePasswordResult(results, out newPasswordHash, out newPasswordSalt, out newPasswordHashAlgorithm, out newPasswordLastChangedDateTime, out passwordLastUpdatedOperation);
                }
                catch (HeadquarterTransactionServiceException exception)
                {
                    int? errorCode = (int?)exception.HeadquartersErrorData.FirstOrDefault();

                    switch (errorCode)
                    {
                        case StaffNotFoundErrorCode:
                        case RecordSavingFailedErrorCode:
                            throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_ResetPasswordFailed, exception, "Resetting the password failed.");
                        case PasswordComplexityValidationFailed:
                            throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordComplexityRequirementsNotMet, exception, "The password complexity was not met.");
                        case PasswordHistoryValidationFailed:
                            throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordHistoryRequirementsNotMet, exception, "The password history requirements were not met.");
                        default:
                            throw;
                    }
                }
            }

            /// <summary>
            /// Change password for a user.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="oldPasswordHash">The old password hash.</param>
            /// <param name="newPassword">The new password.</param>
            /// <param name="changePassword">The change password parameter.</param>
            /// <param name="newPasswordHash">The new password hash as output parameter.</param>
            /// <param name="newPasswordSalt">The new password salt as output parameter.</param>
            /// <param name="newPasswordHashAlgorithm">The new password hash algorithm as output parameter.</param>
            /// <param name="newPasswordLastChangedDateTime">The UTC date and time on which the password was changed.</param>
            /// <param name="newPasswordLastUpdatedOperation">The authentication operation for the last password update.</param>
            public void StaffChangePassword(
                string staffId,
                string oldPasswordHash,
                string newPassword,
                bool changePassword,
                out string newPasswordHash,
                out string newPasswordSalt,
                out string newPasswordHashAlgorithm,
                out DateTimeOffset newPasswordLastChangedDateTime,
                out AuthenticationOperation newPasswordLastUpdatedOperation)
            {
                const int StaffNotFoundErrorCode = 1;
                const int IncorrectPasswordErrorCode = 2;
                
                ReadOnlyCollection<object> results;

                try
                {
                    results = this.InvokeMethod(StaffChangePasswordMethodName, new object[] { staffId, oldPasswordHash, newPassword, changePassword });
                    GetChangePasswordResult(results, out newPasswordHash, out newPasswordSalt, out newPasswordHashAlgorithm, out newPasswordLastChangedDateTime, out newPasswordLastUpdatedOperation);
                }
                catch (HeadquarterTransactionServiceException exception)
                {
                    int? errorCode = (int?)exception.HeadquartersErrorData.FirstOrDefault();

                    switch (errorCode)
                    {
                        case StaffNotFoundErrorCode:
                        case IncorrectPasswordErrorCode:
                            throw new UserAuthenticationException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword,
                                exception,
                                "User name or password mismatch.");
                        case PasswordComplexityValidationFailed:
                            throw new SecurityException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordComplexityRequirementsNotMet,
                                exception,
                                "The password complexity was not met.");
                        case PasswordHistoryValidationFailed:
                            throw new SecurityException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordHistoryRequirementsNotMet,
                                exception,
                                "The password history requirements were not met.");
                        default:
                            throw;
                    }
                }
            }

            /// <summary>
            /// Logs the specified user on.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="storeRecordId">The store record identifier.</param>
            /// <param name="terminalRecordId">The terminal record identifier.</param>
            /// <param name="password">The password.</param>
            /// <returns>The employee information.</returns>
            public Employee RetailServerStaffLogOn(string staffId, long storeRecordId, long terminalRecordId, string password)
            {
                return this.RetailServerStaffLogOn(staffId, storeRecordId, terminalRecordId, password, logOntoStore: true, skipPasswordVerification: false);
            }

            /// <summary>
            /// Logs the specified user on.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="storeRecordId">The store record identifier.</param>
            /// <param name="terminalRecordId">The terminal record identifier.</param>
            /// <param name="password">The password.</param>
            /// <param name="logOntoStore"> Specifies to login to store.</param>
            /// <param name="skipPasswordVerification">A value indicating whether password verification must not be performed.</param>
            /// <returns>The employee information.</returns>
            public Employee RetailServerStaffLogOn(string staffId, long storeRecordId, long terminalRecordId, string password, bool logOntoStore, bool skipPasswordVerification)
            {
                const int UserIsBlocked = 1;
                const int UserPasswordNotConfiguredError = 2;
                const int UserPasswordInvalid = 3;
                const int UserCannotUseMultipleTerminalsAtSameTime = 4;

                ReadOnlyCollection<object> logonData;

                var parameters = new object[]
                {
                staffId,
                storeRecordId,
                terminalRecordId,
                password,
                logOntoStore,
                skipPasswordVerification
                };

                try
                {
                    logonData = this.InvokeMethod(RetailServerStaffLogOnMethodName, parameters);
                }
                catch (HeadquarterTransactionServiceException exception)
                {
                    int? errorCode = (int?)exception.HeadquartersErrorData.FirstOrDefault();
                    switch (errorCode)
                    {
                        case UserIsBlocked:
                            RetailLogger.Log.CrtServicesStaffAuthorizationServiceBlockedUserAccessAttempt(staffId);
                            throw new UserAuthorizationException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed,
                                exception,
                                "User is not authorized.");

                        case UserPasswordNotConfiguredError:
                            RetailLogger.Log.CrtServicesStaffAuthorizationServiceUserPasswordNotConfigured(staffId);
                            throw new UserAuthorizationException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, 
                                exception,
                                "User is not authorized.");

                        case UserPasswordInvalid:
                            RetailLogger.Log.CrtServicesStaffAuthenticationServiceInvalidPassword(staffId);
                            throw new UserAuthenticationException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword,
                                exception,
                                "User entered invalid password.");

                        case UserCannotUseMultipleTerminalsAtSameTime:
                            RetailLogger.Log.CrtServicesStaffAuthenticationServiceInvalidPassword(staffId);
                            throw new UserAuthorizationException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UserLogonAnotherTerminal,
                                "User is already authorized against another terminal and cannot be authorized against multiple terminals at once.");

                        default:
                            // an error without erroCode is not expected
                            // since all authentication failures are captured above, map this to authorization
                            RetailLogger.Log.CrtServicesRealTimeUnexpectedErrorCode(RetailServerStaffLogOnMethodName, errorCode, exception);
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, exception, "User is not authorized.");
                    }
                }

                if (logonData == null || logonData.Count < EmployeeObjectSize)
                {
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                return GetEmployee(logonData);
            }

            /// <summary>
            /// Logs the specified user off.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="storeRecordId">The store record identifier.</param>
            /// <param name="terminalRecordId">The terminal record identifier.</param>
            public void RetailServerStaffLogOff(string staffId, long storeRecordId, long terminalRecordId)
            {
                this.InvokeMethodNoDataReturn(
                    RetailServerStaffLogOffMethodName,
                    new object[] { staffId, storeRecordId, terminalRecordId });
            }

            /// <summary>
            /// Logs the specified user off.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="storeRecordId">The store record identifier.</param>
            /// <param name="terminalRecordId">The terminal record identifier.</param>
            /// <param name="logOffFromStore">Specifies to logoff from a store.</param>
            public void RetailServerStaffLogOff(string staffId, long storeRecordId, long terminalRecordId, bool logOffFromStore)
            {
                this.InvokeMethodNoDataReturn(
                    RetailServerStaffLogOffMethodName,
                    new object[] { staffId, storeRecordId, terminalRecordId, logOffFromStore });
            }

            /// <summary>
            /// Logs the specified user on.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <returns>The employee information.</returns>
            public Employee RetailServerStaffLogOnRenewal(string staffId)
            {
                ReadOnlyCollection<object> logonData = this.InvokeMethod(
                    RetailServerStaffLogOnRenewalMethodName,
                    new object[] { staffId });

                if (logonData == null || logonData.Count < EmployeeObjectSize)
                {
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                return GetEmployee(logonData);
            }

            /// <summary>
            /// Gets the retail server staff given an  external identity.
            /// </summary>
            /// <param name="externalIdentityId">The external identity identifier.</param>
            /// <param name="externalIdentitySubId">The external identity sub identifier.</param>
            /// <returns>The employee information.</returns>
            public Employee GetRetailServerStaffByExternalIdentity(string externalIdentityId, string externalIdentitySubId)
            {
                ReadOnlyCollection<object> staffData = this.InvokeMethod(
                                                        GetRetailServerStaffByExternalIdentityMethodName,
                                                        new object[] { externalIdentityId, externalIdentitySubId });
                if (staffData == null || staffData.Count < EmployeeObjectSize)
                {
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                return GetEmployee(staffData);
            }

            /// <summary>
            /// Validates if the password is correct or not.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="password">The password.</param>
            public void RetailServerStaffIsPasswordValidForStaff(string staffId, string password)
            {
                this.InvokeMethodNoDataReturn(RetailServerStaffIsPasswordValidForStaffMethodName, new object[] { staffId, password });
            }

            /// <summary>
            /// Gets the list of stores accessible by this employee from address book.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>The paged results of stores accessible by this employee.</returns>
            public PagedResult<OrgUnit> GetEmployeeStoresFromAddressBook(string staffId, QueryResultSettings settings)
            {
                ThrowIf.Null(settings, "settings");
                ThrowIf.Null(settings.Paging, "settings.Paging");

                var orgUnits = new List<OrgUnit>();
                var data = this.InvokeMethodAllowNullResponse(
                    GetEmployeeStoresFromAddressBookMethodName,
                    new object[] { staffId, settings.Paging.NumberOfRecordsToFetch, settings.Paging.Skip });

                if (data != null)
                {
                    // Parse response data
                    foreach (var orgUnitDataRow in data)
                    {
                        var orgUnitData = (object[])orgUnitDataRow;
                        var orgUnit = new OrgUnit()
                        {
                            OrgUnitNumber = (string)orgUnitData[0],
                            OrgUnitName = (string)orgUnitData[1],
                            OrgUnitFullAddress = (string)orgUnitData[2],
                            OMOperatingUnitNumber = (string)orgUnitData[3]
                        };

                        orgUnits.Add(orgUnit);
                    }
                }

                return new PagedResult<OrgUnit>(orgUnits.AsReadOnly(), settings.Paging);
            }

            /// <summary>
            /// Enrolls user credentials with the head quarters.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credentialId">The credential identifier.</param>
            /// <param name="credential">The credential.</param>
            /// <param name="additionalAuthenticationData">Any additional information stored for authentication.</param>
            /// <returns>The <see cref="UserCredential"/> created or updated as part of enrollment.</returns>
            public UserCredential EnrollUserCredentials(string staffId, string grantType, string credentialId, string credential, string additionalAuthenticationData)
            {
                const int ExpectedDataElementCount = 4;
                const int HashedCredentialIndex = 0;
                const int SaltIndex = 1;
                const int HashAlgorithmIndex = 2;
                const int RecordIdIndex = 3;

                const int StaffNotFoundErrorId = 0;
                const int CredentialIdAlreadyInUseErrorId = 1;

                ReadOnlyCollection<object> data;

                try
                {
                    data = this.InvokeMethod(
                        EnrollUserCredentialsMethodName,
                        new object[] { staffId, grantType, credentialId, credential, additionalAuthenticationData });
                }
                catch (HeadquarterTransactionServiceException exception)
                {
                    int? errorCode = (int?)exception.HeadquartersErrorData.FirstOrDefault();
                    switch (errorCode)
                    {
                        case StaffNotFoundErrorId:
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeDetailsNotFound,
                                string.Format("Employee '{0}' not found.", staffId));

                        case CredentialIdAlreadyInUseErrorId:
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CredentialIdentifierAlreadyInUse, "The credential id is already in use.");

                        default:
                            throw;
                    }
                }

                if (data == null || data.Count < ExpectedDataElementCount)
                {
                    RetailLogger.Log.CrtServicesRealTimeCouldNotParseRTSResponse(EnrollUserCredentialsMethodName, ExpectedDataElementCount, data.Count, string.Empty);
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                return new UserCredential()
                {
                    HashedCredential = (string)data[HashedCredentialIndex],
                    Salt = (string)data[SaltIndex],
                    HashAlgorithm = (string)data[HashAlgorithmIndex],
                    RecId = (long)data[RecordIdIndex],
                    StaffId = staffId,
                    GrantType = grantType,
                    CredentialId = credentialId,
                    AdditionalAuthenticationData = additionalAuthenticationData,
                };
            }

            /// <summary>
            /// Removes credentials associated with staff and grant type.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="grantType">The grant type.</param>
            public void UnenrollUserCredentials(string staffId, string grantType)
            {
                const int StaffNotFoundErrorId = 0;

                try
                {
                    this.InvokeMethodNoDataReturn(UnenrollUserCredentialsMethodName, new object[] { staffId, grantType });
                }
                catch (HeadquarterTransactionServiceException exception)
                {
                    int? errorCode = (int?)exception.HeadquartersErrorData.FirstOrDefault();
                    switch (errorCode)
                    {
                        case StaffNotFoundErrorId:
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeDetailsNotFound, 
                                string.Format("Employee '{0}' not found.", staffId));

                        default:
                            throw;
                    }
                }                
            }

            /// <summary>
            /// Gets the user credential associated to the provided <paramref name="credentialId"/> and <paramref name="grantType"/>.
            /// </summary>
            /// <param name="credentialId">The credential identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <returns>The <see cref="UserCredential"/> associated with <paramref name="credentialId"/> and <paramref name="grantType"/>.</returns>
            public UserCredential GetUserCredentials(string credentialId, string grantType)
            {
                var data = this.InvokeMethod(
                    GetUserCredentialsMethodName,
                    new object[] { credentialId, grantType });

                try
                {
                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes((string)data[0])))
                    {
                        return (UserCredential)new DataContractSerializer(typeof(UserCredential), "RetailStaffCredentialTable", string.Empty).ReadObject(stream);
                    }
                }
                catch (Exception exception)
                {
                    RetailLogger.Log.CrtServicesRealTimeCouldNotParseRTSResponse(GetUserCredentialsMethodName, 1, data.Count, exception.Message);

                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        exception,
                        string.Format("Could not parse result for RTS API: {0}", GetUserCredentialsMethodName));
                }
            }

            /// <summary>
            /// Gets the new password hash and salt from the result object.
            /// </summary>
            /// <param name="changePasswordResult">The result for changing the password.</param>
            /// <param name="newPasswordHash">The new password hash as output parameter.</param>
            /// <param name="newPasswordSalt">The new password salt as output parameter.</param>
            /// <param name="newPasswordHashAlgorithm">The new password hash algorithm as output parameter.</param>
            /// <param name="newPasswordLastChangedDateTime">The UTC date and time on which the password was changed.</param>
            /// <param name="newPasswordLastUpdatedOperation">The authentication operation for the last password update.</param>
            private static void GetChangePasswordResult(ReadOnlyCollection<object> changePasswordResult, out string newPasswordHash, out string newPasswordSalt, out string newPasswordHashAlgorithm, out DateTimeOffset newPasswordLastChangedDateTime, out AuthenticationOperation newPasswordLastUpdatedOperation)
            {
                if (changePasswordResult == null || changePasswordResult.Count < ChangePasswordObjectSize)
                {
                    throw new Microsoft.Dynamics.Commerce.Runtime.CommunicationException(CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError);
                }

                newPasswordHash = (string)changePasswordResult[0];
                newPasswordSalt = (string)changePasswordResult[1];
                newPasswordHashAlgorithm = (string)changePasswordResult[2];
                newPasswordLastChangedDateTime = new DateTimeOffset((DateTime)changePasswordResult[3]);
                newPasswordLastUpdatedOperation = (AuthenticationOperation)changePasswordResult[4];
            }

            /// <summary>
            /// Gets Employee from Logon data.
            /// </summary>
            /// <param name="logonData">Logon data from service.</param>
            /// <returns>The employee information.</returns>
            private static Employee GetEmployee(ReadOnlyCollection<object> logonData)
            {
                Employee employee = new Employee();
                employee.Permissions = new EmployeePermissions();
                employee.StaffId = logonData[StaffIdIndex].ToString();
                employee.NameOnReceipt = logonData[NameOnReceiptIndex].ToString();
                employee.Name = logonData[NameIndex].ToString();
                employee.CultureName = logonData[CultureNameIndex].ToString();
                employee.IsPasswordExpired = Convert.ToBoolean(logonData[ChangePasswordIndex]);
                employee.PasswordLastChangedDateTime = new DateTimeOffset(Convert.ToDateTime(logonData[PasswordLastChangedDateTimeIndex]));
                employee.Permissions.MaximumDiscountPercentage = Convert.ToDecimal(logonData[MaximumDiscountPercentageIndex]);
                employee.Permissions.MaximumLineDiscountAmount = Convert.ToDecimal(logonData[MaximumLineDiscountAmountIndex]);
                employee.Permissions.MaximumLineReturnAmount = Convert.ToDecimal(logonData[MaximumLineReturnAmountIndex]);
                employee.Permissions.MaximumTotalDiscountAmount = Convert.ToDecimal(logonData[MaximumTotalDiscountAmountIndex]);
                employee.Permissions.MaximumTotalDiscountPercentage = Convert.ToDecimal(logonData[MaximumTotalDiscountPercentageIndex]);
                employee.Permissions.MaxTotalReturnAmount = Convert.ToDecimal(logonData[MaximumTotalReturnAmountIndex]);
                employee.Permissions.AllowBlindClose = Convert.ToBoolean(logonData[AllowBlindCloseIndex]);
                employee.Permissions.AllowChangeNoVoid = Convert.ToBoolean(logonData[AllowChangeNoVoidIndex]);
                employee.Permissions.AllowCreateOrder = Convert.ToBoolean(logonData[AllowCreateOrderIndex]);
                employee.Permissions.AllowEditOrder = Convert.ToBoolean(logonData[AllowEditOrderIndex]);
                employee.Permissions.AllowFloatingTenderDeclaration = Convert.ToBoolean(logonData[AllowFloatingTenderDeclarationIndex]);
                employee.Permissions.AllowMultipleLogins = Convert.ToBoolean(logonData[AllowMultipleLoginsIndex]);
                employee.Permissions.AllowMultipleShiftLogOn = Convert.ToBoolean(logonData[AllowMUltipleShiftLogOnIndex]);
                employee.Permissions.AllowOpenDrawer = Convert.ToBoolean(logonData[AllowOpenDrawerIndex]);
                employee.Permissions.AllowPriceOverride = Convert.ToInt32(logonData[AllowPriceOverrideIndex]);
                employee.Permissions.AllowRetrieveOrder = Convert.ToBoolean(logonData[AllowRetrieveOrderIndex]);
                employee.Permissions.AllowSalesTaxChange = Convert.ToBoolean(logonData[AllowSalesTaxChangeIndex]);
                employee.Permissions.AllowTenderDeclaration = Convert.ToBoolean(logonData[AllowTenderDeclarationIndex]);
                employee.Permissions.AllowTransactionSuspension = Convert.ToBoolean(logonData[AllowTransactionSuspensionIndex]);
                employee.Permissions.AllowTransactionVoiding = Convert.ToBoolean(logonData[AllowTransactionVoidingIndex]);
                employee.Permissions.AllowXReportPrinting = Convert.ToBoolean(logonData[AllowXReportPrintingIndex]);
                employee.Permissions.AllowZReportPrinting = Convert.ToBoolean(logonData[AllowZReportPrintingIndex]);
                employee.Permissions.AllowKitDisassembly = Convert.ToBoolean(logonData[AllowKitDisassemblyIndex]);
                employee.Permissions.AllowChangePeripheralStation = Convert.ToBoolean(logonData[AllowChangePeripheralStationIndex]);
                employee.Permissions.ManageDevice = Convert.ToBoolean(logonData[ManageDeviceIndex]);
                employee.Permissions.HasManagerPrivileges = Convert.ToBoolean(logonData[AllowManagerPrivilegesIndex]);
                employee.Permissions.AllowPasswordChange = Convert.ToBoolean(logonData[AllowPasswordChangeIndex]);
                employee.Permissions.AllowResetPassword = Convert.ToBoolean(logonData[AllowResetPasswordIndex]);
                employee.Permissions.ContinueOnTSErrors = Convert.ToBoolean(logonData[ContinueOnTSErrorsIndex]);
                employee.Permissions.AllowUseSharedShift = Convert.ToBoolean(logonData[AllowUseSharedShiftIndex]);
                employee.Permissions.AllowManageSharedShift = Convert.ToBoolean(logonData[AllowManageSharedShiftIndex]);

                if (logonData[ImageIndex] != null &&
                    !string.IsNullOrWhiteSpace(logonData[ImageIndex].ToString()))
                {
                    employee.Images = RichMediaHelper.PopulateEmployeeMediaInformation(employee.StaffId, logonData[ImageIndex].ToString(), null);
                }

                return employee;
            }
        }
    }
}