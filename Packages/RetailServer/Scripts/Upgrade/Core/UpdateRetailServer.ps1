<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

<#
.SYNOPSIS
    Allows a user to update Retail Server deployment.

.DESCRIPTION
    This script provides a mechanism to update existing Retail Server deployment on a local system.

.PARAMETER RETAILSERVERWEBSITENAME
    The name of Retail Server website which is deployed on the local system.

.PARAMETER MINSUPPORTEDVERSION
    An optional parameter to specify the minimum supported build version for Retail Server service. If the current installed version is less than this then the script will not support update.

.PARAMETER LOGPATH
    Location where all the logs will be stored.

.EXAMPLE 
    # Update the existing Retail Server deployment with minimum supported version "7.0.0.0". 
    .\UpdateRetailServer.ps1 -MinSupportedVersion "7.0.0.0"
#>

param (
	$RetailServerWebSiteName = 'RetailServer',
	$MinSupportedVersion
)

$ErrorActionPreference = 'Stop'

function Run-DatabaseUpgrade(
	$ScriptDir = $(Throw 'ScriptDir parameter required'),
	$server = $(Throw 'server parameter required'),
	$database = $(Throw 'database parameter required'),
    $SqlUserName = $(Throw 'SqlUserName parameter required'),
	$SqlUserPassword = $(Throw 'SqlUserPassword parameter required'))
{
	# Get channel database settings and topology xml files.
	# TODO Update the database deployment scripts to support sql azure database
	$dbScriptDirPath = Join-Path (Split-Path $ScriptDir -Parent) 'Database'
	$dbSettingsXmlFileName = Join-Path $dbScriptDirPath 'channeldb-settings.xml'
	$dbTopologyXmlFileName = Join-Path $dbScriptDirPath 'channeldb-topology.xml'
	$dbUpdatedSettingsXmlFileName = Join-Path $dbScriptDirPath 'channeldb-settings-updated.xml'
	
	Write-Log "Trying to upgrade Retail Channel database." -logFile $logFile

    Invoke-Script -scriptBlock `
    {
        & (Join-Path $dbScriptDirPath 'Setup-SettingsForDatabaseDeployment.ps1') `
                                -channelDatabaseServerName $server `
                                -channelDatabaseName $database `
                                -SqlUserName $SqlUserName `
                                -SettingsXmlFilePathInput $dbSettingsXmlFileName `
                                -SettingsXmlFilePathOutput $dbUpdatedSettingsXmlFileName
    }

    $credentials = New-Object System.Management.Automation.PSCredential($SqlUserName, (ConvertTo-SecureString $SqlUserPassword -AsPlainText -Force))

    Invoke-Script -scriptBlock `
    {
        & (Join-Path $dbScriptDirPath 'Deploy-Databases.ps1') `
                                -TopologyXmlFilePath $dbTopologyXmlFileName `
                                -SettingsXmlFilePath $dbUpdatedSettingsXmlFileName `
                                -Credentials $credentials `
                                -Verbose $True 
    }

    Write-Log "Finished upgrading Retail Channel database." -logFile $logFile
}

function Update-CrtRealtimeServiceThumbprint(
    [string]$sourceCrtConfigPath = $(throw 'sourceCrtConfigPath is required'),
    [string]$targetCrtConfigPath = $(throw 'targetCrtConfigPath is required'))
{
    $certStoreName = 'My'
    $certStoreLocation = 'LocalMachine'
    if((Test-Path $sourceCrtConfigPath) -and (Test-Path $targetCrtConfigPath))
    {
        [xml]$sourceCrtConfigXml = Get-Content $sourceCrtConfigPath
        [xml]$targetCrtConfigXml = Get-Content $targetCrtConfigPath
        
        $sourceCertificateNode = $sourceCrtConfigXml.SelectSingleNode("//commerceRuntime/realtimeService/certificate[@storeName=`'$certStoreName`' and @storeLocation=`'$certStoreLocation`']")
        $targetCertificateNode = $targetCrtConfigXml.SelectSingleNode("//commerceRuntime/realtimeService/certificate[@storeName=`'$certStoreName`' and @storeLocation=`'$certStoreLocation`']")
        
        if($sourceCertificateNode -and $targetCertificateNode)
        {
            $targetCertificateNode.SetAttribute('thumbprint', $sourceCertificateNode.thumbprint)
        }
    
        Set-ItemProperty $targetCrtConfigPath -name IsReadOnly -value $false
        $targetCrtConfigXml.Save($targetCrtConfigPath)
    }
    else
    {
        throw ("Error: Either {0} or {1} doesn't exist!" -f $sourceCrtConfigPath, $targetCrtConfigPath)
    }
}

