<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

# Make script to interrupt if exception is thrown.
$ErrorActionPreference = "Stop"

$ScriptDir = split-path -parent $MyInvocation.MyCommand.Path;
. (Join-Path $ScriptDir 'Common-Configuration.ps1')
. (Join-Path $ScriptDir 'Common-Web.ps1')

function Get-FileVersion(
	[string] $filePath = $(throw 'FilePath is required'))
{
	if(-not (Test-Path -Path $filePath))
	{
		throw "$filePath doesn't exist."
	}
	return [System.Diagnostics.FileVersionInfo]::GetVersionInfo($filePath).FileVersion
}

function Compare-FileVersion(
	[string] $sourceVersion = $(throw 'sourceVersion is required'),
	[string] $targetVersion = $(throw 'targetVersion is required'))
{
	[Version] $sourceVersionValue = $sourceVersion
	[Version] $targetVersionValue = $targetVersion
	
	return $sourceVersionValue.CompareTo($targetVersionValue)
}

function Check-UpgradeEligibility(
	[string] $webSiteName = $(throw 'webSiteName is required'),
	[string] $flagFileSubPathToIdentifyCurrentVersion = $(throw 'flagFileSubPathToIdentifyCurrentVersion is required'),
	[string] $minSupportedVersion = $(throw 'minSupportedVersion is required'))
{
	$webSite = Get-WebSiteSafe -Name $webSiteName

	if(-not $webSite)
	{
		throw "Cannot find website with name: $webSiteName"
	}
	
	# check version support
	$currentVersion = Get-FileVersion -filePath (Join-Path $webSite.physicalPath $flagFileSubPathToIdentifyCurrentVersion)
	
	if((Compare-FileVersion -sourceVersion $currentVersion -targetVersion $minSupportedVersion) -lt 0)
	{
		throw "Current Version $currentVersion is lower than minimum supported version: $minSupportedVersion"
	}
	
	return $true
}

function Get-ChannelDbRegistryPath
{
    return 'HKLM:\SOFTWARE\Microsoft\Dynamics\{0}\RetailChannelDatabase\Servicing' -f (Get-ProductVersionMajorMinor)
}

function Get-ChannelDbServicingPropertyName(
    [int]$propertyIndex = '1'
)
{
    return 'UpgradeServicingData' + $propertyIndex
}

function Get-ChannelDbServicingDataFromRegistry()
{
    $result = @()
    $channelDbRegistryPath = (Get-ChannelDbRegistryPath)
    $propertyIndex = 1
    
    while($true)
    {
        $channelDbServicingPropertyName = (Get-ChannelDbServicingPropertyName -propertyIndex $propertyIndex)
        $channelDbEncryptedServicingData = Read-RegistryValue -targetRegistryKeyPath $channelDbRegistryPath -targetPropertyName $channelDbServicingPropertyName
        
        if($channelDbEncryptedServicingData)
        {
            $servicingDataAsSecureString = ConvertTo-SecureString $channelDbEncryptedServicingData
            $servicingDataAsPlainText = [System.Runtime.InteropServices.marshal]::PtrToStringAuto([System.Runtime.InteropServices.marshal]::SecureStringToBSTR($servicingDataAsSecureString))
            
            $propertyIndex += 1
            $result += $servicingDataAsPlainText
        }
        else
        {
            break;
        }
    }
    
    return $result
}

function Extract-ConnectionStringsFromWebConfig([string] $webConfigPath = $(throw 'webConfigPath is required'))
{
	[xml] $webConfigDoc = Get-Content $webConfigPath

	if($webConfigDoc.configuration.connectionStrings -and $webConfigDoc.configuration.connectionStrings.InnerXml)
	{
		return $webConfigDoc.configuration.connectionStrings.InnerXml
	}
	else
	{
		return $null
	}
}

