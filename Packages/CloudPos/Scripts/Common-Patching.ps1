<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

function Locate-ConfigFile
{
    param(
        [string]$ScriptDirectory = $(Throw 'ScriptDirectory parameter required'),

        [string]$ConfigFileName = $(Throw 'ConfigFileName parameter required'),

        [ValidateSet("RetailServer", "HardwareStation", "AsyncClient", IgnoreCase = $false)]
        [string]$ComponentType = $(Throw 'ComponentType parameter required')
    )

    Log-TimedMessage 'Trying to locate configuration file...'

    $configFilePath = $null;

    switch -Regex ($ComponentType) 
    {
        "RetailServer|HardwareStation|AsyncClient"
        {
            $configFileFolder = Join-Path -Path (Get-Item -Path $ScriptDirectory).Parent.FullName -ChildPath 'Package'
            $configFilePath = Join-Path -Path $configFileFolder -ChildPath $ConfigFileName

            if ((Test-Path -Path $configFilePath))
            {
                Log-TimedMessage ('Found configuration file at {0}' -f $configFilePath)
            }
            else
            {
                throw 'ERROR! Missing configuration file from installation.'
            }
        }

        default 
        {
            throw 'Component not supported.'
        }
    }

    return $configFilePath
}

<#
.SYNOPSIS
Adds element to a .config file:

    <section name="commerceRuntime" type="Microsoft.Dynamics.Commerce.Runtime.Configuration.CommerceRuntimeSection, Microsoft.Dynamics.Commerce.Runtime.ConfigurationProviders, Version=6.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL"/>

in the XML section with XPath:
    
    '/configuration/configSections'

The above section sets AutoFlush property of the listener set to true.
#>
function Update-CommerceRuntimeConfigurationSectionClass
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required'),
        
        [string]$SectionName = 'commerceRuntime'
    )

    Log-TimedMessage 'Updating Commerce Runtime configuration section class...'

    $className = 'Microsoft.Dynamics.Commerce.Runtime.Configuration.CommerceRuntimeSection';
    $namespace = 'Microsoft.Dynamics.Commerce.Runtime.ConfigurationProviders';

    $commerceRuntimeSection = @{
        'xmlDoc' = $ConfigXml;
        'parentXPath' = '/configuration/configSections';
        'childXpath' = ("section[@name='{0}']" -f $SectionName);
        'childName' = "section";
        'attributesHashtable' = @{
            'name' = $SectionName;
            'type' = ('{0}, {1}, Version=6.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL' -f $className, $namespace);
        }
    }

    CreateOrUpdateChildXmlNodeWithAttributes @commerceRuntimeSection

    Log-TimedMessage 'Finished updating Commerce Runtime configuration section class.'
}

function SafeEnable-HardwareStationMonitoringLogging
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required')
    )

    try
    {
        Log-TimedMessage 'Enabling Hardware Station monitoring logging...'

        Enable-ServiceMonitoringLogging -MonitoringEventSourceNameFormat 'Microsoft Dynamics AX Retail Monitoring : Hardware Station {0}' `
            -MonitoringListenerName 'MonitoringEventLogTraceListener' -DefaultInstanceName 'HardwareStation' `
            -MonitoringSourceName 'RetailMonitoringTracer' -EventLogName 'Application' -ConfigXml $ConfigXml

        Log-TimedMessage 'Finished enabling Hardware Station monitoring logging.'
    }
    catch
    {
        Log-TimedError 'Enabling Hardware Station monitoring logging failed.'
        Log-Exception $_
    }
}

function SafeEnable-RetailServerMposMonitoringLogging
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required')
    )

    try 
    {
        Log-TimedMessage 'Enabling Retail Server Mpos monitoring logging...'

        Enable-ServiceMonitoringLogging -MonitoringEventSourceNameFormat 'Microsoft Dynamics AX Retail Monitoring : Retail Server {0} MPOS' `
            -MonitoringListenerName 'MposMonitoringEventLogTraceListener' -DefaultInstanceName 'RetailServer' `
            -MonitoringSourceName 'RetailMposMonitoringTracer' -EventLogName 'Retail MPOS Devices' -ConfigXml $ConfigXml

        Log-TimedMessage 'Finished enabling Retail Server Mpos monitoring logging.'
    }
    catch
    {
        Log-TimedError 'Enabling Retail Server Mpos monitoring logging failed.'
        Log-Exception $_
    }
}

