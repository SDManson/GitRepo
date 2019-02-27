Create Procedure GetClass
@P1 nvarchar(5),@ReturnType nvarchar(10) OUTPUT
AS

Select @ReturnType = [dbo].[Wormholes].[Leads]  from Wormholes 
where 
[dbo].[Wormholes].[WormType] = '%@P1%'
return 
