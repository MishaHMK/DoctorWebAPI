using AutoMapper;
using DoctorWebApi.Models;
using System.Security.AccessControl;

namespace DoctorWebApi.Mapper
{
    public class AllMappersProfile : Profile
    {
        public AllMappersProfile()
        {
            CreateMap<Message, MessageDTO>().ReverseMap();
        }
    }
}
