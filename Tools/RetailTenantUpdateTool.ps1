﻿<#
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
#>

# Prerequisites: 
# Microsoft Online Services Sign-In Assistant for IT Professionals, download link: http://go.microsoft.com/fwlink/?LinkID=286152
# Azure Active Directory Module for Windows PowerShell (64-bit version), download link: http://go.microsoft.com/fwlink/p/?linkid=236297
# This script is intended to patch the Retail component after running AdminUserProvisioning.exe.
# This script requires the administrator credential.

param
(
	[ValidateNotNullOrEmpty()]
    $DefaultRetailWorkerToMap = '000160',
	
	[ValidateNotNullOrEmpty()]
    $AxDbServer = 'localhost',
	
	[ValidateNotNullOrEmpty()]
    $AxDbName = 'AxDBRAIN',
	
	[ValidateNotNullOrEmpty()]
    $RetailChannelDbName = 'AxDBRAIN',
	
	[ValidateNotNullOrEmpty()]
	$RetailWebSiteName = 'RetailServer'
)

function ExecuteSql(
	[string] $sqlServer = '(local)',
    [string] $database,		
	[string] $sqlStatement)
{
	Write-Host "Running sql statement in server: $sqlServer, DB: $database, sqlStatement: $sqlStatement "
	try
	{
		Invoke-SqlCmd -ServerInstance $sqlServer -Database  $database -Query $sqlStatement -ErrorAction 'Stop'		
	}
	catch
	{
		Write-Host  "$($_.Exception.Message)"
		Write-Error "Error while running a SQL command: $sqlServer, DB: $database, sqlStatement: $sqlStatement, error: $($_.Exception.Message) "		
	}
	
	Write-Host "Finished running sql statement in server: $sqlServer, DB: $database, sqlStatement: $sqlStatement, retailReturnCode: $retailReturnCode, LASTEXITCODE: $LASTEXITCODE "	
}

