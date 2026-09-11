using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Contexts;

namespace JCA.WorkSpace.Infrastructure.Data.Repositories;

public class SpaceRepository : BaseRepository<Space>, ISpaceRepository
{
    public SpaceRepository(WorkSpaceContext context) : base(context) { }

    public async Task<IEnumerable<Space>> GetByFloorAsync(int floor)
            => await _context.Spaces
                .Where(s => s.Floor == floor)
                .OrderBy(s => s.Name)
                .ToListAsync();

    public async Task<IEnumerable<Space>> GetAvailableAsync(DateTime start, DateTime end, SpaceType type)
    {
        return await _context.Spaces
            .Where(s => s.Type == type && !s.IsBlocked)
            .Where(s => !s.MaintenanceUntil.HasValue || s.MaintenanceUntil.Value < DateOnly.FromDateTime(start))
            .Where(s => !_context.Reservations.Any(r =>
                r.SpaceId == s.Id &&
                r.Status == ReservationStatus.Pending &&
                r.StartTime < end &&
                r.EndTime > start))
            .ToListAsync();
    }

    public async Task SetMaintenanceAsync(Guid id, bool isBlocked, string? reason, DateOnly? until)
    {
        await _context.Spaces
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsBlocked, isBlocked)
                .SetProperty(p => p.MaintenanceReason, reason)
                .SetProperty(p => p.MaintenanceUntil, until));
    }

    public async Task<IEnumerable<Space>> GetInMaintenanceAsync()
    {
        return await _context.Spaces
            .Where(s => s.IsBlocked)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}