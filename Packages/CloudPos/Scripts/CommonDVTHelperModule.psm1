# Common DVT Helper Functions Module

# Log function
function Log-TimedMessage(
    [string]$logMessage,
	[string]$log)
{
	$timeStamp = Get-TimeStamp
	$logMessage = '[{0}] {1}' -f $timeStamp, $logMessage
	Write-Host $logMessage
	if($log)
	{
		$logMessage | Out-File -FilePath $log -Append -Force
	}
}

function Log-TimedError (
	[string]$message,
	[string]$log)
{
    Log-TimedMessage -logMessage '############### Error occurred: ###############' -log $log
    Log-TimedMessage -logMessage $message -log $log
}

function Log-Parameter(
	[string]$parameterName, 
	[string]$parameterValue, 
	[string]$log)
{
	$message = 'Parameter Name: {0} Parameter Value: {1}' -f $parameterName, $parameterValue
	Log-TimedMessage -logMessage $message -log $log
}

function Log-Exception(
	$exception,
	[string]$log)
{
    $message = ($exception | fl * -Force | Out-String -Width 4096)
    # If passed object is a string, just log the string.
    if($exception -is [string])
    {
        $message = $exception
    }
    Log-TimedError -message $message -log $log
}

# Timestamp for logging
function Get-TimeStamp()
{
    [string]$timeStamp = [System.DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss")
    return $timeStamp
}

# Handle DVt Error
function Handle-DvtError(
	$errorObject,
	[string]$log)
{
	Log-Exception -exception $errorObject -log $log
    Throw $errorObject
}

# Convert the raw base64 to a useful string
function Convert-Base64ToString(
    [string]$base64String)
{
	Log-TimedMessage -logMessage 'Converting Base64 string to string' -log $log
    [string]$rawString = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($base64String))
    
    return $rawString
}

# Retrieve values from XML based on XPath
function Get-ServiceModelParameterValue (
   [xml]$serviceModelXml,
   [string]$xPath,
   [string]$paramName)
{
    return (Select-Xml $serviceModelXml -XPath $xPath | Where { $_.Node.Name -eq $paramName }).Node.Value
}

# Make sure the Directory path exists
function New-DirectoryIfNotExists(
	[ValidateNotNullOrEmpty()]
	[string]$dirPath)
{
	if(-not (Test-Path -Path $dirPath))
	{
		New-Item -Path $dirPath -Type Directory -Force | Out-Null
	}
}

function Get-PathLeafNoFileExtenstion (
	[string]$path)
{
	return ([IO.Path]::GetFileNameWithoutExtension($path))
}

# Create DVT local directory
function CreateDirectoryAndCopy-ScriptsToDVTLocalDir(
    [string]$dvtScript,
    [array]$dvtHelperScripts,
    [string]$log)
{
	if(!$env:SERVICEDRIVE)
	{
		$env:SERVICEDRIVE = $env:TEMP
	}

	# Create DVT local directory
	$dvtLocalBinChildPath = Get-PathLeafNoFileExtenstion -path $dvtScript
	$dvtLocalBin = (Join-Path -Path $env:SERVICEDRIVE -ChildPath "DynamicsDiagnostics\$dvtLocalBinChildPath\Input")
	New-DirectoryIfNotExists -DirPath $dvtLocalBin

	# Copy DVT scripts to local directory
	$logMessage = 'Copy DVT Script {0} to local DVT bin {1}' -f $dvtScript, $dvtLocalBin
	Log-TimedMessage -logMessage $logMessage -log $log
	Copy-Item -Path $dvtScript -Destination $dvtLocalBin -Recurse -Force | Out-Null
	
	# Copy DVT helper scripts to local directory
	foreach ($dvtHelperScript in $dvtHelperScripts)
	{
		$logMessage = 'Copy DVT helper Scripts {0} to local DVT bin {1}' -f $dvtHelperScript, $dvtLocalBin
		Log-TimedMessage -logMessage $logMessage -log $log
		Copy-Item -Path $dvtHelperScript -Destination $dvtLocalBin -Recurse -Force | Out-Null
	}

	return $dvtLocalBin
}

