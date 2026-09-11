namespace JCA.WorkSpace.Application.Dtos.Dashboards;

public class DashboardGeneralDto
{
    public DashboardTotalsDto Totals { get; set; } = new();
    public IEnumerable<DayMetricDto> ByDayOfWeek { get; set; } = new List<DayMetricDto>();
    public IEnumerable<RoomMetricDto> TopRooms { get; set; } = new List<RoomMetricDto>();
    public IEnumerable<DepartmentMetricDto> ByDepartment { get; set; } = new List<DepartmentMetricDto>();
    public IEnumerable<FloorMetricDto> ByFloor { get; set; } = new List<FloorMetricDto>();
}

public class DashboardTotalsDto
{
    public int TotalReservations { get; set; }
    public int ActiveReservations { get; set; }
    public int CheckIns { get; set; }
    public int BlockedSpaces { get; set; }
}

public class DayMetricDto
{
    public string Day { get; set; } = string.Empty;
    public int Reservas { get; set; }
}

public class RoomMetricDto
{
    public string Name { get; set; } = string.Empty;
    public int Reservas { get; set; }
}

public class DepartmentMetricDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class FloorMetricDto
{
    public string Name { get; set; } = string.Empty;
    public int Reservas { get; set; }
}