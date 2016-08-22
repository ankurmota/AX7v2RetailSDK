<#
.EXAMPLE
E:\RainMain\Test\BVT\Setup\CallRetailPostDeploymentConfigurationService.ps1 -PathToCallerFolder 'C:\CustomerServiceUnit\Dobind\Packages\Cloud\AosWebApplication\AosWebApplication.csx\roles\AosWeb\approot\bin' -BinDirectory 'C:\Packages' -MetadataDirectory 'C:\Packages' -AosDatabaseUser 'sqluser' -AosDatabasePass 'Microsoft1!'

.EXAMPLE
E:\RainMain\Test\BVT\Setup\CallRetailPostDeploymentConfigurationService.ps1 -PathToCallerFolder 'C:\CustomerServiceUnit\Dobind\Packages\Cloud\AosWebApplication\AosWebApplication.csx\roles\AosWeb\approot\bin' -BinDirectory 'C:\Packages' -MetadataDirectory 'C:\Packages' -AosDatabaseServer 'ADS' -AosDatabaseName 'ADN' -AosDatabaseUser 'ADU' -AosDatabasePass 'ADP' -ExecuteRunSeedDataGenerator -ExecuteConfigureAsyncService -ChannelDatabaseServer 'CDS' -ChannelDatabaseName 'CDN' -ChannelDatabaseUser 'CDU' -ChannelDatabasePass 'CDP' -ExecuteConfigureRealTimeService -AosUrl 'https://usncmultiboxax1aos.cloud.onebox.dynamics.com/' -ExecuteRunCdxJobs

.EXAMPLE
E:\RainMain\Test\BVT\Setup\CallRetailPostDeploymentConfigurationService.ps1 -PathToCallerFolder 'C:\ConsoleApplication1\ConsoleApplication1\bin\Debug' -CallerExeName 'ConsoleApplication1.exe' -BinDirectory 'C:\Packages' -MetadataDirectory 'C:\Packages' -AosDatabaseServer 'ADS' -AosDatabaseName 'ADN' -AosDatabaseUser 'ADU' -AosDatabasePass 'ADP' -ExecuteRunSeedDataGenerator -ExecuteConfigureAsyncService -ChannelDatabaseServer 'CDS' -ChannelDatabaseName 'CDN' -ChannelDatabaseUser 'CDU' -ChannelDatabasePass 'CDP' -ExecuteConfigureRealTimeService -AosUrl 'https://usncmultiboxax1aos.cloud.onebox.dynamics.com/'
#>
param(
    # Executable parameters       
    [string]$IsServiceModelDeployment = "false",

    [string]$PathToCallerFolder,

    [string]$CallerExeName = 'Microsoft.Dynamics.AX.Deployment.Setup.exe',

    [string]$BinDirectory,

    [string]$MetadataDirectory = $BinDirectory,

    [string]$AosWebsiteName = 'AOSService',

    [string]$AosDatabaseServer = '.',

    [string]$AosDatabaseName = 'AXDBRAIN',

    [string]$AosDatabaseUser = 'AOSUser',

    [string]$AosDatabasePass = 'AOSWebSite@123',

    [string]$SetupMode = 'RunStaticXppMethod',

    [string]$ClassName = 'RetailPostDeploymentConfiguration',

    [string]$MethodName = 'Apply',

    # Method input parameters
    [switch]$ExecuteRunSeedDataGenerator,

    [switch]$ExecuteConfigureAsyncService,

    [string]$ChannelDatabaseServer = '',

    [string]$ChannelDatabaseName = '',

    [string]$ChannelDatabaseUser = '',

    [string]$ChannelDatabasePass = '',

    [string]$ChannelDatabaseDataSyncUser = '',

    [string]$ChannelDatabaseDataSyncPass = '',

    [string]$DataGroupName = 'Default',

    [string]$DataGroupDescription = 'Default data group',

    [string]$DatabaseProfileName = 'Default',

    [switch]$ExecuteConfigureRealTimeService,

    [string]$AosUrl = 'https://usnconeboxax1aos.cloud.onebox.dynamics.com',

    [string]$IdentityProvider = 'https://sts.windows.net/',

    [string]$UserId = 'RetailServerSystemAccount@dynamics.com',

    [string]$AosAdminUserId = 'Tusr1@TAEOfficial.ccsctp.net',

    [string]$RetailChannelProfileName = 'Retail server channel profile',

    [string]$RetailServerUrl = 'https://usnconeboxax1ret.cloud.onebox.dynamics.com/RetailServer/Commerce',

    [string]$MediaServerUrl = 'https://usnconeboxax1ret.cloud.onebox.dynamics.com/RetailServer/MediaServer',

    [string]$CloudPOSUrl = 'https://usnconeboxax1pos.cloud.onebox.dynamics.com',

    [string]$AudienceUrn = 'spn:00000015-0000-0000-c000-000000000000',

    [string]$RtsProfileId = 'Default',

    [switch]$ExecuteRunCdxJobs,

    [switch]$ConfigureSelfService,

    [string]$RetailSideloadingKey,

    [string]$DisableDBServerCertificateValidation
)

