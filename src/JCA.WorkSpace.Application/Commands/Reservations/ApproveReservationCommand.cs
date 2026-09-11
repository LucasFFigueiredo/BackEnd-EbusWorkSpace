using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class ApproveReservationCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid ReservationId { get; set; }

    [JsonIgnore]
    public Guid ApproverId { get; set; }
    public bool IsApproved { get; set; }
    public string? Justification { get; set; }
}