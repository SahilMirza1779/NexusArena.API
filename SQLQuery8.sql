UPDATE [dbo].[Arenas] 
SET IsActive = 1 
WHERE IsActive IS NULL OR IsActive = 0;