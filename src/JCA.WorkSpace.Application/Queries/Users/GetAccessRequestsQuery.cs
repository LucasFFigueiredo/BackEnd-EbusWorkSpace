using MediatR;
using JCA.WorkSpace.Application.Dtos.Users;

namespace JCA.WorkSpace.Application.Queries.Users;

public class GetAccessRequestsQuery : IRequest<IEnumerable<UserAccessRequestDto>> { }