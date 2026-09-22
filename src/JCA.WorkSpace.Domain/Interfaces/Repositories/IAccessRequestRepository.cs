using JCA.WorkSpace.Domain.Entities;

namespace JCA.WorkSpace.Domain.Interfaces.Repositories;

public interface IAccessRequestRepository : IRepository<AccessRequest>
{
    Task<IEnumerable<AccessRequest>> GetPendingRequestsAsync();
    Task<bool> HasPendingRequestAsync(Guid userId);
}
