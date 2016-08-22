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
    namespace Commerce.Runtime.TestHost
    {
        using System.Diagnostics;
        using CrossLoyaltySample.Messages;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Sample.ExtensionProperties.Messages;
        using StoreHoursSample.Messages;

        internal static class Program
        {
            private const string DefaultCustomerAccountNumber = "100001";
            private static CommerceRuntime runtime = null;

            private static void Main()
            {
                // This is a sample test host for the CommerceRuntime. Use it during development to get your extensions to work or troubleshoot. 
                // In order to setup, you must 
                //    1) configure the commerceRuntime.config for the correct assemblies and types (according to your environment/customization)
                //    2) configure the commerceRuntime.config for the default channel (i.e. <storage defaultOperatingUnitNumber="052" />)
                //    3) configure the app.config's connection string for Houston to point to a valid database
                CommerceRuntimeManager.SpecifiedRoles = new string[] { "Device" };
                runtime = CommerceRuntimeManager.Runtime;

                // These should execute with default configuration of commerceruntime.config.
                Program.RunDefaultTests();

                // These require additional commerceruntime.config changes. See comments inside method.
                Program.RunSdkSampleTests();

                // You could use this to test your own CommerceRuntime extensions.
                Program.RunExtensionTests();
            }

            private static void RunDefaultTests()
            {
                var queryResultSettings = QueryResultSettings.FirstRecord;
                queryResultSettings.Paging = new PagingInfo(10);

                // query a page of customers
                var customer = CustomerManager.Create(runtime).GetCustomer(DefaultCustomerAccountNumber);
                Debug.WriteLine("Default Customer was ", (customer == null) ? "not found" : "found");

                // query a page of products
                var products = ProductManager.Create(runtime).GetProducts(queryResultSettings);
                Debug.WriteLine("Found {0} product(s).", products.Results.Count);

                // query for pricing with PricingEngine
                SalesTransaction salesTransaction = new SalesTransaction();
                salesTransaction.SalesLines.Add(ConstructSalesLine("0045"));
                GetPriceServiceRequest request = new GetPriceServiceRequest(salesTransaction);
                GetPriceServiceResponse response = runtime.Execute<GetPriceServiceResponse>(request, new RequestContext(runtime));
                Debug.WriteLine("Price for first line item is {0}.", response.Transaction.ActiveSalesLines[0].Price);
            }

            private static void RunSdkSampleTests()
            {
                /* BEGIN SDKSAMPLE_CROSSLOYALTY (do not remove this)
                // Setup: Add these to commerceruntime.config
                // <add source="type" value="Contoso.Commerce.Runtime.Sample.CrossLoyalty.CrossLoyaltyCardService, Contoso.Commerce.Runtime.Sample" />
                GetCrossLoyaltyCardRequest getCrossLoyaltyCardRequest = new GetCrossLoyaltyCardRequest("425-999-4444");
                GetCrossLoyaltyCardResponse getCrossLoyaltyCardResponse = runtime.Execute<GetCrossLoyaltyCardResponse>(getCrossLoyaltyCardRequest, new RequestContext(runtime));
                Debug.WriteLine("The service registered to serve GetCrossLoyaltyCardRequest returned a discount of '{0}'.", getCrossLoyaltyCardResponse.Discount);
                // END SDKSAMPLE_CROSSLOYALTY (do not remove this) */

                /* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
                // Setup: 
                // 1. Add these to commerceruntime.config
                // <add source="type" value="Contoso.Commerce.Runtime.StoreHoursSample.StoreHoursDataService, Contoso.Commerce.Runtime.StoreHoursSample" />
                // 2. SQL updates need to be deployed from Instructions\StoreHours\ChannelDBUpgrade.sql
                // 3. Enable this code
                QueryResultSettings queryResultSettings = QueryResultSettings.SingleRecord;
                queryResultSettings.Paging = new PagingInfo(10);
                GetStoreHoursDataRequest getStoreHoursDataRequest = new GetStoreHoursDataRequest("HOUSTON") { QueryResultSettings = queryResultSettings };
                GetStoreHoursDataResponse getStoreHoursDataResponse = runtime.Execute<GetStoreHoursDataResponse>(getStoreHoursDataRequest, new RequestContext(runtime));
                Debug.WriteLine("The service registered to serve GetStoreHoursDataRequest returned '{0}' instances of StoreDayHours in the first page.", getStoreHoursDataResponse.DayHours.Results.Count);
                // END SDKSAMPLE_STOREHOURS (do not remove this) */

                /* BEGIN SDKSAMPLE_EXTENSIONPROPERTIES (do not remove this)
                // Setup: Add this to commerceruntime.config
                // <add source="type" value="Contoso.Commerce.Runtime.Sample.ExtensionProperties.ExtensionPropertiesService, Contoso.Commerce.Runtime.Sample" />
                // <add source="type" value="Contoso.Commerce.Runtime.Sample.ExtensionProperties.ExtensionPropertiesTriggers, Contoso.Commerce.Runtime.Sample" />
                // <add source="type" value="Contoso.Commerce.Runtime.Sample.ExtensionProperties.CustomNotificationHandler, Contoso.Commerce.Runtime.Sample" />
                ExtensionPropertiesRequest extensionPropertiesRequest = new ExtensionPropertiesRequest();
                extensionPropertiesRequest.SetProperty("EXTENSION_PROPERTY_ADDED", true);
                ExtensionPropertiesResponse extensionPropertiesResponse = runtime.Execute<ExtensionPropertiesResponse>(extensionPropertiesRequest, new RequestContext(runtime));
                bool? entityExtended = (bool?)extensionPropertiesResponse.Entity.GetProperty("EXTENSION_PROPERTY_ADDED");
                Debug.Assert(entityExtended.HasValue && entityExtended.Value == true, "An assumption has not been met!");
                bool? responseExtended = (bool?)extensionPropertiesResponse.GetProperty("EXTENSION_PROPERTY_ADDED");
                Debug.Assert(responseExtended.HasValue && responseExtended.Value == true, "An assumption has not been met!");
                bool? preTriggerCalled = (bool?)extensionPropertiesRequest.GetProperty("PRE_TRIGGER_CALLED");
                Debug.Assert(preTriggerCalled.HasValue && preTriggerCalled.Value == true, "An assumption has not been met!");
                bool? postTriggerCalled = (bool?)extensionPropertiesRequest.GetProperty("POST_TRIGGER_CALLED");
                Debug.Assert(postTriggerCalled.HasValue && postTriggerCalled.Value == true, "An assumption has not been met!");
                // END SDKSAMPLE_EXTENSIONPROPERTIES (do not remove this) */

                /* BEGIN SDKSAMPLE_HEALTHCHECK (do not remove this)
                // Setup: Add this to commerceruntime.config
                // <add source="type" value="Contoso.Commerce.Runtime.Sample.HealthCheck.HealthCheckService, Contoso.Commerce.Runtime.Sample" />
                RunHealthCheckServiceRequest runHealthCheckServiceRequest = new RunHealthCheckServiceRequest(HealthCheckType.DatabaseHealthCheck);
                runtime.Execute<RunHealthCheckServiceResponse>(runHealthCheckServiceRequest, new RequestContext(runtime));
                Debug.WriteLine("The service registered to serve RunHealthCheckServiceRequest was successfully called.");
                // END SDKSAMPLE_HEALTHCHECK (do not remove this) */
            }

            private static void RunExtensionTests()
            {
                // add your own tests here
            }

            private static SalesLine ConstructSalesLine(string itemId, string lineId = "1")
            {
                SalesLine salesLine = new SalesLine();
                salesLine.OriginalSalesOrderUnitOfMeasure = "ea";
                salesLine.SalesOrderUnitOfMeasure = "ea";
                salesLine.ItemId = itemId;
                salesLine.Quantity = 1;
                salesLine.UnitOfMeasureConversion = UnitOfMeasureConversion.CreateDefaultUnitOfMeasureConversion();
                salesLine.LineId = lineId;
                return salesLine;
            }
        }
    }
}
