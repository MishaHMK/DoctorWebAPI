using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWebApi.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string? Gender { get; set; }
        public DateTime? RegisteredOn { get; set; }
        public DateTime? LastActive { get; set; }
        public string? Introduction { get; set; }
        public string? Speciality { get; set; }
        public List<Message>? MessagesSent { get; set; }
        public List<Message>? MessagesReceived { get; set; }
    }
}
