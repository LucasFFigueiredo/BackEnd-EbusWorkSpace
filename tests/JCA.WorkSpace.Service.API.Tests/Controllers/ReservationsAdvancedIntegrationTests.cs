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

public class ReservationsAdvancedIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;

    public ReservationsAdvancedIntegrationTests(WorkSpaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateUserAsync()
    {
        var command = new CreateUserCommand
        {
            Name = "Colaborador Reserva",
            Email = $"res_{Guid.NewGuid()}@empresa.com",
            Profile = UserProfile.Employee,
            Sector = "Marketing"
        };
        var res = await _client.PostAsJsonAsync("/api/Users", command);
        var content = await res.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CancelReservation_ShouldUpdateStatusToCanceled_WhenUserIsOwner()
    {
        // Arrange
        var userId = await CreateUserAsync();
        var spaceId = Guid.Parse("57b94b5f-34f9-46a8-bc11-63b0d0e0ec38");

        var createCommand = new CreateReservationCommand
        {
            UserId = userId,
            SpaceId = spaceId,
            StartTime = DateTime.UtcNow.AddDays(3).AddHours(9),
            EndTime = DateTime.UtcNow.AddDays(3).AddHours(12)
        };
        var createRes = await _client.PostAsJsonAsync("/api/Reservations", createCommand);
        var createContent = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var reservationId = createContent.GetProperty("id").GetGuid();

        var cancelCommand = new CancelReservationCommand
        {
            UserId = userId,
            ReservationId = reservationId
        };

        // Act
        var cancelResponse = await _client.PatchAsJsonAsync($"/api/Reservations/{reservationId}/cancel", cancelCommand);

        // Assert
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/Reservations/{reservationId}");
        var reservationDto = await getResponse.Content.ReadFromJsonAsync<ReservationDto>();
        reservationDto!.Status.Should().Be("Canceled");
    }

    [Fact]
    public async Task CreateBatchReservation_ShouldCreateMultipleSlots_WhenNoConflicts()
    {
        // Arrange
        var userId = await CreateUserAsync();
        var spaceId = Guid.Parse("38d0959b-562b-43d1-828c-23aeec84ebaa");
        var baseDate = DateTime.UtcNow.AddDays(5);

        var batchCommand = new CreateBatchReservationCommand
        {
            UserId = userId,
            Reservations = new List<BatchReservationItem>
            {
                new() { SpaceId = spaceId, StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(12) },
                new() { SpaceId = spaceId, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(12) }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Reservations/batch", batchCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        var successfulCount = resultJson.GetProperty("data").GetProperty("successfulCount").GetInt32();

        successfulCount.Should().Be(2);
    }
}