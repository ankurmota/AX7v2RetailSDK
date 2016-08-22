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
    namespace RetailServer.TestClient
    {
        using System;
        using System.IO;
        using System.Linq;
        using System.Reflection;
        using System.Text;
        using System.Windows.Forms;
        using System.Xml;
        using System.Xml.Serialization;
        using Commerce.RetailProxy;
        using Commerce.RetailProxy.Authentication;

        /// <summary>
        /// Helper functions.
        /// </summary>
        internal static class Helpers
        {
            /// <summary>
            /// Tries to get a windows form if it is already open.
            /// </summary>
            /// <typeparam name="T">Any type of Windows form.</typeparam>
            /// <param name="form">The form if it was found.</param>
            /// <returns>True if found, otherwise false.</returns>
            internal static bool TryGetForm<T>(out T form)
            {
                form = Application.OpenForms.OfType<T>().FirstOrDefault();
                return form != null;
            }

            /// <summary>
            /// Deserializes an Xml file.
            /// </summary>
            /// <typeparam name="T">Any type.</typeparam>
            /// <param name="filePath">The path of the Xml file to load.</param>
            /// <param name="instance">The instance if deserialization succeeded.</param>
            /// <returns>True if succeeded, otherwise false.</returns>
            internal static bool TryLoadFromXml<T>(string filePath, out T instance)
            {
                instance = default(T);
                if (File.Exists(filePath))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    FileStream stream = new FileStream(filePath, FileMode.Open);
                    using (XmlTextReader reader = new XmlTextReader(stream))
                    {
                        reader.DtdProcessing = DtdProcessing.Prohibit;
                        instance = (T)xmlSerializer.Deserialize(reader);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Serializes an object to Xml.
            /// </summary>
            /// <typeparam name="T">The type to serialize.</typeparam>
            /// <param name="filePath">The file path to write the Xml to.</param>
            /// <param name="instance">The object to serialize.</param>
            internal static void ToXml<T>(string filePath, T instance)
            {
                XmlSerializer writer = new XmlSerializer(instance.GetType());
                using (StreamWriter file = new StreamWriter(filePath))
                {
                    writer.Serialize(file, instance);
                }
            }

            /// <summary>
            /// Creates a new RetailServerContext for online communications.
            /// </summary>
            /// <param name="retailServerUrl">The url to the RetailServer.</param>
            /// <returns>The RetailServerContext instance.</returns>
            internal static RetailServerContext CreateNewRetailServerContext(string retailServerUrl)
            {
                CommerceAuthenticationRetailServerProvider authenticationProvider = new CommerceAuthenticationRetailServerProvider(new Uri(string.Format("{0}/auth", retailServerUrl)));
                RetailServerContext context = new RetailServerContext(new Uri(string.Format("{0}/commerce", retailServerUrl)), authenticationProvider);
                return context;
            }

            /// <summary>
            /// Tests if all interfaces that should be implemented are implemented. This is important as these classes are called in offline mode via reflection. Hence it is 
            /// possible to forget an implementation without getting any compiler errors, just run time errors. This helper method tests that all possible adapters that 
            /// could be called have appropriate classes implemented.
            /// </summary>
            /// <returns>The string to log.</returns>
            internal static string TestRetailProxyIsFullyImplemented()
            {
                StringBuilder sb = new StringBuilder();

                Assembly retailProxyAssembly = typeof(RetailServerContext).Assembly;
                Type interfaceToBeImplemented = retailProxyAssembly.DefinedTypes.Where(type => type.FullName.Contains("Commerce.RetailProxy.Adapters.IEntityManager")).First();

                // get all other interfaces in the same assembly that implement Adapters.IEntityManager
                var adapterInterfaces = retailProxyAssembly.DefinedTypes.Where(type => type.IsInterface && type.ImplementedInterfaces.Any(inter => inter == interfaceToBeImplemented));

                // get all classes that implement each interface, make sure there is only one and the name is correct
                foreach (TypeInfo adapterInterface in adapterInterfaces)
                {
                    TypeInfo foundClass = retailProxyAssembly.DefinedTypes.Where(type => type.IsClass && type.ImplementedInterfaces.Any(inter => inter.FullName == adapterInterface.FullName)).FirstOrDefault();
                    if (foundClass != null)
                    {
                        string expectedClassName = adapterInterface.FullName.Replace(".Adapters.I", ".Adapters.");
                        if (foundClass.FullName.Equals(expectedClassName))
                        {
                            sb.AppendFormat("OK: Found {0} implementing {1}.{2}", foundClass.FullName, adapterInterface.FullName, Environment.NewLine);
                        }
                        else
                        {
                            sb.AppendFormat("Error: Did not find any class implementing {0}.{1}", adapterInterface.FullName, Environment.NewLine);
                        }
                    }
                    else
                    {
                        sb.AppendFormat("Error: Did not find any class implementing {0}.{1}", adapterInterface.FullName, Environment.NewLine);
                    }
                }

                return sb.ToString();
            }
        }
    }
}
