using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Handlers.Reservations;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Reservations;

[Collection(nameof(ReservationFixtureCollection))]
public class CreateReservationCommandHandlerTests
{
    private readonly ReservationTestsFixture _fixture;
    private readonly CreateReservationCommandHandler _handler;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public CreateReservationCommandHandlerTests(ReservationTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new CreateReservationCommandHandler(
            _fixture.ReservationRepositoryMock.Object,
            _fixture.SpaceRepositoryMock.Object,
            _userRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    private void SetupValidUser(Guid userId, UserProfile profile = UserProfile.Employee)
    {
        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Profile = profile });
    }

    [Fact]
    public async Task Handle_ShouldCreateReservationAsPending_WhenSpaceDoesNotRequireApproval()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace(requiresApproval: false, isBlocked: false);
        var command = new CreateReservationCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        SetupValidUser(command.UserId);

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.SpaceId))
            .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetConflictsAsync(command.SpaceId, command.StartTime, command.EndTime))
            .ReturnsAsync(Enumerable.Empty<Reservation>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _fixture.ReservationRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Reservation>(r =>
            r.Status == ReservationStatus.Pending &&
            r.UserId == command.UserId &&
            r.SpaceId == command.SpaceId
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateReservationAsAwaitingApproval_WhenSpaceRequiresApproval()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace(requiresApproval: true, isBlocked: false);
        var command = new CreateReservationCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        SetupValidUser(command.UserId);

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.SpaceId))
            .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetConflictsAsync(command.SpaceId, command.StartTime, command.EndTime))
            .ReturnsAsync(Enumerable.Empty<Reservation>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _fixture.ReservationRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Reservation>(r =>
            r.Status == ReservationStatus.AwaitingApproval &&
            r.UserId == command.UserId &&
            r.SpaceId == command.SpaceId
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStartTimeIsGreaterThanOrEqualEndTime()
    {
        // Arrange
        var command = new CreateReservationCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("O hor*rio de in*cio deve ser menor que o hor*rio de t*rmino.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSpaceIsBlocked()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace(isBlocked: true);
        space.MaintenanceReason = "Manutenção preventiva";
        var command = new CreateReservationCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.SpaceId))
            .ReturnsAsync(space);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Espa*o bloqueado para manuten*o: {space.MaintenanceReason}");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenHasDoubleBookingConflict()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace(isBlocked: false);
        var command = new CreateReservationCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        SetupValidUser(command.UserId);

        var conflictingReservation = _fixture.GenerateValidReservation(Guid.NewGuid(), space.Id);

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.SpaceId))
            .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetConflictsAsync(command.SpaceId, command.StartTime, command.EndTime))
            .ReturnsAsync(new List<Reservation> { conflictingReservation });

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já está reservada*");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRoomReservationExceeds3Hours_AndUserIsStandard()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace(requiresApproval: false, isBlocked: false);
        space.Type = SpaceType.Room;

        var command = new CreateReservationCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(5)
        };

        SetupValidUser(command.UserId, UserProfile.Employee);

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.SpaceId))
            .ReturnsAsync(space);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*tempo máximo permitido para reserva de salas é de 3 horas*");
    }

    [Fact]
    public async Task Handle_ShouldCreateReservation_WhenRoomReservationExceeds3Hours_AndUserIsAdmin()
    {
        // Arrange
        var space = _fixture.GenerateValidSpace(requiresApproval: false, isBlocked: false);
        space.Type = SpaceType.Room;

        var command = new CreateReservationCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = space.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(5)
        };

        SetupValidUser(command.UserId, UserProfile.Admin);

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.SpaceId))
            .ReturnsAsync(space);

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetConflictsAsync(command.SpaceId, command.StartTime, command.EndTime))
            .ReturnsAsync(Enumerable.Empty<Reservation>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }
}