# Add column values to the rows for the XML template
function Add-ColumnToDvtTestResultsXml(
    [string]$columnName,
    [xml]$dvtOutputXmlTemplate,
    [System.Xml.XmlLinkedNode]$row,
    [string]$log)
{
	$column = $dvtOutputXmlTemplate.CreateElement('string')
    $column.InnerText = $columnName
    [void]$row.AppendChild($column)
	Log-TimedMessage -logMessage ('Adding column value: {0}' -f $columnName) -log $log | Out-Null
}

# Append XML Rows to Template
function Append-RowToTestResultsXml(
    [string]$testName,
    [string]$testType,
    [string]$testResult,
    [string]$rawResult,
    [string]$timeStamp,
    [xml]$dvtOutputXmlTemplate,
    [string]$log)
{
	Log-TimedMessage -logMessage 'Getting existing rows from XML Template' -log $log
    $rows = $dvtOutputXmlTemplate.SelectSingleNode('CollectionResult/TabularResults/TabularData/Rows')
	Log-TimedMessage -logMessage 'Creating new row' -log $log
    $row = $dvtOutputXmlTemplate.CreateElement('ArrayOfStrings')
		
    Add-ColumnToDvtTestResultsXml -columnName $testName -dvtOutputXmlTemplate $dvtOutputXmlTemplate -row $row -log $log
	Add-ColumnToDvtTestResultsXml -columnName $testType -dvtOutputXmlTemplate $dvtOutputXmlTemplate -row $row -log $log
	Add-ColumnToDvtTestResultsXml -columnName $testResult -dvtOutputXmlTemplate $dvtOutputXmlTemplate -row $row -log $log
	Add-ColumnToDvtTestResultsXml -columnName $rawResult -dvtOutputXmlTemplate $dvtOutputXmlTemplate -row $row -log $log
	Add-ColumnToDvtTestResultsXml -columnName $timeStamp -dvtOutputXmlTemplate $dvtOutputXmlTemplate -row $row -log $log

    $rows.AppendChild($row)
    $dvtOutputXmlTemplate.CollectionResult.TabularResults.TabularData.AppendChild($rows)
    $dvtOutputXmlTemplate.Save($dvtOutputXmlTemplate)
	Log-TimedMessage -logMessage 'Saved rows to XML Template' -log $log
}

# Validation Helper Functions
# Validate appPool is started
function Validate-AppPool(
	[string]$appPoolName,
	[ValidateSet("Started","Stopped")]
	[string]$expectedState,
	[string]$logFile)
{
    [bool]$IsAppPoolInExpectedState = $false

    try
	{
		Log-TimedMessage -logMessage ('Validating AppPool: {0} is {1}' -f $appPoolName, $expectedState) -log $logFile
        Import-Module WebAdministration
        $thisAppPool = Get-WebAppPoolState -Name $appPoolName
        $rawResult = ('AppPoolName: {0}; Status: {1}' -f $thisAppPool.ItemXPath, $thisAppPool.Value)
        $IsAppPoolInExpectedState = $thisAppPool.Value.ToString() -eq $expectedState
		$logMessage = ('AppPool: {0} is {1}' -f $appPoolName, $thisAppPool.Value)
		Log-TimedMessage -logMessage $logMessage -log $logFile
    }
    catch
	{
		Log-Exception -exception $_ -log $logFile
    }
    
    if($IsAppPoolInExpectedState)
	{
		$returnProperties = @{
			Result=1;
			RawResults=$rawResult;
			TimeStamp= Get-TimeStamp
		}
    }
    else
	{
		$returnProperties = @{
			Result=0;
			RawResults=$rawResult;
			TimeStamp= Get-TimeStamp
		}
    }
    $resultObject = New-Object PsObject -Property $returnProperties
    return $resultObject
}