function Enable-ServiceMonitoringLogging
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required'),
        [string]$MonitoringEventSourceNameFormat = $(Throw 'MonitoringEventSourceNameFormat parameter required'),
        
        [string]$MonitoringListenerName = $(Throw 'MonitoringListenerName parameter required'),
        [string]$MonitoringSourceName = $(Throw 'MonitoringSourceName parameter required'),
        
        [string]$DefaultInstanceName = $(Throw 'DefaultInstanceName parameter required'),
        [string]$EventLogName = 'Application'
    )

    $instanceName = SafeGet-InstanceName -ConfigXml $ConfigXml -DefaultInstanceName $DefaultInstanceName

    $monitoringEventSourceName = ($MonitoringEventSourceNameFormat -f $instanceName)

    Create-SharedEventLogTraceListener -ConfigXml $ConfigXml -ListenerName $MonitoringListenerName -EventSourceName $monitoringEventSourceName

    Create-TraceSource -ConfigXml $ConfigXml -SourceName $MonitoringSourceName -ListenerNames $MonitoringListenerName

    Update-AutoFlushList -ConfigXml $ConfigXml -ListenerName $MonitoringListenerName
    
    Create-RetailMonitoringEventSource

    Create-EventSource -LogName $EventLogName -Source $monitoringEventSourceName
}

<#
.SYNOPSIS
Derives web application name for the web service from the web.config.

.NOTES
In the case of failure it returns default value provided as a script parameter.
#>
function SafeGet-InstanceName
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required'),

        [string]$DefaultInstanceName = $(Throw 'DefaultInstanceName parameter required')
    )

    try
    {
        $listenerName = 'EventLogTraceListener';

        # derive the name of the web application name from the ending of the listener
        $listener = $ConfigXml.SelectSingleNode(("/configuration/system.diagnostics/sharedListeners/add[@name='{0}']" -f $listenerName))
        
        if ([bool]$listener)
        {
            $eventSourceName = $listener.GetAttribute('initializeData')
            $instanceName = ($eventSourceName -split ' ' | Select-Object -Last 1)
            if ((-not $instanceName) -and ([string]::IsNullOrEmpty($instanceName.Trim())))
            {
                throw 'instanceName has incorrect value.';
            }
        }
        else
        {
            $message = ('Could not find shared listener with name {0}.' -f $listenerName)
            throw $message;
        }
    }
    catch
    {
        $instanceName = $DefaultInstanceName

        Log-TimedError 'Deriving of the instance name failed.'
        Log-Exception $_
        Log-TimedMessage ('Using default instance name {0}.' -f $instanceName)
    }

    return $instanceName
}

<# 
.SYNOPSIS
Adds section to a .config file:

    <source name="$SourceName" switchValue="$TracingLevel">
        <listeners>
            <add name="$ListenerNames[0]" />
            <!-- ... -->
            <add name="$ListenerNames[n]" />
        </listeners>
    </source>

in the XML section with XPath:
    
    "/configuration/system.diagnostics/sources"

The above section creates trace source and attaches shared listeners to it.
#>    
function Create-TraceSource
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required'),
        [string]$SourceName = $(Throw 'SourceName parameter required'),
        
        [string[]]$ListenerNames,
        [string]$TracingLevel = 'Information'
    )

    $source = @{
        'xmlDoc' = $ConfigXml;
        'parentXPath' = '/configuration/system.diagnostics/sources';
        'childXpath' = ("source[@name='{0}']" -f $SourceName);
        'childName' = 'source';
        'attributesHashtable' = @{
            'name' = $SourceName;
            'switchValue' = $TracingLevel;
        }
    }

    CreateOrUpdateChildXmlNodeWithAttributes @source

    $sourceListeners = @{
        'xmlDoc' = $ConfigXml;
        'parentXPath' = ("/configuration/system.diagnostics/sources/source[@name='{0}']" -f $SourceName);
        'childXpath' = 'listeners';
        'childName' = 'listeners';
        'attributesHashtable' = @{
        }
    };

    CreateOrUpdateChildXmlNodeWithAttributes @sourceListeners

    $ListenerNames | Attach-SharedListener -ConfigXml $ConfigXml -SourceName $monitoringSourceName
}

