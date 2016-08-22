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
        using System.Collections.Concurrent;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics;
        using System.Globalization;
        using System.IdentityModel.Tokens;
        using System.ServiceModel;
        using System.Text;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using CP = Retail.TransactionServices.ClientProxy;
        using CRT = Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            #region Fields
    
            // Http correlation activity id header.
            private const string HttpCorrelationActivityIDHeader = "ms-dyn-aid";
    
            /// <summary>
            /// AX date sequence <c>(yyyyMMdd)</c>.
            /// </summary>
            private const int AxDateSequence = 321;
    
            /// <summary>
            /// Transaction service method name in AX constants.
            /// </summary>
            private const string IsAliveMethodName = "IsAlive";
    
            /// <summary>
            /// The failed authentication fault exception sub code.
            /// </summary>
            private const string FailedAuthenticationFaultCode = "FailedAuthentication";
    
            /// <summary>
            /// The forbidden fault exception sub code.
            /// </summary>
            private const string ForbiddenFaultCode = "Forbidden";
    
            /// <summary>
            /// The fault exception code meaning the fault is from the sender with a bad SOAP message.
            /// </summary>
            private const string SenderFaultCode = "Sender";
    
            private static readonly Lazy<ConcurrentDictionary<string, bool>> MethodsNotFoundInAx = new Lazy<ConcurrentDictionary<string, bool>>();
    
            /// <summary>
            /// AX minimum date time.
            /// </summary>
            private static readonly DateTime AxMinDateTime = new DateTime(1900, 1, 1);
    
            private readonly RequestContext context;
            private readonly ITransactionServiceClientFactory clientFactory;
    
            #endregion
    
            /// <summary>
            /// Initializes a new instance of the <see cref="TransactionServiceClient"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public TransactionServiceClient(RequestContext context)
                : this(context, new TransactionServiceClientFactory(context))
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="TransactionServiceClient"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transactionServiceClientFactory">The transaction service client factory.</param>
            public TransactionServiceClient(RequestContext context, ITransactionServiceClientFactory transactionServiceClientFactory)
            {
                ThrowIf.Null(context, "context");
    
                this.context = context;
                this.clientFactory = (transactionServiceClientFactory == null) ? new TransactionServiceClientFactory(context) : transactionServiceClientFactory;
            }
    
            private delegate CP.RetailTransactionServiceResponse TransactionServiceInvoker(CP.RetailRealTimeServiceContractChannel channel, CP.RetailTransactionServiceRequestInfo requestInfo);
    
            /// <summary>
            /// Gets the algorithm used in password hashing from the channel configuration.
            /// </summary>
            public string PasswordHashAlgorithm
            {
                get { return this.clientFactory.PasswordHashAlgorithm; }
            }
    
            /// <summary>
            /// Creates the mapped exception for <see cref="Exception"/> returned during invoking realtime transaction service method.
            /// </summary>
            /// <param name="methodName">The method name.</param>
            /// <param name="exception">The exception.</param>
            /// <param name="errorResourceId">The <see cref="CommunicationErrors"/> enumeration.</param>
            /// <param name="errorMessage">The error message in the communication exception.</param>
            /// <returns>The <see cref="CommunicationException"/>.</returns>
            public static CRT.CommunicationException CreateCommunicationException(string methodName, Exception exception, CommunicationErrors errorResourceId, string errorMessage = "")
            {
                ThrowIf.Null(methodName, "methodName");
                ThrowIf.Null(exception, "exception");
    
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? string.Format("Exception while calling invoke method {0}: {1}", methodName, exception.Message) : errorMessage;
    
                return new CRT.CommunicationException(
                    errorResourceId,
                    exception,
                    errorMessage);
            }
    
            /// <summary>
            /// Invoke method with given method name and parameter list from AX.
            /// </summary>
            /// <param name="methodName">Method name.</param>
            /// <param name="parameters">The parameter set.</param>
            /// <returns>A list of returned items if available.</returns>
            /// <exception cref="CommunicationException">Throws if the call failed.</exception>
            public ReadOnlyCollection<object> InvokeMethod(string methodName, params object[] parameters)
            {
                ThrowIf.Null<string>(methodName, "methodName");
    
                CP.RetailTransactionServiceResponse serviceResponse = this.GetResponseFromMethod(methodName, parameters);
    
                // Throw if service response does not contain any data.
                if (serviceResponse.Data == null || serviceResponse.Data.Length == 0)
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        "Service response does not contain any data.");
                }
    
                return new ReadOnlyCollection<object>(serviceResponse.Data);
            }
    
            /// <summary>
            /// Invoke extension method with given method name and parameter list from AX.
            /// </summary>
            /// <param name="methodName">Method name.</param>
            /// <param name="parameters">The parameter set.</param>
            /// <returns>A list of returned items if available.</returns>
            /// <exception cref="CommunicationException">Throws if the call failed.</exception>
            public ReadOnlyCollection<object> InvokeExtensionMethod(string methodName, params object[] parameters)
            {
                ThrowIf.Null<string>(methodName, "methodName");
    
                CP.RetailTransactionServiceResponse serviceResponse = this.GetResponseFromMethodEx(methodName, parameters);
    
                // Throw if service response does not contain any data.
                if (serviceResponse.Data == null || serviceResponse.Data.Length == 0)
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        "Service response does not contain any data.");
                }
    
                return new ReadOnlyCollection<object>(serviceResponse.Data);
            }
    
            #region Health check
            /// <summary>
            /// Invokes transaction service health check.
            /// </summary>
            /// <returns>True if IsAlive call succeeded and false otherwise.</returns>
            public bool IsAlive()
            {
                TransactionServiceInvoker invoker =
                    (channel, info) => { return channel.IsAlive(new CP.IsAlive()).result; };
    
                CP.RetailTransactionServiceResponse response = this.GetResponseFromMethod(invoker, IsAliveMethodName, 0);
                return response.Success;
            }
    
            #endregion
    
            /// <summary>
            /// Converts extension properties object data to xml string.
            /// </summary>
            /// <param name="commerceProperties">The commerce properties.</param>
            /// <returns>The xml string.</returns>
            internal static string CreateExtensionPropertiesParameter(ICollection<CommerceProperty> commerceProperties)
            {
                if (commerceProperties == null)
                {
                    return string.Empty;
                }
    
                StringBuilder parameterBuilder = new StringBuilder("<ExtensionProperties>");
                foreach (CommerceProperty commerceProperty in commerceProperties)
                {
                    object propertyValue = commerceProperty.Value.GetPropertyValue();
                    string propertyValueAsString = propertyValue != null ? string.Format(CultureInfo.InvariantCulture, "{0}", propertyValue) : string.Empty;
                    parameterBuilder.AppendFormat("<{0}>{1}</{0}>", commerceProperty.Key, propertyValueAsString);
                }
    
                parameterBuilder.Append("</ExtensionProperties>");
    
                return parameterBuilder.ToString();
            }
    
            #region Helper Methods
    
            /// <summary>
            /// Validates the date time offset.
            /// </summary>
            /// <param name="dateTimeOffset">The date time offset.</param>
            private static void ValidateDateTimeOffset(DateTimeOffset? dateTimeOffset)
            {
                if (dateTimeOffset.HasValue &&
                    (dateTimeOffset.Value.DateTime == DateTime.MaxValue || dateTimeOffset.Value.DateTime == DateTime.MinValue))
                {
                    throw new ArgumentException("DateTimeOffset value out of range", "dateTimeOffset");
                }
            }
    
            /// <summary>
            /// Parses the attribute value of type <c>DateTimeOffset</c> from the XML element.
            /// </summary>
            /// <param name="element">The XML element.</param>
            /// <param name="attributeName">The attribute name.</param>
            /// <returns>The attribute value.</returns>
            private static DateTimeOffset ParseDateTimeOffset(XElement element, string attributeName)
            {
                DateTime dateTime;
                DateTimeOffset dateTimeOffset = DateTimeOffset.MinValue;
                if (DateTime.TryParse(TransactionServiceClient.GetAttributeValue(element, attributeName), out dateTime))
                {
                    DateTime? nullableDateTime = dateTime;
                    dateTimeOffset = nullableDateTime.ToUtcDateTimeOffset().GetValueOrDefault();
                }
    
                return dateTimeOffset;
            }
    
            /// <summary>
            /// Parses the fault exception and retrieves the fault and sub fault codes.
            /// </summary>
            /// <param name="exception">The exception instance.</param>
            /// <returns>A tuple where first element is the fault code and second element is the first sub fault code.</returns>
            private static Tuple<string, string> ParseFaultException(Exception exception)
            {
                string faultCode = string.Empty;
                string faultSubCode = string.Empty;
    
                while (exception != null)
                {
                    FaultException faultException = exception as FaultException;
                    if (faultException != null && faultException.Code != null)
                    {
                        faultCode = faultException.Code.Name;
                        if (faultException.Code.SubCode != null)
                        {
                            faultSubCode = faultException.Code.SubCode.Name;
                        }
    
                        break;
                    }
    
                    exception = exception.InnerException;
                }
    
                return new Tuple<string, string>(faultCode, faultSubCode);
            }
    
            /// <summary>
            /// Gets the response from method.
            /// </summary>
            /// <param name="methodName">Name of the method.</param>
            /// <param name="parameterList">The parameter list.</param>
            /// <returns>The service response.</returns>
            private CP.RetailTransactionServiceResponse GetResponseFromMethod(string methodName, params object[] parameterList)
            {
                TransactionServiceInvoker invoker =
                    (channel, requestInfo) =>
                    {
                        CP.InvokeMethod invokeRequest = new CP.InvokeMethod() { request = requestInfo, methodName = methodName, parameters = parameterList };
                        return channel.InvokeMethod(invokeRequest).result;
                    };
    
                int parameterCount = parameterList == null ? 0 : parameterList.Length;
                return this.GetResponseFromMethod(invoker, methodName, parameterCount);
            }
    
            /// <summary>
            /// Gets the response from extension method.
            /// </summary>
            /// <param name="methodName">Name of the method.</param>
            /// <param name="parameterList">The parameter list.</param>
            /// <returns>The service response.</returns>
            private CP.RetailTransactionServiceResponse GetResponseFromMethodEx(string methodName, params object[] parameterList)
            {
                TransactionServiceInvoker invoker =
                    (channel, requestInfo) =>
                    {
                        CP.InvokeExtensionMethod invokeRequest = new CP.InvokeExtensionMethod() { request = requestInfo, methodName = methodName, parameters = parameterList };
                        return channel.InvokeExtensionMethod(invokeRequest).result;
                    };
    
                int parameterCount = parameterList == null ? 0 : parameterList.Length;
                return this.GetResponseFromMethod(invoker, methodName, parameterCount);
            }
    
            /// <summary>
            /// Gets the response from method.
            /// </summary>
            /// <param name="transactionServiceInvoker">Delegate that invokes a specific operation on channel object.</param>
            /// <param name="methodName">Name of the method.</param>
            /// <param name="parameterCount">Number of parameters used during the call.  Used for instrumentation purposes.</param>
            /// <returns>The service response.</returns>
            private CP.RetailTransactionServiceResponse GetResponseFromMethod(TransactionServiceInvoker transactionServiceInvoker, string methodName, int parameterCount)
            {
                CP.RetailTransactionServiceResponse response = null;
    
                using (RealTimeServiceClientBoundaryPerfContext perfContext = new RealTimeServiceClientBoundaryPerfContext())
                {
                    Guid correlationId = Guid.NewGuid();
                    Guid relatedActivityId = Guid.NewGuid();
                    RetailLogger.Log.CrtTransactionServiceClientRtsCallStarted(correlationId, methodName, parameterCount, relatedActivityId);
                    int resultCount = -1;
                    string language = null;
                    string company = null;
    
                    CP.RetailRealTimeServiceContractChannel channel = null;
                    Exception exception = null;
    
                    try
                    {
                        channel = this.clientFactory.CreateTransactionServiceClient();
    
                        // Add HTTP header attribute named 'ms-dyn-aid' with value as activity id.
                        using (var contextScope = new OperationContextScope(channel))
                        {
                            this.SetActivityIdInHttpHeader(relatedActivityId);
                            CP.RetailTransactionServiceRequestInfo requestInfo = this.clientFactory.CreateRequestInfo();
                            company = requestInfo.Company;
                            language = requestInfo.Language;
                            response = transactionServiceInvoker(channel, requestInfo);
                            channel.Close();
                        }
                    }
                    catch (System.ServiceModel.CommunicationException ex)
                    {
                        // Retrieves the SubCode in the fault exception, and maps them to corresponding error resources and diagnostic entries.
                        CommunicationErrors errorResourceId = CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure;
                        string errorMessage = string.Empty;
                        Tuple<string, string> faultCodes = TransactionServiceClient.ParseFaultException(ex);
    
                        if (faultCodes.Item2.Equals(TransactionServiceClient.FailedAuthenticationFaultCode, StringComparison.OrdinalIgnoreCase))
                        {
                            errorMessage = string.Format(
                                "Real-time Service call for method '{0}' failed due to security reason such as misconfigured, or expired Real-time Service certificate. Please also verify if the Real-time Service certificate is being properly configured in AX.",
                                methodName);
                            errorResourceId = CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_TransactionServiceAuthenticationFailedFault;
                        }
                        else if (faultCodes.Item2.Equals(TransactionServiceClient.ForbiddenFaultCode, StringComparison.OrdinalIgnoreCase))
                        {
                            errorMessage = string.Format(
                                "Real-time Service call for method '{0}' failed due to invalid Real-time Service profile settings. Please make sure the Real-time Service profile user and identity provider fields are defined correctly in AX.",
                                methodName);
                            errorResourceId = CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_TransactionServiceForbiddenFault;
                        }
                        else if (faultCodes.Item1.Equals(TransactionServiceClient.SenderFaultCode, StringComparison.OrdinalIgnoreCase))
                        {
                            errorMessage = string.Format(
                                "Real-time Service call for method '{0}' failed due to an unhandled exception, or due to invalid user permissions settings in Real-time Service profile. Please refer to the exception details for more information.",
                                methodName);
                            errorResourceId = CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_TransactionServiceSenderFault;
                        }
    
                        exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, errorResourceId, errorMessage);
                    }
                    catch (SecurityTokenException ex)
                    {
                        // channel.Abort() will never throw
                        if (channel != null)
                        {
                            channel.Abort();
                        }
    
                        exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure);
                    }
                    catch (TimeoutException ex)
                    {
                        exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_TransactionServiceTimeOut);
                    }
                    catch (Exception ex)
                    {
                        // channel.Abort() will never throw
                        if (channel != null)
                        {
                            channel.Abort();
                        }
    
                        exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_TransactionServiceException);
                    }
    
                    // Throws an exception wrapping localized AX message for unsuccessful request.
                    if (exception == null && !response.Success)
                    {
                        exception = new HeadquarterTransactionServiceException(
                            response.Data,
                            string.Format("Real-time Service was successfully connected, but the method call {0} failed with this error : {1}", methodName, response.Message))
                        {
                            // Since content in the response.Message is already localized on AX, we copy it directly to user message field.
                            LocalizedMessage = response.Message
                        };
                    }
                    else if (response != null && response.Data != null)
                    {
                        resultCount = response.Data.Length;
                    }
    
                    if (exception != null)
                    {
                        RetailLogger.Log.CrtTransactionServiceClientRtsCallError(correlationId, methodName, parameterCount, language, company, exception.GetType().ToString(), exception, relatedActivityId);
                        throw exception;
                    }
    
                    perfContext.ResultsCount = resultCount;
                    perfContext.CallWasSuccessful();
                    RetailLogger.Log.CrtTransactionServiceClientRtsCallSuccessful(correlationId, methodName, parameterCount, resultCount, language, company, relatedActivityId);
                }
    
                return response;
            }
    
            /// <summary>
            /// Helper method to invoke method with no return data.
            /// </summary>
            /// <param name="methodName">Method to invoke.</param>
            /// <param name="parameterList">The list of parameters as object array.</param>
            private void InvokeMethodNoDataReturn(string methodName, params object[] parameterList)
            {
                this.GetResponseFromMethod(methodName, parameterList);
            }
    
            /// <summary>
            /// Invoke method with given method name and parameter list from AX without the check for null on the responding data.
            /// </summary>
            /// <param name="methodName">Method name.</param>
            /// <param name="parameters">The parameter set.</param>
            /// <returns>A list of returned items if available.</returns>
            /// <exception cref="CommunicationException">Throws if the call failed.</exception>
            private ReadOnlyCollection<object> InvokeMethodAllowNullResponse(string methodName, params object[] parameters)
            {
                ThrowIf.Null<string>(methodName, "methodName");
    
                CP.RetailTransactionServiceResponse serviceResponse = this.GetResponseFromMethod(methodName, parameters);
    
                if (serviceResponse == null)
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        "Service response is null.");
                }
    
                return new ReadOnlyCollection<object>(serviceResponse.Data);
            }
    
            /// <summary>
            /// For backward compatibility with new AX methods: when new AX methods are not yet deployed.
            /// </summary>
            /// <param name="newMethodName">New method name.</param>
            /// <param name="extensionProperties">Extension properties parameter for new methods.</param>
            /// <param name="oldMethodName">Old method name.</param>
            /// <param name="parameters">The parameters.</param>
            /// <returns>The data from AX.</returns>
            private ReadOnlyCollection<object> TryNewMethodOrFallback(string newMethodName, string extensionProperties, string oldMethodName, params object[] parameters)
            {
                ReadOnlyCollection<object> data = null;
                try
                {
                    if (!MethodsNotFoundInAx.Value.ContainsKey(newMethodName))
                    {
                        var newParams = new List<object>(parameters);
                        newParams.Add(extensionProperties);
                        data = this.InvokeMethod(newMethodName, newParams.ToArray());
                    }
                }
                catch (Exception)
                {
                    if (!MethodsNotFoundInAx.Value.ContainsKey(newMethodName))
                    {
                        throw;
                    }
                }
    
                if (MethodsNotFoundInAx.Value.ContainsKey(newMethodName))
                {
                    // Fallback to old AX method
                    RetailLogger.Log.CrtTransactionServiceClientFallbackMethodCalledWarning(newMethodName, oldMethodName);
                    data = this.InvokeMethod(oldMethodName, parameters);
                }
    
                return data;
            }
    
            /// <summary>
            /// Set correlation activity id in the header of request.
            /// </summary>
            /// <param name="activityId">The correlation activity identifier to set in the header.</param>
            private void SetActivityIdInHttpHeader(Guid activityId)
            {
                System.ServiceModel.Channels.HttpRequestMessageProperty httpHeaders = new System.ServiceModel.Channels.HttpRequestMessageProperty();
                httpHeaders.Headers[HttpCorrelationActivityIDHeader] = activityId.ToString();
                OperationContext.Current.OutgoingMessageProperties[System.ServiceModel.Channels.HttpRequestMessageProperty.Name] = httpHeaders;
            }
    
            #endregion
        }
    }
}
