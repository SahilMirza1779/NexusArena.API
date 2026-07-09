SET IDENTITY_INSERT [dbo].[SportCategories] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[SportCategories] WHERE CategoryId = 1)
BEGIN
    INSERT INTO [dbo].[SportCategories] ([CategoryId], [Name], [Icon]) 
    VALUES (1, 'Box Cricket', 'fas fa-table-tennis');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[SportCategories] WHERE CategoryId = 2)
BEGIN
    INSERT INTO [dbo].[SportCategories] ([CategoryId], [Name], [Icon]) 
    VALUES (2, 'Cricket', 'fas fa-baseball-bat-ball');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[SportCategories] WHERE CategoryId = 3)
BEGIN
    INSERT INTO [dbo].[SportCategories] ([CategoryId], [Name], [Icon]) 
    VALUES (3, 'Football', 'fas fa-futbol');
END

SET IDENTITY_INSERT [dbo].[SportCategories] OFF;