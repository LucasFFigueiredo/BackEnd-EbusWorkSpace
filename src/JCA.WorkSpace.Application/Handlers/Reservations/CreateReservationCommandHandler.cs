using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Reservations;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationCommandHandler(
        IReservationRepository reservationRepository,
        ISpaceRepository spaceRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _spaceRepository = spaceRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        if (request.StartTime >= request.EndTime)
        {
            throw new InvalidOperationException("O horário de início deve ser menor que o horário de término.");
        }

        var space = await _spaceRepository.GetByIdAsync(request.SpaceId);
        if (space == null) throw new InvalidOperationException("Espaço não encontrado.");
        if (space.IsBlocked) throw new InvalidOperationException($"Espaço bloqueado para manutenção: {space.MaintenanceReason}");

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) throw new InvalidOperationException("Usuário solicitante não encontrado.");

        bool isPrivilegedUser = user.Profile == UserProfile.Admin || user.Profile == UserProfile.Facilities;

        if (space.Type == SpaceType.Room)
        {
            var duration = (request.EndTime - request.StartTime).TotalHours;

            if (!isPrivilegedUser && duration > 3.0)
            {
                throw new InvalidOperationException("O tempo máximo permitido para reserva de salas é de 3 horas. Para períodos maiores, faça a reserva e solicite extensão.");
            }
        }

        var conflicts = await _reservationRepository.GetConflictsAsync(request.SpaceId, request.StartTime, request.EndTime);
        if (conflicts.Any())
        {
            var conflict = conflicts.First();
            var startStr = conflict.StartTime.ToString("HH:mm");
            var endStr = conflict.EndTime.ToString("HH:mm");

            throw new InvalidOperationException(
                $"Conflito! A sala já está reservada das {startStr} às {endStr}. Ajuste o seu horário e tente novamente."
            );
        }

        var initialStatus = space.RequiresApproval
            ? ReservationStatus.AwaitingApproval
            : ReservationStatus.Pending;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SpaceId = request.SpaceId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = initialStatus,
            CreatedAt = DateTime.UtcNow
        };

        await _reservationRepository.AddAsync(reservation);
        await _unitOfWork.CommitAsync();

        return reservation.Id;
    }
}