using JCA.WorkSpace.Application.Commands.NoShows;
using JCA.WorkSpace.Service.API.Workers;
using Microsoft.Extensions.Logging;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.NoShow;

[Collection(nameof(NoShowFixtureCollection))]
public class NoShowWorkerTests
{
    private readonly NoShowTestsFixture _fixture;
    private readonly NoShowWorker _worker;
    private readonly Mock<ILogger<NoShowWorker>> _loggerMock;

    public NoShowWorkerTests(NoShowTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
        
        _loggerMock = new Mock<ILogger<NoShowWorker>>();
        _worker = new NoShowWorker(_loggerMock.Object, _fixture.ServiceProviderMock.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldExecuteLoop_AndCallMediator()
    {
        // Arrange
        _fixture.MediatorMock
            .Setup(m => m.Send(It.IsAny<ProcessNoShowsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var cts = new CancellationTokenSource();
        
        // Act
        await _worker.StartAsync(CancellationToken.None);
        
        await Task.Delay(100);
        
        await _worker.StopAsync(CancellationToken.None);

        // Assert
        _fixture.ServiceScopeFactoryMock.Verify(s => s.CreateScope(), Times.AtLeastOnce);

        _fixture.MediatorMock.Verify(m => m.Send(It.IsAny<ProcessNoShowsCommand>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_ShouldGracefullyCancel_WhenTokenIsCancelledBeforeLoop()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var method = typeof(NoShowWorker).GetMethod("ExecuteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var task = (Task)method!.Invoke(_worker, new object[] { cts.Token })!;
        await task;

        // Assert
        _fixture.ServiceScopeFactoryMock.Verify(s => s.CreateScope(), Times.Never);

        _fixture.MediatorMock.Verify(m => m.Send(It.IsAny<ProcessNoShowsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
