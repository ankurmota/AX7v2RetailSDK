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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Reflection;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Newtonsoft.Json;

        /// <summary>
        /// Class encapsulates the logic to call each individual CRT fa�ade API adapter.
        /// </summary>
        public sealed class AdaptorCaller
        {
            private const string CommerceRuntimeRequestErrorPrefix = "CommerceRuntimeRequestError_";
            private const string CommerceRuntimeRequestErrorFormat = CommerceRuntimeRequestErrorPrefix + "{0}";
            private const string CommerceRuntimeRequestResponsePrefix = "CommerceRuntimeRequestResponse_";
            private const string CommerceRuntimeRequestResponseFormat = CommerceRuntimeRequestResponsePrefix + "{0}";
            private const string AdaptorParameterPrefix = "$";
            private const string AdaptorLocaleParameter = "$locale";
            private static string managerNamespacePrefix = typeof(AdaptorCaller).GetTypeInfo().Namespace + ".";
            private static Func<string, CommerceRuntimeConfiguration> getCrtConfigByHostFunc;

            /// <summary>
            /// Prevents a default instance of the <see cref="AdaptorCaller"/> class from being created.
            /// </summary>
            private AdaptorCaller()
            {
            }

            internal static Func<string, CommerceRuntimeConfiguration> GetCrtConfigurationByHostFunc
            {
                get { return getCrtConfigByHostFunc; }
            }

            /// <summary>
            /// Gets or sets the host name.
            /// </summary>
            internal static string HostName { get; set; }

            /// <summary>
            /// Sets the CommerceRuntimeConfiguration loading function delegate.
            /// </summary>
            /// <param name="getCrtConfigByHostFunc">The get commerce runtime configuration function delegate.</param>
            public static void SetGetConfigurationFunc(Func<string, CommerceRuntimeConfiguration> getCrtConfigByHostFunc)
            {
                AdaptorCaller.getCrtConfigByHostFunc = getCrtConfigByHostFunc;
            }

            /// <summary>
            /// Processes the request.
            /// </summary>
            /// <param name="allInOneUrl">A URL string to store CRT API call information.</param>
            /// <returns>The result as string.</returns>
            public static string Execute(string allInOneUrl)
            {
                return AdaptorCaller.ExecuteAsync(allInOneUrl).Result;
            }

            /// <summary>
            /// Processes the request asynchronously.
            /// </summary>
            /// <param name="allInOneUrl">A URL string to store CRT API call information.</param>
            /// <returns>
            /// The prefixed commerce runtime response. If it succeeds, the response is prefixed with <see cref="CommerceRuntimeRequestResponsePrefix"/>.
            /// If the request fails, the response is prefixed with <see cref="CommerceRuntimeRequestErrorPrefix"/>.
            /// </returns>
            public static async Task<string> ExecuteAsync(string allInOneUrl)
            {
                try
                {
                    return string.Format(CommerceRuntimeRequestResponseFormat, await CallCommerceRuntimeAsync(allInOneUrl));
                }
                catch (Exception exception)
                {
                    return string.Format(CommerceRuntimeRequestErrorFormat, exception.SerializeToCommerceException());
                }
            }

            /// <summary>
            /// Removes the <see cref="CommerceRuntimeRequestErrorPrefix"/> or <see cref="CommerceRuntimeRequestResponsePrefix"/> from the result, if any.
            /// </summary>
            /// <param name="result">The prefixed result.</param>
            /// <returns>Returns the result without the <see cref="CommerceRuntimeRequestErrorPrefix"/> or <see cref="CommerceRuntimeRequestResponsePrefix"/>.</returns>
            public static string RemoveCommerceRuntimePrefix(string result)
            {
                if (string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }

                if (result.StartsWith(CommerceRuntimeRequestErrorPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return result.Remove(0, CommerceRuntimeRequestErrorPrefix.Length);
                }

                if (result.StartsWith(CommerceRuntimeRequestResponsePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return result.Remove(0, CommerceRuntimeRequestResponsePrefix.Length);
                }

                return result;
            }

            private static async Task<string> CallCommerceRuntimeAsync(string allInOneUrlString)
            {
                Uri uri = new Uri(allInOneUrlString);

                AdaptorCaller.HostName = uri.Host;
                string managerName = uri.Segments[1].TrimEnd('/');
                string methodName = uri.Segments[2];

                HttpValueCollection httpValues = HttpUtility.ParseQueryString(uri.Query);

                // Setup locale if provided in URL
                if (httpValues.ContainsKey(AdaptorLocaleParameter))
                {
                    CommerceRuntimeManager.Locale = httpValues[AdaptorLocaleParameter];
                }

                // Remove all adaptor specific parameters.
                httpValues.RemoveAll(hv => hv.Key.StartsWith(AdaptorParameterPrefix));

                Type managerType = Type.GetType(managerNamespacePrefix + managerName);
                if (managerType != null)
                {
                    var methodInfos = managerType.GetRuntimeMethods().Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));

                    foreach (MethodInfo methodInfo in methodInfos)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();

                        // Make sure the actual method parameter count matches with parameters passed in thru query string.
                        if (parameters.Length == httpValues.Count)
                        {
                            object result = string.Empty;
                            Task task;
                            object classInstance = Activator.CreateInstance(managerType, null);
                            if (parameters.Length == 0)
                            {
                                task = (Task)methodInfo.Invoke(classInstance, null);
                            }
                            else
                            {
                                List<object> parametersList = new List<object>();

                                foreach (ParameterInfo parameter in parameters)
                                {
                                    object parameterValue = null;
                                    var value = httpValues[parameter.Name];

                                    if (value != null)
                                    {
                                        parameterValue = JsonConvert.DeserializeObject(
                                            value,
                                            parameter.ParameterType);
                                    }

                                    parametersList.Add(parameterValue);
                                }

                                task = (Task)methodInfo.Invoke(classInstance, parametersList.ToArray());
                            }

                            await task;

                            if (methodInfo.ReturnType.GetTypeInfo().IsGenericType)
                            {
                                var property = task.GetType().GetTypeInfo().GetDeclaredProperty("Result");
                                if (property != null)
                                {
                                    result = property.GetValue(task);
                                }
                            }

                            if (result == null)
                            {
                                return null;
                            }

                            return result.SerializeToJsonObject();
                        }
                    }

                    throw new InvalidOperationException(string.Format("Can't find method ({0}) on {1}", methodName, managerNamespacePrefix + managerName));
                }

                throw new InvalidOperationException(string.Format("Can't find manager ({0}).", managerNamespacePrefix + managerName));
            }
        }
    }
}
