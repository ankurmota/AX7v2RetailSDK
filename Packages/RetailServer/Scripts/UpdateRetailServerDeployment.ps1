param(
    $dbServer = $(Throw 'dbServer is required!'), 
    $dbName = $(Throw 'dbName is required!'), 
    $dbUser = $(Throw 'dbUser is required!'), 
    $dbPassword = $(Throw 'dbPassword is required!'),
    $AxDbServer = $(Throw 'AxDbServer is required!'), 
    $AxDbName = $(Throw 'AxDbName is required!'), 
    $AxDbDeploySqlUser = $(Throw 'AxDbDeploySqlUser is required!'), 
    $AxDbDeploySqlPwd = $(Throw 'AxDbDeploySqlPwd is required!'),
    $serviceUri = $(Throw 'serviceUri is required!'),
    $AADTokenIssuerPrefix = $(Throw 'AADTokenIssuerPrefix is required!'),
    $AdminPrincipalName = $(Throw 'AdminPrincipalName is required!'),
    $AllowAnonymousContextRetailServerRequests = $(Throw 'AllowAnonymousContextRetailServerRequests is required!'),
    $RetailCryptographyThumbprint = $(Throw 'RetailCryptographyThumbprint is required!'),
    $RetailRTSAuthenticationCertificateThumbprint = $(Throw 'RetailRTSAuthenticationCertificateThumbprint is required!'),
    $HardwareStationAppInsightsInstrumentationKey = $(Throw 'HardwareStationAppInsightsInstrumentationKey is required!'),
    $ClientAppInsightsInstrumentationKey = $(Throw 'ClientAppInsightsInstrumentationKey is required!'),
    $EnvironmentId = $(Throw 'EnvironmentId is required!'),
	
	[string] $retailWebSiteName = "Default Web Site",
	[string] $validationKey = '',
	[string] $decryptionKey = '',	
	[string] $validationMethod = 'SHA1',
	[string] $decryptionMethod = 'AES',
	[string] $RetailChannelStoreName = 'HoustonStore',
	[string] $retailCloudPOSUrl,
	[string] $retailStorefrontUrl,	
	$Encryption = $true,
	$DisableDBServerCertificateValidation = $true
	)
	
$parentdir = Split-Path -parent $PSCommandPath
$grandparentdir = Split-Path -parent $parentdir

. "$grandparentdir\Scripts\Common-Configuration.ps1"

function Encrypt-WebConfigAppSetting(
	[string]$webConfigFilePath = $(throw 'webConfigFilePath is required'),
	[string] $key = $(throw 'key is required'))
{
	[xml]$doc = Get-Content $webConfigFilePath

	# Check and see if we have one of these elements already.
	$configElement = $doc.configuration.appSettings.add | Where-Object { $_.key -eq $key }

	# Only add a new element if one doesn't already exist.
	if($configElement)
	{		
		$configElement.value = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($configElement.value))
	}	

	Set-ItemProperty $webConfigFilePath -name IsReadOnly -value $false
	$doc.Save($webConfigFilePath)
}

function ExecuteSql
{
	param
	(
		[string] $sqlServer = '(local)',
        [string] $database,
		[string] $sqlUser,
		[string] $sqlUserPassword,
		[string] $sqlStatement
	)
	
	Write-Log "Running sql statement in server: $sqlServer, DB: $database, User: $sqlUser, Password: *******"
	try
	{
		Invoke-SqlCmd -ServerInstance $sqlServer -Database  $database -Username $sqlUser -Password $sqlUserPassword -Query $sqlStatement -ErrorAction Stop		
	}
	catch
	{
		Write-Log  "$($_.Exception.Message)"
		throw "Running sql statement in server: $sqlServer, DB: $database, User: $sqlUser, Password: *******, error: $($_.Exception.Message) "		
	}
	
	Write-Log "Finished running sql statement in server: $sqlServer, DB: $database, User: $sqlUser, Password: *******, retailReturnCode: $retailReturnCode, LASTEXITCODE: $LASTEXITCODE "	
}

