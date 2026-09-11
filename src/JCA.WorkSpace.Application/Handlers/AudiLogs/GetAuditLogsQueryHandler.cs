using AutoMapper;
using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.AuditLogs;
using JCA.WorkSpace.Application.Queries.AuditLogs;

namespace JCA.WorkSpace.Application.Handlers.AudiLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public GetAuditLogsQueryHandler(
        IAuditLogRepository auditLogRepository, 
        IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogRepository.GetRecentLogsAsync(request.Limit);
        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }
}