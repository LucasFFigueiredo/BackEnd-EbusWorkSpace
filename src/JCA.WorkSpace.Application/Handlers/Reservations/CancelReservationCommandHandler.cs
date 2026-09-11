using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Repositories;
using MediatR;
using System.Text.Json;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUserRepository _userRepository;


    public CancelReservationCommandHandler(
        IReservationRepository reservationRepository, 
        IUnitOfWork unitOfWork, 
        IAuditLogRepository auditLogRepository,
        IUserRepository userRepository
        )
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        if (!request.ReservationId.HasValue && !request.BatchId.HasValue)
            throw new ArgumentException("É necessário informar o ID da Reserva ou o ID do Lote para cancelar.");

        if (request.BatchId.HasValue)
        {
            await _reservationRepository.UpdateBatchStatusAsync(request.BatchId.Value, ReservationStatus.Canceled, request.UserId);
            return true;
        }

        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId!.Value);

        if (reservation == null)
            throw new InvalidOperationException("Reserva não encontrada.");

        var requester = await _userRepository.GetByIdAsync(request.UserId);
        bool isSuperUser = requester != null && (requester.Profile == UserProfile.Admin || requester.Profile == UserProfile.Facilities);

        if (reservation.UserId != request.UserId && !isSuperUser)
        {
            throw new InvalidOperationException("Você só pode cancelar as suas próprias reservas.");
        }

        bool isOwner = reservation.UserId == request.UserId;

        if (!isOwner && !isSuperUser)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para cancelar uma reserva de outro colaborador.");
        }

        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.AwaitingApproval)
        {
            throw new InvalidOperationException($"Não é possível cancelar. O status atual da reserva é {reservation.Status}.");
        }

        reservation.Status = ReservationStatus.Canceled;

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = request.UserId,
            Action = "Cancelamento de Reserva",
            EntityId = reservation.Id,
            Details = JsonSerializer.Serialize(new
            {
                Mensagem = request.BatchId.HasValue ? $"Cancelamento em lote: {request.BatchId}" : "Cancelamento unitário"
            }),
            CreatedAt = DateTime.UtcNow
        });

        await _reservationRepository.UpdateAsync(reservation);
        await _unitOfWork.CommitAsync();

        return true;
    }
}