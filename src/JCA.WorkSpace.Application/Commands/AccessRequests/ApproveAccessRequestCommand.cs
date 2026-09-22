using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.AccessRequests;

public class ApproveAccessRequestCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid RequestId { get; set; }
}
