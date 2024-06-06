using AutoMapper;
using BlogAPI.Dto;
using BlogAPI.Models;

namespace BlogAPI.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<BlogDto, Blog>();
            CreateMap<Blog, BlogDto>();
        }
    }
}
