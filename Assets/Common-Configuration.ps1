<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

${global:PrerequisiteFailureReturnCode} = 100

function New-ErrorObject(
    [int]$ErrorCode = $(throw 'ErrorCode is required'),
    [string]$ErrorDescription = $(throw 'ErrorDescription is required'),
    [string]$ErrorResolution,
    $InnerErrorObject)
{
    $errorObject = New-Object System.Object
    [void]($errorObject | Add-Member -type NoteProperty -name ErrorCode -value $ErrorCode -Force)
    [void]($errorObject | Add-Member -type NoteProperty -name ErrorDescription -value $ErrorDescription -Force)
    [void]($errorObject | Add-Member -type NoteProperty -name ErrorResolution -value $ErrorResolution -Force)
    [void]($errorObject | Add-Member -type NoteProperty -name InnerErrorObject -value $InnerErrorObject -Force)
    return $errorObject
}

function Test-CustomErrorObject($errorObject)
{
    return $errorObject -and $errorObject.ErrorCode -and $errorObject.ErrorDescription
}

# This table is to be extended with new errors.
function Get-ErrorTable
{
    if($global:__ErrorObjects -eq $null)
    {
        [hashtable]$global:__ErrorObjects = @{
            'ModernPosGpUpdateFailed' = New-ErrorObject -ErrorCode 300001 `
                -ErrorDescription "Installer wasn't able to update group policy. Retail Modern POS has been installed for the administrator account that ran the installer." `
                -ErrorResolution 'Check connectivity to the domain controller. Running "gpupdate /force" from a Command Prompt fixes the issue once connectivity has returned. If this command is not run, Retail Modern POS will not be installed for any other user in the system.'
        }
    }

    return $global:__ErrorObjects;
}

function Get-ErrorObject([string]$errorId = $(throw 'errorId is required'))
{
    $errorTable = Get-ErrorTable
    $result = $errorTable[$errorId]

    if($result -eq $null)
    {
        $result = New-ErrorObject -ErrorCode 300000 -ErrorDescription ('Unknown error code ID "{0}".' -f  $errorId)
    }
    
    $stackTraceString = Get-PsCallStack | Format-Table Command, Location -AutoSize | Out-String -Width 4096
    [void]($result | Add-Member -type NoteProperty -name StackTrace -value $stackTraceString -Force)
    
    return $result
}

function Add-NonTerminatingError([string]$errorId = $(throw 'errorId is required'))
{
    if($global:__NonTerminatingError -eq $null)
    {
        [array]$global:__NonTerminatingError = @()
    }
    
    $errorObject = Get-ErrorObject -errorId $errorId 
    
    # Add to the head similar of what $error variable is.
    $global:__NonTerminatingError = @($errorObject) + $global:__NonTerminatingError
}

function Get-NonTerminatingError
{
    $result = @()
    if($global:__NonTerminatingError)
    {
        $result = $global:__NonTerminatingError.Clone()
    }
    
    return $result
}

function Get-LastNonTerminatingError
{
    return Get-NonTerminatingError | Select-Object -First 1
}

function Expand-VariablesFromSettingsFile([string]$inputXmlFilePath, [string]$settingsXmlFilePath)
{
    if((Test-Path -Path $inputXmlFilePath) -ne $True)
    {
        throw "File $inputXmlFilePath was not found!";
    }

    if((Test-Path -Path $settingsXmlFilePath) -eq $True)
    {
        [xml]$settingsXml = get-content $settingsXmlFilePath;
        $settingsNodes = $settingsXml.selectNodes("Settings/Setting");
        [hashtable]$settingsTable = @{};
        foreach($settingsNode in $settingsNodes)
        {
            $key = $settingsNode.GetAttribute("key");
            $value = $settingsNode.GetAttribute("value");
            $settingsTable.Add($key, $value)
        }
    }
    
    [hashtable]$expandedSettingsTable = @{};
    # make a pass through the values to expand values, push into new hashtable
    foreach($settingsKey in $settingsTable.Keys)
    {
        $settingsValue = $settingsTable[$settingsKey]
        $updatedSettingsValue = $settingsValue;

        # does the value include a placeholder? If so, update it after looking it up.
        $matches = [System.Text.RegularExpressions.Regex]::Matches($settingsValue, "(\[\w+\])")
        if($matches -ne $null)
        {
            foreach($match in $matches)
            {  
                $key = ($match.Groups[1].Value).Replace("[", "").Replace("]", "")
                $value = $settingsTable[$key]
                $updatedSettingsValue = $updatedSettingsValue.Replace("[" + $key + "]", $value);
            }
        }
        
        $expandedSettingsTable.Add($settingsKey, $updatedSettingsValue);
    }
    
    
    return Expand-VariablesFromTable $inputXmlFilePath $expandedSettingsTable
}

function Expand-VariablesFromTable([string]$inputXmlFilePath, [hashtable]$settingsTable)
{
    if((Test-Path -Path $inputXmlFilePath) -ne $True)
    {
        throw "File $inputXmlFilePath was not found!";
    }

    [string]$content = get-content $inputXmlFilePath;

    if($settingsTable -ne $null)
    {
        foreach($key in $settingsTable.Keys)
        {
            $value = $settingsTable[$key];
            $content = $content.Replace("[" + $key + "]", $value);
        }
    }

    $content = [System.Environment]::ExpandEnvironmentVariables($content);
    
    # make sure we do not have any tokens left
    $matches = [System.Text.RegularExpressions.Regex]::Matches($content, "(\[\w+\])")
    if($matches -ne $null)
    {
        foreach($match in $matches)
        {  
            $values += " " + $match.Groups[1].Value;
        }
    
        throw "Found the following tokens that did not have values supplied:$values!";
    }
    
    $outputXml = New-Object -TypeName System.Xml.XmlDocument;
    $outputXml.LoadXml($content);
    $tempOutputFile = [System.IO.Path]::GetTempFileName()
    $outputXml.Save($tempOutputFile)
    Write-Host Saved final topology file at $tempOutputFile
    return $outputXml;
}

function Get-DatabaseConfigurationFromXPath([xml]$xmlDocument, [string]$xpath)
{
    $nodelist = $xmlDocument.selectnodes($xpath)
    $firstnode = $nodelist.item(0)
    
    if($nodelist -eq $null)
    {
        throw "Could not find expected node $xpath";
    }
    
    return Get-DatabaseConfiguration $xmlDocument $firstNode
}

function Get-DatabaseConfigurationsFromXmlNodeList([xml]$xmlDocument, [System.Xml.XmlNodeList]$xmlNodeList)
{
    [array]$dbConfigs = @();
    foreach($xmlNode in $xmlNodeList)
    {
        $dbConfig = Get-DatabaseConfiguration $xmlDocument $xmlNode
        $dbConfigs = $dbConfigs + $dbConfig
    }

    return $dbConfigs
}

function Get-DatabaseConfiguration([xml]$xmlDocument, $dbConfigNode)
{
    if($verbose -eq $true)
    {
        Write-Host Found config $dbConfigNode.get_OuterXml();
    }

    [hashtable]$ht = @{};
    $ht.Install = $dbConfigNode.GetAttribute("install");
    $ht.DropIfExists = $dbConfigNode.GetAttribute("dropifexists");
    $ht.ServerName = $dbConfigNode.SelectSingleNode("ServerName").get_InnerXml();
    $ht.InstanceName = $dbConfigNode.SelectSingleNode("ServerNamedInstanceName").get_InnerXml();
    $ht.DatabaseName = $dbConfigNode.SelectSingleNode("DatabaseName").get_InnerXml();
    $ht.InstallationType = $dbConfigNode.SelectSingleNode("Installation/InstallationType").get_InnerXml();
    $installationValueNode  = $dbConfigNode.SelectSingleNode("Installation/InstallationValue");
	$maxSqlServerMemoryLimitRatio = $dbConfigNode.SelectSingleNode("Installation/MaxSqlServerMemoryLimitRatio");
    $upgradeNode = $dbConfigNode.SelectSingleNode("Installation/Upgrades");
    $databaseFilesMinSizeInMB = $dbConfigNode.SelectSingleNode("Installation/DatabaseFilesMinSizeInMB");
    $databaseFilesGrowthRateInPercent = $dbConfigNode.SelectSingleNode("Installation/DatabaseFilesGrowthRateInPercent");
    $databaseAutoClose = $dbConfigNode.SelectSingleNode("Installation/DatabaseAutoClose");
    
	$sqlUserNameNode = $dbConfigNode.SelectSingleNode("SqlUserName")
	if($sqlUserNameNode)
	{
		$ht.SqlUserName = $sqlUserNameNode.get_InnerXml();
	}
	
	if($maxSqlServerMemoryLimitRatio -ne $null)
	{
		$ht.MaxSqlServerMemoryLimitRatio = $maxSqlServerMemoryLimitRatio.get_InnerXml();
	}

	if($databaseFilesMinSizeInMB)
	{
		$ht.DatabaseFilesMinSizeInMB = $databaseFilesMinSizeInMB.get_InnerXml();
	}    
    
	if($databaseFilesGrowthRateInPercent)
	{
		$ht.DatabaseFilesGrowthRateInPercent = $databaseFilesGrowthRateInPercent.get_InnerXml();
	}    
	
    if($databaseAutoClose)
    {
        # This is boolean value.
        $ht.DatabaseAutoClose = $databaseAutoClose.get_InnerXml().Trim() -eq 'true'
    }
    
	if($installationValueNode -ne $null)
    {
        $ht.InstallationValue = $installationValueNode.get_InnerXml();
    }
    else
    {
        $ht.InstallationValue = [string]::Empty;
    }

    # sql auth
    [array]$SqlLogins = @();
    foreach($node in $dbConfigNode.SelectNodes("SqlLogin"))
    {
        [hashtable]$sqlLogin = @{};
        $sqlLogin.Id = $node.GetAttribute("id");
        $sqlLogin.Name = $node.GetAttribute("Name");
        $sqlLogin.Password = $node.GetAttribute("Password");
        $sqlLogin.MappedSqlRoleName = $node.GetAttribute("MappedSqlRoleName");
        $SqlLogins = $SqlLogins + $sqlLogin
    }
    $ht.SqlLogins = $SqlLogins

    # windows auth
    [array]$WindowsLogins = @();
    foreach($node in $dbConfigNode.SelectNodes("WindowsLogin"))
    {
        [hashtable]$windowsLogin = @{};
        $windowsLogin.Id = $node.GetAttribute("id");
        $windowsLogin.GroupName = $node.GetAttribute("GroupName");
        $windowsLogin.CreateIfNotExists = $node.GetAttribute("CreateIfNotExists");
        $windowsLogin.UserName = $node.GetAttribute("UserName");
        $windowsLogin.MappedSqlRoleName = $node.GetAttribute("MappedSqlRoleName");
        $WindowsLogins = $WindowsLogins + $windowsLogin
    }
    $ht.WindowsLogins = $WindowsLogins

    # upgrades
    if($upgradeNode -ne $null)
    {
        $RetailScriptPath = $upgradeNode.SelectSingleNode("RetailScriptPath").get_InnerXml();
        $ht.RetailScriptPath = $RetailScriptPath;
        $CustomScriptPath = $upgradeNode.SelectSingleNode("CustomScriptPath").get_InnerXml();
        $ht.CustomScriptPath = $CustomScriptPath;
    }

    return $ht;
}

function Get-TrustedIdentityTokenIssuerConfiguration([xml]$xmlDocument, $firstnode)
{
    if($verbose -eq $true)
    {
        Write-Host Found config $firstnode.get_OuterXml();
    }

    [hashtable]$ht = @{};

    $ht.Name = $firstnode.SelectSingleNode("Name").get_InnerXml();
    $ht.Description = $firstnode.SelectSingleNode("Description").get_InnerXml();
    $ht.IdClaimTypeDisplayName = $firstnode.SelectSingleNode("IdClaimTypeDisplayName").get_InnerXml();
    $ht.Realm = $firstnode.SelectSingleNode("Realm").get_InnerXml();
    $ht.SignInUrl = $firstnode.SelectSingleNode("SignInUrl").get_InnerXml();
    $ht.CertificateDirectory = $firstnode.SelectSingleNode("CertificateDirectory").get_InnerXml();
    $ht.CertificateLocalCopyDirectory = $firstnode.SelectSingleNode("CertificateLocalCopyDirectory").get_InnerXml();
    $ht.SigningCertificateCerFileName = $firstnode.SelectSingleNode("SigningCertificateCerFileName").get_InnerXml();
    $ht.SigningCertificatePfxFileName = $firstnode.SelectSingleNode("SigningCertificatePfxFileName").get_InnerXml();
    $ht.SigningCertificatePfxPassword = $firstnode.SelectSingleNode("SigningCertificatePfxPassword").get_InnerXml();
    $ht.SigningCertificateThumbprint = $firstnode.SelectSingleNode("SigningCertificateThumbprint").get_InnerXml();
    $ht.SigningCertificateUser = $firstnode.SelectSingleNode("SigningCertificateUser").get_InnerXml();
    $ht.SslCertificateAuthorityCerFileName = $firstnode.SelectSingleNode("SslCertificateAuthorityCerFileName").get_InnerXml();
    $ht.SslCertificateAuthorityThumbprint = $firstnode.SelectSingleNode("SslCertificateAuthorityThumbprint").get_InnerXml();

    return $ht;
}

