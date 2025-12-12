using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AutoMapper;
using MFAWebApplication.Entities;

namespace MFAWebApplication.DTOs;

public class MapperConfiguration
{
    public static Mapper InitializeAutomapper()
    {
        var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDTO>().ReverseMap();
                cfg.CreateMap<User, UserUpsertEvent>().ReverseMap();
                cfg.CreateMap<UserUpsertEvent, UserReadModel>()
                    .ForMember(
                        dest => dest.Id,
                        opt => opt.MapFrom(src => src.Id.ToString()));
            }
        );

        var mapper = new Mapper(config);
        return mapper;
    }
}
