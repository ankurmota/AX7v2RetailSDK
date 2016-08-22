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
. "$ScriptDir\Common-Configuration.ps1"

function Get-WinServiceConfigFromXPath(
    [xml]$xmlDocument = $(Throw 'xmlDocument parameter required'),
    [string]$xpath = $(Throw 'xpath parameter required'))
{
    $node = $xmlDocument.selectsinglenode($xpath)
    if($node -eq $null)
    {
        throw "Could not find expected node $xpath";
    }

    [hashtable]$ht = @{};   

    $ht.Name = Get-XmlAttributeValue $node "Name"
    $ht.DisplayName = Get-XmlAttributeValue $node "DisplayName"
    $ht.SourcePath = Get-XmlAttributeValue $node "SourcePath"
    $ht.LogOnUser = Get-XmlAttributeValue $node "LogOnUser"
    $ht.ExecutionFileName = Get-XmlAttributeValue $node "ExecutionFileName"
    $ht.PhysicalPath = Get-XmlAttributeValue $node "PhysicalPath"
    [hashtable]$ht.AppSettings = @{};	

    $propertiesXml = $node.Selectnodes("AppSettings/Property")

    if($propertiesXml -ne $null)
    {
        foreach($propertyXml in $propertiesXml)
        {

            $key = $propertyXml.GetAttribute("Key");
            $value = $propertyXml.GetAttribute("Value");
            $ht.AppSettings.Add($key, $value)
        }
    }

    return $ht;
}

function Check-CurrentUserIsAdmin {  
    $identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()  
    $principal = new-object System.Security.Principal.WindowsPrincipal($identity)  
    $admin = [System.Security.Principal.WindowsBuiltInRole]::Administrator  
    $principal.IsInRole($admin)  
} 

function ConvertFrom-SecureToPlain(
    [System.Security.SecureString] $SecureString = $(Throw 'SecureString parameter required'))
{   
    $strPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
    $plainTextString = [Runtime.InteropServices.Marshal]::PtrToStringAuto($strPointer) 
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($strPointer)
    $plainTextString    
}

function Grant-UserLogonAsServiceRights(
    [string]$userAccount = $(Throw 'userAccount parameter required'))
    {
    # In case if User account is in form ".\account", convert it to "ComputerName\account". Otherwise call to .dll method fails
    if($userAccount -like ".\*")
    {
        $userAccount = $env:COMPUTERNAME + "\" + ($userAccount.split("\\")[1])
    }

    try
    {
        # Use script scope of MyInvocation because $MyInvocation.MyCommand.Path is not defined in function.
        $ScriptDir = split-path -parent $script:MyInvocation.MyCommand.Path;
        $dllPath = Join-Path $ScriptDir "UserAccountRightsManager\Microsoft.Dynamics.Retail.UserAccountRightsManager.dll"
        Log-ActionItem "Trying to load [$dllPath]"
        # Using LoadFile rather than LoadFrom as it has no dependencies on Fusion.
        [System.Reflection.Assembly]::LoadFile($dllPath)
        Log-ActionResult "Complete"
    }
    catch
    {
        Log-ActionResult "Failed"
        Log-Error $_
        Throw-Error "Failed To Load [$dllPath]"
    }

    try
    {
        Log-ActionItem "Granting log on as service rights to user account [$userAccount]"
        # Works fine if user already has service logon rights.
        [SCSetup.UserManager]::AddLogOnAsServiceRightToAccount($userAccount)
        Log-ActionResult "Success"
    }
    catch
    {
        Log-ActionResult "Failed"
        Throw-Error "Failed to grant log on as service permission to [$userAccount]"
    }
}