<# 
.SYNOPSIS
Adds element to a .config file:

    <add name="$ListenerName" type="System.Diagnostics.EventLogTraceListener" initializeData="$EventSourceName" />

in the XML section with XPath:

    '/configuration/system.diagnostics/sharedListeners'

The above section creates a shared event log trace listener with specified name.
#>
function Create-SharedEventLogTraceListener
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required'),
        
        [string]$ListenerName = $(Throw 'ListenerName parameter required'),
        
        [string]$EventSourceName = $(Throw 'EventSourceName parameter required')
    )

    $sharedListener = @{
        'xmlDoc' = $ConfigXml;
        'parentXPath' = '/configuration/system.diagnostics/sharedListeners';
        'childXpath' = ("add[@name='{0}']" -f $ListenerName);
        'childName' = 'add';
        'attributesHashtable' = @{
            'name' = $ListenerName;
            'type' = 'System.Diagnostics.EventLogTraceListener';
            'initializeData' = $EventSourceName;
        };
    }

    CreateOrUpdateChildXmlNodeWithAttributes @sharedListener
}

<# 
.SYNOPSIS
Adds element to a .config file:

    <add name="$ListenerName" />

in the XML section with XPath:
    
    "/configuration/system.diagnostics/sources/source[@name='$SourceName']/listeners"

The above section attaches shared listener to the trace source.
#>  
function Attach-SharedListener
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required'),
        
        [string]$SourceName = $(Throw 'SourceName parameter required'),
        
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$ListenerName
    )

    $attachment = @{
        'xmlDoc' = $ConfigXml;
        'parentXPath' = ("/configuration/system.diagnostics/sources/source[@name='{0}']/listeners" -f $SourceName);
        'childXpath' = ("add[@name='{0}']" -f $ListenerName);
        'childName' = 'add';
        'attributesHashtable' = @{
            'name' = $ListenerName
        }
    }

    CreateOrUpdateChildXmlNodeWithAttributes @attachment
}

<# 
.SYNOPSIS
Adds element to a .config file:

    <add name="$ListenerName" />

in the XML section with XPath:
    
    "/configuration/system.diagnostics/trace[@autoflush='true']/listeners"
	
The above section sets AutoFlush property of the listener set to true.
#>	
function Update-AutoFlushList
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required'),
        
        [string]$ListenerName = $(Throw 'ListenerName parameter required')
    )

    $autoFlushSwitchForListener = @{
        'xmlDoc' = $ConfigXml;
        'parentXPath' = "/configuration/system.diagnostics/trace[@autoflush='true']/listeners";
        'childXpath' = ("add[@name='{0}']" -f $ListenerName);
        'childName' = 'add';
        'attributesHashtable' = @{
            'name' = $ListenerName
        }
    }
    
    CreateOrUpdateChildXmlNodeWithAttributes @autoFlushSwitchForListener
}

<#
.SYNOPSIS
	Makes sure that a certain child node exists with the correct attributes and attribute values, if it does not, creates it		

.EXAMPLE
PS C:\> $xml = [xml]@"
<a>
    <b name="c">
    </b> 
    <b name="d">
    </b>
</a>
"@
PS C:\> CreateOrUpdateChildXmlNodeWithAttributes -xmlDoc $xml -parentXPath '/a' -childXpath 'e' -childName 'e' -attributesHashtable @{}
PS C:\> $xml.OuterXml
<a><b name="c"></b><b name="d"></b><e /></a>

.EXAMPLE
PS C:\> $xml = [xml]@"
<a>
    <b name="c">
    </b> 
    <b name="d">
    </b>
</a>
"@
PS C:\> CreateOrUpdateChildXmlNodeWithAttributes -xmlDoc $xml -parentXPath "/a/b[@name='c']" -childXpath 'f' -childName 'f' -attributesHashtable @{'name'='noname'}
PS C:\> $xml.OuterXml
<a><b name="c"><f name="noname" /></b><b name="d"></b></a>

