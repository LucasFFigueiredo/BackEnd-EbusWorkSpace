using JCA.WorkSpace.Domain.Entities;

namespace JCA.WorkSpace.Domain.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByEntityIdAsync(Guid entityId);
    Task<IEnumerable<AuditLog>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int limit = 50);
    Task<IEnumerable<AuditLog>> GetAccessRequestsLogsAsync();
}