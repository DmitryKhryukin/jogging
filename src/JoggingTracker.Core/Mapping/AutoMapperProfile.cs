using AutoMapper;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.DTOs.Run;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.Core.Helpers;
using JoggingTracker.DataAccess.DbEntities;

namespace JoggingTracker.Core.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserDb, UserDto>();
            CreateMap<UserDb, UserWithRolesDto>();

            CreateMap<RunCreateRequest, RunDb>()
                .ForMember(dest => dest.Date,
                    opt => opt.MapFrom(src => src.Date.Date));
            CreateMap<RunUpdateRequest, RunDb>()
                .ForMember(dest => dest.Date, 
                    opt => opt.MapFrom(src => src.Date.Date));;
            CreateMap<RunDb, RunDto>();
        }
    }
}