.EXAMPLE
PS C:\> $xml = [xml]@"
<a>
    <b name="c">
    </b> 
    <b name="d">
    </b>
</a>
"@
PS C:\> CreateOrUpdateChildXmlNodeWithAttributes -xmlDoc $xml -parentXPath "/a" -childXpath "b[@name='d']" -childName 'b' -attributesHashtable @{'name'='fi'}
PS C:\> $xml.OuterXml
<a><b name="c"></b><b name="fi"></b></a>
#>
function CreateOrUpdateChildXmlNodeWithAttributes
{    
    param(
        [xml]$xmlDoc = $(Throw 'xmlDoc parameter required'),

        [string]$parentXPath = $(Throw 'parentXPath parameter required'),
    
        [string]$childXpath = $(Throw 'childXpath parameter required'),
    
        [string]$childName = $(Throw 'childName parameter required'),
    
        [hashtable]$attributesHashtable,

        [switch]$UpdateOnly
    )

    $childXPath = $parentXPath + '/' + $childXpath
    $childNode = $xmlDoc.SelectSingleNode($childXPath)

    if ($childNode -eq $null)
    {
        Log-TimedMessage ('Child with XPath {0} was not found.' -f $childXPath)
        
        if ($UpdateOnly)
        {
            Log-TimedMessage 'Leaving function.'
            return;
        }
        else 
        {
            Log-TimedMessage ('Creating child node {0}.' -f $childName)
            
            $parentNode = $xmlDoc.SelectSingleNode($parentXPath)
            $childElement = $xmlDoc.CreateElement($childName)
            $childNode = $parentNode.AppendChild($childElement)
           
            Log-TimedMessage ('Finished creating child node {0}.' -f $childName)
        }
    }
    else
    {
        Log-TimedMessage ('Found element with XPath {0}.' -f $childXPath)
    }

    foreach($attributeKey in $attributesHashtable.Keys)
    {
        $attributeValueToSet = $attributesHashtable[$attributeKey]
        $existingAttributeValue = $childNode.GetAttribute($attributeKey)
        
        if ($existingAttributeValue -eq $null)
        {
            Log-TimedMessage "Creating attribute [$attributeKey] with value = $attributeValueToSet."
            $childNode.SetAttribute($attributeKey, $attributeValueToSet)
        }
        else 
        {
            if ($existingAttributeValue -ne $attributeValueToSet)
            {
                Log-TimedMessage "Setting attribute [$attributeKey] with value = $attributeValueToSet."
                $childNode.SetAttribute($attributeKey, $attributeValueToSet)
            }
            else
            {
                Log-TimedMessage "Attribute [$attributeKey] already has value = $attributeValueToSet."
            }
        }
    }
}

function CreateChildXmlNodeWithoutAttributes
{
    param(
        [xml]$xmlDoc = $(Throw 'xmlDoc parameter required'),

        [string]$parentXPath = $(Throw 'parentXPath parameter required'),
    
        [string]$childXpath = $(Throw 'childXpath parameter required'),
    
        [string]$childName = $(Throw 'childName parameter required'),

        [string]$innerText
    )

    $childXPath = $parentXPath + '/' + $childXpath
    $childNode = $xmlDoc.SelectSingleNode($childXPath)

    if ($childNode -eq $null)
    {
        Log-TimedMessage ('Child with XPath {0} was not found.' -f $childXPath)
        
        Log-TimedMessage ('Creating child node {0}.' -f $childName)
            
        $childElement = $xmlDoc.CreateElement($childName)

        if (-not ([string]::IsNullOrEmpty($innerText)))
        {
            $childElement.InnerText = $innerText
        }

        $parentNode = $xmlDoc.SelectSingleNode($parentXPath)
        [void]$parentNode.AppendChild($childElement)
           
        Log-TimedMessage ('Finished creating child node {0}.' -f $childName)
    }
}

