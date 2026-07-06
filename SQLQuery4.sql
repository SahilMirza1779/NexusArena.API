-- Identity Insert ON kar rahe hain
SET IDENTITY_INSERT [dbo].[SportCategories] ON;

-- Sahi column names (Name aur Icon) ke sath data insert kar rahe hain
INSERT INTO [dbo].[SportCategories] ([CategoryId], [Name], [Icon]) 
VALUES 
(1, 'Box Cricket', 'fas fa-table-tennis'),
(2, 'Pool / Snooker', 'fas fa-bullseye');

-- Data insert hone ke baad wapas OFF kar diya
SET IDENTITY_INSERT [dbo].[SportCategories] OFF;