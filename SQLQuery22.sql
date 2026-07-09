DELETE FROM [dbo].[Notifications];

SET IDENTITY_INSERT [dbo].[Notifications] ON;

INSERT INTO [dbo].[Notifications] 
(
    [NotificationId], [UserId], [Message], [Type], [IsSent], [CreatedAt]
) 
VALUES 
(1, 3, 'Hello How Are You Guys', 'System Broadcast', 1, '2026-06-30 12:13:15'),
(2, 3, 'How are you Bachhi', 'System Broadcast', 1, '2026-06-30 12:35:36');

SET IDENTITY_INSERT [dbo].[Notifications] OFF;