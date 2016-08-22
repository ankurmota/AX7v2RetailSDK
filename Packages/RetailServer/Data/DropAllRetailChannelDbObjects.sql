/* Drop all non-system stored procs under schema crt and ax */
DECLARE @schemaCrt INT
DECLARE @schemaAx INT

SELECT @schemaCrt = schema_id FROM sys.schemas WHERE [NAME] = 'crt'
SELECT @schemaAx = schema_id FROM sys.schemas WHERE [NAME] = 'ax'

DECLARE @name NVARCHAR(128)
DECLARE @objId INT
DECLARE @SQL NVARCHAR(1024)

SELECT TOP 1 @name = [name], @objId = [object_id] FROM sys.procedures WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [create_date] DESC

WHILE @name is not null AND len(@name) > 0
BEGIN
    SELECT @SQL = 'DROP PROCEDURE [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT @SQL  
	SELECT @name = NULL, @objId = 0  
	SELECT TOP 1 @name = [name], @objId = [object_id] FROM sys.procedures WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [create_date] DESC
END
GO

/* Drop all views under schema crt and ax */
DECLARE @schemaCrt INT
DECLARE @schemaAx INT

SELECT @schemaCrt = schema_id FROM sys.schemas WHERE [NAME] = 'crt'
SELECT @schemaAx = schema_id FROM sys.schemas WHERE [NAME] = 'ax'

DECLARE @name NVARCHAR(128)
DECLARE @objId INT
DECLARE @SQL NVARCHAR(1024)

/* Order by id DESC to remove the later view first since there may be some dependency between different views */
SELECT TOP 1 @name = [name], @objId = [object_id] FROM sys.views WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [create_date] DESC

WHILE @name is not null AND len(@name) > 0
BEGIN
    SELECT @SQL = 'DROP VIEW [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT @SQL	
	SELECT @name = NULL, @objId = 0
    SELECT TOP 1 @name = [name], @objId = [object_id] FROM sys.views WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [create_date] DESC
END
GO

/* Drop all functions */
DECLARE @schemaCrt INT
DECLARE @schemaAx INT

SELECT @schemaCrt = schema_id FROM sys.schemas WHERE [NAME] = 'crt'
SELECT @schemaAx = schema_id FROM sys.schemas WHERE [NAME] = 'ax'

DECLARE @name NVARCHAR(128)
DECLARE @objId INT
DECLARE @SQL NVARCHAR(1024)

SELECT TOP 1 @name = [name], @objId = id FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND OBJECT_SCHEMA_NAME(id) IN ('crt','ax') ORDER BY [crdate] DESC

WHILE @name IS NOT NULL AND len(LTRIM(RTRIM(@name))) > 0
BEGIN
    SELECT @SQL = 'DROP FUNCTION [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT @SQL
	SELECT @name = NULL, @objId = 0
    SELECT TOP 1 @name = [name], @objId = id FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND OBJECT_SCHEMA_NAME(id) IN ('crt','ax') ORDER BY [crdate] DESC
	PRINT @name
END
GO

/* Drop all Foreign Key constraints  under schema crt and ax */
DECLARE @schemaCrt INT
DECLARE @schemaAx INT

SELECT @schemaCrt = schema_id FROM sys.schemas WHERE [NAME] = 'crt'
SELECT @schemaAx = schema_id FROM sys.schemas WHERE [NAME] = 'ax'

DECLARE @name NVARCHAR(128)
DECLARE @constraint NVARCHAR(254)
DECLARE @tableSchema NVARCHAR(254)
DECLARE @SQL NVARCHAR(1024)

SELECT TOP 1 @name = TABLE_NAME, @tableSchema = TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_SCHEMA IN ('crt','ax') ORDER BY TABLE_NAME

WHILE @name is not null
BEGIN
	SELECT @constraint = NULL
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_NAME = @name AND CONSTRAINT_SCHEMA IN ('crt','ax') ORDER BY CONSTRAINT_NAME)
    WHILE @constraint IS NOT NULL
    BEGIN
        SELECT @SQL = 'ALTER TABLE [' + @tableSchema + '].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint) +']'
        EXEC (@SQL)
        PRINT @SQL
		SELECT @constraint = NULL		
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name AND CONSTRAINT_SCHEMA IN ('crt','ax') ORDER BY CONSTRAINT_NAME)
    END
SELECT TOP 1 @name = TABLE_NAME, @tableSchema = TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_SCHEMA IN ('crt','ax') ORDER BY TABLE_NAME
END
GO

/* Drop all tables under schema crt and ax */
DECLARE @schemaCrt INT
DECLARE @schemaAx INT