# Execute DVT Script
function Execute-DVTScript(
    [string]$dvtLocalScript,
    [string]$log,
    [string]$xmlInputPath)
{
	if(Test-Path -Path $dvtLocalScript)
	{
		Log-TimedMessage -logMessage ('Executing DVT Script: {0}' -f $dvtLocalScript) -log $log
		$commandArgs = @{
			"InputXML" = $xmlInputPath;
			"Log" = $log
		}

		$output = & $dvtLocalScript @commandArgs *>&1
		Log-TimedMessage -logMessage $output -log $log
	}
	else
	{
		Throw "$dvtLocalScript was not found."
	}
}

function Run-ServiceModelDVT(
    [string]$serviceModelXml,
    [string]$config,
    [string]$serviceModelDirName,
    [string]$dvtScript,
    [array]$dvtHelperScripts,
    [string]$log)
{
	if(-not($config) -and -not($serviceModelXml)) 
	{
		Throw 'Atleast one of config or serviceModelXml parameters required'
	}

	# Check if the directory exists for the Log File Path
	New-DirectoryIfNotExists -DirPath (Join-Path -Path $env:SystemDrive -ChildPath $serviceModelDirName)
	Log-TimedMessage -logMessage 'Running DVT Process' -log $log

	# Create DVT local directory
	[string]$dvtLocalBin = CreateDirectoryAndCopy-ScriptsToDVTLocalDir -dvtScript $dvtScript -dvthelperScripts $dvtHelperScripts -log $log

	# Parse service model parameters and create input XML for on demand DVT
	Log-TimedMessage -logMessage 'Parsing service model parameters, and creating input XML' -log $log

	# Parameters from Service Model
	if($config)
	{
		Log-TimedMessage -logMessage 'Parsing service model params as JSON' -log $log
		Log-Parameter -parameterName 'Input Config string' -parameterValue $config -log $log

		[string]$jsonString = Convert-Base64ToString -base64String $config
		$dvtParams = $jsonString | ConvertFrom-Json
		$serviceName = $dvtParams.ExpectedDVTServiceName
		$appPoolName = $dvtParams.ExpectedDVTAppPoolName
		$appPoolState = $dvtParams.ExpectedDVTAppPoolState
	}
	elseif($serviceModelXml)
	{
		Log-TimedMessage -logMessage 'Parsing service model params as XML' -log $log

		[xml]$paramXML = Get-Content $serviceModelXml
		$serviceName = Get-ServiceModelParameterValue -ServiceModelXml $paramXML -XPath '//Configuration/Setting' -ParamName 'ExpectedDVTServiceName'
		$appPoolName = Get-ServiceModelParameterValue -ServiceModelXml $paramXML -XPath '//Configuration/Setting' -ParamName 'ExpectedDVTAppPoolName'
		$appPoolState = Get-ServiceModelParameterValue -ServiceModelXml $paramXML -XPath '//Configuration/Setting' -ParamName 'ExpectedDVTAppPoolState'
	}
	else
	{
		Throw ('Unable to parse settings from service model. Config: {0}' -f $config)
	}

	Log-TimedMessage -logMessage ('Parameters: {0} {1} {2}' -f $serviceName, $appPoolName, $appPoolState) -log $log
	[string]$dvtOutputRoot = (Split-Path -Path $dvtLocalBin -Parent)
	[string]$DVTOutputBin = (Join-Path -Path $dvtOutputRoot -ChildPath "output")

# DVT input XML Template
[xml]$xmlTemplate = @"
<?xml version="1.0"?>
<DVTParameters xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
<ServiceName>$serviceName</ServiceName>
<AppPoolName>$appPoolName</AppPoolName>
<AppPoolState>$appPoolState</AppPoolState>
<OutputPath>$DVTOutputBin</OutputPath>
</DVTParameters>
"@

	# Calculate Input XML Path
	$xmlInputChildPath =  Get-PathLeafNoFileExtenstion -path $dvtScript
	$xmlInputPath = (Join-Path -Path $dvtLocalBin -ChildPath ('{0}.xml' -f $xmlInputChildPath))
	Log-TimedMessage -logMessage ('Executing DVT XML at: {0}' -f $xmlInputPath) -log $log
	$xmlTemplate.InnerXml | Out-File -FilePath $xmlInputPath -Force -Encoding utf8
	$dvtLocalScript = (Join-Path -Path $dvtLocalBin -ChildPath (Split-Path -Path $dvtScript -Leaf))

	# Execute DVT Script
	Execute-DVTScript -dvtLocalScript $dvtLocalScript -log $log -xmlInputPath $xmlInputPath
}

