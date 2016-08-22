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
        using System.IO;
        using System.IO.Compression;
    
        /// <summary>
        /// Zips and unzips directories.
        /// </summary>
        public static class Zipper
        {
            /// <summary>
            /// Compress the given directory to the out file.
            /// </summary>
            /// <param name="pathToCompress">The directory to compress.</param>
            /// <param name="compressedOutFilePath">The output file path.</param>
            public static void CompressDirectory(string pathToCompress, string compressedOutFilePath)
            {
                if (string.IsNullOrWhiteSpace(pathToCompress))
                {
                    throw new ArgumentNullException("pathToCompress");
                }
    
                if (string.IsNullOrWhiteSpace(compressedOutFilePath))
                {
                    throw new ArgumentNullException("compressedOutFilePath");
                }
    
                RemoveItem(compressedOutFilePath);
                ZipFile.CreateFromDirectory(pathToCompress, compressedOutFilePath);
            }
    
            /// <summary>
            /// Decompresses a compressed directory to the output directory.
            /// </summary>
            /// <param name="compressedDirectoryPath">The path to the compressed file.</param>
            /// <param name="outputDirectory">The directory where the uncompressed file will be placed.</param>
            public static void DecompressToDirectory(string compressedDirectoryPath, string outputDirectory)
            {
                if (string.IsNullOrWhiteSpace(compressedDirectoryPath))
                {
                    throw new ArgumentNullException("compressedDirectoryPath");
                }
    
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    throw new ArgumentNullException("outputDirectory");
                }
    
                RemoveItem(outputDirectory);
                ZipFile.ExtractToDirectory(compressedDirectoryPath, outputDirectory);
            }
    
            /// <summary>
            /// Decompresses a compressed stream to the output directory.
            /// </summary>
            /// <param name="compressedDirectoryStream">Stream representing compressed directory.</param>
            /// <param name="outputDirectory">The directory where the uncompressed file will be placed.</param>
            public static void DecompressToDirectory(Stream compressedDirectoryStream, string outputDirectory)
            {
                if (compressedDirectoryStream == null)
                {
                    throw new ArgumentNullException("compressedDirectoryStream");
                }
    
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    throw new ArgumentNullException("outputDirectory");
                }
    
                RemoveItem(outputDirectory);
                using (ZipArchive archive = new ZipArchive(compressedDirectoryStream, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(outputDirectory);
                }
            }
    
            private static void RemoveItem(string path)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
    
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }
    }
}
