SET IDENTITY_INSERT [dbo].[Roles] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 1)
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleId], [RoleName]) VALUES (1, 'SuperAdmin');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 2)
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleId], [RoleName]) VALUES (2, 'Owner');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 3)
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleId], [RoleName]) VALUES (3, 'Receptionist');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 4)
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleId], [RoleName]) VALUES (4, 'User');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 5)
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleId], [RoleName]) VALUES (5, 'Customer');
END

SET IDENTITY_INSERT [dbo].[Roles] OFF;