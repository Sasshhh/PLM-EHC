0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000DECLARE @AppId INT = 4198;
DECLARE @PropManagerId INT;
DECLARE @CEOId INT;

SELECT @PropManagerId = CAST(Value AS INT) FROM dbo.AppSettings WHERE [Key] = 'r_property_manager';
SELECT @CEOId = CAST(Value AS INT) FROM dbo.AppSettings WHERE [Key] = 's_ceo'; -- Wait, what is CEO key? It's 's_ceo' or something else? I'll just look up AppSettings.

DELETE FROM dbo.RoundRobinQueues
WHERE PropertyLeaseApplicationId = @AppId 
  AND ClerkId IN (
      SELECT CAST(Value AS INT) FROM dbo.AppSettings WHERE [Key] IN ('r_property_manager', 'u_ceo')
  );