function Update-RollingXmlWriterTraceListenerTypeName 
{
    param(
        [xml]$ConfigXml = $(Throw 'ConfigXml parameter required')
    )

    Log-TimedMessage 'Updating RollingXmlWriterTraceListener type name...'

    $rollingXmlWriterTraceListener = @{
        'xmlDoc' = $ConfigXml;
        'parentXPath' = '/configuration/system.diagnostics/sharedListeners';
        'childXpath' = ("add[@name='{0}']" -f 'RollingXmlWriterTraceListener');
        'childName' = 'add';
        'attributesHashtable' = @{
            'type' = 'Microsoft.Dynamics.Retail.Diagnostics.Sinks.RollingXmlWriterTraceListener, Microsoft.Dynamics.Retail.Diagnostics.Sinks';
        }
    }

    CreateOrUpdateChildXmlNodeWithAttributes @rollingXmlWriterTraceListener -UpdateOnly

    Log-TimedMessage 'Finished updating RollingXmlWriterTraceListener type name.'
}
# SIG # Begin signature block
# MIIdsAYJKoZIhvcNAQcCoIIdoTCCHZ0CAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU0+XT/iAn3ci/VOY/FSbYzth6
# dKqgghhkMIIEwzCCA6ugAwIBAgITMwAAAJ1CaO4xHNdWvQAAAAAAnTANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBLYwggSyAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCByjAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQU+ezh/j+JrBGnuTWaSufJlCfHUcswagYKKwYB
# BAGCNwIBDDFcMFqgKIAmAEMAbwBtAG0AbwBuAC0AUABhAHQAYwBoAGkAbgBnAC4A
# cABzADGhLoAsaHR0cDovL3d3dy5NaWNyb3NvZnQuY29tL01pY3Jvc29mdER5bmFt
# aWNzLyAwDQYJKoZIhvcNAQEBBQAEggEANAPbl90KI5MlLxeKG0xA5C9gEY9gljVE
# z5xwa9E1L4V2KKQODLv81ufkq2hZtx11ciXSs89551Ko75suwoVaNM2lZZEZdB7z
# Sl3HqU5NuE9g2Jrb6asvo5vaxd1oE/ErwOA6x00g+jYD/BVVoIlDaJCW94arGRMe
# 7O+Ce0WQEFvB7eyrxaPiUi/heV07zG19vT2u1ww1PIrUldYRlJt3DTkHDibBzCae
# 7nBGt1fw8CGsTah05KmGeF5ydQR2j9uO+5Bi6ZqiyinRlYfKNlY7lqyvWEGHZgVc
# k95bO845BlwEwY17rMa1sBkWU/ZHIf403WdmxKH+NT52ng1L89xnAqGCAigwggIk
# BgkqhkiG9w0BCQYxggIVMIICEQIBATCBjjB3MQswCQYDVQQGEwJVUzETMBEGA1UE
# CBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9z
# b2Z0IENvcnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQ
# Q0ECEzMAAACdQmjuMRzXVr0AAAAAAJ0wCQYFKw4DAhoFAKBdMBgGCSqGSIb3DQEJ
# AzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE2MDcyMTIxMDcxNVowIwYJ
# KoZIhvcNAQkEMRYEFNMQ8DEQHGayDgmlc7mla0yel+lWMA0GCSqGSIb3DQEBBQUA
# BIIBAJ542zs6lazQBocPYxUF2gEgpteTc6BuHeWNJ5N5oDkU7Ep+E9hPBfKNr0lg
# H7D8zeWw6dEImjx4DqbhSNLXQjA12MWV+4nb4NIxHJnGyNNbaeGbjHEyQvTjIr+n
# S8SbF135nO77COi5992vN6U2ulmk6RU0hTAiKJSAv2s9p9Vx8NdMxjFMoTDPmU1N
# 5VkXZMeIg7KethNw4GwiAJ+feYdt+FV2EfhtK2weNmPPaNEv6MLrXB5p/3W2BBCk
# Bg5ToXPzIh02zZYnpbqkl/VQP4aHUHioqoGfC/AWwgKWc4eVORUKpO64qM2x4Yly
# uiigL+oxWkZsLrk0iTLOgV/OQgE=
# SIG # End signature block