function Create-WebSiteDBConfiguration(
[string]$connectionString = $(throw 'connectionString is required'))
{
	[hashtable]$ht = @{};

    # sample connection string for sql azure "Server=$dbServer;Database=$dbName;User ID=$dbUser;Password=$dbPassword;Trusted_Connection=False;Encrypt=True;"
    $connectionStringObject = New-Object System.Data.Common.DbConnectionStringBuilder
    $connectionStringObject.PSObject.Properties['ConnectionString'].Value = $connectionString
    
    $ht.server = $connectionStringObject['Server']
    $ht.database = $connectionStringObject['Database']
    $ht.sqlUserName = $connectionStringObject['User ID']
    $ht.sqlUserPassword = $connectionStringObject['password']
    
	return $ht
}

function Update-WebsiteConnectionStringSettings(
	[string] $webConfigPath = $(throw 'webConfigPath is required'),
	[string] $connectionStringsXml = $(throw 'connectionStringsXml is required'))
{
	[xml] $webConfigDocNew = Get-Content $webConfigPath
	$webConfigDocNew.SelectSingleNode("//configuration/connectionStrings").InnerXml = $connectionStringsXml
	$webConfigDocNew.Save($webConfigPath)
}

function Replace-WebsiteFiles(
	[string] $webSiteName = $(throw 'webSiteName is required'),
	[string] $newWebFilesPath = $(throw 'newWebFilesPath is required'))
{
    $physicalPath = Get-WebSitePhysicalPath -webSiteName $webSiteName
	
	# Stop website
	Stop-WebSite $webSiteName

    # Back-up the current working directory
	Backup-Directory -sourceFolder $physicalPath
	
	# Replace files
	Copy-Files -SourceDirPath $newWebFilesPath -DestinationDirPath $physicalPath
	
	# Start website
	Start-WebSite $webSiteName
}

function Get-WebSitePhysicalPath([string]$webSiteName = $(throw 'webSiteName is required'))
{
    $webSite = Get-WebSiteSafe -Name $webSiteName
	
	if(!$webSite)
	{
		throw ("Cannot find the website with name: {0} " -f $webSiteName)
	}

    return $webSite.physicalPath
}

function Get-WebSiteId([string]$webSiteName = $(throw 'webSiteName is required'))
{
    $webSite = Get-WebSiteSafe -Name $webSiteName
	
	if(!$webSite)
	{
		throw ("Cannot find the website with name: {0} " -f $webSiteName)
	}

    return $webSite.Id
}

function Is-PackageDelta(
    [ValidateNotNullOrEmpty()]
    [string]$installationInfoFile = $(throw 'installationInfoFile is required'))
{
    [bool]$isPackageDelta = $false
    
	Log-TimedMessage 'Checking if the current package is of type delta.'

	
	$installInfoContent = [XML] (Get-Content -Path $installationInfoFile)
	$updateFilesNode = $installInfoContent.SelectSingleNode('//ServiceModelInstallationInfo/UpdateFiles')
	
	if($updateFilesNode -and $updateFilesNode.HasChildNodes)
	{
	   Log-TimedMessage 'Yes'
	   $isPackageDelta = $true 
	}
	else
	{
	    Log-TimedMessage 'No'
	}
	
	return $isPackageDelta
}

function Get-InstallationInfoFilePath([string]$scriptDir = $(throw 'scriptDir parameter is required'))
{
    $svcModelScriptsDir = (Split-Path (Split-Path $scriptDir -Parent) -Parent)
	$filePath = Join-Path $svcModelScriptsDir 'InstallationInfo.xml'
    
    if(-not(Test-Path -Path $filePath))
	{
	    throw ('Could not locate installation info file at location {0}' -f $filePath)
	}

    return $filePath
}

function Is-UpdateFileActionInclude(
    [ValidateNotNullOrEmpty()]
    [XML]$xml = $(throw 'xml parameter is required'))
{
    [bool]$isFileActionInclude = $false
    $updateAction = $xml.ServiceModelInstallationInfo.UpdateFiles.updateAction

    if([string]::IsNullOrWhiteSpace($updateAction))
    {
        throw 'File update action missing. Please specify whether you want to include or exclude update files.'
    }

    if($updateAction -eq 'include')
    {
        $isFileActionInclude = $true
    }

    return $isFileActionInclude
}

