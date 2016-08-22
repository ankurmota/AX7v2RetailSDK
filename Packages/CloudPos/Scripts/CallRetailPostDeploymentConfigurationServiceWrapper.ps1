# This is a wrapper script where we collect all the paramter from service model and then use it to run CallRetailPostDeploymentConfigurationService.ps1
# and CallRetailPostDeploymentConfigurationService.ps1 will run Microsoft.Dynamics.AX.Deployment.Setup.exe
param
(
    $servicemodelxml,
    [string]$log,
	$isServiceModelXmlEncrypted = $true
)

try
{
	if(-not $servicemodelxml)
	{
		Write-host "servicemodelxml is not passed in, will return now"
		return
	}

	$DecodedServiceModelXml = $servicemodelxml
	if($isServiceModelXmlEncrypted)
	{
		$DecodedServiceModelXml=[xml] [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($servicemodelxml))
	}
	$ConfigurePrerequisites=$DecodedServiceModelXml.SelectSingleNode("//Configuration/Setting[@Name='ConfigurePrerequisites']")

	if($ConfigurePrerequisites -eq $null -or $ConfigurePrerequisites.getAttribute("Value") -eq $false)
	{
		Write-host 'ConfigurePrerequisites is set to false, will skipt it now'
		return
	}	
		
	$Error.Clear()
	$ErrorActionPreference = 'Stop';
		
	. "$PSScriptRoot\Common-Configuration.ps1"
	. "$PSScriptRoot\Common-Web.ps1"

	$parameters = @{}	

	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'AxDbServer' -ParameterName 'AosDatabaseServer'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'AxDbName' -ParameterName 'AosDatabaseName'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'AxDbDeploySqlUser' -ParameterName 'AosDatabaseUser'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'AxDbDeploySqlPwd' -ParameterName 'AosDatabasePass'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'dbServer' -ParameterName 'ChannelDatabaseServer'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'dbName' -ParameterName 'ChannelDatabaseName'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'dbUser' -ParameterName 'ChannelDatabaseUser'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'dbPassword' -ParameterName 'ChannelDatabasePass'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'dbUser' -ParameterName 'ChannelDatabaseDataSyncUser'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'dbPassword' -ParameterName 'ChannelDatabaseDataSyncPass'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'AdminPrincipalName' -ParameterName 'AosAdminUserId'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'AosSoapUrl' -ParameterName 'AosUrl'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'serviceUri' -ParameterName 'RetailServerUrl'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'retailCloudPOSUrl' -ParameterName 'CloudPOSUrl'
	Get-ParametersFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -OutputSettingsHashset ([ref]$parameters) -SettingName 'retailStorefrontUrl' -ParameterName 'StorefrontUrl'
	
	
	$IsServiceModelDeployment = "true"
	$CallerExeName = 'Microsoft.Dynamics.AX.Deployment.Setup.exe'

	$UserId = 'RetailServerSystemAccount@dynamics.com'
	$RetailSideloadingKey = 'W274K-NVCM3-8XM4C-J37PP-GQ4FH'
	$SetupMode = 'RunStaticXppMethod'
	$ClassName = 'RetailPostDeploymentConfiguration'
	$MethodName = 'Apply'

	$parameters.Add('IsServiceModelDeployment',$IsServiceModelDeployment)
	$parameters.Add('CallerExeName',$CallerExeName)
	$parameters.Add('RetailSideloadingKey',$RetailSideloadingKey)
	$parameters.Add('SetupMode',$SetupMode)
	$parameters.Add('ClassName',$ClassName)
	$parameters.Add('MethodName',$MethodName)

    $parameters.Add('DataGroupName','Default')
    $parameters.Add('DataGroupDescription','Default data group')
    $parameters.Add('DatabaseProfileName','Default')
    $parameters.Add('UserId','RetailServerSystemAccount@dynamics.com')
    $parameters.Add('RtsProfileId','Default')
    $parameters.Add('RetailChannelProfileName','Retail server channel profile')

	import-module WebAdministration
	$AosWebsiteName = (Get-ConfigurationValueFromServiceModelXml -ServiceModelXml $DecodedServiceModelXml -XPath "//Configuration/Setting[@Name='AosWebsiteName']")
	$aosWebsite = Get-WebSiteSafe -Name $AosWebsiteName
	if($aosWebsite -eq $null)
	{
		throw "Unable to find Aos website with name $AosWebsiteName"
	}

	Write-Output 'Searching for AOS web.config'
	$webConfigPath = Join-Path -Path $aosWebsite.PhysicalPath -ChildPath 'web.config'

	if (-not (Test-Path $webConfigPath))
	{
		throw 'Could not find AOS web config parent folder'
	}

	$aosWebConfigContent = [xml](Get-Content -Path $webConfigPath)
	$axDeplSetupExeFolder = "$($aosWebsite.PhysicalPath)\bin"

	if (-not (Test-Path -Path (Join-Path -Path $axDeplSetupExeFolder -ChildPath $CallerExeName)))
	{
		throw 'Could not find {0} in {1}' -f  $CallerExeName,$axDeplSetupExeFolder
	}

	$parameters.Add('PathToCallerFolder', $axDeplSetupExeFolder)

	$parameters.Add('ExecuteRunSeedDataGenerator', $true)
	$parameters.Add('ExecuteConfigureAsyncService', $true)
	$parameters.Add('ExecuteConfigureRealTimeService', $true)
	$parameters.Add('ExecuteRunCdxJobs', $true)
	$parameters.Add('ConfigureSelfService', $false)

	$identityProvider = Get-WebConfigAppSetting -WebConfig $aosWebConfigContent -SettingName 'Provisioning.AdminIdentityProvider'
	
	$parameters.Add('IdentityProvider', $identityProvider)
	$aadRealm = Get-WebConfigAppSetting -WebConfig $aosWebConfigContent -SettingName 'Aad.Realm'
	$parameters.Add('AudienceUrn', $aadRealm)
	
	$MetadataDirectory = Get-WebConfigAppSetting -WebConfig $aosWebConfigContent -SettingName 'Aos.PackageDirectory'
	$parameters.Add('MetadataDirectory', $MetadataDirectory)

	$BinDirectory = Get-WebConfigAppSetting -WebConfig $aosWebConfigContent -SettingName 'Common.BinDir'
	$parameters.Add('BinDirectory', $BinDirectory)

	Write-Output 'AdjustAxSetupConfig'
	AdjustAxSetupConfig -AosWebConfigPath $webConfigPath -AxSetupExeFolder $axDeplSetupExeFolder	
	
	Write-Output 'Searching for script dir path'
	$scriptName = 'CallRetailPostDeploymentConfigurationService.ps1'
	$scriptLocation =  Join-Path -Path $PSScriptRoot -ChildPath $scriptName
	if (-not (Test-Path -Path $scriptLocation))
	{
		throw 'Script file {0} was not found' -f $scriptLocation 
	}

	$retailServerUrlKey = 'RetailServerUrl'
	if($parameters.ContainsKey($retailServerUrlKey) -and (![string]::IsNullOrEmpty($parameters[$retailServerUrlKey].Trim())))
	{
		$retailServerUrlValue = $parameters[$retailServerUrlKey].Trim()
		$retailServerUrlValue = $retailServerUrlValue.trimEnd('/')
		Write-Output "Found $retailServerUrlKey in the parameter list, replacing RetailServerUrl with $retailServerUrlValue"

		# this is to handle both non-ARR deployment and ARR deployment, 
		# in non-ARR deployment, the passed in url doesn't contain /Commerce
		# in ARR deployment, the passed in url contains /Commerce
		if($retailServerUrlValue -like '*/Commerce*')
		{
			$retailServerUrlValue = $retailServerUrlValue.substring(0, $retailServerUrlValue.toLower().indexOf('/commerce'))
		}
		
		$parameters['RetailServerUrl'] = ('{0}/Commerce' -f $retailServerUrlValue)
		$parameters['MediaServerUrl'] = ('{0}/MediaServer' -f $retailServerUrlValue)
	}
	else
	{
		Write-Output "Error: RetailServerUrl is not in the parameter list"
	}
	
	if(IsAosConfiguredWithStorageEmulator -AosWebSiteName $AosWebsiteName)
	{
		Write-Output "AOS is using storage emulator, try to start it."
		Start-StorageEmulator
	}

	foreach ($parameter in $parameters.Keys) 
	{
		if (($parameter -ne 'ChannelDatabasePass') -and ($parameter -ne 'AosDatabasePass') -and ($parameter -ne 'ChannelDatabaseDataSyncPass'))
		{
			Write-Log ('Passed Param Name: {0} , Param Value {1}' -f $parameter, $parameters.Item($parameter))
		}
	}
	
	Write-Output "Calling Retail Post Deployment Configuration Service: $scriptLocation"
	${global:LASTEXITCODE} = 0

	& $scriptLocation @parameters *>&1 | Tee-Object $log
	$exitCode = ${global:LASTEXITCODE}

	if ($exitCode -ne 0)
    {
		Write-Log ($global:error[0] | format-list * -f | Out-String)   
		Write-Log ("Error thrown from CallRetailPostDeploymentConfigurationService.ps1 and caught in `
			CallRetailPostDeploymentConfigurationServiceWrapper.ps1, Exiting with error code $exitCode." -f [System.Environment]::NewLine)
        exit $exitCode
    }
	
	Write-Output "Resetting IIS."
	IISRESET
	Write-Output ('{0} ended with exit code {1}' -f $scriptLocation, $exitCode)
	return $exitCode
}
catch
{
    Write-Log ($global:error[0] | format-list * -f | Out-String)   
    $exitCode = 2
    Write-Log ("Error at CallRetailPostDeploymentConfigurationServiceWrapper.ps1, Exiting with error code $exitCode." -f [System.Environment]::NewLine)
    return $exitCode
}
# SIG # Begin signature block
# MIId/AYJKoZIhvcNAQcCoIId7TCCHekCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUXxNhIEqLLiHTheqMXa6rDWeN
# mAigghhkMIIEwzCCA6ugAwIBAgITMwAAAK7sP622i7kt0gAAAAAArjANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBQIwggT+AgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCCARUwGQYJ
# KoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQB
# gjcCARUwIwYJKoZIhvcNAQkEMRYEFHFS4X5jeon9INZu0kWhG64nkr9FMIG0Bgor
# BgEEAYI3AgEMMYGlMIGioHCAbgBDAGEAbABsAFIAZQB0AGEAaQBsAFAAbwBzAHQA
# RABlAHAAbABvAHkAbQBlAG4AdABDAG8AbgBmAGkAZwB1AHIAYQB0AGkAbwBuAFMA
# ZQByAHYAaQBjAGUAVwByAGEAcABwAGUAcgAuAHAAcwAxoS6ALGh0dHA6Ly93d3cu
# TWljcm9zb2Z0LmNvbS9NaWNyb3NvZnREeW5hbWljcy8gMA0GCSqGSIb3DQEBAQUA
# BIIBAG/KZ3Q2zyFmbcnZ8Gc0c5W8M2xfPgWaG4ffm1DMFtR+T5hHhI3lbTUf6GFd
# ImPyZTU9tqCEZkwyqxd0rXXxTzYVtRZ86pO3DBi23kNYtV7N00HOAaNjsmcyJWbO
# /FGMfrj4z9C26vwj5HQgb4xquaqNz6MRkw5I4RpEJzkcFgSeUHRCekGJnTggNldP
# SlMkrt8OdDEbSjBpr79hhVIcYKds/UYPoJz1WmZc8mLMZwp2+8nl84gm5OVQrIDa
# qYgvTcHX/CKxuIZnbNtTQYAUvA18KpIF92VHccYH9fdOUdKttfV0cR0aVaKyrR/R
# WyC3vPo8Urz3RYv7ctkeqAjHvOihggIoMIICJAYJKoZIhvcNAQkGMYICFTCCAhEC
# AQEwgY4wdzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNV
# BAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8G
# A1UEAxMYTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBAhMzAAAAruw/rbaLuS3SAAAA
# AACuMAkGBSsOAwIaBQCgXTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqG
# SIb3DQEJBTEPFw0xNjA3MjEyMTA3MTZaMCMGCSqGSIb3DQEJBDEWBBThABVjGmz4
# kTWWbs5stonknMaGDjANBgkqhkiG9w0BAQUFAASCAQChjxAxXCv8p7pdP1+ZEORy
# pCdO8n7KhstpWz7kKC28ARdj7lITOCUoPCcn4UYaezJ33tfdmEVGVS1spda5ORep
# klQnKVfHt8Rk5/RmKpscw7L+A9WRCbqfJMohBx9pgxlM2Yv5NYWmrtl+mijziSjn
# ikjYTo9G7FL8FXwmO1lz+oileswh3t+k9KY/2I8Ml7JysE/GWK2fSIbxsgykLF3v
# o94kMNEyL6LR5kC/N8FNRTScuW55tz746ap3GWtHC4wNrYrrNeSZlW4hJqLRLHqa
# x08TDbLEoStbynn0WcCiEWVmEGrEoxyVJkD6LC8V2sjw8HlTbbmIAuxNHdxQ2X1L
# SIG # End signature block
