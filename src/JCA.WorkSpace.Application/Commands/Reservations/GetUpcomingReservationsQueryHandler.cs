using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Application.Queries.Reservations;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class GetUpcomingReservationsQueryHandler : IRequestHandler<GetUpcomingReservationsQuery, IEnumerable<ReservationDto>>
{
    private readonly IReservationRepository _reservationRepository;

    public GetUpcomingReservationsQueryHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<IEnumerable<ReservationDto>> Handle(GetUpcomingReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetWithDetailsByPeriodAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(15));

        var activeReservations = reservations
            .Where(r => r.Status != ReservationStatus.Canceled && r.Status != ReservationStatus.NoShow)
            .Where(r => !request.OnlyRooms || r.Space?.Type == SpaceType.Room)
            .OrderBy(r => r.StartTime)
            .Select(r => new ReservationDto
            {
                Id = r.Id,
                SpaceId = r.SpaceId,
                SpaceName = r.Space?.Name ?? "Espaço Indisponível",
                UserName = r.User?.Name ?? "Desconhecido",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status.ToString(),
                CheckInAt = r.CheckInAt 
            });

        return activeReservations;
    }
}