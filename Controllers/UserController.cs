using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using DoctorWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        UserManager<User> _userManager;

        public UserController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                var IsUserAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);

                var model = new PersonalData
                {
                    User = user,
                    IsUserAdmin = IsUserAdmin
                };

                return Ok(model);
            }

            return NotFound();
        }

    }
}
