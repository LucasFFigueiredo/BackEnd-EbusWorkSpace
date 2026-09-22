using FluentAssertions;
using Moq;
using JCA.WorkSpace.Application.Commands.AccessRequests;
using JCA.WorkSpace.Application.Handlers.AccessRequests;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;

namespace JCA.WorkSpace.Application.Tests.Handlers.AccessRequests;

public class CreateAccessRequestHandlerTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateAccessRequestHandler _handler;

    public CreateAccessRequestHandlerTests()
    {
        _accessRequestRepoMock = new Mock<IAccessRequestRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateAccessRequestHandler(
            _accessRequestRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAccessRequest_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAccessRequestCommand { UserId = userId, RequestedProfile = UserProfile.Manager };
        
        var user = new User { Id = userId, Profile = UserProfile.Employee };
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _accessRequestRepoMock.Setup(x => x.HasPendingRequestAsync(userId)).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _accessRequestRepoMock.Verify(x => x.AddAsync(It.Is<AccessRequest>(a => a.UserId == userId && a.RequestedProfile == UserProfile.Manager)), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserHasPendingRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAccessRequestCommand { UserId = userId, RequestedProfile = UserProfile.Manager };
        
        var user = new User { Id = userId, Profile = UserProfile.Employee };
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _accessRequestRepoMock.Setup(x => x.HasPendingRequestAsync(userId)).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Já existe uma solicitação de acesso pendente para este usuário.");
    }
}
