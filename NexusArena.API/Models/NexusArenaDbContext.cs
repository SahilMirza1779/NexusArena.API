using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NexusArena.API.Models;

public partial class NexusArenaDbContext : DbContext
{
    public NexusArenaDbContext()
    {
    }

    public NexusArenaDbContext(DbContextOptions<NexusArenaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Arena> Arenas { get; set; }
    public virtual DbSet<ArenaSport> ArenaSports { get; set; }
    public virtual DbSet<Booking> Bookings { get; set; }
    public virtual DbSet<BookingEquipment> BookingEquipments { get; set; }
    public virtual DbSet<Equipment> Equipments { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<PendingArena> PendingArenas { get; set; }
    public virtual DbSet<Resource> Resources { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<SportCategory> SportCategories { get; set; }
    public virtual DbSet<TimeSlot> TimeSlots { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=NexusArenaDB;Integrated Security=True;Pooling=False;Encrypt=True;Trust Server Certificate=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Arena>(entity =>
        {
            entity.HasKey(e => e.ArenaId).HasName("PK__Arenas__F6F7E7A7109DE6F7");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.Property(e => e.HourlyRegularPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HourlyPeakPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HalfDayMorningPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HalfDayEveningPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FullDayPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Owner).WithMany(p => p.Arenas)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Arenas_Users");
        });

        modelBuilder.Entity<ArenaSport>(entity =>
        {
            entity.HasKey(e => e.ArenaSportId);

            entity.HasOne(d => d.Arena)
                .WithMany(p => p.ArenaSports)
                .HasForeignKey(d => d.ArenaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ArenaSports_Arenas");

            entity.HasOne(d => d.SportCategory)
                .WithMany(p => p.ArenaSports)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ArenaSports_Categories");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951AED6738B769");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentMode).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(255);

            entity.Property(e => e.BookingMode).HasMaxLength(50).HasDefaultValue("Hourly");
            entity.Property(e => e.TournamentPackage).HasMaxLength(100);

            entity.HasOne(d => d.Resource).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Resources");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Users");

            // 🚨 THE GHOST KILLER: EF Core ko force karo ki wo in properties ko bhool jaye
            entity.Ignore("TimeSlot");
            entity.Ignore("SlotId");
            entity.Ignore("TimeSlotSlotId");
        });

        modelBuilder.Entity<BookingEquipment>(entity =>
        {
            entity.HasKey(e => e.BookingEqId).HasName("PK__BookingE__AAFB2B77147F0851");

            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingEquipments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingEq_Bookings");

            entity.HasOne(d => d.Equipment).WithMany(p => p.BookingEquipments)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingEq_Equipments");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__34474479FA2A15D6");

            entity.Property(e => e.ItemName).HasMaxLength(100);
            entity.Property(e => e.PricePerItem).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Arena).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.ArenaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Equipments_Arenas");

            entity.HasOne(d => d.Category).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Equipments_Categories");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1220BDADC7");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsSent).HasDefaultValue(false);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38339D0908");

            entity.Property(e => e.AdvancePaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GatewayTransactionId).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PendingAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefundAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefundStatus)
                .HasMaxLength(50)
                .HasDefaultValue("N/A");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Bookings");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("PK__Resource__4ED1816F7C3A9A56");

            entity.Property(e => e.BasePricePerHour).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Capacity).HasDefaultValue(1);
            entity.Property(e => e.ResourceName).HasMaxLength(100);
            entity.Property(e => e.ResourceType).HasMaxLength(50);

            entity.HasOne(d => d.Arena).WithMany(p => p.Resources)
                .HasForeignKey(d => d.ArenaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resources_Arenas");

            entity.HasOne(d => d.Category).WithMany(p => p.Resources)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resources_Categories");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE843EA123");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Arena).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ArenaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Arenas");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A065F5D07");

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<SportCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__SportCat__19093A0B1E9B2E69");

            entity.Property(e => e.Icon).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__TimeSlot__0A124AAFBD2411BF");

            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsPremium).HasDefaultValue(false);

            entity.HasOne(d => d.Resource).WithMany(p => p.TimeSlots)
                .HasForeignKey(d => d.ResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimeSlots_Resources");

            // 🚨 EXTRA SAFETY: TimeSlot ko bhi bata do ki Bookings se koi lena dena nahi hai ab
            entity.Ignore("Bookings");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0EA70CEF");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder); // Typo fixed here
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}