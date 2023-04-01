using Doctor.BLL.Interface;
using Doctor.DataAcsess;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Doctor.DataAcsess.Models;
using DoctorWebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Web;

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accService;
        private readonly IEmailSender _emailSender;
        private readonly IJWTService _jWTManager;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly HostUrlOptions _hostUrl;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            IEmailSender emailSender, IAccountService accService, IJWTService jWTManager,
            IOptions<HostUrlOptions> options)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _accService = accService;
            _jWTManager = jWTManager;
            _hostUrl = options.Value;
        }

        public string BackEndApiURL => _hostUrl.BackEnd + "/api";

        // GET api/Account/roles
        [HttpGet("roles")]
        public async Task<IActionResult> CheckRoles()
        {
            var roleList =  await _accService.GetRoles();

            return Ok(roleList);
        }

        // GET api/Account/times
        [HttpGet("times")]
        public async Task<IActionResult> GetTimes()
        {
            var timeList = await _accService.GetTimes();

            return Ok(timeList);
        }

        // GET api/Account/specialities
        [HttpGet("specialities")]
        public async Task<IActionResult> GetSpecialities()
        {
            var specList = await _accService.GetSpecialities();

            return Ok(specList);
        }

        // POST api/Account/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(Register model)
        {
            await CheckRoles();

            string userName = await _accService.GetUsername(model.Name);
            
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
                        //await _signInManager.SignInAsync(user, isPersistent: true);
                        user.Speciality = model.Speciality;
                        try
                        {
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            var encoded = HttpUtility.UrlEncode(token);
                            string url = $"{BackEndApiURL}/Account/confirmEmail?userId={user.Id}&token={token}";
                            await _emailSender.SendEmailAsync(user.Email, "Confirm your account",
                                                     $"Follow the: <br/><a href={url}>link to confirm</a>");

                            await _accService.SaveAllAsync();
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
        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<ActionResult> Authenticate([FromBody] Login model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jWTManager.Authenticate(user.Id, user.Name, roles);

            if (token == null)
            {
                return Unauthorized();
            }

            user.LastActive = DateTime.Now;
            await _accService.SaveAllAsync();

            return Ok(token);
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
            UserDTO user = await _accService.GetUserDTOById(id);

            return Ok(user);
        }


        // GET: api/Account/pagedDocs
        [Authorize]
        [HttpGet("pagedDocs")]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var userList = await _accService.GetUsersAsync(userParams);
            var responce = new PaginationHeader<User>(userList, userParams.PageNumber, userParams.PageSize, userList.TotalCount);
            return Ok(responce);
        }

        // GET: api/Account/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var userList = await _accService.GetAllUsers();
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
            user.LastActive = DateTime.Now;
            await _accService.SaveAllAsync();
            return Ok(result);
        }

        // PUT api/Account/Edit/id
        [Authorize]
        [HttpPut]
        [Route("Edit/{id}")]
        public async Task<IActionResult> EditAccountById(string id, [FromBody] EditUserForm editUserForm)
        {
            var userToUpdate = await _accService.GetUserById(id);
            if (userToUpdate == null)
            {
                return NotFound($"User with Id = {id} not found");
            }

            userToUpdate.Name = editUserForm.Name;
            userToUpdate.Introduction = editUserForm.Introduction;
            userToUpdate.Speciality = editUserForm.Speciality;

            await _accService.SaveAllAsync();
            return Ok(userToUpdate);
        }
    }
}