# Generate MachineKey

function Gen-Key($CertThumbPrint) 
{   
   $pass = [Text.Encoding]::UTF8.GetBytes($CertThumbPrint)   
   
   $cert = (Get-ChildItem -path cert:\LocalMachine\My | Where {$_.Thumbprint -eq $CertThumbPrint})
   
   if(($cert -eq $null) -or ($cert.PrivateKey -eq $null))
   {
		throw "Cannot find cert with Thumbprint $CertThumbPrint or the private key doesn't exist"
   }
   
   [System.Security.Cryptography.RSACryptoServiceProvider]$key = $cert.PrivateKey
   $buff = $key.Encrypt($pass, $true)
   
   $sb = new-object System.Text.StringBuilder(128)
    for($i = 0; ($i -lt $buff.Length); $i++)
    {
        $sb = $sb.AppendFormat("{0:X2}", $buff[$i])
    }
    return $sb.ToString()
}

function Remove-NonProductionReleaseFiles(
	[string]$TargetFolder = $(throw 'TargetFolder is required')
)
{
	$filesToRemove = @(
		'Microsoft.Dynamics.Retail.DynamicsOnlineConnector.dll',
		'Microsoft.Dynamics.Retail.DynamicsOnlineConnector.pdb',
		'Microsoft.Dynamics.Retail.DynamicsOnlineConnector.Portable.dll',
		'Microsoft.Dynamics.Retail.DynamicsOnlineConnector.Portable.pdb')
		
	foreach($file in $filesToRemove)
	{
		Remove-Item -Path (Join-Path $TargetFolder $file) -Force -ErrorAction SilentlyContinue
	}	
}

function Get-ChannelDbRegistryPath
{
    return 'HKLM:\SOFTWARE\Microsoft\Dynamics\7.0\RetailChannelDatabase\Servicing'
}

function Get-ChannelDbServicingPropertyName(
    [int]$propertyIndex = '1'
)
{
    return 'UpgradeServicingData' + $propertyIndex
}

function Save-ChannelDbServicingDataToRegistry(
    [string]$servicingData = $(throw 'servicingData is required.'),
    [string]$targetRegistryKeyPath = $(throw 'targetRegistryKeyPath is required.'),
    [string]$targetPropertyName =  $(throw 'targetPropertyName is required.')
)
{
    # Convert servicing data to secure text.
    $servicingDataAsSecureStringObject = ConvertTo-SecureString -AsPlainText $servicingData -Force
    $servicingDataAsEncryptedString = ConvertFrom-SecureString $servicingDataAsSecureStringObject
    
    # Save encrypted servicing data to the registry
    New-Item -Path $targetRegistryKeyPath -ItemType Directory -Force
    New-ItemProperty -Path $targetRegistryKeyPath -Name $targetPropertyName -Value $servicingDataAsEncryptedString
}

Import-Module ServerManager

import-module WebAdministration

Set-Location -Path "IIS:\Sites"
$defaultSite = Get-WebSite -Name $retailWebSiteName

$targetAppPool = Get-Item "IIS:\Sites\$retailWebSiteName"| Select-Object applicationPool 
$targetAppPoolName = $targetAppPool.applicationPool

$rsWebPath = [System.Environment]::ExpandEnvironmentVariables($defaultsite.physicalPath)

$webConfigPath = "$rsWebPath\web.config"
$doc = New-Object System.Xml.XmlDocument
$doc.Load($webConfigPath)

if ($HardwareStationAppInsightsInstrumentationKey -ne "")
{
    $doc.configuration.environment.instrumentation.hardwareStationAppinsightsKey = $HardwareStationAppInsightsInstrumentationKey
}