function Get-ListOfFilesToCopy(
    [bool]$isPackageDelta,
    
    [ValidateNotNullOrEmpty()]
    [string]$installationInfoXmlPath = $(throw 'installationInfoXmlPath parameter is required'),

    [ValidateNotNullOrEmpty()]
    [string]$updatePackageCodeFolder = $(throw 'updatePackageCodeFolder parameter is required'))
{
    if($isPackageDelta)
    {
        $installInfoContent = [XML] (Get-Content -Path $installationInfoXmlPath)
        [bool]$includeFiles = Is-UpdateFileActionInclude -xml $installInfoContent
        
        $filesNode = Select-Xml -XPath '//ServiceModelInstallationInfo/UpdateFiles/File' -Xml $installInfoContent

        if($includeFiles)
        {
            $listOfFilesToCopy = $filesNode | % {$_.Node.Name}
        }
        else
        {
            $filesToExclude = @()
            $filesNode | % {
                $fullPath = (Join-Path $updatePackageCodeFolder ("{0}\{1}" -f $_.Node.RelativePath, $_.Node.Name))
                $filesToExclude += $fullPath
            }

            $listOfFilesToCopy = Get-ChildItem -File -Path $updatePackageCodeFolder -Recurse | ? { $_.FullName -notin $filesToExclude } | % {$_.Name}
        }

    }
    else
    {
        $listOfFilesToCopy = Get-ChildItem -File -Path $updatePackageCodeFolder -Recurse | % {$_.Name}
    }

    return $listOfFilesToCopy
}

function Check-IfAnyFilesExistInFolder(
    [string]$folderPath = $(throw 'folderPath parameter is required')
)
{
    if(!(Test-Path -Path $folderPath))
    {
        Log-TimedMessage ("Folder {0} does not exist." -f $folderPath)
        return $false
    }

    $fileList = Get-ChildItem -File $folderPath -Recurse | Measure-Object
    if($fileList.Count -eq 0)
    {
        Log-TimedMessage ("No files exist in the {0} folder." -f $folderPath)
        return $false
    }

    return $true
}

function Check-IfCustomPublisherExistsInInstallInfoXml(
    [string]$installationInfoXml = $(throw 'installationInfoFile is required'))
{
    [xml]$content = Get-Content $installationInfoXml
    
    Log-ActionItem 'Checking if CustomPublisher node is populated in the installation info file'
    $customPublisher = $content.ServiceModelInstallationInfo.CustomPublisher

    if([string]::IsNullOrWhiteSpace($customPublisher))
    {
        Log-ActionResult 'No'
        return $false
    }
    else
    {
        Log-ActionResult ('Yes. Its value is [{0}]' -f $customPublisher)
        return $true
    }
}

function Check-IfCurrentDeploymentOfComponentIsCustomized(
    [string]$componentName = $(throw 'componentName is required'),
    [string]$updatePackageRootDir = $(throw 'installationInfoFile is required'))
{
    [bool]$isCurrentDeploymentCustomized = $false

    Log-ActionItem 'Checking whether the current deployment on the machine is customized'

    # Load the installation info assembly
    $AxInstallationInfoDllPath = Join-Path $updatePackageRootDir 'Microsoft.Dynamics.AX.AXInstallationInfo.dll'
    Add-Type -Path $AxInstallationInfoDllPath

    # Get a list of all service model installation info
    $serviceModelInfoList = [Microsoft.Dynamics.AX.AXInstallationInfo.AXInstallationInfo]::GetInstalledServiceModel()

    $currentServiceModelInfo = $serviceModelInfoList | ? {$_.Name -ieq $componentName}

    if(!([string]::IsNullOrWhiteSpace($currentServiceModelInfo)) -and (Check-IfCustomPublisherExistsInInstallInfoXml -installationInfoXml $currentServiceModelInfo.InstallationInfoFilePath))
    {
        Log-ActionResult 'Yes'
        $isCurrentDeploymentCustomized = $true
    }
    else
    {
        Log-ActionResult 'No'
    }

    return $isCurrentDeploymentCustomized
}

function Check-IfUpdatePackageIsReleasedByMicrosoft(
    [string]$installationInfoXml = $(throw 'installationInfoFile is required'))
{
    return (-not (Check-IfCustomPublisherExistsInInstallInfoXml -installationInfoXml $installationInfoXml))
}

