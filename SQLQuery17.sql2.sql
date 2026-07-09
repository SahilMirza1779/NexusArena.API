UPDATE [dbo].[Arenas]
SET [HourlyRegularPrice] = 800.00
WHERE [HourlyRegularPrice] = 0.00 OR [HourlyRegularPrice] IS NULL;