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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Localization;
        using Newtonsoft.Json;

        /// <summary>
        /// This class provides the default exception handling behaviors.
        /// </summary>
        internal static class DefaultExceptionHandlingBehavior
        {
            /// <summary>
            /// Internal error resource identifiers for unhandled/unknown errors.
            /// </summary>
            private const string InternalServerErrorResourceId = "Microsoft_Dynamics_Internal_Server_Error";

            private static readonly string CommerceExceptionTypeName = typeof(CommerceException).Name;
            private static readonly string RetailProxyExceptionTypeName = typeof(RetailProxyException).Name;

            /// <summary>
            /// The default instance of the <see cref="JsonSerializerSettings"/>.
            /// </summary>
            private static readonly Lazy<JsonSerializerSettings> DefaultJsonSerializerSettings = new Lazy<JsonSerializerSettings>(
                () =>
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.None
                });

            /// <summary>
            /// Collection of known commerce exception types deserialization purpose.
            /// </summary>
            private static readonly Lazy<Dictionary<string, Type>> KnownCommerceExceptionTypes = new Lazy<Dictionary<string, Type>>(
                () =>
                {
                    TypeInfo baseExceptionTypeInfo = typeof(RetailProxyException).GetTypeInfo();
                    IEnumerable<TypeInfo> derievedExceptionsTypeInfo = baseExceptionTypeInfo
                        .Assembly.DefinedTypes
                        .Where(t => baseExceptionTypeInfo.IsAssignableFrom(t));

                // Registers itself and all of the derived types from RetailProxyException as a known exception type.
                Dictionary<string, Type> knownTypes = derievedExceptionsTypeInfo.ToDictionary(
                        typeInfo => typeInfo.Name,
                        typeInfo => typeInfo.AsType(),
                        StringComparer.OrdinalIgnoreCase);

                    return knownTypes;
                });

            private static readonly Lazy<RuntimeExceptionLocalizer> ExceptionLocalizerInstance
                    = new Lazy<RuntimeExceptionLocalizer>(() => new RuntimeExceptionLocalizer());

            /// <summary>
            /// Tries to deserialize an JSON string into a retail proxy exception, and suppresses any exceptions, if any.
            /// </summary>
            /// <param name="source">The JSON string.</param>
            /// <param name="exception">The retail proxy exception instance.</param>
            /// <returns>True, if the exception could be deserialized from the input; False, otherwise.</returns>
            internal static bool TryDeserializeFromJsonString(string source, out RetailProxyException exception)
            {
                ThrowIf.Null(source, "source");

                exception = default(RetailProxyException);

                try
                {
                    // Deserializes to a CommerceError object.
                    CommerceError error = JsonConvert.DeserializeObject<CommerceError>(
                           source,
                           DefaultJsonSerializerSettings.Value);

                    // Cannot deserialize the input to a CommerceError.
                    if (error == null
                        || string.IsNullOrWhiteSpace(error.TypeName)
                        || error.Exception == null)
                    {
                        return false;
                    }

                    // Then, based on the type of the exception, deserializes to the actual exception.
                    string typeName = error.TypeName.Replace(CommerceExceptionTypeName, RetailProxyExceptionTypeName);

                    exception = JsonConvert.DeserializeObject(
                            error.Exception,
                            KnownCommerceExceptionTypes.Value[typeName],
                            DefaultJsonSerializerSettings.Value) as RetailProxyException;

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Creates serialized <see cref="CommerceRuntimeException"/> based on the given exception object.
            /// </summary>
            /// <param name="exception">The exception object.</param>
            /// <returns>The serialized <see cref="CommerceRuntimeException"/> object.</returns>
            internal static string SerializeToCommerceException(this Exception exception)
            {
                CommerceException crtException = exception.ResolveToCommerceException();

                return crtException.SerializeToCommerceError(SerializeObjectToJson);
            }

            /// <summary>
            /// Translates the user message in the exception instance.
            /// </summary>
            /// <param name="exception">The exception object whose error resource identifier is to be localized.</param>
            /// <param name="localizer">The localizer instance.</param>
            /// <param name="cultureName">The culture name.</param>
            private static void TranslateUserMessage(this Exception exception, IRuntimeLocalizer localizer, string cultureName)
            {
                ThrowIf.Null(localizer, "localizer");
                ThrowIf.Null(exception, "exception");

                var crtException = exception as CommerceException;
                if (crtException != null && string.IsNullOrWhiteSpace(crtException.LocalizedMessage))
                {
                    // Translates the localized message for commerce exception.
                    crtException.LocalizedMessage = localizer.GetLocalizedString(cultureName, crtException.ErrorResourceId, crtException.LocalizedMessageParameters);
                }

                var dataValidationException = exception as DataValidationException;
                if (dataValidationException != null && dataValidationException.ValidationResults.Any())
                {
                    // Translates the validation results.
                    foreach (DataValidationFailure failure in dataValidationException.ValidationResults)
                    {
                        failure.LocalizedMessage = localizer.GetLocalizedString(cultureName, failure.ErrorResourceId, failure.LocalizedMessageParameters);
                    }
                }
            }

            /// <summary>
            /// Serializes the commerce runtime exception into commerce error string based on the provided serialization function.
            /// </summary>
            /// <param name="exception">The <see cref="CommerceRuntimeException"/> to be serialized.</param>
            /// <param name="serialize">The serialization function.</param>
            /// <returns>The serialized error in <see cref="CommerceError"/> format.</returns>
            private static string SerializeToCommerceError(this CommerceException exception, Func<object, string> serialize)
            {
                string exceptionName = exception.GetType().Name;

                // Then serializes the commerce runtime exception.
                string exceptionString = serialize(exception);
                var error = new CommerceError(exceptionName, exceptionString);

                // Finally, serializes the commerce error, which will be the object exposed to the client through the wire.
                return serialize(error);
            }

            /// <summary>
            /// Serializes an object to JSON string.
            /// </summary>
            /// <param name="value">The object to be serialized.</param>
            /// <returns>The serialized string representation of the object.</returns>
            private static string SerializeObjectToJson(object value)
            {
                return JsonConvert.SerializeObject(
                    value,
                    DefaultJsonSerializerSettings.Value);
            }

            /// <summary>
            /// Resolves an input exception to an instance of <see cref="CommerceRuntimeException"/>.
            /// </summary>
            /// <param name="exception">The exception to be resolved.</param>
            /// <returns>The instance of <see cref="CommerceRuntimeException"/>.</returns>
            private static CommerceException ResolveToCommerceException(this Exception exception)
            {
                var crtException = exception as CommerceException;

                if (crtException != null)
                {
                    // Gets the exception type's name.
                    string typeName = crtException.GetType().Name.Replace(CommerceExceptionTypeName, RetailProxyExceptionTypeName);

                    // Hides the exception if the type name is not recognized by client (not exposed via retail server).
                    if (!KnownCommerceExceptionTypes.Value.ContainsKey(typeName))
                    {
                        crtException = new CommerceException(InternalServerErrorResourceId, exception.Message);
                    }
                }
                else
                {
                    crtException = new CommerceException(InternalServerErrorResourceId, exception.Message);
                }

                if (string.IsNullOrWhiteSpace(crtException.LocalizedMessage))
                {
                    // Translates the error message.
                    string cultureName = CommerceRuntimeManager.Locale;
                    crtException.TranslateUserMessage(ExceptionLocalizerInstance.Value, cultureName);
                }

                return crtException;
            }
        }
    }
}