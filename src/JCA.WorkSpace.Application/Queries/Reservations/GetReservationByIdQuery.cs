using MediatR;
using JCA.WorkSpace.Application.Dtos.Reservations;

namespace JCA.WorkSpace.Application.Queries.Reservations;

public class GetReservationByIdQuery : IRequest<ReservationDto?>
{
    public Guid Id { get; set; }
}