function Upgrade-RetailServer(
    [string] $webSiteName = $(throw 'webSiteName is required'),
    [string] $webConfigPath = $(throw 'webConfigPath is required'),
    [string] $webSitePhysicalPath = $(throw 'webSitePhysicalPath is required'),
	[string] $scriptDir = $(throw 'scriptDir is required'),
    [bool] $isPackageDelta,
    [ValidateNotNullOrEmpty()]
    [string] $installationInfoXmlPath = $(throw 'installationInfoXmlPath is required'))
{
    Log-TimedMessage 'Begin updating Retail Server deployment...'

    $webConfigFileName = 'web.config'
	try
	{
		# Decrypt web.config connection strings section.
		Log-TimedMessage 'Decrypt connectionStrings section in web.config'
		$webSiteId = Get-WebSiteId -webSiteName $webSiteName	
		aspnet_regiis -pd "connectionStrings" -app "/" -Site $webSiteId

		if(!$isPackageDelta)
		{
			# Run channel database upgrade
			Log-TimedMessage 'Upgrade Retail Channel database'
			Upgrade-RetailChannelDatabase -scriptDir $scriptDir
		}

		# Read cert thumbprint before update
		Log-TimedMessage 'Retrieve Retail Server SSL certificate thumbprint from web.config file.'
		$certThumbprint = Get-RetailServerAuthCertThumbPrintFromWebConfig -webConfigPath $webConfigPath
		Log-TimedMessage ('Found Retail Server SSL certificate thumbprint - {0}' -f $certThumbprint)

		# Create a temp working folder
		$tempWorkingFolder = Join-Path $env:temp ("{0}_Temp_{1}" -f $webSiteName, $(Get-Date -f yyyy-MM-dd_hh-mm-ss))

		# Get the service model Code folder from the update package
		# If Code folder does not exist or is empty, skip the remaining update process.
		Log-TimedMessage 'Getting the Code folder from the deployable update package.'
		$updatePackageCodeDir = (Join-Path (Split-Path (Split-Path (Split-Path $ScriptDir -Parent) -Parent) -Parent) 'Code')
		
		if(Check-IfAnyFilesExistInFolder -folderPath $updatePackageCodeDir)
		{

			Log-TimedMessage ('Found the Code folder from the deployable update package at - {0}.' -f $updatePackageCodeDir)

			# Get list of all files to be updated
			Log-TimedMessage 'Getting the list of all files to be updated.'
			$fileList = Get-ListOfFilesToCopy -isPackageDelta $isPackageDelta `
											  -installationInfoXmlPath $installationInfoXmlPath `
											  -updatePackageCodeFolder $updatePackageCodeDir

			# Copy all the update files to a temp location
			$fileList | % { 
				Copy-Files -SourceDirPath $updatePackageCodeDir `
						   -DestinationDirPath $tempWorkingFolder `
						   -FilesToCopy $_ `
						   -RobocopyOptions '/S /njs /ndl /np /njh'
			}

			# Migrate settings from web.config file
			Log-TimedMessage 'Checking if web.config file needs to be merged.'
			if($fileList -contains $webConfigFileName)
			{
				Log-TimedMessage 'Yes.'
				Merge-WebConfig -tempWorkingFolder $tempWorkingFolder -webConfigPath $webConfigPath
			}

			# Migrate the Real-time Service thumbprint in commerceruntime config file
			Log-TimedMessage 'Retrieving commerce runtime config file name from web.config.'
			$crtConfigFileName = Get-CrtConfigFileNameFromWebConfig -webConfigPath $webConfigPath
			Log-TimedMessage ('Found commerce runtime config file name from web.config as - {0}' -f $crtConfigFileName)

			if($fileList -contains $crtConfigFileName)
			{
				Log-TimedMessage 'Updating Real-time Service SSL thumbprint in the commerce runtime config file.'   
				Update-CrtRealtimeServiceThumbprint -sourceCrtConfigPath (Join-Path (Join-Path $webSitePhysicalPath 'bin') $crtConfigFileName) `
													 -targetCrtConfigPath (Join-Path (Join-Path $tempWorkingFolder 'bin') $crtConfigFileName)
				Log-TimedMessage 'Finished updating Real-time Service SSL thumbprint in the commerce runtime config file.' 
			}

			# Encrypt web.config connection strings before taking a backup
			aspnet_regiis -pe "connectionStrings" -app "/" -Site $webSiteId  

			# Replace website files from temp working directory to actual working directory
			Replace-WebsiteFiles -webSiteName $webSiteName -newWebFilesPath $tempWorkingFolder

			# Update the Retail Server authentication cert thumbprint
			Update-RetailServerAuthenticationKeys -scriptDir $ScriptDir `
												  -retailServerDeploymentPath $webSitePhysicalPath `
												  -retailServerAuthCertThumbprint $certThumbprint
		}
	}
	finally
	{
		# Encrypt back web.config connection strings section.
		Log-TimedMessage 'Encrypt connectionStrings section in web.config'
		aspnet_regiis -pe "connectionStrings" -app "/" -Site $webSiteId
		
		# Remove the temp working folder
		if($tempWorkingFolder -and (Test-Path -Path $tempWorkingFolder))
		{
			Log-TimedMessage ('Removing temporary working directory {0}' -f $tempWorkingFolder)
			Remove-Item $tempWorkingFolder -Recurse -Force -ErrorAction SilentlyContinue
		}
	}

    Log-TimedMessage 'Finished updating Retail Server deployment...' 
}

function Update-RetailServerAuthenticationKeys(
    [string]$scriptDir = $(throw 'scriptDir is required'),
    [string]$retailServerDeploymentPath = $(throw 'retailServerDeploymentPath is required'),
    $retailServerAuthCertThumbprint = $(throw 'retailServerAuthCertThumbprint is required'))
{
    $deploymentScriptsDir = (Split-Path (Split-Path $ScriptDir -Parent) -Parent)
    $updateRetailServerAuthenticationKeysScriptPath = Join-Path $deploymentScriptsDir 'UpdateRetailServerAuthenticationKeys.ps1'

    if(-not (Test-Path $updateRetailServerAuthenticationKeysScriptPath))
    {
        throw "Cannot find script $updateRetailServerAuthenticationKeysScriptPath."
    }
    
    Invoke-Script -scriptBlock `
    {
        & $updateRetailServerAuthenticationKeysScriptPath -RetailServerDeploymentPath $retailServerDeploymentPath `
                                                          -CertificateThumbprint $retailServerAuthCertThumbprint
    }
}

function Get-RetailServerAuthCertThumbPrintFromWebConfig([string]$webConfigPath = $(throw 'webConfigPath is required'))
{
    [xml]$webConfigXml = Get-Content $webConfigPath
    $retailServerAuthCertThumbprint = $webConfigXml.configuration.retailServer.authentication.CertThumbprint

    if(-not $retailServerAuthCertThumbprint)
    {
        throw "Could not find Retail Server authentication thumbprint in web.config file at: $webConfigPath"
    }

    return $retailServerAuthCertThumbprint
}

function Upgrade-RetailChannelDatabase(
    [string]$scriptDir = $(throw 'scriptDir is required'))
{
    $websiteConnectionStringSettings = (Get-ChannelDbServicingDataFromRegistry)

    if($websiteConnectionStringSettings)
    {
        foreach($websiteConnectionStringSetting in $websiteConnectionStringSettings)
        {
            $dbSetting = Create-WebSiteDBConfiguration -connectionString $websiteConnectionStringSetting

            # Upgrade the database
            Run-DatabaseUpgrade -ScriptDir $scriptDir -server $dbSetting.server -database $dbSetting.database -SqlUserName $dbSetting.sqlUserName -SqlUserPassword $dbSetting.sqlUserPassword
        }
    }
    else
    {
        throw 'Failed to locate channel database servicing information. In-place update or deployment of customizations of Dynamics AX on a locally deployed VHD is not supported at this time. You may use an LCS deployed developer topology for development scenarios.'
    }
}

