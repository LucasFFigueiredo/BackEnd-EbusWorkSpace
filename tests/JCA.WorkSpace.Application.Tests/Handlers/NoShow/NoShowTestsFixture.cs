using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.NoShow;

public class NoShowTestsFixture
{
    public Mock<IServiceProvider> ServiceProviderMock { get; set; }
    public Mock<IServiceScopeFactory> ServiceScopeFactoryMock { get; set; }
    public Mock<IServiceScope> ServiceScopeMock { get; set; }
    public Mock<IMediator> MediatorMock { get; set; }

    public Mock<IReservationRepository> ReservationRepositoryMock { get; set; }
    public Mock<IAuditLogRepository> AuditLogRepositoryMock { get; set; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; set; }

    public NoShowTestsFixture()
    {
        ServiceProviderMock = new Mock<IServiceProvider>();
        ServiceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        ServiceScopeMock = new Mock<IServiceScope>();
        MediatorMock = new Mock<IMediator>();

        ReservationRepositoryMock = new Mock<IReservationRepository>();
        AuditLogRepositoryMock = new Mock<IAuditLogRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
    }

    public void ResetMocks()
    {
        ServiceProviderMock.Reset();
        ServiceScopeFactoryMock.Reset();
        ServiceScopeMock.Reset();
        MediatorMock.Reset();

        ReservationRepositoryMock.Reset();
        AuditLogRepositoryMock.Reset();
        UnitOfWorkMock.Reset();

        ServiceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(ServiceScopeFactoryMock.Object);

        ServiceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(ServiceScopeMock.Object);

        ServiceScopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(ServiceProviderMock.Object);

        ServiceProviderMock
            .Setup(x => x.GetService(typeof(IMediator)))
            .Returns(MediatorMock.Object);

        UnitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);
    }

    public List<Reservation> GenerateDelayedReservations()
    {
        var now = DateTime.UtcNow;
        return new List<Reservation>
        {
            new Reservation
            {
                Id = Guid.NewGuid(),
                SpaceId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StartTime = now.AddMinutes(-20), 
                EndTime = now.AddHours(1),
                Status = ReservationStatus.Pending,
                CheckInAt = null, 
                Space = new Space { Id = Guid.NewGuid(), Name = "Sala A", Type = SpaceType.Room }
            },
            new Reservation
            {
                Id = Guid.NewGuid(),
                SpaceId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StartTime = now.AddHours(-1), 
                EndTime = now.AddHours(5),
                Status = ReservationStatus.Pending,
                CheckInAt = null,
                Space = new Space { Id = Guid.NewGuid(), Name = "Mesa 1", Type = SpaceType.Desk }
            }
        };
    }
}

[CollectionDefinition(nameof(NoShowFixtureCollection))]
public class NoShowFixtureCollection : ICollectionFixture<NoShowTestsFixture>
{
}
