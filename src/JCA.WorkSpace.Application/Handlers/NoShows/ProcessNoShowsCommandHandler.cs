using MediatR;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.NoShows;

namespace JCA.WorkSpace.Application.Handlers.NoShows;

public class ProcessNoShowsCommandHandler : IRequestHandler<ProcessNoShowsCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessNoShowsCommandHandler> _logger;

    public ProcessNoShowsCommandHandler(
        IReservationRepository reservationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessNoShowsCommandHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessNoShowsCommand request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        int noShowsCount = 0;

        if (request.ProcessRooms)
        {
            var roomThreshold = nowUtc.AddMinutes(-15);
            var pendingRooms = await _reservationRepository.GetPendingForWorkerAsync(roomThreshold);

            foreach (var res in pendingRooms.Where(r => r.Space?.Type == SpaceType.Room))
            {
                await ApplyNoShow(res);
                noShowsCount++;
            }
        }

        if (request.ProcessDesks)
        {
            var pendingDesks = await _reservationRepository.GetPendingForWorkerAsync(nowUtc);

            foreach (var res in pendingDesks.Where(r => r.Space?.Type == SpaceType.Desk))
            {
                await ApplyNoShow(res);
                noShowsCount++;
            }
        }

        if (noShowsCount > 0)
        {
            await _unitOfWork.CommitAsync();
            _logger.LogInformation($"[WORKER] Foram processados {noShowsCount} No-Shows automaticamente.");
        }

        return true;
    }

    private async Task ApplyNoShow(Reservation reservation)
    {
        reservation.Status = ReservationStatus.NoShow;

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = reservation.UserId,
            Action = "NO_SHOW_AUTOMATICO",
            EntityId = reservation.Id,
            Details = JsonSerializer.Serialize(new
            {
                Mensagem = $"Reserva cancelada automaticamente pelo Worker. Espaço: {reservation.Space?.Name}"
            }),
            CreatedAt = DateTime.UtcNow
        });
    }
}