if ($ClientAppInsightsInstrumentationKey -ne "")
{
    $doc.configuration.environment.instrumentation.clientAppinsightsKey = $ClientAppInsightsInstrumentationKey
}

if ($EnvironmentId -ne "")
{
    $doc.configuration.environment.id = $EnvironmentId
}

if ($RetailCryptographyThumbprint -ne "")
{
    $doc.configuration.retailServer.cryptography.certificateThumbprint = $RetailCryptographyThumbprint
}

# Set isConnectionStringOverridden
$isConnectionStringOverriddenKey = "isConnectionStringOverridden"
# Check and see if we have one of these elements already.
$isConnectionStringOverriddenKeyElement = $null
foreach($add in $doc.configuration.appSettings.add)
{ 
	if($add.key -eq $isConnectionStringOverriddenKey)
	{
		$isConnectionStringOverriddenKeyElement = $add
		break
	}
}

# Only add a new element if one doesn't already exist.
if(!$isConnectionStringOverriddenKeyElement)
{
	$isConnectionStringOverriddenKeyElement = $doc.configuration.appSettings.add[0].Clone()
	$isConnectionStringOverriddenKeyElement.key = $isConnectionStringOverriddenKey
	$doc.configuration.appSettings.AppendChild($isConnectionStringOverriddenKeyElement)
}

$isConnectionStringOverriddenKeyElement.value = "true"

$nodeconnectionStrings = $doc.SelectSingleNode("//connectionStrings")
	
if(!$nodeconnectionStrings)
{	
	$nodeconnectionStrings = $doc.CreateElement('connectionStrings')
	$doc.SelectSingleNode("//configuration").AppendChild($nodeconnectionStrings)
}
else
{
	$nodeconnectionStrings.RemoveAll()
}

IF([string]::IsNullOrEmpty($DisableDBServerCertificateValidation))
{
	$DisableDBServerCertificateValidation = $true
}

$retailChannelDBConnectionString = ('Server="{0}";Database="{1}";User ID="{2}";Password="{3}";Trusted_Connection=False;Encrypt={4};TrustServerCertificate={5};' -f $dbServer,$dbName,$dbUser,$dbPassword, $Encryption, $DisableDBServerCertificateValidation)
$channelDBServicingConnectionString = ('Server="{0}";Database="{1}";User ID="{2}";Password="{3}";Trusted_Connection=False;Encrypt={4};TrustServerCertificate={5};' -f $dbServer,$dbName,$AxDbDeploySqlUser,$AxDbDeploySqlPwd, $Encryption, $DisableDBServerCertificateValidation)

$channelDbRegistryPath = (Get-ChannelDbRegistryPath)
$channelDbServicingPropertyName = (Get-ChannelDbServicingPropertyName -propertyIndex '1')
Save-ChannelDbServicingDataToRegistry -servicingData $channelDBServicingConnectionString -targetRegistryKeyPath $channelDbRegistryPath -targetPropertyName $channelDbServicingPropertyName

$nodeStorageLookupDatabase = $doc.CreateElement('add')
$nodeStorageLookupDatabase.SetAttribute('name','StorageLookupDatabase')
$nodeStorageLookupDatabase.SetAttribute('connectionString',$retailChannelDBConnectionString)

$nodeRetailHoustonStore = $doc.CreateElement('add')
$nodeRetailHoustonStore.SetAttribute('name',"$RetailChannelStoreName") 
$nodeRetailHoustonStore.SetAttribute('connectionString',$retailChannelDBConnectionString)

$nodeconnectionStrings.AppendChild($nodeStorageLookupDatabase) > $null
$nodeconnectionStrings.AppendChild($nodeRetailHoustonStore) > $null

$validationKey = Gen-Key -CertThumbPrint $RetailCryptographyThumbprint
$decryptionKey = Gen-Key -CertThumbPrint $RetailRTSAuthenticationCertificateThumbprint

# Update MachineKey
$machineKeySetting = $doc.SelectSingleNode("//configuration/system.web/machineKey")