try
{
	Write-Host 'Checking prerequisites...'
	$command = Get-Command Connect-MsolService -ErrorAction Ignore
	if(!$command)	
	{
		$Host.UI.WriteErrorLine('please download and install below prerequisites:')
		$Host.UI.WriteErrorLine('Microsoft Online Services Sign-In Assistant for IT Professionals, download link: http://go.microsoft.com/fwlink/?LinkID=286152')
		$Host.UI.WriteErrorLine('Azure Active Directory Module for Windows PowerShell (64-bit version), download link: http://go.microsoft.com/fwlink/p/?linkid=236297')
		pause
		exit 1
	}

	$credential = Get-Credential
	$UPN = $credential.UserName
	$alias = $UPN.Substring(0,$UPN.IndexOf('@'))
	Write-Output $UPN

	Connect-MsolService -Credential $credential

	$company = Get-MsolCompanyInformation 
	$AADTenantID = $company.ObjectId
	$AADUserID = ''

	Write-Host ("TenantID: {0}" -f $company.ObjectId)

	$users = Get-MsolUser -SearchString "$($credential.username)"
	$identityprovider = 'https://sts.windows.net/'
	$rtsUserSID = 'S-1-19-2668615710-2480941646-1684813103-2026890972-2302207704-2864208061-3273002221-3047239075-1510753007-4079435192'

	if($credential.username -ilike '*.ccsctp.net')
	{
		$identityprovider = 'https://sts.windows-ppe.net/'
		$rtsUserSID = 'S-1-19-2668615710-2480941646-1684813103-2026890972-2302207704-1598698826-400483830-1280886117-2147114868-3359996204'
	}

	foreach($u in $users)
	{		
		if($u.UserPrincipalName -eq $UPN)
		{
			$AADUserID = $u.ObjectId
			Write-Host "UserId: $($u.ObjectId); UserPrincipalName:$($u.UserPrincipalName);"
		}
	}

	if([string]::IsNullOrWhiteSpace($AADUserID))
	{
		$msg = "Cannot find the user id in this tenant: $UPN "
		Write-Host $msg
		throw $msg
	}

	# Update SQL Database
	$insertServicePrincipalToSYSSERVICECONFIGURATIONSETTING = "if not exists (select 1 from SYSSERVICECONFIGURATIONSETTING where [Name] = 'SERVICEPRINCIPAL') begin insert into SYSSERVICECONFIGURATIONSETTING([Value],[Name]) values('$AADServicePrincipalName','SERVICEPRINCIPAL') end "
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement  $insertServicePrincipalToSYSSERVICECONFIGURATIONSETTING

	$insertTenantIdToSYSSERVICECONFIGURATIONSETTING = "if not exists (select 1 from SYSSERVICECONFIGURATIONSETTING where [Name] = 'TENANTID') begin insert into SYSSERVICECONFIGURATIONSETTING([Value],[Name]) values('$AADTenantID','TENANTID') end " 
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement $insertTenantIdToSYSSERVICECONFIGURATIONSETTING

	$updateTenantIdToSYSSERVICECONFIGURATIONSETTINGToAxDB = "UPDATE SYSSERVICECONFIGURATIONSETTING SET [Value] = '$AADTenantID' where [Name] = 'TENANTID'" 
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement $updateTenantIdToSYSSERVICECONFIGURATIONSETTINGToAxDB

	$updateTenantIdToSYSSERVICECONFIGURATIONSETTINGToChannelDB = "UPDATE ax.SYSSERVICECONFIGURATIONSETTING SET [Value] = '$AADTenantID' where [Name] = 'TENANTID'" 
	ExecuteSql -sqlServer $AxDbServer -database $RetailChannelDbName -sqlStatement $updateTenantIdToSYSSERVICECONFIGURATIONSETTINGToChannelDB

	$updateRETAILSTAFFTABLEClean = "UPDATE [dbo].[RETAILSTAFFTABLE] SET [EXTERNALIDENTITYID] = '',[EXTERNALIDENTITYSUBID] = ''  WHERE STAFFID = '$DefaultRetailWorkerToMap' "
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement $updateRETAILSTAFFTABLEClean

	$clearRETAILSTAFFTABLE = "UPDATE [dbo].[RETAILSTAFFTABLE] SET [EXTERNALIDENTITYID] = '',[EXTERNALIDENTITYSUBID] = '', EXTERNALIDENTITYNAME = '',EXTERNALIDENTITYALIAS = '' "
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement $clearRETAILSTAFFTABLE

	$updateRETAILSTAFFTABLE = "UPDATE [dbo].[RETAILSTAFFTABLE] SET [EXTERNALIDENTITYID] = '$AADTenantID',[EXTERNALIDENTITYSUBID] = '$AADUserID', EXTERNALIDENTITYNAME = '$UPN',EXTERNALIDENTITYALIAS = '$alias' WHERE STAFFID = '$DefaultRetailWorkerToMap' "
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement $updateRETAILSTAFFTABLE

	$updateRTSUserSID = "UPDATE USERINFO SET SID = '$rtsUserSID',NETWORKDOMAIN = '$identityprovider' WHERE NETWORKALIAS = 'RetailServerSystemAccount@dynamics.com' "
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement $updateRTSUserSID

	$updateRTSProfileInAxDB = "UPDATE RETAILTRANSACTIONSERVICEPROFILE set userid = 'RetailServerSystemAccount@dynamics.com',IDENTITYPROVIDER = '$identityprovider' "
	ExecuteSql -sqlServer $AxDbServer -database $AxDbName  -sqlStatement $updateRTSProfileInAxDB

	$updateRTSProfileInChnDB = "UPDATE ax.RETAILTRANSACTIONSERVICEPROFILE set userid = 'RetailServerSystemAccount@dynamics.com', IDENTITYPROVIDER = '$identityprovider' "
	ExecuteSql -sqlServer $AxDbServer -database $RetailChannelDbName  -sqlStatement $updateRTSProfileInChnDB
	
	$retailServerWebSite = Get-WebSite -Name $RetailWebSiteName
	$retailServerWebPath = [System.Environment]::ExpandEnvironmentVariables($retailServerWebSite.physicalPath)
	$retailServerWebConfigPath = (Join-Path $retailServerWebPath 'web.config')
	[xml] $retailServerWebConfigDoc = Get-Content $retailServerWebConfigPath
	if ($AADTenantID -and $retailServerWebConfigDoc.configuration.environment.tenant.id)
	{
		$retailServerWebConfigDoc.configuration.environment.tenant.id = $AADTenantID
	}
	$retailServerWebConfigDoc.Save($retailServerWebConfigPath)
	
	Write-Host "Restart iis to refresh configurations."
	IISRESET
	
	Write-Host "Please run a full sync to make sure all the changes will take effect."
	Start-Sleep -s 5
}
catch
{
	Write-Host 'Error when running RetailTenantUpdateTool'
    Write-Host ($global:error[0] | format-list * -f | Out-String)
	exit 1
}
# SIG # Begin signature block
# MIIdvgYJKoZIhvcNAQcCoIIdrzCCHasCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU0U8+kAIv1o5o4Db/Kddx8uw7
# VNagghhkMIIEwzCCA6ugAwIBAgITMwAAAJvgdDfLPU2NLgAAAAAAmzANBgkqhkiG
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
# bWrJUnMTDXpQzTGCBMQwggTAAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcg
# UENBIDIwMTECEzMAAABkR4SUhttBGTgAAAAAAGQwCQYFKw4DAhoFAKCB2DAZBgkq
# hkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGC
# NwIBFTAjBgkqhkiG9w0BCQQxFgQU66d47uWWCwqWAKPV5JsuCqjGm2IweAYKKwYB
# BAGCNwIBDDFqMGigNoA0AFIAZQB0AGEAaQBsAFQAZQBuAGEAbgB0AFUAcABkAGEA
# dABlAFQAbwBvAGwALgBwAHMAMaEugCxodHRwOi8vd3d3Lk1pY3Jvc29mdC5jb20v
# TWljcm9zb2Z0RHluYW1pY3MvIDANBgkqhkiG9w0BAQEFAASCAQCOyXg0BzDsNOn7
# iL6mly6HE5AIHqUjfnO/PLjl9ibELhif9uerTwqOmDH8z8YGaMCLOadecILJYYRL
# QYrzjeyZssMLJDlZHe1FOAx4+WeL2LQQlmT3xy/JV1LoaQZ59sbQSCv7xBJz65FH
# gZAwns+YDF+DI+NpJAt45JAh9XjSCYrT26pN7UnCVdyGwLzkLQxOcv3K8IbQ1Bjf
# nNbkfpRptRM88X43liLRSW1dKZrsaZvEVizY4mVBuImild46fZzy2xu3Mo9q8TYu
# IlhNNrX/OGPZrcBog7rsDTSPx6ZJ8dWTKXIK8jAvP8xro4F6QspQqfGBSCvhe+sl
# pAL10h8woYICKDCCAiQGCSqGSIb3DQEJBjGCAhUwggIRAgEBMIGOMHcxCzAJBgNV
# BAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
# HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAfBgNVBAMTGE1pY3Jvc29m
# dCBUaW1lLVN0YW1wIFBDQQITMwAAAJvgdDfLPU2NLgAAAAAAmzAJBgUrDgMCGgUA
# oF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcNMTYw
# ODA2MDg1MzIxWjAjBgkqhkiG9w0BCQQxFgQUbG7S07o62xeFMvScj5oJKJR3+nAw
# DQYJKoZIhvcNAQEFBQAEggEAGmSYr58VhrsWB/1Ny7cx78PwCvzWfgZDwwa9Li5g
# /3yGJ+LAaYMsY8LRkDiJKeZtzg9LehWhT0uY92oynjyCaJXddNU0z+2spKDjFTo4
# rmmG3Pahco+eutk4MlGwicB4SeqU8cbUhhuhYD6NgjDQY9QiXmdt//H+N3Noj9uz
# GWCszio/ml0fUnVr7rJ64DsKyprB8xM+T79n32x42/SMWT+54hdJv48b8z39IB12
# 8KT6tREdEz83QBSxZyvMH/HXz7oj8QlCXJEEuJ/oAefYu0VbtpZAzU2NZnaz1wx6
# +GKjjcJ9vgNu0BNjGsd2bi6UEh5GUxZ1zBkBTkOZz8LPOw==
# SIG # End signature block
