DELETE FROM [dbo].[TimeSlots];
DELETE FROM [dbo].[Bookings];
DELETE FROM [dbo].[Resources];

SET IDENTITY_INSERT [dbo].[SportCategories] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[SportCategories] WHERE CategoryId = 1)
BEGIN
    INSERT INTO [dbo].[SportCategories] ([CategoryId], [Name], [Icon]) 
    VALUES (1, 'Box Cricket', 'fas fa-table-tennis');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[SportCategories] WHERE CategoryId = 2)
BEGIN
    INSERT INTO [dbo].[SportCategories] ([CategoryId], [Name], [Icon]) 
    VALUES (2, 'Pool / Snooker', 'fas fa-bullseye');
END

SET IDENTITY_INSERT [dbo].[SportCategories] OFF;

SET IDENTITY_INSERT [dbo].[Resources] ON;

INSERT INTO [dbo].[Resources] 
(
    [ResourceId], [ArenaId], [CategoryId], [ResourceName], 
    [Capacity], [BasePricePerHour], [ResourceType], [IsActive]
) 
VALUES 
(1, 1, 1, 'Turf A - Premium', 14, 1200.00, 'Turf', 1),
(2, 1, 2, 'Box 2', 14, 0.00, 'Box Cricket', 1),
(3, 1, 2, 'Pool B', 4, 500.00, 'Pool', 1),
(12, 1, 1, 'Premium Box Cricket', 0, 0.00, 'Box Football, Bowling, Pool', 1),
(16, 1, 1, 'box cricket 2', 0, 1500.00, 'box criket', 1),
(17, 1, 1, 'sahil box', 0, 1400.00, 'box criket', 1),
(18, 1, 1, 'zubair box', 0, 4500.00, 'Box Cricket', 1),
(19, 1, 1, 'hussain box 2', 0, 0.00, 'Box Football', 1),
(1014, 1, 1, 'Adajan_drift', 0, 0.00, 'Box Cricket', 1),
(1015, 1, 1, 'Adajan_drift', 0, 0.00, 'Box Football', 1),
(1016, 1, 1, 'Adajan_drift', 0, 0.00, 'Box Cricket', 1),
(1018, 1, 1, 'hussainbox1', 0, 0.00, 'Box Cricket, Box Football', 1),
(1019, 1, 1, 'box11', 0, 0.00, 'Box Football, Bowling', 1);

SET IDENTITY_INSERT [dbo].[Resources] OFF;