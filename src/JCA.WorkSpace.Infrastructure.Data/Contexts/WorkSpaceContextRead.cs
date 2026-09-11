using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;

namespace JCA.WorkSpace.Infrastructure.Data.Contexts;

public class WorkSpaceContextRead : DbContext
{
    public WorkSpaceContextRead(DbContextOptions<WorkSpaceContextRead> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Space> Spaces { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<Domain.Enums.UserProfile>();
        modelBuilder.HasPostgresEnum<Domain.Enums.SpaceType>();
        modelBuilder.HasPostgresEnum<Domain.Enums.ReservationStatus>();
    }

    public override int SaveChanges()
    {
        throw new InvalidOperationException("Violação de Arquitetura: Este contexto é exclusivo para LEITURA. Use o WorkSpaceContext para gravação.");
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Violação de Arquitetura: Este contexto é exclusivo para LEITURA. Use o WorkSpaceContext para gravação.");
    }
}