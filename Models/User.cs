using Microsoft.AspNetCore.Identity;

namespace DoctorWebApi.Models
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }
    }
}
