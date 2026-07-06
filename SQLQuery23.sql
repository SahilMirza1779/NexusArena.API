DELETE FROM [dbo].[Reviews];

SET IDENTITY_INSERT [dbo].[Reviews] ON;

INSERT INTO [dbo].[Reviews] 
(
    [ReviewId], [UserId], [ArenaId], [Rating], [Comment], [CreatedAt]
) 
VALUES 
(8, 4, 1, 5, 'm', '2026-06-25 15:54:33'),
(10, 4, 1, 1, 'zzzzz', '2026-06-25 16:12:26'),
(11, 4, 1, 5, 'l', '2026-06-27 00:34:18'),
(12, 4, 1, 5, 'mm', '2026-06-29 10:30:01'),
(14, 4, 1, 5, 'nn', '2026-07-03 23:34:19'),
(15, 4, 1, 4, 'nn', '2026-07-03 23:34:28'),
(16, 4, 1, 5, 'zub', '2026-07-06 10:55:00');

SET IDENTITY_INSERT [dbo].[Reviews] OFF;