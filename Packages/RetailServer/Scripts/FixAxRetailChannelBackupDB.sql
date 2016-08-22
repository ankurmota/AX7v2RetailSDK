declare @dbName nvarchar(1024)
declare @hostIP nvarchar(1024)
select @dbName = DB_Name()
select @hostIP = '$(dbServer)'

-- Patch up retail server redirection.
UPDATE ax.RETAILCHANNELPROFILEPROPERTY SET VALUE = '$(rsServerUrl)/Commerce' WHERE VALUE = 'http://localhost:35080/RetailServer/Commerce'
       
UPDATE [ax].[RETAILCONNDATABASEPROFILE] SET [SERVER] = @hostIP
UPDATE [ax].[RETAILCONNDATABASEPROFILE] SET [DATABASE] = @dbName

DECLARE @serverName Nvarchar(100)
SET @serverName = (select @@SERVERNAME)