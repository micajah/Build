IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'et_temp') DROP DATABASE [et_temp]
GO

CREATE DATABASE [et_temp]  ON (NAME = N'et_temp_schema', FILENAME = N'C:\Temp\et_temp_Data.mdf' , SIZE = 28, FILEGROWTH = 10%) LOG ON (NAME = N'et_temp_schema_Log', FILENAME = N'C:\Temp\et_temp_log.ldf' , SIZE = 285, FILEGROWTH = 10%) COLLATE SQL_Latin1_General_CP1_CI_AS
GO
exec sp_dboption N'et_temp', N'autoclose', N'false'
GO
exec sp_dboption N'et_temp', N'bulkcopy', N'false'
GO
exec sp_dboption N'et_temp', N'trunc. log', N'false'
GO
exec sp_dboption N'et_temp', N'torn page detection', N'false'
GO
exec sp_dboption N'et_temp', N'read only', N'false'
GO
exec sp_dboption N'et_temp', N'dbo use', N'false'
GO
exec sp_dboption N'et_temp', N'single', N'false'
GO
exec sp_dboption N'et_temp', N'autoshrink', N'false'
GO
exec sp_dboption N'et_temp', N'ANSI null default', N'false'
GO
exec sp_dboption N'et_temp', N'recursive triggers', N'false'
GO
exec sp_dboption N'et_temp', N'ANSI nulls', N'false'
GO
exec sp_dboption N'et_temp', N'concat null yields null', N'false'
GO
exec sp_dboption N'et_temp', N'cursor close on commit', N'false'
GO
exec sp_dboption N'et_temp', N'default to local cursor', N'false'
GO
exec sp_dboption N'et_temp', N'quoted identifier', N'false'
GO
exec sp_dboption N'et_temp', N'ANSI warnings', N'false'
GO
exec sp_dboption N'et_temp', N'auto create statistics', N'true'
GO
exec sp_dboption N'et_temp', N'auto update statistics', N'true'
GO
if( ( (@@microsoftversion / power(2, 24) = 8) and (@@microsoftversion & 0xffff >= 724) ) or ( (@@microsoftversion / power(2, 24) = 7) and (@@microsoftversion & 0xffff >= 1082) ) ) exec sp_dboption N'et_temp', N'db chaining', N'false'
GO 

use [et_temp]
GO

if not exists (select * from dbo.sysusers where name = N'guest' and hasdbaccess = 1)
	EXEC sp_grantdbaccess N'guest'
GO

