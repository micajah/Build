use [et_test_db]
GO
BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
GO 
--
-- Script To Update dbo.Book Table In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:52 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Updating dbo.Book Table'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.Book Table Updated Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Update dbo.Book Table'
END
GO


--
-- Script To Update dbo.Book Table In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:52 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Updating dbo.Book Table'
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
   IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = N'FK_Book_Author')
      ALTER TABLE [dbo].[Book] ADD CONSTRAINT [FK_Book_Author] FOREIGN KEY ([Author_ID]) REFERENCES [dbo].[Author] ([Author_ID])
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.Book Table Updated Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Update dbo.Book Table'
END
GO

--
-- Script To Update Data In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:52 AM
--
-- Please backup Largo.et_test_db before executing this script
--


SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL, XACT_ABORT ON
GO
SET NUMERIC_ROUNDABORT OFF
GO

BEGIN TRANSACTION

ALTER TABLE [dbo].[Author] DROP CONSTRAINT [PK_Author]
ALTER TABLE [dbo].[Author] DISABLE TRIGGER [AUTHOR_UPDATE]

SET IDENTITY_INSERT [dbo].[Author] ON
INSERT INTO [dbo].[Author]([Author_ID], [Name]) VALUES (1, N'T. Schevchenko')
SET IDENTITY_INSERT [dbo].[Author] OFF
ALTER TABLE [dbo].[Author] ADD CONSTRAINT [PK_Author] UNIQUE CLUSTERED ([Author_ID])
ALTER TABLE [dbo].[Author] ENABLE TRIGGER [AUTHOR_UPDATE]

COMMIT TRANSACTION
GO 
IF @@TRANCOUNT = 1
BEGIN
   COMMIT TRANSACTION
END