function Get-WebApplicationConfiguration([xml]$xmlDocument, $firstnode)
{
    if($verbose -eq $true)
    {
        Write-Host Found config $firstnode.get_OuterXml();
    }

    [hashtable]$ht = @{};
    $ht.Id = $firstnode.GetAttribute("id");
    $ht.Install = $firstnode.GetAttribute("install");
    $ht.DeleteIfExists = $firstnode.GetAttribute("deleteifexists");
    $ht.ApplicationPoolAccount = $firstnode.SelectSingleNode("ApplicationPoolAccount").get_InnerXml();
    $ht.ApplicationPoolName = $firstnode.SelectSingleNode("ApplicationPoolName").get_InnerXml();

    $sslBindingNode = $firstnode.SelectSingleNode("SSLBinding")
    if($sslBindingNode -ne $null)
    {
        $ht.Port = $sslBindingNode.SelectSingleNode("Port").get_InnerXml();
        $ht.IISSiteName = $sslBindingNode.SelectSingleNode("IISSiteName").get_InnerXml();
        $ht.CertificateThumbprint = $sslBindingNode.SelectSingleNode("CertificateThumbprint").get_InnerXml();

        $certificateNode = $sslBindingNode.SelectSingleNode("Certificate")
        if ($certificateNode -ne $null)
        {
            $ht.CertificateDirectory = $certificateNode.SelectSingleNode("CertificateDirectory").get_InnerXml();
            $ht.CertificatePfxFileName = $certificateNode.SelectSingleNode("CertificatePfxFileName").get_InnerXml();
            $ht.CertificateLocalCopyDirectory = $certificateNode.SelectSingleNode("CertificateLocalCopyDirectory").get_InnerXml();
            $ht.CertificatePfxPassword = $certificateNode.SelectSingleNode("CertificatePfxPassword").get_InnerXml();
        }
    }
    
    # zones
    [array]$Zones = @();
    foreach($node in $firstnode.SelectNodes("Zone"))
    {
        [hashtable]$zone = @{};
        $zone.Id = $node.SelectSingleNode("Id").get_InnerXml();
        $zone.Name = $node.SelectSingleNode("Name").get_InnerXml();
        $zone.Url = $node.SelectSingleNode("Url").get_InnerXml();
        $zone.AuthenticationProvider = $node.SelectSingleNode("AuthenticationProvider").get_InnerXml();
        $zone.AllowAnonymousAccess = $node.SelectSingleNode("AllowAnonymousAccess").get_InnerXml();
        $claimsAuthUrlNode = $node.SelectSingleNode("ClaimsAuthenticationRedirectionUrl");
        if($claimsAuthUrlNode -ne $null)
        {
            $zone.ClaimsAuthenticationRedirectionUrl = $claimsAuthUrlNode.get_InnerXml();
        }
        $Zones = $Zones + $zone
    }
    $ht.Zones = $Zones


    return $ht;
}

function Get-SiteCollectionConfiguration([xml]$xmlDocument, $firstnode)
{
    if($verbose -eq $true)
    {
        Write-Host Found config $firstnode.get_OuterXml();
    }

    [hashtable]$ht = @{};
    $ht.Id = $firstnode.GetAttribute("id");
    $ht.Install = $firstnode.GetAttribute("install");
    $ht.DeleteIfExists = $firstnode.GetAttribute("deleteifexists");
    $ht.Url = $firstnode.SelectSingleNode("Url").get_InnerXml();
    $ht.Name = $firstnode.SelectSingleNode("Name").get_InnerXml();
    $ht.OwnerEmail = $firstnode.SelectSingleNode("OwnerEmail").get_InnerXml();
    $ht.OwnerAlias = $firstnode.SelectSingleNode("OwnerAlias").get_InnerXml();
    $ht.Template = $firstnode.SelectSingleNode("Template").get_InnerXml();

    $node = $firstnode.SelectSingleNode("ResultsPageAddress");
    if($node -ne $null)
    {
        $ht.ResultsPageAddress = $node.get_InnerXml();
    }


    $node = $firstnode.SelectSingleNode("DisableVersioning");
    if($node -ne $null)
    {
        $ht.DisableVersioning = $node.get_InnerXml();
    }

    $node = $firstnode.SelectSingleNode("HostHeaderWebApplicationUrl");
    if($node -ne $null)
    {
        $ht.HostHeaderWebApplicationUrl = $node.get_InnerXml();
    }

    $node = $firstnode.SelectSingleNode("LanguageId");
    if($node -ne $null)
    {
        $ht.LanguageId = $node.get_InnerXml();
    }
    
    # site urls
    [array]$SiteUrls = @();
    foreach($node in $firstnode.SelectNodes("SiteUrls/SiteUrl"))
    {
        [hashtable]$siteUrl = @{};
        $siteUrl.Zone = $node.GetAttribute("zone")
        $siteUrl.Url = $node.GetAttribute("url")
        $SiteUrls = $SiteUrls + $siteUrl
    }
    $ht.SiteUrls = $SiteUrls

    # SPWebSettings
    [array]$SPWebSettings = @();
    foreach($node in $firstnode.SelectNodes("SPWebSettings/SPWebSetting"))
    {
        [hashtable]$sPWebSetting = @{};
        $sPWebSetting.Name = $node.GetAttribute("name")
        $sPWebSetting.Value = $node.GetAttribute("value")
        $SPWebSettings = $SPWebSettings + $sPWebSetting
    }
    $ht.SPWebSettings = $SPWebSettings

    # SPWeb AllProperties 
    [array]$SPWebAllProperties = @();
    foreach($node in $firstnode.SelectNodes("SPWebAllProperties/SPWebAllPropertiesItem"))
    {
        [hashtable]$sPWebAllPropertiesItem = @{};
        $sPWebAllPropertiesItem.Name = $node.GetAttribute("name")
        $sPWebAllPropertiesItem.Value = $node.GetAttribute("value")
        $SPWebAllProperties = $SPWebAllProperties + $sPWebAllPropertiesItem
    }
    $ht.SPWebAllProperties = $SPWebAllProperties

    return $ht;
}

function Get-WspGenerationConfiguration([xml]$xmlDocument, $firstnode)
{
    if($verbose -eq $true)
    {
        Write-Host Found config $firstnode.get_OuterXml();
    }

    [hashtable]$ht = @{};
    $ht.Generate = $firstnode.GetAttribute("generate");
    $ht.Deploy = $firstnode.GetAttribute("deploy");
    $ht.Retract = $firstnode.GetAttribute("retract");
    $ht.InstallScope = $firstnode.SelectSingleNode("InstallScope").get_InnerXml();
    
    $packageIdentifierNode = $firstnode.SelectSingleNode("PackageIdentifier");
    if ($packageIdentifierNode -ne $null)
    {
        $ht.PackageIdentifier = $packageIdentifierNode.get_InnerXml();
    }
    
    if($ht.InstallScope -eq "web")
    {
        $installScopeWebXPathNode = $firstnode.SelectSingleNode("InstallScopeWebAppXPath");
        if($installScopeWebXPathNode -ne $null)
        {
            if($installScopeWebXPathNode.get_InnerXml() -ne "")
            {
                $webApp = $xmlDocument.selectsinglenode($installScopeWebXPathNode.get_InnerText());
                $webAppConfig = Get-WebApplicationConfiguration $xmlDocument $webApp;
                $ht.InstallScopeWebUrl = $webAppConfig.Zones[0].Url;
            }
        }
    }
    
    $ht.RootFolder = $firstnode.SelectSingleNode("RootFolder").get_InnerXml();
    $ht.OutputWspName = $firstnode.SelectSingleNode("OutputWspName").get_InnerXml();
    $ht.InputWspName = $firstnode.SelectSingleNode("InputWspName").get_InnerXml();


    [hashtable]$featureProperties = @{};
    $featurePropertyNodes = $firstnode.selectNodes("FeatureProperties/Property");
    foreach($featurePropertyNode in $featurePropertyNodes)
    {
        $key = $featurePropertyNode.GetAttribute("Key");
        $value = $featurePropertyNode.GetAttribute("Value");
        $featureProperties.Add($key, $value);
    }
    
    $connectionStringPropertyNodes = $firstnode.selectNodes("PropertyForConnectionString");
    foreach($connectionStringPropertyNode in $connectionStringPropertyNodes)
    {
        $property = Get-ConnectionStringPropertyFromConfigurationXmlNode $connectionStringPropertyNode
        $featureProperties.Add($property.Key, $property.Value);
    }
    
    $ht.FeatureProperties = $featureProperties;
    return $ht;
}

function Get-ConnectionStringPropertyFromConfigurationXmlNode($connectionStringPropertyNode)
{
    $key = $connectionStringPropertyNode.GetAttribute("Key");
    $xPath = $connectionStringPropertyNode.GetAttribute("DatabaseXpath");
    $loginId = $connectionStringPropertyNode.GetAttribute("LoginId");
    
    $dbConfig = Get-DatabaseConfigurationFromXPath $connectionStringPropertyNode.OwnerDocument $xpath;
    $connectionString = Build-SqlConnectionString $dbConfig $loginId;

    [hashtable]$property = @{}
    $property.Key = $key
    $property.Value = $connectionString
    return $property
}

function Get-CustomScriptConfiguration($scriptNode)
{
    if($scriptNode.Name -eq "UpdateRetailPublishingJobAppConfig")
    {
        [hashtable]$updateRetailPublishingJobAppConfig = @{};
        $updateRetailPublishingJobAppConfig.ScriptName = $scriptNode.Name
        $updateRetailPublishingJobAppConfig.Generate = $scriptNode.GetAttribute("generate");
        $updateRetailPublishingJobAppConfig.AppConfigFile = $scriptNode.GetAttribute("appConfigFile");
        $updateRetailPublishingJobAppConfig.LoggingServiceName = $scriptNode.GetAttribute("loggingServiceName");
        $updateRetailPublishingJobAppConfig.LoggingCategoryName = $scriptNode.GetAttribute("loggingCategoryName");
        $updateRetailPublishingJobAppConfig.MonitoringEventLogSourceName = $scriptNode.GetAttribute("monitoringEventLogSourceName");
                        
        return $updateRetailPublishingJobAppConfig
    }
    elseif($scriptNode.Name -eq "CreateMobileDeviceChannel")
    {
        [hashtable]$createMobileDeviceChannel = @{};
        $createMobileDeviceChannel.ScriptName = $scriptNode.Name
        $createMobileDeviceChannel.Deploy = $scriptNode.GetAttribute("deploy");
        $createMobileDeviceChannel.Retract = $scriptNode.GetAttribute("retract");
        $createMobileDeviceChannel.ListName = $scriptNode.GetAttribute("listName");
        
        return $createMobileDeviceChannel
    }
    elseif($scriptNode.Name -eq "UpdateWorkflowFoundationConfig")
    {
        [hashtable]$updateWorkflowFoundationConfig = @{};
        $updateWorkflowFoundationConfig.ScriptName = $scriptNode.Name
        $updateWorkflowFoundationConfig.Generate = $scriptNode.GetAttribute("generate");
        $updateWorkflowFoundationConfig.WfConfigFile = $scriptNode.GetAttribute("wfConfigFile");
        $updateWorkflowFoundationConfig.PostInstallationAssetsPath = $scriptNode.GetAttribute("postInstallationAssetsPath");
                
        return $updateWorkflowFoundationConfig;
    }
    elseif($scriptNode.Name -eq "UpdateCommerceRuntimeConfig")
    {
        [hashtable]$updateCommerceRuntimeConfig = @{};
        $updateCommerceRuntimeConfig.ScriptName = $scriptNode.Name
        $updateCommerceRuntimeConfig.Generate = $scriptNode.GetAttribute("generate");
        $updateCommerceRuntimeConfig.CrtConfigFile = $scriptNode.GetAttribute("crtConfigFile");
        $updateCommerceRuntimeConfig.ChannelOperatingUnitNumber = $scriptNode.GetAttribute("channelOperatingUnitNumber");
                
        return $updateCommerceRuntimeConfig;
    }
    else
    {
        throw "Custom script with name $scriptName is not supported."   
    }
}

