use [et_temp]
GO


CREATE PROCEDURE dbo.SECONDPRC
as
EXEC dbo.FIRSTPRC
if @@error <>0 
begin
	raiserror ('Procedure SECONDPRC Error',16,1)
end
GO
  