SELECT @schemaCrt = schema_id FROM sys.schemas WHERE [NAME] = 'crt'
SELECT @schemaAx = schema_id FROM sys.schemas WHERE [NAME] = 'ax'

DECLARE @name NVARCHAR(128)
DECLARE @objId INT
DECLARE @SQL NVARCHAR(1024)

SELECT TOP 1 @name = [name], @objId = [object_id] FROM sys.tables WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [create_date] DESC

WHILE @name IS NOT NULL
BEGIN	
	SELECT @SQL = 'DROP TABLE [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
	EXEC (@SQL)
	PRINT @SQL	
	SELECT @name = NULL, @objId = 0
    SELECT TOP 1 @name = [name], @objId = [object_id] FROM sys.tables WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [create_date] DESC
END
GO

/* Drop all types under schema crt and ax */
DECLARE @schemaCrt INT
DECLARE @schemaAx INT

SELECT @schemaCrt = schema_id FROM sys.schemas WHERE [NAME] = 'crt'
SELECT @schemaAx = schema_id FROM sys.schemas WHERE [NAME] = 'ax'

DECLARE @name NVARCHAR(128)
DECLARE @schemaId INT
DECLARE @SQL NVARCHAR(1024)

/* Order by id DESC to remove the later type first since there may be some dependency between different types */
SELECT TOP 1 @name = [name], @schemaId = [schema_id] FROM sys.types WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [user_type_id] DESC

WHILE @name is not null AND len(@name) > 0
BEGIN
    SELECT @SQL = 'DROP TYPE [' + SCHEMA_NAME(@schemaId) + '].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT @SQL	
	SELECT @name = NULL, @schemaId = 0
    SELECT TOP 1 @name = [name], @schemaId = [schema_id]  FROM sys.types WHERE [schema_id] IN (@schemaCrt,@schemaAx) ORDER BY [user_type_id] DESC
END
GO

/* Drop retail full text search catalog */
IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = 'COMMERCEFULLTEXTCATALOG')
BEGIN
	DROP FULLTEXT CATALOG [COMMERCEFULLTEXTCATALOG]
END

/* Drop retail db roles */

IF EXISTS (SELECT 1 FROM sys.procedures WHERE [name] = 'DropDatabaseRoleExt')
BEGIN
	DROP PROCEDURE dbo.DropDatabaseRoleExt
END
GO

CREATE PROCEDURE dbo.DropDatabaseRoleExt
(
	@RoleName NVARCHAR(255)
)
AS BEGIN
	IF  EXISTS (SELECT * FROM dbo.sysusers WHERE name = @RoleName AND issqlrole = 1)
	BEGIN 
      DECLARE @RoleMemberName sysname	  
      /* Cursor to Loop in for Each Member have the Role Privilege and Drop RoleMember */
      DECLARE Member_Cursor CURSOR FOR
      SELECT [name]
      FROM dbo.sysusers
      WHERE UID IN (
            SELECT memberuid
            FROM dbo.sysmembers
            WHERE groupuid IN (
                  SELECT UID FROM dbo.sysusers WHERE [name] = @RoleName AND issqlrole = 1)) 
      OPEN Member_Cursor;
 
      FETCH NEXT FROM Member_Cursor INTO @RoleMemberName
 
      WHILE @@FETCH_STATUS = 0
      BEGIN 
            EXEC sp_droprolemember @rolename=@RoleName, @membername= @RoleMemberName 
            FETCH NEXT FROM Member_Cursor INTO @RoleMemberName
      END;
 
      CLOSE Member_Cursor;
      DEALLOCATE Member_Cursor;
      /* End Of Cursor */ 
	END
	/* Checking If Role Name Exists In Database */
	IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @RoleName AND TYPE = 'R')
	BEGIN
		DECLARE @SqlStatement NVARCHAR(1024)
		SELECT @SqlStatement = 'DROP ROLE ' + @RoleName
		EXEC sp_executesql @SqlStatement
	END
END
GO

EXEC dbo.DropDatabaseRoleExt 'DataSyncUsersRole'
EXEC dbo.DropDatabaseRoleExt 'ReportUsersRole'
EXEC dbo.DropDatabaseRoleExt 'db_executor'
EXEC dbo.DropDatabaseRoleExt 'PublishersRole'
EXEC dbo.DropDatabaseRoleExt 'UsersRole'
GO

IF EXISTS (SELECT 1 FROM sys.procedures WHERE [name] = 'DropDatabaseRoleExt')
BEGIN
	DROP PROCEDURE dbo.DropDatabaseRoleExt
END
GO

/* Drop retail schema crt, ax */
IF EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = 'crt')
BEGIN
	DROP SCHEMA crt
END
GO

IF EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = 'ax')
BEGIN
	DROP SCHEMA ax
END
GO