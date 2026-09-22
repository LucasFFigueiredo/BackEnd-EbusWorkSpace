using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.AccessRequests;

public class RejectAccessRequestCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid RequestId { get; set; }
}
