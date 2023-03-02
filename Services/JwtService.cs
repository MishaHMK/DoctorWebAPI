using DoctorWebApi.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DoctorWebApi.Helper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace DoctorWebApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly JwtOptions _options;
        private readonly UserManager<Doctor.DataAcsess.Entities.User> _userManager;

        public JwtService(IConfiguration configuration, UserManager<Doctor.DataAcsess.Entities.User> userManager, IOptions<JwtOptions> options)
        {
            _configuration = configuration;
            _userManager = userManager;
            _options = options.Value;
        }
        public async Task<string> GenerateJWTTokenAsync(Doctor.DataAcsess.Entities.User user)
        {
            var id = user.Id;
            var roles = await _userManager.GetRolesAsync(user);

            return Generate(id, roles);
        }

        private string Generate(string id, IEnumerable<string> roles)
         {
             var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
             var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
             var role = roles.FirstOrDefault();
             var claims = new[]
             {
                 new Claim("NameIdentifier", id),
                 new Claim("Role", role)
             };

             var token = new JwtSecurityToken(_options.Issuer,
                                              _options.Audience,
                                              claims,
                                              null,
                                              DateTime.UtcNow.AddHours(1),
                                              signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
         }
    }
}