# Only add a new element if one doesn't already exist.
if($machineKeySetting -eq $null)
{
	$machineKeyElement = $doc.CreateElement('machineKey')
	$machineKeyElement.SetAttribute("validationKey","$validationKey")
	$machineKeyElement.SetAttribute("decryptionKey","$decryptionKey")
	$machineKeyElement.SetAttribute("validation","$validationMethod")
	$machineKeyElement.SetAttribute("decryption","$decryptionMethod")
	$doc.configuration['system.web'].AppendChild($machineKeyElement)
}
else
{	
	$machineKeySetting.SetAttribute("validationKey","$validationKey")
	$machineKeySetting.SetAttribute("decryptionKey","$decryptionKey")
	$machineKeySetting.SetAttribute("validation","$validationMethod")
	$machineKeySetting.SetAttribute("decryption","$decryptionMethod")
}

$AADStsHost = 'windows.net'
$DeviceActivationAllowedIdentityProviders = 'https://sts.windows.net'
if ($AdminPrincipalName.ToLower().EndsWith('.ccsctp.net') -or ($AADTokenIssuerPrefix.IndexOf('windows-ppe.net') -gt 0)) 
{ 
	$AADStsHost = 'windows-ppe.net'
        
    # For test environments allowing both AAD PPE and the Commerce identity provider for device activation.
    $DeviceActivationAllowedIdentityProviders = 'https://sts.windows.net, https://commerce.dynamics.com/auth'
}

$serviceUri = $serviceUri.trimEnd('/')

# this is to handle both non-ARR deployment and ARR deployment, 
# in non-ARR deployment, the passed in url doesn't contain /Commerce, for example: https://clxtestax378ret.cloud.test.dynamics.com/
# in ARR deployment, the passed in url contains /Commerce, for example: https://rtarr17aret.axcloud.test.dynamics.com/Commerce
if($serviceUri -like '*/Commerce*')
{
	$serviceUri = $serviceUri.substring(0, $serviceUri.toLower().indexOf('/commerce'))
}

foreach($add in $doc.configuration.appSettings.add)
{	
	if($add.key -eq "FederationMetadataAddress")
	{		
		$add.value = "https://login.{0}/common/FederationMetadata/2007-06/FederationMetadata.xml" -f $AADStsHost
	}
	if($add.key -eq "AADTokenIssuerPrefix")
	{		
		$add.value = "https://sts.$AADStsHost/"
	}
}

$doc.configuration.retailServer.deviceActivation.allowedIdentityProviders = $DeviceActivationAllowedIdentityProviders

$doc.Save($webConfigPath)

# If not test environment then remove the non Production files.
if (-Not ($AdminPrincipalName.ToLower().EndsWith('.ccsctp.net'))) 
{
	Remove-NonProductionReleaseFiles -TargetFolder (Join-Path (Split-Path $webConfigPath -Parent) 'bin')
}

$settingIsAnonymousEnabled = 'false'

# If AllowAnonymousContextRetailServerRequests is set to true then set the IsAnonymousEnabled to true else set it to false.
if($AllowAnonymousContextRetailServerRequests -eq 'true')
{
	$settingIsAnonymousEnabled = 'true'
}

Set-WebConfigAppSetting -webConfigFilePath $webConfigPath -key 'IsAnonymousEnabled' -value $settingIsAnonymousEnabled
Write-Log 'Setting IsAnonymousEnabled to {0}.' -f $settingIsAnonymousEnabled

$AllowedOriginsString = ""
if($retailCloudPOSUrl)
{
	$retailCloudPOSUrl = $retailCloudPOSUrl.trimEnd('/')
	$AllowedOriginsString = $AllowedOriginsString +  $retailCloudPOSUrl
}
if($retailStorefrontUrl)
{
	$retailStorefrontUrl = $retailStorefrontUrl.trimEnd('/')
	$AllowedOriginsString = $AllowedOriginsString +  (';{0}' -f $retailStorefrontUrl)
}
if($AllowedOriginsString)
{
	Set-WebConfigAppSetting -webConfigFilePath $webConfigPath -key 'AllowedOrigins' -value $AllowedOriginsString
}

