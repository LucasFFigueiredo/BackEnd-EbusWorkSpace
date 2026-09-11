using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Application.Dtos.Dashboards;
using JCA.WorkSpace.Application.Queries.Dashboards;

namespace JCA.WorkSpace.Application.Handlers.Dashboards;

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, DashboardGeneralDto>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISpaceRepository _spaceRepository;

    public GetDashboardMetricsQueryHandler(
        IReservationRepository reservationRepository,
        ISpaceRepository spaceRepository)
    {
        _reservationRepository = reservationRepository;
        _spaceRepository = spaceRepository;
    }

    public async Task<DashboardGeneralDto> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetWithDetailsByPeriodAsync(request.StartDate, request.EndDate);
        var allSpaces = await _spaceRepository.GetAllAsync();

        var now = DateTime.UtcNow;

        var totals = new DashboardTotalsDto
        {
            TotalReservations = reservations.Count(),
            ActiveReservations = reservations.Count(r => r.EndTime >= now && r.Status != ReservationStatus.Canceled),
            CheckIns = reservations.Count(r => r.CheckInAt != null),
            BlockedSpaces = allSpaces.Count(s => s.IsBlocked)
        };

        var daysOfWeekNames = new[] { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };
        var byDayOfWeek = Enumerable.Range(0, 7).Select(i => new DayMetricDto
        {
            Day = daysOfWeekNames[i],
            Reservas = reservations.Count(r => (int)r.StartTime.DayOfWeek == i)
        }).ToList();

        var topRooms = reservations
            .Where(r => r.Space != null && r.Space.Type == SpaceType.Room)
            .GroupBy(r => r.Space!.Name)
            .Select(g => new RoomMetricDto { Name = g.Key, Reservas = g.Count() })
            .OrderByDescending(x => x.Reservas)
            .Take(6)
            .ToList();

        var byDepartment = reservations
            .Where(r => r.User != null)
            .GroupBy(r => string.IsNullOrWhiteSpace(r.User!.Sector) ? "Sem time" : r.User.Sector)
            .Select(g => new DepartmentMetricDto { Name = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToList();

        var byFloor = reservations
            .Where(r => r.Space != null)
            .GroupBy(r => r.Space!.Floor)
            .Select(g => new FloorMetricDto { Name = $"{g.Key}º Andar", Reservas = g.Count() })
            .OrderBy(x => x.Name)
            .ToList();

        return new DashboardGeneralDto
        {
            Totals = totals,
            ByDayOfWeek = byDayOfWeek,
            TopRooms = topRooms,
            ByDepartment = byDepartment,
            ByFloor = byFloor
        };
    }
}