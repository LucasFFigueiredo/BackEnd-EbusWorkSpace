using FluentAssertions;
using JCA.WorkSpace.Application.Handlers.Dashboards;
using JCA.WorkSpace.Application.Queries.Dashboards;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Dashboard;

[Collection(nameof(DashboardFixtureCollection))]
public class GetUserDashboardQueryHandlerTests
{
    private readonly DashboardTestsFixture _fixture;
    private readonly GetUserDashboardQueryHandler _handler;

    public GetUserDashboardQueryHandlerTests(DashboardTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
        _handler = new GetUserDashboardQueryHandler(_fixture.ReservationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectStats_WhenUserHasHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var spaces = _fixture.GenerateSpacesHistory();
        var reservations = _fixture.GenerateReservationHistory(specificUserId: userId, spaces: spaces);

        var userReservations = reservations.Where(r => r.UserId == userId).ToList();

        var query = new GetUserDashboardQuery { UserId = userId, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(30) };

        _fixture.ReservationRepositoryMock
            .Setup(r => r.GetWithDetailsByUserIdAndPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(userReservations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Stats.Should().NotBeNull();

        result.Stats.Total.Should().Be(3);
        result.Stats.Desks.Should().Be(1);
        result.Stats.Rooms.Should().Be(2);
        result.Stats.CheckIns.Should().Be(2);
        
        result.Stats.AvgDaysPerWeek.Should().Be(2);

        result.ByFloor.Should().NotBeEmpty();
        var floor2 = result.ByFloor.FirstOrDefault(f => f.Name == "2º Andar");
        floor2.Should().NotBeNull();
        floor2!.Reservas.Should().Be(2);

        var floor13 = result.ByFloor.FirstOrDefault(f => f.Name == "13º Andar");
        floor13.Should().NotBeNull();
        floor13!.Reservas.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroedStats_WhenUserHasNoHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDashboardQuery { UserId = userId, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(30) };

        _fixture.ReservationRepositoryMock
            .Setup(r => r.GetWithDetailsByUserIdAndPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Domain.Entities.Reservation>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Stats.Should().NotBeNull();

        result.Stats.Total.Should().Be(0);
        result.Stats.Desks.Should().Be(0);
        result.Stats.Rooms.Should().Be(0);
        result.Stats.CheckIns.Should().Be(0);
        result.Stats.AvgDaysPerWeek.Should().Be(0);

        result.ByFloor.Should().BeEmpty();
        
        result.ByDayOfWeek.Should().HaveCount(7);
        result.ByDayOfWeek.All(d => d.Reservas == 0).Should().BeTrue();
    }
}
