using MediatR;
using JCA.WorkSpace.Application.Dtos.Dashboards;

namespace JCA.WorkSpace.Application.Queries.Dashboards;

public class GetDashboardMetricsQuery : IRequest<DashboardGeneralDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}