# Configure Retail Server authentication keys
Write-Log "Configuring Retail Server authentication keys"
$global:LASTEXITCODE = 0
& "$PSScriptRoot\UpdateRetailServerAuthenticationKeys.ps1" -CertificateThumbprint $RetailRTSAuthenticationCertificateThumbprint -RetailServerDeploymentPath $rsWebPath
$capturedExitCode = $global:LASTEXITCODE

if($capturedExitCode -eq 0)
{
    Write-Log "Finished configuring Retail Server authentication keys."
}
else
{
    throw ("Configuring Retail Server authentication keys failed with exit code {0}" -f $capturedExitCode)
}

# update commerceRuntime.config
$crConfigPath = "$rsWebPath\bin\commerceRuntime.config"
$crDoc = New-Object System.Xml.XmlDocument
$crDoc.Load($crConfigPath)
$crDoc.commerceRuntime.realtimeService.certificate.thumbprint = $RetailRTSAuthenticationCertificateThumbprint
$crDoc.Save($crConfigPath)

# Register retail performance counter
Write-Log "Registering retail performance counter"
$global:LASTEXITCODE = 0& "$PSScriptRoot\Register-PerfCounters.ps1" -InstrumentedAssemblyPath "$PSScriptRoot\..\ETWManifest\Microsoft.Dynamics.Retail.Diagnostics.dll"
$capturedExitCode = $global:LASTEXITCODE
if($capturedExitCode -eq 0)
{
    Write-Log "Registering retail performance counter completed successfully."
}
else
{
    throw "Registering retail performance counter failed with exit code $capturedExitCode"
}

Write-Log "Encrypting retail web.config"
$connectionStringSectionName = "connectionStrings"
$targetWebApplicationPath = "/"
$global:LASTEXITCODE = 0
$output = aspnet_regiis -pe $connectionStringSectionName -app $targetWebApplicationPath -Site $defaultSite.id
Write-Log $output
$capturedExitCode = $global:LASTEXITCODE

