SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF (SELECT OBJECT_ID('ax.ISVRETAILSTOREHOURSTABLE')) IS NULL 
BEGIN
	CREATE TABLE [ax].[ISVRETAILSTOREHOURSTABLE](
		[RECID] [bigint] NOT NULL,
		[DAY] [int] NOT NULL DEFAULT ((0)),
		[OPENTIME] [int] NOT NULL DEFAULT ((0)),
		[CLOSINGTIME] [int] NOT NULL DEFAULT ((0)),
		[RETAILSTORETABLE] [bigint] NOT NULL DEFAULT ((0)),
		CONSTRAINT [I_104425RECID] PRIMARY KEY CLUSTERED 
	(
		[RECID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [ax].[ISVRETAILSTOREHOURSTABLE]  WITH CHECK ADD CHECK  (([RECID]<>(0)))
END
GO

-- grant Read/Insert/Update/Delete permission to DataSyncUserRole so CDX can function
GRANT SELECT ON OBJECT::[ax].[ISVRETAILSTOREHOURSTABLE] TO [DataSyncUsersRole]
GO
GRANT INSERT ON OBJECT::[ax].[ISVRETAILSTOREHOURSTABLE] TO [DataSyncUsersRole]
GO
GRANT UPDATE ON OBJECT::[ax].[ISVRETAILSTOREHOURSTABLE] TO [DataSyncUsersRole]
GO
GRANT DELETE ON OBJECT::[ax].[ISVRETAILSTOREHOURSTABLE] TO [DataSyncUsersRole]
GO

IF (SELECT OBJECT_ID('crt.ISVRETAILSTOREHOURSVIEW')) IS NOT NULL 
BEGIN
	DROP VIEW [crt].[ISVRETAILSTOREHOURSVIEW]
END
GO

CREATE VIEW [crt].[ISVRETAILSTOREHOURSVIEW] AS
(
	SELECT
		sdht.DAY, 
		sdht.OPENTIME,
		sdht.CLOSINGTIME,
		sdht.RECID, 
		rst.STORENUMBER
	FROM [ax].[ISVRETAILSTOREHOURSTABLE] sdht
	INNER JOIN [ax].RetailStoreTable rst ON rst.RECID = sdht.RETAILSTORETABLE
)
GO

GRANT SELECT ON OBJECT::[crt].[ISVRETAILSTOREHOURSVIEW] TO [UsersRole];
GO
