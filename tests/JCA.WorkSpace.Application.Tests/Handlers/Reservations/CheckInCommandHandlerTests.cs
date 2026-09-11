using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Handlers.Reservations;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Reservations;

[Collection(nameof(ReservationFixtureCollection))]
public class CheckInCommandHandlerTests
{
    private readonly ReservationTestsFixture _fixture;
    private readonly CheckInCommandHandler _handler;

    private const double ValidLatitude = -23.503187;
    private const double ValidLongitude = -46.848757;

    private const double InvalidLatitude = -23.550520;
    private const double InvalidLongitude = -46.633308;

    public CheckInCommandHandlerTests(ReservationTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new CheckInCommandHandler(
            _fixture.ReservationRepositoryMock.Object,
            _fixture.SpaceRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCheckIn_WhenDistanceIsWithinLimitAndUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var space = _fixture.GenerateValidSpace();
        var reservation = _fixture.GenerateValidReservation(userId, space.Id, ReservationStatus.Pending);

        var command = new CheckInCommand
        {
            UserId = userId,
            SpaceId = space.Id,
            UserLatitude = ValidLatitude,
            UserLongitude = ValidLongitude
        };

        _fixture.SpaceRepositoryMock.Setup(repo => repo.GetByIdAsync(command.SpaceId))
                                    .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock.Setup(repo => repo.GetCurrentBySpaceIdAsync(command.SpaceId, It.IsAny<DateTime>()))
                                          .ReturnsAsync(reservation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.CheckedIn);
        reservation.CheckInAt.Should().NotBeNull();

        _fixture.ReservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDistanceIsGreaterThanLimit()
    {
        // Arrange
        var command = new CheckInCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            UserLatitude = InvalidLatitude,
            UserLongitude = InvalidLongitude
        };

        var space = _fixture.GenerateValidSpace();

        _fixture.SpaceRepositoryMock.Setup(repo => repo.GetByIdAsync(command.SpaceId))
                                    .ReturnsAsync(space);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Você está muito longe do escritório. Distância atual:*m. Aproxime-se para fazer o check-in.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSpaceIsOccupiedByAnotherUser()
    {
        // Arrange
        var commandUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var space = _fixture.GenerateValidSpace();

        var reservationOfAnotherUser = _fixture.GenerateValidReservation(ownerUserId, space.Id, ReservationStatus.Pending);
        reservationOfAnotherUser.User = new User { Name = "João da Silva" };

        var command = new CheckInCommand
        {
            UserId = commandUserId,
            SpaceId = space.Id,
            UserLatitude = ValidLatitude,
            UserLongitude = ValidLongitude
        };

        _fixture.SpaceRepositoryMock.Setup(repo => repo.GetByIdAsync(command.SpaceId))
                                    .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock.Setup(repo => repo.GetCurrentBySpaceIdAsync(command.SpaceId, It.IsAny<DateTime>()))
                                          .ReturnsAsync(reservationOfAnotherUser);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*já está reservada para João da Silva neste momento*");
    }

    [Fact]
    public async Task Handle_ShouldThrowExceptionWithRoomTag_WhenRoomIsFree()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace();
        space.Type = SpaceType.Room;

        var command = new CheckInCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            UserLatitude = ValidLatitude,
            UserLongitude = ValidLongitude
        };

        _fixture.SpaceRepositoryMock.Setup(repo => repo.GetByIdAsync(command.SpaceId))
                                    .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock.Setup(repo => repo.GetCurrentBySpaceIdAsync(command.SpaceId, It.IsAny<DateTime>()))
                                          .ReturnsAsync((Reservation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("FREE_ROOM|Esta sala está livre no momento. Por favor, retorne ao menu e faça a reserva informando o período de uso.");
    }

    [Fact]
    public async Task Handle_ShouldThrowExceptionWithDeskTag_WhenDeskIsFree()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace();
        space.Type = SpaceType.Desk; // É uma mesa

        var command = new CheckInCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            UserLatitude = ValidLatitude,
            UserLongitude = ValidLongitude
        };

        _fixture.SpaceRepositoryMock.Setup(repo => repo.GetByIdAsync(command.SpaceId))
                                    .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock.Setup(repo => repo.GetCurrentBySpaceIdAsync(command.SpaceId, It.IsAny<DateTime>()))
                                          .ReturnsAsync((Reservation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("FREE_DESK|Esta mesa está livre no momento. Deseja reservá-la pelo resto do dia e fazer o check-in automático?");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenAlreadyCheckedIn()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var space = _fixture.GenerateValidSpace();

        var reservation = _fixture.GenerateValidReservation(userId, space.Id, ReservationStatus.CheckedIn);

        var command = new CheckInCommand
        {
            UserId = userId,
            SpaceId = space.Id,
            UserLatitude = ValidLatitude,
            UserLongitude = ValidLongitude
        };

        _fixture.SpaceRepositoryMock.Setup(repo => repo.GetByIdAsync(command.SpaceId))
                                    .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock.Setup(repo => repo.GetCurrentBySpaceIdAsync(command.SpaceId, It.IsAny<DateTime>()))
                                          .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Você já realizou o check-in neste espaço.");
    }
}