using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Dashboard;

public class DashboardTestsFixture
{
    public Mock<IReservationRepository> ReservationRepositoryMock { get; set; }
    public Mock<ISpaceRepository> SpaceRepositoryMock { get; set; }

    public DashboardTestsFixture()
    {
        ReservationRepositoryMock = new Mock<IReservationRepository>();
        SpaceRepositoryMock = new Mock<ISpaceRepository>();
    }

    public void ResetMocks()
    {
        ReservationRepositoryMock.Reset();
        SpaceRepositoryMock.Reset();
    }

    public List<Space> GenerateSpacesHistory()
    {
        return new List<Space>
        {
            new Space { Id = Guid.NewGuid(), Name = "Sala A", Type = SpaceType.Room, Floor = 2, IsBlocked = false },
            new Space { Id = Guid.NewGuid(), Name = "Sala B", Type = SpaceType.Room, Floor = 2, IsBlocked = true },
            new Space { Id = Guid.NewGuid(), Name = "Mesa 1", Type = SpaceType.Desk, Floor = 13, IsBlocked = false }
        };
    }

    public List<Reservation> GenerateReservationHistory(Guid? specificUserId = null, List<Space>? spaces = null)
    {
        var uid1 = specificUserId ?? Guid.NewGuid();
        var uid2 = Guid.NewGuid();

        var s1 = spaces?.Find(s => s.Name == "Sala A") ?? new Space { Id = Guid.NewGuid(), Name = "Sala A", Type = SpaceType.Room, Floor = 2, IsBlocked = false };
        var s2 = spaces?.Find(s => s.Name == "Mesa 1") ?? new Space { Id = Guid.NewGuid(), Name = "Mesa 1", Type = SpaceType.Desk, Floor = 13, IsBlocked = false };

        var now = DateTime.UtcNow;

        return new List<Reservation>
        {
            new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = uid1,
                SpaceId = s1.Id,
                Space = s1,
                Status = ReservationStatus.CheckedIn,
                StartTime = GetNextWeekday(now, DayOfWeek.Monday).AddHours(9),
                EndTime = GetNextWeekday(now, DayOfWeek.Monday).AddHours(11),
                CheckInAt = GetNextWeekday(now, DayOfWeek.Monday).AddHours(9).AddMinutes(5),
                User = new User { Id = uid1, Sector = "Engenharia" }
            },
            new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = uid1,
                SpaceId = s2.Id,
                Space = s2,
                Status = ReservationStatus.Pending,
                StartTime = GetNextWeekday(now, DayOfWeek.Tuesday).AddHours(10),
                EndTime = GetNextWeekday(now, DayOfWeek.Tuesday).AddHours(18),
                CheckInAt = null,
                User = new User { Id = uid1, Sector = "Engenharia" }
            },
            new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = uid2,
                SpaceId = s1.Id,
                Space = s1,
                Status = ReservationStatus.Canceled,
                StartTime = GetNextWeekday(now, DayOfWeek.Thursday).AddHours(14),
                EndTime = GetNextWeekday(now, DayOfWeek.Thursday).AddHours(15),
                CheckInAt = null,
                User = new User { Id = uid2, Sector = "Marketing" }
            },
            new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = uid1,
                SpaceId = s1.Id,
                Space = s1,
                Status = ReservationStatus.Completed,
                StartTime = GetNextWeekday(now, DayOfWeek.Friday).AddDays(-7).AddHours(10),
                EndTime = GetNextWeekday(now, DayOfWeek.Friday).AddDays(-7).AddHours(12),
                CheckInAt = GetNextWeekday(now, DayOfWeek.Friday).AddDays(-7).AddHours(10),
                User = new User { Id = uid1, Sector = "Engenharia" }
            }
        };
    }

    private DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7;
        return start.AddDays(daysToAdd).Date;
    }
}

[CollectionDefinition(nameof(DashboardFixtureCollection))]
public class DashboardFixtureCollection : ICollectionFixture<DashboardTestsFixture>
{
}
