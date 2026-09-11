using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Spaces;
using JCA.WorkSpace.Application.Handlers.Spaces;
using JCA.WorkSpace.Domain.Entities;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Spaces;

[Collection(nameof(SpaceFixtureCollection))]
public class SetSpaceMaintenanceCommandHandlerTests
{
    private readonly SpaceTestsFixture _fixture;
    private readonly SetSpaceMaintenanceCommandHandler _handler;

    public SetSpaceMaintenanceCommandHandlerTests(SpaceTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new SetSpaceMaintenanceCommandHandler(
            _fixture.SpaceRepositoryMock.Object,
            _fixture.AuditLogRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldBlockSpaceAndSaveAuditLog_WhenIsBlockedIsTrue()
    {
        // Arrange
        var command = new SetSpaceMaintenanceCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            IsBlocked = true,
            MaintenanceReason = "Ar condicionado quebrado",
            MaintenanceUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _fixture.SpaceRepositoryMock.Verify(repo => repo.SetMaintenanceAsync(
            command.SpaceId,
            command.IsBlocked,
            command.MaintenanceReason,
            command.MaintenanceUntil
        ), Times.Once);

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log => 
            log.UserId == command.UserId &&
            log.EntityId == command.SpaceId &&
            log.Action.Contains("Bloqueio de") &&
            log.Details != null && log.Details.Contains(command.MaintenanceReason)
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUnblockSpaceAndSaveAuditLog_WhenIsBlockedIsFalse()
    {
        // Arrange
        var command = new SetSpaceMaintenanceCommand
        {
            UserId = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            IsBlocked = false,
            MaintenanceReason = null,
            MaintenanceUntil = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _fixture.SpaceRepositoryMock.Verify(repo => repo.SetMaintenanceAsync(
            command.SpaceId,
            command.IsBlocked,
            command.MaintenanceReason,
            command.MaintenanceUntil
        ), Times.Once);

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log => 
            log.UserId == command.UserId &&
            log.EntityId == command.SpaceId &&
            log.Action.Contains("Desbloqueio de") &&
            log.Details != null && log.Details.Contains("Sem justificativa")
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }
}
