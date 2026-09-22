using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Infrastructure.Data.Contexts;

public class WorkSpaceContext : DbContext
{
    public WorkSpaceContext(DbContextOptions<WorkSpaceContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Space> Spaces { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AccessRequest> AccessRequests { get; set; }
    public DbSet<ExtensionRequest> ExtensionRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.HasPostgresEnum<UserProfile>();
        modelBuilder.HasPostgresEnum<SpaceType>();
        modelBuilder.HasPostgresEnum<ReservationStatus>();
        modelBuilder.HasPostgresEnum<AccessRequestStatus>();
        modelBuilder.HasPostgresEnum<ExtensionRequestStatus>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkSpaceContext).Assembly);
        modelBuilder.SeedData();

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(u => u.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.Email)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.HasIndex(u => u.Email)
                  .IsUnique()
                  .HasDatabaseName("idx_users_email");

            entity.Property(u => u.Sector)
                  .HasMaxLength(50);

            entity.Property(u => u.Profile)
                  .IsRequired();

            entity.Property(u => u.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.Property(u => u.LastLoginAt)
                  .IsRequired(false);
        });

        modelBuilder.Entity<Space>(entity =>
        {
            entity.ToTable("Spaces");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(s => s.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(s => s.Type)
                  .IsRequired();

            entity.Property(s => s.Floor)
                  .IsRequired();

            entity.Property(s => s.Capacity)
                  .IsRequired();

            entity.Property(s => s.Sector)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(s => s.IsBlocked)
                  .HasDefaultValue(false);

            entity.Property(s => s.MaintenanceReason)
                  .HasMaxLength(255)
                  .IsRequired(false);

            entity.Property(s => s.MaintenanceUntil)
                  .IsRequired(false);

            entity.Property(s => s.Resources)
                  .HasColumnType("jsonb")
                  .IsRequired(false);

            entity.Property(s => s.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.HasIndex(s => s.Floor)
                  .HasDatabaseName("idx_spaces_floor");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(r => r.BatchId)
                  .IsRequired();

            entity.Property(r => r.Status)
                  .IsRequired();

            entity.Property(r => r.StartTime)
                  .IsRequired();

            entity.Property(r => r.EndTime)
                  .IsRequired();

            entity.Property(r => r.CheckInAt)
                  .IsRequired(false);

            entity.Property(r => r.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.Space)
                  .WithMany()
                  .HasForeignKey(r => r.SpaceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.SpaceId, r.StartTime })
                  .HasDatabaseName("idx_reservations_space_date");

            entity.HasIndex(r => new { r.Status, r.StartTime })
                  .HasDatabaseName("idx_reservations_status_date");

            entity.HasIndex(r => r.BatchId)
                  .HasDatabaseName("idx_reservations_batchid");

            entity.HasIndex(r => r.UserId)
                  .HasDatabaseName("idx_reservations_userid");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(a => a.Action)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(a => a.EntityId)
                  .IsRequired();

            entity.Property(a => a.Details)
                  .HasColumnType("jsonb")
                  .IsRequired(false);

            entity.Property(a => a.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(a => a.EntityId)
                  .HasDatabaseName("idx_auditlogs_entityid");

            entity.HasIndex(a => a.Action)
                  .HasDatabaseName("idx_auditlogs_action");
        });

        modelBuilder.Entity<AccessRequest>(entity =>
        {
            entity.ToTable("AccessRequests");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(a => a.RequestedProfile)
                  .IsRequired();

            entity.Property(a => a.Status)
                  .IsRequired();

            entity.Property(a => a.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.Property(a => a.UpdatedAt)
                  .IsRequired(false);

            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => a.UserId)
                  .HasDatabaseName("idx_accessrequests_userid");

            entity.HasIndex(a => a.Status)
                  .HasDatabaseName("idx_accessrequests_status");
        });

        modelBuilder.Entity<ExtensionRequest>(entity =>
        {
            entity.ToTable("ExtensionRequests");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.RequestedMinutes)
                  .IsRequired();

            entity.Property(e => e.Status)
                  .IsRequired();

            entity.Property(e => e.Justification)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                  .IsRequired(false);

            entity.HasOne(e => e.Reservation)
                  .WithMany()
                  .HasForeignKey(e => e.ReservationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ReservationId)
                  .HasDatabaseName("idx_extensionrequests_reservationid");
                  
            entity.HasIndex(e => e.Status)
                  .HasDatabaseName("idx_extensionrequests_status");
        });
    }
}