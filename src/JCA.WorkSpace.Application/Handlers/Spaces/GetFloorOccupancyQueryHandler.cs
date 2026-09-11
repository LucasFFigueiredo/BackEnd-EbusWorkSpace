using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Spaces;
using JCA.WorkSpace.Application.Queries.Spaces;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class GetFloorOccupancyQueryHandler : IRequestHandler<GetFloorOccupancyQuery, IEnumerable<SpaceOccupancyDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IReservationRepository _reservationRepository;

    public GetFloorOccupancyQueryHandler(ISpaceRepository spaceRepository, IReservationRepository reservationRepository)
    {
        _spaceRepository = spaceRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<IEnumerable<SpaceOccupancyDto>> Handle(GetFloorOccupancyQuery request, CancellationToken cancellationToken)
    {
        var spaces = await _spaceRepository.GetByFloorAsync(request.Floor);

        var reservations = await _reservationRepository.GetWithDetailsByPeriodAsync(request.StartTime, request.EndTime);

        var result = new List<SpaceOccupancyDto>();

        foreach (var space in spaces)
        {
            var activeReservation = reservations.FirstOrDefault(r =>
                r.SpaceId == space.Id &&
                r.Status != ReservationStatus.Canceled &&
                r.Status != ReservationStatus.NoShow);

            result.Add(new SpaceOccupancyDto
            {
                SpaceId = space.Id,
                Name = space.Name,
                IsBlocked = space.IsBlocked || (space.MaintenanceUntil.HasValue && space.MaintenanceUntil.Value >= DateOnly.FromDateTime(request.StartTime)),
                MaintenanceReason = space.MaintenanceReason,
                IsOccupied = activeReservation != null,
                OccupantName = activeReservation?.User?.Name,
                OccupantId = activeReservation?.UserId
            });
        }

        return result.OrderBy(s => s.Name);
    }
}