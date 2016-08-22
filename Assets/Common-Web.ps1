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
. $ScriptDir\Common-Configuration.ps1

function Check-CurrentUserIsAdmin {  
    $identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()  
    $principal = new-object System.Security.Principal.WindowsPrincipal($identity)  
    $admin = [System.Security.Principal.WindowsBuiltInRole]::Administrator  
    $principal.IsInRole($admin)  
} 

# We need this function because get-website has bug in windows server 2008 Datacenter.
# Get-Website -Name "websiteName" returns all websites, not the specific name only.
# There is another bug with Get-Website in Windows Server 2008 R2 SP1 with PowerShell 3.0. 
# We need to retry the Get-Website call if it fails for the first time
function Get-WebSiteSafe(
    [string]$Name = $(Throw 'Name parameter required'))
{
    try
    {
        Log-ActionItem "Trying to get information of the website - $Name"
        Get-WebSite | Where-Object { $_.Name -eq $Name }
    }
    catch [System.IO.FileNotFoundException]
    {
        Log-ActionItem "Re-trying to get information of the website - $Name"
        Get-WebSite | Where-Object { $_.Name -eq $Name }
    }
}

# In Windows Server 2008 Get-WebBinding throws an exception in case if no bindings exist yet.
function Get-WebBindingSafe(
    $Name,
    $Protocol,
    $Port
)
{
    [hashtable]$getWebBindingParameters = @{}
    if($Name)
    {
        $getWebBindingParameters['Name'] = $Name
    }

    if($Protocol)
    {
        $getWebBindingParameters['Protocol'] = $Protocol
    }

    if($Port)
    {
        $getWebBindingParameters['Port'] = $Port
    }

    try
    {
        $result = Get-WebBinding @getWebBindingParameters
    }
    catch{}

    return $result
}

function Test-IfWebAppPoolExists(
    [string]$Name = $(Throw 'Name parameter required'))
{
    $exists = $false
    try { $exists = Get-WebAppPoolState -Name $AppPoolName } catch{}
    return $exists
}

