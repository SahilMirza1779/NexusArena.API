ALTER TABLE [dbo].[Bookings] 
ADD [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE [dbo].[BookingEquipments] 
ADD [IsReturned] BIT NOT NULL DEFAULT 0;