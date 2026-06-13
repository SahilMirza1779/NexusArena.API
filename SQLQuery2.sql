-- 1. Ek Sport Category Add Karein
INSERT INTO [dbo].[SportCategories] ([Name], [Icon]) 
VALUES ('Box Cricket', 'cricket-icon.png');

-- 2. Ek Naya Arena (Turf) Add Karein (OwnerId = 2)
INSERT INTO [dbo].[Arenas] ([OwnerId], [Name], [Location], [City], [IsActive]) 
VALUES (2, 'Dream Box Cricket', 'Vesu', 'Surat', 1);

-- 3. Us Arena me ek Resource (Ground) Add Karein (ArenaId=1, CategoryId=1)
INSERT INTO [dbo].[Resources] ([ArenaId], [CategoryId], [ResourceName], [Capacity]) 
VALUES (1, 1, 'Turf A - Premium', 14);

-- 4. Us Turf ke liye ek Time Slot Add Karein (Subah 10 se 11 baje)
INSERT INTO [dbo].[TimeSlots] ([ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium]) 
VALUES (1, '10:00:00', '11:00:00', 1200.00, 1);