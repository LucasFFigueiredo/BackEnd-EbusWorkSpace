using MediatR;
using JCA.WorkSpace.Application.Dtos.Reservations;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class CreateBatchReservationCommand : IRequest<ReservationResultDto>
{
    public Guid UserId { get; set; }

    public List<BatchReservationItem> Reservations { get; set; } = new();
}

public class BatchReservationItem
{
    public Guid SpaceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}