# Adds binding to website if it doesn't exist yet.
function Add-WebSiteBinding(
    [string]$Name = $(Throw 'Name parameter required'),
    
    [ValidateSet("http","https","net.tcp")]
    [string]$Protocol = $(Throw 'Protocol parameter required'),
    
    [ValidateRange(1,65535)]
    [UInt32]$Port = $(Throw 'Port parameter required'))
{
    if($Protocol -eq "net.tcp")
    {
        Throw-Error "The net.tcp protocol is not supported"
    }

    Log-ActionItem "Checking if a(n) $Protocol binding using port $Port exists for web site $Name"
    if(-not (Get-WebBindingSafe -Name $Name -Protocol $Protocol -Port $Port))
    {
        Log-ActionResult "No"
        
        Log-ActionItem "Checking if another protocol using port $Port exists for web site $Name"
        if(Get-WebBindingSafe -Name $Name -Port $Port)
        {
            Log-ActionResult "Yes"

            Log-ActionItem "Removing other protocol bindings that use port $Port, to prevent conflicts"
            Remove-WebBinding -Name $Name -Port $Port
            Log-ActionResult "Update complete"
        }
        else
        {
            Log-ActionResult "No"
        }

        Log-ActionItem "Checking if a(n) $Protocol binding exists for web site $Name"
        if(Get-WebBindingSafe -Name $Name -Protocol $Protocol)
        {
            Log-ActionResult "Yes"

            Log-ActionItem "Updating existing $Protocol binding to use port $Port"
            Update-WebBinding -websitename $Name -Protocol $Protocol -PropertyName 'Port' -Value $Port
            Log-ActionResult "Update complete"
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

    Log-ActionItem "Checking again if a(n) $Protocol binding using port $Port exists for web site $Name"
    if(-not (Get-WebBindingSafe -Name $Name -Protocol $Protocol -Port $Port))
    {
        Log-ActionResult "No"
        Log-ActionItem "Adding new binding"

        New-WebBinding -Name $Name -Protocol $Protocol -Port $Port
    }
    else
    {
        Log-ActionResult "Yes"
        Log-ActionItem "No further action necessary"
    }

    Log-ActionResult "Complete"
}

function Test-IfWebSiteUsesHttp(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    return (-not ([string]::IsNullOrEmpty($WebSiteConfig.Port)))
}

function Test-IfWebSiteUsesTcp(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    return (-not ([string]::IsNullOrEmpty($WebSiteConfig.PortTcp)))
}

function Test-IfWebSiteUsesSSL(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    return (-not ([string]::IsNullOrEmpty($WebSiteConfig.PortSSL)))
}

###############################
# Function to Add SSL binding #
###############################
function Add-SslBinding(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    Validate-IfWebAdministrationInstalled $true
    
    Log-ActionItem "Check if SSL binding required"
    if(Test-IfWebSiteUsesSSL $WebSiteConfig)
    {
        Log-ActionResult "Yes. Adding SSL binding"
    }
    else
    {
        Log-ActionResult "No. Exiting"
        return
    }

    $HttpsPort = $WebSiteConfig.PortSSL; Validate-NotNull $HttpsPort "HttpsPort"
    $CertRootStore = $WebSiteConfig.ServerCertificateRootStore; Validate-NotNull $CertRootStore "CertRootStore"
    $CertStore = $WebSiteConfig.ServerCertificateStore; Validate-NotNull $CertStore "CertStore"
    $CertThumbprint = $WebSiteConfig.ServerCertificateThumbprint; Validate-NotNull $CertThumbprint "CertThumbprint"
    $CertThumbprint = $CertThumbprint.ToString().ToUpper();
    $SSLCertificatePath = "{0}\{1}\{2}" -f $CertRootStore, $CertStore, $CertThumbprint

    Log-Step "Updating SSL binding for https port [$HttpsPort] and Certificate [$SSLCertificatePath]"
    Log-ActionItem "Check if binding configuration for port [$HttpsPort] already exists"
    $bindingPath = "IIS:\SslBindings\0.0.0.0!$HttpsPort" 
    if (Test-Path $bindingPath)
    {
        Log-ActionResult "Yes"
        $bindingThumbprint = (Get-Item $bindingPath).Thumbprint
        if($CertThumbprint -ne $bindingThumbprint)
        {
            Log-ActionItem "Existing [$bindingPath] thumbprint [$bindingThumbprint] is different from passed certificate thumbprint [$CertThumbprint]. Removing binding."
            Remove-Item $bindingPath -Force
        }
    }
    else
    {
        Log-ActionResult "No"
    }
    
    if(-not (Test-Path $bindingPath))
    {
        Log-ActionItem "Configuring the server certificate for SSL."
        Get-Item cert:\$SSLCertificatePath | New-Item $bindingPath -Force
    }
    
    $UserName = $WebSiteConfig.WebAppPool.ProcessModel_UserName; Validate-NotNull $UserName "WebAppPoolUserName"
    GrantPermission-UserToSSLCert -certThumbprint $CertThumbprint -certRootStore $CertRootStore -certStore $CertStore -appPoolUserName $UserName
    Log-ActionResult "Complete"
}

function GrantPermission-UserToSSLCert(
    $certThumbprint = $(Throw 'certThumbprint parameter required'),
    $certRootStore = $(Throw 'certRootStore parameter required'),
    $certStore = $(Throw 'certStore parameter required'),
    $appPoolUserName = $(Throw 'appPoolUserName parameter required')
)
{
    Log-ActionItem "Granting read permissions for certificate with thumbprint $certThumbprint to user $appPoolUserName ..."
    $certPath = "cert:\{0}\{1}" -f $certRootStore, $certStore
    $keyname = (((gci $certPath | ? {$_.thumbprint -like $certThumbprint}).PrivateKey).CspKeyContainerInfo).UniqueKeyContainerName
    $keyPath = Join-Path -Path ${env:ProgramData} -ChildPath "\Microsoft\Crypto\RSA\MachineKeys"
    $fullpath = Join-Path -Path $keypath -ChildPath $keyname
    icacls $fullpath /grant $appPoolUserName`:RX
    Log-ActionResult "Complete"
}

function Get-ProtocolsToEnable(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'),

    [string]$existingProtocolsToUpdate)
{
    $protocolTable = [HashTable] @{};    
    if(Test-IfWebSiteUsesHttp $WebSiteConfig)
    {
        $protocolTable["http"] = 1
    }

    if(Test-IfWebSiteUsesSSL $WebSiteConfig)
    {
        $protocolTable["https"] = 1
    }

    if(Test-IfWebSiteUsesTcp $WebSiteConfig)
    {
        $protocolTable["net.tcp"] = 1
    }
    
     
    if(-not ([string]::IsNullOrEmpty($existingProtocolsToUpdate)))
    {
        $existingProtocolsToUpdate.Split(",") | % { $protocolTable[$_] = 1 } | Out-Null
    }

    return ($protocolTable.Keys -join ",")
}

function Test-IfPathsEqual(
    [string]$firstPath = $(Throw 'firstPath parameter required'),

    [string]$secondPath = $(Throw 'secondPath parameter required'))
{
    $directorySeparator = [IO.Path]::DirectorySeparatorChar
    [Environment]::ExpandEnvironmentVariables($firstPath).TrimEnd($directorySeparator) -eq [Environment]::ExpandEnvironmentVariables($secondPath).TrimEnd($directorySeparator)
}

#####################################
# Web Site Management prerequisites #
#####################################
function Validate-IfCurrentUserIsAdmin
{
    Log-ActionItem "Check if this is run under admin privilege"
    $IsAdmin = Check-CurrentUserIsAdmin
    if ($IsAdmin)
    {
        Log-ActionResult "Yes"
    }
    else
    {
        Log-ActionResult "No"
        Throw-Error "The install script must be run with Administrator privilege."
    }
}

function Validate-IfWebAdministrationInstalled(
    [bool]$InstallDependencies = $(Throw 'InstallDependencies parameter required'))
{
    try
    {
        Log-ActionItem "Check if WebAdministration module is available"
        $webadminModule = Get-Module -Name WebAdministration -ListAvailable
        if ($webadminModule)
        {
            Log-ActionResult "Yes"
            Log-ActionItem "Import web admin module"
            Import-Module WebAdministration
        }
        else
        {
            Log-ActionItem "Try add snapin for older version of Windows"
            # Check if Snap-in is already loaded before loading.
            if ( (Get-PSSnapin -Name WebAdministration -ErrorAction SilentlyContinue) -eq $null )
            {
                Add-PSSnapin -Name WebAdministration
            }
            Log-ActionResult "WebAdministration snapin added"
        }
        Log-ActionResult "Complete"
    }
    catch
    {
        Throw-Error "Unable to import web administration module or snapin. Please install the IIS web administration module or snapin for PowerShell"
    }
}

function Validate-WebManagementPrerequisites(
    [bool]$InstallDependencies = $true)
{
    try
    {
        Log-Step "Check IIS management prerequisites"
        Validate-IfCurrentUserIsAdmin
        Validate-IfWebAdministrationInstalled $InstallDependencies
    }
    catch
    {
        Log-Exception $_
        Throw-Error "IIS management prerequisites validation failed."
    }
}

##########################
# Web Site Setup Helpers #
##########################
function CreateAndConfigure-ApplicationPool(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'),

    [System.Management.Automation.PSCredential[]]$Credentials = $(Throw 'Credentials parameter required'))
{
    $UserName = $WebSiteConfig.WebAppPool.ProcessModel_UserName; Validate-NotNull $UserName "WebAppPoolUserName"
    $AppPoolName = $WebSiteConfig.WebAppPool.Name; Validate-NotNull $AppPoolName "WebAppPoolName"
    $Enable32BitApp = $WebSiteConfig.WebAppPool.Enable32BitAppOnWin64;
    try
    {
        Log-Step "Create and configure Application Pool [$AppPoolName]"
        # Create application pool if not already exists
        Log-ActionItem "Check if application pool [$AppPoolName] already exists"
        if(Test-IfWebAppPoolExists -Name $AppPoolName)
        {
            Log-ActionResult "Yes"
        }
        else
        {
            Log-ActionResult "No"
            
            Log-ActionItem "Create application pool [$AppPoolName]"
            New-WebAppPool $AppPoolName
            Log-ActionResult "Complete"
        }
        
        # Config application pool
        # Always reconfigure user in case of password change
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
            Throw-Error "Credential for user with name [$UserName] was not received. The script cannot continue."
        }
        Log-ActionResult "Complete"
    
        Log-ActionItem "Decrypting password for [$UserName]"
        $Password = $foundCredential.GetNetworkCredential().Password
        Log-ActionResult "Complete"
        
        $appPoolPath = "IIS:\AppPools\$AppPoolName"
        Log-ActionItem "Config application pool [$AppPoolName] to use custom identity [$Username]"
        Set-ItemProperty -Path $appPoolPath -Name ProcessModel -Value @{IdentityType=3;Username="$UserName";Password="$Password";loadUserProfile="true";setProfileEnvironment="true"} -Force # 3 = Custom
        Log-ActionResult "Complete"

        Log-ActionItem "Config application pool [$AppPoolName] to use .NET 4.0 Runtime"
        Set-ItemProperty -Path $appPoolPath -Name ManagedRuntimeVersion -Value v4.0 -Force
        Log-ActionResult "Complete"

        if($Enable32BitApp -eq $true)
        {
            Log-ActionItem "Config application pool [$AppPoolName] to enable 32bit mode"
            Set-ItemProperty -Path $appPoolPath -Name "enable32BitAppOnWin64" -Value "true" -Force
            Log-ActionResult "Complete"
        }

        # The application pool is not immediately available.  We need to query its state and only if we get a valid state continue and start it
        # We will not infinitely retry to query the state, a minute should be plenty of time. If if fails, we fail the script.
        [Microsoft.IIs.PowerShell.Framework.CodeProperty]$appPoolState = $null
        $loopBeginTime = [Datetime]::Now
        do
        {
            try
            {
                $appPoolState = Get-WebAppPoolState $AppPoolName -ErrorAction SilentlyContinue
            } 
            catch
            {
                if(([Datetime]::Now) -gt ($loopBeginTime + [Timespan]::FromSeconds(60)))
                {
                    Throw-Error "Application pool [$AppPoolName] could not be created and polled in a timely manner."
                }
                Log-ActionItem "Application pool [$AppPoolName] is not ready for use. Sleeping..."
                Start-Sleep -Second 1 
            }
        }
        until($appPoolState -ne $null)
        
        Log-ActionItem "Starting application pool [$AppPoolName]"
        Start-WebAppPool -Name $AppPoolName
        Log-ActionResult "Complete"
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Failed: Create and configure application pool [$AppPoolName]"    
    }
}

function Verify-ApplicationPoolCredentials(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'),

    [System.Management.Automation.PSCredential[]]$Credentials = $(Throw 'Credentials parameter required'))
{
    $UserName = $WebSiteConfig.WebAppPool.ProcessModel_UserName; Validate-NotNull $UserName "WebAppPoolUserName"
    if(-not (Validate-UserIsInCredentialsArray $UserName $Credentials))
    {
        Write-Warning -Message "Credential for user with name [$UserName] was not supplied. Make sure you pass an array with the credential for this user."
        exit ${global:PrerequisiteFailureReturnCode};
    }
    
    return $true
}

function Remove-ApplicationPoolSafe(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $AppPoolName = $WebSiteConfig.WebAppPool.Name; Validate-NotNull $AppPoolName "WebAppPoolName"
    $WebsiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"
    try
    {
        Log-Step "Delete application pool [$AppPoolName] if it exists and doesn't contain any applications"

        Log-ActionItem "Check if Application pool [$AppPoolName] exists"
        $appPoolExists = Test-IfWebAppPoolExists -Name $AppPoolName
        if($appPoolExists)
        {
            Log-ActionResult "Yes"
        }
        else
        {
            Log-ActionResult "No. Skip removal"
        }
    
        # Delete non-default AppPool
        if ($appPoolExists -and $AppPoolName.CompareTo("DefaultAppPool"))
        {
            $applicationsWithCurrentPool = (Get-ChildItem "IIS:\Sites\$WebSiteName" -ErrorAction "SilentlyContinue") |
                Where-Object { $_.NodeType -eq "Application" } |
                Where-Object { $_.ApplicationPool -eq $AppPoolName }
            
            if($applicationsWithCurrentPool -eq $null -or $applicationsWithCurrentPool.Count -eq 0)
            {
                # Delete Application Pool
                Log-ActionItem "Delete application pool [$AppPoolName]"
                Remove-WebAppPool $AppPoolName
            }
            else
            {
                Log-ActionItem "Application pool [$AppPoolName] still has applications. Skip removal."
            }
            
            Log-ActionResult "Complete"
        }
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Error : Delete application pool [$AppPoolName]"
    }
}

# Remove extra un-used bindings.
function Remove-UnusedWebsiteBindings(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    Log-Step "Remove unnecessary bindings"
    $websiteName = $WebSiteConfig.Name;
    $webAppProtocolList = @();    
    Get-WebApplication -Site $WebSiteConfig.Name | foreach { $webAppProtocolList += ($_.EnabledProtocols -split ",")}
    Get-Website -Name $WebSiteConfig.Name | foreach { $websiteProtocolList = $_.EnabledProtocols -split ","}
    
    $webAppProtocolList = $webAppProtocolList | sort -unique;
    Log-ActionItem "Checking if website [$websiteName] has any unused bindings"
    
    $bindingsToRemove = Compare-Object $websiteProtocolList $webAppProtocolList -PassThru

    if($bindingsToRemove)
    {
        Log-ActionResult "Yes"
        foreach($protocol in $bindingsToRemove)
        {
            Log-ActionResult "Removing [$protocol] protocol binding."
            $siteProtocolBindings = Get-WebBindingSafe -Name $websiteName -Protocol "$protocol"
            if($siteProtocolBindings)
            {
                $siteProtocolBindings | Remove-WebBinding
            }
        }
        Log-ActionResult "Complete."
        
        $newEnabledProtocols = $webAppProtocolList -join ","
        Set-ItemProperty -Path "IIS:/Sites/$WebsiteName" -Name "enabledProtocols" -Value "$newEnabledProtocols" -Force
    }
    else
    {
        Log-ActionResult "No"
    }
}

function CreateAndConfigure-WebSite(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $WebsiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"
    $httpPort = $WebSiteConfig.Port;
    $HttpsPort = $WebSiteConfig.PortSSL;
    $SiteInstallFolder = $WebSiteConfig.PhysicalPath; Validate-NotNull $SiteInstallFolder "SiteInstallFolder"
    $AppPoolName = $WebSiteConfig.WebAppPool.Name; Validate-NotNull $AppPoolName "AppPoolName"
    $TcpPort = $WebSiteConfig.PortTcp;
    
    if((-not (Test-IfWebSiteUsesHttp $WebSiteConfig)) -and (-not (Test-IfWebSiteUsesSSL $WebSiteConfig)))
    {
        Throw-Error "Website [$WebsiteName] does not have a valid http or https port specified. Please specify a port for at least one of these protocols."        
    }
    
    try
    {
        Log-Step "Create and configure Web Site [$WebsiteName]"
    
        # Create website if not already exists
        Log-ActionItem "Check if website [$WebsiteName] already exists"
        $site = Get-WebSiteSafe -Name $WebsiteName
        if($site)
        {
            Log-ActionResult "Yes"                        
            Log-ActionItem "Validating existing website settings with parameters passed to the script"
            $webSitePhysicalPath = $site.PhysicalPath 
            if(-not (Test-IfPathsEqual $webSitePhysicalPath $SiteInstallFolder))
            {
                Throw-Error "Existing site physical path is [$webSitePhysicalPath]. Cannot change it to [$SiteInstallFolder]. Exiting."
            }
            
            Log-ActionResult "Validation has completed successfully"
        }
        else
        {
            Log-ActionResult "No"            
            # Create website directory if not already exists
            Log-ActionItem "Check if directory [$SiteInstallFolder] already exists"
            if (Test-Path -path "$SiteInstallFolder")
            {
                Log-ActionResult "Yes"
            }
            else
            {
                Log-ActionResult "No"
                
                Log-ActionItem "Create application directory [$SiteInstallFolder]"
                New-Item -path "$SiteInstallFolder" -type directory -force
                Log-ActionResult "Complete"
            }
            
            Log-ActionItem "Create website [$WebsiteName]"
            
            # Create a new Website and let it add a default binding, which we proceed to remove and set appropriately
            # Note: Known bug: if no websites exist, ID generation fails for IIS.  So we provide a default ID if no sites exist.
            $webSites = Get-WebSite
            if(-not ($webSites))
            {
                New-WebSite -Name $WebsiteName -PhysicalPath $SiteInstallFolder -ApplicationPool $AppPoolName -ID 1
            }
            else
            {
                New-WebSite -Name $WebsiteName -PhysicalPath $SiteInstallFolder -ApplicationPool $AppPoolName
            }
            
            $siteBindings = Get-WebBindingSafe -Name $WebsiteName
            if($siteBindings)
            {
                $siteBindings| Remove-WebBinding
            }
                        
            if (Get-WebSiteSafe -Name $WebsiteName)
            {
                Log-ActionResult "Complete"
            }
            else
            {
                Throw-Error "Failed to create WebSite [$WebsiteName]"
            }
        }
        
        Log-ActionItem "Configure website [$WebsiteName] to allow anonymous authentication"
        Set-WebConfiguration -value false system.webserver/security/authentication/anonymousAuthentication "IIS:\sites\$WebsiteName" -force
        Log-ActionResult "Complete"
        
        if(Test-IfWebSiteUsesHttp $WebSiteConfig)
        {
            Add-WebSiteBinding -Name $WebsiteName -Protocol "http" -Port $httpPort
        }
        if(Test-IfWebSiteUsesSSL $WebSiteConfig)
        {
            Add-WebSiteBinding -Name $WebsiteName -Protocol "https" -Port $HttpsPort
            Add-SSLBinding -WebSiteConfig $WebSiteConfig
        }
            
        # Tcp port is not used by Retail Server.
        if(Test-IfWebSiteUsesTcp $WebSiteConfig)
        {
            Log-ActionItem "Adding net.tcp binding to port $TcpPort"
            Add-WebSiteBinding -Name $WebsiteName -Protocol "net.tcp" -Port "$TcpPort"            
        }
        
        $webSiteProtocols = Get-ProtocolsToEnable $WebSiteConfig $site.EnabledProtocols
        Log-ActionItem "Configure website to enable [$webSiteProtocols] protocols"
        Set-ItemProperty -Path "IIS:/Sites/$WebsiteName" -Name "enabledProtocols" -Value "$webSiteProtocols" -Force
        Log-ActionResult "Complete"
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Failed: Create and configure website [$WebsiteName]"    
    }

}

function Remove-WebSiteSafe(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $WebSiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"

    try
    {
        Log-Step "Remove website if it exists and doesn't contain any applications"
        Log-ActionItem "Check if webSite [$WebSiteName] exists"
        $webSiteExists = Get-WebSiteSafe -Name $WebSiteName
        if($webSiteExists)
        {
            Log-ActionResult "Yes"
        }
        else
        {
            Log-ActionResult "No. Skip removal"
        }
    
        # Delete non-default website
        if ($webSiteExists -and $websiteName.CompareTo("Default Web Site"))
        {
            $applicationPools = (Get-ChildItem "IIS:\Sites\$WebSiteName" -ErrorAction "SilentlyContinue") |
                % { $_.Attributes |
                Where-Object { $_.Name  -eq "applicationpool" }}
            
            if($applicationPools -eq $null -or $applicationPools.Count -eq 0)
            {
                # Remove website
                Log-ActionItem "Remove Website [$WebsiteName]"
                Remove-Website $WebsiteName
            }
            else
            {
                Log-ActionItem "Website [$WebsiteName] contains applications. Skip removal."
            }
            
            Log-ActionResult "Complete"
        }
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Error : Remove Website [$WebSiteName]"
    }
}

function CreateAndConfigure-WebApplication(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $ServiceBinarySourceFolder = $WebSiteConfig.WebApplication.ServiceBinarySourceFolder; Validate-NotNull $ServiceBinarySourceFolder "ServiceBinarySourceFolder"
    $WebsiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"
    $AppPoolName = $WebSiteConfig.WebAppPool.Name; Validate-NotNull $AppPoolName "AppPoolName"
    $WebApplicationName = $WebSiteConfig.WebApplication.Name; Validate-NotNull $WebApplicationName "WebApplicationName"
    $WebApplicationWorkingFolder = $WebSiteConfig.WebApplication.PhysicalPath; Validate-NotNull $WebApplicationWorkingFolder "WebApplicationWorkingFolder"
    
    try
    {
        Log-Step "Create and configure web application [$WebApplicationName]"
        # Create application directory if not already exist
        Log-ActionItem "Check if directory [$WebApplicationWorkingFolder] already exists"
        if (Test-Path -path $WebApplicationWorkingFolder)
        {
            Log-ActionResult "Yes"
        }
        else
        {
            Log-ActionResult "No"
            
            Log-ActionItem "Create application directory [$WebApplicationWorkingFolder]"
            New-Item -path "$WebApplicationWorkingFolder" -type directory -force
            Log-ActionResult "Complete"
        }        
        
        Log-ActionItem "Check if application directory and binary source folder are the same."
        if(-not (Test-IfPathsEqual $ServiceBinarySourceFolder $WebApplicationWorkingFolder))
        {
            Log-ActionResult "No"
            # Copy service binaries and config files to the application directory
            Log-ActionItem "Copy server binaries and config files to the application directory [$WebApplicationWorkingFolder]"
            $global:LASTEXITCODE = 0
            & robocopy.exe /E $ServiceBinarySourceFolder $WebApplicationWorkingFolder *
            $capturedExitCode = $global:LASTEXITCODE
            #Robocopy Exit codes related info: http://support.microsoft.com/kb/954404
            if(($capturedExitCode -ge 0) -and ($capturedExitCode -le 8))
            {            
                Log-ActionResult "[Robocopy] completed successfully"
            }
            else
            {
                Throw-Error "[Robocopy] failed with exit code $capturedExitCode"
            }
        }
        else
        {
            Log-ActionResult "Yes"
        }        

        # Check if application already exist
        Log-ActionItem "Check if application [$WebApplicationName] already exists"
        $application = Get-WebApplication -Site $WebsiteName -Name $WebApplicationName
        if($application)
        {
            Log-ActionResult "Yes"
            
            Log-ActionItem "Validating if existing web application can be reconfigured with parameters passed to the script"
            $removeApplication = $false
            $applicationWorkingFolder = $application.PhysicalPath 
            if(-not (Test-IfPathsEqual $applicationWorkingFolder $WebApplicationWorkingFolder))
            {
                Log-ActionResult "Existing application working folder is [$applicationWorkingFolder]. Cannot change it to [$WebApplicationWorkingFolder]. Application will be removed."
                $removeApplication = $true
            }
            
            $applicationPool = $application.ApplicationPool
            if($applicationPool -ne $AppPoolName)
            {
                Log-ActionResult "Existing web application pool is [$applicationPool]. Cannot change it to [$AppPoolName]. Application will be removed."
                $removeApplication = $true
            }
            Log-ActionResult "Validation has completed successfully"
            
            if($removeApplication)
            {
                Remove-WebApplicationSafe -WebSiteConfig $WebSiteConfig
            }
        }

        $application = Get-WebApplication -Site $WebsiteName -Name $WebApplicationName
        if(!$application)
        {
            Log-ActionResult "No"

            # Create application
            Log-ActionItem "Create application [$WebApplicationName]"
            New-WebApplication -Site "$WebsiteName" -Name "$WebApplicationName" -physicalPath "$WebApplicationWorkingFolder" -ApplicationPool "$AppPoolName" -Force
            Log-ActionResult "Complete"
        }

        # Register ASP.NET version with IIS. We will register either .NET 4.0 or .NET 4.5. They cannot be on the box at the same time. The setup/user decides which one should be there.  
        if((-not (IsLocalOSWindows2012OrLater)))
        {
            Log-ActionItem "Register ASP.NET version with IIS"
            $global:LASTEXITCODE = 0
            $isaWidth = Get-WmiObject -Class Win32_Processor | Select-Object AddressWidth
            if ($isaWidth -eq 64)
            {
                & "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe" -i
            }
            else
            {
                & "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe" -i
            }
            $capturedExitCode = $global:LASTEXITCODE
            if ($capturedExitCode -eq 0)
            {
                Log-ActionResult "Complete"
            }
            else
            {
                Log-ActionResult "Failed"
                Throw-Error "Failed to register ASP.NET version with IIS using [aspnet_regiis.exe]. Return code: $capturedExitCode"
            }
        }
        
        # Configure application to use tcp, https, http protocol
        $applicationProtocols = Get-ProtocolsToEnable $WebSiteConfig
        Log-ActionItem "Configure application to enable [$applicationProtocols] protocols"
        Sleep 3
        Set-ItemProperty -Path "IIS:/Sites/$WebsiteName/$WebApplicationName" -Name "enabledProtocols" -Value "$applicationProtocols" -Force
        Log-ActionResult "Complete"
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Failed: Create and configure application [$WebApplicationName]"
    }
}

function Remove-WebApplicationSafe(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $WebApplicationName = $WebSiteConfig.WebApplication.Name; Validate-NotNull $WebApplicationName "WebApplicationName"
    $WebsiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"

    try
    {    
        Log-Step "Delete application [$WebApplicationName] if it exists."
        if(Get-WebApplication -Name $WebApplicationName -Site $WebsiteName)
        {
            # Delete Application
            Log-ActionItem "Delete application [$WebApplicationName]"
            Remove-WebApplication -Site "$WebsiteName" -Name "$WebApplicationName"
        }
        else
        {
            Log-ActionItem "Application [$WebApplicationName] does not exist. Skip removal"
        }
        
        Log-ActionResult "Complete"
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Error: Delete application [$WebApplicationName]"
    }
}

function Check-IfWebApplicationIsUpAndRunning(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $AppPoolName = $WebSiteConfig.WebAppPool.Name; Validate-NotNull $AppPoolName "AppPoolName"
    $WebsiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"
    
    try
    {
        Log-Step "Check if web application [$WebApplicationName] on site [$WebsiteName] is up and running"
        Log-ActionItem "Check application pool [$AppPoolName] state"
        $appPoolState = (Get-WebAppPoolState -Name $AppPoolName).Value
        Log-ActionResult "Application pool is in [$appPoolState] state"
        if($appPoolState -ne "Started")
        {
            Start-WebAppPool -Name $AppPoolName
            $appPoolState = (Get-WebAppPoolState -Name $AppPoolName).Value
            if($appPoolState -ne "Started")
            {
                Throw-Error "Application pool [$AppPoolName] is not started. Please check if provided credentials are correct."
            }
        }

        Log-ActionItem "Check web site [$WebsiteName] state"
        $siteState = Get-WebSiteStateSafe -Name $WebsiteName
        Log-ActionResult "Site is in [$siteState] state"
        if($siteState -ne "Started")
        {
            $httpPort = $WebSiteConfig.Port
            $httpsPort = $WebSiteConfig.PortSSL
            $tcpPort = $WebSiteConfig.PortTcp
            
            # List all the listening port
            $portListeningInfo = netstat -aonb
            Log-ActionResult $portListeningInfo

            $startWebSiteAction = {
                Log-ActionItem "Trying to start the website."
                Start-Website -Name $WebsiteName
                Log-ActionResult "Website started successfully."
            }
                        
            Perform-RetryOnDelegate -numTries 10 -numSecondsToSleep 30 -delegateCommand $startWebSiteAction

            $siteState = Get-WebSiteStateSafe -Name $WebsiteName
            if($siteState -ne "Started")
            {
                Throw-Error "Site [$WebsiteName] is not started. Please check if provided ports: http=[$httpPort] https=[$httpsPort] tcp=[$tcpPort] are not used by any other site."
            }
        }
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Site and / or application pool are not Started. Check your configuration."
    }
}

# When we create a website in IIS, there seems to be a time lag
# between execution of the command and IIS actually preparing the
# website, To prevent unwanted code exceptions we're introducing the
# below method to poll IIS at regular intervals.
function Get-WebSiteStateSafe(
    [string]$Name = $(Throw 'Name parameter required'))
{
    $secondsToSleep = 10
    $numOfTries = 7
    for($try = 1;$try -le $numOfTries; ++$try)
    {
        try
        {
            $WebsiteState = (Get-WebSiteState -Name $Name).Value
            break;
        }
        catch
        {
            # If we reach here, the process should sleep for sometime,
            # if we haven't reached the limit to the number of try's.
            if($try -eq $numOfTries)
            {
                $lastTryError = $_
            }
            else
            {
                Start-Sleep -Seconds $secondsToSleep
                $totalWaitTime += $secondsToSleep
            }
        }
    }    
    if($lastTryError)
    {
        Log-Exception $lastTryError
        Throw-Error "Failed to retrieve website [$Name] state after [$totalWaitTime] seconds of waiting."
    }
        
    return $WebsiteState
}

# Removes folder if it exists
function Remove-FolderRecurse(
    [string]$Path = $(Throw 'Path parameter required'))
{
    Log-ActionItem "Checking if [$Path] exists."
    
    if(Test-Path $Path)
    {
        Log-ActionResult "Yes"

        # Files maybe still used by IIS even though application and website were already removed.
        # So removing files may require some time to wait.
        $numTries = 3
        $numSecondsToSleep = 5
        for($try = 1; $try -le $numTries; ++$try)
        {
            Log-ActionItem "Removing [$Path]. Attempt #[$try]"
            try
            {
                $lastTryError = $null
                Remove-Item -Path $Path -Recurse -Force
                Log-ActionResult "Removed successfully from attempt #[$try]"
                break
            }
            catch
            {
                $lastTryError = $_
            }

            if($try -ne $numTries)
            {
                Log-ActionResult "Failed to remove. Sleeping for [$numSecondsToSleep] seconds"
                Start-Sleep -Seconds $numSecondsToSleep
            }
        }

        if($lastTryError)
        {
            Log-Exception $lastTryError
            $totalWaitTime = $numSecondsToSleep * ($numTries - 1)
            Throw-Error "Failed to remove [$Path] after [$totalWaitTime] seconds of waiting"
        }
    }
    else
    {
        Log-ActionResult "No"
    }
    
    Log-ActionResult "Complete"
}

function Remove-FolderIfNotUsedByIIS(
    [string]$Path = $(Throw 'Path parameter required'))
{
    Log-ActionItem "Checking if [$Path] is used by IIS"

    [hashtable]$foldersUsedByIIS = @{}
    Get-WebApplication | % { if ($_.PhysicalPath -ne $null -and $($_.PhysicalPath).Length -gt 0) { $foldersUsedByIIS[$_.PhysicalPath] = 1 } }
    Get-WebSite | % { if ($_.PhysicalPath -ne $null -and $($_.PhysicalPath).Length -gt 0) { $foldersUsedByIIS[$_.PhysicalPath] = 1 } }
    
    # Path is used by IIS if it is a prefix of any IIS folder.
    $Path = [Environment]::ExpandEnvironmentVariables($Path)
    $folderUsedByIIS = ($foldersUsedByIIS.Keys | % { [Environment]::ExpandEnvironmentVariables($_).StartsWith($Path) }) -contains $true
    
    if($folderUsedByIIS)
    {
        Log-ActionResult "Yes. Skip removal"
    }
    else
    {
        Log-ActionResult "No"
        Remove-FolderRecurse -Path $Path
    }
}

function Remove-ServiceBinaries(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $WebApplicationInstallFolder = $WebSiteConfig.WebApplication.PhysicalPath; Validate-NotNull $WebApplicationInstallFolder "WebApplicationInstallFolder"
    $WebsiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"
    $WebSiteInstallFolder = $WebSiteConfig.PhysicalPath; Validate-NotNull $WebSiteInstallFolder "WebSiteInstallFolder"
    $ServiceBinarySourceFolder = $WebSiteConfig.WebApplication.ServiceBinarySourceFolder; Validate-NotNull $ServiceBinarySourceFolder "ServiceBinarySourceFolder"

    try
    {
        Log-Step "Remove service binaries"

        if(-not (Test-IfPathsEqual $ServiceBinarySourceFolder $WebApplicationInstallFolder))
        {
            # Delete service binaries
            Log-ActionItem "Delete service binaries from [$WebApplicationInstallFolder]"
            Remove-FolderIfNotUsedByIIS -Path $WebApplicationInstallFolder 
        }
        else
        {
            Log-ActionItem "Web application and service binary folders are the same, binaries will not be removed."
        }

        # Delete website binaries
        Log-ActionItem "Remove $WebsiteName binaries if website does not exist."
        Log-ActionItem "Checking if web site $WebsiteName exists."
        if(Get-WebSiteSafe -Name $WebsiteName)
        {
            Log-ActionResult "Yes"
            Log-ActionItem "Skip removal"
        }
        else
        {
            Log-ActionResult "No"
            Log-ActionItem "Remove service binaries from [$WebSiteInstallFolder]"

            if(-not (Test-IfPathsEqual $ServiceBinarySourceFolder $WebSiteInstallFolder))
            {
                # Delete service binaries
                Log-ActionItem "Delete service binaries from [$WebSiteInstallFolder]"
                Remove-FolderIfNotUsedByIIS -Path $WebSiteInstallFolder
            }
            else
            {
                Log-ActionItem "Website and service binary folders are the same, binaries will not be removed."
            }
        }
    }
    catch
    {
        Log-Exception $_
        throw "Error: Delete service binaries"
    }
}

# Function to set override web site install folder parameter in $WebSiteConfig with existing website install folder.
# And web application working folder with existing web application working folder.
function Update-WebSiteConfigWithInstallFolderOverrides(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    Log-Step "Setting up existing Web Site working folder parameters to WebSiteConfig if any"

    $WebsiteName = $WebSiteConfig.Name; Validate-NotNull $WebsiteName "WebsiteName"
    $WebSiteInstallFolder = $WebSiteConfig.PhysicalPath; Validate-NotNull $WebSiteInstallFolder "WebSiteInstallFolder"

    $webSite = Get-WebSiteSafe -Name $WebsiteName
    if($webSite)
    {
        if(-not (Test-IfPathsEqual $webSite.PhysicalPath $WebSiteInstallFolder))
        {
            Log-ActionItem "Existing Web Site install folder [$($webSite.PhysicalPath)] will override passed [$WebSiteInstallFolder]"
            $WebSiteConfig.PhysicalPath = $webSite.PhysicalPath
        }
    }

    Log-ActionResult "Complete"
}

function Get-WebBindingProperties(
    $binding = @(throw 'Value is required!'))
{
    $tokens = $binding.bindingInformation.Split(':')

    $result = New-Object PSObject -Prop (@{
        Protocol = $binding.Protocol
        SslFlags = $binding.SslFlags
        IPAddress = $tokens[0..($tokens.Length - 3)] -join ':'
        Port = $tokens[-2]
        HostHeader = $tokens[-1]
    })

    return $result
}

function Update-WebBinding(
    $WebSiteName = $(Throw 'Web site name is required!'),
    $Protocol,
    $Port,
    [ValidateSet('HostHeader','IPAddress','Port', 'Protocol', 'SslFlags')]
    $PropertyName = $(Throw 'Property Name is required!'),
    $Value = $(Throw 'Value is required!')
)
{
    if(-not ($Protocol -or $Port))
    {
        throw 'Either port or protocol should not be null'
    }

    $binding = Get-WebBindingSafe -Name $WebsiteName -Protocol $Protocol -Port $Port

    if($binding)
    {
        # This code is needed due to a bug in Remove-WebBinding when using IPv6, and a bug in Set-WebBinding on Windows 7 and Server 2k8
        # See: http://forums.iis.net/t/1159223.aspx?Set+WebBinding+to+change+SSL+port for the Set-WebBinding bug
        # The Remove-WebBinding bug appears to be undocumented, but for now piping Get-WebBinding into Remove-WebBinding fails to remove IPv6 bindings
        $binding | ForEach { `
            $props = Get-WebBindingProperties -Binding $_
            Remove-WebBinding -Name $WebsiteName -Protocol $props.Protocol -Port $props.Port -HostHeader $props.HostHeader
        }
        $binding | ForEach { $props = Get-WebBindingProperties $_; $props.$PropertyName = $Value; $props } | New-WebBinding -Name $WebsiteName
    }
    else
    {
        Log-TimedMessage ("Could not find website binding for website {0}, port {1} and protocol {2}. Skipping update." -f $WebsiteName, $Port, $Protocol)
    }
}

