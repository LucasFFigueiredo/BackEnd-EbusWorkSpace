using FluentAssertions;
using Moq;
using JCA.WorkSpace.Application.Commands.AccessRequests;
using JCA.WorkSpace.Application.Handlers.AccessRequests;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;

namespace JCA.WorkSpace.Application.Tests.Handlers.AccessRequests;

public class RejectAccessRequestHandlerTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RejectAccessRequestHandler _handler;

    public RejectAccessRequestHandlerTests()
    {
        _accessRequestRepoMock = new Mock<IAccessRequestRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RejectAccessRequestHandler(
            _accessRequestRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRejectRequest_WhenValid()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new RejectAccessRequestCommand { RequestId = requestId };
        
        var accessRequest = new AccessRequest 
        { 
            Id = requestId, 
            Status = AccessRequestStatus.Pending 
        };
        
        _accessRequestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(accessRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        accessRequest.Status.Should().Be(AccessRequestStatus.Rejected);
        
        _accessRequestRepoMock.Verify(x => x.UpdateAsync(accessRequest), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRequestNotPending()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new RejectAccessRequestCommand { RequestId = requestId };
        
        var accessRequest = new AccessRequest 
        { 
            Id = requestId, 
            Status = AccessRequestStatus.Approved // Já aprovada
        };
        
        _accessRequestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(accessRequest);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("A solicitação não está pendente.");
    }
}
