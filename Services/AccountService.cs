using DoctorWebApi.Helpers;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace DoctorWebApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _db;
        public AccountService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedList<User>> GetUsersAsync(UserParams userParams)
        {
            var query = _db.Users.AsNoTracking();

            return await PagedList<User>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
        }
    }
}