function Install-WindowsService(
    $WinServiceConfig = $(Throw 'WinServiceConfig parameter required'),
    [System.Management.Automation.PSCredential[]]$Credentials = $(Throw 'Credentials parameter required')
)
{    
    $UserName = $WinServiceConfig.LogOnUser; Validate-NotNull $UserName "UserName"
    $ServiceBinarySourceFolder = $WinServiceConfig.SourcePath; Validate-NotNull $ServiceBinarySourceFolder "ServiceBinarySourceFolder"
    $ServiceExeName = $WinServiceConfig.ExecutionFileName; Validate-NotNull $ServiceExeName "ServiceExeName"
    $ServiceName = $WinServiceConfig.Name; Validate-NotNull $ServiceName "ServiceName"
    $DisplayName = $WinServiceConfig.DisplayName; Validate-NotNull $DisplayName "DisplayName"
    $ServiceInstallFolder = $WinServiceConfig.PhysicalPath;	Validate-NotNull $DisplayName "DisplayName"

    Grant-UserLogonAsServiceRights $UserName

    Write-Host "------------------------------------------"
    Write-Host " Installing Windows Service [$ServiceName]"
    Write-Host "------------------------------------------"

    $ExePath = Join-Path -Path $ServiceInstallFolder -ChildPath $ServiceExeName 
    
    Log-ActionItem "Looking up credential for user with name [$UserName]"
    $foundCredential = $null
    for($i = 0; $i -le $Credentials.length - 1; $i++)
    {
        if($Credentials[$i].UserName -like $UserName)
        {
            $foundCredential = $Credentials[$i]
            break
        }
    }
    if($foundCredential -eq $null)
    {
        throw "Credential for user with name $UserName was not passed in. The script cannot continue."
    } 
    Log-ActionResult "Complete"

    Log-ActionItem "Decrypting password for [$UserName]"
    $Password = $foundCredential.GetNetworkCredential().Password
    Log-ActionResult "Complete"

    Log-ActionItem "Check if service [$ServiceName] already exists"
    if (Get-Service $ServiceName -ErrorAction SilentlyContinue)
    {
        Log-ActionResult "Yes"

        Stop-WindowsService $ServiceName
        Copy-WinServiceBinaries $ServiceInstallFolder $ServiceBinarySourceFolder

        Log-ActionItem "Change service attributes"
        $service = Get-WmiObject Win32_Service -filter "name='$ServiceName'"
        $service.Change($DisplayName,$ExePath,$null,$null,$null,$null,$UserName,$Password,$null,$null,$null)
        Log-ActionResult "Complete"
    }
    else 
    {
        Log-ActionResult "No"
        Copy-WinServiceBinaries "$ServiceInstallFolder" "$ServiceBinarySourceFolder"

        # Creating windows service using all provided parameters
        Log-ActionItem "Installing service"        
        New-Service -name $ServiceName -binaryPathName $ExePath -displayName $DisplayName -startupType Automatic -description $DisplayName -credential $foundCredential  
        Log-ActionResult "Complete"		
    } 

    Write-Host "------------------------------------------"
    Write-Host "Successfully installed windows service [$ServiceName]"
    Write-Host "------------------------------------------"      
}

function Verify-WindowsServiceCredentials(
    $WinServiceConfig = $(Throw 'WinServiceConfig parameter required'),

    [System.Management.Automation.PSCredential[]]$Credentials = $(Throw 'Credentials parameter required')
)
{    
    $UserName = $WinServiceConfig.LogOnUser; Validate-NotNull $UserName "UserName"
    if(-not (Validate-UserIsInCredentialsArray $UserName $Credentials))
    {
        Write-Warning -Message "Credential for user with name [$UserName] was not supplied. Make sure you pass an array with the credential for this user."
        exit ${global:PrerequisiteFailureReturnCode};
    }

    return $true
}

function Copy-WinServiceBinaries(
    [string]$ServiceInstallFolder = $(Throw 'ServiceInstallFolder parameter required'),
    [string]$ServiceBinarySourceFolder = $(Throw 'ServiceBinarySourceFolder parameter required')
)
{
    
   Log-ActionItem "Check if service install folder and binary source folder are the same."
   if(-not (Test-IfPathsEqual $ServiceBinarySourceFolder $ServiceInstallFolder))
   {
        Log-ActionResult "No"
        Log-ActionItem "Check if service path $ServiceInstallFolder exists"
        if (!(Test-Path $ServiceInstallFolder -PathType Container))
        {
            Log-ActionResult "No"
            Log-ActionItem "Create service path $ServiceInstallFolder"
            New-Item "$ServiceInstallFolder" -type directory -force | out-null
            Log-ActionResult "Complete"
        }
        else
        {
           Log-ActionResult "Yes" 
        }
        
        # Create a copy of the binaries and the config file 
        $serviceBinaryAllFiles = Join-Path -Path $ServiceBinarySourceFolder -ChildPath "\*"
        $copyServiceBinaries = {
                    Log-ActionItem "Copy files from [$ServiceBinarySourceFolder] to [$ServiceInstallFolder]."
                    Copy-Item "$serviceBinaryAllFiles" "$ServiceInstallFolder" -Recurse -Force
                    Log-ActionResult "Copied successfully."
        }
        
        try
        {
            Perform-RetryOnDelegate $copyServiceBinaries
        }
        catch
        {
            Throw-Error "Failed to copy files from [$ServiceBinarySourceFolder] to [$ServiceInstallFolder]."
        }  
    }
    else
    {
        Log-ActionResult "Yes"
    }  

    Log-ActionResult "Complete"
}

