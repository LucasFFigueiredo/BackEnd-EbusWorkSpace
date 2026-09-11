using MediatR;
using JCA.WorkSpace.Application.Dtos.Reservations;

namespace JCA.WorkSpace.Application.Queries.Reservations;

public class GetUpcomingReservationsQuery : IRequest<IEnumerable<ReservationDto>>
{
    public bool OnlyRooms { get; set; } = true;
}