function Validate-HttpResponse(
    [string] $Url = $(Throw 'URL is required!')
)    
{
    Log-TimedMessage ("URL to test is {0}" -f $Url)
    
    $request = [System.Net.HttpWebRequest][System.Net.WebRequest]::Create($Url);
    $request.Method = "GET";
    [System.Net.HttpWebResponse]$response = [System.Net.HttpWebResponse]$request.GetResponse();
    $httpStatus = $response.StatusCode
    
    if(-not ($httpStatus -eq 'OK'))
    {
        throw ("Error occurred when trying to validate URL: {0}" -f $Url)
    }
    
    Log-TimedMessage "Finished validating URL."
}

##################################
# Functions to be used by others #
##################################

# Most of the functions in this class work off a website config hashtable, so to enable
# reuse of existing code, we can use this function to generate the required hashtable.
function Create-WebSiteConfigMap(
    [string]$webSiteName,
    [string]$appPoolName,
    [string]$httpsPort,
    [string]$certificateStore = 'My',
    [string]$certificateRootStore = 'LocalMachine',
    [string]$certificateThumbprint,
    [string]$websiteAppPoolUsername
)
{
    $result = @{}
    $result.WebAppPool = @{}
    $result.ServerCertificateRootStore = $certificateRootStore
    $result.ServerCertificateStore = $certificateStore

    if ($webSiteName)
    {
        $result.Name = $webSiteName
    }
    
    if ($appPoolName)
    {
        $result.WebAppPool.Name = $appPoolName
    }
    
    if ($httpsPort)
    {
        $result.PortSSL = $httpsPort
    }

    if ($certificateThumbprint)
    {
        $result.ServerCertificateThumbprint = $certificateThumbprint
    }

    if ($websiteAppPoolUsername)
    {
        $result.WebAppPool.ProcessModel_UserName = $websiteAppPoolUsername
    }

    return $result
}

