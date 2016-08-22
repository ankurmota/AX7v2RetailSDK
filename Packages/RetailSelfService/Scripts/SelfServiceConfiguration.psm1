<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
. $ScriptDir\Common-Configuration.ps1
. $ScriptDir\Common-Web.ps1

function Write-Log(
    $objectToLog = $(Throw 'objectToLog is required!'),
    [string] $LogFile)
{
    try
    {
        $date = (Get-Date -DisplayHint Time)
        $objectToLogString = $objectToLog |  Out-String -Width 4096
        $message = "{0}: {1}" -f $date, $objectToLogString

        # Write to log file and to console.
        if($LogFile)
        {
            $message | Out-File -FilePath $LogFile -Append -Force
        }

        Write-Host $message
    }
    catch
    {
        # swallow any log exceptions
    }
}

function Get-SelfServiceRegistryPath
{
    return 'HKLM:\SOFTWARE\Microsoft\Dynamics\{0}\RetailSelfService' -f (Get-ProductVersionMajorMinor)
}

function Get-SelfServicePkgLocationRegKeyName
{
    return 'SelfServicePackagesLocation'
}

function Get-SelfServiceScriptsLocationRegKeyName
{
    return 'SelfServiceScriptsLocation'
}

function Get-SelfServiceAOSWebsiteRegKeyName
{
    return 'AOSWebsiteName'
}

function Get-AOSWebsiteName
{
    $aosWebsiteName = 'AOSService'
    $aosWebsiteNameObjectFromRegistry = Get-ItemProperty -Path (Get-SelfServiceRegistryPath) -Name 'AOSWebsiteName' -ErrorAction SilentlyContinue
    
    if ($aosWebsiteNameObjectFromRegistry.AOSWebsiteName)
    {
        $aosWebsiteName = $aosWebsiteNameObjectFromRegistry.AOSWebsiteName
    }

    return $aosWebsiteName
}

function Get-WebsitePhysicalPath(
    [string]$webSiteName = $(Throw 'webSiteName is required!')
)
{
    $IISWebSiteObject = Get-WebSiteSafe -Name $webSiteName

    if(!$IISWebSiteObject)
    {
        Throw ("Unable to find a website: '{0}'. Verify that the website exists." -f $webSiteName)
    }
    
    return $IISWebSiteObject.physicalPath
}

function Get-AOSWebConfigFilePath(
    [string]$AOSWebsitePhysicalPath = $(Throw 'AOSWebsitePhysicalPath is required!')
)
{
    $webConfigFilePath =  Join-Path -Path $AOSWebsitePhysicalPath -ChildPath 'web.config'

    if(-not(Test-Path -Path $webConfigFilePath))
    {
        Throw ("Unable to locate Web.config for website: '{0}'. Verify that the website exists." -f $AOSWebsiteName)
    }

    return $webConfigFilePath
}

function Get-AXDeploymentUtilityFilesPath(
    [string]$AOSWebsitePhysicalPath = $(Throw 'AOSWebsitePhysicalPath is required!'),
    [string]$UtilityFileName = $(Throw 'UtilityFileName is required!')
)
{
    $UtilityFilePath =  Join-Path -Path $AOSWebsitePhysicalPath -ChildPath (Join-Path -Path 'bin' -ChildPath $UtilityFileName)

    if(-not(Test-Path -Path $UtilityFilePath))
    {
        Throw ("Unable to locate '{0}'" -f $UtilityFilePath)
    }

    return $UtilityFilePath
}

