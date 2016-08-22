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
    namespace Retail.Ecommerce.Publishing
    {
        using System.Configuration;
        using System.Diagnostics;
        using Retail.Ecommerce.Sdk.Core.Publishing;
    
        internal class Program
        {
            private static void Main()
            {
                const int PageSize = 100;
                const bool IncludeCataloglessPublishing = false;
                const bool ForceTimingInfoLogging = false;
                const bool CheckIndividualListingForRemoval = true;
    
                // Creating publishing configuration by specifying page sizes and so on.
                PublishingConfiguration publishingConfig = new PublishingConfiguration(PageSize, PageSize, IncludeCataloglessPublishing, PageSize, ForceTimingInfoLogging, CheckIndividualListingForRemoval);
    
                // Reading application configuration
                Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    
                // Instantiating an instance of publisher.
                Publisher publisher = new Publisher(appConfig, publishingConfig);
    
                Trace.TraceInformation("Initiating channel publishing ...\r\n");
    
                IChannelPublisher channelPublisher = new ChannelPublisher();
                publisher.PublishChannel(channelPublisher);
    
                Trace.TraceInformation("Channel publishing completed.\r\n");
    
                Trace.TraceInformation("Initiating catalog publishing ...");
                ICatalogPublisher catalogPublisher = new CatalogPublisher();
                bool changesDetected = publisher.PublishCatalog(catalogPublisher);
    
                Trace.TraceInformation("Catalog publishing completed. Changes detected = " + changesDetected);
            }
        }
    }
}
