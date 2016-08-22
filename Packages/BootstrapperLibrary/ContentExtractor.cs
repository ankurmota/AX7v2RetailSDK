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
    namespace Retail.Deployment.SelfService.BootstrapperLibrary
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.IO;
        using System.Reflection;
        using System.Text.RegularExpressions;
    
        /// <summary>
        /// Class to extract content embedded to assembly.
        /// </summary>
        public class ContentExtractor
        {
            private string resourceExtractionDirectoryPath;
            private string assemblyRootNamespace;
            private Assembly assembly;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ContentExtractor" /> class.
            /// </summary>
            /// <param name="assembly">Assembly containing resources.</param>
            /// <param name="assemblyRootNamespace">Root namespace of assembly (embedded resource name prefix).</param>
            /// <param name="resourceExtractionDirectoryPath">Path to extract embedded content to.</param>
            public ContentExtractor(Assembly assembly, string assemblyRootNamespace, string resourceExtractionDirectoryPath)
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException("assembly");
                }
    
                if (string.IsNullOrEmpty(assemblyRootNamespace))
                {
                    throw new ArgumentNullException("assemblyRootNamespace");
                }
    
                if (string.IsNullOrEmpty(resourceExtractionDirectoryPath))
                {
                    throw new ArgumentNullException("resourceExtractionDirectoryPath");
                }
    
                this.assembly = assembly;
                this.assemblyRootNamespace = assemblyRootNamespace;
                this.resourceExtractionDirectoryPath = resourceExtractionDirectoryPath;
            }
    
            /// <summary>
            /// Extracts resources.
            /// </summary>
            public void UnpackResources()
            {
                if (File.Exists(this.resourceExtractionDirectoryPath))
                {
                    File.Delete(this.resourceExtractionDirectoryPath);
                }
    
                if (!Directory.Exists(this.resourceExtractionDirectoryPath))
                {
                    Directory.CreateDirectory(this.resourceExtractionDirectoryPath);
                }
    
                IEnumerable<string> resourceNames = this.assembly.GetManifestResourceNames();
                foreach (string resourceName in resourceNames)
                {
                    if (this.IsResourceEmbedded(resourceName))
                    {
                        this.ExtractEmbeddedResource(resourceName, this.resourceExtractionDirectoryPath);
                    }
                }
            }
    
            /// <summary>
            /// Unzips content. Requires .NET 4.5.
            /// </summary>
            public void UnzipContent()
            {
                IEnumerable<string> resourceNames = this.assembly.GetManifestResourceNames();
    
                string contentZipPath = this.GetContentZipPath(resourceNames);
                string contentOutputPath = this.GetContentExtractionPath();
    
                Zipper.DecompressToDirectory(contentZipPath, contentOutputPath);
            }
    
            /// <summary>
            /// Gets path to extracted setup.exe.
            /// </summary>
            /// <returns>Path to extracted setup.exe.</returns>
            public string GetSetupExePath()
            {
                var tempFolder = this.GetContentExtractionPath();
                var exes = Directory.GetFiles(tempFolder, "*.exe");
                if (exes.Length > 1 || exes.Length == 0)
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The installer requires a single exe file embedded in the content zip package. It found {0}.", exes.Length));
                }
    
                return Path.Combine(tempFolder, exes[0]);
            }
    
            /// <summary>
            /// Gets the content extraction path.
            /// </summary>
            /// <returns>Path to the extracted contents.</returns>
            public string GetContentExtractionPath()
            {
                string contentOutputPath = Path.Combine(this.resourceExtractionDirectoryPath, "content");
                return contentOutputPath;
            }
    
            private static void WriteStreamToFile(Stream stream, string path)
            {
                byte[] streamBytes = new byte[stream.Length];
                stream.Read(streamBytes, 0, streamBytes.Length);
                File.WriteAllBytes(path, streamBytes);
            }
    
            private bool IsResourceEmbedded(string resourceName)
            {
                return !resourceName.Contains("resources")
                    && (resourceName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || resourceName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || resourceName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || resourceName.EndsWith(".config", StringComparison.OrdinalIgnoreCase));
            }
    
            private void ExtractEmbeddedResource(string resourceName, string extractionDirectoryPath)
            {
                using (Stream resourceStream = this.assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream != null)
                    {
                        string extractionFileName = this.GetEmbeddedResourceNameWithoutAssemblyPrefix(resourceName);
    
                        string extractionFilePath = Path.Combine(extractionDirectoryPath, extractionFileName);
                        WriteStreamToFile(resourceStream, extractionFilePath);
                    }
                }
            }
    
            private string GetEmbeddedResourceNameWithoutAssemblyPrefix(string resourceName)
            {
                string resourcePrefix = this.assemblyRootNamespace + ".";
    
                // Get rid of resourcePrefix to get file name we want to extract.
                Regex resourcePrefixReplacer = new Regex(resourcePrefix);
    
                // This replace first occurence only whereas string.Replace replaces all occurences.
                string embeddedResourceNameWithoutAssemblyPrefix = resourcePrefixReplacer.Replace(resourceName, string.Empty, 1);
    
                return embeddedResourceNameWithoutAssemblyPrefix;
            }
    
            private string GetContentZipPath(IEnumerable<string> resourceNames)
            {
                string contentZipPath = string.Empty;
    
                foreach (string resourceName in resourceNames)
                {
                    if (this.IsResourceEmbedded(resourceName) && (resourceName.IndexOf("content.zip", StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        string resourceNameWithoutAssemblyPrefix = this.GetEmbeddedResourceNameWithoutAssemblyPrefix(resourceName);
                        contentZipPath = Path.Combine(this.resourceExtractionDirectoryPath, resourceNameWithoutAssemblyPrefix);
                        break;
                    }
                }
    
                return contentZipPath;
            }
        }
    }
}