function Run-NonWebServiceBasedServiceModelDVT(
    [string]$serviceModelXml,
    [string]$config,
    [string]$serviceModelDirName,
    [string]$dvtScript,
    [array]$dvtHelperScripts,
    [string]$log)
{
    if(-not($config) -and -not($serviceModelXml)) 
    {
        Throw 'Atleast one of config or serviceModelXml parameters required'
    }

    # Check if the directory exists for the Log File Path
    New-DirectoryIfNotExists -DirPath (Join-Path -Path $env:SystemDrive -ChildPath $serviceModelDirName)
    Log-TimedMessage -logMessage 'Running DVT Process' -log $log

    # Create DVT local directory
    [string]$dvtLocalBin = CreateDirectoryAndCopy-ScriptsToDVTLocalDir -dvtScript $dvtScript -dvthelperScripts $dvtHelperScripts -log $log

    # Parse service model parameters and create input XML for on demand DVT
    Log-TimedMessage -logMessage 'Parsing service model parameters, and creating input XML' -log $log

    # Parameters from Service Model
    if ($config)
    {
        Log-TimedMessage -logMessage 'Parsing service model params as encoded JSON.' -log $log
		Log-Parameter -parameterName 'Input Config string' -parameterValue $config -log $log
        
        $decodedConfig = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($config))
        $settings = ConvertFrom-Json $decodedConfig
        $aosWebsiteName = $settings.AOSWebsiteName
    }
    elseif ($serviceModelXml)
    {
        Log-TimedMessage -logMessage 'Parsing service model params as XML' -log $log

        [xml]$paramXML = Get-Content $serviceModelXml
        $aosWebsiteName = Get-ServiceModelParameterValue -ServiceModelXml $paramXML -XPath '//Configuration/Setting' -ParamName 'AOSWebsiteName'
    }
    else
    {
        Throw ('Unable to parse settings from service model. Config: {0}' -f $config)
    }
	
	Log-Parameter -parameterName 'AOS Website Name' -parameterValue $aosWebsiteName -log $log
    [string]$dvtOutputRoot = (Split-Path -Path $dvtLocalBin -Parent)
    [string]$DVTOutputBin = (Join-Path -Path $dvtOutputRoot -ChildPath "output")

# DVT input XML Template
[xml]$xmlTemplate = @"
<?xml version="1.0"?>
<DVTParameters xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
<ServiceName>$serviceName</ServiceName>
<AOSWebsiteName>$aosWebsiteName</AOSWebsiteName>
<OutputPath>$DVTOutputBin</OutputPath>
</DVTParameters>
"@

    # Calculate Input XML Path
    $xmlInputChildPath =  Get-PathLeafNoFileExtenstion -path $dvtScript
    $xmlInputPath = (Join-Path -Path $dvtLocalBin -ChildPath ('{0}.xml' -f $xmlInputChildPath))
    Log-TimedMessage -logMessage ('Executing DVT XML at: {0}' -f $xmlInputPath) -log $log
    $xmlTemplate.InnerXml | Out-File -FilePath $xmlInputPath -Force -Encoding utf8
    $dvtLocalScript = (Join-Path -Path $dvtLocalBin -ChildPath (Split-Path -Path $dvtScript -Leaf))

    # Execute DVT Script
    Execute-DVTScript -dvtLocalScript $dvtLocalScript -log $log -xmlInputPath $xmlInputPath
}

