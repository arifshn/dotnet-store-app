// MappingProfiles.cs
using AutoMapper;
using API.Entity;
using API.DTO;

namespace API.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Favorite, FavoriteDTO>().ReverseMap();
        }
    }
}