function Rename-InstallationInfoFile(
    [string]$filePath = $(throw 'filePath is required'))
{
    $folderPath = Split-Path $filePath -Parent
    $fileName = Split-Path $filePath -Leaf

    $renamedInstallInfoFilePath = Join-Path $folderPath ('Delta_{0}_{1}' -f $(Get-Date -f yyyy-MM-dd_hh-mm-ss), $fileName)

    Log-TimedMessage ('Renaming the manifest installation info file [{0}] to [{1}]' -f $filePath, $renamedInstallInfoFilePath)
    
    Rename-Item -Path $filePath -NewName $renamedInstallInfoFilePath -Force | Out-Null
    
    Log-TimedMessage 'Done renaming the file.'

    return $renamedInstallInfoFilePath
}

function Get-LcsEnvironmentId()
{
    $monitoringInstallRegKeyPath = 'HKLM:\SOFTWARE\Microsoft\Dynamics\AX\Diagnostics\MonitoringInstall'
    Log-TimedMessage ('Trying to find LCS Environment Id from registry key - {0}' -f $monitoringInstallRegKeyPath)

    $value = Read-RegistryValue -targetRegistryKeyPath $monitoringInstallRegKeyPath -targetPropertyName 'LCSEnvironmentID'

    if(!$value)
    {
        throw 'Failed to find the LCS Environment Id. Update cannot continue.'
    }

    Log-TimedMessage ('Found LCS Environment Id - {0}' -f $value)

    return $value
}

