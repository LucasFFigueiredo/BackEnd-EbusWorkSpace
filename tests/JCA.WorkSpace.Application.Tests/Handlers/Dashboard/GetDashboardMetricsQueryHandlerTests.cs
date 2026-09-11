using FluentAssertions;
using JCA.WorkSpace.Application.Handlers.Dashboards;
using JCA.WorkSpace.Application.Queries.Dashboards;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Dashboard;

[Collection(nameof(DashboardFixtureCollection))]
public class GetDashboardMetricsQueryHandlerTests
{
    private readonly DashboardTestsFixture _fixture;
    private readonly GetDashboardMetricsQueryHandler _handler;

    public GetDashboardMetricsQueryHandlerTests(DashboardTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
        _handler = new GetDashboardMetricsQueryHandler(_fixture.ReservationRepositoryMock.Object, _fixture.SpaceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectMetrics_WhenDataExists()
    {
        // Arrange
        var spaces = _fixture.GenerateSpacesHistory();
        var reservations = _fixture.GenerateReservationHistory(spaces: spaces);
        
        var query = new GetDashboardMetricsQuery { StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(30) };

        _fixture.SpaceRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(spaces);

        _fixture.ReservationRepositoryMock
            .Setup(r => r.GetWithDetailsByPeriodAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(reservations);

        var now = DateTime.UtcNow;

        var expectedActive = reservations.Count(r => r.EndTime >= now && r.Status != Domain.Enums.ReservationStatus.Canceled);
        var expectedTotal = reservations.Count;
        var expectedCheckIns = reservations.Count(r => r.CheckInAt != null);
        var expectedBlocked = spaces.Count(s => s.IsBlocked);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Totals.Should().NotBeNull();
        result.Totals.TotalReservations.Should().Be(expectedTotal);
        result.Totals.CheckIns.Should().Be(expectedCheckIns);
        result.Totals.BlockedSpaces.Should().Be(expectedBlocked);

        result.Totals.ActiveReservations.Should().Be(2);

        result.ByDepartment.Should().NotBeEmpty();
        var engenharia = result.ByDepartment.FirstOrDefault(d => d.Name == "Engenharia");
        engenharia.Should().NotBeNull();
        engenharia!.Value.Should().Be(3);

        var mkt = result.ByDepartment.FirstOrDefault(d => d.Name == "Marketing");
        mkt.Should().NotBeNull();
        mkt!.Value.Should().Be(1);

        result.ByFloor.Should().NotBeEmpty();
        var floor2 = result.ByFloor.FirstOrDefault(f => f.Name == "2º Andar");
        floor2.Should().NotBeNull();
        floor2!.Reservas.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectDatesToRepository()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);
        var query = new GetDashboardMetricsQuery { StartDate = startDate, EndDate = endDate };

        _fixture.SpaceRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new System.Collections.Generic.List<Domain.Entities.Space>());
        _fixture.ReservationRepositoryMock.Setup(r => r.GetWithDetailsByPeriodAsync(startDate, endDate)).ReturnsAsync(new System.Collections.Generic.List<Domain.Entities.Reservation>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _fixture.ReservationRepositoryMock.Verify(r => r.GetWithDetailsByPeriodAsync(startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTopRoomsCorrectly()
    {
        // Arrange
        var spaces = _fixture.GenerateSpacesHistory();
        var reservations = _fixture.GenerateReservationHistory(spaces: spaces);
        var query = new GetDashboardMetricsQuery { StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(30) };

        _fixture.SpaceRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(spaces);
        _fixture.ReservationRepositoryMock.Setup(r => r.GetWithDetailsByPeriodAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(reservations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TopRooms.Should().NotBeEmpty();
        result.TopRooms.Should().ContainSingle();
        result.TopRooms.First().Name.Should().Be("Sala A");
        result.TopRooms.First().Reservas.Should().Be(3);
    }
}
