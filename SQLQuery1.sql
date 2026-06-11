-- 1. Pehle hum 4 Roles insert karenge
INSERT INTO [dbo].[Roles] ([RoleName]) 
VALUES 
('SuperAdmin'), 
('Owner'), 
('Receptionist'), 
('User');

-- 2. Ab hum har Role ke liye ek User account banayenge
-- (Note: Hum password ko simple '1234' rakh rahe hain testing ke liye)
INSERT INTO [dbo].[Users] ([RoleId], [FullName], [Email], [PasswordHash], [Phone], [IsActive])
VALUES 
(1, 'System Admin', 'admin@nexus.com', '1234', '9999999999', 1),
(2, 'Turf Owner', 'owner@nexus.com', '1234', '8888888888', 1),
(3, 'Front Desk Staff', 'staff@nexus.com', '1234', '7777777777', 1),
(4, 'Sahil Mirza', 'sahil@nexus.com', '1234', '6666666666', 1);