using DoctorWebApi.Extensions;
using DoctorWebApi.Helpers;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly IAccountService _accService;
        private readonly IEmailSender _emailSender;
        UserManager<User> _userManager;
        SignInManager<User> _signInManager;
        RoleManager<IdentityRole> _roleManager;
        private readonly HostUrlOptions _hostUrl;

        public AccountController(ApplicationDbContext context, UserManager<User> userManager,
            SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager,
            IJwtService jwtService, IOptions<HostUrlOptions> options, 
            IEmailSender emailSender, IAccountService accService)
        {
            _db = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _hostUrl = options.Value;
            _emailSender = emailSender;
            _accService = accService;
        }

        public string BackEndApiURL => _hostUrl.BackEnd + "/api";

        // GET api/Account/roles
        [HttpGet("roles")]
        public async Task<List<string>> CheckRoles()
        {
            var roles = new[]
{
                Roles.Admin,
                Roles.Doctor,
                Roles.Patient
            };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var idRole = new IdentityRole(role);
                    await _roleManager.CreateAsync(idRole);
                }
            }

            var identityRoles = _roleManager.Roles.ToList();

            var roleList = new List<string>();

            foreach (var role in identityRoles)
            {
                roleList.Add(role.Name);
            }

            return roleList;
        }

        // GET api/Account/times
        [HttpGet("times")]
        public async Task<List<string>> GetTimes()
        {
            var times = Timestamps.GetTimesForDropDown(); 
            var timeList = new List<string>();

            foreach (var time in times)
            {
                timeList.Add(time.Text);
            }

            return timeList;
        }

        // POST api/Account/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(Register model)
        {
            await CheckRoles();

            if (ModelState.IsValid) {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name 
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    try
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var encoded = HttpUtility.UrlEncode(token);
                        string url = $"{BackEndApiURL}/Account/confirmEmail?userId={user.Id}&token={token}";
                        await _emailSender.SendEmailAsync(user.Email, "Confirm your account",
                                                 $"Follow the: <br/><a href={url}>link to confirm</a>");
                    }
                    catch (Exception)
                    {
                        await _userManager.DeleteAsync(user);
                        throw;
                    }

                    return NoContent();
                }
            }

            return BadRequest("Register failed");
        }


        // POST api/Account/authenticate
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate(Login model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Email);
                    var generatedToken = await _jwtService.GenerateJWTTokenAsync(user);
                    return Ok(new { token = generatedToken });
                }
                return BadRequest("No Succedded");
            }

            return BadRequest("Not valid");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return Ok();
        }


        // GET: api/Account/users
        [HttpGet("users")]
        public async Task<ActionResult<PagedList<User>>> GetAllUsers([FromQuery]UserParams userParams)
        {
            var query = _db.Users.AsNoTracking();
            var userList = await _accService.GetUsersAsync(userParams);
            var responce = new PaginationHeader(userList, userParams.PageNumber, userParams.PageSize, query.Count());
            return Ok(responce);
        }

        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([Required, FromQuery] string userId, [Required, FromQuery] string token)
        {
            var decodedToken = HttpUtility.UrlDecode(token);
            var user = await _userManager.FindByIdAsync(userId);
            decodedToken = decodedToken.Replace(" ", "+"); 
            if (user == null)
            {
                return NotFound("No user");
            }
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            return Ok(result);
        }
    }
}
