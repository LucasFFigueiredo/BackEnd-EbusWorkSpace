using MediatR;
using JCA.WorkSpace.Application.Dtos.Dashboards;

namespace JCA.WorkSpace.Application.Queries.Dashboards;

public class GetUserDashboardQuery : IRequest<DashboardUserDto>
{
    public Guid UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}