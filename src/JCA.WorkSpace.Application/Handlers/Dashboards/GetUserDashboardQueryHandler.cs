using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Application.Dtos.Dashboards;
using JCA.WorkSpace.Application.Queries.Dashboards;

namespace JCA.WorkSpace.Application.Handlers.Dashboards;

public class GetUserDashboardQueryHandler : IRequestHandler<GetUserDashboardQuery, DashboardUserDto>
{
    private readonly IReservationRepository _reservationRepository;

    public GetUserDashboardQueryHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<DashboardUserDto> Handle(GetUserDashboardQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetWithDetailsByUserIdAndPeriodAsync(request.UserId, request.StartDate, request.EndDate);

        var uniqueDates = reservations.Select(r => r.StartTime.Date).Distinct().Count();
        var avgDays = uniqueDates > 0 ? Math.Min(5, (int)Math.Ceiling(uniqueDates / 2.0)) : 0;

        var stats = new UserStatsDto
        {
            Total = reservations.Count(),
            Desks = reservations.Count(r => r.Space?.Type == SpaceType.Desk),
            Rooms = reservations.Count(r => r.Space?.Type == SpaceType.Room),
            CheckIns = reservations.Count(r => r.CheckInAt != null),
            AvgDaysPerWeek = avgDays
        };

        var daysOfWeekNames = new[] { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };
        var byDayOfWeek = Enumerable.Range(0, 7).Select(i => new DayMetricDto
        {
            Day = daysOfWeekNames[i],
            Reservas = reservations.Count(r => (int)r.StartTime.DayOfWeek == i)
        }).ToList();

        var byFloor = reservations
            .Where(r => r.Space != null)
            .GroupBy(r => r.Space!.Floor)
            .Select(g => new FloorMetricDto { Name = $"{g.Key}º Andar", Reservas = g.Count() })
            .OrderBy(x => x.Name)
            .ToList();

        return new DashboardUserDto
        {
            Stats = stats,
            ByDayOfWeek = byDayOfWeek,
            ByFloor = byFloor
        };
    }
}