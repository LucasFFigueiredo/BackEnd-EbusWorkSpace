using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Dtos.Dashboards;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Service.API.Tests.Config;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace JCA.WorkSpace.Service.API.Tests;

public class DashboardControllerIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public DashboardControllerIntegrationTests(WorkSpaceApiFactory factory)
    {
        _client = factory.CreateClient();
        _config = factory.Services.GetRequiredService<IConfiguration>();
        TestAuthHelper.AuthenticateClient(_client, _config);
    }

    private async Task<Guid> CreateUserWithSectorAsync(string sector)
    {
        var command = new CreateUserCommand
        {
            Name = $"User {sector}",
            Email = $"dash_{Guid.NewGuid()}@empresa.com",
            Profile = UserProfile.Employee,
            Sector = sector
        };
        var response = await _client.PostAsJsonAsync("/api/Users", command);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task GetGeneralMetrics_ShouldReturnAggregationsCorrectly()
    {
        var userId = await CreateUserWithSectorAsync("Engenharia");
        var mesaId = Guid.Parse("074891d0-865d-4bad-910a-f48e958f0f63");

        var reservationCommand = new CreateReservationCommand
        {
            UserId = userId,
            SpaceId = mesaId,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4)
        };
        var resResponse = await _client.PostAsJsonAsync("/api/Reservations", reservationCommand);
        resResponse.EnsureSuccessStatusCode();

        var start = DateTime.UtcNow.AddDays(-1).ToString("O");
        var end = DateTime.UtcNow.AddDays(2).ToString("O");
        var response = await _client.GetAsync($"/api/Dashboard/general?startDate={start}&endDate={end}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var general = await response.Content.ReadFromJsonAsync<DashboardGeneralDto>();

        general.Should().NotBeNull();
        general!.Totals.TotalReservations.Should().BeGreaterThanOrEqualTo(1);
        general.ByDepartment.Should().Contain(d => d.Name == "Engenharia");
        general.ByFloor.Should().Contain(f => f.Name == "2º Andar");
    }

    [Fact]
    public async Task GetUserMetrics_ShouldReturnIndividualUserStats()
    {
        var userId = await CreateUserWithSectorAsync("RH");
        var salaId = Guid.Parse("a090562b-4727-4503-b4ac-118867d3fbd5");

        var reservationCommand = new CreateReservationCommand
        {
            UserId = userId,
            SpaceId = salaId,
            StartTime = DateTime.UtcNow.AddDays(1).AddHours(10),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(11)
        };
        await _client.PostAsJsonAsync("/api/Reservations", reservationCommand);

        var start = DateTime.UtcNow.AddDays(-1).ToString("O");
        var end = DateTime.UtcNow.AddDays(2).ToString("O");
        var response = await _client.GetAsync($"/api/Dashboard/user/{userId}?startDate={start}&endDate={end}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var userDash = await response.Content.ReadFromJsonAsync<DashboardUserDto>();

        userDash.Should().NotBeNull();
        userDash!.Stats.Total.Should().BeGreaterThanOrEqualTo(1);
        userDash.Stats.Rooms.Should().BeGreaterThanOrEqualTo(1);
    }
}