using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DoctorWebApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        public JwtService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }
        public async Task<string> GenerateJWTTokenAsync(User user)
        {
            var id = user.Id;
            var roles = await _userManager.GetRolesAsync(user);

            return Generate(id, roles);
        }

        private string Generate(string id, IEnumerable<string> roles)
        {
            string jwtKey = "yJB9VKdj3vWbMdkzU8rYg5IWkICmamcnW5wO537R2VEv5N9zcmUcuLKeG71S7r4z";

            var claims = new List<Claim>
            {
                new Claim("NameIdentifier", id)
            };

            claims.AddRange(roles.Select(role => new Claim("Role", role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
              claims: claims,
              expires: DateTime.UtcNow.AddMinutes(120),
              signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
