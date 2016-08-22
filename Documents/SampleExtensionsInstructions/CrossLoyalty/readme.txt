Sample overview:
Consider that there are 2 retailers, AdventureWorks and Contoso. As a part of a deal, Contoso retailer will accept loyalty points of AdventureWorks. 
The sample shows how to create a simple new CRT service and call it once a button in MPOS is clicked. It simulates the cross loyalty scenario. 

Changes are in AX configuration, CRT, RetailServer, 
RetailProxy and Point of Sale (both Modern POS and Cloud POS). Offline mode for Modern POS is supported for this sample.

Setup steps:

0. It is advised that you do these changes on a untouched RetailSdk. Ideally, you would have it under source control (VSO, or similar) and no files are changed so far. This is ideal, as you could revert at any steps
   without much work.
   
   
1. AX configuration changes: 
    - Create a new POS operation in AX (Retail and Commerce > Channel setup > POS setup > POS > Operations), Name: "AddCrossLoyaltyCard", Id: 1060, Check User access: True
    - Select POS screen layout (Retail and Commerce > Channel setup > POS setup > POS > Screen layouts), pick "FABMGR16:9"
    - Expand Button grids and select "Actions" grid
    - Click on Designer link in Button Grids section, hit Open, Run, login with your credentials
    - Click on the gift card button (row 4, column 1)
    - Click in the empty space, right click, new button, right click, Button Properties, select AddCrossLoyaltyCard action, and change the button size to 2 columns, 1 row
    - Close the designer
    - Run sync job 1090 (which includes layouts) by going to Retail and Commerce > Retail IT > Distribution schedule, pick 1090, Houston, and Run now
    - Verify in download sessions (Retail and commerce > Inquiries and reports > Commerce Data Exchange > Download sessions) that the 1090 succeeded (Status = Applied)
    
Note: The CRT and RetailServer code changes are all part of the RetailSdk\SampleExtensions. Therefore the steps below refer to how to build, deploy and test these. 

2. Enable and test CrossLoyalty CRT sample code:
    - open solution at RetailSdk\SampleExtensions\CommerceRuntime\CommerceRuntime.sln
    - Register the CRT change in commerceruntime.config:
    <add source="type" value="Contoso.Commerce.Runtime.Sample.CrossLoyalty.CrossLoyaltyCardServer, Contoso.Commerce.Runtime.Sample" />
    - add the new CRT extension dll to customization.settings (@(ISV_CommerceRuntime_CustomizableFile))
    - Run CRT test host project (Runtime.Extensions.TestHost.csproj) in debugger and execute code for CrossLoyalty sample

