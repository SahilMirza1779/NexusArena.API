SET IDENTITY_INSERT [dbo].[TimeSlots] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[TimeSlots] WHERE SlotId = 1)
BEGIN
    INSERT INTO [dbo].[TimeSlots] ([SlotId], [ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium], [SportCategoryId], [FestivalName], [DiscountPercent]) 
    VALUES (1, 1, '10:00:00', '11:00:00', 1200.00, 1, NULL, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[TimeSlots] WHERE SlotId = 2)
BEGIN
    INSERT INTO [dbo].[TimeSlots] ([SlotId], [ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium], [SportCategoryId], [FestivalName], [DiscountPercent]) 
    VALUES (2, 1, '18:00:00', '19:00:00', 1200.00, 0, NULL, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[TimeSlots] WHERE SlotId = 3)
BEGIN
    INSERT INTO [dbo].[TimeSlots] ([SlotId], [ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium], [SportCategoryId], [FestivalName], [DiscountPercent]) 
    VALUES (3, 1, '19:00:00', '20:00:00', 1200.00, 0, NULL, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[TimeSlots] WHERE SlotId = 4)
BEGIN
    INSERT INTO [dbo].[TimeSlots] ([SlotId], [ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium], [SportCategoryId], [FestivalName], [DiscountPercent]) 
    VALUES (4, 12, '00:00:00', '00:00:00', 10000.00, 0, NULL, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[TimeSlots] WHERE SlotId = 5)
BEGIN
    INSERT INTO [dbo].[TimeSlots] ([SlotId], [ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium], [SportCategoryId], [FestivalName], [DiscountPercent]) 
    VALUES (5, 12, '02:00:00', '03:00:00', 1000.00, 0, NULL, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[TimeSlots] WHERE SlotId = 6)
BEGIN
    INSERT INTO [dbo].[TimeSlots] ([SlotId], [ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium], [SportCategoryId], [FestivalName], [DiscountPercent]) 
    VALUES (6, 12, '17:00:00', '12:00:00', 1000.00, 0, NULL, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[TimeSlots] WHERE SlotId = 7)
BEGIN
    INSERT INTO [dbo].[TimeSlots] ([SlotId], [ResourceId], [StartTime], [EndTime], [BasePrice], [IsPremium], [SportCategoryId], [FestivalName], [DiscountPercent]) 
    VALUES (7, 17, '02:00:00', '00:00:00', 900.00, 0, NULL, NULL, NULL);
END

SET IDENTITY_INSERT [dbo].[TimeSlots] OFF;