function Get-CrtConfigFileNameFromWebConfig([string]$webConfigPath = $(throw 'webConfigPath is required'))
{
    [xml]$webConfigXml = Get-Content $webConfigPath
    $crtConfigFile = $webConfigXml.configuration.commerceRuntime.configSource

    $crtConfigFileName = Split-Path $crtConfigFile -Leaf

    return $crtConfigFileName
}

function Retain-EnvironmentInfoFromWebConfigSettings(
    [ValidateNotNullOrEmpty()]
    [string]$sourceWebConfigFilePath = $(throw 'sourceWebConfigFilePath is required'),

    [ValidateNotNullOrEmpty()]
    [string]$targetWebConfigFilePath = $(throw 'targetWebConfigFilePath is required'))
{

    if(!((Test-Path -Path $sourceWebConfigFilePath) -and (Test-Path -Path $targetWebConfigFilePath)))
    {
        throw "Either $sourceWebConfigFilePath or $targetWebConfigFilePath doesn't exist"
    }

    [xml]$sourceWebConfigFilePathDoc = Get-Content $sourceWebConfigFilePath
    [xml]$targetWebConfigFilePathDoc = Get-Content $targetWebConfigFilePath
    
    # Read the machineKey element under //configuration/environment. 
    # If it exists, then retain it in the new web.config file.
    Log-ActionItem 'Check if node //configuration/environment exists in the source web.config file'
    $sourceMachineKeyNode = $sourceWebConfigFilePathDoc.SelectSingleNode('//configuration/environment')
    $targetMachineKeyNode = $targetWebConfigFilePathDoc.SelectSingleNode('//configuration/environment')
        
    if($sourceMachineKeyNode)
    {
        Log-ActionResult 'Yes. Retain this value in the target web.config'
		# need to use deep copy here since there will be some sub element under machineKey once we encrypt this section
        $importedNode = $targetWebConfigFilePathDoc.ImportNode($sourceMachineKeyNode, $true)

        if(!$targetMachineKeyNode)
        {            
            $targetWebConfigFilePathDoc.configuration.AppendChild($importedNode)                
        }
        else
        {
            $targetWebConfigFilePathDoc.configuration.ReplaceChild($importedNode, $targetMachineKeyNode)
        }
        
        Set-ItemProperty $targetWebConfigFilePath -name IsReadOnly -value $false
		$targetWebConfigFilePathDoc.Save($targetWebConfigFilePath)
    }
    else
    {
        Log-ActionResult 'No. Skip this step'
    }
}

function Retain-MachineKeyFromWebConfigSettings(
    [ValidateNotNullOrEmpty()]
	[string]$sourceWebConfigFilePath = $(throw 'sourceWebConfigFilePath is required'),

    [ValidateNotNullOrEmpty()]
	[string]$targetWebConfigFilePath = $(throw 'targetWebConfigFilePath is required'))
{

	if(!((Test-Path -Path $sourceWebConfigFilePath) -and (Test-Path -Path $targetWebConfigFilePath)))
	{
        throw "Either $sourceWebConfigFilePath or $targetWebConfigFilePath doesn't exist"
    }
	
    [xml]$sourceWebConfigFilePathDoc = Get-Content $sourceWebConfigFilePath
	[xml]$targetWebConfigFilePathDoc = Get-Content $targetWebConfigFilePath
		
    # Read the machineKey element under //configuration/system.web. 
    # If it exists, then retain it in the new web.config file.
    Log-ActionItem 'Check if node //configuration/system.web/machineKey exists in the source web.config file'
    $sourceMachineKeyNode = $sourceWebConfigFilePathDoc.SelectSingleNode('//configuration/system.web/machineKey')
    $targetMachineKeyNode = $targetWebConfigFilePathDoc.SelectSingleNode('//configuration/system.web/machineKey')
        
    if($sourceMachineKeyNode)
    {
        Log-ActionResult 'Yes. Retain this value in the target web.config'
		# need to use deep copy here since there will be some sub element under machineKey once we encrypt this section
        $importedNode = $targetWebConfigFilePathDoc.ImportNode($sourceMachineKeyNode, $true)

        if(!$targetMachineKeyNode)
        {            
            $targetWebConfigFilePathDoc.configuration.'system.web'.AppendChild($importedNode)                
        }
        else
        {
            $targetWebConfigFilePathDoc.configuration.'system.web'.ReplaceChild($importedNode, $targetMachineKeyNode)
        }
        
        Set-ItemProperty $targetWebConfigFilePath -name IsReadOnly -value $false
		$targetWebConfigFilePathDoc.Save($targetWebConfigFilePath)
    }
    else
    {
        Log-ActionResult 'No. Skip this step'
    }
}

function Retain-CustomSettings(
    [ValidateNotNullOrEmpty()]
	[string]$sourceWebConfigFilePath = $(throw 'sourceWebConfigFilePath is required'),

    [ValidateNotNullOrEmpty()]
	[string]$targetWebConfigFilePath = $(throw 'targetWebConfigFilePath is required'))
{

	if(!((Test-Path -Path $sourceWebConfigFilePath) -and (Test-Path -Path $targetWebConfigFilePath)))
	{
        throw "Either $sourceWebConfigFilePath or $targetWebConfigFilePath doesn't exist"
    }
	
    [xml]$sourceWebConfigFilePathDoc = Get-Content $sourceWebConfigFilePath
	[xml]$targetWebConfigFilePathDoc = Get-Content $targetWebConfigFilePath
	
    Log-ActionItem 'Check if Retail Server cryptography certificate thumbprint exists in the source web.config file'
    $sourceCryptographyCertThumbprint = $sourceWebConfigFilePathDoc.configuration.retailServer.cryptography.certificateThumbprint
        
    if($sourceCryptographyCertThumbprint)
    {
        Log-ActionResult 'Yes. Retain this value in the target web.config'
        $targetWebConfigFilePathDoc.configuration.retailServer.cryptography.certificateThumbprint = $sourceCryptographyCertThumbprint
    }
    else
    {
        Log-ActionResult 'No. Skip this step'
    }

    Log-ActionResult 'Finished retaining Retail Server cryptography certificate thumbprint in target web.config'
	
	Log-ActionItem 'Check if Retail Server device activation allowed identity providers exists in the source web.config file'
    $sourceDeviceActivation = $sourceWebConfigFilePathDoc.configuration.retailServer.deviceActivation.allowedIdentityProviders
        
    if($sourceDeviceActivation)
    {
        Log-ActionResult 'Yes. Retain this value in the target web.config'
        $targetWebConfigFilePathDoc.configuration.retailServer.deviceActivation.allowedIdentityProviders = $sourceDeviceActivation
    }
    else
    {
        Log-ActionResult 'No. Skip this step'
    }
	
    Log-ActionResult 'Finished retaining Retail Server device activation allowed identity providers in target web.config'
	
	Set-ItemProperty $targetWebConfigFilePath -name IsReadOnly -value $false
	$targetWebConfigFilePathDoc.Save($targetWebConfigFilePath)
}