3. Enable and test CrossLoyalty RetailServer sample code:
    - open project at RetailSdk\SampleExtensions\RetailServer\RetailServer.Extensions.RetailServerSample.csproj and compile it
    - use inetmgr to find the location of the local Retail Server bin folder
    -  register in web.config file
      <add source="assembly" value="Contoso.RetailServer.Sample" />
    - add the new RS extension dll to customization.settings (@(ISV_RetailServer_CustomizableFile)
    - change customization.settings to use the new RetailServer dll for proxy generation ($(RetailServerLibraryPathForProxyGeneration))
    - in order to regenerate the proxy code build the Proxies folder from command line (msbuild /t:Rebuild)
      Note: you need to implement the interface method ICustomerManager.GetCrossLoyaltyCardDiscountAction in the CustomerManager class. For Now, just add this:
	  
		public Task<decimal> GetCrossLoyaltyCardDiscountAction(string loyaltyCardNumber)
		{
			throw new NotImplementedException();
		}
	
	  We can do this as we do not need this implementation until we implement ModernPos offline.
	  
    - copy the same commerceRuntime.config file from Crt test host into the RetailServer's bin folder
    - drop both the CRT and RS sample dlls into the RetailServer bin folder (or use the RetailSdk's AfterBuildDropBinariesToRetailServer target for rapid development)
    - Use inetmgr to browse to the RetailServer's $metadata, i.e. at https://usnconeboxax1ret.cloud.onebox.dynamics.com/RetailServer/Commerce/$metadata and verify that the CrossLoyalty activity is exposed by it

4. Implement the proxy code for offline mode (equivalent to RetailServer controller but for local CommerceRuntime when the client is not connected)
    - open RetailSDK\Proxies\RetailProxy\Proxies.RetailProxy.csproj, add RetailSDK\SampleExtensions\RetailServer\Extensions.CrossLoyaltySample\RetailServer.Extensions.CrossLoyaltySample.csproj
      to the solution and add a project reference from RetailProxy to CrossLoyaltySample project
    - Implement ICustomerManager.GetCrossLoyaltyCardDiscountAction in the CustomerManager class correctly now:
    
		public Task<decimal> GetCrossLoyaltyCardDiscountAction(string loyaltyCardNumber)
		{
			return Task.Run(() => CommerceRuntimeManager.Runtime.Execute<GetCrossLoyaltyCardResponse>(new GetCrossLoyaltyCardRequest(loyaltyCardNumber), null).Discount);
		}
        
    - add the service dll to RetailSDK\Assets\CommerceRuntime.MPOSOffline.config (similarly as you did for the commerceRuntime.config file)
        <add source="type" value="Contoso.Commerce.Runtime.CrossLoyaltySample.CrossLoyaltyCardService, CrossLoyalty.Commerce.Runtime.CrossLoyaltySample" />
    - update RetailSDK\Assets\dllhost.exe.config so the ClientBroker will load our RetailProxy assembly
        <add key="RetailProxyAssemblyName" value="Contoso.Commerce.RetailProxy"/> (Update to the correct assembly name)
        <add key="AdaptorCallerFullTypeName" value ="Contoso.Commerce.RetailProxy.Adapters.AdaptorCaller"/>
    
	
5. Use the RetailServer test client to verify that calling the new functionality succeeds. 
    - open project at RetailSdk\SampleExtensions\RetailServer\Extensions.TestClient, compile and run it
    - Enter the RetailServer url in the text box next to the "Activate New" button and hit it.
    - Enter device and register ids and hit Activate.
    - Enter the AAD credentials that has the registration priviledges and hit Ok.
    - Wait a few seconds.
    - Test client should now show what device is registered.
    - Hit login button and login with worker credentials.
    - Hit Custom  button. This will call the new functionality.
    Notes:
    - To see a console with errors/logs, use the "Debug" button.

6. Extend and test ModernPOS
    - open solution at RetailSdk\POS\ModernPOS.sln, make sure it fully compiles, and make sure Modern POS can be run from Visual Studio using F5 (uncustomized, enable UAC, uninstall installed MPOS if needed)
    - Prepare SharedApp virtual project
        - in SharedApp\Pos.SharedApp.projitems, add these at the appropriate places: 
        
			<Content Include="$(MsBuildThisFileDirectory)..\Extension.CrossLoyaltySample\Custom.Extension.js">
			  <Link>Custom.Extension.js</Link>
			  <InProject>True</InProject>
			  <IgnoreDuringSdkGeneration>true</IgnoreDuringSdkGeneration>
			</Content>
			<Content Include="$(MsBuildThisFileDirectory)..\Extension.CrossLoyaltySample\Custom.Extension.js.map">
			  <DependentUpon>Custom.Extension.js</DependentUpon>
			  <Link>Custom.Extension.js.map</Link>
			  <InProject>True</InProject>
			  <IgnoreDuringSdkGeneration>true</IgnoreDuringSdkGeneration>
			</Content>
	
			<TypeScriptCompile Include="$(MsBuildThisFileDirectory)Core\Activities\GetCrossLoyaltyCardNumberActivityImpl.ts">
			  <Link>Core\Activities\GetCrossLoyaltyCardNumberActivityImpl.ts</Link>
			  <InProject>True</InProject>
			</TypeScriptCompile>

			<TypeScriptLibraries Include="$(MsBuildThisFileDirectory)..\Extension.CrossLoyaltySample\Custom.Extension.d.ts">
			  <Visible>False</Visible>
			</TypeScriptLibraries>

            - copy GetCrossLoyaltyCardNumberActivityImpl.ts from the Instructions folder to the SharedApp\Core\Activities folder
            - modify SharedApp\Views\Cart\Cartview.ts
                - add a new include: "///<reference path='../../Custom.Extension.d.ts'/>"
                - in operationsButtonGridClick function, add this new case:
                
                    case Custom.Entities.RetailOperationEx.AddCrossLoyaltyCard:
                        var addCrossLoyaltyCardOperationParameters: string[] = actionProperty.split(";");
                        var addCrossLoyaltyCardOperationOptions: Custom.Operations.IAddCrossLoyaltyCardOperationOptions = {
                            cardNumber: addCrossLoyaltyCardOperationParameters.shift()
                        };

                        Commerce.Operations.OperationsManager.instance.runOperation(operationId, addCrossLoyaltyCardOperationOptions)
                            .done((result: Operations.IOperationResult) => {
                                if (!result.canceled) {
                                    this.cartViewModel.cart(Session.instance.cart);
                                }
                            }).fail((errors: Proxy.Entities.Error[]) => { NotificationHandler.displayClientErrors(errors); });
                        return true;

            - in SharedApp\Pos.ts change the creation of the factory to:
            
                Commerce.Model.Managers.Factory = new Custom.Managers.ExtendedManagerFactory(Commerce.Config.retailServerUrl, Commerce.ViewModelAdapter.getCurrentAppLanguage());
    
    - ModernPOS: add POS.Extensions project
        - include the RetailSdk\POS\Pos.Extension.CrossloyaltySample.csproj project in the solution. Change the project build order to the extensions project gets built after Core and before App projects
        - inspect the newly added project and its content
        - in Pos.App\pos.html, add "<script src="Custom.Extension.js" defer="defer"></script>" before the script reference to Pos.js
	- run ModernPOS in the debugger and test the functionality
    - if you did the offline mode changes, follow the steps in the RetailSdk Handbook to test the offline mode for this functionality

7. Extend Cloud POS
    - CloudPOS: add POS.Extensions project
        - Open CloudPos.sln in Visual Studio and include the Pos.Extension.CrossloyaltySample.csproj project to it. Change the project build order to the extensions project gets built after Core and before Web projects
        - in Pos.Web\pos.html, add "<script src="Custom.Extension.js" defer="defer"></script>" before the script reference to Pos.js

    - Rebuild solution(s)
    - run it with F5 (and following the steps in the Retail Sdk handbook)
    - Verify functionality (add item to cart, gift cards, add cross loyalty, type 425-999-4444, should return a value of $4)
    
8. Official Deployment
    - run msbuild for the whole RetailSdk
    - all packages will have all appropriate changes
    - deploy packages via LCS or manual
    