using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Handlers.Reservations;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Reservations;

[Collection(nameof(ReservationFixtureCollection))]
public class ApproveReservationCommandHandlerTests
{
    private readonly ReservationTestsFixture _fixture;
    private readonly ApproveReservationCommandHandler _handler;

    public ApproveReservationCommandHandlerTests(ReservationTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new ApproveReservationCommandHandler(
            _fixture.ReservationRepositoryMock.Object,
            _fixture.AuditLogRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldChangeStatusToPending_WhenApproved()
    {
        // Arrange
        var approverId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();
        var reservation = _fixture.GenerateValidReservation(Guid.NewGuid(), spaceId, ReservationStatus.AwaitingApproval);

        var command = new ApproveReservationCommand
        {
            ApproverId = approverId,
            ReservationId = reservation.Id,
            IsApproved = true
        };

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.ReservationId))
            .ReturnsAsync(reservation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Pending);

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log => 
            log.UserId == approverId &&
            log.EntityId == reservation.Id &&
            log.Action == "RESERVA_APROVADA"
        )), Times.Once);

        _fixture.ReservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldChangeStatusToCanceledAndSaveJustification_WhenRejected()
    {
        // Arrange
        var approverId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();
        var reservation = _fixture.GenerateValidReservation(Guid.NewGuid(), spaceId, ReservationStatus.AwaitingApproval);

        var command = new ApproveReservationCommand
        {
            ApproverId = approverId,
            ReservationId = reservation.Id,
            IsApproved = false,
            Justification = "Motivo da reprovacao."
        };

        _fixture.ReservationRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.ReservationId))
            .ReturnsAsync(reservation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Canceled);

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log => 
            log.UserId == approverId &&
            log.EntityId == reservation.Id &&
            log.Action == "RESERVA_NEGADA" &&
            log.Details != null && log.Details.Contains(command.Justification)
        )), Times.Once);

        _fixture.ReservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRejectedAndJustificationIsEmpty()
    {
        // Arrange
        var approverId = Guid.NewGuid();
        var command = new ApproveReservationCommand
        {
            ApproverId = approverId,
            ReservationId = Guid.NewGuid(),
            IsApproved = false,
            Justification = ""
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Uma justificativa é obrigatória ao negar a solicitação.");
    }
}