function Create-TestResultXML(
	[string]$testName,
	[System.Object]$TestResult,
	[string]$xmlFilePath,
	[string]$logFile)
{
	# Diagnostics Collector XML Template
[xml]$dvtOutputXmlTemplate = @"
<?xml version="1.0"?>
<CollectionResult xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
	<CollectorName>$collectorName</CollectorName>
	<CollectorType>$collectorType</CollectorType>
	<ErrorMessages />
	<TabularResults>
	<TabularData>
		<TargetName>$targetName</TargetName>
		<Columns>
		<string>TestName</string>
		<string>TestType</string>
		<string>PassResult</string>
		<string>RawResult</string>
		<string>TimeStamp</string>
		</Columns>
		<Rows>
		</Rows>
	</TabularData>
	</TabularResults>
</CollectionResult>
"@
	$logMessage = 'Append-RowToTestResultsXml -TestName {0} -TestType DVT -TestResult {1} -RawResult {2} -TimeStamp {3}' -f $testName, $testResult.Result, $testResult.RawResults, $testResult.TimeStamp
	Log-TimedMessage -logMessage $logMessage -log $logFile 

	Append-RowToTestResultsXml -TestName $testName `
							   -TestType 'DVT' `
							   -TestResult $testResult.Result `
							   -RawResult $testResult.RawResults `
							   -TimeStamp $testResult.TimeStamp `
							   -dvtOutputXmlTemplate $dvtOutputXmlTemplate `
							   -log $logFile | Out-Null

	#Writing XML results
	Log-TimedMessage -logMessage ('Writing DVT results to {0}' -f $xmlFilePath) -log $logFile
	$dvtOutputXmlTemplate.InnerXml | Out-File -FilePath $xmlFilePath -Force -Encoding utf8
}

function Report-TestResults(
	[string]$testName,
	[System.Object]$TestResult,
	[string]$xmlFilePath,
	[string]$logFile)
{
	Create-TestResultXML -testName $testName -TestResult $testResult -xmlFilePath $xmlFilePath -logFile $logFile
	[bool]$dvtResult = $testResult.Result

	if($dvtResult)
	{
		$exitProperties = @{'ExitCode'= 0}
		$exitObject = New-Object PsObject -Property $exitProperties
		Log-TimedMessage -logMessage ('Service Model {0} DVT script completed, ExitCode: {1}' -f $serviceModelName, $exitObject.ExitCode) -log $logFile
		return $exitObject
	}
	else
	{
		$exitProperties = @{
			'ExitCode'= 1;
			'Message'= ('Service Model {0} DVT Validation failed, see log: {1} for further details, and {2} for test results' -f $serviceModelName, $logFile, $xmlFilePath)
		}
		$exitObject = New-Object PsObject -Property $exitProperties
		Log-TimedMessage -logMessage ('Service Model {0} DVT Script Completed, ExitCode: {1}' -f $serviceModelName, ($exitObject.ExitCode)) -log $logFile
		throw $exitObject
	}
}

function Execute-ServiceModelAppPoolDVT(
    [string]$serviceModelName,
    [string]$appPoolName,
    [string]$appPoolState,
    [string]$logFile,
    [string]$xmlFilePath)
{
	# IIS AppPool Validation
	$testName = $serviceModelName + '.Validate-AppPool'

	$logMessage = 'Validate-AppPool -AppPoolName {0} -expectedState {1}' -f $appPoolName, $appPoolState
	Log-TimedMessage -logMessage $logMessage -log $logFile 

	$appPoolResult = Validate-AppPool -AppPoolName $appPoolName -expectedState $appPoolState -logFile $logFile

	# Reports the test results in an xml and returns the results object
	Report-TestResults -testName $testName -TestResult $appPoolResult -xmlFilePath $xmlFilePath -logFile $logFile
}

