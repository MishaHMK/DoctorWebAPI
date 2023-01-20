using DoctorWebApi.Models;

namespace DoctorWebApi.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateJWTTokenAsync(User user);
    }
}
