using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class ApproveExtensionCommand : IRequest<bool>
{
    public Guid ApproverId { get; set; }
    public Guid ExtensionRequestId { get; set; }
}