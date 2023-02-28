using DoctorWebApi.Interfaces;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Doctor.DataAcsess.Entities;

namespace DoctorWebApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<Doctor.DataAcsess.Entities.User> _userManager;

        public JwtService(IConfiguration configuration, UserManager<Doctor.DataAcsess.Entities.User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }
        public async Task<string> GenerateJWTTokenAsync(Doctor.DataAcsess.Entities.User user)
        {
            var id = user.Id;
            var roles = await _userManager.GetRolesAsync(user);

            return Generate(id, roles);
        }

        private string Generate(string id, IEnumerable<string> roles)
         {
             var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Df6YcHO4ZiHEMWM4IN0cnWwbM"));
             var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
             var role = roles.FirstOrDefault();
             var claims = new[]
             {
                 new Claim("NameIdentifier", id),
                 new Claim("Role", role)
             };

             var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                                              _configuration["Jwt:Audiences"],
                                              claims,
                                              expires: DateTime.UtcNow.AddMinutes(30),
                                              signingCredentials: credentials);

             return new JwtSecurityTokenHandler().WriteToken(token);
         }
    }
}
