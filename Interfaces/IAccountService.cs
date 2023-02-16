using DoctorWebApi.Helpers;
using DoctorWebApi.Models;

namespace DoctorWebApi.Interfaces
{
    public interface IAccountService
    {
        public Task<PagedList<User>> GetUsersAsync(UserParams userParams);
    }
}
