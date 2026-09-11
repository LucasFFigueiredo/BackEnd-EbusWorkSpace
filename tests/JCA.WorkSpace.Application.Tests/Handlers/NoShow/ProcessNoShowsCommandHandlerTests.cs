using FluentAssertions;
using JCA.WorkSpace.Application.Commands.NoShows;
using JCA.WorkSpace.Application.Handlers.NoShows;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.NoShow;

[Collection(nameof(NoShowFixtureCollection))]
public class ProcessNoShowsCommandHandlerTests
{
    private readonly NoShowTestsFixture _fixture;
    private readonly ProcessNoShowsCommandHandler _handler;

    public ProcessNoShowsCommandHandlerTests(NoShowTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
        
        var loggerMock = new Mock<ILogger<ProcessNoShowsCommandHandler>>();
        
        _handler = new ProcessNoShowsCommandHandler(
            _fixture.ReservationRepositoryMock.Object,
            _fixture.AuditLogRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelDelayedReservations_AndCallAudit_WhenNoShowsExist()
    {
        // Arrange
        var command = new ProcessNoShowsCommand { ProcessRooms = true, ProcessDesks = true };
        var delayedReservations = _fixture.GenerateDelayedReservations(); 
        
        _fixture.ReservationRepositoryMock
            .Setup(r => r.GetPendingForWorkerAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(delayedReservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        delayedReservations.All(r => r.Status == ReservationStatus.NoShow).Should().BeTrue();

        _fixture.AuditLogRepositoryMock.Verify(a => a.AddAsync(It.Is<AuditLog>(log => 
            log.Action == "NO_SHOW_AUTOMATICO" && 
            log.Details != null && 
            log.Details.Contains("cancelada automaticamente"))), Times.Exactly(2));

        _fixture.UnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCompleteWithoutCommits_WhenNoDelaysFound()
    {
        // Arrange
        var command = new ProcessNoShowsCommand { ProcessRooms = true, ProcessDesks = true };
        
        _fixture.ReservationRepositoryMock
            .Setup(r => r.GetPendingForWorkerAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reservation>()); 

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _fixture.AuditLogRepositoryMock.Verify(a => a.AddAsync(It.IsAny<AuditLog>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}
