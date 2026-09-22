using MediatR;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class RejectExtensionCommand : IRequest<bool>
{
    public Guid ExtensionRequestId { get; set; }
}
