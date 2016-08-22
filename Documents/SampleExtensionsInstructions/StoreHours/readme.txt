Sample overview:
The sample shows how to create a new business entity (StoreHours) accross both AX and the channel. 

Changes are in AX tables, CDX, Channel DB, CRT, RetailServer, Point of Sale (both Modern POS and Cloud POS). Offline mode for Modern POS is supported for this sample.

Setup steps:

0. It is advised that you do these changes on a untouched RetailSdk. Ideally, you would have it under source control (VSO, or similar) and no files are changed so far. This is ideal, as you could revert at any steps
   without much work. Also, ideally, you would change some specific settings for your whole org, i.e. versioning, naming, etc. See the Retail Sdk documentation for details.
   
1. AX customization: 
    - import project file AXchanges.axpp, compile and run job

    OR do these manual steps

    - Create a new Table called ISVRetailStoreHoursTable
        - Enum field: Day,  enum type: WeekDays, mandatory, 
        - Int field: OpenTime, extended data type: TimeOfDay, mandatory, 
        - Int field: ClosingTime, extended data type: TimeOfDay, mandatory, 
        - Int64 field: RetailStoreTable, extended data type: RefRecId, mandatory
    - Create a Foreign Key Relation to RetailStoreTable
        - Name: RetailStoreTable
        - Cardinality: ZeroOne
        - RelatedTable: RetailStoreTable
        - Related table cardinality: ExactlyOne
        - Relationship type: Association
        - Contraint: Normal, name: RetailStoreTable, Field: RetailStoreTable, Related field: RecId
    
    
    - populate some data by running the Temp_InsertData job (make it a Startup object and hit F5)

2. CDX:
    1.  In AX, go to Retail -> Setup -> Retail Scheduler -> Retail Channel schema and edit the channel schema by adding a new table (channel tables, new): 
        a.  ax.ISVRetailStoreHoursTable, Save
        b.  add fields including RecId, excluding recVersion and Partition
        c.  hit Ok
 
    2.  Create a new subjob (Retail -> Setup -> Retail Scheduler -> scheduler subjobs):
        a.  Name: ISVRetailStoreHoursTable,
        b.  Channel table name: ax.ISVRetailStoreHoursTable, 
        c.  AX table ISVRetailStoreHoursTable,
        d.  Hit “Match fields”, 
        e.  Save

    3.  Add the new sub job to job “Channel configuration” - 1070 (Retail -> Setup -> Retail Scheduler -> scheduler jobs) and Save it
    
    4.  Edit the CDX table distribution xml (Retail -> Setup -> Retail Scheduler -> Retail channel schema, export, edit, save with new name, import), add this XML fragment inside both RetailStoreTable nodes
        <Table name="ISVRetailStoreHoursTable">
          <LinkGroup>
            <Link type="FieldMatch" fieldName="RetailStoreTable" parentFieldName="RecId" />
          </LinkGroup>
        </Table>
    
    By adding this, we explicitely instruct to include changes in this table when synced with the channels

    5.  Regenerate queries (Retail channel schema screen)
    
3. Channel db (manual, just for development, for official change, see deployment below):
    - apply schema change from ChannelDBUpgrade.sql to correct channel database
    
4. Verify CDX:
    - run 1070 job full sync (channel data group)
    - check Download sessions and channel db that the data arrived

Note: The CRT and RetailServer code changes are all part of the RetailSdk\SampleExtensions. Therefore the steps below refer to how to build, deploy and test these. 

    
5. Enable and test CRT sample code:
    - open solution at RetailSdk\SampleExtensions\CommerceRuntime\CommerceRuntimeSamples.sln
    - Register the CRT change in RetailSDK\Assets\commerceruntime.configcommerceruntime.config:
    <add source="type" value="Contoso.Commerce.Runtime.Sample.StoreHours.StoreHoursDataService, Contoso.Commerce.Runtime.Sample" />
    - add the new CRT extension dll to customization.settings
    - in commerceRuntime.config, add defaultOperatingUnitNumber=052 (if Contoso demo data)
    - Run CRT test host project (Runtime.Extensions.TestHost.csproj) in debugger and execute code for store hours sample (part of RunSdkSampleTest() method)
    
