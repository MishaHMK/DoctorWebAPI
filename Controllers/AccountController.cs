using DoctorWebApi.Helpers;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        // GET api/Account/specialities
        [HttpGet("specialities")]
        public async Task<List<string>> GetSpecialities()
        {
            var specs = Specialities.GetSpecialitiesForDropDown();
            var list = new List<string>();

            foreach (var s in specs)
            {
                list.Add(s.Text);
            }

            return list;
        }

        // POST api/Account/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(Register model)
        {
            await CheckRoles();

            var userName = _db.Users.Where(u => u.Name == model.Name).Select(x => x.Name).FirstOrDefault();

            if (userName == null)
            {
                if (ModelState.IsValid)
                {
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
                        user.Speciality = model.Speciality;
                        try
                        {
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            var encoded = HttpUtility.UrlEncode(token);
                            string url = $"{BackEndApiURL}/Account/confirmEmail?userId={user.Id}&token={token}";
                            await _emailSender.SendEmailAsync(user.Email, "Confirm your account",
                                                     $"Follow the: <br/><a href={url}>link to confirm</a>");
                            await _db.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            await _userManager.DeleteAsync(user);
                            throw;
                        }

                        return NoContent();
                    }

                    return BadRequest("Register failed");

                }
            }
     
            return BadRequest("Name is already exists");
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
                    user.LastActive = DateTime.UtcNow;
                    _db.SaveChangesAsync(); 

                    return Ok(new { token = generatedToken });
                }
                return Unauthorized();
            }

            return Unauthorized();
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return Ok();
        }



        // GET: api/Account/users/id
        [HttpGet]
        [Route("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            UserDTO user = new UserDTO();
            user = _db.Users.Where(x => x.Id == id).Select(c => new UserDTO()
            {
                Id = c.Id,  
                Name = c.Name,
                Email = c.Email,
                Introduction = c.Introduction,
                Speciality = c.Speciality
            }).SingleOrDefault();

            return Ok(user);
        }


        // GET: api/Account/pagedDocs
        [HttpGet("pagedDocs")]
        public async Task<ActionResult<PagedList<User>>> GetUsers([FromQuery]UserParams userParams)
        {
            var userList = await _accService.GetUsersAsync(userParams);
            var responce = new PaginationHeader<User>(userList, userParams.PageNumber, userParams.PageSize, userList.TotalCount);
            return Ok(responce);
        }

        // GET: api/Account/users
        [HttpGet("users")]
        public async Task<ActionResult<PagedList<User>>> GetAllUsers()
        {
            var userList = await _db.Users.ToListAsync();
            return Ok(userList);
        }


        // GET: api/Account/confirmEmail
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
            user.LastActive = DateTime.UtcNow;
            _db.SaveChangesAsync(); 
            return Ok(result);
        }

        // PUT api/Account/Edit/id
        [HttpPut]
        [Route("Edit/{id}")]
        public async Task<IActionResult> EditAccountById(string id, [FromBody] EditUserForm editUserForm)
        {
            var userToUpdate = _db.Users.FirstOrDefault(x => x.Id == id);
            if (userToUpdate == null)
            {
                return NotFound($"User with Id = {id} not found");
            }

            userToUpdate.Name = editUserForm.Name;
            userToUpdate.Introduction = editUserForm.Introduction;
            userToUpdate.Speciality = editUserForm.Speciality;

            await _db.SaveChangesAsync();
            return Ok(userToUpdate);
        }
    }
}