function Build-SqlConnectionString([hashtable]$databaseConfig, [string]$loginId)
{
    if ($databaseConfig -eq $null)
    {
        Throw "Build-SqlConnectionString: Missing argument databaseConfig."
    };

    if ($true -eq (StringIsNullOrWhiteSpace $loginId))
    {
        Throw "Build-SqlConnectionString: Missing argument loginId."
    };

    [bool]$found = $false;
    [string]$connectionString = "Server=" +$databaseConfig.InstanceName + ";Database=" + $databaseConfig.DatabaseName + ";"
    
    # find the $loginId
    foreach($sqlLogin in $databaseConfig.SqlLogins)
    {
        if($sqlLogin.Id -eq $loginId)
        {
            $connectionString = $connectionString + "User Id=" + $sqlLogin.Name + ";Password=" + $sqlLogin.Password;
            $found = $true;
            break;
        }
    }
    $sqlLogin = $null;
    
    foreach($windowsLogin in $databaseConfig.WindowsLogins)
    {
        if($windowsLogin.Id -eq $loginId)
        {
            $connectionString = $connectionString + "Trusted_Connection=Yes";
            $found = $true;
            break;
        }
    }
    $windowsLogin = $null;
    
    if($verbose -eq $true)
    {
        Write-Host Built connection string: $connectionString
    }
    
    if($found -eq $false)
    {
        Throw "Build-SqlConnectionString: The connection string could not be generated, the login with id $loginId was not found.";
    }
    
    return $connectionString
}

