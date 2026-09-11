using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Contexts;

namespace JCA.WorkSpace.Infrastructure.Data.Repositories;

public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(WorkSpaceContext context) : base(context) { }

    public async Task<IEnumerable<AuditLog>> GetByEntityIdAsync(Guid entityId)
        => await _context.AuditLogs
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetPagedAsync(int page, int pageSize)
        => await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int limit = 50)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAccessRequestsLogsAsync()
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.Action == "Solicitação de Acesso Enviada")
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}