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
    namespace Commerce.Runtime.TransactionService.Serialization
    {
        using System;
        using System.IO;
        using System.Runtime.Serialization;
        using System.Runtime.Serialization.Formatters.Binary;
        using System.Text;
        using System.Xml;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Encapsulates functionality for serialization.
        /// </summary>
        public static class SerializationHelper
        {
            /// <summary>
            /// Serialize object to byte array.
            /// </summary>
            /// <param name="target">The object.</param>
            /// <returns>The byte array.</returns>
            public static byte[] SerializeObjectToByteArray(object target)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream memStream = new MemoryStream())
                {
                    formatter.Serialize(memStream, target);
                    memStream.Flush();
                    memStream.Position = 0;
    
                    BinaryReader br = new BinaryReader(memStream);
                    return br.ReadBytes((int)memStream.Length);
                }
            }
    
            /// <summary>
            /// Deserializes byte array into object.
            /// </summary>
            /// <typeparam name="T">Object type.</typeparam>
            /// <param name="source">Byte array.</param>
            /// <returns>Deserialized object.</returns>
            public static T DeserializeObjectDataContractFromByteArray<T>(byte[] source)
                where T : class
            {
                if (source == null)
                {
                    return null;
                }
    
                using (MemoryStream memStream = new MemoryStream())
                {
                    BinaryWriter binaryWriter = new BinaryWriter(memStream);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryWriter.Write(source);
    
                    // RESET the memStream position
                    memStream.Position = 0;
                    return (T)binaryFormatter.Deserialize(memStream);
                }
            }
    
            /// <summary>
            /// Serializes object to XML string.
            /// </summary>
            /// <typeparam name="T">Object type.</typeparam>
            /// <param name="target">Object to serialize.</param>
            /// <returns>Target serialized in XML string.</returns>
            public static string SerializeObjectToXml<T>(T target)
                where T : class
            {
                return SerializationHelper.SerializeObjectToXml(target, typeof(T));
            }
    
            /// <summary>
            /// Serializes object to XML string.
            /// </summary>
            /// <param name="target">Object to serialize.</param>
            /// <returns>Target serialized to xml string.</returns>
            public static string SerializeObjectToXml(object target)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }
    
                return SerializationHelper.SerializeObjectToXml(target, target.GetType());
            }
    
            /// <summary>
            /// Serializes object to XML string with Unicode encoding.
            /// </summary>
            /// <param name="target">Object to serialize.</param>
            /// <param name="typeInfo">Object type information.</param>
            /// <returns>Target serialized to xml string.</returns>
            public static string SerializeObjectToXml(object target, Type typeInfo)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }
    
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Serialize
                    string xmlString = string.Empty;
                    XmlSerializer xmlSerializer = new XmlSerializer(typeInfo);
                    using (StringWriter writer = new StringWriter())
                    {
                        xmlSerializer.Serialize(writer, target);
                        writer.Flush();
                        xmlString = writer.ToString();
                    }
    
                    return xmlString;
                }
            }
    
            /// <summary>
            /// Deserializes XML string into object.
            /// </summary>
            /// <typeparam name="T">Object type.</typeparam>
            /// <param name="source">XML string.</param>
            /// <returns>Deserialized object.</returns>
            public static T DeserializeObjectFromXml<T>(string source)
                where T : class
            {
                return (T)SerializationHelper.DeserializeObjectFromXml(source, typeof(T));
            }
    
            /// <summary>
            /// Deserializes XML string into object.
            /// </summary>
            /// <typeparam name="T">Object type.</typeparam>
            /// <param name="source">XML string.</param>
            /// <returns>Deserialized object.</returns>
            public static T DeserializeObjectDataContractFromXml<T>(string source)
                where T : class
            {
                if (string.IsNullOrEmpty(source))
                {
                    return null;
                }
    
                using (MemoryStream memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(source)))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    return (T)serializer.ReadObject(memoryStream);
                }
            }
    
            /// <summary>
            /// Deserializes XML string into object.
            /// </summary>
            /// <param name="source">XML string.</param>
            /// <param name="typeInfo">Object type info.</param>
            /// <returns>Deserialized object.</returns>
            public static object DeserializeObjectFromXml(string source, Type typeInfo)
            {
                if (string.IsNullOrEmpty(source))
                {
                    return null;
                }
    
                XmlSerializer serializer = new XmlSerializer(typeInfo);
                StringReader sourceReader = null;
                System.Xml.XmlReader reader = null;
                try
                {
                    sourceReader = new StringReader(source);
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.XmlResolver = null;
                    reader = System.Xml.XmlReader.Create(sourceReader, settings);
                    sourceReader = null;
                    return serializer.Deserialize(reader);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Dispose();
                    }
    
                    if (sourceReader != null)
                    {
                        sourceReader.Dispose();
                    }
                }
            }
    
            /// <summary>
            /// Gets the AX date string for given DateTime with given AX date sequence format.
            /// </summary>
            /// <param name="date">DateTimeOffset to convert.</param>
            /// <param name="dateSequence">Date sequence for string to date in AX. Acceptable inputs: 321, 123, 213.</param>
            /// <returns>Formatted date string according to the date sequence.</returns>
            public static string ConvertDateTimeToAXDateString(DateTimeOffset date, int dateSequence)
            {
                if (date == null)
                {
                    throw new ArgumentNullException("date");
                }
    
                switch (dateSequence)
                {
                    case 321:
                        return date.ToString("yyyy-MM-dd");
                    case 123:
                        return date.ToString("dd-MM-yyyy");
                    case 213:
                        return date.ToString("MM-dd-yyyy");
                    default:
                        throw new NotSupportedException(string.Format("Unrecognized format string: {0}", dateSequence));
                }
            }
    
            /// <summary>
            /// Gets the AX time string for given DateTime.
            /// </summary>
            /// <param name="date">DateTime to convert.</param>
            /// <returns>Formatted date string according to the date sequence.</returns>
            public static string ConvertDateTimeToAXTimeString(DateTimeOffset date)
            {
                if (date == null)
                {
                    throw new ArgumentNullException("date");
                }
    
                return date.ToString("HH:mm:ss");
            }
        }
    }
}
