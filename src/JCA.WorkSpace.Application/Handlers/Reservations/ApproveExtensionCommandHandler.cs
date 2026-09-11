using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Domain.Entities;
using System.Text.Json;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class ApproveExtensionCommandHandler : IRequestHandler<ApproveExtensionCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveExtensionCommandHandler(IReservationRepository reservationRepository, IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ApproveExtensionCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
        if (reservation == null) throw new InvalidOperationException("Reserva não encontrada.");

        if (!request.IsApproved)
        {
            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = request.ApproverId,
                Action = "EXTENSAO_NEGADA",
                EntityId = reservation.Id,
                Details = JsonSerializer.Serialize(new { Mensagem = $"Pedido negado. Motivo: {request.DenialReason}" }),
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.CommitAsync();
            return true;
        }

        var proposedEndTime = reservation.EndTime.AddMinutes(request.AdditionalMinutes);
        var conflicts = await _reservationRepository.GetConflictsAsync(reservation.SpaceId, reservation.EndTime, proposedEndTime);

        if (conflicts.Any()) throw new InvalidOperationException("Não é possível aprovar a extensão. A sala já foi reservada logo em seguida.");

        reservation.EndTime = proposedEndTime;
        await _reservationRepository.UpdateAsync(reservation);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = request.ApproverId,
            Action = "EXTENSAO_APROVADA",
            EntityId = reservation.Id,
            Details = JsonSerializer.Serialize(new { Mensagem = $"Tempo estendido em {request.AdditionalMinutes} minutos." }),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync();
        return true;
    }
}