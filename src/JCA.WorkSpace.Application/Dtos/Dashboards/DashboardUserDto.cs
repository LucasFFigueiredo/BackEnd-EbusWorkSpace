namespace JCA.WorkSpace.Application.Dtos.Dashboards;

public class DashboardUserDto
{
    public UserStatsDto Stats { get; set; } = new();
    public IEnumerable<DayMetricDto> ByDayOfWeek { get; set; } = new List<DayMetricDto>();
    public IEnumerable<FloorMetricDto> ByFloor { get; set; } = new List<FloorMetricDto>();
}

public class UserStatsDto
{
    public int Total { get; set; }
    public int Desks { get; set; }
    public int Rooms { get; set; }
    public int CheckIns { get; set; }
    public int AvgDaysPerWeek { get; set; }
}