function Start-WindowsService(
    [string]$ServiceName = $(Throw 'ServiceName parameter required')
)
{  
    if (Get-Service $ServiceName -ErrorAction SilentlyContinue)
    {
        Log-ActionItem "Start service $ServiceName"	
        #Set windows service start mode to Automatic(Delayed)
        & "$env:SystemRoot\System32\sc.exe" config $ServiceName start= 'delayed-auto'
        Start-Service $serviceName        
        # The service will not immediately start.  We need to query its state and only if we get a valid state continue.
        # If it does not start for 60 seconds, fail the script.
        [string]$serviceState = $null
        $loopBeginTime = [Datetime]::Now
        do
        {
            try
            {
                $scService = Get-Service -Name $ServiceName
                $serviceState = $scService.Status
            } 
            catch
            {
                if(([Datetime]::Now) -gt ($loopBeginTime + [Timespan]::FromSeconds(60)))
                {
                    Throw-Error "Service [$ServiceName] could not be started in a timely manner."
                }
                Log-ActionItem "Service [$ServiceName] is not ready for use. Sleeping..."
                Start-Sleep -Second 1 
            }
        }
        until($serviceState -eq "Running")
        
        Log-ActionResult "Service started"  
    }  
}

function Stop-WindowsService(
    [string]$ServiceName = $(Throw 'ServiceName parameter required')
)
{  
    if (Get-Service $ServiceName -ErrorAction SilentlyContinue)
    {
        Log-ActionItem "Stop service $ServiceName"        
        Stop-Service -Name $serviceName -Force #Stop service, ignore dependencies    
        # The service will not immediately stop.  We need to query its state and only if we get a valid state continue.
        # If it does not stop for 60 seconds, fail the script.
        [string]$serviceState = $null
        $loopBeginTime = [Datetime]::Now
        do
        {
            try
            {
                $scService = Get-Service -Name $ServiceName
                $serviceState = $scService.Status
            } 
            catch
            {
                if(([Datetime]::Now) -gt ($loopBeginTime + [Timespan]::FromSeconds(60)))
                {
                    Throw-Error "Service [$ServiceName] could not be stopped in a timely manner."
                }
                Log-ActionItem "Service [$ServiceName] is not ready for use. Sleeping..."
                Start-Sleep -Second 1 
            }
        }
        until($serviceState -eq "Stopped")
        Log-ActionResult "Service stopped"  
    }     
}

function Uninstall-WindowsService(
    $WinServiceConfig = $(Throw 'WinServiceConfig parameter required'))
{
    $ServiceName = $WinServiceConfig.Name; Validate-NotNull $ServiceName "ServiceName"
    Write-Host "------------------------------------------"
    Write-Host " Uninstalling Windows Service [$ServiceName]"
    Write-Host "------------------------------------------"

    try
    {
        Remove-WindowsServiceSafe -ServiceName $ServiceName	       
        Remove-ServiceBinaries -WinServiceConfig $WinServiceConfig
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Failed to uninstall windows service [$ServiceName]"
    }
    Write-Host "------------------------------------------"
    Write-Host "Successfully uninstalled windows service [$ServiceName]"
    Write-Host "------------------------------------------"
}