function Validate-RegistryEntry(
    [string]$registryPath,
    [string]$registryKey,
    
    [string]$expectedRegistryValue,
    [switch]$ensureRegistryValueIsValidPath,

    [string]$logFile)
{
    [bool]$isValidationSuccessful = $false

    try
    {
        Log-TimedMessage -logMessage ('Validating existence of registry Key: {0}' -f $registryPath) -log $logFile
        
        # Check if registry path is valid.
        $isValidationSuccessful = Test-Path -Path $registryPath
        $rawResult = 'Registry path = {0} ;' -f $registryPath
        
        if ($isValidationSuccessful)
        {
            # Get the registry key value.
            $actualRegistryValue = (Get-ItemProperty -Path $registryPath -Name $registryKey -ErrorAction SilentlyContinue).$registryKey
            
            Log-TimedMessage -logMessage ('Actual registry value is {0}.' -f $actualRegistryValue) -log $logFile
            $rawResult += 'Actual registry value = {0} ;' -f $actualRegistryValue
            
            # If the registry key value is null, then validation fails.
            if (-not($actualRegistryValue))
            {
                $isValidationSuccessful = $false
            }
            else
            {
                # Validate the registry value if expected to be a valid path.
                if ($ensureRegistryValueIsValidPath)
                {
                    $isValidationSuccessful = Test-Path -Path $actualRegistryValue
                    $rawResult += 'Ensuring registry value is a valid path = {0} ;' -f $isValidationSuccessful
                    
                    Log-TimedMessage -logMessage ('Validation result of registry value {0} as valid path is {1}' -f $actualRegistryValue, $isValidationSuccessful) -log $logFile
                    
                }
                
                # Validate the registry value if expected registry value is not NULL.
                if ($expectedRegistryValue)
                {
                    $isValidationSuccessful = $expectedRegistryValue -eq $actualRegistryValue
                    $rawResult += 'Expected registry value = {0}; Expected registry value validation result = {1}' -f $expectedRegistryValue, $isValidationSuccessful
                    
                    Log-TimedMessage -logMessage ('Validation result for comparison of registry value {0} to expected value {1} is {2}' -f $actualRegistryValue, $expectedRegistryValue, $isValidationSuccessful) -log $logFile
                }
            }
        }
        else
        {
            Log-TimedMessage -logMessage ('Registry path {0} is invalid.' -f $registryPath) -log $logFile
        }
    }
    catch
    {
        Log-Exception -exception $_ -log $logFile
    }

    if ($isValidationSuccessful)
    {
        $returnProperties = @{
            Result=1;
            RawResults=$rawResult;
            TimeStamp= Get-TimeStamp
        }
    }
    else
    {
        $returnProperties = @{
            Result=0;
            RawResults=$rawResult;
            TimeStamp= Get-TimeStamp
        }
    }
    
    $resultObject = New-Object PsObject -Property $returnProperties
    return $resultObject
}

