using AutoMapper;
using JCA.WorkSpace.Application.Dtos.AuditLogs;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Application.Dtos.Spaces;
using JCA.WorkSpace.Application.Dtos.Users;
using JCA.WorkSpace.Domain.Entities;

namespace JCA.WorkSpace.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Space, SpaceDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<Reservation, ReservationDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SpaceName, opt => opt.MapFrom(src => src.Space != null ? src.Space.Name : "Espaço Indisponível"))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : "Usuário Excluído"))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : ""));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile.ToString()));

        CreateMap<AuditLog, AuditLogDto>();
    }
}