function Remove-ServiceBinaries(
    $WinServiceConfig = $(Throw 'WinServiceConfig parameter required'))
{
    $ServiceInstallFolder = $WinServiceConfig.PhysicalPath; Validate-NotNull $ServiceInstallFolder "ServiceInstallFolder"	
    $ServiceBinarySourceFolder = $WinServiceConfig.SourcePath; Validate-NotNull $ServiceBinarySourceFolder "ServiceBinarySourceFolder"

    Log-ActionItem "Check if service install folder and binary source folder are the same. Do not delete the folder if paths are same."
    if(-not (Test-IfPathsEqual $ServiceBinarySourceFolder $ServiceInstallFolder))
    {
        Log-ActionResult "No"
        
        Log-Step "Remove service binaries"
        # Delete service binaries
        Log-ActionItem "Delete service binaries from [$ServiceInstallFolder]"
        Log-ActionItem "Checking if [$ServiceInstallFolder] exists."
        if(Test-Path $ServiceInstallFolder)
        {
            Log-ActionResult "Yes"

            # Remove binaries from service folders. 
            $deleteServiceBinaries = {
                        Log-ActionItem "Delete files from [$ServiceInstallFolder]."
                        Remove-Item -Path "$ServiceInstallFolder" -Recurse -Force
                        Log-ActionResult "Deleted successfully."
            }
            
            try
            {
                Perform-RetryOnDelegate $deleteServiceBinaries
            }
            catch
            {
                Throw-Error "Failed to delete files from [$ServiceInstallFolder]."
            }
        }
        else
        {
            Log-ActionResult "No"
        }
    }
    else
    {
        Log-ActionResult "Yes"
    } 

    Log-ActionResult "Complete"
}

function Remove-WindowsServiceSafe(
    $ServiceName = $(Throw 'ServiceName parameter required')
)
{
    Log-ActionItem "Check if service [$ServiceName] already exists"

    # Verify if the service already exists, and if yes remove it 
    if (Get-Service $ServiceName -ErrorAction SilentlyContinue)
    {
        Log-ActionResult "Yes"	

        # Using WMI to remove Windows service because PowerShell does not have CmdLet for this
        Log-ActionItem "Delete the service $ServiceName"
        $serviceToRemove = Get-WmiObject -Class Win32_Service -Filter "name='$ServiceName'"
        $serviceToRemove.StopService()
        $serviceToRemove.Delete()      
        Log-ActionResult "Service removed"		
    }
    else
    {
        Log-ActionResult "Service does not exist on the system"
    }
}

function Enable-WindowsServiceMonitoringDiscovery(
    $WinServiceConfig = $(Throw 'WinServiceConfig parameter required'),

    [string]$RetailComponentRegistryKey = $(Throw 'RetailComponentRegistryKey parameter required'))
{
    $ServiceName = $WinServiceConfig.Name; Validate-NotNull $ServiceName "ServiceName"

    Log-Step "Enable discovery of Windows Service [$ServiceName] for monitoring purposes"
    try 
    {    
        $WinServiceKeyName = Join-Path $RetailComponentRegistryKey $ServiceName
        # always overwrite
        Log-ActionItem "Save parameters of the Windows Service [$ServiceName] in registry path [$WinServiceKeyName]"
        [void](New-Item $WinServiceKeyName -Force)
        New-ItemPropertyNotNull -Path $WinServiceKeyName -Name "UserName" -PropertyType "String" -Value $WinServiceConfig.LogOnUser
        New-ItemPropertyNotNull -Path $WinServiceKeyName -Name "ServiceName" -PropertyType "String" -Value $ServiceName
        New-ItemPropertyNotNull -Path $WinServiceKeyName -Name "DisplayName" -PropertyType "String" -Value $WinServiceConfig.DisplayName
        New-ItemPropertyNotNull -Path $WinServiceKeyName -Name "ExeName" -PropertyType "String" -Value $WinServiceConfig.ExecutionFileName
        New-ItemPropertyNotNull -Path $WinServiceKeyName -Name "ServiceInstallFolder" -PropertyType "String" -Value $WinServiceConfig.PhysicalPath     

        Log-ActionResult "Complete"    
    }
    catch
    {
        Log-Exception $_
        Write-Warning -Message "Failed: Enabling discovery of Windows Service [$ServiceName] for monitoring purposes"
    }
}