function CreateLocalNTGroup($computer, [string]$group, $groupDescription)
{
    if ($true -eq (StringIsNullOrWhiteSpace $computer))
    {
        Throw "CreateLocalNTGroup: Missing argument computer."
    };

    if ($true -eq (StringIsNullOrWhiteSpace $group))
    {
        Throw "CreateLocalNTGroup: Missing argument group."
    };

    if ($true -eq (StringIsNullOrWhiteSpace $groupDescription))
    {
        Throw "CreateLocalNTGroup: Missing argument groupDescription."
    };

    $parsedGroup = $group.Split("\");
    if($parsedGroup.Length -eq 2)
    {
        if($computer -eq $parsedGroup[0])
        {
            $group = $parsedGroup[1];
        }
        else
        {
            Throw "CreateLocalNTGroup: computer and group do not match. $computer, $group";
        }
    }
    
    $computer = GetExplicitComputerName $computer
    
    $ouName = "WinNT://$computer/$group"
    if($false -eq [ADSI]::Exists($ouName))
    {
        Log-TimedMessage "Group $ouName was not found. Creating it now."
        $objOu = [ADSI]"WinNT://$computer"
        $objUser = $objOU.Create("Group", $group)
        [void]$objUser.SetInfo()
        $objUser.description = $groupDescription
        [void]$objUser.SetInfo()
    }
    else
    {
        Log-TimedMessage "Group $ouName was found. Done."
    }
}

function GetTargetUserAdPath($windowsGroupMembershipNode)
{
    [string]$targetMachineName = GetExplicitComputerName $windowsGroupMembershipNode.MachineName
    [string]$localMachineName = GetExplicitComputerName("localhost");

    [string]$userName = $windowsGroupMembershipNode.UserName;

    $userNameParts = $userName.Split("\")

    # Here we determine if the configured user name should be used 'as is' 
    # or it needs to be qualified with the target machine name (as in workgroup scenario)
    if ($userNameParts.Length -lt 2) 
    {
        $isQualifiedAccountName = $false;
    }
    else 
    {
        if (0 -eq [string]::Compare(".", $userNameParts[0])) 
        {
            $userNameParts[0] = "localhost";
        }

        $userNameParts[0] = GetExplicitComputerName($userNameParts[0]);
        $userName = [string]::join("\", $userNameParts);
        
        if (0 -ne [string]::Compare($localMachineName, $userNameParts[0], $true)) 
        {
            # if the account qualifier is not equal the local machine name we assume its a domain user and use the configured value
            $isQualifiedAccountName = $true;
        }
        elseif (IfDomainControllerGetDomain -ne $null)
        {
            # if the local machine is a domain controller then the target machine cannot be in a workgroup (mix of domain and workgroup machines is not supported)
            $isQualifiedAccountName = $true;
        }
        else
        {
            # if the account qualifier is the local machine name then we assume that both machine are in workgroup and we replace account qualifier with target machine name
            # so it can be successfully created / manipulated in the context of the target machine
            $isQualifiedAccountName = $false;
            $userName = $userNameParts[1];
        }
    }

    if($isQualifiedAccountName)
    {
        Log-TimedMessage ("The account $userName is determined to be a domain account.")
        $userAdPath = "WinNT://" + $userName
    }
    else
    {
        Log-TimedMessage ("The account $userName is determined to be a local account. The target account is located on the target machine $targetMachineName")
        $userAdPath = "WinNT://" + $targetMachineName + "/" + $userName
    }

    return $userAdPath.Replace("\", "/");
}

function CheckIfADObjectExists([string]$objectAdPath, [string]$adSearchQualifier, [string]$objectDisplayName, [int]$maxAttempts = 5, [int]$sleepBetweenRetriesSecs = 10)
{
    $attemptNumber = 0  
    $objectExists = $null
    
    do 
    {
        $attemptNumber++;
        try
        {
            $objectExists = [ADSI]::Exists($objectAdPath + $adSearchQualifier);
        }
        catch
        {
            Log-Error "Failed to access active directory to find the $objectDisplayName $objectAdPath."
            if ($attemptNumber -ge $maxAttempts)
            {
                throw
            }
        }
        if ($objectExists -eq $null -and $attemptNumber -lt $maxAttempts)
        {
            Log-TimedMessage ("Sleeping for $sleepBetweenRetriesSecs seconds and retrying $objectDisplayName existence check. attemptNumber:$attemptNumber; maxAttempts:$maxAttempts")
            Start-Sleep($sleepBetweenRetriesSecs)
        }
    }
    while ($objectExists -eq $null -and $attemptNumber -lt $maxAttempts)

    if ($objectExists -eq $null)
    {
        throw "Failed to access active directory to find the $objectDisplayName $objectAdPath"  
    }
    return $objectExists;
}

function CheckIfUserExists([string]$userAdPath, [int]$maxAttempts = 4, [int]$sleepBetweenRetriesSecs = 10)
{
    return CheckIfADObjectExists $userAdPath  ",user" "user" $maxAttempts $sleepBetweenRetriesSecs
}

function CheckIfGroupExists([string]$userAdPath, [int]$maxAttempts = 4, [int]$sleepBetweenRetriesSecs = 10)
{
    return CheckIfADObjectExists $userAdPath ",group" "group" $maxAttempts $sleepBetweenRetriesSecs
}

function AddOrRemoveUserToNTGroup($windowsGroupMembershipNode)
{
    $machineName = GetExplicitComputerName $windowsGroupMembershipNode.MachineName
    $userAdPath = GetTargetUserADPath $windowsGroupMembershipNode

    $userExists = CheckIfUserExists $userAdPath
    if($userExists -eq $false)
    {
        throw "User $userAdPath from $windowsGroupMembershipNode.UserName does not exist!"  
    }

    $userDirectoryObject = [ADSI]$userAdPath

    $longWindowsGroupName = (CreateWindowsGroup $windowsGroupMembershipNode.GroupName $machineName)
    $longWindowsGroupNameParts = $longWindowsGroupName.Split('\')
    $groupNameAdPath = "WinNT://" + $longWindowsGroupNameParts[0] + "/" + $longWindowsGroupNameParts[1]

    $groupExists = CheckIfGroupExists $groupNameAdPath
    if($groupExists -eq $false)
    {
        throw "Group $groupNameAdPath does not exist!"  
    }


    $groupDirectoryObject = [ADSI]$groupNameAdPath

    $userDirectoryObjectAdsPath = $userDirectoryObject.AdsPath.ToString()

    $Exists = 0

    # PowerShell 5.0 has a bug in GetType() for COM objects IADSUser. For more details please refer the following link:
    # https://connect.microsoft.com/PowerShell/feedback/details/1437363/powershell-5-bug-in-gettype-for-com-objects-iadsuser
	$groupDirectoryObject.Members() | % {
		$memberDirectoryAdsPath = ([ADSI]$_).InvokeGet("AdsPath")
		
        if ($memberDirectoryAdsPath -ilike $userDirectoryObjectAdsPath)                  
        {                 
            Log-TimedMessage "Found user $userAdPath as a member of the group $groupNameAdPath."
            $Exists = 1
            break
        }
	}  
    
    if($windowsGroupMembershipNode.Name -ilike "add")
    {
        if($Exists -eq 0)
        {
            try
            {
                $groupDirectoryObject.Add($userAdPath);
            }
            catch [System.UnauthorizedAccessException]
            {
                $message = ("You do not have permission to add the user $userAdPath to the group $groupNameAdPath. Verify that your user account has the correct permissions. If you are using remote workgroup accounts, either enable Remote Management (http://technet.microsoft.com/en-us/library/hh921475.aspx) or manually add the user $userAdPath to the group $groupNameAdPath on the computer $machineName.")
                throw $message
            }
            catch 
            {
                Log-Exception $_
                throw "Failed to add $userAdPath to the group $groupNameAdPath. If you are using remote workgroup accounts, either enable Remote Management (http://technet.microsoft.com/en-us/library/hh921475.aspx) or manually add the user $userAdPath to the group $groupNameAdPath on the computer $machineName."
            }
            Log-TimedMessage ("The user $userAdPath was successfully added to the group $groupNameAdPath.")
        }
        else
        {
            Log-TimedMessage ("The user $userAdPath is already added to the group $groupNameAdPath.")
        }
    }
    
    if($windowsGroupMembershipNode.Name -ilike "remove")
    {
        if($Exists -eq 1)
        {
            $groupDirectoryObject.Remove($userAdPath);
            try
            {
                $groupDirectoryObject.Remove($userAdPath);
            }
            catch [System.UnauthorizedAccessException]
            {
                $message = ("You do not have permission to remove the user $userAdPath from the group $groupNameAdPath. Verify that your user account has the correct permissions. If you are using remote workgroup accounts, either enable Remote Management (http://technet.microsoft.com/en-us/library/hh921475.aspx) or manually remove the user $userAdPath from the group $groupNameAdPath on the computer $machineName.")
                throw $message
            }
            Log-TimedMessage ("The user $userAdPath was successfully removed from the group $groupNameAdPath.")
        }
        else
        {
            Log-TimedMessage ("The user $userAdPath is not member of the group $groupNameAdPath.")
        }
    }
}

# Creates a windows group, locally, remotely or on a DC.
# Returns the name of the group created.
function CreateWindowsGroup([string]$windowsLoginGroupName, [string]$serverName)
{
    # there are 2 code paths here: 1) DB creation also creates the domain groups, 2) app install makes sure domain group exists, if not creates it
    # there are two ways the windowslogingroupname may be specified
        # 1) machinename\groupname was specified
        # 2) groupname was specified

    $parts = $windowsLoginGroupName.Split("\")
    $parsedGroup = $parts[1]
    if(StringIsNullOrWhiteSpace $parsedGroup)
    {
        $parsedGroup = $windowsLoginGroupName
    }

    [string]$longWindowsGroupName = $null
    $domainName = IfDomainControllerGetDomain
    if ($domainName -ne $null)
    {
        # we are executing this on a DC
        CreateLocalDomainGroup $parsedGroup
        $longWindowsGroupName = $domainName + "\" + $parsedGroup
    }
    else
    {
        CreateLocalNTGroup $serverName $parsedGroup $parsedGroup
        $longWindowsGroupName = $serverName + "\" + $parsedGroup
    }
    
    return $longWindowsGroupName
}

function GetExplicitComputerName($computerName)
{
    if(0 -eq [string]::Compare($computerName.Trim(), "localhost", $false) `
       -or 0 -eq [string]::Compare($computerName.Trim(), "127.0.0.1", $false))
    {
        return $Env:COMPUTERNAME
    }
    else
    {
        return $computerName
    }
}

function GetExplicitWindowsUserName($userName)
{
    $parsedGroup = $userName.Split("\");
    if($parsedGroup.Length -eq 2)
    {
        return (GetExplicitComputerName $parsedGroup[0]) + "\" + $parsedGroup[1];
    }
    else
    {
        Throw "GetExplicitWindowsUserName: $userName was malformed.";
    }
}

function Log-TimedMessage([string]$message)
{
    Write-Host ('{0}: {1}' -f (Get-Date -DisplayHint Time), $message)
}

function Log-TimedError([string]$message)
{
    Write-Host ('{0}: {1}' -f (Get-Date -DisplayHint Time), $message) -ForegroundColor Red -BackgroundColor Black
}

function StringIsNullOrWhiteSpace([string]$s)
{
    return (([string]::IsNullOrEmpty($s)) -or ($s.Trim().Count -eq 0))
}

function Test-IsAdmin 
{
    try 
    {
        $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = New-Object Security.Principal.WindowsPrincipal -ArgumentList $identity
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    } 
    catch 
    {
        throw "Failed to determine if the current user has elevated privileges. The error was: '{0}'." -f $_
    }
}

function CreateLocalDomainGroup([string]$groupName)
{
    $groupTypeGlobal = 2
    $groupTypeSecurityEnabled = "&H80000000"
    $groupPrefix = "CN=" + $groupName + ",CN=Users,"
    $providerPrefix = "LDAP://"

    $root = [ADSI]"LDAP:"
    $enumerator = $root.psbase.Children.GetEnumerator()
    [void]$enumerator.MoveNext()
    $dc = $enumerator.Current
    $fullDcPath = $dc.Path
    $dcPath = $fullDcPath.Substring($providerPrefix.Length);
    $searchPath = $providerPrefix + $GroupPrefix + $dcPath;
    if ([ADSI]::Exists($searchPath))
    {
        Log-TimedMessage "Domain Group $groupName was found. Skipping creation."
    }
    else
    {
        $users = $dc.Children.Find("CN=Users")
        $newGroup = $users.Create("group", "CN=" + $groupName)
        [void]$newGroup.Put("groupType", $groupTypeGlobal -bor $groupTypeSecurityEnabled)
        [void]$newGroup.Put("sAMAccountName", $groupName)
        [void]$newGroup.SetInfo()
        Log-TimedMessage "Domain Group $groupName was created."
    }
}

# returns a valid NetBIOS domain name string if this machine is also a domain controller, otherwise $null
function IfDomainControllerGetDomain()
{
    if((Check-IfDomainController) -eq $true)
    {
        $domainName = (gwmi win32_computersystem).Domain
        Log-TimedMessage ('Found domain name: {0}' -f $domainName)
        Import-Module ActiveDirectory  | Out-Null
        $netBIOSDomainName = (Get-ADDomain -Identity $domainName).NetBIOSName
        Log-TimedMessage ('Found NetBIOS name: {0}' -f $netBIOSDomainName)
        return $netBIOSDomainName
    }
    else
    {
        return $null
    }
}

function Check-IfDomainController()
{
    $domainRole = (gwmi win32_computersystem).DomainRole
    if($domainRole -eq 5)
    {
        Log-TimedMessage ('Found that this machine is a domain controller. DomainRole: {0}' -f $domainRole)
        return $true
    }
    else
    {
        Log-TimedMessage ('Found that this machine is not a domain controller. DomainRole: {0}' -f $domainRole)
        return $false
    }
}

function IsLocalOSVersionGreaterOrEqualTo([System.Version]$lowestVersionAllowed)
{
    $wmi = (Get-WmiObject -computerName localhost -class Win32_OperatingSystem)
    [System.Version]$version = $wmi.Version
    return ($version.CompareTo($lowestVersionAllowed) -gt 0)
}

# Returns a boolean indicating if the current process runs on an OS of Windows Server 2012/Windows 8 or newer.  
function IsLocalOSWindows2012OrLater()
{
    return IsLocalOSVersionGreaterOrEqualTo "6.2"
}

# Returns a boolean indicating if the current process runs on an OS of Windows Server 2008 R2/Windows 7 or newer.  
function IsLocalOSWindows2008R2OrLater()
{
    return IsLocalOSVersionGreaterOrEqualTo "6.1"
}

# Returns a boolean indicating if the current process runs on an OS of Windows Server 2008/Windows Vista or newer.  
function IsLocalOSWindows2008OrLater()
{
    return IsLocalOSVersionGreaterOrEqualTo "6.0"
}

# Updates an xml element's attribute at a certain xpath and saves the changes into an xml file.
function UpdateXmlAttributeValue([string]$XmlFilePath, [string]$OutputXmlFilePath, [string]$xpath, [string]$attributeName, [string]$value)
{
    [xml]$xml = get-content $XmlFilePath;
    $node = $xml.SelectSingleNode($xpath);
    if($node -eq $null)
    {
        throw "The node at $xpath could not be found."
    }
    $node.SetAttribute($attributeName, $value);
    Write-Host "Updated attribute '$attributeName' at '$xpath' with '$value'."
    $OutputXmlFilePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputXmlFilePath)
    $xml.Save($OutputXmlFilePath)
    Write-Host "$OutputXmlFilePath has been written."
}

# Updates an xml element's attribute at a certain xpath and saves the changes into an xml file.
# Accepts hashtable of key value pairs.
# Applies key to xPath format.
function Update-XmlKeyValuePairs(
    [ValidateNotNullOrEmpty()]
    [ValidateScript({ Test-Path $_ })]
    [string]$XmlFilePath, 

    [ValidateNotNullOrEmpty()]
    [string]$OutputXmlFilePath, 
    
    [ValidateNotNullOrEmpty()]
    [string]$xPathFormat,

    [ValidateNotNull()]
    [hashtable]$KeyValuePairs,

    [ValidateNotNullOrEmpty()]
    [string]$ValueAttributeName = 'value')
{
    [xml]$xml = get-content $XmlFilePath
    
    foreach($key in $KeyValuePairs.Keys)
    {
        [string]$value = $KeyValuePairs[$key]

        $xPath = $xPathFormat -f $key
        $node = $xml.SelectSingleNode($xPath)

        if($node -eq $null)
        {
            throw ('The node at [{0}] could not be found.' -f $xpath)
        }

        $node.SetAttribute($ValueAttributeName, $value)
        Log-TimedMessage ('Updated attribute [{0}] at [{1}] with [{2}]' -f $ValueAttributeName, $xpath, $value)
    }    

    $OutputXmlFilePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputXmlFilePath)
    $xml.Save($OutputXmlFilePath)
    Log-TimedMessage ('[{0}] has been written.' -f $OutputXmlFilePath)
}

# Updates configuration xml element's attribute with value and saves the changes into an xml file.
function UpdateSettingsXmlAttributeValue(
    [ValidateScript( { Test-Path $_ } )]
    [string]$SettingsXmlFilePath,
    
    [ValidateScript( { -not ([string]::IsNullOrEmpty($_)) } )]
    [string]$attributeName,
    
    [string]$value)
{
    if([string]::IsNullOrEmpty($value))
    {
        Write-Host "Value for [$attributeName] to update settings .xml [$SettingsXmlFilePath] is null. Skipping update"
        return
    }
    UpdateXmlAttributeValue $SettingsXmlFilePath $SettingsXmlFilePath "Settings/Setting[@key='$attributeName']" "value" $value
}

function Get-RetailServerConfigFromXPath([xml]$xmlDocument, [string]$xpath)
{
    $node = $xmlDocument.selectsinglenode($xpath)
    if($node -eq $null)
    {
        throw "Could not find expected node $xpath";
    }
    
    [hashtable]$ht = @{};
    # TODO: Remove this when topologies and scripts for Real-time Service and Synch Service Head Office.
    $ht.InstallDependencies = $true
    
    $webSiteNode = $node.SelectSingleNode("WebSite")
    $ht.WebSite = Get-IISWebSiteConfigFromXmlNode $webSiteNode
    
    return $ht
}

function Get-RealtimeServiceConfigFromXPath([xml]$xmlDocument, [string]$xpath)
{
    $node = $xmlDocument.selectsinglenode($xpath)
    if($node -eq $null)
    {
        throw "Could not find expected node $xpath";
    }
    
    [hashtable]$ht = @{};
    $ht.AOSServer = $node.SelectSingleNode("AOSServer").get_InnerXml();
    $ht.EnableMetadataExchange = $node.SelectSingleNode("EnableMetadataExchange").get_InnerXml();
    
    $webSiteNode = $node.SelectSingleNode("WebSite")
    $ht.WebSite = Get-IISWebSiteConfigFromXmlNode $webSiteNode
    
    return $ht
}

function Get-IISAppPoolConfigFromXmlNode($node)
{
    [hashtable]$ht = @{};
    $ht.Name = $node.SelectSingleNode("Name").get_InnerXml();
    $ht.ProcessModel_IdentityType = $node.SelectSingleNode("ProcessModel_IdentityType").get_InnerXml();
    $ht.ProcessModel_UserName = $node.SelectSingleNode("ProcessModel_UserName").get_InnerXml();
    $ht.Enable32BitAppOnWin64 = Get-XmlAttributeValue $node "Enable32BitAppOnWin64"

    return $ht;
}

function Get-XmlAttributeValue($node, $attributeName)
{
    $xmlNode = $node.SelectSingleNode($attributeName);
    [string] $value = ""
    
    if($xmlNode -ne $null)
    {
        $value = $xmlNode.get_InnerXml();
    }
    return $value;
}

function Get-IISWebSiteConfigFromXmlNode($node)
{
    [hashtable]$ht = @{};
   $ht.Name = Get-XmlAttributeValue $node "Name"
    $ht.Port = Get-XmlAttributeValue $node "Port"
    $ht.PortSSL = Get-XmlAttributeValue $node "PortSSL"
    $ht.PortTcp = Get-XmlAttributeValue $node "PortTcp"
    $ht.SSLCertificatePath = Get-XmlAttributeValue $node "SSLCertificatePath"
    $ht.ServerCertificateRootStore = Get-XmlAttributeValue $node "ServerCertificateRootStore"
    $ht.ServerCertificateStore = Get-XmlAttributeValue $node "ServerCertificateStore"
    $ht.ServerCertificateThumbprint = Get-XmlAttributeValue $node "ServerCertificateThumbprint"
    $ht.PhysicalPath = Get-XmlAttributeValue $node "PhysicalPath"   
    $applicationPoolXPath = Get-XmlAttributeValue $node "ApplicationPoolXPath"
    $ht.EnableMetadataExchange = Get-XmlAttributeValue $node "EnableMetadataExchange"
    $appPoolNode = $node.OwnerDocument.SelectSingleNode($applicationPoolXPath)
    if($appPoolNode -eq $null)
    {
        throw "Could not find expected node $applicationPoolXPath";
    }
    $ht.WebAppPool = Get-IISAppPoolConfigFromXmlNode $appPoolNode
    
    $webAppNode = $node.SelectSingleNode("WebApplication")
    if($webAppNode -eq $null)
    {
        throw "Could not find expected node 'WebApplication'";
    }
    $ht.WebApplication = Get-IISWebApplicationConfigFromXmlNode $webAppNode

    return $ht;
}

function Get-IISWebApplicationConfigFromXmlNode($node)
{
    [hashtable]$ht = @{}
    $ht.Name = $node.SelectSingleNode("Name").get_InnerXml();
    $ht.PhysicalPath= $node.SelectSingleNode("PhysicalPath").get_InnerXml();
    $ht.ServiceBinarySourceFolder = $node.SelectSingleNode("ServiceBinarySourceFolder").get_InnerXml();
    
    $nodeFound = $node.SelectSingleNode("RequireSSL");
    if($nodeFound -ne $null)
    {
        $ht.RequireSSL = $nodeFound.get_InnerXml();
    }
    
    $nodeFound = $node.SelectSingleNode("AllowAnonymousMetadata");
    if($nodeFound -ne $null)
    {
        $ht.AllowAnonymousMetadata = $nodeFound.get_InnerXml();
    }
    
    $applicationPoolXPath = $node.SelectSingleNode("ApplicationPoolXPath").get_InnerXml();
    $appPoolNode = $node.OwnerDocument.SelectSingleNode($applicationPoolXPath)
    if($appPoolNode -eq $null)
    {
        throw "Could not find expected node $applicationPoolXPath";
    }
    $ht.WebAppPool = Get-IISAppPoolConfigFromXmlNode $appPoolNode
    $ht.AppSettings = @()
    
    $connStringPropertiesXml = $node.Selectnodes("AppSettings/PropertyForConnectionString")
    if($connStringPropertiesXml -ne $null)
    {
        foreach($propertyXml in $connStringPropertiesXml)
        {
            $ht.AppSettings = $ht.AppSettings + (Get-ConnectionStringPropertyFromConfigurationXmlNode $propertyXml)
        }
    }

    $propertiesXml = $node.Selectnodes("AppSettings/Property")

    if($propertiesXml -ne $null)
    {
        foreach($propertyXml in $propertiesXml)
        {

            $key = $propertyXml.GetAttribute("Key");
            $value = $propertyXml.GetAttribute("Value");
            [hashtable]$property = @{}
            $property.Key = $key
            $property.Value = $value
        
            $ht.AppSettings = $ht.AppSettings + $property
        }
    }

    return $ht;
}

function Log-Step{
    param([string]$message)
    Log-TimedMessage $message
}

function Log-ActionItem{
    param([string]$message)    
    Log-TimedMessage "    - $message ..."
}

function Log-ActionResult{
    param([string]$message)
    Log-TimedMessage "      $message."
}

function Log-Error {
    param([string]$message)
    Log-TimedError -message "############### Error occurred: ###############"
    Log-TimedError -message $message
}

function Log-Exception {
    param($exception)
    $message = ($exception | fl * -Force | Out-String -Width 4096)
    # If passed object is a string, just log the string.
    if($exception -is [string])
    {
        $message = $exception
    }
    Log-Error $message
}

function Throw-Error(
    $errorObject = $(Throw 'errorObject parameter required'))
{
    Log-Exception $errorObject
    Throw $errorObject
}

function Validate-NotNull(
    $argument,

    [string]$argumentName = $(Throw 'argumentName parameter required'))
{
    if($argument -eq $null `
        -or ($argument -is [string] -and (StringIsNullOrWhiteSpace $argument)))
    {
        Throw-Error "[$argumentName] is null."
    }
}

function New-ItemPropertyNotNull(
    $Value,
    
    [string]$Path = $(Throw 'Path parameter required'),
    
    [string]$Name = $(Throw 'Name parameter required'),
    
    [string]$PropertyType = $(Throw 'PropertyType parameter required'))
{
    if($Value -ne $null)
    {
         [void](New-ItemProperty -Path $Path -Name $Name -PropertyType $PropertyType -Value $Value -Force)
    }
}
    

function Add-RegistryEntry(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'),

    [string]$RetailComponentRegistryKey = $(Throw 'RetailComponentRegistryKey parameter required'),
    
    [hashtable]$AdditionalStringProperties)
{
    $WebApplicationName = $WebSiteConfig.WebApplication.Name
    $WebConfigFilePath =  Join-Path ($WebSiteConfig.WebApplication.PhysicalPath) "Web.config"
    
    Log-Step "Enable discovery of Web application [$WebApplicationName] for monitoring purposes"
    try 
    {    
        $WebApplicationRegistryKeyName = Join-Path $RetailComponentRegistryKey $WebApplicationName
        # always overwrite
        Log-ActionItem "Save parameters of the WebApplication [$WebApplicationName] in registry path [$WebApplicationRegistryKeyName]"
        [void](New-Item $WebApplicationRegistryKeyName -Force)
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "UserName" -PropertyType "String" -Value $WebSiteConfig.WebAppPool.ProcessModel_UserName
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "ServiceInstallFolder" -PropertyType "String" -Value $WebSiteConfig.PhysicalPath
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "AppPoolName" -PropertyType "String" -Value $WebSiteConfig.WebAppPool.Name
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "WebsiteName" -PropertyType "String" -Value $WebSiteConfig.Name
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "WebApplicationName" -PropertyType "String" -Value $WebApplicationName
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "HttpPort" -PropertyType "DWord" -Value $WebSiteConfig.Port
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "HttpsPort" -PropertyType "DWord" -Value $WebSiteConfig.PortSSL
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "ApplicationConfigFilePath" -PropertyType "String" -Value $WebConfigFilePath
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "ServerCertificateThumbprint" -PropertyType "String" -Value $WebSiteConfig.ServerCertificateThumbprint
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "ServerCertificateStore" -PropertyType "String" -Value $WebSiteConfig.ServerCertificateStore
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "ServerCertificateRootStore" -PropertyType "String" -Value $WebSiteConfig.ServerCertificateRootStore
        New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name "TcpPort" -PropertyType "DWord" -Value $WebSiteConfig.PortTcp
        
        if($AdditionalStringProperties -ne $null)
        {
            foreach ($additionalStringProperty in $AdditionalStringProperties.GetEnumerator())
            {
                New-ItemPropertyNotNull -Path $WebApplicationRegistryKeyName -Name $additionalStringProperty.Key -PropertyType "String" -Value $additionalStringProperty.Value
            }
        }
    
        Log-ActionResult "Complete"    
    }
    catch
    {
        Log-Exception $_
        Write-Warning -Message "Failed: Enabling discovery of WebApplication [$WebApplicationName] for monitoring purposes"
    }
}

function Disable-WebApplicationMonitoringDiscovery(
    $WebSiteConfig = $(Throw 'WebSiteConfig parameter required'),

    [string]$RetailComponentRegistryKey = $(Throw 'RetailComponentRegistryKey parameter required'))
{
    $WebApplicationName = $WebSiteConfig.WebApplication.Name
    Log-Step "Disable discovery of Web application [$WebApplicationName] for monitoring purposes"
    
    try
    {
        $WebApplicationRegistryKeyName = Join-Path $RetailComponentRegistryKey $WebApplicationName
        Log-ActionItem "Delete registry entry [$WebApplicationRegistryKeyName]"
        if (Test-Path $WebApplicationRegistryKeyName)
        {
            Remove-Item -Path $WebApplicationRegistryKeyName -Recurse -Force
        }
        Log-ActionResult "Complete"
    }
    catch
    {
        Log-Exception $_
        Write-Warning -Message "Failed: Disable discovery of WebApplication [$WebApplicationName] for monitoring purposes"
    }
}

function Add-WindowsGroupMemberships([System.Xml.XmlNodeList]$windowsGroupMembershipsNodes)
{
    foreach($node in $windowsGroupMembershipsNodes)
    {
        if ($true -eq (StringIsNullOrWhiteSpace $node.MachineName))
        {
            throw ("MachineName has not been specified for WindowsGroupMemberShip node")
        }
        if ($true -eq (StringIsNullOrWhiteSpace $node.UserName))
        {
            throw ("UserName has not been specified for WindowsGroupMemberShip node")
        }
        if ($true -eq (StringIsNullOrWhiteSpace $node.GroupName))
        {
            throw ("GroupName has not been specified for WindowsGroupMemberShip node")
        }

        AddOrRemoveUserToNTGroup $node
    }

    Log-TimedMessage "All windows group memberships are added."
}

function Create-ApplicationLogEventSource([string]$kLogSourceName)
{
    Create-EventSource -LogName "Application" -Source $kLogSourceName
}

function Create-EventSource
{
    param(
        [string]$LogName = $(Throw 'LogName parameter required'), 
        [string]$Source = $(Throw 'Source parameter required'))
    
    Log-Step "Create Retail Monitoring event source"

    try 
    {
        if (![System.Diagnostics.EventLog]::SourceExists($Source)) 
        {
            Log-ActionItem ("Creating {0} event source in event log {1}" -f $Source, $LogName)
            New-EventLog -Source $Source -LogName $LogName
        } 
        else 
        {
            Log-ActionResult ("{0} already exists on the computer" -f $Source)
        }
        
        Log-ActionResult "Complete"
    }
    catch 
    {
        Log-Exception $_
        Throw "Failed: Create Retail event source with name $Source"
    }
}

function Create-RetailMonitoringEventSource
{
    Create-ApplicationLogEventSource "Microsoft Dynamics AX Retail Monitoring"
}

function Remove-ApplicationLogEventSource([string]$kLogSourceName)
{
    Log-Step "Remove Retail Monitoring event source"

    try 
    {
        if ([System.Diagnostics.EventLog]::SourceExists($kLogSourceName)) 
        {
            Log-ActionItem ("Removing {0} event source" -f $kLogSourceName)
            Remove-EventLog -Source $kLogSourceName
        } 
        else 
        {
            Log-ActionResult ("{0} doesn't exist on the computer" -f $kLogSourceName)
        }
        
        Log-ActionResult "Complete"
    }
    catch 
    {
        Log-Exception $_
        Throw "Failed: Remove Retail event source with name $kLogSourceName"
    }
}

# This method tries to restart the specified web application pool.
# Returns $true if the web app pool could be restarted. Returns $false if the web app pool could not be found.
# The method will fail if the web application pool was found but could not be started.
function Try-RestartWebApplicationPool([string]$webAppPoolName)
{
    Import-Module WebAdministration;
    $appPool = ((dir IIS:\AppPools) | where {$_.Name -match $webAppPoolName})
    if($appPool -ne $null)
    {
        Write-Host "Found application pool by name [$webAppPoolName]."
        $AppPoolName = $appPool.Name
        $latestAppPoolStatus = (Get-WebAppPoolState $AppPoolName).Value;
        
        if ($latestAppPoolStatus -eq 'Stopped')
        {
            Write-Host "Starting application pool [$AppPoolName]."
            Start-WebAppPool -Name $AppPoolName;
        }
        else
        {
            Write-Host "ReStarting application pool [$AppPoolName]."
            Restart-WebAppPool -Name $AppPoolName;
        }
        
        $latestAppPoolStatus = (Get-WebAppPoolState $AppPoolName).Value;
        if ($latestAppPoolStatus -ne 'Started')
        {
            $message = "Web application pool '$AppPoolName' could not be started. ExpectedState:'Started' ActualState:'$latestAppPoolStatus'. This is a requirement for deployment to proceed!"
            Write-Warning -Message $message;
            exit ${global:PrerequisiteFailureReturnCode};
        }
        return $true;
    }
    else
    {
        Write-Host "Did not find application pool by name [$webAppPoolName]."
        return $false;
    }
}

# This method runs some dummy IIS PowerShell commandlets to ensure that the WebAdministration module can be imported correctly.
function Ensure-WebAdminModuleCanBeLoaded()
{
    Try 
    {
        Import-Module WebAdministration;
        Log-TimedMessage "Attemping to run a IIS PowerShell commandlet to verify if WebAdministration was imported correctly.";
        $Websites = Get-ChildItem IIS:\Sites
    } 
    Catch [System.IO.FileNotFoundException]
    {
        # Try again.
        Log-TimedMessage "WebAdministration module might not have loaded in time. This is a bug in PowerShell. Attempting again to run a IIS PowerShell commandlet to verify if WebAdministration was imported correctly..."
        Try
        {
            $Websites = Get-ChildItem IIS:\Sites
            Log-TimedMessage "WebAdministration could be loaded correctly.";
        }
        Catch [System.IO.FileNotFoundException]
        {
            $message = "There seems to be a problem loading the WebAdministration PowerShell module. Please ensure that the module has been installed properly. This is a requirement for deployment to proceed!"
            Write-Warning -Message $message;
            exit ${global:PrerequisiteFailureReturnCode};
        }
    }
}

function Validate-SSLConfigurationParameters(
    $ConfigParameters = $(Throw 'ConfigParameters parameter required'))
{
    $WebSiteConfig = $ConfigParameters.WebSite; Validate-NotNull $WebSiteConfig "WebSiteConfig"
    
    $HttpsPort = $WebSiteConfig.PortSSL;
    $ServerCertificateThumbprint = $WebSiteConfig.ServerCertificateThumbprint;
    
    Validate-NotNull $HttpsPort "HTTPS Port"
    Validate-NotNull $ServerCertificateThumbprint "Server Certificate Thumbprint"
}

function Validate-UserIsInCredentialsArray([string]$UserName, [System.Management.Automation.PSCredential[]]$Credentials)
{
    $foundCredential = $null
    if($Credentials -eq $null)
    {
        return $false
    }

    for($i = 0; $i -le $Credentials.length - 1; $i++)
    {
        if($Credentials[$i].UserName -like $UserName)
        {
            $foundCredential = $Credentials[$i]
            break
        }
    }
    return ($foundCredential -ne $null)
}

function Fix-PowerShellRedirection()
{
    # This function is needed to guard against the PowerShell 2.0 bug in redirection.
    # Look at http://www.leeholmes.com/blog/2008/07/30/workaround-the-os-handles-position-is-not-what-filestream-expected/ for details

    $powerShellMajorVersion = $(Get-Host).Version.Major

    if ($powerShellMajorVersion -eq 2)
    {
        $bindingFlags = [Reflection.BindingFlags] "Instance,NonPublic,GetField"
        $objectRef = $host.GetType().GetField("externalHostRef", $bindingFlags).GetValue($host) 

        $bindingFlags = [Reflection.BindingFlags] "Instance,NonPublic,GetProperty"
        $consoleHost = $objectRef.GetType().GetProperty("Value", $bindingFlags).GetValue($objectRef, @()) 

        [void] $consoleHost.GetType().GetProperty("IsStandardOutputRedirected", $bindingFlags).GetValue($consoleHost, @())
        $bindingFlags = [Reflection.BindingFlags] "Instance,NonPublic,GetField"
        $field = $consoleHost.GetType().GetField("standardOutputWriter", $bindingFlags)
        $field.SetValue($consoleHost, [Console]::Out)
        $field2 = $consoleHost.GetType().GetField("standardErrorWriter", $bindingFlags)
        $field2.SetValue($consoleHost, [Console]::Out)
    }
}

# pass in name of hotfix in format of KB12345678 or 12345678 (simple regular match is done against all hot fixes installed)
function IsKBHotfix-IsInstalled([string] $kbHotfixName)
{
    $hotfix = Get-HotFix | where {$_.HotFixID -match $kbHotfixName}
    if($hotfix -eq $null)
    {
        Log-TimedMessage ("The hotfix '{0}' was not found to be installed." -f $kbHotfixName)
        return $false
    }
    else
    {
        Log-TimedMessage ("The hotfix '{0}' was found to be installed." -f $kbHotfixName)
        return $true
    }
}

function IsKB2701373-InstalledIfNeeded()
{
    if((-not (IsLocalOSWindows2012OrLater)) -and (IsLocalOSWindows2008R2OrLater))
    {
        if(-not (IsKBHotfix-IsInstalled 'KB2701373'))
        {
            Write-Warning -Message "Prerequisite check failed: Hot fix KB2701373 is applicable for this OS but was not detected. Install from http://support.microsoft.com/kb/2701373."
            return $false
        }
        else
        {
            Write-Host "Prerequisite check succeeded: Hot fix KB2701373 is correctly installed."
        }
    }
    else
    {
        Write-Host "Prerequisite check succeeded: Hot fix KB2701373 is not applicable for this OS."
    }
    
    return $true
}

# Function checks that the OS is at the appropriate version if it is the primary domain controller. Primary domain controller must be Windows 7 or Windows 2008 R2 or higher.
function CheckOsVersion-IfDomainController()
{
    if((-not (IsLocalOSWindows2008R2OrLater)) -and (IsLocalOSWindows2008OrLater))
    {
        if(Check-IfDomainController)
        {
            Write-Warning -Message "Domain controller installations are not supported on Windows Server 2008."
            return $false
        }
        else
        {
            Write-Host "Prerequisite check succeeded: Machine is not a domain controller."
        }
    }
    else
    {
        Write-Host "Prerequisite check succeeded: No issues found for this OS."
    }
    
    return $true
}

# Function to check if two file paths are equal.
function Test-IfPathsEqual(
    [string]$firstPath = $(Throw 'firstPath parameter required'),
    
    [string]$secondPath = $(Throw 'secondPath parameter required'))
{
    $directorySeparator = [IO.Path]::DirectorySeparatorChar
    [Environment]::ExpandEnvironmentVariables($firstPath).TrimEnd($directorySeparator) -eq [Environment]::ExpandEnvironmentVariables($secondPath).TrimEnd($directorySeparator)
}

# Function to perform any passed delegate function in a retry logic and throw error if it fails even after retries.
function Perform-RetryOnDelegate(
    $delegateCommand = $(Throw 'delegateCommand parameter required'),
    [int]$numTries=3, 
    [int]$numSecondsToSleep=5)
{  
        Log-ActionItem "Performing delegate command in retry logic."
        for($try = 1; $try -le $numTries; ++$try)
        {
            Log-ActionItem "Perform delegate command. Attempt #[$try]"
            try
            {
                $lastTryError = $null
                &$delegateCommand
                Log-ActionResult "Performed successfully from attempt #[$try]"
                break
            }
            catch
            {
               $lastTryError = $_
            }
            
            if($try -ne $numTries)
            {
               Log-ActionResult "Failed to perform. Sleeping for [$numSecondsToSleep] seconds"
               Start-Sleep -Seconds $numSecondsToSleep
            }
        }
        
        if($lastTryError)
        {           
            Log-Exception $lastTryError         
            $totalWaitTime = $numSecondsToSleep * ($numTries - 1)
            Throw-Error "Failed to perform the delegate after [$totalWaitTime] seconds of waiting"
        }
        
        Log-ActionResult "Performed delegate successfully."
}

# Function checks if Microsoft SQL Server package (e.g. System CLR Types, Shared Management Objects)for 2008 R2 msi is installed or not.
function Check-IfSQLPackageInstalled(
    [string]$minVersion = $(Throw 'minVersion parameter required'),
    
    [string]$registryKey = $(Throw 'registryKey parameter required'),
    
    [string]$featureName = $(Throw 'featureName parameter required'),
    
    [string]$installLink)
{   
    [System.Version]$ExpectedVersion = [System.Version]$minVersion
    [bool]$isFeatureInstalled = $false
    
    # Check if the msi is installed by inspecting the registry key.
    Write-Host "Performing a lookup for $featureName version $minVersion"
    if(Test-Path $registryKey)
    {
        # Get the installed version
        $registryValue = Get-ItemProperty -Path $registryKey
        if(($registryValue -eq $null) -or ($registryValue.Version -eq $null))
        {
            Write-Warning -Message "Prerequisite check failed: Registry value for $featureName does not exist."
        }
        else
        {
            $actualVersion = [System.Version]$registryValue.Version
            
            # Compare the actual version with the minimum version.
            if($actualVersion.CompareTo($ExpectedVersion) -ge 0)
            {
                Write-Host "Prerequisite check succeeded: $featureName version [$actualVersion] is installed."
                $isFeatureInstalled = $true
            }
            else
            {
                Write-Warning -Message "Prerequisite check failed: Expected version is [$minVersion]. Actual version found is [$actualVersion]."
            }
        }
    }
    else
    {
        # Pre-requisite check failed. Registry key not found.
        Write-Warning -Message "Prerequisite check failed: $featureName is not installed."
    }
    
    if($isFeatureInstalled -eq $false)
    {
        Write-Warning -Message "Download and install $featureName from $installLink."
    }
    
    return $isFeatureInstalled
}

# Function to check existence of Microsoft SQL Server CMD 
function Check-IfSqlCmdExists()
{
    $pathSysVariable = $ENV:Path.Replace('"','').Split(';')
    
    foreach($sqlCmdPath in $pathSysVariable)
    {
        if(-Not ([String]::IsNullOrEmpty($sqlCmdPath)))
        {
            if(Test-Path (Join-Path $sqlCmdPath "sqlcmd.exe"))
            {  
                Write-Host "Prerequisite check succeeded: SQL Server CMD exists on the machine and is included in the PATH environment variable."
                return $true
            }
        }
    }
    
    Write-Host "Prerequisite check Failed: SQL Server CMD does exists on the machine and is not included in the PATH environment variable."
    return $false
}

function Get-WindowsAccountFromSecurityIdentifier([string]$sid)
{
    $objectSID = New-Object System.Security.Principal.SecurityIdentifier($sid)
    $objUser = $objectSID.Translate([System.Security.Principal.NTAccount])

    if(([String]::IsNullOrEmpty($objUser.Value)))
    {
        Throw-Error "Cannot translate SID $sid to Windows account."
    }

    return $objUser.Value
}

function Invoke-ScriptAndRedirectOutput(
    [ScriptBlock] $scriptBlock = $(Throw 'ScriptBlock parameter required'),
    [string] $logFile = $(Throw 'LogFile parameter required'))
{
    begin
    {
        # Redefine Write-Host function behavior.
        # See http://latkin.org/blog/2012/04/25/how-to-capture-or-redirect-write-host-output-in-powershell for more details.
        function Redefine-WriteFunction(
            [string]$functionName = $(Throw 'functionName parameter required'),

            [string]$argumentToLogName = $(Throw 'argumentToLogName parameter required'))
        {
            # Create $functionName proxy.
            $metaData = New-Object System.Management.Automation.CommandMetaData (Get-Command "Microsoft.PowerShell.Utility\$functionName")
            $proxy = [System.Management.Automation.ProxyCommand]::create($metaData)

            # Change its behavior.
            # Make $functionName to output it's argument to the file.
            $additionalWriteHostBehaviour = 'for($attempt = 1; $attempt -le 3; $attempt++){ try {if($NoNewLine) { [System.IO.File]::AppendAllText("$logFile", $%argPlaceHolder%, [System.Text.Encoding]::Unicode); } else { $%argPlaceHolder% | Out-File "$logFile" -Append }; break}catch{Start-Sleep -seconds 2} }'

            # Replace '$logFile' substring with actual logFile value.
            $additionalWriteHostBehaviour = $additionalWriteHostBehaviour -replace '\$logfile', $logFile

            $additionalWriteHostBehaviour = $additionalWriteHostBehaviour -replace '%argPlaceHolder%', $argumentToLogName

            # Append additional behavior to the beginning of current behavior.
            $content = $proxy -replace '(\$steppablePipeline.Process)', "$additionalWriteHostBehaviour; `$1"

            # Load our version into the global scope.
            Invoke-Expression "function global:$functionName { $content }"	  
        }


        # Cleans up proxy function redefinition from global scope.
        function Cleanup-FunctionRedefinition(
            [string]$functionName = $(Throw 'functionName parameter required'))
        {
           Remove-Item "function:$functionName" -ErrorAction "SilentlyContinue"
        }
    }

    process
    {
        try
        {
            . Redefine-WriteFunction "Write-Host" "Object"
            . Redefine-WriteFunction "Write-Warning" "Message"
            Invoke-ScriptBlock $scriptBlock
        }
        finally
        {
            Cleanup-FunctionRedefinition "Write-Host"
            Cleanup-FunctionRedefinition "Write-Warning"
        }
    }
}

function Invoke-ScriptBlock(
    [ScriptBlock] $scriptBlock = $(Throw 'ScriptBlock parameter required'))
    {
    try
    {
        $capturedExitCode = 0
        $global:lastexitcode = 0
        $output = $scriptBlock.Invoke()
        $capturedExitCode = $global:lastexitcode

        if($output)
        {
            Log-TimedMessage $output
        }
    }
    catch
    {
        # $_ is Dot net invocation exception, we need to get rid of it
        Log-TimedError ($global:error[0] | format-list * -f | Out-String)
        throw $_.Exception.InnerException.ErrorRecord
    }

    if($capturedExitCode -ne 0)
    {
        throw ("Scriptblock exited with error code $capturedExitCode.")
    }
}

function Invoke-Script(
    [ScriptBlock] $scriptBlock = $(Throw 'ScriptBlock parameter required'),	
    [string] $logFile)
{
    if($logFile)
    {
        Invoke-ScriptAndRedirectOutput -scriptBlock $scriptBlock -logFile $logFile
    }
    else
    {
        Invoke-ScriptBlock -scriptBlock $scriptBlock
    }
}

function Invoke-Executable(
    [ValidateNotNullOrEmpty()]
    [string]$executableName = $(throw 'ExecutableName is required!'),
    [array]$arguments = @(),
    [string]$logFile)
{
    [ScriptBlock]$scriptBlock =
    {
        Log-TimedMessage ('Running {0} with arguments:{1}{2}' -f $executableName, [Environment]::NewLine, ($arguments -join [Environment]::NewLine))
        $global:lastexitcode = 0
        &$executableName $arguments
    }
    
    Invoke-Script -scriptBlock $scriptBlock -logFile $logFile
}

function Unregister-EtwManifest(
    [ValidateNotNullOrEmpty()]
    [string]$etwManifestFilePath = $(throw 'etwManifestFilePath is required!'))
{
    Log-TimedMessage 'Unregistering ETW Manifest.'
    Log-TimedMessage ('Checking if ETW Manifest file "{0}" exists.' -f $etwManifestFilePath)
    if(Test-Path $etwManifestFilePath)
    {
        Log-TimedMessage 'Yes. Performing unregistration.'
        $arguments = @('um', $etwManifestFilePath) 
        Invoke-Executable -executableName 'wevtutil' -arguments $arguments
    }
    else
    {
        Log-TimedMessage 'No. No action will be performed.'
    }

    Log-TimedMessage "Finished unregistering ETW Manifest."
}

function Validate-PathExists(
    [ValidateNotNullOrEmpty()]
    [string]$path = $(throw 'path is required!'),
    
    [ValidateNotNullOrEmpty()]
    [string]$errorMessageFormat = $(throw 'errorMessageFormat is required!'))
{
    Log-TimedMessage ('Checking if "{0}" exists.' -f $path)
    if(Test-Path -Path $path -ErrorAction 'SilentlyContinue')
    {
        Log-TimedMessage 'Yes'
    }
    else
    {
        throw ($errorMessageFormat -f $path)
    }
}

function Register-EtwManifest(
    [ValidateNotNullOrEmpty()]
    [string]$etwManifestFilePath = $(throw 'etwManifestFilePath is required!'),
    
    [ValidateNotNullOrEmpty()]
    [string]$etwManifestResourceFilePath = $(throw 'etwManifestResourceFilePath is required!'))
{
    Log-TimedMessage 'Registering ETW Manifest.'
    Validate-PathExists -path $etwManifestFilePath -errorMessageFormat 'ETW Manifest file "{0}" not found'
    Validate-PathExists -path $etwManifestResourceFilePath -errorMessageFormat 'ETW Manifest resource file "{0}" not found'

    Log-TimedMessage 'Unregister manifest first to properly update existing manifest entries'
    Unregister-EtwManifest -etwManifestFilePath $etwManifestFilePath
    
    Log-TimedMessage 'Now register the manifest'
    $arguments = 
    @(
        'im'
        $etwManifestFilePath # No quotes around the path needed!
        '/rf:{0}' -f $etwManifestResourceFilePath # No quotes around the path needed!
        '/mf:{0}' -f $etwManifestResourceFilePath # No quotes around the path needed!
    )

    Invoke-Executable -ExecutableName 'wevtutil' -arguments $arguments
    Log-TimedMessage 'Finished registering ETW Manifest.'
}

# Helper to exit script.
# We are putting the exit code in the pipeline since this is a requirement for ESS scripts to process exit codes.
# With this function, we will cater to both the needs of returning exit codes to caller and actually exiting with exit code.
function Exit-Script($exitCode = 1)
{
    $exitCode 
    exit $exitCode
}

function Get-ProductVersionMajorMinor()
{
    [string]  $productVersionMajorMinorString = '7.0'
    return $productVersionMajorMinorString
}

# Note: This function may throw if, for example, security settings prevent removing the specified key
function Drop-SelfServiceRegistryKey (
    [ValidateNotNullOrEmpty()]
    [string] $keyName = $(Throw 'KeyName parameter required'))
{
    $selfServiceRoot = 'HKLM:\SOFTWARE\Microsoft\Dynamics\Setup\SelfServiceDeployment'
    $registryKey = Join-Path $selfServiceRoot $keyName

    Log-ActionItem "Dropping self-service registry key - [$registryKey]"
    if (Test-Path $registryKey)
    {
        Remove-Item -Path $registryKey -Recurse -Force
        Log-ActionResult "Successfully dropped the self-service registry key - [$registryKey]"
    }
    else
    {
        Log-ActionResult "Nothing done, the self-service registry key [$registryKey] does not appear to exist"
    }
}

    function Write-Log(
    $objectToLog,
    [string] $logFile)
{
    try
    {
        $date = (Get-Date -DisplayHint Time)
        $message = "{0}: {1}" -f $date, $objectToLog

        if($logFile)
        {
            $message | Out-File -FilePath $logFile -Append -Force
        }
        else
        {
            Write-Host $message
        }
    }
    catch
    {
        # swallow any log exceptions
    }
}

function Copy-Files(
    [string] $SourceDirPath = $(Throw 'SourceDirPath parameter required'),
    [string] $DestinationDirPath = $(Throw 'DestinationDirPath parameter required'),
    [string] $FilesToCopy = '*',
    [string] $RobocopyOptions = '/e',
    [string] $logFile)
{
    $global:LASTEXITCODE = 0
    
    # if dir path is quoted and ends with '\', robocopy fails.
    $SourceDirPath = $SourceDirPath.TrimEnd('\')
    $DestinationDirPath = $DestinationDirPath.TrimEnd('\')
    $command = 'robocopy.exe "{0}" "{1}" "{2}" {3}' -f $SourceDirPath, $DestinationDirPath, $FilesToCopy, $RobocopyOptions
    Write-Log $command -logFile $logFile
    
    $output = Invoke-Expression $command

    $capturedExitCode = $global:LASTEXITCODE

    Write-Log ($output | Out-String) -logFile $logFile

    #Robocopy Exit codes related info: http://support.microsoft.com/kb/954404
    if(($capturedExitCode -ge 0) -and ($capturedExitCode -le 8))
    {
        Write-Log "[Robocopy] completed successfully." -logFile $logFile
        $global:LASTEXITCODE = 0
    }
    else
    {
        throw "[Robocopy] failed with exit code $capturedExitCode"
    }
}

function Backup-Directory(
    [string] $sourceFolder = $(throw 'sourceFolder is required'),
    [string] $targetFolder)
{
    Write-Log "Begin to backup web folder: $sourceFolder"

    if(-not $targetFolder)
    {
        $sourceFolderShortName = Split-Path $sourceFolder -Leaf
        $targetFolder = Join-Path (Split-Path $sourceFolder -parent) "$sourceFolderShortName-Backup-$(Get-Date -f yyyy-MM-dd_hh-mm-ss)"

        if(Test-Path -Path $targetFolder)
        {
            Remove-Item $targetFolder -Force -Recurse
        }
    }

    Copy-Files $sourceFolder $targetFolder
}

function Get-ConfigurationValueFromServiceModelXml
(
	[xml] $ServiceModelXml = $(Throw 'ServiceModelXml is required!'),
	[string] $XPath  = $(Throw 'XPath is required!')
)
{
	$node = $ServiceModelXml.SelectSingleNode($XPath)
	if(-not $node)
	{
		throw ('cannot get value from {0}' -f $XPath)
	}
	return $ServiceModelXml.SelectSingleNode($XPath).getAttribute("Value")
}

function Get-ParametersFromServiceModelXml(
	[xml] $ServiceModelXml = $(Throw 'ServiceModelXml is required!'), 
	[ref] $OutputSettingsHashset  = $(Throw 'OutputSettingsHashset  is required!'), 
	[string] $SettingName = $(Throw 'SettingName is required!'), 
	[string] $ParameterName)
{
	if(-not $ParameterName)
	{
		$ParameterName = $SettingName
	}
	$settingValue = (Get-ConfigurationValueFromServiceModelXml -ServiceModelXml $ServiceModelXml -XPath "//Configuration/Setting[@Name='$SettingName']")
	$OutputSettingsHashset.Value.Add($ParameterName,$settingValue)
}

function Get-WebConfigAppSetting(
	[xml] $WebConfig = $(Throw 'WebConfig is required!'), 	
	[string] $SettingName = $(Throw 'SettingName is required!'))
{
	$appSettingElement = $WebConfig.SelectSingleNode("/configuration/appSettings/add[@key='$SettingName']")
	
	if(!$appSettingElement)
	{
		throw "Failed to get app setting from aos web.config because the key $SettingName doesn't exist"
	}
	
	$appSettingValue = $appSettingElement.Value
		
	if(!$appSettingValue)
	{
		return ""
	}
	
	return $appSettingValue
}

function Set-WebConfigAppSetting(
    [string]$webConfigFilePath = $(Throw 'webConfigFilePath is required'),
    [string]$key = $(Throw 'key is required'),
    [string]$value = $(Throw 'value is required'))
{
    [xml]$doc = Get-Content $webConfigFilePath

    # Check and see if we have one of these elements already.
    $configElement = $doc.configuration.appSettings.add | Where-Object { $_.key -eq $key }

    # Only add a new element if one doesn't already exist.
    if (!$configElement)
    {
        $configElement = $doc.CreateElement('add')
        $xmlKeyAtt = $doc.CreateAttribute("key")
        $xmlKeyAtt.Value = $key
        $configElement.Attributes.Append($xmlKeyAtt)
        $xmlValueAtt = $doc.CreateAttribute("value")
        $xmlValueAtt.Value = $value
        $configElement.Attributes.Append($xmlValueAtt)
        $doc.configuration.appSettings.AppendChild($configElement)
    }

    $configElement.value = $value

    Set-ItemProperty $webConfigFilePath -name IsReadOnly -value $false
    $doc.Save($webConfigFilePath)
}

function Move-ConfigToConfig(
	[xml] $FromConfigXml = $(throw 'FromConfigXml is required'),
	[string] $ToConfigPath = $(throw 'ToConfigPath is required'),
	$KeysToMove = $(throw 'KeysToMove is required')
)
{
	foreach($key in $KeysToMove)
	{
		$appSettingValue = Get-WebConfigAppSetting -WebConfig $FromConfigXml -SettingName $key
		if($appSettingValue)
		{
			Set-WebConfigAppSetting -webConfigFilePath $ToConfigPath -key $key -value $appSettingValue		
		}
	}
}

function AdjustAxSetupConfig(
	[string] $AosWebConfigPath = $(throw 'AosWebConfigPath is required'),
	[string] $AxSetupExeFolder = $(throw 'AxSetupExeFolder is required')
)
{
	$AosWebRootFolder = Split-Path $AosWebConfigPath -parent
	$aosWebConfigContent = [xml](Get-Content -Path $AosWebConfigPath)
	$PostDeploymentUtilityConfigFileName = 'Microsoft.Dynamics.AX.Deployment.Setup.exe.config'
	$PostDeploymentUtilityConfigFilePath = Join-Path -Path $AxSetupExeFolder -ChildPath $PostDeploymentUtilityConfigFileName
	if (-not (Test-Path -Path $PostDeploymentUtilityConfigFilePath))
	{
		throw 'Could not find Microsoft.Dynamics.AX.Deployment.Setup.exe.config'
	}

	[xml] $PostDeploymentUtilityConfigDoc = Get-Content -Path $PostDeploymentUtilityConfigFilePath

	# These four appSettings need to be copied from aos web.config to DB utility config in order to access the enryption api from DB Sync utility context.
	$appSettingKeysForMove = @(
		'Aos.SafeMode',
		'AzureStorage.StorageConnectionString',
		'DataAccess.DataEncryptionCertificateThumbprint',
		'DataAccess.DataSigningCertificateThumbprint')
	
	Move-ConfigToConfig -FromConfigXml $aosWebConfigContent -ToConfigPath $PostDeploymentUtilityConfigFilePath -KeysToMove $appSettingKeysForMove
}

function Set-WebConfigDiagnosticsProperty(
    [string] $webConfigFilePath = $(throw 'webConfigFilePath is required'),
    [string] $sinkClassName = $(throw 'sinksClassName is required'),
    [string] $name = $(throw 'name is required'),
    [string] $value = $(throw 'value is required'))
{
    [xml]$doc = Get-Content $webConfigFilePath

    # Check and see if we have one of these elements already.
    $sinkElement = $doc.configuration.diagnosticsSection.sinks.sink | Where { $_.class -eq $sinkClassName }
    $eventDbPropertyElement = $sinkElement.Properties.Property | Where { $_.name -eq $name }
    if(!$eventDbPropertyElement)
    {
        throw ('Element was not found in {0}' -f $webConfigFilePath)
    }
    $eventDbPropertyElement.value = $value

    Set-ItemProperty $webConfigFilePath -name IsReadOnly -value $false
    $doc.Save($webConfigFilePath)
}

function Merge-WebConfigAppSettings(
    [ValidateNotNullOrEmpty()]
    [string]$sourceWebConfigFilePath = $(throw 'sourceWebConfigFilePath is required'),

    [ValidateNotNullOrEmpty()]
    [string]$targetWebConfigFilePath = $(throw 'targetWebConfigFilePath is required'),

    $nonCustomizableAppSettings)
{
    if ((Test-Path -Path $sourceWebConfigFilePath) -and (Test-Path -Path $targetWebConfigFilePath))
    {
        [xml]$sourceWebConfigFilePathDoc = Get-Content $sourceWebConfigFilePath
        [xml]$targetWebConfigFilePathDoc = Get-Content $targetWebConfigFilePath

        # Iterate over each app setting in the original deployment config.
        foreach($setting in $sourceWebConfigFilePathDoc.configuration.appSettings.add)
        {
            # Check if the app setting exists in the new config.
            $configElement = $targetWebConfigFilePathDoc.configuration.appSettings.add | Where-Object { $_.key -eq $setting.key }
            
            Log-ActionItem ('Checking if the setting with key [{0}] already exists in the new config' -f $setting.key)
            if(!$configElement)
            {
                Log-ActionResult 'No. Check if we need to retain this setting in new config'
                # Note: If the app setting does not exist in the new config but is one of the non-customizable setting, 
                # then retain it from original deployment config into the new config.
                if($nonCustomizableAppSettings -icontains $setting.key)
                {
                    Log-ActionResult 'Yes. Bringing back the app setting from deployed config'

                    $configElement = $targetWebConfigFilePathDoc.CreateElement('add')
                    $configElement.SetAttribute('key', $setting.key)
                    $configElement.SetAttribute('value', $setting.value)

                    # Append the new element
                    $targetWebConfigFilePathDoc.configuration.appSettings.AppendChild($configElement)
                }
            }
            else
            {
                Log-ActionResult ('Yes. Value is [{0}]' -f $configElement.value)

                # If an app setting is non-customizable, then retain it. Otherwise over-write with a customized value.
                Log-ActionItem ('Checking if this setting can overwrite existing value - [{0}]' -f $setting.value)
                if ($nonCustomizableAppSettings -icontains $setting.key)
                {
                    Log-ActionResult 'No! This is a non-customizable setting. Retaining the existing app setting value'
                    $configElement.value = $setting.value
                }
                else
                {
                    Log-ActionResult 'Yes. This setting can over-write the existing value'
                }
            }
        }
        
        Set-ItemProperty $targetWebConfigFilePath -name IsReadOnly -value $false
        $targetWebConfigFilePathDoc.Save($targetWebConfigFilePath)
    }
    else
    {
        throw "Either $sourceWebConfigFilePath or $targetWebConfigFilePath doesn't exist"
    }
}

function Merge-JsonFile(
    [string] $sourceJsonFilePath = $(throw 'sourceJsonFilePath is required'),
    [string] $targetJsonFilePath = $(throw 'targetJsonFilePath is required'),
    $nonCustomizableConfigSettings)
{
    if(-not (Test-Path -Path $sourceJsonFilePath))
    {
        throw "Source json file $sourceJsonFilePath doesn't exist."
    }  
    if(-not (Test-Path -Path $targetJsonFilePath))
    {
        throw "Target json file $targetJsonFilePath doesn't exist."
    }

    $sourceJsonString = Get-Content $sourceJsonFilePath
    $sourceJsonObject = "[ $sourceJsonString ]" | ConvertFrom-JSON
    $targetJsonString = Get-Content $targetJsonFilePath
    $targetJsonObject = "[ $targetJsonString ]" | ConvertFrom-JSON
    $properties = $sourceJsonObject | Get-Member -MemberType NoteProperty
          
    foreach($property in $properties)
    {
		# Retain the config setting from source file ONLY IF it is a reserved setting. Else overwrite it from the target file.
		if($nonCustomizableConfigSettings -icontains $property.Name)
		{
			$targetJsonObject | Add-Member -MemberType NoteProperty -Name $property.Name -Value $sourceJsonObject.$($property.Name) -Force
		}
    }

    $targetJsonObject | ConvertTo-Json | Out-File $targetJsonFilePath -Force 
}

function Execute-AxMethod(
    [string] $AxClassName = $(Throw 'AxClassName parameter required'),
    [string] $AxMethodName = $(Throw 'AxMethodName parameter required'),
    [string] $CallAxMethodScriptPath = $(Throw 'CallAxMethodScriptPath parameter required'),
    [string] $logFile)
{
    try
    {
        Write-Log "Applying CallAxMethod.ps1 -ClassName $axClassName -MethodName $axMethodName" -logFile $logFile
        [scriptblock]$scriptToExecute = {& $CallAxMethodScriptPath -ClassName $axClassName -MethodName $axMethodName}
        Invoke-Script -scriptBlock $scriptToExecute -logFile $logFile
    }
    catch
    {
        $errorMsg = ($global:error[0] | format-list * -f | Out-String)
        Write-Log "Failed to call AxClass $axClassName AxMethod $axMethodName, $errorMsg" -logFile $logFile
        throw "Failed to call AxClass $axClassName AxMethod $axMethodName, $errorMsg"		
    }
}

# Firewall rules
function Test-IfFirewallRuleExists(
    [ValidateNotNullOrEmpty()]
    [string]$ruleName = $(throw 'ruleName is required!')
)
{
    Log-TimedMessage ('Checking if firewall rule [{0}] exists' -f $ruleName)
    $global:lastexitcode = 0
    # no quotes around $ruleName here otherwise it won't work. It handles spaces in name just fine with no quotes.
    $output = netsh advfirewall firewall show rule name=$ruleName
    $capturedExitCode = $global:lastexitcode 
    Log-TimedMessage $output
    # If rule does not exist, netsh returns non zero exit code.
    return $capturedExitCode -eq 0
}

function Remove-FirewallRule(
    [ValidateNotNullOrEmpty()]
    [string]$ruleName = $(throw 'ruleName is required!')
)
{
    Log-TimedMessage ('Removing firewall rule [{0}].'-f $ruleName)
    if(Test-IfFirewallRuleExists -ruleName $ruleName)
    {
        Log-TimedMessage ('Firewall rule [{0}] exists. Removing.' -f $ruleName)
        $removeRuleArguments = @(
            'advfirewall'
            'firewall'
            'delete'
            'rule'
            'name={0}' -f $ruleName # no quotes around $ruleName here otherwise it won't work. It handles spaces in name just fine with no quotes.
        )

        Invoke-Executable -executableName 'netsh' -arguments $removeRuleArguments
    }
    else
    {
        Log-TimedMessage ('Firewall rule [{0}] does not exist. Skip removal.' -f $ruleName)
    }
}

function Get-FirewallRuleNamesByPrefix(
    [ValidateNotNullOrEmpty()]
    $ruleNamePrefix = $(throw 'ruleNamePrefix is required!')
)
{
    $result = @()
    # Match prefix until first space or end of line after it
    # (\s matches a character of whitespace (space, tab, carriage return, line feed)).
    # ($ means end of line).
    # (.*?) means lazy match of any characters.
    $ruleNamePrefixMatchRegexp = '{0}.*?(\s|$)' -f ([Regex]::Escape($ruleNamePrefix))

    # Get netsh output that has information about all rules
    $allRulesLines = netsh advfirewall firewall show rule all
    
    # Get only lines that contain information about rule names.
    $ruleNameLines = $allRulesLines | where { $_ -match $ruleNamePrefixMatchRegexp }
    
    if ($ruleNameLines)
    {
        # Get rule names.
        $result = $ruleNameLines | foreach { [void]($_ -match $ruleNamePrefixMatchRegexp); $matches[0] }
        $result = $result | where { $_ }
        $result = $result | foreach { $_.Trim() }
    }
    
    return $result
}

function Remove-FirewallRulesByNamePrefix(
    [ValidateNotNullOrEmpty()]
    $ruleNamePrefix = $(throw 'ruleNamePrefix is required!')
)
{
    # This function is based on raw output parsing.
    # So in case if something goes wrong. We don't want uninstall to fail.
    try
    {
        $ruleNames = Get-FirewallRuleNamesByPrefix -ruleNamePrefix $ruleNamePrefix
        $ruleNames | ForEach { Remove-FirewallRule $_ }
    }
    catch
    {
        Log-TimedError ($global:error[0] | format-list * -f | Out-String)
    }
}

function Check-IfDemoDataLoaded(
    [string] $AxDatabaseName = $(Throw 'AxDatabaseName parameter required'),
    [string] $AxDatabaseServerInstanceName = 'localhost')
{
    # Query the AX database to see if demo data set is loaded or not.
    $returnResult = $false
    $query = "SELECT * from DATAAREA where ID <> 'DAT'"
    $result = Invoke-SqlCmd -Query $query -ServerInstance $AxDatabaseServerInstanceName -Database $AxDatabaseName

    if($result)
    {
        $returnResult = $true
    }
    return $returnResult	
}

function Check-IfPerfDataLoaded(
    [string] $AxDatabaseName = $(Throw 'AxDatabaseName parameter required'),
    [string] $AxDatabaseServerInstanceName = 'localhost')
{
    # Query the AX database to see if demo data set is loaded or not.
    $returnResult = $false
    $query = "SELECT * FROM RETAILSTOREENTITY WHERE STORENUMBER = 'S1001' "
    $result = Invoke-SqlCmd -Query $query -ServerInstance $AxDatabaseServerInstanceName -Database $AxDatabaseName

    if($result)
    {
        $returnResult = $true
    }
    return $returnResult	
}

function Update-HostFileLines(
    [string[]] $HostNames = $(throw "Please provide a list of host names"),
    [string] $IPAddress = '127.0.0.1'
)
{
    $hostFilePath = Join-Path $env:windir 'System32\drivers\etc\hosts'
    $hostFileBackupPath = "{0}{1}" -f $hostFilePath, ([System.Guid]::NewGuid().ToString())

    $originalFileContent = Get-Content $hostFilePath
    Set-Content -Path $hostFileBackupPath -Value $originalFileContent -Force
    [System.Collections.ArrayList]$originalFileContentCopy = $originalFileContent.Clone()

    $HostNames | ForEach `
    {
        $hostName = $_
        $originalFileContent | ForEach  `
        {
            if(!$_.StartsWith('#') -and ($_.IndexOf($hostName) -gt 0))
            {
                    if($_.IndexOf($IPAddress) -ge 0)
                    { 
                        $originalFileContentCopy.Remove($_)
                    }
                    else
                    {
                        $originalFileContentCopy.Remove($_)
                        $originalFileContentCopy.Add("# $_")
                    }
                }
            }
        }

    $HostNames | ForEach  `
    {
        $HostNameLine = "{0} {1}" -f $IPAddress, $_ 
        $originalFileContentCopy.Add($HostNameLine)
    }

	Set-Content -Path $hostFilePath -Value $originalFileContentCopy -Force
}

function IsAosConfiguredWithStorageEmulator([string] $AosWebSiteName = 'AosWebApplication')
{
	Import-Module WebAdministration
	$aosWebsite = Get-WebSiteSafe -Name $AosWebSiteName
	if($aosWebsite)
	{		
		$aosWebConfigPath = Join-Path -Path $aosWebsite.PhysicalPath -ChildPath 'web.config'
		$aosWebConfigContent = [xml](Get-Content -Path $aosWebConfigPath)
		
		$AzureStorageStorageConnectionStringKey = 'AzureStorage.StorageConnectionString'
		$AzureStorageStorageConnectionString = $aosWebConfigContent.SelectSingleNode("/configuration/appSettings/add[@key='$AzureStorageStorageConnectionStringKey']").Value
		
		return ($AzureStorageStorageConnectionString -eq 'UseDevelopmentStorage=true')
	}
	else
	{
		return $false
	}
}

function Start-StorageEmulator
{
	$storageEmulatorRegPath = 'HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows Azure Storage Emulator'
	if(Test-Path -Path $storageEmulatorRegPath)
	{		
		$storageEmulatorReg = (Get-ItemProperty -Path $storageEmulatorRegPath)
		if($storageEmulatorReg -and $storageEmulatorReg.InstallPath)
		{
			$storageEmulatorExe = (Join-Path $storageEmulatorReg.InstallPath 'AzureStorageEmulator.exe')
			if((Test-Path -Path $storageEmulatorExe))
			{
				$global:LASTEXITCODE = 0
				try
				{
					Write-Output "Trying to start storage emulator from $storageEmulatorExe"
					& $storageEmulatorExe start
				}
				catch
				{
					Write-Output ($global:error[0] | format-list * -f | Out-String)
				}
				$global:LASTEXITCODE = 0
			}
		}
	}
}

function Read-RegistryValue(
    [ValidateNotNullOrEmpty()]
    [string]$targetRegistryKeyPath = $(throw 'targetRegistryKeyPath is required'),
    [string]$targetPropertyName = $(throw 'targetPropertyName is required')
)
{
    $targetPropertyRegistryObject = Get-ItemProperty -Path $targetRegistryKeyPath -Name $targetPropertyName -ErrorAction SilentlyContinue
    $result = $targetPropertyRegistryObject.$targetPropertyName

    return $result
}

function Set-SChannelProtocol(
    [ValidateNotNullOrEmpty()]
    [string]$protocolName = $(throw 'protocolName is required'),
    
    [ValidateSet('Server', 'Client')]
    [string]$target = $(throw 'target is required'),
    
    [ValidateSet('Enable', 'Disable')]
    [string]$action = $(throw 'action is required'))
{
    [int]$value = 0
    if ($action -eq 'Enable')
    {
        $value = 1
    }

    $registryPath = 'HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\{0}\{1}' -f $protocolName, $target
    
    if (!(Test-Path -Path $registryPath -ErrorAction 'SilentlyContinue'))
    {
        New-Item -Path $registryPath -Force
    }

    Set-ItemProperty -Path $registryPath -Name 'Enabled' -Type 'dword' -Value $value -Force

    # Required for Windows 7 to negotiate TLS 1.2, later protocols will be impacted most likely as well.
    # This property also exists in Windows 10, so it is fine to set it for all.
    if ($action -eq 'Enable')
    {
        Set-ItemProperty -Path $registryPath -Name 'DisabledByDefault' -Type 'dword' -Value 0 -Force
    }
}

function Configure-SChannelProtocols(
    [ValidateNotNull()]
    $protocols  = $(throw 'protocols is required'))
{
    foreach($protocol in $protocols)
    {
        $protocolName = $protocol.name
        $target = $protocol.target
        $action = $protocol.action

        Log-TimedMessage ('Setting SChannel protocol {0} for {1} to be {2}' -f $protocolName, $target, $action)
        Set-SChannelProtocol -protocolName $protocolName -target $target -action $action
        Log-TimedMessage 'Set completed successfully'
    }
}


Fix-PowerShellRedirection

# SIG # Begin signature block
# MIIdugYJKoZIhvcNAQcCoIIdqzCCHacCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUurITnvOQy3kGy9xg9IKGNmsE
# Kr6gghhkMIIEwzCCA6ugAwIBAgITMwAAAJ1CaO4xHNdWvQAAAAAAnTANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBMAwggS8AgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCB1DAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQUvLmg3g/pYFPZlEbUzJ/P+yA0WHkwdAYKKwYB
# BAGCNwIBDDFmMGSgMoAwAEMAbwBtAG0AbwBuAC0AQwBvAG4AZgBpAGcAdQByAGEA
# dABpAG8AbgAuAHAAcwAxoS6ALGh0dHA6Ly93d3cuTWljcm9zb2Z0LmNvbS9NaWNy
# b3NvZnREeW5hbWljcy8gMA0GCSqGSIb3DQEBAQUABIIBAIAf1mffqhAe0FNo6AxK
# esJbZEFHC6wL0/I29Vcox0UEJLDCAVxhPz8uhGGokdFtv6rUtw/5KyMg2pV3yEXP
# ukMEuUkXCmMTXHB90w6tdBFxmnwYgL26ANrJ4GFIksdhfuJR0aljd7At8sRdPD5F
# jevKDsj/uUVWZxlaT+KzKaz2WI9cX9RkJn6AC6wxltj1qIl2hNL23yLJz1KD7EYT
# kHElU2Pa2q4e7X3TM2PJ0EBdcWxkA65dcUwnTyvTvGuIBfTYfQ6EY2uufrXT2enJ
# VRVfDEO+ckXODwjR7jDxSw3tbrIEJJR1fSPmWJwZ3VTVj6bTHGGsOe3UtXwa5vPL
# 2b6hggIoMIICJAYJKoZIhvcNAQkGMYICFTCCAhECAQEwgY4wdzELMAkGA1UEBhMC
# VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
# BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWljcm9zb2Z0IFRp
# bWUtU3RhbXAgUENBAhMzAAAAnUJo7jEc11a9AAAAAACdMAkGBSsOAwIaBQCgXTAY
# BgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0xNjA3MjEy
# MTA2NDBaMCMGCSqGSIb3DQEJBDEWBBSbnbwY9Z+DIe++n7y1wVnznR/pdTANBgkq
# hkiG9w0BAQUFAASCAQCj5nylbbH+1RywxyXwgAe7tP7UWBjF/4dsf0U2sUAbVp6M
# xG+Wh6fRgnJZcZewvbm/tOls++WM/FUKvCdouJwau7q1BJoj6p8tN57t45Pg5OwV
# vq0Hgc4bsTe5okvnxnjmjXnlwvhw8xu6VA1DRUPPqxetSigqEuIB0N9AkdlgSYeO
# VHlr9vdKFltDVMlzqpa1G9KcqsXPUPiEXnc8yy5/QNdirEItmJMEytyPdaBcuStU
# CLj1mdIFUZ+NcPR1Mt6m1ZQodZhT7D88AV2jtAfFqMvO30COyyu5y2uSAffZqKCe
# YDjUjx/VOsy2BMvCT+yBSrc/wWMfNtFeGm0g4EWP
# SIG # End signature block
