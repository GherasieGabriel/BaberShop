using BarberShop.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Data;

public class BarberShopDbContext(DbContextOptions<BarberShopDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Barber> Barbers => Set<Barber>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ClientNotificationSettings> ClientNotificationSettings => Set<ClientNotificationSettings>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("CLIENT");
            entity.HasKey(e => e.ClientId);
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.MemberSince).HasColumnName("member_since").HasColumnType("date");
        });

        modelBuilder.Entity<Barber>(entity =>
        {
            entity.ToTable("BARBER");
            entity.HasKey(e => e.BarberId);
            entity.Property(e => e.BarberId).HasColumnName("barber_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(50);
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(50);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.HireDate).HasColumnName("hire_date").HasColumnType("date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("SERVICE");
            entity.HasKey(e => e.ServiceId);
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.BaseDuration).HasColumnName("base_duration");
            entity.Property(e => e.BasePrice).HasColumnName("base_price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("APPOINTMENT");
            entity.HasKey(e => e.AppointmentId);
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.BarberId).HasColumnName("barber_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.StartDateTime).HasColumnName("start_datetime");
            entity.Property(e => e.EndDateTime).HasColumnName("end_datetime");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Appointments)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Barber)
                .WithMany(b => b.Appointments)
                .HasForeignKey(e => e.BarberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("PRODUCT");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<ClientNotificationSettings>(entity =>
        {
            entity.ToTable("CLIENT_NOTIFICATION_SETTINGS");
            entity.HasKey(e => e.ClientId);
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.NotifyEmail).HasColumnName("notify_email");
            entity.Property(e => e.NotifySms).HasColumnName("notify_sms");

            entity.HasOne(e => e.Client)
                .WithOne(c => c.NotificationSettings)
                .HasForeignKey<ClientNotificationSettings>(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("REVIEW");
            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.BarberId).HasColumnName("barber_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");

            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.Reviews)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Reviews)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Barber)
                .WithMany(b => b.Reviews)
                .HasForeignKey(e => e.BarberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("PAYMENT");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(e => e.Method).HasColumnName("method").HasMaxLength(20);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.TransactionRef).HasColumnName("transaction_ref").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.Payments)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}