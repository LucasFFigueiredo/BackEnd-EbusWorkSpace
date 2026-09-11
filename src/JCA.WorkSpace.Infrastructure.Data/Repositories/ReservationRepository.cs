using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Contexts;

namespace JCA.WorkSpace.Infrastructure.Data.Repositories;

public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(WorkSpaceContext context) : base(context) { }

    public async Task<IEnumerable<Reservation>> GetConflictsAsync(Guid spaceId, DateTime start, DateTime end)
    {
        return await _context.Reservations
            .Include(r => r.User)
            .Where(r => r.SpaceId == spaceId
                     && r.Status != ReservationStatus.Canceled
                     && r.Status != ReservationStatus.NoShow
                     && r.StartTime < end
                     && r.EndTime > start)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByBatchIdAsync(Guid batchId)
        => await _context.Reservations
            .Where(r => r.BatchId == batchId)
            .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetPendingForWorkerAsync(DateTime threshold)
    {
        return await _context.Reservations
            .Include(r => r.Space)
            .Where(r => r.Status == ReservationStatus.Pending
                     && r.CheckInAt == null
                     && r.StartTime <= threshold)
            .ToListAsync();
    }

    public async Task UpdateBatchStatusAsync(Guid batchId, ReservationStatus status)
    {
        await _context.Reservations
            .Where(r => r.BatchId == batchId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, status));
    }

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();
    }

    public async Task UpdateBatchStatusAsync(Guid batchId, ReservationStatus status, Guid userId)
    {
        await _context.Reservations
            .Where(r => r.BatchId == batchId
                     && r.UserId == userId
                     && r.Status == ReservationStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, status));
    }

    public async Task<IEnumerable<Reservation>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Reservations
            .Where(r => r.StartTime >= startDate && r.EndTime <= endDate)
            .ToListAsync();
    }

    public async Task<int> ProcessRoomNoShowsAsync(DateTime thresholdTime)
    {
        return await _context.Reservations
            .Where(r => r.Space!.Type == SpaceType.Room
                     && r.Status == ReservationStatus.Pending
                     && r.CheckInAt == null
                     && r.StartTime <= thresholdTime)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReservationStatus.NoShow));
    }

    public async Task<int> ProcessDeskNoShowsAsync()
    {
        return await _context.Reservations
            .Where(r => r.Space!.Type == SpaceType.Desk
                     && r.Status == ReservationStatus.Pending
                     && r.CheckInAt == null
                     && r.StartTime <= DateTime.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReservationStatus.NoShow));
    }

    public async Task<IEnumerable<Reservation>> GetAllWithDetailsAsync()
    {
        return await _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.User)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reservation>> GetWithDetailsByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.User)
            .Where(r => r.StartTime >= startDate && r.EndTime <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetWithDetailsByUserIdAndPeriodAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Reservations
            .Include(r => r.Space)
            .Where(r => r.UserId == userId && r.StartTime >= startDate && r.EndTime <= endDate)
            .ToListAsync();
    }

    public async Task<Reservation?> GetCurrentBySpaceIdAsync(Guid spaceId, DateTime targetTime)
    {
        var toleranceTime = targetTime.AddMinutes(120);

        return await _context.Reservations
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.SpaceId == spaceId &&
                                      r.StartTime <= toleranceTime &&
                                      r.EndTime >= targetTime &&
                                      r.Status != ReservationStatus.Canceled &&
                                      r.Status != ReservationStatus.NoShow);
    }

    public async Task<IEnumerable<Reservation>> GetAllWithDetailsAsync(ReservationStatus? status = null)
    {
        var query = _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.User)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task CompleteExpiredReservationsAsync(DateTime referenceTime)
    {
        var expiredReservations = await _context.Reservations
            .Where(r => r.Status == ReservationStatus.CheckedIn && r.EndTime <= referenceTime)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
        }
    }
}