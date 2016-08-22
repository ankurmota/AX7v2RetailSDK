/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
CREATE PROCEDURE [dbo].[GETCARDPAYMENTRESULTBYRESULTACCESSCODE]
    @nvc_ServiceAccountId   [nvarchar](255),
    @id_ResultAccessCode    [uniqueidentifier]
AS
BEGIN

    SET NOCOUNT ON;

    DECLARE @i_ReturnCode                           INT;
    DECLARE @i_RowCount                             INT;
    DECLARE @i_TransactionIsOurs                    INT;
    DECLARE @i_Error                                INT;

    -- initializes the return code and assume the transaction is not ours by default
    SET @i_ReturnCode = 0;
    SET @i_TransactionIsOurs = 0;

    IF @@TRANCOUNT = 0
    BEGIN
        BEGIN TRANSACTION;

        SELECT @i_Error = @@ERROR;
        IF @i_Error <> 0
        BEGIN
            SET @i_ReturnCode = @i_Error;
            GOTO exit_label;
        END;

        SET @i_TransactionIsOurs = 1;
    END;
	
    -- SELECT CARDPAYMENTRESULT ONLY WHEN IT IS NOT RETRIEVED ALREADY
    SELECT [ENTRYID]
          ,[RECORDID]
          ,[RETRIEVED]
          ,[RESULTACCESSCODE]
          ,[RESULTDATA]
          ,[SERVICEACCOUNTID]
    FROM [dbo].[CARDPAYMENTRESULT] WITH(NOLOCK)
    WHERE [RESULTACCESSCODE] = @id_ResultAccessCode
      AND [SERVICEACCOUNTID] = @nvc_ServiceAccountId
      AND [RETRIEVED] = 0;

    SELECT @i_Error = @@ERROR;
    IF @i_Error <> 0
    BEGIN
        SET @i_ReturnCode = @i_Error;
        GOTO exit_label;
    END;

    IF @i_TransactionIsOurs = 1
    BEGIN
        COMMIT TRANSACTION;

        SET @i_Error = @@ERROR;
        IF @i_Error <> 0
        BEGIN
            SET @i_ReturnCode = @i_Error;
            GOTO exit_label;
        END;

        SET @i_TransactionIsOurs = 0;
    END;

exit_label:
    IF @i_TransactionIsOurs = 1
    BEGIN
        ROLLBACK TRANSACTION;
    END;
    RETURN @i_ReturnCode;
END;
GO
