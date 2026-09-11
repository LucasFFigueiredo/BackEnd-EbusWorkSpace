using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Service.API.Tests.Config;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JCA.WorkSpace.Service.API.Tests;

public class ReservationsBatchIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;

    public ReservationsBatchIntegrationTests(WorkSpaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateTestUserAsync(string name, string email)
    {
        var command = new CreateUserCommand
        {
            Name = name,
            Email = email,
            Profile = UserProfile.Employee,
            Sector = "Engenharia"
        };
        var res = await _client.PostAsJsonAsync("/api/Users", command);
        res.EnsureSuccessStatusCode();
        var content = await res.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CreateBatch_ShouldSucceed_WhenUsingDifferentDesksForDifferentDays()
    {
        var userId = await CreateTestUserAsync("Lucas", $"lucas_{Guid.NewGuid()}@jcatlm.com");

        var mesa201Id = Guid.Parse("074891d0-865d-4bad-910a-f48e958f0f63");
        var mesa202Id = Guid.Parse("57b94b5f-34f9-46a8-bc11-63b0d0e0ec38");

        var baseDate = DateTime.UtcNow.AddDays(10).Date;

        var batchCommand = new CreateBatchReservationCommand
        {
            UserId = userId,
            Reservations = new List<BatchReservationItem>
            {
                new() { SpaceId = mesa201Id, StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(18) },
                new() { SpaceId = mesa202Id, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(18) }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Reservations/batch", batchCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = resultJson.GetProperty("data");

        data.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        data.GetProperty("successfulCount").GetInt32().Should().Be(2);
        data.GetProperty("errors").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task CreateBatch_ShouldReturnPartialSuccess207_WhenSomeoneStealsADeskAtTheLastSecond()
    {
        // Arrange
        var anaAdminId = await CreateTestUserAsync("Ana Admin", $"ana_{Guid.NewGuid()}@jcatlm.com");
        var lucasId = await CreateTestUserAsync("Lucas", $"lucas_conflito_{Guid.NewGuid()}@jcatlm.com");

        var mesa201Id = Guid.Parse("074891d0-865d-4bad-910a-f48e958f0f63");
        var baseDate = DateTime.UtcNow.AddDays(15).Date;

        var anaCommand = new CreateReservationCommand
        {
            UserId = anaAdminId,
            SpaceId = mesa201Id,
            StartTime = baseDate.AddDays(1).AddHours(9),
            EndTime = baseDate.AddDays(1).AddHours(18)
        };
        await _client.PostAsJsonAsync("/api/Reservations", anaCommand);

        var lucasBatchCommand = new CreateBatchReservationCommand
        {
            UserId = lucasId,
            Reservations = new List<BatchReservationItem>
            {
                new() { SpaceId = mesa201Id, StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(18) },
                new() { SpaceId = mesa201Id, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(18) }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Reservations/batch", lucasBatchCommand);

        // Assert
        response.StatusCode.Should().Be((HttpStatusCode)207);

        var resultJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = resultJson.GetProperty("data");

        data.GetProperty("isPartial").GetBoolean().Should().BeTrue();
        data.GetProperty("successfulCount").GetInt32().Should().Be(1);

        var errors = data.GetProperty("errors");
        errors.GetArrayLength().Should().Be(1);
        errors[0].GetString().Should().Contain("Ana Admin");
    }
}