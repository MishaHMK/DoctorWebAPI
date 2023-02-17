using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorWebApi.Models
{
    public class Specialities
    {
        public static string Pediatrics = "Pediatrics";
        public static string Neurology = "Neurology";
        public static string Cardiology = "Cardiology";
        public static string Radiology = "Radiology";

        public static List<SelectListItem> GetSpecialitiesForDropDown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = Specialities.Pediatrics, Text = Specialities.Pediatrics },
                new SelectListItem { Value = Specialities.Neurology, Text = Specialities.Neurology },
                new SelectListItem { Value = Specialities.Cardiology, Text = Specialities.Cardiology },
                new SelectListItem { Value = Specialities.Radiology, Text = Specialities.Radiology }
            };
        }
    }
}
