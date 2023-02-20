using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DoctorWebApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<Models.User> _userManager;

        public JwtService(IConfiguration configuration, UserManager<Models.User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }
        public async Task<string> GenerateJWTTokenAsync(Models.User user)
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
                 new Claim("Role", role),
                 //new Claim(ClaimTypes.NameIdentifier, id),
                 //new Claim(ClaimTypes.Role, role)
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
