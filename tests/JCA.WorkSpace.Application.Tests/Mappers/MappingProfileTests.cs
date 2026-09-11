using AutoMapper;
using FluentAssertions;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Application.Mappers;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using System;
using System.Linq;
using Xunit;

namespace JCA.WorkSpace.Application.Tests.Mappers;

public class MappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public MappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void MappingProfile_ShouldHaveValidConfiguration()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Reservation_To_ReservationDto_ShouldMapSpaceName_WhenSpaceIsNotNull()
    {
        // Arrange
        var space = new Space
        {
            Id = Guid.NewGuid(),
            Name = "Sala de Reunião Alpha",
            Type = SpaceType.Room
        };

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            SpaceId = space.Id,
            Space = space,
            UserId = Guid.NewGuid(),
            Status = ReservationStatus.Pending,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = _mapper.Map<ReservationDto>(reservation);

        // Assert
        result.Should().NotBeNull();
        result.SpaceName.Should().Be(space.Name);
    }

    [Fact]
    public void Reservation_To_ReservationDto_ShouldMapFallbackSpaceName_WhenSpaceIsNull()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            Space = null, // Cenrio onde a entidade no foi carregada via Include
            UserId = Guid.NewGuid(),
            Status = ReservationStatus.Pending
        };

        // Act
        var result = _mapper.Map<ReservationDto>(reservation);

        // Assert
        result.Should().NotBeNull();
        // Fallback default: "Espao Indisponvel"
        result.SpaceName.Should().Contain("Indispon");
    }

    [Fact]
    public void Reservation_To_ReservationDto_ShouldMapFallbackUserName_WhenUserIsNull()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            User = null, // Cenrio de User null
            Status = ReservationStatus.Pending
        };

        // Act
        var result = _mapper.Map<ReservationDto>(reservation);

        // Assert
        result.Should().NotBeNull();
        // Fallback default: "Usurio Excludo"
        result.UserName.Should().Contain("Exclu");
        result.UserEmail.Should().BeEmpty();
    }
}