function Get-NonCustomizableAppSettings()
{
    $nonCustomizableAppSettings = @(
    'AADObjectIdClaimName',
    'AADTenantIdClaimName',
    'AADTokenIssuerPrefix',
    'AdminPrincipalName',
    'AllowedOrigins',
    'AosAudienceURN',
    'AxDbName',
    'AxDbServer',
    'AxDbSqlPwd',
    'AxDbSqlUser',
    'dbName',
    'dbPassword',
    'dbServer',
    'dbUser',
    'FederationMetadataAddress',
    'isConnectionStringOverridden',
    'retailCloudPOSUrl',
    'RetailRTSAuthenticationCertificateThumbprint',
    'RetailServerPackageMetadata',
    'retailStorefrontUrl',
    'serviceUri',
	'IsAnonymousEnabled',
    'WebSiteName')
    
    return $nonCustomizableAppSettings 
}

function Merge-WebConfig(
    [string]$tempWorkingFolder = $(throw 'tempWorkingFolder is required'),
    [string]$webConfigPath = $(throw 'webConfigPath is required'))
{
    $tempWebConfigPath = Join-Path $tempWorkingFolder (Split-Path $webConfigPath -Leaf)

    # Merge the connection strings
    Log-TimedMessage 'Merging connection strings section in web.config file.'
    $websiteConnectionStringSettings = Extract-ConnectionStringsFromWebConfig -webConfigPath $webConfigPath
    Update-WebsiteConnectionStringSettings -webConfigPath $tempWebConfigPath -connectionStringsXml $websiteConnectionStringSettings
    Log-TimedMessage 'Finished merging connection strings section in web.config file.'       

    # Merge the app settings
    Log-TimedMessage 'Merging app settings section in web.config file.'
       
    [array]$nonCustomizableAppSettings = Get-NonCustomizableAppSettings
    Merge-WebConfigAppSettings -sourceWebConfigFilePath $webConfigPath `
                                -targetWebConfigFilePath $tempWebConfigPath `
                                -nonCustomizableAppSettings $nonCustomizableAppSettings

     Log-TimedMessage 'Finished merging app settings section in web.config file.'
     
    # Merge the environment key
    Retain-EnvironmentInfoFromWebConfigSettings -sourceWebConfigFilePath $webConfigPath `
                                                -targetWebConfigFilePath $tempWebConfigPath `

    # Merge the machine key
    Retain-MachineKeyFromWebConfigSettings -sourceWebConfigFilePath $webConfigPath `
                                           -targetWebConfigFilePath $tempWebConfigPath `

    # Retail the Retail Server cryptography certificate thumbprint
    Retain-CustomSettings -sourceWebConfigFilePath $webConfigPath `
                          -targetWebConfigFilePath $tempWebConfigPath `

    # Handle CTP8 to future releases upgrade
    Handle-CTP8Upgrade -manifestXmlPath $installationInfoFile -targetWebConfigFilePath $tempWebConfigPath
}

function Is-UpdateApplicable(
    [string]$installationInfoFile = $(throw 'installationInfoFile is required'),
    [string]$webConfigPath = $(throw 'webConfigPath is required'),
    [string]$updatePackageRootDir = $(throw 'updatePackageRootDir is required'))
{
    Log-ActionItem 'Check if update package is released by Microsoft and if the current deployment on the machine is customized ISV generated package'

    if((Check-IfUpdatePackageIsReleasedByMicrosoft -installationInfoXml $installationInfoFile) -and
       (Check-IfCurrentDeploymentOfComponentIsCustomized -componentName 'RetailServer' -updatePackageRootDir $updatePackageRootDir))
    {
        Log-ActionResult 'Yes'
        
        $crtConfigFileName = Get-CrtConfigFileNameFromWebConfig -webConfigPath $webConfigPath
        
        $crtConfigFilePath = Join-Path (Join-Path $updatePackageRootDir 'RetailServer\Code\bin') $crtConfigFileName

        Log-ActionItem ('Checking if commerce runtime config file [{0}] exists in the update package.' -f $crtConfigFilePath)

        if((Test-Path -Path $crtConfigFilePath))
        {
            Log-ActionResult 'Yes'
            Log-TimedMessage '#### WARNING: commerce runtime config file exists in the update package. Skipping the update. ####'
            return $false
        }
    }

    return $true
}

function Handle-CTP8Upgrade(
    [ValidateNotNullOrEmpty()]
	[string]$manifestXmlPath = $(throw 'manifestXmlPath is required'),

    [ValidateNotNullOrEmpty()]
	[string]$targetWebConfigFilePath = $(throw 'targetWebConfigFilePath is required')
)
{
    # Check if update web.config environment parameters are updated
    [xml]$configXml = Get-Content $targetWebConfigFilePath
    [xml]$manifestXml = Get-Content $manifestXmlPath

    Log-TimedMessage 'Updating EnvironmentId...'
    $envNode = $configXml.SelectSingleNode('//configuration/environment')
    if(!$envNode.id)
    {
        # Read from registry and set
        $envId = Get-LcsEnvironmentId

        Log-TimedMessage ('Setting EnvironmentId - {0}' -f $envId)
        $envNode.SetAttribute("id", $envId)
    }

    $instrumentationNode = $configXml.SelectSingleNode('//configuration/environment/instrumentation')

    Log-TimedMessage 'Updating clientAppInsightsKey...'
    if(!$instrumentationNode.clientAppInsightsKey)
    {
        # Read from manifest XML
        $manifestClientAppInsightsKey = $manifestXml.ServiceModelInstallationInfo.ClientAppInsightsKey
        if(!$manifestClientAppInsightsKey)
        {
           throw ('ClientAppInsightsKey is required for update. Please add it to the {0} file. E.g.: <ClientAppInsightsKey>[your_Key]</ClientAppInsightsKey>' -f $manifestXmlPath) 
        }
        
        Log-TimedMessage ('Setting clientAppInsightsKey - {0}' -f $manifestClientAppInsightsKey)
        $instrumentationNode.SetAttribute('clientAppInsightsKey', $manifestClientAppInsightsKey)
    }

    Log-TimedMessage 'Updating hardwareStationAppInsightsKey...'
    if(!$instrumentationNode.hardwareStationAppinsightsKey)
    {
        # Read from manifest XML
        $manifestHwsAppinsightsKey = $manifestXml.ServiceModelInstallationInfo.hardwareStationAppinsightsKey
        if(!$manifestHwsAppinsightsKey)
        {
            throw ('HardwareStationAppinsightsKey is required for update. Please add it to the {0} file. E.g.: <HardwareStationAppInsightsKey>[your_Key]</HardwareStationAppInsightsKey>' -f $manifestXmlPath)     
        }

        Log-TimedMessage ('Setting hardwareStationAppinsightsKey - {0}' -f $manifestHwsAppinsightsKey)
        $instrumentationNode.SetAttribute('hardwareStationAppinsightsKey', $manifestHwsAppinsightsKey)
    }

    Set-ItemProperty $targetWebConfigFilePath -name IsReadOnly -value $false
    $configXml.Save($targetWebConfigFilePath)
}

