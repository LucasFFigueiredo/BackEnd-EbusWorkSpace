using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Handlers.Reservations;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Reservations;

[Collection(nameof(ReservationFixtureCollection))]
public class CancelReservationCommandHandlerTests
{
    private readonly ReservationTestsFixture _fixture;
    private readonly CancelReservationCommandHandler _handler;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public CancelReservationCommandHandlerTests(ReservationTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new CancelReservationCommandHandler(
            _fixture.ReservationRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object,
            _fixture.AuditLogRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCancelReservationAndGenerateAuditLog_WhenReservationIsPendingAndUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();
        var reservation = _fixture.GenerateValidReservation(userId, spaceId, ReservationStatus.Pending);

        var command = new CancelReservationCommand
        {
            UserId = userId,
            ReservationId = reservation.Id
        };

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.ReservationId.Value))
            .ReturnsAsync(reservation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Canceled);

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log =>
            log.UserId == userId &&
            log.EntityId == reservation.Id &&
            log.Action.Contains("Cancelamento de Reserva")
        )), Times.Once);

        _fixture.ReservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIdIsDifferentFromReservationOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var requestUserId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();
        var reservation = _fixture.GenerateValidReservation(ownerId, spaceId, ReservationStatus.Pending);

        var command = new CancelReservationCommand
        {
            UserId = requestUserId,
            ReservationId = reservation.Id
        };

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.ReservationId.Value))
            .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Voc* s* pode cancelar as suas pr*prias reservas.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenReservationIsNotPending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();
        var reservation = _fixture.GenerateValidReservation(userId, spaceId, ReservationStatus.CheckedIn);

        var command = new CancelReservationCommand
        {
            UserId = userId,
            ReservationId = reservation.Id
        };

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.ReservationId.Value))
            .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"N*o * poss*vel cancelar. O status atual da reserva * {reservation.Status}.");
    }
}