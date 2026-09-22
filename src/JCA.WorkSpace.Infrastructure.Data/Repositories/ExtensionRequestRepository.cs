using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JCA.WorkSpace.Infrastructure.Data.Repositories;

public class ExtensionRequestRepository : BaseRepository<ExtensionRequest>, IExtensionRequestRepository
{
    public ExtensionRequestRepository(WorkSpaceContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExtensionRequest>> GetPendingRequestsAsync()
    {
        return await _dbSet
            .Include(x => x.User)
            .Include(x => x.Reservation)
                .ThenInclude(r => r.Space)
            .Where(x => x.Status == ExtensionRequestStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasPendingRequestAsync(Guid reservationId)
    {
        return await _dbSet.AnyAsync(x => x.ReservationId == reservationId && x.Status == ExtensionRequestStatus.Pending);
    }
}
