using MediatR;
using JCA.WorkSpace.Application.Dtos.AuditLogs;

namespace JCA.WorkSpace.Application.Queries.AuditLogs;

public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLogDto>>
{
    public int Limit { get; set; } = 50;
}