function Update-AXDeploymentUtilityConfigFile(
    [string]$AOSWebConfigFilePath = $(Throw 'AOSWebConfigFilePath is required!'),
    [string]$AXDeploymentUtilityConfigFilePath = $(Throw 'AXDeploymentUtilityConfigFilePath is required!'),
    [string]$LogFile = $(Throw 'LogFile is required!')
)
{
    # Load AOS web.config and AX Deployment Utility config files
    $aosWebConfigContent = [xml](Get-Content -Path $AOSWebConfigFilePath)
    $AXDeploymentUtilityConfigDoc = [xml](Get-Content -Path $AXDeploymentUtilityConfigFilePath)

    # Get Azure storage connection string from aos web.config file.
    $AzureStorageConnectionStringKey = 'AzureStorage.StorageConnectionString'
    $AzureStorageConnectionStringValue = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='{0}']" -f $AzureStorageConnectionStringKey).Value

    # Find the Azure storage connection string in post deployment utility config file.
    $AzureStorageConnectionStringKeyElement = $AXDeploymentUtilityConfigDoc.SelectSingleNode("/configuration/appSettings/add[@key='{0}']" -f $AzureStorageConnectionStringKey)
    Write-Log -objectToLog ('Azure storage connection string in xml config: {0}' -f ($AzureStorageConnectionStringKeyElement | Out-String)) -logFile $LogFile

    # Only add a new element if one doesn't already exist.
    if(!$AzureStorageConnectionStringKeyElement)
    {
        Write-Log -objectToLog 'Adding connection string to xml config.' -logFile $LogFile

        $AzureStorageConnectionStringKeyElement = $AXDeploymentUtilityConfigDoc.CreateElement('add')
        $AzureStorageConnectionStringKeyElement.SetAttribute('key', $AzureStorageConnectionStringKey)

        $AzureStorageConnectionStringKeyElement.SetAttribute('value', $AzureStorageConnectionStringValue )
        $AXDeploymentUtilityConfigDoc.configuration.appSettings.AppendChild($AzureStorageConnectionStringKeyElement)
    }
    else
    {
        Write-Log -objectToLog 'Modifying connection string in xml config.' -logFile $LogFile
        $AzureStorageConnectionStringKeyElement.Value = $AzureStorageConnectionStringValue
    }

    $AXDeploymentUtilityConfigDoc.Save($AXDeploymentUtilityConfigFilePath)
}

function Get-RequisiteParametersFromAosWebConfig(
    [string]$AOSWebConfigFilePath = $(Throw 'AOSWebConfigFilePath is required!')
)
{
    # The AosDatabasePass is potentially encrypted and needs to be decrypted for use.
    # Step 1: Create a copy of web.config
    $aosWebsiteRootFolder = Split-Path -Path $AOSWebConfigFilePath -Parent
    $duplicateWebConfigFilePath = Join-Path -Path $aosWebsiteRootFolder -ChildPath 'RetailSelfService.config'

    Copy-Item -Path $AOSWebConfigFilePath -Destination $duplicateWebConfigFilePath

    # Step 2: Decrypt web.config
    $configEncryptorExeName = 'Microsoft.Dynamics.AX.Framework.ConfigEncryptor.exe'
    $pathToConfigEncryptor = [System.IO.Path]::Combine($aosWebsiteRootFolder, 'bin', $configEncryptorExeName)

    try
    {
        & $pathToConfigEncryptor -decrypt $duplicateWebConfigFilePath
    }
    catch
    {
        # Only reason for entering this block is cause the web config is already decrypted.
        Write-Log -objectToLog 'Copied web config file is not encrypted.'
    }

    # Step 3: Read the web config, sample xml element: <add key="Aos.MetadataDirectory" value="F:\AosService\PackagesLocalDirectory" />
    $aosWebConfigContent = [xml](Get-Content -Path $duplicateWebConfigFilePath)

    $result = @{
        'BinDirectory'      = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='Common.BinDir']").value;
        'MetadataDirectory' = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='Aos.MetadataDirectory']").value;
        'AosDatabaseServer' = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='DataAccess.DbServer']").value;
        'AosDatabaseName'   = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='DataAccess.Database']").value;
        'AosDatabaseUser'   = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='DataAccess.SqlUser']").value;
        'AosDatabasePass'   = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='DataAccess.SqlPwd']").value;
    }

    # Step 4 : Delete the duplicate web config
    Remove-Item -Path $duplicateWebConfigFilePath -Force

    return $result
}