trap 
{
    Write-Output ($Error | Format-List * -Force | Out-String -Width 1024)
    exit 2
}

$Error.Clear()
$ErrorActionPreference = 'Stop'

Write-Output 'Script started execution.'

if($IsServiceModelDeployment -ne "true") # Read values from registry which will be populated by AxSetup.exe
{
	$DynDeploymentRegistryRoot = 'HKLM:\SOFTWARE\Microsoft\Dynamics\Deployment'
	$PathToCallerFolder = (Get-ItemProperty -Path $DynDeploymentRegistryRoot  `
		| Select-Object -ExpandProperty DeploymentDirectory `
		| Join-Path -ChildPath 'Dobind\Packages\Cloud\AosWebApplication\AosWebApplication.csx\roles\AosWeb\approot\bin')
	$BinDirectory = (Get-ItemProperty -Path $DynDeploymentRegistryRoot | Select-Object -ExpandProperty BinDir)
	$MetadataDirectory = $BinDirectory
}
else # For ServiceModelDeployment the registry values won't be populated in multibox, caller script will read the values from AOS web.config and pass the values in. 
{
	
}

$pathToCaller = Join-Path -Path $PathToCallerFolder -ChildPath $CallerExeName

if (-not(Test-Path -Path $pathToCaller -PathType Leaf))
{
    $message = 'Executable {0} does not exist in the folder {1}' -f $CallerExeName, $PathToCallerFolder
    throw $message
}

if ($AosAdminUserId.ToLower().EndsWith('.ccsctp.net')) 
{ 
	$IdentityProvider = 'https://sts.windows.net/'
}

$methodInputXmlFilePath = [System.IO.Path]::GetTempFileName();

Write-Output ('Method input xml file is located in {0}' -f $methodInputXmlFilePath)

$configureAsyncServiceSection =
@"
	<ConfigureAsyncService>
        <ChannelDatabaseServer>{0}</ChannelDatabaseServer>
        <ChannelDatabaseName>{1}</ChannelDatabaseName>
        <ChannelDatabaseUser>{2}</ChannelDatabaseUser>
        <ChannelDatabasePass>{3}</ChannelDatabasePass>
		<DataGroupName>{4}</DataGroupName>
        <DataGroupDescription>{5}</DataGroupDescription>
        <DatabaseProfileName>{6}</DatabaseProfileName>
        <TrustServerCertificate>{7}</TrustServerCertificate>
    </ConfigureAsyncService>
"@ -f $ChannelDatabaseServer, $ChannelDatabaseName, $ChannelDatabaseDataSyncUser, $ChannelDatabaseDataSyncPass, $DataGroupName, $DataGroupDescription, $DatabaseProfileName, $DisableDBServerCertificateValidation

$AosUrl = $AosUrl.trimEnd('/')

$configureRealTimeServiceSection =
@"
	<ConfigureRealTimeService execute="{0}">
        <AosUrl>{0}</AosUrl>
        <IdentityProvider>{1}</IdentityProvider>
        <UserId>{2}</UserId>
        <AudienceUrn>{3}</AudienceUrn>
		<AosAdminUserId>{4}</AosAdminUserId>
        <RtsProfileId>{5}</RtsProfileId>
    </ConfigureRealTimeService>
"@ -f $AosUrl, $IdentityProvider, $UserId, $AudienceUrn, $AosAdminUserId, $RtsProfileId

$configureSelfServiceSection =
@"
	<ConfigureRetailSelfService>
        <RetailSideloadingKey>{0}</RetailSideloadingKey>
    </ConfigureRetailSelfService>
"@ -f $RetailSideloadingKey

$operationsToExecute = 'skipRunSeedDataGenerator="{0}" skipConfigureAsyncService="{1}" skipConfigureRealTimeService="{2}" skipRunCdxJobs="{3}" skipConfigureSelfService="{4}"' -f
    (-not $ExecuteRunSeedDataGenerator.ToBool()), (-not $ExecuteConfigureAsyncService.ToBool()), (-not $ExecuteConfigureRealTimeService.ToBool()), 
    (-not $ExecuteRunCdxJobs.ToBool()), (-not $ConfigureSelfService.ToBool())
	
$configureChannelProfileSection =
@"
	<ConfigureChannelProfile>
        <RetailChannelProfileName>{0}</RetailChannelProfileName>
        <RetailServerUrl>{1}</RetailServerUrl>
        <MediaServerUrl>{2}</MediaServerUrl>
		<CloudPOSUrl>{3}</CloudPOSUrl>
    </ConfigureChannelProfile>
"@ -f $RetailChannelProfileName, $RetailServerUrl, $MediaServerUrl, $CloudPOSUrl

$methodInputXmlString = 
@'
<?xml version="1.0" encoding="UTF-8"?>
<Configuration {0}>
    {1}
    {2}
    {3}
	{4}
