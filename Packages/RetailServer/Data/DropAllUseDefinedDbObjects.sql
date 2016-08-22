/* Drop all non-system stored procs */
DECLARE @name VARCHAR(128)
DECLARE @objId INT
DECLARE @SQL VARCHAR(254)

SELECT TOP 1 @name = [name], @objId = id  FROM sysobjects WHERE [type] = 'P' AND category = 0 ORDER BY [name]

WHILE @name is not null AND len(@name) > 0
BEGIN
    SELECT @SQL = 'DROP PROCEDURE [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT @SQL  
	SELECT @name = NULL, @objId = 0  
	SELECT TOP 1 @name = [name], @objId = id  FROM sysobjects WHERE [type] = 'P' AND category = 0 ORDER BY [name]
END
GO

/* Drop all views */
DECLARE @name VARCHAR(128)
DECLARE @objId INT
DECLARE @SQL VARCHAR(254)

SELECT TOP 1 @name = [name], @objId = id FROM sysobjects WHERE [type] = 'V' AND category = 0 ORDER BY [name]

WHILE @name is not null AND len(@name) > 0
BEGIN
    SELECT @SQL = 'DROP VIEW [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT @SQL	
	SELECT @name = NULL, @objId = 0
    SELECT TOP 1 @name = [name], @objId = id  FROM sysobjects WHERE [type] = 'V' AND category = 0 ORDER BY [name]
END
GO

/* Drop all functions */
DECLARE @name VARCHAR(128)
DECLARE @objId INT
DECLARE @SQL VARCHAR(254)

SELECT TOP 1 @name = [name], @objId = id FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 ORDER BY [name]

WHILE @name IS NOT NULL AND len(LTRIM(RTRIM(@name))) > 0
BEGIN
    SELECT @SQL = 'DROP FUNCTION [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT @SQL
	SELECT @name = NULL, @objId = 0
    SELECT TOP 1 @name = [name], @objId = id FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 ORDER BY [name]
	PRINT @name
END
GO

/* Drop all Foreign Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @tableSchema VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT TOP 1 @name = TABLE_NAME, @tableSchema = TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME

WHILE @name is not null
BEGIN
	SELECT @constraint = NULL
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint IS NOT NULL
    BEGIN
        SELECT @SQL = 'ALTER TABLE [' + @tableSchema + '].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint) +']'
        EXEC (@SQL)
        PRINT @SQL
		SELECT @constraint = NULL		
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT TOP 1 @name = TABLE_NAME, @tableSchema = TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME
END
GO

/* Drop all Primary Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @tableSchema VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT TOP 1 @name = TABLE_NAME, @tableSchema = TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME

WHILE @name IS NOT NULL
BEGIN
	SELECT @constraint = NULL
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint is not null
    BEGIN
        SELECT @SQL = 'ALTER TABLE [' + @tableSchema + '].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint)+']'
        EXEC (@SQL)
        PRINT @SQL
		SELECT @constraint = NULL
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT TOP 1 @name = TABLE_NAME, @tableSchema = TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME
END
GO

/* Drop all tables */
DECLARE @name VARCHAR(128)
DECLARE @objId INT
DECLARE @SQL VARCHAR(254)

SELECT TOP 1 @name = [name], @objId = id FROM sysobjects WHERE [type] = 'U' AND category = 0 ORDER BY [name]

WHILE @name IS NOT NULL
BEGIN	
	SELECT @SQL = 'DROP TABLE [' + OBJECT_SCHEMA_NAME(@objId) + '].[' + RTRIM(@name) +']'
	EXEC (@SQL)
	PRINT @SQL	
	SELECT @name = NULL, @objId = 0
    SELECT TOP 1 @name = [name], @objId = id FROM sysobjects WHERE [type] = 'U' AND category = 0 ORDER BY [name]	
END
GO