function Call-AXDeploymentSetupUtility(
    $parametersFromAosWebConfig = $(Throw 'parametersFromAosWebConfig is required!'),
    $methodInputXmlFilePath = $(Throw 'methodInputXmlFilePath is required!'),
    $AXDeploymentSetupUtilityFilePath = $(Throw 'AXDeploymentSetupUtilityFilePath is required!'),
    $className = 'RetailSelfServicePackageManager',
    $methodName = 'ProcessExternalRequests',
    $LogFile = $(Throw 'LogFile is required!')
)
{
    $metadataDirectory = $parametersFromAosWebConfig["MetadataDirectory"];

    if(-not (Test-Path -Path $MetadataDirectory))
    {
        $metadataDirectory = $parametersFromAosWebConfig["BinDirectory"]
    }

    # Generate array of parameters to pass to Deployment Setup Utility without password based parameters
    $axDeploymentCallArguments = @(
        '--isemulated',      'false',
        '--bindir',          $parametersFromAosWebConfig['BinDirectory'],
        '--metadatadir',     $metadataDirectory,
        '--sqlserver',       $parametersFromAosWebConfig['AosDatabaseServer'],
        '--sqldatabase',     $parametersFromAosWebConfig['AosDatabaseName'],
        '--sqluser',         $parametersFromAosWebConfig['AosDatabaseUser'],
        '--setupmode',       'RunStaticXppMethod',
        '--classname',       $className,
        '--methodname',      $methodName,
        '--methodinputfile', $methodInputXmlFilePath
    )
    
    Write-Log -objectToLog ('Calling {0}' -f $AXDeploymentSetupUtilityFilePath) -logFile $LogFile
    Write-Log -objectToLog ('Passing parameters {0}' -f ($axDeploymentCallArguments -join ' ')) -logFile $LogFile

    # Add password based parameters
    $axDeploymentCallArguments += ('--sqlpwd', $parametersFromAosWebConfig['AosDatabasePass'])
    
    $Global:LASTEXITCODE = 0
    $commandOutput = & $AXDeploymentSetupUtilityFilePath $axDeploymentCallArguments 2>&1 | Out-String

    $exitCode = $Global:LASTEXITCODE
    Write-Log -objectToLog ("Program Output: {0}" -f $commandOutput) -LogFile $logFile

    if ($exitCode -ne 0)
    {
        throw 'Exception occured during execution of call to DeploymentSetupUtility. Please see the log for further details.'
    }
}

Export-ModuleMember -Function Write-Log
Export-ModuleMember -Function Get-SelfServiceRegistryPath
Export-ModuleMember -Function Get-SelfServicePkgLocationRegKeyName

Export-ModuleMember -Function Get-SelfServiceScriptsLocationRegKeyName
Export-ModuleMember -Function Get-SelfServiceAOSWebsiteRegKeyName
Export-ModuleMember -Function Get-WebsitePhysicalPath

Export-ModuleMember -Function Get-AOSWebsiteName
Export-ModuleMember -Function Get-AOSWebConfigFilePath
Export-ModuleMember -Function Get-AXDeploymentUtilityFilesPath

