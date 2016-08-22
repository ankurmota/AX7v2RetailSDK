<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
. $scriptDir\Common-Web.ps1
. $scriptDir\Common-Configuration.ps1

function CheckIf-CertificateExistInTargetStore(
    [string]$certificateThumbprint = $(throw 'certificateThumbprint is required.'),
    [string]$certificateStore = 'My',
    [string]$certificateRootStore = 'LocalMachine'
)
{
    $queryForCertificateInTargetStore = Get-ChildItem -Path Cert:\$certificateRootStore\$certificateStore | Where Thumbprint -eq $certificateThumbprint
    return ($queryForCertificateInTargetStore -ne $null)
}

function Install-Certificate(
    [ValidateNotNullOrEmpty()]
    [string]$certificateFilePath = $(Throw 'certificateFilePath parameter required.'),

    [Security.SecureString]$secureCertificatePassword = $(Throw 'secureCertificatePassword parameter required.'),
    [string]$certificateStore = 'My',
    [string]$certificateRootStore = 'LocalMachine',
    
    [ValidateNotNullOrEmpty()]
    [string]$expectedThumbprint,
    [string]$logFile = $(Throw 'logFile parameter required.')
)
{
    # Check if pfx file exists.
    if (-not (Test-Path -Path $certificateFilePath))
    {
        Throw-Error ('Unable to locate certificate file {0}' -f $certificateFilePath)
    }

    try
    {
        [int]$keyStorageFlags = 0
        $keyStorageFlags = $keyStorageFlags -bor ([int]([System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable))
        $keyStorageFlags = $keyStorageFlags -bor ([int]([System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::MachineKeySet))
        $keyStorageFlags = $keyStorageFlags -bor ([int]([System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet))

        # Check if certificate has already been installed.
        $certificateToInstall = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 -ArgumentList @($certificateFilePath, $secureCertificatePassword, $keyStorageFlags)

        Write-Log ('Check if certificate already exits in target store {0}\{1}.' -f $certificateRootStore, $certificateStore) $logFile
        $doesCertificateExistInTargetStore = CheckIf-CertificateExistInTargetStore -certificateThumbprint $certificateToInstall.Thumbprint -certificateStore $certificateStore

        # Validate that the certificate thumbprint from pfx file matches expectedThumbprint.
        if ($certificateToInstall.Thumbprint -ne $expectedThumbprint)
        {
            throw ('Certificate thumbprint from pfx file {0} did not match expected thumbprint {1}.' -f $certificateToInstall.Thumbprint, $expectedThumbprint)
        }
        
        # If certificate is not present in the store then proceed with installation.
        if (-not($doesCertificateExistInTargetStore))
        {
            Write-Log ('Installing certificate to {0}\{1}.' -f $certificateRootStore, $certificateStore) $logFile
            $X509Store = New-Object System.Security.Cryptography.X509Certificates.X509Store($certificateStore, $certificateRootStore)

            $X509Store.Open([Security.Cryptography.X509Certificates.OpenFlags]::MaxAllowed);
            $X509Store.Add($certificateToInstall);
            $X509Store.Close()
            Write-Log ('Successfully installed certificate with thumbprint {0}.' -f $certificateToInstall.Thumbprint) $logFile
        }
        else
        {
            Write-Log ('Certificate with thumbprint {0} already exists in target store. Skipping install.' -f $certificateToInstall.Thumbprint) $logFile
        }
    }
    catch
    {
        Throw-Error $_
    }
}

function Get-BackupFolderPath(
    [string]$folderToBackup = $(Throw 'folderToBackup parameter is required.')
)
{
    $currentTimeStamp = Get-Date -format "MM-dd-yyyy_HH-mm-ss"
    return (Join-Path -Path $folderToBackup -ChildPath ('Backup_{0}' -f $currentTimeStamp))
}

function ModifyUser-InChannelDbConnectionString(
    [string]$existingConnectionString = $(throw 'existingConnectionString is required.'),
    [string]$newChannelDbUser = $(throw 'newChannelDbUser is required.'),
    [string]$newChannelDbUserPassword = $(throw 'newChannelDbUserPassword is required.')
)
{
    $connectionStringBuilderAsReference = New-Object System.Data.SqlClient.SqlConnectionStringBuilder -ArgumentList $existingConnectionString
    $newConnectionString = Generate-ChannelDbConnectionString -serverName $connectionStringBuilderAsReference.'Server' `
                                                              -databaseName $connectionStringBuilderAsReference.'Database' `
                                                              -channelDatabaseUser $newChannelDbUser `
                                                              -channelDatabaseUserPassword $newChannelDbUserPassword `
                                                              -encrypt $connectionStringBuilderAsReference.Encrypt `
                                                              -trustServerCertificate $connectionStringBuilderAsReference.TrustServerCertificate
    return $newConnectionString
}

function Generate-ChannelDbConnectionString(
    [string]$serverName = $(throw 'serverName is required.'),
    [string]$databaseName = $(throw 'databaseName is required.'),
    [string]$channelDatabaseUser = $(throw 'channelDatabaseUser is required.'),
    [string]$channelDatabaseUserPassword = $(throw 'channelDatabaseUserPassword is required.'),
    [string]$encrypt = 'True',
    [string]$trustServerCertificate = 'False'
)
{
    $channelDbConnectionString = 'Application Name=Retail Server;Server="{0}";Database="{1}";User ID="{2}";Password="{3}";Trusted_Connection=False;Encrypt={4};TrustServerCertificate={5};' -f $serverName, `
                                        $databaseName, `
                                        $channelDatabaseUser, `
                                        $channelDatabaseUserPassword, `
                                        $encrypt, `
                                        $trustServerCertificate
    return $channelDbConnectionString
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
    if (-not(Test-Path -Path $targetRegistryKeyPath))
    {
        New-Item -Path $targetRegistryKeyPath -ItemType Directory -Force
    }

    New-ItemProperty -Path $targetRegistryKeyPath -Name $targetPropertyName -Value $servicingDataAsEncryptedString -Force
}
##############################################################
#
#    Rotate Auth data custom deployable package functions.
#
##############################################################
function Load-RotationInfoModule(
    [string]$packageRootPath = $(Throw 'packageRootPath parameter is required.')
)
{
    $serviceTemplateFolderPath = Join-Path -Path $packageRootPath -ChildPath 'RotateConfigData'
    $pathToRotationInfoModuleDllParent = Join-Path -Path $serviceTemplateFolderPath -ChildPath (Join-Path -Path 'Scripts' -ChildPath 'RotationInfo')
    $pathToRotationInfoModuleDll = Join-Path -Path $pathToRotationInfoModuleDllParent -ChildPath 'RotationInfoModule.dll'
    
    if (-not(Test-Path -Path $pathToRotationInfoModuleDll))
    {
        throw 'Unable to locate DLL file RotationInfoModule.'
    }
    
    # Import RotationInfoModule.Dll
    Import-Module $pathToRotationInfoModuleDll
    Add-Type -Path $pathToRotationInfoModuleDll
}

function Get-RotationInfoDecryptor(
    $rotationInfo = $(Throw 'rotationInfo parameter is required.')
)
{
    $rotationInfoDecryptor = New-Object Microsoft.Dynamics.AX.Servicing.Rotation.Decryptor $rotationInfo
    return $rotationInfoDecryptor
}

function Get-RotationInfo(
    [string]$serviceModelName = $(Throw 'serviceModelName parameter required.'),
    [string]$packageRootPath = $(Throw 'packageRootPath parameter required.')
)
{
    # Locate and validate the template file.
    $serviceTemplateFolderPath = Join-Path -Path $packageRootPath -ChildPath 'RotateConfigData'
    $rotationInfo = Get-ServicingRotationInfo -Name $serviceModelName -Path $serviceTemplateFolderPath

    if ($rotationInfo.EncryptionThumbprint -eq $null)
    {
        throw ('Servicing template information for {0} has not been encrypted.' -f $serviceModelName)
    }
    
    return $rotationInfo
}

function GetValueFor-CertificateThumbprintWithKey(
    [string]$key = $(Throw 'key parameter required.'),
    $rotationInfo = $(Throw 'rotationInfo parameter required.')
)
{
    $certificateThumbprintObject = $rotationInfo.CertificateThumbprints | Where {$_.Key -eq $key}
    return $certificateThumbprintObject.Value
}

function GetValueFor-KeyValuePairWithKey(
    [string]$key = $(Throw 'key parameter required.'),
    $rotationInfo = $(Throw 'rotationInfo parameter required.'),
    $rotationInfoDecryptor = $(Throw 'rotationInfoDecryptor parameter required.')
)
{
    $keyValuePairObject = $rotationInfo.KeyValues | Where {$_.Key -eq $key}
    $rotationInfoDecryptor.Decrypt($keyValuePairObject)
    
    return $keyValuePairObject.Value
}

function CheckIf-UserProvidedValidCredentialData(
    [string]$credentialString
)
{
    $result = $true
    if ([System.String]::IsNullOrWhiteSpace($credentialString))
    {
        $result = $false
    }
    if (($credentialString -eq '[userId]') -or ($credentialString -eq '[Password]'))
    {
        $result = $false
    }
    
    return $result
}

function Update-RetailWebsiteSSLBinding(
    [string]$websiteName = $(Throw 'websiteName parameter required.'),
    [string]$webAppPoolUser = $(Throw 'webAppPoolUser parameter required.'),
    [string]$certificateThumbprint = $(Throw 'certificateThumbprint parameter required.'),
    [string]$logFile = $(Throw 'logFile parameter required.')
)
{
    # Check if any https bindings exist for the website. If any exist we need to update the SSL binding.
    $websiteBindings = Get-WebBindingSafe -Name $websiteName -Protocol 'https'
    if ($websiteBindings)
    {
        foreach ($websiteBinding in $websiteBindings)
        {
            $websiteBindingProperties = Get-WebBindingProperties -Binding $websiteBinding
            $httpsPort = $websiteBindingProperties.Port

            # Update the website SSL binding.
            $websiteConfig = Create-WebSiteConfigMap -httpsPort $httpsPort -certificateThumbprint $certificateThumbprint -websiteAppPoolUsername $webAppPoolUser
            Invoke-ScriptAndRedirectOutput -scriptBlock {Add-SslBinding -WebSiteConfig $websiteConfig} -logFile $logFile
        }
    }
    else
    {
        Write-Log ('Website {0} does not use https. Skipping SSL binding update.' -f $websiteName) -logFile $logFile
    }
}

function InstallCertificatesAndUpdateIIS(
    $rotationInfo = $(Throw 'rotationInfo parameter required.'),
    $rotationInfoDecryptor = $(Throw 'rotationInfoDecryptor parameter required.'),

    [string]$packageRootPath = $(Throw 'packageRootPath parameter required.'),
    [boolean]$updateWebsiteSSLBinding = $false,
    [string]$websiteName,
    [string]$webAppPoolUser,
    [string]$certRootStore = 'LocalMachine',
    [string]$logFile = $(Throw 'logFile parameter required.')
)
{
    $pathToCertsFolder = Join-Path -Path (Join-Path -Path $packageRootPath -ChildPath 'RotateConfigData') -ChildPath 'Cert'

    # Read certificate information and install certificates not previously installed.
    foreach ($certificate in $rotationInfo.Certificates)
    {
        $rotationInfoDecryptor.Decrypt($certificate)
        $secureCertificatePassword = ConvertTo-SecureString $certificate.Password -AsPlainText -Force

        $certificateFilePath = Join-Path -Path $pathToCertsFolder -ChildPath $certificate.PfxFile
        $certificateStore = $certificate.CertStore
        $certificateThumbprint = $certificate.Thumbprint

        # Check if the path to certificate is valid. If invalid, no action required.
        if (Test-Path -Path $certificateFilePath)
        {
            # Import certificate. This will only import certificates not previously installed.
            Install-Certificate -certificateFilePath $certificateFilePath `
                                -secureCertificatePassword $secureCertificatePassword `
                                -certificateStore $certificateStore `
                                -expectedThumbprint $certificateThumbprint `
                                -logFile $logFile

            # Update the website binding, if specified.
            if ($updateWebsiteSSLBinding)
            {
                # Check if any https bindings exist for the website. If any exist we need to update the SSL binding.
                Update-RetailWebsiteSSLBinding -websiteName $websiteName `
                                               -webAppPoolUser $webAppPoolUser `
                                               -certificateThumbprint $certificateThumbprint `
                                               -logFile $logFile
            }
            else
            {
                # If we aren't updating the SSL binding, we still need to update permissions for the certificate.
                GrantPermission-UserToSSLCert -certThumbprint $certificateThumbprint -certRootStore $certRootStore -certStore $certificateStore -appPoolUserName $webAppPoolUser
            }
        }
        else
        {
            $logMessage = 'No valid certificates located. Skipping certificate installation.'
            if ($updateWebsiteSSLBinding)
            {
                $logMessage = 'No valid SSL certificates located. Skipping certificate installation and website SSL binding update.'
            }

            Write-Log $logMessage -logFile $logFile
        }
    }
}

function Get-RobocopyOptions(
    [switch]$includeEmptySubFolders
)
{
    # No job headers, summary and progress. No directory listing and 360 retries with 5 sec waits.
    $robocopyOptions = "/NJS /NP /NJH /NDL /R:360 /W:5"
    
    if ($includeEmptySubFolders)
    {
        $robocopyOptions += " /E"
    }
    
    return $robocopyOptions
}

function Encrypt-WebConfigSection(
    [string]$webConfigSection = $(throw 'webConfigSection is required.'),
    $websiteId = $(throw 'websiteId is required.'),
    [string]$targetWebApplicationPath = '/',
    [string]$logFile = $(throw 'logFile is required.')
)
{
    Write-Log 'Encrypting target web.config .' $logFile
    $global:LASTEXITCODE = 0

    aspnet_regiis -pe $webConfigSection -app $targetWebApplicationPath -Site $websiteId | Tee-Object -FilePath $logFile -Append
    $capturedExitCode = $global:LASTEXITCODE

    if($capturedExitCode -eq 0)
    {
        Write-Log 'Web.config encryption completed successfully.' $logFile
    }
    else
    {
        throw "Web.config encryption failed with encryption exit code: $capturedExitCode"
    }
}

function Decrypt-WebConfigSection(
    [string]$webConfigSection = $(throw 'webConfigSection is required.'),
    $websiteId = $(throw 'websiteId is required.'),
    [string]$targetWebApplicationPath = '/',
    [string]$logFile = $(throw 'logFile is required.')
)
{
    Write-Log 'Decrypting web.config' $logFile
    $global:LASTEXITCODE = 0

    aspnet_regiis -pd $webConfigSection -app $targetWebApplicationPath -Site $websiteId | Tee-Object -FilePath $logFile -Append
    $capturedExitCode = $global:LASTEXITCODE

    if($capturedExitCode -eq 0)
    {
        Write-Log 'Web.config decryption completed successfully.' $logFile
    }
    else
    {
        throw "Web.config decryption failed with decryption exit code: $capturedExitCode"
    }
}

Export-ModuleMember -Function CheckIf-CertificateExistInTargetStore
Export-ModuleMember -Function Install-Certificate
Export-ModuleMember -Function Get-BackupFolderPath
Export-ModuleMember -Function ModifyUser-InChannelDbConnectionString
Export-ModuleMember -Function Generate-ChannelDbConnectionString
Export-ModuleMember -Function Save-ChannelDbServicingDataToRegistry
Export-ModuleMember -Function Load-RotationInfoModule
Export-ModuleMember -Function Get-RotationInfoDecryptor
Export-ModuleMember -Function Get-RotationInfo
Export-ModuleMember -Function GetValueFor-CertificateThumbprintWithKey
Export-ModuleMember -Function GetValueFor-KeyValuePairWithKey
Export-ModuleMember -Function CheckIf-UserProvidedValidCredentialData
Export-ModuleMember -Function Update-RetailWebsiteSSLBinding
Export-ModuleMember -Function InstallCertificatesAndUpdateIIS
Export-ModuleMember -Function Get-RobocopyOptions
Export-ModuleMember -Function Encrypt-WebConfigSection
Export-ModuleMember -Function Decrypt-WebConfigSection
Export-ModuleMember -Function Write-Log
Export-ModuleMember -Function Invoke-ScriptAndRedirectOutput
Export-ModuleMember -Function Log-TimedMessage
Export-ModuleMember -Function Copy-Files
# SIG # Begin signature block
# MIIdtAYJKoZIhvcNAQcCoIIdpTCCHaECAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUQ31IR7zqDRpTCaadW1tSEZEQ
# ScygghhkMIIEwzCCA6ugAwIBAgITMwAAAKxjFufjRlWzHAAAAAAArDANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTYwNTAzMTcxMzIz
# WhcNMTcwODAzMTcxMzIzWjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OkMwRjQtMzA4Ni1ERUY4MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAnyHdhNxySctX
# +G+LSGICEA1/VhPVm19x14FGBQCUqQ1ATOa8zP1ZGmU6JOUj8QLHm4SAwlKvosGL
# 8o03VcpCNsN+015jMXbhhP7wMTZpADTl5Ew876dSqgKRxEtuaHj4sJu3W1fhJ9Yq
# mwep+Vz5+jcUQV2IZLBw41mmWMaGLahpaLbul+XOZ7wi2+qfTrPVYpB3vhVMwapL
# EkM32hsOUfl+oZvuAfRwPBFxY/Gm0nZcTbB12jSr8QrBF7yf1e/3KSiqleci3GbS
# ZT896LOcr7bfm5nNX8fEWow6WZWBrI6LKPx9t3cey4tz0pAddX2N6LASt3Q0Hg7N
# /zsgOYvrlwIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFCFXLAHtg1Boad3BTWmrjatP
# lDdiMB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBAEY2iloCmeBNdm4IPV1pQi7f4EsNmotUMen5D8Dg4rOLE9Jk
# d0lNOL5chmWK+d9BLG5SqsP0R/gqph4hHFZM4LVHUrSxQcQLWBEifrM2BeN0G6Yp
# RiGB7nnQqq86+NwX91pLhJ5LBzJo+EucWFKFmEBXLMBL85fyCusCk0RowdHpqh5s
# 3zhkMgjFX+cXWzJXULfGfEPvCXDKIgxsc5kUalYie/mkCKbpWXEW6gN+FNPKTbvj
# HcCxtcf9mVeqlA5joTFe+JbMygtOTeX0Mlf4rTvCrf3kA0zsRJL/y5JdihdxSP8n
# KX5H0Q2CWmDDY+xvbx9tLeqs/bETpaMz7K//Af4wggYHMIID76ADAgECAgphFmg0
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
# bWrJUnMTDXpQzTGCBLowggS2AgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCBzjAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQUnHjX1N6/boIbZwOJfyD3iCpXsQEwbgYKKwYB
# BAGCNwIBDDFgMF6gLIAqAEMAbwBtAG0AbwBuAC0AUwBlAHIAdgBpAGMAaQBuAGcA
# LgBwAHMAbQAxoS6ALGh0dHA6Ly93d3cuTWljcm9zb2Z0LmNvbS9NaWNyb3NvZnRE
# eW5hbWljcy8gMA0GCSqGSIb3DQEBAQUABIIBAAafks42WaYoa9yS+83+OdsIy3co
# tdOc+X4K7iyNy21WyYUxQ73gxAFws0zrjE5PpnKOlMwWylUlprceCmAx+E7qqfeW
# DfdGQX6OreZKhNpGq6Xj2vA/HAaOs/tOUXRB+CtzIkxoliU9fQFZxQuAWifjuElf
# nS2eATw/3dY7qPQG2AaaedOTeECCXGswrj1/qhcWCmCvtgn4OeqywJCGALaboLJh
# z5IsjEuwkdyxMIMbw7WwtHDjCBOtQ4ykoJeHH8Ohrd5gd4b6HKBS4gOEnRcfc6hD
# i7xnKlKbgTUFol4GM57DSc5O0IkXGBZawOMAfv5GS0x7v6KGfGKFyxoTWA6hggIo
# MIICJAYJKoZIhvcNAQkGMYICFTCCAhECAQEwgY4wdzELMAkGA1UEBhMCVVMxEzAR
# BgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
# Y3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWljcm9zb2Z0IFRpbWUtU3Rh
# bXAgUENBAhMzAAAArGMW5+NGVbMcAAAAAACsMAkGBSsOAwIaBQCgXTAYBgkqhkiG
# 9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0xNjA3MjEyMTA3MTBa
# MCMGCSqGSIb3DQEJBDEWBBR11PynOHkAjBgqo6Y/OQ0jOWhArDANBgkqhkiG9w0B
# AQUFAASCAQA+yb4WjZIg/MZg0/Qc3aA2HcSLA5zxtf7HwMxultHPYdzlPlqFWcqo
# 2aJjpyWm0l+Srny4EwJaBCZSulWPgvxzrfDvkoV1M1Jid48YKVBZR6a2pZAHr4+q
# EMgaXEvahwMHyAnDQU6/6Wm7fSxbgWFA88yxmerDzIQP+GSxqvS3SDmKsTRK1CHB
# TeEXQk34m/Esmijra7WBlNWxG2Cm4JmwjeilwzqpRPUyeTKGg5/dbSzpuMe2C3zo
# 8kOjAz5VXFapXEb8m7qGtYQgsjHjSeaYXwVZGHoTYCuJ8sQ14Wvxld1pQ4khC6Fz
# 1tbVD5sNea4kPDC2I7Ft2GAseiYbqM87
# SIG # End signature block