function Start-RetailWebsiteAndAppPool(
    [string]$webSiteName = $(Throw 'webSiteName parameter required'),
    [string]$appPoolName = $(Throw 'appPoolName parameter required')
)
{
    # Define Start web app pool action.
    $startWebAppPoolAction = {
                Log-ActionItem ('Check application pool [{0}] state.' -f $appPoolName)
                $appPoolState = (Get-WebAppPoolState -Name $appPoolName).Value
                Log-ActionResult ('Application pool is in [{0}] state.' -f $appPoolState)
                if ($appPoolState -ne 'Started')
                {
                    Log-ActionItem 'Attempting to start website app pool.'
                    Start-WebAppPool -Name $appPoolName
                    Log-ActionResult 'Website app pool started successfully.'

                    $appPoolState = (Get-WebAppPoolState -Name $appPoolName).Value
                    if ($appPoolState -ne 'Started')
                    {
                        throw ('Attempt to start application pool {0} for website {1} failed.' -f $appPoolName, $websiteName)
                    }
                }
            }

    # Define Start website action.
    $startWebSiteAction = {
                Log-ActionItem ('Check web site [{0}] state.' -f $webSiteName)
                $siteState = Get-WebSiteStateSafe -Name $webSiteName
                Log-ActionResult ('Site is in [{0}] state' -f $siteState)
                if($siteState -ne 'Started')
                {
                    Log-ActionItem 'Attempting to start the website.'
                    Start-Website -Name $websiteName
                    Log-ActionResult 'Website started successfully.'

                    $siteState = Get-WebSiteStateSafe -Name $websiteName
                    if ($siteState -ne 'Started')
                    {
                        throw ('Attempt to start website {0} failed.' -f $websiteName)
                    }
                }
            }

    # Start web app pool and validate.
    Perform-RetryOnDelegate -numTries 10 -numSecondsToSleep 30 -delegateCommand $startWebAppPoolAction
    
    # Start website and validate.
    Perform-RetryOnDelegate -numTries 10 -numSecondsToSleep 30 -delegateCommand $startWebSiteAction
}