Export-ModuleMember -Function Update-AXDeploymentUtilityConfigFile
Export-ModuleMember -Function Get-RequisiteParametersFromAosWebConfig
Export-ModuleMember -Function Call-AXDeploymentSetupUtility
# SIG # Begin signature block
# MIIdxAYJKoZIhvcNAQcCoIIdtTCCHbECAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUyWxGD36Y3hIyjuQFA4TN5yOF
# zDOgghhkMIIEwzCCA6ugAwIBAgITMwAAAJgEWMt/IwmwngAAAAAAmDANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTYwMzMwMTkyMTI3
# WhcNMTcwNjMwMTkyMTI3WjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OjdBRkEtRTQxQy1FMTQyMSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1jclqAQB7jVZ
# CvOuH5jFixrRTGFtwMHws1sEZaA3ciobVIdWIejc5fBu3XdwRLfxjsmyou3JeTaa
# 8lqA929q2AyZ9A3ZBfxf8VqTxbu06wBj4o4g5YCsz0C/81N2ESsQZbjDxbW5sKzD
# hhT0nTzr82aepe1drjT5dvyU/AvEkCzaEDU0dZTq2Bm6NIif11GzA+OkD0bdZG+u
# 4EJRylQ4ijStGgXUpAapb0y2RtlAYvZSpLYzeFFcA/yRXacCnoD++h9r66he/Scv
# Gfd/J/5hPRCtgsbNr3vFJzBWgV9zVqmWOvZBPGpLhCLglTh0stPa/ZxZjTS/nKJL
# a7MZId131QIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFPPCI5/SvSWNvaj1nBvoSHO7
# 6ZPBMB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBAD+xPVIhFl30XEe39rlgUqCCr2fXR9o0aL0Oioap6LAUMXLK
# 4B+/L2c+BgV32joU6vMChTaA+7XEw7pXCRN+uD8ul4ifHrdAOEEqOTBD7N5203u2
# LN667/WY71purP2ezNB1y+YAgjawEt6VjjQcSGZ9bTPRtS2JPS5BS868paym355u
# 16HMxwmhlv1klX6nfVOs1DYK5cZUrPAblCZEWzGab8j9d2ZIGLQmTEmStdslOq79
# vujEI0nqDnJBusUGi28Kh3Hz1QIHM5UZg/F5sWgt0EobFGHmk4KH2vreGZArtCIB
# amDc5cIJ48na9GfA2jqJLWsbvNcwC486g5cauwkwggYHMIID76ADAgECAgphFmg0
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
# bWrJUnMTDXpQzTGCBMowggTGAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCB3jAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQU151PR/wc1Nx37ytL9RQ96cF83LcwfgYKKwYB
# BAGCNwIBDDFwMG6gPIA6AFMAZQBsAGYAUwBlAHIAdgBpAGMAZQBDAG8AbgBmAGkA
# ZwB1AHIAYQB0AGkAbwBuAC4AcABzAG0AMaEugCxodHRwOi8vd3d3Lk1pY3Jvc29m
# dC5jb20vTWljcm9zb2Z0RHluYW1pY3MvIDANBgkqhkiG9w0BAQEFAASCAQBzP9DM
# DgNEoarrprJ0de9nG8JpTRd2QUxZ2B8wl6AsUqCjkX/h8KI3/Zvdt8hT8MNKkhe6
# h6LmthDSZ+rJnb08mPEwe+ThH01Eb+HjAKcvyPuZk54FH7/D8JKwhfAR+/2ItbLP
# 7EscjPh+/jgIXTl9GacWmpPnTDQBGrIS03/ls7bkv37A2MWHMLHnD/yGVUGYa5B5
# NB8/SzjZ24Yal3s9KJKWrwALoFUCa9nlrA4FuRIOxNv2cxEzFKjBmUjlTzQKDKUJ
# Rr7FeQ82FD39Xc5oyQ2CTK74N17d/5+F1+74IPIilklVF9Sb6oquaMg5W95Nk15L
# hyrO4si2xDZ3ihVPoYICKDCCAiQGCSqGSIb3DQEJBjGCAhUwggIRAgEBMIGOMHcx
# CzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRt
# b25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAfBgNVBAMTGE1p
# Y3Jvc29mdCBUaW1lLVN0YW1wIFBDQQITMwAAAJgEWMt/IwmwngAAAAAAmDAJBgUr
# DgMCGgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUx
# DxcNMTYwODA2MDg1MzIxWjAjBgkqhkiG9w0BCQQxFgQUgNxdoO9ET0SmYBJTzdHF
# PzOySX0wDQYJKoZIhvcNAQEFBQAEggEAWhoKBjtCbcnLo/HuIWN/E5+Zvwav7Jtk
# IE/VHAhXuwCXDn2Fo3H8piza8JbAaeqkma29bWKpMBZO5EtFJXotbKW7OnmB4kB7
# GLB8G8XQg8xYWBQCGPu4WyAdg33D/g6RwscKY7nfZCnY66hpIEyctdPoFnuGruoa
# zk/7b2S9iwGv/P9B0zCWRHlUkqk/o7hzO0vAqKUs0G3X9/p90iFrW8Drg+i9x8Ga
# MKS2U4FXBhEToskwmMnpsF4EkLkFRIu//eN9EofhnGzwejzW2oC2WYGbq2cp1C7V
# smbikIUt5Nbpyhd9hFrmHdYey2tazYAm2CjHt8WBjtzRujoDqCqkVA==
# SIG # End signature block
