using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Dtos.Reservations;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class CreateBatchReservationCommandHandler : IRequestHandler<CreateBatchReservationCommand, ReservationResultDto>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBatchReservationCommandHandler(
        IReservationRepository reservationRepository,
        ISpaceRepository spaceRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _spaceRepository = spaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationResultDto> Handle(CreateBatchReservationCommand request, CancellationToken cancellationToken)
    {
        var result = new ReservationResultDto { BatchId = Guid.NewGuid() };

        if (!request.Reservations.Any())
        {
            result.Errors.Add("Nenhuma reserva foi enviada no lote.");
            return result;
        }

        foreach (var item in request.Reservations)
        {
            if (item.StartTime >= item.EndTime)
            {
                result.Errors.Add($"Horário inválido ({item.StartTime:dd/MM HH:mm}). O Início deve ser antes do fim.");
                continue;
            }

            var space = await _spaceRepository.GetByIdAsync(item.SpaceId);
            if (space == null)
            {
                result.Errors.Add($"Espaço não encontrado para o dia {item.StartTime:dd/MM/yyyy}.");
                continue;
            }

            if (space.IsBlocked)
            {
                result.Errors.Add($"O espaço '{space.Name}' está em manutenção: {space.MaintenanceReason}");
                continue;
            }

            var conflicts = await _reservationRepository.GetConflictsAsync(item.SpaceId, item.StartTime, item.EndTime);

            if (conflicts.Any())
            {
                var occupier = conflicts.First().User?.Name ?? "Outro colaborador";

                result.Errors.Add($"Conflito no dia {item.StartTime:dd/MM/yyyy}. A mesa '{space.Name}' foi reservada por {occupier} pouco antes de você.");
                continue;
            }

            var initialStatus = space.RequiresApproval
                ? ReservationStatus.AwaitingApproval
                : ReservationStatus.Pending;

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                BatchId = result.BatchId,
                UserId = request.UserId,
                SpaceId = item.SpaceId,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Status = initialStatus,
                CreatedAt = DateTime.UtcNow
            };

            await _reservationRepository.AddAsync(reservation);
            result.SuccessfulCount++;
        }

        if (result.SuccessfulCount > 0)
        {
            await _unitOfWork.CommitAsync();
        }
        else
        {
            result.BatchId = Guid.Empty;
        }

        return result;
    }
}