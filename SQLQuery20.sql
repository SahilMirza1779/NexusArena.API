ALTER TABLE [dbo].[Bookings] ADD [AmountPaid] DECIMAL(18, 2) NOT NULL DEFAULT 0;
ALTER TABLE [dbo].[Bookings] ADD [PaymentStatus] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[Bookings] ADD [TransactionId] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[Bookings] ADD [PaymentMode] NVARCHAR(50) NULL;