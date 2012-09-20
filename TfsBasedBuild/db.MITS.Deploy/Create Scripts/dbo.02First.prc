use [et_temp]
GO


CREATE PROCEDURE dbo.FIRSTPRC
as
if @@error <>0 
begin
	raiserror ('Procedure FIRSTPRC Error',16,1)
end
GO
 