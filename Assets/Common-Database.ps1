<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo") | Out-null;

function Import-SqlPsModule()
{
	Push-Location
	try
	{
		# Import the SQL Server Module.
		Import-Module "sqlps" -DisableNameChecking
	}
	finally
	{
		Pop-Location
	}
}

Import-SqlPsModule

function Create-SqlAspNetMembershipDatabase([hashtable]$dbConfig)
{
	$sqlServer = Get-SqlServerInstance $dbConfig;
	$SqlServerInstance = Get-SqlServerInstanceName $dbConfig
	$DatabaseName = $dbConfig.DatabaseName
	
	$database = $SqlServer.Databases[$DatabaseName];
	if ($database.name -eq $DatabaseName)
	{
		Throw "Database '$DatabaseName' exists already."
	};

	Write-Host "Create the ASPNETDB SQL Server database '$DatabaseName' for the membership system on server '$SqlServerInstance'...";

	# we try to find the exe in the V4 folder, however, .NET 4.0 might not be installed, in that case we fall back to the
	# default framework location.
	$frameworkDir = $([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory());
	$frameworkParentDir = Split-Path $frameworkDir -parent;
	$frameworkv4Dir = Join-Path -Path $frameworkParentDir -ChildPath "v4.0.30319";
	$aspnetReqSqlExeV4 = Join-Path $frameworkv4Dir -ChildPath aspnet_regsql.exe
	$aspnetReqSqlExe = Join-Path $frameworkDir -ChildPath aspnet_regsql.exe
	if($aspnetReqSqlExeV4 -ne $null -and (Test-Path $aspnetReqSqlExeV4))
	{
		Set-Alias -Name aspnet_regsql -Value $aspnetReqSqlExeV4 -Force;
	}
	elseif($aspnetReqSqlExe -ne $null -and (Test-Path $aspnetReqSqlExe))
	{
		Set-Alias -Name aspnet_regsql -Value $aspnetReqSqlExe -Force;
	}
	else
	{
		throw "Could not find .NET Framework directory.";
	}
	
	aspnet_regsql -S $SqlServerInstance -E -A all -d $DatabaseName;
}

function CreateLoginsUsersAndRoleMembers([hashtable]$dbConfig)
{
	foreach($sqlLogin in $dbConfig.SqlLogins)
	{
		Add_SqlLogin $dbConfig $sqlLogin.Name $sqlLogin.Password
		$sqlUser = Add_SqlUser $dbConfig $sqlLogin.Name
		if(($sqlUser.UserType -eq "SqlLogin") -and ($sqlUser.IsSystemObject -eq $true) -and ($sqlUser.Name -eq "dbo"))
		{
			Write-Host "The Sql User is the database's owner. Because of this, it cannot be added to any additional roles.";
		}
		else
		{
			Add_SqlRoleMember $dbConfig $sqlLogin.MappedSqlRoleName $sqlLogin.Name
		}
	}
	$sqlLogin = $null;
	
	foreach($windowsLogin in $dbConfig.WindowsLogins)
	{
		[string]$longWindowsGroupName = $null
		if($false -eq (StringIsNullOrWhiteSpace $windowsLogin.GroupName))
		{
			if(($false -eq (StringIsNullOrWhiteSpace $windowsLogin.CreateIfNotExists)) -and (0 -eq [string]::Compare($windowsLogin.CreateIfNotExists, "true", $false)))
			{
				$longWindowsGroupName = CreateWindowsGroup $windowsLogin.GroupName $dbConfig.ServerName
			}
		}
		else
		{
			$longWindowsGroupName = $windowsLogin.UserName
		}
		
		Add_WindowsUser $dbConfig $longWindowsGroupName
		$sqlUser = Add_SqlUser $dbConfig $longWindowsGroupName
		if(($sqlUser.UserType -eq "SqlLogin") -and ($sqlUser.IsSystemObject -eq $true) -and ($sqlUser.Name -eq "dbo"))
		{
			Write-Host "The Sql User is the database's owner. Because of this, it cannot be added to any additional roles.";
		}
		else
		{
			Add_SqlRoleMember $dbConfig $windowsLogin.MappedSqlRoleName $longWindowsGroupName
		}
	}
	$windowsLogin = $null;
}

function Add_SqlLogin([hashtable]$dbConfig, [String]$LoginName, [String]$Password)
{
	$sqlServer = Get-SqlServerInstance $dbConfig;
	$SqlServerInstance = Get-SqlServerInstanceName $dbConfig
	$DatabaseName = $dbConfig.DatabaseName
	$database = Get-SqlDatabase $sqlServer $DatabaseName

	if($true -eq (StringIsNullOrWhiteSpace $LoginName))
	{
		Throw "Add_SqlLogin: LoginName was null or empty.";
	}

	if($true -eq (StringIsNullOrWhiteSpace $Password))
	{
		Throw "Add_SqlLogin: Password was null or empty.";
	}

	$login = $sqlServer.Logins[$LoginName];
	if ($login -eq $null)
	{
		Write-Host "Adding Sql login to instance '$SqlServerInstance', login name '$LoginName', database '$DatabaseName'...";

		$login = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Login -ArgumentList $SqlServerInstance, $LoginName;
		$login.LoginType = 'SqlLogin';
		$login.DefaultDatabase = "master";
		$login.Create($Password);
	}
	else
	{
		$login.ChangePassword($Password);
		Write-Host "login '$LoginName' already exists. Password updated.";
	}
}

function Add_WindowsUser([hashtable]$dbConfig, [String]$LoginName)
{
	$sqlServer = Get-SqlServerInstance $dbConfig;
	$SqlServerInstance = Get-SqlServerInstanceName $dbConfig
	$DatabaseName = $dbConfig.DatabaseName
	$database = Get-SqlDatabase $sqlServer $DatabaseName;

	$LoginName = GetExplicitWindowsUserName $LoginName;
	if($true -eq (StringIsNullOrWhiteSpace $LoginName))
	{
		Throw "Add_WindowsUser: LoginName was null or empty.";
	}

	$login = $sqlServer.Logins[$LoginName];
	if ($login -eq $null)
	{
		Write-Host "Adding windows based login to instance '$SqlServerInstance', login name '$LoginName', database '$DatabaseName'...";

		$login = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Login -ArgumentList $SqlServerInstance, $LoginName;
		$login.LoginType = 'WindowsUser';
		$login.DefaultDatabase = "master";
		$login.Create("password");
	}
	else
	{
		Write-Host "login '$LoginName' already exists. Done.";
	}
}

function Add_SqlUser([hashtable]$dbConfig, [String]$LoginName)
{
	$sqlServer = Get-SqlServerInstance $dbConfig;
	$SqlServerInstance = Get-SqlServerInstanceName $dbConfig
	$DatabaseName = $dbConfig.DatabaseName
	$database = Get-SqlDatabase $sqlServer $DatabaseName;

	$LoginName = GetExplicitWindowsUserName $LoginName
	
	# The account that created the db is automatically the dbo and added as a Sql login already
	# We need to explicitely look at the Login. The Name could be dbo for the main user that created the db, but we do not deal with dbo but with user name
	$sqlUser = $database.Users | ? {$_.Login -eq $LoginName}
	if ($sqlUser -eq $null)
	{
		Write-Host "Adding Sql user '$LoginName' to instance '$SqlServerInstance', database '$DatabaseName'...";
		[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo") | Out-null;
		$sqlUser = New-Object ('Microsoft.SqlServer.Management.Smo.User') ($database, $LoginName);
		$sqlUser.Login = $LoginName;
		$sqlUser.Create();
	}
	else
	{
		Write-Host "Sql user '$LoginName' at instance '$SqlServerInstance', database '$DatabaseName' already exists.";
	}
	
	return $sqlUser
}

function Add_SqlRoleMember([hashtable]$SdbConfig, [string]$RoleName, [string]$SqlLoginName)
{
	$sqlServer = Get-SqlServerInstance $dbConfig;
	$SqlServerInstance = Get-SqlServerInstanceName $dbConfig
	$DatabaseName = $dbConfig.DatabaseName
	$database = Get-SqlDatabase $sqlServer $DatabaseName;

	$SqlLoginName = GetExplicitWindowsUserName $SqlLoginName;

    Write-Host "Adding Sql user '$SqlLoginName' role membership. Role '$RoleName', database '$DatabaseName'...";

	$Exists = 0;
	foreach($role in $database.Roles)
	{              
		if ($role.Name -ilike $RoleName)                  
		{                 
			$Exists = 1 
		}         
	}  

	if($Exists -eq 0)
	{
		Throw "Add_SqlRoleMember: Role $RoleName does not exist in database $DatabaseName!";
	}

	$database.Roles[$RoleName].AddMember($SqlLoginName);
}

function Get-SqlServerInstanceName([hashtable]$dbConfig)
{
	if($true -eq (StringIsNullOrWhiteSpace $dbConfig.InstanceName))
	{
		return $dbConfig.ServerName
	}
	else
	{
		return $dbConfig.InstanceName
	}
}

function Get-SqlServerInstance([hashtable]$dbConfig)
{
    [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.Smo') | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.ConnectionInfo') | Out-Null

    $SqlServerInstance = Get-SqlServerInstanceName $dbConfig

    if (IsSqlServerAuth -dbConfig $dbConfig)
    {
        $conn = New-Object 'Microsoft.SqlServer.Management.Common.ServerConnection'
        $conn.LoginSecure = $false # Set to false for SQL authentication
        $conn.Login = $dbConfig.SqlUserName
        $conn.Password = $dbConfig.SqlUserPassword
        $conn.ServerInstance = $SqlServerInstance
        $sqlServer = New-Object 'Microsoft.SqlServer.Management.Smo.Server' $conn
    }
    else
    {
        $sqlServer = New-Object 'Microsoft.SqlServer.Management.Smo.Server' $SqlServerInstance
    }

	if ($sqlServer.Version -eq  $null )
	{
		Throw "Can't find the instance $SqlServerInstance";
	}
	
	if($verbose -eq $true)
	{
		Write-Host "found server '$SqlServerInstance'";
	}
	return $sqlServer;
}

function Get-SqlDatabase([Microsoft.SqlServer.Management.Smo.Server]$SqlServer, [string]$DatabaseName)
{
	$database = $SqlServer.Databases[$DatabaseName];

	if($verbose -eq $true)
	{
		Write-Host "found database '$DatabaseName'";
	}
	
	return $database;
}

function Create-SqlDatabase([Microsoft.SqlServer.Management.Smo.Server]$SqlServer, [string]$DatabaseName)
{
	$database = $SqlServer.Databases[$DatabaseName];
	if ($database.name -eq $DatabaseName)
	{
		Throw "Database '$DatabaseName' exists already."
	};

    # Backup the database mdf and ldf files if they exist.
    $databaseMdfFileName = "{0}.mdf" -f $DatabaseName
    $databaseMdfBackupFileName = "{0}.mdf.backup" -f $DatabaseName

    if($SqlServer.DefaultFile -and (Test-Path $SqlServer.DefaultFile))
	{
        Move-DatabaseFile -SourceFileName $databaseMdfFileName `
                            -TargetFileName $databaseMdfBackupFileName `
                            -SqlFileDirectory $SqlServer.DefaultFile
	}

    $databaseLdfFileName = "{0}_Log.ldf" -f $DatabaseName
	$databaseLdfBackupFileName = "{0}_Log.ldf.backup" -f $DatabaseName

    if($SqlServer.DefaultLog -and (Test-Path $SqlServer.DefaultLog))
    {
        Move-DatabaseFile -SourceFileName $databaseLdfFileName `
                            -TargetFileName $databaseLdfBackupFileName `
                            -SqlFileDirectory $SqlServer.DefaultLog
    }

	echo "Creating database '$DatabaseName'..."
	$database = new-object Microsoft.SqlServer.Management.Smo.Database ($SqlServer, $DatabaseName)
	$database.Create()

	return $database;
}

function Drop-SqlDatabase([Microsoft.SqlServer.Management.Smo.Server]$SqlServer, [Microsoft.SqlServer.Management.Smo.Database]$database)
{
	Write-Host "Closing all connections to '$Database'..."
	$SqlServer.KillAllProcesses($database.Name)
	Write-Host "Deleting database '$Database'..."
	$database.Drop()
}

function Execute-Sql([string]$sql, [string]$Server, [string]$dbName, [hashtable]$variables, [bool]$WindowsAuthentication=$true, [string]$Username, [string]$Password) 
{
    $tempFile = [IO.Path]::GetTempFileName()
    [IO.File]::WriteAllText($tempFile, $sql)
    Execute-SqlFile -file $tempFile -Server $server -dbName $dbName -variables $variables -WindowsAuthentication $WindowsAuthentication -Username $Username -Password $Password
	Remove-Item $tempFile -Force
}

function Execute-SqlFile($file, [string]$Server, [string]$dbName, [hashtable]$variables, [bool]$WindowsAuthentication=$true, [string]$Username, [string]$Password) 
{
	if((Test-Path -Path $file) -ne $True)
	{
	    throw "File $file was not found!"
	}

	# get file name only. We want to generate the final command and the output file with the filename in it, so we can later come back and troubleshoot errors...
	$fileName = Split-Path -Path $file -Leaf
	$output = (join-Path $env:TEMP ($dbName + "-" + $fileName + ".log"))

	write-Host Connecting to $Server

	$data = @()
	$data += '-S'
    $data += $Server
	
	if ($WindowsAuthentication) {
	    $data += '-E'
	} else {
	    $data += '-U' 
        $data += $Username
        $data += '-P' 
        $data += $Password
	}

	if ($dbname) {
        $data += '-d'
        $data += $dbName
        $data += '-v'
        $data += ('DatabaseName="{0}"' -f $dbName)
	}
	
	if ($variables -and $variables.Count -gt 0) {
	    $data += '-v'
	    foreach($key in $variables.keys) {
             $val = $variables[$key]
             $data += ('{0}="{1}"' -f $key, $val)
	    }
	}

    $data += '-i' 
    $data += $file
	
	$data += '-o'
    $data += $output
	
	# return error level on syntax errors
	$data += '-b'
	
	${global:LASTEXITCODE} = 0

	sqlcmd $data
	
	$capturedExitCode = ${global:LASTEXITCODE}
	# Output to the console before exiting script with error.
	Write-Host (gc "$output")
	
	if($capturedExitCode -ne 0)
	{
	    throw ("SQL script execution failed with return code: $capturedExitCode. Please inspect the log file at `"$output`"")
	}
    Log-TimedMessage ('Finished executing the command')
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
	[string]$connectionString = "Server=" +$databaseConfig.ServerName + ";Database=" + $databaseConfig.DatabaseName + ";"
	
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
		Write-Host "Built connection string: $connectionString"
	}
	
	if($found -eq $false)
	{
		Throw "Build-SqlConnectionString: The connection string could not be generated, the login with id $loginId was not found.";
	}
	
	return $connectionString
}

function Execute-SqlCmd(
    [string] $Server = "localhost",
    [string] $DbName = $(Throw 'DbName parameter required'),
    $SqlCommand = $(Throw 'SqlCommand parameter required'),
    [string] $logFile
)
{
    foreach($cmd in $SqlCommand)
	{
	    [scriptblock]$scriptToExecute = 
		{
            Log-TimedMessage ("Executing SQL command - [{0}]" -f $cmd)		
		    Execute-Sql -server $Server -dbName $DbName -sql $cmd 
		}
	    Invoke-Script -scriptBlock $scriptToExecute -logFile $logFile
	}
}

function Move-DatabaseFile(
    [string]$SourceFileName = $(Throw 'SourceFileName parameter required'),
    [string]$TargetFileName = $(Throw 'TargetFileName parameter required'),
    [string]$SqlFileDirectory = $(Throw 'SqlFileDirectory parameter required')
)
{
    $databaseFilePath = Join-Path $SqlFileDirectory $SourceFileName
    $databaseBackupFilePath = Join-Path $SqlFileDirectory $TargetFileName
    
    if(Test-Path $databaseFilePath)
    {
        Write-Host ("Moving database file {0}." -f $databaseFilePath)
        Move-Item -Path $databaseFilePath -Destination $databaseBackupFilePath -Force
    }
}

function Get-RetailUpgradeScripts(
    [string]$ScriptsFolderPath = $(Throw 'ScriptsFolderPath parameter required'))
{
    [array]$RetailUpgrades = @();
    $scripts = Get-ChildItem -Path $ScriptsFolderPath -Recurse -Filter "DB*.sql"
    foreach($script in $scripts)
    {
        $FileNameTokens = [System.IO.Path]::GetFileNameWithoutExtension($script).Split("_")
        $FromBuild = $FileNameTokens[0] -ireplace "DB", ""
        $ToBuild = $FileNameTokens[1]
        [hashtable]$retailUpgrade = @{}
        $retailUpgrade.ScriptPath = $script.FullName
        $retailUpgrade.FromBuild = [version]$FromBuild
        $retailUpgrade.ToBuild = [version]$ToBuild
        $retailUpgrade.FileName = $script.Name
        $RetailUpgrades += $retailUpgrade   
    }

    $RetailUpgradesSorted = $RetailUpgrades | Sort-Object {$_.FromBuild}
    return $RetailUpgradesSorted
}

function Get-CustomUpgradeScripts(
    [string]$ScriptsFolderPath = $(Throw 'ScriptsFolderPath parameter required'))
{
    [array]$CustomUpgrades = @()
    $scripts = Get-ChildItem -Path $ScriptsFolderPath -Recurse -Filter "*.sql"
    foreach($script in $scripts)
    {
        [hashtable]$customUpgrade = @{}
        $customUpgrade.ScriptPath = $script.FullName
        $customUpgrade.FileName = $script.Name
        $CustomUpgrades += $customUpgrade   
    }

    $CustomUpgradesSorted = $CustomUpgrades | Sort-Object {$_.FileName}
    return $CustomUpgradesSorted
}

function Get-DatabaseVersionNumber(
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'), 
    [string]$UserName, 
    [string]$Password)
{
    $getVersionQuery = "SELECT TOP 1 VERSIONSTRING FROM [ax].[DBVERSION] WHERE VERSIONTYPE='databaseVersion'"
    $result = Execute-SqlQuery -Query $getVersionQuery `
                               -SqlServerInstanceName $SqlServerInstanceName `
                               -DatabaseName $DatabaseName `
                               -UserName $UserName `
                               -Password $Password

    return [version]$result.VERSIONSTRING  
}

function Set-DatabaseVersionNumber(
    $DbVersion = $(Throw 'dbVersion parameter required'), 
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'), 
    [string]$UserName, 
    [string]$Password)
{
    $thisDbVersionString = $DbVersion.ToString()
    $thisDBVersionType = "databaseVersion"

    # This variable is to keep build number in integer form and is ported over AS-IS. This value is not used anywhere.
    $thisDbVersion = [math]::pow(10, 7) * $DbVersion.Major + [math]::Pow(10, 5) * $DbVersion.Minor + [math]::Pow(10, 2) * $DbVersion.Build + $DbVersion.Revision
    $setVersionQuery = "IF EXISTS(SELECT TOP 1 VersionNumber FROM [ax].[DBVersion] WHERE VersionType = '$thisDBVersionType')  
              UPDATE [ax].[DBVersion] SET VersionNumber = $thisDbVersion, VersionString = '$thisDbVersionString'
              ELSE 
              INSERT [ax].[DBVersion] (VersionType, VersionNumber, VersionString) VALUES ('$thisDBVersionType', $thisDbVersion, '$thisDbVersionString')"

    $null = Execute-SqlQuery -Query $setVersionQuery `
                             -SqlServerInstanceName $SqlServerInstanceName `
                             -DatabaseName $DatabaseName `
                             -UserName $UserName `
                             -Password $Password

    return $DbVersion
}

function Apply-RetailUpgradeScripts(
    $RetailUpgrades = $(Throw 'RetailUpgrades parameter is required'),
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'),
    [string]$UserName, 
    [string]$Password
    )
{    
    $message = ("Getting version number on database ""{0}""" -f $DatabaseName)
	Log-TimedMessage $message

    $dbVersion = Get-DatabaseVersionNumber -SqlServerInstanceName $SqlServerInstanceName `
                                           -DatabaseName $DatabaseName `
                                           -UserName $UserName `
                                           -Password $Password

    $message = ("Database ""{0}"" version number is {1} before upgrade." -f $DatabaseName, $dbVersion)
	Log-TimedMessage $message
        
    $buildUpgradeApplicable = $false                        
    $lastToBuildNumber
    $appliedRetailUpgrades = Get-AlreadyAppliedUpgradeScriptFileNames -UpgradeType 'RETAIL' `
                                                                      -SqlServerInstanceName $SqlServerInstanceName `
                                                                      -DatabaseName $DatabaseName `
                                                                      -UserName $UserName `
                                                                      -Password $Password
                        
    foreach($retailUpgrade in $RetailUpgrades)
	{
        if ([version]$retailUpgrade.FromBuild -ge $dbVersion)
        {
            $message = ("Database version number is {0}." -f $dbVersion.ToString())
            Log-TimedMessage $message
			
            $result = Execute-UpgradeScript -Upgrade $retailUpgrade `
                                            -AppliedUpgrade $appliedRetailUpgrades `
                                            -UpgradeType 'RETAIL' `
                                            -SqlServerInstanceName $SqlServerInstanceName `
                                            -DatabaseName $DatabaseName `
                                            -UserName $UserName `
                                            -Password $Password

            $lastToBuildNumber = $retailUpgrade.ToBuild

            $message = ("Setting version number {0} on database ""{1}""." -f $lastToBuildNumber, $DatabaseName)
		    Log-TimedMessage $message   

            $dbVersion = Set-DatabaseVersionNumber -DbVersion $lastToBuildNumber  `
                                                   -SqlServerInstanceName $SqlServerInstanceName `
                                                   -DatabaseName $DatabaseName `
                                                   -UserName $UserName `
                                                   -Password $Password
        }            
    }
}

function Apply-CustomUpgradeScripts(
    $CustomUpgrades = $(Throw 'CustomUpgrades parameter is required'),
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'),
    [string]$UserName, 
    [string]$Password
    )
{ 
    $appliedCustomUpgrades = Get-AlreadyAppliedUpgradeScriptFileNames -UpgradeType 'CUSTOM' `
                                                                      -SqlServerInstanceName $SqlServerInstanceName `
                                                                      -DatabaseName $DatabaseName `
                                                                      -UserName $UserName `
                                                                      -Password $Password
                           
    foreach($customUpgrade in $CustomUpgrades)
	{
        Execute-UpgradeScript -Upgrade $customUpgrade `
                              -AppliedUpgrade $appliedCustomUpgrades `
                              -UpgradeType 'CUSTOM' `
                              -SqlServerInstanceName $SqlServerInstanceName `
                              -DatabaseName $DatabaseName `
                              -UserName $UserName `
                              -Password $Password
    }
}

function Execute-UpgradeScript(
    $Upgrade = $(Throw 'Upgrade parameter is required'),    
    [ValidateSet('CUSTOM', 'RETAIL')][string]$UpgradeType = $(Throw 'UpgradeType parameter is required'),
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'),
    $AppliedUpgrades,
    [string]$UserName, 
    [string]$Password
)
{
    if((Check-UpgradeApplied -UpgradeFileName $Upgrade.FileName -AppliedUpgrades $AppliedUpgrades ) -eq $false)
    {
        $message = ("Applying {0} upgrade script {1} on database ""{2}""." -f $UpgradeType, $Upgrade.ScriptPath, $DatabaseName)
		Log-TimedMessage $message    
        Execute-SqlScript -SqlScript $Upgrade.ScriptPath `
                          -SqlServerInstanceName $SqlServerInstanceName `
                          -DatabaseName $DatabaseName `
                          -UserName $UserName `
                          -Password $Password

        Create-UpgradeRecord -Upgrade $Upgrade `
                             -UpgradeType $UpgradeType `
                             -SqlServerInstanceName $SqlServerInstanceName `
                             -DatabaseName $DatabaseName `
                             -UserName $UserName `
                             -Password $Password	            

        Log-TimedMessage "Applied"
    }
}

function Check-UpgradeApplied(
    [string]$UpgradeFileName = $(Throw 'UpgradeFileName parameter required').
    $AppliedUpgrades)    
{
    [bool]$upgradeApplied = $false
    if ($AppliedUpgrades)
    {
        foreach($upgradeRecord in $AppliedUpgrades)
        {
            if($UpgradeFileName.CompareTo($upgradeRecord.FileName) -eq 0)
            {
                $upgradeApplied = $true
                break
            }
        }
    }

    return $upgradeApplied
}

function Create-UpgradeRecord(
    $Upgrade = $(Throw 'Upgrade parameter required'),
    [string]$UpgradeType = $(Throw 'UpgradeType parameter required'), 
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'),
    [string]$UserName, 
    [string]$Password
)
{
    $scriptPath = $Upgrade.ScriptPath
    $fileName = $Upgrade.FileName
    $build = $Upgrade.ToBuild
    $insertUpgradeQuery = "INSERT [crt].[RETAILUPGRADEHISTORY] (UpgradeType, FilePath, FileName, Build) VALUES ('$UpgradeType', '$scriptPath', '$fileName', '$build')"
    Execute-SqlQuery -Query $insertUpgradeQuery `
                     -SqlServerInstanceName $SqlServerInstanceName `
                     -DatabaseName $DatabaseName `
                     -UserName $UserName `
                     -Password $Password
}

function Get-AlreadyAppliedUpgradeScriptFileNames(
    [string]$UpgradeType = $(Throw 'UpgradeType parameter required'), 
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'),
    [string]$UserName, 
    [string]$Password)
{
    $getUpgradesQuery = "SELECT FILENAME FROM [crt].[RETAILUPGRADEHISTORY] WHERE UPGRADETYPE='$UpgradeType'"
    Execute-SqlQuery -Query $getUpgradesQuery `
                     -SqlServerInstanceName $SqlServerInstanceName `
                     -DatabaseName $DatabaseName `
                     -UserName $UserName `
                     -Password $Password
    return $result 
}

function Execute-SqlQuery(
    [string]$Query = $(Throw 'Query parameter required'), 
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'), 
    [string]$UserName, 
    [string]$Password)
{
    try
    {
        # This function is used to execute sql query in Invoke-SqlCmd to enable transaction scope.
        if($UserName -and $Password)
        {
            $result = Invoke-SqlCmd -Query $Query `
                                    -ServerInstance $SqlServerInstanceName `
                                    -Database $DatabaseName `
                                    -Username $UserName `
                                    -Password $Password `
                                    -ErrorAction Stop       
        }
        else
        {
            $result = Invoke-SqlCmd -Query:$Query `
                                    -ServerInstance:$SqlServerInstanceName `
                                    -Database:$DatabaseName `
                                    -ErrorAction Stop   
        }
                               
        return $result
    }
    catch
    {
        Log-TimedMessage ("Error occurred while executing SQL query '{0}' on server {1} and database {2}. Please validate this query." -f $Query, $SqlServerInstanceName, $DatabaseName)
        throw
    } 
}

function Check-IfLoginExistsForDatabase(
    [string]$targetUser = $(Throw 'targetUser parameter required'),
    [string]$targetDatabase = $(Throw 'targetDatabase parameter required'),
    [string]$sqlServerInstanceName = $(Throw 'sqlServerInstanceName parameter required'),
    [string]$dbAccessUser = $(Throw 'dbAccessUser parameter required'), 
    [string]$dbAccessUserPassword = $(Throw 'dbAccessUserPassword parameter required')
)
{
    $query = "SELECT COUNT(*) AS USERSFOUND FROM SYS.DATABASE_PRINCIPALS WHERE NAME = '{0}'" -f $targetUser
    $queryResult = Execute-SqlQuery -SqlServerInstanceName $sqlServerInstanceName `
                                    -DatabaseName $targetDatabase `
                                    -UserName $dbAccessUser `
                                    -Password $dbAccessUserPassword `
                                    -Query $query

    $result = ($queryResult.USERSFOUND -ne 0)
    return $result
}

function Create-DatabaseUser(
    [string]$targetUser = $(Throw 'targetUser parameter required'),
    [string]$targetDatabase = $(Throw 'targetDatabase parameter required'),
    [string]$sqlServerInstanceName = $(Throw 'sqlServerInstanceName parameter required'),
    [string]$dbAccessUser = $(Throw 'dbAccessUser parameter required'), 
    [string]$dbAccessUserPassword = $(Throw 'dbAccessUserPassword parameter required')
)
{
    $doesLoginExistForTargetDb = Check-IfLoginExistsForDatabase -targetUser $targetUser `
                                                                -targetDatabase $targetDatabase `
                                                                -sqlServerInstanceName $sqlServerInstanceName `
                                                                -dbAccessUser $dbAccessUser `
                                                                -dbAccessUserPassword $dbAccessUserPassword
    if (-not($doesLoginExistForTargetDb))
    {
        $query = 'CREATE USER [{0}] FOR LOGIN [{0}] WITH DEFAULT_SCHEMA=[dbo]' -f $targetUser
        Execute-SqlQuery -SqlServerInstanceName $sqlServerInstanceName -DatabaseName $targetDatabase -UserName $dbAccessUser -Password $dbAccessUserPassword -Query $query
        
        Log-TimedMessage ('Created database user {0}.' -f $targetUser)
    }
}

function Copy-DatabaseUserRoles(
    [string]$copyRolesFromUser = $(Throw 'copyRolesFromUser parameter required'),
    [string]$copyRolesToUser = $(Throw 'copyRolesToUser parameter required'),
    [string]$sqlServerInstanceName = $(Throw 'sqlServerInstanceName parameter required'),
    [string]$databaseName = $(Throw 'databaseName parameter required'), 
    [string]$dbAccessUser = $(Throw 'dbAccessUser parameter required'), 
    [string]$dbAccessUserPassword = $(Throw 'dbAccessUserPassword parameter required')
)
{
    # Get all the roles associated to the 'copyRolesToUser' user.
    $getSourceUserRolesQuery = "SELECT USER_NAME(rm.role_principal_id) as role FROM sys.database_role_members AS rm WHERE USER_NAME(rm.member_principal_id) = '{0}'" -f $copyRolesFromUser
    $sourceUserRoles = Execute-SqlQuery -SqlServerInstanceName $sqlServerInstanceName -DatabaseName $databaseName -UserName $dbAccessUser -Password $dbAccessUserPassword -Query $getSourceUserRolesQuery
    Log-TimedMessage ('Recorded all roles for user {0}.' -f $copyRolesFromUser)

    Create-DatabaseUser -targetUser $copyRolesToUser `
                        -targetDatabase $databaseName `
                        -sqlServerInstanceName $sqlServerInstanceName `
                        -dbAccessUser $dbAccessUser `
                        -dbAccessUserPassword $dbAccessUserPassword

    # Add the target roles to the 'copyRolesToUser' user.
    foreach ($targetRole in $sourceUserRoles)
    {
        $role = $targetRole.role
        $queryToAddUserToRole = "IF  EXISTS (SELECT * FROM dbo.sysusers WHERE name = '$role' AND issqlrole = 1) BEGIN EXEC sp_addrolemember '$role', '$copyRolesToUser' END"

        Execute-SqlQuery -SqlServerInstanceName $sqlServerInstanceName -DatabaseName $databaseName -UserName $dbAccessUser -Password $dbAccessUserPassword -Query $queryToAddUserToRole
        Log-TimedMessage ('Copied role {0} from user {1} to user {2}.' -f $role, $copyRolesFromUser, $copyRolesToUser)
    }
}

function Execute-SqlScript(
    [string]$SqlScript = $(Throw 'SqlScript parameter required'), 
    [string]$SqlServerInstanceName = $(Throw 'SqlServerInstanceName parameter required'), 
    [string]$DatabaseName = $(Throw 'DatabaseName parameter required'), 
    [string]$UserName, 
    [string]$Password,
    [bool]$DeployData=$false)
{
    try
    {
        # This function is used to execute sql script in Invoke-SqlCmd to enable transaction scope.
        $databaseNameVar = ("DatabaseName={0}" -f $DatabaseName)
        $deployDataFlag = ("DeployData={0}" -f $DeployData)
        $variables = @($databaseNameVar, $deployDataFlag)

        if($UserName -and $Password)
        {
            $result = Invoke-SqlCmd -InputFile $SqlScript `
                                    -ServerInstance $SqlServerInstanceName `
                                    -Database $DatabaseName `
                                    -Username $UserName `
                                    -Password $Password `
                                    -Variable $variables `
                                    -ErrorAction Stop   
        
        }
        else
        {
           $result = Invoke-SqlCmd -InputFile $SqlScript `
                                   -ServerInstance $SqlServerInstanceName `
                                   -Database $DatabaseName `
                                   -Variable $variables `
                                   -ErrorAction Stop   
        }
                                
        return $result 
    }
    catch
    {
        Log-Exception ("Error occurred while executing SQL script '{0}' on server {1} and database {2}. Please validate this script." -f $SqlScript, $SqlServerInstanceName, $DatabaseName)
        throw $_
    }
}

function IsSqlServerAuth(
    [hashtable]$dbConfig
)
{
    return ($dbConfig.SqlUserName -and $dbConfig.SqlUserPassWord)
}

# Function to configure memory for Microsoft SQL Server installation 
function Configure-MaxSqlMemoryLimit(
	[string]$sqlServerInstanceName = $(throw 'sqlServerInstanceName is required'),
	[double]$maxSqlServerMemoryLimitRatio = $(throw 'maxSqlServerMemoryLimitRatio is required'))
{
	#Get the amount of memory installed on Computer
	$winOS = Get-WMIObject Win32_OperatingSystem
	[int] $computerMemoryInMB = $winOS.TotalVisibleMemorySize / 1024
	$SqlMinMemory = 128

	# Set the value of $maxMemInMB
	[int] $maxMemInMB = [int]($computerMemoryInMB * $maxSqlServerMemoryLimitRatio)

	$isMaxSqlMemoryLimitSet = Test-IfSqlMaxMemoryLimitSet -sqlServerInstanceName $sqlServerInstanceName
	$isMaxSqlServerMemoryLimitRatioValid = ($maxSqlServerMemoryLimitRatio -ge 0.2) -and ($maxSqlServerMemoryLimitRatio -le 1)

	if(!$isMaxSqlMemoryLimitSet -and $ismaxSqlServerMemoryLimitRatioValid -and ($maxMemInMB -ge $SqlMinMemory))
	{
		Log-TimedMessage ('The total memory on computer (in MB): {0}' -f $ComputerMemoryInMB)
		Set-MaxSqlMemoryLimit -sqlServerInstanceName $sqlServerInstanceName -maxMemInMB $maxMemInMB
	}
}

# Function to check if max memory for Microsoft SQL Server installation has been set
function Test-IfSqlMaxMemoryLimitSet(
	[string]$sqlServerInstanceName = $(throw 'sqlServerInstanceName is required'))
{
	$SqlDefaultMaxMemory = 2147483647

	$command = "exec sp_configure 'max server memory';"
	$outvar = Invoke-SqlCmdWithAdvancedOptionsPrereq -sqlServerInstanceName $sqlServerInstanceName -command $command
	$isSqlMaxMemoryLimitSet = $outvar.config_value -ne $SqlDefaultMaxMemory

	return $isSqlMaxMemoryLimitSet
}

# Function to set memory for Microsoft SQL Server installation 
function Set-MaxSqlMemoryLimit(
	[string]$sqlServerInstanceName = $(throw 'sqlServerInstanceName is required'),
	[int]$maxMemInMB = $(throw 'maxMemInMB is required'))
{
	$command = "exec sp_configure 'max server memory', {0};
				RECONFIGURE;" -f $maxMemInMB

	Log-TimedMessage ('Setting Max SQL Server memory limit (in MB) to: {0}' -f $maxMemInMB)
	
	Invoke-SqlCmdWithAdvancedOptionsPrereq -sqlServerInstanceName $sqlServerInstanceName -command $command
}

function Invoke-SqlCmdWithAdvancedOptionsPrereq(
	[string]$sqlServerInstanceName = $(throw 'sqlServerInstanceName is required'),
	[string]$command = $(throw 'command is required'))
{
	# get existing advanced options.
	$originalConfig = Invoke-Sqlcmd -Query "exec sp_configure 'show advanced options';" -ServerInstance $sqlServerInstanceName
	Log-TimedMessage ('The existing advanced options value for SQL is : {0}' -f $originalConfig.config_value)

	try
	{
		# if not 1, set to 1
		if($originalConfig.config_value -ne 1)
		{
			[void] (Invoke-Sqlcmd -Query "exec sp_configure 'show advanced options', 1; RECONFIGURE;" -ServerInstance $sqlServerInstanceName)
			Log-TimedMessage ('The advanced options value options for SQL was set to : 1')
		}
		# $result = invoke query
		$result = Invoke-Sqlcmd -Query $command -ServerInstance $sqlServerInstanceName
	}
	finally
	{
		# if it was not 1 set it back
		if($originalConfig.config_value -ne 1)
		{
			$restoreCommand = "exec sp_configure 'show advanced options', {0}; RECONFIGURE;" -f $originalConfig.config_value
			[void] (Invoke-Sqlcmd -Query $restoreCommand -ServerInstance $sqlServerInstanceName)
			Log-TimedMessage ('The advanced options value options for SQL was reset to : {0}' -f $originalConfig.config_value)
		}
	}

	return $result
}

function Get-DatabaseFiles(
    [ValidateNotNull()]
    [Microsoft.SqlServer.Management.Smo.Database]$database = $(throw 'database parameter is required'))
{
    [array]$result = @()
    foreach ($fileGroup in $database.FileGroups)
    {
        $result += $fileGroup.Files
    }
    
    $result += $database.LogFiles
    return $result
}

function Configure-DatabaseFilesMinSize(
    [ValidateNotNull()]
    [Microsoft.SqlServer.Management.Smo.Database]$database = $(throw 'database parameter is required'),
    
    [ValidateNotNull()]
    [ValidateScript( { $_ -gt 0 } )]
    [int]$minSizeInMB = $(throw 'minSizeInMB parameter is required'))
{
    Log-TimedMessage ('Configuring minimum file size for database "{0}" to be {1} MB' -f $database.Name, $minSizeInMB)

    $minSizeInKB = $minSizeInMB * 1024
    $databaseFiles = Get-DatabaseFiles -database $database
    foreach($file in $databaseFiles)
    {
        if($file.Size -lt $minSizeInKB)
        {
            Set-DatabaseFileSize -file $file -sizeInKB $minSizeInKB
        }
    }
    
    Log-TimedMessage ('Completed configuring minimum file size for database "{0}"' -f $database.Name)
}

function Set-DatabaseFileSize(
    [ValidateNotNull()]
    [Microsoft.SqlServer.Management.Smo.DatabaseFile]$file = $(throw 'file parameter is required'),
    
    [ValidateNotNull()]
    [ValidateScript( { $_ -gt 0 } )]
    [int]$sizeInKB = $(throw 'sizeInKB parameter is required'))
{
    Log-TimedMessage ('Setting file "{0}" to have size of {1} KB' -f $file.FileName, $sizeInKB)
    $file.Size = $sizeInKB
    $file.Alter()
    Log-TimedMessage ('Size set for file "{0}" has completed successfully' -f $file.FileName)
}

function Configure-DatabaseFilesGrowth(
    [ValidateNotNull()]
    [Microsoft.SqlServer.Management.Smo.Database]$database = $(throw 'database parameter is required'),
    
    # https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.filegrowthtype.aspx
    [ValidateNotNull()]
    [ValidateSet('KB', 'Percent')]
    $growthType = $(throw 'growthType parameter is required'),

    [ValidateNotNull()]
    [ValidateScript( { $_ -gt 0 } )]
    [int]$growthRate = $(throw 'growthRate parameter is required'))
{
    Log-TimedMessage ('Configuring file growth for database "{0}" to be {1} {2}' -f $database.Name, $growthRate, $growthType)

    $databaseFiles = Get-DatabaseFiles -database $database
    foreach($file in $databaseFiles)
    {
        Set-DatabaseFileGrowth -file $file -growthType $growthType -growthRate $growthRate
    }
    
    Log-TimedMessage ('Completed configuring file growth for database "{0}"' -f $database.Name)
}

function Set-DatabaseFileGrowth(
    [ValidateNotNull()]
    [Microsoft.SqlServer.Management.Smo.DatabaseFile]$file = $(throw 'file parameter is required'),
    
    # https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.filegrowthtype.aspx
    [ValidateNotNull()]
    [ValidateSet('KB', 'Percent')]
    $growthType = $(throw 'growthType parameter is required'),

    [ValidateNotNull()]
    [ValidateScript( { $_ -gt 0 } )]
    [int]$growthRate = $(throw 'growthRate parameter is required'))
{
    Log-TimedMessage ('Setting file "{0}" to have growth of {1} {2}' -f $file.FileName, $growthRate, $growthType)
    $file.GrowthType = $growthType
    $file.Growth = $growthRate
    $file.Alter()
    Log-TimedMessage ('Growth set for file "{0}" has completed successfully' -f $file.FileName)
}

function Set-DatabaseAutoClose(
    [ValidateNotNull()]
    [Microsoft.SqlServer.Management.Smo.Database]$database = $(throw 'database parameter is required'),

    [ValidateNotNull()]
    [bool]$autoCloseValue = $(throw 'autoCloseValue parameter is required'))
{
    Log-TimedMessage ('Setting database "{0}" AutoClose to {1}' -f $database.Name, $autoCloseValue)
    $database.AutoClose = $autoCloseValue
    $database.Alter()
    Log-TimedMessage ('AutoClose set for database "{0}" has completed successfully' -f $database.Name)
}
# SIG # Begin signature block
# MIIdsAYJKoZIhvcNAQcCoIIdoTCCHZ0CAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU81db/HawEMe6KghJH8Y+nODS
# n0+gghhkMIIEwzCCA6ugAwIBAgITMwAAAJvgdDfLPU2NLgAAAAAAmzANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTYwMzMwMTkyMTI5
# WhcNMTcwNjMwMTkyMTI5WjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OjcyOEQtQzQ1Ri1GOUVCMSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjaPiz4GL18u/
# A6Jg9jtt4tQYsDcF1Y02nA5zzk1/ohCyfEN7LBhXvKynpoZ9eaG13jJm+Y78IM2r
# c3fPd51vYJxrePPFram9W0wrVapSgEFDQWaZpfAwaIa6DyFyH8N1P5J2wQDXmSyo
# WT/BYpFtCfbO0yK6LQCfZstT0cpWOlhMIbKFo5hljMeJSkVYe6tTQJ+MarIFxf4e
# 4v8Koaii28shjXyVMN4xF4oN6V/MQnDKpBUUboQPwsL9bAJMk7FMts627OK1zZoa
# EPVI5VcQd+qB3V+EQjJwRMnKvLD790g52GB1Sa2zv2h0LpQOHL7BcHJ0EA7M22tQ
# HzHqNPpsPQIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFJaVsZ4TU7pYIUY04nzHOUps
# IPB3MB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBACEds1PpO0aBofoqE+NaICS6dqU7tnfIkXIE1ur+0psiL5MI
# orBu7wKluVZe/WX2jRJ96ifeP6C4LjMy15ZaP8N0OckPqba62v4QaM+I/Y8g3rKx
# 1l0okye3wgekRyVlu1LVcU0paegLUMeMlZagXqw3OQLVXvNUKHlx2xfDQ/zNaiv5
# DzlARHwsaMjSgeiZIqsgVubk7ySGm2ZWTjvi7rhk9+WfynUK7nyWn1nhrKC31mm9
# QibS9aWHUgHsKX77BbTm2Jd8E4BxNV+TJufkX3SVcXwDjbUfdfWitmE97sRsiV5k
# BH8pS2zUSOpKSkzngm61Or9XJhHIeIDVgM0Ou2QwggYHMIID76ADAgECAgphFmg0
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
# bWrJUnMTDXpQzTGCBLYwggSyAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCByjAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQU4wQb43jv4dLPvdKW8KW+bjkByh4wagYKKwYB
# BAGCNwIBDDFcMFqgKIAmAEMAbwBtAG0AbwBuAC0ARABhAHQAYQBiAGEAcwBlAC4A
# cABzADGhLoAsaHR0cDovL3d3dy5NaWNyb3NvZnQuY29tL01pY3Jvc29mdER5bmFt
# aWNzLyAwDQYJKoZIhvcNAQEBBQAEggEAKCXGOGI2rw2p+RZezi4xx2REASVc1ahB
# bZMoH6dtyRzlvUK3zQVVAywkz8YwoBy3d1uRW8XcN+38B3cCAllLhS/S9G5RorQ2
# bwc9tPvzEySDIOZEXDxGLd+WOwAZzv0LTzM1BEfGRFBFMofINqe+FoH/1+Jw4k4N
# VXU8ZVjLBpd71yY23El15X9lZTstXS7bMHYJO57thcbOnS8AKQA5h64KgHXr40vO
# FCDTHd5hIVDVW7INJ8WtfQrOiU7PVh53p4+JJqEAvHNCo7QglDOqJKvA7qgFKngu
# rmFY3Pg1fEHcyd+8CB2QiGLyzawpiaCAH2k56aNpCWStvhLIJDfSt6GCAigwggIk
# BgkqhkiG9w0BCQYxggIVMIICEQIBATCBjjB3MQswCQYDVQQGEwJVUzETMBEGA1UE
# CBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9z
# b2Z0IENvcnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQ
# Q0ECEzMAAACb4HQ3yz1NjS4AAAAAAJswCQYFKw4DAhoFAKBdMBgGCSqGSIb3DQEJ
# AzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE2MDcyMTIxMDY1N1owIwYJ
# KoZIhvcNAQkEMRYEFAzcj6Ql56CHvvm9UaINllkLJU2iMA0GCSqGSIb3DQEBBQUA
# BIIBACQ+91IdaMfcTaoYRDJqC9NgSK/2gIkr2RCnPvrxOEkYpWjfQdb6NxwCoJWD
# KhBM5vC8zEnnXWn+c9ok5NBEzJQHPVkxZmILV40GRTUoQUyoqIyOMcTRNqPLpILO
# wXaZwLLvnPSat1axGePkRIWsTo0aA9kcEh0gKXlG0nShquhscqc7JNSE6WC1rXUZ
# BHbRSowgxPiUDf98FaqM+E8s3XVLMNeWu3eh/hu4BOaXZaRUWlV0gn/Wk7pvrQfS
# IjrRHzSFVEFCpZLdcpz5JV23Gea/AJRifnI1b1rvSLvWXBLv/xFAaQyVzaYhvQu9
# GUgxSXohcTxgvfuJ9yHViUVQe80=
# SIG # End signature block
