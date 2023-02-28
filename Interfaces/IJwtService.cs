using Doctor.DataAcsess.Entities;
using static DoctorWebApi.Services.JwtService;

namespace DoctorWebApi.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateJWTTokenAsync(User user);
    }
}
