
﻿INSERT INTO [dbo].[Arenas] 
(
    [OwnerId], 
    [Name], 
    [Location], 
    [City], 
    [IsActive], 
    [HourlyRegularPrice], 
    [HourlyPeakPrice], 
    [HalfDayMorningPrice], 
    [HalfDayEveningPrice], 
    [FullDayPrice]
)
VALUES
(5, 'Sultania Turf', 'Rander', 'Surat', 1, 0.00, 0.00, 0.00, 0.00, 0.00),
(6, 'D Villa', 'D Villa, Near Pal RTO, Pal, Surat', 'Surat', 0, 0.00, 0.00, 0.00, 0.00, 0.00),
(7, 'Cricetto', 'Cricetto Box, Jahangirpura, Surat', 'Surat', 0, 0.00, 0.00, 0.00, 0.00, 0.00),
(8, 'pkk', 'Palm Box Cricket, Near Suman Vandna, Jahangirpura, Surat', 'Surat', 1, 0.00, 0.00, 0.00, 0.00, 0.00),
(1, 'Nrxas Sports Arena', 'Vesu', 'Surat', 1, 0.00, 0.00, 0.00, 0.00, 0.00),
(2, 'Dream Box Cricket', 'Vesu', 'Surat', 1, 1200.00, 1800.00, 5000.00, 7000.00, 10000.00);-- Pehle identity insert ON karo taaki hum custom ArenaId daal sakein


SET IDENTITY_INSERT [dbo].[Arenas] OFF;
﻿-- Identity Insert ON kar rahe hain
SET IDENTITY_INSERT [dbo].[SportCategories] ON;

-- Sahi column names (Name aur Icon) ke sath data insert kar rahe hain
INSERT INTO [dbo].[SportCategories] ([CategoryId], [Name], [Icon]) 
VALUES 
(1, 'Box Cricket', 'fas fa-table-tennis'),
(2, 'Pool / Snooker', 'fas fa-bullseye');

-- Data insert hone ke baad wapas OFF kar diya
SET IDENTITY_INSERT [dbo].[SportCategories] OFF; 
