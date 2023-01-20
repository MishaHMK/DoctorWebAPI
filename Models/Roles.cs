using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorWebApi.Models
{
    public class Roles
    {
        public static string Admin = "Admin";
        public static string Patient = "Patient";
        public static string Doctor = "Doctor";

        public static List<SelectListItem> GetRolesForDropDown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = Roles.Admin, Text = Roles.Admin },
                new SelectListItem { Value = Roles.Patient, Text = Roles.Patient },
                new SelectListItem { Value = Roles.Doctor, Text = Roles.Doctor }
            };
        }
    }
}
