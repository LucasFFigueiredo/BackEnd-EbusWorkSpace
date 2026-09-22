using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Contexts;

namespace JCA.WorkSpace.Infrastructure.Data.Repositories;

public class AccessRequestRepository : BaseRepository<AccessRequest>, IAccessRequestRepository
{
    public AccessRequestRepository(WorkSpaceContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AccessRequest>> GetPendingRequestsAsync()
    {
        return await _dbSet
            .Include(x => x.User)
            .Where(x => x.Status == AccessRequestStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasPendingRequestAsync(Guid userId)
    {
        return await _dbSet
            .AnyAsync(x => x.UserId == userId && x.Status == AccessRequestStatus.Pending);
    }
}