function CreateOrUpdate-ServicingStepSetting(
    [ValidateNotNullOrEmpty()]
    $settingsFilePath = $(throw 'settingsFilePath is required'),
    
    [ValidateNotNullOrEmpty()]
    $runbookStepName = $(throw 'runbookStepName is required'),
    
    [ValidateNotNullOrEmpty()]
    $propertyName = $(throw 'propertyName is required'),
    
    $value = $(throw 'value is required'))
{
    $settingsJson = Get-Content $settingsFilePath -Raw | ConvertFrom-Json

    if(!$settingsJson)
    {
        $settingsJson = New-Object psobject
    }

    Log-TimedMessage ('Check if step [{0}] exists in the file.' -f $runbookStepName)
    if($settingsJson.$runbookStepName -eq $null)
    {
        Log-TimedMessage ('No. Adding Step = [{0}] PropertyName = [{1}] Value = [{2}]' -f $runbookStepName, $propertyName, $value)
        $obj = New-Object psobject | Add-Member -PassThru NoteProperty -Name $propertyName -Value $value -Force
        $settingsJson | Add-Member -MemberType NoteProperty -Name $runbookStepName -Force -Value $obj
    }
    else
    {
        Log-TimedMessage ('Yes. Updating Step = [{0}] PropertyName = [{1}] Value = [{2}]' -f $runbookStepName, $propertyName, $value)
        $settingsJson.$runbookStepName | Add-Member -MemberType NoteProperty -Name $propertyName -Value $value -Force
    }
    
    Log-TimedMessage ('Saving the file {0}' -f $settingsFilePath)
    $settingsJson | ConvertTo-Json | Out-File $settingsFilePath -Force
}
# SIG # Begin signature block
# MIIdrgYJKoZIhvcNAQcCoIIdnzCCHZsCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUXqsmGV6KnKcjdb+L5V9e3jeF
# I1WgghhkMIIEwzCCA6ugAwIBAgITMwAAAJ1CaO4xHNdWvQAAAAAAnTANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTYwMzMwMTkyMTMw
# WhcNMTcwNjMwMTkyMTMwWjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OjE0OEMtQzRCOS0yMDY2MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAy8PvNqh/8yl1
# MrZGvO1190vNqP7QS1rpo+Hg9+f2VOf/LWTsQoG0FDOwsQKDBCyrNu5TVc4+A4Zu
# vqN+7up2ZIr3FtVQsAf1K6TJSBp2JWunjswVBu47UAfP49PDIBLoDt1Y4aXzI+9N
# JbiaTwXjos6zYDKQ+v63NO6YEyfHfOpebr79gqbNghPv1hi9thBtvHMbXwkUZRmk
# ravqvD8DKiFGmBMOg/IuN8G/MPEhdImnlkYFBdnW4P0K9RFzvrABWmH3w2GEunax
# cOAmob9xbZZR8VftrfYCNkfHTFYGnaNNgRqV1rEFt866re8uexyNjOVfmR9+JBKU
# FbA0ELMPlQIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFGTqT/M8KvKECWB0BhVGDK52
# +fM6MB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBAD9dHEh+Ry/aDJ1YARzBsTGeptnRBO73F/P7wF8dC7nTPNFU
# qtZhOyakS8NA/Zww74n4gvm1AWfHGjN1Ao8NiL3J6wFmmON/PEUdXA2zWFYhgeRe
# CPmATbwNN043ecHiGjWO+SeMYpvl1G4ma0NIUJau9DmTkfaMvNMK+/rNljr3MR8b
# xsSOZxx2iUiatN0ceMmIP5gS9vUpDxTZkxVsMfA5n63j18TOd4MJz+G0I62yqIvt
# Yy7GTx38SF56454wqMngiYcqM2Bjv6xu1GyHTUH7v/l21JBceIt03gmsIhlLNo8z
# Ii26X6D1sGCBEZV1YUyQC9IV2H625rVUyFZk8f4wggYHMIID76ADAgECAgphFmg0
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
# bWrJUnMTDXpQzTGCBLQwggSwAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCByDAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQUEpR73Dy3X3sHxdX4vKd6Jh0TS2wwaAYKKwYB
# BAGCNwIBDDFaMFigJoAkAEMAbwBtAG0AbwBuAC0AVQBwAGcAcgBhAGQAZQAuAHAA
# cwAxoS6ALGh0dHA6Ly93d3cuTWljcm9zb2Z0LmNvbS9NaWNyb3NvZnREeW5hbWlj
# cy8gMA0GCSqGSIb3DQEBAQUABIIBADD2JYAK6M7aP5o8y+V3Qd15UiitZNxKVJCr
# EwYEoM5B8OAIljGjMd9Cup3QCpXhiK3IV1zE2WwOtZI4Ys3kuj8hM0r/rJwbb7We
# /qc0v13kmTFb9Z2EJhrIiuZdU3Ds0O8jDpUL4crVAIm9Fz/8qAclJ92D9fKC8WUX
# 6LUPVxkx66lr9/UjRK1RrtGGPeTnPZ5fzKJTLOTRsymz046D0u3XfcTo7yw593ZW
# FYkIb/sPXiA7coNyg98rt4GN6vGWipTGyyVlkjaGHseM6ki7zdDD1q7tVQAiqbXj
# 8LJzg0MkyIMc9NWh7kttf4lyfHmp1cslvIq0K4TzxkUkfdVQd0qhggIoMIICJAYJ
# KoZIhvcNAQkGMYICFTCCAhECAQEwgY4wdzELMAkGA1UEBhMCVVMxEzARBgNVBAgT
# Cldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29m
# dCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENB
# AhMzAAAAnUJo7jEc11a9AAAAAACdMAkGBSsOAwIaBQCgXTAYBgkqhkiG9w0BCQMx
# CwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0xNjA3MjEyMTA3MTZaMCMGCSqG
# SIb3DQEJBDEWBBR6y6XhBaa6xk2WYVIkazxzyodZWjANBgkqhkiG9w0BAQUFAASC
# AQARt3qZUyYl++z3fQ5ZtNJSftiTIwx9QRu+o//abI87jy3c7pwM95eZXetfe6pf
# x4/OKMIiK2WnQrgLJEYbGJ4eY6fiaHm/U5/Z93H01CvgPcvZwJQ7GIM5wsXN1vbt
# j/OhNlZl9jzbAo7o3kIRDIW8IviXht/Jmc56XLcKhCdIrqOx9t3TKW3EFve7OLKg
# gAJ3aMdqPhbovuRdVeTb6S4I41SYLceLR8ak3y0JcBP0AUl9q+hVrzZB1ShDbMzH
# ExaNTQUe7/URdqiZF88P3DJnTjmWEyIQcRv3TjD9o8Rz1/ATmOMdTR3x/MLYuRQv
# SbV7MlfUquLQoTPsP5zmUm1w
# SIG # End signature block
