using MediatR;
using System.Text.Json;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Application.Queries.Reservations;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class GetExtensionRequestsQueryHandler : IRequestHandler<GetExtensionRequestsQuery, IEnumerable<ExtensionRequestDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IReservationRepository _reservationRepository;

    public GetExtensionRequestsQueryHandler(IAuditLogRepository auditLogRepository, IReservationRepository reservationRepository)
    {
        _auditLogRepository = auditLogRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<IEnumerable<ExtensionRequestDto>> Handle(GetExtensionRequestsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogRepository.GetPagedAsync(1, 50);
        var requestsLogs = logs.Where(l => l.Action == "SOLICITACAO_EXTENSAO").ToList();

        var result = new List<ExtensionRequestDto>();

        foreach (var log in requestsLogs)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(log.EntityId);
            if (reservation == null) continue;

            int minutes = 0;
            string justification = "Sem justificativa";

            if (!string.IsNullOrEmpty(log.Details))
            {
                try
                {
                    using var doc = JsonDocument.Parse(log.Details);
                    minutes = doc.RootElement.GetProperty("Minutos").GetInt32();
                    justification = doc.RootElement.GetProperty("Justificativa").GetString() ?? justification;
                }
                catch { }
            }

            result.Add(new ExtensionRequestDto
            {
                ReservationId = log.EntityId,
                UserName = reservation.User?.Name ?? "Desconhecido",
                SpaceName = reservation.Space?.Name ?? "Sala Removida",
                RequestedMinutes = minutes,
                Justification = justification,
                RequestedAt = log.CreatedAt
            });
        }

        return result
            .GroupBy(r => r.ReservationId)
            .Select(g => g.OrderByDescending(x => x.RequestedAt).First());
    }
}