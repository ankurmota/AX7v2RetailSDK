-- Configuration variables
DECLARE @dt_RetryInterval           DATETIME = '00:00:01'; -- Retry every second
DECLARE @i_MaxNumberOfRetries       INT = 1200; -- Timeout after 20 minutes

-- Internal variables
DECLARE @i_ReturnCode               INT = 1; -- Failed
DECLARE @i_NumberOfTablesIndexed    INT = 0;
DECLARE @i_CurrentRetryCount        INT = 0;
DECLARE @i_NumberOfTables           INT;
DECLARE @FullTextIndexTableNames    AS TABLE (NAME NVARCHAR(257));

-- Start full text catalog rebuild
ALTER FULLTEXT CATALOG COMMERCEFULLTEXTCATALOG REBUILD

-- Get list of tables with fulltext indexes
INSERT INTO @FullTextIndexTableNames
    SELECT [s].NAME + '.' + [t].NAME
        FROM sys.fulltext_indexes fti
        INNER JOIN sys.tables t ON [t].[object_id] = [fti].[object_id]
        INNER JOIN sys.schemas s ON [s].[schema_id] = [t].[schema_id]

SELECT @i_NumberOfTables = COUNT(*) FROM @FullTextIndexTableNames;

WHILE @i_CurrentRetryCount < @i_MaxNumberOfRetries
BEGIN
    
    -- Get number of tables with completed index population
    SELECT @i_NumberOfTablesIndexed = COUNT(*)
        FROM @FullTextIndexTableNames t
        WHERE OBJECTPROPERTY(object_id(t.NAME), 'TableFulltextPopulateStatus') = 0; -- Idle
    
    -- Check if all tables have been completed
    IF @i_NumberOfTablesIndexed < @i_NumberOfTables
    BEGIN
        -- Throttling
        WAITFOR DELAY @dt_RetryInterval;
    END
    ELSE BEGIN
        -- Exit loop and return success
        SET @i_ReturnCode = 0;
        BREAK
    END
    
    SET @i_CurrentRetryCount = @i_CurrentRetryCount + 1;
END

SELECT 'Completed or timed-out (see below status) after ' + CONVERT(VARCHAR(10), @i_CurrentRetryCount) + ' retries. Result: ' + CONVERT(VARCHAR(1), @i_ReturnCode); -- If timed out @i_ReturnCode will have default value 1

-- Log status for each table:
--     0 = Idle 
--     1 = Full population in progress
--     2 = Paused 
--     3 = Throttled 
--     4 = Recovering 
--     5 = Shutdown 
--     6 = Incremental population in progress 
--     7 = Building index 
--     8 = Disk is full. Paused.
--     9 = Change tracking
SELECT 
    t.NAME AS TABLENAME,
    OBJECTPROPERTY(object_id(t.NAME), 'TableFulltextPopulateStatus') AS [STATUS]
    FROM @FullTextIndexTableNames t
