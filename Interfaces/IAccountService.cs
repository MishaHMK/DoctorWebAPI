using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using DoctorWebApi.Helpers;

namespace DoctorWebApi.Interfaces
{
    public interface IAccountService
    {
        public Task<PagedList<User>> GetUsersAsync(UserParams userParams);

        public Task<User> GetUserAsync(string name);

        public Task<bool> SaveAllAsync();
    }
}
