using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Domain.Interfaces.Repositories;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Reservation>> GetConflictsAsync(Guid spaceId, DateTime start, DateTime end);
    Task<IEnumerable<Reservation>> GetByBatchIdAsync(Guid batchId);
    Task<IEnumerable<Reservation>> GetPendingForWorkerAsync(DateTime threshold);
    Task UpdateBatchStatusAsync(Guid batchId, ReservationStatus status, Guid userId);
    Task<IEnumerable<Reservation>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<int> ProcessRoomNoShowsAsync(DateTime thresholdTime);
    Task<int> ProcessDeskNoShowsAsync();
    Task<IEnumerable<Reservation>> GetAllWithDetailsAsync();
    Task<Reservation?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Reservation>> GetWithDetailsByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Reservation>> GetWithDetailsByUserIdAndPeriodAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<Reservation?> GetCurrentBySpaceIdAsync(Guid spaceId, DateTime now);
    Task<IEnumerable<Reservation>> GetAllWithDetailsAsync(ReservationStatus? status = null);
    Task CompleteExpiredReservationsAsync(DateTime referenceTime);
}