if($capturedExitCode -eq 0)
{
    Write-Log "web.config encryption completed successfully."
}
else
{
    throw "web.config encryption failed with connectionString encryption exit code: $capturedExitCode"
}
# SIG # Begin signature block
# MIIdywYJKoZIhvcNAQcCoIIdvDCCHbgCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUkcV1oVUuODqlpmhGibVgRT9F
# YD6gghhkMIIEwzCCA6ugAwIBAgITMwAAAK7sP622i7kt0gAAAAAArjANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTYwNTAzMTcxMzI1
# WhcNMTcwODAzMTcxMzI1WjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OkI4RUMtMzBBNC03MTQ0MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxTU0qRx3sqZg
# 8GN4YCrqA1CzmYPp8+U/MG7axHXPZGdMvNbRSPl29ba88jCYRut/6p5OjvCGNcRI
# MPWKFMqKVeY8zUoQNp46jYsXenl4vTAgJ2cUCeaGy9vxLYTGuXtaChn+jIpPuR6x
# UQ60Y44M2jypsbcQZYc6Oukw4co+CIw8fKqxPcDjdm1c/gyzVnhSYTXsv8S0NBwl
# iuhNCNE4D8b0LNj7Exj5zfVYGvP6Z+JtGY7LT+7caUCT0uItKlE0D/iDvlY5zLrb
# luUb4WLUBpglMw7bU0BSAcvcNx0XyV7+AdcmhiFQGt4pZjbVzOsXs3POWHTq4/KX
# RmtGHKfvMwIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFBw4ctJakrpBibpB9TJkYJsJ
# gGBUMB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBAAAZsVbJVNFZUNMcXRxKeelc1DgiQHLC60Sika98OwDFXomY
# akk6yvE+fJ3DICnDUK9kmf83sYTOQ5Y7h3QzwHcPdyhLPHSBBmuPklj6jcWGuvHK
# pUuP9PTjyKBw0CPZ1PTO1Jc5RjsQYvxqu01+G5UvZolnM6Ww7QpmBoDEyze5J+dg
# GwrWMhIKDzKLV9do6R5ouZQvLvV7bjH50AX2tK2n3zpZYvAl/LayLHFNIO7A2DQ1
# VzWa3n2yyYvameaX1NkSLA32PqjAXykmkDfHQ6DFVuDV4nqrNI+s14EJgMQy8DzU
# 9X7+KIkCzLFNq/bc2WDo15qsQiACPVSKY1IOGiIwggYHMIID76ADAgECAgphFmg0
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
# bWrJUnMTDXpQzTGCBNEwggTNAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCB5TAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQUc9rG/6K8gXCR4X9xh3VqI35AZswwgYQGCisG
# AQQBgjcCAQwxdjB0oEKAQABVAHAAZABhAHQAZQBSAGUAdABhAGkAbABTAGUAcgB2
# AGUAcgBEAGUAcABsAG8AeQBtAGUAbgB0AC4AcABzADGhLoAsaHR0cDovL3d3dy5N
# aWNyb3NvZnQuY29tL01pY3Jvc29mdER5bmFtaWNzLyAwDQYJKoZIhvcNAQEBBQAE
# ggEAF5xEBMoQWmTWkq5mAwP1C/Dg/UkOJyLICWuhQDAwDzbe34qzZIvpcp4fKZXO
# O1jgmbCvr3rTaPJ4LRN/9HGEvlGtulg1WRQCuAcquAgK5CcEwv1KMM6j/A6iHAX6
# TF3EcWVO6Hsyhf3ZFsOjhLgaWvlvpbOFZCxZF7W4Bm14gTS9fb7NM3B6rtg+X0ZB
# 8ke7F31jUYCIFDlz/TerM7tZD5bL+6t+tPukwCBtiqxOSFnANtCR8dAztiXG8rIJ
# ByHtSZv60q6Ik2DNVPHUlVZfep/1gUc1qf2ifpwe1DMO8imn/m0ywxCWicpx7So6
# sj3FSEwqT8ftMkl54IEv04z3bKGCAigwggIkBgkqhkiG9w0BCQYxggIVMIICEQIB
# ATCBjjB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
# BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEwHwYD
# VQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0ECEzMAAACu7D+ttou5LdIAAAAA
# AK4wCQYFKw4DAhoFAKBdMBgGCSqGSIb3DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZI
# hvcNAQkFMQ8XDTE2MDgwNjA4NTI1N1owIwYJKoZIhvcNAQkEMRYEFDVaPGzP0+Nr
# Cwch86tbBe/6pJBZMA0GCSqGSIb3DQEBBQUABIIBAGq3H+p57l1/Qnx596YCCnrC
# 5QPCm8zGw0+ehQY2WGRq3Txg9DDr3q/GROKK5cCP21gF7kr7Q2zoxHRMm1sy8vth
# eDXVk1/3+BmKyIpK49H6wZiwjvuxFHFwSpP3+RvI4vP/rYgge1w1KncbXQU7gX1A
# z114U7zqPmqExpDOg/1oavDCm8XbkebsmufFe/ytYYdeZh9mxapyQbItnSrJb25N
# cIbQhxT/LBH+weVwg2LIIpm4/WAZATUdcMuQfv0Znh7JiVLr6pd95iX2EkrSV5RC
# LFq151nljPG3p410umWgCAAm/KIPkXsn6fVK4QWaVpFTgb1JRKBo8mzIcEvUPBw=
# SIG # End signature block