function Execute-RegistryValidationDVT(
    [string]$registryPath,
    [string]$registryKey,
    
    [string]$expectedRegistryValue,
    [switch]$ensureRegistryValueIsValidPath,
    
    [string]$serviceModelName,
    [string]$logFile,
    [string]$xmlFilePath)
{
    $testName = '{0}.Validate-RegistryEntry' -f $serviceModelName
    
    $logMessage = 'Validate-RegistryEntry -registryPath {0} -registryKey {1} -expectedRegistryValue {2} -logFile {3} -xmlFilePath {4}' -f $registryPath, $registryKey, $expectedRegistryValue, $logFile, $xmlFilePath

    if ($ensureRegistryValueIsValidPath)
    {
        $logMessage += '-ensureRegistryValueIsValidPath'
    }
    
    Log-TimedMessage -logMessage $logMessage -log $logFile 

    if ($ensureRegistryValueIsValidPath)
    {
        $testExecutionResults = Validate-RegistryEntry -registryPath $registryPath `
                                                       -registryKey $registryKey `
                                                       -expectedRegistryValue $expectedRegistryValue `
                                                       -ensureRegistryValueIsValidPath `
                                                       -logFile $logFile
    }
    else
    {
        $testExecutionResults = Validate-RegistryEntry -registryPath $registryPath `
                                                       -registryKey $registryKey `
                                                       -expectedRegistryValue $expectedRegistryValue `
                                                       -logFile $logFile
    }

    # Reports the test results in an xml and returns the results object
    Report-TestResults -testName $testName -TestResult $testExecutionResults -xmlFilePath $xmlFilePath -logFile $logFile
}

Export-ModuleMember -Function Log-TimedMessage
Export-ModuleMember -Function Get-TimeStamp
Export-ModuleMember -Function Convert-Base64ToString
Export-ModuleMember -Function Get-ServiceModelParameterValue 
Export-ModuleMember -Function New-DirectoryIfNotExists
Export-ModuleMember -Function CreateDirectoryAndCopy-ScriptsToDVTLocalDir
Export-ModuleMember -Function Append-RowToTestResultsXml
Export-ModuleMember -Function Validate-AppPool
Export-ModuleMember -Function Execute-DVTScript
Export-ModuleMember -Function Run-ServiceModelDVT
Export-ModuleMember -Function Execute-ServiceModelAppPoolDVT
Export-ModuleMember -Function Log-Exception
Export-ModuleMember -Function Handle-DvtError
Export-ModuleMember -Function Execute-RegistryValidationDVT
Export-ModuleMember -Function Run-NonWebServiceBasedServiceModelDVT

# SIG # Begin signature block
# MIIdvgYJKoZIhvcNAQcCoIIdrzCCHasCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUHXxG5RdO6Jwc/3B7qCgrH0P5
# MCegghhkMIIEwzCCA6ugAwIBAgITMwAAAKxjFufjRlWzHAAAAAAArDANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBMQwggTAAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCB2DAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQUlnNOwfdH/bx9/xnHnZXlihCF15MweAYKKwYB
# BAGCNwIBDDFqMGigNoA0AEMAbwBtAG0AbwBuAEQAVgBUAEgAZQBsAHAAZQByAE0A
# bwBkAHUAbABlAC4AcABzAG0AMaEugCxodHRwOi8vd3d3Lk1pY3Jvc29mdC5jb20v
# TWljcm9zb2Z0RHluYW1pY3MvIDANBgkqhkiG9w0BAQEFAASCAQCOYM8BhyZhJPz9
# xoAiKKU294ce4oeGDWezZj7xF6mejGU5TjLgq4oQ+qvKTIq2Afk860QoSCSsBhRl
# p/Osednr0IMI56MLzMQ3kATkF2TJk4uZN5IWbSNlrsXJLf9u/0BARp4KtUNtOgA1
# biy5HpODshNq73VjRRCi+F9VGGx85LEc6jNXGewKNRLDC0ZBM1XF00hJPY6tbzl/
# 2F9dyRBqgk20GRA1PTK/iQ6Qo9M6PArYF240IXY8TCphI7O982L0mQ39b0DW/CYF
# +oztJcx7LvmHteB0maVA5u9neXzTD9WvG9tWJoKPHD2CJLTh3tYPubo4ECt55h9K
# VOshlZFPoYICKDCCAiQGCSqGSIb3DQEJBjGCAhUwggIRAgEBMIGOMHcxCzAJBgNV
# BAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
# HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAfBgNVBAMTGE1pY3Jvc29m
# dCBUaW1lLVN0YW1wIFBDQQITMwAAAKxjFufjRlWzHAAAAAAArDAJBgUrDgMCGgUA
# oF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcNMTYw
# NzIxMjEwNzE1WjAjBgkqhkiG9w0BCQQxFgQUpRspoM/AE11E+AcapGxhmHtTfPYw
# DQYJKoZIhvcNAQEFBQAEggEAR5GsDyi9wOnxPgUsDJE8M9wYX8+BIzIo9NSJRb5W
# LbAFnu6NP/OnplL6GOaocle2V684ShELQSdNTz8jHecR4m9R+GpIZc7NoLzJqahz
# aEG+TOBlIy1NiLbCzzOGcAjRBSr18J5WpV8PCcBaGOdqttLOcSSMWtTdqAMFGbo7
# boML1VewVhnynvAzOIyRXg4JQogEyooxaaRHJieVhrnZnnpRGEdA1sJfqjHjbpLC
# dw5M5Rd97eUs1gFApI1nCY2N/YQIPIUtWfx5kWwUrfONpf3CjGdK6ek2g1kKxWY7
# F/zGDflQG6/NCbY7PQYUogsrU9SAfuoWevUT9o7xm51Veg==
# SIG # End signature block
