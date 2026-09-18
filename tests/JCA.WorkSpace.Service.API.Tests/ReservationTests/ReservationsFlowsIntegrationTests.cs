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

public class ReservationsFlowsIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;

    public ReservationsFlowsIntegrationTests(WorkSpaceApiFactory factory)
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
            Sector = "Operações"
        };
        var res = await _client.PostAsJsonAsync("/api/Users", command);
        res.EnsureSuccessStatusCode();
        var content = await res.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CheckIn_ShouldUpdateReservationStatusToCheckedIn()
    {
        // Arrange
        var userId = await CreateTestUserAsync("Carlos Checkin", $"carlos_{Guid.NewGuid()}@jcatlm.com");
        var mesaId = Guid.Parse("432ad042-c232-4978-bf4f-2702ffa5c50e");

        var createCommand = new CreateReservationCommand
        {
            UserId = userId,
            SpaceId = mesaId,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        var createRes = await _client.PostAsJsonAsync("/api/Reservations", createCommand);
        var createContent = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var reservationId = createContent.GetProperty("id").GetGuid();

        var checkInCommand = new CheckInCommand
        {
            UserId = userId,
            SpaceId = mesaId,
            UserLatitude = -23.503187,
            UserLongitude = -46.848757
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Reservations/scan-checkin", checkInCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/Reservations/{reservationId}");
        var reservationDto = await getResponse.Content.ReadFromJsonAsync<ReservationDto>();

        reservationDto.Should().NotBeNull();
        reservationDto!.Status.Should().Be("CheckedIn");
    }

    [Fact]
    public async Task ExtensionFlow_ShouldRequestListAndApproveExtraTime()
    {
        // Arrange
        var solicitanteId = await CreateTestUserAsync("Colaborador Extensao", $"colab_{Guid.NewGuid()}@jcatlm.com");
        var facilitiesId = await CreateTestUserAsync("Admin Facilities", $"fac_{Guid.NewGuid()}@jcatlm.com");
        var salaId = Guid.Parse("0b51cbe6-6556-4f95-a10c-85d4c4d31715");

        var dataInicio = DateTime.UtcNow.AddDays(2).Date.AddHours(14);
        var dataFimOriginal = dataInicio.AddHours(2);

        var createCommand = new CreateReservationCommand
        {
            UserId = solicitanteId,
            SpaceId = salaId,
            StartTime = dataInicio,
            EndTime = dataFimOriginal
        };
        var createRes = await _client.PostAsJsonAsync("/api/Reservations", createCommand);
        var createContent = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var reservationId = createContent.GetProperty("id").GetGuid();


        var requestCommand = new RequestExtensionCommand
        {
            UserId = solicitanteId,
            ReservationId = reservationId,
            AdditionalMinutes = 60,
            Justification = "Reunião de alinhamento trimestral vai atrasar."
        };
        var requestRes = await _client.PostAsJsonAsync("/api/Reservations/extension/request", requestCommand);
        requestRes.StatusCode.Should().Be(HttpStatusCode.OK);


        var listRes = await _client.GetAsync("/api/Reservations/extension/requests");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var requestsList = await listRes.Content.ReadFromJsonAsync<List<ExtensionRequestDto>>();
        requestsList.Should().NotBeNull();
        requestsList.Should().Contain(r => r.ReservationId == reservationId && r.RequestedMinutes == 60);


        var approveCommand = new ApproveExtensionCommand
        {
            ApproverId = facilitiesId,
            ReservationId = reservationId,
            AdditionalMinutes = 60,
            IsApproved = true
        };
        var approveRes = await _client.PatchAsJsonAsync("/api/Reservations/extension/approve", approveCommand);
        approveRes.StatusCode.Should().Be(HttpStatusCode.OK);


        var getRes = await _client.GetAsync($"/api/Reservations/{reservationId}");
        var finalReservation = await getRes.Content.ReadFromJsonAsync<ReservationDto>();

        finalReservation!.EndTime.Should().Be(dataFimOriginal.AddMinutes(60));
    }
}