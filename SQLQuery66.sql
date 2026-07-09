SET IDENTITY_INSERT [dbo].[Users] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 1)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (1, 1, 'System Admin', 'admin@nexus.com', '$2a$11$xORa/5YKFslAfNgLO6a0eOGPN85cwU2dyxri0bskZ8uYaKVM5pqb.', '9999999999', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 2)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (2, 2, 'Turf Owner', 'owner@nexus.com', '$2a$11$5kRSEFxmASRR6DNia898VOmFJ8tZr6TXUNVscta4mQweRyWL6Dttu', '8888888888', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 3)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (3, 3, 'Front Desk Staff', 'staff@nexus.com', '$2a$11$t3j8r/v0DSwnd4F0IDyn7uhpjMr1OfwV2R5/RViNk/fPNOIpk1SoC', '7777777777', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 4)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (4, 4, 'Sahil Mirza', 'sahil@nexus.com', '$2a$11$2QAukbuonOrGD7naLBZfOujSC2D3kyvqghUnvBVmS8blu07LfX41K', '6666666666', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 5)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (5, 2, 'Sahil Mirza', 'sahilmirza@gmail.comm', '$2a$11$lY7O0p.S275SYODX4mJu3OLJo3.b.LtnQ0a84jwKjPfZc2q.1iLFe', '987654210', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 6)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (6, 4, 'Zubeir', 'zubeir@nexus.com', '$2a$11$VdVu4BW2cLzr1f/hClYqGuuIZtrvN0e.QaJg4SfKZfaxsA.gYT/zW', '9876543210', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 7)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (7, 2, 'Mirza', 'sahilmirza2779@gmail.com', '$2a$11$2ANfe8xPNjtDrEciBAMQ/.oq/u0QEArQmQhJlEhd5JzZHjV6Tkj9u', '9999999999', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 8)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (8, 2, 'Virat', 'selotzubeir69@gmail.com', '$2a$11$Up9tiZs/fFRxBlo7j.ldVOWMi7ABDoAT.UJ4pqdMHIOHp4L6N1OrO', '9999999999', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 9)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (9, 5, 'Hussain Mirza', 'walkin_639185025101884283@nexus.com', '$2a$11$QByUlMVGENbP.yDHwZYMFejXkwlMjk9c9ihKrglnzjECRj8N2GIpW', '9586273922', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 10)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (10, 5, 'Zubier Selot', 'walkin_639185079997414314@nexus.com', '$2a$11$UWuLNDPDJhMf/hilkQSYU.FLTRqtUqMMnmKf4xcoqRi/t8vh7o/iG8', '8160953678', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 11)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (11, 5, 'Mirza', 'walkin_639185086984091307@nexus.com', '$2a$11$ko70oihIsAnXrhdgD7bY1Ow6X8AjOHvbHsKRLAC4eT1d2LDB7/lIi', '6354961591', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 12)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (12, 5, 'Sahil Mirza', 'sahilmirza4400@gmail.com', '$2a$11$4bqcmx58jiH9MBsRHlf0SekqbzaseNFvLAY2zPUxoJwb/jdPMBRa.', '7359055058', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 1002)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (1002, 3, 'sahil mirza', 'sahil@gmail.com', 'hu1234', '5856584796', 1, NULL);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE UserId = 1003)
BEGIN
    INSERT INTO [dbo].[Users] ([UserId], [RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive], [BusinessName]) 
    VALUES (1003, 3, 'zubair', 'zubair@gmail.com', 'zubair1243', '4587952367', 1, NULL);
END

SET IDENTITY_INSERT [dbo].[Users] OFF;