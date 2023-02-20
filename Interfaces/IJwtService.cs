using DoctorWebApi.Models;
using static DoctorWebApi.Services.JwtService;

namespace DoctorWebApi.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateJWTTokenAsync(User user);
        //Task<string> GenerateJWTNewTokenAsync(User user);
    }
}
