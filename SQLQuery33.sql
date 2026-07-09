DELETE FROM [dbo].[PendingArenas];

SET IDENTITY_INSERT [dbo].[PendingArenas] ON;

INSERT INTO [dbo].[PendingArenas] 
(
    [Id], [OwnerName], [ArenaName], [Email], [Address], 
    [Latitude], [Longitude], [ImagePaths], [Status], [AppliedOn]
) 
VALUES 
(
    2, 'Rohit Sharma', 'Rayyan Cage', 'sahilmirza2779@gmail.com', 'Rayyan Cage, Rayyan School, Near Zainab Hospital, Rander, Surat', 
    21.20517069942759, 72.802134769131825, 
    '/uploads/arenas/45b90d81-291e-4229-be7d-bd225d764b3a_Palm 4.jpg,/uploads/arenas/31fdbeb8-2509-49bc-b7fd-3ad79f528f1f_Palm3.jpg,/uploads/arenas/d9368131-517c-4fde-8ff8-447761eb08ec_Palm2.jpg,/uploads/arenas/5b5a9b43-3722-41ea-88b0-b7d550a31ab6_Palm1.jpg', 
    'Approved', '2026-06-22 12:23:34'
),
(
    3, 'Zubeir Selot', 'D Villa', 'selotzubeir69@gmail.com', 'D Villa, Near Pal RTO, Pal, Surat', 
    21.180138964583616, 72.774285845182121, 
    '/uploads/arenas/0a258930-881f-445a-b211-886c2fcfb6a7_Palm 4.jpg,/uploads/arenas/b6466b4f-81da-47bf-a075-ebb68f187cc8_Palm3.jpg,/uploads/arenas/481db151-460a-4edb-9a70-abbb9f40bf6f_Palm2.jpg,/uploads/arenas/57cb5457-ffff-4889-94c7-4fac4e513277_Palm1.jpg', 
    'Approved', '2026-06-22 12:38:40'
),
(
    4, 'Mirza', 'Cricetto', 'sahilmirza2779@gmail.com', 'Cricetto Box, Jahangirpura, Surat', 
    21.247920039264134, 72.7940618552882, 
    '/uploads/arenas/1d6a5acb-2a3c-43ce-ac90-67273ca834ff_Palm 4.jpg,/uploads/arenas/e7c355e2-655e-4e34-9fd1-8f313b8b5682_Palm3.jpg,/uploads/arenas/ec9aced1-d9f2-424a-8310-ccf219c0cc67_Palm2.jpg,/uploads/arenas/867b01c4-af5c-4628-a62c-f16df8edbd37_Palm1.jpg', 
    'Approved', '2026-06-22 12:55:35'
),
(
    5, 'Virat', 'pkk', 'selotzubeir69@gmail.com', 'Palm Box Cricket, Near Suman Vandna, Jahangirpura, Surat', 
    21.173562795353494, 72.822230435132511, 
    '/uploads/arenas/1ac1d943-5d3a-4ebe-ae65-d7bbafa30d16_Palm 4.jpg,/uploads/arenas/403d0928-7880-43e3-964c-addc8d9f1bf0_Palm3.jpg,/uploads/arenas/7b9defe3-2442-4907-a688-7f8db574011c_Palm2.jpg', 
    'Approved', '2026-06-22 13:20:51'
);

SET IDENTITY_INSERT [dbo].[PendingArenas] OFF;