function Stop-RetailWebsiteAndAppPool(
    [string]$webSiteName = $(Throw 'webSiteName parameter required'),
    [string]$appPoolName = $(Throw 'appPoolName parameter required')
)
{
    # Define Start web app pool action.
    $stopWebAppPoolAction = {
                Log-ActionItem ('Check application pool [{0}] state.' -f $appPoolName)
                $appPoolState = (Get-WebAppPoolState -Name $appPoolName).Value
                Log-ActionResult ('Application pool is in [{0}] state.' -f $appPoolState)
                if ($appPoolState -ne 'Stopped')
                {
                    Log-ActionItem 'Attempting to stop website app pool.'
                    Stop-WebAppPool -Name $appPoolName
                    Log-ActionResult 'Website app pool stopped successfully.'

                    $appPoolState = (Get-WebAppPoolState -Name $appPoolName).Value
                    if ($appPoolState -ne 'Stopped')
                    {
                        throw ('Attempt to stop application pool {0} for website {1} failed.' -f $appPoolName, $websiteName)
                    }
                }
            }

    # Define Start website action.
    $stopWebSiteAction = {
                Log-ActionItem ('Check web site [{0}] state.' -f $webSiteName)
                $siteState = Get-WebSiteStateSafe -Name $webSiteName
                Log-ActionResult ('Site is in [{0}] state' -f $siteState)
                if ($siteState -ne 'Stopped')
                {
                    Log-ActionItem 'Attempting to stop the website.'
                    Stop-Website -Name $websiteName
                    Log-ActionResult 'Website stopped successfully.'

                    $siteState = Get-WebSiteStateSafe -Name $websiteName
                    if ($siteState -ne 'Stopped')
                    {
                        throw ('Attempt to stop website {0} failed.' -f $websiteName)
                    }
                }
            }

    # Stop web app pool and validate.
    Perform-RetryOnDelegate -numTries 10 -numSecondsToSleep 30 -delegateCommand $stopWebAppPoolAction

    # Stop website and validate.
    Perform-RetryOnDelegate -numTries 10 -numSecondsToSleep 30 -delegateCommand $stopWebSiteAction
}

