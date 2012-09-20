use [et_test_db]
GO
BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
GO 
--
-- Script To Create dbo.Book Table In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.Book Table'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

CREATE TABLE [dbo].[Book] (
   [Book_ID] [int] IDENTITY (1, 1) NOT NULL,
   [Author_ID] [int] NULL,
   [Name] [char] (10) NULL,
   [Date] [datetime] NULL
)
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
   ALTER TABLE [dbo].[Book] ADD CONSTRAINT [PK_Book] PRIMARY KEY CLUSTERED ([Book_ID])
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.Book Table Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.Book Table'
END
GO


--
-- Script To Create dbo.FirstUDF Function In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.FirstUDF Function'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

SET QUOTED_IDENTIFIER OFF
GO
exec('CREATE FUNCTION dbo.FirstUDF(@sValue as varchar(8000))
RETURNS int AS
BEGIN 
	DECLARE @iValue int

	SELECT @iValue = case when @sValue like ''%[^0-9]%'' then NULL else convert(int, @sValue) end
	
	RETURN @iValue
END')
GO
SET QUOTED_IDENTIFIER ON
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.FirstUDF Function Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.FirstUDF Function'
END
GO

--
-- Script To Create dbo.SecondUDF Function In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.SecondUDF Function'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

SET QUOTED_IDENTIFIER OFF
GO
exec('CREATE FUNCTION dbo.SecondUDF ()
RETURNS int AS
BEGIN 
DECLARE @iValue int
	SELECT @iValue = dbo.FirstUDF(''225'')	
RETURN @iValue	
END')
GO
SET QUOTED_IDENTIFIER ON
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.SecondUDF Function Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.SecondUDF Function'
END
GO

--
-- Script To Create dbo.View1 View In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.View1 View'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

SET QUOTED_IDENTIFIER OFF
GO
exec('CREATE VIEW dbo.View1 AS SELECT dbo.FirstUDF(''255'') AS S1')
GO
SET QUOTED_IDENTIFIER ON
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.View1 View Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.View1 View'
END
GO

--
-- Script To Create dbo.View2 View In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.View2 View'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

SET QUOTED_IDENTIFIER OFF
GO
exec('CREATE VIEW dbo.View2
AS 
SELECT * FROM dbo.View1')
GO
SET QUOTED_IDENTIFIER ON
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.View2 View Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.View2 View'
END
GO

--
-- Script To Create dbo.FIRSTPRC Procedure In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.FIRSTPRC Procedure'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

SET QUOTED_IDENTIFIER OFF
GO
exec('CREATE PROCEDURE dbo.FIRSTPRC
as
if @@error <>0 
begin
	raiserror (''Procedure FIRSTPRC Error'',16,1)
end')
GO
SET QUOTED_IDENTIFIER ON
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.FIRSTPRC Procedure Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.FIRSTPRC Procedure'
END
GO

--
-- Script To Create dbo.SECONDPRC Procedure In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.SECONDPRC Procedure'
GO

SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL ON
GO

SET NUMERIC_ROUNDABORT OFF
GO

SET QUOTED_IDENTIFIER OFF
GO
exec('CREATE PROCEDURE dbo.SECONDPRC
as
EXEC dbo.FIRSTPRC
if @@error <>0 
begin
	raiserror (''Procedure SECONDPRC Error'',16,1)
end')
GO
SET QUOTED_IDENTIFIER ON
GO

IF @@ERROR <> 0
   IF @@TRANCOUNT = 2 ROLLBACK TRANSACTION
GO

IF @@TRANCOUNT = 2
BEGIN
   PRINT 'dbo.SECONDPRC Procedure Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.SECONDPRC Procedure'
END
GO

--
-- Script To Create dbo.Book Table In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
--
-- Please backup Largo.et_test_db before executing this script
--


BEGIN TRANSACTION
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO

PRINT 'Creating dbo.Book Table'
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
   PRINT 'dbo.Book Table Added Successfully'
   COMMIT TRANSACTION
END ELSE
BEGIN
   PRINT 'Failed To Add dbo.Book Table'
END
GO

--
-- Script To Update Data In Largo.et_test_db
-- Generated Saturday, December 1, 2007, at 04:59 AM
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
