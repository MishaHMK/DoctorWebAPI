using System.ComponentModel.DataAnnotations;

namespace DoctorWebApi.Models
{
    public class Register
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string? Password { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The pasword and confirmation do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string? RoleName { get; set; }
    }
}