function Remove-RetailWebsiteAndAssociatedBinaries(
    [string]$webSiteName = $(Throw 'webSiteName parameter required'),
    [string]$appPoolName = $(Throw 'appPoolName parameter required'),
    [string]$webSitePhysicalPath = $(Throw 'webSitePhysicalPath parameter required')
)
{
    $webSiteConfig = Create-WebSiteConfigMap -webSiteName $webSiteName -appPoolName $appPoolName
    
    Remove-ApplicationPoolSafe -WebSiteConfig $webSiteConfig
    Remove-WebSiteSafe -WebSiteConfig $webSiteConfig
    Remove-FolderIfNotUsedByIIS -Path $webSitePhysicalPath
}

#########################
# Main Install function #
#########################
function Install-WebApplication(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'),
    
    [System.Management.Automation.PSCredential[]]$Credentials = $(Throw 'Credentials parameter required')
)
{
    $WebApplicationName = $WebSiteConfig.WebApplication.Name; Validate-NotNull $WebApplicationName "WebApplicationName"
    Write-Host "------------------------------------------"
    Write-Host " Installing web application [$WebApplicationName]"
    Write-Host "------------------------------------------"

    try
    {
        Validate-WebManagementPrerequisites
        Update-WebSiteConfigWithInstallFolderOverrides -WebSiteConfig $WebSiteConfig
        CreateAndConfigure-ApplicationPool -WebSiteConfig $WebSiteConfig -Credentials $Credentials
        CreateAndConfigure-WebSite -WebSiteConfig $WebSiteConfig 
        CreateAndConfigure-WebApplication -WebSiteConfig $WebSiteConfig
        Remove-UnusedWebsiteBindings -WebSiteConfig $WebSiteConfig
        Check-IfWebApplicationIsUpAndRunning -WebSiteConfig $WebSiteConfig 
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Failed to install web application [$WebApplicationName]"
    }
    Write-Host "------------------------------------------"
    Write-Host "Successfully installed web application [$WebApplicationName]"
    Write-Host "------------------------------------------"
}

