using JCA.WorkSpace.Domain.Entities;

namespace JCA.WorkSpace.Domain.Interfaces.Repositories;

public interface IExtensionRequestRepository : IRepository<ExtensionRequest>
{
    Task<IEnumerable<ExtensionRequest>> GetPendingRequestsAsync();
    Task<bool> HasPendingRequestAsync(Guid reservationId);
}
