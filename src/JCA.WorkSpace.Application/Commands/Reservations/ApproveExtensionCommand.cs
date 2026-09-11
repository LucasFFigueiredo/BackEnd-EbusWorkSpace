using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class ApproveExtensionCommand : IRequest<bool>
{
    public Guid ApproverId { get; set; }
    public Guid ReservationId { get; set; }
    public int AdditionalMinutes { get; set; }
    public bool IsApproved { get; set; }
    public string? DenialReason { get; set; }
}