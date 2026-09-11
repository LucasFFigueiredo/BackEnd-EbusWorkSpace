using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Application.Commands.Reservations;
using System.Text.Json;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class RequestExtensionCommandHandler : IRequestHandler<RequestExtensionCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestExtensionCommandHandler(IReservationRepository reservationRepository, IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RequestExtensionCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
        if (reservation == null || reservation.UserId != request.UserId)
            throw new InvalidOperationException("Reserva não encontrada ou você não tem permissão.");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = request.UserId,
            Action = "SOLICITACAO_EXTENSAO",
            EntityId = reservation.Id,
            Details = JsonSerializer.Serialize(new
            {
                Minutos = request.AdditionalMinutes,
                Justificativa = request.Justification,
                NovaDataFimEsperada = reservation.EndTime.AddMinutes(request.AdditionalMinutes)
            }),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync();
        return true;
    }
}