6. Enable and test RetailServer sample code:
    - open project at RetailSDK\SampleExtensions\RetailServer\Extensions.StoreHoursSample\RetailServer.Extensions.StoreHoursSample.csproj and compile it
    - use inetmgr to find the location of the local Retail Server bin folder
    -  register in web.config file
      <add source="assembly" value="StoreHours.RetailServer.StoreHoursSample" />
    - add the new RS extension dll to customization.settings
    - in customization.settings, switch the RetailServerLibraryPathForProxyGeneration to use the new RetailServer sample dll: $(SdkReferencesPath)\StoreHours.RetailServer.StoreHoursSample.dll
    - copy the same commerceRuntime.config file from Crt test host into the RetailServer's bin folder
          <add source="type" value="Contoso.Commerce.Runtime.Sample.StoreHours.StoreHoursDataService, Contoso.Commerce.Runtime.Sample" />
    - drop both the CRT and RS sample dlls into the RetailServer bin folder (or use the RetailSdk's AfterBuildDropBinariesToRetailServer target for rapid development)
      (If you use the AfterBuildDropBinariesToRetailServer, you have to rebuild the 2 dlls at least once to binplace them automatically)
    - Use inetmgr to browse to the RetailServer's $metadata, i.e. at https://usnconeboxax1ret.cloud.onebox.dynamics.com/RetailServer/Commerce/$metadata and verify that the 
      StoreHours entity is exposed by it (search for "StoreHours in the xml")
      
7. Implement the proxy code for offline mode (equivalent to RetailServer controller but for local CommerceRuntime when the client is not connected)
    - open RetailSDK\Proxies\RetailProxy\Proxies.RetailProxy.csproj, add RetailSDK\SampleExtensions\CommerceRuntime\Extensions.StoreHoursSample\Runtime.Extensions.StoreHoursSample.csproj
      to the solution and add a project reference from RetailProxy to StoreHoursSample project
    - Add a new class StoreDayHoursManager.cs at Adapters\StoreDayHoursManager.cs. Use another manager as a template, so namespaces using statements are correct:
    
        namespace Contoso
        {
            namespace Commerce.RetailProxy.Adapters
            {
                using System;
                using System.Threading.Tasks;
                using Contoso.Commerce.Runtime.DataModel;
                using Microsoft.Dynamics.Commerce.Runtime;
                using Microsoft.Dynamics.Commerce.Runtime.DataModel;
                using Runtime.StoreHoursSample.Messages;

                internal class StoreDayHoursManager : IStoreDayHoursManager
                {
                }
            }
        }
        
    - use Visual Studio's feature to implenent the interface methods for you (CTRL+.) and leave all methods as they are, except implement the one we need:
    
        public Task<PagedResult<StoreDayHours>> GetStoreDaysByStore(string storeNumber, QueryResultSettings queryResultSettings)
        {
            var request = new GetStoreHoursDataRequest(storeNumber) { QueryResultSettings = queryResultSettings };
            return Task.Run(() => CommerceRuntimeManager.Runtime.Execute<GetStoreHoursDataResponse>(request, null).DayHours);
        }
    
    - configure the file RetailSDK\Proxies\RetailProxy\Adapters\UsingStatements.Extensions.txt to add the using statment: "using Contoso.Commerce.Runtime.DataModel;"
    - add the service dll to RetailSDK\Assets\CommerceRuntime.MPOSOffline.config (similarly as you did for the commerceRuntime.config file)
        <add source="type" value="Contoso.Commerce.Runtime.Sample.StoreHours.StoreHoursDataService, Contoso.Commerce.Runtime.Sample" />
    - update RetailSDK\Assets\dllhost.exe.config so the ClientBroker will load our RetailProxy assembly
        <add key="RetailProxyAssemblyName" value="Contoso.Commerce.RetailProxy" />  (Update to the correct assembly name)
        <add key="AdaptorCallerFullTypeName" value="Contoso.Commerce.RetailProxy.Adapters.AdaptorCaller" />
    

8. Use the RetailServer test client to verify that calling the new functionality succeeds. 
    - open solution file RetailSdk\SampleExtensions\RetailServer\Extensions.TestClient
    - uncomment the code that is marked with "SDKSAMPLE_STOREHOURS" in MainForm.cs
    - compile solution (if you get compilation errors regarding missing StoreHours-related types, your missed the step above to switch RetailServerLibraryPathForProxyGeneration to the new dll)
    - there should be no compilation errors.
    - run the app
    - Enter the RetailServer url in the text box next to the "Activate New" button and hit it.
    - Enter device and register ids and hit Activate.
    - Enter the AAD credentials that has the registration priviledges and hit Ok.
    - Wait a few seconds.
    - Test client should now show what device is registered.
    - Hit login button and login with worker credentials.
    - Hit Custom  button. This will call the new functionality. If you see GetStoreDaysByStore in the Debug window succeeding, your validation succeeded.
    Notes:
    - To see a console with errors/logs, use the "Debug" button.

9. Extend Modern POS
    - open solution at RetailSdk\POS\ModernPOS.sln, make sure it fully compiles, and make sure Modern POS can be run from Visual Studio using F5 (uncustomized, enable UAC, uninstall installed MPOS if needed)
    - include the RetailSdk\POS\Pos.Extension.StoreHoursSample.csproj project in the solution. Change the project build order to the extensions project gets built after Core and before App projects
    - inspect the newly added project and its content
    - Prepare SharedApp virtual project
        - in SharedApp\Pos.SharedApp.projitems, add these at the appropriate places (this adds a reference to the extension project): 
        
            <Content Include="$(MsBuildThisFileDirectory)..\Extension.StoreHoursSample\Custom.Extension.js">
              <Link>Custom.Extension.js</Link>
              <InProject>True</InProject>
              <IgnoreDuringSdkGeneration>true</IgnoreDuringSdkGeneration>
            </Content>
            <Content Include="$(MsBuildThisFileDirectory)..\Extension.StoreHoursSample\Custom.Extension.js.map">
              <DependentUpon>Custom.Extension.js</DependentUpon>
              <Link>Custom.Extension.js.map</Link>
              <InProject>True</InProject>
              <IgnoreDuringSdkGeneration>true</IgnoreDuringSdkGeneration>
            </Content>
            
            <TypeScriptLibraries Include="$(MsBuildThisFileDirectory)..\Extension.StoreHoursSample\Custom.Extension.d.ts">
              <Visible>False</Visible>
            </TypeScriptLibraries>
    
    - Prepare View project
        - in Pos.View\Pos.View\Pos.ViewModels.csproj, add this at the appropriate places (this adds a reference to the extension project): 

            - in SharedApp\Pos.ts change the creation of the factory to:
            
                Commerce.Model.Managers.Factory = new Custom.Managers.ExtendedManagerFactory(Commerce.Config.retailServerUrl, Commerce.ViewModelAdapter.getCurrentAppLanguage());

    - do a global search for "BEGIN SDKSAMPLE_STOREHOURS" in the whole solution.
    - enable the code at all places you found and recompile
    - run Modern POS and verify by making an intentory lookup for any item, pick Houston, and click on store details
    - the new table with the store hours for Houston should be visible
    - if you did the offline mode changes, follow the steps in the RetailSdk Handbook to test the offline mode for this functionality
    

10. Extend Cloud POS
    - open solution at RetailSdk\POS\CloudPOS.sln, make sure it fully compiles and runs with F5 (see Retail Sdk handbook for details how to set this up), and make sure Modern POS can be run from Visual Studio using F5 (uncustomized, enable UAC, uninstall installed MPOS if needed)
    - include the RetailSdk\POS\Pos.Extension.StoreHoursSample.csproj project in the solution. Change the project build order to the extensions project gets built after Core and before Web projects
    - Prepare SharedApp virtual project (see ModernPOS)
    - Prepare View project (see ModernPOS)
    - do a global search for "BEGIN SDKSAMPLE_STOREHOURS" in the whole solution.
    - enable the code at all places you found and recompile
    - run it with F5 (and following the steps in the Retail Sdk handbook)
    - run Cloud POS and verify by making an intentory lookup for any item, pick Houston, and click on store details
    - the new table with the store hours for Houston should be visible

    
11. Official Deployment
    - add the channel DB change file to the database folder and register it in customization.settings
    - run msbuild for the whole RetailSdk
    - all packages will have all appropriate changes
    - deploy packages via LCS or manual