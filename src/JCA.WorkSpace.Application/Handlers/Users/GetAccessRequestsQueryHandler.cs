using System.Text.Json;
using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Users;
using JCA.WorkSpace.Application.Queries.Users;

namespace JCA.WorkSpace.Application.Handlers.Users;

public class GetAccessRequestsQueryHandler : IRequestHandler<GetAccessRequestsQuery, IEnumerable<UserAccessRequestDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAccessRequestsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IEnumerable<UserAccessRequestDto>> Handle(GetAccessRequestsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogRepository.GetAccessRequestsLogsAsync();

        var result = new List<UserAccessRequestDto>();

        foreach (var log in logs)
        {
            string requestedProfile = "Desconhecido";

            if (!string.IsNullOrEmpty(log.Details))
            {
                try
                {
                    using var doc = JsonDocument.Parse(log.Details);
                    var msg = doc.RootElement.GetProperty("Mensagem").GetString();
                    requestedProfile = msg?.Replace("Solicitou o perfil: ", "") ?? "Desconhecido";
                }
                catch { }
            }

            var currentProfile = log.User?.Profile.ToString() ?? "Desconhecido";

            if (currentProfile.Equals(requestedProfile, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(new UserAccessRequestDto
            {
                UserId = log.UserId ?? Guid.Empty,
                UserName = log.User?.Name ?? "Usuário Deletado",
                UserEmail = log.User?.Email ?? "Sem e-mail",
                CurrentProfile = currentProfile,
                RequestedProfile = requestedProfile,
                RequestedAt = log.CreatedAt
            });
        }

        return result
            .GroupBy(r => r.UserId)
            .Select(g => g.OrderByDescending(x => x.RequestedAt).First());
    }
}