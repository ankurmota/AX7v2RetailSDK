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
    namespace Commerce.RetailProxy
    {
        using System;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Newtonsoft.Json;
    
        /// <summary>
        /// Encapsulates functionality used to extend the <see cref="System.String"/> class.
        /// </summary>
        internal static class StringExtensions
        {
            /// <summary>
            /// Deserializes a JSON string into an object.
            /// </summary>
            /// <typeparam name="T">The object type.</typeparam>
            /// <param name="source">The Xml string.</param>
            /// <returns>The deserialized object.</returns>
            public static T DeserializeJsonObject<T>(this string source)
            {
                return (T)DeserializeJsonObject(source, typeof(T));
            }
    
            /// <summary>
            /// Deserializes a JSON string into an object.
            /// </summary>
            /// <param name="source">The JSON string.</param>
            /// <param name="typeInfo">The object type info.</param>
            /// <returns>The deserialized object.</returns>
            public static object DeserializeJsonObject(this string source, Type typeInfo)
            {
                if (string.IsNullOrWhiteSpace(source))
                {
                    return null;
                }
    
                return JsonConvert.DeserializeObject(
                    source,
                    typeInfo,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.None,
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
    
            /// <summary>
            /// Tries to deserialize an JSON string into an object.
            /// </summary>
            /// <typeparam name="T">The object type info.</typeparam>
            /// <param name="source">The JSON string.</param>
            /// <param name="deserializedObject">The deserialized object if deserialization successful or <c>null</c>, otherwise.</param>
            /// <returns><c>True</c> if deserialization succeeded or <c>false</c>, otherwise.</returns>
            public static bool TryDeserializeJsonObject<T>(this string source, out T deserializedObject)
            {
                try
                {
                    deserializedObject = DeserializeJsonObject<T>(source);
                    return true;
                }
                catch
                {
                    deserializedObject = default(T);
                    return false;
                }
            }
    
            /// <summary>
            /// Tries the deserialize data contract JSON object.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <param name="typeInfo">The type information.</param>
            /// <param name="deserializedObject">The deserialized object.</param>
            /// <returns>No return.</returns>
            public static bool TryDeserializeJsonObject(this string source, Type typeInfo, out object deserializedObject)
            {
                try
                {
                    deserializedObject = DeserializeJsonObject(source, typeInfo);
                    return true;
                }
                catch
                {
                    deserializedObject = default(Type);
                    return false;
                }
            }
        }
    }
}
