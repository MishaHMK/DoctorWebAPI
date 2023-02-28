using AutoMapper;
using Doctor.DataAcsess.Entities;

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