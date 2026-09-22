using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Users;
using JCA.WorkSpace.Application.Queries.Users;

namespace JCA.WorkSpace.Application.Handlers.Users;

public class GetAccessRequestsQueryHandler : IRequestHandler<GetAccessRequestsQuery, IEnumerable<UserAccessRequestDto>>
{
    private readonly IAccessRequestRepository _accessRequestRepository;

    public GetAccessRequestsQueryHandler(IAccessRequestRepository accessRequestRepository)
    {
        _accessRequestRepository = accessRequestRepository;
    }

    public async Task<IEnumerable<UserAccessRequestDto>> Handle(GetAccessRequestsQuery request, CancellationToken cancellationToken)
    {
        var pendingRequests = await _accessRequestRepository.GetPendingRequestsAsync();

        return pendingRequests.Select(r => new UserAccessRequestDto
        {
            RequestId = r.Id,
            UserId = r.UserId,
            UserName = r.User.Name,
            UserEmail = r.User.Email,
            CurrentProfile = r.User.Profile.ToString(),
            RequestedProfile = r.RequestedProfile.ToString(),
            RequestedAt = r.CreatedAt
        });
    }
}