function Disable-WindowsServiceMonitoringDiscovery(
    $WinServiceConfig = $(Throw 'WinServiceConfig parameter required'),

    [string]$RetailComponentRegistryKey = $(Throw 'RetailComponentRegistryKey parameter required'))
{
    $ServiceName = $WinServiceConfig.Name; Validate-NotNull $ServiceName "ServiceName"
    Log-Step "Disable discovery of Windows Service [$ServiceName] for monitoring purposes"

    try
    {
        $WinServiceKeyName = Join-Path $RetailComponentRegistryKey $ServiceName
        Log-ActionItem "Delete registry entry [$WinServiceKeyName]"
        if (Test-Path $WinServiceKeyName)
        {
            Remove-Item -Path $WinServiceKeyName -Recurse -Force
        }
        Log-ActionResult "Complete"
    }
    catch
    {
        Log-Exception $_
        Write-Warning -Message "Failed: Disable discovery of Windows Service [$ServiceName] for monitoring purposes"
    }
}
# SIG # Begin signature block
# MIIdtAYJKoZIhvcNAQcCoIIdpTCCHaECAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUSINjeWomc62CYuhE4us7nzqx
# 20CgghhkMIIEwzCCA6ugAwIBAgITMwAAAJgEWMt/IwmwngAAAAAAmDANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBLowggS2AgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCBzjAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQURrAukksdApvEA6N51AeUqRXsjWwwbgYKKwYB
# BAGCNwIBDDFgMF6gLIAqAEMAbwBtAG0AbwBuAC0AVwBpAG4AUwBlAHIAdgBpAGMA
# ZQAuAHAAcwAxoS6ALGh0dHA6Ly93d3cuTWljcm9zb2Z0LmNvbS9NaWNyb3NvZnRE
# eW5hbWljcy8gMA0GCSqGSIb3DQEBAQUABIIBABC7UB1rWqnPAL73oCjZA0iI7Ysk
# kz/k99wowemTrRGVaZw6Loc9dJ1cantiXVCIp6XWLsmUkQ/mvUmxRPsCyU7Pk31o
# sLanR6zhHaBiFPcnhY6lEa0qaXYhOeuRWvSnqhMgSArOOF0qmCTG3POJOmMc8dcG
# ng6e+WrbWTTN2ujlrb37kcT9Y48be9tPwNX+GePiq8fPWukCaK3H6FQmPl1yD4ed
# KRpprVMDmgwTOyHw+OJBG0a30PMN0Ww8aZeWASDu9f6PoaH952SNvIThX/kJCI8M
# 6jPXw4Viqzm9VHNKa6xgo3P1f3ck/ZJq9Wu5T7M5m8NheFEAlYiFbzSE2k6hggIo
# MIICJAYJKoZIhvcNAQkGMYICFTCCAhECAQEwgY4wdzELMAkGA1UEBhMCVVMxEzAR
# BgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
# Y3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWljcm9zb2Z0IFRpbWUtU3Rh
# bXAgUENBAhMzAAAAmARYy38jCbCeAAAAAACYMAkGBSsOAwIaBQCgXTAYBgkqhkiG
# 9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0xNjA4MDYwODUzMDha
# MCMGCSqGSIb3DQEJBDEWBBTjL3zVd32xoxNpVfEYZ/zaIeL5CDANBgkqhkiG9w0B
# AQUFAASCAQCbkwIBMjDgUFZjxK2OalG3y69Sa8lrWVuZPbPWmn2uogKEfLxViZ9j
# izcL9l9XmIwWJ86NWWBnx9AK/ouV8XMSRm5Sp2E/cjoAVPqcbsuZXuqH0u55FkcC
# Dl/Mg9jBrwL3lpOJgnsKPgWw/zyVyLDu6nxbb+o2sGSvknTuG9zh6qo4FehFRZlH
# vYLQUqvp7yaiKSk4P9H6s0VWdBYh7AAIKE2mSzTFO+DLEN5ZSOqEqM4r4HDz3tgr
# cohR0pPJZFdEshrrzoW+l5oPbRLAWn+1ZZz56zTXItfoToafhNZfWaIGYRiPjxoP
# ZdRX8lOifFcvAkrBsIIBSr6YChtAb7FO
# SIG # End signature block