</Configuration>
'@ -f $operationsToExecute, $configureAsyncServiceSection, $configureRealTimeServiceSection, $configureSelfServiceSection, $configureChannelProfileSection

$methodInputXml = New-Object System.Xml.XmlDocument;
$methodInputXml.LoadXml($methodInputXmlString);

Write-Output 'Saving method input to xml file ...'

$methodInputXml.Save($methodInputXmlFilePath);

Write-Output 'Saved.'

$arguments = @(
    "--isemulated", "false",
    "--bindir", $BinDirectory,
    "--metadatadir", $MetadataDirectory,
    "--sqlserver", $AosDatabaseServer,
    "--sqldatabase", $AosDatabaseName,
    "--sqluser", $AosDatabaseUser,
    "--sqlpwd", $AosDatabasePass,
    "--setupmode", $SetupMode,
    "--classname", $ClassName,
    "--methodname", $MethodName,
    "--methodinputfile", $methodInputXmlFilePath
);

Write-Output ('Calling {0} ...' -f $pathToCaller)

$ErrorActionPreference = 'Continue'

& $pathToCaller $arguments *>&1 | Tee-Object "$PSScriptRoot\CallRetailPostDeploymentConfigurationService.log"
$exitCode = $Global:LASTEXITCODE

$ErrorActionPreference = 'Stop'

Remove-Item $methodInputXmlFilePath  -Force

Write-Output ('Execution completed with exit code {0}' -f $exitCode)
Write-Output $exitCode
return $exitCode
# SIG # Begin signature block
# MIId7gYJKoZIhvcNAQcCoIId3zCCHdsCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUfaK3CGRwJVyxMAEqvS3NW3VS
# pyWgghhkMIIEwzCCA6ugAwIBAgITMwAAAJgEWMt/IwmwngAAAAAAmDANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBPQwggTwAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCCAQcwGQYJ
# KoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQB
# gjcCARUwIwYJKoZIhvcNAQkEMRYEFC0Ve7/HHSpYw/wobNhL93wYTW9PMIGmBgor
# BgEEAYI3AgEMMYGXMIGUoGKAYABDAGEAbABsAFIAZQB0AGEAaQBsAFAAbwBzAHQA
# RABlAHAAbABvAHkAbQBlAG4AdABDAG8AbgBmAGkAZwB1AHIAYQB0AGkAbwBuAFMA
# ZQByAHYAaQBjAGUALgBwAHMAMaEugCxodHRwOi8vd3d3Lk1pY3Jvc29mdC5jb20v
# TWljcm9zb2Z0RHluYW1pY3MvIDANBgkqhkiG9w0BAQEFAASCAQAsvZbs2w3R2Tvx
# uY0aW1EAuaOh6XycoRuQow769lGEcSIZ3kLuEQLEDgujjCZIuaGvLV7y4cTR3T35
# pf2MKkBrrTbYxaYWEKhOmKv2HHZVJHGdE7C1LnAdftOutzcoSZK0pROcZXbDPWn6
# SMQCnyn0TDXsQTSa31YADShUHhGe/Xy9wKmSUQxeWuGFA8VsN6X/upKghWyNY7O2
# GdWsbkJGzqp7FgbBhZlRoGJm+v7G61qU8EMmlJ4GWk7SB9CgVeWI7/IW5ymXgQuG
# V5OtuScRNkLqlQwshrmqaNp+uwXNehRbxwniiCywSWUzNSjuu9m52QlNTtru/Ebt
# l+bLAYFZoYICKDCCAiQGCSqGSIb3DQEJBjGCAhUwggIRAgEBMIGOMHcxCzAJBgNV
# BAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
# HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAfBgNVBAMTGE1pY3Jvc29m
# dCBUaW1lLVN0YW1wIFBDQQITMwAAAJgEWMt/IwmwngAAAAAAmDAJBgUrDgMCGgUA
# oF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcNMTYw
# ODA2MDg1MjE5WjAjBgkqhkiG9w0BCQQxFgQUbtejAFNnr9ToepQ5tUUD2kyB+kQw
# DQYJKoZIhvcNAQEFBQAEggEAEm96h2AaZFdw19bQDnmEQsp89rRPKbOHrHXz4GDi
# 4cvN66QEOWDuDxXj7f057LLQZaX9xypacsRBbx6lMvc8fT0UEZ5nXlyDkNaIbPcp
# aX+rJ/1AR6tcBpw7gZHJxRKmxCfVVamWYRNykXHttg8G6rXae4OgG98cquh1mSlE
# XUxp1iaz8oEX086LgXSNlnO0qYCCNeKKujg4Im5GkmFm8eZPDZVpPS5X73MAAot5
# ivfF/dG3f59CqDmzWQP0pOv2qAKVkVORsWo8jnm6jUZbMav7dVt9+AYAq1GIzqri
# 709T4ilJv72/oS3SlQsSK32nQ7SEyhUIsn7JtBKj8WT4Hw==
# SIG # End signature block
