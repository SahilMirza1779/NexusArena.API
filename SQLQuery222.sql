-- 1. Pehle confirm karo Resource table me entry hai
SELECT * FROM [dbo].[Resources] WHERE [ArenaId] = 1;

-- 2. Ab TimeSlots add karo (ResourceId = 1 ke liye)
INSERT INTO [dbo].[TimeSlots] ([ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium])
VALUES (1, '18:00:00', '19:00:00', 1200.00, 0);

INSERT INTO [dbo].[TimeSlots] ([ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium])
VALUES (1, '19:00:00', '20:00:00', 1200.00, 0);