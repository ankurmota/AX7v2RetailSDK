Sample overview:
The sample shows how to expand an existing CRT service with additional functionality. In this case, some other system’s health could be checked as part of the already existing RunHealthCheckServiceRequest.

Changes are in CRT and CRT Test Host.

Setup steps:

0. It is advised that you do these changes on a untouched RetailSdk. Ideally, you would have it under source control (VSO, or similar) and no files are changed so far. This is ideal, as you could revert at any steps without much work.
   
   
    
Note: The CRT code changes are all part of the RetailSdk\SampleExtensions. Therefore the steps below refer to how to build, deploy and test these. 

1. Enable and test CRT sample code:
    - open solution at RetailSdk\SampleExtensions\CommerceRuntime\CommerceRuntime.sln
    - Register the CRT change in commerceruntime.config.
    - add the new CRT extension dll to customization.settings (@(ISV_CommerceRuntime_CustomizableFile))
	- uncomment the related code in CRT test host for the HealthCheck sample
    - Run CRT test host project (Runtime.Extensions.TestHost.csproj) in debugger and execute the code