###########################
# Main Uninstall function #
###########################
function Uninstall-WebApplication(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'))
{
    $WebApplicationName = $WebSiteConfig.WebApplication.Name; Validate-NotNull $WebApplicationName "WebApplicationName"
    Write-Host "------------------------------------------"
    Write-Host " Uninstalling web application [$WebApplicationName]"
    Write-Host "------------------------------------------"

    try
    {
        Validate-WebManagementPrerequisites
        Update-WebSiteConfigWithInstallFolderOverrides -WebSiteConfig $WebSiteConfig
        Remove-WebApplicationSafe -WebSiteConfig $WebSiteConfig
        Remove-ApplicationPoolSafe -WebSiteConfig $WebSiteConfig
        Remove-WebSiteSafe -WebSiteConfig $WebSiteConfig
        Remove-ServiceBinaries -WebSiteConfig $WebSiteConfig
    }
    catch
    {
        Log-Exception $_
        Throw-Error "Failed to uninstall web application [$WebApplicationName]"
    }
    Write-Host "------------------------------------------"
    Write-Host "Successfully uninstalled web application [$WebApplicationName]"
    Write-Host "------------------------------------------"
}
# SIG # Begin signature block
# MIIdpgYJKoZIhvcNAQcCoIIdlzCCHZMCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUzrmWkL5r9ehwat6uE5pBYS8n
# KvCgghhkMIIEwzCCA6ugAwIBAgITMwAAAKxjFufjRlWzHAAAAAAArDANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBKwwggSoAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCBwDAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQU3nC5KQvoy1nSqU41EsNAVrTZBXYwYAYKKwYB
# BAGCNwIBDDFSMFCgHoAcAEMAbwBtAG0AbwBuAC0AVwBlAGIALgBwAHMAMaEugCxo
# dHRwOi8vd3d3Lk1pY3Jvc29mdC5jb20vTWljcm9zb2Z0RHluYW1pY3MvIDANBgkq
# hkiG9w0BAQEFAASCAQARNuoxBd/XtM055vms9Z7NszwVYoOzqjl9hmLxMUCrKFlh
# aTx77BOpXQ6on+Kwte2N2p7djYbEyzkwXP845vSgTvvz3f6UdCXTSjwvrmRvfA+9
# /wVrU5UaPQBsgMv+rz1b3jwN3R4T8xXf2JSMDoG6xLRrPdls0v346F5K/MhXE46d
# 8PPIB41Gyh702wLrSTan4Vw6SG+pWVaNqMdwrT3hgsYvr8o275Il+6hgHRUuhyBL
# 60zrcoao7SuoHICdL+qdZr8BK5IKQHsivktfPfGBIdOyaUOtU9VJBRPRy7xoiTOi
# 31EfqUR/ct3N+iyLGsztdALsVwHWqJko8QTrZZKYoYICKDCCAiQGCSqGSIb3DQEJ
# BjGCAhUwggIRAgEBMIGOMHcxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5n
# dG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9y
# YXRpb24xITAfBgNVBAMTGE1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQQITMwAAAKxj
# FufjRlWzHAAAAAAArDAJBgUrDgMCGgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3
# DQEHATAcBgkqhkiG9w0BCQUxDxcNMTYwNzIxMjEwNjQ0WjAjBgkqhkiG9w0BCQQx
# FgQU0jR2mWH0yiaXtQ4TGMor6EJsCZYwDQYJKoZIhvcNAQEFBQAEggEABH3ByWPc
# +5WM0IatbVEB5xStJ+aA0ihw+EklgMA3AQp4Cpb3DyHYwcL6FQmwL3iDMKlag1EG
# vTKzwa91FQHAUgkBIZM8LSQTmZqw9GH0TG38GExVOl5lJ8qOqr79HwKnvA8EUpC3
# kkQhrXAJbyWzq3d5rB3rEfsvmQDNVAWtIuOREMMVnBQNFgoXc2NU8GBNvbuVSxOz
# dLj49MLFd4f0/kLqaGAqr5OKpV9A8hUuJGqF2ZTllK5182nL+8SvRAK/E41Id1hf
# GOmZ/USL4R+By7Dd/9Bv35khfJaqjft1vdRav2KAg3oQtwyJb41SSnNs1tsuNQ3g
# A8E5nqRsvjxg/Q==
# SIG # End signature block