try
{	
	$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
    . (Join-Path $ScriptDir 'Common-Configuration.ps1')
    . (Join-Path $ScriptDir 'Common-Web.ps1')
    . (Join-Path $ScriptDir 'Common-Database.ps1')
	. (Join-Path $ScriptDir 'Common-Upgrade.ps1')

    # Get website physical path.
    Log-TimedMessage ('Getting website physical path for website - {0}' -f $RetailServerWebSiteName)
    $webSitePhysicalPath = Get-WebSitePhysicalPath -webSiteName $RetailServerWebSiteName
    Log-TimedMessage ('Found website physical path - {0}' -f $webSitePhysicalPath)
    
    # Get web.config path.
    Log-TimedMessage 'Getting web.config path.'
    $webConfigPath = Join-Path $webSitePhysicalPath 'web.config'
    Log-TimedMessage ('Found web.config path - {0}' -f $webConfigPath)

    # Get the installation info manifest file.
    Log-TimedMessage 'Getting installation info XML path.'
	$installationInfoFile = Get-InstallationInfoFilePath -scriptDir $ScriptDir
    Log-TimedMessage ('Found installation info XML path - {0}' -f $installationInfoFile)

    # Get the update package root folder
    Log-TimedMessage 'Getting the update package root folder from the deployable update package.'
    $updatePackageRootDir = (Split-Path (Split-Path (Split-Path (Split-Path $ScriptDir -Parent) -Parent) -Parent) -Parent)
    Log-TimedMessage ('Found update package root directory - {0}' -f $updatePackageRootDir)

    # We skip the update if following conditions are true:
    #    1. update package is released by Microsoft
    #    2. the current deployment on the machine is done via customized ISV generated package.
    #    3. the update package contains commerceruntime.config changes in it
    [bool]$isUpdateApplicable = Is-UpdateApplicable -installationInfoFile $installationInfoFile `
                                                    -webConfigPath $webConfigPath `
                                                    -updatePackageRootDir $updatePackageRootDir
	
    # Determine the package type.
    [bool]$isPackageDelta = Is-PackageDelta -installationInfoFile $installationInfoFile
    
    # Add an entry to the servicing settings file to skip/continue the ETW Manifest update
    $scriptsPath = Join-Path $updatePackageRootDir 'RetailServer\Scripts'
    $defaultSettingsFilePath = (Join-Path $scriptsPath 'DefaultServicing.settings')
    $updatedSettingsFilePath = (Join-Path $scriptsPath 'Servicing.settings')
    
	if(Test-Path -Path $defaultSettingsFilePath)
	{
		Copy-Item -Path $defaultSettingsFilePath -Destination $updatedSettingsFilePath -Force
	}
	else
	{
		New-Item -Path $updatedSettingsFilePath -ItemType File -Force
	}
    
    Log-TimedMessage ('Updating the setting for ETW Manifest update in {0}' -f $updatedSettingsFilePath)
    CreateOrUpdate-ServicingStepSetting -settingsFilePath $updatedSettingsFilePath -runbookStepName 'ETWManifestUpdate' -propertyName 'Skip' -value (!$isUpdateApplicable)
    Log-TimedMessage ('Finished updating the setting for ETW Manifest update in {0}' -f $updatedSettingsFilePath)

    # Rename the manifest installation info file if:
    #    1. Package is of type delta
    #    2. If update is not applicable.
	# For each update, the installer updates the deployment version of the machine with values in the installation info file.
    # Renaming this file for above cases will not update the deployment version on the machine.
    if(!$isUpdateApplicable -or $isPackageDelta)
    {
       $installationInfoFile = Rename-InstallationInfoFile -filePath $installationInfoFile
    }

    # Skip the update if update unapplicable
    if(!$isUpdateApplicable)
    {
        exit 0
    }

    # Upgrade Retail Server
	Upgrade-RetailServer -webSiteName $retailServerWebSiteName `
                         -scriptDir $scriptDir `
                         -webConfigPath $webConfigPath `
                         -webSitePhysicalPath $webSitePhysicalPath `
                         -isPackageDelta $isPackageDelta `
                         -installationInfoXmlPath $installationInfoFile  
}
catch
{
    Log-Error ($global:error[0] | format-list * -f | Out-String)
    $ScriptLine = "{0}{1}" -f $MyInvocation.MyCommand.Path.ToString(), [System.Environment]::NewLine
    $PSBoundParameters.Keys | % { $ScriptLine += "Parameter: {0} Value: {1}{2}" -f $_.ToString(), $PSBoundParameters[$_.ToString()], [System.Environment]::NewLine}
    Write-Host ("Executed:{0}$ScriptLine{0}Exiting with error code $exitCode." -f [System.Environment]::NewLine)
    throw ($global:error[0] | format-list * -f | Out-String)
}
# SIG # Begin signature block
# MIIdtgYJKoZIhvcNAQcCoIIdpzCCHaMCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUhSZbbu7hbpxPHlHWbijygRz+
# fSOgghhkMIIEwzCCA6ugAwIBAgITMwAAAJzu/hRVqV01UAAAAAAAnDANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTYwMzMwMTkyMTMw
# WhcNMTcwNjMwMTkyMTMwWjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OjU4NDctRjc2MS00RjcwMSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzCWGyX6IegSP
# ++SVT16lMsBpvrGtTUZZ0+2uLReVdcIwd3bT3UQH3dR9/wYxrSxJ/vzq0xTU3jz4
# zbfSbJKIPYuHCpM4f5a2tzu/nnkDrh+0eAHdNzsu7K96u4mJZTuIYjXlUTt3rilc
# LCYVmzgr0xu9s8G0Eq67vqDyuXuMbanyjuUSP9/bOHNm3FVbRdOcsKDbLfjOJxyf
# iJ67vyfbEc96bBVulRm/6FNvX57B6PN4wzCJRE0zihAsp0dEOoNxxpZ05T6JBuGB
# SyGFbN2aXCetF9s+9LR7OKPXMATgae+My0bFEsDy3sJ8z8nUVbuS2805OEV2+plV
# EVhsxCyJiQIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFD1fOIkoA1OIvleYxmn+9gVc
# lksuMB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBAFb2avJYCtNDBNG3nxss1ZqZEsphEErtXj+MVS/RHeO3TbsT
# CBRhr8sRayldNpxO7Dp95B/86/rwFG6S0ODh4svuwwEWX6hK4rvitPj6tUYO3dkv
# iWKRofIuh+JsWeXEIdr3z3cG/AhCurw47JP6PaXl/u16xqLa+uFLuSs7ct7sf4Og
# kz5u9lz3/0r5bJUWkepj3Beo0tMFfSuqXX2RZ3PDdY0fOS6LzqDybDVPh7PTtOwk
# QeorOkQC//yPm8gmyv6H4enX1R1RwM+0TGJdckqghwsUtjFMtnZrEvDG4VLA6rDO
# lI08byxadhQa6k9MFsTfubxQ4cLbGbuIWH5d6O4wggYHMIID76ADAgECAgphFmg0
# AAAAAAAcMA0GCSqGSIb3DQEBBQUAMF8xEzARBgoJkiaJk/IsZAEZFgNjb20xGTAX
# BgoJkiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1pY3Jvc29mdCBSb290
# IENlcnRpZmljYXRlIEF1dGhvcml0eTAeFw0wNzA0MDMxMjUzMDlaFw0yMTA0MDMx
# MzAzMDlaMHcxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYD
# VQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAf
# BgNVBAMTGE1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQTCCASIwDQYJKoZIhvcNAQEB
# BQADggEPADCCAQoCggEBAJ+hbLHf20iSKnxrLhnhveLjxZlRI1Ctzt0YTiQP7tGn
# 0UytdDAgEesH1VSVFUmUG0KSrphcMCbaAGvoe73siQcP9w4EmPCJzB/LMySHnfL0
# Zxws/HvniB3q506jocEjU8qN+kXPCdBer9CwQgSi+aZsk2fXKNxGU7CG0OUoRi4n
# rIZPVVIM5AMs+2qQkDBuh/NZMJ36ftaXs+ghl3740hPzCLdTbVK0RZCfSABKR2YR
# JylmqJfk0waBSqL5hKcRRxQJgp+E7VV4/gGaHVAIhQAQMEbtt94jRrvELVSfrx54
# QTF3zJvfO4OToWECtR0Nsfz3m7IBziJLVP/5BcPCIAsCAwEAAaOCAaswggGnMA8G
# A1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFCM0+NlSRnAK7UD7dvuzK7DDNbMPMAsG
# A1UdDwQEAwIBhjAQBgkrBgEEAYI3FQEEAwIBADCBmAYDVR0jBIGQMIGNgBQOrIJg
# QFYnl+UlE/wq4QpTlVnkpKFjpGEwXzETMBEGCgmSJomT8ixkARkWA2NvbTEZMBcG
# CgmSJomT8ixkARkWCW1pY3Jvc29mdDEtMCsGA1UEAxMkTWljcm9zb2Z0IFJvb3Qg
# Q2VydGlmaWNhdGUgQXV0aG9yaXR5ghB5rRahSqClrUxzWPQHEy5lMFAGA1UdHwRJ
# MEcwRaBDoEGGP2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1
# Y3RzL21pY3Jvc29mdHJvb3RjZXJ0LmNybDBUBggrBgEFBQcBAQRIMEYwRAYIKwYB
# BQUHMAKGOGh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljcm9z
# b2Z0Um9vdENlcnQuY3J0MBMGA1UdJQQMMAoGCCsGAQUFBwMIMA0GCSqGSIb3DQEB
# BQUAA4ICAQAQl4rDXANENt3ptK132855UU0BsS50cVttDBOrzr57j7gu1BKijG1i
# uFcCy04gE1CZ3XpA4le7r1iaHOEdAYasu3jyi9DsOwHu4r6PCgXIjUji8FMV3U+r
# kuTnjWrVgMHmlPIGL4UD6ZEqJCJw+/b85HiZLg33B+JwvBhOnY5rCnKVuKE5nGct
# xVEO6mJcPxaYiyA/4gcaMvnMMUp2MT0rcgvI6nA9/4UKE9/CCmGO8Ne4F+tOi3/F
# NSteo7/rvH0LQnvUU3Ih7jDKu3hlXFsBFwoUDtLaFJj1PLlmWLMtL+f5hYbMUVbo
# nXCUbKw5TNT2eb+qGHpiKe+imyk0BncaYsk9Hm0fgvALxyy7z0Oz5fnsfbXjpKh0
# NbhOxXEjEiZ2CzxSjHFaRkMUvLOzsE1nyJ9C/4B5IYCeFTBm6EISXhrIniIh0EPp
# K+m79EjMLNTYMoBMJipIJF9a6lbvpt6Znco6b72BJ3QGEe52Ib+bgsEnVLaxaj2J
# oXZhtG6hE6a/qkfwEm/9ijJssv7fUciMI8lmvZ0dhxJkAj0tr1mPuOQh5bWwymO0
# eFQF1EEuUKyUsKV4q7OglnUa2ZKHE3UiLzKoCG6gW4wlv6DvhMoh1useT8ma7kng
# 9wFlb4kLfchpyOZu6qeXzjEp/w7FW1zYTRuh2Povnj8uVRZryROj/TCCBhAwggP4
# oAMCAQICEzMAAABkR4SUhttBGTgAAAAAAGQwDQYJKoZIhvcNAQELBQAwfjELMAkG
# A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQx
# HjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEoMCYGA1UEAxMfTWljcm9z
# b2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMTAeFw0xNTEwMjgyMDMxNDZaFw0xNzAx
# MjgyMDMxNDZaMIGDMQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQ
# MA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9u
# MQ0wCwYDVQQLEwRNT1BSMR4wHAYDVQQDExVNaWNyb3NvZnQgQ29ycG9yYXRpb24w
# ggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCTLtrY5j6Y2RsPZF9NqFhN
# FDv3eoT8PBExOu+JwkotQaVIXd0Snu+rZig01X0qVXtMTYrywPGy01IVi7azCLiL
# UAvdf/tqCaDcZwTE8d+8dRggQL54LJlW3e71Lt0+QvlaHzCuARSKsIK1UaDibWX+
# 9xgKjTBtTTqnxfM2Le5fLKCSALEcTOLL9/8kJX/Xj8Ddl27Oshe2xxxEpyTKfoHm
# 5jG5FtldPtFo7r7NSNCGLK7cDiHBwIrD7huTWRP2xjuAchiIU/urvzA+oHe9Uoi/
# etjosJOtoRuM1H6mEFAQvuHIHGT6hy77xEdmFsCEezavX7qFRGwCDy3gsA4boj4l
# AgMBAAGjggF/MIIBezAfBgNVHSUEGDAWBggrBgEFBQcDAwYKKwYBBAGCN0wIATAd
# BgNVHQ4EFgQUWFZxBPC9uzP1g2jM54BG91ev0iIwUQYDVR0RBEowSKRGMEQxDTAL
# BgNVBAsTBE1PUFIxMzAxBgNVBAUTKjMxNjQyKzQ5ZThjM2YzLTIzNTktNDdmNi1h
# M2JlLTZjOGM0NzUxYzRiNjAfBgNVHSMEGDAWgBRIbmTlUAXTgqoXNzcitW2oynUC
# lTBUBgNVHR8ETTBLMEmgR6BFhkNodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtp
# b3BzL2NybC9NaWNDb2RTaWdQQ0EyMDExXzIwMTEtMDctMDguY3JsMGEGCCsGAQUF
# BwEBBFUwUzBRBggrBgEFBQcwAoZFaHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3Br
# aW9wcy9jZXJ0cy9NaWNDb2RTaWdQQ0EyMDExXzIwMTEtMDctMDguY3J0MAwGA1Ud
# EwEB/wQCMAAwDQYJKoZIhvcNAQELBQADggIBAIjiDGRDHd1crow7hSS1nUDWvWas
# W1c12fToOsBFmRBN27SQ5Mt2UYEJ8LOTTfT1EuS9SCcUqm8t12uD1ManefzTJRtG
# ynYCiDKuUFT6A/mCAcWLs2MYSmPlsf4UOwzD0/KAuDwl6WCy8FW53DVKBS3rbmdj
# vDW+vCT5wN3nxO8DIlAUBbXMn7TJKAH2W7a/CDQ0p607Ivt3F7cqhEtrO1Rypehh
# bkKQj4y/ebwc56qWHJ8VNjE8HlhfJAk8pAliHzML1v3QlctPutozuZD3jKAO4WaV
# qJn5BJRHddW6l0SeCuZmBQHmNfXcz4+XZW/s88VTfGWjdSGPXC26k0LzV6mjEaEn
# S1G4t0RqMP90JnTEieJ6xFcIpILgcIvcEydLBVe0iiP9AXKYVjAPn6wBm69FKCQr
# IPWsMDsw9wQjaL8GHk4wCj0CmnixHQanTj2hKRc2G9GL9q7tAbo0kFNIFs0EYkbx
# Cn7lBOEqhBSTyaPS6CvjJZGwD0lNuapXDu72y4Hk4pgExQ3iEv/Ij5oVWwT8okie
# +fFLNcnVgeRrjkANgwoAyX58t0iqbefHqsg3RGSgMBu9MABcZ6FQKwih3Tj0DVPc
# gnJQle3c6xN3dZpuEgFcgJh/EyDXSdppZzJR4+Bbf5XA/Rcsq7g7X7xl4bJoNKLf
# cafOabJhpxfcFOowMIIHejCCBWKgAwIBAgIKYQ6Q0gAAAAAAAzANBgkqhkiG9w0B
# AQsFADCBiDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNV
# BAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEyMDAG
# A1UEAxMpTWljcm9zb2Z0IFJvb3QgQ2VydGlmaWNhdGUgQXV0aG9yaXR5IDIwMTEw
# HhcNMTEwNzA4MjA1OTA5WhcNMjYwNzA4MjEwOTA5WjB+MQswCQYDVQQGEwJVUzET
# MBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMV
# TWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBT
# aWduaW5nIFBDQSAyMDExMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA
# q/D6chAcLq3YbqqCEE00uvK2WCGfQhsqa+laUKq4BjgaBEm6f8MMHt03a8YS2Avw
# OMKZBrDIOdUBFDFC04kNeWSHfpRgJGyvnkmc6Whe0t+bU7IKLMOv2akrrnoJr9eW
# WcpgGgXpZnboMlImEi/nqwhQz7NEt13YxC4Ddato88tt8zpcoRb0RrrgOGSsbmQ1
# eKagYw8t00CT+OPeBw3VXHmlSSnnDb6gE3e+lD3v++MrWhAfTVYoonpy4BI6t0le
# 2O3tQ5GD2Xuye4Yb2T6xjF3oiU+EGvKhL1nkkDstrjNYxbc+/jLTswM9sbKvkjh+
# 0p2ALPVOVpEhNSXDOW5kf1O6nA+tGSOEy/S6A4aN91/w0FK/jJSHvMAhdCVfGCi2
# zCcoOCWYOUo2z3yxkq4cI6epZuxhH2rhKEmdX4jiJV3TIUs+UsS1Vz8kA/DRelsv
# 1SPjcF0PUUZ3s/gA4bysAoJf28AVs70b1FVL5zmhD+kjSbwYuER8ReTBw3J64HLn
# JN+/RpnF78IcV9uDjexNSTCnq47f7Fufr/zdsGbiwZeBe+3W7UvnSSmnEyimp31n
# gOaKYnhfsi+E11ecXL93KCjx7W3DKI8sj0A3T8HhhUSJxAlMxdSlQy90lfdu+Hgg
# WCwTXWCVmj5PM4TasIgX3p5O9JawvEagbJjS4NaIjAsCAwEAAaOCAe0wggHpMBAG
# CSsGAQQBgjcVAQQDAgEAMB0GA1UdDgQWBBRIbmTlUAXTgqoXNzcitW2oynUClTAZ
# BgkrBgEEAYI3FAIEDB4KAFMAdQBiAEMAQTALBgNVHQ8EBAMCAYYwDwYDVR0TAQH/
# BAUwAwEB/zAfBgNVHSMEGDAWgBRyLToCMZBDuRQFTuHqp8cx0SOJNDBaBgNVHR8E
# UzBRME+gTaBLhklodHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtpL2NybC9wcm9k
# dWN0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFfMDNfMjIuY3JsMF4GCCsGAQUFBwEB
# BFIwUDBOBggrBgEFBQcwAoZCaHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraS9j
# ZXJ0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFfMDNfMjIuY3J0MIGfBgNVHSAEgZcw
# gZQwgZEGCSsGAQQBgjcuAzCBgzA/BggrBgEFBQcCARYzaHR0cDovL3d3dy5taWNy
# b3NvZnQuY29tL3BraW9wcy9kb2NzL3ByaW1hcnljcHMuaHRtMEAGCCsGAQUFBwIC
# MDQeMiAdAEwAZQBnAGEAbABfAHAAbwBsAGkAYwB5AF8AcwB0AGEAdABlAG0AZQBu
# AHQALiAdMA0GCSqGSIb3DQEBCwUAA4ICAQBn8oalmOBUeRou09h0ZyKbC5YR4WOS
# mUKWfdJ5DJDBZV8uLD74w3LRbYP+vj/oCso7v0epo/Np22O/IjWll11lhJB9i0ZQ
# VdgMknzSGksc8zxCi1LQsP1r4z4HLimb5j0bpdS1HXeUOeLpZMlEPXh6I/MTfaaQ
# dION9MsmAkYqwooQu6SpBQyb7Wj6aC6VoCo/KmtYSWMfCWluWpiW5IP0wI/zRive
# /DvQvTXvbiWu5a8n7dDd8w6vmSiXmE0OPQvyCInWH8MyGOLwxS3OW560STkKxgrC
# xq2u5bLZ2xWIUUVYODJxJxp/sfQn+N4sOiBpmLJZiWhub6e3dMNABQamASooPoI/
# E01mC8CzTfXhj38cbxV9Rad25UAqZaPDXVJihsMdYzaXht/a8/jyFqGaJ+HNpZfQ
# 7l1jQeNbB5yHPgZ3BtEGsXUfFL5hYbXw3MYbBL7fQccOKO7eZS/sl/ahXJbYANah
# Rr1Z85elCUtIEJmAH9AAKcWxm6U/RXceNcbSoqKfenoi+kiVH6v7RyOA9Z74v2u3
# S5fi63V4GuzqN5l5GEv/1rMjaHXmr/r8i+sLgOppO6/8MO0ETI7f33VtY5E90Z1W
# Tk+/gFcioXgRMiF670EKsT/7qMykXcGhiJtXcVZOSEXAQsmbdlsKgEhr/Xmfwb1t
# bWrJUnMTDXpQzTGCBLwwggS4AgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCB0DAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQUb6POuK7DVV9FmYPFjFnZxWaEP7cwcAYKKwYB
# BAGCNwIBDDFiMGCgLoAsAFUAcABkAGEAdABlAFIAZQB0AGEAaQBsAFMAZQByAHYA
# ZQByAC4AcABzADGhLoAsaHR0cDovL3d3dy5NaWNyb3NvZnQuY29tL01pY3Jvc29m
# dER5bmFtaWNzLyAwDQYJKoZIhvcNAQEBBQAEggEAFokvSaSZzCMV3FjBp1ewWy/K
# gjWjlSLxXtPMytDA2ZZADOhvgpIbHi33vAAIzjdp8lMBSDaMqpLRelPZgKS6chXX
# NHbaQVhkrXpxDUmQA+f3QUtaPY0xhcTM3yIuK9hyx5Nt8dgN0jQv79mvjVTZjdRJ
# e9n4uCED1SAZWE7FAUtGQQeQm9UmghLxggNTy0Xw3JUD4xnswUJ6Konl25S15xc0
# rB18877H+Fzztr7GUhIWq7yTocbk+k7+6MHSIua3P/TvITsyu0kGV/Ut7rNY0p2P
# HYHZr0Ryi7bM18ofK9rblGefpYVdfaXHLLxAMVWTBFxuREWWH27969lICRPd46GC
# AigwggIkBgkqhkiG9w0BCQYxggIVMIICEQIBATCBjjB3MQswCQYDVQQGEwJVUzET
# MBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMV
# TWljcm9zb2Z0IENvcnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQgVGltZS1T
# dGFtcCBQQ0ECEzMAAACc7v4UValdNVAAAAAAAJwwCQYFKw4DAhoFAKBdMBgGCSqG
# SIb3DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE2MDgwNjA4NTI1
# NlowIwYJKoZIhvcNAQkEMRYEFNWn6bqRxVoG8b0B8eWNKaWNIwdeMA0GCSqGSIb3
# DQEBBQUABIIBAGuiDSLcFuxY+q8KQYuP1GsgPy1HTCOjbRef/HKJAsY3GAEXVUtR
# QWclpau6RdRVJQFYJ3rfvV8DAgsqiAuRYaFmJrdyodn0fB0sshPWnvhD/fmZx5oa
# nj+GKcoHlqLVx1HgqAvgk73OkD+7MHIc5RUD8RIDAG5zpFwMLUdNA7UPXxZdIDE3
# 5DI1IVahakFMPZVlOwa1PO66/d3u9R8Wq9/D/FV67Y365wlDXGVR0Oi3VAm2X5Ip
# zHQaBP9CVd5bz7BdqnB/AAxtnnQYywIKPDOFH40sLItKKJM8q1DHO2ghGktZwp2L
# FzxgiCk+ADuvOdERPz+u1b7AC0/V/ovBhqg=
# SIG # End signature block
